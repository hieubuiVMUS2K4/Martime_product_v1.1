# lib/ — Tiện ích cho hệ UI kiểu shadcn/ui

## Mục đích

Thư mục quy ước của [shadcn/ui](https://ui.shadcn.com/) — nơi đặt hàm `cn()` giúp gộp className Tailwind an toàn. Đây là dấu hiệu cho thấy `components/ui/` được sinh ra từ shadcn CLI (xem `components/ui/README.md`).

## Cấu trúc & vai trò

| File | Export | Nội dung |
|---|---|---|
| `utils.ts` | `cn(...inputs: ClassValue[])` | `twMerge(clsx(inputs))` — gộp nhiều className, cho phép override class Tailwind xung đột (vd `"p-2"` và `"p-4"` truyền sau sẽ thắng) mà không bị trùng lặp. |

## Luồng hoạt động chính

```
component.tsx
  className={cn("base-classes", condition && "conditional-class", className)}
```

Không có state, không gọi API — thuần hàm helper.

## Liên kết với phần khác

- **components/ui/button.tsx** (và `badge.tsx`, `input.tsx`, `card.tsx`) — tất cả import `cn` từ `@/lib/utils` để dựng class theo `class-variance-authority` (`cva`).
- Ngoài `components/ui/`, **không có nơi nào khác trong `src/` import `cn`** — vì phần còn lại của app viết className trực tiếp bằng template string hoặc Tailwind thuần, không qua `cn()`.

## Ghi chú khi đọc/dạy

- Thư mục chỉ có 1 file, đừng nhầm với `utils/` (khác thư mục, khác mục đích — xem `utils/README.md`, hiện đang **rỗng**). `lib/utils.ts` (có nội dung) vs `utils/index.ts` (rỗng) là hai thứ dễ gõ nhầm tên khi import — luôn dùng alias `@/lib/utils` cho `cn`.
- Vì `components/ui/*` hiện chỉ được dùng bởi một component không được route nào tham chiếu (`components/MaritimeFleetDashboard.tsx` — xem `components/README.md`), `lib/utils.ts` hiện cũng gián tiếp "mồ côi" theo, dù bản thân hàm `cn()` vẫn hữu ích nếu bạn muốn dùng thêm shadcn/ui component trong tương lai.
