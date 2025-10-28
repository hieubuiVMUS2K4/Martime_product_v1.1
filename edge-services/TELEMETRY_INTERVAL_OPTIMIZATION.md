# Tối ưu hóa Tần suất Ghi Dữ liệu Giả lập

## Vấn đề

Trước đây, `TelemetrySimulatorService` ghi dữ liệu giả lập **mỗi 5 giây**, gây ra:
- Database nhanh chóng đầy dữ liệu không cần thiết
- Tăng I/O và CPU usage
- Khó theo dõi và debug
- Không phản ánh thực tế (tàu thực không cần ghi dữ liệu nhanh như vậy)

## Giải pháp

### 1. Điều chỉnh Interval chính (appsettings.json)

```json
"TelemetrySimulator": {
  "Enabled": true,
  "IntervalSeconds": 60,  // Thay đổi từ 5s → 60s
  "DataRetentionHours": 24
}
```

### 2. Thêm Interval riêng cho từng loại dữ liệu (TelemetrySimulatorService.cs)

| Loại dữ liệu | Interval cũ | Interval mới | Lý do |
|--------------|-------------|--------------|-------|
| Position & Navigation | 5s | 60s (1 phút) | GPS/Gyro: đủ cho tracking tàu |
| Engine & Generator | 5s | 60s (1 phút) | Máy móc: thay đổi chậm, 1 phút là hợp lý |
| Environmental | 5s | 300s (5 phút) | Thời tiết: thay đổi rất chậm |

### 3. Cơ chế Tick-based

Service giờ sử dụng `_tickCounter` để kiểm soát:
- **Mỗi tick = 60 giây** (base interval)
- Position/Navigation: mỗi tick (60s)
- Engine/Generator: mỗi tick (60s)
- Environmental: mỗi 5 ticks (300s = 5 phút)

## Kết quả

### Giảm số lượng records ghi mỗi giờ:

| Loại dữ liệu | Trước (5s) | Sau (60s/300s) | Giảm |
|--------------|------------|----------------|------|
| Position | 720 records/h | 60 records/h | **-91.7%** |
| Navigation | 720 records/h | 60 records/h | **-91.7%** |
| Engine (2x) | 1440 records/h | 120 records/h | **-91.7%** |
| Generator (3x) | 2160 records/h | 180 records/h | **-91.7%** |
| Environmental | 720 records/h | 12 records/h | **-98.3%** |
| **Tổng** | **5760 records/h** | **432 records/h** | **-92.5%** |

### Giảm số lượng records ghi mỗi ngày:

- **Trước:** ~138,240 records/ngày
- **Sau:** ~10,368 records/ngày
- **Tiết kiệm:** 127,872 records/ngày (-92.5%)

## So sánh với Thực tế Hàng hải

Các tàu thực tế thường thu thập dữ liệu với tần suất:

| Hệ thống | Tần suất thực tế | Simulator (mới) | Hợp lý? |
|----------|------------------|-----------------|---------|
| GPS/AIS Position | 10-60s | 60s | ✅ Phù hợp |
| Navigation (Gyro/Log) | 1-10s | 60s | ✅ Đủ cho testing |
| Engine Telemetry | 15-60s | 60s | ✅ Phù hợp |
| Environmental | 5-10 phút | 5 phút | ✅ Phù hợp |

## Cấu hình Nâng cao

Nếu cần điều chỉnh thêm, chỉnh trong `TelemetrySimulatorService.cs`:

```csharp
private const int POSITION_NAV_INTERVAL = 1;   // Every 60s (tick × 60s)
private const int ENGINE_GEN_INTERVAL = 1;     // Every 60s
private const int ENVIRONMENTAL_INTERVAL = 5;   // Every 300s (5 ticks × 60s)
```

Hoặc thay đổi base interval trong `appsettings.json`:
```json
"IntervalSeconds": 60  // Base tick interval
```

## Testing

Để kiểm tra interval mới:

```bash
# Restart edge-services
cd edge-services
dotnet run

# Xem logs
# Sẽ thấy: "Telemetry data simulated (tick X): Pos/Nav=true, Engine/Gen=true, Env=true/false"
# Environmental chỉ true mỗi 5 ticks (5 phút)

# Kiểm tra database
psql -h localhost -p 5433 -U edge_user -d maritime_edge

# Đếm records gần đây
SELECT 
  'position_data' as table_name, COUNT(*) as count_last_hour
FROM position_data 
WHERE created_at > NOW() - INTERVAL '1 hour'
UNION ALL
SELECT 'environmental_data', COUNT(*)
FROM environmental_data 
WHERE created_at > NOW() - INTERVAL '1 hour';
```

## Lưu ý Production

Trong production (tàu thực):
1. Set `TelemetrySimulator.Enabled = false` trong appsettings.json
2. Dữ liệu sẽ đến từ sensors thực (GPS, Modbus, Serial ports)
3. Tần suất thu thập được điều khiển bởi `DataCollection` config:
   ```json
   "DataCollection": {
     "PositionInterval": 5,        // GPS thực: mỗi 5s
     "NavigationInterval": 10,     // Gyro: mỗi 10s
     "EngineInterval": 15,         // Modbus: mỗi 15s
     "EnvironmentalInterval": 300  // Weather: mỗi 5 phút
   }
   ```

## Files Thay đổi

1. `edge-services/appsettings.json` - Tăng IntervalSeconds từ 5→60
2. `edge-services/Services/TelemetrySimulatorService.cs` - Thêm tick-based intervals
3. `edge-services/TELEMETRY_INTERVAL_OPTIMIZATION.md` - Tài liệu này

---

**Ngày cập nhật:** 2025-10-24  
**Tác giả:** GitHub Copilot  
**Trạng thái:** ✅ Hoàn thành và kiểm tra
