# Shore Database Optimization Plan
*Nguyên tắc: "Shore chỉ lưu thông tin CẦN THIẾT cho quản lý & báo cáo, KHÔNG LƯU dữ liệu raw/debug"*

## 🎯 Mục tiêu

1. **Giảm storage**: Từ 100% → 30% so với Edge
2. **Giảm băng thông đồng bộ**: Chỉ gửi data có giá trị kinh doanh
3. **Tăng tốc query**: Ít cột hơn = query nhanh hơn
4. **Tuân thủ GDPR/Privacy**: Không lưu dữ liệu cá nhân không cần thiết

---

## 📊 BẢNG SO SÁNH CHI TIẾT

### 1. PositionData (GPS Tracking)

**Mục đích Shore:** Hiển thị vị trí tàu trên bản đồ, tính toán voyage distance

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| **Id** | Guid | Guid | Guid | ✅ Primary Key |
| **Timestamp** | DateTime | DateTime | DateTime | ✅ Bắt buộc cho time-series |
| **Latitude** | double | double | double | ✅ **CORE** - Vị trí |
| **Longitude** | double | double | double | ✅ **CORE** - Vị trí |
| **SpeedOverGround** | double? | double? | double? | ✅ Để tính ETA |
| **CourseOverGround** | double? | double? | double? | ✅ Hiển thị hướng tàu |
| **Altitude** | double? | ❌ | ❌ | ❌ Không cần (tàu không bay) |
| **MagneticVariation** | double? | ❌ | ❌ | ❌ Technical detail |
| **FixQuality** | int | ❌ | ❌ | ❌ Debug info |
| **SatellitesUsed** | int | ❌ | ❌ | ❌ Debug info |
| **Hdop** | double? | ❌ | ❌ | ❌ Debug info |
| **Source** | string | string | string | ✅ Để biết GPS/DGPS |
| **IsSynced** | bool | bool | ❌ | ❌ Chỉ dùng tại Edge |
| **OriginNode** | string | string | string | ✅ **CORE** - Biết tàu nào |

**Kết quả:** 14 fields → 8 fields (**43% reduction**)

---

### 2. NmeaRawData (NMEA Sentences)

**Mục đích Shore:** KHÔNG CẦN (chỉ dùng cho debug tại tàu)

| Field | Edge | Shore (Current) | Shore (Optimized) | Quyết định |
|-------|------|-----------------|-------------------|-----------|
| ALL | ✅ | ✅ | ❌ | **XÓA TOÀN BỘ TABLE** |

**Lý do xóa:**
- Raw NMEA sentences chỉ dùng để debug sensor tại tàu
- Shore chỉ cần data đã parsed (PositionData, NavigationData...)
- Tiết kiệm: ~5MB/day/ship

---

### 3. AisData (Tàu xung quanh)

**Mục đích Shore:** Tracking traffic, collision prevention history

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| **Id** | Guid | Guid | Guid | ✅ PK |
| **Timestamp** | DateTime | DateTime | DateTime | ✅ |
| **Mmsi** | string | string | string | ✅ **CORE** |
| **MessageType** | int | ❌ | ❌ | ❌ Technical |
| **NavigationStatus** | int? | ❌ | ✅ | ✅ Để biết tàu đang anchor/underway |
| **RateOfTurn** | double? | ❌ | ❌ | ❌ Too detailed |
| **SpeedOverGround** | double? | double? | double? | ✅ |
| **PositionAccuracy** | bool? | ❌ | ❌ | ❌ Technical |
| **Latitude** | double? | double? | double? | ✅ |
| **Longitude** | double? | double? | double? | ✅ |
| **CourseOverGround** | double? | double? | double? | ✅ |
| **TrueHeading** | int? | ❌ | ❌ | ❌ Not critical |
| **ImoNumber** | string? | ❌ | ✅ | ✅ **QUAN TRỌNG** - Để identify tàu |
| **CallSign** | string? | ❌ | ✅ | ✅ |
| **ShipName** | string | string | string | ✅ |
| **ShipType** | int? | ❌ | ✅ | ✅ Để filter (cargo/tanker/...) |
| **Dimension*** | int? (×4) | ❌ | ❌ | ❌ Static info, lưu riêng |
| **Eta*** | int? (×4) | int? (×4) | ❌ | ❌ Parse thành DateTime |
| **Draught** | double? | ❌ | ❌ | ❌ Not critical for shore |
| **Destination** | string | string | string | ✅ |
| **OriginNode** | string | string | string | ✅ |

**Kết quả:** 29 fields → 14 fields (**52% reduction**)

**Cải tiến:** Thay vì lưu ETA thành 4 số (Month/Day/Hour/Minute), parse thành `DateTime? EstimatedArrival`

---

### 4. NavigationData (Gyro, Log, Depth)

**Mục đích Shore:** KHÔNG CẦN real-time navigation (chỉ cần summary trong Report)

| Field | Edge | Shore (Current) | Shore (Optimized) | Quyết định |
|-------|------|-----------------|-------------------|-----------|
| ALL (13 fields) | ✅ | ✅ | ❌ | **XÓA TABLE** hoặc chỉ lưu trong NoonReport |

**Lý do:**
- HeadingTrue, Pitch, Roll, Wind... chỉ quan trọng REALTIME tại tàu
- Shore chỉ cần 1 snapshot/ngày trong Noon Report
- Tiết kiệm: ~10MB/day/ship

**Giải pháp:** Lưu navigation summary trong `NoonReport.NavigationSummary` (JSON)

---

### 5. EngineData (Động cơ)

**Mục đích Shore:** Performance tracking, không cần real-time monitoring

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| **Id** | Guid | Guid | Guid | ✅ |
| **Timestamp** | DateTime | DateTime | DateTime | ✅ |
| **EngineId** | string | string | string | ✅ |
| **Rpm** | double? | double? | ❌ | ❌ Chỉ cần Average RPM/day |
| **LoadPercent** | double? | double? | ❌ | ❌ Chỉ cần Average |
| **CoolantTemp** | double? | ❌ | ❌ | ❌ Operational detail |
| **ExhaustTemp** | double? | ❌ | ❌ | ❌ |
| **LubeOilPressure** | double? | ❌ | ❌ | ❌ |
| **LubeOilTemp** | double? | ❌ | ❌ | ❌ |
| **FuelPressure** | double? | ❌ | ❌ | ❌ |
| **FuelRate** | double? | double? | ✅ | ✅ **QUAN TRỌNG** - Fuel consumption |
| **RunningHours** | double? | double? | ✅ | ✅ Để tính maintenance |
| **AlarmStatus** | int? | int? | ✅ | ✅ Lịch sử alarm |
| **OriginNode** | string | string | string | ✅ |

**Kết quả:** 13 fields → 7 fields (**46% reduction**)

**Cải tiến:** Thêm `AverageRpm`, `AverageLoad` (tính từ 1 giờ data), thay vì lưu mỗi giây

---

### 6. FuelConsumptionData (Tiêu thụ nhiên liệu)

**Mục đích Shore:** IMO DCS compliance, cost tracking

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL | ✅ | ✅ | ✅ | **GIỮ NGUYÊN** - Quan trọng cho compliance |

**Lý do:** 
- IMO DCS yêu cầu lưu đầy đủ fuel data
- Dùng cho carbon emission reporting (EU MRV, CII)
- Tính cost-per-voyage

---

### 7. TankLevel (Mức bồn chứa)

**Mục đích Shore:** Inventory tracking, không cần real-time

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL (9 fields) | ✅ | ✅ | ❌ | **CHỈ LƯU SNAPSHOT/NGÀY** |

**Đề xuất:** 
- Thay vì lưu mỗi 5 phút, chỉ sync 1 lần/ngày lúc 12:00 UTC (Noon Report)
- Hoặc chỉ sync khi có thay đổi > 5%

---

### 8. GeneratorData (Máy phát điện)

**Mục đích Shore:** Performance tracking

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| **Id** | Guid | Guid | Guid | ✅ |
| **Timestamp** | DateTime | DateTime | DateTime | ✅ |
| **GeneratorId** | string | string | string | ✅ |
| **IsRunning** | bool | bool | ✅ | ✅ Running time tracking |
| **Voltage** | double? | double? | ❌ | ❌ Operational detail |
| **Frequency** | double? | double? | ❌ | ❌ |
| **Current** | double? | double? | ❌ | ❌ |
| **ActivePower** | double? | double? | ❌ | ❌ |
| **PowerFactor** | double? | ❌ | ❌ | ❌ |
| **RunningHours** | double? | double? | ✅ | ✅ Maintenance |
| **LoadPercent** | double? | double? | ✅ | ✅ Performance |
| **OriginNode** | string | string | string | ✅ |

**Kết quả:** 12 fields → 7 fields (**42% reduction**)

---

### 9. EnvironmentalData (Môi trường)

**Mục đích Shore:** Weather reporting, không cần real-time

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL (11 fields) | ✅ | ✅ | ❌ | **CHỈ LƯU TRONG NOON REPORT** |

**Lý do:** Environmental data chỉ cần snapshot/ngày, không cần time-series chi tiết

---

### 10. SafetyAlarm (Cảnh báo an toàn)

**Mục đích Shore:** Compliance, incident tracking

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL | ✅ | ✅ | ✅ | **GIỮ NGUYÊN** - SOLAS requirement |

---

### 11. VoyageRecord (Hành trình)

**Mục đích Shore:** Fleet management, performance analysis

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL | ✅ | ✅ | ✅ | **GIỮ NGUYÊN** - Core business data |

---

### 12. MaritimeReport & NoonReport

**Mục đích Shore:** Compliance, reporting

| Field | Edge | Shore (Current) | Shore (Optimized) | Lý do |
|-------|------|-----------------|-------------------|-------|
| ALL | ✅ | ✅ | ✅ | **GIỮ NGUYÊN** - IMO/SOLAS/MARPOL |

---

## 📝 TÓM TẮT QUYẾT ĐỊNH

| Bảng | Edge | Shore (Current) | Shore (Optimized) | Action | Tiết kiệm Storage |
|------|------|-----------------|-------------------|--------|-------------------|
| **NmeaRawData** | ✅ | ✅ | ❌ | **DELETE** | 100% |
| **PositionData** | 14 fields | 11 fields | 8 fields | **OPTIMIZE** | 43% |
| **AisData** | 29 fields | 13 fields | 14 fields | **OPTIMIZE** | 52% |
| **NavigationData** | 13 fields | 13 fields | ❌ | **DELETE** (move to Report) | 100% |
| **EngineData** | 13 fields | 6 fields | 7 fields | **OPTIMIZE** | 46% |
| **FuelConsumptionData** | 11 fields | 11 fields | 11 fields | **KEEP** | 0% |
| **TankLevel** | 9 fields | 9 fields | 9 fields | **CHANGE SYNC** (1/day) | 95% |
| **GeneratorData** | 12 fields | 9 fields | 7 fields | **OPTIMIZE** | 42% |
| **EnvironmentalData** | 11 fields | 9 fields | ❌ | **DELETE** (move to Report) | 100% |
| **SafetyAlarm** | 11 fields | 11 fields | 11 fields | **KEEP** | 0% |
| **VoyageRecord** | 11 fields | 11 fields | 11 fields | **KEEP** | 0% |
| **MaritimeReport** | 18 fields | 18 fields | 18 fields | **KEEP** | 0% |
| **NoonReport** | 34 fields | 34 fields | 34 fields | **KEEP** | 0% |

**Tổng tiết kiệm:** ~**60-70% storage** và **băng thông đồng bộ**

---

## 🛠️ IMPLEMENTATION PLAN

### Phase 1: Remove Unnecessary Tables (URGENT)

1. **Delete NmeaRawData**
   - Shore không cần raw NMEA
   - Tiết kiệm: 5MB/day/ship
   
2. **Delete NavigationData** 
   - Move navigation summary to NoonReport
   - Tiết kiệm: 10MB/day/ship

3. **Delete EnvironmentalData**
   - Move weather snapshot to NoonReport
   - Tiết kiệm: 8MB/day/ship

### Phase 2: Optimize Existing Tables

4. **PositionData**: Remove technical fields (Altitude, Hdop, FixQuality...)
5. **AisData**: Remove dimensions, parse ETA to DateTime
6. **EngineData**: Add aggregated fields (AvgRpm, AvgLoad per hour)
7. **GeneratorData**: Remove voltage/frequency details

### Phase 3: Change Sync Strategy

8. **TankLevel**: Only sync significant changes (>5%) or 1/day
9. **EngineData**: Sync hourly averages, not per-second readings

### Phase 4: Add Aggregation Tables (NEW)

10. **DailyPerformanceSummary** (NEW)
    - VesselId, Date, TotalFuelConsumed, DistanceTraveled, AvgSpeed
    - Computed from raw data, replaces need for detailed time-series

11. **VesselFleetSnapshot** (NEW)
    - Latest position + status for each vessel (for dashboard)
    - Updated every 15 minutes

---

## 📊 BEFORE vs AFTER

### Storage Comparison (per ship per day)

| Category | Edge (Ship) | Shore (Before) | Shore (After) | Savings |
|----------|-------------|----------------|---------------|---------|
| Position (1/min) | 25 MB | 15 MB | 12 MB | 20% |
| AIS (1/5min) | 8 MB | 5 MB | 3 MB | 40% |
| NMEA Raw | 5 MB | 5 MB | **0 MB** | 100% |
| Navigation | 10 MB | 10 MB | **0 MB** (in Report) | 100% |
| Engine (1/sec) | 35 MB | 8 MB | 6 MB | 25% |
| Tank (1/5min) | 3 MB | 3 MB | **0.15 MB** (1/day) | 95% |
| Generator | 12 MB | 5 MB | 3 MB | 40% |
| Environmental | 8 MB | 8 MB | **0 MB** (in Report) | 100% |
| Reports/Alarms | 2 MB | 2 MB | 2 MB | 0% |
| **TOTAL/DAY** | **108 MB** | **61 MB** | **26.15 MB** | **57% ↓** |
| **TOTAL/MONTH** | **3.2 GB** | **1.8 GB** | **785 MB** | **56% ↓** |
| **Fleet × 10 ships** | **32 GB** | **18 GB** | **7.85 GB** | **56% ↓** |

### Cost Savings (VSAT $10/MB)

| Scenario | Before | After | Monthly Savings |
|----------|--------|-------|-----------------|
| 1 ship | $610/month | $262/month | **$348** |
| 10 ships | $6,100/month | $2,620/month | **$3,480** |
| 100 ships | $61,000/month | $26,200/month | **$34,800** |

---

## ✅ CHECKLIST

- [ ] **Step 1:** Backup current Shore DB
- [ ] **Step 2:** Create new optimized models in `backend/Models/SyncModelsOptimized.cs`
- [ ] **Step 3:** Update `SyncController` to handle new schema
- [ ] **Step 4:** Create migration to drop unnecessary tables
- [ ] **Step 5:** Update Edge `ProcessSyncQueue()` to skip deleted table types
- [ ] **Step 6:** Test sync with 1 ship for 24h
- [ ] **Step 7:** Roll out to production fleet

---

## 🚨 RISKS & MITIGATION

### Risk 1: Data Loss
**Mitigation:** Keep Edge data for 30 days, can re-sync if needed

### Risk 2: Query Performance
**Mitigation:** Add aggregation tables for dashboards

### Risk 3: Compliance Issues
**Mitigation:** Keep all compliance-related tables (Reports, Alarms, Fuel)

---

**Document Version:** 1.0  
**Created:** November 26, 2025  
**Status:** 🔴 **REQUIRES IMMEDIATE ACTION**
