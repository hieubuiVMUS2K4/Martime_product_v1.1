# Services/Crew — Nghiệp vụ HR cơ bản của thuyền viên

## Mục đích

Lớp nghiệp vụ đứng sau `Controllers/Crew/`: CRUD hồ sơ thuyền viên, 4 loại tài liệu tùy thân, lịch sử phục vụ, và quản lý chứng chỉ (cả danh mục lẫn chứng chỉ cá nhân). Đây là service "hệ thống ghi nhận" (system of record) có chủ đích đơn giản — không có state machine, không kiểm tra compliance phức tạp, không ghi audit log. Phần workflow/quy trình phức tạp hơn nằm ở `Services/CrewManagement/` (đọc kèm để hiểu ranh giới).

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `ICrewService.cs` / `CrewService.cs` (995 dòng) | CRUD `CrewMember`, tài liệu (travel/seafarer/employment/health), `ServiceRecord` (lịch sử phục vụ), thống kê hạm đội, gán/bỏ gán tàu |
| `ICertificateService.cs` / `CertificateService.cs` (584 dòng) | CRUD danh mục loại chứng chỉ + chứng chỉ cá nhân từng thuyền viên, báo cáo hết hạn/compliance đơn giản (khác với rule-engine ở `CrewManagement/ComplianceService`) |

## Luồng hoạt động chính

**Tạo mới một thuyền viên** (`CrewService.CreateCrewAsync`) là điểm khởi đầu của toàn bộ vòng đời crew, bắc cầu sang `CrewManagement/`:

```
CreateCrewAsync(request)
   │
   ├─ validate FullName/CrewId không rỗng, CrewId duy nhất
   ├─ lưu CrewMember (IsSynced=false, OriginNode="SHORE")
   ├─ try { IOnboardingService.CreateCaseAsync(...) }   ← cầu nối sang CrewManagement/
   │    catch { /* không chặn việc tạo crew nếu tạo case lỗi */ }
   └─ try { ISyncOutboxService.BroadcastAsync("crew_member", ...) }  ← đẩy xuống MỌI tàu
        catch { /* log warning, không throw */ }
```

**Gán thuyền viên vào tàu** (`AssignToVesselAsync`) khác biệt về hướng sync: thay vì `BroadcastAsync` (mọi tàu), nó dùng `EnqueueAsync(vessel.IMO, ...)` (chỉ đúng 1 tàu) để snapshot **toàn bộ** hồ sơ + chứng chỉ + 4 loại tài liệu của crew đó xuống đúng con tàu họ sắp lên — set `VesselId`, `IsOnboard=false` (chưa true — cờ này chỉ bật thật khi có sự kiện sign-on, xem `Services/CrewManagement/README.md`), `PoolStatus="Assigned"`, `OnboardStatus="PendingReview"`.

**Quản lý chứng chỉ loại (`CertificateService`)** phân biệt rõ 2 chính sách sync: chứng chỉ CỤ THỂ của một thuyền viên (`crew_certificate`) dùng `BroadcastAsync` (mọi tàu); còn LOẠI chứng chỉ (master data, `certificate`) cố tình **không** broadcast — chỉ `EnqueueAsync` tới đúng những tàu đã được gán loại chứng chỉ đó qua `VesselCertificateAssignments` (comment trong code: *"Certificate types are only pushed to a specific vessel's edge when they are explicitly assigned"*).

## Liên kết với phần khác

- **Cầu nối duy nhất sang `Services/CrewManagement/`**: `CrewService.CreateCrewAsync` gọi `IOnboardingService.CreateCaseAsync` (optional, best-effort). Ngoài điểm này, hai thư mục Services độc lập với nhau.
- Gọi `ISyncOutboxService` (`Services/Sync/`) ở hầu hết thao tác ghi — xem `Services/Sync/README.md` để hiểu `EnqueueAsync` (1 tàu) khác `BroadcastAsync` (mọi tàu) thế nào.
- Dữ liệu do đây quản lý (`CrewMember`, `CrewCertificate`, 4 bảng tài liệu, `ServiceRecord`) là các entity đồng bộ hai chiều — khi Edge push lên, `Services/Sync/SyncInboxService.cs` và `Services/Sync/ConflictResolverService.cs` mới là nơi xử lý xung đột, không phải các service ở đây.
- Được gọi bởi `Controllers/Crew/CrewController.cs` và `Controllers/Crew/CertificatesController.cs`.

## Ghi chú khi đọc/dạy

- **`ISyncOutboxService` là dependency tùy chọn (nullable) ở đây** — mọi lời gọi đều bọc `if (_syncOutbox != null) { try {...} catch { LogWarning } }`. Điều này khác biệt rõ với `Services/CrewManagement/AssignmentService`/`TravelService`, nơi `ISyncOutboxService` là dependency bắt buộc và lời gọi không có try/catch (một exception sync ở đó sẽ làm hỏng cả thao tác nghiệp vụ). Khi debug lỗi sync, nhớ kiểm tra file đang đọc thuộc nhóm "phòng thủ" (Crew) hay "không phòng thủ" (CrewManagement).
- **`UpdateCrewAsync` cố tình KHÔNG cho phép sửa `IsOnboard`** — comment giải thích trường này được quản lý bởi "các endpoint sign-on/sign-off chuyên biệt" (tức `Services/CrewManagement/OnboardEventService`), không phải qua CRUD chung.
- **`CrewMember` có nguyên một cơ chế review thay đổi từ Edge**: `OnboardStatus`, `ReviewChecklist` (JSON), `ReviewNotes`, `EdgeChanges` (JSON diff từng trường), `EdgeChangesViewed` — cho phép thuyền viên chỉnh sửa hồ sơ ngay trên tàu và Shore phải xem-xét/xác nhận thay đổi đó thay vì tin tưởng tuyệt đối "ai ghi sau cùng thắng". Cơ chế tính `EdgeChanges` thực tế nằm ở `Services/Sync/SyncInboxService.cs` (hàm `ComputeCrewEdgeChanges`), không phải ở đây — file trong thư mục này chỉ đọc/hiển thị và cho phép Shore xóa cờ qua endpoint `mark-changes-viewed`.
- **Không có `IAuditService`** — mọi log chỉ dùng `ILogger`, không có audit trail có cấu trúc như bên `CrewManagement/`. Nếu cần dò "ai sửa hồ sơ crew này lúc nào", sẽ không tìm thấy trong bảng `AuditLogs`.
- Chỉ có 2 tiêu chí xác định "hết compliance" ở `GetCrewComplianceAsync`: thiếu chứng chỉ bắt buộc hoặc chứng chỉ đã hết hạn — trạng thái "sắp hết hạn" (`EXPIRING_SOON`) KHÔNG làm crew mất compliance ở phép tính này (khác với rule engine `ComplianceService` bên `CrewManagement/`, có khái niệm mức độ nghiêm trọng riêng theo từng rule).
