# Hướng dẫn chức năng Báo động an toàn

## 📱 Tổng quan

Chức năng **Safety Alarm Management** (Quản lý báo động an toàn) cho phép thuyền viên:
- Xem danh sách báo động đang hoạt động (chưa giải quyết)
- Xác nhận đã nhận biết báo động
- Đánh dấu báo động đã được giải quyết
- Xem lịch sử báo động
- Xem thống kê báo động theo nhiều tiêu chí

---

## 🚀 Cách sử dụng

### 1. Truy cập chức năng

**Cách 1:** Từ màn hình Dashboard
- Nhấn vào card **"Báo động an toàn"** trong phần "Truy cập nhanh"

**Cách 2:** Từ Menu (Drawer)
- Mở menu bên trái (nhấn ☰)
- Chọn **"Báo động an toàn"** (biểu tượng 🔔 màu đỏ)

---

### 2. Màn hình danh sách báo động

#### Thống kê tổng quan (cards trên đầu)
- **Nghiêm trọng** (đỏ): Số lượng alarm mức CRITICAL chưa giải quyết
- **Cảnh báo** (cam): Số lượng alarm mức WARNING chưa giải quyết  
- **Chưa xác nhận** (xanh): Số lượng alarm chưa được acknowledge

#### Danh sách báo động
Mỗi card hiển thị:
- **Mức độ nghiêm trọng**: CRITICAL (đỏ), WARNING (cam), INFO (xanh)
- **Badge "MỚI"**: Báo động chưa được xác nhận
- **Loại báo động**: 🔥 Hỏa hoạn, 💧 Nước hầm, ⚙️ Máy móc, 🧭 Hàng hải...
- **Mô tả**: Chi tiết sự cố
- **Vị trí**: Buồng máy, Khoang lái, Boong tàu...
- **Mã báo động**: FIRE-001, BILGE-002...
- **Thời gian**: "5 phút trước", "2 giờ trước"...
- **Trạng thái xác nhận**: "Đã xác nhận bởi Chief Engineer"

#### Tính năng
- **Pull to refresh**: Kéo xuống để làm mới
- **Nhấn vào card**: Xem chi tiết báo động
- **Nút ⟲**: Làm mới danh sách
- **Nút 📊**: Xem thống kê
- **Nút 🕒**: Xem lịch sử

---

### 3. Màn hình chi tiết báo động

#### Thông tin hiển thị
1. **Header**: Mức độ + Loại báo động với màu sắc nổi bật
2. **Badge "CẦN XÁC NHẬN"**: Nếu chưa acknowledge
3. **Mô tả chi tiết**: Thông tin đầy đủ về sự cố
4. **Vị trí**: Khu vực xảy ra sự cố
5. **Mã báo động**: Mã định danh
6. **Thời gian phát sinh**: Ngày giờ chính xác + thời gian trôi qua
7. **Trạng thái**: Chưa xử lý / Đã xác nhận / Đã giải quyết

#### Thông tin xác nhận (nếu đã acknowledge)
- Người xác nhận
- Thời gian xác nhận

#### Thông tin giải quyết (nếu đã resolve)
- Thời gian giải quyết

#### Hành động

**Nút "Xác nhận" (✓)** - Màu xanh lá
- Hiện khi: Báo động chưa được xác nhận
- Chức năng: Xác nhận đã biết về báo động
- Lưu: Tên người xác nhận (từ profile) + thời gian

**Nút "Giải quyết" (✓✓)** - Màu xanh dương  
- Hiện khi: Báo động đã xác nhận nhưng chưa giải quyết
- Chức năng: Đánh dấu đã xử lý xong sự cố
- Có: Dialog xác nhận trước khi thực hiện

---

### 4. Màn hình thống kê

#### Bộ lọc thời gian
Chọn khoảng thời gian: **7 ngày**, **30 ngày**, **90 ngày**

#### Thống kê tổng quan (4 cards)
- **Tổng số**: Tất cả báo động
- **Đang hoạt động**: Chưa giải quyết
- **Đã xác nhận**: Đã acknowledge
- **Đã giải quyết**: Đã resolve

#### Biểu đồ phân tích

**1. Theo mức độ nghiêm trọng**
- CRITICAL: Màu đỏ
- WARNING: Màu cam
- INFO: Màu xanh
- Hiển thị: Progress bar + số lượng + phần trăm

**2. Theo loại báo động**
- FIRE, BILGE, ENGINE, NAVIGATION, SAFETY, SECURITY...
- Màu: Indigo
- Hiển thị: Progress bar + số lượng + phần trăm

**3. Theo vị trí**
- ENGINE_ROOM, BRIDGE, DECK, GALLEY...
- Màu: Teal
- Hiển thị: Progress bar + số lượng + phần trăm

---

### 5. Màn hình lịch sử

#### Bộ lọc
Chọn: **7 ngày**, **30 ngày**, **90 ngày**

#### Danh sách lịch sử
Hiển thị tất cả báo động (đã giải quyết + chưa giải quyết)

Mỗi card có:
- Icon theo mức độ nghiêm trọng
- Tên loại báo động
- Ngày giờ phát sinh
- Mô tả
- Vị trí
- **Badge trạng thái**:
  - "Đã giải quyết" (✓) - Xanh lá
  - "Đã xác nhận" (✓) - Xanh dương

#### Tính năng
- **Pull to refresh**: Kéo xuống để làm mới
- **Nhấn vào card**: Xem chi tiết
- **Sắp xếp**: Mới nhất trước

---

## 🎨 Màu sắc & Ý nghĩa

### Mức độ nghiêm trọng
- 🔴 **CRITICAL** (Đỏ): Nguy hiểm nghiêm trọng, cần xử lý ngay
- 🟠 **WARNING** (Cam): Cảnh báo, cần chú ý
- 🔵 **INFO** (Xanh): Thông tin, không khẩn cấp

### Loại báo động
- 🔥 **FIRE**: Hỏa hoạn
- 💧 **BILGE**: Nước hầm
- ⚙️ **ENGINE**: Máy móc
- 🧭 **NAVIGATION**: Hàng hải
- ⚠️ **SAFETY**: An toàn
- 🔒 **SECURITY**: Bảo mật

### Vị trí
- **ENGINE_ROOM**: Buồng máy
- **BRIDGE**: Khoang lái
- **DECK**: Boong tàu
- **GALLEY**: Bếp
- **MACHINERY_SPACE**: Khu máy móc
- **CARGO_HOLD**: Khoang hàng

---

## 🔧 API Endpoints (cho dev)

### Backend đang chạy
```
Server: http://localhost:5000
Database: PostgreSQL (port 5433)
```

### Danh sách API

#### 1. Lấy báo động đang hoạt động
```http
GET /api/alarms/active
```
Trả về: Tất cả alarms chưa resolve (IsResolved = false)

#### 2. Lấy lịch sử báo động
```http
GET /api/alarms/history?days=7
```
Parameters:
- `days`: Số ngày lấy lịch sử (default: 7)

#### 3. Xác nhận báo động
```http
POST /api/alarms/{id}/acknowledge
Content-Type: application/json

{
  "acknowledgedBy": "Tên người xác nhận"
}
```

#### 4. Giải quyết báo động
```http
POST /api/alarms/{id}/resolve
```

#### 5. Lấy thống kê
```http
GET /api/alarms/statistics?days=30
```
Parameters:
- `days`: Số ngày thống kê (default: 30)

#### 6. Tạo báo động mẫu (DEV)
```http
POST /api/alarms/test/generate-sample
```
Tạo 5 alarm mẫu với các mức độ và trạng thái khác nhau

#### 7. Tạo báo động thủ công (DEV)
```http
POST /api/alarms/create
Content-Type: application/json

{
  "alarmType": "FIRE",
  "alarmCode": "FIRE-001",
  "severity": "CRITICAL",
  "location": "ENGINE_ROOM",
  "description": "High temperature detected"
}
```

---

## 📊 Database Schema

### Table: SafetyAlarms
```sql
CREATE TABLE "SafetyAlarms" (
    "Id" SERIAL PRIMARY KEY,
    "Timestamp" TIMESTAMP NOT NULL,
    "AlarmType" VARCHAR(50) NOT NULL,
    "AlarmCode" VARCHAR(50),
    "Severity" VARCHAR(20) NOT NULL,
    "Location" VARCHAR(100),
    "Description" TEXT,
    "IsAcknowledged" BOOLEAN DEFAULT FALSE,
    "AcknowledgedBy" VARCHAR(100),
    "AcknowledgedAt" TIMESTAMP,
    "IsResolved" BOOLEAN DEFAULT FALSE,
    "ResolvedAt" TIMESTAMP,
    "IsSynced" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);
```

---

## 🧪 Testing

### 1. Tạo dữ liệu mẫu
```bash
curl -X POST http://localhost:5000/api/alarms/test/generate-sample
```

### 2. Kiểm tra alarm đang hoạt động
```bash
curl http://localhost:5000/api/alarms/active
```

### 3. Xác nhận alarm
```bash
curl -X POST http://localhost:5000/api/alarms/5/acknowledge \
  -H "Content-Type: application/json" \
  -d '{"acknowledgedBy":"Test User"}'
```

### 4. Giải quyết alarm
```bash
curl -X POST http://localhost:5000/api/alarms/5/resolve
```

---

## 🎯 Luồng xử lý báo động

```
1. Phát sinh báo động
   ↓
2. Hiện trong danh sách ACTIVE
   ├─ Badge "MỚI"
   ├─ Border màu theo severity (đỏ/cam/xanh)
   └─ Sắp xếp: CRITICAL > WARNING > INFO > Timestamp
   ↓
3. Thuyền viên nhấn vào card
   ↓
4. Xem chi tiết
   ↓
5. Nhấn nút "Xác nhận" ✓
   ├─ Lưu: AcknowledgedBy + AcknowledgedAt
   ├─ IsAcknowledged = true
   └─ Badge "MỚI" biến mất
   ↓
6. Xử lý sự cố thực tế
   ↓
7. Nhấn nút "Giải quyết" ✓✓
   ├─ Xác nhận qua dialog
   ├─ Lưu: ResolvedAt
   ├─ IsResolved = true
   └─ Xóa khỏi danh sách ACTIVE
   ↓
8. Báo động chuyển sang HISTORY
```

---

## 📱 Screenshots Flow

### 1. Dashboard → Báo động an toàn
```
[Dashboard]
  ├─ Thống kê nhiệm vụ
  └─ Truy cập nhanh
      └─ [🔔 Báo động an toàn] ← Nhấn vào đây
```

### 2. Danh sách báo động
```
[Alarm List Screen]
  ├─ Header: "Báo động an toàn"
  ├─ Actions: [📊 Statistics] [🕒 History]
  ├─ Summary Cards: [Nghiêm trọng: 2] [Cảnh báo: 1] [Chưa xác nhận: 3]
  └─ Danh sách:
      ├─ [🔥 FIRE-005 | CRITICAL | MỚI | 5 phút trước]
      ├─ [🔥 FIRE-001 | CRITICAL | MỚI | 30 phút trước]
      └─ [💧 BILGE-002 | WARNING | Đã xác nhận | 2 giờ trước]
```

### 3. Chi tiết báo động
```
[Alarm Detail Screen]
  ├─ Header: [⚠️ NGHIÊM TRỌNG - 🔥 Hỏa hoạn]
  ├─ Badge: [CẦN XÁC NHẬN]
  ├─ Mô tả: "High temperature detected in main engine exhaust"
  ├─ Vị trí: Buồng máy
  ├─ Mã: FIRE-001
  ├─ Thời gian: 29/10/2025 18:01:08 (30 phút trước)
  └─ FAB: [✓ Xác nhận]
```

### 4. Sau khi xác nhận
```
[Alarm Detail Screen]
  ├─ Thông tin xác nhận:
  │   ├─ Người xác nhận: Nguyễn Văn A
  │   └─ Thời gian: 29/10/2025 18:35:00
  └─ FAB: [✓✓ Giải quyết]
```

---

## ⚠️ Lưu ý

1. **Yêu cầu kết nối mạng**: Chức năng này cần kết nối với Edge Server
2. **Tên người xác nhận**: Lấy từ SharedPreferences (crew_name)
3. **Xác nhận trước khi giải quyết**: Phải acknowledge trước khi resolve
4. **Real-time**: Dữ liệu không tự động refresh, cần pull-to-refresh hoặc nhấn nút refresh
5. **Sorting**: Active alarms tự động sắp xếp theo mức độ nghiêm trọng

---

## 🚀 Tính năng mở rộng (Future)

- [ ] Push notification khi có báo động mới
- [ ] Âm thanh/rung khi báo động CRITICAL
- [ ] Real-time updates qua WebSocket/SignalR
- [ ] Thêm ảnh/video vào báo động
- [ ] Ghi chú khi xác nhận/giải quyết
- [ ] Export báo cáo PDF
- [ ] Dashboard thống kê nâng cao
- [ ] Offline mode với sync queue

---

## 📚 Tài liệu liên quan

- **Backend API**: `edge-services/Controllers/AlarmsController.cs`
- **Database Schema**: `edge-services/Data/EdgeModels.cs` (SafetyAlarm)
- **Mobile Models**: `lib/data/models/safety_alarm.dart`
- **UI Screens**: `lib/presentation/screens/alarms/`
- **Provider**: `lib/presentation/providers/alarm_provider.dart`

---

**Ngày tạo**: 29/10/2025  
**Version**: 1.0  
**Author**: GitHub Copilot
