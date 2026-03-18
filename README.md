# 🚢 MARITIME MANAGEMENT SYSTEM — Shore-Edge Architecture

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-blue.svg)](https://reactjs.org/)
[![Flutter](https://img.shields.io/badge/Flutter-3.x-02569B.svg)](https://flutter.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791.svg)](https://postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED.svg)](https://docker.com/)

Hệ thống quản lý hạm đội tàu biển theo mô hình **Edge-Shore Computing** — thiết kế cho môi trường hàng hải thực tế, nơi kết nối mạng không liên tục và dữ liệu phải được đồng bộ hai chiều giữa tàu (Edge) và trung tâm điều hành trên bờ (Shore).

---

## Mục Lục

- [1. Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
- [2. Mô Hình Edge-Shore Computing](#2-mô-hình-edge-shore-computing)
- [3. Shore System (Trên Bờ)](#3-shore-system-trên-bờ)
- [4. Edge System (Trên Tàu)](#4-edge-system-trên-tàu)
- [5. Cơ Chế Đồng Bộ Dữ Liệu (Sync Protocol)](#5-cơ-chế-đồng-bộ-dữ-liệu-sync-protocol)
- [6. Xử Lý Xung Đột (Conflict Resolution)](#6-xử-lý-xung-đột-conflict-resolution)
- [7. Shared Library](#7-shared-library)
- [8. Cấu Trúc Thư Mục](#8-cấu-trúc-thư-mục)
- [9. Công Nghệ Sử Dụng](#9-công-nghệ-sử-dụng)
- [10. Hướng Dẫn Cài Đặt & Chạy](#10-hướng-dẫn-cài-đặt--chạy)
- [11. Tiêu Chuẩn Hàng Hải](#11-tiêu-chuẩn-hàng-hải)

---

## 1. Tổng Quan Kiến Trúc

Dự án được tách thành **2 hệ thống độc lập**, mỗi hệ thống có backend API, frontend dashboard, và database riêng biệt. Chúng giao tiếp qua **giao thức đồng bộ store-and-forward** với khả năng hoạt động offline hoàn toàn.

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                          SHORE SYSTEM (Trên bờ)                             │
│                                                                              │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │  Frontend    │  │  Backend API │  │ PostgreSQL  │  │     Redis       │  │
│  │  React 19    │◄►│  .NET 8      │◄►│  Port 5434  │  │  Cache/Session  │  │
│  │  Port 3000   │  │  Port 5000   │  │  110+ bảng  │  │  Port 6379      │  │
│  └─────────────┘  └──────┬───────┘  └─────────────┘  └─────────────────┘  │
│                           │                                                  │
│              ┌────────────┴────────────┐                                    │
│              │  Sync Infrastructure    │                                    │
│              │  • SyncInboxService     │                                    │
│              │  • SyncOutboxService    │                                    │
│              │  • ConflictResolver     │                                    │
│              │  • SyncDashboard        │                                    │
│              └────────────┬────────────┘                                    │
└───────────────────────────┼─────────────────────────────────────────────────┘
                            │
               ┌────────────┴────────────┐
               │   VSAT / 4G / Iridium   │
               │   Kết nối không liên tục │
               └────────────┬────────────┘
                            │
┌───────────────────────────┼─────────────────────────────────────────────────┐
│                           │           EDGE SYSTEM (Trên tàu)                │
│              ┌────────────┴────────────┐                                    │
│              │  Sync Infrastructure    │                                    │
│              │  • SyncQueue (outgoing) │                                    │
│              │  • SyncBackgroundWorker │                                    │
│              │  • SyncConflictHandler  │                                    │
│              └────────────┬────────────┘                                    │
│                           │                                                  │
│  ┌─────────────┐  ┌──────┴───────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │  Frontend    │  │  Backend API │  │ PostgreSQL  │  │  Mobile App     │  │
│  │  React 19    │◄►│  .NET 8      │◄►│  Port 5433  │  │  Flutter        │  │
│  │  Port 3002   │  │  Port 5001   │  │  60+ bảng   │  │  (Android/iOS)  │  │
│  └─────────────┘  └──────┬───────┘  └─────────────┘  └─────────────────┘  │
│                           │                                                  │
│              ┌────────────┴────────────┐                                    │
│              │  Background Services    │                                    │
│              │  • TelemetrySimulator   │                                    │
│              │  • SignalK Collector    │                                    │
│              │  • DataCleanup          │                                    │
│              └─────────────────────────┘                                    │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Nguyên tắc thiết kế cốt lõi:**

| Nguyên tắc | Mô tả |
|---|---|
| **Offline-First** | Edge hoạt động hoàn toàn độc lập khi mất kết nối |
| **Store-and-Forward** | Dữ liệu được xếp hàng đợi và gửi khi có mạng |
| **Network-Aware** | Ưu tiên đồng bộ theo loại kết nối (Iridium → chỉ Critical) |
| **Domain-Based Conflict Resolution** | Mỗi bảng/trường có quy tắc xung đột riêng |
| **Bidirectional Sync** | Dữ liệu chảy hai chiều Shore ↔ Edge |
| **Idempotent Processing** | Cùng một bản ghi sync nhiều lần không gây trùng lặp |

---

## 2. Mô Hình Edge-Shore Computing

### 2.1 Tại Sao Edge Computing?

Trong hàng hải, tàu hoạt động ở giữa đại dương — nơi kết nối Internet rất hạn chế:

| Loại kết nối | Băng thông | Chi phí | Khả dụng |
|---|---|---|---|
| **Iridium** (vệ tinh) | 0.12 – 1 kbps | Rất cao | Toàn cầu |
| **VSAT** (vệ tinh) | 64 kbps – 2 Mbps | Cao | Gần toàn cầu |
| **4G/LTE** (cellular) | 1 – 100 Mbps | Trung bình | Gần bờ |
| **Shore WiFi** | 10+ Mbps | Thấp | Tại cảng |

**⇒ Giải pháp:** Đặt một hệ thống tính toán hoàn chỉnh trên tàu (Edge) có khả năng hoạt động offline 100%, và đồng bộ dữ liệu với trung tâm trên bờ (Shore) khi có kết nối.

### 2.2 Phân Chia Trách Nhiệm

```
┌─────────────────────────────────┐    ┌─────────────────────────────────┐
│         SHORE (Trên Bờ)         │    │         EDGE (Trên Tàu)         │
│                                 │    │                                 │
│  ✦ Quản lý đội tàu tổng thể    │    │  ✦ Thu thập dữ liệu cảm biến   │
│  ✦ HR & quản lý thuyền viên    │    │  ✦ Giám sát máy móc real-time   │
│  ✦ Quy hoạch chuyến đi         │    │  ✦ Ghi nhật ký hành trình       │
│  ✦ Dữ liệu master (STCW, ref) │    │  ✦ Báo cáo Noon/Departure/...   │
│  ✦ Compliance & audit toàn cầu │    │  ✦ Quản lý bảo trì trên tàu    │
│  ✦ Phân tích dữ liệu nhiều tàu │    │  ✦ Quản lý kho vật tư           │
│  ✦ Tuyển dụng & onboarding     │    │  ✦ An toàn & diễn tập khẩn cấp │
│  ✦ Quản lý chứng chỉ tập trung │    │  ✦ Logbook (Deck, Engine, Oil)  │
│                                 │    │                                 │
│  📊 29 Controllers              │    │  📊 51 Controllers              │
│  🗄️ 110+ bảng database          │    │  🗄️ 60+ bảng database           │
│  ⚙️ 35+ Services                │    │  ⚙️ 35+ Services                │
└─────────────────────────────────┘    └─────────────────────────────────┘
```

### 2.3 Quyền Sở Hữu Dữ Liệu (Data Ownership)

Mỗi loại dữ liệu có một **nguồn gốc sở hữu** xác định — quyết định ai "thắng" khi xung đột:

| Dữ liệu | Chủ sở hữu | Ví dụ |
|---|---|---|
| Master Data (chứng chỉ, quốc gia, rank) | **Shore** | Dữ liệu tham chiếu do HR/Compliance quản lý |
| Crew HR (mã bảo hiểm, mã thuế) | **Shore** | Thông tin hành chính từ phòng nhân sự |
| Crew Operational (trạng thái trên tàu) | **Edge** | IsOnboard, EmbarkDate, DisembarkDate |
| Service Records | **Edge** | Lịch sử phục vụ thực tế trên tàu |
| Telemetry (GPS, engine, generator) | **Edge** | Dữ liệu cảm biến tàu |
| Voyage Operational Data | **Edge** | Noon reports, logbooks, engine logs |
| Vessel Technical Data | **Edge** | Thông số kỹ thuật tàu |
| Vessel Commercial Data | **Shore** | Chủ tàu, bảo hiểm, thuê tàu |
| Crew Certs (metadata) | **Shore** | Số chứng chỉ, ngày cấp/hết hạn |
| Crew Certs (file scan) | **Edge** | Bản scan, file upload trên tàu |

---

## 3. Shore System (Trên Bờ)

### 3.1 Backend API — `shore_product/backend/`

ASP.NET Core 8.0 Web API phục vụ quản lý đội tàu tập trung.

**Controllers (29):**

| Nhóm | Controllers | Chức năng |
|---|---|---|
| **Maritime Core** | Ships, Vessels, Reports, Ports, Voyages | Quản lý tàu, báo cáo, cảng, chuyến đi |
| **Crew HR** | Crew, Certificates, Countries, Ranks | STCW/MLC crew management |
| **Crew Workflow** | Assignment, Compliance, Onboarding, Travel, ExternalRequest | Quy trình phân công, tuân thủ, onboarding |
| **Sync** | SyncController, SyncDashboard | Nhận push từ Edge, phục vụ pull cho Edge |
| **PMS & Materials** | Maintenance, Equipment, Inventory, Material, StockReceipts | Hệ thống bảo trì & kho vật tư |

**Background Services:**

| Service | Chu kỳ | Chức năng |
|---|---|---|
| `AlertBackgroundService` | 15 phút | Xử lý cảnh báo tự động |
| `CertificateExpiryMonitorService` | Theo lịch | Giám sát hết hạn chứng chỉ |
| `SyncHealthMonitorService` | Liên tục | Giám sát sức khỏe hệ thống sync |

**Xác thực & Phân quyền:**
- JWT Bearer Token với role-based policies
- Roles: `Admin`, `HRAdmin`, `CrewCoordinator`, `ComplianceOfficer`, `FleetManager`, `PortCaptain`, `Master`, `ChiefOfficer`, `SystemAdmin`

### 3.2 Frontend Dashboard — `shore_product/frontend/`

React 19 + TypeScript + Vite — quản lý đội tàu từ trung tâm điều hành.

**35+ trang/routes** bao gồm:

| Module | Trang chính | Mô tả |
|---|---|---|
| **Fleet** | Vessels, Reports | Tổng quan đội tàu, báo cáo tổng hợp |
| **Crew** | Crew, Certificates, Assignments | Quản lý thuyền viên & phân công |
| **Voyage** | Voyages (list/create/detail) | Quản lý chuyến đi |
| **Compliance** | Rule Sets, Crew Evaluation | Đánh giá tuân thủ |
| **PMS** | Master Schedule, Assets, Work Planning | Bảo trì phòng ngừa |
| **Materials** | Inventory, Requests, Receipts | Quản lý kho vật tư |
| **Workflow** | Onboarding, Travel, Verification | Quy trình nhân sự |
| **Sync** | Sync Dashboard | Giám sát đồng bộ toàn đội tàu |

### 3.3 Database — PostgreSQL (Port 5434)

**110+ bảng** tổ chức theo domain:

- **Vessel & Navigation:** Vessel (40+ trường mở rộng), VesselPosition, VesselAlert, VesselCertificate
- **Voyage (15+):** VoyagePlanLeg, VoyageStatusHistory, VoyageCargo, VoyageExpense, VoyageSettlement...
- **Crew (20+):** CrewMember, CrewCertificate, ServiceRecord, TravelDocument, SeafarerDocument, HealthDocument...
- **Crew Workflow (15+):** OnboardingCase, ComplianceRuleSet, CrewAssignment, TravelRequest...
- **PMS & Materials (17):** EquipmentAsset, MaintenanceSchedule, MaterialItem, InventoryStock...
- **Edge Sync Mirror (10+):** PositionData, AisData, EngineData, NoonReport, SafetyAlarm...
- **Sync Infrastructure:** SyncOutbox, SyncLog, SyncNodeTracker, SyncIdempotencyRecord, SyncTableStats

---

## 4. Edge System (Trên Tàu)

### 4.1 Backend API — `edge_product/edge-services/`

ASP.NET Core 8.0 Web API chạy trên server đặt tại tàu.

**Controllers (51):**

| Nhóm | Controllers | Chức năng |
|---|---|---|
| **Infrastructure** | Auth, Dashboard, AuditLog, Health | Xác thực, dashboard, audit |
| **Voyage (6)** | Voyage, VoyageLog, VoyageCockpit, VoyageFinancial, VoyageEfficiency, Port | Quản lý chuyến đi đầy đủ |
| **Crew (4)** | Crew, Certificates, Countries, CountryCertificates | Quản lý thuyền viên trên tàu |
| **Logbook (7)** | DeckLogbook, EngineLogbook, BallastWater, GarbageRecord, OilRecordBook, Watchkeeping, AbstractLog | Nhật ký tuân thủ SOLAS/MARPOL |
| **Reporting (3)** | Reporting, WeeklyReport, MonthlyReport | Noon, Departure, Arrival, Bunker, Position reports |
| **Maintenance (6)** | Maintenance, EquipmentAsset, EquipmentGroup, TaskChecklist, TaskRealTimeUpdates, DeferralRequest | PMS ISM Code |
| **Inventory (3)** | Inventory, Material, FuelAnalytics | Kho vật tư & phân tích nhiên liệu |
| **Safety (3)** | Alarms, Compliance, Drill | An toàn & diễn tập khẩn cấp |
| **Telemetry (1)** | Telemetry | Vị trí, navigation, engine real-time |

**Background Services (4):**

| Service | Chu kỳ | Chức năng |
|---|---|---|
| `SyncBackgroundWorker` | Push: 30s, Pull: 30s | Đồng bộ tự động với Shore |
| `TelemetrySimulatorService` | 60s | Giả lập dữ liệu cảm biến (dev) |
| `SignalKDataCollectorService` | 60s | Thu thập dữ liệu SignalK thực (production) |
| `DataCleanupService` | Hàng ngày | Xóa dữ liệu telemetry cũ (retention policy) |

### 4.2 Frontend Dashboard — `edge_product/frontend-edge/`

React 19 + TypeScript + Vite + Zustand — dashboard cho thuyền trưởng & sĩ quan trên tàu.

**55+ trang/routes** bao gồm:

| Module | Trang chính | Mô tả |
|---|---|---|
| **Bridge** | Dashboard, Navigation, Engine | Giám sát real-time |
| **Crew** | Crew, Detail, Certificates | Quản lý thuyền viên |
| **PMS (6 module)** | Master Schedule, Approval, Work Planning | Bảo trì phòng ngừa |
| **Logistics (5)** | Materials, Requests, Receipts, Inventory | Quản lý kho |
| **Reports (5 loại)** | Noon, Departure, Arrival, Bunker, Position | Form báo cáo IMO |
| **Logbooks (9)** | Deck, Engine, Oil Record, Garbage, Ballast, Watchkeeping, Voyage, Abstract | Sổ ghi chép chính thức |
| **Safety** | Drill Timeline, Alarms | An toàn & diễn tập |
| **System** | Audit Log, Sync Status, Ship Data | Quản lý hệ thống |

**Tính năng đặc biệt:** Export Excel/PDF, Drag & Drop task management, Recharts visualization, Zustand persistent state.

### 4.3 Mobile App — `edge_product/frontend-mobile/`

Flutter 3.x — ứng dụng di động cho thuyền viên trên tàu.

| Feature | Mô tả |
|---|---|
| **Alarms** | Xem, lọc, theo dõi cảnh báo an toàn |
| **Tasks** | Quản lý công việc được phân công |
| **Schedule** | Lịch trực & ca làm việc |
| **Offline** | Hive local caching + sync queue |
| **Multi-language** | Tiếng Việt & Tiếng Anh |

### 4.4 Database — PostgreSQL (Port 5433)

**60+ bảng** tổ chức theo domain:

- **Navigation:** PositionData, NavigationData, AisData, NmeaRawData
- **Engine:** EngineData, GeneratorData, TankLevel, FuelConsumption
- **Voyage (15+):** VoyageRecord, VoyagePlanLeg, CargoOperation, VoyageCost, VoyageExpense...
- **Crew:** CrewMember, CrewCertificate, ServiceRecord (tất cả ISyncableEntity)
- **Logbooks:** DeckLogBook, EngineLogBook, GarbageRecord, BallastWaterRecord, OilRecordBook, WatchkeepingLog
- **Reporting:** NoonReport, DepartureReport, ArrivalReport, BunkerReport, PositionReport
- **Maintenance:** MaintenanceTask, EquipmentAsset, MaintenanceSchedule, MaintenanceHistory
- **Inventory:** MaterialItem, MaterialReceipt, InventoryStock
- **Safety:** SafetyAlarm, DrillLog
- **Sync:** SyncQueue (outgoing store-and-forward queue)
- **Auth:** User, Role, UserSession, SystemLog

---

## 5. Cơ Chế Đồng Bộ Dữ Liệu (Sync Protocol)

### 5.1 Tổng Quan Giao Thức

```
                    EDGE (Tàu)                              SHORE (Bờ)
                    ═══════════                              ═══════════

  ┌─────────┐     ┌──────────────┐                    ┌──────────────┐
  │ App     │────►│ SyncQueue    │                    │ SyncOutbox   │
  │ SaveChanges   │ (store-and-  │    ──── PUSH ───►  │ (incoming    │
  │         │     │  forward)    │   POST /api/sync   │  processing) │
  └─────────┘     └──────┬───────┘                    └──────┬───────┘
                         │                                    │
              SyncBackgroundWorker                   SyncInboxService
              (push every 30s)                       + ConflictResolver
              (pull every 30s)                       + Idempotency Check
                         │                                    │
                  ┌──────┴───────┐                    ┌──────┴───────┐
                  │ Apply Shore  │   ◄── PULL ────    │ SyncOutbox   │
                  │ changes      │  GET /api/sync/pull│ (outgoing    │
                  │ + Conflict   │                    │  to Edge)    │
                  │   Handler    │                    └──────────────┘
                  └──────┬───────┘
                         │
                  POST /api/sync/acknowledge ────►  Mark Delivered
```

### 5.2 Push: Edge → Shore

**Khi ứng dụng lưu dữ liệu:**
1. EF Core change tracker phát hiện thay đổi trên entity `ISyncableEntity`
2. `SyncQueue` entry được tạo tự động (trong cùng transaction)
3. Priority được gán theo loại entity (Critical / Operational / Low)

**SyncBackgroundWorker chạy mỗi 30 giây:**
1. Kiểm tra loại kết nối mạng hiện tại
2. Lọc queue theo priority phù hợp với bandwidth
3. Gửi batch (tối đa 100 items) qua `POST /api/sync`
4. Đánh dấu `SyncedAt` cho items thành công
5. Exponential backoff cho items thất bại (tối đa 5 lần retry)

**Lọc theo mức ưu tiên & kết nối:**

```
┌──────────────────┬───────────────────────────────────────────┐
│   Kết nối        │  Priority được phép đồng bộ               │
├──────────────────┼───────────────────────────────────────────┤
│   Iridium        │  🔴 Critical ONLY (distress, safety)     │
│   VSAT           │  🔴 Critical + 🟡 Operational            │
│   4G / WiFi      │  🔴 Critical + 🟡 Operational + 🟢 Low  │
│   Không có mạng  │  ❌ Không đồng bộ (queue lưu trữ)       │
└──────────────────┴───────────────────────────────────────────┘
```

**3 mức ưu tiên:**

| Priority | Loại dữ liệu | Ví dụ |
|---|---|---|
| 🔴 **Critical** | An toàn, báo động khẩn | Safety alarms, distress signals |
| 🟡 **Operational** | Vận hành, báo cáo | Noon reports, crew, positions |
| 🟢 **Low** | Hành chính, kho | Inventory, logbook entries, documents |

### 5.3 Pull: Shore → Edge

**Shore enqueue dữ liệu cho Edge:**
1. Khi Shore business logic thay đổi crew/cert/master data → gọi `SyncOutboxService.EnqueueAsync()`
2. `TargetNode` = mã tàu cụ thể hoặc `"*"` (broadcast toàn bộ)
3. **Deduplication:** Nếu item chưa delivered cho cùng (node, table, key) đã tồn tại → cập nhật payload thay vì tạo mới

**Edge pull về mỗi 30 giây:**
1. `GET /api/sync/pull?nodeId={shipIMO}&since={lastPull}&cursor={lastId}&pageSize=50`
2. Cursor-based pagination → lấy từng trang
3. Mỗi item được xử lý qua `SyncConflictHandler`
4. Gửi acknowledgment → Shore đánh dấu `DeliveredAt`

### 5.4 Heartbeat

```
POST /api/sync/heartbeat
Body: { nodeId, shipName, imoNumber, networkType }
```

Edge gửi heartbeat định kỳ để Shore biết tàu đang online, loại kết nối hiện tại, và tình trạng đồng bộ.

### 5.5 Idempotency

Shore sử dụng `SyncIdempotencyRecord` với key: `{TableName}:{RecordKey}:{SyncVersion}` — đảm bảo cùng một sync item được xử lý nhiều lần mà không tạo trùng lặp.

---

## 6. Xử Lý Xung Đột (Conflict Resolution)

Hệ thống sử dụng **Domain-Based Conflict Resolution** thay vì Last-Write-Wins — mỗi bảng và mỗi trường có quy tắc xung đột riêng, phản ánh đúng thực tế nghiệp vụ hàng hải.

### 6.1 Quy Tắc Tại Shore (Nhận Push Từ Edge)

**`ConflictResolverService` trên Shore:**

| Quy tắc | Bảng | Hành vi |
|---|---|---|
| **Master Data → Shore thắng** | certificate, country, rank, rank_certificate, country_certificate | Edge không thể ghi đè master data |
| **Operational → Edge thắng** | service_record, position_data, engine_data, noon_report, voyage_* | Dữ liệu vận hành từ tàu luôn được tin cậy |
| **Crew → Merge theo trường** | crew_member | Mỗi trường có chủ sở hữu riêng (xem bảng dưới) |
| **Cert → Chia theo trường** | crew_certificate | Metadata = Shore, File scan = Edge |
| **Fallback** | Các bảng khác | Last-Write-Wins dựa trên UpdatedAt |

### 6.2 Merge Crew Member (Trường Hợp Phức Tạp Nhất)

```
┌─────────────────────────────────────────────────────────────┐
│                  CREW MEMBER MERGE RULES                    │
├─────────────────────────────┬───────────────────────────────┤
│    Shore Sở Hữu             │    Edge Sở Hữu               │
├─────────────────────────────┼───────────────────────────────┤
│ • SocialInsuranceNumber     │ • IsOnboard                   │
│ • TaxIdNumber               │ • EmbarkDate / DisembarkDate  │
│                             │ • EmbarkPort / DisembarkPort  │
│                             │ • ShipId / CurrentShipName    │
│                             │ • AvatarUrl                   │
├─────────────────────────────┴───────────────────────────────┤
│    Trường khác (FullName, DOB, Nationality...):             │
│    • Nếu origin = Shore → áp dụng                          │
│    • Nếu origin = Edge  → chỉ áp dụng nếu timestamp mới   │
└─────────────────────────────────────────────────────────────┘
```

### 6.3 Quy Tắc Tại Edge (Nhận Pull Từ Shore)

**`SyncConflictHandler` trên Edge (logic đảo ngược):**

- **Master Data từ Shore:** Luôn chấp nhận
- **Service Record:** Từ chối (Edge sở hữu)
- **Crew mới từ Shore (PendingReview):** Đặt `IsOnboard = false` → hiện trong danh sách chờ duyệt cho thuyền trưởng
- **Crew update từ Shore:** Chấp nhận trường HR, giữ nguyên trường operational

### 6.4 Ship Data — Hybrid Master

Dữ liệu tàu (Vessel) sử dụng **chiến lược chủ sở hữu hybrid:**

```
┌─────────────────────────────┬───────────────────────────────┐
│  Edge Sở Hữu (Kỹ Thuật)    │  Shore Sở Hữu (Thương Mại)   │
├─────────────────────────────┼───────────────────────────────┤
│ • VesselType                │ • ShipownerName               │
│ • MainEnginePowerKW         │ • ShipownerCountry            │
│ • Dimensions                │ • Charterer                   │
│ • FlagState                 │ • InsuranceCompany            │
│ • ReferenceDepth            │ • (chỉ nhận lần sync đầu)    │
└─────────────────────────────┴───────────────────────────────┘
```

---

## 7. Shared Library

**`Maritime.Shared`** — thư viện dùng chung giữa Edge và Shore:

### 7.1 Interfaces

```csharp
ISyncableEntity
├── IsSynced: bool            // Đã được đồng bộ chưa
├── OriginNode: string        // "SHIP_01" hoặc "SHORE"
├── SyncVersion: long         // Optimistic concurrency control
├── CreatedAt: DateTime
└── UpdatedAt: DateTime
```

### 7.2 Sync Models

| Model | Vị trí | Mô tả |
|---|---|---|
| `SyncQueue` | Edge | Hàng đợi outgoing (store-and-forward) |
| `SyncOutbox` | Shore | Hàng đợi dữ liệu gửi cho Edge |
| `SyncLog` | Cả hai | Audit trail mọi thao tác sync |

### 7.3 Sync Enums

| Enum | Giá trị | Mục đích |
|---|---|---|
| `SyncActionType` | CREATE, UPDATE, DELETE, SNAPSHOT | Loại thao tác |
| `SyncPriority` | Critical, Operational, Low | Mức ưu tiên |
| `NetworkType` | None, Satellite_Iridium, Satellite_VSAT, Cellular_4G, Shore_WiFi | Loại kết nối |

### 7.4 Shared Domain Models

Crew, Certificate, Country, Rank, ServiceRecord, TravelDocument, SeafarerDocument, EmploymentDocument, HealthDocument — tất cả implement `ISyncableEntity`.

### 7.5 Shared DTOs

`SyncQueueItemDto`, `SyncPullResponse`, `SyncStatusDto`, `SyncAcknowledgeDto`, `SyncHeartbeatDto`, `CrewMemberDto`, `CertificateDto`, `ServiceRecordDto`...

---

## 8. Cấu Trúc Thư Mục

```
📦 Martime_product_v1.1/
│
├── 🏢 shore_product/                  → SHORE SYSTEM
│   ├── backend/                       → ASP.NET Core 8 API (Port 5000)
│   │   ├── Controllers/ (29)          → API endpoints
│   │   ├── Services/ (35+)            → Business logic
│   │   │   └── Sync/                  → SyncInbox, SyncOutbox, ConflictResolver
│   │   ├── Models/ (110+)             → Domain entities
│   │   ├── Data/                      → EF Core DbContext
│   │   └── Middleware/                → Auth, Error handling
│   ├── frontend/                      → React 19 Dashboard (Port 3000)
│   │   └── src/pages/ (35+)          → Shore management UI
│   ├── shared/                        → Maritime.Shared library
│   ├── docker-compose.yml             → PostgreSQL 5434, pgAdmin 8081
│   └── shore.sln
│
├── 🚢 edge_product/                   → EDGE SYSTEM
│   ├── edge-services/                 → ASP.NET Core 8 API (Port 5001)
│   │   ├── Controllers/ (51)          → API endpoints
│   │   ├── Services/ (35+)            → Business logic
│   │   │   └── Core/                  → SyncService, SyncBackgroundWorker
│   │   ├── Models/ (97+)             → Domain entities
│   │   ├── Data/                      → EF Core DbContext
│   │   └── BackgroundServices/        → Telemetry, SignalK, Cleanup
│   ├── frontend-edge/                 → React 19 Dashboard (Port 3002)
│   │   └── src/pages/ (55+)          → Ship operations UI
│   ├── frontend-mobile/               → Flutter Mobile App
│   │   └── lib/                       → Clean Architecture (data/core/presentation)
│   ├── shared/                        → Maritime.Shared library
│   ├── docker-compose.yml             → PostgreSQL 5433, pgAdmin 5050
│   └── edge.sln
│
├── 📚 docs/                           → Tài liệu kỹ thuật
├── 🔧 scripts/                        → Backup/export/restore scripts
├── ⚙️ configs/                         → Cấu hình chung
└── 📋 product.sln                     → Root solution
```

---

## 9. Công Nghệ Sử Dụng

### 9.1 Backend

| Thành phần | Công nghệ | Phiên bản |
|---|---|---|
| Framework | ASP.NET Core Web API | 8.0 |
| ORM | Entity Framework Core + Npgsql | 8.0 |
| Database | PostgreSQL Alpine | 15 |
| Cache | Redis Alpine (Shore) | 7 |
| Auth | JWT Bearer Token | - |
| API Docs | Swagger / OpenAPI 3.0 | - |
| Background Tasks | IHostedService | - |

### 9.2 Frontend Web

| Thành phần | Shore | Edge |
|---|---|---|
| Framework | React 19 + TypeScript 5.9 | React 19 + TypeScript 5.9 |
| Bundler | Vite 7 (rolldown) | Vite 6 |
| Styling | Tailwind CSS 3.4 | Tailwind CSS 3.4 |
| State | React Context API | Zustand 5.0 (persistent) |
| UI | Radix UI + Lucide | Radix UI + Lucide + dnd-kit |
| HTTP | Axios | Axios |
| Charts | — | Recharts 2.14 |
| Export | XLSX | XLSX + jsPDF + ExcelJS |

### 9.3 Mobile

| Thành phần | Công nghệ |
|---|---|
| Framework | Flutter 3.x (Dart) |
| State | Provider 6.1 |
| HTTP | Dio 5.4 + Retrofit 4.4 |
| Local DB | Hive 2.2 |
| DI | GetIt |
| Auth | Flutter Secure Storage |
| UI | Material Design + Google Fonts |

### 9.4 Infrastructure

| Thành phần | Công nghệ |
|---|---|
| Container | Docker + Docker Compose v2 |
| Database | PostgreSQL 15 Alpine (health checks) |
| Admin | pgAdmin 4 |
| Network | Docker bridge network |
| Volumes | Named volumes (data persistence) |

---

## 10. Hướng Dẫn Cài Đặt & Chạy

### 10.1 Yêu Cầu Hệ Thống

```
✅ .NET 8 SDK
✅ Node.js 20+
✅ Docker Desktop
✅ PostgreSQL 15+ (hoặc Docker)
✅ Flutter SDK 3.x (cho mobile)
✅ RAM: 8GB+ khuyến nghị
✅ Disk: 5GB trống
```

### 10.2 Khởi Động Edge System (Trên Tàu)

```powershell
cd edge_product

# 1. Khởi động Database
cd edge-services
docker compose up -d edge-postgres edge-pgadmin

# 2. Chạy Backend (Terminal 1)
dotnet restore
dotnet run --urls "http://localhost:5001"

# 3. Chạy Frontend (Terminal 2)
cd ../frontend-edge
npm install && npm run dev
```

### 10.3 Khởi Động Shore System (Trên Bờ)

```powershell
cd shore_product

# Cách 1: Docker all-in-one
docker compose up -d

# Cách 2: Manual
# Terminal 1 - Database
docker compose up -d postgres

# Terminal 2 - Backend
cd backend
dotnet restore && dotnet run --urls "http://localhost:5000"

# Terminal 3 - Frontend
cd frontend
npm install && npm run dev
```

### 10.4 Truy Cập

| Dịch vụ | URL |
|---|---|
| Shore Dashboard | http://localhost:3000 |
| Shore API / Swagger | http://localhost:5000/swagger |
| Edge Dashboard | http://localhost:3002 |
| Edge API / Swagger | http://localhost:5001/swagger |
| pgAdmin (Shore) | http://localhost:8081 |
| pgAdmin (Edge) | http://localhost:5050 |

### 10.5 Cấu Hình Sync

**Edge** (`edge_product/edge-services/appsettings.json`):
```json
{
  "Sync": {
    "BatchSize": 100,
    "SyncInterval": 30,
    "HighPriorityInterval": 30,
    "RetryAttempts": 5,
    "RetryDelaySeconds": 60,
    "NetworkType": "Shore_WiFi"
  },
  "ShoreApi": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

---

## 11. Tiêu Chuẩn Hàng Hải

Hệ thống tuân thủ các tiêu chuẩn quốc tế:

| Tiêu chuẩn | Phạm vi | Áp dụng trong |
|---|---|---|
| **IMO DCS** | Data Collection System | FuelAnalytics, NoonReport |
| **IMO MRV** | Monitoring, Reporting, Verification | Emission tracking |
| **IMO CII** | Carbon Intensity Indicator | Vessel efficiency scoring |
| **SOLAS** | Safety of Life at Sea | Logbooks, Watchkeeping, Drills |
| **MARPOL** | Marine Pollution Prevention | Oil Record Book, Garbage Record, Ballast Water |
| **ISM Code** | International Safety Management | Maintenance PMS, Audit trails |
| **ISPS** | Ship and Port Facility Security | Compliance tracking |
| **STCW** | Standards of Training, Certification and Watchkeeping | Crew certificates |
| **MLC** | Maritime Labour Convention | Crew management, Service records |

---

## Thống Kê Dự Án

| Metric | Shore | Edge | Mobile |
|---|---|---|---|
| Controllers | 29 | 51 | — |
| Database Tables | 110+ | 60+ | — |
| Services | 35+ | 35+ | — |
| Frontend Pages | 35+ | 55+ | 10+ |
| Background Services | 3 | 4 | — |
| Sync Entities | — | Toàn bộ ISyncableEntity | — |
| Compliance Standards | 9 | 9 | — |

