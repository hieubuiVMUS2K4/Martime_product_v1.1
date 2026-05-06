# 🗺️ Kế Hoạch Hoàn Thiện GPS Sync + Map Realtime

> **Dự án:** Maritime Management System — Edge-Shore Architecture  
> **Ngày:** 04/05/2026  
> **Branch:** feature/tinhai  

---

## Mục Lục

1. [Tổng Quan](#1-tổng-quan)
2. [Phân Tích Hiện Trạng](#2-phân-tích-hiện-trạng)
3. [Sơ Đồ Kiến Trúc](#3-sơ-đồ-kiến-trúc)
4. [Giai Đoạn 1: GPS Collector Service (TCP)](#4-giai-đoạn-1-gps-collector-service-tcp)
5. [Giai Đoạn 2: Position Sync Enqueuer](#5-giai-đoạn-2-position-sync-enqueuer)
6. [Giai Đoạn 3: Hiển Thị Bản Đồ Realtime](#6-giai-đoạn-3-hiển-thị-bản-đồ-realtime)
7. [Giai Đoạn 4: Tích Hợp & Kiểm Thử](#7-giai-đoạn-4-tích-hợp--kiểm-thử)
8. [Danh Sách File Cần Tạo/Sửa](#8-danh-sách-file-cần-tạosửa)

---

## 1. Tổng Quan

### Mục tiêu
1. **Edge:** Xây dựng service nền đọc dữ liệu GPS thật qua TCP, parse NMEA, lưu local DB, tự động sync lên Shore
2. **Shore:** Hiển thị bản đồ realtime vị trí tàu với marker, popup thông tin, và polyline hành trình

### Tech Stack
| Layer | Công nghệ |
|---|---|
| GPS Collector | C# .NET 8 BackgroundService + TcpClient |
| NMEA Parser | C# (đã có sẵn `NmeaParser.cs`) |
| Sync | SyncQueue → SyncBackgroundWorker → Shore SyncInbox |
| Frontend Map | React 19 + Leaflet + react-leaflet |
| Map Tiles | OpenStreetMap (miễn phí) |

---

## 2. Phân Tích Hiện Trạng

### 2.1 ĐÃ CÓ SẴN ✅

| # | Thành phần | File | Trạng thái |
|---|---|---|---|
| 1 | **NmeaParser** | `edge-services/Services/Parsers/NmeaParser.cs` | ✅ Hỗ trợ GGA, RMC, VTG, HDT, ROT, DPT, DBT, MTW, MWV, MWD, VHW. Có ValidateChecksum, ParseLatitude/Longitude |
| 2 | **TelemetrySimulatorService** | `edge-services/Services/Voyage/TelemetrySimulatorService.cs` | ✅ Giả lập GPS mỗi 60s, lưu PositionData |
| 3 | **NmeaPlaybackService** | `edge-services/Services/Voyage/NmeaPlaybackService.cs` | ✅ Đọc file NMEA, parse từng dòng |
| 4 | **SignalKDataCollectorService** | `edge-services/Services/Voyage/SignalKDataCollectorService.cs` | ✅ Thu thập từ SignalK HTTP API |
| 5 | **PositionData Model (Edge)** | `edge-services/Models/EdgeModels.cs` | ✅ Lat, Lon, SOG, COG, Altitude, Satellites, Hdop, FixQuality, IsSynced |
| 6 | **PositionData Model (Shore)** | `shore/backend/Models/SyncModels.cs` | ✅ Lat, Lon, SOG, COG, Source, OriginNode |
| 7 | **SyncService** | `edge-services/Services/Core/SyncService.cs` | ✅ Push batch, Pull cursor, file transfer, token bucket, adaptive batch |
| 8 | **SyncBackgroundWorker** | `edge-services/Services/Core/SyncBackgroundWorker.cs` | ✅ Push/Pull/Heartbeat schedule |
| 9 | **SyncInboxService** | `shore/backend/Services/Sync/SyncInboxService.cs` | ✅ Đã map `position_data` → `PositionData` |
| 10 | **ConflictResolver** | `shore/backend/Services/Sync/ConflictResolverService.cs` | ✅ `position_data` → Edge thắng |
| 11 | **TelemetryController (Edge)** | `edge-services/Controllers/Voyage/TelemetryController.cs` | ✅ GET /position/latest, /position/history |
| 12 | **VesselTelemetryController (Shore)** | `shore/Controllers/VesselTelemetryController.cs` | ✅ GET vessel/{id}/position, /route |
| 13 | **Edge DB Config** | `edge-services/Data/EdgeDbContext.cs` | ✅ PositionData table, indexes, cleanup |

### 2.2 CÒN THIẾU ❌

| # | Thành phần | Mô tả |
|---|---|---|
| 1 | **GPS Collector Service** | Service nền kết nối TCP tới thiết bị GPS thật, đọc NMEA stream |
| 2 | **Position Sync Enqueuer** | Tự động enqueue PositionData vào SyncQueue để push lên Shore |
| 3 | **NavigationPage (Edge)** | Đang là stub "Coming Soon", chưa có bản đồ |
| 4 | **VesselTrackingPage (Shore)** | Chưa có trang hiển thị bản đồ vị trí tàu |
| 5 | **Leaflet Map Component** | Chưa cài đặt thư viện bản đồ |
| 6 | **Realtime polling** | Chưa có cơ chế tự động cập nhật marker |

---

## 3. Sơ Đồ Kiến Trúc

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           EDGE SYSTEM (Tàu)                             │
│                                                                          │
│  ┌──────────────────┐                                                   │
│  │  Thiết bị GPS    │  TCP Server (VD: 192.168.1.200:2947)             │
│  │  (gpsd / NMEA)   │                                                   │
│  └────────┬─────────┘                                                   │
│           │ TCP Stream ($GPRMC, $GPGGA, ...)                            │
│  ┌────────▼─────────┐                                                   │
│  │ GpsCollector     │  🆕 BackgroundService                             │
│  │ Service          │  • TcpClient kết nối                              │
│  │                  │  • Buffer xử lý phân mảnh (\r\n)                  │
│  │                  │  • Auto-reconnect (exponential backoff)           │
│  └────────┬─────────┘                                                   │
│           │ Raw NMEA sentences                                          │
│  ┌────────▼─────────┐                                                   │
│  │  NmeaParser      │  ✅ ĐÃ CÓ                                         │
│  │  • $GPRMC → Lat, Lon, SOG, COG, Time                                │
│  │  • $GPGGA → Altitude, Satellites, FixQuality                        │
│  │  • ValidateChecksum → bỏ qua invalid                                │
│  └────────┬─────────┘                                                   │
│           │ PositionData object                                         │
│  ┌────────▼─────────┐                                                   │
│  │ EdgeDbContext    │  ✅ position_data table                            │
│  │ PositionData     │  IsSynced = false                                 │
│  └────────┬─────────┘                                                   │
│           │                                                              │
│  ┌────────▼─────────┐                                                   │
│  │PositionSyncEnq   │  🆕 BackgroundService                             │
│  │ Service          │  • Quét PositionData IsSynced=false mỗi 30s       │
│  │                  │  • Đóng gói → SyncQueue                           │
│  └────────┬─────────┘                                                   │
│           │                                                              │
│  ┌────────▼─────────┐                                                   │
│  │ SyncBackground   │  ✅ ĐÃ CÓ                                         │
│  │ Worker           │  • Push batch lên Shore                           │
│  │                  │  • Priority-based filtering                       │
│  │                  │  • Retry + DLQ                                    │
│  └────────┬─────────┘                                                   │
│           │                                                              │
│  ┌────────▼─────────┐                                                   │
│  │ NavigationPage   │  🔧 CẬP NHẬT (từ stub → Leaflet Map)             │
│  │ + Leaflet Map    │  • Marker vị trí hiện tại                         │
│  │                  │  • Popup: tốc độ, hướng, thời gian                │
│  │                  │  • Polyline hành trình                             │
│  └──────────────────┘                                                   │
└──────────────────────────────────┬──────────────────────────────────────┘
                                   │ POST /api/sync (JSON batch)
                                   │ SyncQueueItemDto[]
                                   ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                          SHORE SYSTEM (Bờ)                               │
│                                                                           │
│  ┌──────────────────┐                                                    │
│  │ SyncInboxService │  ✅ ĐÃ CÓ                                          │
│  │  • ProcessBatch  │  • Deserialize position_data → PositionData        │
│  │  • Idempotency   │  • ConflictResolver (Edge thắng)                   │
│  │  • Save to DB    │  • Lưu vào shore position_data                     │
│  └────────┬─────────┘                                                    │
│           │                                                               │
│  ┌────────▼─────────┐                                                    │
│  │ VesselTelemetry  │  🔧 CẬP NHẬT (thêm endpoint realtime)             │
│  │ Controller       │  GET /api/VesselTelemetry/vessel/{id}/realtime     │
│  │                  │      → { latest, route[] }                          │
│  └────────┬─────────┘                                                    │
│           │                                                               │
│  ┌────────▼─────────┐                                                    │
│  │VesselTrackingPage│  🆕 React Page                                     │
│  │ + Leaflet Map    │  • Dropdown chọn tàu                               │
│  │                  │  • Marker vị trí hiện tại + popup                  │
│  │                  │  • Polyline hành trình                              │
│  │                  │  • Auto-refresh mỗi 30s                            │
│  │                  │  • Filter thời gian (24h/7d/30d)                   │
│  └──────────────────┘                                                    │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Giai Đoạn 1: GPS Collector Service (TCP)

### 4.1 Mô tả

Service nền chạy trên Edge server, kết nối TCP đến thiết bị GPS (gpsd hoặc NMEA streamer). Đọc stream NMEA, parse qua `NmeaParser`, lưu vào `position_data`.

### 4.2 File tạo mới

**`edge_product/edge-services/Services/Voyage/GpsCollectorService.cs`**

```csharp
// BackgroundService — TCP GPS Collector
// - Kết nối TCP đến GPS device
// - Đọc buffer, tách câu NMEA bằng \r\n
// - Parse qua NmeaParser (dùng chung singleton)
// - Lưu PositionData vào DB
// - Auto-reconnect với exponential backoff (1s→60s max)
// - Ghi log NmeaRawData để debug (optional)
```

**Cấu trúc class chính:**

| Method | Chức năng |
|---|---|
| `ExecuteAsync` | Vòng lặp chính: connect → read loop → reconnect |
| `ConnectWithRetryAsync` | Kết nối TCP với retry + backoff |
| `ReadNmeaStreamAsync` | Đọc TCP stream, tách buffer thành các câu NMEA |
| `ProcessSentenceAsync` | Parse NMEA → PositionData → Save DB |
| `CalculateBackoff` | Tính delay reconnect: 1s, 2s, 4s, 8s, 16s, 32s, max 60s |

**Cấu hình (`appsettings.json`):**

```json
"GpsCollector": {
    "Enabled": false,
    "Host": "192.168.1.200",
    "Port": 2947,
    "ReconnectDelayMs": 1000,
    "MaxReconnectDelayMs": 60000,
    "ReadBufferSize": 4096,
    "LogRawNmea": true
}
```

**Đăng ký trong `Program.cs`:**

```csharp
// Dòng 206-209 (trong region Background Services)
var gpsCollectorEnabled = builder.Configuration.GetValue("GpsCollector:Enabled", false);
if (gpsCollectorEnabled)
{
    builder.Services.AddHostedService<MaritimeEdge.Services.Voyage.GpsCollectorService>();
}
```

### 4.3 Xử lý buffer & phân mảnh

```
TCP Stream: "$GPGGA,...\r\n$GPRMC,...\r\n$GPGGA,..." (có thể bị cắt ngang)
                                                    ↓
Buffer đọc: byte[] → string (UTF8/ASCII)
                                                    ↓
StringBuilder _leftover: lưu phần dở dang từ lần đọc trước
                                                    ↓
Tách bằng "\r\n" → mảng các câu NMEA hoàn chỉnh
                                                    ↓
Câu cuối cùng (không có \r\n) → giữ lại trong _leftover
                                                    ↓
Mỗi câu hoàn chỉnh → NmeaParser.ParseSentence()
                                                    ↓
Nếu là PositionData → lưu DB
```

---

## 5. Giai Đoạn 2: Position Sync Enqueuer

### 5.1 Mô tả

BackgroundService quét `position_data` có `is_synced = false`, đóng gói thành `SyncQueue` entry. `SyncBackgroundWorker` hiện có sẽ tự động push.

### 5.2 File tạo mới

**`edge_product/edge-services/Services/Voyage/PositionSyncEnqueuerService.cs`**

```csharp
// BackgroundService — quét PositionData chưa sync
// - Chạy mỗi 30s (configurable)
// - Lấy tối đa 100 PositionData IsSynced=false
// - Serialize → SyncQueue (table=position_data, priority=Operational)
// - Đánh dấu IsSynced=true
// - Batch save
```

**Cấu hình (`appsettings.json`):**

```json
"PositionSyncEnqueuer": {
    "Enabled": true,
    "IntervalSeconds": 30,
    "BatchSize": 100
}
```

**Đăng ký trong `Program.cs`:**

```csharp
if (builder.Configuration.GetValue("PositionSyncEnqueuer:Enabled", true))
{
    builder.Services.AddHostedService<MaritimeEdge.Services.Voyage.PositionSyncEnqueuerService>();
}
```

### 5.3 Tại sao cần service riêng?

- `TelemetrySimulatorService` hiện tại chỉ set `IsSynced=false`, không enqueue vào SyncQueue
- `SyncService` chỉ push những gì có trong `SyncQueue`, không tự quét bảng
- Service này đóng vai trò "bridge" giữa Data Layer và Sync Layer
- Không can thiệp vào code hiện có, chỉ thêm service mới

---

## 6. Giai Đoạn 3: Hiển Thị Bản Đồ Realtime

### 6.1 Cài đặt thư viện

```bash
# Shore Frontend
cd shore_product/frontend
npm install leaflet react-leaflet @types/leaflet

# Edge Frontend
cd edge_product/frontend-edge
npm install leaflet react-leaflet @types/leaflet
```

### 6.2 Component dùng chung

**`shore_product/frontend/src/components/vessel/VesselMap.tsx`**

```tsx
// Leaflet Map component với:
// - OpenStreetMap tiles
// - Marker vị trí tàu (custom icon)
// - Popup: Tốc độ, Hướng, Thời gian, Số vệ tinh
// - Polyline nối các điểm hành trình
// - Props: positions[], latestPosition, autoCenter
```

**`edge_product/frontend-edge/src/components/ship-data/VesselMap.tsx`**

```tsx
// Tương tự như Shore version
// Dùng chung logic, khác API endpoint
```

### 6.3 Trang Shore: VesselTrackingPage

**File:** `shore_product/frontend/src/pages/VesselManagement/VesselTrackingPage.tsx`

```
┌─────────────────────────────────────────────────┐
│  🚢 Vessel Tracking                             │
│  ┌──────────────┐  ┌─────┐ ┌──────┐ ┌────────┐ │
│  │ Chọn tàu ▼   │  │ 24h │ │ 7d   │ │ 30d    │ │
│  └──────────────┘  └─────┘ └──────┘ └────────┘ │
│                                                  │
│  ┌────────────────────────────────────────────┐ │
│  │                                            │ │
│  │           🗺️ BẢN ĐỒ LEAFLET               │ │
│  │                                            │ │
│  │    🚢  ← Marker tàu                        │ │
│  │    ─── ← Polyline hành trình               │ │
│  │                                            │ │
│  └────────────────────────────────────────────┘ │
│                                                  │
│  📊 Thông tin hiện tại:                          │
│  Vĩ độ: 10.7769°N  |  Kinh độ: 106.7009°E       │
│  Tốc độ: 12.5 knots | Hướng: 95°                │
│  Cập nhật: 2026-05-04 14:30:00 UTC              │
└─────────────────────────────────────────────────┘
```

### 6.4 Trang Edge: NavigationPage (cập nhật)

**File:** `edge_product/frontend-edge/src/pages/Navigation/NavigationPage.tsx`

- Thay thế stub "Coming Soon" bằng bản đồ Leaflet
- Gọi `GET /api/telemetry/position/latest` + `GET /api/telemetry/position/history`
- Hiển thị marker + polyline
- Auto-refresh mỗi 30s

### 6.5 API Shore: Endpoint Realtime

**Cập nhật:** `shore_product/backend/Controllers/VesselTelemetryController.cs`

```csharp
// Thêm method mới:
[HttpGet("vessel/{vesselId}/realtime")]
public async Task<IActionResult> GetRealtimeData(string vesselId, [FromQuery] int hours = 24)
{
    // Trả về:
    // {
    //   latest: { lat, lon, sog, cog, timestamp, source },
    //   route: [{ lat, lon, sog, cog, timestamp }, ...],
    //   stats: { totalPoints, distanceNm, avgSpeed }
    // }
}
```

### 6.6 API Edge: Endpoint cho Map (đã có)

```
GET /api/telemetry/position/latest
→ { id, timestamp, latitude, longitude, speedOverGround, courseOverGround, ... }

GET /api/telemetry/position/history?hours=24&page=1&pageSize=500
→ { data: [...], pagination: {...}, timeRange: {...} }
```

---

## 7. Giai Đoạn 4: Tích Hợp & Kiểm Thử

### 7.1 Test GPS Collector

```bash
# 1. Dùng netcat giả lập TCP GPS server
# Terminal 1: Tạo fake GPS server
# Windows: dùng PowerShell script gửi NMEA sentences qua TCP

# 2. Chạy Edge với GpsCollector enabled
cd edge_product/edge-services
dotnet run --urls "http://localhost:5001"

# 3. Kiểm tra log
# [INFO] GpsCollectorService: Connected to GPS device at 192.168.1.200:2947
# [INFO] GpsCollectorService: Position saved: 10.7769, 106.7009

# 4. Kiểm tra DB
psql -h localhost -p 5433 -U edge_user -d maritime_edge
SELECT COUNT(*), MAX(timestamp) FROM position_data;
```

### 7.2 Test Sync Flow

```bash
# 1. Đảm bảo PositionSyncEnqueuer và SyncBackgroundWorker đang chạy

# 2. Kiểm tra SyncQueue
SELECT COUNT(*) FROM sync_queue WHERE table_name = 'position_data';

# 3. Kiểm tra Shore DB sau sync
SELECT COUNT(*), MAX(timestamp) FROM position_data;

# 4. Gọi API Shore
curl http://localhost:5000/api/VesselTelemetry/vessel/{id}/realtime
```

### 7.3 Test Map Frontend

```bash
# Shore
cd shore_product/frontend
npm run dev
# Mở http://localhost:3000 → Vessel Tracking

# Edge
cd edge_product/frontend-edge
npm run dev
# Mở http://localhost:3002 → Navigation
```

### 7.4 Test Auto-Reconnect

```bash
# 1. Dừng TCP GPS simulator
# → Log: [WARN] GpsCollectorService: Connection lost. Reconnecting in 1s...
# → Log: [WARN] GpsCollectorService: Reconnect attempt 2 failed. Retrying in 2s...

# 2. Khởi động lại TCP GPS simulator
# → Log: [INFO] GpsCollectorService: Reconnected successfully
```

---

## 8. Danh Sách File Cần Tạo/Sửa

### 8.1 File Tạo Mới 🆕

| # | File | Mô tả |
|---|---|---|
| 1 | `edge_product/edge-services/Services/Voyage/GpsCollectorService.cs` | TCP GPS Collector BackgroundService |
| 2 | `edge_product/edge-services/Services/Voyage/PositionSyncEnqueuerService.cs` | Auto-enqueue PositionData → SyncQueue |
| 3 | `shore_product/frontend/src/components/vessel/VesselMap.tsx` | Leaflet Map component (dùng chung) |
| 4 | `shore_product/frontend/src/pages/VesselManagement/VesselTrackingPage.tsx` | Trang tracking tàu trên bờ |
| 5 | `edge_product/frontend-edge/src/components/ship-data/VesselMap.tsx` | Leaflet Map component (Edge) |

### 8.2 File Cần Sửa 🔧

| # | File | Thay đổi |
|---|---|---|
| 6 | `edge_product/edge-services/Program.cs` | Đăng ký GpsCollectorService + PositionSyncEnqueuerService |
| 7 | `edge_product/edge-services/appsettings.json` | Thêm cấu hình GpsCollector + PositionSyncEnqueuer |
| 8 | `edge_product/frontend-edge/src/pages/Navigation/NavigationPage.tsx` | Thay stub bằng bản đồ Leaflet |
| 9 | `shore_product/backend/Controllers/VesselTelemetryController.cs` | Thêm endpoint GET /realtime |
| 10 | `shore_product/frontend/package.json` | Thêm leaflet, react-leaflet |
| 11 | `edge_product/frontend-edge/package.json` | Thêm leaflet, react-leaflet |

---

## ✅ Checklist Triển Khai

- [ ] **GĐ1:** GpsCollectorService + cấu hình + đăng ký
- [ ] **GĐ2:** PositionSyncEnqueuerService + cấu hình + đăng ký
- [ ] **GĐ3.1:** Cài Leaflet cho cả 2 frontend
- [ ] **GĐ3.2:** Tạo VesselMap component (Shore + Edge)
- [ ] **GĐ3.3:** Tạo VesselTrackingPage (Shore)
- [ ] **GĐ3.4:** Cập nhật NavigationPage (Edge)
- [ ] **GĐ3.5:** Thêm API realtime (Shore)
- [ ] **GĐ4:** Test end-to-end GPS → Sync → Map

---

> 📝 **Ghi chú:** File NMEA test mẫu có thể tìm thấy tại `c:\Users\tinhv\Downloads\output.nmea`
