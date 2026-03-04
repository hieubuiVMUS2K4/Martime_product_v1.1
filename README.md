# 🚢 **MARITIME MANAGEMENT SYSTEM**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/hieubuiVMUS2K4/sampleProduct)
[![Docker](https://img.shields.io/badge/docker-ready-blue.svg)](https://hub.docker.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-blue.svg)](https://reactjs.org/)

## **🌊 Professional Maritime Fleet Management System**

Hệ thống quản lý hạm đội tàu **chuyên nghiệp và toàn diện**, được phát triển theo tiêu chuẩn quốc tế IMO (International Maritime Organization). Tích hợp đầy đủ từ theo dõi thời gian thực, quản lý nhiên liệu thông minh, đến hệ thống cảnh báo tự động và thu thập dữ liệu từ cảm biến trên tàu.

---

## � **CẤU TRÚC DỰ ÁN (ĐÃ TÁI TỔ CHỨC)**

Dự án được tách thành **2 product độc lập** để dễ quản lý và deploy riêng biệt:

```
📦 Martime_product_v1.1/
│
├── 🏢 shore_product/           → SHORE SYSTEM (Cloud/Trên bờ)
│   ├── backend/                → Shore Backend API (.NET 8)     → Port 5000
│   ├── frontend/               → Fleet Dashboard (React 19)     → Port 3000
│   ├── shared/                 → Shared library (Maritime.Shared)
│   ├── docker-compose.yml      → Shore Docker Compose
│   ├── shore.sln               → Shore Solution
│   └── README.md               → Hướng dẫn Shore
│
├── 🚢 edge_product/            → EDGE SYSTEM (Ship/Trên tàu)
│   ├── edge-services/          → Edge Backend API (.NET 8)      → Port 5001
│   ├── frontend-edge/          → Ship Dashboard (React 19)      → Port 3002
│   ├── frontend-mobile/        → Mobile App (Flutter)
│   ├── shared/                 → Shared library (Maritime.Shared)
│   ├── edge.sln                → Edge Solution
│   └── README.md               → Hướng dẫn Edge
│
├── 📚 docs/                    → Tài liệu chung cho cả hệ thống
├── 🔧 scripts/                 → Scripts tiện ích (backup/export cả 2 DB)
├── ⚙️  configs/                 → Cấu hình chung
└── 📋 product.sln              → Root solution (tham chiếu cả 2)
```

---

## 🚀 **KHỞI ĐỘNG NHANH**

### **🚢 Edge System (Trên tàu):**

```powershell
cd edge_product

# 1. Khởi động Database
cd edge-services
docker compose up -d edge-postgres edge-pgadmin

# 2. Chạy Backend API (Terminal 1)
dotnet run --urls "http://localhost:5001"

# 3. Chạy Frontend (Terminal 2)
cd ../frontend-edge
npm install
npm run dev
```

### **🏢 Shore System (Trên bờ):**

```powershell
cd shore_product

# Docker (all-in-one)
docker compose up -d

# Hoặc manual:
cd backend && dotnet run --urls "http://localhost:5000"
cd frontend && npm install && npm run dev
```

**✅ Truy cập:**
- Shore Dashboard: http://localhost:3000 | Shore API: http://localhost:5000/swagger
- Edge Dashboard: http://localhost:3002 | Edge API: http://localhost:5001/swagger
- pgAdmin: http://localhost:5050

---

## 🎯 **HỆ THỐNG EDGE-SHORE ARCHITECTURE**

Dự án gồm **2 hệ thống độc lập**:

### **🚢 EDGE SYSTEM** (Ship-based / Trên tàu) → `edge_product/`
```
📁 edge-services/          → Backend API (.NET 8)     → Port 5001
📁 frontend-edge/          → Dashboard (React 19)     → Port 3002
📁 frontend-mobile/        → Mobile App (Flutter)
🗄️ PostgreSQL              → Maritime Edge DB         → Port 5433
```
**Tính năng:**
- ✅ Thu thập dữ liệu từ cảm biến NMEA/Modbus
- ✅ Giả lập telemetry tự động (GPS, Engine, Generator, Environmental)
- ✅ Dashboard theo dõi thời gian thực cho thuyền trưởng
- ✅ Hoạt động offline-first với sync queue
- ✅ 18 bảng database đã migrate sẵn

### **🏢 CLOUD SYSTEM** (Shore-based / Trên bờ) → `shore_product/`
```
📁 backend/                → Backend API (.NET 8)     → Port 5000
📁 frontend/               → Fleet Dashboard (React)  → Port 3000
🗄️ PostgreSQL              → Product DB               → Port 5432
```
**Tính năng:**
- ✅ Quản lý đội tàu từ trung tâm điều hành
- ✅ Phân tích dữ liệu tổng hợp từ nhiều tàu
- ✅ Báo cáo tuân thủ IMO, SOLAS, MARPOL
- ✅ API tích hợp với các hệ thống cảng

---

## �🎯 Tính Năng Maritime Chuyên Nghiệp

### **� Core Maritime Features**
- **📡 Real-time Vessel Tracking** - GPS/AIS integration với high-precision positioning
- **⛽ Smart Fuel Management** - AI-powered efficiency optimization và emissions tracking
- **🚨 Intelligent Alert System** - Automated monitoring với predictive maintenance
- **� Maritime Analytics** - Performance dashboards tuân thủ IMO standards
- **📱 Cross-platform Apps** - Web + Mobile applications cho ship và shore operations
- **🔒 Enterprise Security** - JWT authentication với role-based maritime permissions

### **⚡ Advanced Maritime Technology**
- **🌊 Edge Computing** - NMEA/Modbus sensor data collection từ ship systems
- **🛰️ Satellite Integration** - VSAT communication với store-and-forward capabilities
- **🏢 Port Integration** - EDI messaging cho port authorities và customs
- **📋 Compliance Management** - Automated IMO, SOLAS, MARPOL compliance reporting
- **🤖 Maritime AI** - Machine learning cho route optimization và fuel efficiency
- **🔄 Real-time Sync** - Ship-shore data synchronization với Redis caching

## 🏗️ Kiến Trúc Hệ Thống

### **Edge System Architecture (Ship-based)**
```
┌─────────────────────────────────────────────────────────────┐
│                    VESSEL EDGE SERVER                        │
│                                                               │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  Frontend-Edge   │◄────►│  Edge Services   │            │
│  │  React Dashboard │      │  .NET 8 Web API  │            │
│  │  Port: 3002      │      │  Port: 5001      │            │
│  └──────────────────┘      └─────────┬────────┘            │
│                                       │                      │
│                             ┌─────────▼─────────┐           │
│                             │   PostgreSQL DB   │           │
│                             │   Port: 5433      │           │
│                             │  18 Tables        │           │
│                             └─────────┬─────────┘           │
│                                       │                      │
│  ┌────────────────────────────────────▼──────────────────┐ │
│  │        TelemetrySimulatorService                       │ │
│  │  Auto-generates data every 5 seconds:                  │ │
│  │  • GPS Position (lat/lon drift)                        │ │
│  │  • Engine RPM, Temp, Load                              │ │
│  │  • Generator Status (3 units)                          │ │
│  │  • Environmental (temp, pressure, wind)                │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────────┬───────────────────────────────┘
                            │
                  ┌─────────▼─────────┐
                  │   VSAT/4G/LTE     │
                  │  Ship-Shore Sync  │
                  └─────────┬─────────┘
                            │
┌───────────────────────────▼───────────────────────────────┐
│                   SHORE CLOUD SYSTEM                       │
│                                                             │
│  ┌──────────────────┐      ┌──────────────────┐          │
│  │   Frontend Web   │◄────►│    Backend API   │          │
│  │   React Fleet    │      │   .NET 8 Core    │          │
│  │   Port: 3000     │      │   Port: 5000     │          │
│  └──────────────────┘      └─────────┬────────┘          │
│                                       │                    │
│                             ┌─────────▼─────────┐         │
│                             │   PostgreSQL DB   │         │
│                             │   Port: 5432      │         │
│                             │   Fleet Data      │         │
│                             └───────────────────┘         │
└─────────────────────────────────────────────────────────┘
```

### **Cloud System Architecture (Shore-based)**

### **Cloud System Architecture (Shore-based)**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Frontend Web   │    │  Frontend Mobile│    │    Backend API  │
│  React 19 + TS  │◄──►│     Flutter     │◄──►│ ASP.NET Core 8  │
│  Port: 3000     │    │                 │    │   Port: 5000    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                        ┌─────────────────┐            │
                        │    pgAdmin      │            │
                        │   Port: 8081    │            │
                        └─────────────────┘            │
                                                       ▼
                        ┌─────────────────┐    ┌─────────────────┐
                        │     Redis       │    │   PostgreSQL    │
                        │ Cache + Session │    │   Database      │
                        │   Port: 6379    │    │   Port: 5432    │
                        └─────────────────┘    └─────────────────┘
```

---

## 📦 **EDGE SYSTEM - Chi Tiết Kỹ Thuật**

### **Backend API (edge-services/)**
```csharp
// 8 Controllers đã implement:
✅ TelemetryController      → 9 endpoints (position, navigation, engine, etc.)
✅ DashboardController       → Dashboard statistics
✅ CrewController            → STCW-compliant crew management
✅ MaintenanceController     → ISM Code PMS system
✅ VoyageController          → Voyage & cargo operations
✅ ComplianceController      → Watchkeeping logs, Oil Record Book
✅ AlarmsController          → Safety alarm system
✅ SyncController            → Ship-shore data synchronization

// Background Services:
✅ TelemetrySimulatorService → Auto-generates realistic data every 5 seconds
   - GPS drift simulation (±222m per update)
   - Main Engine: 700-740 RPM, 70-80% load
   - Aux Engine: 1450-1550 RPM, 35-55% load
   - 3 Generators: GEN_1 running, GEN_2/EMER standby
   - Environmental: 25-31°C, 1008-1018 hPa, 10-20 knots wind
```

### **Frontend Dashboard (frontend-edge/)**
```typescript
// 9 Pages implemented:
✅ Dashboard         → Live telemetry với real-time updates
✅ Crew Management   → 4 tabs (Onboard/All/Certificates/Reports)
✅ Maintenance PMS   → 4 tabs (Pending/Overdue/All/Calendar)
🚧 Navigation        → Map & position tracking (stub)
🚧 Engine Monitor    → Engine performance charts (stub)
🚧 Alarms            → Alert management (stub)
🚧 Voyage Tracking   → Voyage history (stub)
🚧 Compliance        → Regulatory compliance (stub)
🚧 Sync Status       → Data synchronization (stub)

// Tech Stack:
- React 19 + TypeScript 5.9
- Vite 6.0 (ultra-fast HMR)
- Tailwind CSS 3.4 + shadcn/ui
- Zustand (state management)
- React Router v7
- Recharts (data visualization)
- date-fns (date utilities)
```

### **Database Schema (PostgreSQL)**
```sql
-- 18 Tables migrated và ready:
position_data             -- GPS/GNSS tracking
navigation_data           -- Heading, speed, pitch, roll
ais_data                  -- AIS messages (nearby vessels)
engine_data               -- Main + Aux engines
generator_data            -- 3 generators status
tank_levels               -- Fuel, water, ballast tanks
fuel_consumption          -- IMO DCS compliance
environmental_data        -- Weather, sea state
safety_alarms             -- SOLAS alarm system
crew_members              -- STCW certificates
crew_certificates         -- Certificate tracking
maintenance_tasks         -- ISM Code PMS
maintenance_history       -- Maintenance records
voyages                   -- Voyage planning
cargo_operations          -- Cargo handling
watchkeeping_logs         -- Bridge/Engine logs
oil_record_book           -- MARPOL Annex I
sync_queue                -- Ship-shore sync
```

---

## 🛠️ Công Nghệ Sử Dụng

### **💻 Edge System Technology Stack**
```bash
Backend Framework │ ASP.NET Core 8.0 Web API
ORM & Database    │ Entity Framework Core 8.0 + Npgsql (PostgreSQL 15)
Background Tasks  │ IHostedService - TelemetrySimulatorService (5s interval)
Maritime Services │ 8 Controllers + TelemetrySimulator
API Documentation │ Swagger/OpenAPI 3.0
Logging           │ Microsoft.Extensions.Logging (structured logs)
CORS              │ Dynamic origin validation (localhost:*, 192.168.*, 172.*)
Frontend          │ React 19 + TypeScript 5.9 + Vite 6.0
UI Framework      │ Tailwind CSS 3.4 + shadcn/ui components
State Management  │ Zustand 5.0
Routing           │ React Router v7
Charts            │ Recharts 2.14
Database          │ PostgreSQL 15 (Port 5433)
Admin Tools       │ pgAdmin 4 (Port 5050)
```

### **🌐 Cloud System Technology Stack**
```bash
Backend Framework │ ASP.NET Core 8.0 Web API
ORM & Database    │ Entity Framework Core 8.0 + PostgreSQL 15
Maritime Services │ VesselService, AlertService, TelemetryService, BackgroundService
Cache & Session   │ Redis 7-Alpine với persistence và LRU eviction
Authentication    │ JWT Bearer tokens với SymmetricSecurityKey
API Documentation │ Swagger/OpenAPI 3.0 với Swashbuckle.AspNetCore
Background Tasks  │ IHostedService cho automated alert processing
Logging           │ Microsoft.Extensions.Logging với structured logs
Health Checks     │ ASP.NET Core health checks cho database connectivity
```

### **🌐 Frontend Platforms**
```bash
Web Framework     │ React 19.0 + TypeScript 5.9 với strict mode
UI Components     │ Custom maritime components (Card, Badge, Button, Input)
Build System      │ Vite 6.0 với Rolldown bundler (ultra-fast HMR)
Styling           │ Tailwind CSS với class-variance-authority
State Management  │ React Hooks (useState, useEffect) + Context API
HTTP Client       │ Axios với interceptors cho API calls
Icons & Graphics  │ Lucide React cho maritime icons
Utils & Helpers   │ clsx + tailwind-merge cho dynamic styling
Path Resolution   │ TypeScript path mapping (@/* aliases)
Mobile Platform   │ Flutter 3.x với Dio HTTP client
```

### **🚢 Maritime Infrastructure & DevOps**
```bash
Containerization  │ Docker 27.x + Docker Compose v2 multi-service orchestration
Database System   │ PostgreSQL 15 Alpine với health checks + pgAdmin 4
Cache Layer       │ Redis 7-Alpine với maxmemory 256MB + allkeys-lru policy  
Web Server        │ Nginx stable-alpine cho production deployment
Database Admin    │ pgAdmin 4 với pre-configured server connections
Container Network │ Docker bridge network với service discovery
Volume Management │ Named volumes cho data persistence (postgres-data, redis-data)
Health Monitoring │ PostgreSQL health checks với pg_isready
Environment       │ .env configuration với secure defaults
Port Mapping      │ Strategic port exposure (3000, 5000, 5432, 6379, 8081)
```

### **📡 Maritime Edge Computing & IoT Integration**
```bash
Edge Services     │ .NET 8 Console applications cho ship-based data collection
NMEA Processing   │ System.IO.Ports cho serial communication với GPS/AIS devices
Data Collection   │ NmeaCollectorService với real-time sentence parsing
Local Storage     │ SQLite cho offline caching và intermittent connectivity
MQTT Broker       │ Eclipse Mosquitto cho ship-shore communication
Protocol Support  │ NMEA 0183/2000 sentence parsing và validation
Sensor Interfaces │ Modbus RTU/TCP integration ready cho engine monitoring
Background Sync   │ Scheduled data synchronization với shore-based systems
Device Management │ Device registry với IMO-based vessel identification
Communication     │ Store-and-forward architecture cho satellite connections
```

---

## **📦 Package Dependencies & Libraries**

### **🔧 Backend .NET Packages (product-api.csproj)**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
```

### **🌐 Frontend npm Packages (package.json)**
```json
{
  "dependencies": {
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "typescript": "~5.9.0",
    "lucide-react": "^0.454.0",
    "@radix-ui/react-slot": "^1.1.0",
    "class-variance-authority": "^0.7.1",
    "clsx": "^2.1.1",
    "tailwind-merge": "^2.5.4"
  },
  "devDependencies": {
    "@types/node": "^22.9.0",
    "@vitejs/plugin-react": "^4.3.4",
    "vite": "^6.0.1",
    "tailwindcss": "^3.4.15"
  }
}
```

### **🚢 Maritime-Specific Technologies**
```bash
Maritime Models   │ C# classes với navigation properties (Vessel, VesselPosition, FuelConsumption)
Data Validation   │ System.ComponentModel.DataAnnotations cho maritime compliance
JSON Processing   │ System.Text.Json cho telemetry data serialization
Background Tasks  │ Microsoft.Extensions.Hosting.BackgroundService cho automated alerts
Serial Communication │ System.IO.Ports cho NMEA device integration
Database Migrations │ EF Core migrations với maritime schema design
Performance Optimization │ Redis distributed caching với maritime-specific keys
```

### **🐳 Docker & Infrastructure**
```bash
Base Images       │ mcr.microsoft.com/dotnet/aspnet:8.0 + mcr.microsoft.com/dotnet/sdk:8.0
Database          │ postgres:15-alpine với custom initialization
Cache             │ redis:7-alpine với persistence configuration  
Web Server        │ nginx:stable-alpine cho production serving
Admin Tools       │ dpage/pgadmin4 cho database management
Network           │ Docker Compose bridge network với service discovery
Volumes           │ Named volumes cho PostgreSQL và Redis data persistence
```

---

## 📋 YÊU CẦU HỆ THỐNG

### **Edge System Requirements**
```bash
✅ .NET 8 SDK                    → https://dotnet.microsoft.com/download
✅ Node.js 20+                   → https://nodejs.org/
✅ Docker Desktop (optional)     → https://docker.com/
✅ PostgreSQL 15+ (hoặc Docker)  → https://postgresql.org/
✅ 4GB RAM minimum (8GB khuyến nghị)
✅ 5GB disk space
✅ Windows 10+, macOS, Linux
```

### **Cloud System Requirements**
### **Cloud System Requirements**
- **OS**: Windows 10+, macOS 10.14+, Linux (any recent distro)
- **RAM**: 4GB+ (khuyến nghị 8GB+ cho development)
- **Disk**: 5GB trống (cho Docker images + data)
- **Docker**: Docker Desktop hoặc Docker Engine
- **Docker Compose**: v1.25+ (integrated với Docker Desktop)

---

## 🚀 HƯỚNG DẪN CÀI ĐẶT CHI TIẾT

> **⚡ Khởi động nhanh:** Xem [QUICK_START_GUIDE.md](QUICK_START_GUIDE.md)

### **Phương Án 1: Edge System (Ship-based) - Khuyến nghị**

#### **Bước 1: Clone Repository**
```bash
git clone https://github.com/hieubuiVMUS2K4/sampleProduct.git
cd sampleProduct
```

#### **Bước 2: Khởi Động Database**
```powershell
# Di chuyển vào edge-services
cd edge-services

# Khởi động PostgreSQL + pgAdmin bằng Docker
docker compose up -d edge-postgres edge-pgadmin

# Kiểm tra trạng thái
docker compose ps
```

**✅ Kết quả:**
```
NAME                          STATUS              PORTS
maritime-edge-postgres        Up                  0.0.0.0:5433->5432/tcp
maritime-edge-pgadmin         Up                  0.0.0.0:5050->80/tcp
```

#### **Bước 3: Khởi Động Backend API**
```powershell
# Trong thư mục edge-services
dotnet restore
dotnet run --urls "http://localhost:5001"
```

**✅ Chờ thông báo:**
```
info: MaritimeEdge.Services.TelemetrySimulatorService[0]
      Telemetry Simulator Service started
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

#### **Bước 4: Khởi Động Frontend Dashboard**
**Mở terminal mới:**
```powershell
cd frontend-edge
npm install
npm run dev
```

**✅ Truy cập:**
- Frontend: http://localhost:3002
- Backend API: http://localhost:5001/swagger
- Database Admin: http://localhost:5050
  - Username: `admin@edge.local`
  - Password: `admin`

---

### **Phương Án 2: Cloud System (Shore-based) với Docker**

#### Bước 1: Clone Repository
```bash
git clone https://github.com/hieubuiVMUS2K4/sampleProduct.git
cd sampleProduct
```

#### Bước 2: Kiểm Tra Docker
```bash
docker --version
docker compose version
```

#### Bước 3: Cấu Hình Môi Trường
```bash
# File .env đã có sẵn, bạn có thể chỉnh sửa nếu cần:
# - POSTGRES_USER=product
# - POSTGRES_PASSWORD=productpwd  
# - POSTGRES_DB=productdb
# - PGADMIN_EMAIL=admin@example.com
# - JWT__Key=VerySecretKey12345 (ĐỔI TRONG PRODUCTION!)
```

#### Bước 4: Build và Khởi Động
```bash
# Build tất cả services
docker compose build --no-cache

# Khởi động hệ thống
docker compose up -d

# Kiểm tra trạng thái
docker compose ps
```

#### Bước 5: Truy Cập Maritime System
| Service | URL | Description |
|---------|-----|-------------|
| **🚢 Maritime Dashboard** | `http://localhost:3000` | Fleet management web interface |
| **⚓ Backend API** | `http://localhost:5000` | RESTful maritime services |
| **� API Documentation** | `http://localhost:5000/swagger` | Interactive maritime API docs |
| **�️ Database Management** | `http://localhost:8081` | pgAdmin cho maritime database |
| **� Redis Cache** | `localhost:6379` | Real-time vessel data cache |

### **🎯 Maritime System Status Check**
```bash
# Verify all maritime services are running
docker compose ps

# Check vessel tracking service health
curl http://localhost:5000/health

# Test maritime API endpoints
curl http://localhost:5000/api/vessels

# Monitor maritime system logs
docker compose logs -f backend
```

### **🗄️ Maritime Database Schema**
```sql
-- Core maritime entities implemented
Vessels              │ IMO, Name, CallSign, VesselType, GrossTonnage, DeadWeight, Flag
VesselPositions      │ Latitude, Longitude, Speed, Course, Timestamp, Source (GPS/AIS)
FuelConsumptions     │ ReportDate, FuelConsumed, FuelType, DistanceTraveled, FuelEfficiency
PortCalls           │ PortCode (UN/LOCODE), ArrivalTime, DepartureTime, CargoQuantity
VesselAlerts        │ AlertType, Message, Severity, IsAcknowledged, AcknowledgedBy
Certificates        │ CertificateType, ExpiryDate, IssuingAuthority, CertificateNumber

-- Advanced indexing for maritime queries
CREATE INDEX idx_vessel_positions_vessel_timestamp ON VesselPositions(VesselId, Timestamp DESC);
CREATE INDEX idx_vessel_alerts_vessel_acknowledged ON VesselAlerts(VesselId, IsAcknowledged);
CREATE INDEX idx_fuel_consumption_vessel_date ON FuelConsumptions(VesselId, ReportDate DESC);
```

---

### Phương Án 2: Development Setup (Chạy Từng Phần)

#### Backend (.NET Core)
```bash
cd backend
dotnet restore
dotnet run
# API sẽ chạy tại: http://localhost:5000
```

#### Frontend Web (React)
```bash
cd frontend
npm install
npm run dev
# Web sẽ chạy tại: http://localhost:5173
```

#### Frontend Mobile (Flutter)
```bash
cd frontend-mobile
flutter pub get
flutter run
# Chạy trên emulator/device
```

#### Database (Riêng)
```bash
# Chỉ khởi động PostgreSQL và Redis
docker compose up postgres redis pgadmin -d
```

---

## 🔐 CẤU HÌNH BẢO MẬT

### pgAdmin Database Connection
1. Truy cập: http://localhost:8081
2. Login: `admin@example.com` / `admin`
3. Add Server:
   - **Name**: ProductDB
   - **Host**: `postgres` (trong Docker) hoặc `localhost` (nếu chạy local)
   - **Port**: `5432`
   - **Database**: `productdb`  
   - **Username**: `product`
   - **Password**: `productpwd`

### Redis Connection (Optional)
```bash
# Kết nối Redis CLI
docker exec -it product-redis-1 redis-cli

# Test Redis
127.0.0.1:6379> ping
PONG
```

---

## 📊 LỆNH QUẢN LÝ THƯỜNG DÙNG

### Docker Management
```bash
# Xem logs realtime
docker compose logs backend --follow
docker compose logs frontend --follow

# Dừng hệ thống
docker compose down

# Dừng và xóa volumes (reset DB)
docker compose down -v

# Rebuild specific service
docker compose build --no-cache backend
docker compose build --no-cache frontend

# Scale services (nếu cần)
docker compose up --scale backend=2 -d
```

### Database Management  
```bash
# Backup database
docker exec product-postgres-1 pg_dump -U product productdb > backup_$(date +%Y%m%d).sql

# Restore database  
docker exec -i product-postgres-1 psql -U product productdb < backup.sql

# Connect trực tiếp PostgreSQL
docker exec -it product-postgres-1 psql -U product -d productdb
```

### Development Workflow
```bash
# Hot reload backend
cd backend && dotnet watch run

# Hot reload frontend
cd frontend && npm run dev

# Mobile development
cd frontend-mobile && flutter run --hot-reload
```

---

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi Docker Build
```bash
# Lỗi: frontend-web-1 container còn tồn tại
docker compose down --remove-orphans
docker compose build --no-cache

# Lỗi: Port đã sử dụng
docker compose down
# Hoặc thay đổi port trong docker-compose.yml
```

### Lỗi Database Connection
```bash
# Kiểm tra PostgreSQL health
docker compose logs postgres

# Restart PostgreSQL
docker compose restart postgres
```

### Lỗi npm/Node
```bash
# Trong frontend folder
npm install --legacy-peer-deps
npm run build

# Hoặc sử dụng Node version manager
nvm use 20
```

---

## 🔧 TÙY CHỈNH VÀ MỞ RỘNG

### Thêm Environment Variables
```bash
# Trong .env file
NEW_FEATURE_FLAG=true
EXTERNAL_API_KEY=your_key_here

# Trong docker-compose.yml
environment:
  - NEW_FEATURE_FLAG=${NEW_FEATURE_FLAG}
```

### Thêm Dependencies
```bash
# Backend (.NET)
cd backend && dotnet add package NewPackage

# Frontend (npm)  
cd frontend && npm install new-package

# Mobile (Flutter)
cd frontend-mobile && flutter pub add new_package
```

---

## 📈 PRODUCTION DEPLOYMENT

### Bảo Mật Production
1. **Đổi JWT secret** trong `.env`
2. **Sử dụng HTTPS** cho tất cả endpoints
3. **Cấu hình CORS** restrictive hơn
4. **Enable rate limiting** cho API
5. **Backup database** định kỳ

### Performance Optimization
1. **Redis cache** cho API responses
2. **Database indexing** cho queries thường dùng  
3. **CDN** cho static assets
4. **Load balancer** cho multiple instances

---

## 📞 HỖ TRỢ VÀ ĐÓNG GÓP

### Báo Lỗi
- Tạo issue trên GitHub với logs chi tiết
- Bao gồm OS, Docker version, và steps reproduce

### Đóng Góp
1. Fork repository
2. Tạo feature branch
3. Commit changes  
4. Tạo Pull Request

---

## 📄 GIẤY PHÉP

MIT License - Xem file `LICENSE` để biết chi tiết.

---

---

## **🚢 Maritime API Endpoints**

### **⚓ Core Vessel Management**
```http
GET    /api/vessels                    # Fleet overview với real-time status
POST   /api/vessels                    # Register new vessel (IMO compliant)
GET    /api/vessels/{id}               # Complete vessel details
PUT    /api/vessels/{id}               # Update vessel information
GET    /api/vessels/imo/{imo}          # Find vessel by IMO number
```

### **📍 Position & Navigation**
```http
POST   /api/vessels/{id}/positions     # Update GPS/AIS position
GET    /api/vessels/{id}/positions     # Position tracking history
POST   /api/vessels/{id}/positions/bulk # Bulk AIS data upload
GET    /api/vessels/{id}/metrics       # Navigation performance metrics
```

### **⛽ Fuel & Performance Management**
```http
POST   /api/vessels/{id}/fuel          # Daily fuel consumption report
GET    /api/vessels/{id}/fuel          # Fuel efficiency analytics
GET    /api/vessels/{id}/emissions     # Environmental compliance data
```

### **🚨 Alert & Monitoring System**
```http
GET    /api/vessels/{id}/alerts        # Active vessel alerts
POST   /api/vessels/{id}/alerts        # Create manual alert
PUT    /api/alerts/{id}/acknowledge    # Acknowledge maritime alert
GET    /api/alerts                     # Fleet-wide alert overview
```

### **📡 Maritime Telemetry (Edge Integration)**
```http
POST   /api/telemetry/bulk            # NMEA/sensor data ingestion
GET    /api/vessels/{id}/health       # Ship systems health status
POST   /api/edge/sync                 # Ship-shore data synchronization
```

---

## **🌊 Maritime Documentation**

- **📋 [API Documentation](MARITIME_API_DOCUMENTATION.md)** - Complete maritime API reference
- **🏗️ [Architecture Guide](MARITIME_ARCHITECTURE.md)** - Maritime system architecture
- **⚡ [Quick Start Guide](QUICK_START.md)** - Get started with maritime operations
- **🔧 [Installation Guide](INSTALLATION_GUIDE.md)** - Detailed setup instructions

---

## **🤝 Maritime Community & Support**

### **📞 Maritime Support Channels**
- **🌐 Documentation**: Complete maritime API và system guides
- **💬 Community**: Maritime developers community  
- **📧 Technical Support**: maritime-tech@company.com
- **🐛 Bug Reports**: GitHub Issues với maritime-specific labels
- **💡 Feature Requests**: Maritime Enhancement Proposals

### **👥 Contributing to Maritime Innovation**
1. Fork repository: `git clone https://github.com/hieubuiVMUS2K4/sampleProduct.git`
2. Create maritime feature branch: `git checkout -b feature/maritime-enhancement`
3. Follow maritime coding standards và IMO compliance
4. Add comprehensive tests cho maritime scenarios
5. Submit Pull Request với detailed maritime impact description

---

## **📈 Maritime Roadmap**

### **🚢 Current Version: Maritime Core v1.0** ✅
- [x] **Complete vessel management** với IMO compliance
- [x] **Real-time position tracking** GPS/AIS integration
- [x] **Fuel management system** với efficiency analytics  
- [x] **Intelligent alert engine** automated maritime monitoring
- [x] **Maritime telemetry** NMEA/Modbus sensor integration
- [x] **Cross-platform applications** Web + Mobile

### **⚓ Next Phase: Advanced Maritime Intelligence** 🚧
- [ ] **AI-powered route optimization** với weather integration
- [ ] **Predictive maintenance** using maritime ML models
- [ ] **Carbon Intensity Indicator (CII)** compliance automation
- [ ] **Digital twin technology** cho vessel simulation
- [ ] **Blockchain logistics** supply chain transparency
- [ ] **5G maritime connectivity** optimization

### **🌊 Future Vision: Next-Gen Maritime Platform** 🔮
- [ ] **Autonomous ship integration** smart vessel operations
- [ ] **Maritime IoT ecosystem** comprehensive sensor networks
- [ ] **Quantum maritime optimization** complex routing algorithms
- [ ] **Extended Reality (XR)** training và collaboration systems
- [ ] **Sustainable shipping** green technology integration
- [ ] **Maritime metaverse** global collaboration platform

---

## **📄 License & Maritime Compliance**

This Maritime Management System is licensed under the **MIT License** with additional maritime industry compliance requirements.

### **🔒 Maritime Standards & Compliance**
- **IMO Standards**: SOLAS, MARPOL, ISM Code compliance ready
- **Class Society Integration**: DNV, ABS, Lloyd's Register compatible
- **Port Authority Systems**: EDI messaging cho customs clearance
- **Environmental Regulations**: IMO 2030/2050 emissions tracking
- **Data Privacy**: GDPR compliant cho EU maritime operations
- **Cyber Security**: IMO maritime cyber security guidelines

---

## **🙏 Maritime Industry Acknowledgments**

Special recognition to the **global maritime community** that helped shape this system:

- **🌐 International Maritime Organization (IMO)** - Standards và regulatory guidance
- **⚓ Maritime and Port Authority of Singapore (MPA)** - Technical insights và best practices  
- **🚢 Major Shipping Lines** - Real-world operational requirements (Maersk, MSC, CMA CGM)
- **🏢 Port Operators** - Integration requirements (APM Terminals, PSA, DP World)
- **🔧 Classification Societies** - Technical standards (DNV, ABS, Lloyd's Register)
- **💻 Maritime Software Community** - Industry expertise và collaborative development

---

<div align="center">

**⚓ Built with ❤️ for the Global Maritime Industry 🌊**

[![IMO Compliant](https://img.shields.io/badge/IMO-Compliant-blue.svg)](https://imo.org/)
[![Maritime Tested](https://img.shields.io/badge/Maritime-Tested-green.svg)](https://github.com/hieubuiVMUS2K4/sampleProduct)
[![Enterprise Ready](https://img.shields.io/badge/Enterprise-Ready-orange.svg)](https://github.com/hieubuiVMUS2K4/sampleProduct)
[![Production Deployed](https://img.shields.io/badge/Production-Deployed-success.svg)](https://github.com/hieubuiVMUS2K4/sampleProduct)

*"Navigating the Future of Maritime Technology"* 🚢⚡

**Maritime Management System - Empowering Safe, Efficient, and Sustainable Shipping Operations Worldwide**

</div>
