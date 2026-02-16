# BÁO CÁO PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG

## HỆ THỐNG QUẢN LÝ TÀU BIỂN - MARITIME MANAGEMENT SYSTEM

**Báo cáo kết thúc Giai đoạn 1**

---

**Ngày báo cáo:** 04 tháng 02 năm 2026  
**Phiên bản:** 1.0  
**Trạng thái:** Hoàn thành Giai đoạn 1

---

# PHẦN I: PHÂN TÍCH THIẾT KẾ TỔNG THỂ

Phần này trình bày tổng quan về hệ thống từ góc nhìn nghiệp vụ, bao gồm các tác nhân sử dụng hệ thống, cấu trúc chức năng, và quy trình vận hành chính. Nội dung được thiết kế để nhà quản lý và khách hàng có thể nắm bắt được phạm vi và cách thức hoạt động của phần mềm.

---

## CHƯƠNG 1: CÁC TÁC NHÂN VÀ QUYỀN HẠN (USE CASE)

### 1.1. Tổng quan các tác nhân

Hệ thống Quản lý Tàu biển được thiết kế phục vụ hai môi trường hoạt động riêng biệt: trên tàu (Edge System) và trên bờ (Shore System). Mỗi môi trường có các nhóm người dùng với vai trò và quyền hạn khác nhau, được xác định dựa trên quy định của Bộ luật ISM (International Safety Management) và thực tiễn vận hành hàng hải.

### 1.2. Sơ đồ Use Case tổng quát

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         HỆ THỐNG QUẢN LÝ TÀU BIỂN                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────┐                                    ┌─────────────┐        │
│   │    ADMIN    │                                    │  THUYỀN     │        │
│   │  VĂN PHÒNG  │                                    │  TRƯỞNG     │        │
│   └──────┬──────┘                                    └──────┬──────┘        │
│          │                                                  │               │
│          ▼                                                  ▼               │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐ ┌──────────────┐ │
│   │ Quản lý toàn │    │ Theo dõi     │    │ Phê duyệt    │ │ Ký duyệt    │ │
│   │ bộ đội tàu   │    │ vị trí tàu   │    │ báo cáo      │ │ nhật ký     │ │
│   └──────────────┘    └──────────────┘    └──────────────┘ └──────────────┘ │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐ ┌──────────────┐ │
│   │ Quản lý      │    │ Phân tích    │    │ Quản lý      │ │ Phê duyệt   │ │
│   │ thuyền viên  │    │ nhiên liệu   │    │ thuyền viên  │ │ công việc   │ │
│   └──────────────┘    └──────────────┘    │ trên tàu     │ │ bảo trì     │ │
│                                           └──────────────┘ └──────────────┘ │
│                                                                             │
│   ┌─────────────┐                                    ┌─────────────┐        │
│   │   MÁY       │                                    │  THUYỀN     │        │
│   │   TRƯỞNG    │                                    │   VIÊN      │        │
│   └──────┬──────┘                                    └──────┬──────┘        │
│          │                                                  │               │
│          ▼                                                  ▼               │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐ ┌──────────────┐ │
│   │ Quản lý PMS  │    │ Ký duyệt     │    │ Thực hiện    │ │ Tra cứu     │ │
│   │ phòng máy    │    │ nhật ký máy  │    │ công việc    │ │ bằng cấp    │ │
│   └──────────────┘    └──────────────┘    │ bảo trì      │ │ cá nhân     │ │
│   ┌──────────────┐    ┌──────────────┐    └──────────────┘ └──────────────┘ │
│   │ Phê duyệt    │    │ Giám sát     │    ┌──────────────┐ ┌──────────────┐ │
│   │ hoãn task    │    │ tiêu thụ     │    │ Ghi nhật ký  │ │ Xem lịch    │ │
│   │ máy         │    │ nhiên liệu   │    │ trực ca      │ │ bảo trì     │ │
│   └──────────────┘    └──────────────┘    └──────────────┘ └──────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.3. Mô tả chi tiết quyền hạn từng tác nhân

**Admin Văn phòng (Shore Office Administrator)** là người dùng có quyền cao nhất trong hệ thống Shore. Người này có khả năng xem và quản lý toàn bộ đội tàu thuộc công ty, bao gồm theo dõi vị trí real-time của tất cả tàu, xem báo cáo tổng hợp từ nhiều tàu, quản lý hồ sơ thuyền viên của toàn đội tàu, và nhận cảnh báo khi chứng chỉ của bất kỳ thuyền viên nào sắp hết hạn. Admin cũng có quyền phê duyệt các báo cáo quan trọng được gửi từ tàu lên và thực hiện điều động thuyền viên giữa các tàu.

**Thuyền trưởng (Master/Captain)** đảm nhiệm vai trò quản lý cao nhất trên tàu. Phạm vi quyền hạn của Thuyền trưởng được giới hạn trong phạm vi con tàu mình chỉ huy. Cụ thể, Thuyền trưởng chỉ xem được danh sách và hồ sơ thuyền viên đang làm việc trên tàu của mình, không thể truy cập thông tin thuyền viên trên các tàu khác. Thuyền trưởng có quyền ký duyệt các bút toán trong Nhật ký Boong, phê duyệt hoặc từ chối các yêu cầu hoãn công việc bảo trì từ bộ phận Boong, xác nhận và ký các báo cáo hàng hải (Noon Report, Departure/Arrival Report) trước khi truyền về bờ.

**Máy trưởng (Chief Engineer)** có vai trò tương đương Thuyền trưởng nhưng chịu trách nhiệm về bộ phận máy. Máy trưởng quản lý toàn bộ hệ thống bảo trì (PMS) của các thiết bị thuộc phòng máy, ký duyệt Nhật ký Máy, phê duyệt công việc bảo trì và yêu cầu hoãn task từ các Sỹ quan máy, đồng thời giám sát tiêu thụ nhiên liệu và vận hành máy chính cùng máy phát điện.

**Thuyền viên (Crew Member)** là người dùng cuối thực hiện các công việc hàng ngày. Thuyền viên chỉ có quyền xem thông tin cá nhân của chính mình, bao gồm hồ sơ cá nhân và danh sách chứng chỉ đang nắm giữ. Thuyền viên được phân công thực hiện công việc bảo trì, ghi nhận kết quả thực hiện qua ứng dụng mobile, chụp ảnh minh chứng, và gửi lên để sỹ quan phê duyệt. Thuyền viên cũng ghi nhật ký trực ca theo phiên được phân công.

### 1.4. Ma trận phân quyền

| Chức năng | Admin | Thuyền trưởng | Máy trưởng | Sỹ quan | Thuyền viên |
|-----------|:-----:|:-------------:|:----------:|:-------:|:-----------:|
| Xem tất cả tàu | ✓ | | | | |
| Xem thuyền viên toàn đội | ✓ | | | | |
| Xem thuyền viên trên tàu | ✓ | ✓ | ✓ | ✓ | |
| Xem hồ sơ cá nhân | ✓ | ✓ | ✓ | ✓ | ✓ |
| Điều động thuyền viên | ✓ | | | | |
| Ký Nhật ký Boong | | ✓ | | | |
| Ký Nhật ký Máy | | | ✓ | | |
| Phê duyệt task Boong | | ✓ | | | |
| Phê duyệt task Máy | | | ✓ | | |
| Thực hiện task | | | | ✓ | ✓ |
| Ghi nhật ký trực ca | | | | ✓ | ✓ |
| Ký báo cáo hàng hải | | ✓ | | | |

---

## CHƯƠNG 2: CẤU TRÚC CHỨC NĂNG HỆ THỐNG (FUNCTIONAL TREE)

### 2.1. Sơ đồ cây chức năng

Hệ thống được tổ chức thành các module chức năng theo cấu trúc phân cấp như sau:

```
HỆ THỐNG QUẢN LÝ TÀU BIỂN
│
├── 1. QUẢN LÝ THUYỀN VIÊN (Crew Management)
│   ├── 1.1. Hồ sơ thuyền viên
│   │   ├── Thông tin cá nhân (họ tên, ngày sinh, quốc tịch)
│   │   ├── Thông tin hộ chiếu và thị thực
│   │   ├── Thông tin liên hệ khẩn cấp
│   │   └── Lịch sử đi tàu
│   ├── 1.2. Quản lý chứng chỉ
│   │   ├── Danh mục loại chứng chỉ (STCW, Y tế, An toàn)
│   │   ├── Chứng chỉ của từng thuyền viên
│   │   ├── Cảnh báo hết hạn
│   │   └── Lưu trữ file scan chứng chỉ
│   ├── 1.3. Hợp đồng lao động
│   │   ├── Thông tin hợp đồng
│   │   ├── Ngày lên tàu / xuống tàu
│   │   └── Cảnh báo hết hạn hợp đồng
│   └── 1.4. Trực ca (Watchkeeping)
│       ├── Lịch trực ca
│       ├── Ghi nhận giờ làm việc
│       └── Kiểm tra tuân thủ giờ nghỉ (STCW)
│
├── 2. BẢO TRÌ THEO KẾ HOẠCH (Planned Maintenance System - PMS)
│   ├── 2.1. Quản lý thiết bị
│   │   ├── Danh mục thiết bị (tài sản)
│   │   ├── Nhóm thiết bị
│   │   ├── Thông số kỹ thuật
│   │   └── Giờ vận hành (Running Hours)
│   ├── 2.2. Lịch bảo trì
│   │   ├── Cấu hình chu kỳ (theo ngày hoặc giờ vận hành)
│   │   ├── Checklist mẫu
│   │   └── Vật tư cần thiết
│   ├── 2.3. Công việc bảo trì (Task)
│   │   ├── Tự động tạo task
│   │   ├── Phân công thực hiện
│   │   ├── Ghi nhận kết quả
│   │   └── Phê duyệt / Từ chối
│   └── 2.4. Hoãn công việc (Deferral)
│       ├── Tạo yêu cầu hoãn
│       ├── Phê duyệt yêu cầu
│       └── Lưu lịch sử hoãn
│
├── 3. NHẬT KÝ HÀNG HẢI (Maritime Logbooks)
│   ├── 3.1. Nhật ký Boong (Deck Log Book)
│   ├── 3.2. Nhật ký Máy (Engine Log Book)
│   ├── 3.3. Sổ Ghi Dầu (Oil Record Book)
│   ├── 3.4. Sổ Ghi Rác (Garbage Record Book)
│   └── 3.5. Sổ Ghi Nước Ballast (Ballast Water Record Book)
│
├── 4. QUẢN LÝ VẬT TƯ (Material Management)
│   ├── 4.1. Danh mục vật tư
│   ├── 4.2. Tồn kho
│   ├── 4.3. Nhập kho
│   └── 4.4. Cảnh báo tồn kho thấp
│
├── 5. THU THẬP DỮ LIỆU (Telemetry)
│   ├── 5.1. Dữ liệu GPS/AIS
│   ├── 5.2. Dữ liệu động cơ
│   ├── 5.3. Dữ liệu máy phát
│   ├── 5.4. Mức nhiên liệu
│   └── 5.5. Dữ liệu môi trường
│
├── 6. PHÂN TÍCH NHIÊN LIỆU (Fuel Analytics)
│   ├── 6.1. Tiêu thụ nhiên liệu
│   ├── 6.2. Chỉ số CII
│   ├── 6.3. Phát thải CO2
│   └── 6.4. Xu hướng và báo cáo
│
├── 7. BÁO CÁO HÀNG HẢI (Maritime Reporting)
│   ├── 7.1. Noon Report
│   ├── 7.2. Departure Report
│   ├── 7.3. Arrival Report
│   ├── 7.4. Bunker Report
│   └── 7.5. Báo cáo tổng hợp tuần/tháng
│
└── 8. QUẢN TRỊ HỆ THỐNG
    ├── 8.1. Quản lý tài khoản
    ├── 8.2. Phân quyền
    └── 8.3. Đồng bộ dữ liệu (Sync)
```

### 2.2. Mô tả các module đã triển khai trong Giai đoạn 1

Trong giai đoạn đầu tiên, nhóm phát triển đã hoàn thành việc xây dựng nền tảng cho toàn bộ hệ thống và triển khai đầy đủ các module sau:

**Module Quản lý Thuyền viên** đã hoàn thành toàn bộ chức năng quản lý hồ sơ thuyền viên và hệ thống chứng chỉ đa cấp. Hệ thống cho phép lưu trữ thông tin cá nhân đầy đủ theo yêu cầu STCW và MLC 2006, quản lý nhiều loại chứng chỉ khác nhau cho mỗi thuyền viên với cơ chế cảnh báo hết hạn tự động.

**Module Bảo trì Theo Kế hoạch (PMS)** là module phức tạp nhất và đã được triển khai hoàn chỉnh. Hệ thống cho phép quản lý danh mục thiết bị, cấu hình lịch bảo trì với ba loại chu kỳ (theo ngày, theo giờ vận hành, hoặc kết hợp), tự động tạo công việc bảo trì trước ngày đến hạn, quy trình phê duyệt đa cấp, và cơ chế hoãn công việc với kiểm soát chặt chẽ.

**Module Thu thập Dữ liệu Telemetry** đã hoàn thành với khả năng thu thập dữ liệu từ các nguồn GPS, động cơ, máy phát, bồn nhiên liệu và cảm biến môi trường. Dữ liệu được cập nhật mỗi 5 giây và hiển thị real-time trên dashboard.

**Module Nhật ký Hàng hải** đã triển khai đầy đủ 5 loại nhật ký bắt buộc theo quy định SOLAS và MARPOL với nguyên tắc bất biến (immutability) - bút toán sau khi ký không được phép sửa đổi.

---

## CHƯƠNG 3: QUY TRÌNH NGHIỆP VỤ (WORKFLOW)

### 3.1. Quy trình Thực hiện Công việc Bảo trì

Quy trình bảo trì thiết bị là quy trình nghiệp vụ cốt lõi của hệ thống PMS, đảm bảo tất cả thiết bị trên tàu được bảo dưỡng đúng lịch theo yêu cầu của Bộ luật ISM. Quy trình này đã được triển khai hoàn chỉnh trong Giai đoạn 1.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    QUY TRÌNH THỰC HIỆN CÔNG VIỆC BẢO TRÌ                    │
└─────────────────────────────────────────────────────────────────────────────┘

    ┌──────────────┐
    │ Lịch bảo trì │
    │ (Schedule)   │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐     7 ngày trước     ┌──────────────┐
    │ Hệ thống tự  │────────────────────▶│ Task được    │
    │ động kiểm tra│     ngày đến hạn    │ tạo tự động  │
    └──────────────┘                      └──────┬───────┘
                                                 │
                                                 ▼
                                          ┌──────────────┐
                                          │ Trạng thái:  │
                                          │ SCHEDULED    │
                                          └──────┬───────┘
                                                 │
                                    ┌────────────┴────────────┐
                                    ▼                         ▼
                             Đến ngày hạn              Quá ngày hạn
                                    │                         │
                                    ▼                         ▼
                             ┌──────────────┐         ┌──────────────┐
                             │ Trạng thái:  │         │ Trạng thái:  │
                             │ DUE          │         │ OVERDUE      │
                             └──────┬───────┘         └──────┬───────┘
                                    │                         │
                                    └────────────┬────────────┘
                                                 │
                           ┌─────────────────────┼─────────────────────┐
                           │                     │                     │
                           ▼                     ▼                     ▼
                    ┌──────────────┐      ┌──────────────┐      ┌──────────────┐
                    │ Thuyền viên  │      │ Thuyền viên  │      │ Thuyền viên  │
                    │ nhấn "Bắt    │      │ yêu cầu      │      │ hủy task     │
                    │ đầu"         │      │ hoãn         │      │ (có lý do)   │
                    └──────┬───────┘      └──────┬───────┘      └──────┬───────┘
                           │                     │                     │
                           ▼                     ▼                     ▼
                    ┌──────────────┐      ┌──────────────┐      ┌──────────────┐
                    │ Trạng thái:  │      │ Chờ phê      │      │ Trạng thái:  │
                    │ IN_PROGRESS  │      │ duyệt hoãn   │      │ CANCELLED    │
                    └──────┬───────┘      └──────┬───────┘      └──────────────┘
                           │                     │
                           ▼                     ▼
                    ┌──────────────┐      ┌──────────────┐
                    │ Hoàn thành:  │      │ Sỹ quan phê  │
                    │ - Checklist  │      │ duyệt/từ chối│
                    │ - Ảnh chụp   │      └──────┬───────┘
                    │ - Ghi chú    │             │
                    └──────┬───────┘      ┌──────┴──────┐
                           │              ▼             ▼
                           ▼         Đồng ý        Từ chối
                    ┌──────────────┐      │             │
                    │ Nhấn "Gửi   │      ▼             ▼
                    │ phê duyệt"   │ Cập nhật     Giữ nguyên
                    └──────┬───────┘ ngày hạn    trạng thái
                           │
                           ▼
                    ┌──────────────┐
                    │ Trạng thái:  │
                    │ PENDING_     │
                    │ APPROVAL     │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │ Sỹ quan      │
                    │ xem xét      │
                    └──────┬───────┘
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
         Phê duyệt                   Từ chối
              │                         │
              ▼                         ▼
       ┌──────────────┐         ┌──────────────┐
       │ Trạng thái:  │         │ Trạng thái:  │
       │ COMPLETED    │         │ RECTIFY      │
       └──────┬───────┘         └──────┬───────┘
              │                         │
              ▼                         │
       ┌──────────────┐                 │
       │ Hệ thống:    │                 │
       │ - Trừ vật tư │                 │
       │ - Ghi lịch sử│                 │
       │ - Tính ngày  │                 │
       │   hạn mới    │◀────────────────┘
       └──────────────┘      Sửa và gửi lại
```

**Mô tả quy trình:** Hệ thống tự động kiểm tra tất cả lịch bảo trì mỗi 6 giờ. Khi phát hiện công việc sắp đến hạn trong vòng 7 ngày tới, hệ thống tự động tạo một Task mới với trạng thái SCHEDULED. Khi đến ngày hạn, trạng thái chuyển thành DUE, và nếu quá hạn sẽ chuyển thành OVERDUE kèm cảnh báo.

Thuyền viên được phân công sẽ nhận thông báo qua ứng dụng mobile. Khi bắt đầu thực hiện, thuyền viên nhấn "Bắt đầu" và trạng thái chuyển thành IN_PROGRESS. Sau khi hoàn thành, thuyền viên điền checklist, chụp ảnh minh chứng (nếu yêu cầu), ghi chú kết quả và nhấn "Gửi phê duyệt".

Sỹ quan phụ trách (Máy trưởng đối với thiết bị máy, Thuyền trưởng hoặc Đại phó đối với thiết bị boong) sẽ xem xét kết quả. Nếu đạt yêu cầu, sỹ quan phê duyệt và trạng thái chuyển thành COMPLETED. Hệ thống sẽ tự động trừ vật tư đã sử dụng từ kho, ghi nhận lịch sử bảo trì, và tính toán ngày hạn mới cho chu kỳ tiếp theo. Nếu không đạt, sỹ quan từ chối với lý do và task chuyển về RECTIFY để thuyền viên sửa chữa và gửi lại.

### 3.2. Quy trình Quản lý Chứng chỉ Thuyền viên

Quy trình này đảm bảo tất cả thuyền viên luôn có chứng chỉ hợp lệ khi làm việc trên tàu.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    QUY TRÌNH QUẢN LÝ CHỨNG CHỈ THUYỀN VIÊN                  │
└─────────────────────────────────────────────────────────────────────────────┘

    ┌──────────────┐
    │ Thuyền viên  │
    │ mới hoặc     │
    │ cập nhật CC  │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │ Nhập thông   │
    │ tin chứng    │
    │ chỉ vào hệ   │
    │ thống        │
    └──────┬───────┘
           │
           ▼
    ┌──────────────────────────────────────────────────────────────┐
    │  Thông tin cần nhập:                                         │
    │  - Loại chứng chỉ (STCW, Y tế, An toàn...)                  │
    │  - Số chứng chỉ                                              │
    │  - Ngày cấp                                                  │
    │  - Ngày hết hạn                                              │
    │  - Cơ quan cấp                                               │
    │  - File scan chứng chỉ (PDF/Ảnh)                            │
    └──────────────────────────────────────────────────────────────┘
           │
           ▼
    ┌──────────────┐         ┌──────────────┐
    │ Hệ thống     │         │ Kiểm tra     │
    │ lưu trữ và   │────────▶│ hàng ngày    │
    │ theo dõi     │         │              │
    └──────────────┘         └──────┬───────┘
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
             Còn > 30 ngày   Còn 7-30 ngày    Còn < 7 ngày
                    │               │          hoặc hết hạn
                    ▼               ▼               │
             ┌──────────────┐ ┌──────────────┐     │
             │ Trạng thái:  │ │ Trạng thái:  │     ▼
             │ VALID        │ │ WARNING      │ ┌──────────────┐
             │ (Màu xanh)   │ │ (Màu vàng)   │ │ Trạng thái:  │
             └──────────────┘ └──────┬───────┘ │ CRITICAL/    │
                                     │         │ EXPIRED      │
                                     ▼         │ (Màu đỏ)     │
                              ┌──────────────┐ └──────┬───────┘
                              │ Gửi cảnh báo │        │
                              │ đến thuyền   │◀───────┘
                              │ viên và      │
                              │ văn phòng    │
                              └──────────────┘
```

---

# PHẦN II: PHÂN TÍCH THIẾT KẾ CHI TIẾT

Phần này trình bày chi tiết kỹ thuật những gì đã được thiết kế và triển khai trong Giai đoạn 1, bao gồm thiết kế cơ sở dữ liệu và logic xử lý của các chức năng đã hoàn thành.

---

## CHƯƠNG 4: THIẾT KẾ CƠ SỞ DỮ LIỆU

### 4.1. Tổng quan

Cơ sở dữ liệu được thiết kế trên nền tảng PostgreSQL 15, bao gồm 55 bảng được tổ chức thành các nhóm chức năng. Thiết kế tuân thủ chuẩn Third Normal Form (3NF) và áp dụng soft delete để đảm bảo tính toàn vẹn dữ liệu lịch sử theo yêu cầu audit của ngành hàng hải.

### 4.2. Sơ đồ quan hệ thực thể (ERD) - Các bảng chính

#### 4.2.1. Nhóm Quản lý Thuyền viên

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ERD - QUẢN LÝ THUYỀN VIÊN                           │
└─────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────┐       ┌───────────────────────┐
│    CREW_MEMBERS       │       │     CERTIFICATES      │
├───────────────────────┤       ├───────────────────────┤
│ PK  Id (GUID)         │       │ PK  Id (INT)          │
│     CrewId (VARCHAR)  │       │     CertificateCode   │
│     FullName          │       │     CertificateName   │
│     Position          │       │     Category          │
│     Rank              │       │     ValidityMonths    │
│     Department        │       │     IsMandatory       │
│     Nationality       │       │     IsActive          │
│     PassportNumber    │       └───────────┬───────────┘
│     PassportExpiry    │                   │
│     DateOfBirth       │                   │ 1
│     JoinDate          │                   │
│     ContractEnd       │                   │
│     IsOnboard         │                   │
│     IsSynced          │                   │
│     OriginNode        │                   │
└───────────┬───────────┘                   │
            │                               │
            │ 1                             │
            │                               │
            │         ┌─────────────────────┴───────────────────────┐
            │         │                                             │
            ▼         ▼                                             │
┌───────────────────────────────────────┐                          │
│         CREW_CERTIFICATES             │                          │
├───────────────────────────────────────┤                          │
│ PK  Id (INT)                          │                          │
│ FK  CrewMemberId ─────────────────────┼──────────────────────────┘
│ FK  CertificateId                     │
│     CertificateNumber (Số thực tế)    │
│     IssueDate                         │
│     ExpiryDate                        │
│     IssuingAuthority                  │
│     DocumentFilePath                  │
│     Status (VALID/EXPIRED/SUSPENDED)  │
│     Notes                             │
│     IsSynced                          │
│     OriginNode                        │
└───────────────────────────────────────┘

┌───────────────────────┐       ┌───────────────────────┐
│      COUNTRIES        │       │  COUNTRY_CERTIFICATES │
├───────────────────────┤       ├───────────────────────┤
│ PK  Id (INT)          │◀──────│ FK  CountryId         │
│     CountryCode (ISO) │   N   │ FK  CertificateId     │──────▶ CERTIFICATES
│     CountryName       │       │ PK  Id                │
│     IsActive          │       └───────────────────────┘
└───────────────────────┘
        (Quốc gia nào chấp nhận chứng chỉ nào)
```

**Mô tả thiết kế:** Bảng `crew_members` lưu thông tin cơ bản của thuyền viên. Bảng `certificates` là bảng danh mục (master data) chứa các loại chứng chỉ hàng hải được công nhận. Bảng `crew_certificates` là bảng trung gian lưu chứng chỉ thực tế của từng thuyền viên, bao gồm số chứng chỉ, ngày cấp, ngày hết hạn, và file scan. Thiết kế này cho phép một thuyền viên có nhiều chứng chỉ thuộc nhiều loại khác nhau.

Bảng `country_certificates` xác định chứng chỉ nào được quốc gia nào công nhận, phục vụ cho việc kiểm tra Flag State compliance khi thuyền viên làm việc trên tàu mang cờ quốc gia cụ thể.

#### 4.2.2. Nhóm Bảo trì Theo Kế hoạch (PMS)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           ERD - HỆ THỐNG PMS                                │
└─────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────┐           ┌───────────────────────┐
│   EQUIPMENT_GROUPS    │           │   EQUIPMENT_ASSETS    │
├───────────────────────┤           ├───────────────────────┤
│ PK  Id (GUID)         │◀──────────│ FK  EquipmentGroupId  │
│     GroupCode         │     N     │ PK  Id (GUID)         │
│     GroupName         │           │     AssetCode         │
│     Category          │           │     AssetName         │
│     Department        │           │     Category          │
│     PicRole           │           │     Manufacturer      │
│     PicCrewId         │           │     SerialNumber      │
└───────────┬───────────┘           │     CurrentRunningHrs │
            │                       │     Location          │
            │                       │     Criticality       │
            │                       │     Status            │
            │                       │     DefaultExecutorRole│
            │                       └───────────────────────┘
            │
            │ 1
            ▼
┌───────────────────────────────────┐
│     MAINTENANCE_SCHEDULES         │
├───────────────────────────────────┤
│ PK  Id (GUID)                     │
│ FK  EquipmentGroupId              │
│     ScheduleCode                  │
│     ScheduleName                  │
│     IntervalType (CALENDAR/       │
│                   RUNNING_HOURS/  │
│                   HYBRID)         │
│     IntervalDays                  │
│     IntervalHours                 │
│     DaysBeforeDue (mặc định: 7)   │
│     LastExecutedAt                │
│     NextDueDate                   │
│     NextDueRunningHours           │
│     AutoGenerate                  │
│     AssignedToRole                │
└───────────┬───────────────────────┘
            │
            │ 1
            │
     ┌──────┴──────┬──────────────────────────────────┐
     ▼             ▼                                  ▼
┌─────────────┐ ┌─────────────────────┐  ┌─────────────────────────┐
│ SCHEDULE_   │ │ SCHEDULE_CHECKLIST_ │  │    MAINTENANCE_TASKS    │
│ SPARE_PARTS │ │ TEMPLATES           │  ├─────────────────────────┤
├─────────────┤ ├─────────────────────┤  │ PK  Id (GUID)           │
│FK ScheduleId│ │ FK  ScheduleId      │  │ FK  ScheduleId          │
│FK MaterialId│ │     SequenceOrder   │  │     TaskId (unique)     │
│   Quantity  │ │     Checkpoint      │  │     Status              │
│   IsMandatory│ │     RequiresReading│  │     NextDueAt           │
└─────────────┘ │     NormalRangeMin  │  │     AssignedTo          │
                │     NormalRangeMax  │  │     StartedAt           │
                │     Unit            │  │     SubmittedAt         │
                └─────────────────────┘  │     VerifiedAt          │
                                         │     CompletedAt         │
                                         │     HasPendingDeferral  │
                                         │     DeferralCount       │
                                         └───────────┬─────────────┘
                                                     │
                              ┌──────────────────────┼──────────────────────┐
                              ▼                      ▼                      ▼
                    ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
                    │ TASK_CHECKLIST_ │   │ TASK_DEFERRAL_  │   │ TASK_STATUS_    │
                    │ ITEMS           │   │ REQUESTS        │   │ HISTORY         │
                    ├─────────────────┤   ├─────────────────┤   ├─────────────────┤
                    │ FK  TaskId      │   │ FK  TaskId      │   │ FK  TaskId      │
                    │ FK  AssetId     │   │     RequestedBy │   │     FromStatus  │
                    │     IsCompleted │   │     Reason      │   │     ToStatus    │
                    │     ReadingValue│   │     CurrentDue  │   │     ChangedBy   │
                    │     Remarks     │   │     ProposedDue │   │     ChangedAt   │
                    │     IsAbnormal  │   │     Status      │   │     Reason      │
                    └─────────────────┘   │     ReviewedBy  │   └─────────────────┘
                                          └─────────────────┘
```

**Mô tả thiết kế:** Cấu trúc PMS được thiết kế theo hướng linh hoạt, cho phép bảo trì theo nhóm thiết bị hoặc thiết bị đơn lẻ.

Bảng `equipment_assets` lưu danh mục tất cả thiết bị trên tàu cần bảo trì, mỗi thiết bị có mã tài sản duy nhất (AssetCode), thông số kỹ thuật, và giờ vận hành hiện tại (CurrentRunningHours) được cập nhật tự động từ telemetry.

Bảng `equipment_groups` cho phép gom nhiều thiết bị cùng loại vào một nhóm, ví dụ nhóm "Tất cả bình chữa cháy" hoặc "Hệ thống động cơ chính". Trường Department xác định bộ phận phụ trách (ENGINE hoặc DECK) để phân quyền phê duyệt.

Bảng `maintenance_schedules` định nghĩa lịch bảo trì với ba loại chu kỳ: CALENDAR (theo số ngày), RUNNING_HOURS (theo giờ vận hành), hoặc HYBRID (kết hợp cả hai, điều kiện nào đến trước sẽ được áp dụng). Trường DaysBeforeDue xác định hệ thống sẽ tự động tạo task trước ngày hạn bao nhiêu ngày.

Bảng `maintenance_tasks` lưu công việc bảo trì cụ thể được tạo từ schedule. Mỗi task có TaskId duy nhất theo format {ScheduleCode}-{YYYYMMDD}-{XXXX}, trạng thái theo state machine, và các trường theo dõi quy trình từ khi tạo đến khi hoàn thành.

### 4.3. Mô tả chi tiết các bảng chính

#### Bảng crew_members (Thuyền viên)

| Trường | Kiểu dữ liệu | Mô tả |
|--------|--------------|-------|
| Id | GUID | Khóa chính, tự động sinh |
| CrewId | VARCHAR(50) | Mã thuyền viên duy nhất |
| FullName | VARCHAR(200) | Họ và tên đầy đủ |
| Position | VARCHAR(100) | Chức danh (Captain, Chief Engineer...) |
| Rank | VARCHAR(50) | Cấp bậc (Officer, Rating) |
| Department | VARCHAR(100) | Bộ phận (Deck, Engine, Catering) |
| Nationality | VARCHAR(50) | Quốc tịch |
| PassportNumber | VARCHAR(50) | Số hộ chiếu |
| PassportExpiry | DATE | Ngày hết hạn hộ chiếu |
| DateOfBirth | DATE | Ngày sinh |
| JoinDate | DATE | Ngày vào công ty |
| EmbarkDate | DATE | Ngày lên tàu hiện tại |
| ContractEnd | DATE | Ngày kết thúc hợp đồng |
| IsOnboard | BOOLEAN | Đang trên tàu hay không |
| IsSynced | BOOLEAN | Đã đồng bộ lên Shore chưa |
| OriginNode | VARCHAR(50) | Nguồn dữ liệu (SHIP_01, SHORE) |

#### Bảng maintenance_tasks (Công việc bảo trì)

| Trường | Kiểu dữ liệu | Mô tả |
|--------|--------------|-------|
| Id | GUID | Khóa chính |
| TaskId | VARCHAR(50) | Mã task duy nhất (ME-OIL-20260204-0001) |
| ScheduleId | GUID | FK đến maintenance_schedules |
| EquipmentGroupId | GUID | FK đến equipment_groups |
| TaskDescription | TEXT | Mô tả công việc |
| NextDueAt | TIMESTAMP | Ngày hạn phải hoàn thành |
| Priority | VARCHAR(20) | Mức ưu tiên (CRITICAL/HIGH/NORMAL/LOW) |
| Status | VARCHAR(20) | Trạng thái hiện tại |
| AssignedTo | VARCHAR(100) | Mã thuyền viên được phân công |
| AssignedDepartment | VARCHAR(20) | Bộ phận (ENGINE/DECK) |
| StartedAt | TIMESTAMP | Thời điểm bắt đầu thực hiện |
| SubmittedAt | TIMESTAMP | Thời điểm gửi phê duyệt |
| VerifiedAt | TIMESTAMP | Thời điểm được phê duyệt |
| VerifiedBy | VARCHAR(50) | Người phê duyệt |
| CompletedAt | TIMESTAMP | Thời điểm hoàn thành |
| HasPendingDeferral | BOOLEAN | Có yêu cầu hoãn đang chờ |
| DeferralCount | INT | Số lần đã hoãn |
| ChecklistCompleted | BOOLEAN | Checklist đã hoàn thành |
| PhotosUploaded | INT | Số ảnh đã upload |
| SparePartsUsed | JSONB | Vật tư đã sử dụng |

---

## CHƯƠNG 5: LOGIC XỬ LÝ CÁC CHỨC NĂNG ĐÃ TRIỂN KHAI

### 5.1. Logic Cảnh báo Chứng chỉ Hết hạn

Hệ thống kiểm tra tự động tình trạng chứng chỉ của tất cả thuyền viên và đưa ra cảnh báo theo các mức độ khác nhau. Logic được triển khai như sau:

**Công thức tính số ngày còn lại:**
$$\text{DaysRemaining} = \text{ExpiryDate} - \text{CurrentDate}$$

**Quy tắc xác định trạng thái:**

Nếu $\text{DaysRemaining} < 0$ thì trạng thái là **EXPIRED** (Màu đỏ đậm) - Chứng chỉ đã hết hạn, thuyền viên không đủ điều kiện làm việc.

Nếu $0 \leq \text{DaysRemaining} < 7$ thì trạng thái là **CRITICAL** (Màu đỏ) - Cần gia hạn khẩn cấp trong vòng 7 ngày.

Nếu $7 \leq \text{DaysRemaining} < 30$ thì trạng thái là **WARNING** (Màu vàng) - Cần lên kế hoạch gia hạn.

Nếu $\text{DaysRemaining} \geq 30$ thì trạng thái là **VALID** (Màu xanh) - Chứng chỉ còn hiệu lực bình thường.

**Cách hệ thống hoạt động:** Mỗi khi truy vấn danh sách thuyền viên hoặc chi tiết thuyền viên, hệ thống tự động tính toán DaysRemaining cho tất cả chứng chỉ và đính kèm trạng thái tương ứng trong kết quả trả về. Dashboard hiển thị tổng hợp số lượng chứng chỉ theo từng trạng thái, và danh sách chi tiết các chứng chỉ sắp hết hạn được sắp xếp theo thứ tự ưu tiên (EXPIRED trước, sau đó đến CRITICAL và WARNING).

Khi phát hiện chứng chỉ ở trạng thái CRITICAL hoặc EXPIRED, hệ thống ghi vào Sync Queue với priority Critical để đồng bộ ngay lập tức về văn phòng trên bờ.

### 5.2. Logic Tự động Tạo Task Bảo trì

Background Service `MaintenanceSchedulerService` chạy mỗi 6 giờ và thực hiện việc kiểm tra tất cả lịch bảo trì để tạo task mới khi cần thiết.

**Đối với lịch bảo trì loại CALENDAR (theo ngày):**

$$\text{ShouldCreateTask} = (\text{NextDueDate} - \text{CurrentDate}) \leq \text{DaysBeforeDue}$$

Nếu điều kiện trên thỏa mãn và chưa có task pending cho schedule này, hệ thống sẽ tạo task mới.

**Đối với lịch bảo trì loại RUNNING_HOURS (theo giờ vận hành):**

$$\text{HoursRemaining} = \text{NextDueRunningHours} - \text{CurrentRunningHours}$$

$$\text{EstimatedDaysToReach} = \frac{\text{HoursRemaining}}{\text{AverageDailyRunningHours}}$$

$$\text{ShouldCreateTask} = \text{EstimatedDaysToReach} \leq \text{DaysBeforeDue}$$

Hệ thống lấy CurrentRunningHours từ bảng `equipment_assets` (được cập nhật tự động từ telemetry) và AverageDailyRunningHours được tính từ dữ liệu lịch sử.

**Đối với lịch bảo trì loại HYBRID:**

Task được tạo khi một trong hai điều kiện CALENDAR hoặc RUNNING_HOURS được thỏa mãn, tùy theo điều kiện nào đến trước.

**Quy trình tạo task:** Khi điều kiện được thỏa mãn, hệ thống tạo bản ghi mới trong bảng `maintenance_tasks` với TaskId theo format chuẩn, sao chép toàn bộ checklist từ `schedule_checklist_templates` thành `task_checklist_items`, và gán AssignedTo dựa trên AssignedToRole của schedule hoặc PicRole của equipment group.

### 5.3. Logic Phân quyền theo Vai trò

Hệ thống áp dụng phân quyền theo vai trò (Role-based Access Control) với logic kiểm tra ở tầng API.

**Quy tắc phân quyền xem thuyền viên:**

Admin văn phòng có RoleCode = 'ADMIN' được phép truy vấn tất cả thuyền viên trong hệ thống không giới hạn tàu.

Thuyền trưởng, Máy trưởng và các Sỹ quan trên tàu chỉ được truy vấn thuyền viên có `IsOnboard = true` và thuộc cùng tàu (xác định qua OriginNode của user đang đăng nhập).

Thuyền viên thường chỉ được xem thông tin của chính mình, xác định qua CrewId khớp với Username của tài khoản đăng nhập.

**Quy tắc phân quyền phê duyệt task:**

Task thuộc AssignedDepartment = 'ENGINE' chỉ có thể được phê duyệt bởi user có RoleCode = 'CHIEF_ENGINEER' hoặc 'ADMIN'.

Task thuộc AssignedDepartment = 'DECK' chỉ có thể được phê duyệt bởi user có RoleCode = 'CAPTAIN', 'CHIEF_OFFICER' hoặc 'ADMIN'.

**Quy tắc ký nhật ký:**

Deck Log Book chỉ cho phép user có RoleCode = 'CAPTAIN' thực hiện ký (ghi MasterSignature và SignedAt).

Engine Log Book chỉ cho phép user có RoleCode = 'CHIEF_ENGINEER' thực hiện ký.

### 5.4. Logic Trừ Vật tư Tự động

Khi một task bảo trì được phê duyệt hoàn thành (chuyển từ PENDING_APPROVAL sang COMPLETED), hệ thống tự động thực hiện trừ vật tư theo quy trình sau:

Bước 1: Đọc trường SparePartsUsed của task (định dạng JSON array chứa danh sách vật tư và số lượng sử dụng).

Bước 2: Với mỗi item trong SparePartsUsed, truy vấn bản ghi tương ứng trong bảng `material_items`.

Bước 3: Kiểm tra $\text{OnHandQuantity} \geq \text{QuantityUsed}$. Nếu không đủ, ghi cảnh báo nhưng vẫn cho phép hoàn thành task (ghi nhận số âm để theo dõi).

Bước 4: Thực hiện cập nhật $\text{OnHandQuantity} = \text{OnHandQuantity} - \text{QuantityUsed}$.

Bước 5: Kiểm tra $\text{OnHandQuantity} \leq \text{MinStock}$. Nếu đúng, tạo cảnh báo Low Stock để thông báo cần đặt hàng bổ sung.

Bước 6: Ghi nhận vào bảng `maintenance_history` để lưu audit trail.

### 5.5. Logic Đồng bộ Dữ liệu (Delta Sync)

Cơ chế Delta Sync được thiết kế để tối ưu băng thông khi đồng bộ dữ liệu giữa tàu và bờ qua kết nối vệ tinh có chi phí cao.

**Nguyên tắc hoạt động:** Thay vì gửi toàn bộ bản ghi mỗi khi có thay đổi, hệ thống chỉ gửi các trường đã thay đổi cùng với loại thao tác.

Khi tạo bản ghi mới (CREATE), Payload chứa toàn bộ object được serialize thành JSON.

Khi cập nhật bản ghi (UPDATE), hệ thống so sánh giá trị cũ và mới, chỉ ghi các trường thay đổi vào Payload. Ví dụ, khi cập nhật Status của task từ "IN_PROGRESS" sang "PENDING_APPROVAL", Payload chỉ chứa `{"Status": "PENDING_APPROVAL", "SubmittedAt": "2026-02-04T10:30:00Z"}` thay vì toàn bộ object task.

**Ước tính tiết kiệm băng thông:** Với task bảo trì có khoảng 40 trường, kích thước full object khoảng 2KB. Khi chỉ gửi 2-3 trường thay đổi, kích thước giảm còn khoảng 100 bytes, tiết kiệm khoảng 95% cho mỗi lần cập nhật. Tổng hợp trên toàn hệ thống với nhiều loại dữ liệu, ước tính tiết kiệm trung bình 82% băng thông.

---

## CHƯƠNG 6: KẾT QUẢ GIAI ĐOẠN 1

### 6.1. Những gì đã hoàn thành

**Về mặt thiết kế:** Nhóm đã hoàn thành thiết kế cơ sở dữ liệu với 55 bảng, bao phủ đầy đủ các nghiệp vụ chính của hệ thống quản lý tàu biển. Kiến trúc Edge-Shore đã được định nghĩa rõ ràng với cơ chế đồng bộ Delta Sync. Quy trình nghiệp vụ cho các module chính đã được phân tích và document hóa.

**Về mặt triển khai code:** Backend API (ASP.NET Core 8.0) đã hoàn thành với hơn 125 endpoints phân bổ trên 31 controllers. Frontend Web (React 19) đã hoàn thành với dashboard real-time và các trang quản lý. Frontend Mobile (Flutter) đã hoàn thành các màn hình thực hiện task bảo trì. Cơ sở dữ liệu PostgreSQL đã được migration đầy đủ với seed data mẫu.

**Các module đã hoạt động:** Module Quản lý Thuyền viên với đầy đủ CRUD và hệ thống chứng chỉ. Module PMS với tự động tạo task, quy trình phê duyệt, và trừ vật tư. Module Telemetry với thu thập dữ liệu mỗi 5 giây. Module Nhật ký Hàng hải với 5 loại sổ bắt buộc. Module Phân tích Nhiên liệu với các chỉ số CII và EEOI. Module Báo cáo với Noon Report và báo cáo xuất/nhập cảng.

### 6.2. Hướng triển khai Giai đoạn 2

Giai đoạn tiếp theo sẽ tập trung vào: tích hợp thực tế với thiết bị cảm biến qua giao thức Signal K, triển khai đồng bộ dữ liệu thực tế giữa Edge và Shore qua kết nối VSAT, hoàn thiện ứng dụng mobile với các tính năng offline-first, và kiểm thử toàn diện trên môi trường thực tế.

---

**PHỤ LỤC**

**A. Danh sách trạng thái Task (State Machine)**

| Trạng thái | Mô tả | Chuyển tiếp được phép |
|------------|-------|----------------------|
| SCHEDULED | Task mới tạo, chưa đến hạn | → DUE, OVERDUE, CANCELLED |
| DUE | Đến ngày hạn | → IN_PROGRESS, OVERDUE, CANCELLED |
| OVERDUE | Quá ngày hạn | → IN_PROGRESS, CANCELLED |
| IN_PROGRESS | Đang thực hiện | → PENDING_APPROVAL |
| PENDING_APPROVAL | Chờ phê duyệt | → COMPLETED, RECTIFY |
| RECTIFY | Bị từ chối, cần sửa | → PENDING_APPROVAL |
| COMPLETED | Hoàn thành | (Kết thúc) |
| CANCELLED | Đã hủy | (Kết thúc) |

**B. Danh sách Operation Code - Oil Record Book (MARPOL Annex I)**

Mã 1-10: Ballast/cleaning water operations. Mã 11-19: Fuel/cargo oil transfer. Mã 20-29: Disposal of residues. Mã 30-39: Discharge overboard. Mã 40-44: Accidental discharge.

**C. Danh sách Garbage Category (MARPOL Annex V)**

A: Plastics. B: Food wastes. C: Domestic wastes. D: Cooking oil. E: Incinerator ashes. F: Operational wastes. G: Cargo residues (non-HME). H: Cargo residues (HME). I: Animal carcasses. J: Fishing gear. K: E-waste.
