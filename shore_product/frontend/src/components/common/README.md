# components/common/ — Component UI nguyên tử

## Mục đích

Thư mục "component nền" theo mẫu thường thấy trong React: mỗi component nhỏ, không phụ thuộc nghiệp vụ, nằm trong thư mục con riêng kèm CSS + `index.ts`. Đây là các khối được liệt kê gộp theo yêu cầu tài liệu (Button, Card, Input, Modal, Toast, StatusBadge, ConfirmDialog) — cộng thêm 3 file không thuộc nhóm "nguyên tử" nhưng vẫn nằm chung thư mục (`ProtectedImage`, `ImageViewerModal`, `PortSelect`).

## Cấu trúc & vai trò

| Thư mục/file | Export (qua `index.ts` gốc) | Ghi chú |
|---|---|---|
| `Button/` | `Button` (`variant`: primary/secondary/ghost, `size`: sm/md/lg, `fullWidth`) | |
| `Card/` | `Card`, `CardHeader`, `CardBody`, `CardFooter` | |
| `Input/` | `Input` (label, error, fullWidth), `Select` | |
| `Modal/` | `Modal` (`size`: sm/md/lg/xl, đóng bằng phím Esc, khoá scroll body), `ModalHeader`, `ModalBody`, `ModalFooter` | |
| `StatusBadge/` | `StatusBadge` (`status: WorkStatus`, `size`) | Prop `status` bị **khoá cứng kiểu `WorkStatus`** (từ `types/work.types.ts`: pending/approved/in-progress/completed/overdue) — xem ghi chú. |
| `Toast/` | `ToastProvider`, `useToast()` → `{ success, error, warning, info }(title, message?)` | Toast tự quản lý bằng Context + `useState`, hiển thị góc màn hình, có icon theo loại. |
| `ConfirmDialog/` | `ConfirmDialogProvider`, `useConfirmDialog()` → `confirm(options): Promise<{confirmed, inputValue?}>` | Dialog xác nhận kiểu **imperative** (`await confirm({...})` thay vì render JSX) — hỗ trợ cả ô nhập lý do (`withInput`). |
| `ProtectedImage.tsx` | `ProtectedImage` (default export) | `<img>` thay thế: nếu `src` bắt đầu bằng `/uploads/` (ảnh cần JWT) thì tự tải qua `services/protectedMedia.ts` thành Blob URL thay vì gắn thẳng `src` (ảnh sau JWT không thể load bằng thẻ `<img src>` thông thường). |
| `ImageViewerModal.tsx` | `ImageViewerModal` (default export) | Modal xem ảnh/PDF full-size + upload thay thế file, cũng dùng `fetchProtectedMediaObjectUrl`. |
| `PortSelect.tsx` | `PortSelect` | Ô input tự động gợi ý cảng biển, debounce 250ms, gọi `services/port.service.ts#searchPorts`. |

`index.ts` ở gốc `common/` chỉ re-export **7 nhóm đầu** (Button…ConfirmDialog) — `ProtectedImage`, `ImageViewerModal`, `PortSelect` **không** nằm trong barrel, phải import trực tiếp theo đường dẫn file.

## Luồng hoạt động chính

- **Button/Card/Input/Modal/StatusBadge**: component thuần, không state, không gọi API — nhận props, render JSX + CSS riêng.
- **Toast, ConfirmDialog**: Provider bọc ở gốc `App.tsx` → bất kỳ component con nào cũng gọi `useToast()`/`useConfirmDialog()` được, không cần truyền props qua nhiều tầng.
- **ProtectedImage/ImageViewerModal**: `src` (đường dẫn `/uploads/...`) → `fetchProtectedMediaObjectUrl()` gắn `Authorization` header → nhận Blob → `URL.createObjectURL()` → gán vào `<img>`. Phải tự `revokeObjectURL` khi unmount (đã xử lý trong `useEffect` cleanup).

## Liên kết với phần khác

- **services/protectedMedia.ts**: hậu thuẫn `ProtectedImage`, `ImageViewerModal`.
- **services/port.service.ts**: hậu thuẫn `PortSelect` — dùng ở các form có trường cảng đi/đến (Voyage, Travel...).
- **types/work.types.ts**: `StatusBadge` phụ thuộc cứng vào `WorkStatus`.
- **App.tsx**: nơi duy nhất mount `ToastProvider`/`ConfirmDialogProvider`.

## Ghi chú khi đọc/dạy

- **Phát hiện quan trọng nhất của thư mục này**: mặc dù `Button`, `Card`, `Modal`, `Input`/`Select`, `StatusBadge` được thiết kế như "hệ design-system dùng chung", grep toàn bộ `src/` cho thấy **chúng chỉ thực sự được dùng ở đúng một trang: `pages/WorkAssignment/WorkAssignmentPage.tsx`** — mà trang đó lại đang chạy **dữ liệu mock** (xem `pages/README.md`). Phần còn lại của app (CrewListPage, các Dashboard, SyncDashboardPage...) tự viết JSX + Tailwind/inline-style riêng, không tái sử dụng các component này. Đừng mặc định "cứ cần nút bấm thì `import { Button } from '@/components/common'`" — hầu hết code hiện tại không làm vậy.
- Ngược lại, **`useToast()` và `useConfirmDialog()` mới thực sự là phần được dùng khắp nơi** (~20 file, luôn import trực tiếp `.../components/common/Toast` hoặc `.../ConfirmDialog`, hiếm khi qua barrel `components/common`).
- `StatusBadge` không generic như tên gợi ý — nó chỉ nhận `WorkStatus`, nên các trang có "trạng thái" riêng (Travel, ExternalRequest, Compliance, Voyage, Sync...) đều tự viết `STATUS_COLORS`/`STATUS_LABELS` dạng `Record<string,string>` + `<span>` tô màu thủ công thay vì tái dùng `StatusBadge`. Nếu được giao "chuẩn hoá badge trạng thái", đây là chỗ đáng để tổng quát hoá `StatusBadge` trước tiên.
- Có **sonner** (`toast()` từ thư viện ngoài) chạy song song với `useToast()` tự viết ở đây — xem ghi chú trong `contexts/README.md` để biết nơi nào dùng cái nào.
