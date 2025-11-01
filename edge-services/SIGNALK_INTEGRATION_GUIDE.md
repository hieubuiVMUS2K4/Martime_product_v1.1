# 🌊 SignalK Data Integration Guide

## 📖 Overview

Dự án của bạn đã được tích hợp **SignalK Data Collector** - một service thu thập dữ liệu maritime thực từ SignalK demo server hoặc SignalK server thực tế trên tàu.

**SignalK là gì?**
- Chuẩn dữ liệu mở cho ngành hàng hải (open-source marine data format)
- Được sử dụng rộng rãi trên các tàu hiện đại
- Hỗ trợ đầy đủ: GPS, AIS, engine, environmental, electrical data
- Website: https://signalk.org

---

## 🎯 Cấu trúc đã implement

### **1. SignalKDataCollectorService** 
📁 `edge-services/Services/SignalKDataCollectorService.cs`

**Background service** chạy liên tục để:
- ✅ Thu thập dữ liệu từ SignalK server mỗi 60 giây (configurable)
- ✅ Parse và convert đơn vị (m/s → knots, radians → degrees, Kelvin → Celsius, etc.)
- ✅ Lưu vào PostgreSQL database (tương tự TelemetrySimulator)
- ✅ Auto cleanup dữ liệu cũ

**Dữ liệu thu thập:**
- 🧭 **Navigation**: Position (GPS), speed, course, heading, attitude (pitch/roll)
- 🌤️ **Environment**: Temperature, pressure, humidity, wind, waves
- ⚙️ **Propulsion**: Engine RPM, load, temperatures, fuel rate
- 🔋 **Electrical**: Generators, batteries (có thể mở rộng)

---

### **2. SignalKTestController**
📁 `edge-services/Controllers/SignalKTestController.cs`

**Test API endpoints** để xem dữ liệu SignalK trực tiếp:

```
GET /api/signalk-test/health          - Kiểm tra kết nối
GET /api/signalk-test/vessel          - Toàn bộ dữ liệu vessel
GET /api/signalk-test/navigation      - Dữ liệu navigation
GET /api/signalk-test/environment     - Dữ liệu môi trường
GET /api/signalk-test/propulsion      - Dữ liệu động cơ
GET /api/signalk-test/electrical      - Dữ liệu điện
```

---

### **3. Configuration**
📁 `edge-services/appsettings.json`

```json
{
  "TelemetrySimulator": {
    "Enabled": false,    // TẮT simulator cũ
    "Comment": "DISABLED - Using SignalK data instead"
  },

  "SignalK": {
    "Enabled": true,     // BẬT SignalK collector
    "BaseUrl": "https://demo.signalk.org/signalk/v1/api",
    "IntervalSeconds": 60,
    "Comment": "Real maritime data from SignalK demo server"
  }
}
```

---

## 🚀 Quick Start

### **Bước 1: Chạy Edge Server**

```powershell
cd f:\NCKH\Product\Martime_product_v1.1\edge-services
dotnet run
```

Bạn sẽ thấy log:
```
[INFO] SignalK Data Collector started - Endpoint: https://demo.signalk.org/signalk/v1/api, Interval: 60s
[DEBUG] SignalK navigation data collected: Lat=48.123456, Lon=-122.654321, SOG=12.5kts
```

---

### **Bước 2: Test SignalK Connection**

Mở browser hoặc dùng Postman:

**Kiểm tra health:**
```
http://localhost:5000/api/signalk-test/health
```

**Xem dữ liệu navigation:**
```
http://localhost:5000/api/signalk-test/navigation
```

Response mẫu:
```json
{
  "endpoint": "navigation",
  "source": "https://demo.signalk.org/signalk/v1/api",
  "timestamp": "2025-10-30T10:30:45Z",
  "data": {
    "position": {
      "value": {
        "latitude": 48.123456,
        "longitude": -122.654321
      },
      "timestamp": "2025-10-30T10:30:44.123Z"
    },
    "speedOverGround": {
      "value": 6.43,  // m/s (will be converted to knots)
      "timestamp": "2025-10-30T10:30:44.123Z"
    },
    "courseOverGroundTrue": {
      "value": 1.658,  // radians (will be converted to degrees)
      "timestamp": "2025-10-30T10:30:44.123Z"
    }
  }
}
```

---

### **Bước 3: Xem dữ liệu trong Database**

Dữ liệu SignalK sẽ tự động lưu vào các bảng:
- `position_data` - GPS position
- `navigation_data` - Heading, pitch, roll, depth
- `engine_data` - Engine telemetry
- `environmental_data` - Weather, sea conditions

**Kiểm tra trong pgAdmin:**
```sql
-- Xem 10 vị trí mới nhất từ SignalK
SELECT timestamp, latitude, longitude, speed_over_ground, source
FROM position_data
WHERE source = 'SignalK'
ORDER BY timestamp DESC
LIMIT 10;

-- Xem dữ liệu navigation
SELECT timestamp, heading_true, speed_through_water, depth
FROM navigation_data
ORDER BY timestamp DESC
LIMIT 10;

-- Xem dữ liệu engine
SELECT timestamp, engine_id, rpm, load_percent, coolant_temp
FROM engine_data
ORDER BY timestamp DESC
LIMIT 10;
```

---

## 📊 So sánh: Simulator vs SignalK

| Feature | TelemetrySimulator | SignalK Collector |
|---------|-------------------|-------------------|
| **Dữ liệu** | Giả lập ngẫu nhiên | Thực tế từ demo server |
| **Định dạng** | Custom | Chuẩn SignalK (industry standard) |
| **Khi nào dùng** | Dev/test offline | Dev/test với real data |
| **Khi deploy** | Không dùng | Switch to vessel's SignalK server |

**Khuyến nghị:**
- ✅ Development: Dùng **SignalK** (đã enable)
- ✅ Testing offline: Dùng **Simulator** (set Enabled: true)
- ✅ Production: Dùng **SignalK** với server trên tàu

---

## 🔧 Cấu hình nâng cao

### **Switch giữa Demo và Production SignalK Server**

**Demo (hiện tại):**
```json
"SignalK": {
  "Enabled": true,
  "BaseUrl": "https://demo.signalk.org/signalk/v1/api",
  "IntervalSeconds": 60
}
```

**Production (trên tàu):**
```json
"SignalK": {
  "Enabled": true,
  "BaseUrl": "http://192.168.1.100:3000/signalk/v1/api",  // Local SignalK server
  "IntervalSeconds": 30  // Có thể frequent hơn vì LAN
}
```

---

### **Chạy cả Simulator và SignalK cùng lúc**

Nếu muốn so sánh:
```json
{
  "TelemetrySimulator": {
    "Enabled": true,
    "IntervalSeconds": 60
  },
  "SignalK": {
    "Enabled": true,
    "BaseUrl": "https://demo.signalk.org/signalk/v1/api",
    "IntervalSeconds": 60
  }
}
```

Khi đó:
- Simulator tạo data với `source = "GPS"`
- SignalK tạo data với `source = "SignalK"`
- Có thể filter trong query: `WHERE source = 'SignalK'`

---

## 🌐 SignalK Resources

**Demo Server:**
- Web UI: https://demo.signalk.org/admin
- Data Browser: https://demo.signalk.org/admin/#/databrowser
- API Docs: https://demo.signalk.org/documentation

**Available Endpoints:**
```
https://demo.signalk.org/signalk/v1/api/vessels/self
https://demo.signalk.org/signalk/v1/api/vessels/self/navigation
https://demo.signalk.org/signalk/v1/api/vessels/self/environment
https://demo.signalk.org/signalk/v1/api/vessels/self/propulsion
https://demo.signalk.org/signalk/v1/api/vessels/self/electrical
https://demo.signalk.org/signalk/v1/api/vessels/self/tanks
```

**Documentation:**
- SignalK Specification: http://signalk.org/specification/
- GitHub: https://github.com/SignalK/signalk-server

---

## 🐛 Troubleshooting

### **Lỗi: Cannot connect to SignalK server**

**Nguyên nhân:**
- Demo server down (hiếm)
- Firewall/network issue
- Wrong BaseUrl

**Giải pháp:**
```powershell
# Test connection manually
curl https://demo.signalk.org/signalk/v1/api/vessels/self

# Or use your test controller
curl http://localhost:5000/api/signalk-test/health
```

---

### **Không thấy dữ liệu trong database**

**Check logs:**
```powershell
# Xem log edge service
# Tìm dòng: "SignalK data collected successfully"
```

**Check configuration:**
```json
"SignalK": {
  "Enabled": true  // ← Phải là true
}
```

---

### **Muốn thu thập data frequent hơn**

Edit `appsettings.json`:
```json
"SignalK": {
  "IntervalSeconds": 30  // Thay vì 60
}
```

**Lưu ý:** Demo server có rate limiting, không nên < 10 seconds.

---

## 📈 Next Steps

### **Tích hợp thêm endpoints:**

SignalK còn nhiều data khác có thể thu thập:
- 🔧 **Tanks** (fuel, water levels)
- ⚓ **Steering** (rudder angle, autopilot)
- 📡 **Communication** (AIS, VHF)
- 🔔 **Notifications** (alarms, warnings)

### **WebSocket real-time streaming:**

Thay vì polling mỗi 60s, có thể dùng WebSocket:
```
wss://demo.signalk.org/signalk/v1/stream
```

### **Multi-vessel tracking:**

Thu thập dữ liệu nhiều tàu:
```
GET /api/vessels/{mmsi}/navigation
```

---

## ✅ Summary

| Component | Status | Location |
|-----------|--------|----------|
| SignalK Collector Service | ✅ Created | `Services/SignalKDataCollectorService.cs` |
| Test Controller | ✅ Created | `Controllers/SignalKTestController.cs` |
| Configuration | ✅ Updated | `appsettings.json` |
| Program.cs | ✅ Registered | `AddHostedService<SignalKDataCollectorService>()` |
| Database Models | ✅ Reused | `Models/EdgeModels.cs` (existing) |

**Kết quả:**
- 🎯 Thu thập dữ liệu maritime **THỰC TẾ** từ SignalK demo
- 🎯 Dữ liệu chuẩn quốc tế (NMEA, Signal K)
- 🎯 Sẵn sàng deploy lên tàu (chỉ cần đổi BaseUrl)
- 🎯 Test được ngay không cần hardware

**Run ngay:**
```powershell
cd edge-services
dotnet run
```

Sau đó truy cập: http://localhost:5000/swagger hoặc http://localhost:5000/api/signalk-test/health

---

🚢 **Maritime Edge Server + SignalK = Production Ready!** ⚓
