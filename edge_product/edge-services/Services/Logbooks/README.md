# Services/Logbooks — Nghiệp vụ 9 loại sổ nhật ký hàng hải bắt buộc

## Mục đích

Hiện thực hoá business logic cho 9 loại sổ nhật ký/ghi chép mà luật hàng hải quốc tế bắt buộc tàu phải duy trì: Deck Log (SOLAS V/28), Engine Log, Oil Record Book (MARPOL Annex I), 3 biến thể Garbage Record (MARPOL Annex V), Ballast Water Record (BWM Convention), Watchkeeping Log (STCW), và Voyage Log (nhật ký hành trình SOLAS V/28). Đây là "tầng service" tương ứng 1-1 với `Controllers/Logbooks/`.

## Cấu trúc & vai trò

| Cặp Interface / Implementation | Model chính | Điểm nghiệp vụ riêng |
|---|---|---|
| `IDeckLogbookService` / `DeckLogbookService` | `DeckLogBook` | Watch period, sĩ quan trực ca, vị trí/hướng/tốc độ, thời tiết, drill, thay đổi thuyền viên, thông tin cảng/hoa tiêu. |
| `IEngineLogbookService` / `EngineLogbookService` | `EngineLogBook` | RPM/load/nhiệt độ/áp suất máy chính-phụ-nồi hơi, tiêu thụ nhiên liệu ME/AE/Boiler, ROB. Chữ ký dùng field riêng `ChiefEngineerSignature` (khác `MasterSignature` ở các logbook khác). Là nguồn dữ liệu mà `AbstractLogService.AutoFillAsync` đọc lại. |
| `IOilRecordService` / `OilRecordService` | `OilRecordBook` | Oil Record Book Part I (MARPOL Annex I): `OperationCode`, tank nguồn/đích, khối lượng, vị trí. Không validate mã operation hay bắt buộc vị trí khi xả (khác 2 service Garbage Part I/II bên dưới). |
| `IGarbageRecordService` / `GarbageRecordService` | `GarbageRecordBook` | Sổ rác "gộp" kiểu cũ — `GarbageCategory` là chuỗi tự do, **không validate category A-K**. Có thêm field xả sự cố (`AccidentalDischargeReason/Measures`). |
| `IGarbagePartIService` / `GarbagePartIService` | `GarbageRecordPartI` | **Có validate MARPOL Annex V đầy đủ** — category A-I; nhóm `{A, D, F, H}` bị cấm xả biển tuyệt đối; bắt buộc toạ độ khi xả biển, bắt buộc tên cảng/cơ sở tiếp nhận khi giao nộp, bắt buộc giờ bắt đầu khi đốt. |
| `IGarbagePartIIService` / `GarbagePartIIService` | `GarbageRecordPartII` | Category chỉ **J hoặc K** (cargo residues). Rule nghiêm ngặt nhất: **Category K (HME) cấm xả biển tuyệt đối** và **bắt buộc** phải giao cơ sở tiếp nhận. |
| `IBallastWaterService` / `BallastWaterService` | `BallastWaterRecordBook` | Tank, thể tích, vị trí bắt đầu/kết thúc trao đổi nước dằn, độ mặn, hệ thống xử lý B-3/D-2. Không có logic kiểm tra tuân thủ D-1/D-2 trong code — chỉ lưu trữ. |
| `IWatchkeepingService` / `WatchkeepingService` | `WatchkeepingLog` | Đầy đủ field STCW rest-hours (`RestHoursLast24h`, `RestHoursLast7Days`, `RestHoursCompliant`), tình trạng thiết bị (Radar/ECDIS/AIS/Gyro/Autopilot/GMDSS), bàn giao ca, đánh giá mệt mỏi. **`RestHoursCompliant` do client tự tính và gửi lên — service không tự validate 10h/24h hay 77h/7 ngày.** |
| `IVoyageLogService` / `VoyageLogService` | `VoyageLogEntry` | **Khác hẳn pattern chung** — xem mục riêng bên dưới. |

## Luồng hoạt động chính

### A. Khuôn mẫu CRUD dùng chung cho 8/9 service (trừ `VoyageLogService`)

```
Interface (giống hệt hình dạng ở cả 8 service):
  CreateEntryAsync(CreateXxxDto, username) → (Success, Id, Error)
  GetEntryAsync(Guid id)                   → XxxResponseDto?
  GetEntriesAsync(LogbookPaginationDto)    → PaginatedLogbookResponseDto<XxxResponseDto>
  UpdateEntryAsync(Guid id, UpdateXxxDto)  → (Success, Error)
  DeleteEntryAsync(Guid id)                → (Success, Error)   -- SOFT DELETE
  SignEntryAsync(Guid id, SignXxxDto)      → (Success, Error)

Constructor: EdgeDbContext context, ILogger logger, IVoyageContextService voyageContext  (giống nhau ở cả 8 class)

CreateEntryAsync:
  - Id = Guid.NewGuid(), CreatedAt/UpdatedAt = UtcNow, OriginNode = Environment.MachineName, IsSynced = false
  - voyageContext.ResolveActiveVoyageAsync(<mốc thời gian log>) → tự gán VoyageId/VoyagePlanLegId
    (auto-link log vào chuyến đi đang chạy — "Phase 6: Voyage Context", xem Services/Voyage/README.md)

UpdateEntryAsync:
  - CHẶN nếu đã ký (MasterSignature/ChiefEngineerSignature khác rỗng) → lỗi "Cannot update a signed entry"
  - Field bắt buộc: luôn ghi đè. Field nullable: chỉ ghi đè nếu client gửi giá trị (tránh mất dữ liệu
    khi frontend gửi partial update)

DeleteEntryAsync:
  - SOFT DELETE (IsDeleted=true, DeletedAt, DeletedBy) — KHÔNG xoá vật lý

SignEntryAsync:
  - Chỉ set chữ ký + (thường) SignedAt — KHÔNG khoá các field khác

GetEntriesAsync:
  - Lọc !IsDeleted, filter FromDate/ToDate (theo field ngày riêng từng logbook),
    filter SearchTerm (Contains case-insensitive trên vài field text)
  - Sort giảm dần theo ngày rồi CreatedAt, Skip/Take PHÂN TRANG THỦ CÔNG
    (KHÔNG dùng Services/Common/PaginationExtensions.cs có sẵn — xem Ghi chú)

Mọi thao tác ghi đều set IsSynced = false → outbox tự động trong EdgeDbContext.SaveChanges()
sẽ tự tạo SyncQueue entry tương ứng (xem Data/README.md) — 8 service này KHÔNG tự tay
thêm SyncQueue.Add(...) ở bất kỳ đâu.
```

### B. `GarbagePartIService` — ví dụ validate MARPOL Annex V cụ thể

```
Category A-I hợp lệ (IsValidPartICategory)
ProhibitedSeaDischargeCategories = { A (Plastics), D (Cooking oil), F (Operational wastes),
                                      H (Cargo residues HME cleaned) }
  → nếu EstimatedAmountDischargedToSea > 0 và category thuộc nhóm này → LỖI
Xả biển                → bắt buộc DischargeLatitude/DischargeLongitude
Giao cơ sở tiếp nhận    → bắt buộc PortName hoặc ReceptionFacilityName
Đốt (incinerated)       → bắt buộc IncinerationStartTime
Tổng khối lượng (sea + reception + incinerated) phải > 0
```

`GarbagePartIIService` áp rule còn nghiêm ngặt hơn cho Category K: **cấm xả biển tuyệt đối + bắt buộc giao cơ sở tiếp nhận** — vi phạm được từ chối ngay ở tầng service với message rõ ràng "MARPOL VIOLATION...".

### C. `VoyageLogService` — ngoại lệ so với pattern chung

- Namespace DTO khác (`MaritimeEdge.DTOs`, không phải `.Logbooks`); có thêm 2 method: `GetTimelineAsync(voyageId, limit)` và `GetLastEntryAsync(voyageId)`.
- **Tự tính khoảng cách bằng công thức Haversine** (bán kính Trái Đất 3440.065 hải lý) khi client không gửi `DistanceFromLast`/`TotalVoyageDistance`, dựa vào toạ độ entry gần nhất.
- **`DeleteEntryAsync` là HARD DELETE thật sự** (`Remove()`) — khác hẳn 8 service kia đều soft-delete. Đây là điểm không nhất quán cần lưu ý.
- `GetTimelineAsync` build timeline UI-friendly (icon/màu) tra từ `VoyageLogEventTypes.EventInfoMap` (bản đồ tĩnh định nghĩa ở tầng DTO).
- `SignEntryAsync`: nếu có `Remarks` thì NỐI THÊM vào remarks hiện có (`"...\n[Master's note]: ..."`) thay vì ghi đè.

## Liên kết với phần khác

- **`Controllers/Logbooks/README.md`** — mỗi service ở đây có đúng 1 controller gọi trực tiếp qua interface.
- **`DTOs/Logbooks/README.md`** (gộp trong `DTOs/README.md`) — 4 DTO/loại (`CreateXxxDto`/`UpdateXxxDto`/`XxxResponseDto`/`SignXxxDto`) dùng làm input/output.
- **`Services/Voyage/VoyageContextService`** — được tiêm vào cả 8 service để tự động resolve `VoyageId`/`VoyagePlanLegId` theo mốc thời gian log (Phase 6 "auto-link").
- **`Data/README.md`** — cơ chế outbox tự động (`ProcessSyncQueue()` trong `EdgeDbContext`) là nơi thực sự tạo `SyncQueue` cho các entity logbook này khi `IsSynced` được set `false`.
- **`Services/AbstractLog/AbstractLogService.AutoFillAsync`** — đọc lại dữ liệu từ `EngineLogBook` (và `NoonReport`) để tự động điền "Nhật ký vắn tắt" theo ngày.

## Ghi chú khi đọc/dạy

- **Đọc 1 service là hiểu 7 service còn lại** (trừ `VoyageLogService`) — nên chọn `GarbagePartIService`/`GarbagePartIIService` để dạy trước vì có validate nghiệp vụ rõ ràng nhất (MARPOL Annex V), sau đó chỉ ra các service khác dùng ĐÚNG khung sườn nhưng không có phần validate.
- **`GarbageRecordService` (sổ gộp) tồn tại song song với `GarbagePartIService`/`GarbagePartIIService`** (sổ chia theo MARPOL Annex V) — 3 service cho cùng một chủ đề "rác thải", khác mức độ chi tiết/tuân thủ. Khi dạy, nên làm rõ đây không phải trùng lặp vô nghĩa mà là 2 thế hệ/2 mức độ chi tiết khác nhau của cùng một yêu cầu MARPOL.
- **`WatchkeepingLog` có đủ field STCW rest-hours nhưng KHÔNG tự validate** — nếu được giao nhiệm vụ "thêm kiểm tra tuân thủ giờ nghỉ tối thiểu 10h/24h và 77h/7 ngày", đây chính là chỗ cần sửa (`WatchkeepingService.CreateEntryAsync`/`UpdateEntryAsync`), một bài tập thực hành tốt.
- **`VoyageLogService.DeleteEntryAsync` hard-delete** trong khi mọi service khác trong cùng thư mục đều soft-delete — nên nêu ra như một câu hỏi thảo luận: "đây là chủ đích hay thiếu sót?"
- **8/9 service không dùng `Services/Common/PaginationExtensions.cs`** dù bộ tiện ích này đã được viết sẵn — mỗi service tự viết `Skip/Take` thủ công lặp lại. Ví dụ tốt về "DRY chưa triệt để" trong một dự án đang phát triển nhanh.
