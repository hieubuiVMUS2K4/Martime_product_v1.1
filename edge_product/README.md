# 🚢 MARITIME EDGE PRODUCT (Ship System)

Hệ thống thu thập và quản lý dữ liệu trên tàu - Ship-based Edge System.

## 📁 Cấu trúc thư mục

```
edge_product/
├── edge-services/           → Backend API (.NET 8)               → Port 5001
│   ├── docker-compose.yml   → Docker compose (Edge Postgres + pgAdmin)
│   ├── Controllers/         → API Controllers
│   ├── Services/            → Business logic + Sync services
│   ├── Models/              → Data models
│   ├── Migrations/          → EF Core migrations
│   └── init-scripts/        → Edge DB seed data
├── frontend-edge/           → Dashboard (React 19 + Tailwind)    → Port 3002
├── frontend-mobile/         → Mobile App (Flutter)
├── shared/                  → Shared library (Maritime.Shared)
├── database-backups/        → Edge database backups
├── team-database-backups/   → Team shared backups
├── scripts/                 → Edge-specific scripts
├── edge.sln                 → Visual Studio Solution
├── edge_database_export.sql → Full database export
├── edge_data_only.sql       → Data-only export
├── motagiaodien.md          → Mô tả giao diện
├── create-excel-template.js → BIO DATA Excel template creator
├── excel-template-redesign.ts → Excel template redesign
├── template1.xlsx           → Template file
└── Copy of 2. BIO DATA...csv → Crew BIO DATA sample
```

## 🚀 Khởi động nhanh

```powershell
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

## ✅ Truy cập
- Frontend Dashboard: http://localhost:3002
- Backend API Swagger: http://localhost:5001/swagger
- Database Admin (pgAdmin): http://localhost:5050

## 🎯 Tính năng chính
- ✅ Thu thập dữ liệu từ cảm biến NMEA/Modbus
- ✅ Giả lập telemetry tự động (GPS, Engine, Generator, Environmental)
- ✅ Dashboard theo dõi thời gian thực cho thuyền trưởng
- ✅ Hoạt động offline-first với sync queue
- ✅ Quản lý crew, certificates trên tàu
- ✅ Quản lý vật tư (Materials Management)
- ✅ PMS Planning - Lập kế hoạch bảo trì
- ✅ Báo cáo PDF/Excel
- ✅ Mobile app (Flutter) cho thủy thủ đoàn
- ✅ Đồng bộ dữ liệu lên Shore System

## 🗄️ Database
- **Database**: `maritime_edge`
- **Port**: 5433
- **User**: `edge_user`

## 📱 Mobile App
```powershell
cd frontend-mobile
flutter pub get
flutter run
```
