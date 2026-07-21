# pages/Material — Danh mục vật tư & tồn kho (Materials Catalog)

## Mục đích

`Material/` là giao diện quản lý **danh mục vật tư (material items)** và **nhóm vật tư (categories)** trên tàu:
thêm/sửa/xoá vật tư, phân loại theo cây danh mục, theo dõi tồn kho thấp, điều chỉnh số lượng, gán vật tư cho
thiết bị (phục vụ PMS), và nhập kho hàng loạt từ file Excel. Đây là "danh mục gốc" (catalog) — mọi module kho
khác (`MaterialRequest`, `StockReceipt`, `Inventory`) đều tham chiếu tới các vật tư khai báo ở đây.

## Cấu trúc & vai trò

| File | Route (App.tsx) | Vai trò |
|---|---|---|
| `MaterialPage.tsx` | `pms/catalog/materials` | Trang chính: bảng danh mục vật tư (lọc theo tên/mã/nhóm/ĐVT, phân trang 25 dòng, chọn nhiều dòng để xoá hàng loạt), mở các modal bên dưới |
| `ItemFormModal.tsx` | — | Thêm/sửa một vật tư (mã, tên, nhóm, ĐVT, tồn min/max, ảnh...) |
| `CategoryFormModal.tsx` | — | Thêm/sửa **nhóm vật tư** (cây danh mục có cha-con) |
| `ImportReceiptModal.tsx` | — | Nhập kho hàng loạt từ **Excel** (preview trước khi commit) |
| `AssignEquipmentModal.tsx` | — | Gán vật tư ↔ thiết bị (quan hệ nhiều-nhiều, phục vụ tra "thiết bị này cần vật tư gì") |
| `ReceiptDetailModal.tsx` | — | Xem chi tiết một phiếu nhập kho (dùng `receiptService`) |
| `StockAdjustmentModal.tsx` | — | Điều chỉnh tồn kho thủ công (ADD/SUBTRACT/SET) |
| `oldMaterialPage.tsx` | — | ⚠️ **Code chết** — không nơi nào import (bản cũ của `MaterialPage`, giữ lại làm tham chiếu) |

## Luồng hoạt động chính

```
MaterialPage  (pms/catalog/materials)
   ▼ loadData()  — gọi song song (Promise.all):
     materialService.getItems({ onlyActive:true })   → GET /api/material/items
     materialService.getCategories(true)             → GET /api/material/categories
     materialService.getEquipmentCounts()            → số thiết bị gắn mỗi vật tư
   ▼
   render bảng vật tư (lọc + phân trang + chọn dòng)
   ▼ các hành động → mở modal → gọi materialService → loadData() lại:
     Thêm/sửa vật tư   → createItem / updateItem
     Xoá (đơn/hàng loạt) → deleteItem
     Điều chỉnh tồn    → adjustStock          (StockAdjustmentModal)
     Gán thiết bị      → assignEquipment / removeEquipmentLink  (AssignEquipmentModal)
     Nhập Excel        → (ImportReceiptModal → receiptService)
     Ảnh vật tư        → uploadItemImage / deleteItemImage
```

Tất cả đi qua một service duy nhất: **`services/materialService.ts`** (dùng `apiClient`), gọi tới nhóm endpoint
`/api/material/*` của Edge backend.

## Liên kết với phần khác

- **`services/materialService.ts`**: getItems, getCategories, createItem, updateItem, deleteItem, adjustStock,
  assignEquipment, removeEquipmentLink, getEquipmentCounts, getItemEquipment, getItemActivity, getLowStockItems,
  uploadItemImage, deleteItemImage, createCategory, updateCategory, deleteCategory.
- **`services/receiptService`** (qua `ReceiptDetailModal`): xem chi tiết phiếu nhập — lưu ý có **2 service nhập kho
  song song** trong dự án (`receiptService` vs `stockReceipt.service`), xem `services/README.md`.
- **`pages/MaterialRequest/`** & **`pages/StockReceipt/`** & **`pages/Inventory/`**: đều tiêu thụ danh mục vật tư
  khai báo ở đây.
- **PMS**: quan hệ vật tư↔thiết bị (`AssignEquipmentModal`) phục vụ tính "vật tư cần cho công việc bảo trì".
- **`types/maritime.types.ts`**: `MaterialItem`, `MaterialCategory`.
- **Backend Edge** `Controllers/Inventory/MaterialController.cs` — xem `edge-services/Controllers/Inventory/README.md`.

## Ghi chú khi đọc/dạy

- Phân biệt rõ **3 khái niệm kho** dễ lẫn: *Material item* (danh mục — ở đây) ≠ *Inventory stock* (số lượng tồn
  theo vị trí — `pages/Inventory`) ≠ *Material request* (yêu cầu xin cấp — `pages/MaterialRequest`). Trang này chỉ
  quản lý **danh mục + tồn tổng**, không quản lý tồn theo vị trí kho.
- `oldMaterialPage.tsx` là **code chết** — đừng sửa nhầm vào file này khi thấy nó xuất hiện trong tìm kiếm.
- Nút "Import" mở `ImportReceiptModal` thực chất là **nhập kho từ Excel** (tạo phiếu nhập + cập nhật vật tư), không
  phải chỉ import danh mục — dễ gây hiểu nhầm về chức năng.
- Xoá vật tư ở backend là **soft-delete kèm đổi mã** (`ItemCode` được thêm hậu tố `_DELETED_<timestamp>` để giải
  phóng mã cho vật tư mới) — nên sau khi xoá, mã cũ có thể tái sử dụng ngay. Chi tiết logic nằm ở phía backend.
- Gán thiết bị có tính **kế thừa theo cây thiết bị** ở backend (vật tư gán cho thiết bị cha áp dụng cho cả thiết bị
  con) — UI chỉ hiển thị kết quả, logic thật ở `MaterialController.GetMaterialsByEquipment`.
