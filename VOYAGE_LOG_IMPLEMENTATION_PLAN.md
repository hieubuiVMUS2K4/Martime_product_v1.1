# 🚢 Voyage Log (Nhật ký Hành trình) - Kế hoạch Triển khai

## 📋 Tổng quan

**Mục đích**: Ghi lại hành trình di chuyển của tàu - xuất cảng, nhập cảng, vị trí trên biển.

**Nguyên tắc thiết kế**:
- ✅ Đơn giản, tập trung vào thông tin cốt lõi
- ✅ Không trùng lặp với các logbook khác (Deck Log, Engine Log, etc.)
- ✅ Tuân thủ tiêu chuẩn quốc tế (SOLAS, ISM Code)

---

## 📜 Tiêu chuẩn & Quy định áp dụng

| Tiêu chuẩn | Mô tả | Yêu cầu |
|------------|-------|---------|
| **SOLAS Chapter V, Reg 28** | Voyage Data Recording | Ghi nhận thời gian, vị trí, sự kiện hành trình |
| **ISM Code** | International Safety Management | Lưu trữ hồ sơ hành trình |
| **UN/LOCODE** | Mã cảng quốc tế | Sử dụng mã chuẩn cho cảng (VD: VNSGN = TP.HCM) |
| **Port State Control** | Kiểm tra tàu tại cảng | Yêu cầu trình bày lịch sử hành trình |

---

## 🎯 Các loại Sự kiện Hành trình (Event Types)

### Sự kiện Cảng (Port Events)
| Code | Tên tiếng Anh | Tên tiếng Việt | Mô tả |
|------|---------------|----------------|-------|
| `DEP` | Departure | Rời cảng | Tàu rời cầu cảng (Cast Off) |
| `ARR` | Arrival | Cập cảng | Tàu cập cầu cảng (All Fast) |
| `ANCHOR_DROP` | Anchor Drop | Thả neo | Tàu thả neo tại vũng neo |
| `ANCHOR_UP` | Anchor Up | Kéo neo | Tàu thu neo |

### Sự kiện Hành trình (Passage Events)
| Code | Tên tiếng Anh | Tên tiếng Việt | Mô tả |
|------|---------------|----------------|-------|
| `COSP` | Commencement of Sea Passage | Bắt đầu hành trình | Pilot xuống, bắt đầu chạy biển |
| `EOSP` | End of Sea Passage | Kết thúc hành trình | Pilot lên, sắp vào cảng |
| `NOON` | Noon Position | Vị trí trưa | Báo cáo vị trí 12:00 UTC hàng ngày |

### Sự kiện Đặc biệt (Special Events)
| Code | Tên tiếng Anh | Tên tiếng Việt | Mô tả |
|------|---------------|----------------|-------|
| `PILOT_ON` | Pilot Boarding | Hoa tiêu lên tàu | Hoa tiêu lên tàu dẫn đường |
| `PILOT_OFF` | Pilot Disembark | Hoa tiêu rời tàu | Hoa tiêu rời tàu |
| `DRIFT` | Drifting | Trôi dạt | Tàu trôi (chờ lệnh, thời tiết xấu) |
| `DEVIATION` | Route Deviation | Đổi hành trình | Thay đổi tuyến đường (SAR, thời tiết) |

---

## 📊 Cấu trúc Dữ liệu

### Model: `VoyageLogEntry`

```csharp
public class VoyageLogEntry
{
    // === Identification ===
    public Guid Id { get; set; }
    public Guid? VoyageId { get; set; }           // Link to VoyageRecord (optional)
    
    // === Event Info ===
    public string EventType { get; set; }          // DEP, ARR, NOON, COSP, EOSP, etc.
    public DateTime EventDateTime { get; set; }    // UTC time
    public DateTime EventDateTimeLocal { get; set; } // Local time
    public string TimeZone { get; set; }           // e.g., "UTC+7"
    
    // === Position ===
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // === Port Info (for DEP/ARR) ===
    public string? PortName { get; set; }          // e.g., "Ho Chi Minh City"
    public string? PortLocode { get; set; }        // UN/LOCODE: "VNSGN"
    public string? PortCountry { get; set; }       // e.g., "Vietnam"
    public string? BerthNumber { get; set; }       // e.g., "Berth 5"
    
    // === Distance & Navigation ===
    public double? DistanceToGo { get; set; }      // NM to next port
    public double? DistanceFromLast { get; set; }  // NM from last event
    public double? TotalVoyageDistance { get; set; } // NM total
    public double? CourseOverGround { get; set; }  // Degrees
    public double? SpeedOverGround { get; set; }   // Knots
    
    // === Pilot Info (for PILOT_ON/OFF) ===
    public string? PilotName { get; set; }
    public string? PilotStation { get; set; }
    
    // === Officer & Signature ===
    public string OfficerOnWatch { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    
    // === Remarks ===
    public string? Remarks { get; set; }
    
    // === System Fields ===
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OriginNode { get; set; }
}
```

### TypeScript DTO

```typescript
// Frontend Types
export interface CreateVoyageLogEntryDto {
  voyageId?: string;
  eventType: string;
  eventDateTime: string;
  eventDateTimeLocal?: string;
  timeZone?: string;
  latitude: number;
  longitude: number;
  portName?: string;
  portLocode?: string;
  portCountry?: string;
  berthNumber?: string;
  distanceToGo?: number;
  distanceFromLast?: number;
  totalVoyageDistance?: number;
  courseOverGround?: number;
  speedOverGround?: number;
  pilotName?: string;
  pilotStation?: string;
  officerOnWatch: string;
  remarks?: string;
}

export interface VoyageLogEntryResponseDto extends CreateVoyageLogEntryDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}
```

---

## 🖥️ Giao diện Frontend

### Layout chính

```
┌─────────────────────────────────────────────────────────────┐
│  🚢 Voyage Log - Nhật ký Hành trình                [+ New]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─── Timeline View ───────────────────────────────────┐   │
│  │                                                      │   │
│  │  ● 15/12 08:00  DEP - VNSGN (Ho Chi Minh)           │   │
│  │  │              Berth 5, Pilot: Nguyen Van A         │   │
│  │  │                                                   │   │
│  │  ● 15/12 09:30  PILOT_OFF - Pilot Station           │   │
│  │  │              10.45°N, 107.05°E                    │   │
│  │  │                                                   │   │
│  │  ● 15/12 10:00  COSP - Sea Passage Started          │   │
│  │  │              Course: 045°, Speed: 12.5 kts        │   │
│  │  │              Distance to go: 850 NM               │   │
│  │  │                                                   │   │
│  │  ○ 16/12 12:00  NOON - Position Report              │   │
│  │                 12.30°N, 109.45°E                    │   │
│  │                 Distance run: 280 NM                 │   │
│  │                                                      │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  Table View (toggle)                                        │
│  ┌──────────┬────────┬──────────┬──────────┬────────────┐  │
│  │ DateTime │ Event  │ Port/Pos │ Distance │ Status     │  │
│  ├──────────┼────────┼──────────┼──────────┼────────────┤  │
│  │ 15/12    │ DEP    │ VNSGN    │ -        │ ✓ SIGNED   │  │
│  │ 08:00    │        │          │          │            │  │
│  └──────────┴────────┴──────────┴──────────┴────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Form nhập liệu (Step-by-step)

**Step 1: Chọn loại sự kiện**
```
┌─────────────────────────────────────────┐
│  Select Event Type                      │
│                                         │
│  ┌─ Port Events ─────────────────────┐  │
│  │ [🚢 DEP] Departure - Rời cảng     │  │
│  │ [⚓ ARR] Arrival - Cập cảng       │  │
│  │ [⚓ ANCHOR] Anchoring             │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌─ Passage Events ──────────────────┐  │
│  │ [🌊 COSP] Start Sea Passage       │  │
│  │ [🌊 EOSP] End Sea Passage         │  │
│  │ [☀️ NOON] Noon Position           │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌─ Pilot Events ────────────────────┐  │
│  │ [👤 PILOT_ON] Pilot Boarding      │  │
│  │ [👤 PILOT_OFF] Pilot Disembark    │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

**Step 2: Nhập thông tin (tùy theo loại sự kiện)**

- **DEP/ARR**: Port Name, LOCODE, Berth, Position
- **NOON**: Position, Distance Run, COG, SOG
- **COSP/EOSP**: Position, Distance to Go
- **PILOT_ON/OFF**: Pilot Name, Station, Position

---

## 📁 Cấu trúc Files cần tạo

### Backend (edge-services)

```
edge-services/
├── Models/
│   └── EdgeModels.cs              # Thêm VoyageLogEntry
├── DTOs/
│   └── VoyageLogDtos.cs           # NEW: DTOs cho Voyage Log
├── Services/
│   └── Logbooks/
│       └── VoyageLogService.cs    # NEW: Service xử lý logic
├── Controllers/
│   └── VoyageLogController.cs     # NEW: API Controller
└── Data/
    └── EdgeDbContext.cs           # Thêm DbSet<VoyageLogEntry>
```

### Frontend (frontend-edge)

```
frontend-edge/src/
├── pages/
│   └── logbooks/
│       └── VoyageLogPage.tsx      # NEW: Trang Voyage Log
├── types/
│   └── logbook.types.ts           # Thêm VoyageLog types
└── services/
    └── logbook.service.ts         # Thêm VoyageLog API calls
```

---

## 🔄 API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/api/voyage-log` | Lấy danh sách entries |
| `GET` | `/api/voyage-log/{id}` | Lấy chi tiết entry |
| `POST` | `/api/voyage-log` | Tạo entry mới |
| `PUT` | `/api/voyage-log/{id}` | Cập nhật entry |
| `POST` | `/api/voyage-log/{id}/sign` | Ký xác nhận |
| `GET` | `/api/voyage-log/current-voyage` | Lấy entries của voyage hiện tại |
| `GET` | `/api/voyage-log/timeline` | Lấy dạng timeline |

---

## ✅ Checklist Triển khai

### Phase 1: Backend Model & Database
- [x] Thêm `VoyageLogEntry` vào EdgeModels.cs
- [x] Thêm DbSet vào EdgeDbContext.cs
- [x] Tạo Migration
- [x] Tạo VoyageLogDtos.cs

### Phase 2: Backend Service & Controller
- [x] Tạo IVoyageLogService interface
- [x] Tạo VoyageLogService implementation
- [x] Tạo VoyageLogController
- [x] Register Service trong Program.cs

### Phase 3: Frontend Types & Service
- [x] Thêm types vào logbook.types.ts
- [x] Thêm API calls vào logbook.service.ts

### Phase 4: Frontend UI
- [x] Tạo VoyageLogPage.tsx
- [x] Thêm route vào router
- [x] Thêm menu navigation

### Phase 5: Testing & Polish
- [ ] Test tạo entry
- [ ] Test ký xác nhận
- [ ] Test timeline view
- [ ] Kiểm tra responsive

---

## 🎨 Event Type Colors & Icons

| Event | Color | Icon | Badge |
|-------|-------|------|-------|
| DEP | 🟢 Green | 🚢 | Departure |
| ARR | 🔵 Blue | ⚓ | Arrival |
| NOON | 🟡 Yellow | ☀️ | Position |
| COSP | 🟣 Purple | 🌊 | Sea Start |
| EOSP | 🟣 Purple | 🌊 | Sea End |
| PILOT_ON | 🟠 Orange | 👤 | Pilot On |
| PILOT_OFF | 🟠 Orange | 👤 | Pilot Off |
| ANCHOR_DROP | ⚫ Gray | ⚓ | Anchored |
| ANCHOR_UP | ⚫ Gray | ⚓ | Anchor Up |
| DRIFT | 🔴 Red | ⏸️ | Drifting |

---

## 📝 Ghi chú

1. **Không trùng lặp**: Voyage Log chỉ ghi sự kiện hành trình, không ghi:
   - Thời tiết (→ Deck Log)
   - Tiêu thụ nhiên liệu (→ Engine Log)
   - Xả thải (→ Oil/Garbage Record)
   
2. **Tích hợp VoyageRecord**: Có thể liên kết với VoyageRecord để tạo báo cáo tổng hợp

3. **UN/LOCODE**: Sử dụng mã cảng chuẩn quốc tế để dễ tra cứu và báo cáo

---

**Tạo bởi**: GitHub Copilot  
**Ngày**: 15/12/2025  
**Version**: 1.0
