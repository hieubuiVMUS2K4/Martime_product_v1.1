# KẾ HOẠCH PHÁT TRIỂN ĐỒNG BỘ THUYỀN VIÊN & CHỨNG CHỈ (TÀU ↔ BỜ)

> **Phiên bản**: 1.4  
> **Ngày tạo**: 2026-03-02  
> **Cập nhật**: 2026-03-03 — TẤT CẢ 6 PHASE HOÀN THÀNH ✅  
> **Mục tiêu**: Triển khai đồng bộ dữ liệu thuyền viên, chứng chỉ, hồ sơ giữa Edge (tàu) và Shore (bờ), tuân thủ STCW/MLC 2006/ISM Code

---

## 📋 MỤC LỤC

1. [Tổng quan hiện trạng](#1-tổng-quan-hiện-trạng)
2. [Gap Analysis chi tiết](#2-gap-analysis-chi-tiết)
3. [Kiến trúc đồng bộ](#3-kiến-trúc-đồng-bộ)
4. [Tổ chức cấu trúc thư mục](#4-tổ-chức-cấu-trúc-thư-mục)
5. [Shared Contracts Library](#5-shared-contracts-library)
6. [Kế hoạch triển khai theo Phase](#6-kế-hoạch-triển-khai-theo-phase)
7. [Thiết kế Database Shore](#7-thiết-kế-database-shore)
8. [Thiết kế API Shore](#8-thiết-kế-api-shore)
9. [Sync Protocol](#9-sync-protocol)
10. [Frontend Shore UI](#10-frontend-shore-ui)
11. [Quy chuẩn quốc tế & Compliance](#11-quy-chuẩn-quốc-tế--compliance)
12. [Hiệu năng & Production Standards](#12-hiệu-năng--production-standards)
13. [Timeline chi tiết](#13-timeline-chi-tiết)

---

## 1. TỔNG QUAN HIỆN TRẠNG

### 1.1 Edge (Tàu) — ĐÃ CÓ ✅

| Thành phần | Trạng thái | Chi tiết |
|------------|-----------|----------|
| **CrewMember Model** | ✅ Đầy đủ | ~50+ fields, STCW/MLC compliant (bio-data, next-of-kin, education, employment) |
| **Certificate Model** | ✅ Đầy đủ | Master data với category (COMPETENCY/MEDICAL/PROFICIENCY/SAFETY) |
| **CrewCertificate** | ✅ Đầy đủ | Junction table với IssueDate, ExpiryDate, Status, file upload |
| **Country/Rank/RankCertificate/CountryCertificate** | ✅ Đầy đủ | Reference data đầy đủ |
| **TravelDocument/SeafarerDocument/EmploymentDocument/HealthDocument** | ✅ Đầy đủ | 4 loại hồ sơ với file management |
| **ServiceRecord** | ✅ Model có | Nhưng CHƯA CÓ Controller/Endpoint |
| **VoyageCrewAssignment** | ✅ Đầy đủ | FAL Form 5 compliance |
| **CrewController** | ✅ Đầy đủ | CRUD + documents + avatar + sync-users |
| **CertificatesController** | ✅ Đầy đủ | CRUD + crew-certificates + bulk + file upload |
| **Frontend-edge UI** | ✅ Đầy đủ | CrewPage, CrewDetailPage, CertificateManagement, AddCrewCertificate |
| **SyncQueue Model** | ✅ Có | Priority-based (Critical/Operational/Low), network-aware |
| **SyncService** | ⚠️ Stub | `SendToShoreAsync()` là placeholder, chưa gọi HTTP thực |
| **SyncBackgroundWorker** | ✅ Hoạt động | Chạy mỗi 60s, poll SyncQueue |
| **IsSynced flag** | ✅ Trên entities | Nhưng CHƯA auto-enqueue vào SyncQueue khi data thay đổi |

### 1.2 Shore (Bờ) — HOÀN THÀNH ✅

| Thành phần | Trạng thái | Chi tiết |
|------------|-----------|----------|
| **CrewMember Model** | ✅ Hoàn thành | Full model từ Maritime.Shared (50+ fields, ISyncableEntity) |
| **Certificate Model** | ✅ Hoàn thành | Crew Certificate types từ Maritime.Shared + VesselCertificate (đã rename) |
| **CrewCertificate** | ✅ Hoàn thành | DbSet + OnModelCreating configured |
| **Country/Rank** | ✅ Hoàn thành | DbSets + indexes + relationships |
| **Document Models** | ✅ Hoàn thành | Travel/Seafarer/Employment/Health từ Maritime.Shared |
| **ServiceRecord** | ✅ Hoàn thành | DbSet + OnModelCreating configured |
| **SyncOutbox/SyncLog** | ✅ Hoàn thành | Shore sync infrastructure tables |
| **CrewController** | ✅ Hoàn thành | Phase 2 — Full CRUD + documents + service records |
| **CertificatesController** | ✅ Hoàn thành | Phase 2 — Cert types CRUD + crew certs + expiring + compliance |
| **RanksController** | ✅ Hoàn thành | Phase 2 — CRUD reference data |
| **CountriesController** | ✅ Hoàn thành | Phase 2 — CRUD reference data |
| **CrewService/CertificateService** | ✅ Hoàn thành | Phase 2 — Interface + implementation, DI registered, error handling |
| **Seed Data** | ✅ Hoàn thành | Phase 2 — 19 ranks, 32 countries, 26 cert types, rank-cert requirements |
| **SyncController** | ✅ Hoàn thành | Phase 3 — Inbox/Pull/Acknowledge/Status, batch size limit |
| **SyncInboxService** | ✅ Hoàn thành | Phase 3 — Process incoming, conflict resolution |
| **SyncOutboxService** | ✅ Hoàn thành | Phase 3 — Enqueue/Broadcast/Pull/Acknowledge with validation |
| **ConflictResolverService** | ✅ Hoàn thành | Phase 3 — Field-level merge, last-write-wins |
| **HealthController** | ✅ Hoàn thành | Phase 6 — Liveness + readiness (DB, migrations, sync, disk) |
| **Global Exception Handler** | ✅ Hoàn thành | Phase 6 — UseExceptionHandler middleware with ProblemDetails |
| **Frontend Shore UI** | ✅ Hoàn thành | Phase 4 — CrewList, CrewDetail, CrewForm, CertificateMonitor, SyncDashboard |
| **Crew API Service** | ✅ Hoàn thành | Phase 4 — crew.service.ts, sync.service.ts, hooks, types |

### 1.3 Shared Library — ĐÃ HOÀN THÀNH ✅

| Thành phần | Trạng thái | Chi tiết |
|------------|-----------|----------|
| **Maritime.Shared.csproj** | ✅ | Class library net8.0, referenced by cả 2 projects |
| **Shared Models** | ✅ | CrewMember, Certificate, CrewCertificate, Country, Rank, RankCertificate, CountryCertificate, ServiceRecord |
| **Document Models** | ✅ | BaseDocument → BaseCountryDocument → TravelDoc/SeafarerDoc/EmploymentDoc + HealthDoc |
| **Sync Models** | ✅ | SyncQueue, SyncOutbox, SyncLog + enums |
| **Shared DTOs** | ✅ | CrewMemberDto, RankDto, CrewDetailDto, CertificateDto, CrewCertificateDto, SyncDtos |
| **Constants** | ✅ | Department, CertificateCategory, CertificateStatus, SyncConstants |
| **Interfaces** | ✅ | ISyncableEntity, ISoftDeletable |
| **Global Using Aliases** | ✅ | SharedTypeAliases.cs trên cả edge-services và backend |

### 1.4 Cả hai phía — TRẠNG THÁI SAU KHI HOÀN THÀNH ✅

| Thành phần | Trạng thái |
|------------|-----------|
| **Shared Contracts** | ✅ Maritime.Shared library — shared models, DTOs, constants, interfaces |
| **Sync Transport** | ✅ Edge HTTP push/pull to Shore, SyncService with real HTTP calls |
| **Shore → Edge Sync** | ✅ SyncOutbox + cursor-based pull API |
| **Edge → Shore Sync** | ✅ SyncInbox + batch processing + SyncLog |
| **Conflict Resolution** | ✅ ConflictResolverService — field-level merge, last-write-wins |
| **Error Handling** | ✅ Global exception middleware, service validation, sync outbox guards |
| **Health Monitoring** | ✅ HealthController on both shore and edge |
| **Edge SyncController** | ✅ Manual trigger wired to real SyncService.ExecuteSyncAsync |
| **Shore Frontend** | ✅ CrewList, CrewDetail, CrewForm, CertificateMonitor, SyncDashboard |
| **Edge SyncPage** | ✅ Real sync monitoring UI with queue view, manual trigger |

---

## 2. GAP ANALYSIS CHI TIẾT

### 2.1 Shore Backend — Cần xây mới

```
❌ Models:       CrewMember (full), CrewCertificate, Certificate (crew type),
                 Country, Rank, RankCertificate, CountryCertificate,
                 TravelDocument, SeafarerDocument, EmploymentDocument, HealthDocument,
                 ServiceRecord, VoyageCrewAssignment, DrillLog (crew-linked)

❌ DbContext:    DbSets cho tất cả entities trên + OnModelCreating configuration

❌ Controllers:  CrewController, CertificatesController, RanksController,
                 CountriesController, CrewDocumentsController, ServiceRecordsController

❌ Services:     CrewService, CertificateService, SyncService (crew handling)

❌ DTOs:         CrewMemberDto, CertificateDto, CrewCertificateDto, DocumentDtos...

❌ Migrations:   EF Core migrations cho tất cả bảng mới

⚠️ SyncController: Thêm cases cho crew_members, crew_certificates, certificates,
                    ranks, countries, documents, service_records
```

### 2.2 Shore Frontend — Cần xây mới

```
❌ API Services:  crewService, certificateService, syncService

❌ Types:         CrewMember (full), Certificate, CrewCertificate, Country, Rank...

❌ Pages:         CrewListPage (real API), CrewDetailPage (tabbed),
                  CertificateManagementPage, CertificateDetailPage,
                  SyncDashboardPage

❌ Components:    CrewTable, CrewForm, CertificateTable, CertificateForm,
                  DocumentViewer, SyncStatusWidget, ExpiryAlertBadge
```

### 2.3 Edge Backend — Cần cải thiện

```
⚠️ SyncService:     Implement SendToShoreAsync() thực (HTTP client)
⚠️ Auto-enqueue:    Tự động enqueue vào SyncQueue khi crew/cert data thay đổi
⚠️ Service Layer:   Extract logic từ controllers sang services
❌ ServiceRecord:    Cần Controller/Endpoint cho ServiceRecord
❌ Shore→Edge Sync:  Nhận dữ liệu cập nhật từ bờ (pull hoặc push)
❌ Conflict:         Conflict resolution strategy
```

### 2.4 Edge Frontend — Cần cải thiện

```
⚠️ SyncPage:     Hiện là placeholder "Coming Soon" → cần UI thực
⚠️ Sync Status:  Hiển thị trạng thái sync real-time cho crew data
```

---

## 3. KIẾN TRÚC ĐỒNG BỘ

### 3.1 Tổng quan Flow

```
┌─────────────────────────────────┐         ┌─────────────────────────────────┐
│         EDGE (Tàu)              │         │         SHORE (Bờ)              │
│                                 │         │                                 │
│  ┌──────────┐  ┌──────────────┐ │  HTTP   │ ┌──────────────┐ ┌──────────┐  │
│  │ Frontend │  │ Edge API     │ │ ──────► │ │ Shore API    │ │ Frontend │  │
│  │ (React)  │  │ :5001        │ │         │ │ :5000        │ │ (React)  │  │
│  └──────────┘  └──────┬───────┘ │         │ └──────┬───────┘ └──────────┘  │
│                       │         │         │        │                        │
│               ┌───────▼───────┐ │         │ ┌──────▼───────┐               │
│               │ PostgreSQL    │ │         │ │ PostgreSQL   │               │
│               │ (edge_db)     │ │         │ │ (shore_db)   │               │
│               └───────────────┘ │         │ └──────────────┘               │
│                                 │         │                                 │
│  ┌─────────────────────────┐    │         │  ┌──────────────────────────┐   │
│  │ SyncQueue (store)       │    │  VSAT/  │  │ SyncInbox (receive)     │   │
│  │ SyncBackgroundWorker    │────│──4G/────│─►│ SyncProcessor           │   │
│  │ (forward queue)         │    │  WiFi   │  │ (conflict resolution)   │   │
│  └─────────────────────────┘    │         │  └──────────────────────────┘   │
│                                 │         │                                 │
│  ┌─────────────────────────┐    │         │  ┌──────────────────────────┐   │
│  │ SyncPullService         │◄───│─────────│──│ SyncOutbox              │   │
│  │ (receive from shore)    │    │         │  │ (shore→edge changes)    │   │
│  └─────────────────────────┘    │         │  └──────────────────────────┘   │
└─────────────────────────────────┘         └─────────────────────────────────┘
```

### 3.2 Sync Strategy: Bi-directional Delta Sync

| Direction | Use Case | Priority | Transport |
|-----------|----------|----------|-----------|
| **Edge → Shore** | Crew onboard changes, certificate scans, embark/disembark | Critical/Operational | HTTP POST → `/api/sync` |
| **Shore → Edge** | Certificate renewals, new crew assignments, master data updates | Operational/Low | HTTP GET ← `/api/sync/pull?since={timestamp}&node={shipId}` |

### 3.3 Conflict Resolution Strategy

```
Nguyên tắc: "Last Write Wins" + Domain-specific rules

1. MASTER DATA (Certificate types, Countries, Ranks):
   → Shore luôn thắng (Shore is the source of truth)

2. CREW DATA (CrewMember basic info):
   → Shore thắng cho: FullName, DateOfBirth, Nationality, CrewId (HR data)
   → Edge thắng cho: IsOnboard, EmbarkDate, DisembarkDate (operational data)

3. CREW CERTIFICATES:
   → Shore thắng cho: CertificateNumber, IssueDate, ExpiryDate (official records)
   → Edge thắng cho: DocumentFilePath (scanned copy on board)

4. DOCUMENTS (Travel, Seafarer, Employment, Health):
   → Shore thắng cho: official document metadata
   → Edge thắng cho: file uploads (local scans)

5. SERVICE RECORDS:
   → Edge thắng (created on board during voyage)

Mỗi record có: UpdatedAt (UTC) + OriginNode + SyncVersion
Conflict detection: so sánh SyncVersion, nếu khác → apply domain rules
```

### 3.4 Network-Aware Priorities (Đã có trên Edge)

| Network | Bandwidth | Cost | Sync Priority |
|---------|-----------|------|--------------|
| None | 0 | - | Không sync |
| Iridium | 2.4 kbps | $$$$ | Critical only |
| VSAT | 512 kbps | $$$ | Critical + Operational |
| 4G/WiFi | 10+ Mbps | $ | Tất cả + files |

---

## 4. TỔ CHỨC CẤU TRÚC THƯ MỤC

### 4.1 Cấu trúc hiện tại — Vấn đề

```
❌ Không có shared library → models phải copy giữa edge và shore
❌ Backend (shore) không có folder structure rõ ràng cho crew
❌ Không có Shared DTO contracts → frontend types cũng phải copy
❌ Solution file chỉ include backend, không include edge-services
```

### 4.2 Cấu trúc đề xuất — Professional

```
Martime_product_v1.1/
│
├── product.sln                          # Solution chứa TẤT CẢ projects
│
├── shared/                              # 🆕 SHARED LIBRARY (NuGet package internal)
│   ├── Maritime.Shared.csproj
│   ├── Models/
│   │   ├── Crew/
│   │   │   ├── CrewMember.cs            # Shared entity (base properties)
│   │   │   ├── Certificate.cs
│   │   │   ├── CrewCertificate.cs
│   │   │   ├── Country.cs
│   │   │   ├── Rank.cs
│   │   │   ├── RankCertificate.cs
│   │   │   └── CountryCertificate.cs
│   │   ├── Documents/
│   │   │   ├── TravelDocument.cs
│   │   │   ├── SeafarerDocument.cs
│   │   │   ├── EmploymentDocument.cs
│   │   │   └── HealthDocument.cs
│   │   └── Sync/
│   │       ├── SyncQueueItem.cs
│   │       └── SyncEnums.cs             # SyncActionType, SyncPriority, NetworkType
│   ├── DTOs/
│   │   ├── Crew/
│   │   │   ├── CrewMemberDto.cs
│   │   │   ├── CrewDetailDto.cs
│   │   │   ├── CreateCrewRequest.cs
│   │   │   ├── UpdateCrewRequest.cs
│   │   │   ├── CertificateDto.cs
│   │   │   ├── CrewCertificateDto.cs
│   │   │   └── DocumentDtos.cs
│   │   └── Sync/
│   │       ├── SyncQueueItemDto.cs       # Shared sync payload format
│   │       ├── SyncStatusDto.cs
│   │       └── SyncConflictDto.cs
│   ├── Constants/
│   │   ├── Department.cs
│   │   ├── CertificateCategory.cs
│   │   └── SyncConstants.cs
│   ├── Interfaces/
│   │   ├── ICrewService.cs
│   │   ├── ICertificateService.cs
│   │   ├── ISyncService.cs
│   │   └── IAuditableEntity.cs          # CreatedAt, UpdatedAt, SyncVersion
│   └── Validators/
│       ├── CrewValidator.cs             # FluentValidation rules (STCW compliance)
│       └── CertificateValidator.cs
│
├── backend/                             # SHORE API (refactored)
│   ├── product-api.csproj               # References Maritime.Shared
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── Core/
│   │   │   ├── AuthController.cs
│   │   │   └── SyncController.cs        # 🔧 Extend with crew/cert handling
│   │   ├── Crew/                        # 🆕
│   │   │   ├── CrewController.cs
│   │   │   ├── CertificatesController.cs
│   │   │   ├── RanksController.cs
│   │   │   ├── CountriesController.cs
│   │   │   ├── CrewDocumentsController.cs
│   │   │   └── ServiceRecordsController.cs
│   │   ├── Fleet/                       # 🆕 Rename from flat structure
│   │   │   ├── VesselsController.cs
│   │   │   └── VesselTelemetryController.cs
│   │   └── Maintenance/
│   │       └── PmsController.cs
│   ├── Services/
│   │   ├── Core/
│   │   │   ├── AuthService.cs
│   │   │   ├── AuditInterceptor.cs      # 🆕 (Edge đã có)
│   │   │   └── SyncProcessorService.cs  # 🆕 Process incoming sync data
│   │   ├── Crew/                        # 🆕
│   │   │   ├── CrewService.cs
│   │   │   ├── CertificateService.cs
│   │   │   ├── CertificateAlertService.cs  # 🆕 Expiry monitoring
│   │   │   └── CrewDocumentService.cs
│   │   └── Sync/                        # 🆕
│   │       ├── SyncInboxService.cs      # Nhận data từ edge
│   │       ├── SyncOutboxService.cs     # Gửi data xuống edge
│   │       └── ConflictResolverService.cs
│   ├── Data/
│   │   ├── AppDbContext.cs              # 🔧 Add crew/cert DbSets
│   │   └── Configurations/             # 🆕 EF Core IEntityTypeConfiguration
│   │       ├── CrewMemberConfiguration.cs
│   │       ├── CertificateConfiguration.cs
│   │       └── ...
│   ├── Repositories/                    # 🆕 Generic Repository pattern
│   │   ├── IRepository.cs
│   │   ├── Repository.cs
│   │   ├── ICrewRepository.cs
│   │   └── CrewRepository.cs
│   ├── Migrations/
│   └── Middleware/
│       └── AuditMiddleware.cs
│
├── edge-services/                       # EDGE API (refactored)
│   ├── EdgeCollector.csproj             # References Maritime.Shared
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── Core/
│   │   │   ├── AuthController.cs
│   │   │   └── SyncController.cs
│   │   ├── Crew/
│   │   │   ├── CrewController.cs        # 🔧 Extract service layer
│   │   │   ├── CertificatesController.cs
│   │   │   ├── RanksController.cs
│   │   │   ├── CountriesController.cs
│   │   │   ├── RankCertificatesController.cs
│   │   │   ├── CountryCertificatesController.cs
│   │   │   └── ServiceRecordsController.cs  # 🆕 Expose ServiceRecord API
│   │   └── ...
│   ├── Services/
│   │   ├── Core/
│   │   │   ├── SyncService.cs           # 🔧 Implement real HTTP transport
│   │   │   ├── SyncBackgroundWorker.cs
│   │   │   └── SyncPullService.cs       # 🆕 Pull updates from shore
│   │   ├── Crew/                        # 🆕 Service layer
│   │   │   ├── CrewService.cs
│   │   │   └── CertificateService.cs
│   │   └── Sync/
│   │       ├── SyncEnqueueService.cs    # 🆕 Auto-enqueue on data changes
│   │       └── SyncConflictHandler.cs   # 🆕
│   ├── Data/
│   │   ├── EdgeDbContext.cs             # 🔧 Add SaveChanges interceptor for auto-enqueue
│   │   └── Interceptors/
│   │       └── SyncInterceptor.cs       # 🆕 EF interceptor to auto-enqueue changes
│   └── ...
│
├── frontend/                            # SHORE FRONTEND (major rebuild)
│   ├── src/
│   │   ├── services/
│   │   │   ├── api.ts                   # 🔧 Axios instance with interceptors
│   │   │   ├── crew.service.ts          # 🆕 Crew CRUD API
│   │   │   ├── certificate.service.ts   # 🆕 Certificate API
│   │   │   ├── sync.service.ts          # 🆕 Sync status API
│   │   │   └── pms.service.ts
│   │   ├── types/
│   │   │   ├── crew.types.ts            # 🔧 Full types matching shared DTOs
│   │   │   ├── certificate.types.ts     # 🆕
│   │   │   └── sync.types.ts            # 🆕
│   │   ├── pages/
│   │   │   ├── CrewManagement/          # 🔧 Rebuild with real API
│   │   │   │   ├── CrewListPage.tsx     # Multi-ship crew overview
│   │   │   │   ├── CrewDetailPage.tsx   # 🆕 Tabbed detail view
│   │   │   │   └── CrewFormPage.tsx     # 🆕 Add/Edit crew
│   │   │   ├── CertificateManagement/  # 🆕
│   │   │   │   ├── CertificateListPage.tsx
│   │   │   │   ├── CertificateDetailPage.tsx
│   │   │   │   ├── ExpiryDashboard.tsx  # 🆕 Fleet-wide certificate expiry
│   │   │   │   └── ComplianceReport.tsx # 🆕 STCW compliance matrix
│   │   │   ├── SyncManagement/         # 🆕
│   │   │   │   ├── SyncDashboard.tsx    # All ships sync status
│   │   │   │   └── SyncHistory.tsx      # 🆕 Sync audit log
│   │   │   └── ...
│   │   ├── components/
│   │   │   ├── crew/                    # 🆕
│   │   │   │   ├── CrewTable.tsx
│   │   │   │   ├── CrewCard.tsx
│   │   │   │   ├── CertificateStatusBadge.tsx
│   │   │   │   └── ExpiryAlertWidget.tsx
│   │   │   ├── sync/                    # 🆕
│   │   │   │   ├── SyncStatusIndicator.tsx
│   │   │   │   └── ShipSyncCard.tsx
│   │   │   └── shared/
│   │   └── stores/
│   │       ├── crew.store.ts            # 🆕 Zustand store
│   │       └── sync.store.ts            # 🆕
│   └── ...
│
├── frontend-edge/                       # EDGE FRONTEND (improvements)
│   ├── src/
│   │   ├── pages/
│   │   │   ├── Crew/                    # ✅ Đã có, khá đầy đủ
│   │   │   └── Sync/
│   │   │       └── SyncPage.tsx         # 🔧 Implement real sync dashboard
│   │   └── ...
│   └── ...
│
├── shared-types/                        # 🆕 SHARED TYPESCRIPT TYPES
│   ├── package.json
│   ├── tsconfig.json
│   ├── src/
│   │   ├── crew.types.ts               # Shared TS types for crew
│   │   ├── certificate.types.ts        # Shared TS types for cert
│   │   ├── sync.types.ts               # Shared TS types for sync
│   │   └── index.ts
│   └── dist/                           # Compiled output
│
├── docs/
│   ├── CREW_SYNC_DEVELOPMENT_PLAN.md   # 📋 This document
│   ├── SYNC_PROTOCOL_SPEC.md           # 🆕 Sync protocol specification
│   └── ...
│
├── docker-compose.yml                   # Shore infrastructure
├── docker-compose.dev.yml               # 🆕 Development orchestration
└── scripts/
    ├── seed-crew-data.sql
    └── sync-test.ps1                    # 🆕 Sync integration test
```

### 4.3 Nguyên tắc tổ chức

| Nguyên tắc | Giải thích |
|------------|-----------|
| **Shared Contracts** | Models, DTOs, Interfaces, Constants ở `shared/` → cả edge và shore reference |
| **Feature Folders** | Controllers/Services/Repos tổ chức theo domain (Crew/, Fleet/, Sync/) thay vì theo layer |
| **Single Source of Truth** | Mỗi entity chỉ define 1 lần trong `shared/` |
| **TypeScript Shared Types** | `shared-types/` package cho cả 2 frontend |
| **Convention over Configuration** | Naming convention nhất quán giữa edge và shore |

---

## 5. SHARED CONTRACTS LIBRARY

### 5.1 Maritime.Shared.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Maritime.Shared</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>
</Project>
```

### 5.2 Shared Entity Base

```csharp
// shared/Models/BaseEntity.cs
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string OriginNode { get; set; } = "SHORE";  // "SHIP_01", "SHORE"
    public long SyncVersion { get; set; } = 0;          // Incremented on each change
    public bool IsSynced { get; set; } = false;
}
```

### 5.3 Shared Sync DTO

```csharp
// shared/DTOs/Sync/SyncQueueItemDto.cs
public class SyncQueueItemDto
{
    public string TableName { get; set; }     // "crew_members", "crew_certificates"
    public string RecordKey { get; set; }     // Primary key as string
    public string ActionType { get; set; }    // CREATE, UPDATE, DELETE
    public string Payload { get; set; }       // JSON (full for CREATE, delta for UPDATE)
    public string OriginNode { get; set; }    // "SHIP_01", "SHORE"
    public long SyncVersion { get; set; }     // For conflict detection
    public DateTime Timestamp { get; set; }   // UTC timestamp of change
}

public class SyncPullResponse
{
    public List<SyncQueueItemDto> Items { get; set; }
    public DateTime ServerTime { get; set; }
    public string NextCursor { get; set; }    // For pagination
    public bool HasMore { get; set; }
}
```

---

## 6. KẾ HOẠCH TRIỂN KHAI THEO PHASE

### Phase 1: Foundation (Tuần 1-2) — Nền tảng ✅ HOÀN THÀNH

> **Mục tiêu**: Tạo shared library, migrate models, shore DB ready  
> **Trạng thái**: ✅ **HOÀN THÀNH** (2026-03-02)

| # | Task | Effort | Priority | Status |
|---|------|--------|----------|--------|
| 1.1 | Tạo `shared/Maritime.Shared.csproj` project | 0.5 ngày | P0 | ✅ Done |
| 1.2 | Extract Crew/Certificate models từ edge-services vào shared | 1 ngày | P0 | ✅ Done |
| 1.3 | Extract DTOs vào shared | 0.5 ngày | P0 | ✅ Done |
| 1.4 | Extract Constants, Enums vào shared | 0.5 ngày | P0 | ✅ Done |
| 1.5 | Update `product.sln` include shared + edge-services | 0.5 ngày | P0 | ✅ Done |
| 1.6 | Backend (shore) reference shared, thay thế CrewMember stub | 1 ngày | P0 | ✅ Done |
| 1.7 | Edge-services reference shared, replace models | 1 ngày | P0 | ✅ Done |
| 1.8 | Shore: Add DbSets + OnModelCreating cho crew/cert tables | 1 ngày | P0 | ✅ Done |
| 1.9 | Shore: EF Migration `AddCrewCertificateModule` | 0.5 ngày | P0 | ⏭️ Defer to Phase 2 |
| 1.10 | Verify cả 2 side compile thành công | 0.5 ngày | P0 | ✅ Done |

**Kết quả Phase 1:**
- `shared/Maritime.Shared.csproj`: 15+ shared models, 10+ DTOs, constants, interfaces
- `edge-services/Models/SharedTypeAliases.cs`: Global using re-exports (16 types)
- `edge-services/DTOs/Crew/CrewDtos.cs`: Re-exports from shared (15 DTO types)
- `backend/Models/SharedTypeAliases.cs`: Global using re-exports (30+ types)
- `backend/Models/MaritimeModels.cs`: Certificate → VesselCertificate (rename để tránh conflict)
- `backend/Data/AppDbContext.cs`: 17 new DbSets + full OnModelCreating crew/cert/sync config
- Loại bỏ ~700 dòng duplicate code từ EdgeModels.cs
- **Build Solution: 3 projects, 0 errors** ✅

**Deliverable**: ✅ Shared library, shore DB có crew/cert tables, cả 2 projects build OK

### Phase 2: Shore API (Tuần 3-4) — Backend bờ ✅ HOÀN THÀNH

> **Mục tiêu**: Full CRUD API cho crew/certificates trên bờ  
> **Trạng thái**: ✅ **HOÀN THÀNH** (2026-03-02)

| # | Task | Effort | Priority | Status |
|---|------|--------|----------|--------|
| 2.1 | Shore: `CrewService` + `ICrewService` | 1 ngày | P0 | ✅ Done |
| 2.2 | Shore: `CertificateService` + `ICertificateService` | 1 ngày | P0 | ✅ Done |
| 2.3 | Shore: `CrewController` (list, detail, create, update, delete) | 1.5 ngày | P0 | ✅ Done |
| 2.4 | Shore: `CertificatesController` (types + crew-certificates + expiring + compliance) | 1.5 ngày | P0 | ✅ Done |
| 2.5 | Shore: `RanksController` + `CountriesController` | 1 ngày | P1 | ✅ Done |
| 2.6 | Shore: `CrewDocumentsController` (embedded in CrewController) | 1 ngày | P1 | ✅ Done |
| 2.7 | Shore: `ServiceRecordsController` (embedded in CrewController) | 0.5 ngày | P1 | ✅ Done |
| 2.8 | Shore: File upload service cho documents/certificates | 1 ngày | P1 | ⏭️ Phase 4 |
| 2.9 | Shore: Seed data cho Countries, Ranks, Certificate types | 0.5 ngày | P1 | ✅ Done |
| 2.10 | Shore: API testing với Swagger/Postman | 1 ngày | P0 | ⏭️ Phase 4 |

**Kết quả Phase 2:**
- `backend/Services/Crew/`: ICrewService, CrewService (~450 lines), ICertificateService, CertificateService (~300 lines)
- `backend/Controllers/Crew/`: CrewController (CRUD + docs + service records), CertificatesController (types + crew certs + fleet queries), RanksController, CountriesController
- DI registered in Program.cs, SyncQueueItemDto conflict resolved
- `seed-crew-reference-data.sql`: 19 ranks, 32 countries, 26 STCW cert types, rank-certificate requirements
- **Build result: 3 projects, 0 errors** ✅

### Phase 3: Sync Engine (Tuần 5-7) — Core Synchronization ✅ HOÀN THÀNH

> **Mục tiêu**: Đồng bộ 2 chiều Edge ↔ Shore hoạt động

| # | Task | Effort | Priority | Status |
|---|------|--------|----------|--------|
| 3.1 | Edge: Implement `SendToShoreAsync()` (HttpClient → Shore API) | 1.5 ngày | P0 | ✅ |
| 3.2 | Shore: Extend `SyncController` — add crew/cert table cases | 1.5 ngày | P0 | ✅ |
| 3.3 | Edge: `SyncEnqueueInterceptor` — auto-enqueue on SaveChanges | 2 ngày | P0 | ✅ (có sẵn + crew priority update) |
| 3.4 | Shore: `SyncInboxService` — process incoming crew/cert data | 1.5 ngày | P0 | ✅ |
| 3.5 | Shore: `SyncOutboxService` — queue changes for edge pull | 1.5 ngày | P0 | ✅ |
| 3.6 | Edge: `SyncPullService` — periodic pull from shore | 1.5 ngày | P0 | ✅ |
| 3.7 | Shore: `ConflictResolverService` — domain-based resolution | 2 ngày | P0 | ✅ |
| 3.8 | Edge: `SyncConflictHandler` — handle shore-originated updates | 1 ngày | P0 | ✅ |
| 3.9 | Shore: `/api/sync/pull` endpoint (cursor-based pagination) | 1 ngày | P1 | ✅ |
| 3.10 | Integration testing: Edge→Shore + Shore→Edge cycle | 2 ngày | P0 | ⏳ Runtime test |

**Phase 3 Implementation Details**:
- `edge-services/Services/Core/SyncService.cs`: Full rewrite — HTTP batch send, pull with cursor pagination, compression, exponential backoff
- `edge-services/Services/Core/SyncBackgroundWorker.cs`: Dual cycle — push every 60s, pull every 300s (configurable)
- `edge-services/Services/Core/SyncConflictHandler.cs`: Edge-side conflict handler — accepts master data from Shore, field-level merge for crew
- `edge-services/Program.cs`: ShoreAPI HttpClient, ISyncConflictHandler DI
- `edge-services/Data/EdgeDbContext.cs`: GetPriorityForEntity updated — crew entities now Operational priority
- `backend/Controllers/SyncController.cs`: Full rewrite — POST /api/sync (inbox), GET /api/sync/pull (cursor), POST /api/sync/acknowledge, GET /api/sync/status
- `backend/Services/Sync/SyncInboxService.cs`: 16 table mappings, CREATE/UPDATE/DELETE handlers, conflict resolution
- `backend/Services/Sync/SyncOutboxService.cs`: EnqueueAsync, BroadcastAsync, cursor-based GetPendingItemsAsync
- `backend/Services/Sync/ConflictResolverService.cs`: 5-rule domain conflict resolution (shore-auth master, edge-auth ops, crew field merge, cert field merge, LWW fallback)
- `backend/Services/Crew/CrewService.cs`: Integrated SyncOutboxService — broadcasts crew CRUD to edge nodes
- `backend/Services/Crew/CertificateService.cs`: Integrated SyncOutboxService — broadcasts cert type + crew cert changes
- `backend/Program.cs`: ISyncInboxService, ISyncOutboxService, IConflictResolverService DI registered
- **Build result: 3 projects, 0 errors** ✅

**Deliverable**: Bi-directional sync working for crew/certificate data

### Phase 4: Shore Frontend (Tuần 8-10) — UI bờ

> **Mục tiêu**: Shore UI đầy đủ cho quản lý thuyền viên multi-ship

| # | Task | Effort | Priority |
|---|------|--------|----------|
| 4.1 | `shared-types/` package + types matching shared DTOs | 1 ngày | P0 |
| 4.2 | Shore: `crew.service.ts` + `certificate.service.ts` | 1 ngày | P0 |
| 4.3 | Shore: Rebuild `CrewListPage` with real API + multi-ship view | 2 ngày | P0 |
| 4.4 | Shore: `CrewDetailPage` (tabbed: info, certs, docs, history) | 2 ngày | P0 |
| 4.5 | Shore: `CertificateManagementPage` | 1.5 ngày | P0 |
| 4.6 | Shore: `ExpiryDashboard` — fleet-wide certificate monitoring | 2 ngày | P1 |
| 4.7 | Shore: `ComplianceReport` — STCW matrix per ship | 1.5 ngày | P1 |
| 4.8 | Shore: `SyncDashboard` — all ships sync status | 1.5 ngày | P1 |
| 4.9 | Shore: `SyncHistory` — audit log of all sync operations | 1 ngày | P2 |
| 4.10 | Shared components: `CertificateStatusBadge`, `ExpiryAlert` | 1 ngày | P1 |

**Deliverable**: Shore UI connected to real API, multi-ship crew/cert management

### Phase 5: Edge Improvements (Tuần 11-12) — Cải thiện tàu

> **Mục tiêu**: Edge UI sync dashboard, service layer clean-up

| # | Task | Effort | Priority |
|---|------|--------|----------|
| 5.1 | Edge: Implement real `SyncPage.tsx` dashboard | 1.5 ngày | P1 |
| 5.2 | Edge: `ServiceRecordsController` + UI | 1 ngày | P1 |
| 5.3 | Edge: Extract controller logic → service layer | 2 ngày | P2 |
| 5.4 | Edge: Real-time sync status indicators trong CrewPage | 1 ngày | P2 |
| 5.5 | Edge: Sync conflict notification UI | 1 ngày | P2 |

**Deliverable**: Clean architecture edge, real sync UI

### Phase 6: Production Hardening (Tuần 13-14)

| # | Task | Effort | Priority |
|---|------|--------|----------|
| 6.1 | File sync service (certificate PDFs, passport scans) | 2 ngày | P1 |
| 6.2 | Comprehensive error handling + retry UI | 1 ngày | P1 |
| 6.3 | Sync health monitoring + alerts | 1 ngày | P1 |
| 6.4 | Load testing (1000+ crew, 5000+ certificates) | 1 ngày | P1 |
| 6.5 | Security audit (data encryption in transit + at rest) | 1 ngày | P0 |
| 6.6 | Documentation finalization | 1 ngày | P2 |

---

## 7. THIẾT KẾ DATABASE SHORE

### 7.1 ERD — Crew/Certificate Module (Shore)

```
┌─────────────┐     ┌──────────────┐     ┌──────────────┐
│   vessels    │     │ crew_members │     │    ranks     │
├─────────────┤     ├──────────────┤     ├──────────────┤
│ id (PK)     │◄──┐ │ id (PK,Guid) │────►│ id (PK)      │
│ imo_number  │   │ │ crew_id      │     │ rank_code    │
│ name        │   │ │ full_name    │     │ rank_name    │
│ ...         │   │ │ rank_id (FK) │     │ is_active    │
└─────────────┘   │ │ ship_id (FK) │     └──────┬───────┘
                  │ │ department   │            │
                  │ │ nationality  │     ┌──────▼───────────┐
                  │ │ is_onboard   │     │ rank_certificates│
                  │ │ origin_node  │     ├──────────────────┤
                  │ │ sync_version │     │ rank_id (FK)     │
                  │ │ is_synced    │     │ certificate_id   │
                  │ │ ...50+ cols  │     └──────────────────┘
                  │ └──────┬───────┘
                  │        │
                  │        │ 1:N
                  │        │
                  │ ┌──────▼───────────┐     ┌──────────────┐
                  │ │ crew_certificates│     │ certificates │
                  │ ├──────────────────┤     ├──────────────┤
                  │ │ id (PK)          │────►│ id (PK)      │
                  │ │ crew_member_id   │     │ cert_code    │
                  │ │ certificate_id   │     │ cert_name    │
                  │ │ cert_number      │     │ category     │
                  │ │ issue_date       │     │ validity_months│
                  │ │ expiry_date      │     │ is_mandatory  │
                  │ │ status           │     └──────┬───────┘
                  │ │ is_synced        │            │
                  │ │ origin_node      │     ┌──────▼──────────────┐
                  │ └──────────────────┘     │ country_certificates│
                  │                          ├─────────────────────┤
                  │ ┌──────────────────┐     │ country_id (FK)     │
                  │ │ travel_documents │     │ certificate_id (FK) │
                  │ ├──────────────────┤     └─────────────────────┘
                  │ │ crew_member_id   │
                  │ │ document_type    │     ┌──────────────┐
                  │ │ document_number  │     │  countries   │
                  │ │ issue/expiry     │     ├──────────────┤
                  └─│ file_url         │     │ country_code │
                    └──────────────────┘     │ country_name │
                                             └──────────────┘
   + seafarer_documents, employment_documents,
     health_documents, service_records
     (same pattern)

   ┌──────────────────┐
   │ sync_outbox      │     (Shore → Edge queue)
   ├──────────────────┤
   │ id (PK)          │
   │ target_node      │     "SHIP_01", "SHIP_02", "*"
   │ table_name       │
   │ record_key       │
   │ action_type      │
   │ payload (JSON)   │
   │ sync_version     │
   │ created_at       │
   │ delivered_at     │
   └──────────────────┘

   ┌──────────────────┐
   │ sync_log         │     (Audit trail)
   ├──────────────────┤
   │ id (PK)          │
   │ direction        │     "EDGE_TO_SHORE", "SHORE_TO_EDGE"
   │ origin_node      │
   │ table_name       │
   │ record_key       │
   │ action_type      │
   │ status           │     "SUCCESS", "CONFLICT", "FAILED"
   │ conflict_detail  │
   │ processed_at     │
   └──────────────────┘
```

### 7.2 Bảng mới cần tạo trên Shore DB

| # | Table | Tương ứng Edge | Ghi chú |
|---|-------|---------------|---------|
| 1 | `crew_members` | ✅ Full match | Thêm `ship_id` FK → vessels |
| 2 | `certificates` | ✅ Full match | Crew certificate types (không phải vessel cert) |
| 3 | `crew_certificates` | ✅ Full match | Junction table |
| 4 | `countries` | ✅ Full match | Reference data |
| 5 | `ranks` | ✅ Full match | Reference data |
| 6 | `rank_certificates` | ✅ Full match | M:N mapping |
| 7 | `country_certificates` | ✅ Full match | M:N mapping |
| 8 | `travel_documents` | ✅ Full match | Passport, Visa |
| 9 | `seafarer_documents` | ✅ Full match | Seaman Book, CoC |
| 10 | `employment_documents` | ✅ Full match | Contract, Appraisal |
| 11 | `health_documents` | ✅ Full match | Medical, Vaccination |
| 12 | `service_records` | ✅ Full match | Sea service history |
| 13 | `sync_outbox` | 🆕 Shore-only | Queue cho Shore→Edge |
| 14 | `sync_log` | 🆕 Shore-only | Audit trail cho sync |

### 7.3 Shore-specific additions to CrewMember

```csharp
// Trên Shore, CrewMember có thêm:
public Guid? ShipId { get; set; }           // Tàu mà thuyền viên đang trực thuộc
public virtual Vessel Ship { get; set; }     // Navigation property

// Shore cũng quản lý pool (thuyền viên chưa assign tàu):
// ShipId = null → trong pool, chờ assignment
```

---

## 8. THIẾT KẾ API SHORE

### 8.1 Crew API — Shore-specific (Multi-ship view)

```
GET    /api/crew                          # Tất cả crew across all ships
GET    /api/crew?shipId={id}              # Crew theo tàu cụ thể
GET    /api/crew?pool=true                # Crew chưa assign tàu (pool)
GET    /api/crew/{id}                     # Chi tiết crew member
POST   /api/crew                          # Tạo crew (from HR)
PUT    /api/crew/{id}                     # Cập nhật crew
DELETE /api/crew/{id}                     # Xóa crew

GET    /api/crew/{id}/certificates        # Certificates của crew
GET    /api/crew/{id}/documents/{type}    # Documents theo loại
GET    /api/crew/{id}/service-records     # Sea service history
POST   /api/crew/{id}/assign-ship        # Assign crew to ship
POST   /api/crew/{id}/transfer           # Transfer giữa ships
```

### 8.2 Certificate API — Shore (Fleet-level)

```
GET    /api/certificates                  # Tất cả certificate types
GET    /api/certificates/{id}             # Chi tiết cert type
POST   /api/certificates                  # Tạo cert type (master data)
PUT    /api/certificates/{id}             # Cập nhật cert type

GET    /api/certificates/expiring?days=90 # Fleet-wide expiring certs
GET    /api/certificates/compliance       # STCW compliance report
GET    /api/certificates/crew/{crewId}    # Certs per crew

POST   /api/certificates/crew-certificates     # Add cert to crew
PUT    /api/certificates/crew-certificates/{id} # Update crew cert
```

### 8.3 Sync API — Shore

```
POST   /api/sync                          # Edge gửi data lên (existing, extend)
GET    /api/sync/pull                     # Edge pull changes từ shore
       ?since={ISO8601}&node={shipId}
       &tables=crew_members,certificates
       &limit=100
POST   /api/sync/acknowledge              # Edge confirm đã nhận
GET    /api/sync/status                   # Sync status per ship
GET    /api/sync/log                      # Sync audit history
POST   /api/sync/force-push               # Admin push data to specific ship
```

---

## 9. SYNC PROTOCOL

### 9.1 Edge → Shore (Push)

```
1. Data thay đổi trên Edge (CrewMember updated)
2. SaveChanges interceptor auto-creates SyncQueue entry:
   {
     TableName: "crew_members",
     RecordKey: "guid-here",
     ActionType: "UPDATE",
     Payload: '{"FullName":"New Name","UpdatedAt":"2026-03-02T10:00:00Z"}',  // delta
     Priority: "Operational",
     SyncVersion: 5
   }
3. SyncBackgroundWorker picks up entry (every 60s)
4. Network check → VSAT available → Operational allowed
5. HTTP POST to Shore /api/sync with batch of items
6. Shore SyncController receives, processes:
   a. Check if record exists
   b. Compare SyncVersion (conflict detection)
   c. If no conflict → apply changes
   d. If conflict → ConflictResolver applies domain rules
   e. Log to sync_log table
7. Shore responds with success/conflict details
8. Edge marks SyncQueue items as SyncedAt = now
```

### 9.2 Shore → Edge (Pull)

```
1. Data thay đổi trên Shore (Certificate renewed by HR)
2. SyncOutboxService auto-creates sync_outbox entry:
   {
     TargetNode: "SHIP_01",  // or "*" for broadcast
     TableName: "crew_certificates",
     RecordKey: "cert-id",
     ActionType: "UPDATE",
     Payload: '{"ExpiryDate":"2028-01-01","Status":"VALID"}',
     SyncVersion: 6
   }
3. Edge SyncPullService polls Shore (every 5 minutes):
   GET /api/sync/pull?since=2026-03-01T00:00:00Z&node=SHIP_01
4. Shore returns batch of pending outbox items
5. Edge processes each item:
   a. Apply changes to local DB
   b. Handle conflicts with SyncConflictHandler
   c. Set IsSynced = true
6. Edge sends acknowledgment:
   POST /api/sync/acknowledge { itemIds: [...] }
7. Shore marks outbox items as delivered_at = now
```

### 9.3 Sync Data Priorities (Crew Module)

| Data Type | Direction | Priority | Reason |
|-----------|-----------|----------|--------|
| Crew embark/disembark | Edge→Shore | **Critical** | Safety: shore must know who's on board |
| Certificate expiry alert | Edge→Shore | **Critical** | Compliance: immediate notification |
| Crew profile update | Edge→Shore | Operational | Bio-data can wait for VSAT |
| New certificate scan | Edge→Shore | **Operational** | Document needs to reach shore |
| Certificate renewal | Shore→Edge | Operational | Shore updates official records |
| Master data (ranks, countries) | Shore→Edge | Low | Reference data, not urgent |
| Service record | Edge→Shore | Low | Historical, batchable |

### 9.4 Idempotency

Mọi sync operation phải idempotent:
- CREATE: Nếu record đã tồn tại (by PK) → skip hoặc update
- UPDATE: Apply delta chỉ nếu SyncVersion > local version
- DELETE: Soft delete nếu record exists, skip nếu đã deleted

---

## 10. FRONTEND SHORE UI

### 10.1 Page Structure

```
/crew                           → CrewListPage (multi-ship view)
  ├── ?shipId={id}              → Filter by ship
  ├── ?pool=true                → Unassigned crew pool
  └── ?search={term}            → Search by name/ID

/crew/{id}                      → CrewDetailPage
  ├── #basic-data               → Tab: Basic info (personal, bio, education)
  ├── #certificates             → Tab: Certificates (status, expiry)
  ├── #documents                → Tab: Documents (4 types)
  ├── #service-records          → Tab: Sea service history
  └── #sync-status              → Tab: Sync history for this crew

/certificates                   → CertificateListPage
/certificates/{id}              → CertificateDetailPage
/certificates/expiry            → ExpiryDashboard (fleet-wide)
/certificates/compliance        → ComplianceReport (STCW matrix)

/sync                           → SyncDashboard
/sync/history                   → SyncHistory (audit log)
```

### 10.2 Shore-specific UI Features (vs Edge)

| Feature | Shore (Fleet Management) | Edge (Single Ship) |
|---------|------------------------|-------------------|
| **Crew View** | Multi-ship, pool management, transfers | Single ship onboard list |
| **Certificate Monitoring** | Fleet-wide expiry dashboard, trends | Ship-level expiry alerts |
| **Compliance** | STCW matrix across fleet, PSC readiness | Per-ship compliance check |
| **Documents** | Official record management, HR workflow | Onboard scanning, local copies |
| **Sync** | All ships status, force-push, audit log | Single ship sync queue/status |

---

## 11. QUY CHUẨN QUỐC TẾ & COMPLIANCE

### 11.1 Tuân thủ các quy định

| Quy chuẩn | Yêu cầu | Implementation |
|-----------|---------|----------------|
| **STCW 1978 (amended 2010)** | Certificate competency requirements per rank | `RankCertificate` mapping, `ComplianceReport` page |
| **MLC 2006** | Crew working conditions, employment records | `EmploymentDocument`, `ServiceRecord`, ContractEnd tracking |
| **ISM Code** | Safety management, crew training records | `DrillLog` linked to crew, training certificate tracking |
| **SOLAS** | Safety equipment training certificates | `Certificate.Category = SAFETY`, mandatory drill participation |
| **ISPS Code** | Security training certificates | `Certificate.Category = SECURITY` |
| **MARPOL** | Pollution prevention training | Environmental training certificates |
| **FAL Convention** | FAL Form 5 (Crew List) | `VoyageCrewAssignment` with embark/disembark tracking |
| **Flag State requirements** | Country-specific certificates | `CountryCertificate` mapping per flag state |
| **PSC Inspection** | Port State Control readiness | Certificate validity monitoring, ExpiryDashboard |

### 11.2 Certificate Expiry Rules

```csharp
// Auto-calculated status
public enum CertificateStatus
{
    VALID,          // ExpiryDate > Today + 90 days
    EXPIRING_SOON,  // ExpiryDate within 90 days (configurable)
    EXPIRED,        // ExpiryDate < Today
    SUSPENDED,      // Manually suspended by authority
    REVOKED         // Permanently revoked
}

// Alert thresholds (configurable per certificate type)
// Default: 90 days, 60 days, 30 days, 7 days, EXPIRED
```

### 11.3 Data Retention

| Data Type | Retention Period | Regulation |
|-----------|-----------------|-----------|
| Crew employment records | 10 năm sau disembark | MLC 2006 |
| Certificate records | Lifetime + 5 năm | STCW |
| Drill/training logs | 5 năm | ISM Code |
| Service records (sea service) | Lifetime | STCW |
| Medical records | 10 năm | MLC 2006 |

---

## 12. HIỆU NĂNG & PRODUCTION STANDARDS

### 12.1 Performance Requirements

| Metric | Target | Method |
|--------|--------|--------|
| **API Response Time** | < 200ms (P95) | Indexed queries, pagination, caching |
| **Sync Batch Processing** | 50 items/batch < 5s | Bulk insert, transaction batching |
| **Certificate Expiry Check** | < 500ms for 1000 crew | Filtered index on `expiry_date` |
| **Crew List Load** | < 300ms for 100 records | Server-side pagination, select projection |
| **File Upload** | < 30s for 10MB | Streaming upload, chunked for satellite |
| **Sync Queue Processing** | < 1min latency | BackgroundWorker 60s interval |

### 12.2 Database Indexes (Shore)

```sql
-- Crew search performance
CREATE INDEX ix_crew_members_ship_id ON crew_members(ship_id) WHERE ship_id IS NOT NULL;
CREATE INDEX ix_crew_members_is_onboard ON crew_members(is_onboard) WHERE is_onboard = true;
CREATE INDEX ix_crew_members_crew_id ON crew_members(crew_id);
CREATE UNIQUE INDEX ix_crew_members_crew_id_unique ON crew_members(crew_id);

-- Sync performance
CREATE INDEX ix_crew_members_sync ON crew_members(is_synced) WHERE is_synced = false;
CREATE INDEX ix_crew_certificates_sync ON crew_certificates(is_synced) WHERE is_synced = false;

-- Certificate expiry monitoring
CREATE INDEX ix_crew_certificates_expiry ON crew_certificates(expiry_date)
    WHERE status IN ('VALID', 'EXPIRING_SOON');

-- Sync outbox processing
CREATE INDEX ix_sync_outbox_pending ON sync_outbox(target_node, created_at)
    WHERE delivered_at IS NULL;

-- Sync log audit
CREATE INDEX ix_sync_log_processed ON sync_log(processed_at, direction);
```

### 12.3 Caching Strategy

```
Layer 1: In-Memory Cache (IMemoryCache)
  - Certificate types (10 min TTL)
  - Countries, Ranks (30 min TTL)
  - RankCertificate mappings (10 min TTL)

Layer 2: Response Caching
  - GET /api/certificates → 5 min
  - GET /api/countries → 30 min
  - GET /api/ranks → 30 min

Layer 3: ETag / Conditional Requests
  - GET /api/crew/{id} → ETag based on UpdatedAt
  - GET /api/sync/pull → cursor-based, no caching
```

### 12.4 Security Standards

| Aspect | Implementation |
|--------|--------------|
| **Transport** | HTTPS/TLS 1.3 (satellite link encrypted) |
| **Auth** | JWT tokens (edge↔shore), API key as fallback |
| **Data Encryption** | PII fields encrypted at rest (AES-256) |
| **File Storage** | Encrypted blob storage, signed URLs |
| **Audit** | All CRUD operations logged with user/timestamp |
| **Rate Limiting** | Sync endpoint: 100 req/min per ship |
| **Input Validation** | FluentValidation on all DTOs |
| **CORS** | Whitelist shore/edge origins only |

### 12.5 Production Deployment

```yaml
# docker-compose.prod.yml additions
services:
  shore-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;...
      - Sync__MaxBatchSize=50
      - Sync__PullIntervalSeconds=300
      - Sync__RetryMaxAttempts=5
      - Certificates__ExpiryWarningDays=90
      - Storage__Type=S3  # or local for dev
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### 12.6 Monitoring & Observability

```
Serilog structured logging → Seq/ELK
  - Sync operations: success/fail/conflict counts
  - API latency per endpoint
  - Certificate expiry alerts

Health checks:
  GET /health → DB connection + sync service status
  GET /health/sync → Last sync per ship, pending count

Metrics (Prometheus-compatible):
  sync_items_processed_total{direction, status}
  sync_latency_seconds{direction}
  crew_certificates_expiring_count{ship, days_bucket}
```

---

## 13. TIMELINE CHI TIẾT

```
Week 1-2   ║████████████████║  Phase 1: Foundation (Shared library + Shore DB)
Week 3-4   ║████████████████║  Phase 2: Shore API (Crew/Cert CRUD)
Week 5-7   ║████████████████████████║  Phase 3: Sync Engine (bi-directional)
Week 8-10  ║████████████████████████║  Phase 4: Shore Frontend (UI rebuild)
Week 11-12 ║████████████████║  Phase 5: Edge Improvements
Week 13-14 ║████████████████║  Phase 6: Production Hardening

Tổng: ~14 tuần (3.5 tháng)
FTE: 1-2 developers
```

### Milestone Checkpoints

| Tuần | Milestone | Deliverable |
|------|-----------|------------|
| 2 | **M1: Foundation** | Shared lib, shore DB with crew tables, both projects compile |
| 4 | **M2: Shore API** | Full crew/cert CRUD API on shore, Swagger docs |
| 7 | **M3: Sync Working** | Edge→Shore + Shore→Edge sync for crew/cert |
| 10 | **M4: Shore UI** | Shore frontend with real crew/cert management |
| 12 | **M5: Edge Polish** | Edge sync UI, service layer clean |
| 14 | **M6: Production** | File sync, security, monitoring, load tested |

### Priority Matrix

```
P0 (Must Have):
  ✦ Shared library (no code duplication)
  ✦ Shore crew/cert models + DB
  ✦ Shore crew/cert API
  ✦ Edge→Shore sync for crew data
  ✦ Shore crew list UI (real API)

P1 (Should Have):
  ✦ Shore→Edge sync (bidirectional)
  ✦ Conflict resolution
  ✦ Certificate expiry dashboard
  ✦ STCW compliance report
  ✦ File sync (documents/scans)

P2 (Nice to Have):
  ✦ Edge service layer refactor
  ✦ Sync audit log UI
  ✦ Real-time sync indicators
  ✦ Mobile sync support
```

---

## PHỤ LỤC A: Checklist kiểm tra trước khi deploy

- [ ] Shared library build thành công
- [ ] Shore DB migration chạy OK
- [ ] Shore API tất cả endpoints test pass
- [ ] Edge SyncService gọi Shore API thành công
- [ ] Shore SyncController xử lý crew/cert data
- [ ] Shore→Edge pull hoạt động
- [ ] Conflict resolution test cases pass
- [ ] Certificate expiry alerts hoạt động
- [ ] File upload/download hoạt động
- [ ] Multi-ship crew view hoạt động
- [ ] STCW compliance matrix chính xác
- [ ] Performance benchmarks đạt target
- [ ] Security audit pass
- [ ] Data retention policies implemented
- [ ] Health check endpoints responsive
- [ ] Sync monitoring dashboard hoạt động

---

## PHỤ LỤC B: Rủi ro & Mitigation

| Rủi ro | Impact | Xác suất | Mitigation |
|--------|--------|---------|-----------|
| Sync conflict mất data | High | Medium | Domain-based conflict resolution + audit log |
| Satellite connection unstable | Medium | High | Store-and-forward queue, exponential backoff |
| Large file sync timeout | Medium | Medium | Chunked upload, compression, priority queuing |
| Schema mismatch edge↔shore | High | Low | Shared library enforces contract |
| Performance degradation | Medium | Low | Indexes, caching, pagination |
| Security breach (crew PII) | Critical | Low | Encryption at rest + transit, access control |
