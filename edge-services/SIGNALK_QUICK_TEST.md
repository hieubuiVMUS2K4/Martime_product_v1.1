# 🚀 SignalK Quick Test Guide

## ✅ IMPLEMENTATION COMPLETE!

Dự án đã tích hợp thành công **SignalK Data Collector**!

---

## 📊 Test Results

### **Server đang chạy và thu thập dữ liệu:**

```
✅ SignalK Data Collector started
✅ Endpoint: https://demo.signalk.org/signalk/v1/api
✅ Interval: 60 seconds

📍 Navigation data collected:
   - Latitude: 59.708280° N (Estonia, Baltic Sea)
   - Longitude: 24.727651° E
   - Speed Over Ground: 6.7 knots
   
🌤️ Environmental data collected: ✅
⚙️ Propulsion endpoint: Not available on demo server (normal)
```

---

## 🧪 How to Test

### **Option 1: Swagger UI (Easiest)**

1. Chạy edge service:
   ```powershell
   cd f:\NCKH\Product\Martime_product_v1.1\edge-services
   dotnet run --project EdgeCollector.csproj
   ```

2. Mở browser:
   ```
   http://localhost:5000/swagger
   ```

3. Test các endpoint:
   - **GET /api/signalk-test/health** - Check connection
   - **GET /api/signalk-test/navigation** - See GPS data
   - **GET /api/signalk-test/environment** - See weather
   - **GET /api/signalk-test/vessel** - See all data

---

### **Option 2: Direct Browser**

Mở browser và paste các URL này:

```
http://localhost:5000/api/signalk-test/health
http://localhost:5000/api/signalk-test/navigation
http://localhost:5000/api/signalk-test/environment
http://localhost:5000/api/signalk-test/vessel
```

---

### **Option 3: PowerShell/curl**

```powershell
# Health check
Invoke-RestMethod http://localhost:5000/api/signalk-test/health | ConvertTo-Json

# Navigation data
Invoke-RestMethod http://localhost:5000/api/signalk-test/navigation | ConvertTo-Json

# All vessel data
Invoke-RestMethod http://localhost:5000/api/signalk-test/vessel | ConvertTo-Json -Depth 10
```

---

### **Option 4: Check Database**

Mở pgAdmin và query:

```sql
-- Xem dữ liệu SignalK mới nhất
SELECT 
    timestamp, 
    latitude, 
    longitude, 
    speed_over_ground, 
    course_over_ground,
    source
FROM position_data
WHERE source = 'SignalK'
ORDER BY timestamp DESC
LIMIT 10;

-- Xem navigation data
SELECT 
    timestamp,
    heading_true,
    speed_through_water,
    depth,
    pitch,
    roll
FROM navigation_data
ORDER BY timestamp DESC
LIMIT 10;

-- Xem environmental data
SELECT 
    timestamp,
    air_temperature,
    wind_speed,
    wind_direction,
    sea_temperature
FROM environmental_data
ORDER BY timestamp DESC
LIMIT 10;
```

---

## 📈 Expected Data Sample

### **Navigation Response:**
```json
{
  "endpoint": "navigation",
  "source": "https://demo.signalk.org/signalk/v1/api",
  "timestamp": "2025-10-30T13:14:35Z",
  "data": {
    "position": {
      "value": {
        "latitude": 59.70828,
        "longitude": 24.727651
      }
    },
    "speedOverGround": {
      "value": 3.446  // m/s → converted to 6.7 knots
    },
    "courseOverGroundTrue": {
      "value": 1.658  // radians → converted to ~95 degrees
    },
    "headingTrue": {
      "value": 1.658
    }
  }
}
```

---

## 🔄 Data Flow

```
SignalK Demo Server
        ↓ (every 60s)
SignalKDataCollectorService
        ↓
   Convert Units
   (m/s → knots, radians → degrees, Kelvin → Celsius)
        ↓
PostgreSQL Database
        ↓
Your Frontend/API
```

---

## 🎯 What's Working

| Component | Status | Notes |
|-----------|--------|-------|
| **SignalK Collector Service** | ✅ Running | Collecting every 60s |
| **Navigation Data** | ✅ Working | GPS, speed, course, heading |
| **Environmental Data** | ✅ Working | Temperature, wind, pressure |
| **Propulsion Data** | ⚠️ N/A | Demo server doesn't have engine data |
| **Database Storage** | ✅ Working | Auto-saving to PostgreSQL |
| **Test API** | ✅ Working | All endpoints responding |
| **Simulator** | ⏸️ Disabled | Using real data instead |

---

## 🌍 Real Vessel Location

Dữ liệu đang lấy từ một tàu thực ở:
- **Coordinates:** 59.708°N, 24.728°E
- **Location:** Near Tallinn, Estonia (Baltic Sea)
- **Speed:** ~6.7 knots
- **Heading:** ~95° (East)

Mở Google Maps để xem: https://www.google.com/maps/@59.708280,24.727651,15z

---

## 🔧 Configuration

### **Current Settings (appsettings.json):**

```json
{
  "TelemetrySimulator": {
    "Enabled": false  // ← DISABLED (using SignalK instead)
  },
  
  "SignalK": {
    "Enabled": true,  // ← ENABLED
    "BaseUrl": "https://demo.signalk.org/signalk/v1/api",
    "IntervalSeconds": 60
  }
}
```

### **To switch back to Simulator:**
```json
{
  "TelemetrySimulator": {
    "Enabled": true
  },
  "SignalK": {
    "Enabled": false
  }
}
```

### **To use both:**
```json
{
  "TelemetrySimulator": {
    "Enabled": true  // Random data with source="GPS"
  },
  "SignalK": {
    "Enabled": true  // Real data with source="SignalK"
  }
}
```

---

## 📦 Files Created

| File | Purpose |
|------|---------|
| `Services/SignalKDataCollectorService.cs` | Background service thu thập data |
| `Controllers/SignalKTestController.cs` | Test API endpoints |
| `SIGNALK_INTEGRATION_GUIDE.md` | Hướng dẫn chi tiết |
| `SIGNALK_QUICK_TEST.md` | This file (quick reference) |

---

## 🐛 Troubleshooting

### **Service không chạy?**
Check logs for: `SignalK Data Collector started`

### **Không có dữ liệu?**
- Check `"SignalK:Enabled": true` in appsettings.json
- Check logs for: `SignalK data collected successfully`
- Query database: `SELECT * FROM position_data WHERE source='SignalK'`

### **Demo server down?**
Test directly: https://demo.signalk.org/signalk/v1/api/vessels/self

---

## 🎉 Next Steps

✅ **Hoàn thành:** Thu thập dữ liệu thực từ SignalK
🔜 **Tiếp theo:** 
   - Xem dữ liệu trên frontend
   - Tích hợp thêm endpoints (tanks, electrical)
   - Setup WebSocket cho real-time streaming
   - Deploy lên tàu thực (đổi BaseUrl)

---

## 📞 Quick Commands

```powershell
# Run server
cd f:\NCKH\Product\Martime_product_v1.1\edge-services
dotnet run --project EdgeCollector.csproj

# Test in another terminal
Invoke-RestMethod http://localhost:5000/api/signalk-test/health

# View logs
# Check console output for "SignalK data collected"

# Check database
psql -h localhost -p 5433 -U edge_user -d maritime_edge
SELECT COUNT(*) FROM position_data WHERE source='SignalK';
```

---

🚢 **SignalK Integration: COMPLETE!** ⚓

**You're now collecting REAL maritime data from a vessel in the Baltic Sea!** 🌊
