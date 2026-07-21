# pages/MaterialRequest — Yêu cầu cấp vật tư (Material Requests)

## Mục đích

`MaterialRequest/` là giao diện lập và duyệt **yêu cầu xin cấp vật tư** trên tàu — quy trình
`Draft → Submitted → Approved / Rejected`. Một yêu cầu gồm nhiều dòng vật tư (lấy từ danh mục ở
`pages/Material`), có thể gắn với **chuyến đi (voyage)** và **thiết bị (equipment)** để giải thích lý do cần vật
tư. Sau khi được duyệt (Approved), yêu cầu này là đầu vào cho khâu nhập kho ở `pages/StockReceipt`.

Đây là mắt xích giữa "danh mục vật tư" và "nhập kho thực tế" trong luồng logistics của PMS.

## Cấu trúc & vai trò

| File | Route (App.tsx) | Vai trò |
|---|---|---|
| `MaterialRequestPage.tsx` | `pms/logistics/material-requests` | Toàn bộ tính năng: danh sách yêu cầu + màn chi tiết (dùng chung 1 component, chuyển bằng `ViewMode = 'list' \| 'detail'`), tạo/sửa/gửi duyệt/duyệt/từ chối/xoá. Có component con `SearchableSelect` (dropdown tìm kiếm tự cuộn theo vị trí) khai báo ngay trong file |
| `MaterialRequestPage.old.tsx` | — | ⚠️ **Code chết** — không nơi nào import (bản cũ, giữ lại tham chiếu) |

## Luồng hoạt động chính

```
MaterialRequestPage  (pms/logistics/material-requests)
   ▼ list view:
     materialRequestService.getAll()   → GET /api/material-requests
   ▼ tạo mới (nạp dữ liệu nền để chọn):
     materialService.getItems()            → danh mục vật tư chọn vào từng dòng
     maritimeService (voyage) / equipmentAssetService → gắn voyage & thiết bị
     inventoryService                      → tham chiếu tồn hiện có
   ▼
   Vòng đời một yêu cầu (state machine):
     create  → materialRequestService.create()   → Draft      (backend sinh mã YC-<yyyyMMdd>-<seq>)
     update  → materialRequestService.update()    (còn ở Draft)
     submit  → materialRequestService.submit()   → Submitted   (khoá sửa, chờ duyệt)
     approve → materialRequestService.approve()  → Approved    (sẵn sàng cho nhập kho)
     reject  → materialRequestService.reject()   → Rejected
     delete  → materialRequestService.delete()   (soft delete)
```

Mỗi dòng vật tư (`MaterialRequestItem`) bắt buộc trỏ tới một `MaterialItemId` hợp lệ trong danh mục — backend
`ValidateItems` chặn submit/approve nếu có dòng không hợp lệ.

## Liên kết với phần khác

- **`services/materialRequest.service.ts`**: create, update, submit, approve, reject, delete, getAll, getById.
- Nhiều service phụ trợ để dựng form: **`materialService`** (danh mục vật tư), **`inventoryService`** (tồn kho),
  **`maritimeService`** (voyage), **`equipmentAssetService`** (cây thiết bị).
- **`config/app.config.ts`**: `VESSEL_CONFIG` (thông tin tàu hiện tại đưa vào yêu cầu).
- **`types/pms.types.ts`**: `MaterialRequest`, `MaterialRequestItem`, `EquipmentAsset`;
  **`types/maritime.types.ts`**: `MaterialItem`, `VoyageRecord`.
- **Hạ nguồn**: yêu cầu ở trạng thái `Approved` được `pages/StockReceipt` dùng để tạo phiếu nhập kho và tự đánh
  dấu request `Completed`.
- **Backend Edge** `Controllers/Inventory/MaterialRequestController.cs` — xem
  `edge-services/Controllers/Inventory/README.md`.

## Ghi chú khi đọc/dạy

- **Toàn bộ tính năng nằm trong 1 file** `MaterialRequestPage.tsx` (~52 KB, cả list lẫn detail lẫn `SearchableSelect`).
  Khi dạy, chỉ cho học viên khái niệm `ViewMode` để hiểu vì sao không có file `...DetailPage` riêng như các module khác.
- `MaterialRequestPage.old.tsx` là **code chết** — bỏ qua khi đọc.
- Phân biệt với 2 module lân cận: đây là **xin cấp** (yêu cầu), còn `pages/StockReceipt` là **nhập kho thực tế**
  (làm tăng tồn), và `pages/Material` là **danh mục**. Ba module nối tiếp nhau trong luồng logistics.
- Import mặc định là `export default` (khác phần lớn trang khác dùng named export) — App.tsx import
  `import MaterialRequestPage from ...` không có ngoặc nhọn.
- Mã yêu cầu `YC-` (Yêu Cầu) do **backend sinh**, frontend không tự đặt — đừng nhầm với mã nhập kho `NK-` bên
  `StockReceipt`.
