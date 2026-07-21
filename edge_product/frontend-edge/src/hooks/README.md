# hooks/ — Custom React hooks dùng chung

## Mục đích

Các hook nhỏ, tái sử dụng ở nhiều page, tách riêng khỏi logic gọi API cụ thể (nằm ở `services/`) và khỏi
state toàn cục (nằm ở `stores/`). Đây là các tiện ích "vừa và nhỏ": quản lý trạng thái async thủ công, fetch
đơn giản, đọc tên tài khoản hiện tại, cache metadata tĩnh, và lưu nháp offline.

## Cấu trúc & vai trò

| File | Hook | Công dụng |
|---|---|---|
| `index.ts` | barrel export | **Chỉ** re-export `useAsync` và `useFetch` — xem lưu ý bên dưới |
| `useAsync.ts` | `useAsync<T>()` | Bọc một async function bất kỳ thành `{ data, loading, error, execute, reset }`. Không tự chạy khi mount — phải gọi `execute(fn)` thủ công |
| `useFetch.ts` | `useFetch<T>(url, options)` | Gọi thẳng `fetch(url)` (không qua `apiClient`), tự chạy lại khi `url`/`options.skip` đổi, trả về `{ data, loading, error, refetch }` |
| `useCurrentAccountName.ts` | `useCurrentAccountName()` | Lấy `username` (hoặc `fullName`) hiện tại từ `stores/auth.store.ts` — dùng để hiển thị "người thực hiện" trong form (audit trail) |
| `useCachedMetadata.ts` | `useCountries()`, `useRanks()`, `useVoyages(params)` | Bọc **React Query** (`@tanstack/react-query`) quanh `maritimeService.countries/ranks/voyage`, `staleTime` 24h (countries/ranks) hoặc 15 phút (voyages) — tránh gọi lại API cho dữ liệu gần như tĩnh |
| `useOfflineDraft.ts` | `useOfflineDraft<T>(key, state, onRestore)` | Tự động lưu nháp form vào IndexedDB (qua `lib/draftsDb.ts`) mỗi 5 giây, cho phép khôi phục nếu mất điện/rớt VSAT giữa chừng khi đang nhập báo cáo dài |

## Luồng hoạt động chính

`useCachedMetadata.ts` là ví dụ rõ nhất cho luồng "hook → service → Edge API":

```
Component (vd. form chọn Quốc tịch)
   │  const { data: countries } = useCountries()
   ▼
useCountries() (hooks/useCachedMetadata.ts)
   │  useQuery({ queryKey: ['countries'], queryFn: () => maritimeService.countries.getAll(), staleTime: 24h })
   ▼
maritimeService.countries.getAll()  (services/maritime.service.ts)
   │  apiClient.get('/countries')
   ▼
Edge API GET /api/countries → PostgreSQL
   ▼
React Query cache (QueryClientProvider khai báo ở main.tsx) giữ kết quả 24h
   → mọi component gọi lại useCountries() trong 24h nhận cache, KHÔNG gọi lại API
```

`useOfflineDraft` không gọi API — nó chỉ tương tác với `IndexedDB` cục bộ (qua Dexie), đúng tinh thần
"Edge offline-first": nếu mất kết nối hoặc tắt máy đột ngột giữa lúc thuyền phó đang gõ báo cáo Noon dài,
dữ liệu chưa submit vẫn còn khi mở lại trang.

## Liên kết với phần khác

- **`services/maritime.service.ts`**: nguồn dữ liệu cho `useCachedMetadata.ts`.
- **`stores/auth.store.ts`**: nguồn dữ liệu cho `useCurrentAccountName.ts`.
- **`lib/draftsDb.ts`**: nơi `useOfflineDraft.ts` thật sự ghi/đọc IndexedDB.
- **`main.tsx`**: khai báo `QueryClient` toàn cục (staleTime mặc định 24h, `refetchOnWindowFocus: false`) —
  mọi hook dùng `useQuery` (kể cả `useCachedMetadata.ts`) đều thừa hưởng cấu hình này trừ khi override riêng.
- Dùng nhiều nhất trong các form dài: Reporting (Noon/Departure/...), HSQE (điền form SMS).

## Ghi chú khi đọc/dạy

- **`hooks/index.ts` không đầy đủ**: chỉ export `useAsync`/`useFetch`. Ba hook còn lại
  (`useCurrentAccountName`, `useCachedMetadata`, `useOfflineDraft`) phải import trực tiếp theo đường dẫn file
  (vd. `import { useOfflineDraft } from '@/hooks/useOfflineDraft'`), **không** import qua `@/hooks`. Đây là
  điều dễ gây lỗi "not exported" cho người mới — cần kiểm tra file gốc thay vì chỉ nhìn `index.ts`.
- `useFetch` gọi thẳng `fetch()`, không tự gắn Bearer token như `apiClient` — chỉ nên dùng cho endpoint công
  khai hoặc khi component tự thêm header thủ công.
- `useAsync` và `useFetch` có vẻ trùng mục đích với pattern `useState + useEffect` viết tay rải rác trong rất
  nhiều page (đa số page trong `pages/` **không** dùng 2 hook này mà tự viết `loading/error/data` bằng
  `useState` — `useAsync`/`useFetch` là tiện ích có sẵn nhưng chưa được áp dụng nhất quán toàn dự án).
