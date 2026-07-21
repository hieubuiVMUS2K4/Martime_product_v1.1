# stores/ — Zustand global state (persistent & ephemeral)

## Mục đích

Chứa các store dùng chung toàn ứng dụng, viết bằng **Zustand 5**. Đây là nơi lưu trạng thái cần chia sẻ
giữa nhiều page/component mà không tiện truyền qua props (phiên đăng nhập, cài đặt hiển thị, số liệu
telemetry mới nhất...). Zustand được chọn thay vì Redux vì API tối giản (không cần Provider bọc ngoài) —
phù hợp dashboard Edge chạy trên máy tính buồng lái, ít cần DevTools phức tạp.

## Cấu trúc & vai trò

| File | Store / hook | Persist? | Storage key | Trạng thái quản lý |
|---|---|---|---|---|
| `auth.store.ts` | `useAuthStore` | Có — `persist` + `localStorage` | `maritime-auth` | `user`, `accessToken`, `storedRefreshToken`, `expiresAt`, `isAuthenticated`, `mustChangePassword`... + action `login/logout/initializeAuth/doRefreshToken` |
| `settings.store.ts` | `useSettingsStore` | Có — `persist` **+ ghi tay `localStorage` song song** | `maritime-edge-settings` | `theme` ('light'\|'dark'\|'system'), `fontSize`, `language` ('en'\|'vi'), `isSettingsOpen` |
| `maritime.store.ts` | `useMaritimeStore` | **Không** (thuần in-memory) | — | `dashboardStats`, `currentPosition`, `currentNavigation`, `activeAlarms`, `isSyncing`, `lastSyncTime`, `isOnline` |

> Lưu ý: có một store **thứ tư** tên `useStore` nằm ở `src/lib/store.ts` (không phải trong thư mục này).
> Nó cache `crew`/`maintenanceTasks`/`dashboardStats` vào `sessionStorage` kèm TTL 5 phút, và export sẵn
> `useCrew()/useMaintenance()/useDashboard()`. Qua rà soát (`grep` toàn bộ `src/`), **không có page/component
> nào còn import file này** — đây là code cũ chưa bị xoá, không phải một phần luồng dữ liệu hiện hành. Khi
> dạy người mới, nên chỉ rõ để họ không nhầm với `stores/maritime.store.ts`.

## Luồng hoạt động chính

```
Component
   │  const token = useAuthStore(s => s.accessToken)     // đọc state qua selector
   │  const { login } = useAuthStore()                    // hoặc lấy action
   ▼
store action (vd. login())
   │  gọi services/auth.service.ts  → apiClient → Edge API /auth/login
   ▼
set({ user, accessToken, ... })     // Zustand cập nhật state
   │  nếu có persist middleware → tự ghi vào localStorage (partialize chỉ chọn field cần lưu)
   ▼
Mọi component đang subscribe store re-render với state mới
```

Điểm đặc biệt của **`auth.store.ts`**: nó không chỉ giữ token mà còn tự quản lý **vòng đời JWT**:
- `scheduleRefresh()` đặt `setTimeout` để tự gọi `doRefreshToken()` khi đạt 80% thời gian sống của access
  token (vd. token 10 phút → refresh ở phút thứ 8).
- `initializeAuth()` được gọi **một lần** bởi `components/auth/AuthGuard.tsx` lúc app khởi động: nếu còn
  access token hợp lệ → validate với server; nếu hết hạn nhưng còn refresh token → tự refresh; nếu không
  còn gì → chuyển về `/login`.
- Để tránh phụ thuộc vòng giữa `auth.store.ts` (cần gọi API) và `services/api.client.ts` (cần đọc token),
  `AuthGuard` gọi `registerAuthProvider()` để "tiêm" 3 hàm (`getAccessToken`, lấy tên user, `clearAuth`) vào
  `api.client.ts` dưới dạng closure — `api.client.ts` không import trực tiếp `auth.store.ts`.

**`settings.store.ts`** điều khiển theme/font/ngôn ngữ **và** trực tiếp thao tác DOM
(`document.documentElement.classList`, CSS variable `--font-scale`) mỗi khi giá trị đổi — đây là store duy
nhất có side-effect DOM ngay trong action, cộng thêm 1 listener `matchMedia('(prefers-color-scheme: dark)')`
khi theme = `'system'`.

## Liên kết với phần khác

- **`contexts/I18nContext.tsx`**: đọc `useSettingsStore(s => s.language)` để chọn file dịch (`locales/en.json`
  / `vi.json`).
- **`services/api.client.ts`**: nhận token từ `auth.store` qua `registerAuthProvider` (xem `services/README.md`).
- **`components/settings/SettingsDialog.tsx`**, **`SettingsButton.tsx`**: giao diện điều khiển
  `settings.store`.
- **`components/layouts/Header.tsx` / `UserMenu.tsx`**: hiển thị user hiện tại từ `auth.store`.
- **`pages/Sync/SyncPage.tsx`**: đọc/ghi `maritime.store` cho cờ `isSyncing`/`isOnline` (một số nơi khác tự
  quản lý state cục bộ thay vì dùng store này — xem `pages/Sync/README.md`).

## Ghi chú khi đọc/dạy

- Ba store thật sự khác nhau về **độ bền dữ liệu**: `auth` (sống qua reload, mất khi logout),
  `settings` (sống mãi cho tới khi user đổi), `maritime` (mất khi F5 — chỉ là cache tạm cho phiên hiện tại).
  Khi thêm state mới, hỏi trước: "dữ liệu này cần sống qua reload không?" để chọn đúng store/đúng cấu hình
  `persist`.
- `settings.store.ts` có một điểm dễ gây nhầm lẫn khi đọc code: nó vừa dùng middleware `persist` (tự động
  ghi `localStorage`) vừa có các hàm `getStoredSettings()/saveSettings()` viết tay ghi **cùng một key**
  `maritime-edge-settings`. Hai cơ chế này chạy song song (một phần để đảm bảo theme được áp dụng ngay khi
  module load, trước khi React kịp render) — không phải lỗi, nhưng là chi tiết cần giải thích khi có người
  hỏi "sao lại ghi localStorage 2 lần".
- Không dùng Redux DevTools/middleware `devtools` — nếu cần debug state, dùng trực tiếp
  `useXStore.getState()` trong console trình duyệt.
- Selector hook dạng `selectUser`, `useTheme`, `useIsSettingsOpen`... được export sẵn ở cuối mỗi file để
  tránh re-render thừa (component chỉ subscribe đúng field cần, thay vì toàn bộ store).
