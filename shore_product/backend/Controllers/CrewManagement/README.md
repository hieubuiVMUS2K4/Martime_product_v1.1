# Controllers/CrewManagement — Cỗ máy quy trình vòng đời thuyền viên

## Mục đích

Nếu `Controllers/Crew/` trả lời "thuyền viên này LÀ AI", thì `Controllers/CrewManagement/` trả lời **"thuyền viên này ĐANG Ở BƯỚC NÀO"**: từ lúc tuyển ngoài, tạo case onboarding, phân công lên tàu (có phát hiện xung đột lịch), duyệt hồ sơ/tài liệu, thu xếp di chuyển, đến khi ký sign-on/sign-off tại cầu tàu. Đây là phần phức tạp nhất về nghiệp vụ trong toàn bộ Shore Backend — 8 controller, mọi action đều là pass-through mỏng xuống một service chuyên trách tương ứng trong `Services/CrewManagement/` (đọc kỹ `Services/CrewManagement/README.md` để hiểu state machine thật sự nằm ở đâu).

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `AssignmentController.cs` | `api/assignments` | Định biên (manning standard/position) theo tàu, phân công thuyền viên cụ thể, phát hiện xung đột lịch, quy trình xác nhận, planning board, tìm ứng viên nội bộ |
| `ComplianceController.cs` | `api/compliance` | Rule engine cấu hình được: bộ luật (rule set), luật (rule), miễn trừ (waiver), đánh giá 1 thuyền viên hoặc mô phỏng "what-if", snapshot & tổng hợp hạm đội |
| `CrewProfileController.cs` | `api/crew-profiles` **và** `api/audit-logs` | **File chứa 2 class controller**: `CrewProfileController` (đổi trạng thái vòng đời `CrewMember.Status`, lịch sử trạng thái) + `AuditLogController` (tra cứu audit log tổng quát, không riêng crew) |
| `DocumentWorkflowController.cs` | `api/document-submissions` | Quy trình duyệt tài liệu chính thức: draft → submit → gửi xác minh → verify/reject/yêu cầu nộp lại, có versioning và gia hạn (renew) |
| `ExternalRequestController.cs` | `api/external-requests` | Yêu cầu gửi cho đại lý cung ứng thuyền viên bên ngoài, nhận ứng viên đại lý gửi về, duyệt ứng viên, tin nhắn trao đổi |
| `OnboardEventController.cs` | `api/onboard-events` | 3 nhóm con: sự kiện lên/xuống tàu (`OnboardEvent`), cấp quyền truy cập hệ thống trên tàu (`CrewAccessGrant`), và sổ ký sign-on/sign-off chính thức |
| `OnboardingController.cs` | `api/onboarding-cases` | Case chuẩn bị lên tàu: checklist tự sinh (9 mục chuẩn + mục sinh từ compliance engine), đổi trạng thái case, hoàn thành/miễn trừ từng mục checklist |
| `TravelController.cs` | `api/travel-requests` | Vé/lịch trình di chuyển gắn với một `CrewAssignment`, tự sinh từ assignment đã xác nhận, quản lý từng chặng (segment) |

## Luồng hoạt động chính

Đây là luồng nghiệp vụ đầy đủ, xâu chuỗi qua nhiều controller/service (không phải lúc nào cũng gọi trực tiếp nhau — nhiều bước gọi thẳng vào bảng `CrewAssignments`/`CrewMembers` qua `AppDbContext` dùng chung thay vì gọi service khác, xem ghi chú "Gotcha" ở `Services/CrewManagement/README.md`):

```
1. ExternalRequestController          → gửi yêu cầu tuyển ngoài, nhận CandidateSubmitted
   (hoặc CrewController.CreateAsync)  → tự động tạo OnboardingCase (Draft)

2. OnboardingController               → checklist (giấy tờ, xác minh...) → Activated
                                         (Activated bắt buộc mọi mục IsMandatory phải Completed/Waived)

3. AssignmentController               → tạo CrewAssignment (Draft), chạy compliance
                                         không chặn) → khi crew "Confirmed": BẮT BUỘC compliance
                                         Eligible (chặn cứng nếu NotEligible)
                                       → tự động sinh TravelRequest (qua TravelService)

4. TravelController                   → đặt vé/khách sạn theo segment
                                         khi chuyển "InTransit" → tự set Assignment = TravelInProgress

5. OnboardEventController             → CreateSignOnAsync: 1 lệnh gọi kéo theo 6 hiệu ứng phụ
                                         (SignOnRecord, OnboardEvent, ServiceRecord mới,
                                          CrewAccessGrant "All", Assignment→OnBoarded,
                                          CrewMember.IsOnboard=true)
                                       → CreateSignOffAsync: đảo ngược tương tự khi rời tàu

6. ComplianceController               → chạy song song bất kỳ lúc nào: đánh giá lại,
                                         cấp/duyệt miễn trừ (waiver), snapshot định kỳ

DocumentWorkflowController            → độc lập, phục vụ việc nộp/xác minh từng loại giấy tờ
                                         (được checklist ở bước 2 tham chiếu tới)
```

## Liên kết với phần khác

- Mỗi controller ánh xạ 1-1 tới một interface service cùng tên trong `Services/CrewManagement/` (`IAssignmentService`, `IComplianceService`, `IOnboardingService`, `IDocumentWorkflowService`, `IExternalRequestService`, `IOnboardEventService`, `ITravelService`, cộng `ICrewStatusService`/`IAuditService` cho `CrewProfileController`). Toàn bộ state machine, quy tắc nghiệp vụ nằm ở tầng service — xem `Services/CrewManagement/README.md`.
- Tham chiếu ngược sang `Controllers/Crew/` qua khóa `crewMemberId` (không có FK code-level, chỉ là Guid dùng chung).
- `OnboardEventController`/`OnboardEventService` là nơi entity `OnboardEvent`/`SignOnRecord`/`SignOffRecord` mang cờ `IsSynced` nhưng **không có service nào ở đây chủ động đẩy sync** — ngụ ý các bản ghi này được author ở Edge (tàu) và Shore chỉ tiếp nhận qua pipeline `Services/Sync/SyncInboxService.cs` (xử lý `onboard_event`/`sign_on_record`/`sign_off_record` — xem `ProcessOnboardEventFromEdgeAsync` trong `Services/Sync/README.md`), chứ không phải luôn được tạo thủ công từ Shore.

## Ghi chú khi đọc/dạy

- **`CrewProfileController.cs` chứa 2 class, 2 route prefix khác nhau** (`api/crew-profiles` và `api/audit-logs`) — tên file dễ gây hiểu lầm chỉ có một controller.
- **Không phải mọi state machine đều được validate.** Chỉ `CrewStatusService` (đổi `CrewMember.Status`) và `OnboardingService` (đổi `OnboardingCase.Status`) có bảng chuyển trạng thái hợp lệ được kiểm tra rõ ràng. `AssignmentController`, `ExternalRequestController`, `TravelController` cho phép set thẳng `Status` mới mà không kiểm tra chuyển trạng thái có hợp lệ hay không (chỉ có vài điều kiện tiền đề rời rạc theo từng action) — khi thấy trạng thái "kỳ lạ", đừng giả định luôn có validation chặn nó.
- **`OnboardingController.UpdateCaseStatus` và `CrewProfileController.ChangeStatus` cùng dùng chung DTO `ChangeCrewStatusRequest`** nhưng điều khiển **hai state machine hoàn toàn khác nhau** (case onboarding vs hồ sơ crew) — trùng tên/hình dạng request nhưng khác ý nghĩa, dễ gây nhầm khi debug.
- **`OnboardEventController.ReinstateAccess` nhận `grantedBy` qua query string** (client tự khai ai là người cấp lại quyền) thay vì lấy từ `User.Identity` như hầu hết action khác trong thư mục — một điểm client có thể khai khống danh tính.
- **`ExternalRequestController`/`ExternalRequestService` không ghi audit log** và cũng không tự động liên kết ứng viên trúng tuyển (`ExternalCandidate`) vào một `CrewMember`/`OnboardingCase` thật — trường `LinkedCrewMemberId` tồn tại nhưng không có code nào set nó trong phạm vi các file này; nếu cần luồng "tuyển ngoài → thành nhân viên chính thức" đầy đủ, kiểm tra xem bước nối này được làm ở đâu (frontend? thủ công?) trước khi giả định nó tự động.
- **Kiểm tra actor (ai thực hiện thao tác) không đồng nhất.** `AssignmentController` có property `Actor` dùng chung gọn gàng; `ComplianceController` lặp lại `User.Identity?.Name ?? "system"` ở ~8 chỗ; `ExternalRequestController`/`TravelController` không lấy actor từ `User.Identity` ở bất kỳ đâu.
- Toàn bộ 8 controller gắn `[Authorize(Policy = "InternalAccess")]` — theo cấu hình mặc định hiện tại (`Security:RequireInternalAccess = false`), policy này **không chặn request nào cả** (xem `Security/README.md`).
