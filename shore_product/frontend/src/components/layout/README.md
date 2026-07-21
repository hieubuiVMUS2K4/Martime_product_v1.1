# components/layout/ — Khung giao diện

## Mục đích

Chứa khung bao ngoài (nav + `<Outlet/>`) cho toàn bộ trang sau đăng nhập. Thư mục có **hai bộ layout** — chỉ một bộ đang thực sự chạy.

## Cấu trúc & vai trò

| File / thư mục | Export | Đang được `routes/AppRoutes.tsx` dùng? | Vai trò |
|---|---|---|---|
| `TopNavLayout/TopNavLayout.tsx` | `TopNavLayout` | ✅ **Có** — bọc toàn bộ route sau `RequireAuth` | Thanh điều hướng **ngang**, cố định trên cùng: logo "Maritime", các link/dropdown (Danh mục, Danh sách tàu, Tracking, Thông tin [Onboarding/Xác minh/Tuân thủ/Phân công/Tuyển ngoài/Di chuyển/Onboard], Báo cáo, Đồng bộ), chuông thông báo (gộp 2 nguồn: `crewApi.holdNotifications()` — thuyền viên bị tạm giữ, và `notificationApi.getRecent()` — thông báo sync), `UserMenu`, menu mobile responsive. Tự poll thông báo mỗi 30 giây. |
| `UserMenu.tsx` | `UserMenu` | ✅ Có (bên trong `TopNavLayout`) | Avatar chữ cái đầu tên user + dropdown (portal ra `document.body`) hiển thị tên/role, nút Hồ sơ (chưa nối chức năng), nút Đăng xuất (`useAuth().logout()` rồi `navigate('/login')`). |
| `MainLayout.tsx` | `MainLayout` | ❌ **Không** — không route nào import | Layout **cũ**: `Sidebar` dọc bên trái (danh sách 7 menu cứng: Dashboard, QL danh mục, QL tàu, QL thuyền viên, QL hải trình, QL phân công công việc, Master Schedule PMS) + `<Outlet/>` bên phải. |
| `Sidebar/Sidebar.tsx` | `Sidebar`, type `MenuItem` | ❌ Không (chỉ được `MainLayout` dùng) | Danh sách link dọc, tô "active" theo `location.pathname === item.path`. Nhận `menuItems` qua prop — không tự biết route nào tồn tại. |

`index.ts` export cả 3 (`MainLayout`, `Sidebar`, `TopNavLayout`) — barrel không phản ánh việc 2/3 export đã ngừng dùng.

## Luồng hoạt động chính

```
routes/AppRoutes.tsx
  <Route element={<TopNavLayout/>}>      ← layout thật, tất cả route nghiệp vụ nằm trong này
    <Route path="/report" .../>
    ...
  </Route>

TopNavLayout.tsx
  useEffect: poll mỗi 30s → crewApi.holdNotifications() + notificationApi.getRecent(30)
  render: <header class="topnav">...</header> + <main><Outlet/></main>
```

`MainLayout`/`Sidebar` không nằm trong luồng runtime nào — chỉ tồn tại trong bundle nếu có file khác `import` chúng (hiện không có).

## Liên kết với phần khác

- **contexts/AuthContext**: `UserMenu` đọc `user`/`logout()`.
- **services/crew.service.ts** (`crewApi.holdNotifications`) và **services/notification.service.ts** (`notificationApi.getRecent`, `markAllRead`): nguồn dữ liệu chuông thông báo trong `TopNavLayout`.
- **routes/AppRoutes.tsx**: nơi duy nhất quyết định layout nào bọc route nào.

## Ghi chú khi đọc/dạy

- **Menu điều hướng khai báo ở 2 nơi độc lập, không liên kết với nhau**: danh sách `navItems` trong `TopNavLayout.tsx` (thực tế đang hiển thị) và danh sách `menuItems` trong `MainLayout.tsx` (không hiển thị vì không được dùng). Khi thêm route mới trong `routes/AppRoutes.tsx`, phải tự tay thêm mục menu tương ứng vào mảng `navItems` bên trong `TopNavLayout.tsx` — không có cơ chế tự động sinh menu từ route.
- `MainLayout` + `Sidebar` là tàn dư từ bản refactor HTML→React đầu tiên (xem `pages/REFACTOR_NOTES.md`, mục "6. Routes" mô tả đúng mẫu `<Route element={<MainLayout />}>`). Đọc code không có nghĩa là đang chạy — luôn đối chiếu với `routes/AppRoutes.tsx` để biết layout nào thật.
- Route `path="/vessels/tracking"` trong `TopNavLayout` được đặt **phía trên** `path="/vessels/:id"` trong `AppRoutes.tsx` — thứ tự này bắt buộc, nếu đảo ngược `react-router` sẽ khớp `:id = "tracking"` trước.
