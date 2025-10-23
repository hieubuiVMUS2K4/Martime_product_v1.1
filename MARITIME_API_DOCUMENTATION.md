# 📊 **MARITIME MANAGEMENT SYSTEM - API DOCUMENTATION**

## **🚢 Tổng Quan Hệ Thống Maritime Professional**

Maritime Management System là một hệ thống quản lý hạm đội toàn diện được thiết kế theo tiêu chuẩn quốc tế IMO (International Maritime Organization) với khả năng:

- **Real-time Vessel Tracking**: Theo dõi vị trí tàu theo thời gian thực
- **Fuel Management**: Quản lý nhiên liệu và hiệu suất vận hành
- **Alert System**: Hệ thống cảnh báo tự động và thông minh
- **Certificate Management**: Quản lý chứng chỉ và tài liệu pháp lý
- **Port Call Management**: Quản lý thông tin cảng và logistics
- **Edge Computing**: Thu thập dữ liệu từ cảm biến tàu (NMEA/Modbus)

---

## **🏗️ Kiến Trúc Hệ Thống**

### **Backend Architecture (ASP.NET Core 8)**
```
backend/
├── Controllers/           # API Controllers
│   ├── VesselsController.cs     # Vessel Management API
│   ├── VesselTelemetryController.cs  # Edge Data Collection
│   └── ShipsController.cs       # Legacy Ship Management
├── Services/             # Business Logic Layer
│   ├── VesselService.cs         # Vessel Operations
│   ├── AlertService.cs          # Alert Management
│   └── AlertBackgroundService.cs # Automated Alert Processing
├── Models/              # Data Models
│   ├── MaritimeModels.cs        # Core Maritime Entities
│   └── MaritimeDto.cs           # Data Transfer Objects
├── Data/               # Database Context
│   └── AppDbContext.cs          # EF Core Configuration
└── edge-services/      # Maritime Edge Computing
    ├── NmeaCollectorService.cs  # NMEA Data Collection
    └── Program.cs               # Edge Service Entry Point
```

### **Database Schema (PostgreSQL)**
- **Vessels**: Thông tin cơ bản tàu (IMO, Name, CallSign, Tonnage)
- **VesselPositions**: Dữ liệu định vị GPS/AIS theo thời gian
- **FuelConsumptions**: Báo cáo tiêu thụ nhiên liệu và hiệu suất
- **PortCalls**: Lịch sử cập cảng và hoạt động logistics
- **VesselAlerts**: Hệ thống cảnh báo đa cấp độ
- **Certificates**: Quản lý chứng chỉ và tài liệu pháp lý

---

## **🔌 API Endpoints Chính**

### **1. Vessel Management APIs**

#### **GET /api/vessels**
Lấy danh sách tất cả tàu với thông tin cơ bản và vị trí gần nhất
```json
{
  "vessels": [
    {
      "id": "uuid",
      "imo": "9123456",
      "name": "MSC OSCAR",
      "callSign": "ABCD",
      "vesselType": "Container Ship",
      "grossTonnage": 195636.0,
      "deadWeight": 199500.0,
      "buildDate": "2015-01-15T00:00:00Z",
      "flag": "Panama",
      "isActive": true,
      "lastPosition": {
        "latitude": 1.2345,
        "longitude": 103.6789,
        "speed": 14.5,
        "course": 045.0,
        "timestamp": "2024-01-15T10:30:00Z",
        "source": "AIS"
      },
      "unacknowledgedAlerts": 2
    }
  ]
}
```

#### **GET /api/vessels/{id}**
Lấy thông tin chi tiết một tàu bao gồm lịch sử vị trí, fuel records, port calls
```json
{
  "id": "uuid",
  "imo": "9123456",
  "name": "MSC OSCAR",
  "positions": [...],
  "fuelRecords": [...],
  "portCalls": [...],
  "alerts": [...]
}
```

#### **POST /api/vessels**
Tạo tàu mới với thông tin cơ bản
```json
{
  "imo": "9123456",
  "name": "MSC OSCAR",
  "callSign": "ABCD",
  "vesselType": "Container Ship",
  "grossTonnage": 195636.0,
  "deadWeight": 199500.0,
  "buildDate": "2015-01-15",
  "flag": "Panama"
}
```

### **2. Position Tracking APIs**

#### **POST /api/vessels/{id}/positions**
Cập nhật vị trí tàu (từ GPS/AIS/Manual)
```json
{
  "latitude": 1.2345,
  "longitude": 103.6789,
  "speed": 14.5,
  "course": 045.0,
  "timestamp": "2024-01-15T10:30:00Z",
  "source": "AIS"
}
```

#### **POST /api/vessels/{id}/positions/bulk**
Upload hàng loạt dữ liệu vị trí (cho AIS data dump)
```json
[
  {
    "latitude": 1.2345,
    "longitude": 103.6789,
    "speed": 14.5,
    "course": 045.0,
    "timestamp": "2024-01-15T10:30:00Z",
    "source": "AIS"
  },
  // ... more positions
]
```

### **3. Fuel Management APIs**

#### **POST /api/vessels/{id}/fuel**
Báo cáo tiêu thụ nhiên liệu
```json
{
  "reportDate": "2024-01-15",
  "fuelConsumed": 45.5,
  "fuelType": "MGO",
  "distanceTraveled": 320.5,
  "averageSpeed": 13.4
}
```

Response tự động tính toán fuel efficiency và tạo alert nếu cần:
```json
{
  "id": "uuid",
  "vesselId": "uuid", 
  "reportDate": "2024-01-15T00:00:00Z",
  "fuelConsumed": 45.5,
  "fuelType": "MGO",
  "distanceTraveled": 320.5,
  "averageSpeed": 13.4,
  "fuelEfficiency": 0.142  // MT per nautical mile
}
```

### **4. Alert Management APIs**

#### **GET /api/vessels/{id}/alerts**
Lấy danh sách cảnh báo của tàu
```json
[
  {
    "id": "uuid",
    "vesselId": "uuid",
    "alertType": "FUEL_EFFICIENCY",
    "message": "High fuel consumption detected: 0.185 MT/NM",
    "severity": "WARNING",
    "timestamp": "2024-01-15T10:30:00Z",
    "isAcknowledged": false,
    "data": "{\"currentEfficiency\": 0.185, \"threshold\": 0.150}"
  }
]
```

#### **POST /api/vessels/{id}/alerts**
Tạo cảnh báo thủ công
```json
{
  "alertType": "MAINTENANCE",
  "message": "Engine maintenance required",
  "severity": "WARNING",
  "data": "{\"equipment\": \"Main Engine\", \"dueDate\": \"2024-02-01\"}"
}
```

### **5. Performance Metrics APIs**

#### **GET /api/vessels/{id}/metrics**
Báo cáo hiệu suất tổng hợp của tàu
```json
{
  "vesselId": "uuid",
  "vesselName": "MSC OSCAR",
  "vesselIMO": "9123456",
  "period": {
    "from": "2024-01-01T00:00:00Z",
    "to": "2024-01-15T00:00:00Z"
  },
  "navigation": {
    "totalPositions": 1440,
    "averageSpeed": 13.8,
    "maxSpeed": 18.2,
    "lastPosition": {...}
  },
  "fuel": {
    "totalRecords": 15,
    "totalFuelConsumed": 672.5,
    "totalDistanceTraveled": 4580.3,
    "averageFuelEfficiency": 0.147,
    "bestEfficiency": 0.132,
    "worstEfficiency": 0.168
  },
  "alerts": {
    "total": 8,
    "unacknowledged": 3,
    "critical": 0,
    "warnings": 3,
    "byType": [
      {"type": "FUEL_EFFICIENCY", "count": 2},
      {"type": "POSITION_TIMEOUT", "count": 1}
    ]
  }
}
```

---

## **⚡ Edge Computing Integration**

### **NMEA Data Collection Service**
Dịch vụ thu thập dữ liệu từ các thiết bị hàng hải:

```csharp
// Tự động thu thập từ GPS/AIS qua Serial Port
NmeaCollectorService:
- COM Port Configuration
- Real-time NMEA sentence parsing  
- Local SQLite caching for offline operation
- MQTT publishing to shore systems
- Store-and-forward for satellite connections
```

### **Vessel Telemetry Controller**
API endpoint cho edge devices gửi dữ liệu:

#### **POST /api/telemetry/bulk**
```json
{
  "deviceId": "VESSEL_001_GPS",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": [
    {
      "type": "position",
      "latitude": 1.2345,
      "longitude": 103.6789,
      "speed": 14.5,
      "course": 45.0
    },
    {
      "type": "engine",
      "rpm": 1850,
      "temperature": 78.5,
      "pressure": 2.4
    }
  ]
}
```

---

## **🚨 Automated Alert System**

### **Background Service Processing**
Hệ thống tự động chạy mỗi 15 phút để:

1. **Position Timeout Alerts**: Cảnh báo khi tàu không gửi vị trí >2 giờ
2. **Certificate Expiry Alerts**: Thông báo chứng chỉ sắp hết hạn
3. **Fuel Efficiency Alerts**: Phát hiện bất thường tiêu thụ nhiên liệu
4. **Geofence Alerts**: Cảnh báo khi tàu vào/ra khỏi khu vực

### **Alert Severity Levels**
- **CRITICAL**: Cần xử lý ngay lập tức (certificate expired, emergency)
- **WARNING**: Cần chú ý trong 24h (fuel efficiency, maintenance due)
- **INFO**: Thông tin tham khảo (port arrival, route deviation)
- **LOW**: Ghi nhận (scheduled maintenance, routine reports)

---

## **🏗️ Deployment Architecture**

### **Docker Compose Services**
```yaml
services:
  backend:         # ASP.NET Core API (Port 5000)
  postgres:        # Database (Port 5432)
  redis:          # Cache & Sessions (Port 6379)
  frontend:       # React Web App (Port 3000)
  edge-collector: # NMEA/Modbus Service
  mqtt-broker:    # Ship Communication (Port 1883)
```

### **Environment Variables**
```bash
# Database
POSTGRES_DB=maritime_db
POSTGRES_USER=maritime
POSTGRES_PASSWORD=SecurePassword123!

# Redis
REDIS_PASSWORD=RedisPassword123!

# JWT Authentication
JWT__KEY=VeryLongSecretKeyForJWTTokens123456789

# MQTT Broker
MQTT_USERNAME=maritime
MQTT_PASSWORD=MqttPassword123!
```

---

## **🔐 Security Features**

### **Authentication & Authorization**
- **JWT Bearer Tokens**: Stateless API authentication
- **Role-based Access**: Captain, Officer, Shore Manager permissions
- **API Rate Limiting**: Prevent DoS attacks
- **CORS Configuration**: Secure cross-origin requests

### **Data Protection**
- **SQL Injection Protection**: EF Core parameterized queries
- **Input Validation**: Data annotations and model validation
- **Audit Logging**: Track all data modifications
- **Encryption**: Sensitive data encrypted at rest

---

## **📊 Performance Optimization**

### **Database Indexing**
```sql
-- Optimized indexes cho maritime queries
CREATE INDEX idx_vessel_positions_vessel_timestamp ON VesselPositions(VesselId, Timestamp DESC);
CREATE INDEX idx_vessel_alerts_vessel_acknowledged ON VesselAlerts(VesselId, IsAcknowledged);
CREATE INDEX idx_fuel_consumption_vessel_date ON FuelConsumptions(VesselId, ReportDate DESC);
```

### **Redis Caching**
- **Vessel Summary Data**: Cache 5 phút
- **Position Data**: Cache 1 phút cho real-time tracking
- **Alert Counts**: Cache 2 phút
- **Performance Metrics**: Cache 15 phút

### **Background Processing**
- **Alert Processing**: Async background service
- **Bulk Data Import**: Queue-based processing
- **Report Generation**: Scheduled batch jobs

---

## **🌐 Integration Capabilities**

### **Third-party Maritime Systems**
- **AIS Integration**: Automatic Identification System data
- **Weather APIs**: Route optimization và safety alerts
- **Port Authorities**: EDI integration cho port clearance
- **Fuel Suppliers**: Automated bunker reports
- **Insurance Companies**: Real-time risk assessment data

### **Mobile App Integration**
- **Flutter Mobile App**: Cross-platform vessel monitoring
- **Offline Capabilities**: Local SQLite caching
- **Push Notifications**: Critical alerts to mobile devices
- **Photo Upload**: Maintenance và inspection reports

---

## **📈 Roadmap & Future Enhancements**

### **Phase 2 - Advanced Analytics**
- Machine Learning fuel optimization
- Predictive maintenance algorithms
- Route optimization based on weather/traffic
- Carbon footprint tracking và reporting

### **Phase 3 - IoT Integration**
- Sensor data from engine room
- Automated cargo monitoring
- Environmental compliance monitoring
- Crew welfare và safety systems

### **Phase 4 - Blockchain Integration**
- Immutable voyage records
- Smart contracts cho cargo delivery
- Decentralized port documentation
- Supply chain transparency

---

*Hệ thống Maritime Management System được thiết kế để đáp ứng đầy đủ các yêu cầu của ngành hàng hải hiện đại với khả năng mở rộng và tích hợp cao.*