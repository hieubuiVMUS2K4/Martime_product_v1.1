# 🏢 MARITIME SHORE PRODUCT (Cloud System)

Hệ thống quản lý tàu biển phía bờ - Shore-based Fleet Management System.

## 📁 Cấu trúc thư mục

```
shore_product/
├── backend/                 → Backend API (.NET 8)           → Port 5000
├── frontend/                → Fleet Dashboard (React 19)     → Port 3000
├── shared/                  → Shared library (Maritime.Shared)
├── init-scripts/            → Database init scripts
├── database-backups/        → Shore database backups
├── scripts/                 → Shore-specific scripts
├── docker-compose.yml       → Docker compose (Postgres + Backend + Frontend)
├── shore.sln                → Visual Studio Solution
├── seed-data.sql            → Seed data
├── seed-crew-data.sql       → Crew seed data
├── seed-crew-reference-data.sql → Crew reference seed data
└── shore_database_export.sql → Database export file
```

## 🚀 Khởi động nhanh

### Với Docker:
```powershell
docker compose up -d
```

### Không Docker:
```powershell
# 1. Chạy Backend API
cd backend
dotnet run --urls "http://localhost:5000"

# 2. Chạy Frontend (terminal khác)
cd frontend
npm install
npm run dev
```

## ✅ Truy cập
- Frontend Dashboard: http://localhost:3000
- Backend API Swagger: http://localhost:5000/swagger
- Database: PostgreSQL Port 5432

## 🎯 Tính năng chính
- ✅ Quản lý đội tàu từ trung tâm điều hành
- ✅ Phân tích dữ liệu tổng hợp từ nhiều tàu
- ✅ Quản lý crew, certificates, departments
- ✅ Báo cáo tuân thủ IMO, SOLAS, MARPOL
- ✅ API tích hợp với các hệ thống cảng
- ✅ Nhận dữ liệu sync từ Edge System

## 🗄️ Database
- **Database**: `productdb`
- **Port**: 5432
- **User**: `product`
