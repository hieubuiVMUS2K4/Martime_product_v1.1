# 📋 HƯỚNG DẪN SỬ DỤNG WATCHKEEPING LOG

## Tổng quan

Watchkeeping Log là sổ ghi ca trực bắt buộc theo các quy định hàng hải quốc tế:
- **SOLAS Chapter V/28** - Yêu cầu ghi nhận ca trực buồng lái
- **STCW Convention** - Quy định giờ nghỉ ngơi tối thiểu
- **MLC 2006** - Quản lý mệt mỏi thuyền viên

---

## 🚀 Truy cập Watchkeeping Log

1. Đăng nhập vào hệ thống Edge
2. Truy cập menu **Logbooks** → **Watchkeeping Log**
3. Giao diện chính hiển thị danh sách các ca trực đã ghi

---

## ➕ Tạo mới một Ca Trực

### Bước 1: Mở Form tạo mới
Click nút **"+ New Watch"** ở góc trên bên phải

### Bước 2: Điền thông tin cơ bản

#### Watch Information (Thông tin ca trực)
| Trường | Mô tả | Bắt buộc |
|--------|-------|----------|
| **Watch Date** | Ngày trực ca | ✅ |
| **Watch Period** | Ca trực (6 ca/ngày) | ✅ |
| **Watch Type** | Loại ca: NAVIGATION hoặc ENGINE | ✅ |
| **Officer on Watch** | Sĩ quan trực ca | ✅ |
| **Relief Officer** | Sĩ quan nhận bàn giao | ❌ |
| **Lookout** | Người quan sát (AB) | ❌ |
| **Bridge Manning Level** | Số người trên buồng lái | ✅ |
| **Lookout Posted** | Đã bố trí người quan sát | ✅ |

#### Các ca trực tiêu chuẩn:
```
🌙 00:00 - 04:00  Middle Watch (Ca nửa đêm)
🌅 04:00 - 08:00  Morning Watch (Ca sáng)
☀️ 08:00 - 12:00  Forenoon Watch (Ca trước trưa)
🌞 12:00 - 16:00  Afternoon Watch (Ca chiều)
🌆 16:00 - 20:00  First Dog/Second Dog (Ca chó)
🌃 20:00 - 24:00  First Watch (Ca đầu)
```

### Bước 3: Điền thông tin Hàng hải & Thời tiết

#### Navigation & Weather
| Trường | Mô tả | Đơn vị |
|--------|-------|--------|
| **Course Logged** | Hướng đi | Độ True (°T) |
| **Speed Logged** | Tốc độ | Knots (hải lý/giờ) |
| **Distance Run** | Quãng đường đã đi | Nautical Miles |
| **Position Lat/Lon** | Vị trí tàu | Độ |
| **Sea State** | Trạng thái biển (Douglas Scale) | 0-8 |
| **Visibility** | Tầm nhìn xa | NM |
| **Weather Conditions** | Điều kiện thời tiết | Mô tả |

#### Douglas Sea Scale (Thang đo trạng thái biển):
| Cấp | Tên | Chiều cao sóng |
|-----|-----|----------------|
| 0 | Calm (Lặng) | 0 m |
| 1 | Smooth (Phẳng) | 0-0.1 m |
| 2 | Slight (Nhẹ) | 0.1-0.5 m |
| 3 | Moderate (Vừa) | 0.5-1.25 m |
| 4 | Rough (Gợn) | 1.25-2.5 m |
| 5 | Very Rough (Dữ) | 2.5-4 m |
| 6 | High (Cao) | 4-6 m |
| 7 | Very High (Rất cao) | 6-9 m |
| 8 | Phenomenal (Bất thường) | >9 m |

#### Tầm nhìn xa:
| Loại | Khoảng cách |
|------|-------------|
| Good (Tốt) | > 5 NM |
| Moderate (Trung bình) | 2-5 NM |
| Poor (Kém) | 0.5-2 NM |
| Fog (Sương mù) | < 0.5 NM |

---

## ⚠️ STCW Rest Hours Compliance (Tuân thủ giờ nghỉ ngơi)

### Đây là phần **BẮT BUỘC** theo STCW A-VIII/1

| Trường | Mô tả | Yêu cầu tối thiểu |
|--------|-------|-------------------|
| **Work Hours** | Số giờ làm việc ca này | Thường 4 giờ |
| **Rest Hours (Last 24h)** | Giờ nghỉ ngơi trong 24h qua | ≥ **10 giờ** |
| **Rest Hours (Last 7 Days)** | Giờ nghỉ ngơi trong 7 ngày qua | ≥ **77 giờ** |

### Trạng thái tuân thủ:
- ✅ **COMPLIANT** (Tuân thủ) - Đủ giờ nghỉ ngơi
- ❌ **NON-COMPLIANT** (Không tuân thủ) - Thiếu giờ nghỉ ngơi

### ⚠️ Nếu NON-COMPLIANT:
Bạn **BẮT BUỘC** phải điền lý do ngoại lệ vào trường **"Exception Reason"**

Ví dụ lý do hợp lệ:
- Emergency situation requiring all hands
- Vessel maneuvering in congested waters
- Search and rescue operations
- Essential safety operations

---

## 🖥️ Bridge Equipment Status (Trạng thái thiết bị buồng lái)

Tick vào checkbox nếu thiết bị **HOẠT ĐỘNG TỐT**:

| Thiết bị | Mô tả |
|----------|-------|
| ☑️ **Radar** | Radar hàng hải |
| ☑️ **ECDIS** | Hệ thống hải đồ điện tử |
| ☑️ **AIS** | Hệ thống nhận dạng tự động |
| ☑️ **Gyro** | La bàn con quay |
| ☐ **Autopilot** | Lái tự động (tick nếu đang sử dụng) |

### Nếu có thiết bị hỏng:
- Bỏ tick checkbox của thiết bị đó
- Điền mô tả lỗi vào trường **"Equipment Defects Details"**

---

## 📡 GMDSS Watch (Trực canh GMDSS - SOLAS Chapter IV)

| Trường | Mô tả |
|--------|-------|
| ☑️ **GMDSS Distress Watch Maintained** | Duy trì trực canh cấp cứu GMDSS |
| **Navigation Warnings Received** | Các cảnh báo hàng hải đã nhận (NAVTEX, SafetyNET) |

---

## 😴 Fatigue Management (Quản lý mệt mỏi - MLC 2006)

| Trường | Mô tả |
|--------|-------|
| **Fatigue Risk Level** | Mức độ rủi ro mệt mỏi: 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH |
| ☑️ **Fatigue Assessment Completed** | Đã hoàn thành đánh giá mệt mỏi |

### Đánh giá mức độ mệt mỏi:
- **🟢 LOW** - Nghỉ ngơi đầy đủ, tỉnh táo
- **🟡 MEDIUM** - Hơi mệt, cần theo dõi
- **🔴 HIGH** - Mệt mỏi nghiêm trọng, cần được thay thế

---

## 🔄 Watch Handover (Bàn giao ca trực)

### Thông tin bàn giao:
| Trường | Mô tả |
|--------|-------|
| **Watch Start Time** | Thời gian bắt đầu ca |
| **Watch End Time** | Thời gian kết thúc ca |
| **Handover Notes** | Ghi chú bàn giao cho sĩ quan nhận ca |
| ☑️ **Handover Checklist Completed** | Đã hoàn thành checklist bàn giao |

### Nội dung bàn giao quan trọng:
- Tình hình giao thông hàng hải
- Thay đổi thời tiết
- Thay đổi hướng đi
- Các tình huống đang diễn ra
- Cảnh báo hàng hải đã nhận
- Trạng thái thiết bị

---

## 📝 Additional Information (Thông tin bổ sung)

### Engine Status (chỉ cho ca ENGINE):
Mô tả trạng thái máy chính, ví dụ:
- "Main Engine running at 85% MCR, All systems normal"
- "ME RPM: 90, Load: 75%, Fuel consumption: 45 MT/day"

### Notable Events (Sự kiện đáng chú ý):
Ghi nhận các sự kiện quan trọng:
- Thay đổi hướng đi
- Tàu bè gặp phải
- Thay đổi thời tiết đột ngột
- Báo động
- Diễn tập
- Liên lạc VTS
- Pilot lên/xuống tàu

---

## ✅ Lưu và Ký Ca Trực

### Lưu bản nháp:
1. Điền đầy đủ thông tin
2. Click **"Save Watch Entry"**
3. Entry sẽ được lưu với trạng thái **DRAFT**

### Ký xác nhận (Master Signature):
1. Tìm entry cần ký trong bảng
2. Click nút **"SIGN"**
3. Thuyền trưởng ký xác nhận
4. Trạng thái chuyển thành **SIGNED**

---

## 📊 Bảng danh sách Ca Trực

### Các cột hiển thị:
| Cột | Mô tả |
|-----|-------|
| **Date** | Ngày trực |
| **Watch** | Ca trực (00-04, 04-08, ...) |
| **Type** | Loại: NAVIGATION hoặc ENGINE |
| **OOW** | Officer on Watch (Sĩ quan trực) |
| **Position** | Vị trí tàu |
| **C/S** | Course/Speed (Hướng/Tốc độ) |
| **Rest Hours** | Trạng thái tuân thủ giờ nghỉ |
| **Equipment** | Trạng thái thiết bị |
| **Status** | DRAFT hoặc SIGNED |

### Chỉ báo màu:
- 🟢 **Rest Hours OK** - Tuân thủ giờ nghỉ ngơi
- 🔴 **EXCEPTION** - Không tuân thủ (có lý do ngoại lệ)
- 🟢 **Equipment All OK** - Tất cả thiết bị hoạt động
- 🔴 **Defects** - Có thiết bị hỏng
- 🟡 **DRAFT** - Bản nháp chưa ký
- 🟢 **SIGNED** - Đã được Thuyền trưởng ký

---

## 🔍 Quy trình làm việc đề xuất

### Trước khi bắt đầu ca:
1. Nhận bàn giao từ sĩ quan ca trước
2. Kiểm tra handover notes
3. Kiểm tra trạng thái thiết bị
4. Xác nhận vị trí tàu

### Trong suốt ca trực:
1. Ghi nhận các sự kiện đáng chú ý
2. Cập nhật vị trí định kỳ
3. Theo dõi thời tiết
4. Duy trì GMDSS watch

### Kết thúc ca:
1. Hoàn thành form Watchkeeping
2. Điền handover notes chi tiết
3. Tick "Handover Checklist Completed"
4. Click "Save Watch Entry"
5. Bàn giao cho sĩ quan ca sau

### Hàng ngày:
- Thuyền trưởng review và ký xác nhận các entries

---

## ⚠️ Lưu ý quan trọng

### Port State Control (PSC):
Watchkeeping Log là tài liệu **BẮT BUỘC kiểm tra** khi PSC inspection. Đảm bảo:
- ✅ Tất cả ca trực đều được ghi nhận
- ✅ Rest hours được theo dõi chính xác
- ✅ Exceptions có lý do hợp lệ
- ✅ Entries được Thuyền trưởng ký

### STCW Compliance:
- Minimum 10 hours rest in any 24-hour period
- Minimum 77 hours rest in any 7-day period
- Rest periods may be divided into no more than two periods
- One of which must be at least 6 hours

### Lưu trữ:
- Records phải được giữ **tối thiểu 3 năm**
- Tự động đồng bộ lên Shore khi có kết nối

---

## 🆘 Hỗ trợ

Nếu gặp vấn đề khi sử dụng Watchkeeping Log:
1. Kiểm tra kết nối mạng
2. Refresh trang và thử lại
3. Liên hệ IT Support

---

**Phiên bản:** 2.0 (STCW/MLC 2006 Enhanced)  
**Cập nhật:** December 2025
