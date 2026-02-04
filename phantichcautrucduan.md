# 🚢 PHÂN TÍCH CẤU TRÚC DỰ ÁN MARITIME MANAGEMENT SYSTEM

## 📋 Mục Lục
1. [Tổng Quan Dự Án](#tổng-quan-dự-án)
2. [Kiến Trúc Hệ Thống](#kiến-trúc-hệ-thống)
3. [Cấu Trúc Thư Mục Chi Tiết](#cấu-trúc-thư-mục-chi-tiết)
4. [Công Nghệ Sử Dụng](#công-nghệ-sử-dụng)
5. [Module và Chức Năng](#module-và-chức-năng)
6. [Luồng Dữ Liệu](#luồng-dữ-liệu)
7. [Cơ Sở Dữ Liệu](#cơ-sở-dữ-liệu)
8. [Tích Hợp và Đồng Bộ](#tích-hợp-và-đồng-bộ)

---

## 🎯 Tổng Quan Dự Án

**Maritime Management System** là hệ thống quản lý hạm đội tàu biển toàn diện, tuân thủ tiêu chuẩn quốc tế IMO (International Maritime Organization). Hệ thống được thiết kế theo kiến trúc **Edge-Shore** để hoạt động hiệu quả cả trong môi trường có và không có kết nối internet.

### Đặc Điểm Chính
- **Kiến trúc phân tán**: Edge (trên tàu) và Shore (trên bờ)
- **Đồng bộ dữ liệu tự động**: Hỗ trợ làm việc offline và sync khi có kết nối
- **Multi-platform**: Web (Desktop/Tablet), Mobile (iOS/Android)
- **Real-time monitoring**: Theo dõi tàu và thiết bị thời gian thực
- **Quản lý đầy đủ**: PMS, Crew, Fuel, Certificates, Compliance, Telemetry

---

## 🏗️ Kiến Trúc Hệ Thống

### Kiến Trúc Edge-Shore

```
┌─────────────────────────────────────────────────────────────┐
│                      SHORE SYSTEM (Trên bờ)                  │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐    │
│  │  Frontend   │  │   Backend   │  │    PostgreSQL     │    │
│  │   (React)   │◄─┤  (.NET 8)   │◄─┤   (Shore DB)      │    │
│  └─────────────┘  └─────────────┘  └──────────────────┘    │
│         ▲               ▲                                     │
└─────────┼───────────────┼─────────────────────────────────────┘
          │               │
   ╔══════╧═══════════════╧═════════════════════════════════╗
   ║              Internet / Satellite Link                  ║
   ╚══════╤═══════════════╤═════════════════════════════════╝
          │               │
┌─────────┼───────────────┼─────────────────────────────────────┐
│         ▼               ▼           EDGE SYSTEM (Trên tàu)     │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐      │
│  │ Edge Web UI │  │ Edge API    │  │   PostgreSQL      │      │
│  │  (React)    │◄─┤ (.NET 8)    │◄─┤   (Edge DB)       │      │
│  └─────────────┘  └─────────────┘  └──────────────────┘      │
│         ▲               ▲                    ▲                 │
│         │               │                    │                 │
│  ┌─────────────┐  ┌────┴──────┐      ┌─────┴────────┐       │
│  │   Mobile    │  │   Redis   │      │   MQTT       │       │
│  │   (Flutter) │  │  (Cache)  │      │  (Sensors)   │       │
│  └─────────────┘  └───────────┘      └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
```

### Các Hệ Thống Con

#### 1. **SHORE SYSTEM** (Hệ thống trên bờ)
- **Backend API**: ASP.NET Core 8.0, RESTful API
- **Frontend Dashboard**: React 19 + Vite + TypeScript
- **Database**: PostgreSQL (Shore Database)
- **Cache**: Redis (Session & Data Cache)
- **Purpose**: Quản lý tập trung, báo cáo, phân tích dữ liệu từ nhiều tàu

#### 2. **EDGE SYSTEM** (Hệ thống trên tàu)
- **Edge API**: ASP.NET Core 8.0 (Autonomous operation)
- **Edge Frontend**: React + Vite (Optimized for ship environment)
- **Mobile App**: Flutter (Crew member tasks & schedules)
- **Edge Database**: PostgreSQL (Local data storage)
- **Sync Service**: Background service đồng bộ với Shore
- **MQTT Broker**: Thu thập dữ liệu từ cảm biến tàu
- **Purpose**: Hoạt động độc lập khi mất kết nối, đồng bộ khi có internet

---

## 📁 Cấu Trúc Thư Mục Chi Tiết

```
Martime_product_2.0/
│
├── 📦 backend/                          # SHORE Backend API
│   ├── Controllers/                     # API Controllers
│   │   ├── ShipsController.cs          # Quản lý tàu
│   │   ├── VesselsController.cs        # Chi tiết vessel
│   │   ├── SyncController.cs           # Đồng bộ dữ liệu
│   │   └── VesselTelemetryController.cs # Telemetry data
│   ├── DTOs/                           # Data Transfer Objects
│   ├── Models/                         # Domain Models
│   │   ├── Ship.cs
│   │   ├── CrewMember.cs
│   │   ├── MaintenanceTask.cs
│   │   ├── User.cs
│   │   └── SyncModels.cs
│   ├── Data/                           # DbContext & Configurations
│   ├── Repositories/                   # Data Access Layer
│   ├── Services/                       # Business Logic
│   ├── Migrations/                     # EF Core Migrations
│   ├── Program.cs                      # Application Entry Point
│   ├── appsettings.json               # Configuration
│   └── product-api.csproj             # Project File
│
├── 🌐 frontend/                         # SHORE Web Dashboard
│   ├── src/
│   │   ├── pages/                      # Page Components
│   │   │   ├── Dashboard/              # Main dashboard
│   │   │   ├── CrewManagement/         # Crew management
│   │   │   ├── CategoryManagement/     # Categories
│   │   │   ├── PMS/                    # Planned Maintenance
│   │   │   └── WorkAssignment/         # Work assignments
│   │   ├── components/
│   │   │   ├── ui/                     # UI Components (Radix UI)
│   │   │   │   ├── button.tsx
│   │   │   │   ├── card.tsx
│   │   │   │   ├── input.tsx
│   │   │   │   └── badge.tsx
│   │   │   └── common/                 # Common components
│   │   │       ├── Button/
│   │   │       ├── Input/
│   │   │       └── StatusBadge/
│   │   ├── services/                   # API Services
│   │   │   ├── api.ts                 # API Client
│   │   │   └── pms.service.ts         # PMS specific
│   │   ├── types/                      # TypeScript Types
│   │   │   ├── crew.types.ts
│   │   │   ├── pms.types.ts
│   │   │   ├── work.types.ts
│   │   │   └── dashboard.types.ts
│   │   └── utils/                      # Utility functions
│   ├── public/                         # Static assets
│   ├── package.json                    # Dependencies
│   ├── vite.config.ts                 # Vite configuration
│   └── tsconfig.json                  # TypeScript config
│
├── 🚢 edge-services/                    # EDGE Backend API (On Ship)
│   ├── Controllers/                    # Edge API Controllers
│   │   ├── AuthController.cs          # Authentication
│   │   ├── CrewController.cs          # Crew management
│   │   ├── MaintenanceController.cs    # Maintenance tasks
│   │   ├── MaintenanceScheduleController.cs
│   │   ├── EquipmentAssetController.cs # Equipment management
│   │   ├── EquipmentGroupController.cs
│   │   ├── MaterialController.cs       # Spare parts
│   │   ├── AlarmsController.cs        # Alarm system
│   │   ├── CertificatesController.cs   # Certificate management
│   │   ├── TelemetryController.cs     # Sensor data
│   │   ├── FuelAnalyticsController.cs  # Fuel monitoring
│   │   ├── VoyageController.cs        # Voyage tracking
│   │   ├── VoyageLogController.cs     # Voyage logs
│   │   ├── WatchkeepingController.cs  # Watch schedules
│   │   ├── TaskWorkflowController.cs   # Task workflows
│   │   ├── DeferralRequestController.cs # Deferral requests
│   │   ├── ReportingController.cs      # Reports
│   │   ├── SyncController.cs          # Sync with shore
│   │   └── Logbooks/                  # Logbook controllers
│   ├── Models/                         # Domain Models
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Services/                       # Business Logic
│   ├── Repositories/                   # Data Access Layer
│   ├── Data/                          # DbContext
│   ├── Constants/                      # Constants & Enums
│   ├── Scripts/                        # Database scripts
│   ├── init-scripts/                   # Initialization scripts
│   ├── appsettings.json               # Configuration
│   └── EdgeCollector.csproj           # Project file
│
├── 🌊 frontend-edge/                    # EDGE Web UI (On Ship)
│   ├── src/
│   │   ├── pages/                      # Page components
│   │   ├── components/                 # UI components
│   │   ├── services/                   # API services
│   │   ├── types/                      # TypeScript types
│   │   └── utils/                      # Utilities
│   ├── package.json                    # Dependencies
│   └── vite.config.ts                 # Vite config
│
├── 📱 frontend-mobile/                  # MOBILE App (Flutter)
│   ├── lib/
│   │   ├── core/                       # Core functionality
│   │   │   ├── network/               # HTTP client
│   │   │   ├── auth/                  # Authentication
│   │   │   ├── storage/               # Local storage
│   │   │   ├── cache/                 # Cache management
│   │   │   ├── theme/                 # App theming
│   │   │   ├── di/                    # Dependency Injection
│   │   │   ├── localization/          # i18n
│   │   │   └── constants/             # Constants
│   │   ├── data/                      # Data layer
│   │   │   ├── models/                # Data models
│   │   │   ├── repositories/          # Repositories
│   │   │   ├── data_sources/          # API data sources
│   │   │   │   └── remote/            # Remote API calls
│   │   │   └── api/                   # API definitions
│   │   ├── presentation/              # Presentation layer
│   │   │   ├── screens/               # App screens
│   │   │   │   ├── auth/              # Login/Register
│   │   │   │   ├── home/              # Home screen
│   │   │   │   ├── tasks/             # Task management
│   │   │   │   ├── schedule/          # Schedule view
│   │   │   │   ├── alarms/            # Alarms
│   │   │   │   ├── profile/           # User profile
│   │   │   │   └── settings/          # Settings
│   │   │   ├── widgets/               # Reusable widgets
│   │   │   │   └── common/            # Common widgets
│   │   │   └── providers/             # State providers
│   │   ├── providers/                 # Global providers
│   │   └── l10n/                      # Localization files
│   ├── android/                        # Android configuration
│   ├── ios/                           # iOS configuration
│   ├── windows/                        # Windows configuration
│   ├── test/                          # Unit tests
│   ├── pubspec.yaml                   # Flutter dependencies
│   └── analysis_options.yaml          # Dart analyzer config
│
├── 🐳 Docker & Infrastructure
│   ├── docker-compose.yml             # Docker services orchestration
│   ├── .env                           # Environment variables
│   ├── .env.example                   # Environment template
│   └── init-scripts/                  # Database init scripts
│
├── 📚 Documentation (28 markdown files)
│   ├── README.md                      # Main documentation
│   ├── QUICK_START_GUIDE.md          # Quick start
│   ├── TEAM_WORKFLOW.md              # Team workflow
│   ├── PMS_IMPLEMENTATION_COMPLETE.md # PMS docs
│   ├── MOBILE_PMS_IMPLEMENTATION_PLAN_V2.md
│   ├── CERTIFICATE_OF_COMPETENCY_IMPLEMENTATION.md
│   ├── DELTA_SYNC_IMPLEMENTATION.md   # Sync mechanism
│   ├── EQUIPMENT_GROUPS_CRUD_TEST.md
│   ├── VOYAGE_LOG_IMPLEMENTATION_PLAN.md
│   ├── WATCHKEEPING_USER_GUIDE.md
│   ├── PERFORMANCE_OPTIMIZATION_SUMMARY.md
│   └── ... (many more technical docs)
│
└── 🛠️ Utilities & Scripts
    ├── cleanup-git.ps1                # Git cleanup
    ├── cleanup-old-files.ps1         # File cleanup
    ├── convert-with-mermaid.js       # Diagram conversion
    ├── product.sln                   # Visual Studio solution
    ├── rules.md                      # Development rules
    └── sqlseed.sql                   # Database seed data
```

---

## 🔧 Công Nghệ Sử Dụng

### Backend (.NET 8.0)
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 15
- **Cache**: Redis 7
- **Authentication**: JWT Bearer
- **API Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Packages**:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
  - `Microsoft.EntityFrameworkCore.Design` - EF Tools
  - `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT Auth
  - `Microsoft.Extensions.Caching.StackExchangeRedis` - Redis cache

### Frontend (React)
- **Framework**: React 19.1.1
- **Build Tool**: Vite (với Rolldown)
- **Language**: TypeScript 5.9.3
- **Routing**: React Router DOM 7.9.3
- **UI Library**: 
  - Radix UI (Headless components)
  - Tailwind CSS 3.x (Styling)
  - Lucide React (Icons)
- **State Management**: React Context/Hooks
- **Package Manager**: npm

### Mobile (Flutter)
- **Framework**: Flutter 3.0+
- **Language**: Dart 3.0+
- **State Management**: 
  - Provider 6.1.1
  - Riverpod 3.0.3
- **HTTP Client**: Dio 5.4.0 + Retrofit 4.0.3
- **Local Storage**: 
  - Hive 2.2.3 (NoSQL database)
  - Shared Preferences 2.2.2
  - Flutter Secure Storage 9.0.0
- **Authentication**: JWT Decoder 2.0.1
- **UI**: 
  - Google Fonts 6.1.0
  - Cached Network Image 3.3.0
  - Shimmer 3.0.0
  - Lottie 3.3.2
- **Localization**: intl 0.20.2 + flutter_localizations

### Infrastructure
- **Containerization**: Docker + Docker Compose
- **Database**: PostgreSQL 15
- **Cache**: Redis 7
- **Database Admin**: pgAdmin 4
- **Message Broker**: Eclipse Mosquitto MQTT 2.0
- **Reverse Proxy**: (To be configured)

---

## 🧩 Module và Chức Năng

### 1. **Authentication & Authorization**
- **Location**: `edge-services/Controllers/AuthController.cs`
- **Features**:
  - JWT-based authentication
  - Role-based authorization (Admin, Captain, Engineer, Crew)
  - User registration & login
  - Password reset
  - Token refresh

### 2. **Crew Management**
- **Location**: `edge-services/Controllers/CrewController.cs`
- **Features**:
  - Crew member CRUD
  - Rank and position management
  - Department assignment
  - Certificate tracking
  - Work hour logging

### 3. **Planned Maintenance System (PMS)**
- **Controllers**:
  - `MaintenanceController.cs` - Task management
  - `MaintenanceScheduleController.cs` - Scheduling
  - `TaskWorkflowController.cs` - Workflow management
- **Features**:
  - Maintenance task creation & assignment
  - Schedule generation (Auto/Manual)
  - Task checklist management
  - Work order processing
  - Spare parts integration
  - Deferral requests
  - Compliance tracking

### 4. **Equipment Management**
- **Controllers**:
  - `EquipmentAssetController.cs`
  - `EquipmentGroupController.cs`
- **Features**:
  - Equipment hierarchy (Groups → Assets)
  - Equipment status tracking
  - Running hours monitoring
  - Maintenance history
  - Auto-update status based on tasks

### 5. **Material Management (Spare Parts)**
- **Location**: `MaterialController.cs`
- **Features**:
  - Inventory management
  - Material requisition
  - Receipt tracking
  - Stock level alerts
  - Excel import/export

### 6. **Certificate Management**
- **Controllers**:
  - `CertificatesController.cs`
  - `CountryCertificatesController.cs`
- **Features**:
  - Crew certificates (COC, COP, STCW)
  - Ship certificates
  - Expiry tracking
  - Renewal notifications
  - Compliance reporting

### 7. **Alarm System**
- **Location**: `AlarmsController.cs`
- **Features**:
  - Real-time alarm monitoring
  - Alarm acknowledgment
  - Alarm history
  - Severity levels
  - Push notifications (Mobile)

### 8. **Telemetry & Monitoring**
- **Controllers**:
  - `TelemetryController.cs`
  - `SignalKTestController.cs`
- **Features**:
  - Engine parameters (RPM, temperature, pressure)
  - Position tracking (GPS)
  - Speed & course
  - Environmental data
  - Sensor data collection via MQTT
  - SignalK protocol integration

### 9. **Fuel Analytics**
- **Location**: `FuelAnalyticsController.cs`
- **Features**:
  - Fuel consumption tracking
  - Efficiency analysis
  - ROB (Remaining On Board) monitoring
  - Bunkering records
  - Cost analysis

### 10. **Voyage Management**
- **Controllers**:
  - `VoyageController.cs`
  - `VoyageLogController.cs`
- **Features**:
  - Voyage planning
  - Port schedule
  - Voyage logs (Captain's log)
  - Arrival/Departure tracking

### 11. **Watchkeeping**
- **Location**: `WatchkeepingController.cs`
- **Features**:
  - Watch schedule management
  - Duty roster
  - Watch handover notes
  - Bridge/Engine room logs

### 12. **Logbooks**
- **Location**: `Controllers/Logbooks/`
- **Features**:
  - Engine room logbook
  - Deck logbook
  - Oil record book
  - Garbage record book

### 13. **Reporting & Analytics**
- **Controllers**:
  - `ReportingController.cs`
  - `AggregateReportController.cs`
  - `DashboardController.cs`
- **Features**:
  - Weekly/Monthly reports
  - Performance analytics
  - Compliance reports
  - KPI dashboard
  - Export to Excel/PDF

### 14. **Synchronization**
- **Location**: `SyncController.cs`
- **Features**:
  - Delta sync between Edge and Shore
  - Conflict resolution
  - Background sync service
  - Offline support
  - Data versioning

---

## 🔄 Luồng Dữ Liệu

### 1. Normal Operation Flow (Edge → Shore)

```
┌─────────────────┐
│  Mobile App     │
│  (Crew Member)  │
└────────┬────────┘
         │ 1. Task Update
         ▼
┌─────────────────┐       2. Save to        ┌──────────────┐
│  Edge API       │◄─────────────────────────│  Edge DB     │
│  (On Ship)      │                          │ (PostgreSQL) │
└────────┬────────┘                          └──────────────┘
         │ 3. Sync Queue
         │ (When internet available)
         ▼
┌─────────────────┐       4. Delta Sync     ┌──────────────┐
│  Shore API      │◄─────────────────────────│  Sync Queue  │
│  (On Land)      │                          │  (Redis)     │
└────────┬────────┘                          └──────────────┘
         │ 5. Store & Process
         ▼
┌─────────────────┐                          ┌──────────────┐
│  Shore DB       │                          │  Shore       │
│  (PostgreSQL)   │◄─────────────────────────│  Dashboard   │
└─────────────────┘      6. View Reports    └──────────────┘
```

### 2. Offline Operation Flow

```
┌─────────────────┐
│  Mobile App     │  ❌ No Internet
│  (Offline)      │
└────────┬────────┘
         │ 1. Local Operations
         ▼
┌─────────────────┐       2. Cache         ┌──────────────┐
│  Edge API       │◄───────────────────────│  Redis Cache │
│  (Autonomous)   │                        └──────────────┘
└────────┬────────┘
         │ 3. Store Locally
         ▼
┌─────────────────┐
│  Edge DB        │  📦 Data queued for sync
│  (Local)        │
└─────────────────┘
         │
         │ ⏰ When internet restored
         ▼
┌─────────────────┐
│  Shore Sync     │  ✅ Automatic sync
└─────────────────┘
```

### 3. Real-time Monitoring Flow

```
┌──────────────┐
│   Sensors    │  (Temperature, Pressure, etc.)
│  (Ship)      │
└──────┬───────┘
       │ 1. MQTT Publish
       ▼
┌──────────────┐       2. Subscribe        ┌──────────────┐
│ MQTT Broker  │◄────────────────────────────│ Edge Collector│
└──────┬───────┘                            └──────────────┘
       │ 3. Process & Store
       ▼
┌──────────────┐       4. Update UI        ┌──────────────┐
│  Edge API    │────────────────────────────▶│  Dashboard   │
└──────────────┘       (WebSocket)          └──────────────┘
       │
       │ 5. Sync to Shore
       ▼
┌──────────────┐
│  Shore API   │  📊 Historical Analysis
└──────────────┘
```

---

## 🗄️ Cơ Sở Dữ Liệu

### Database Architecture

Hệ thống sử dụng **2 PostgreSQL databases độc lập**:

#### 1. **Shore Database** (Trên bờ)
- **Purpose**: Central repository cho tất cả tàu trong hạm đội
- **Location**: Backend server (Cloud/On-premise)
- **Tables**: (Ước tính 50+ tables)
  - Ships/Vessels
  - Fleet management
  - Consolidated crew records
  - Aggregated telemetry
  - Cross-ship analytics
  - Shore-side users & roles

#### 2. **Edge Database** (Trên tàu - mỗi tàu có 1 DB)
- **Purpose**: Autonomous operation, offline-first
- **Location**: Edge server on ship
- **Main Table Groups**:
  
  **User & Authentication**
  - Users
  - Roles
  - Permissions
  - UserRoles
  
  **Crew Management**
  - CrewMembers
  - CrewPositions
  - CrewDepartments
  - CrewCertificates
  - CrewWorkHours
  
  **Equipment & Assets**
  - EquipmentGroups
  - EquipmentAssets
  - EquipmentMaintenance
  - EquipmentRunningHours
  
  **Maintenance (PMS)**
  - MaintenanceTasks
  - MaintenanceSchedules
  - TaskChecklists
  - ChecklistItems
  - TaskAssignments
  - TaskWorkOrders
  - DeferralRequests
  
  **Materials (Spare Parts)**
  - Materials
  - MaterialCategories
  - MaterialInventory
  - MaterialRequisitions
  - MaterialReceipts
  
  **Certificates**
  - ShipCertificates
  - CrewCertificates
  - CertificateTypes
  - CertificateAuthorities
  - Countries
  
  **Alarms & Monitoring**
  - Alarms
  - AlarmHistory
  - AlarmAcknowledgments
  
  **Telemetry & Sensors**
  - TelemetryData
  - SensorReadings
  - EngineParameters
  - PositionData
  
  **Fuel Management**
  - FuelConsumption
  - BunkeringRecords
  - FuelTypes
  - FuelAnalytics
  
  **Voyage**
  - Voyages
  - VoyageLogs
  - Ports
  - VoyageEvents
  
  **Watchkeeping**
  - WatchSchedules
  - WatchLogs
  - WatchHandovers
  
  **Logbooks**
  - EngineLogEntries
  - DeckLogEntries
  - OilRecordEntries
  - GarbageRecordEntries
  
  **Reporting**
  - Reports
  - ReportTemplates
  - ReportSchedules
  
  **Synchronization**
  - SyncQueue
  - SyncHistory
  - SyncConflicts
  - DataVersions

### Entity Relationships

**Key Relationships**:
- `CrewMembers` 1:N `CrewCertificates`
- `EquipmentGroups` 1:N `EquipmentAssets`
- `EquipmentAssets` 1:N `MaintenanceTasks`
- `MaintenanceTasks` 1:N `TaskChecklists`
- `MaintenanceTasks` N:M `CrewMembers` (via TaskAssignments)
- `MaintenanceTasks` 1:N `MaterialRequisitions`
- `Users` 1:1 `CrewMembers`
- `Voyages` 1:N `VoyageLogs`
- `Alarms` 1:N `AlarmHistory`

---

## 🔄 Tích Hợp và Đồng Bộ

### Delta Sync Mechanism

Hệ thống sử dụng **Delta Sync** để tối ưu băng thông:

```csharp
// Pseudocode
1. Edge tracks LastSyncTimestamp
2. Edge collects changes since LastSyncTimestamp
3. Edge sends only changed records to Shore
4. Shore processes and sends back updated data
5. Edge applies Shore changes and updates LastSyncTimestamp
```

**Conflict Resolution**:
- **Last-Write-Wins**: Thời gian muộn hơn thắng
- **Priority-Based**: Edge data có priority cao hơn cho operational data
- **Manual Resolution**: Cho critical conflicts (via UI)

### Sync Queue (Redis)

```
syncQueue:{vesselId}:{entityType} = [
  { operation: 'create', entity: {...}, timestamp: ... },
  { operation: 'update', entity: {...}, timestamp: ... },
  { operation: 'delete', entityId: ..., timestamp: ... }
]
```

### External Integrations

1. **MQTT (Message Queue Telemetry Transport)**
   - Purpose: Collect sensor data from ship equipment
   - Topics: 
     - `vessel/{vesselId}/engine/temperature`
     - `vessel/{vesselId}/position`
     - `vessel/{vesselId}/alarms`

2. **SignalK Protocol**
   - Marine data standard
   - Integration via `SignalKTestController.cs`
   - Real-time vessel data exchange

3. **Redis Cache**
   - Session management
   - Sync queue
   - Real-time data cache
   - Rate limiting

---

## 📊 Các Pattern và Best Practices

### Architecture Patterns
- **Clean Architecture**: Separation of concerns
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic encapsulation
- **DTO Pattern**: Data transfer optimization
- **CQRS (light)**: Separate read/write operations

### Code Organization
- **Controllers**: Thin, delegate to services
- **Services**: Business logic
- **Repositories**: Data access
- **DTOs**: API contracts
- **Models**: Domain entities

### Mobile App (Flutter)
- **Clean Architecture**: core/data/presentation layers
- **Provider/Riverpod**: State management
- **Repository Pattern**: Data abstraction
- **Dependency Injection**: Service locator

---

## 🚀 Deployment Strategy

### Edge Deployment (On Ship)
```
1. Pre-configure Edge server with Edge API + DB
2. Install Mobile app on crew tablets/phones
3. Configure MQTT broker for sensor data
4. Set up Redis for caching
5. Configure sync credentials for Shore connection
6. Run in autonomous mode
```

### Shore Deployment (On Land)
```
1. Deploy Backend API (Docker/Kubernetes)
2. Setup PostgreSQL (Clustered for HA)
3. Setup Redis (Cluster mode)
4. Deploy Frontend (CDN + Web server)
5. Configure SSL/TLS
6. Setup monitoring & logging
```

---

## 📈 Scalability Considerations

- **Horizontal Scaling**: Backend API via load balancer
- **Database Sharding**: By vessel for shore DB
- **Cache Strategy**: Redis for hot data
- **CDN**: Static assets for frontend
- **Message Queue**: For async operations
- **Read Replicas**: For reporting queries

---

## 🔐 Security Features

- **JWT Authentication**: Stateless auth
- **Role-Based Access Control (RBAC)**
- **HTTPS/TLS**: All communications
- **Password Hashing**: BCrypt
- **SQL Injection Protection**: Parameterized queries (EF Core)
- **CORS Configuration**: Restricted origins
- **Rate Limiting**: Redis-based
- **Secure Storage**: Flutter Secure Storage for mobile

---

## 📝 Key Files Reference

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Service orchestration |
| `product.sln` | Visual Studio solution |
| `backend/Program.cs` | Backend entry point |
| `edge-services/Program.cs` | Edge API entry point |
| `frontend/vite.config.ts` | Frontend build config |
| `frontend-mobile/pubspec.yaml` | Flutter dependencies |
| `README.md` | Main documentation |
| `DELTA_SYNC_IMPLEMENTATION.md` | Sync documentation |
| `PMS_IMPLEMENTATION_COMPLETE.md` | PMS feature docs |

---

## 🎓 Learning Resources

Để hiểu rõ hơn về dự án, đọc các file sau theo thứ tự:

1. `README.md` - Tổng quan
2. `QUICK_START_GUIDE.md` - Bắt đầu nhanh
3. `TEAM_WORKFLOW.md` - Quy trình làm việc
4. `DELTA_SYNC_IMPLEMENTATION.md` - Cơ chế đồng bộ
5. `PMS_IMPLEMENTATION_COMPLETE.md` - Hệ thống PMS
6. `MOBILE_PMS_IMPLEMENTATION_PLAN_V2.md` - Mobile app

---

## 📞 Technical Stack Summary

```
┌─────────────────────────────────────────────────────┐
│                 TECHNICAL STACK                      │
├─────────────────────────────────────────────────────┤
│ Backend:      .NET 8.0, C#, ASP.NET Core           │
│ Frontend:     React 19, TypeScript, Vite           │
│ Mobile:       Flutter, Dart                         │
│ Database:     PostgreSQL 15                         │
│ Cache:        Redis 7                               │
│ Message:      MQTT (Mosquitto 2.0)                  │
│ Container:    Docker, Docker Compose                │
│ ORM:          Entity Framework Core 8.0             │
│ API Doc:      Swagger/OpenAPI                       │
│ Auth:         JWT Bearer                            │
│ UI Library:   Radix UI, Tailwind CSS               │
│ State Mgmt:   React Hooks, Provider/Riverpod       │
└─────────────────────────────────────────────────────┘
```

---

## 🎯 Kết Luận

**Maritime Management System** là một hệ thống phức tạp và toàn diện với:

- ✅ **Kiến trúc Edge-Shore** cho hoạt động offline
- ✅ **Multi-platform** (Web, Mobile) 
- ✅ **Microservices ready** architecture
- ✅ **Real-time monitoring** capabilities
- ✅ **Comprehensive PMS** implementation
- ✅ **Maritime standards compliance** (IMO)
- ✅ **Scalable và maintainable** codebase

Dự án được tổ chức tốt với documentation đầy đủ, phù hợp cho phát triển và bảo trì lâu dài.

---

*Document created: 2026-02-04*  
*Version: 1.0*  
*Author: Maritime Development Team*
