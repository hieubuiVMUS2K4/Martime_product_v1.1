# Controllers/Inventory — Kho vật tư, nhập kho, phân tích nhiên liệu

## Mục đích

Quản lý danh mục vật tư (phụ tùng, vật liệu tiêu hao), tồn kho theo từng vị trí trên tàu, quy trình xin cấp vật tư (Material Request) và nhận hàng nhập kho, cùng module phân tích hiệu suất nhiên liệu/khí thải (CII/EEOI theo IMO). Đây là nhóm có **2 cơ chế "nhập kho" song song** cho cùng một khái niệm nghiệp vụ — điểm quan trọng nhất cần hiểu khi đọc thư mục này.

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `MaterialController.cs` (file lớn nhất, ~1140 dòng) | `api/material` | Danh mục vật tư (`categories`) cây cha-con + vật tư (`items`) — CRUD, tìm kiếm, ảnh, điều chỉnh tồn kho thủ công (`adjust-stock`), lịch sử hoạt động 1 vật tư, và **gán vật tư cho thiết bị** (`MaterialItemEquipment`, M:N, có kế thừa theo cây thiết bị cha-con). |
| `InventoryController.cs` | `api/inventory` | Tồn kho tổng hợp theo vị trí (join `InventoryStock`+`MaterialItem`+`StoreLocation`), tổng giá trị, xuất CSV, lịch sử nhập/xuất (gộp cả tiêu hao từ bảo trì `MaintenanceHistory.SparePartsUsed`), khai báo tồn kho ban đầu (`declare`), điều chỉnh thủ công (`adjust`). |
| `StoreLocationController.cs` | `api/store-locations` | CRUD vị trí kho theo cây phân cấp (Ship → Store → Sub-store). |
| `MaterialRequestController.cs` | `api/material-requests` | Quy trình **xin cấp vật tư**: Draft → Submit → Approve/Reject, danh sách request đã duyệt. |
| `StockReceiptController.cs` | `api/stock-receipts` | Quy trình **nhận hàng vật lý** (mã `NK-yyyyMMdd-XXX`) — có thể liên kết tới 1 `MaterialRequest` (`MaterialRequestId`), khi `PUT {id}/complete` sẽ cập nhật `InventoryStock` theo từng vị trí VÀ tự động đánh dấu `MaterialRequest` liên kết là `Completed`. |
| `MaterialReceiptsController.cs` | `api/materials/receipts` | Nhập kho **hàng loạt từ file Excel** (mã `PN-yyyyMMdd-XXX`) — xem trước (`preview`) rồi mới ghi thật (`import`), qua `MaterialReceiptService` (`Services/Inventory/`). |
| `FuelAnalyticsController.cs` | `api/fuel-analytics` | Toàn bộ endpoint phân tích nhiên liệu: hiệu suất (EEOI/SFOC/CII), xu hướng tiêu thụ, so sánh 2 kỳ, dự báo, xếp hạng CII, tổng hợp tuần/tháng, dashboard — 100% pass-through tới `FuelAnalyticsService` (`Services/Inventory/`, thuần tính toán không ghi DB). |

## Luồng hoạt động chính

### A. Hai cơ chế "nhập kho" khác nhau — dễ nhầm lẫn nếu không phân biệt

```
Cơ chế 1 — Nhận hàng theo yêu cầu (StockReceiptController):
  MaterialRequest (Draft→Submit→Approve)
        │  (yêu cầu đã duyệt, hàng về cảng)
        ▼
  POST /api/stock-receipts  { MaterialRequestId?, Items[] }     -- Status = "Draft"
  PUT  /api/stock-receipts/{id}/complete
        → CỘNG vào InventoryStock theo TỪNG StoreLocationId
        → Cộng MaterialItem.OnHandQuantity
        → Nếu có MaterialRequestId → tự đánh dấu MaterialRequest.Status = "Completed"

Cơ chế 2 — Nhập kho hàng loạt từ Excel (MaterialReceiptsController + MaterialReceiptService):
  POST /api/materials/receipts/preview   -- xem trước, phân loại CREATE (vật tư mới) / UPDATE / ERROR
  POST /api/materials/receipts/import    -- ghi thật, tạo MaterialItem mới nếu chưa có,
                                             cộng thẳng MaterialItem.OnHandQuantity
                                             (KHÔNG phân bổ theo StoreLocationId/InventoryStock)
```

Hai cơ chế này **không dùng chung 1 bảng** (`StockReceipt`/`StockReceiptItem` vs `MaterialReceipt`/`MaterialReceiptItem`), không dùng chung service, và cập nhật tồn kho theo 2 cách khác nhau (cơ chế 1 cập nhật `InventoryStock` theo vị trí cụ thể; cơ chế 2 chỉ cộng thẳng vào `MaterialItem.OnHandQuantity` tổng, không qua `InventoryStock`).

### B. Vòng đời Material Request

```
Draft --Submit--> Submitted --Approve--> Approved --(tạo StockReceipt liên kết)--> chờ hàng về
                       |
                       +--Reject--> Rejected
```

## Liên kết với phần khác

- **`Services/Inventory/README.md`** — `FuelAnalyticsService` (thuần đọc, không ghi DB) và `MaterialReceiptService` (cơ chế nhập kho Excel) là 2 service duy nhất của domain Inventory; phần lớn logic còn lại (Material, Inventory, StoreLocation, MaterialRequest, StockReceipt) nằm trực tiếp trong Controller, không tách Service riêng.
- **`Services/Maintenance/MaintenanceCompletionService`** — khi hoàn thành task bảo trì có dùng phụ tùng, service này TỰ trừ `InventoryStock` (chiến lược "vị trí tồn kho lớn nhất trước") — đây là đường TRỪ kho, đối xứng với đường CỘNG kho ở `StockReceiptController`/`MaterialReceiptsController` mô tả trên.
- **`InventoryController.GetHistory`** tự parse JSON `MaintenanceHistories.SparePartsUsed`/`MaintenanceTasks.SparePartsUsed` để hiển thị các dòng "OUT" do bảo trì tiêu hao — một cách tích hợp dữ liệu xuyên domain (Inventory đọc ngược từ Maintenance) thay vì có bảng giao dịch kho tập trung.
- **`Models/Inventory/`** — `FuelAnalyticsModels.cs` (bao gồm `IMOEmissionFactors` hệ số phát thải tĩnh) và `MaterialReceiptModels.cs`.

## Ghi chú khi đọc/dạy

- **Đây là ví dụ giáo khoa rõ ràng thứ hai (sau `Services/Reporting/Generators`) về "2 cách giải quyết cùng 1 bài toán cùng tồn tại"** — nhưng khác ở chỗ CẢ HAI đều đang được dùng thật (không phải 1 cái chết), chỉ là phục vụ 2 kịch bản nhập kho khác nhau (nhận hàng lẻ theo yêu cầu vs nhập hàng loạt từ Excel). Khi dạy, nên đặt câu hỏi: "nếu phải hợp nhất 2 cơ chế này thành 1, cần thay đổi gì ở tầng dữ liệu?"
- **`MaterialController.DeleteItem` không xoá thật** — soft-delete bằng cách set `IsActive=false` VÀ đổi `ItemCode` thêm hậu tố `_DELETED_{timestamp}` để "giải phóng" mã cũ cho vật tư mới dùng lại — một kỹ thuật cụ thể đáng chú ý (thay vì thêm cột `IsDeleted` riêng như các domain khác).
- **`InventoryStock` không có field `IsSynced`/`CreatedAt` đầy đủ** (xem `Models/README.md` — chỉ có `OriginNode`+`UpdatedAt`) — tồn kho hiện được đồng bộ Edge→Shore không đầy đủ như các entity khác; cần lưu ý khi debug sai lệch tồn kho giữa Edge và Shore.
- **`FuelAnalyticsController` là controller "mỏng" chuẩn mực** — toàn bộ 9 endpoint chỉ gọi 1 dòng tới `FuelAnalyticsService` tương ứng, tốt để dạy đối lập với `MaterialController` (rất "dày", nhiều logic trực tiếp trên `EdgeDbContext`).
