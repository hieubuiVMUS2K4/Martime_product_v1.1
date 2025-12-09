# KẾ HOẠCH TRIỂN KHAI CHI TIẾT HỆ THỐNG NHẬT KÝ ĐIỆN TỬ HÀNG HẢI (MARITIME E-LOGBOOKS) - V2.0

## 1. Tổng quan & Mục tiêu
Xây dựng hệ thống nhật ký điện tử thay thế hoàn toàn nhật ký giấy, đảm bảo tính pháp lý và tuân thủ các công ước quốc tế (SOLAS, MARPOL, BWM, ISM Code). Hệ thống được thiết kế theo kiến trúc "Offline-First" để hoạt động ổn định trong môi trường biển không có internet.

## 2. Kiến trúc Kỹ thuật (Technical Architecture)

### 2.1. Backend (Core Services)
-   **Framework:** ASP.NET Core 8 Web API.
-   **Database:** PostgreSQL 16 (Lưu trữ dữ liệu chính) + TimescaleDB (Lưu trữ dữ liệu cảm biến/telemetry liên kết với nhật ký).
-   **ORM:** Entity Framework Core (Code-First Migration).
-   **Validation:** FluentValidation (Kiểm tra logic nghiệp vụ hàng hải chặt chẽ).
-   **Security:** JWT Authentication + Role-Based Access Control (RBAC) theo cấp bậc thuyền viên (Master, Chief Engineer, OOW).

### 2.2. Frontend (Industrial UI)
-   **Framework:** React + TypeScript.
-   **State Management:** TanStack Query (Quản lý server state & caching).
-   **Offline Storage:** Dexie.js (IndexedDB wrapper) để lưu trữ dữ liệu khi mất kết nối.
-   **Design System:** "Maritime Industrial" - Giao diện tối, độ tương phản cao, không bo góc, tối ưu cho màn hình cảm ứng và điều kiện ánh sáng yếu (Night Mode).

## 3. Kế hoạch triển khai chi tiết (Detailed Implementation Plan)

### Giai đoạn 1: Backend Core & Business Logic (Tuần 1)
*Mục tiêu: Hoàn thiện API và logic xử lý dữ liệu cho 6 loại nhật ký.*

#### 1.1. Data Transfer Objects (DTOs) & Mapping
Tách biệt hoàn toàn Data Model và API Model.
-   **Deck Log:** `CreateDeckLogDto` (Position, Weather, EventType), `SignDeckLogDto`.
-   **Engine Log:** `CreateEngineLogDto` (Counters, Pressures, Temps), `FuelConsumptionDto`.
-   **Oil Record Book (ORB):**
    -   `CreateOrbEntryDto`: Bao gồm `Code` (C-J), `ItemNo`, `Quantity`, `TankId`.
    -   `OrbCorrectionDto`: DTO đặc biệt để tạo bút toán sửa sai (Strike-through logic).
-   **Garbage Record Book:** `CreateGarbageEntryDto` (Category A-K, ProcessingType, Location).
-   **Watchkeeping:** `CreateWatchLogDto` (Course, Speed, Lookout, Events).

#### 1.2. Validation Rules (Maritime Constraints)
Sử dụng FluentValidation để đảm bảo tính hợp lệ của dữ liệu ngay từ đầu vào:
-   **Geo-Validation:** Tọa độ Lat (-90 đến 90), Lon (-180 đến 180). Cảnh báo nếu vị trí thay đổi quá nhanh so với tốc độ tàu (Impossible travel).
-   **Sequence Validation:** Thời gian nhập liệu phải >= Thời gian của bút toán trước đó (đối với các nhật ký tuần tự).
-   **MARPOL Logic:**
    -   Nếu `OperationCode` là "Discharge to Sea", bắt buộc phải có tọa độ và tốc độ tàu > 7 knots (theo luật).
    -   Nếu `GarbageCategory` là "Plastics", cấm xả xuống biển (Discharge to Sea = False).
-   **Tank Capacity:** `Quantity` nhập vào <= Dung tích tối đa của két chứa (`TankCapacity`).

#### 1.3. API Endpoints Strategy
Mỗi loại nhật ký sẽ có bộ Controller riêng biệt:
-   `GET /api/logbooks/{type}`: Lấy danh sách (phân trang, lọc theo ngày).
-   `POST /api/logbooks/{type}`: Tạo bút toán mới (Draft).
-   `PUT /api/logbooks/{type}/{id}`: Sửa bút toán (chỉ khi chưa ký).
-   `POST /api/logbooks/{type}/{id}/sign`: Ký điện tử (Khóa bút toán).
-   `POST /api/logbooks/{type}/{id}/strike-through`: Gạch bỏ bút toán sai (tạo bút toán sửa sai).

### Giai đoạn 2: Frontend Industrial UI/UX (Tuần 2)
*Mục tiêu: Xây dựng giao diện nhập liệu chuyên nghiệp, hiệu quả.*

#### 2.1. Design System "Maritime Industrial"
-   **Màu sắc:** Nền đen/xám đậm (`#1a1a1a`), Text vàng hổ phách (`#ffb000`) hoặc xanh lá (`#00ff00`) cho chế độ ban đêm.
-   **Typography:** Font Monospace (Consolas, Roboto Mono) cho các con số và tọa độ.
-   **Components:**
    -   `MaritimeInput`: Input số lớn, dễ bấm trên màn hình cảm ứng.
    -   `CoordinatePicker`: Input nhập vĩ độ/kinh độ chuyên dụng (DD°MM.mm').
    -   `SignaturePad`: Khu vực ký tên (vẽ tay hoặc nhập PIN).

#### 2.2. Form Layouts đặc thù
-   **Deck Log View:** Dạng Timeline dọc, hiển thị sự kiện theo trình tự thời gian.
-   **Engine Log View:** Dạng Bảng tính (Spreadsheet) cho các thông số máy theo giờ (00:00, 04:00, 08:00...).
-   **ORB Wizard:** Giao diện từng bước (Step-by-step) để tránh sai sót mã code MARPOL:
    -   *Step 1:* Chọn loại hoạt động (VD: Bunkering, Discharge).
    -   *Step 2:* Hệ thống tự lọc Code và Item No phù hợp.
    -   *Step 3:* Nhập số liệu và chọn két.

### Giai đoạn 3: Advanced Features & Sync (Tuần 3)
*Mục tiêu: Đảm bảo tính toàn vẹn dữ liệu và hoạt động offline.*

#### 3.1. Digital Signature & Chain of Custody
-   **Hashing:** Mỗi bút toán khi ký sẽ được tạo một mã Hash (SHA-256) dựa trên nội dung + Hash của bút toán trước đó (tạo thành chuỗi liên kết giống Blockchain).
-   **Tamper Evidence:** Bất kỳ thay đổi nào vào DB trực tiếp sẽ làm sai lệch chuỗi Hash, giúp phát hiện gian lận.
-   **Approval Workflow:** OOW nhập liệu -> Chief Officer kiểm tra -> Master ký duyệt cuối cùng.

#### 3.2. Offline-First Sync Engine
-   **Local Queue:** Khi mất mạng, các hành động (Create, Sign) được lưu vào IndexedDB.
-   **Background Sync:** Service Worker tự động đẩy dữ liệu lên Server khi có kết nối lại.
-   **Conflict Resolution:** Ưu tiên dữ liệu từ Server (Server-wins) cho các nhật ký tuần tự để đảm bảo tính nhất quán thời gian.

### Giai đoạn 4: Reporting & Compliance (Tuần 4)
*Mục tiêu: Xuất báo cáo phục vụ thanh tra (PSC/Vetting).*

#### 4.1. IMO Standard Reports
-   Sử dụng thư viện tạo PDF (VD: QuestPDF hoặc iText) để generate file PDF.
-   **Format:** Phải giống hệt mẫu nhật ký giấy quy định trong các công ước (IMO MEPC/MSC Circulars).
-   **Header/Footer:** Tự động điền thông tin tàu (Tên, IMO, Call Sign).

#### 4.2. Audit Trail
-   Ghi lại lịch sử: Ai đã xem, ai đã xuất PDF, ai đã đăng nhập.

## 4. Checklist công việc chi tiết (Granular Checklist)

### Backend Development
- [ ] **Setup:** Cấu trúc thư mục `DTOs`, `Validators`, `Services` cho từng Logbook.
- [ ] **Deck Log:**
    - [ ] API CRUD cơ bản.
    - [ ] Logic tự động lấy tọa độ từ `PositionData` (nếu có).
- [ ] **Engine Log:**
    - [ ] API CRUD dạng bảng.
    - [ ] Logic tính toán tiêu thụ nhiên liệu (ROB Start - ROB End).
- [ ] **Oil Record Book:**
    - [ ] Implement danh sách Code/Item chuẩn MARPOL.
    - [ ] Logic validate "Tank-to-Tank" transfer.
- [ ] **Garbage Record Book:**
    - [ ] API CRUD.
    - [ ] Logic tính tổng rác thải theo Category.
- [ ] **Ballast Water:**
    - [ ] API CRUD.
    - [ ] Validate quy trình D-1 (Exchange) và D-2 (Treatment).

### Frontend Development
- [ ] **UI Base:** Setup Tailwind config cho Maritime Theme (Colors, Spacing).
- [ ] **Components:** Xây dựng `LogbookGrid`, `MaritimeInput`, `SignaturePad`.
- [ ] **Screens:**
    - [ ] Màn hình nhập liệu Deck Log.
    - [ ] Màn hình nhập liệu Engine Log (Grid view).
    - [ ] Màn hình Wizard cho ORB.
- [ ] **Integration:** Kết nối API và xử lý lỗi (Error Handling).

### Testing & QA
- [ ] **Unit Test:** Test các logic validation MARPOL phức tạp.
- [ ] **Integration Test:** Test luồng ký duyệt (Sign flow).
- [ ] **Performance Test:** Load test với bảng `engine_log_books` (nhiều cột).
- [ ] **UAT:** Giả lập quy trình nhập liệu thực tế của thuyền viên.

## 5. Lưu ý quan trọng
-   **Không xóa dữ liệu:** API Delete chỉ thực hiện Soft Delete (nếu cần) hoặc đánh dấu là "Cancelled" với lý do cụ thể.
-   **Timezone:** Tất cả thời gian lưu trữ UTC. Hiển thị theo Local Time của tàu (Ship's Time) nhưng phải lưu offset.
-   **Backup:** Cơ chế backup database tự động hàng ngày (đã có script).

