# components/crew — Modal quản lý thuyền viên

## Mục đích

Nhóm modal phục vụ `pages/Crew/` (đặc biệt là `CrewPage.tsx` và `CrewDetailPage.tsx`): thêm mới thuyền viên,
quản lý hồ sơ giấy tờ (travel/seafarer/employment document), hồ sơ sức khoẻ, và xem/thay ảnh (avatar hoặc
file scan chứng chỉ). Đây là mảnh ghép frontend cho phần **Crew HR & Crew Operational** mô tả trong README
gốc dự án — Edge sở hữu dữ liệu vận hành (`IsOnboard`, ngày lên/xuống tàu...) trong khi Shore sở hữu dữ liệu
hành chính (mã bảo hiểm, mã thuế).

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `AddCrewModal.tsx` | Form thêm thuyền viên mới — đầy đủ nhóm field: thông tin cơ bản, bio-data (chiều cao/cân nặng/nhóm máu/size đồng phục), người thân (next of kin), học vấn. Tự tải danh sách `ranks`/`countries` khi mở |
| `AddDocumentModal.tsx` | Thêm giấy tờ hành trình/thuyền viên/lao động — 3 nhóm tài liệu (`travel_documents`, `seafarer_documents`, `employment_documents`), mỗi nhóm có danh sách loại giấy tờ con riêng (passport/visa/seaman book, SID/COC, contract/appraisal...) |
| `AddHealthDocumentModal.tsx` | Thêm hồ sơ sức khoẻ (giấy khám sức khoẻ, tiêm chủng, tiêm chủng COVID-19, bảo hiểm y tế...) |
| `DetailCertificatesModal.tsx` | Tạo/định nghĩa **loại chứng chỉ** mới trong danh mục hệ thống (không phải gán chứng chỉ cho 1 thuyền viên cụ thể — xem lưu ý) |
| `ImageViewerModal.tsx` | Modal xem ảnh/PDF dùng chung: xem avatar, xem file scan chứng chỉ/giấy tờ, hỗ trợ thay file mới qua `customUploadHandler` truyền từ nơi gọi |

## Luồng hoạt động chính

```
CrewPage / CrewDetailPage
   │  useState(showAddModal / showDocumentModal / ...)
   ▼
<AddCrewModal isOpen onClose onSave={handleSaveCrew} />
   │  tự gọi maritimeService.ranks.getAll() + maritimeService.countries.getAll() khi mở (điền dropdown)
   │  validate field bắt buộc phía client
   ▼ (submit)
onSave(formData) → cha gọi maritimeService.crew.add(formData)   → POST /api/crew
   ▼
CrewPage tự load lại danh sách thuyền viên (loadCrewData())
```

`ImageViewerModal` là component "linh hoạt nhất" nhóm này — không tự biết endpoint upload, mà nhận
`customUploadHandler` từ component cha (vd. cha truyền
`(id, formData) => maritimeService.crew.uploadAvatar(id, formData)` khi xem avatar, hoặc truyền hàm khác khi
xem file chứng chỉ) — nhờ vậy 1 modal phục vụ được nhiều ngữ cảnh "xem + thay ảnh" khác nhau trong toàn bộ
module Crew.

## Liên kết với phần khác

- **`pages/Crew/CrewPage.tsx`, `CrewDetailPage.tsx`, `CertificateManagementPage.tsx`**: nơi mở các modal này.
- **`services/maritime.service.ts`** (`maritimeService.crew`, `maritimeService.certificates`,
  `maritimeService.ranks`, `maritimeService.countries`): toàn bộ API được gọi.
- **`pages/Crew/AddCertificateModal.tsx`, `AddCrewCertificateModal.tsx`** (nằm trong `pages/Crew/`, không
  phải `components/crew/`): đây là các modal **gán chứng chỉ cụ thể cho một thuyền viên** — khác với
  `components/crew/DetailCertificatesModal.tsx` (tạo *loại* chứng chỉ dùng chung trong danh mục hệ thống).
  Đừng nhầm 2 tầng "chứng chỉ" này.
- **README gốc dự án, mục 6.2 (Merge Crew Member)**: giải thích quy tắc field nào Shore sở hữu (mã bảo
  hiểm/mã thuế) và field nào Edge sở hữu (`IsOnboard`, ngày lên/xuống tàu, cảng lên/xuống, `AvatarUrl`) khi
  đồng bộ — các modal trong thư mục này chủ yếu ghi vào các field **Edge sở hữu**.

## Ghi chú khi đọc/dạy

- Tên `DetailCertificatesModal.tsx` dễ gây hiểu lầm là "xem chi tiết chứng chỉ của 1 thuyền viên" — thực tế
  nó là form **tạo loại chứng chỉ mới** (`certificateCode`, `certificateName`, `category`, chu kỳ hiệu lực,
  quốc gia áp dụng) cho danh mục dùng chung toàn hệ thống. Khi cần sửa luồng "gán chứng chỉ cho thuyền viên",
  phải tìm trong `pages/Crew/`, không phải ở đây.
- `AddCrewModal.tsx` không dùng `hooks/useCachedMetadata.ts` (`useCountries`/`useRanks`) dù mục đích tương tự
  — nó tự viết `loadRanks()`/`loadCountries()` gọi thẳng `maritimeService` mỗi lần modal mở, không có cache
  React Query. Đây là ví dụ tốt cho thấy tiện ích cache có sẵn (`hooks/useCachedMetadata.ts`) chưa được áp
  dụng nhất quán trong toàn bộ code liên quan tới crew.
- Phần lớn modal ở đây dùng `react-toastify` (`toast` từ `'react-toastify'`) trong khi nhiều nơi khác của
  dự án đã chuyển sang `sonner` (`toast` từ `'sonner'`) — cả 2 thư viện toast cùng được cài
  (`package.json`) và cùng được khởi tạo ở `App.tsx` (`<Toaster />` của sonner + `<ToastContainer />` của
  react-toastify chạy song song) — không phải lỗi, nhưng là điểm không nhất quán cần biết khi thêm thông báo
  mới (nên ưu tiên `sonner` vì đó là hướng các module mới hơn đang dùng).
