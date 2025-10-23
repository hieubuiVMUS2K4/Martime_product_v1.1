# 🚢 HƯỚNG DẪN CÀI ĐẶT DỰ ÁN - MARITIME MANAGEMENT SYSTEM

> **Hướng dẫn chi tiết từng bước để chạy dự án lần đầu tiên**

---

## 📋 MỤC LỤC

1. [Yêu Cầu Hệ Thống](#1-yêu-cầu-hệ-thống)
2. [Clone Dự Án](#2-clone-dự-án)
3. [Cài Đặt Database](#3-cài-đặt-database)
4. [Cài Đặt Backend](#4-cài-đặt-backend)
5. [Cài Đặt Frontend](#5-cài-đặt-frontend)
6. [Kiểm Tra Hoạt Động](#6-kiểm-tra-hoạt-động)
7. [Xử Lý Lỗi Thường Gặp](#7-xử-lý-lỗi-thường-gặp)

---

## 1️⃣ YÊU CẦU HỆ THỐNG

### ✅ Phần mềm cần cài đặt:

#### **Bắt buộc:**
- **Docker Desktop** (khuyến nghị - cài database dễ dàng)
  - Download: https://www.docker.com/products/docker-desktop/
  - Yêu cầu: Windows 10/11, macOS, hoặc Linux
  
- **.NET 8.0 SDK** (cho backend)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Kiểm tra version: `dotnet --version`
  
- **Node.js 20+** (cho frontend)
  - Download: https://nodejs.org/
  - Kiểm tra version: `node --version`

#### **Tùy chọn:**
- **Git** (để clone repository)
  - Download: https://git-scm.com/downloads
- **Visual Studio Code** (editor khuyến nghị)
  - Download: https://code.visualstudio.com/

### 💾 Yêu cầu phần cứng:
- RAM: Tối thiểu 4GB (khuyến nghị 8GB)
- Ổ cứng: ~5GB trống
- CPU: Bất kỳ (khuyến nghị 2 cores trở lên)

---

## 2️⃣ CLONE DỰ ÁN

### Bước 2.1: Clone repository

```bash
# Clone dự án từ GitHub
git clone https://github.com/tinh97658-hash/Martime_product_v1.git

# Di chuyển vào thư mục dự án
cd Martime_product_v1
```

### Bước 2.2: Kiểm tra cấu trúc thư mục

Bạn sẽ thấy các folder chính:
```
Martime_product_v1/
├── backend/              # Backend API chính (Shore/Cloud)
├── frontend/             # Frontend chính (Shore/Cloud)
├── edge-services/        # Backend cho tàu (Ship/Edge)
├── frontend-edge/        # Frontend cho tàu (Ship/Edge)
├── frontend-mobile/      # Mobile app (Flutter)
└── docker-compose.yml    # Docker configuration
```

**🎯 LƯU Ý:** Hướng dẫn này tập trung vào **Edge System** (chạy trên tàu) vì đây là phần hoàn chỉnh nhất.

---

## 3️⃣ CÀI ĐẶT DATABASE (QUAN TRỌNG!)

### ⚠️ Tại sao cần Database?

Dự án sử dụng **PostgreSQL** để lưu trữ:
- Dữ liệu cảm biến (GPS, Engine, Environmental)
- Thông tin thuyền viên (Crew)
- Lịch bảo dưỡng (Maintenance)
- Cảnh báo (Alarms)
- Nhật ký hoạt động (Logs)

### 🐳 Option 1: Sử dụng Docker (KHUYẾN NGHỊ - DỄ NHẤT)

#### Bước 3.1: Khởi động Docker Desktop

1. Mở **Docker Desktop**
2. Đợi Docker khởi động hoàn tất (biểu tượng Docker ở system tray không còn animation)
3. **QUAN TRỌNG:** Docker phải đang chạy trước khi làm bước tiếp theo!

#### Bước 3.2: Di chuyển vào thư mục edge-services

```bash
# Windows PowerShell
cd edge-services

# Hoặc nếu đường dẫn có khoảng trắng
cd "đường\dẫn\đến\edge-services"
```

#### Bước 3.3: Tạo file .env (Quan trọng!)

```bash
# Tạo file .env từ template
copy .env.example .env

# Linux/Mac:
cp .env.example .env
```

**Nếu không có file .env.example**, tạo file `.env` với nội dung:

```env
# Database Configuration
EDGE_POSTGRES_DB=maritime_edge
EDGE_POSTGRES_USER=edge_user
EDGE_POSTGRES_PASSWORD=edge_password
EDGE_POSTGRES_PORT=5433

# pgAdmin Configuration
EDGE_PGADMIN_EMAIL=admin@edge.local
EDGE_PGADMIN_PASSWORD=admin
EDGE_PGADMIN_PORT=5050
```

#### Bước 3.4: Khởi động PostgreSQL Database

```bash
# Khởi động database và pgAdmin
docker compose up -d edge-postgres edge-pgadmin
```

**✅ Output mong đợi:**
```
[+] Running 3/3
 ✔ Network edge-services_edge-network      Created
 ✔ Container maritime-edge-postgres        Started
 ✔ Container maritime-edge-pgadmin         Started
```

#### Bước 3.5: Kiểm tra Database đã chạy

```bash
docker compose ps
```

**✅ Kết quả mong đợi:**
```
NAME                          STATUS              PORTS
maritime-edge-postgres        Up (healthy)        0.0.0.0:5433->5432/tcp
maritime-edge-pgadmin         Up                  0.0.0.0:5050->80/tcp
```

**Nếu Status là "Up (healthy)" → Database đã sẵn sàng! ✅**

---

### 🖥️ Option 2: Cài PostgreSQL Thủ Công (Không dùng Docker)

<details>
<summary>Click để xem hướng dẫn cài thủ công</summary>

#### Bước 3.1: Tải PostgreSQL

- **Windows**: https://www.postgresql.org/download/windows/
- **macOS**: https://postgresapp.com/ hoặc `brew install postgresql@15`
- **Linux**: `sudo apt install postgresql-15`

#### Bước 3.2: Cài đặt PostgreSQL

1. Chạy installer
2. **Ghi nhớ password cho user `postgres`** khi được hỏi
3. Port mặc định: `5432` (giữ nguyên)
4. Hoàn tất cài đặt

#### Bước 3.3: Tạo Database và User

Mở **pgAdmin** hoặc **psql** và chạy:

```sql
-- Tạo user mới
CREATE USER edge_user WITH PASSWORD 'edge_password';

-- Tạo database
CREATE DATABASE maritime_edge;

-- Cấp quyền cho user
GRANT ALL PRIVILEGES ON DATABASE maritime_edge TO edge_user;

-- Cấp quyền schema (PostgreSQL 15+)
\c maritime_edge
GRANT ALL ON SCHEMA public TO edge_user;
```

#### Bước 3.4: Cập nhật Connection String

Sửa file `edge-services/appsettings.json`:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=maritime_edge;Username=edge_user;Password=edge_password"
  }
}
```

</details>

---

### 🔍 Bước 3.6: Xác Minh Database (Quan Trọng!)

#### Option A: Qua pgAdmin Web Interface

1. Mở trình duyệt, truy cập: **http://localhost:5050**
2. Đăng nhập:
   - Email: `admin@edge.local`
   - Password: `admin`
3. Click **"Add New Server"** hoặc chuột phải **Servers** → **Register** → **Server**

4. **Tab "General":**
   - Name: `Maritime Edge DB` (đặt tên gì cũng được)

5. **Tab "Connection":**
   ```
   Host name/address: edge-postgres
   Port: 5432
   Maintenance database: maritime_edge
   Username: edge_user
   Password: edge_password
   ```
   
   **⚠️ LƯU Ý:**
   - Dùng `edge-postgres` (tên container), KHÔNG phải `localhost`
   - Port là `5432` (internal), KHÔNG phải `5433`
   - Tick "Save password"

6. Click **"Save"**

**✅ Nếu kết nối thành công** → Bạn sẽ thấy database `maritime_edge` ở sidebar bên trái!

#### Option B: Qua Command Line

```bash
# Test connection từ container
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT version();"
```

**✅ Output mong đợi:** Hiển thị version của PostgreSQL

---

## 4️⃣ CÀI ĐẶT BACKEND (Edge Services)

### Bước 4.1: Di chuyển vào thư mục backend

```bash
cd edge-services
```

### Bước 4.2: Kiểm tra Connection String

Mở file `appsettings.json` và xác nhận:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=edge_password"
  }
}
```

**⚠️ CHÚ Ý:**
- Nếu dùng Docker: `Host=localhost`, `Port=5433` ✅
- Nếu cài PostgreSQL thủ công: `Host=localhost`, `Port=5432`

### Bước 4.3: Restore NuGet Packages

```bash
dotnet restore
```

**✅ Output mong đợi:**
```
Restore succeeded.
```

### Bước 4.4: Chạy Database Migrations (TỰ ĐỘNG TẠO TABLES)

**🎉 TIN TỐT:** Bạn **KHÔNG CẦN** chạy migrations thủ công!

Backend được cấu hình để **tự động** chạy migrations khi khởi động lần đầu.

Code trong `Program.cs`:
```csharp
// Tự động apply migrations khi backend start
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
    await dbContext.Database.MigrateAsync(); // ← Tự động tạo tables
}
```

### Bước 4.5: Khởi động Backend

**Cho Desktop/Web Frontend:**
```bash
dotnet run --urls "http://localhost:5001"
```

**Cho Mobile App (LAN access):**
```bash
dotnet run --urls "http://0.0.0.0:5001"
```

💡 **Lưu ý:** 
- `localhost:5001` - Chỉ truy cập được từ máy local (desktop/web)
- `0.0.0.0:5001` - Truy cập được từ mọi thiết bị trên LAN (mobile app)

**✅ Output mong đợi:**

```
2025-10-22 21:00:00 info: MaritimeEdge.Program[0]
      Applying database migrations...
2025-10-22 21:00:03 info: MaritimeEdge.Program[0]
      Database migrations applied successfully
2025-10-22 21:00:03 info: MaritimeEdge.Services.TelemetrySimulatorService[0]
      Telemetry Simulator Service started
2025-10-22 21:00:03 info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
2025-10-22 21:00:03 info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**🎊 QUAN TRỌNG:** 
- Nếu thấy "Database migrations applied successfully" → Tables đã được tạo! ✅
- Nếu thấy "Telemetry data simulated successfully" mỗi 5 giây → Dữ liệu giả lập đang chạy! ✅

### Bước 4.6: Kiểm tra API hoạt động

Mở trình duyệt và truy cập các URL sau:

1. **Swagger UI (API Documentation):**
   - URL: http://localhost:5001/swagger
   - Bạn sẽ thấy danh sách tất cả API endpoints

2. **Health Check:**
   - URL: http://localhost:5001/health
   - Kết quả: `"Healthy"`

3. **Test API:**
   - URL: http://localhost:5001/api/dashboard/stats
   - Kết quả: JSON với thống kê (crew, alarms, maintenance)

**✅ Nếu tất cả URLs trên hoạt động → Backend đã sẵn sàng!**

---

## 5️⃣ CÀI ĐẶT FRONTEND (Edge Dashboard)

### Bước 5.1: Mở Terminal MỚI

**⚠️ QUAN TRỌNG:** Giữ terminal backend đang chạy, mở terminal mới!

### Bước 5.2: Di chuyển vào thư mục frontend

```bash
cd frontend-edge
```

### Bước 5.3: Cài đặt Node Modules

```bash
npm install
```

**⏱️ Thời gian:** 2-5 phút (tùy tốc độ mạng)

**✅ Output mong đợi:**
```
added 500 packages in 120s
```

**Nếu gặp lỗi:**
```bash
# Thử với legacy peer deps
npm install --legacy-peer-deps
```

### Bước 5.4: Kiểm tra cấu hình API URL

Mở file `vite.config.ts` và xác nhận:

```typescript
export default defineConfig({
  server: {
    port: 3002,
    proxy: {
      '/api': {
        target: 'http://localhost:5001', // Backend URL
        changeOrigin: true,
      }
    }
  }
})
```

### Bước 5.5: Khởi động Frontend Development Server

```bash
npm run dev
```

**✅ Output mong đợi:**

```
  VITE v6.0.1  ready in 1234 ms

  ➜  Local:   http://localhost:3002/
  ➜  Network: http://192.168.1.x:3002/
  ➜  press h + enter to show help
```

### Bước 5.6: Truy cập Dashboard

Mở trình duyệt và truy cập: **http://localhost:3002**

**✅ Bạn sẽ thấy:**
- Dashboard với các thẻ thống kê (Total Alarms, Crew Onboard, etc.)
- Dữ liệu Navigation, Engine, Environmental
- Sidebar với menu điều hướng

**🎉 CHÚC MỪNG! Bạn đã chạy thành công dự án!**

---

## 5️⃣.7 (OPTIONAL) CÀI ĐẶT MOBILE APP (Flutter)

### Bước 5.7.1: Yêu cầu

- **Flutter SDK** (>= 3.0.0)
  - Download: https://flutter.dev/docs/get-started/install
  - Kiểm tra: `flutter doctor`

### Bước 5.7.2: Setup Flutter

```bash
cd frontend-mobile
flutter pub get
```

### Bước 5.7.3: Chạy app trên Windows (để test)

```bash
flutter run -d windows
```

### Bước 5.7.4: Cấu hình Server URL trong App

1. Mở app → Settings → Server Configuration
2. Nhập IP của máy chạy Edge Server:
   ```
   http://192.168.1.100:5001
   ```
3. Click "Save Configuration"
4. Restart app

### Bước 5.7.5: Test Login

**Tài khoản test:**
- Crew ID: `CM001`, `CM002`, `CM003`, v.v. (từ database)
- Password: `password123` (mặc định cho tất cả crew)

**Lưu ý:** Edge Server phải chạy với `http://0.0.0.0:5001` (không phải localhost) để mobile app kết nối được qua LAN:

```bash
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

---

## 6️⃣ KIỂM TRA HOẠT ĐỘNG

### ✅ Checklist Đầy Đủ

- [ ] Docker Desktop đang chạy
- [ ] Database container status = "Up (healthy)"
- [ ] pgAdmin accessible tại http://localhost:5050
- [ ] Có thể kết nối database trong pgAdmin
- [ ] Backend API đang chạy (http://localhost:5001)
- [ ] Swagger UI accessible (http://localhost:5001/swagger)
- [ ] Backend log hiển thị "Telemetry data simulated successfully" mỗi 5 giây
- [ ] Frontend đang chạy (http://localhost:3002)
- [ ] Dashboard hiển thị dữ liệu
- [ ] Console trình duyệt không có lỗi (F12)

### 🔄 Xem Dữ Liệu Tự Động

1. Truy cập pgAdmin: http://localhost:5050
2. Expand: `Maritime Edge DB` → `Databases` → `maritime_edge` → `Schemas` → `public` → `Tables`
3. Click chuột phải vào table `position_data` → **View/Edit Data** → **All Rows**
4. Bạn sẽ thấy dữ liệu GPS được thêm mỗi 5 giây!

### 📊 Kiểm tra Tables đã tạo

**18 tables đã được tạo:**

```sql
-- Telemetry & Sensor Data
position_data
navigation_data
ais_data
engine_data
generator_data
tank_levels
fuel_consumption
environmental_data
nmea_raw_data

-- Operational Data
safety_alarms
crew_members
crew_certificates
maintenance_tasks
voyage_records
cargo_operations

-- Compliance
watchkeeping_logs
oil_record_books

-- System
sync_queue
```

Chạy query trong pgAdmin:

```sql
-- Xem tất cả tables
SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename;

-- Đếm số dòng trong mỗi table
SELECT 
    'position_data' as table_name, COUNT(*) as row_count FROM position_data
UNION ALL
SELECT 'crew_members', COUNT(*) FROM crew_members
UNION ALL
SELECT 'maintenance_tasks', COUNT(*) FROM maintenance_tasks;
```

---

## 7️⃣ XỬ LÝ LỖI THƯỜNG GẶP

### ❌ Lỗi: "Docker daemon is not running"

**Nguyên nhân:** Docker Desktop chưa được khởi động

**Giải pháp:**
1. Mở Docker Desktop
2. Đợi icon Docker ở system tray không còn animation
3. Thử lại lệnh `docker compose up`

---

### ❌ Lỗi: "Port 5433 is already in use"

**Nguyên nhân:** Port đã được sử dụng bởi process khác

**Giải pháp:**

```bash
# Windows: Tìm process đang dùng port
netstat -ano | findstr :5433

# Kill process
taskkill /PID <PID> /F

# Hoặc đổi port trong .env
EDGE_POSTGRES_PORT=5434
```

---

### ❌ Lỗi: "Password authentication failed for user edge_user"

**Nguyên nhân:** Password không khớp giữa Docker và Backend

**Giải pháp:**

**Option 1: Xóa và tạo lại database**

```bash
# Stop và xóa container + volume
docker compose down -v

# Start lại
docker compose up -d edge-postgres edge-pgadmin
```

**Option 2: Đảm bảo password khớp**

File `.env`:
```env
EDGE_POSTGRES_PASSWORD=edge_password
```

File `appsettings.json`:
```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=edge_password"
  }
}
```

---

### ❌ Lỗi: "Cannot connect to PostgreSQL at localhost:5433"

**Nguyên nhân:** Database chưa khởi động hoặc chưa healthy

**Giải pháp:**

```bash
# Kiểm tra status
docker compose ps

# Xem logs
docker compose logs edge-postgres

# Restart nếu cần
docker compose restart edge-postgres

# Đợi database healthy (10-30 giây)
```

---

### ❌ Lỗi: "Migration failed" khi chạy Backend

**Nguyên nhân:** Database chưa sẵn sàng hoặc connection string sai

**Giải pháp:**

1. **Kiểm tra database đang chạy:**
```bash
docker compose ps edge-postgres
# Status phải là "Up (healthy)"
```

2. **Test connection thủ công:**
```bash
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT 1;"
```

3. **Chạy migrations thủ công (nếu cần):**
```bash
cd edge-services
dotnet ef database update
```

---

### ❌ Lỗi: "npm install failed" hoặc dependency conflicts

**Giải pháp:**

```bash
cd frontend-edge

# Xóa cache
rm -rf node_modules package-lock.json
# Windows:
rmdir /s /q node_modules
del package-lock.json

# Cài lại với legacy peer deps
npm install --legacy-peer-deps
```

---

### ❌ Lỗi: "EADDRINUSE: Port 3002 already in use"

**Giải pháp:**

```bash
# Windows: Tìm và kill process
netstat -ano | findstr :3002
taskkill /PID <PID> /F

# Hoặc đổi port trong vite.config.ts
server: {
  port: 3003, // Đổi port
}
```

---

### ❌ Frontend hiển thị "N/A" cho tất cả dữ liệu

**Nguyên nhân:** Backend chưa chạy hoặc CORS issue

**Giải pháp:**

1. Kiểm tra backend đang chạy: http://localhost:5001/swagger
2. Kiểm tra console browser (F12) có lỗi CORS không
3. Test API trực tiếp: http://localhost:5001/api/dashboard/stats
4. Đảm bảo backend log hiển thị "Telemetry data simulated successfully"

---

### ❌ pgAdmin không kết nối được database

**Giải pháp:**

Khi add server trong pgAdmin, phải dùng:

```
Host: edge-postgres      ← Tên container, KHÔNG phải localhost
Port: 5432              ← Internal port, KHÔNG phải 5433
```

**Tại sao?** pgAdmin chạy TRONG Docker network, phải dùng tên container.

---

## 📚 TÀI LIỆU THAM KHẢO

### File Hướng Dẫn Khác:
- `QUICK_START_GUIDE.md` - Hướng dẫn nhanh
- `SETUP_GUIDE.md` - Hướng dẫn setup tổng quan
- `edge-services/README.md` - Chi tiết về Edge Services
- `MARITIME_API_DOCUMENTATION.md` - API Documentation

### Các Port Sử Dụng:

| Service | Port | URL |
|---------|------|-----|
| Backend API | 5001 | http://localhost:5001 |
| Frontend | 3002 | http://localhost:3002 |
| PostgreSQL | 5433 | localhost:5433 (from host) |
| pgAdmin | 5050 | http://localhost:5050 |

### Thông Tin Đăng Nhập:

**pgAdmin:**
- URL: http://localhost:5050
- Email: `admin@edge.local`
- Password: `admin`

**Database trong pgAdmin:**
- Host: `edge-postgres`
- Port: `5432`
- Database: `maritime_edge`
- Username: `edge_user`
- Password: `edge_password`

---

## 🎯 BƯỚC TIẾP THEO

Sau khi chạy thành công:

1. **Khám phá Dashboard:**
   - Xem dữ liệu real-time cập nhật mỗi 5 giây
   - Truy cập các trang: Crew, Maintenance, Alarms

2. **Xem Database:**
   - Mở pgAdmin và explore các tables
   - Xem dữ liệu được thêm tự động

3. **Thử API:**
   - Truy cập Swagger UI: http://localhost:5001/swagger
   - Test các endpoints khác nhau

4. **Test Mobile App:**
   - Xem: `frontend-mobile/COMPLETE_GUIDE.md`
   - Hoặc chạy: `START_MOBILE_APP.bat`

5. **Tìm hiểu Code:**
   - Backend: `edge-services/Controllers/`
   - Frontend: `frontend-edge/src/pages/`
   - Mobile: `frontend-mobile/lib/`
   - Database Models: `edge-services/Models/`

---

## 💡 TIPS HỮU ÍCH

### Stop và Start lại dự án:

```bash
# Stop Backend (Ctrl+C trong terminal backend)
# Stop Frontend (Ctrl+C trong terminal frontend)

# Stop Database
docker compose down

# Start lại Database
docker compose up -d edge-postgres edge-pgadmin

# Start lại Backend
cd edge-services
dotnet run --urls "http://localhost:5001"

# Start lại Frontend (terminal khác)
cd frontend-edge
npm run dev
```

### Xem Logs:

```bash
# Database logs
docker compose logs edge-postgres -f

# Backend logs: Xem trực tiếp trong terminal

# Frontend logs: Xem trực tiếp trong terminal
```

### Reset Database (Xóa tất cả data):

```bash
# Stop và xóa volume
docker compose down -v

# Start lại
docker compose up -d edge-postgres edge-pgadmin

# Chạy backend để tạo lại tables
cd edge-services
dotnet run --urls "http://localhost:5001"
```

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề không có trong tài liệu:

1. Kiểm tra logs của service bị lỗi
2. Đảm bảo tất cả ports không bị xung đột
3. Restart Docker Desktop nếu cần
4. Tạo GitHub Issue với log chi tiết

---

**🎉 CHÚC BẠN THÀNH CÔNG!**

Nếu đã chạy được đến đây, bạn đã hoàn thành 100% quá trình setup! 🚢⚓
