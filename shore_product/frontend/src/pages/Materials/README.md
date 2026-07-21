# pages/Materials — Quản lý kho vật tư

## Mục đích

Quản lý vật tư/phụ tùng cho toàn đội tàu: danh mục & tồn kho, yêu cầu cấp vật tư, nhập kho, và vị trí lưu trữ. Đây là module **cấu trúc phẳng nhất** trong `pages/` — không có thư mục con, không có `index.ts` barrel, không có file `.css` riêng (100% style bằng Tailwind, khác hầu hết module còn lại).

## Cấu trúc & vai trò

| File | Route | Vai trò | Service chính |
|---|---|---|---|
| `MaterialPage.tsx` | `/materials` | Danh mục vật tư: cây category + danh sách item, lọc theo tên/mã/category/đơn vị, phân trang, chọn nhiều dòng. Mở các modal con bên dưới. | `materialService` |
| `ItemFormModal.tsx` | *(modal trong MaterialPage)* | Thêm/sửa 1 vật tư (item). | `materialService` |
| `CategoryFormModal.tsx` | *(modal trong MaterialPage)* | Thêm/sửa danh mục (category), hỗ trợ cây cha/con. | `materialService` |
| `StockAdjustmentModal.tsx` | *(modal trong MaterialPage)* | Điều chỉnh tồn kho thủ công (Add/Subtract/Set) kèm lý do. | `materialService` (`adjustStock`) |
| `AssignEquipmentModal.tsx` | *(modal trong MaterialPage)* | Gắn vật tư ↔ thiết bị (equipment asset) — vật tư nào dùng cho máy nào. | `materialService`, `equipmentAssetService` |
| `ImportReceiptModal.tsx` | *(modal trong MaterialPage)* | Nhập kho nhanh kiểu "Import Receipt": preview rồi tạo Material Receipt. | `receiptService` |
| `ReceiptDetailModal.tsx` | *(modal, xem chi tiết receipt vừa tạo)* | Xem lại 1 Material Receipt đã tạo qua `ImportReceiptModal`. | `receiptService` |
| `StoreLocationPage.tsx` | `/materials/store-locations` | Cây vị trí lưu kho (kệ/khu vực, cha/con qua `parentId`), CRUD. | `store-location.service.ts` |
| `StoreLocationFormModal.tsx` | *(modal trong StoreLocationPage)* | Thêm/sửa 1 vị trí lưu kho. | `storeLocationService` |
| `MaterialRequestPage.tsx` | `/materials/requests` | Phiếu **yêu cầu vật tư**: list/create/edit/detail gộp trong 1 file (`ViewMode`), mức độ khẩn cấp (Normal/Urgent/Critical), trạng thái Draft→Submitted→Approved/Rejected→Completed, liên kết tới chuyến đi (`VoyageRecord`) và thiết bị. | `materialRequestService`, `materialService`, `maritimeService`, `equipmentAssetService` |
| `StockReceiptPage.tsx` | `/materials/receipts` | Phiếu **nhập kho** đầy đủ (list/create/edit/detail gộp 1 file), trạng thái Draft→Approved→Completed, có thể tạo từ 1 `MaterialRequest` đã duyệt. | `stockReceiptService`, `materialService`, `storeLocationService`, `materialRequestService`, `maritimeService` |
| `InventoryPage.tsx` | `/materials/inventory` | Tồn kho tổng hợp: tổng giá trị, tồn theo vị trí, lịch sử xuất/nhập theo item, cảnh báo tồn thấp. | `inventoryService`, `storeLocationService`, `materialService` |

## Luồng hoạt động chính

```
MaterialPage (danh mục gốc)
   │  categories + items  ──►  materialService (qua apiClient, có Bearer token)
   │
   ├─ ImportReceiptModal ──► receiptService.previewImport()/importReceipt()   [luồng nhập kho #1: "Material Receipt"]
   │
   └─ (điều hướng menu-ngoài-nav tới) ─────────────────────────────────────────────
                                                                                    │
StoreLocationPage ──► storeLocationService (axios)                                │
MaterialRequestPage ──► materialRequestService (axios) ──► trạng thái Approved ────┤
StockReceiptPage ──► stockReceiptService (axios)   [luồng nhập kho #2: "Stock Receipt", có thể xuất phát từ Request đã duyệt]
InventoryPage ──► inventoryService (axios) — tổng hợp lại số liệu từ cả 2 luồng trên
```

Tất cả service trong luồng này (trừ `materialService`/`receiptService` qua `apiClient`) dùng **axios trực tiếp**, tự động có `Authorization` header nhờ interceptor toàn cục đăng ký ở `services/maritime.service.ts`/`services/equipment-asset.service.ts` (xem `services/README.md`).

## Liên kết với phần khác

- **pages/VesselManagement/VesselDetailPage.tsx**: nhúng `MaterialPage`, `MaterialRequestPage`, `StockReceiptPage`, `InventoryPage` làm tab "Vật tư" — đây là lối vào chính người dùng thực tế sẽ dùng (menu chính `TopNavLayout` không có mục nào trỏ tới `/materials/*` — xem ghi chú trong `pages/README.md`).
- **components/pms/**: `AddScheduleModal`/`EditScheduleModal` (PMS) gọi `materialService` để gắn phụ tùng vào lịch bảo trì — Materials và PMS chia sẻ cùng danh mục vật tư.
- **services/**: xem bảng chi tiết ở `services/README.md` — đặc biệt mục "Hai luồng nhập kho song song".
- **contexts/I18nContext**: Materials là module dùng `useTranslationSafe()` nhất quán nhất trong toàn app (mọi trang ở đây đều gọi `const { t } = useTranslationSafe()`), khác phần lớn nơi khác hard-code tiếng Việt thẳng trong JSX.

## Ghi chú khi đọc/dạy

- **Không có `index.ts`** — khác 17 module còn lại trong `pages/`. `routes/AppRoutes.tsx` phải import trực tiếp từng file (`import StoreLocationPage from '../pages/Materials/StoreLocationPage'`...). Nếu quen thói quen "cứ tìm `pages/Materials/index.ts` để biết trang nào được export", sẽ không thấy gì — phải mở từng file.
- **Hai luồng "nhập kho" độc lập, dễ gây nhầm lẫn khi debug**: `ImportReceiptModal`/`ReceiptDetailModal` (gọi `receiptService.ts`, khái niệm "Material Receipt", nằm ngay trong `MaterialPage`) và `StockReceiptPage.tsx` (gọi `stockReceiptService.ts`, khái niệm "Stock Receipt", trang riêng ở `/materials/receipts`). Cả hai đều tạo ra bản ghi "nhập kho vật tư" nhưng qua 2 bảng/endpoint khác nhau phía Backend — khi vật tư trong kho có số liệu không khớp kỳ vọng, kiểm tra xem đã nhập qua luồng nào.
- `MaterialRequestPage.tsx` và `StockReceiptPage.tsx` đều tự quản lý 4 chế độ xem (`list`/`create`/`edit`/`detail`) **ngay trong một file** bằng state `ViewMode`, thay vì tách route con (`/materials/requests/new`, `/materials/requests/:id`...) như `VoyageManagement` hay `AssignmentManagement` làm. Khi cần thêm bước trong quy trình, sửa trực tiếp trong file đó, không tìm "trang con" riêng.
- `StoreLocationPage.tsx` và `pages/PMS/AssetsPage.tsx` có cùng một đoạn hàm `buildTree()`/`getDescendantIds()` xây cây từ danh sách phẳng theo `parentId` — code trùng lặp giữa 2 module, ứng viên tốt để tách thành hàm dùng chung trong `utils/` (hiện đang rỗng — xem `utils/README.md`).
