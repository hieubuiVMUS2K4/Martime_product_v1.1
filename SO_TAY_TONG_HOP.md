# 📘 SỔ TAY TỔNG HỢP — HỆ THỐNG QUẢN LÝ HÀNG HẢI EDGE–SHORE

> **Tài liệu này dành cho ai?** Nhóm phát triển **mới tiếp nhận dự án**. Đây là điểm bắt đầu duy nhất bạn cần
> mở đầu tiên: nó cho bạn bức tranh toàn cảnh, cách chạy hệ thống, các luồng nghiệp vụ xuyên suốt, và **bản đồ
> dẫn tới toàn bộ 96 README chi tiết** đặt trong từng thư mục nguồn.
>
> **Cách dùng sổ tay:** Đọc lần lượt Mục 1 → 4 để nắm tổng thể và chạy được hệ thống. Khi cần làm việc với một
> module cụ thể, nhảy tới **Mục 3 (Bản đồ tài liệu)** để mở đúng README của thư mục đó. Mục 5–11 là tham chiếu
> tra cứu khi cần đào sâu.
>
> *Cập nhật: 2026-07-21. Sổ tay này tổng hợp lại — không thay thế — README gốc (`README.md`) và 96 README
> chi tiết trong từng thư mục.*

---

## 🧭 MỤC LỤC

1. [Tổng quan kiến trúc Edge–Shore](#1-tổng-quan-kiến-trúc-edgeshore)
2. [Bản đồ thư mục toàn dự án](#2-bản-đồ-thư-mục-toàn-dự-án)
3. [Bản đồ tài liệu — 96 README chi tiết](#3-bản-đồ-tài-liệu--96-readme-chi-tiết)
4. [Hướng dẫn cài đặt & chạy (thực chiến)](#4-hướng-dẫn-cài-đặt--chạy-thực-chiến)
5. [Các luồng nghiệp vụ xuyên suốt (end-to-end)](#5-các-luồng-nghiệp-vụ-xuyên-suốt-end-to-end)
6. [Mô hình dữ liệu & quyền sở hữu (Ownership Matrix)](#6-mô-hình-dữ-liệu--quyền-sở-hữu-ownership-matrix)
7. [Công nghệ sử dụng](#7-công-nghệ-sử-dụng)
8. [Quy ước & cạm bẫy khi phát triển (Gotchas)](#8-quy-ước--cạm-bẫy-khi-phát-triển-gotchas)
9. [Lộ trình học cho người mới (Onboarding Path)](#9-lộ-trình-học-cho-người-mới-onboarding-path)
10. [Xử lý sự cố thường gặp (Troubleshooting)](#10-xử-lý-sự-cố-thường-gặp-troubleshooting)
11. [Bảng thuật ngữ (Glossary)](#11-bảng-thuật-ngữ-glossary)

---

## 1. TỔNG QUAN KIẾN TRÚC EDGE–SHORE

### 1.1 Ý tưởng cốt lõi

Dự án mô phỏng thực tế ngành hàng hải: **tàu hoạt động giữa đại dương nơi Internet chập chờn hoặc mất hẳn**.
Không thể đặt toàn bộ hệ thống trên bờ rồi bắt tàu gọi API qua vệ tinh — độ trễ và chi phí quá lớn, mà mất mạng
là tê liệt. Giải pháp là **Edge Computing**: đặt một hệ thống hoàn chỉnh **ngay trên tàu (Edge)** chạy offline
100%, và **đồng bộ hai chiều** với trung tâm điều hành **trên bờ (Shore)** khi có kết nối.

Vì vậy dự án tách thành **2 hệ thống độc lập**, mỗi hệ có backend + frontend + database riêng, giao tiếp qua một
**giao thức đồng bộ store-and-forward**:

```
        SHORE (Trên bờ)                                    EDGE (Trên tàu)
   ┌──────────────────────┐                          ┌──────────────────────┐
   │ Frontend React :3000 │                          │ Frontend React :3002 │
   │ Backend .NET   :5000 │  ◄───  VSAT / 4G  ───►   │ Backend .NET   :5001 │
   │ PostgreSQL     :5434 │       Iridium / WiFi     │ PostgreSQL     :5433 │
   │ Redis          :6379 │      (không liên tục)    │ Mobile App (Flutter) │
   └──────────────────────┘                          └──────────────────────┘
    Quản lý đội tàu tổng thể                          Vận hành 1 con tàu cụ thể
    Master data, HR, compliance                       Telemetry, logbook, PMS, báo cáo
```

### 1.2 Sáu nguyên tắc thiết kế (phải thuộc)

| Nguyên tắc | Ý nghĩa thực tế |
|---|---|
| **Offline-First** | Edge chạy độc lập hoàn toàn khi mất mạng — không có ngoại lệ |
| **Store-and-Forward** | Dữ liệu được xếp hàng đợi (`SyncQueue`) và gửi khi có mạng |
| **Network-Aware** | Ưu tiên đồng bộ theo loại kết nối: Iridium → chỉ dữ liệu Critical |
| **Domain-Based Conflict Resolution** | Mỗi bảng/trường có quy tắc "ai thắng" riêng, KHÔNG dùng Last-Write-Wins làm mặc định |
| **Bidirectional Sync** | Dữ liệu chảy hai chiều Shore ↔ Edge |
| **Idempotent Processing** | Cùng một item sync xử lý nhiều lần không gây trùng lặp |

### 1.3 Phân chia trách nhiệm

| | SHORE (Trên bờ) | EDGE (Trên tàu) |
|---|---|---|
| **Vai trò** | Quản lý cả đội tàu, HR, tuyển dụng, compliance toàn cầu | Vận hành 1 con tàu: cảm biến, nhật ký, bảo trì, báo cáo |
| **Controllers** | ~29 (13 gốc + nhóm con) | ~51 |
| **Bảng DB** | 110+ | 60+ |
| **Sở hữu dữ liệu** | Master data, HR hành chính, thương mại tàu | Vận hành, telemetry, logbook, trạng thái onboard |
| **Người dùng** | Fleet Manager, HR, Compliance Officer | Thuyền trưởng, sĩ quan, thuyền viên |

### 1.4 Ba tầng ưu tiên đồng bộ (Priority)

Khi băng thông hạn chế, `SyncBackgroundWorker` lọc hàng đợi theo mức ưu tiên phù hợp loại kết nối:

| Priority | Loại dữ liệu | Iridium | VSAT | 4G/WiFi |
|---|---|:---:|:---:|:---:|
| 🔴 **Critical** | Báo động an toàn, distress | ✅ | ✅ | ✅ |
| 🟡 **Operational** | Noon report, crew, vị trí | ❌ | ✅ | ✅ |
| 🟢 **Low** | Kho, logbook, tài liệu | ❌ | ❌ | ✅ |

---

## 2. BẢN ĐỒ THƯ MỤC TOÀN DỰ ÁN

```
📦 Martime_product_v1.1/
│
├── 📄 README.md                    → README gốc (kiến trúc + sync protocol chi tiết)
├── 📄 SO_TAY_TONG_HOP.md           → (BẠN ĐANG ĐỌC) sổ tay tổng hợp cho người mới
├── 📄 product.sln                  → Solution gốc (mở cả 2 backend trong Visual Studio)
├── 📄 start-edge.ps1               → Script khởi động nhanh Edge (DB + backend + emulator)
│
├── 🏢 shore_product/               → HỆ THỐNG SHORE (Trên bờ)
│   ├── backend/                    → ASP.NET Core 8 API — cổng 5000  ⟶ [19 README]
│   │   ├── Controllers/            → API endpoints (13 controller gốc + 4 nhóm con)
│   │   ├── Services/               → Business logic (+ Sync/ = trái tim đồng bộ)
│   │   ├── Models/                 → Entity (+ Sync/)
│   │   ├── Data/                   → AppDbContext (EF Core)
│   │   ├── DTOs/ Repositories/ Security/
│   │   ├── Migrations/             → (sinh tự động, KHÔNG cần đọc)
│   │   └── uploads/                → File thật (avatar, chứng chỉ) — KHÔNG commit
│   ├── frontend/                   → React 19 + Vite — cổng 3000  ⟶ [24 README]
│   │   └── src/{pages,components,services,store,contexts,hooks,...}
│   ├── shared/                     → Thư viện Maritime.Shared (dùng chung Edge–Shore)
│   ├── docker-compose.yml          → PostgreSQL :5434, pgAdmin :8081, Redis :6379
│   └── shore.sln
│
├── 🚢 edge_product/                → HỆ THỐNG EDGE (Trên tàu)
│   ├── edge-services/              → ASP.NET Core 8 API — cổng 5001  ⟶ [22 README]
│   │   │                             (project: EdgeCollector.csproj)
│   │   ├── Controllers/            → API endpoints (~51, chia 10 nhóm)
│   │   ├── Services/               → Business logic (+ Core/ = trái tim đồng bộ Edge)
│   │   ├── Models/ Data/ DTOs/ Repositories/ Security/
│   │   ├── Constants/ Extensions/ Helpers/ Mappings/
│   │   └── uploads/                → File thật — KHÔNG commit
│   ├── frontend-edge/              → React 19 + Zustand + Vite — cổng 3002  ⟶ [24 README]
│   │   └── src/{pages,components,services,stores,contexts,hooks,...}
│   ├── frontend-mobile/            → Flutter (app thuyền viên)  ⟶ [14 README]
│   │   └── lib/{core,data,presentation}  (Clean Architecture)
│   ├── shared/                     → Maritime.Shared
│   ├── docker-compose.yml          → PostgreSQL :5433, pgAdmin :5050
│   └── edge.sln
│
├── 📚 docs/                        → Tài liệu kỹ thuật chuyên đề (sync, ERD, AI, presentation)
├── 🔧 scripts/                     → Backup/export/restore
└── 🗄️ (nhiều file .sql seed/dump ở gốc — dữ liệu mẫu & backup)
```

> **Quy ước "vùng":** Toàn bộ tài liệu chia dự án thành **5 vùng** — Shore backend, Shore frontend, Edge backend,
> Edge frontend, Mobile. Mỗi vùng có một cụm README riêng (con số `[N README]` ở trên). Xem Mục 3.

---

## 3. BẢN ĐỒ TÀI LIỆU — 96 README CHI TIẾT

Mỗi thư mục nguồn quan trọng có một `README.md` riêng theo khuôn: **Mục đích / Cấu trúc & vai trò / Luồng hoạt
động chính / Liên kết với phần khác / Ghi chú khi đọc-dạy**. Bảng dưới là "mục lục" dẫn tới từng file. Cột **⭐**
đánh dấu file **nên đọc trước** (trọng tâm nhất của mỗi vùng).

### 3.1 🏢 SHORE BACKEND — `shore_product/backend/` (19 file)

| ⭐ | README | Nội dung |
|:---:|---|---|
| | [backend/Controllers](shore_product/backend/Controllers/README.md) | Tổng quan lớp API, 13 controller gốc; policy `InternalAccess` hiện không chặn ai theo cấu hình mặc định |
| | [Controllers/Crew](shore_product/backend/Controllers/Crew/README.md) | HR cơ bản: hồ sơ, chứng chỉ, quốc gia, chức danh, logbook |
| | [Controllers/CrewManagement](shore_product/backend/Controllers/CrewManagement/README.md) | Cỗ máy workflow: tuyển ngoài → onboarding → phân công → di chuyển → sign-on/off |
| | [Controllers/Materials](shore_product/backend/Controllers/Materials/README.md) | Kho vật tư (nêu rõ bug lệch số liệu tồn kho đã xác minh) |
| | [Controllers/Pms](shore_product/backend/Controllers/Pms/README.md) | Bảo trì phòng ngừa, quan hệ với Materials |
| | [backend/Services](shore_product/backend/Services/README.md) | Tổng quan lớp nghiệp vụ (gồm cả Network) |
| | [Services/AI](shore_product/backend/Services/AI/README.md) | Chatbot + đánh giá AI; ⚠️ `GeminiEvaluationService` thực chất gọi **Groq** |
| | [Services/Background](shore_product/backend/Services/Background/README.md) | Hàng đợi đánh giá AI + dò mạng (chưa nối vào sync thật) |
| | [Services/Crew](shore_product/backend/Services/Crew/README.md) | Nghiệp vụ HR cơ bản |
| | [Services/CrewManagement](shore_product/backend/Services/CrewManagement/README.md) | 14 file, state machine đầy đủ của quy trình nhân sự |
| ⭐ | [Services/Sync](shore_product/backend/Services/Sync/README.md) | **TRÁI TIM ĐỒNG BỘ phía Shore**: Inbox/Outbox, ConflictResolver, DLQ, file transfer |
| | [Services/Voyage](shore_product/backend/Services/Voyage/README.md) | Vòng đời chuyến đi |
| | [backend/Data](shore_product/backend/Data/README.md) | AppDbContext, NoTracking mặc định, patch SQL thô trong Program.cs |
| | [backend/Models](shore_product/backend/Models/README.md) | Bản đồ entity, 5 kiểu "trường sync" cùng tồn tại |
| ⭐ | [Models/Sync](shore_product/backend/Models/Sync/README.md) | **Bản đồ nơi model Sync thật sự nằm** (Shore vs `Maritime.Shared`) — dễ lạc nhất |
| | [backend/DTOs](shore_product/backend/DTOs/README.md) | Các DTO, cặp tên dễ nhầm Voyage vs VoyagePlanning |
| | [backend/Repositories](shore_product/backend/Repositories/README.md) | Repository pattern chỉ dùng cho 1 entity demo |
| | [backend/Security](shore_product/backend/Security/README.md) | Policy nội bộ, ký request Sync, mã hoá — cả 2 cờ bảo mật tắt theo mặc định |

### 3.2 💻 SHORE FRONTEND — `shore_product/frontend/src/` (24 file)

| ⭐ | README | Nội dung |
|:---:|---|---|
| ⭐ | [src/pages](shore_product/frontend/src/pages/README.md) | Bảng 18 module trang; đánh dấu trang thật/legacy/mock, các route không có lối vào menu |
| | [pages/SyncManagement](shore_product/frontend/src/pages/SyncManagement/README.md) | Dashboard đồng bộ Shore↔Edge |
| | [pages/VesselManagement](shore_product/frontend/src/pages/VesselManagement/README.md) | Hồ sơ tàu (12 tab + 6 tab PMS/Materials nhúng), cờ `edgeSource` từng tab |
| | [pages/PMS](shore_product/frontend/src/pages/PMS/README.md) | 4 trang PMS; WorkReportPage hiển thị dữ liệu sync từ Edge |
| | [pages/Materials](shore_product/frontend/src/pages/Materials/README.md) | 2 luồng nhập kho song song (receiptService vs stockReceipt.service) |
| | [src/components](shore_product/frontend/src/components/README.md) | Tổng quan mọi nhóm component |
| | [components/common](shore_product/frontend/src/components/common/README.md) | Button/Card/Input/Modal/Toast... (nhiều atom chỉ dùng 1 nơi) |
| | [components/layout](shore_product/frontend/src/components/layout/README.md) | TopNavLayout thật vs MainLayout/Sidebar tàn dư |
| | [components/pms](shore_product/frontend/src/components/pms/README.md) | 10 modal PMS |
| | [components/ui](shore_product/frontend/src/components/ui/README.md) | 4 primitive shadcn mồ côi |
| | [components/vessel](shore_product/frontend/src/components/vessel/README.md) | VesselMap, DisasterMapLayer, VesselDataFields |
| | [components/vessel-detail](shore_product/frontend/src/components/vessel-detail/README.md) | 12 tab hồ sơ tàu |
| | [components/Reports](shore_product/frontend/src/components/Reports/README.md) | AIInsights, AIChatWidget |
| ⭐ | [src/services](shore_product/frontend/src/services/README.md) | 27 service, phân loại 3 kiểu HTTP client + cảnh báo thiếu auth header |
| | [src/routes](shore_product/frontend/src/routes/README.md) | Điều hướng, guard (lưu ý `routes/index.tsx` rỗng) |
| | [src/contexts](shore_product/frontend/src/contexts/README.md) | Context API (auth, i18n) |
| | [src/hooks](shore_product/frontend/src/hooks/README.md) | Custom hooks |
| | [src/config](shore_product/frontend/src/config/README.md) | Hằng số cấu hình |
| | [src/lib](shore_product/frontend/src/lib/README.md) | Tiện ích |
| | [src/types](shore_product/frontend/src/types/README.md) | Kiểu TypeScript |
| | [src/store](shore_product/frontend/src/store/README.md) | ⚠️ Rỗng — chưa dùng |
| | [src/utils](shore_product/frontend/src/utils/README.md) | ⚠️ Rỗng — chưa dùng |
| | [frontend/README](shore_product/frontend/README.md) · [src/README](shore_product/frontend/src/README.md) | README gốc frontend + tổng quan `src/` |

### 3.3 🚢 EDGE BACKEND — `edge_product/edge-services/` (22 file)

| ⭐ | README | Nội dung |
|:---:|---|---|
| ⭐ | [edge-services/README](edge_product/edge-services/README.md) | Tổng quan Edge backend, luồng khởi động, các phát hiện xuyên suốt |
| | [Controllers](edge_product/edge-services/Controllers/README.md) | Tổng quan 10 nhóm controller |
| ⭐ | [Controllers/Core](edge_product/edge-services/Controllers/Core/README.md) | Auth, Sync thủ công, Health, Audit, Dashboard, Ship Data |
| | [Controllers/Crew](edge_product/edge-services/Controllers/Crew/README.md) | Thuyền viên, chứng chỉ, duyệt crew mới từ Shore |
| | [Controllers/Inventory](edge_product/edge-services/Controllers/Inventory/README.md) | Kho vật tư, 2 cơ chế nhập kho song song |
| | [Controllers/Logbooks](edge_product/edge-services/Controllers/Logbooks/README.md) | 9 sổ nhật ký SOLAS/MARPOL/BWM/STCW |
| | [Controllers/Maintenance](edge_product/edge-services/Controllers/Maintenance/README.md) | PMS ISM Code; 2 quy trình duyệt task song song |
| | [Controllers/Reporting](edge_product/edge-services/Controllers/Reporting/README.md) | Noon/Departure/Arrival/Bunker/Position + Weekly/Monthly |
| | [Controllers/Safety](edge_product/edge-services/Controllers/Safety/README.md) | Báo động, diễn tập, HSQE, Sổ tay SMS (⚠️ PIN cứng "1111") |
| | [Controllers/Voyage](edge_product/edge-services/Controllers/Voyage/README.md) | Voyage, cảng, cockpit, tài chính + Telemetry (điểm nạp cảm biến) |
| | [Services](edge_product/edge-services/Services/README.md) | Tổng quan (gộp AI/AbstractLog/Common/Parsers/Inventory) |
| ⭐ | [Services/Core](edge_product/edge-services/Services/Core/README.md) | **TRÁI TIM ĐỒNG BỘ phía Edge**: SyncService, SyncBackgroundWorker, SyncConflictHandler, Auth |
| | [Services/Sync](edge_product/edge-services/Services/Sync/README.md) | BaseSyncEnqueuerService — pattern viết sẵn nhưng không được kế thừa |
| | [Services/Logbooks](edge_product/edge-services/Services/Logbooks/README.md) | Nghiệp vụ 9 sổ, validate MARPOL Annex V |
| | [Services/Maintenance](edge_product/edge-services/Services/Maintenance/README.md) | Validate report, trừ kho tự động, thuật toán lead-time PMS |
| | [Services/Reporting](edge_product/edge-services/Services/Reporting/README.md) | ReportingService chạy thật + Generators pattern không dùng |
| | [Services/Voyage](edge_product/edge-services/Services/Voyage/README.md) | Background telemetry (Simulator/SignalK/GPS/NMEA) + 3 sync-enqueuer |
| | [DTOs](edge_product/edge-services/DTOs/README.md) · [Data](edge_product/edge-services/Data/README.md) · [Models](edge_product/edge-services/Models/README.md) | DTO; Data (outbox tự động trong SaveChanges); Models (3 cấp "syncable") |

### 3.4 📱 EDGE FRONTEND — `edge_product/frontend-edge/src/` (24 file)

| ⭐ | README | Nội dung |
|:---:|---|---|
| ⭐ | [src/pages](edge_product/frontend-edge/src/pages/README.md) | Bảng ~24 module trang, route thật, đánh dấu placeholder/ẩn route |
| | [pages/Crew](edge_product/frontend-edge/src/pages/Crew/README.md) | Thuyền viên + duyệt crew mới + danh mục chứng chỉ |
| | [pages/Material](edge_product/frontend-edge/src/pages/Material/README.md) | Danh mục vật tư & tồn kho (catalog) |
| | [pages/MaterialRequest](edge_product/frontend-edge/src/pages/MaterialRequest/README.md) | Yêu cầu cấp vật tư Draft→Submitted→Approved |
| | [pages/Maintenance](edge_product/frontend-edge/src/pages/Maintenance/README.md) | Kanban công việc bảo trì (dnd-kit) |
| | [pages/PMS](edge_product/frontend-edge/src/pages/PMS/README.md) | 10 trang PMS, workflow duyệt Chief Engineer |
| | [pages/HSQE](edge_product/frontend-edge/src/pages/HSQE/README.md) | Tài liệu SMS/ISM (cấu trúc nhiều tầng); DocumentLibrary chưa route |
| | [pages/Reporting](edge_product/frontend-edge/src/pages/Reporting/README.md) | 5 báo cáo IMO vs Weekly/Monthly |
| | [pages/Sync](edge_product/frontend-edge/src/pages/Sync/README.md) | Trạng thái/hàng đợi đồng bộ, trigger, snapshot, reset lỗi |
| | [pages/logbooks](edge_product/frontend-edge/src/pages/logbooks/README.md) | 8 loại sổ nhật ký SOLAS/MARPOL |
| | [src/components](edge_product/frontend-edge/src/components/README.md) | Tổng quan mọi nhóm component |
| | [components/crew](edge_product/frontend-edge/src/components/crew/README.md) | 5 modal crew |
| | [components/editor](edge_product/frontend-edge/src/components/editor/README.md) | RichTextEditor (tiptap 3) — thư viện mới thêm |
| | [components/logbooks](edge_product/frontend-edge/src/components/logbooks/README.md) | GarbagePartI/II — MARPOL Annex V |
| | [components/maintenance](edge_product/frontend-edge/src/components/maintenance/README.md) | KanbanBoard/Card/Column (dnd-kit) |
| | [components/pms](edge_product/frontend-edge/src/components/pms/README.md) | Modal Asset/Group/Schedule/Import/Deferral |
| | [components/reporting](edge_product/frontend-edge/src/components/reporting/README.md) | Gộp reporting/+Monthly/+Weekly; `SharedComponents.tsx` là code chết |
| ⭐ | [src/services](edge_product/frontend-edge/src/services/README.md) | 23 service; ⚠️ 3 cách gọi API cùng tồn tại, vài service trùng tên |
| ⭐ | [src/stores](edge_product/frontend-edge/src/stores/README.md) | 3 Zustand store; ⚠️ `lib/store.ts` là store thứ 4 đã chết |
| | [src/hooks](edge_product/frontend-edge/src/hooks/README.md) · [src/contexts](edge_product/frontend-edge/src/contexts/README.md) | Hooks (React Query, Dexie offline draft); I18nContext |
| | [src/config](edge_product/frontend-edge/src/config/README.md) · [src/lib](edge_product/frontend-edge/src/lib/README.md) · [src/types](edge_product/frontend-edge/src/types/README.md) | Cấu hình; utils/Dexie; ⚠️ `MaintenanceTask` định nghĩa trùng 2 nơi |

### 3.5 📲 MOBILE (Flutter) — `edge_product/frontend-mobile/lib/` (12 file)

| ⭐ | README | Nội dung |
|:---:|---|---|
| ⭐ | [lib/README](edge_product/frontend-mobile/lib/README.md) | Kiến trúc 2 tầng (presentation → data); ⚠️ `watchkeeping_provider` trỏ nhầm cổng Shore 5000 |
| | [lib/core](edge_product/frontend-mobile/lib/core/README.md) | 8 thư mục con; ⚠️ 7 ngôn ngữ khai báo nhưng chỉ 2 hoạt động |
| | [core/di](edge_product/frontend-mobile/lib/core/di/README.md) | GetIt: thứ tự đăng ký singleton |
| | [core/network](edge_product/frontend-mobile/lib/core/network/README.md) | Dio/Retrofit/ApiInterceptor, baseUrl, refresh-token chưa hoàn thiện |
| | [core/cache](edge_product/frontend-mobile/lib/core/cache/README.md) | CacheManager (Hive TTL) + SyncQueue store-and-forward — trọng tâm offline |
| | [core/storage](edge_product/frontend-mobile/lib/core/storage/README.md) | ServerConfigStorage; ⚠️ ServerConfigScreen lưu sai key nên không hoạt động |
| | [lib/data](edge_product/frontend-mobile/lib/data/README.md) | api/Retrofit, 25 model (5 mồ côi), luồng lấy dữ liệu |
| | [data/repositories](edge_product/frontend-mobile/lib/data/repositories/README.md) | So sánh chiến lược offline giữa 6 repository |
| | [lib/presentation](edge_product/frontend-mobile/lib/presentation/README.md) | Provider pattern, widgets |
| | [presentation/providers](edge_product/frontend-mobile/lib/presentation/providers/README.md) | 5 provider, 2 cách lấy dependency |
| | [presentation/screens](edge_product/frontend-mobile/lib/presentation/screens/README.md) | Bảng 20 screen theo 8 nhóm tính năng |
| | [lib/l10n](edge_product/frontend-mobile/lib/l10n/README.md) | flutter gen-l10n; ⚠️ 5/7 ngôn ngữ sẽ crash |

---

## 4. HƯỚNG DẪN CÀI ĐẶT & CHẠY (THỰC CHIẾN)

> Phần này viết lại từ **quá trình chạy thật** hệ thống, gồm cả các lỗi thực tế đã gặp và cách xử lý.

### 4.1 Yêu cầu môi trường

| Công cụ | Phiên bản | Ghi chú |
|---|---|---|
| .NET SDK | **8.0** | Chạy 2 backend |
| Node.js | **20+** (đã test 22) | Chạy 2 frontend Vite |
| Docker Desktop | mới nhất | Chạy PostgreSQL (+ pgAdmin, Redis) |
| Flutter SDK | 3.x | Chỉ khi chạy app mobile |
| RAM | 8GB+ | Cả 4 tiến trình + Docker |

### 4.2 Bản đồ cổng (PORT MAP) — thuộc lòng

| Dịch vụ | URL | Cổng |
|---|---|---|
| Shore Frontend | http://localhost:3000 | 3000 |
| Shore API / Swagger | http://localhost:5000/swagger | 5000 |
| Edge Frontend | http://localhost:3002 | 3002 |
| Edge API | http://localhost:5001 | 5001 |
| Shore PostgreSQL | (Docker) | 5434 |
| Edge PostgreSQL | (Docker) | 5433 |
| pgAdmin (Shore) | http://localhost:8081 | 8081 |
| pgAdmin (Edge) | http://localhost:5050 | 5050 |
| Redis (Shore) | (Docker) | 6379 |

### 4.3 Khởi động — chạy TỪNG PHẦN (khuyến nghị khi dev)

Mở **mỗi lệnh trong một cửa sổ terminal PowerShell riêng** để xem log độc lập, không cần tắt phần này để chạy phần kia.

**① Database (Docker) — chạy 1 lần, giữ nền:**
```powershell
# Shore PostgreSQL (:5434) + Redis + pgAdmin
cd E:\NCKH\Martime_product_v1.1\shore_product
docker compose up -d postgres

# Edge PostgreSQL (:5433)
cd E:\NCKH\Martime_product_v1.1\edge_product\edge-services
docker compose up -d edge-postgres
```

**② Shore Backend (:5000):**
```powershell
cd E:\NCKH\Martime_product_v1.1\shore_product\backend
dotnet run --urls "http://localhost:5000"
```

**③ Shore Frontend (:3000):**
```powershell
cd E:\NCKH\Martime_product_v1.1\shore_product\frontend
npm install    # chỉ lần đầu hoặc khi đổi package.json
npm run dev
```

**④ Edge Backend (:5001):**
```powershell
cd E:\NCKH\Martime_product_v1.1\edge_product\edge-services
dotnet run --project EdgeCollector.csproj --urls "http://localhost:5001"
```

**⑤ Edge Frontend (:3002):**
```powershell
cd E:\NCKH\Martime_product_v1.1\edge_product\frontend-edge
npm install    # BẮT BUỘC lần đầu — thiếu là lỗi "@tiptap/react" (xem 10.2)
npm run dev
```

**⑥ (Tuỳ chọn) Mobile Flutter:**
```powershell
cd E:\NCKH\Martime_product_v1.1\edge_product\frontend-mobile
flutter pub get
flutter run -d emulator-5554
```

### 4.4 Xác nhận đã chạy đúng

Sau khi cả 4 tiến trình lên, kiểm tra nhanh:

```powershell
curl http://localhost:5000/swagger/index.html   # Shore API  → 200
curl http://localhost:3000/                       # Shore FE   → 200
curl http://localhost:5001/                       # Edge API   → 401 (đang chạy, cần auth — BÌNH THƯỜNG)
curl http://localhost:3002/                       # Edge FE    → 200
```

> **Lưu ý về Edge API:** trả **401/404 ở root là bình thường** — API vẫn chạy, chỉ là Swagger UI bị tắt ngoài
> môi trường Development và mọi endpoint yêu cầu xác thực. Đừng tưởng nhầm là lỗi.

### 4.5 Cấu hình đồng bộ (nếu cần)

`edge_product/edge-services/appsettings.json`:
```json
{
  "Sync": { "BatchSize": 100, "SyncInterval": 30, "RetryAttempts": 5, "NetworkType": "Shore_WiFi" },
  "ShoreApi": { "BaseUrl": "http://localhost:5000" }
}
```
- `NetworkType` quyết định mức Priority nào được phép sync (xem 1.4). Để test đầy đủ, đặt `Shore_WiFi`.
- `ShoreApi.BaseUrl` phải trỏ đúng cổng Shore backend (mặc định 5000).

---

## 5. CÁC LUỒNG NGHIỆP VỤ XUYÊN SUỐT (END-TO-END)

Đây là phần quan trọng nhất để **hiểu hệ thống như một tổng thể**, thay vì từng mảnh rời rạc. Mỗi luồng dưới đây
đi xuyên nhiều thư mục — kèm README nên đọc song song.

### 5.1 ⭐ Luồng ĐỒNG BỘ (Sync) — xương sống toàn hệ thống

Đây là cơ chế khiến 2 hệ thống độc lập trở thành một. **Bắt buộc hiểu trước mọi thứ khác.**

#### A. Push: Edge → Shore (đẩy dữ liệu vận hành lên bờ)

```
[TÀU]  App lưu dữ liệu (vd tạo Noon Report)
   │  EF Core SaveChanges phát hiện entity ISyncableEntity thay đổi
   ▼
   SyncQueue: tự tạo 1 bản ghi hàng đợi (cùng transaction) + gán Priority
   │
   │  ⏱ SyncBackgroundWorker (Services/Core) chạy mỗi 30s:
   │     1. kiểm tra loại kết nối mạng hiện tại
   │     2. lọc queue theo Priority phù hợp băng thông
   │     3. gom batch (tối đa 100) → POST /api/sync
   ▼
[BỜ]  SyncController.Sync()  →  SyncInboxService (file lớn nhất backend, 3345 dòng)
   │     • kiểm tra Idempotency (SyncIdempotencyRecord: {Table}:{Key}:{Version})
   │     • gọi ConflictResolverService → quyết định ai thắng theo từng bảng/trường
   │     • áp dụng vào DB Shore, ghi SyncLog
   ▼
   Edge nhận kết quả → đánh dấu SyncedAt cho item thành công
                     → Exponential backoff (30/60/120s) cho item lỗi, tối đa 5 lần → DLQ
```

**Đọc kèm:** [Edge Services/Core](edge_product/edge-services/Services/Core/README.md) (đầu đẩy) →
[Shore Services/Sync](shore_product/backend/Services/Sync/README.md) (đầu nhận).

#### B. Pull: Shore → Edge (kéo master data / thay đổi HR xuống tàu)

```
[BỜ]  Business logic đổi crew/cert/master data → SyncOutboxService.EnqueueAsync()
   │     TargetNode = mã tàu cụ thể HOẶC "*" (broadcast toàn đội)
   │     Deduplication: nếu item chưa giao cho cùng (node, table, key) đã có → cập nhật payload
   ▼
[TÀU]  ⏱ SyncBackgroundWorker mỗi 30s:
   │     GET /api/sync/pull?nodeId={IMO}&since={lastPull}&cursor={lastId}&pageSize=50
   │     (phân trang theo cursor)
   ▼
   SyncConflictHandler (Services/Core): áp dụng dữ liệu Shore vào DB Edge (logic đảo ngược)
   │     • Master data từ Shore → luôn nhận
   │     • Crew mới từ Shore → đặt IsOnboard=false → vào hàng chờ duyệt (xem 5.2)
   ▼
   POST /api/sync/acknowledge → Shore đánh dấu DeliveredAt
```

#### C. Heartbeat & giám sát

```
[TÀU] POST /api/sync/heartbeat { nodeId, shipName, imoNumber, networkType }  (định kỳ)
[BỜ]  SyncHealthMonitorService (5 phút/lần): đánh dấu tàu offline khi hết heartbeat,
      dọn outbox/idempotency cũ, làm mới SyncTableStats
```

**Chống trùng lặp & bảo mật:** Idempotency key + Nonce registry (chống replay) + (tuỳ chọn) ký HMAC request.
⚠️ Ký request và policy `InternalAccess` **tắt theo cấu hình mặc định** — xem Mục 8.

### 5.2 Luồng ONBOARDING THUYỀN VIÊN (Shore → Edge)

Minh hoạ rõ nhất cho **Domain-Based Conflict Resolution** và quyền sở hữu chia theo trường.

```
[BỜ] HR tạo/duyệt thuyền viên → SyncOutbox đẩy xuống tàu (TargetNode = tàu được phân)
   ▼
[TÀU] SyncConflictHandler nhận crew mới → đặt OnboardStatus = PendingReview, IsOnboard = false
   ▼
   Thuyền trưởng mở pages/Crew (crew/members) → khu "duyệt crew mới":
        ├─ Approve → POST /api/crew/:id/approve → Approved, IsOnboard=true, EmbarkDate=now, tự cấp tài khoản
        ├─ Hold    → POST /api/crew/:id/hold    → OnHold, ghi ReviewChecklist/Notes
        └─ Reject  → POST /api/crew/:id/reject  → Rejected
   ▼
   Mọi thay đổi enqueue SyncQueue → đẩy ngược trạng thái onboard lên Shore
```

**Quy tắc sở hữu (quan trọng):** HR (họ tên, DOB, mã BHXH/thuế) do **Shore** sở hữu; trạng thái vận hành
(`IsOnboard`, ngày lên/xuống tàu) do **Edge** sở hữu → `IsOnboard` **không** sửa ở form cập nhật, chỉ đổi qua
approve/hold/reject. **Đọc kèm:** [Edge pages/Crew](edge_product/frontend-edge/src/pages/Crew/README.md) +
[Shore Controllers/CrewManagement](shore_product/backend/Controllers/CrewManagement/README.md).

### 5.3 Luồng PMS — Bảo trì phòng ngừa (ISM Code)

Luồng phức tạp nhất phía Edge, có **2 cơ chế tiến trạng thái** khác nhau:

```
1. LẬP LỊCH:  pages/PMS/MasterSchedule → tạo MaintenanceSchedule (theo lịch CALENDAR hoặc theo giờ máy RUNNING_HOURS)
                                              │
2. SINH TASK: hệ thống sinh MaintenanceTask từ schedule
                                              │
3. TIẾN TRẠNG THÁI (2 đường khác nhau — ĐỪNG nhầm):
      • CALENDAR/AD_HOC → AutoCorrectTaskStatuses() chạy mỗi lần GET tasks
      • RUNNING_HOURS   → CHỈ EquipmentAssetController.CheckAndPromoteTasksByRunningHours() (khi cập nhật giờ máy)
                                              │
4. THỰC THI (Kanban kéo-thả, pages/Maintenance):
      SCHEDULED → DUE/OVERDUE → [Start] IN_PROGRESS → [Submit] PENDING_APPROVAL → [Verify] COMPLETED / RECTIFY
                                              │
5. KHI DUYỆT (Verify=APPROVE) — TaskWorkflowController:
      • parse SparePartsUsed JSON → TRỪ KHO THẬT (InventoryStock, ưu tiên vị trí nhiều nhất), chặn nếu INSUFFICIENT_STOCK
      • đẩy EquipmentAsset.CurrentRunningHours nếu giờ báo cáo lớn hơn
      • sinh chu kỳ PERIODIC tiếp theo (PostApprovalScheduleUpdate)
```

⚠️ **Có 2 quy trình duyệt task song song** trong code (`MaintenanceController` cũ có phần kiểm rank đã bị comment,
và `TaskWorkflowController` mới bọc transaction) — xem
[Edge Controllers/Maintenance](edge_product/edge-services/Controllers/Maintenance/README.md). **Đọc kèm:**
[Edge pages/Maintenance](edge_product/frontend-edge/src/pages/Maintenance/README.md) +
[pages/PMS](edge_product/frontend-edge/src/pages/PMS/README.md).

### 5.4 Luồng LOGISTICS VẬT TƯ (Materials)

Ba module nối tiếp, đừng nhầm lẫn:

```
Material (danh mục)          MaterialRequest (xin cấp)         StockReceipt (nhập kho)          Inventory (tồn)
─────────────────            ────────────────────────         ───────────────────             ────────────────
Khai báo item + category     YC-<ngày>-<seq>                  NK-<ngày>-<seq>                 Tồn theo vị trí kho
(pages/Material)             Draft→Submitted→Approved   ─────► Complete: +InventoryStock ─────► GetHistory gộp
                             (pages/MaterialRequest)           đánh dấu request Completed       3 nguồn IN/OUT
```

- **Nhập kho** (`StockReceipt.Complete`) là giao dịch thật: cộng `InventoryStock.Quantity`, cập nhật `UnitCost`,
  cộng `MaterialItem.OnHandQuantity`, và đóng luôn `MaterialRequest` liên kết.
- **Sổ cái tồn kho** (`Inventory.GetHistory`) được **tính tại query-time** bằng cách gộp 3 nguồn: nhập kho (IN),
  yêu cầu đã duyệt (OUT), tiêu hao bảo trì (parse `MaintenanceHistories.SparePartsUsed`, OUT) — **không có bảng sổ
  cái riêng**. Đây là điểm nghiệp vụ đặc thù nhất.

⚠️ Có **2 cơ chế nhập kho song song** (StockReceipt thủ công vs MaterialReceipt import Excel). **Đọc kèm:**
[Edge Controllers/Inventory](edge_product/edge-services/Controllers/Inventory/README.md) +
[Edge pages/Material](edge_product/frontend-edge/src/pages/Material/README.md).

### 5.5 Luồng BÁO CÁO IMO (Reporting)

```
Sĩ quan điền form (pages/Reporting) → 5 loại báo cáo tuân thủ IMO:
   Noon (giữa trưa) · Departure (rời cảng) · Arrival (đến cảng) · Bunker (nhận nhiên liệu) · Position (vị trí)
   ▼
POST → ReportingService (Edge) validate → lưu DB → enqueue SyncQueue (Priority Operational) → đẩy lên Shore
   ▼
[BỜ] SyncInboxService nhận Noon Report → tự enqueue AI đánh giá (Services/Background)
   ▼
Weekly/MonthlyReport: tổng hợp lại từ các báo cáo lẻ (phục vụ IMO DCS/MRV/CII)
```

**Đọc kèm:** [Edge Controllers/Reporting](edge_product/edge-services/Controllers/Reporting/README.md) +
[Edge Services/Reporting](edge_product/edge-services/Services/Reporting/README.md).

### 5.6 Luồng LOGBOOK (Sổ nhật ký SOLAS/MARPOL)

9 loại sổ chính thức dùng **chung một khuôn** CRUD + Sign (ký số) + Sync:
Deck, Engine, Oil Record Book, Garbage (Part I/II — MARPOL Annex V), Ballast Water, Watchkeeping, Voyage, Abstract Log.

```
Ghi entry (EntryOrigin="EDGE") → lưu DB + enqueue SyncQueue → sync lên Shore
Sửa/Xoá → tăng SyncVersion / enqueue DELETE
```
Mỗi loại có validate nghiệp vụ riêng (vd Garbage: nhóm rác H/K bị cấm xả biển). **Đọc kèm:**
[Edge Controllers/Logbooks](edge_product/edge-services/Controllers/Logbooks/README.md) +
[Services/Logbooks](edge_product/edge-services/Services/Logbooks/README.md).

---

## 6. MÔ HÌNH DỮ LIỆU & QUYỀN SỞ HỮU (OWNERSHIP MATRIX)

"Ai thắng khi xung đột" được quyết định theo **chủ sở hữu dữ liệu**, không phải theo thời gian ghi.

| Loại dữ liệu | Chủ sở hữu | Ví dụ |
|---|---|---|
| Master Data (chứng chỉ, quốc gia, rank) | **Shore** | Dữ liệu tham chiếu do HR/Compliance quản lý |
| Crew HR (mã BHXH, mã thuế) | **Shore** | Thông tin hành chính |
| Crew Operational (trạng thái onboard) | **Edge** | IsOnboard, EmbarkDate, DisembarkDate, AvatarUrl |
| Service Records | **Edge** | Lịch sử phục vụ thực tế trên tàu |
| Telemetry (GPS, engine, generator) | **Edge** | Dữ liệu cảm biến |
| Voyage Operational | **Edge** | Noon reports, logbooks, engine logs |
| Vessel Technical | **Edge** | VesselType, MainEnginePower, Dimensions, FlagState |
| Vessel Commercial | **Shore** | Chủ tàu, bảo hiểm, thuê tàu (chỉ nhận lần sync đầu) |
| Crew Certs — metadata | **Shore** | Số chứng chỉ, ngày cấp/hết hạn |
| Crew Certs — file scan | **Edge** | Bản scan/upload trên tàu |

**Merge Crew Member** (trường hợp phức tạp nhất — chia sở hữu theo TỪNG TRƯỜNG):

| Shore giữ | Edge giữ |
|---|---|
| SocialInsuranceNumber, TaxIdNumber | IsOnboard, Embark/Disembark Date & Port, ShipId, CurrentShipName, AvatarUrl |
| Trường khác (FullName, DOB, Nationality...): origin=Shore→áp dụng; origin=Edge→chỉ áp dụng nếu timestamp mới hơn | |

**Interface chuẩn** (`Maritime.Shared`): mọi entity đồng bộ implement `ISyncableEntity`:
`IsSynced`, `OriginNode` ("SHIP_01"/"SHORE"), `SyncVersion` (optimistic concurrency), `CreatedAt`, `UpdatedAt`.
Chi tiết bản đồ model: [Shore Models/Sync](shore_product/backend/Models/Sync/README.md).

---

## 7. CÔNG NGHỆ SỬ DỤNG

| Tầng | Shore | Edge |
|---|---|---|
| **Backend** | ASP.NET Core 8, EF Core + Npgsql, PostgreSQL 15, **Redis 7** | ASP.NET Core 8, EF Core + Npgsql, PostgreSQL 15 |
| **Auth** | JWT Bearer + role policies | Token tự chế + `SessionAuthMiddleware` (⚠️ xem Mục 8) |
| **Frontend** | React 19 + TS 5.9, Vite 7 (rolldown), **Context API** | React 19 + TS 5.9, Vite 6, **Zustand 5** (persistent) |
| **UI** | Tailwind 3.4, Radix, Lucide | Tailwind 3.4, Radix, Lucide, **dnd-kit**, **tiptap 3** |
| **Charts/Export** | XLSX | **Recharts**, jsPDF, ExcelJS, XLSX |
| **Offline FE** | — | **Dexie** (IndexedDB) cho draft offline |
| **Mobile** | — | Flutter 3.x, Provider, **Dio + Retrofit**, **Hive**, GetIt |
| **Background** | AlertService, CertExpiryMonitor, SyncHealthMonitor | SyncBackgroundWorker, TelemetrySimulator, SignalKCollector, DataCleanup |

---

## 8. QUY ƯỚC & CẠM BẪY KHI PHÁT TRIỂN (GOTCHAS)

> Tổng hợp các phát hiện **đã xác minh bằng đọc code/grep** khi viết 96 README. Đây là những chỗ khiến người mới
> mất nhiều giờ nhất — đọc trước khi sửa.

### 8.1 🔒 Bảo mật (lưu ý khi demo/triển khai thật)
- **Edge** hash mật khẩu bằng **SHA256 không salt** (`CrewController.HashPassword`); token là chuỗi tự chế
  `access_{userId}_{crewId}_{timestamp}_{random}`, không phải JWT chuẩn.
- Nhiều controller xử lý **PII nhạy cảm** (hộ chiếu, sức khoẻ) **không có `[Authorize]`**.
- Policy `InternalAccess` và **ký HMAC request Sync** đang **TẮT theo cấu hình mặc định** ở cả 2 backend — nghĩa là
  endpoint sync hiện không bị chặn. Bật lại qua `SyncSecurity`/`Security` trong appsettings trước khi lên production.
- **Shore** dùng PBKDF2 (100k vòng, NIST SP 800-132) — chuẩn hơn Edge; đừng nhầm 2 cơ chế hash.
- Sổ tay SMS (Edge Safety) có **PIN cứng "1111"** trong code.

### 8.2 🧟 Code chết / chưa nối dây (đừng sửa nhầm)
- Frontend Edge: `lib/store.ts` (Zustand store thứ 4 không ai import), `components/reporting/SharedComponents.tsx`,
  `pages/Material/oldMaterialPage.tsx`, `pages/MaterialRequest/MaterialRequestPage.old.tsx`.
- Frontend Shore: `store/`, `utils/index.ts`, `routes/index.tsx`, `services/api.ts`, `hooks/useToggle.ts` — **rỗng**.
- Backend Edge: `Services/Reporting/Generators` (không dùng), `Services/Sync/BaseSyncEnqueuerService` (không kế thừa),
  `QueryPerformanceInterceptor` (không đăng ký).
- Mobile: 5 model DTO mồ côi trong `data/models/`.

### 8.3 🔀 Cơ chế song song cho cùng bài toán
- **2 quy trình duyệt task** Maintenance (`MaintenanceController` cũ vs `TaskWorkflowController` mới).
- **2 cơ chế nhập kho** (StockReceipt thủ công vs MaterialReceipt import Excel).
- **3 cách gọi API** ở Edge frontend (`apiClient` / `axios` / `fetch` thô) — xem
  [services/README](edge_product/frontend-edge/src/services/README.md).
- **2 hệ thống toast** ở Shore frontend (`useToast` tự viết vs `sonner`).
- **React Query** đã cấu hình ở Shore nhưng **chưa nơi nào gọi `useQuery`**.

### 8.4 🏷️ Đặt tên gây nhầm
- `GeminiEvaluationService` (Shore) **thực chất gọi Groq**, không phải Gemini.
- `MaintenanceTask` được định nghĩa **trùng ở 2 file type** khác nhau (Edge frontend).
- Vài service trùng tên 2 bên (`materialService`, `maintenance.service.ts` vs `maritimeService.maintenance`).
- Mobile `watchkeeping_provider.dart` **trỏ nhầm cổng Shore 5000** thay vì Edge 5001.
- Mobile khai báo **7 ngôn ngữ nhưng chỉ 2 hoạt động** — 5 ngôn ngữ còn lại có thể **crash**.

### 8.5 Quy ước chung
- Entity cần đồng bộ **phải** implement `ISyncableEntity`; mọi ghi phải enqueue `SyncQueue` (Edge) hoặc
  `SyncOutbox` (Shore) — nếu không, dữ liệu sẽ không sync.
- File build/artifact (`bin/`, `obj/`, `node_modules/`, `tmp/edge-build-check/`) **không phải** nguồn — bỏ qua khi đọc.
- Uploads (`uploads/`) là **file thật lúc chạy**, không commit.

---

## 9. LỘ TRÌNH HỌC CHO NGƯỜI MỚI (ONBOARDING PATH)

Thứ tự đề xuất để một dev mới đạt năng suất nhanh nhất:

| Ngày | Mục tiêu | Đọc / Làm |
|---|---|---|
| **1** | Chạy được toàn hệ thống | Mục 4 sổ tay này → chạy 4 tiến trình → mở 3000 & 3002 |
| **1** | Hiểu bức tranh lớn | Mục 1 + `README.md` gốc (mục 1–2) |
| **2** | Hiểu đồng bộ (quan trọng nhất) | Mục 5.1 → [Edge Services/Core](edge_product/edge-services/Services/Core/README.md) → [Shore Services/Sync](shore_product/backend/Services/Sync/README.md) |
| **2** | Hiểu quyền sở hữu dữ liệu | Mục 6 + [Models/Sync](shore_product/backend/Models/Sync/README.md) |
| **3** | Nắm 1 luồng nghiệp vụ đầu-cuối | Mục 5.2 (onboarding) → đọc README Crew cả 2 phía |
| **3–4** | Vào vùng mình phụ trách | Mở README `pages/` hoặc `Controllers/` tương ứng (Mục 3) |
| **4** | Tránh cạm bẫy | Đọc Mục 8 (Gotchas) trước khi commit dòng đầu tiên |
| **5** | Thực hành | Sửa 1 bug nhỏ trong vùng của mình, chạy lại, kiểm tra sync 2 chiều |

**Nguyên tắc vàng:** với bất kỳ thư mục nào bạn sắp sửa, **mở `README.md` trong chính thư mục đó trước** — nó có
mục "Ghi chú khi đọc/dạy" cảnh báo đúng cạm bẫy của khu vực đó.

---

## 10. XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING)

### 10.1 Cổng bị chiếm (Port already in use)
Triệu chứng: backend/frontend không lên, hoặc Vite tự nhảy sang cổng khác (3003...).
```powershell
# Tìm tiến trình giữ cổng (vd 3002)
Get-NetTCPConnection -LocalPort 3002 -State Listen | Select-Object OwningProcess
# Kết thúc tiến trình đó
Stop-Process -Id <PID> -Force
```
> Thực tế đã gặp: cổng **5000** bị một container Docker của dự án khác chiếm (`qlth-backend`) →
> `docker stop qlth-backend` để giải phóng.

### 10.2 Lỗi `Failed to resolve import "@tiptap/react"` (Edge frontend)
Nguyên nhân: `node_modules` cũ, thiếu package tiptap (thư viện mới thêm).
```powershell
cd E:\NCKH\Martime_product_v1.1\edge_product\frontend-edge
npm install
# Nếu Vite đang chạy: Ctrl+C rồi npm run dev lại để nạp dependency mới
```
> Quy tắc chung: **mỗi khi `package.json` đổi**, phải `npm install` lại và **khởi động lại** dev server của
> đúng frontend đó (không cần đụng phần khác).

### 10.3 Database không kết nối được
```powershell
# Kiểm tra container Postgres đang chạy & healthy
docker ps
# Kiểm tra Edge DB sẵn sàng
docker exec maritime-edge-postgres pg_isready -U edge_user -d maritime_edge
```
Nếu chưa chạy: `docker compose up -d edge-postgres` (Edge) / `docker compose up -d postgres` (Shore).

### 10.4 Edge API trả 401/404 ở root
**Không phải lỗi** — Swagger tắt ngoài Development và mọi endpoint cần auth. Kiểm tra bằng log
`Now listening on: http://localhost:5001` là đủ biết backend đã lên.

### 10.5 Sync không chạy / dữ liệu không qua được
- Kiểm tra `ShoreApi.BaseUrl` (Edge appsettings) trỏ đúng cổng Shore.
- Kiểm tra `NetworkType`: nếu để `Satellite_Iridium` thì chỉ dữ liệu **Critical** được đẩy (xem 1.4).
- Xem hàng đợi & lỗi ở trang **Sync** (Edge FE, `/sync`) hoặc **SyncManagement** (Shore FE).

---

## 11. BẢNG THUẬT NGỮ (GLOSSARY)

### Kỹ thuật / Kiến trúc
| Thuật ngữ | Nghĩa |
|---|---|
| **Edge** | Hệ thống chạy trên tàu (offline-first) |
| **Shore** | Hệ thống trung tâm trên bờ |
| **Store-and-Forward** | Xếp dữ liệu vào hàng đợi, gửi khi có mạng |
| **SyncQueue / SyncOutbox** | Hàng đợi đẩy đi của Edge / hàng đợi gửi-cho-Edge của Shore |
| **Idempotency** | Xử lý cùng 1 item nhiều lần không gây trùng |
| **ISyncableEntity** | Interface mọi entity đồng bộ phải implement |
| **OriginNode** | Nguồn gốc bản ghi ("SHORE" hoặc mã tàu) |
| **DLQ** | Dead-Letter Queue — nơi chứa item sync lỗi lặp lại |
| **Heartbeat** | Tín hiệu Edge báo "còn sống" + loại mạng cho Shore |

### Nghiệp vụ hàng hải
| Thuật ngữ | Nghĩa |
|---|---|
| **PMS** | Planned Maintenance System — bảo trì phòng ngừa (ISM Code) |
| **Noon Report** | Báo cáo lúc giữa trưa (vị trí, tiêu hao nhiên liệu...) |
| **Running Hours** | Số giờ chạy máy — mốc kích hoạt bảo trì theo giờ |
| **PIC** | Person In Charge — người phụ trách một task |
| **Seaman Book** | Sổ thuyền viên |
| **Embark / Disembark** | Lên tàu / rời tàu |

### Tiêu chuẩn quốc tế
| Viết tắt | Đầy đủ | Áp dụng |
|---|---|---|
| **IMO DCS** | Data Collection System | FuelAnalytics, NoonReport |
| **IMO MRV** | Monitoring, Reporting, Verification | Theo dõi khí thải |
| **IMO CII** | Carbon Intensity Indicator | Chấm điểm hiệu suất tàu |
| **SOLAS** | Safety of Life at Sea | Logbook, Watchkeeping, Drills |
| **MARPOL** | Marine Pollution Prevention | Oil Record Book, Garbage, Ballast |
| **ISM Code** | International Safety Management | PMS, Audit trail |
| **ISPS** | Ship & Port Facility Security | Compliance |
| **STCW** | Standards of Training, Certification, Watchkeeping | Chứng chỉ thuyền viên |
| **MLC** | Maritime Labour Convention | Quản lý thuyền viên, service record |

---

> **Kết:** Sổ tay này là điểm vào. Chi tiết luôn nằm ở **96 README trong từng thư mục** (Mục 3) — chúng bám sát
> code thật và được cập nhật cùng code. Khi tài liệu và code mâu thuẫn, **code là chân lý** — hãy sửa README cho
> đúng và giữ thói quen đó cho nhóm.




