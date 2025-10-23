# 🚀 HƯỚNG DẪN KHỞI ĐỘNG NHANH - MARITIME EDGE SYSTEM

## 📋 Tổng Quan Dự Án

Dự án này bao gồm **2 hệ thống độc lập**:

### 1️⃣ **EDGE SYSTEM** (Chạy trên tàu - Ship-based)
- **Backend**: `edge-services/` - ASP.NET Core Web API (Port 5001)
- **Frontend**: `frontend-edge/` - React + TypeScript (Port 3002)
- **Database**: PostgreSQL (Port 5433)
- **Mục đích**: Thu thập dữ liệu từ cảm biến tàu, hiển thị dashboard cho thuyền trưởng

### 2️⃣ **CLOUD SYSTEM** (Chạy trên bờ - Shore-based)
- **Backend**: `backend/` - ASP.NET Core Web API (Port 5000)
- **Frontend**: `frontend/` - React (Port 3000)
- **Database**: PostgreSQL (Port 5432)
- **Mục đích**: Quản lý đội tàu, phân tích dữ liệu tổng hợp

---

## ⚡ KHỞI ĐỘNG NHANH - EDGE SYSTEM (Khuyến nghị)

### 🎯 Yêu Cầu Hệ Thống
```
✅ .NET 8 SDK
✅ Node.js 20+
✅ PostgreSQL 15+ (hoặc Docker)
✅ 4GB RAM
✅ Windows 10+, macOS, hoặc Linux
```

---

## 🚢 PHẦN 1: KHỞI ĐỘNG EDGE SYSTEM

### **Bước 1: Khởi Động Database (Docker - Khuyến nghị)**

```powershell
# Di chuyển vào thư mục edge-services
cd "F:\NCKH\Product\sampleProduct-master (1)\sampleProduct-master\edge-services"

# Khởi động PostgreSQL + pgAdmin
docker compose up -d edge-postgres edge-pgadmin

# Kiểm tra trạng thái
docker compose ps
```

**✅ Kết quả mong đợi:**
```
NAME                          STATUS              PORTS
maritime-edge-postgres        Up                  0.0.0.0:5433->5432/tcp
maritime-edge-pgadmin         Up                  0.0.0.0:5050->80/tcp
```

**📊 Truy cập pgAdmin:**
- URL: http://localhost:5050
- Email: `admin@edge.local`
- Password: `admin`

**🔗 Kết nối Database trong pgAdmin:**
1. Click "Add New Server"
2. **General Tab**:
   - Name: `Maritime Edge DB`
3. **Connection Tab**:
   - Host: `localhost`
   - Port: `5433`
   - Database: `maritime_edge`
   - Username: `edge_user`
   - Password: `ChangeMe_EdgePassword123!`
4. Click "Save"

---

### **Bước 2: Khởi Động Backend API (Edge Services)**

```powershell
# Di chuyển vào thư mục edge-services
cd "F:\NCKH\Product\sampleProduct-master (1)\sampleProduct-master\edge-services"

# Restore packages
dotnet restore

# Run backend API
dotnet run --urls "http://localhost:5001"
```

**✅ Kết quả mong đợi:**
```
2025-10-19 21:19:16 info: MaritimeEdge.Program[0]
      Applying database migrations...
2025-10-19 21:19:17 info: MaritimeEdge.Program[0]
      Database migrations applied successfully
2025-10-19 21:19:17 info: MaritimeEdge.Services.TelemetrySimulatorService[0]
      Telemetry Simulator Service started
2025-10-19 21:19:17 info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

**🎉 QUAN TRỌNG:** Bạn sẽ thấy log "Telemetry data simulated successfully" mỗi 5 giây - đây là dữ liệu giả lập tự động!

**📡 Kiểm tra API:**
- Swagger UI: http://localhost:5001/swagger
- Health Check: http://localhost:5001/health
- Test API: http://localhost:5001/api/telemetry/position/latest

---

### **Bước 3: Khởi Động Frontend (Edge Dashboard)**

**Mở Terminal MỚI** (giữ backend đang chạy):

```powershell
# Di chuyển vào thư mục frontend-edge
cd "F:\NCKH\Product\sampleProduct-master (1)\sampleProduct-master\frontend-edge"

# Cài đặt dependencies (chỉ lần đầu)
npm install

# Khởi động dev server
npm run dev
```

**✅ Kết quả mong đợi:**
```
  VITE v6.0.1  ready in 1234 ms

  ➜  Local:   http://localhost:3002/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
```

**🎯 Truy cập ứng dụng:**
- **Frontend**: http://localhost:3002
- **Swagger API**: http://localhost:5001/swagger

---

### **🎉 HOÀN TẤT - Edge System Đang Chạy!**

Bạn sẽ thấy:
- ✅ **Dashboard** với dữ liệu giả lập thời gian thực
- ✅ **Position tracking** cập nhật mỗi 5 giây
- ✅ **Engine telemetry** RPM, nhiệt độ, áp suất
- ✅ **Generator status** 3 máy phát điện
- ✅ **Environmental data** nhiệt độ, áp suất, gió

---

## 📊 CÁC TÍNH NĂNG ĐÃ HOÀN THÀNH

### ✅ Backend API (edge-services)
- [x] **TelemetryController** - 9 endpoints cho dữ liệu cảm biến
- [x] **DashboardController** - Thống kê tổng quan
- [x] **CrewController** - Quản lý thuyền viên + chứng chỉ STCW
- [x] **MaintenanceController** - Hệ thống bảo dưỡng ISM Code
- [x] **VoyageController** - Quản lý chuyến đi + hàng hóa
- [x] **ComplianceController** - Nhật ký trực ca + Oil Record Book
- [x] **AlarmsController** - Hệ thống cảnh báo
- [x] **SyncController** - Đồng bộ dữ liệu với bờ
- [x] **TelemetrySimulatorService** - Giả lập dữ liệu tự động mỗi 5 giây

### ✅ Frontend Dashboard (frontend-edge)
- [x] **Dashboard** - Tổng quan với live telemetry
- [x] **Crew Management** - Quản lý thuyền viên chi tiết
- [x] **Maintenance System** - PMS theo ISM Code
- [x] **Navigation** - Bản đồ + định vị (stub)
- [x] **Engine Monitoring** - Giám sát động cơ (stub)
- [x] **Alarms** - Hệ thống cảnh báo (stub)
- [x] **Voyage Tracking** - Theo dõi chuyến đi (stub)
- [x] **Compliance** - Tuân thủ quy chuẩn (stub)
- [x] **Sync Status** - Trạng thái đồng bộ (stub)

### 🗄️ Database (PostgreSQL)
18 bảng đã migrate:
- Position, Navigation, AIS Data
- Engine, Generator, Tank Levels
- Fuel Consumption, Environmental
- Safety Alarms, Crew, Certificates
- Maintenance Tasks, Voyages, Cargo
- Watchkeeping Logs, Oil Record Book
- Sync Queue

---

## 🔄 LUỒNG DỮ LIỆU TỰ ĐỘNG

```
┌─────────────────────────────────────────────────────────────┐
│           TelemetrySimulatorService                          │
│  (Chạy ngầm, cập nhật mỗi 5 giây)                           │
│                                                               │
│  1. GPS Position (Lat/Lon dịch chuyển)                      │
│  2. Navigation Data (Heading, Course, Speed)                 │
│  3. Engine Data (Main Engine + Aux Engine)                   │
│  4. Generator Data (3 máy phát)                              │
│  5. Environmental Data (Nhiệt độ, áp suất, gió)             │
└───────────────────┬─────────────────────────────────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │   PostgreSQL Database  │
        │   (Port 5433)         │
        └───────────┬───────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │   REST API Endpoints  │
        │   (Port 5001)         │
        └───────────┬───────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │  React Frontend       │
        │  (Port 3002)          │
        │  - Dashboard cập nhật │
        │  - Charts realtime    │
        └───────────────────────┘
```

---

## 🛠️ LỆNH QUẢN LÝ HỮU ÍCH

### Backend Commands
```powershell
# Stop backend (Ctrl+C trong terminal)

# Rebuild backend
cd edge-services
dotnet build

# Clean và rebuild
dotnet clean
dotnet build

# Xem migrations
dotnet ef migrations list

# Tạo migration mới
dotnet ef migrations add NewMigration

# Apply migrations thủ công
dotnet ef database update
```

### Frontend Commands
```powershell
# Stop frontend (Ctrl+C trong terminal)

# Rebuild frontend
cd frontend-edge
npm run build

# Xóa cache và reinstall
rm -rf node_modules package-lock.json
npm install

# Type checking
npm run type-check
```

### Database Commands
```powershell
# Stop database
docker compose down edge-postgres

# Restart database
docker compose restart edge-postgres

# Xem logs
docker compose logs edge-postgres -f

# Backup database
docker exec maritime-edge-postgres pg_dump -U edge_user maritime_edge > backup.sql

# Restore database
docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge < backup.sql

# Xóa database và tạo mới
docker compose down -v edge-postgres
docker compose up -d edge-postgres
```

---

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### ❌ Lỗi: Port 5001 đã được sử dụng
```powershell
# Tìm process đang dùng port
Get-Process -Id (Get-NetTCPConnection -LocalPort 5001).OwningProcess

# Kill process
Stop-Process -Id <PID> -Force
```

### ❌ Lỗi: Không kết nối được database
```powershell
# Kiểm tra database đang chạy
docker compose ps edge-postgres

# Xem logs
docker compose logs edge-postgres

# Restart database
docker compose restart edge-postgres
```

### ❌ Lỗi: Frontend không load CSS
```powershell
# Xóa cache và rebuild
cd frontend-edge
rm -rf node_modules .vite dist
npm install
npm run dev
```

### ❌ Lỗi: npm install failed
```powershell
# Dùng legacy peer deps
npm install --legacy-peer-deps

# Hoặc dùng npm 10
npm install
```

---

## 📈 NEXT STEPS - PHÁT TRIỂN TIẾP

### 1. Hoàn thiện các Stub Pages
```
- Navigation Page: Bản đồ tương tác với Leaflet/Mapbox
- Engine Page: Biểu đồ thời gian thực với Recharts
- Alarms Page: Danh sách cảnh báo với âm thanh
- Voyage Page: Lịch sử chuyến đi + báo cáo
- Compliance Page: Nhật ký + báo cáo tuân thủ
- Sync Page: Quản lý đồng bộ dữ liệu
```

### 2. Tích hợp WebSocket/SignalR
```csharp
// Để push real-time updates thay vì polling
builder.Services.AddSignalR();
app.MapHub<TelemetryHub>("/hub/telemetry");
```

### 3. Thêm Sample Data
```sql
-- Chạy file insert-sample-data.sql trong pgAdmin
-- Tạo 4 crew members + 4 maintenance tasks
```

### 4. Deploy Production
```powershell
# Build frontend
cd frontend-edge
npm run build

# Build backend
cd edge-services
dotnet publish -c Release -o ./publish

# Deploy với Docker
docker compose -f docker-compose.prod.yml up -d
```

---

## 📞 HỖ TRỢ

### 🔧 Technical Stack
- Backend: .NET 8, Entity Framework Core, Npgsql
- Frontend: React 19, TypeScript, Vite, Tailwind CSS
- Database: PostgreSQL 15
- Real-time: Background Services (mỗi 5 giây)

### 📚 Documentation
- [Maritime API Documentation](MARITIME_API_DOCUMENTATION.md)
- [Maritime Architecture](MARITIME_ARCHITECTURE.md)
- [Edge Services README](edge-services/README.md)

### 🐛 Issues
- GitHub Issues: https://github.com/hieubuiVMUS2K4/sampleProduct/issues

---

## 🎯 CHECKLIST KHỞI ĐỘNG

- [ ] Database running (port 5433)
- [ ] pgAdmin accessible (port 5050)
- [ ] Backend API running (port 5001)
- [ ] Swagger UI accessible (http://localhost:5001/swagger)
- [ ] Telemetry simulator running (log mỗi 5 giây)
- [ ] Frontend running (port 3002)
- [ ] Dashboard hiển thị data
- [ ] No console errors

---

**🚢 Chúc bạn khởi động thành công! ⚓**

*Nếu gặp vấn đề, hãy kiểm tra logs của từng service và đảm bảo tất cả ports không bị xung đột.*
