# PHẦN 2: MÔ TẢ HỆ THỐNG ĐANG XÂY DỰNG

## 2.1. Phân tích thiết kế tổng thể

Hệ thống Quản lý Tàu biển (Maritime Management System - MMS) được định hình dựa trên sự phân cấp vận hành chặt chẽ giữa môi trường trên bờ (Shore System) và dưới tàu (Edge System), đảm bảo sự đồng bộ dữ liệu xuyên suốt theo các quy định của Bộ luật ISM. Tại trung tâm của thiết kế tổng quát là sơ đồ Use Case với sự tham gia của bốn nhóm tác nhân chính, trong đó quyền hạn được phân định rõ ràng dựa trên vai trò thực tế trong ngành hàng hải. Admin văn phòng giữ vai trò điều phối tổng thể đội tàu và phê duyệt các báo cáo chiến lược, trong khi Thuyền trưởng và Máy trưởng thực hiện quyền quản trị trực tiếp trên tàu, từ việc ký duyệt nhật ký đến giám sát kỹ thuật phòng máy.

### Sơ đồ Use Case tổng quát

```mermaid
graph TB
    subgraph Shore["🏢 SHORE SYSTEM"]
        Admin["👔 Admin Văn Phòng"]
    end
    
    subgraph Edge["🚢 EDGE SYSTEM - Trên Tàu"]
        Master["⚓ Thuyền Trưởng"]
        CE["🔧 Máy Trưởng"]
        Officer["👨‍✈️ Sỹ Quan"]
        Crew["👷 Thuyền Viên"]
    end

    subgraph Functions["📋 CHỨC NĂNG HỆ THỐNG"]
        F1["Quản lý đội tàu"]
        F2["Điều động thuyền viên"]
        F3["Phê duyệt báo cáo"]
        F4["Theo dõi vị trí tàu"]
        F5["Ký nhật ký Boong"]
        F6["Phê duyệt task Boong"]
        F7["Quản lý PMS Máy"]
        F8["Ký nhật ký Máy"]
        F9["Thực hiện bảo trì"]
        F10["Ghi nhật ký trực ca"]
        F11["Xem hồ sơ cá nhân"]
    end

    Admin --> F1
    Admin --> F2
    Admin --> F3
    Admin --> F4
    
    Master --> F5
    Master --> F6
    Master --> F11
    
    CE --> F7
    CE --> F8
    CE --> F11
    
    Officer --> F9
    Officer --> F10
    
    Crew --> F9
    Crew --> F10
    Crew --> F11
```

Về mặt cấu trúc chức năng, hệ thống được tổ chức theo mô hình cây phân cấp (Functional Tree) bao gồm sáu module nghiệp vụ chính. **Telemetry & Monitoring** đảm nhiệm việc thu thập dữ liệu thời gian thực từ các cảm biến GPS, động cơ, bồn nhiên liệu và môi trường. **PMS (Planned Maintenance System)** quản lý toàn bộ quy trình bảo trì thiết bị theo kế hoạch, từ lịch trình định kỳ đến theo dõi giờ vận hành. **Fuel Analytics** phân tích tiêu thụ nhiên liệu và tính toán chỉ số CII theo quy định IMO. **Crew Management** quản lý hồ sơ thuyền viên, chứng chỉ nghề nghiệp và giờ trực ca. **Voyage Management** theo dõi hành trình từ cảng đi đến cảng đến, bao gồm các điểm ghé và hoạt động xếp dỡ hàng. Cuối cùng, **Compliance & Reporting** đảm bảo tuân thủ pháp quy thông qua hệ thống nhật ký điện tử (Deck Log, Engine Log, Oil Record Book) và các báo cáo hàng hải bắt buộc.

Sự liên kết giữa các module tạo ra một hệ sinh thái dữ liệu khép kín: dữ liệu Running Hours từ Telemetry tự động kích hoạt tạo task trong PMS; thông tin tiêu thụ nhiên liệu được tổng hợp vào Fuel Analytics để tính CII; dữ liệu vị trí và thời tiết được điền tự động vào Logbooks và Noon Report. Mọi biến động về giờ vận hành thiết bị hay hạn định chứng chỉ đều được hệ thống tự động nhận diện và xử lý.

### Sơ đồ Nghiệp vụ Quản lý Hệ thống (Business Process Diagram)

```mermaid
graph TB
    CENTER["🚢 NGHIỆP VỤ<br/>QUẢN LÝ TÀU BIỂN<br/>Edge System"]
    
    BP1["Quy trình<br/>Giám sát Tàu"]
    BP2["Quy trình<br/>Bảo trì Thiết bị"]
    BP3["Quy trình<br/>Quản lý Thuyền viên"]
    BP4["Quy trình<br/>Quản lý Hành trình"]
    BP5["Quy trình<br/>Ghi Nhật ký"]
    BP6["Quy trình<br/>Báo cáo Tuân thủ"]
    BP7["Quy trình<br/>Quản lý Nhiên liệu"]
    BP8["Quy trình<br/>Đồng bộ Dữ liệu"]
    
    BP1 --- CENTER
    CENTER --- BP2
    
    BP3 --- CENTER
    CENTER --- BP4
    
    BP5 --- BP3
    BP5 --- BP4
    
    BP6 --- BP5
    BP7 --- BP6
    
    BP8 --- BP6
    BP8 --- BP7
    
    style CENTER fill:#1565c0,color:#fff,stroke:#0d47a1,stroke-width:3px
    style BP1 fill:#e3f2fd,stroke:#1976d2
    style BP2 fill:#e3f2fd,stroke:#1976d2
    style BP3 fill:#e3f2fd,stroke:#1976d2
    style BP4 fill:#e3f2fd,stroke:#1976d2
    style BP5 fill:#e3f2fd,stroke:#1976d2
    style BP6 fill:#e3f2fd,stroke:#1976d2
    style BP7 fill:#e3f2fd,stroke:#1976d2
    style BP8 fill:#e3f2fd,stroke:#1976d2
```

### Sơ đồ Phân rã Chức năng (Functional Decomposition Diagram)

```mermaid
graph TD
    ROOT["🚢 HỆ THỐNG QUẢN LÝ TÀU BIỂN<br/>Maritime Management System - Edge"]
    
    ROOT --> M1["1.0 Quản lý<br/>Hệ thống"]
    ROOT --> M2["2.0 Telemetry<br/>& Monitoring"]
    ROOT --> M3["3.0 PMS<br/>Bảo trì"]
    ROOT --> M4["4.0 Quản lý<br/>Thuyền viên"]
    ROOT --> M5["5.0 Quản lý<br/>Hành trình"]
    ROOT --> M6["6.0 Nhật ký<br/>& Báo cáo"]
    
    M1 --> M11["1.1 Quản lý<br/>người dùng"]
    M1 --> M12["1.2 Quản lý<br/>quyền"]
    M1 --> M13["1.3 Phân quyền"]
    M1 --> M14["1.4 Đăng nhập"]
    M1 --> M15["1.5 Đăng xuất"]
    M1 --> M16["1.6 Đổi mật khẩu"]
    
    M2 --> M21["2.1 Thu thập<br/>GPS/AIS"]
    M2 --> M22["2.2 Thu thập<br/>dữ liệu máy"]
    M2 --> M23["2.3 Giám sát<br/>máy phát"]
    M2 --> M24["2.4 Giám sát<br/>bồn nhiên liệu"]
    M2 --> M25["2.5 Thu thập<br/>môi trường"]
    M2 --> M26["2.6 Dashboard<br/>thời gian thực"]
    
    M3 --> M31["3.1 QL nhóm<br/>thiết bị"]
    M3 --> M32["3.2 QL tài sản<br/>thiết bị"]
    M3 --> M33["3.3 QL lịch<br/>bảo trì"]
    M3 --> M34["3.4 QL nhiệm vụ<br/>bảo trì"]
    M3 --> M35["3.5 QL checklist"]
    M3 --> M36["3.6 QL hoãn<br/>bảo trì"]
    M3 --> M37["3.7 QL vật tư<br/>phụ tùng"]
    
    M4 --> M41["4.1 QL hồ sơ<br/>thuyền viên"]
    M4 --> M42["4.2 QL chứng chỉ"]
    M4 --> M43["4.3 QL trực ca"]
    M4 --> M44["4.4 QL giờ nghỉ<br/>ngơi"]
    
    M5 --> M51["5.1 QL chuyến đi"]
    M5 --> M52["5.2 QL cảng ghé"]
    M5 --> M53["5.3 QL hàng hóa"]
    M5 --> M54["5.4 Ghi nhận<br/>Voyage Log"]
    
    M6 --> M61["6.1 Nhật ký<br/>Boong"]
    M6 --> M62["6.2 Nhật ký<br/>Máy"]
    M6 --> M63["6.3 Sổ dầu<br/>Oil Record"]
    M6 --> M64["6.4 Sổ rác<br/>Garbage Record"]
    M6 --> M65["6.5 Sổ nước<br/>dằn Ballast"]
    M6 --> M66["6.6 Noon Report"]
    M6 --> M67["6.7 Báo cáo<br/>Departure/Arrival"]
    
    style ROOT fill:#1565c0,color:#fff
    style M1 fill:#42a5f5,color:#fff
    style M2 fill:#42a5f5,color:#fff
    style M3 fill:#42a5f5,color:#fff
    style M4 fill:#42a5f5,color:#fff
    style M5 fill:#42a5f5,color:#fff
    style M6 fill:#42a5f5,color:#fff
```

### Sơ đồ Luồng Dữ liệu Mức Ngữ cảnh (Context DFD - Level 0)

```mermaid
flowchart LR
    Admin["👔 Quản trị viên"]
    Master["⚓ Thuyền trưởng"]
    CE["🔧 Máy trưởng"]
    Officer["👨‍✈️ Sỹ quan"]
    Crew["👷 Thuyền viên"]
    Sensor["📡 Hệ thống<br/>cảm biến"]
    Shore["🏢 Hệ thống<br/>Shore"]
    
    System(("🚢 HỆ THỐNG<br/>QUẢN LÝ TÀU BIỂN<br/>EDGE"))
    
    Admin -->|"1. TT người dùng, quyền"| System
    System -->|"2. Kết quả xử lý"| Admin
    
    Master -->|"3. Phê duyệt nhật ký, task"| System
    System -->|"4. Thông tin tàu, báo cáo"| Master
    
    CE -->|"5. Phê duyệt PMS, Engine Log"| System
    System -->|"6. TT thiết bị, bảo trì"| CE
    
    Officer -->|"7. Ghi nhật ký, thực hiện task"| System
    System -->|"8. Danh sách task, lịch trực"| Officer
    
    Crew -->|"9. Thực hiện bảo trì, ghi ca"| System
    System -->|"10. Task được giao, hồ sơ"| Crew
    
    Sensor -->|"11. Dữ liệu telemetry"| System
    
    System -->|"12. Dữ liệu đồng bộ"| Shore
    Shore -->|"13. Cập nhật từ bờ"| System
    
    style System fill:#1565c0,color:#fff
```

### Sơ đồ Luồng Dữ liệu Mức Đỉnh (Top-Level DFD - Level 1)

```mermaid
flowchart TB
    subgraph Actors["TÁC NHÂN"]
        Admin["👔 Quản trị viên"]
        Master["⚓ Thuyền trưởng"]
        CE["🔧 Máy trưởng"]
        Officer["👨‍✈️ Sỹ quan"]
        Crew["👷 Thuyền viên"]
        Sensor["📡 Cảm biến"]
    end
    
    subgraph Processes["QUY TRÌNH XỬ LÝ"]
        P1["1.0<br/>Quản lý<br/>Hệ thống"]
        P2["2.0<br/>Telemetry<br/>& Monitoring"]
        P3["3.0<br/>PMS<br/>Bảo trì"]
        P4["4.0<br/>Quản lý<br/>Thuyền viên"]
        P5["5.0<br/>Quản lý<br/>Hành trình"]
        P6["6.0<br/>Nhật ký<br/>& Báo cáo"]
    end
    
    subgraph DataStores["KHO DỮ LIỆU"]
        D1[("D1: Users<br/>Roles")]
        D2[("D2: Telemetry<br/>Data")]
        D3[("D3: Equipment<br/>Tasks")]
        D4[("D4: Crew<br/>Certificates")]
        D5[("D5: Voyages<br/>PortCalls")]
        D6[("D6: Logbooks<br/>Reports")]
        D7[("D7: SyncQueue")]
    end
    
    Admin -->|"1,2"| P1
    P1 -->|"3,4"| D1
    
    Sensor -->|"5,6,7"| P2
    P2 -->|"8"| D2
    P2 -->|"9: Running Hours"| P3
    
    Officer -->|"10,11"| P3
    Crew -->|"12"| P3
    CE -->|"13,14"| P3
    P3 -->|"15,16,17"| D3
    
    Officer -->|"18"| P4
    P4 -->|"19,20,21,22"| D4
    
    Master -->|"23"| P5
    Officer -->|"24"| P5
    P5 -->|"25,26"| D5
    
    Master -->|"27"| P6
    CE -->|"28"| P6
    Officer -->|"29,30"| P6
    P6 -->|"31,32,33"| D6
    
    P2 -->|"34: Auto-fill"| P6
    P5 -->|"35: Voyage info"| P6
    
    D2 & D3 & D4 & D5 & D6 -->|"36"| D7
    
    style P1 fill:#e3f2fd
    style P2 fill:#e8f5e9
    style P3 fill:#fff3e0
    style P4 fill:#fce4ec
    style P5 fill:#f3e5f5
    style P6 fill:#e0f7fa
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 1.0 Quản lý Hệ thống

```mermaid
flowchart TB
    Admin["👔 Quản trị viên"]
    Master["⚓ Thuyền trưởng"]
    CE["🔧 Máy trưởng"]
    Officer["👨‍✈️ Sỹ quan"]
    Crew["👷 Thuyền viên"]
    
    P11["1.1<br/>Quản lý<br/>người dùng"]
    P12["1.2<br/>Quản lý<br/>quyền"]
    P13["1.3<br/>Phân quyền"]
    P14["1.4<br/>Đăng nhập"]
    P15["1.5<br/>Đăng xuất"]
    P16["1.6<br/>Đổi mật khẩu"]
    
    D1[("D1: Users")]
    D2[("D2: Roles")]
    
    Admin -->|"1: TT người dùng"| P11
    P11 -->|"1: Lưu/Cập nhật"| D1
    
    Admin -->|"2: TT quyền"| P12
    P12 -->|"2: Lưu/Cập nhật"| D2
    
    Admin -->|"3: Gán quyền"| P13
    P13 -->|"3"| D1
    P13 -->|"3"| D2
    
    Master & CE & Officer & Crew -->|"4: TT đăng nhập"| P14
    P14 -->|"4: Kiểm tra"| D1
    D1 -->|"4: Kết quả"| P14
    
    Master & CE & Officer & Crew -->|"5: Yêu cầu"| P15
    
    Master & CE & Officer & Crew -->|"6: MK cũ/mới"| P16
    P16 -->|"6: Cập nhật"| D1
    D1 -->|"7: TT tài khoản"| P14
    
    style P11 fill:#e3f2fd
    style P12 fill:#e3f2fd
    style P13 fill:#e3f2fd
    style P14 fill:#e3f2fd
    style P15 fill:#e3f2fd
    style P16 fill:#e3f2fd
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 2.0 Telemetry & Monitoring

```mermaid
flowchart TB
    Sensor["📡 Hệ thống<br/>cảm biến"]
    Officer["👨‍✈️ Sỹ quan"]
    
    P21["2.1<br/>Thu thập<br/>GPS/AIS"]
    P22["2.2<br/>Thu thập<br/>dữ liệu máy"]
    P23["2.3<br/>Giám sát<br/>máy phát"]
    P24["2.4<br/>Giám sát<br/>bồn nhiên liệu"]
    P25["2.5<br/>Thu thập<br/>môi trường"]
    P26["2.6<br/>Dashboard<br/>Real-time"]
    
    D1[("D1: Position<br/>Data")]
    D2[("D2: Engine<br/>Data")]
    D3[("D3: Generator<br/>Data")]
    D4[("D4: Tank<br/>Levels")]
    D5[("D5: Environmental<br/>Data")]
    D6[("D6: Safety<br/>Alarms")]
    
    Sensor -->|"1: Tín hiệu GPS"| P21
    P21 -->|"1: Lưu"| D1
    
    Sensor -->|"2: Dữ liệu động cơ"| P22
    P22 -->|"2: Lưu"| D2
    P22 -->|"3: Running Hours"| D2
    
    Sensor -->|"4: Dữ liệu máy phát"| P23
    P23 -->|"4: Lưu"| D3
    
    Sensor -->|"5: Mức bồn"| P24
    P24 -->|"5: Lưu"| D4
    
    Sensor -->|"6: Dữ liệu môi trường"| P25
    P25 -->|"6: Lưu"| D5
    
    D1 & D2 & D3 & D4 & D5 -->|"7: Dữ liệu"| P26
    P26 -->|"8: Hiển thị"| Officer
    
    P22 & P23 & P24 -->|"9: Cảnh báo"| D6
    D6 -->|"10: Alert"| P26
    
    style P21 fill:#e8f5e9
    style P22 fill:#e8f5e9
    style P23 fill:#e8f5e9
    style P24 fill:#e8f5e9
    style P25 fill:#e8f5e9
    style P26 fill:#e8f5e9
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 3.0 PMS Bảo trì

```mermaid
flowchart TB
    CE["🔧 Máy trưởng"]
    Officer["👨‍✈️ Sỹ quan"]
    Crew["👷 Thuyền viên"]
    
    P31["3.1<br/>QL nhóm<br/>thiết bị"]
    P32["3.2<br/>QL tài sản<br/>thiết bị"]
    P33["3.3<br/>QL lịch<br/>bảo trì"]
    P34["3.4<br/>QL nhiệm vụ<br/>bảo trì"]
    P35["3.5<br/>QL checklist"]
    P36["3.6<br/>QL hoãn<br/>bảo trì"]
    P37["3.7<br/>QL vật tư"]
    
    D1[("D1: Equipment<br/>Groups")]
    D2[("D2: Equipment<br/>Assets")]
    D3[("D3: Maintenance<br/>Schedules")]
    D4[("D4: Maintenance<br/>Tasks")]
    D5[("D5: Checklist<br/>Items")]
    D6[("D6: Deferral<br/>Requests")]
    D7[("D7: Material<br/>Items")]
    D8[("D8: Task<br/>History")]
    
    CE -->|"1: TT nhóm TB"| P31
    P31 -->|"1: Lưu"| D1
    
    CE -->|"2: TT tài sản"| P32
    P32 -->|"2: Lưu"| D2
    D1 -->|"3: Nhóm"| P32
    
    CE -->|"4: TT lịch BT"| P33
    P33 -->|"4: Lưu"| D3
    D1 -->|"5: Nhóm"| P33
    
    P33 -->|"6: Tự động tạo task"| P34
    Crew -->|"7: Thực hiện task"| P34
    Officer -->|"8: Phê duyệt"| P34
    P34 -->|"9: Lưu"| D4
    P34 -->|"10: Lịch sử"| D8
    
    Crew -->|"11: Điền checklist"| P35
    P35 -->|"11: Lưu"| D5
    D4 -->|"12: Task"| P35
    
    Crew -->|"13: Yêu cầu hoãn"| P36
    CE -->|"14: Phê duyệt hoãn"| P36
    P36 -->|"15: Lưu"| D6
    D4 -->|"16: Task"| P36
    
    Officer -->|"17: QL vật tư"| P37
    P37 -->|"17: Lưu"| D7
    P34 -->|"18: Trừ kho"| D7
    
    style P31 fill:#fff3e0
    style P32 fill:#fff3e0
    style P33 fill:#fff3e0
    style P34 fill:#fff3e0
    style P35 fill:#fff3e0
    style P36 fill:#fff3e0
    style P37 fill:#fff3e0
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 4.0 Quản lý Thuyền viên

```mermaid
flowchart TB
    Officer["👨‍✈️ Sỹ quan"]
    Crew["👷 Thuyền viên"]
    
    P41["4.1<br/>QL hồ sơ<br/>thuyền viên"]
    P42["4.2<br/>QL chứng chỉ"]
    P43["4.3<br/>QL trực ca"]
    P44["4.4<br/>QL giờ nghỉ<br/>ngơi"]
    
    D1[("D1: Crew<br/>Members")]
    D2[("D2: Certificates")]
    D3[("D3: Crew<br/>Certificates")]
    D4[("D4: Watchkeeping<br/>Log")]
    D5[("D5: Countries")]
    D6[("D6: Country<br/>Certificates")]
    
    Officer -->|"1: TT thuyền viên"| P41
    P41 -->|"1: Lưu"| D1
    
    Officer -->|"2: TT chứng chỉ gốc"| P42
    P42 -->|"2: Lưu"| D2
    D5 -->|"3: Quốc gia"| P42
    P42 -->|"3: Công nhận"| D6
    
    Crew -->|"4: TT chứng chỉ cá nhân"| P42
    D1 -->|"5: Thuyền viên"| P42
    P42 -->|"6: Lưu"| D3
    
    Crew -->|"7: Ghi ca trực"| P43
    D1 -->|"8: Thuyền viên"| P43
    P43 -->|"9: Lưu"| D4
    
    P43 -->|"10: Dữ liệu ca"| P44
    P44 -->|"11: Tính giờ nghỉ"| D4
    P44 -->|"12: Cảnh báo"| Officer
    
    style P41 fill:#fce4ec
    style P42 fill:#fce4ec
    style P43 fill:#fce4ec
    style P44 fill:#fce4ec
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 5.0 Quản lý Hành trình

```mermaid
flowchart TB
    Master["⚓ Thuyền trưởng"]
    Officer["👨‍✈️ Sỹ quan"]
    
    P51["5.1<br/>QL chuyến đi"]
    P52["5.2<br/>QL cảng ghé"]
    P53["5.3<br/>QL hàng hóa"]
    P54["5.4<br/>Ghi nhận<br/>Voyage Log"]
    
    D1[("D1: Voyage<br/>Records")]
    D2[("D2: Port<br/>Calls")]
    D3[("D3: Cargo<br/>Operations")]
    D4[("D4: Voyage<br/>Log Entry")]
    D5[("D5: Position<br/>Data")]
    
    Master -->|"1: Tạo chuyến đi"| P51
    P51 -->|"1: Lưu"| D1
    
    Officer -->|"2: TT cảng ghé"| P52
    D1 -->|"3: Voyage"| P52
    P52 -->|"4: Lưu"| D2
    
    Officer -->|"5: TT hàng hóa"| P53
    D1 -->|"6: Voyage"| P53
    D2 -->|"7: Cảng"| P53
    P53 -->|"8: Lưu"| D3
    
    Officer -->|"9: Ghi nhận sự kiện"| P54
    D1 -->|"10: Voyage"| P54
    D5 -->|"11: Vị trí"| P54
    P54 -->|"12: Lưu"| D4
    
    Master -->|"13: Phê duyệt"| P51
    P51 -->|"14: Cập nhật trạng thái"| D1
    
    style P51 fill:#f3e5f5
    style P52 fill:#f3e5f5
    style P53 fill:#f3e5f5
    style P54 fill:#f3e5f5
```

### Sơ đồ Luồng Dữ liệu Mức Dưới Đỉnh - 6.0 Nhật ký & Báo cáo

```mermaid
flowchart TB
    Master["⚓ Thuyền trưởng"]
    CE["🔧 Máy trưởng"]
    Officer["👨‍✈️ Sỹ quan"]
    Shore["🏢 Shore"]
    
    P61["6.1<br/>Nhật ký<br/>Boong"]
    P62["6.2<br/>Nhật ký<br/>Máy"]
    P63["6.3<br/>Sổ dầu<br/>Oil Record"]
    P64["6.4<br/>Sổ rác<br/>Garbage"]
    P65["6.5<br/>Sổ nước dằn<br/>Ballast"]
    P66["6.6<br/>Noon Report"]
    P67["6.7<br/>Báo cáo<br/>Departure/Arrival"]
    
    D1[("D1: Deck<br/>LogBook")]
    D2[("D2: Engine<br/>LogBook")]
    D3[("D3: Oil<br/>Record Book")]
    D4[("D4: Garbage<br/>Record Book")]
    D5[("D5: Ballast Water<br/>Record Book")]
    D6[("D6: Noon<br/>Reports")]
    D7[("D7: Maritime<br/>Reports")]
    D8[("D8: Telemetry<br/>Data")]
    D9[("D9: Voyage<br/>Data")]
    
    Officer -->|"1: Ghi nhật ký boong"| P61
    Master -->|"2: Ký duyệt"| P61
    P61 -->|"3: Lưu"| D1
    D8 -->|"4: Vị trí, thời tiết"| P61
    
    Officer -->|"5: Ghi nhật ký máy"| P62
    CE -->|"6: Ký duyệt"| P62
    P62 -->|"7: Lưu"| D2
    D8 -->|"8: Thông số máy"| P62
    
    Officer -->|"9: Ghi sổ dầu"| P63
    P63 -->|"10: Lưu"| D3
    
    Officer -->|"11: Ghi sổ rác"| P64
    P64 -->|"12: Lưu"| D4
    
    Officer -->|"13: Ghi sổ nước dằn"| P65
    P65 -->|"14: Lưu"| D5
    
    Officer -->|"15: Tạo Noon Report"| P66
    Master -->|"16: Phê duyệt"| P66
    D8 -->|"17: Vị trí, nhiên liệu"| P66
    D9 -->|"18: TT chuyến đi"| P66
    P66 -->|"19: Lưu"| D6
    P66 -->|"20: Gửi"| Shore
    
    Officer -->|"21: Tạo báo cáo"| P67
    Master -->|"22: Phê duyệt"| P67
    P67 -->|"23: Lưu"| D7
    P67 -->|"24: Gửi"| Shore
    
    style P61 fill:#e0f7fa
    style P62 fill:#e0f7fa
    style P63 fill:#e0f7fa
    style P64 fill:#e0f7fa
    style P65 fill:#e0f7fa
    style P66 fill:#e0f7fa
    style P67 fill:#e0f7fa
```

Để hiện thực hóa các chức năng này, quy trình nghiệp vụ được thiết kế với tính tự động hóa cao. Điển hình là quy trình bảo trì thiết bị, nơi hệ thống tự động kiểm tra lịch trình định kỳ để khởi tạo các nhiệm vụ (Tasks) trước ngày đến hạn. Mọi thao tác từ lúc thuyền viên bắt đầu thực hiện đến khi sỹ quan ký duyệt đều tuân thủ một trạng thái máy (State Machine) nghiêm ngặt, đảm bảo tính minh bạch và khả năng truy xuất nguồn gốc dữ liệu lịch sử.

### Quy trình Thực hiện Bảo trì (PMS Workflow)

```mermaid
flowchart TD
    A["🕐 Hệ thống tự động<br/>quét lịch mỗi 6 giờ"] --> B{"📅 Kiểm tra<br/>ngày đến hạn"}
    
    B -->|"Còn ≤ 7 ngày"| C["📝 Tạo Task mới<br/>Status: SCHEDULED"]
    B -->|"Chưa đến hạn"| A
    
    C --> D{"⏰ Đến ngày<br/>hạn chưa?"}
    D -->|"Đúng ngày"| E["🔔 Status: DUE"]
    D -->|"Quá hạn"| F["⚠️ Status: OVERDUE"]
    D -->|"Chưa đến"| D
    
    E --> G["👷 Thuyền viên<br/>nhấn Bắt đầu"]
    F --> G
    
    G --> H["🔄 Status: IN_PROGRESS"]
    
    H --> I["✅ Hoàn thành:<br/>• Điền Checklist<br/>• Chụp ảnh<br/>• Ghi chú"]
    
    I --> J["📤 Gửi phê duyệt<br/>Status: PENDING_APPROVAL"]
    
    J --> K{"👨‍✈️ Sỹ quan<br/>xem xét"}
    
    K -->|"❌ Từ chối"| L["🔧 Status: RECTIFY<br/>Yêu cầu sửa"]
    L --> H
    
    K -->|"✅ Đồng ý"| M["✔️ Status: COMPLETED"]
    
    M --> N["🔄 Hệ thống tự động:<br/>• Trừ vật tư từ kho<br/>• Ghi lịch sử bảo trì<br/>• Tính ngày hạn mới"]
    
    N --> A
    
    style A fill:#e1f5fe
    style M fill:#c8e6c9
    style F fill:#ffcdd2
    style L fill:#fff3e0
```

### Quy trình Quản lý Chứng chỉ Thuyền viên

```mermaid
flowchart TD
    A["📄 Nhập thông tin<br/>chứng chỉ mới"] --> B["💾 Lưu vào hệ thống<br/>crew_certificates"]
    
    B --> C["🔄 Kiểm tra<br/>hàng ngày"]
    
    C --> D{"📊 Tính số ngày<br/>còn lại"}
    
    D -->|"> 30 ngày"| E["🟢 VALID<br/>Còn hiệu lực"]
    D -->|"7-30 ngày"| F["🟡 WARNING<br/>Cần lên kế hoạch"]
    D -->|"< 7 ngày"| G["🔴 CRITICAL<br/>Cần gia hạn gấp"]
    D -->|"≤ 0 ngày"| H["⛔ EXPIRED<br/>Đã hết hạn"]
    
    F --> I["📧 Gửi cảnh báo<br/>đến thuyền viên"]
    G --> I
    H --> I
    
    I --> J["📡 Đồng bộ về<br/>văn phòng Shore"]
    
    E --> C
    
    style E fill:#c8e6c9
    style F fill:#fff9c4
    style G fill:#ffcdd2
    style H fill:#d32f2f,color:#fff
```

### Quy trình Thu thập Dữ liệu Telemetry

```mermaid
flowchart TD
    A["📡 Thiết bị cảm biến<br/>GPS/Engine/Tank/Env"] --> B["🔄 Thu thập dữ liệu<br/>mỗi 5 giây"]
    
    B --> C["💾 Lưu vào bảng<br/>telemetry_data"]
    
    C --> D{"🔍 Kiểm tra<br/>ngưỡng cảnh báo"}
    
    D -->|"Bình thường"| E["📊 Hiển thị<br/>Dashboard Real-time"]
    D -->|"Vượt ngưỡng"| F["⚠️ Tạo Alert"]
    
    F --> G["📧 Thông báo<br/>Sỹ quan trực"]
    
    E --> H{"⏰ Đủ chu kỳ<br/>tổng hợp?"}
    
    H -->|"Mỗi giờ"| I["📈 Tổng hợp<br/>dữ liệu giờ"]
    H -->|"Mỗi ngày"| J["📊 Tổng hợp<br/>dữ liệu ngày"]
    
    I --> K["🔄 Cập nhật<br/>Running Hours<br/>thiết bị"]
    
    J --> L["📡 Đồng bộ về<br/>Shore System"]
    
    K --> B
    
    style F fill:#ffcdd2
    style E fill:#c8e6c9
    style L fill:#e3f2fd
```

### Quy trình Phân tích Nhiên liệu & Tính CII

```mermaid
flowchart TD
    A["⛽ Ghi nhận tiêu thụ<br/>nhiên liệu hàng ngày"] --> B["📊 Nhập dữ liệu:<br/>• ME Consumption<br/>• AE Consumption<br/>• Boiler Consumption"]
    
    B --> C["💾 Lưu vào<br/>fuel_consumption_records"]
    
    C --> D["🔄 Tổng hợp<br/>daily_fuel_summaries"]
    
    D --> E{"📅 Cuối năm?"}
    
    E -->|"Chưa"| F["📈 Cập nhật<br/>xu hướng tiêu thụ"]
    E -->|"Rồi"| G["🧮 Tính toán CII"]
    
    G --> H["📐 Công thức:<br/>CII = CO2 / (DWT × Distance)"]
    
    H --> I{"📊 So sánh với<br/>Required CII"}
    
    I -->|"CII ≤ Required"| J["🟢 Rating A/B/C<br/>Đạt yêu cầu"]
    I -->|"CII > Required"| K["🔴 Rating D/E<br/>Cần cải thiện"]
    
    J --> L["📋 Tạo báo cáo<br/>IMO DCS"]
    K --> L
    K --> M["⚠️ Lập kế hoạch<br/>giảm phát thải"]
    
    L --> N["📡 Gửi về Shore<br/>& Cơ quan quản lý"]
    
    style J fill:#c8e6c9
    style K fill:#ffcdd2
    style N fill:#e3f2fd
```

### Quy trình Quản lý Chuyến đi (Voyage)

```mermaid
flowchart TD
    A["📋 Tạo Voyage mới<br/>Nhập thông tin cảng đi/đến"] --> B["🗺️ Lập kế hoạch<br/>tuyến đường"]
    
    B --> C["📍 Định nghĩa<br/>Waypoints"]
    
    C --> D["⏱️ Tính toán<br/>ETD/ETA"]
    
    D --> E["🚢 Bắt đầu<br/>chuyến đi"]
    
    E --> F["📡 Theo dõi<br/>vị trí Real-time"]
    
    F --> G{"🏁 Đến cảng<br/>ghé?"}
    
    G -->|"Có"| H["📝 Ghi nhận<br/>Port Call"]
    G -->|"Không"| F
    
    H --> I{"📦 Có xếp dỡ<br/>hàng?"}
    
    I -->|"Có"| J["📋 Ghi nhận<br/>Cargo Operations"]
    I -->|"Không"| K["⏭️ Tiếp tục<br/>hành trình"]
    
    J --> K
    
    K --> L{"🏁 Đến cảng<br/>đích?"}
    
    L -->|"Chưa"| F
    L -->|"Rồi"| M["✅ Kết thúc<br/>Voyage"]
    
    M --> N["📊 Tổng hợp:<br/>• Quãng đường thực tế<br/>• Nhiên liệu tiêu thụ<br/>• Thời gian hành trình"]
    
    N --> O["📡 Đồng bộ<br/>về Shore"]
    
    style E fill:#e3f2fd
    style M fill:#c8e6c9
    style O fill:#fff3e0
```

### Quy trình Ghi Nhật ký Hàng hải (Logbook)

```mermaid
flowchart TD
    A["👨‍✈️ Sỹ quan trực<br/>bắt đầu ca"] --> B["📝 Tạo bản ghi<br/>Log Book mới"]
    
    B --> C{"📋 Loại<br/>nhật ký?"}
    
    C -->|"Deck Log"| D["📖 Ghi nhận:<br/>• Vị trí, hướng đi<br/>• Thời tiết, sóng<br/>• Sự kiện hàng hải"]
    
    C -->|"Engine Log"| E["📖 Ghi nhận:<br/>• Thông số máy chính<br/>• Máy phát điện<br/>• Tiêu thụ nhiên liệu"]
    
    C -->|"Oil Record"| F["📖 Ghi nhận:<br/>• Mã thao tác (1-44)<br/>• Bồn chứa, số lượng<br/>• Chữ ký sỹ quan"]
    
    C -->|"Garbage Record"| G["📖 Ghi nhận:<br/>• Loại rác (A-K)<br/>• Phương thức xử lý<br/>• Vị trí xả/đốt"]
    
    D --> H["💾 Lưu Entry<br/>vào hệ thống"]
    E --> H
    F --> H
    G --> H
    
    H --> I{"⏰ Hết ca<br/>trực?"}
    
    I -->|"Chưa"| J["➕ Thêm Entry<br/>mới"]
    J --> H
    
    I -->|"Rồi"| K["📤 Gửi cho<br/>cấp trên ký"]
    
    K --> L{"✍️ Ký duyệt?"}
    
    L -->|"Thuyền trưởng<br/>(Deck Log)"| M["⚓ Master<br/>Signature"]
    L -->|"Máy trưởng<br/>(Engine Log)"| N["🔧 Chief Engineer<br/>Signature"]
    
    M --> O["🔒 Khóa bản ghi<br/>IsLocked = true"]
    N --> O
    
    O --> P["📡 Đồng bộ<br/>về Shore"]
    
    style O fill:#c8e6c9
    style P fill:#e3f2fd
```

### Quy trình Tạo Noon Report

```mermaid
flowchart TD
    A["⏰ 12:00 UTC<br/>hàng ngày"] --> B["📊 Thu thập<br/>dữ liệu tự động"]
    
    B --> C["🔄 Lấy từ Telemetry:<br/>• Vị trí GPS<br/>• Tốc độ trung bình<br/>• Thời tiết"]
    
    C --> D["⛽ Lấy từ Fuel:<br/>• ROB HFO/MDO<br/>• Tiêu thụ 24h<br/>• Distance steamed"]
    
    D --> E["📝 Sỹ quan điền<br/>thông tin bổ sung"]
    
    E --> F["📋 Tạo<br/>Noon Report"]
    
    F --> G["📐 Tính toán:<br/>• Distance to go<br/>• ETA revised<br/>• Avg consumption"]
    
    G --> H["👨‍✈️ Thuyền trưởng<br/>xem xét"]
    
    H --> I{"✅ Phê duyệt?"}
    
    I -->|"Cần sửa"| E
    I -->|"Đồng ý"| J["✍️ Ký duyệt"]
    
    J --> K["📡 Gửi về<br/>Shore Office"]
    
    K --> L["📧 Thông báo<br/>cho Charterer/Owner"]
    
    style A fill:#e3f2fd
    style K fill:#c8e6c9
    style L fill:#fff3e0
```

---

## 2.2. Phân tích thiết kế chi tiết các phần đã triển khai

Trong Giai đoạn 1, công tác thiết kế chi tiết đã hoàn thiện nền tảng cơ sở dữ liệu PostgreSQL với hơn 55 bảng nghiệp vụ được chuẩn hóa theo mô hình 3NF. Kiến trúc dữ liệu tập trung vào việc quản lý thực thể thuyền viên và hệ thống chứng chỉ STCW, cho phép lưu trữ đa dạng các loại giấy tờ chuyên môn kèm theo file quét số hóa. Đối với phân hệ PMS, sơ đồ thực thể được xây dựng linh hoạt để hỗ trợ bảo trì theo cả chu kỳ thời gian và giờ vận hành thực tế được cập nhật từ hệ thống Telemetry.

### Sơ đồ ERD - Module Crew Management (Quản lý Thuyền viên)

```mermaid
erDiagram
    CREW_MEMBERS ||--o{ CREW_CERTIFICATES : "holds"
    CREW_MEMBERS ||--o{ WATCHKEEPING_LOG : "records"
    CERTIFICATES ||--o{ CREW_CERTIFICATES : "defines"
    CERTIFICATES ||--o{ COUNTRY_CERTIFICATES : "recognized_by"
    COUNTRIES ||--o{ COUNTRY_CERTIFICATES : "manages"

    CREW_MEMBERS {
        guid Id PK
        string CrewId UK
        string FullName
        string Position
        string Rank
        string Department
        string Nationality
        string PassportNumber
        date PassportExpiry
        date DateOfBirth
        date JoinDate
        date ContractEnd
        boolean IsOnboard
        boolean IsSynced
        string OriginNode
    }
    
    CERTIFICATES {
        int Id PK
        string CertificateCode UK
        string CertificateName
        string Category
        int ValidityMonths
        boolean IsMandatory
        boolean IsActive
    }
    
    CREW_CERTIFICATES {
        int Id PK
        guid CrewMemberId FK
        int CertificateId FK
        string CertificateNumber
        date IssueDate
        date ExpiryDate
        string IssuingAuthority
        string DocumentFilePath
        string Status
        string Notes
    }
    
    COUNTRIES {
        int Id PK
        string CountryCode UK
        string CountryName
        boolean IsActive
    }
    
    COUNTRY_CERTIFICATES {
        int Id PK
        int CountryId FK
        int CertificateId FK
    }
    
    WATCHKEEPING_LOG {
        int Id PK
        guid CrewMemberId FK
        date WatchDate
        time StartTime
        time EndTime
        decimal RestHours
        string WatchType
        string Location
        string Remarks
    }
```

### Sơ đồ ERD - Module PMS (Planned Maintenance System)

```mermaid
erDiagram
    EQUIPMENT_GROUPS ||--o{ EQUIPMENT_ASSETS : "contains"
    EQUIPMENT_GROUPS ||--o{ MAINTENANCE_SCHEDULES : "has"
    MAINTENANCE_SCHEDULES ||--o{ MAINTENANCE_TASKS : "generates"
    MAINTENANCE_SCHEDULES ||--o{ SCHEDULE_CHECKLIST_TEMPLATES : "defines"
    MAINTENANCE_SCHEDULES ||--o{ SCHEDULE_SPARE_PARTS : "requires"
    MAINTENANCE_TASKS ||--o{ TASK_CHECKLIST_ITEMS : "includes"
    MAINTENANCE_TASKS ||--o{ TASK_DEFERRAL_REQUESTS : "may_have"
    MAINTENANCE_TASKS ||--o{ TASK_STATUS_HISTORY : "tracks"
    MATERIAL_ITEMS ||--o{ SCHEDULE_SPARE_PARTS : "used_in"

    EQUIPMENT_GROUPS {
        guid Id PK
        string GroupCode UK
        string GroupName
        string Category
        string Department
        string PicRole
        guid PicCrewId FK
    }
    
    EQUIPMENT_ASSETS {
        guid Id PK
        guid EquipmentGroupId FK
        string AssetCode UK
        string AssetName
        string Manufacturer
        string SerialNumber
        decimal CurrentRunningHours
        string Location
        string Criticality
        string Status
    }
    
    MAINTENANCE_SCHEDULES {
        guid Id PK
        guid EquipmentGroupId FK
        string ScheduleCode UK
        string ScheduleName
        string IntervalType
        int IntervalDays
        int IntervalHours
        int DaysBeforeDue
        timestamp LastExecutedAt
        date NextDueDate
        decimal NextDueRunningHours
        boolean AutoGenerate
        string AssignedToRole
    }
    
    MAINTENANCE_TASKS {
        guid Id PK
        guid ScheduleId FK
        string TaskId UK
        string Status
        timestamp NextDueAt
        string Priority
        string AssignedTo
        string AssignedDepartment
        timestamp StartedAt
        timestamp SubmittedAt
        timestamp VerifiedAt
        string VerifiedBy
        timestamp CompletedAt
        boolean HasPendingDeferral
        int DeferralCount
    }
    
    TASK_CHECKLIST_ITEMS {
        int Id PK
        guid TaskId FK
        guid AssetId FK
        int SequenceOrder
        string Checkpoint
        boolean IsCompleted
        string ReadingValue
        boolean IsAbnormal
        string Remarks
    }
    
    TASK_DEFERRAL_REQUESTS {
        int Id PK
        guid TaskId FK
        string RequestedBy
        string Reason
        date CurrentDueDate
        date ProposedDueDate
        string Status
        string ReviewedBy
        timestamp ReviewedAt
    }
    
    SCHEDULE_CHECKLIST_TEMPLATES {
        int Id PK
        guid ScheduleId FK
        int SequenceOrder
        string Checkpoint
        boolean RequiresReading
        decimal NormalRangeMin
        decimal NormalRangeMax
        string Unit
    }
    
    SCHEDULE_SPARE_PARTS {
        int Id PK
        guid ScheduleId FK
        int MaterialId FK
        decimal Quantity
        boolean IsMandatory
    }
    
    MATERIAL_ITEMS {
        int Id PK
        string MaterialCode UK
        string MaterialName
        decimal OnHandQuantity
        decimal MinStock
        string Unit
    }
    
    TASK_STATUS_HISTORY {
        int Id PK
        guid TaskId FK
        string FromStatus
        string ToStatus
        string ChangedBy
        timestamp ChangedAt
        string Reason
    }
```

### Sơ đồ ERD - Module Telemetry & Monitoring

```mermaid
erDiagram
    TELEMETRY_DATA ||--o{ GPS_POSITIONS : "includes"
    TELEMETRY_DATA ||--o{ ENGINE_DATA : "includes"
    TELEMETRY_DATA ||--o{ GENERATOR_DATA : "includes"
    TELEMETRY_DATA ||--o{ FUEL_TANK_DATA : "includes"
    TELEMETRY_DATA ||--o{ ENVIRONMENTAL_DATA : "includes"

    TELEMETRY_DATA {
        guid Id PK
        timestamp RecordedAt
        string DataSource
        boolean IsSynced
    }
    
    GPS_POSITIONS {
        guid Id PK
        guid TelemetryId FK
        decimal Latitude
        decimal Longitude
        decimal Speed
        decimal Heading
        decimal COG
        decimal SOG
        string NavigationStatus
    }
    
    ENGINE_DATA {
        guid Id PK
        guid TelemetryId FK
        string EngineId
        decimal RPM
        decimal Power
        decimal FuelRate
        decimal ExhaustTemp
        decimal OilPressure
        decimal CoolantTemp
        decimal RunningHours
    }
    
    GENERATOR_DATA {
        guid Id PK
        guid TelemetryId FK
        string GeneratorId
        decimal Voltage
        decimal Current
        decimal Frequency
        decimal Power
        decimal RunningHours
        string Status
    }
    
    FUEL_TANK_DATA {
        guid Id PK
        guid TelemetryId FK
        string TankId
        string FuelType
        decimal CurrentLevel
        decimal Capacity
        decimal Temperature
    }
    
    ENVIRONMENTAL_DATA {
        guid Id PK
        guid TelemetryId FK
        decimal WindSpeed
        decimal WindDirection
        decimal WaveHeight
        decimal SeaTemp
        decimal AirTemp
        decimal Humidity
        decimal Pressure
    }
```

### Sơ đồ ERD - Module Fuel Analytics

```mermaid
erDiagram
    FUEL_CONSUMPTION_RECORDS ||--o{ DAILY_FUEL_SUMMARIES : "aggregates"
    BUNKER_OPERATIONS ||--o{ FUEL_CONSUMPTION_RECORDS : "supplies"
    VOYAGES ||--o{ FUEL_CONSUMPTION_RECORDS : "during"
    CII_CALCULATIONS }o--|| VOYAGES : "calculated_for"
    EMISSION_REPORTS }o--|| VOYAGES : "generated_for"

    FUEL_CONSUMPTION_RECORDS {
        guid Id PK
        guid VoyageId FK
        date RecordDate
        string FuelType
        decimal MEConsumption
        decimal AEConsumption
        decimal BoilerConsumption
        decimal TotalConsumption
        decimal RunningHours
        decimal DistanceTraveled
    }
    
    DAILY_FUEL_SUMMARIES {
        guid Id PK
        date SummaryDate
        decimal TotalHFO
        decimal TotalMDO
        decimal TotalLNG
        decimal AverageSpeed
        decimal AverageConsumption
        decimal CO2Emission
    }
    
    BUNKER_OPERATIONS {
        guid Id PK
        date BunkerDate
        string Port
        string FuelType
        decimal Quantity
        decimal Density
        decimal Sulfur
        string Supplier
        string BDNNumber
    }
    
    CII_CALCULATIONS {
        guid Id PK
        guid VoyageId FK
        int Year
        decimal AttainedCII
        decimal RequiredCII
        string Rating
        decimal CO2Emissions
        decimal TransportWork
    }
    
    EMISSION_REPORTS {
        guid Id PK
        guid VoyageId FK
        string ReportType
        decimal TotalCO2
        decimal TotalSOx
        decimal TotalNOx
        date ReportDate
    }
    
    VOYAGES {
        guid Id PK
        string VoyageNumber
        string DeparturePort
        string ArrivalPort
        timestamp DepartureTime
        timestamp ArrivalTime
        decimal TotalDistance
        string Status
    }
```

### Sơ đồ ERD - Module Compliance & Reporting (Logbooks)

```mermaid
erDiagram
    DECK_LOG_BOOKS ||--o{ DECK_LOG_ENTRIES : "contains"
    ENGINE_LOG_BOOKS ||--o{ ENGINE_LOG_ENTRIES : "contains"
    OIL_RECORD_BOOKS ||--o{ OIL_RECORD_ENTRIES : "contains"
    GARBAGE_RECORD_BOOKS ||--o{ GARBAGE_RECORD_ENTRIES : "contains"
    BALLAST_WATER_RECORD_BOOKS ||--o{ BALLAST_WATER_ENTRIES : "contains"
    NOON_REPORTS }o--|| VOYAGES : "during"

    DECK_LOG_BOOKS {
        guid Id PK
        date LogDate
        string WatchPeriod
        string OOW
        string MasterSignature
        timestamp SignedAt
        boolean IsLocked
    }
    
    DECK_LOG_ENTRIES {
        guid Id PK
        guid DeckLogBookId FK
        time EntryTime
        string EventType
        text Description
        decimal Latitude
        decimal Longitude
        string Weather
        string SeaState
        string Visibility
    }
    
    ENGINE_LOG_BOOKS {
        guid Id PK
        date LogDate
        string WatchPeriod
        string DutyEngineer
        string ChiefEngineerSignature
        timestamp SignedAt
        boolean IsLocked
    }
    
    ENGINE_LOG_ENTRIES {
        guid Id PK
        guid EngineLogBookId FK
        time EntryTime
        string EventType
        text Description
        decimal MEPower
        decimal MERPM
        decimal AELoad
    }
    
    OIL_RECORD_BOOKS {
        guid Id PK
        string BookPart
        date StartDate
        date EndDate
        boolean IsClosed
    }
    
    OIL_RECORD_ENTRIES {
        guid Id PK
        guid OilRecordBookId FK
        date EntryDate
        string OperationCode
        decimal Quantity
        string TankId
        text Remarks
        string OfficerSignature
    }
    
    GARBAGE_RECORD_BOOKS {
        guid Id PK
        date StartDate
        date EndDate
        boolean IsClosed
    }
    
    GARBAGE_RECORD_ENTRIES {
        guid Id PK
        guid GarbageRecordBookId FK
        date EntryDate
        string Category
        decimal EstimatedAmount
        string DisposalMethod
        decimal Latitude
        decimal Longitude
    }
    
    BALLAST_WATER_RECORD_BOOKS {
        guid Id PK
        date StartDate
        date EndDate
        boolean IsClosed
    }
    
    BALLAST_WATER_ENTRIES {
        guid Id PK
        guid BallastWaterRecordBookId FK
        date EntryDate
        string OperationType
        string TankId
        decimal Quantity
        decimal Latitude
        decimal Longitude
        string TreatmentMethod
    }
    
    NOON_REPORTS {
        guid Id PK
        guid VoyageId FK
        date ReportDate
        time ReportTime
        decimal Latitude
        decimal Longitude
        decimal DistanceToGo
        decimal ETAHours
        decimal AvgSpeed
        decimal FuelROB_HFO
        decimal FuelROB_MDO
        string Weather
        string Remarks
    }
```

### Sơ đồ ERD - Module Voyage Management

```mermaid
erDiagram
    VOYAGES ||--o{ PORT_CALLS : "includes"
    VOYAGES ||--o{ CARGO_OPERATIONS : "has"
    VOYAGES ||--o{ VOYAGE_WAYPOINTS : "follows"
    PORT_CALLS ||--o{ CARGO_OPERATIONS : "at"

    VOYAGES {
        guid Id PK
        string VoyageNumber UK
        string VesselName
        string DeparturePort
        string ArrivalPort
        timestamp DepartureTime
        timestamp ArrivalTime
        timestamp ETD
        timestamp ETA
        decimal PlannedDistance
        decimal ActualDistance
        string CargoType
        decimal CargoQuantity
        string Status
    }
    
    PORT_CALLS {
        guid Id PK
        guid VoyageId FK
        string PortCode
        string PortName
        string Country
        string CallPurpose
        timestamp ArrivalTime
        timestamp DepartureTime
        string BerthNumber
        string AgentName
    }
    
    CARGO_OPERATIONS {
        guid Id PK
        guid VoyageId FK
        guid PortCallId FK
        string OperationType
        string CargoType
        decimal Quantity
        timestamp StartTime
        timestamp EndTime
        string HoldNumber
        text Remarks
    }
    
    VOYAGE_WAYPOINTS {
        guid Id PK
        guid VoyageId FK
        int SequenceOrder
        string WaypointName
        decimal Latitude
        decimal Longitude
        decimal PlannedSpeed
        timestamp PlannedArrival
        timestamp ActualArrival
    }
```

---

## 2.3. Kiến trúc tổng thể hệ thống

Về mặt logic xử lý, hệ thống đã cài đặt thành công thuật toán cảnh báo hạn định chứng chỉ dựa trên thời gian thực, tự động phân cấp mức độ rủi ro qua các trạng thái màu sắc để hỗ trợ bộ phận nhân sự. Bên cạnh đó, logic tự động tạo nhiệm vụ bảo trì cũng được triển khai dựa trên việc phân tích dữ liệu giờ vận hành (Running Hours) từ cảm biến. Đặc biệt, cơ chế đồng bộ dữ liệu Delta Sync đã được thiết kế nhằm tối ưu hóa việc truyền tải qua vệ tinh, chỉ tập trung vào các thay đổi nhỏ nhất của dữ liệu để tiết kiệm chi phí băng thông lên đến 82%. Toàn bộ các chức năng này đã được hiện thực hóa qua hơn 125 đầu cuối API và giao diện Dashboard thời gian thực trên cả nền tảng Web và Mobile.

### Sơ đồ Kiến trúc Edge-Shore

```mermaid
flowchart TB
    subgraph Shore["🏢 SHORE SYSTEM"]
        ShoreDB[("PostgreSQL<br/>Shore Database")]
        ShoreAPI["Shore API<br/>ASP.NET Core 8.0"]
        ShoreWeb["Shore Dashboard<br/>React 19"]
        
        ShoreWeb --> ShoreAPI
        ShoreAPI --> ShoreDB
    end
    
    subgraph Satellite["🛰️ VSAT Connection"]
        Sync["Delta Sync<br/>Protocol"]
    end
    
    subgraph Edge["🚢 EDGE SYSTEM - Trên Tàu"]
        EdgeDB[("PostgreSQL<br/>Edge Database")]
        EdgeAPI["Edge API<br/>ASP.NET Core 8.0"]
        EdgeWeb["Edge Dashboard<br/>React 19"]
        Mobile["Mobile App<br/>Flutter"]
        
        subgraph Sensors["📡 Thiết bị cảm biến"]
            GPS["GPS/AIS"]
            Engine["Engine Sensors"]
            Tank["Tank Gauges"]
            Env["Environmental"]
        end
        
        Sensors --> EdgeAPI
        EdgeWeb --> EdgeAPI
        Mobile --> EdgeAPI
        EdgeAPI --> EdgeDB
    end
    
    ShoreAPI <--> Sync
    Sync <--> EdgeAPI
    
    style Shore fill:#e3f2fd
    style Edge fill:#e8f5e9
    style Satellite fill:#fff3e0
```

### Sơ đồ Data Flow - Delta Sync

```mermaid
sequenceDiagram
    participant Edge as 🚢 Edge System
    participant Queue as 📤 Sync Queue
    participant VSAT as 🛰️ VSAT
    participant Shore as 🏢 Shore System
    
    Edge->>Edge: Detect data change
    Edge->>Edge: Calculate delta (changed fields only)
    Edge->>Queue: Add to Sync Queue
    
    Note over Queue: Priority: Critical > High > Normal
    
    loop Every 15 minutes
        Queue->>VSAT: Send batch of changes
        VSAT->>Shore: Transmit via satellite
        Shore->>Shore: Apply changes
        Shore-->>VSAT: Acknowledge
        VSAT-->>Queue: Mark as synced
    end
    
    Note over Edge,Shore: Bandwidth saving: ~82%
```

---

## 2.4. Tóm tắt kết quả Giai đoạn 1

### Thống kê triển khai

| Module | Số bảng | Số API | Trạng thái |
|--------|---------|--------|------------|
| Crew Management | 5 | 25 | ✅ Hoàn thành |
| PMS | 10 | 35 | ✅ Hoàn thành |
| Telemetry | 9 | 15 | ✅ Hoàn thành |
| Fuel Analytics | 6 | 12 | ✅ Hoàn thành |
| Voyage Management | 4 | 10 | ✅ Hoàn thành |
| Compliance & Reporting | 12 | 28 | ✅ Hoàn thành |
| Materials | 4 | 8 | ✅ Hoàn thành |
| Authentication | 5 | 12 | ✅ Hoàn thành |
| **Tổng cộng** | **55** | **125+** | |

### Công nghệ sử dụng

```mermaid
graph LR
    subgraph Backend
        NET["ASP.NET Core 8.0"]
        EF["Entity Framework Core 8.0"]
        PG["PostgreSQL 15"]
    end
    
    subgraph Frontend
        React["React 19"]
        TS["TypeScript"]
        TW["Tailwind CSS"]
    end
    
    subgraph Mobile
        Flutter["Flutter 3.x"]
        Dart["Dart"]
    end
    
    subgraph Infrastructure
        Docker["Docker"]
        Compose["Docker Compose"]
    end
    
    Backend --> Frontend
    Backend --> Mobile
    Infrastructure --> Backend
```
