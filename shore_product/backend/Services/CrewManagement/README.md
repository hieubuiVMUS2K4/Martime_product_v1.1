# Services/CrewManagement — Cỗ máy workflow vòng đời thuyền viên

## Mục đích

Đây là nơi chứa toàn bộ logic nghiệp vụ (state machine, quy tắc duyệt, điều phối liên bảng) đứng sau `Controllers/CrewManagement/`. 14 file, phần lớn theo cặp `IXxxService.cs` + `XxxService.cs`. Nếu `Services/Crew/` là "hệ thống ghi nhận" tĩnh, thư mục này là "cỗ máy quy trình" động — mô hình hoá toàn bộ hành trình một thuyền viên đi qua: tuyển dụng ngoài → tạo case onboarding → phân công lên tàu → di chuyển → ký sign-on → (vận hành) → ký sign-off.

## Cấu trúc & vai trò

| File | Interface | Vai trò |
|---|---|---|
| `AuditService.cs` | `IAuditService` | Bảng audit log dùng chung (`AuditLogs`), append-only, `entityId` lưu dạng string để dùng chung cho entity khóa Guid lẫn khóa int |
| `CrewStatusService.cs` | `ICrewStatusService` | State machine hợp lệ **duy nhất thật sự được validate** cho `CrewMember.Status` (Draft/Active/Inactive/Suspended/Retired) |
| `AssignmentService.cs` (877 dòng — file lớn nhất) | `IAssignmentService` | Định biên tàu (manning standard/position), phân công thuyền viên cụ thể, phát hiện xung đột, xác nhận, planning board, tìm ứng viên nội bộ |
| `ComplianceService.cs` (781 dòng) | `IComplianceService` | Rule engine cấu hình được (rule set → rule → dimension), waiver, đánh giá/mô phỏng, snapshot |
| `DocumentWorkflowService.cs` (525 dòng) | `IDocumentWorkflowService` | Draft → Submit → Gửi xác minh → Verify/Reject/Yêu cầu nộp lại, có versioning và gia hạn |
| `ExternalRequestService.cs` (276 dòng) | `IExternalRequestService` | Yêu cầu tuyển ngoài, ứng viên đại lý, tin nhắn trao đổi |
| `TravelService.cs` (333 dòng) | `ITravelService` | Yêu cầu di chuyển gắn với 1 `CrewAssignment`, tự sinh sau khi assignment được xác nhận |
| `OnboardEventService.cs` (671 dòng) | `IOnboardEventService` | Sự kiện lên/xuống tàu, cấp quyền truy cập hệ thống, sổ ký sign-on/sign-off (2 thao tác nặng nhất về side-effect trong cả thư mục) |
| `OnboardingService.cs` (486 dòng) | `IOnboardingService` | Case chuẩn bị lên tàu + checklist tự sinh (9 mục chuẩn + mục sinh từ compliance engine) |

## Luồng hoạt động chính

### Bảng trạng thái tham chiếu (đọc từ `shore_product/shared/Models/CrewManagement/Enums.cs`)

| Entity | Enum trạng thái | Có validate transition? |
|---|---|---|
| `CrewMember.Status` | Draft, Active, Inactive, Suspended, Retired | **Có** (`CrewStatusService`) |
| `OnboardingCase.Status` | Draft, Invited, InProgress, PendingReview, ReturnedForCompletion, Approved, Activated, Cancelled | **Có** (`OnboardingService`), cộng điều kiện "mọi checklist bắt buộc phải Completed/Waived" trước khi vào `Activated` |
| `CrewAssignment.Status` | Draft, Proposed, PendingCrewConfirmation, Confirmed, TravelInProgress, ReadyToJoin, OnBoarded, Completed, Cancelled, Declined | Không — set trực tiếp |
| `ExternalRequest.Status` | Draft, Sent, Viewed, InProgress, CandidateSubmitted, Shortlisted, Closed, Cancelled | Không |
| `TravelRequest.Status` | Draft, Pending, BookingInProgress, Booked, InTransit, Completed, Cancelled, Reissued | Không |
| `DocumentSubmission.Status` | Draft, Submitted, SentForVerification, UnderReview, Verified, Rejected, Expired, Archived, Superseded | Không (mỗi method tự kiểm tra điều kiện tiền đề riêng) |
| `CrewAccessGrant.Status` | NotGranted, PendingSync, Granted, Suspended, Revoked | Không (nhưng mỗi hàm chỉ cho phép 1 trạng thái nguồn cụ thể) |
| `PoolStatus` (trên `CrewMember`) | Available, Assigned, OnLeave, Medical, Hold | Không — bị ghi trực tiếp từ **4 nơi khác nhau** (xem ghi chú) |

### Chuỗi nghiệp vụ đầy đủ (xuyên nhiều service)

```
1. ExternalRequestService.SubmitCandidateAsync
      → tự chuyển ExternalRequest → "CandidateSubmitted" (chỉ nếu đang Sent/Viewed/InProgress)
        (KHÔNG tự liên kết ExternalCandidate vào CrewMember thật — LinkedCrewMemberId
         tồn tại nhưng không có code nào gán nó trong các file này)

2. OnboardingService.CreateCaseAsync
      → sinh 9 checklist chuẩn (thông tin cá nhân, khẩn cấp, upload Passport/Sổ thuyền viên/
        COC/Giấy khám sức khỏe/Flag Endorsement, xác minh, xác nhận cuối)
      → GenerateComplianceChecklistAsync: gọi IComplianceService.EvaluateCrewAsync(stage=Onboarding),
        thêm checklist item DocumentUpload cho mỗi rule chưa Met/Waived (không trùng 9 mục chuẩn)
      → UpdateCaseStatusAsync("Activated") CHỈ được phép khi mọi mục bắt buộc đã Completed/Waived
      → khi Activated: NẾU CrewMember.Status đang Draft, ghi thẳng CrewMember.Status = Active
        (KHÔNG qua ICrewStatusService — transition này không có dòng trong CrewStatusHistory)

3. AssignmentService.CreateAssignmentAsync
      → gọi IComplianceService.EvaluateCrewAsync(stage=PreConfirm), lưu vào ComplianceResult
        (không chặn — lỗi compliance chỉ hiện thành "conflict", chưa chặn tạo Draft)
      → DetectAndSaveConflictsAsync: kiểm tra 4 loại — trùng lịch (DateOverlap), crew không sẵn
        sàng (CrewUnavailable), sai chức danh (RankMismatch), compliance chặn (ComplianceBlocker)

   AssignmentService.RespondConfirmationAsync (crew phản hồi "Confirmed")
      → CHẠY LẠI compliance PreConfirm — lần này CHẶN CỨNG: NotEligible → throw, không cho Confirmed
      → nếu qua: gọi ITravelService.AutoGenerateFromAssignmentAsync (best-effort)

4. TravelService.AutoGenerateFromAssignmentAsync
      → tạo TravelRequest "Pending", loại "JoinVessel", CreatedBy="AutoGenerate"
      → khi TravelRequest chuyển "InTransit" VÀ Assignment đang "Confirmed":
        ghi thẳng Assignment.Status = TravelInProgress (KHÔNG qua IAssignmentService —
        transition này không xuất hiện trong AssignmentStatusHistory)

5. OnboardEventService.CreateSignOnAsync   ★ 1 lệnh gọi, 6 hiệu ứng phụ ★
      1) tạo SignOnRecord
      2) nếu Assignment chưa OnBoarded: ghi thẳng Assignment.Status=OnBoarded, ActualStartDate=now
         (KHÔNG qua IAssignmentService — không có dòng lịch sử)
      3) tự tạo OnboardEvent loại SignedOn, liên kết ngược lại SignOnRecord
      4) tự tạo ServiceRecord mới (BoardingDate = ngày sign-on)
      5) tự Granted CrewAccessGrant module "All" nếu chưa có
      6) CrewMember.IsOnboard=true, EmbarkDate=now, PoolStatus=Assigned

   OnboardEventService.CreateSignOffAsync   — làm ngược lại tương tự khi rời tàu,
      PoolStatus chỉ về "Available" nếu crew KHÔNG còn assignment active nào khác
```

## Liên kết với phần khác

- **Từ `Services/Crew/`**: `CrewService.CreateCrewAsync` gọi `IOnboardingService.CreateCaseAsync` — điểm nối duy nhất từ ngoài vào thư mục này.
- **Trong nội bộ thư mục**: `AssignmentService` gọi `IComplianceService` (2 lần, khác mức độ chặn) và `ITravelService`; `OnboardingService` gọi `IComplianceService`. `ComplianceService` và `AuditService` là 2 service "lá" — chỉ bị gọi vào, không gọi service nào khác trong thư mục.
- **Sang `Services/Sync/`**: `AssignmentService`/`TravelService` yêu cầu `ISyncOutboxService` là dependency **bắt buộc**, gọi `BroadcastAsync` không bọc try/catch (khác phong cách phòng thủ ở `Services/Crew/`). Các service còn lại trong thư mục này (`OnboardingService`, `DocumentWorkflowService`, `ExternalRequestService`, `OnboardEventService`, `ComplianceService`) **không** gọi `ISyncOutboxService` ở đâu cả — dù `OnboardEvent`/`SignOnRecord`/`SignOffRecord` đều có cột `IsSynced` (còn `CrewAccessGrant` thì không có cột này). Điều này ngụ ý các bản ghi sign-on/sign-off được author ở Edge và Shore chỉ tiếp nhận qua pipeline nhập (`Services/Sync/SyncInboxService.ProcessOnboardEventFromEdgeAsync`) — nếu HR tạo sign-on thủ công từ Shore, dữ liệu đó sẽ **không** được đẩy xuống tàu từ đây.
- **Model tương ứng**: các entity nằm trong `Maritime.Shared.Models.CrewManagement` (thư viện dùng chung Edge/Shore, ngoài phạm vi `shore_product/backend/`), không phải trong `Models/` của Shore.

## Ghi chú khi đọc/dạy

- **Audit logging KHÔNG nhất quán.** Chỉ 4/9 service gọi `IAuditService`: `ComplianceService`, `OnboardingService`, `DocumentWorkflowService`, `CrewStatusService`. `AssignmentService`, `TravelService`, `ExternalRequestService`, `OnboardEventService` — bốn service thay đổi trạng thái nhiều nhất — **không** ghi audit log tập trung; chúng tự có bảng lịch sử riêng (`AssignmentStatusHistory`, `TravelStatusHistory`) hoặc hoàn toàn không có gì (`ExternalRequestService`, `OnboardEventService`). Khi cần điều tra "ai làm gì lúc nào", phải biết kiểm tra đúng bảng theo từng entity, không có một nguồn sự thật duy nhất.
- **`PoolStatus` là trường lỏng lẻo nhất trong toàn bộ hệ thống** — bị ghi trực tiếp (không qua service chuyên trách, không có bảng lịch sử, không có audit) từ ít nhất 4 nơi: `CrewService.AssignToVesselAsync`/`UnassignFromVesselAsync` (chuỗi hardcode `"Assigned"`/`"Available"`, không dùng hằng số `PoolStatus.*`), và `AssignmentService.RespondConfirmationAsync`/`OnboardEventService.CreateSignOnAsync`/`CreateSignOffAsync` (dùng đúng hằng số). Khi debug "tại sao PoolStatus của người này sai", phải rà cả 2 thư mục Services (`Crew/` và `CrewManagement/`), không chỉ một chỗ.
- **Nhiều transition "tắt" bỏ qua service chủ quản, ghi thẳng vào `AppDbContext` dùng chung**: `TravelService` (khi InTransit) và `OnboardEventService` (khi sign-on/sign-off) đều tự set `CrewAssignment.Status` thay vì gọi `IAssignmentService.ChangeStatusAsync` — hệ quả là `AssignmentStatusHistory` chỉ ghi nhận các bước đầu (Draft→Proposed→PendingCrewConfirmation→Confirmed), còn các bước sau (TravelInProgress, OnBoarded, Completed) **im lặng không có dòng lịch sử nào**. Tương tự, `OnboardingService` khi Activate case cũng ghi thẳng `CrewMember.Status` thay vì gọi `ICrewStatusService`, nên riêng transition Draft→Active này không xuất hiện trong `CrewStatusHistory`.
- **Compliance được gọi ở 2 mức độ nghiêm ngặt khác nhau trong cùng một luồng**: tạo assignment (Draft) — không chặn; xác nhận assignment (Confirmed) — chặn cứng. Nếu thấy một assignment "Draft" có `ComplianceResult = NotEligible` mà vẫn tồn tại được, đó là hành vi đúng thiết kế, không phải bug.
- **`ExternalRequestService` là service "cô lập" nhất** — không `IAuditService`, không `ISyncOutboxService`, không gọi service nào khác, không ai gọi nó ngoài controller. Nếu cần hoàn thiện luồng "tuyển ngoài → thành nhân viên chính thức", đây là chỗ còn thiếu kết nối (trường `ExternalCandidateDto.LinkedCrewMemberId` chưa từng được gán ở bất kỳ đâu trong các file này).
- Reviewer/actor (`createdBy`, `approvedBy`...) được lấy theo 3 cách khác nhau tuỳ file: property dùng chung sạch sẽ (`AssignmentController.Actor`), lặp lại `User.Identity?.Name ?? "system"` từng chỗ (`ComplianceController`), hoặc hoàn toàn không lấy actor thật (`ExternalRequestController`, `OnboardEventController.ReinstateAccess` nhận `grantedBy` qua query string do client tự khai).
