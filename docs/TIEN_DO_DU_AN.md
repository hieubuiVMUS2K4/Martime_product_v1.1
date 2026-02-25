# ĐÁNH GIÁ TIẾN ĐỘ DỰ ÁN — Maritime Product v1.1
> Ngày đánh giá: 25/02/2026  
> Phân hệ: **Shore Frontend** (`frontend/`) + **Edge Frontend** (`frontend-edge/`) + **Mobile App** (`frontend-mobile/`) + **Backend** (`edge-services/`)

---

## SƠ ĐỒ TỔNG QUAN

| Hệ thống | Trạng thái tổng thể |
|---|---|
| Shore Frontend (`frontend/`) | ⚠️ Skeletal – UI cơ bản, phần lớn dùng mock data |
| Edge Frontend (`frontend-edge/`) | ✅ Core modules hoàn thiện, 4 module còn là stub |
| Mobile App (`frontend-mobile/`) | 🔄 Một phần – Alarms, Tasks, Schedule, Home |
| Edge Services Backend (`edge-services/`) | ✅ Backend API đầy đủ cho hầu hết module |
| Shore Backend (`backend/`) | ⚠️ Rất hạn chế – chủ yếu Ship/Sync |

---

## PHẦN A — SHORE FRONTEND (Văn phòng / Bờ)

Sidebar menu: `Dashboard` | `QL danh mục` | `QL tàu` | `QL thuyền viên` | `QL hải trình` | `QL phân công công việc` | `Master Schedule (PMS)`

---

### I. Module Quản lý Thuyền viên & Chứng chỉ Thuyền viên

**Đã triển khai:**
- Đầy đủ các trường thông tin cơ bản: mã thuyền viên, họ tên, ngày sinh, giới tính, chức vụ, SĐT, email, địa chỉ, ngày vào làm, trạng thái
- Chức năng CRUD: Thêm / Sửa / Xem chi tiết thuyền viên
- Filter/tìm kiếm theo tên, chức vụ
- UI modal thêm/sửa thuyền viên đầy đủ
- Xem thông tin chi tiết (view detail modal)

**Cần cải thiện:**
- **[Chưa kết nối API]** Toàn bộ dữ liệu đang dùng `mockCrewData` – chưa tích hợp với backend thực
- **[Thiếu module chứng chỉ]** Màn hình shore chưa có tab/menu quản lý chứng chỉ thuyền viên (chỉ edge mới có đầy đủ)
- **[Thiếu sign-on/sign-off]** Không có tính năng điều chỉnh thuyền viên lên/xuống tàu
- **[Thiếu liên kết hải trình]** Chưa có liên kết giữa thuyền viên và hải trình (assignment)
- **[Thiếu phân quyền]** Chưa có cơ chế phân quyền theo vai trò thuyền viên

---

### II. Module Quản lý Danh mục

**Đã triển khai:**
- 7 nhóm danh mục hiển thị dưới dạng card: Loại tàu, Loại vật tư, Chức vụ, Loại báo cáo, Loại chứng chỉ, Loại bảo trì, Loại chi phí
- CRUD (Thêm / Sửa / Xóa) danh mục trong modal riêng
- Tìm kiếm trong từng nhóm danh mục

**Cần cải thiện:**
- **[Chưa kết nối API]** Chỉ có 2 trong 7 nhóm có dữ liệu mẫu (`loai-tau`, `loai-vat-tu`); 5 nhóm còn lại hoàn toàn trống
- **[Không lưu trữ]** Mọi thay đổi mất khi refresh – chưa gọi API backend
- **[Thiếu mã hóa nghiệp vụ]** Chưa có validation ràng buộc (mã trùng, tên trùng)
- **[Thiếu import/export]** Không có chức năng nhập/xuất dữ liệu danh mục hàng loạt

---

### III. Module Quản lý Tàu (QL tàu)

**Đã triển khai:**
- Mục `/vessels` có trong menu sidebar và route
- Backend có `VesselsController` và `ShipsController` trong `backend/`

**Cần cải thiện:**
- **[Chưa có trang UI]** Không có page component riêng cho quản lý tàu trong `frontend/src/pages/`
- **[Thiếu thông tin tàu]** Cần màn hình đầy đủ: thông tin tàu (IMO, call sign, trọng tải, flag), lịch sử, tình trạng kỹ thuật
- **[Chưa có dashboard tàu]** Thiếu giao diện tổng quan trạng thái từng tàu trong đội tàu

---

### IV. Module Quản lý Hải trình (Shore)

**Đã triển khai:**
- Route `/voyages` đã đăng ký trong menu sidebar

**Cần cải thiện:**
- **[Chưa có trang UI]** Không tìm thấy page component cho Voyage trong `frontend/src/pages/`
- **[Hoàn toàn chưa triển khai]** Trong khi Edge đã có hải trình đầy đủ (VoyagePage 2024 dòng code), Shore vẫn chưa có
- **[Thiếu tích hợp]** Không có luồng phê duyệt/giám sát hải trình từ bờ

---

### V. Module Quản lý Phân công Công việc

**Đã triển khai:**
- Danh sách kế hoạch công việc dạng bảng
- Hiển thị các trường: STT, ngày tạo, tên kế hoạch, loại, ngày bắt đầu/kết thúc, trạng thái (Phê duyệt / Chờ / Hoàn thành / Quá hạn)
- Badge màu theo trạng thái
- Filter theo trạng thái và tìm kiếm
- Chức năng Thêm mới / Xem chi tiết / Duyệt / Từ chối (UI)

**Cần cải thiện:**
- **[Chưa kết nối API]** Toàn bộ dùng `mockWorkPlans` – chưa tích hợp backend
- **[Thiếu gán người thực hiện]** Không có cơ chế assign thuyền viên cụ thể vào công việc
- **[Thiếu tích hợp PMS]** Phân công chưa liên kết với lịch bảo trì PMS từ edge
- **[Thiếu thông báo]** Không có push notification khi task quá hạn hoặc được phê duyệt

---

### VI. Module Master Schedule / PMS (Shore)

**Đã triển khai:**
- Kết nối API thực qua `pmsService` – tải lịch từ backend
- Xem theo 3 chế độ: Tháng / Quý / Năm
- Filter theo phòng ban (Engine, Deck, Electrical, Safety, Other)
- Hiển thị lịch trực quan trên calendar grid
- Điều hướng tháng (prev/next)

**Cần cải thiện:**
- **[Chỉ xem]** Chưa có chức năng tạo/sửa lịch trực tiếp từ shore
- **[Thiếu drill-down]** Không thể click vào task để xem chi tiết / duyệt
- **[Thiếu export]** Không có xuất PDF/Excel lịch bảo trì
- **[Thiếu thống kê]** Không có KPI: % completion, task sắp đến hạn, v.v.

---

## PHẦN B — EDGE FRONTEND (Trên tàu)

Sidebar menu: `Dashboard` | `Navigation` | `Engine` | `Fuel Analytics` | `Alarms` | `Crew` | `PMS` | `Materials` | `Reporting` | `Voyage` | `Ports` | `Compliance` | `Sync` | `Logbooks`

---

### VII. Module Dashboard (Edge)

**Đã triển khai:**
- Tổng quan trạng thái tàu
- Hiển thị thông tin tốc độ, vị trí, nhiên liệu, thuyền viên
- Kết nối API thực để lấy dữ liệu real-time

**Cần cải thiện:**
- **[Thiếu widget alarm]** Không hiển thị cảnh báo nổi bật trên dashboard
- **[Thiếu trạng thái PMS]** Chưa hiển thị tóm tắt task bảo dưỡng sắp đến hạn
- **[Thiếu tùy biến]** Không thể cấu hình widget hiển thị theo nhu cầu người dùng

---

### VIII. Module Navigation (Hàng hải)

**Đã triển khai:**
- Menu item và route đã đăng ký

**Cần cải thiện:**
- **[Chưa triển khai]** Trang hiện là stub "Coming Soon"
- Cần: Nhập dữ liệu hành trình (course, speed, position, weather)
- Cần: Tích hợp bản đồ GPS / AIS
- Cần: Chart room log điện tử

---

### IX. Module Engine (Máy tàu)

**Đã triển khai:**
- Menu item và route đã đăng ký

**Cần cải thiện:**
- **[Chưa triển khai]** Trang hiện là stub "Coming Soon"
- Cần: Dashboard thông số máy chính (RPM, nhiệt độ, áp suất)
- Cần: Log ca máy (Engine Room Log trực tiếp từ module này)
- Cần: Cảnh báo ngưỡng thông số

---

### X. Module Fuel Analytics (Phân tích Nhiên liệu)

**Đã triển khai:**
- Dashboard đầy đủ với biểu đồ (BarChart, AreaChart) qua Recharts
- Hiển thị CII Rating (A–E) với màu sắc phân loại
- Thống kê: tổng tiêu thụ nhiên liệu, tốc độ trung bình, khoảng cách
- Kết nối API `fuelAnalyticsService` thực
- Tự động refresh mỗi 5 phút

**Cần cải thiện:**
- **[Chỉ đọc]** Chưa có form nhập liệu bunker / tiêu thụ nhiên liệu trực tiếp
- **[Thiếu dự báo]** Không có tính năng forecast tiêu thụ nhiên liệu kế hoạch
- **[Thiếu so sánh]** Chưa so sánh được giữa các hành trình
- **[Thiếu xuất báo cáo]** Không có export PDF/Excel cho báo cáo nhiên liệu

---

### XI. Module Alarms (Cảnh báo)

**Đã triển khai:**
- Backend `AlarmsController` đã có API
- Mobile app có màn hình alarms đầy đủ (với real-time, badge số lượng)

**Cần cải thiện:**
- **[Chưa triển khai trên Edge]** Frontend-edge chỉ là stub "Coming Soon"
- Cần đưa màn hình alarms vào edge frontend (hiển thị list, xác nhận, đóng alarm)
- **[Thiếu phân loại]** Chưa phân loại alarm theo mức độ (Critical / Warning / Info)
- **[Thiếu lịch sử alarm]** Không có alarm history log

---

### XII. Module Crew – Thuyền viên & Chứng chỉ (Edge/Tàu)

**Đã triển khai:**
- **Tab "Onboard":** Danh sách thuyền viên đang trên tàu, hiển thị rank, quốc gia, trạng thái
- **Tab "Certificates":** Quản lý đầy đủ chứng chỉ của toàn bộ thuyền viên
- Filter: theo quốc gia, theo rank
- Sắp xếp (sort) multi-column
- Cache dữ liệu để tránh reload không cần thiết
- CRUD thuyền viên qua modal (AddCrewModal)
- Xem chi tiết chứng chỉ (DetailCertificatesModal)
- Thêm chứng chỉ trực tiếp (AddCertificateModal)
- Kết nối API thực qua `maritimeService`
- Trang chi tiết thuyền viên (CrewDetailPage)

**Cần cải thiện:**
- **[Thiếu sign-on/off workflow]** Không có luồng ký sign-on/sign-off chính thức với timestamp và xác nhận
- **[Thiếu nhắc nhở chứng chỉ hết hạn]** Chưa có cảnh báo proactive khi chứng chỉ sắp hết hạn (< 30/60/90 ngày)
- **[Chưa đồng bộ với shore]** Dữ liệu thuyền viên edge là độc lập, chưa đồng bộ về shore
- **[Thiếu ảnh thuyền viên]** Không có upload/hiển thị ảnh thuyền viên
- **[Thiếu biểu mẫu ISM]** Chưa tích hợp biểu mẫu ISM signing (Declaration of Health, v.v.)

---

### XIII. Module PMS — Planned Maintenance System (Edge)

**Đã triển khai (5 sub-menu):**

**a) Equipment Assets (Trang thiết bị)**
- CRUD: Thêm / Sửa / Xem / Xóa thiết bị
- Phân loại theo category (Engine, Generator, Pump, v.v.) và status
- Import Excel/CSV hàng loạt
- Phân trang (10 items/trang)
- Tìm kiếm và filter

**b) Equipment Groups (Nhóm thiết bị)**
- Quản lý nhóm thiết bị theo phòng ban
- CRUD đầy đủ

**c) Schedule Config (Cấu hình lịch)**
- Cấu hình lịch bảo trì định kỳ theo running hours / calendar
- Gán checklist item cho từng task

**d) Master Schedule (Lịch tổng)**
- Xem lịch tổng hợp toàn bộ task bảo trì
- Filter theo phòng ban / khoảng thời gian

**e) Maintenance (Bảo dưỡng)**
- Danh sách task bảo trì (+ trạng thái: Pending, In Progress, Completed, Overdue)
- Workflow task: chuyển trạng thái, checklist item tích
- Maintenance history (lịch sử bảo trì)
- Deferral management (hoãn task có lý do)
- Approval dashboard (phê duyệt task)
- Work planning (lên kế hoạch)
- Unassigned tasks (task chưa gán người)

**Cần cải thiện:**
- **[Thiếu xuất PDF]** Không có export maintenance report / job card
- **[Thiếu tích hợp vật tư]** Chưa liên kết task bảo trì với tiêu thụ spare parts từ Materials
- **[Thiếu cost tracking]** Không có theo dõi chi phí bảo trì
- **[Thiếu running hours]** Đồng hồ running hours chưa cập nhật tự động từ module Engine
- **[Thiếu tích hợp alarm]** Task overdue chưa tự động tạo alarm

---

### XIV. Module Materials (Vật tư / Kho)

**Đã triển khai:**
- **Tab Items:** Danh sách vật tư với search, filter theo category/unit, phân trang
- **Tab Low Stock:** Cảnh báo vật tư sắp hết (dưới ngưỡng tối thiểu)
- **Tab Categories:** Quản lý nhóm danh mục vật tư
- **Tab Receipts:** Quản lý phiếu nhập kho
- CRUD đầy đủ các tab
- Điều chỉnh tồn kho (Stock Adjustment)
- Import phiếu nhập từ CSV
- Xem chi tiết phiếu nhập (ReceiptDetailModal)
- Sort multi-column trên bảng
- Kết nối API thực (`materialService`, `receiptService`)

**Cần cải thiện:**
- **[Thiếu phiếu xuất kho]** Chỉ có phiếu nhập (receipt), chưa có phiếu xuất liên kết với PMS
- **[Thiếu vị trí lưu kho]** Không có trường vị trí lưu kho (location / bin)
- **[Thiếu barcode/QR]** Không có tính năng quét mã vạch khi nhập/xuất
- **[Chưa kết nối shore]** Module vật tư trên shore chưa có – không thể đặt hàng / duyệt từ bờ
- **[Thiếu supplier management]** Không có danh sách nhà cung cấp, lịch sử giá

---

### XV. Module Reporting (Báo cáo)

**Đã triển khai:**
- **Noon Report** (Báo cáo noon – 12h hàng ngày): Vị trí tàu, thời tiết, hải lý đã đi, tốc độ gió, tiêu thụ nhiên liệu
- **Departure Report** (Báo cáo rời cảng): Thông tin rời cảng, dự kiến đến
- **Arrival Report** (Báo cáo cập cảng): Thông tin cập bến, tổng kết hành trình
- **Bunker Report** (Báo cáo nhận nhiên liệu): Chi tiết bunkering
- **Position Report** (Báo cáo vị trí): Cập nhật vị trí bất thường
- Dashboard báo cáo: Danh sách báo cáo đã gửi, trạng thái
- Kết nối API thực cho tất cả loại báo cáo

**Cần cải thiện:**
- **[Thiếu export]** Không có xuất PDF / xuất gửi email báo cáo
- **[Thiếu template]** Không có hệ thống template theo yêu cầu công ty / đăng kiểm
- **[Thiếu xác nhận shore]** Không có cơ chế xác nhận/phê duyệt báo cáo từ shore
- **[Chưa có Monthly/Weekly Report hoàn chỉnh]** Có file component nhưng chưa tích hợp vào routing chính
- **[Thiếu báo cáo tổng hợp]** Chưa có aggregate report theo tháng/quý

---

### XVI. Module Voyage (Hải trình – Edge)

**Đã triển khai:**
- CRUD Hải trình: Tạo / Sửa / Xóa / Chuyển trạng thái (Planning → Underway → Completed)
- **Tab Overview:** Thông tin tổng quan hải trình
- **Tab Port Calls:** Quản lý ghé cảng (Departure / Arrival / Transit / Bunkering / Drydock)
- **Tab Crew:** Phân công thuyền viên vào hải trình (sign-on, sign-off, trạng thái)
- **Tab Cargo:** Quản lý hàng hóa (Loading → Loaded → Discharging → Discharged)
- **Tab FAL Form 5:** Crew list theo định dạng IMO FAL Form 5
- Status-based access control (bảo vệ state transitions hợp lệ)
- Kết nối đầy đủ qua `voyageMgmtService`

**Cần cải thiện:**
- **[Thiếu bản đồ]** Không có hiển thị route/waypoint trên bản đồ
- **[Thiếu tích hợp logbooks]** Chưa tự động tạo Voyage Log khi bắt đầu/kết thúc hải trình
- **[Thiếu tích hợp Noon Report]** Báo cáo noon chưa tự động gắn với hải trình đang chạy
- **[Thiếu ETA/ETD thực tế]** Không có tracking độ lệch so với kế hoạch
- **[Thiếu tính toán freight]** Không có module tính toán cước vận tải / laytime

---

### XVII. Module Ports (Cảng)

**Đã triển khai:**
- Danh sách cảng (PortManagementPage)
- CRUD cảng cơ bản
- Sử dụng trong Voyage (chọn cảng đi/đến)

**Cần cải thiện:**
- **[Thiếu thông tin đầy đủ]** Cần bổ sung: múi giờ, UNLOCODE, tọa độ GPS, loại cảng, phí cảng
- **[Thiếu agent information]** Không có thông tin hải vụ / đại lý cảng
- **[Thiếu dịch vụ cảng]** Không có danh sách dịch vụ có sẵn tại cảng

---

### XVIII. Module Compliance (Tuân thủ)

**Đã triển khai:**
- Backend `ComplianceController` đã có API

**Cần cải thiện:**
- **[Chưa triển khai frontend]** Trang hiện là stub "Coming Soon"
- Cần: Danh sách inspection checklist (SOLAS, MARPOL, MLC 2006)
- Cần: Tracking ngày đến hạn kiểm tra / audit
- Cần: Lưu kết quả inspection, findings, corrective actions

---

### XIX. Module Sync (Đồng bộ)

**Đã triển khai:**
- Backend có `SyncController`
- Feature branch hiện tại: `feature/tinhhash` (đang phát triển tính năng hash để bảo đảm tính toàn vẹn dữ liệu khi sync)

**Cần cải thiện:**
- **[Chưa triển khai frontend]** Trang hiện là stub "Coming Soon"
- Cần: Màn hình hiển thị trạng thái đồng bộ (last sync time, pending records)
- Cần: Manual trigger sync
- Cần: Conflict resolution UI khi dữ liệu shore và edge differ
- Cần: Offline queue management

---

### XX. Module Logbooks (Nhật ký tàu)

**Đã triển khai (8 nhật ký):**

**a) Voyage Log (Nhật ký hành trình)**
- CRUD daily entries
- Liên kết với hải trình (Voyage)
- Xem chi tiết theo ngày

**b) Deck Log (Nhật ký boong)**
- Ghi nhận các sự kiện trên boong theo ca
- Thông tin thời tiết, vị trí

**c) Engine Log (Nhật ký máy)**
- Ghi nhận sự kiện, thông số máy theo ca

**d) Oil Record Book (Nhật ký dầu – ORB)**
- Phần I (Machinery Space): ghi nhận mọi thao tác dầu máy theo quy định MARPOL 73/78 Annex I
- Entries đầy đủ các operation code

**e) Garbage Record (Nhật ký rác – GRB)**
- Part I & Part II theo MARPOL Annex V
- Ghi nhận xả rác tại cảng và trên biển

**f) Ballast Water Record (Nhật ký nước dằn)**
- Ghi nhận theo Ballast Water Management Convention
- Các trường: source location, exchange location, quantity, method

**g) Watchkeeping Log (Nhật ký trực ca)**
- 6 ca trực 4h (00-04, 04-08, 08-12, 12-16, 16-20, 20-24)
- Phân loại: Navigation Watch / Engine Watch
- Douglas Sea Scale, visibility, fatigue level (ISM)
- Officer on watch tracking

**h) Abstract Log (Nhật ký tóm tắt)**
- Liên kết với Voyage (chọn hải trình)
- Abstract log theo từng chặng (Leg)
- Daily entries cho từng chặng
- Xem tổng kết (summary tab)
- Export dữ liệu

**Cần cải thiện:**
- **[Thiếu chữ ký số]** Không có cơ chế ký điện tử (e-signature) xác nhận của thuyền trưởng / sỹ quan
- **[Thiếu export PDF]** Chưa có chức năng xuất PDF cho tất cả logbooks (yêu cầu pháp lý quan trọng)
- **[Thiếu lock sau ký]** Giao diện chưa có cơ chế khóa entry đã ký (tránh sửa sau khi submit)
- **[Thiếu tích hợp giữa các logbooks]** Ví dụ: Deck Log và Watchkeeping Log chia sẻ thông tin thời tiết nhưng phải nhập riêng
- **[Abstract Log – thiếu tự động]** Dữ liệu daily entry chưa tự động điền từ Voyage Log / Noon Report

---

## PHẦN C — ỨNG DỤNG MOBILE (Flutter)

### XXI. Mobile App — Thuyền viên

**Đã triển khai:**
- **Auth:** Màn hình đăng nhập
- **Home:** Dashboard tổng quan
- **Alarms:** Danh sách cảnh báo, real-time badge đếm, xác nhận alarm
- **Schedule:** Xem lịch phân công / bảo trì
- **Tasks:** Danh sách và quản lý công việc được giao
- **Profile:** Xem / cập nhật thông tin cá nhân
- **Settings:** Cấu hình ngôn ngữ, theme, thông báo

**Cần cải thiện:**
- **[Thiếu Logbooks]** Không có màn hình điền nhật ký trực tiếp từ mobile
- **[Thiếu Chứng chỉ]** Không xem được chứng chỉ cá nhân, ngày hết hạn
- **[Thiếu Voyage]** Không xem được thông tin hải trình hiện tại
- **[Thiếu offline mode hoàn chỉnh]** Cần cơ chế offline-first cho nhật ký và task khi không có mạng
- **[Thiếu push notification]** Chưa có push notification cho alarm / task mới

---

## PHẦN D — BACKEND API

### XXII. Edge Services Backend (`edge-services/`)

**Đã triển khai đầy đủ:**
- **Crew:** CrewController, CertificatesController, CountriesController, CountryCertificatesController, RankCertificatesController, RanksController
- **Logbooks:** AbstractLog, BallastWater, DeckLogbook, EngineLogbook, GarbagePartI/II, GarbageRecord, OilRecord, Watchkeeping
- **Maintenance (PMS):** EquipmentAsset, EquipmentGroup, MaintenanceController, MaintenanceSchedule, TaskChecklistItems, TaskRealTimeUpdates, TaskWorkflow
- **Voyage:** PortController, TelemetryController, VoyageController, VoyageLogController
- **Reporting:** AggregateReportController, ReportingController
- **Safety:** AlarmsController, ComplianceController, DeferralRequestController
- **Inventory:** FuelAnalyticsController, MaterialController, MaterialReceiptsController

**Cần cải thiện:**
- **[Thiếu authentication/authorization]** Cần kiểm tra middleware JWT / phân quyền role-based
- **[Thiếu rate limiting]** Chưa có throttling cho API
- **[Cần thêm PDF generation]** Chưa có endpoint generate PDF cho logbooks / báo cáo

### XXIII. Shore Backend (`backend/`)

**Đã triển khai:**
- `VesselsController` / `ShipsController`: API quản lý phương tiện
- `SyncController`: Endpoint nhận dữ liệu đồng bộ từ edge
- `VesselTelemetryController`: Nhận telemetry data

**Cần cải thiện:**
- **[Thiếu hầu hết module]** Shore backend hầu như không có API cho crew, voyages, work assignment, v.v.
- Cần phát triển đầy đủ các controller tương ứng với shore frontend modules

---

## BẢNG TỔNG HỢP TIẾN ĐỘ

| # | Module | Shore | Edge | Mobile | Backend | Mức độ |
|---|---|:---:|:---:|:---:|:---:|:---:|
| 1 | Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ Hoàn thiện |
| 2 | QL Thuyền viên | ⚠️ mock | ✅ | ⚠️ | ✅ | 🔄 Cần nâng cấp |
| 3 | Chứng chỉ thuyền viên | ❌ | ✅ | ❌ | ✅ | 🔄 Thiếu shore/mobile |
| 4 | QL Danh mục | ⚠️ mock | N/A | N/A | ⚠️ | ⚠️ Cần kết nối API |
| 5 | QL Tàu | ❌ | N/A | N/A | ✅ | ❌ Chưa có UI shore |
| 6 | Hải trình (Voyage) | ❌ | ✅ | ❌ | ✅ | 🔄 Thiếu shore |
| 7 | Phân công Công việc | ⚠️ mock | N/A | ✅ | ❌ | ⚠️ Cần kết nối API |
| 8 | PMS – Master Schedule | ✅ | ✅ | ✅ | ✅ | ✅ Hoàn thiện |
| 9 | PMS – Assets/Groups | N/A | ✅ | N/A | ✅ | ✅ Hoàn thiện |
| 10 | PMS – Maintenance | N/A | ✅ | ✅ | ✅ | ✅ Hoàn thiện |
| 11 | Navigation | N/A | ❌ stub | N/A | ❌ | ❌ Chưa triển khai |
| 12 | Engine | N/A | ❌ stub | N/A | ❌ | ❌ Chưa triển khai |
| 13 | Fuel Analytics | N/A | ✅ | N/A | ✅ | ✅ Cơ bản xong |
| 14 | Alarms | N/A | ❌ stub | ✅ | ✅ | 🔄 Cần edge frontend |
| 15 | Materials (Vật tư) | N/A | ✅ | N/A | ✅ | ✅ Hoàn thiện |
| 16 | Reporting (Báo cáo) | N/A | ✅ | N/A | ✅ | ✅ Cơ bản xong |
| 17 | Logbooks (8 loại) | N/A | ✅ | N/A | ✅ | ✅ Hoàn thiện |
| 18 | Ports (Cảng) | N/A | ✅ | N/A | ✅ | ✅ Cơ bản xong |
| 19 | Compliance | N/A | ❌ stub | N/A | ✅ | ❌ Cần edge frontend |
| 20 | Sync | N/A | ❌ stub | N/A | ✅ | 🔄 Backend xong, UI thiếu |

**Chú thích:** ✅ Hoàn thiện | ⚠️ Có nhưng cần cải thiện | 🔄 Đang phát triển | ❌ Chưa có / stub

---

## ƯU TIÊN PHÁT TRIỂN TIẾP THEO

### 🔴 Ưu tiên cao (P1 – Ảnh hưởng nghiệp vụ cốt lõi)
1. Kết nối API thực cho Shore Frontend (Crew, Category, WorkAssignment)
2. Xây dựng trang QL Tàu (shore) và trang QL Hải trình (shore)
3. Triển khai module Alarms đầy đủ trên Edge Frontend
4. Export PDF cho Logbooks (yêu cầu pháp lý)
5. Cơ chế chữ ký số / lock entry sau khi ký

### 🟡 Ưu tiên trung (P2 – Hoàn thiện tính năng)
6. Module Navigation và Engine trên Edge (nhập liệu thông số)
7. Kết nối module Sync (UI + conflict resolution)
8. Compliance frontend trên Edge
9. Tích hợp vật tư (Materials) ↔ PMS task
10. Push notification cho Mobile App (alarms, task deadline)

### 🟢 Ưu tiên thấp (P3 – Nâng cao)
11. Bản đồ / route tracking trong Voyage
12. Barcode/QR cho Materials
13. Offline-first mode cho Mobile App
14. Forecast nhiên liệu / CII prediction trong Fuel Analytics
