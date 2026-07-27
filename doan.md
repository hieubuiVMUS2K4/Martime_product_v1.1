# PHÂN TÍCH MODULE QUẢN LÝ THÔNG TIN VÀ HỒ SƠ THUYỀN VIÊN
## Đồ án tốt nghiệp — Hệ thống Quản lý Tàu biển (Maritime Management System)

---

## MỤC LỤC

1. [Giới thiệu đề tài](#1-giới-thiệu-đề-tài)
2. [Phân tích hệ thống](#2-phân-tích-hệ-thống)
3. [Kiến trúc hệ thống tổng thể](#3-kiến-trúc-hệ-thống-tổng-thể)
4. [Mô hình dữ liệu module thuyền viên](#4-mô-hình-dữ-liệu-module-thuyền-viên)
5. [Thiết kế API (Backend)](#5-thiết-kế-api-backend)
6. [Giao diện người dùng (Frontend)](#6-giao-diện-người-dùng-frontend)
7. [Cơ chế đồng bộ tàu–bờ](#7-cơ-chế-đồng-bộ-tàubờ)
8. [Xử lý xung đột dữ liệu (Conflict Resolution)](#8-xử-lý-xung-đột-dữ-liệu-conflict-resolution)
9. [Workflow phê duyệt thuyền viên (Onboard Review)](#9-workflow-phê-duyệt-thuyền-viên-onboard-review)
10. [Kiểm soát lỗi và xác thực dữ liệu](#10-kiểm-soát-lỗi-và-xác-thực-dữ-liệu)
11. [Các vấn đề kỹ thuật đã giải quyết](#11-các-vấn-đề-kỹ-thuật-đã-giải-quyết)
12. [So sánh giải pháp và lý do lựa chọn](#12-so-sánh-giải-pháp-và-lý-do-lựa-chọn)
13. [Kết quả đạt được](#13-kết-quả-đạt-được)
14. [Hướng phát triển](#14-hướng-phát-triển)
15. [Câu hỏi thường gặp (Q&A Bảo vệ)](#15-câu-hỏi-thường-gặp-qa-bảo-vệ)

---

## 1. GIỚI THIỆU ĐỀ TÀI

### 1.1 Bối cảnh

Ngành vận tải biển đòi hỏi quản lý chặt chẽ thông tin thuyền viên theo các tiêu chuẩn quốc tế:
- **STCW (Standards of Training, Certification and Watchkeeping)** — Tiêu chuẩn đào tạo, cấp bằng và trực ca cho thuyền viên.
- **MLC 2006 (Maritime Labour Convention)** — Công ước Lao động Hàng hải 2006.
- **SOLAS Chapter V** — An toàn tàu biển.

**Bài toán thực tế**: Tàu hoạt động trên biển có kết nối internet không ổn định (vệ tinh Iridium, VSAT, 4G khi gần bờ), nhưng cán bộ điều hành trên bờ và thuyền trưởng trên tàu đều cần truy cập và cập nhật hồ sơ thuyền viên theo thời gian thực.

### 1.2 Phạm vi module

Module phụ trách bao gồm:
1. **Quản lý thông tin thuyền viên** — CRUD hồ sơ, chứng chỉ, tài liệu pháp lý.
2. **Đồng bộ dữ liệu tàu–bờ** — Cơ chế sync hai chiều giữa hệ thống bờ (Shore) và hệ thống tàu (Edge).
3. **Workflow phê duyệt** — Thuyền trưởng xác nhận thuyền viên mới lên tàu.

---

## 2. PHÂN TÍCH HỆ THỐNG

### 2.1 Các bên tham gia (Stakeholders)

| Bên tham gia | Vai trò | Hệ thống sử dụng |
|---|---|---|
| HR/Điều hành bờ | Quản lý hồ sơ, phân công tàu | Shore Frontend (Web) |
| Thuyền trưởng | Xác nhận thuyền viên, chỉnh sửa thông tin tác nghiệp | Edge Frontend (Web trên tàu) |
| Thuyền viên | Xem hồ sơ cá nhân | Mobile App (Edge) |
| Hệ thống | Đồng bộ dữ liệu tự động | Backend Services |

### 2.2 Yêu cầu chức năng

**FR-01: Quản lý hồ sơ thuyền viên (Shore)**
- Thêm/sửa/xóa hồ sơ thuyền viên với đầy đủ dữ liệu Bio-Data (STCW/MLC compliant)
- Phân trang, tìm kiếm, lọc theo tên, chức danh, bộ phận, tàu
- Thống kê tổng quan: tổng số, đang trên tàu, trong pool, chờ xét duyệt

**FR-02: Quản lý chứng chỉ**
- Thêm/sửa chứng chỉ với ngày phát hành, ngày hết hạn
- Giám sát tự động trạng thái: `VALID` → `EXPIRING_SOON` (≤90 ngày) → `EXPIRED`
- Broadcast cảnh báo hết hạn đến tàu

**FR-03: Quản lý tài liệu**
- Upload file: hộ chiếu, sổ thuyền viên, hợp đồng, tài liệu y tế
- Bảo vệ file (protected endpoints), xem ảnh trực tiếp trong UI

**FR-04: Phân công tàu (Ship Assignment)**
- Gán thuyền viên vào tàu cụ thể
- Gán hàng loạt (batch assign/unassign)
- Kích hoạt workflow phê duyệt tự động

**FR-05: Đồng bộ tàu–bờ**
- Shore → Edge: đẩy dữ liệu hồ sơ, chứng chỉ, dữ liệu chuẩn (ranks, countries, certificates)
- Edge → Shore: cập nhật thông tin tác nghiệp (ngày lên/xuống tàu, ảnh đại diện)
- Hoạt động với mạng không ổn định, xử lý khi offline

**FR-06: Workflow phê duyệt thuyền viên**
- Thuyền trưởng xét duyệt 7 mục của hồ sơ thuyền viên mới
- Chấp nhận (Approve) / Tạm giữ chờ bổ sung (On Hold)
- Shore nhận thông báo khi tàu sửa đổi hồ sơ

### 2.3 Yêu cầu phi chức năng

| Yêu cầu | Mô tả |
|---|---|
| Tính sẵn sàng | Hệ thống tàu hoạt động độc lập khi mất kết nối |
| Tính nhất quán cuối cùng | Dữ liệu được đồng bộ khi kết nối khôi phục (Eventual Consistency) |
| Idempotency | Xử lý trùng lặp — cùng một gói dữ liệu gửi nhiều lần chỉ được áp dụng một lần |
| Bảo mật | JWT Authentication, HMAC request signing giữa tàu–bờ |
| Hiệu suất | Phân trang server-side, lazy loading, token bucket rate limiting |

---

## 3. KIẾN TRÚC HỆ THỐNG TỔNG THỂ

### 3.1 Mô hình triển khai

```
┌─────────────────────────────────────────────────────────┐
│                    SHORE (Văn phòng bờ)                  │
│  ┌──────────────┐    ┌──────────────┐    ┌────────────┐  │
│  │  Shore       │    │  Shore       │    │ PostgreSQL │  │
│  │  Frontend    │◄──►│  Backend API │◄──►│  Database  │  │
│  │  (React/TS)  │    │  (.NET 8)    │    │            │  │
│  └──────────────┘    └──────┬───────┘    └────────────┘  │
│                             │ REST/HTTPS                 │
└─────────────────────────────┼───────────────────────────┘
                              │ (Vệ tinh / 4G / VSAT)
┌─────────────────────────────┼───────────────────────────┐
│             EDGE (Hệ thống tàu)            │
│                             │                            │
│  ┌──────────────┐    ┌──────┴───────┐    ┌────────────┐  │
│  │  Edge        │    │  Edge        │    │ PostgreSQL │  │
│  │  Frontend    │◄──►│  Services    │◄──►│  Database  │  │
│  │  (React/TS)  │    │  (.NET 8)    │    │  (Local)   │  │
│  └──────────────┘    └──────────────┘    └────────────┘  │
│                                                          │
│  ┌──────────────┐                                        │
│  │  Mobile App  │  (Thuyền viên xem hồ sơ cá nhân)      │
│  └──────────────┘                                        │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Stack công nghệ

| Tầng | Công nghệ |
|---|---|
| Backend | ASP.NET Core 8, Entity Framework Core, PostgreSQL |
| Frontend Shore | React 18, TypeScript, Lucide Icons, React Query (Custom Hooks) |
| Frontend Edge | React 18, TypeScript, i18n (đa ngôn ngữ) |
| Mobile | React Native |
| Containerization | Docker, Docker Compose |
| Authentication | JWT Bearer Token |
| Sync Security | HMAC-SHA256 Request Signing |

### 3.3 Cấu trúc Solution

```
Martime_product_v1.1/
├── shore_product/          ← Hệ thống bờ
│   ├── backend/            ← ASP.NET Core API
│   │   ├── Controllers/Crew/
│   │   ├── Services/Crew/
│   │   ├── Services/CrewManagement/
│   │   └── Services/Sync/
│   ├── shared/             ← Models & DTOs dùng chung
│   │   ├── Models/Crew/
│   │   └── DTOs/Crew/
│   └── frontend/           ← React SPA
│       └── src/pages/CrewManagement/
│
└── edge_product/           ← Hệ thống tàu
    ├── edge-services/      ← ASP.NET Core API (chạy trên tàu)
    │   ├── Controllers/Crew/
    │   └── Services/Core/   ← SyncService, SyncConflictHandler
    ├── shared/             ← Models & DTOs dùng chung
    └── frontend-edge/      ← React SPA (chạy trên tàu)
        └── src/pages/Crew/
```

**Nhận xét kiến trúc**: Dự án sử dụng `shared` library cho Models và DTOs, giúp **đảm bảo schema đồng nhất** giữa Shore và Edge, tránh lỗi mất đồng bộ cấu trúc dữ liệu.

---

## 4. MÔ HÌNH DỮ LIỆU MODULE THUYỀN VIÊN

### 4.1 Sơ đồ thực thể (ERD tóm tắt)

```
ranks ─────────────────────────┐
                               │ RankId (FK)
countries ────────────────────►┤
                               │ CountryId (FK)
                               ▼
                         crew_members (core entity)
                         ├── id (UUID, PK)
                         ├── crew_id (mã nghiệp vụ)
                         ├── full_name
                         ├── [bio-data fields: ~30 trường]
                         ├── is_onboard (bool)
                         ├── onboard_status (PendingReview/Approved/OnHold)
                         ├── vessel_id (FK → vessels)
                         ├── [sync fields: origin_node, sync_version, ...]
                         └── [review fields: review_checklist, edge_changes, ...]
                               │
          ┌────────────────────┼──────────────────────────┐
          ▼                    ▼                           ▼
crew_certificates       travel_documents           service_records
(chứng chỉ cá nhân)    (tài liệu hộ chiếu,        (hồ sơ hành trình)
                         sổ thuyền viên, ...)
          │
          ▼
certificates (loại chứng chỉ — master data)
     ├── rank_certificates (N-N: rank ↔ certificate)
     └── country_certificates (N-N: country ↔ certificate)
```

### 4.2 Entity CrewMember — Trường quan trọng

**Nhóm 1: Thông tin cơ bản (Bio-Data)**
```csharp
public string FullName { get; set; }
public string CrewId { get; set; }       // Mã nghiệp vụ duy nhất
public int? RankId { get; set; }         // FK → Ranks
public int? CountryId { get; set; }      // FK → Countries (thay thế Nationality string)
public DateTime? DateOfBirth { get; set; }
public string? IdCardNumber { get; set; } // CMND/CCCD
public string? BloodGroup { get; set; }
public string? MaritalStatus { get; set; }
// ... + NextOfKin, Education, Physical Details ...
```

**Nhóm 2: Trạng thái lên tàu**
```csharp
public bool IsOnboard { get; set; }          // Đang trên tàu?
public Guid? VesselId { get; set; }          // Tàu đang phục vụ
public string? OnboardStatus { get; set; }  // PendingReview | Approved | OnHold
public DateTime? EmbarkDate { get; set; }   // Ngày lên tàu
public DateTime? DisembarkDate { get; set; } // Ngày xuống tàu
```

**Nhóm 3: Review Workflow (tính năng đặc thù)**
```csharp
public string? ReviewChecklist { get; set; }  // JSON: 7 mục xét duyệt
public string? ReviewNotes { get; set; }       // Ghi chú của thuyền trưởng
public string? EdgeChanges { get; set; }       // JSON: lịch sử thay đổi từ tàu
public bool EdgeChangesViewed { get; set; }    // Shore đã xem chưa?
```

**Nhóm 4: ISyncableEntity Interface (đồng bộ)**
```csharp
public bool IsSynced { get; set; }
public string OriginNode { get; set; }    // "SHORE" hoặc "EDGE-{IMO}"
public long SyncVersion { get; set; }     // Timestamp-based version
public DateTime UpdatedAt { get; set; }  // Last-write timestamp
```

### 4.3 Vì sao dùng UUID làm PK thay int?

Khi cùng lúc Shore và Edge tạo record mới (khi offline), cần tránh xung đột khóa chính. UUID được sinh ngay tại client (Guid.NewGuid()), đảm bảo không trùng lặp giữa 2 node mà không cần giao tiếp mạng.

### 4.4 Bảng dữ liệu chuẩn (Master Data — Shore Authoritative)

| Bảng | Mô tả | Số lượng ước tính |
|---|---|---|
| `ranks` | Danh mục chức danh (Master, Chief Mate, ...) | ~20 bản ghi |
| `countries` | Danh sách quốc gia (ISO 3166) | ~250 bản ghi |
| `certificates` | Loại chứng chỉ (STCW, GMDSS, ...) | ~100 bản ghi |
| `rank_certificates` | Chứng chỉ bắt buộc theo chức danh | ~200 bản ghi |
| `country_certificates` | Chứng chỉ bắt buộc theo quốc tịch | ~150 bản ghi |

---

## 5. THIẾT KẾ API (BACKEND)

### 5.1 Shore Backend — Crew Endpoints

**Base URL**: `POST /api/crew`  
**Auth**: Bearer JWT (`[Authorize(Policy = "InternalAccess")]`)

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/crew` | Danh sách có phân trang, tìm kiếm, lọc |
| `GET` | `/api/crew/stats` | Thống kê: total, onboard, pool, pendingReview |
| `GET` | `/api/crew/{id}` | Chi tiết thuyền viên |
| `GET` | `/api/crew/{id}/detail` | Chi tiết đầy đủ (gồm certs, docs, service records) |
| `POST` | `/api/crew` | Tạo mới thuyền viên |
| `PUT` | `/api/crew/{id}` | Cập nhật thông tin |
| `DELETE` | `/api/crew/{id}` | Xóa thuyền viên |
| `POST` | `/api/crew/{id}/assign-vessel` | Gán tàu (kích hoạt PendingReview) |
| `POST` | `/api/crew/{id}/unassign-vessel` | Gỡ khỏi tàu |
| `POST` | `/api/crew/{id}/mark-changes-viewed` | Đánh dấu shore đã xem thay đổi từ tàu |
| `GET` | `/api/crew/hold-notifications` | Danh sách thuyền viên bị giữ (OnHold) trong 30 ngày |

**Certificate Endpoints** (`/api/certificates`):
- Quản lý danh mục chứng chỉ (CRUD)
- `/api/crew/{crewId}/certificates` — Danh sách chứng chỉ của một thuyền viên
- Giám sát hết hạn theo fleet

### 5.2 Tầng Service — Phân tách trách nhiệm

```
Controllers/Crew/CrewController.cs
    └── Services/Crew/CrewService.cs          ← CRUD cơ bản, mapping DTO
        └── Services/Sync/SyncOutboxService.cs ← Đưa thay đổi vào outbox để sync

Services/CrewManagement/
    ├── OnboardingService.cs    ← Workflow lên tàu, checklist tự động
    ├── AssignmentService.cs    ← Phân công tàu, Manning Standards
    ├── ComplianceService.cs    ← Kiểm tra tuân thủ STCW/MLC
    ├── AuditService.cs         ← Lịch sử thay đổi
    ├── CrewStatusService.cs    ← Quản lý trạng thái vòng đời
    └── DocumentWorkflowService.cs ← Xử lý luồng duyệt tài liệu

Services/Sync/
    ├── SyncOutboxService.cs       ← Shore → Edge outbox
    ├── SyncInboxService.cs        ← Edge → Shore inbox (xử lý incoming)
    ├── CrewSyncOrchestrator.cs    ← Điều phối snapshot & delta sync
    ├── ConflictResolverService.cs ← Giải quyết xung đột dữ liệu
    └── CertificateExpiryMonitorService.cs ← Background service giám sát hết hạn
```

### 5.3 Pagination & Filtering

```csharp
// Server-side pagination với cursor-based sync
GET /api/crew?page=1&pageSize=50&search=nguyen&rankName=Master&isOnboard=true

// Response
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 50,
    "totalCount": 245,
    "totalPages": 5,
    "hasNextPage": true
  }
}
```

---

## 6. GIAO DIỆN NGƯỜI DÙNG (FRONTEND)

### 6.1 Shore Frontend — Các màn hình chính

**CrewListPage** (`/crew`):
- Bảng danh sách với cột tìm kiếm nội tuyến (inline search per column)
- Multi-select để gán/gỡ tàu hàng loạt
- Context menu chuột phải (xem, sửa, xóa, gán tàu)
- View chuyển đổi: Danh sách thuyền viên | Giám sát chứng chỉ
- Thống kê quick: Total / Onboard / Pool / PendingReview / Expiring Certs

**CrewDetailPage** (`/crew/:id`):
- 8 tab: Basic Data | Documents | Voyage History | Onboarding | Doc Workflow | Status History | Audit | Logbook
- **Highlight thay đổi từ tàu**: trường nào tàu vừa sửa sẽ được viền đỏ, hiển thị giá trị cũ/mới
- Banner thông báo khi có thay đổi chưa xem từ Edge
- Upload ảnh đại diện (avatar), xem tài liệu được bảo vệ

**CertificateMonitorPage** — Giám sát chứng chỉ toàn đội tàu:
- Lọc theo trạng thái: Valid / Expiring Soon / Expired
- Sắp xếp theo ngày hết hạn
- Lọc theo tên, tàu

**AssignShipModal** — Gán tàu:
- Chọn tàu từ danh sách
- Batch assign cho nhiều thuyền viên cùng lúc

### 6.2 Edge Frontend — Các màn hình chính

**CrewPage** (`/crew`):
- Danh sách thuyền viên đang trên tàu
- **InlinePendingReviewSection**: Hiển thị thuyền viên chờ xét duyệt ngay bên dưới danh sách
- Nút "Review" dẫn đến trang xét duyệt

**CrewDetailPage** (`/crew/:id`):
- 3 tab: Basic Data | Documents | Logbook
- **Review Banner** (khi isPendingReview = true):
  - Checklist 7 mục: Personal Info, Physical Details, Employment Dates, Next of Kin, Education, Contact Info, Documents
  - Approve (khi tất cả checked) / Hold (với ghi chú lý do)
- Chỉnh sửa thông tin trực tiếp — thay đổi được lưu vào `EdgeChanges`
- Upload ảnh thuyền viên, tài liệu, chứng chỉ

**Mobile App** (`frontend-mobile`):
- Thuyền viên xem hồ sơ cá nhân
- Hỗ trợ i18n (đa ngôn ngữ)

### 6.3 Pattern State Management (Frontend)

**Custom Hooks** (không dùng Redux hay Context nặng):
```typescript
// Ví dụ hook sử dụng
const { data: crew, loading, error, refetch } = useCrewDetail(id);
const { data: certificates, refetch: refetchCerts } = useCrewCertificates(id);
const { ranks, countries } = useReferenceData();
const { data: crewStats } = useCrewStats();
```

Ưu điểm: Tách biệt logic fetch/cache ra khỏi component, dễ tái sử dụng, dễ test.

---

## 7. CƠ CHẾ ĐỒNG BỘ TÀU–BỜ

### 7.1 Mô hình Outbox/Inbox (Store-and-Forward)

```
┌── SHORE ─────────────────────────┐    ┌── EDGE ──────────────────────────┐
│                                  │    │                                  │
│  Crew Update                     │    │  [Background SyncService]        │
│      │                           │    │       │                          │
│      ▼                           │    │       ▼                          │
│  SyncOutboxService          ◄────┼────┼── PULL (polling)                │
│  (SyncOutbox table)              │    │       │                          │
│  [targetNode, tableName,         │    │       ▼                          │
│   recordKey, payload,            │    │  SyncConflictHandler             │
│   syncVersion, deliveredAt]      │    │  → Apply/Reject/Merge            │
│                                  │    │                                  │
│  SyncInboxService           ────►│    │  Edge changes                    │
│  (SyncQueue table)          PUSH │    │      │                           │
│  → ConflictResolverService       │    │      ▼                           │
│  → Apply/Reject/Merge            │    │  SyncQueue (local)               │
│                                  │    │  → POST /api/sync/push           │
└──────────────────────────────────┘    └──────────────────────────────────┘
```

### 7.2 Hai chiều đồng bộ

**Shore → Edge (Pull-based)**:
1. Shore cập nhật dữ liệu → ghi vào `SyncOutbox` cho từng tàu (`targetNode`)
2. Edge định kỳ gọi `GET /api/sync/pull?since={cursor}` để lấy các bản ghi mới
3. Edge xử lý qua `SyncConflictHandler` → áp dụng vào DB local
4. Edge ack: `POST /api/sync/ack` với danh sách `itemIds`

**Edge → Shore (Push-based)**:
1. Edge ghi các thay đổi vào `SyncQueue` local
2. `SyncService` background job đóng gói thành batch → `POST /api/sync/push`
3. Shore xử lý qua `SyncInboxService` → `ConflictResolverService`
4. Shore trả về kết quả (succeeded/failed per item)

### 7.3 Quản lý mạng thích ứng

```csharp
// Network types và cấu hình khác nhau
public enum NetworkType
{
    None,               // Không có kết nối
    Satellite_Iridium,  // Thấp (~2.4 kbps, độ trễ cao)
    Satellite_VSAT,     // Trung bình (~512 kbps)
    Cellular_4G,        // Tốt (~10 Mbps khi gần bờ)
    Shore_WiFi          // Tốt nhất (cảng)
}
```

**Tính năng**: Payload tự động nén (GZip), batch size điều chỉnh theo loại mạng, retry với exponential backoff + jitter, **token bucket rate limiting** tránh làm bão hòa kết nối vệ tinh.

### 7.4 Đảm bảo Idempotency

**Vấn đề**: Khi mạng không ổn định, cùng một batch có thể được gửi lại nhiều lần.

**Giải pháp**:
```sql
-- SyncNonce table: lưu hash của mỗi batch đã xử lý
-- Khi nhận batch mới → kiểm tra nonce → skip nếu đã xử lý
```
```csharp
// SyncOutbox deduplication: nếu đã có item cho cùng (node, table, key) chưa deliver
// → cập nhật payload thay vì insert trùng
existing.Payload = serializedPayload;
existing.SyncVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
```

### 7.5 File Synchronization (Ảnh, Tài liệu)

**Vấn đề phức tạp hơn**: File nhị phân không thể đưa vào JSON payload thông thường.

**Giải pháp — File Manifest + Bundle Upload**:
1. Khi cần sync file → tạo `SyncFileManifest` (metadata: fileId, checksum, size)
2. File được upload riêng qua `POST /api/sync/upload-bundle`
3. `CreateDeterministicFileId(node, table, recordKey, role)` — ID cố định theo entity (không dựa vào checksum) → tránh orphaned manifests khi file thay đổi

---

## 8. XỬ LÝ XUNG ĐỘT DỮ LIỆU (CONFLICT RESOLUTION)

### 8.1 Bài toán xung đột

Khi cả Shore và Edge cùng sửa cùng một trường (ví dụ `FullName`) trong khi offline, khi sync sẽ có 2 giá trị khác nhau. Hệ thống phải quyết định giá trị nào được giữ.

### 8.2 Chiến lược phân quyền theo domain

```
Bảng Dữ liệu               Ai làm chủ?
─────────────────────────────────────────────────────
certificates (master)    →  Shore wins (luôn luôn)
countries (master)       →  Shore wins
ranks (master)           →  Shore wins

CrewMember.FullName      →  Shore wins (HR data)
CrewMember.IsOnboard     →  Edge wins (tác nghiệp)
CrewMember.EmbarkDate    →  Edge wins (tác nghiệp)
CrewMember.PhotoUrl      →  Edge wins (tàu chụp ảnh)

service_records          →  Edge wins (tạo trên tàu)
position_data            →  Edge wins
noon_reports             →  Edge wins
```

**Fallback**: Last-Write-Wins theo `UpdatedAt` timestamp khi không có quy tắc domain.

### 8.3 Cài đặt ConflictResolverService (Shore)

```csharp
// Shore-authoritative crew props (HR data, chỉ shore mới sửa)
private static readonly HashSet<string> _shoreAuthoritativeCrewProps = new()
{
    "SocialInsuranceNumber", "TaxIdNumber"
};

// Edge-authoritative crew props (tác nghiệp trên tàu)
private static readonly HashSet<string> _edgeAuthoritativeCrewProps = new()
{
    "IsOnboard", "EmbarkDate", "DisembarkDate",
    "EmbarkPort", "DisembarkPort",
    "PhotoUrl",   // Edge owns avatar (tàu chụp ảnh thuyền viên)
};
```

**Logic merge crew_member** (field-level merge, không phải entity-level):
1. Với mỗi field trong incoming:
   - Nếu field thuộc `_edgeAuthoritativeCrewProps` → Edge wins (apply luôn)
   - Nếu field thuộc `_shoreAuthoritativeCrewProps` → Shore wins (reject field này)
   - Còn lại → Last-Write-Wins (so sánh `UpdatedAt`)

### 8.4 SyncConflictHandler (Edge)

```csharp
// Edge nhận data từ Shore
// Master data → luôn accept (shore là nguồn chính xác)
// Crew HR data → accept từ shore
// IsOnboard, EmbarkDate → REJECT (edge owns these)
// OnboardStatus → đặc biệt: nếu shore gửi "PendingReview" → set IsOnboard=false
```

---

## 9. WORKFLOW PHÊ DUYỆT THUYỀN VIÊN (ONBOARD REVIEW)

### 9.1 Luồng hoàn chỉnh

```
Shore                          Edge (Tàu)
──────────────────────────────────────────────────────
1. Gán thuyền viên vào tàu
   → OnboardStatus = "PendingReview"
   → Sync crew + certs + docs    ──────────────►  Edge nhận
                                                   SyncConflictHandler:
                                                   → IsOnboard = false
                                                   (crew chưa được xác nhận)

2.                               ◄──────────────  CrewPage:
                                                   Hiện thuyền viên trong
                                                   "PendingReview" section
                                                   Thuyền trưởng bấm "Review"

3.                               CrewDetailPage:
                                  Checklist 7 mục:
                                  □ Personal Info
                                  □ Physical Details
                                  □ Employment Dates
                                  □ Next of Kin
                                  □ Education
                                  □ Contact Info
                                  □ Documents

4a. [Chấp nhận]                  Thuyền trưởng tick đủ 7 mục
                                  → Bấm "Approve"
                                  → IsOnboard = true
                                  → OnboardStatus = "Approved"
    Shore nhận qua sync  ◄──────  Sync về shore

4b. [Tạm giữ]                    Thuyền trưởng tick thiếu
                                  → Bấm "On Hold" + ghi chú
                                  → OnboardStatus = "OnHold"
                                  → ReviewNotes = "Thiếu ảnh CMND"
    Shore nhận thông báo ◄──────  Sync về shore
    (bell icon đỏ)

5. Thuyền viên sửa trực tiếp     EdgeChanges tracking:
   trên tàu (ví dụ số ĐT)  ────► EdgeChanges = [{
                                    "field": "phoneNumber",
                                    "oldValue": "+84...",
                                    "newValue": "+84...",
                                    "changedAt": "2026-06-08T..."
                                  }]
                                  EdgeChangesViewed = false

6. Shore mở hồ sơ thuyền viên
   → Trường đã sửa được highlight màu đỏ
   → Xem giá trị cũ/mới
   → Đánh dấu đã xem (POST /mark-changes-viewed)
   → EdgeChangesViewed = true
```

### 9.2 Lý do thiết kế "OnHold" thay vì "Reject"

Trong thực tế vận hành tàu, thuyền trưởng **không thể từ chối thuyền viên được phân công** (vi phạm hợp đồng lao động hàng hải). Do đó:
- Không có nút "Reject"
- Chỉ có "On Hold" = báo thiếu thông tin để shore bổ sung
- Shore nhận thông báo → liên hệ thuyền viên bổ sung → re-sync

---

## 10. KIỂM SOÁT LỖI VÀ XÁC THỰC DỮ LIỆU

### 10.1 Validation tầng Model

```csharp
[Required]
[MaxLength(200)]
public string FullName { get; set; }

[Required]
[MaxLength(50)]
public string CrewId { get; set; }

[MaxLength(5)]
public string? BloodGroup { get; set; }  // MaxLength ngăn overflow
```

### 10.2 Validation tầng Service

```csharp
// Giới hạn pageSize để tránh truy vấn quá lớn
if (pageSize > 200) pageSize = 200;

// Guard khi đồng bộ
if (string.IsNullOrWhiteSpace(targetNode))
    throw new ArgumentNullException(nameof(targetNode));

// Kiểm tra crew tồn tại trước khi tạo onboarding case
var crew = await _db.CrewMembers.FindAsync(request.CrewMemberId);
if (crew == null) throw new ArgumentException("Crew member not found");

// Kiểm tra không tạo 2 onboarding case cho cùng 1 crew
var existingCase = await _db.OnboardingCases.FirstOrDefaultAsync(...);
if (existingCase != null) throw new InvalidOperationException(...);
```

### 10.3 Xử lý lỗi tầng Controller

```csharp
// Pattern nhất quán: try/catch với logging
try {
    var result = await _crewService.GetAllCrewAsync(...);
    return Ok(result);
} catch (Exception ex) {
    _logger.LogError(ex, "Error getting crew list");
    return StatusCode(500, new { error = "Internal server error" });
    // Không lộ stack trace ra client (bảo mật)
}
```

### 10.4 Xử lý DateTime UTC

```csharp
// Converter tự động khi deserialize JSON từ Edge
private sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(...) {
        var dt = reader.GetDateTime();
        return dt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            : dt.ToUniversalTime();
    }
}
// Ngăn lỗi "Cannot write DateTime with Kind=Local" của PostgreSQL/Npgsql
```

### 10.5 Frontend Error Handling

```typescript
// Toast notifications cho mọi action
try {
    await crewApi.create(data);
    toast.success('Tạo thành công', 'Đã thêm thuyền viên mới.');
} catch (err) {
    toast.error('Lỗi tạo thuyền viên',
        err instanceof Error ? err.message : 'Không thể tạo.');
}

// Confirm dialog trước khi xóa (tránh xóa nhầm)
const { confirmed } = await confirm({
    title: 'Xóa thuyền viên',
    message: `Bạn có chắc muốn xóa "${name}"? Không thể hoàn tác.`,
    variant: 'danger',
});
```

### 10.6 Bảo mật

- **JWT Authentication**: Mọi endpoint đều yêu cầu `[Authorize]`
- **HMAC-SHA256 Request Signing**: Requests từ Edge đến Shore được ký số, tránh giả mạo
- **Protected File Endpoints**: File tài liệu thuyền viên chỉ truy cập được qua endpoint có xác thực, không expose URL trực tiếp
- **No Stack Trace Exposure**: Backend chỉ trả về message chung, không lộ chi tiết lỗi

---

## 11. CÁC VẤN ĐỀ KỸ THUẬT ĐÃ GIẢI QUYẾT

### 11.1 Bug: IsOnboard bị reset về false khi sync delta

**Hiện tượng**: Thuyền viên đang trên tàu, bờ gửi bản cập nhật nhỏ (ví dụ `{"weight": 75}`), sau khi sync tàu nhận thấy `IsOnboard = false`.

**Nguyên nhân gốc rễ**:
```
JSON delta: {"weight": 75}
              ↓ Deserialize vào CrewMember
IsOnboard = false  ← default của bool, KHÔNG có trong payload
              ↓ ConflictResolver áp dụng toàn bộ entity
IsOnboard bị ghi đè thành false!
```

**Giải pháp**:
```csharp
// 1. Parse keys thực sự có trong JSON payload
var payloadKeys = JsonDocument.Parse(item.Payload)
    .RootElement.EnumerateObject()
    .Select(p => /* snake_case → PascalCase */)
    .ToHashSet();

// 2. Snapshot các field không có trong payload
var snapshot = new Dictionary<string, object?>();
foreach (var prop in typeof(CrewMember).GetProperties())
{
    if (!payloadKeys.Contains(prop.Name) && IsNonNullableValueType(prop))
        snapshot[prop.Name] = prop.GetValue(existing);
}

// 3. Sau khi resolve, restore các giá trị snapshot
foreach (var (name, val) in snapshot)
    typeof(CrewMember).GetProperty(name)!.SetValue(existing, val);
```

**Bài học**: Khi deserialize partial JSON vào full entity, các field `bool`, `int`, `DateTime` không có trong payload sẽ nhận giá trị default (`false`, `0`, `MinValue`). Phải track payload keys để tránh ghi đè.

### 11.2 Bug: File sync tạo orphaned manifests

**Hiện tượng**: Khi thuyền viên thay avatar → file mới sync được nhưng manifest cũ vẫn ở trạng thái Pending mãi mãi, gây lãng phí và nhầm lẫn.

**Nguyên nhân gốc rễ**:
```
CreateDeterministicFileId(node, table, recordKey, role, CHECKSUM)
                                                        ↑
                            File mới → checksum mới → FileId mới
                            → Manifest cũ không bao giờ được cập nhật
```

**Giải pháp**:
1. Bỏ `checksum` ra khỏi hàm tạo FileId → ID cố định theo entity
2. Khi manifest mới đến → xóa các manifest cũ của cùng entity+role
3. Reset `Status/RequestedAtUtc/NextRetryAt` khi tìm thấy pending request trùng

### 11.3 Bug: EF Core reflection crash với inherited types

**Hiện tượng**: Background service crash với `InvalidOperationException: No suitable constructor found for type CrewMember`.

**Nguyên nhân**: Entity Framework Core reflection-based object creation gặp vấn đề với constructor phức tạp.

**Giải pháp**: Sử dụng `AsNoTracking()` kết hợp projection rõ ràng, tránh để EF tự tạo proxy types không cần thiết.

### 11.4 Bug: Encoding UTF-8 tên thuyền viên

**Hiện tượng**: Tên tiếng Việt (có dấu) bị mã hoá sai khi export/import CSV.

**Giải pháp**: Các script `fix-encoding.js`, `fix-double-encoding.js` xử lý lại encoding, đảm bảo UTF-8 BOM cho Excel compatibility.

---

## 12. SO SÁNH GIẢI PHÁP VÀ LÝ DO LỰA CHỌN

### 12.1 So sánh kiến trúc đồng bộ

| Giải pháp | Ưu điểm | Nhược điểm | Kết luận |
|---|---|---|---|
| **Real-time WebSocket** | Độ trễ thấp | Cần kết nối liên tục, không phù hợp vệ tinh | **Không phù hợp** |
| **REST Polling (đã chọn)** | Hoạt động offline, retry dễ dàng, idempotency rõ ràng | Không real-time | **Phù hợp nhất** |
| **Message Queue (RabbitMQ)** | Scale tốt, durable | Phức tạp, cần thêm infrastructure | Overkill cho quy mô hiện tại |
| **CRDTs** | Merge tự động, không cần conflict resolver | Phức tạp implement, khó debug | Quá phức tạp |

**Lý do chọn REST Polling + Outbox Pattern**:
- Tàu có thể offline nhiều giờ → phải store-and-forward
- Kết nối vệ tinh không ổn định → retry tự động là bắt buộc
- Outbox đảm bảo không mất dữ liệu (at-least-once delivery)
- Simple, debuggable, dễ monitor

### 12.2 So sánh chiến lược conflict resolution

| Chiến lược | Mô tả | Phù hợp khi |
|---|---|---|
| **Last-Write-Wins (LWW)** | Timestamp mới nhất thắng | Đơn giản, ít xung đột thực sự |
| **Shore-Always-Wins** | Shore luôn ghi đè | Chỉ có shore sửa data |
| **Edge-Always-Wins** | Edge luôn ghi đè | Chỉ có edge sửa data |
| **Domain-based (đã chọn)** | Quy tắc theo loại data | Cả 2 bên đều sửa, nhưng mỗi bên sở hữu loại data khác nhau |

**Lý do chọn Domain-based**:
- HR data (tên, ngày sinh) = Shore owns → Shore wins
- Operational data (IsOnboard, ngày lên/xuống tàu) = Edge owns → Edge wins
- Phù hợp với quy trình nghiệp vụ thực tế
- Rõ ràng, dễ giải thích cho stakeholders

### 12.3 So sánh Database

| Option | Lý do chọn PostgreSQL |
|---|---|
| MySQL | Kém hơn về JSON support, JSONB indexes |
| MongoDB | Schema-less phức tạp hơn cho dữ liệu quan hệ (crew → certs → ranks) |
| **PostgreSQL (đã chọn)** | JSONB cho ReviewChecklist/EdgeChanges, UUID native, Full-text search, hỗ trợ tốt với EF Core |

### 12.4 So sánh Frontend State Management

| Option | Lý do chọn Custom Hooks |
|---|---|
| Redux | Boilerplate nhiều, over-engineering cho CRUD đơn giản |
| React Query | Custom hooks nhẹ hơn, tự viết cache đơn giản đủ dùng |
| **Custom Hooks (đã chọn)** | Kiểm soát hoàn toàn, đơn giản, dễ debug |

---

## 13. KẾT QUẢ ĐẠT ĐƯỢC

### 13.1 Tính năng hoàn thành

| STT | Tính năng | Trạng thái |
|---|---|---|
| 1 | CRUD hồ sơ thuyền viên (Shore) | ✅ Hoàn thành |
| 2 | Quản lý chứng chỉ | ✅ Hoàn thành |
| 3 | Upload/quản lý tài liệu | ✅ Hoàn thành |
| 4 | Phân công tàu (batch assign) | ✅ Hoàn thành |
| 5 | Đồng bộ Shore → Edge | ✅ Hoàn thành |
| 6 | Đồng bộ Edge → Shore | ✅ Hoàn thành |
| 7 | Đồng bộ file (ảnh, tài liệu) | ✅ Hoàn thành |
| 8 | Xử lý xung đột dữ liệu | ✅ Hoàn thành |
| 9 | Workflow phê duyệt thuyền viên | ✅ Hoàn thành |
| 10 | Giám sát hết hạn chứng chỉ (Background) | ✅ Hoàn thành |
| 11 | Tracking thay đổi từ tàu (EdgeChanges) | ✅ Hoàn thành |
| 12 | Idempotency (chống trùng lặp) | ✅ Hoàn thành |
| 13 | Rate limiting & adaptive network | ✅ Hoàn thành |
| 14 | Compliance engine (STCW rules) | ✅ Cơ bản |
| 15 | Mobile app | ✅ Cơ bản |

### 13.2 Độ phức tạp kỹ thuật

- **12 bảng** trong module thuyền viên, quan hệ nhiều chiều
- **2 hệ thống độc lập** (Shore + Edge) với schema dùng chung qua `shared` library
- **Bidirectional sync** với conflict resolution theo domain
- Xử lý **môi trường mạng thực tế** (vệ tinh, 4G, offline)
- **File synchronization** với deduplication và orphan cleanup

### 13.3 Lỗi còn tồn tại (minor, không ảnh hưởng logic cốt lõi)

- Một số trường hợp edge case với timezone khi hiển thị ngày trên frontend (hiển thị lệch 1 ngày do múi giờ)
- Mobile app chưa implement đầy đủ tính năng upload tài liệu
- Compliance engine mới xử lý một số rule set cơ bản

---

## 14. HƯỚNG PHÁT TRIỂN

1. **Thông báo push real-time**: Tích hợp SignalR/WebSocket khi tàu vào vùng 4G
2. **OCR tự động**: Scan hộ chiếu, sổ thuyền viên → tự điền thông tin
3. **AI compliance check**: Kiểm tra tự động thiếu chứng chỉ dựa trên rule set STCW
4. **Blockchain certificate**: Xác thực chứng chỉ thuyền viên không thể giả mạo
5. **Offline-first mobile app**: Thuyền viên dùng được app kể cả không có WiFi trên tàu

---

## 15. CÂU HỎI THƯỜNG GẶP (Q&A BẢO VỆ)

### Q: Tại sao dùng Eventual Consistency thay vì Strong Consistency?

**A**: Strong Consistency yêu cầu tất cả node phải đồng thuận trước khi commit (như 2PC — Two-Phase Commit). Với tàu trên biển, kết nối có thể mất hàng giờ hoặc hàng ngày. Nếu dùng 2PC, khi tàu offline thì toàn bộ hệ thống bị block, không thể ghi dữ liệu. Eventual Consistency cho phép tàu hoạt động độc lập, dữ liệu sẽ nhất quán khi kết nối khôi phục — đây là trade-off bắt buộc trong distributed systems với network partition.

### Q: Làm thế nào đảm bảo dữ liệu không bị mất khi mạng đứt?

**A**: Sử dụng **Outbox Pattern**: thay đổi dữ liệu và ghi vào SyncOutbox trong cùng 1 database transaction. Nếu transaction thành công, dữ liệu chắc chắn sẽ được sync sau khi mạng khôi phục. Edge lưu changes vào SyncQueue local trước khi push. Kết hợp với retry mechanism và idempotency keys, đảm bảo **at-least-once delivery**.

### Q: Nếu cả Shore và Edge cùng sửa FullName thuyền viên, kết quả thế nào?

**A**: FullName không nằm trong `_edgeAuthoritativeCrewProps` cũng không trong `_shoreAuthoritativeCrewProps` — tức là sẽ dùng **Last-Write-Wins**: timestamp `UpdatedAt` nào mới hơn thắng. Trong thực tế, quy trình nghiệp vụ quy định FullName chỉ HR bờ mới sửa (theo hộ chiếu), không cho phép tàu sửa tên — quy tắc này được enforce bằng UI (trường FullName disable trên edge frontend).

### Q: Tại sao không dùng WebSocket cho sync?

**A**: Vệ tinh Iridium có băng thông ~2.4 kbps và độ trễ 600ms–1.5 giây. WebSocket cần duy trì kết nối TCP liên tục, không phù hợp. Với kết nối vệ tinh, mỗi lần reconnect tốn nhiều tài nguyên. REST polling phù hợp hơn vì: stateless (không cần maintain connection), có thể batch nhiều thay đổi vào 1 request, nén payload, retry độc lập.

### Q: `SyncVersion` là gì? Tại sao không dùng số nguyên auto-increment?

**A**: `SyncVersion` dùng **Unix timestamp milliseconds** (`DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()`). Lý do không dùng auto-increment: với hệ thống phân tán, 2 node có thể tạo cùng sequence number khi offline. Timestamp đảm bảo ordering toàn cục (global ordering) và có thể dùng làm **cursor** cho incremental pull (`since=1717804800000`).

### Q: Idempotency được implement như thế nào?

**A**: 3 lớp:
1. **Outbox deduplication**: Nếu chưa deliver, update payload thay vì insert duplicate (same node+table+key).
2. **Nonce registry** (`SyncNonce` table): Hash của mỗi batch. Khi nhận batch → kiểm tra hash → skip nếu đã xử lý.
3. **Upsert semantics**: `ProcessIncomingAsync` dùng FindAsync + update nếu exists, insert nếu không — không bao giờ insert duplicate theo PK.

### Q: Tại sao CrewMember dùng UUID thay int làm PK?

**A**: UUID cho phép generate ở client (Edge) mà không cần round-trip đến server. Khi tàu offline và thêm thuyền viên mới, Edge tự generate `Guid.NewGuid()` làm ID — đảm bảo không trùng với Shore. Nếu dùng int auto-increment, cả 2 bên phải dùng dải số khác nhau (chia đôi) hoặc cần server để cấp ID, mà cả 2 cách đều phức tạp hơn.

### Q: Giải thích ISyncableEntity interface?

**A**: Interface chuẩn hoá để tất cả entity có thể sync đều có:
- `IsSynced` — đã sync chưa
- `OriginNode` — tạo từ đâu (SHORE hay EDGE-{IMO})
- `SyncVersion` — phiên bản để ordering
- `UpdatedAt` — để so sánh LWW
- `CreatedAt` — audit

Thiết kế này giúp `SyncOutboxService` và `SyncInboxService` xử lý bất kỳ entity nào theo cùng một flow, không cần viết lại logic cho từng entity.

### Q: Tại sao cần NormalizePayloadToCamelCase?

**A**: PostgreSQL thường lưu và gửi data với `snake_case` keys (ví dụ `full_name`). System.Text.Json deserialize vào C# properties `PascalCase` (`FullName`) cần key là `camelCase` (`fullName`). Hàm này convert `full_name → fullName` để mapping tự động hoạt động, tránh lỗi silent (data bị bỏ qua thay vì báo lỗi).

---

*Tài liệu được tạo ngày 2026-06-08 — Phục vụ bảo vệ đồ án tốt nghiệp*
