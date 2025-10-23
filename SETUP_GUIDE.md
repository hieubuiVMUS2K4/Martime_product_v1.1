# 🚢 Maritime Management System - Setup Guide

Hướng dẫn chi tiết để cài đặt và chạy dự án Maritime Management System trên máy của bạn.

---

## 📋 Mục Lục
- [Yêu Cầu Hệ Thống](#yêu-cầu-hệ-thống)
- [Cài Đặt Nhanh với Docker](#cài-đặt-nhanh-với-docker)
- [Cài Đặt Thủ Công](#cài-đặt-thủ-công)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [Troubleshooting](#troubleshooting)

---

## 🔧 Yêu Cầu Hệ Thống

### Bắt buộc:
- **Docker Desktop** (khuyến nghị) HOẶC
- **.NET 8.0 SDK** cho backend
- **Node.js 18+** và npm/yarn cho frontend
- **PostgreSQL 15+** (nếu không dùng Docker)

### Tùy chọn:
- **Git** để clone repository
- **Visual Studio Code** hoặc IDE khác
- **pgAdmin** để quản lý database

---

## 🐳 Cài Đặt Nhanh với Docker (Khuyến Nghị)

### Bước 1: Clone Repository
```bash
git clone <repository-url>
cd sampleProduct-master
```

### Bước 2: Cấu Hình Environment Variables
```bash
# Copy file .env.example thành .env
cp .env.example .env

# Chỉnh sửa .env nếu cần (password, ports, etc.)
```

### Bước 3: Khởi Động Tất Cả Services
```bash
# Khởi động database và services chính
docker-compose up -d

# Xem logs để kiểm tra
docker-compose logs -f
```

### Bước 4: Truy Cập Ứng Dụng
- **Frontend Shore**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger API Docs**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:8081
  - Email: `admin@local` (xem trong .env)
  - Password: `admin` (xem trong .env)

---

## 🛠️ Cài Đặt Thủ Công (Không Dùng Docker)

### 1️⃣ Cài Đặt PostgreSQL Database

#### Cách 1: Sử dụng Docker chỉ cho Database
```bash
# Khởi động chỉ PostgreSQL
docker-compose up -d postgres

# Database sẽ chạy trên localhost:5432
```

#### Cách 2: Cài PostgreSQL Trực Tiếp
1. Tải PostgreSQL từ: https://www.postgresql.org/download/
2. Cài đặt và ghi nhớ username/password
3. Tạo database:
```sql
CREATE DATABASE productdb;
CREATE USER product WITH PASSWORD 'productpwd';
GRANT ALL PRIVILEGES ON DATABASE productdb TO product;
```

### 2️⃣ Cài Đặt Backend (.NET API)

```bash
cd backend

# Restore dependencies
dotnet restore

# Update connection string trong appsettings.json
# "ConnectionStrings": {
#   "DefaultConnection": "Host=localhost;Port=5432;Database=productdb;Username=product;Password=productpwd"
# }

# Chạy migrations để tạo tables
dotnet ef database update

# Hoặc chỉ cần chạy, migrations sẽ tự động chạy
dotnet run
```

Backend sẽ chạy trên: http://localhost:5000

### 3️⃣ Cài Đặt Frontend (React/Vite)

```bash
cd frontend

# Cài đặt dependencies
npm install
# hoặc
yarn install

# Chạy development server
npm run dev
# hoặc
yarn dev
```

Frontend sẽ chạy trên: http://localhost:3000

---

## 🚢 Cài Đặt Edge Services (Tùy Chọn - Cho Tàu)

Edge Services chạy trên tàu để thu thập dữ liệu từ sensors và đồng bộ lên cloud.

### Bước 1: Cấu Hình Edge Database

```bash
cd edge-services

# Khởi động PostgreSQL cho edge
docker-compose up -d edge-postgres

# Database sẽ chạy trên localhost:5433
```

### Bước 2: Cấu Hình Connection String

Sửa file `edge-services/appsettings.json`:
```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=edge_password"
  }
}
```

### Bước 3: Chạy Edge Collector

```bash
cd edge-services

# Restore dependencies
dotnet restore

# Chạy migrations
dotnet ef database update

# Chạy service
dotnet run --urls "http://localhost:5001"
```

### Bước 4: Cài Đặt Edge Frontend

```bash
cd frontend-edge

# Cài đặt dependencies
npm install

# Chạy development server
npm run dev
```

Edge Frontend sẽ chạy trên: http://localhost:3002

---

## 📁 Cấu Trúc Dự Án

```
sampleProduct-master/
├── backend/                 # Backend API (.NET 8)
│   ├── Controllers/        # API Controllers
│   ├── Models/            # Data Models
│   ├── Data/              # Database Context & Migrations
│   └── appsettings.json   # Configuration
│
├── frontend/               # Frontend Web App (React + Vite)
│   ├── src/
│   │   ├── components/    # React Components
│   │   ├── pages/         # Page Components
│   │   └── services/      # API Services
│   └── package.json
│
├── edge-services/          # Edge Collector Service (.NET 8)
│   ├── Controllers/       # API Controllers
│   ├── Services/          # Background Services
│   ├── Models/            # Data Models
│   └── docker-compose.yml # Edge Database Setup
│
├── frontend-edge/          # Edge Dashboard (React + Vite)
│   ├── src/
│   └── package.json
│
├── frontend-mobile/        # Mobile App (Flutter)
│   └── lib/
│
├── docker-compose.yml      # Main Docker Setup
├── .env.example           # Environment Variables Template
└── SETUP_GUIDE.md         # This file
```

---

## 🔌 Ports Sử Dụng

| Service | Port | Description |
|---------|------|-------------|
| Backend API | 5000 | Main backend API |
| Frontend | 3000 | Shore-based web UI |
| Edge Frontend | 3002 | Vessel-based dashboard |
| Edge Backend | 5001 | Edge data collector API |
| PostgreSQL | 5432 | Main database |
| Edge PostgreSQL | 5433 | Edge database |
| pgAdmin | 8081 | Database admin UI |
| Edge pgAdmin | 5050 | Edge database admin UI |
| Redis | 6379 | Cache & session store |
| MQTT Broker | 1883 | IoT communication |

---

## 🐛 Troubleshooting

### ❌ "Port already in use"
```bash
# Kiểm tra port nào đang được sử dụng
# Windows:
netstat -ano | findstr :5000

# Linux/Mac:
lsof -i :5000

# Dừng service đang chạy hoặc đổi port trong .env
```

### ❌ "Cannot connect to database"
```bash
# Kiểm tra PostgreSQL có chạy không
docker ps

# Xem logs của database
docker-compose logs postgres

# Restart database
docker-compose restart postgres
```

### ❌ "Migration failed"
```bash
cd backend

# Xóa database và tạo lại
dotnet ef database drop
dotnet ef database update

# Hoặc reset migrations
rm -rf Migrations/
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### ❌ Frontend không kết nối được Backend
Kiểm tra file `frontend/.env`:
```env
VITE_API_URL=http://localhost:5000
```

Và file `vite.config.ts`:
```typescript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5000',
      changeOrigin: true
    }
  }
}
```

### ❌ Edge Services không có dữ liệu
Kiểm tra `edge-services/appsettings.json`:
```json
{
  "TelemetrySimulator": {
    "Enabled": true,
    "IntervalSeconds": 5
  }
}
```

---

## 📝 Lưu Ý Quan Trọng

### 🔒 Bảo Mật
- **KHÔNG** commit file `.env` lên GitHub
- Đổi tất cả password mặc định trong production
- Sử dụng HTTPS trong production
- Cấu hình CORS đúng cách

### 🚀 Production Deployment
- Build frontend: `npm run build`
- Build backend: `dotnet publish -c Release`
- Sử dụng reverse proxy (Nginx/Caddy)
- Cấu hình backup database thường xuyên
- Monitor logs và performance

### 🛳️ Edge Deployment
- Edge services chạy trên tàu, không cần internet liên tục
- Dữ liệu được lưu local và sync khi có kết nối
- Cần cấu hình serial ports cho sensors thực
- Test kỹ trước khi deploy lên tàu

---

## 💡 Tips

- Dùng **Docker Desktop** cho development (dễ nhất)
- Cài **VSCode Extensions**: C# Dev Kit, ESLint, Prettier
- Xem **Swagger UI** tại http://localhost:5000/swagger để test API
- Dùng **pgAdmin** để xem database structure và data
- Đọc `QUICK_START.md` trong mỗi folder để biết thêm chi tiết

---

## 📞 Support

Nếu gặp vấn đề, kiểm tra:
1. File `TROUBLESHOOTING.md` trong `edge-services/`
2. Logs: `docker-compose logs -f`
3. Backend logs: `edge-services/logs/`

---

**Happy Coding! ⚓🚢**
