# Controllers/Materials — Kho vật tư / phụ tùng

## Mục đích

Quản lý danh mục vật tư/phụ tùng dùng cho tàu (catalog), tồn kho theo từng vị trí lưu trữ, yêu cầu cấp phát vật tư, và phiếu nhập kho. Đây là domain "kho hàng" (warehouse), tách biệt với `Controllers/Pms/` (bảo trì thiết bị) dù hai domain có liên hệ (xem phần cuối). Khác với `Crew/`/`CrewManagement/`, **không controller nào trong thư mục này gắn `[Authorize]`** — toàn bộ 5 file mở hoàn toàn theo thiết kế hiện tại.

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `MaterialController.cs` | `api/material` | Danh mục vật tư (categories phân cấp + items), và liên kết nhiều-nhiều item↔thiết bị PMS |
| `StoreLocationsController.cs` | `api/store-locations` | Danh mục vị trí lưu trữ, phân cấp qua `ParentId` (kho tổng → kho con → kệ...) |
| `InventoryController.cs` | `api/inventory` | Sổ tồn kho (`InventoryStock`) theo cặp item+vị trí: xem tồn, khai báo lại (declare), điều chỉnh (adjust), tổng hợp theo vị trí |
| `MaterialRequestsController.cs` | `api/material-requests` | Phiếu yêu cầu cấp vật tư (thường do tàu gửi lên), mỗi phiếu có nhiều dòng, có thể gắn `MaterialItemId` và/hoặc `EquipmentAssetId` |
| `StockReceiptsController.cs` | `api/stock-receipts` | Phiếu nhập kho (hàng về, có thể liên kết ngược tới `MaterialRequest` gốc), khi "complete" sẽ cộng vào tồn kho |

## Luồng hoạt động chính

```
MaterialRequestsController.Create   → tạo phiếu yêu cầu (mã MR-yyyyMMdd-xxxx), status Draft
        │
        ▼ .../submit
                                      status = Submitted
        │  (KHÔNG có endpoint /approve riêng — duyệt bằng cách PUT status="Approved" tay,
        │   không có transition-table kiểm tra hợp lệ)
        ▼
StockReceiptsController.Create      → tạo phiếu nhập (mã SR-yyyyMMdd-xxxx), có thể tham chiếu
                                       MaterialRequest ở trên
        │
        ▼ .../complete   ★ ĐÂY LÀ NƠI DUY NHẤT ĐỒNG BỘ SỐ LIỆU ĐÚNG ★
                                      với mỗi dòng có MaterialItemId + StoreLocationId:
                                      - upsert InventoryStock (cộng dồn Quantity, ghi UnitCost/LastReceiptDate)
                                      - CỘNG THÊM vào MaterialItem.OnHandQuantity (tổng toàn hệ thống)
```

`InventoryController.Declare`/`Adjust` cũng sửa `InventoryStock.Quantity` nhưng **không** đụng vào `MaterialItem.OnHandQuantity` — xem cảnh báo bên dưới.

## Liên kết với phần khác

- Toàn bộ 5 controller gọi thẳng `AppDbContext`, **không qua lớp Service riêng** (khác với phần lớn `CrewManagement/`) — nghiệp vụ (sinh mã phiếu, cập nhật tồn kho...) nằm ngay trong controller.
- Liên hệ với `Controllers/Pms/`: bảng `MaterialItemEquipment` (item ↔ `EquipmentAsset`) và `ScheduleSparePart` (bên trong `MaintenanceSchedule`, chỉ đọc qua `Pms/MaintenanceSchedulesController`) là hai điểm nối quan hệ giữa vật tư và thiết bị — xem "Ghi chú" bên dưới để biết chính xác nối ở đâu và **không** nối ở đâu.
- `MaterialRequestItem` có `EquipmentAssetId` tùy chọn — cho phép một dòng yêu cầu vật tư ghi rõ "phụ tùng này dùng cho thiết bị nào".

## Ghi chú khi đọc/dạy

- **Cảnh báo lệch số liệu (đã xác minh trong code, không phải suy đoán):** `InventoryController.Declare`/`Adjust` chỉ sửa `InventoryStock.Quantity` (tồn theo từng vị trí), KHÔNG cập nhật `MaterialItem.OnHandQuantity` (tổng tồn trên đầu item, dùng để lọc "sắp hết hàng" ở `MaterialController`). Trong khi đó `StockReceiptsController.Complete()` cập nhật CẢ HAI cùng lúc. Kết quả: nếu ai đó chỉnh tồn kho qua `/declare` hoặc `/adjust` thay vì qua phiếu nhập, `MaterialItem.OnHandQuantity` sẽ dần lệch khỏi tổng thực tế của `InventoryStock`. Đây là một bug/bẫy thật sự trong code hiện tại, không phải giả thuyết — cần biết trước khi debug các báo cáo "sai số tồn kho".
- **Sinh mã phiếu không kiểm tra trùng.** `MaterialRequestsController`/`StockReceiptsController` sinh mã bằng `new Random().Next(1000, 9999)` nối với ngày, không kiểm tra unique/retry (khác với `MaterialCategory`/`StoreLocation`, có kiểm tra trùng mã trước khi insert) — nguy cơ trùng mã nhỏ nhưng có thật khi nhiều phiếu được tạo cùng ngày.
- **`GET /api/inventory/history` không phải log thay đổi thật** — nó chỉ tái sử dụng danh sách `StockReceipts` đã hoàn thành làm "lịch sử", không phải audit trail đầy đủ mọi thao tác tồn kho (declare/adjust không xuất hiện ở đây).
- **`GET /api/inventory/export` không thực sự xuất CSV** — trả JSON thô, comment trong code ghi rõ "frontend handles actual CSV generation".
- **Vật tư ↔ Thiết bị (PMS) nối ở tầng KẾ HOẠCH, không nối ở tầng THỰC THI:** `MaterialItemEquipment` (item ↔ thiết bị) và `ScheduleSparePart` (mẫu lịch bảo trì ↔ item, có `QuantityRequired`) là quan hệ khóa ngoại thật. Nhưng `MaintenanceTask` (task bảo trì thực tế, đồng bộ read-only từ Edge — xem `Controllers/Pms/README.md`) chỉ lưu `RequiredSpareParts`/`SparePartsUsed` dưới dạng **chuỗi text/JSON**, không phải khóa ngoại — nghĩa là hoàn thành một task bảo trì **không** tự động trừ tồn kho ở phía Shore.
