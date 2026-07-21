# pages/HSQE — Quản lý tài liệu SMS/ISM, Sự cố & Rủi ro

## Mục đích

HSQE (Health, Safety, Quality, Environment) là trang tuân thủ **ISM Code** trên Edge: quản lý tài liệu Hệ
thống Quản lý An toàn (SMS — Safety Management System) theo cấu trúc chương/thủ tục của ISM Code, biểu mẫu
điền tay có chữ ký số (PIN), theo dõi sự cố (Incident/CAPA — Corrective and Preventive Action), và đánh giá
rủi ro/giấy phép làm việc (Risk Assessment & Work Permit — Hot Work, Enclosed Space, Aloft...).

Đây là khu vực **nhiều tầng con nhất** trong `pages/`. Bản đồ thư mục:

```
pages/HSQE/
├── HSQEPage.tsx                        # Shell 3 tab: Documents | Incidents & CAPA | Risk & Permits
└── components/
    ├── DocumentControl.tsx             # Re-export wrapper — trỏ thẳng sang SmsDocumentPage (xem dưới)
    ├── SmsDocumentPage.tsx             # ~330KB — "workspace" SMS chính (xem mục riêng)
    ├── IncidentManagement.tsx          # Tab Sự cố & CAPA — CRUD độc lập, đơn giản
    ├── RiskWorkPermits.tsx             # Tab Đánh giá rủi ro & Giấy phép — CRUD độc lập, đơn giản
    ├── sms-document/
    │   ├── forms/FormTL1501.tsx        # Biểu mẫu ISM dựng cứng (hard-coded) cho form "TL-15-01"
    │   └── modals/                     # AssignModal, NewFormModal, RecordDetailModal, TemplateSelectorModal
    └── DocumentLibrary/                # Module Document Library kiểu ISO 9001 — XEM CẢNH BÁO bên dưới
        ├── DocumentLibraryPage.tsx
        ├── DocumentSidebar.tsx
        ├── types.ts
        └── tabs/ (DataTab, DocumentTab, AttachedDocumentsTab, ReadLogsTab, ChapterVersionsTab, HistoryTab)
```

## Cấu trúc & vai trò

| Thành phần | Route | Vai trò |
|---|---|---|
| `HSQEPage.tsx` | `/safety/hsqe` | Shell chuyển tab (state `useState`, không dùng router con) giữa 3 module |
| `DocumentControl` (→ `SmsDocumentPage`) | tab "Kiểm soát Tài liệu" | Workspace SMS chính — xem chi tiết bên dưới |
| `SmsDocumentPage` (route riêng) | `/safety/hsqe/form/:templateId` | Cùng component, mở trực tiếp một form theo `templateId` (deep-link) |
| `IncidentManagement` | tab "Sự cố & CAPA" | Khai báo sự cố (Accident/Incident/Near-Miss/Non-Conformity/PSC-Deficiency), phân tích 5 Whys, theo dõi CAPA |
| `RiskWorkPermits` | tab "Đánh giá Rủi ro & Giấy phép" | Đánh giá rủi ro theo bước (initial/residual Likelihood × Severity), giấy phép làm việc (Hot/Enclosed/Aloft/Cold) |
| `DocumentLibrary/*` | *(không có route)* | Module thay thế/tương lai cho quản lý tài liệu kiểu ISO 9001 — **hiện chưa được gắn vào App.tsx hay HSQEPage** |

### `SmsDocumentPage.tsx` — workspace SMS chính (chi tiết)

Đây là file **lớn nhất toàn bộ frontend-edge** (~330KB, hàng nghìn dòng, vượt giới hạn đọc một lần) — chứa 3
chế độ xem (`viewMode`):

| `viewMode` | Nội dung |
|---|---|
| `'workspace'` | Cây Chương (ISM element) → Thủ tục (procedure) → soạn thảo SOP bằng `RichTextEditor` (tiptap), quản lý phiên bản (version bump, diff HTML bằng `tokenizeHtml`/`diffHtml` tự viết), acknowledgement (thuyền viên xác nhận đã đọc), import DOCX (`smsService.importDocx`) |
| `'auditor'` | Duyệt danh sách **bản ghi đã điền** (`SmsFilledRecord`) theo chương/trạng thái — phục vụ vai trò kiểm toán/DPA |
| `'forms'` | Thư viện biểu mẫu (form template) gắn với từng thủ tục — tạo mới, gán (assign) biểu mẫu cho thủ tục |

Biểu mẫu được điền dựa trên `contentSchema` (JSON) lưu trong `SmsFormTemplate` — nghĩa là phần lớn form
render **động** theo schema. Tuy nhiên `sms-document/forms/FormTL1501.tsx` cho thấy có ít nhất một biểu mẫu
được **viết tay/cứng** (form "TL-15-01 — Kế hoạch nhận nhiên liệu / Bunkering Plan") thay vì render từ schema
— tức là hệ thống form đang ở dạng **lai (hybrid)**: một số form dùng renderer JSON chung, một số form đặc
thù được code riêng khi bố cục quá phức tạp cho renderer chung.

Quy trình ký số: người dùng nhập `pin + name + rank` → `smsService.signRecord()` /
`smsService.approveRecord()` → backend trả về mảng `signatures: SignatureEntry[]` (không phải chữ ký ảnh,
mà là bản ghi tên/hạng/thời điểm/mã ký — kiểu "chữ ký điện tử bằng PIN" chứ không phải chữ ký số PKI).

## Luồng hoạt động chính

```
HSQEPage (tab "Kiểm soát Tài liệu")
   ▼
DocumentControl  (chỉ re-export) → SmsDocumentPage
   ▼
smsService.getSmsTree()          → GET /api/sms/tree        → cây Chương/Thủ tục hiển thị bên trái
smsService.getProcedure(id)      → GET /api/sms/procedures/:id → nội dung SOP hiển thị bằng RichTextEditor
   ▼ (người dùng điền form)
smsService.createFilledRecord()  → POST /api/sms/records    → tạo SmsFilledRecord (status Draft)
smsService.signRecord()/approveRecord() → POST .../sign, .../approve → cập nhật chữ ký + trạng thái
   ▼
In tài liệu: lib/printUtils.ts (printSmsDocument) mở cửa sổ in riêng, không qua React render thường
```

`IncidentManagement` và `RiskWorkPermits` **không** dùng `smsService`/`documentService` — chúng gọi thẳng
`apiClient` với endpoint riêng (`/hsqe/incidents`, `/hsqe/risk-assessments`... suy từ ngữ cảnh code, xem file
để biết endpoint chính xác), độc lập hoàn toàn với workspace SMS.

## Liên kết với phần khác

- **`services/sms.service.ts`**: toàn bộ API cho cây chương/thủ tục/template/bản ghi trong `SmsDocumentPage`.
- **`services/document.service.ts`**: API cho `DocumentLibrary/*` (endpoint `/hsqe/documents/*`) — xem cảnh
  báo bên dưới.
- **`components/editor/RichTextEditor.tsx`**: dùng để soạn nội dung SOP và một số tab của `DocumentLibrary`
  (`DocumentTab.tsx`, `ChapterVersionsTab.tsx`).
- **`lib/printUtils.ts`**: in tài liệu SMS chuẩn A4 kèm watermark/chữ ký.
- **`stores/auth.store.ts`**: lấy tên/rank người dùng hiện tại để điền mặc định vào ô ký tên.
- **README gốc dự án**: HSQE là mảnh ghép cho tiêu chuẩn **ISM Code** và **ISPS** nêu ở mục 11 (Tiêu Chuẩn
  Hàng Hải).

## Ghi chú khi đọc/dạy

- **`DocumentLibrary/` là module "mồ côi"**: đã rà soát toàn bộ `src/` bằng `grep` — không có file nào (kể cả
  `App.tsx`, `HSQEPage.tsx`, `SmsDocumentPage.tsx`) import `DocumentLibraryPage`. Module này có đầy đủ 6 tab
  (Data/Document/Attached Documents/Read Logs/Chapter Versions/History), types riêng
  (`DocumentLibrary/types.ts`), và service riêng (`document.service.ts`, gọi `/api/hsqe/documents/*`) — viết
  hoàn chỉnh nhưng **chưa được route vào ứng dụng**. Khi dạy người mới, cần nói rõ đây có thể là (a) tính
  năng đang phát triển dở, hoặc (b) định hướng thay thế `SmsDocumentPage` trong tương lai — không nên coi là
  bug, nhưng cũng không nên tưởng đây là luồng đang chạy trên UI thật.
- `SmsDocumentPage.tsx` quá lớn để đọc tuần tự — nên dùng tìm kiếm theo state (`useState<...>`) hoặc theo
  `viewMode ===` để định vị đúng đoạn cần sửa, thay vì cuộn từ đầu file.
- 3 tab trong `HSQEPage` độc lập gần như hoàn toàn về tầng dữ liệu (3 service khác nhau: `sms.service.ts` /
  gọi trực tiếp `apiClient` trong `IncidentManagement`+`RiskWorkPermits` / `document.service.ts` cho module
  mồ côi) — đừng giả định chúng chia sẻ state hay cùng một "nguồn sự thật".
- Form ký PIN (`signingPin`) không có xác thực phức tạp phía frontend (không mã hoá, gửi thẳng lên backend
  qua HTTPS/API) — logic xác thực PIN thật nằm ở backend, frontend chỉ là ô nhập liệu.
