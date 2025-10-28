# 📊 FUEL ANALYTICS MODULE - TÀI LIỆU KỸ THUẬT

## 🎯 Tổng Quan

**Fuel Analytics Module** là hệ thống phân tích hiệu suất nhiên liệu và tuân thủ quy chuẩn quốc tế hàng hải IMO. Module này cung cấp các công cụ tính toán, theo dõi và báo cáo theo các tiêu chuẩn:

- ✅ **IMO DCS** (Data Collection System) - MEPC.308(73)
- ✅ **EU MRV** (Monitoring, Reporting and Verification)
- ✅ **CII Rating** (Carbon Intensity Indicator) - MEPC.328(76)
- ✅ **EEOI** (Energy Efficiency Operational Indicator) - MEPC.1/Circ.684
- ✅ **ISO 19030** Ship Performance Measurement

---

## 📐 CÁC CHỈ SỐ VÀ CÔNG THỨC TÍNH

### 1. **CII (Carbon Intensity Indicator)** - Chỉ Số Cường Độ Carbon

#### 📌 Mô tả:
CII là chỉ số đo lường lượng khí thải CO₂ trên mỗi tấn trọng tải và hải lý di chuyển. Đây là tiêu chuẩn bắt buộc từ IMO áp dụng cho tàu ≥ 5000 GT từ năm 2023.

#### 🔢 Công thức:
```
CII = (CO₂ Emissions × 10⁶) / (DWT × Distance)

Trong đó:
- CO₂ Emissions: Tổng lượng khí thải CO₂ (metric tonnes)
- DWT: Deadweight tonnage (tấn trọng tải)
- Distance: Quãng đường di chuyển (nautical miles)
- Kết quả: gCO₂/dwt-nm (gram CO₂ trên tấn trọng tải và hải lý)
```

#### 📊 Phân loại Rating (A-E):
| Rating | Ý nghĩa | Hành động yêu cầu |
|--------|---------|-------------------|
| **A** | Superior | Không cần hành động, hiệu suất xuất sắc |
| **B** | Good | Không cần hành động, hiệu suất tốt |
| **C** | Moderate | Chấp nhận được, tiếp tục theo dõi |
| **D** | Below Average | Cần cải thiện trong 3 năm |
| **E** | Poor | Cần hành động khắc phục ngay lập tức |

#### ⚠️ Quy định tuân thủ:
- Tàu đạt Rating **D** liên tiếp 3 năm hoặc **E** 1 năm → Bắt buộc có **Ship Energy Efficiency Management Plan (SEEMP)**
- Mỗi năm, yêu cầu CII giảm dần theo lộ trình của IMO

#### 🎯 Required CII (CII yêu cầu):
```
Required CII = Reference CII × (1 - Reduction Factor)

Reduction Factor theo năm:
- 2023: 5%
- 2024: 7%
- 2025: 9%
- 2026: 11%
- 2027 trở đi: tăng dần theo quyết định IMO
```

**Ví dụ thực tế:**
```
Tàu container 50,000 DWT
- Nhiên liệu tiêu thụ: 150 MT
- Quãng đường: 500 NM
- CO₂ = 150 × 3.114 = 467.1 MT
- CII = (467.1 × 10⁶) / (50,000 × 500) = 18.68 gCO₂/dwt-nm

Nếu Required CII = 15.5 → Rating có thể là C hoặc D
```

---

### 2. **EEOI (Energy Efficiency Operational Indicator)** - Chỉ Số Hiệu Suất Năng Lượng Hoạt Động

#### 📌 Mô tả:
EEOI đo lường lượng CO₂ phát thải trên mỗi tấn hàng hóa vận chuyển và hải lý. Khác với CII, EEOI tính theo **cargo** (hàng hóa) thay vì DWT.

#### 🔢 Công thức:
```
EEOI = (∑ Fuel × Carbon Factor) / (Cargo × Distance)

Trong đó:
- Fuel: Tổng nhiên liệu tiêu thụ (metric tonnes)
- Carbon Factor: Hệ số CO₂ theo loại nhiên liệu
  * HFO (Heavy Fuel Oil): 3.114
  * MDO/MGO (Marine Diesel/Gas Oil): 3.206
  * LNG (Liquefied Natural Gas): 2.750
- Cargo: Tổng trọng lượng hàng hóa (metric tonnes)
- Distance: Quãng đường (nautical miles)
- Kết quả: gCO₂/tonne-nm
```

#### 📊 Cách sử dụng:
- EEOI thấp = Hiệu suất cao (ít CO₂ hơn trên mỗi tấn hàng)
- So sánh EEOI giữa các chuyến đi để tối ưu hóa
- Theo dõi xu hướng EEOI để phát hiện suy giảm hiệu suất

**Ví dụ thực tế:**
```
Chuyến đi Việt Nam → Singapore:
- HFO: 120 MT
- MDO: 30 MT
- Hàng hóa: 25,000 MT
- Quãng đường: 450 NM

CO₂ = (120 × 3.114) + (30 × 3.206) = 373.68 + 96.18 = 469.86 MT
EEOI = (469.86 × 10⁶) / (25,000 × 450) = 41.76 gCO₂/tonne-nm
```

---

### 3. **SFOC (Specific Fuel Oil Consumption)** - Suất Tiêu Hao Nhiên Liệu Riêng

#### 📌 Mô tả:
SFOC đo lường lượng nhiên liệu tiêu thụ của động cơ trên mỗi kilowatt giờ (kWh) sản xuất. Đây là chỉ số quan trọng để đánh giá hiệu suất động cơ.

#### 🔢 Công thức:
```
SFOC = (Fuel Consumed × 10⁶) / (Power Output × Operating Hours)

Trong đó:
- Fuel Consumed: Nhiên liệu tiêu thụ (metric tonnes)
- Power Output: Công suất động cơ (kW)
- Operating Hours: Số giờ hoạt động
- Kết quả: g/kWh (gram trên kilowatt giờ)
```

#### 📊 Giá trị tham khảo:
| Loại động cơ | SFOC chuẩn | SFOC tốt | SFOC kém |
|--------------|------------|----------|----------|
| **2-stroke (Main Engine)** | 165-175 g/kWh | < 160 g/kWh | > 180 g/kWh |
| **4-stroke (Auxiliary)** | 190-210 g/kWh | < 185 g/kWh | > 220 g/kWh |

#### ⚙️ Yếu tố ảnh hưởng:
- **Tải động cơ** (Engine Load): SFOC tối ưu ở 75-85% load
- **Nhiệt độ nước làm mát**: Nhiệt độ cao hơn → SFOC thấp hơn
- **Chất lượng nhiên liệu**: Nhiên liệu sạch → SFOC thấp hơn
- **Bảo trì**: Động cơ được bảo trì tốt → SFOC ổn định

**Ví dụ thực tế:**
```
Động cơ chính:
- Công suất: 15,000 kW
- Nhiên liệu: 8.5 MT trong 24 giờ
- SFOC = (8.5 × 10⁶) / (15,000 × 24) = 23.61 g/kWh

→ Kết quả: 23.61 g/kWh là RẤT TỐT cho động cơ 2-stroke
```

---

### 4. **Fuel per Nautical Mile** - Tiêu Hao Nhiên Liệu Trên Hải Lý

#### 📌 Mô tả:
Chỉ số đơn giản nhưng hiệu quả để đánh giá hiệu suất nhiên liệu theo quãng đường.

#### 🔢 Công thức:
```
Fuel/NM = Total Fuel Consumed / Distance Traveled

Trong đó:
- Total Fuel Consumed: Tổng nhiên liệu (metric tonnes hoặc gallons)
- Distance Traveled: Quãng đường (nautical miles)
- Kết quả: MT/NM hoặc gallons/NM
```

#### 📊 Giá trị tham khảo (Container Ship 50,000 DWT):
| Tốc độ | Fuel/NM | Ghi chú |
|--------|---------|---------|
| **10 knots** | 0.20 MT/NM | Tối ưu nhất |
| **15 knots** | 0.35 MT/NM | Cân bằng tốt |
| **20 knots** | 0.65 MT/NM | Tiêu hao cao |
| **25 knots** | 1.20 MT/NM | Không kinh tế |

---

### 5. **CO₂ Emissions** - Khí Thải Carbon Dioxide

#### 📌 Mô tả:
Tổng lượng CO₂ phát thải từ quá trình đốt nhiên liệu, tính theo loại nhiên liệu và hệ số phát thải của IMO.

#### 🔢 Công thức:
```
CO₂ = ∑ (Fuel Type × Emission Factor)

Emission Factors (IMO MEPC.308(73)):
- HFO (Heavy Fuel Oil): 3.114 tonnes CO₂/tonne fuel
- MDO (Marine Diesel Oil): 3.206 tonnes CO₂/tonne fuel
- MGO (Marine Gas Oil): 3.206 tonnes CO₂/tonne fuel
- LNG (Liquefied Natural Gas): 2.750 tonnes CO₂/tonne fuel
- Methanol: 1.375 tonnes CO₂/tonne fuel
```

**Ví dụ tính toán:**
```
Chuyến đi 7 ngày:
- HFO: 200 MT
- MDO: 50 MT
- LNG: 20 MT (nếu có dual-fuel engine)

CO₂ = (200 × 3.114) + (50 × 3.206) + (20 × 2.750)
    = 622.8 + 160.3 + 55.0
    = 838.1 MT CO₂
```

---

### 6. **Average Speed** - Tốc Độ Trung Bình

#### 🔢 Công thức:
```
Average Speed = Distance / Time

Trong đó:
- Distance: Quãng đường (nautical miles)
- Time: Thời gian (hours)
- Kết quả: knots (1 knot = 1 NM/hour)
```

#### 📊 Mối quan hệ Tốc độ - Nhiên liệu:
Công suất cần thiết tỷ lệ với **bình phương tốc độ** (đối với tàu thủy):
```
Power ∝ Speed²

Ví dụ:
- Tăng tốc từ 15 → 20 knots (tăng 33%)
- Công suất cần tăng: (20/15)² = 1.78 lần (tăng 78%)
- Nhiên liệu tiêu thụ tăng tương ứng 78%
```

#### 💡 Slow Steaming Strategy:
- Giảm tốc độ từ 20 → 15 knots → Tiết kiệm ~44% nhiên liệu
- Được áp dụng rộng rãi để giảm chi phí và CO₂

---

### 7. **Distance Calculation** - Tính Quãng Đường

#### 📌 Mô tả:
Sử dụng **Haversine Formula** để tính quãng đường giữa 2 tọa độ GPS trên bề mặt trái đất.

#### 🔢 Công thức Haversine:
```
a = sin²(Δlat/2) + cos(lat1) × cos(lat2) × sin²(Δlon/2)
c = 2 × atan2(√a, √(1−a))
Distance = R × c

Trong đó:
- lat1, lon1: Tọa độ điểm 1 (radians)
- lat2, lon2: Tọa độ điểm 2 (radians)
- R = 3440.065 (bán kính trái đất tính bằng nautical miles)
- Kết quả: nautical miles
```

#### 💻 Implementation:
```csharp
private double CalculateDistanceTraveled(
    double lat1, double lon1, 
    double lat2, double lon2)
{
    var R = 3440.065; // Earth radius in nautical miles
    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);
    
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c;
}
```

---

### 8. **Data Quality Score** - Điểm Chất Lượng Dữ Liệu

#### 📌 Mô tả:
Đánh giá độ tin cậy của dữ liệu thu thập được, quan trọng cho báo cáo IMO DCS và EU MRV.

#### 🔢 Công thức:
```
Data Quality Score = (Valid Data Points / Total Data Points) × 100%

Tiêu chí đánh giá:
- GPS position valid ✓
- Fuel consumption > 0 ✓
- Speed > 0 và < 30 knots ✓
- Engine data trong phạm vi hợp lý ✓
- Timestamps liên tục (không có gap > 1 hour) ✓
```

#### 📊 Phân loại:
| Score | Chất lượng | Ghi chú |
|-------|------------|---------|
| **95-100%** | Excellent | Đủ tiêu chuẩn báo cáo IMO |
| **85-94%** | Good | Chấp nhận được |
| **70-84%** | Fair | Cần cải thiện hệ thống |
| **< 70%** | Poor | Không đủ tiêu chuẩn |

---

## 🔄 LUỒNG TÍNH TOÁN TRONG FUELANALYTICSSERVICE

### 📊 Method: `CalculateFuelEfficiency()`

**Input:**
- `startDate`, `endDate`: Khoảng thời gian phân tích
- `vesselImo`: Mã IMO của tàu

**Process:**
```
1. Lấy dữ liệu từ database:
   ├── Position data (GPS)
   ├── Fuel consumption records
   ├── Engine telemetry
   └── Vessel specifications

2. Tính toán các chỉ số:
   ├── Distance Traveled (Haversine)
   ├── Total Fuel Consumed (HFO + MDO + MGO)
   ├── CO₂ Emissions (Fuel × Emission Factors)
   ├── CII (CO₂ × 10⁶ / DWT × Distance)
   ├── EEOI (CO₂ × 10⁶ / Cargo × Distance)
   ├── SFOC (Fuel × 10⁶ / Power × Hours)
   ├── Average Speed (Distance / Time)
   └── Fuel per NM (Fuel / Distance)

3. Xác định CII Rating:
   ├── Calculate Required CII
   ├── Calculate boundaries (A/B/C/D/E)
   └── Determine actual rating

4. Đánh giá Data Quality Score

5. Tạo recommendations (nếu rating D/E)
```

**Output:**
```csharp
FuelAnalyticsSummary {
    // Time range
    StartDate, EndDate,
    
    // Fuel data
    TotalFuelConsumedMT,
    HFOConsumedMT,
    MDOConsumedMT,
    MGOConsumedMT,
    
    // Distance & Speed
    DistanceNauticalMiles,
    AverageSpeedKnots,
    FuelPerNauticalMile,
    
    // Emissions
    CO2EmissionsMT,
    
    // Efficiency Indicators
    EEOI,
    CII,
    CIIRating (A/B/C/D/E),
    SFOC,
    
    // Engine Performance
    AvgMainEngineRPM,
    AvgMainEngineLoad,
    AvgMainEnginePower,
    
    // Quality
    DataQualityScore
}
```

---

### 📊 Method: `GetCIIDetails()`

**Chức năng:**
Trả về thông tin chi tiết về CII Rating, bao gồm:
- Actual CII (CII thực tế)
- Required CII (CII yêu cầu theo IMO)
- Deviation % (% chênh lệch)
- Rating boundaries (ngưỡng A/B/C/D/E)
- Compliance status
- Recommendations

**Logic:**
```
1. Calculate Actual CII:
   CII = (CO₂ × 10⁶) / (DWT × Distance)

2. Calculate Required CII:
   Required = Reference CII × (1 - Reduction Factor)
   
   Reduction factors:
   - 2023: 5%
   - 2024: 7%
   - 2025: 9%
   - 2026: 11%

3. Determine Rating:
   RatingA: < Required × 0.85
   RatingB: < Required × 0.95
   RatingC: < Required × 1.10
   RatingD: < Required × 1.25
   RatingE: ≥ Required × 1.25

4. Check Compliance:
   IsCompliant = (Rating is A, B, or C)

5. Generate Recommendations (if D or E):
   - Speed optimization
   - Hull cleaning
   - Engine maintenance
   - Route optimization
```

---

### 📊 Method: `GetTrendData()`

**Chức năng:**
Trả về dữ liệu xu hướng theo ngày/tuần/tháng để vẽ biểu đồ.

**Output:**
```csharp
List<TrendDataPoint> {
    Date,
    FuelConsumedMT,
    CO2EmissionsMT,
    DistanceNM,
    EEOI,
    CII,
    AverageSpeed
}
```

**Use cases:**
- Vẽ biểu đồ line chart fuel consumption
- So sánh EEOI giữa các tháng
- Phát hiện anomalies (tăng đột biến)

---

### 📊 Method: `PredictFuelConsumption()`

**Chức năng:**
Dự đoán lượng nhiên liệu cần thiết cho chuyến đi sắp tới.

**Input:**
```csharp
FuelPredictionRequest {
    Distance (NM),
    AverageSpeed (knots),
    CargoWeight (MT),
    WeatherCondition (Good/Moderate/Bad)
}
```

**Algorithm:**
```
1. Lấy historical SFOC trung bình
2. Tính power cần thiết:
   Power = f(Speed, Cargo, Weather)
   
3. Tính thời gian:
   Time = Distance / Speed
   
4. Dự đoán fuel:
   Fuel = (SFOC × Power × Time) / 10⁶
   
5. Thêm buffer (5-10% dự phòng)
```

---

## 📈 CÁC API ENDPOINTS

### 1. **GET /api/fuel-analytics/efficiency**
Lấy dữ liệu hiệu suất nhiên liệu

**Parameters:**
- `startDate` (datetime)
- `endDate` (datetime)
- `vesselImo` (string, optional)

**Response:**
```json
{
  "totalFuelConsumedMT": 123.45,
  "distanceNauticalMiles": 567.89,
  "averageSpeedKnots": 14.5,
  "eeoi": 32.1,
  "cii": 18.5,
  "ciiRating": "C",
  "co2EmissionsMT": 384.2,
  "dataQualityScore": 98.5
}
```

---

### 2. **GET /api/fuel-analytics/cii-rating**
Lấy chi tiết CII Rating

**Parameters:**
- `year` (int, default: current year)
- `vesselImo` (string, optional)

**Response:**
```json
{
  "year": 2025,
  "actualCII": 18.5,
  "requiredCII": 15.2,
  "rating": "D",
  "isCompliant": false,
  "deviationPercent": 21.7,
  "complianceStatus": "Below required CII. Action plan needed.",
  "boundaries": {
    "ratingA": 12.9,
    "ratingB": 14.4,
    "ratingC": 16.7,
    "ratingD": 19.0,
    "ratingE": 19.1
  },
  "recommendations": [
    "Reduce average speed to 12-14 knots",
    "Schedule hull cleaning and propeller polishing",
    "Optimize voyage routes to minimize distance"
  ]
}
```

---

### 3. **GET /api/fuel-analytics/trend**
Lấy dữ liệu xu hướng

**Parameters:**
- `startDate`, `endDate`
- `granularity` (Daily/Weekly/Monthly)

**Response:**
```json
[
  {
    "date": "2025-10-20",
    "fuelConsumedMT": 18.5,
    "co2EmissionsMT": 57.6,
    "distanceNM": 345.2,
    "eeoi": 28.3,
    "cii": 16.8
  }
]
```

---

### 4. **POST /api/fuel-analytics/predict**
Dự đoán tiêu thụ nhiên liệu

**Request Body:**
```json
{
  "distance": 1200,
  "averageSpeed": 15,
  "cargoWeight": 30000,
  "weatherCondition": "Moderate"
}
```

**Response:**
```json
{
  "estimatedFuelMT": 165.4,
  "estimatedCO2MT": 515.2,
  "estimatedDuration": "80 hours",
  "confidenceLevel": 0.85
}
```

---

### 5. **GET /api/fuel-analytics/dashboard**
Lấy tổng hợp dashboard

**Response:**
```json
{
  "weeklySummary": { ... },
  "monthlySummary": { ... },
  "ciiRating": { ... },
  "trend": [ ... ],
  "generatedAt": "2025-10-27T10:30:00Z"
}
```

---

## 🎓 HỌC TẬP VÀ TÀI LIỆU THAM KHẢO

### 📚 IMO Regulations:
1. **MEPC.308(73)** - IMO DCS (Data Collection System)
2. **MEPC.328(76)** - CII Rating System
3. **MEPC.1/Circ.684** - EEOI Guidelines
4. **MARPOL Annex VI** - Air Pollution Prevention

### 🌐 Nguồn học online:
- [IMO Official Website](http://www.imo.org)
- [DNV Maritime Academy](https://www.dnv.com/maritime)
- [Lloyd's Register Training](https://www.lr.org/en/maritime-training/)

### 📖 Sách tham khảo:
- "Ship Energy Efficiency" - DNV
- "Maritime Energy Management" - IMO
- "Carbon Intensity Indicator Guide" - Lloyd's Register

---

## 🛠️ TROUBLESHOOTING

### ❌ CII = 0 hoặc NaN
**Nguyên nhân:**
- Không có dữ liệu fuel consumption
- Distance = 0 (tàu đang neo đậu)
- DWT không được config

**Giải pháp:**
```csharp
// Check appsettings.json
"VesselSpecifications": {
  "DeadweightTonnage": 50000  // Phải > 0
}
```

### ❌ Data Quality Score thấp
**Nguyên nhân:**
- Sensor lỗi
- GPS signal mất
- Fuel meter không chính xác

**Giải pháp:**
- Kiểm tra hardware sensors
- Verify data trong database
- Thêm data validation

---

## 💡 BEST PRACTICES

### 1. **Tần suất thu thập dữ liệu**
- Position: Mỗi 5-10 phút
- Fuel: Mỗi 15 phút (hoặc khi có thay đổi > 0.5 MT)
- Engine: Mỗi 5 phút

### 2. **Data validation**
- Fuel consumption không được âm
- Speed phải trong khoảng 0-30 knots
- Engine RPM/Load phải hợp lý

### 3. **Backup và reporting**
- Backup dữ liệu hàng ngày
- Tạo monthly reports tự động
- Archive data sau 3 năm (yêu cầu IMO)

---

**🚢 Phát triển bởi Maritime Edge System Team**
*Phiên bản: 1.0.0 | Ngày cập nhật: 27/10/2025*
