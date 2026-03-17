# HƯỚNG DẪN TRIỂN KHAI SHORE LÊN SERVER & KẾT NỐI EDGE

> **Server IP:** `27.71.17.165`  
> **Username:** `labuser`  
> **Shore Port:** `80` (Frontend + Nginx Reverse Proxy → Backend :5000)  
> **Sync API:** `http://27.71.17.165/api/sync`

---

## 1. PHÂN TÍCH CẤU TRÚC SHORE ↔ EDGE

### 1.1 Tổng quan kiến trúc

```
┌─────────────────────────────────────┐       ┌──────────────────────────────────┐
│          SERVER (SHORE)             │       │        MÁY LOCAL (EDGE)          │
│       IP: 27.71.17.165             │       │       IP: máy cá nhân            │
│                                     │       │                                  │
│  ┌──────────┐   ┌────────────────┐  │       │  ┌────────────────────────────┐  │
│  │ Frontend  │   │  Backend API   │  │       │  │    Edge Collector (.NET)   │  │
│  │ (Nginx)   │──▶│  (.NET 8)      │  │       │  │    Port 5001              │  │
│  │ Port 80   │   │  Port 5000     │  │       │  └──────────┬───────────────┘  │
│  └──────────┘   └───────┬────────┘  │       │             │                   │
│                         │            │  HTTP │             │ HTTP              │
│                 ┌───────▼────────┐   │◀──────┼─────────────┘                   │
│                 │  PostgreSQL    │   │       │  ┌────────────────────────────┐  │
│                 │  Port 5434     │   │       │  │  PostgreSQL (Edge)         │  │
│                 └────────────────┘   │       │  │  Port 5433                 │  │
│                                      │       │  └────────────────────────────┘  │
└──────────────────────────────────────┘       └──────────────────────────────────┘
```

### 1.2 Cách Shore và Edge giao tiếp

**Giao thức:** HTTP REST API (JSON)

**Chiều đẩy dữ liệu (Edge → Shore) - PUSH:**
1. Edge thay đổi dữ liệu → ghi vào bảng `sync_queue`
2. `SyncBackgroundWorker` chạy nền mỗi 30 giây
3. Gom batch (tối đa 100 items) → `POST /api/sync` lên Shore
4. Shore nhận → xử lý qua `SyncInboxService` → lưu DB → trả kết quả

**Chiều kéo dữ liệu (Shore → Edge) - PULL:**
1. Shore thay đổi dữ liệu → ghi vào bảng `sync_outbox`
2. Edge gọi `GET /api/sync/pull?nodeId={IMO}&since={lastPull}` định kỳ
3. Shore trả danh sách thay đổi (phân trang cursor)
4. Edge áp dụng thay đổi → gửi `POST /api/sync/acknowledge` xác nhận

**Heartbeat:** Edge gửi `POST /api/sync/heartbeat` để Shore biết tàu đang online

```
Edge ──── POST /api/sync ──────────────────▶ Shore (Push dữ liệu)
Edge ──── GET  /api/sync/pull ─────────────▶ Shore (Pull dữ liệu)  
Edge ──── POST /api/sync/acknowledge ──────▶ Shore (Xác nhận đã nhận)
Edge ──── POST /api/sync/heartbeat ────────▶ Shore (Báo hiệu online)
```

> **Lưu ý:** Shore KHÔNG BAO GIỜ gọi trực tiếp đến Edge. Mọi giao tiếp đều do Edge chủ động khởi tạo (pull-based). Điều này phù hợp thực tế vì tàu có thể offline bất cứ lúc nào.

---

## 2. TRIỂN KHAI SHORE LÊN SERVER

### 2.1 Yêu cầu server

- **OS:** Ubuntu 20.04+ hoặc Windows Server
- **Docker:** Docker Engine 20+ và Docker Compose V2
- **RAM:** Tối thiểu 2GB
- **Ports mở:** 80 (HTTP), 5434 (PostgreSQL - tùy chọn), 8081 (pgAdmin - tùy chọn)

### 2.2 Cài đặt Docker trên server (nếu chưa có)

```bash
# SSH vào server
ssh labuser@27.71.17.165

# Cài Docker (Ubuntu)
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker labuser

# Cài Docker Compose plugin
sudo apt-get install docker-compose-plugin

# Kiểm tra
docker --version
docker compose version
```

### 2.3 Đưa mã nguồn lên server

**Cách 1: Git clone (khuyên dùng)**
```bash
ssh labuser@27.71.17.165

# Clone repo
git clone https://github.com/hieubuiVMUS2K4/Martime_product_2.0.git
cd Martime_product_2.0/shore_product
git checkout feature/tinht
```

**Cách 2: SCP từ máy local**
```powershell
# Từ máy local Windows, copy shore_product lên server
scp -r F:\NCKH\Product\Martime_product_v1.1\shore_product labuser@27.71.17.165:~/shore_product
```

### 2.4 Cấu hình và chạy

```bash
# SSH vào server
ssh labuser@27.71.17.165
cd shore_product   # hoặc Martime_product_2.0/shore_product

# Copy file env production (đổi mật khẩu nếu cần)
cp .env.production .env

# Build và chạy tất cả services
docker compose -f docker-compose.prod.yml up -d --build

# Kiểm tra trạng thái
docker compose -f docker-compose.prod.yml ps

# Xem logs
docker compose -f docker-compose.prod.yml logs -f backend
docker compose -f docker-compose.prod.yml logs -f frontend
```

### 2.5 Kiểm tra Shore đã chạy

```bash
# Từ trên server
curl http://localhost/api/health    # Nếu có health endpoint
curl http://localhost:5000/api/sync  # Backend trực tiếp

# Từ máy local (trình duyệt)
# Mở: http://27.71.17.165
# → Phải thấy giao diện Shore Frontend
```

### 2.6 Mở firewall (nếu cần)

```bash
# Ubuntu UFW
sudo ufw allow 80/tcp      # Frontend + API qua nginx
sudo ufw allow 22/tcp      # SSH (giữ mở!)
# sudo ufw allow 5434/tcp  # PostgreSQL (chỉ mở nếu cần truy cập DB từ xa)
sudo ufw enable
```

---

## 3. CẤU HÌNH EDGE KẾT NỐI VỚI SHORE

### 3.1 Sửa appsettings.json của Edge

Mở file `edge_product/edge-services/appsettings.json`, sửa phần `ShoreAPI`:

```json
"ShoreAPI": {
    "BaseUrl": "http://27.71.17.165",
    "ApiKey": "your-api-key-here",
    "VesselId": "vessel-guid-here",
    "Timeout": 30,
    "Enabled": true
}
```

**Giải thích:**
- `BaseUrl` đổi từ `http://localhost:5000` → `http://27.71.17.165`
- Port 80 là mặc định HTTP nên không cần ghi `:80`
- Nginx trên server sẽ proxy `/api/*` → backend container port 5000

**Hoặc** nếu muốn gọi thẳng backend (không qua nginx):
```json
"ShoreAPI": {
    "BaseUrl": "http://27.71.17.165:5000",
    ...
}
```
> Lưu ý: Nếu dùng cách này thì cần expose port 5000 trong docker-compose.prod.yml

### 3.2 Tạo appsettings.Production.json cho Edge (tùy chọn)

Thay vì sửa appsettings.json gốc, tạo file override:

```json
{
  "ShoreAPI": {
    "BaseUrl": "http://27.71.17.165",
    "Enabled": true
  }
}
```

Chạy edge với environment Production:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project edge_product/edge-services/EdgeCollector.csproj
```

### 3.3 Kiểm tra kết nối Edge → Shore

```powershell
# Test từ máy local - kiểm tra Shore API có phản hồi không
Invoke-RestMethod -Uri "http://27.71.17.165/api/sync/pull?nodeId=9876543&since=2024-01-01T00:00:00Z" -Method GET

# Hoặc dùng curl
curl "http://27.71.17.165/api/sync/pull?nodeId=9876543&since=2024-01-01T00:00:00Z"
```

---

## 4. CẤU TRÚC PORT TỔNG HỢP

| Service | Container Port | Host Port (Server) | Truy cập từ ngoài |
|---------|---------------|--------------------|--------------------|
| Frontend (Nginx) | 80 | **80** | `http://27.71.17.165` |
| Backend API | 5000 | Không expose (*) | Qua nginx `/api/*` |
| PostgreSQL | 5432 | 5434 | `27.71.17.165:5434` (tùy chọn) |
| pgAdmin | 80 | 8081 | `27.71.17.165:8081` (tùy chọn) |

(*) Backend không cần expose port ra ngoài vì Nginx đã reverse proxy `/api/` → `http://backend:5000/api/`

---

## 5. EXPOSE BACKEND PORT (NẾU CẦN)

Nếu muốn Edge kết nối trực tiếp backend port 5000 (không qua nginx), sửa `docker-compose.prod.yml`:

```yaml
backend:
    ...
    ports:
      - "5000:5000"    # Thêm dòng này
    ...
```

Khi đó Edge config:
```json
"ShoreAPI": {
    "BaseUrl": "http://27.71.17.165:5000"
}
```

---

## 6. TROUBLESHOOTING

### Edge không kết nối được Shore

1. **Kiểm tra server có chạy không:**
   ```powershell
   Test-NetConnection -ComputerName 27.71.17.165 -Port 80
   ```

2. **Kiểm tra API phản hồi:**
   ```powershell
   Invoke-WebRequest -Uri "http://27.71.17.165/api/sync" -Method POST -ContentType "application/json" -Body "[]"
   ```

3. **Kiểm tra firewall server:**
   ```bash
   # Trên server
   sudo ufw status
   sudo iptables -L -n | grep 80
   ```

4. **Xem logs backend trên server:**
   ```bash
   docker compose -f docker-compose.prod.yml logs -f backend
   ```

5. **Kiểm tra DNS/IP:**
   ```powershell
   ping 27.71.17.165
   ```

### Sync không hoạt động

1. **Kiểm tra `ShoreAPI.Enabled` = true** trong appsettings.json
2. **Kiểm tra `Sync.NetworkType`** = `"Shore_WiFi"` (cho phép tất cả priority)
3. **Xem logs Edge:**
   ```powershell
   # Edge sẽ log sync errors trong console output
   dotnet run --project edge_product/edge-services/EdgeCollector.csproj
   ```

### Database chưa có dữ liệu

```bash
# Chạy seed data trên server
docker compose -f docker-compose.prod.yml exec -T postgres psql -U product -d productdb < seed-data.sql
docker compose -f docker-compose.prod.yml exec -T postgres psql -U product -d productdb < seed-crew-data.sql
```

---

## 7. TÓM TẮT CÁC BƯỚC THỰC HIỆN

| # | Bước | Nơi thực hiện |
|---|------|---------------|
| 1 | Cài Docker trên server | Server `27.71.17.165` |
| 2 | Copy/clone mã nguồn lên server | Server |
| 3 | Chạy `docker compose -f docker-compose.prod.yml up -d --build` | Server |
| 4 | Kiểm tra `http://27.71.17.165` trên trình duyệt | Máy local |
| 5 | Sửa `ShoreAPI.BaseUrl` = `http://27.71.17.165` | Máy local (Edge appsettings) |
| 6 | Chạy Edge và kiểm tra sync | Máy local |
