# Controllers/Logbooks — 9 sổ nhật ký hàng hải bắt buộc (SOLAS/MARPOL/BWM/STCW)

## Mục đích

Expose API REST cho 9 loại sổ ghi chép mà công ước quốc tế bắt buộc tàu phải duy trì và có thể bị Port State Control (PSC) kiểm tra bất cứ lúc nào. Đây là ví dụ rõ ràng nhất trong cả dự án về **một khuôn mẫu API lặp lại y hệt cho nhiều domain khác nhau** — hiểu 1 controller là hiểu gần hết cả nhóm.

## Cấu trúc & vai trò

| File | Route | Tiêu chuẩn tuân thủ | Model |
|---|---|---|---|
| `DeckLogbookController.cs` | `api/logbooks/deck` | SOLAS Chapter V | `DeckLogBook` |
| `EngineLogbookController.cs` | `api/logbooks/engine` | — | `EngineLogBook` |
| `OilRecordController.cs` | `api/logbooks/oil` | MARPOL Annex I | `OilRecordBook` |
| `GarbageRecordController.cs` | `api/logbooks/garbage` | MARPOL Annex V (bản gộp) | `GarbageRecordBook` |
| `GarbagePartIController.cs` | `api/logbooks/garbage/part-i` | MARPOL Annex V, mục A-I | `GarbageRecordPartI` |
| `GarbagePartIIController.cs` | `api/logbooks/garbage/part-ii` | MARPOL Annex V, mục J-K | `GarbageRecordPartII` |
| `BallastWaterController.cs` | `api/logbooks/ballast` | BWM Convention | `BallastWaterRecordBook` |
| `WatchkeepingController.cs` | `api/logbooks/watchkeeping` | STCW/SOLAS V | `WatchkeepingLog` |
| `AbstractLogController.cs` | `api/logbooks/abstract-log` | — (tổng hợp nội bộ, không phải sổ pháp lý riêng) | `AbstractLogVoyage`/`Leg`/`DailyEntry` — xem mục riêng bên dưới, khác hẳn 8 controller kia. |

## Luồng hoạt động chính

### A. Khuôn mẫu chung cho 8/9 controller (trừ `AbstractLogController`)

```
POST   api/logbooks/{loai}            CreateEntry(CreateXxxDto)     → 201 + { id, message }
GET    api/logbooks/{loai}/{id}       GetEntry(id)                  → 200 hoặc 404
GET    api/logbooks/{loai}?page=&pageSize=&fromDate=&toDate=&searchTerm=
                                       GetEntries(...)               → danh sách phân trang (page≤100)
PUT    api/logbooks/{loai}/{id}       UpdateEntry(id, UpdateXxxDto) → chặn nếu đã ký
DELETE api/logbooks/{loai}/{id}       DeleteEntry(id)               → soft delete
POST   api/logbooks/{loai}/{id}/sign  SignEntry(id, SignXxxDto)     → chữ ký Master/C.E
```

Mỗi controller chỉ tiêm 1 service tương ứng qua interface (`IDeckLogbookService`, `IOilRecordService`...) — không có logic nghiệp vụ nào nằm trong chính controller, toàn bộ validate/soft-delete/auto-link voyage nằm ở `Services/Logbooks/` (xem README đó).

### B. `AbstractLogController` — ngoại lệ, cấu trúc 3 cấp

Khác hẳn 8 controller kia (không có endpoint Create/Get/Update/Delete/Sign đơn giản), `AbstractLogController` quản lý cấu trúc 3 cấp lồng nhau:

```
GET/POST/PUT/DELETE  api/logbooks/abstract-log[/{id}]        -- cấp Voyage (header + SUM sheet)
POST                 api/logbooks/abstract-log/{id}/auto-fill    -- kéo dữ liệu từ NoonReport/EngineLogBook
POST                 api/logbooks/abstract-log/{id}/recalculate  -- tính lại tổng hợp bottom-up
GET                  api/logbooks/abstract-log/{id}/export/excel  -- ClosedXML
GET                  api/logbooks/abstract-log/{id}/export/pdf    -- QuestPDF

POST/PUT/DELETE      api/logbooks/abstract-log/legs[/{legId}]         -- cấp Leg (chặng hải trình)
POST/PUT/DELETE      api/logbooks/abstract-log/legs/{legId}/entries[/{entryId}]  -- cấp Daily Entry
```

## Liên kết với phần khác

- **`Services/Logbooks/README.md`** — mọi business logic (validate MARPOL Annex V cho Garbage Part I/II, auto-link voyage, soft-delete, chặn sửa khi đã ký) nằm ở đây.
- **`DTOs/README.md`** (mục `DTOs/Logbooks/`) — 4 DTO/loại (`CreateXxxDto`/`UpdateXxxDto`/`XxxResponseDto`/`SignXxxDto`) dùng cho request/response.
- **`Controllers/Safety/ComplianceController`** — đọc lại dữ liệu Watchkeeping/Oil Record Book (chỉ đọc, không ghi) cho mục đích báo cáo tuân thủ tổng hợp.
- **`Services/AbstractLog/AbstractLogService`** — auto-fill kéo dữ liệu từ `EngineLogBook` (ghi bởi `EngineLogbookController`) và `NoonReport` (ghi bởi `Controllers/Reporting/ReportingController`).

## Ghi chú khi đọc/dạy

- **Cách dạy hiệu quả nhất**: chọn `GarbagePartIController`/`GarbagePartIIController` để giảng đầu tiên (có validate MARPOL Annex V rõ ràng nhất ở tầng service), sau đó chỉ ra 6 controller còn lại dùng ĐÚNG khuôn HTTP giống hệt nhưng tầng service phía sau ít/không có validate đặc thù.
- **`GarbageRecordController` (sổ "gộp") và `GarbagePartIController`/`GarbagePartIIController` (sổ chia theo MARPOL Annex V) cùng tồn tại** — không phải trùng lặp thừa, mà là 2 mức độ chi tiết khác nhau của cùng một nghĩa vụ tuân thủ (nên xác nhận với nghiệp vụ thật xem tàu đang dùng sổ nào là chính thức).
- **`AbstractLogController` không theo route pattern `{loai}/{id}/sign` như 8 controller kia** — đừng cố suy luận API của nó từ khuôn mẫu chung, cần đọc riêng.
- Toàn bộ 9 controller trong thư mục này **không có `[Authorize]` tường minh trên từng action** — quyền truy cập dựa vào `SessionAuthMiddleware` áp dụng toàn cục (yêu cầu Bearer token hợp lệ cho mọi route không nằm trong whitelist).
