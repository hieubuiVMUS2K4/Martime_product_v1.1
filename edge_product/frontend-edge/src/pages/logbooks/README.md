# pages/logbooks — Sổ nhật ký SOLAS/MARPOL

## Mục đích

Đây là nhóm trang triển khai các **sổ nhật ký chính thức** mà tàu bắt buộc phải ghi theo SOLAS/MARPOL/ISM
Code: nhật ký boong (Deck Log), nhật ký máy (Engine Log), Sổ Nhật ký Dầu (Oil Record Book), nhật ký trực ca
(Watchkeeping), nhật ký rác (Garbage Record — MARPOL Annex V), nhật ký nước dằn tàu (Ballast Water), nhật
ký hành trình theo sự kiện (Voyage Log), và nhật ký tổng hợp theo chuyến đi (Abstract Log). Đa số các sổ có
cơ chế **ký xác nhận điện tử** (Master/Chief Engineer) trước khi coi là chính thức, không thể sửa.

## Cấu trúc & vai trò

| File | Route | Sổ nhật ký | Đặc điểm riêng |
|---|---|---|---|
| `DeckLogPage.tsx` | `/logbooks/deck` | Deck Log | Ghi theo ca trực (`watchPeriod`), vị trí, thời tiết, diễn tập |
| `EngineLogPage.tsx` | `/logbooks/engine` | Engine Log | Thông số máy, ký bởi Chief Engineer (`chiefEngineerSignature`) |
| `OilRecordPage.tsx` | `/logbooks/oil` | Oil Record Book | Bắt buộc theo MARPOL Annex I |
| `GarbageManagementPage.tsx` | `/logbooks/garbage` | Garbage Record (MARPOL Annex V) | 2 tab **Part I** (nhóm rác A–I) / **Part II** (nhóm J–K, dư lượng hàng hoá) — dùng `components/logbooks/GarbagePartIForm.tsx` + `GarbagePartIIForm.tsx` |
| `GarbageRecordPage.tsx` | `/logbooks/garbage-old` | Garbage Record (bản cũ) | Route vẫn tồn tại (`garbage-old`) nhưng không xuất hiện trong menu điều hướng chính — xem lưu ý |
| `BallastWaterPage.tsx` | `/logbooks/ballast` | Ballast Water Record | Quản lý nước dằn tàu |
| `WatchkeepingPage.tsx` | `/logbooks/watchkeeping` | Watchkeeping Log | Nhật ký trực ca |
| `VoyageLogPage.tsx` | `/logbooks/voyage` | Voyage Log | Nhật ký theo **sự kiện** (xem luồng bên dưới), 2 chế độ xem timeline/bảng |
| `VoyageLogDetailPage.tsx` | `/logbooks/voyage/:id` | Voyage Log (chi tiết) | Xem/sửa 1 sự kiện |
| `AbstractLogPage.tsx` | `/logbooks/abstract(/:id)` | Abstract Log | Nhật ký **tổng hợp theo chuyến đi** (chọn Voyage → xem các Leg → daily entries), xuất Excel/PDF |

## Luồng hoạt động chính

Tất cả các trang (trừ Abstract Log) dùng chung **một service duy nhất**: `services/logbook.service.ts`
(`apiClient`), theo khuôn mẫu CRUD giống nhau:

```
*LogPage (vd. DeckLogPage)
   ▼
logbookService.getDeckEntries({ page, pageSize })   → GET /api/logbooks/deck
   ▼
render bằng components/common/LogbookGrid.tsx (bảng dùng chung mọi loại logbook)
   ▼ (thêm/sửa)
logbookService.createDeckEntry(dto) / updateDeckEntry(id, dto)  → POST|PUT /api/logbooks/deck
   ▼ (ký chính thức)
logbookService.signDeckEntry(id, { signature, signedAt })  → POST /api/logbooks/deck/:id/sign
```

Mỗi loại sổ có DTO + endpoint riêng trong `logbook.service.ts` (`getEngineEntries/createEngineEntry/signEngineEntry`,
`getOilEntries/...`, `getGarbagePartI.../getGarbagePartII...`, v.v.) nhưng **cùng pattern hàm** — học một
loại là hiểu được tất cả.

### Voyage Log — nhật ký theo sự kiện

`VoyageLogPage` khác các sổ còn lại: thay vì "một dòng mỗi ca trực", nó ghi **sự kiện rời rạc** theo
`VOYAGE_LOG_EVENT_TYPES` (nhóm trong `types/logbook.types.ts`): sự kiện cảng (DEP/ARR/thả neo/kéo neo), sự
kiện hành trình (COSP/EOSP/NOON), sự kiện hoa tiêu (PILOT_ON/OFF), sự kiện đặc biệt (trôi dạt/chệch hướng).
Giao diện có 2 chế độ xem: `'timeline'` (dòng thời gian trực quan) và `'table'` (bảng dữ liệu thô).

### Abstract Log — tổng hợp theo chuyến đi

`AbstractLogPage` không thuộc luồng CRUD từng-dòng như trên — nó tổng hợp: chọn 1 `Voyage` → hiển thị các
**Leg** (chặng) → mỗi Leg có các **Daily Entry**. Dùng `services/abstractlog.service.ts` (khác
`logbook.service.ts`) và `components/common/VirtualizedTable.tsx` (dựa trên `react-window`) để render danh
sách lớn mà không giật lag. Có nút xuất Excel/PDF gọi trực tiếp:

```
abstractLogService.exportExcel(id) → fetch(`${API_CONFIG.BASE_URL}/logbooks/abstract-log/:id/export/excel`) → nhận Blob → tải file
```

(gọi `fetch` trực tiếp thay vì qua `apiClient`, vì cần nhận `Blob` thay vì JSON).

## Liên kết với phần khác

- **`services/logbook.service.ts`**: service trung tâm cho 7/8 loại sổ (trừ Abstract Log).
- **`services/abstractlog.service.ts`**: riêng cho Abstract Log + export Excel/PDF.
- **`types/logbook.types.ts`**, **`types/abstractlog.types.ts`**: toàn bộ DTO tương ứng.
- **`components/common/LogbookGrid.tsx`, `MaritimeInput.tsx`, `CoordinatePicker.tsx`,
  `VirtualizedTable.tsx`**: UI dùng chung — mọi trang logbook đều dựng trên các component atomic này thay vì
  tự vẽ bảng riêng.
- **`components/logbooks/GarbagePartIForm.tsx` / `GarbagePartIIForm.tsx`**: 2 form phức tạp riêng cho
  Garbage Record — xem `components/logbooks/README.md`.
- **README gốc dự án** (mục 4.1, "Logbook (7)"): liệt kê các controller backend tương ứng
  (`DeckLogbook, EngineLogbook, BallastWater, GarbageRecord, OilRecordBook, Watchkeeping, AbstractLog`).

## Ghi chú khi đọc/dạy

- **`GarbageRecordPage.tsx` là bản cũ**: route `/logbooks/garbage-old` vẫn còn khai báo trong `App.tsx`
  nhưng không có link nào trong Sidebar trỏ tới; trang đang dùng thật là `GarbageManagementPage.tsx`
  (`/logbooks/garbage`, có 2 tab Part I/II theo đúng MARPOL Annex V). Khi tìm hiểu nghiệp vụ rác thải, luôn
  vào `GarbageManagementPage`, không phải `GarbageRecordPage`.
- MARPOL Annex V chia rác thành nhóm A–I (Part I, khai ngay trên tàu) và J–K (Part II, riêng cho dư lượng
  hàng hoá/nước rửa hầm hàng) — nhóm H và K (residue có hại — HME) **bị cấm xả xuống biển tuyệt đối**
  (`seaDischarge: false`), phản ánh đúng trong logic disable ô "lượng xả xuống biển" của
  `GarbagePartIForm`/`GarbagePartIIForm` khi chọn nhóm đó.
- Abstract Log và các sổ nhật ký khác **không dùng chung service** dù cùng nằm trong `pages/logbooks/` — dễ
  nhầm khi tìm "tại sao sửa `logbook.service.ts` không ảnh hưởng Abstract Log".
- Chữ ký (`sign*Entry`) chỉ là chuỗi text + timestamp gửi lên backend (giống cơ chế PIN ở HSQE) — không phải
  chữ ký số PKI.
