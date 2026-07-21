# store/ — (Hiện tại: trống / không dùng)

## Mục đích

Thư mục dự kiến cho state management dạng "store" tập trung (kiểu Zustand mà **Edge Frontend** đang dùng — xem README gốc mục 9.2: *"State: Zustand 5.0 (persistent)"* cho Edge). **Ở Shore Frontend, `zustand` không nằm trong `package.json`** và thư mục này thực chất không có nội dung.

## Cấu trúc & vai trò

| File | Nội dung |
|---|---|
| `useAppInfo.ts` | **File rỗng (0 byte).** Không export gì cả — đừng import từ đây. |

## Luồng hoạt động chính

Không có. Shore quản lý state toàn cục qua:
- **`contexts/`** (Auth, Vessel, I18n) cho state dùng chung toàn app — xem `contexts/README.md`.
- **`@tanstack/react-query`** — đã cài và cấu hình `QueryClientProvider` trong `main.tsx` (staleTime 24h, gcTime 7 ngày, `refetchOnWindowFocus: false`), nhưng **grep toàn `src/` không tìm thấy bất kỳ lệnh gọi `useQuery`/`useMutation` nào** — nghĩa là hạ tầng đã sẵn sàng nhưng chưa được page nào sử dụng. Toàn bộ data-fetching hiện tại vẫn là `useState` + `useEffect` + gọi `services/` thủ công (xem `hooks/README.md`, `services/README.md`).
- **State cục bộ từng trang** (`useState` bên trong mỗi page) — đây là cách phổ biến nhất trong codebase hiện tại.

## Liên kết với phần khác

- Không có import nào từ `store/` ở nơi khác trong `src/` (đã kiểm tra bằng grep).

## Ghi chú khi đọc/dạy

- Nếu bạn được giao nhiệm vụ "thêm state toàn cục mới", **đừng bắt đầu từ `store/`** — hoặc là thêm một Context mới trong `contexts/`, hoặc cân nhắc bắt đầu dùng React Query thật sự (hạ tầng đã có sẵn ở `main.tsx`, chỉ cần viết `useQuery` trong hook/page).
- Thư mục này tồn tại nhiều khả năng do code được khởi tạo từ một template dùng chung với Edge (nơi `store/` chứa các Zustand store thật), nhưng Shore chưa (hoặc không cần) áp dụng theo.
