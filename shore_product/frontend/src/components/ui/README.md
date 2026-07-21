# components/ui/ — Primitives kiểu shadcn/ui (hiện chưa dùng thật)

## Mục đích

4 component UI dựng theo mẫu [shadcn/ui](https://ui.shadcn.com/): Radix primitives (khi cần) + `class-variance-authority` (`cva`) để khai báo variant + `cn()` (`lib/utils.ts`) để gộp className Tailwind. Đây là **bộ component thứ hai** cho cùng mục đích (nút bấm, thẻ, input) song song với `components/common/` — khác phong cách viết (Tailwind utility-class thay vì CSS module riêng).

## Cấu trúc & vai trò

| File | Export | Variants |
|---|---|---|
| `button.tsx` | `Button`, `buttonVariants` | `variant`: default/destructive/outline/secondary/ghost/link; `size`: default/sm/lg/icon. Hỗ trợ `asChild` (dùng `@radix-ui/react-slot` để "mượn" thẻ con làm root, ví dụ render như `<a>` nhưng vẫn có style Button). |
| `card.tsx` | `Card`, `CardHeader`, `CardTitle`, `CardContent`... (bố cục chuẩn shadcn) | Không có variant, chỉ style cố định `rounded-xl border bg-card...`. |
| `badge.tsx` | `Badge`, `badgeVariants` | `variant`: default/secondary/destructive/outline. |
| `input.tsx` | `Input` | Input thuần style Tailwind, không có `label`/`error` tích hợp (khác `components/common/Input`). |

## Luồng hoạt động chính

Component thuần, không state, không gọi API — nhận `variant`/`size`/`className` qua props, tính class cuối bằng `cva(...)` rồi `cn()`.

## Liên kết với phần khác

- **lib/utils.ts** (`cn`): dùng trong cả 4 file.
- **`@radix-ui/react-slot`, `class-variance-authority`, `clsx`, `tailwind-merge`**: 4 dependency trong `package.json` tồn tại **chỉ để phục vụ** `components/ui/` (không nơi nào khác trong `src/` dùng `cva`/`Slot`).
- **components/MaritimeFleetDashboard.tsx**: component **duy nhất** trong toàn bộ `src/` import từ `components/ui/*` (`Card`, `CardHeader`, `CardContent`, `Badge`, `Button`, `Input`).

## Ghi chú khi đọc/dạy

- **Cả thư mục này hiện "mồ côi" theo chuỗi**: `components/ui/*` chỉ được `components/MaritimeFleetDashboard.tsx` dùng, mà chính component đó lại không được `pages/` hay `routes/AppRoutes.tsx` nào render (đã kiểm tra bằng grep — xem `components/README.md`). Nói cách khác, đi theo chuỗi phụ thuộc từ route thật, `components/ui/` hiện không được reach tới.
- Đừng nhầm với `components/common/Button`, `components/common/Card`, `components/common/Input` — đó là **bộ khác**, viết theo phong cách CSS-module/CSS-riêng, và (trớ trêu thay) cũng chỉ thực sự dùng ở một trang duy nhất (`WorkAssignmentPage`, xem `components/common/README.md`). Hiện tại app có **3 hệ thống Button/Card/Input song song** (`components/common/*`, `components/ui/*`, và JSX/Tailwind viết tay trong từng page) mà không hệ nào chiếm vai trò chuẩn thực sự.
- Nếu trong tương lai bạn muốn dùng thêm shadcn/ui component (dialog, table, select...), đây là đúng thư mục để thêm — hạ tầng (`cn`, `cva`, Radix, Tailwind config) đã sẵn sàng.
