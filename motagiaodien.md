# MÔ TẢ GIAO DIỆN HỆ THỐNG QUẢN LÝ TÀU BIỂN

## 1. Dashboard - Tổng Quan

Dashboard là trang chính hiển thị tổng quan hoạt động tàu theo thời gian thực. Trang gồm các thẻ thống kê nhanh về cảnh báo nghiêm trọng, tổng số cảnh báo, số thủy thủ trên tàu và công việc bảo trì đang chờ xử lý.

Phần thông tin chi tiết hiển thị định vị tàu với tọa độ và bản đồ mini, giám sát động cơ chính với đồng hồ đo RPM và tải, và điều kiện môi trường như nhiệt độ, gió và thời tiết.

Khu vực nhiên liệu hiển thị phần trăm nhiên liệu còn lại với thanh tiến trình màu cảnh báo theo mức độ. Danh sách bảo trì sắp đến cho biết các công việc cần thực hiện kèm thời hạn và biểu đồ xu hướng. Dữ liệu tự động làm mới mỗi 5 giây để đảm bảo thông tin luôn cập nhật.

## 2. Crew Management - Quản Lý Thủy Thủ Đoàn

Trang quản lý thủy thủ đoàn có hai tab chính: Crew Members và Certificate Monitor.

Tab Crew Members hiển thị danh sách thủy thủ đang trên tàu với thông tin mã nhân viên, họ tên, chức vụ, cấp bậc, quốc tịch, ngày lên tàu và trạng thái. Bảng có thể sắp xếp theo các cột và hỗ trợ thêm thủy thủ mới. Click chuột phải vào thành viên để mở menu nhanh với các tùy chọn xem chi tiết, chuyển trạng thái hoặc xóa.

Tab Certificate Monitor giám sát chứng chỉ của thủy thủ đoàn với ba phần: danh sách chứng chỉ từng thành viên, các loại chứng chỉ hệ thống (STCW, Medical, Safety) và chứng chỉ từ quốc gia khác. Có thể lọc theo quốc gia và xem trạng thái hợp lệ, sắp hết hạn hoặc đã hết hạn. Hệ thống cho phép thêm loại chứng chỉ mới với thông tin mã code, tên, loại, thời hạn và các quốc gia công nhận. Khi thêm chứng chỉ cho thành viên, form yêu cầu nhập số chứng chỉ, ngày cấp, ngày hết hạn, cơ quan cấp và ghi chú.

Trang chi tiết thành viên có tab Basic Data để chỉnh sửa thông tin cá nhân, ngày quan trọng, giấy tờ du lịch và liên hệ khẩn cấp. Tab Documents quản lý ba nhóm tài liệu: giấy tờ tùy thân, hồ sơ y tế và chứng chỉ chuyên môn. Mỗi tài liệu có thể đính kèm file và ghi nhận thông tin ngày cấp, ngày hết hạn.

## 3. Materials Management - Quản Lý Vật Tư

Trang Materials Management quản lý kho vật tư với bốn tab chính. Tab Items hiển thị toàn bộ vật tư trong kho với thông tin tên, danh mục, đơn vị, số lượng tồn, ngưỡng Min/Max, mã linh kiện và giá. Có thể tìm kiếm, lọc theo danh mục hoặc đơn vị, sắp xếp các cột và cảnh báo màu khi tồn kho thấp hoặc vượt mức. Nút Import Receipt cho phép nhập kho hàng loạt từ file Excel theo mẫu với các trường mã vật tư, tên, danh mục, số lượng, đơn vị, giá, vị trí và thông tin bổ sung.

Tab Categories quản lý danh mục vật tư như Bearings, Fasteners, Filters, Lubricants, Paints với tên, mã, mô tả và danh mục cha. Tab Receipts hiển thị lịch sử phiếu nhập kho với mã phiếu, ngày nhập, tổng giá trị và số mặt hàng. Click vào phiếu để xem chi tiết đầy đủ các vật tư đã nhập, số lượng, giá và tổng tiền. Hệ thống giúp theo dõi tồn kho, cảnh báo thiếu hụt và tra cứu lịch sử nhập xuất hiệu quả.

## 4. PMS Planning - Lập Kế Hoạch Bảo Trì

Hệ thống PMS Planning quản lý bảo trì thiết bị tàu biển với năm trang chức năng. Trang Equipment Assets hiển thị danh sách toàn bộ thiết bị kèm mã, tên, danh mục, trạng thái, nhà sản xuất, model, vị trí, giờ vận hành và mức độ quan trọng. Có thể tìm kiếm, lọc theo danh mục và trạng thái, hoặc nhập hàng loạt từ file Excel theo mẫu.

Trang Equipment Groups tổ chức thiết bị thành nhóm theo bộ phận với người phụ trách (PIC). Mỗi nhóm hiển thị tên, danh mục, bộ phận, PIC, số thiết bị và trạng thái. Khi tạo nhóm mới, có thể chọn thiết bị từ danh sách Available và di chuyển sang Assigned. Trang Schedule Config cấu hình lịch bảo trì định kỳ theo lịch (Calendar), giờ chạy (Running Hours) hoặc kết hợp (Hybrid), với chu kỳ, mức ưu tiên và khả năng tự động tạo task.

Trang Master Schedule hiển thị biểu đồ Gantt toàn bộ công việc bảo trì theo thời gian. Có thể xem theo ngày, tuần, tháng hoặc quý với thanh màu thể hiện work period và chấm tròn đánh dấu due date. Màu sắc phân biệt mức độ ưu tiên: đỏ Critical, cam High, vàng Medium, xanh Low. Trang Maintenance là bảng Kanban với tám cột trạng thái: Scheduled, Due, Overdue, Deferrals, In Progress, Pending Approval, Rectify, Completed. Có thể lọc theo ngày, ưu tiên, nhóm thiết bị, loại lịch, người phụ trách, hiển thị/ẩn cột và tìm kiếm. Thẻ task kéo thả giữa các cột và hệ thống tự động đồng bộ mỗi 10 giây.

## 5. Logbooks - Sổ Nhật Ký Tàu

Hệ thống Logbooks quản lý bảy loại sổ nhật ký bắt buộc tuân thủ chuẩn quốc tế. Voyage Log ghi nhận sự kiện hành trình qua wizard ba bước: chọn loại sự kiện (cảng, hành hải, hoa tiêu, đặc biệt), nhập thông tin chi tiết (thời gian, vị trí, cảng, khóa học, tốc độ, sĩ quan trực), xem lại và gửi. Các entry hiển thị dạng timeline hoặc bảng với đầy đủ thông tin và cho phép ký tên số.

Deck Log ghi nhật ký boong theo ca trực với thông tin thời gian, ca, loại entry, vị trí, khóa học, tốc độ, mô tả và ghi chú. Engine Log là bảng nhập trực tiếp thông số động cơ theo giờ gồm RPM, tải, nhiệt độ khí xả, áp suất dầu bôi trơn và tiêu thụ nhiên liệu. Oil Record Book tuân thủ MARPOL Annex I với wizard ba bước để ghi các thao tác dầu (A-H) kèm số lượng, thiết bị, sĩ quan phụ trách và ký tên.

Garbage Record Book tuân thủ MARPOL Annex V với wizard bốn bước: chọn loại rác (A-K từ nhựa đến e-waste), chọn thao tác (thải xuống biển, giao cơ sở, đốt, thải do sự cố), nhập số lượng và vị trí, xem lại. Hệ thống cảnh báo nếu vi phạm MARPOL khi thải loại rác bị cấm. Ballast Water Record Book tuân thủ BWM Convention với wizard ghi các thao tác nước dằn (1-9) kèm thông tin tank, vị trí, phương pháp trao đổi, thông số nước, hệ thống xử lý D-2 và validate khoảng cách/độ sâu.

Watchkeeping Log tuân thủ STCW ghi chi tiết ca trực với thông tin ca, loại (Navigation/Engine), sĩ quan trực, giờ nghỉ ngơi (validate tối thiểu 10h/24h và 77h/7 ngày), thời tiết, tầm nhìn, vị trí, khóa học, tốc độ, trạng thái thiết bị boong lái, GMDSS, sự kiện, bàn giao ca, mức độ mệt mỏi và ký tên. Tất cả sổ nhật ký đều có validation chặt chẽ theo quy định quốc tế và khả năng ký số cho các entry quan trọng.

## 6. Mobile App - Ứng Dụng Di Động Thủy Thủ

### 6.1 Màn hình Đăng Nhập

Màn hình đăng nhập cho phép thủy thủ truy cập vào ứng dụng bằng mã nhân viên và mật khẩu. Màn hình hiển thị địa chỉ máy chủ đang kết nối và có liên kết để cấu hình lại máy chủ nếu cần. Sau khi đăng nhập thành công, thông tin được lưu lại và ứng dụng chuyển sang màn hình chính.

### 6.2 Màn hình Cấu Hình Máy Chủ

Màn hình này cho phép thay đổi địa chỉ máy chủ Edge mà ứng dụng kết nối đến. Thủy thủ có thể nhập địa chỉ URL, dán nhanh từ clipboard, xem hướng dẫn chi tiết và kiểm tra kết nối trước khi lưu. Hệ thống sẽ xác nhận máy chủ hoạt động bình thường trước khi áp dụng thay đổi.

### 6.3 Màn hình Chính

Màn hình chính là trung tâm điều khiển với bốn phần chuyển đổi qua thanh điều hướng: Trang chủ, Công việc, Lịch trình và Hồ sơ. Trang chủ hiển thị lời chào theo thời gian, banner cảnh báo nếu có công việc quá hạn, bốn ô thống kê công việc (đang chờ, quá hạn, đang thực hiện, đã hoàn thành), các nút truy cập nhanh và thông báo đồng bộ dữ liệu. Menu trượt bên trái cho phép truy cập nhanh các chức năng khác.

### 6.4 Màn hình Danh Sách Công Việc

Màn hình này hiển thị tất cả công việc bảo trì được giao qua sáu tab lọc theo trạng thái: Đến hạn, Đang thực hiện, Quá hạn, Cần sửa lại, Chờ phê duyệt và Đã hoàn thành. Có thanh tìm kiếm để lọc công việc theo tên thiết bị. Danh sách tự động làm mới mỗi 30 giây để cập nhật công việc mới. Nhấn vào công việc bất kỳ để xem chi tiết.

### 6.5 Màn hình Chi Tiết Công Việc

Màn hình hiển thị đầy đủ thông tin về một công việc bảo trì gồm tên thiết bị, mức độ ưu tiên, trạng thái, loại công việc, chu kỳ bảo trì, thời hạn và số ngày còn lại. Banner cảnh báo xuất hiện nếu công việc quá hạn hoặc bị từ chối. Phần checklist cho phép xem các bước cần thực hiện. Lịch sử trạng thái hiển thị dòng thời gian thay đổi. Phía dưới có nút để bắt đầu hoặc hoàn thành công việc.

### 6.6 Màn hình Hoàn Thành Công Việc

Form điền thông tin khi thủy thủ thực hiện xong công việc bảo trì. Gồm các phần: tiến độ checklist với danh sách bước cần đánh dấu hoàn thành, phần vật tư cho phép chọn vật tư đã dùng từ kho với số lượng, trường nhập số giờ chạy máy bắt buộc, phần tải ảnh báo cáo (ảnh tự động nén để phù hợp băng thông vệ tinh), và ô ghi chú tự do. Dữ liệu được tự động lưu tạm để tránh mất khi thoát giữa chừng.

### 6.7 Màn hình Cài Đặt

Màn hình tập hợp các tùy chọn cấu hình gồm bảy phần: chọn ngôn ngữ (tiếng Anh, Việt, Tagalog, Hindi), xem thông tin tài khoản, kiểm tra trạng thái đồng bộ và kích hoạt đồng bộ thủ công, thay đổi cấu hình máy chủ, xóa bộ nhớ đệm, xem thông tin ứng dụng, và đăng xuất.

### 6.8 Màn hình Chọn Ngôn Ngữ

Màn hình liệt kê bốn ngôn ngữ được hỗ trợ với cờ quốc gia và dấu kiểm ở ngôn ngữ đang chọn. Khi chọn ngôn ngữ mới, ứng dụng yêu cầu khởi động lại để áp dụng thay đổi. Lựa chọn được lưu lại vĩnh viễn.

### 6.9 Màn hình Hồ Sơ

Màn hình hiển thị thông tin chi tiết thủy thủ gồm năm phần: ảnh đại diện với họ tên và mã nhân viên, thông tin cá nhân (quốc tịch, ngày sinh, cấp bậc, phòng ban), thông tin liên lạc (email, điện thoại, địa chỉ), liên hệ khẩn cấp, giấy tờ (hộ chiếu, sổ hàng hải, thị thực) với cảnh báo nếu sắp hết hạn, và thông tin tuyển dụng. Có thể kéo xuống để làm mới từ máy chủ.

### 6.10 Màn hình Chứng Chỉ

Màn hình hiển thị danh sách chứng chỉ của thủy thủ gồm chứng chỉ STCW chuyên môn, giấy khám sức khỏe, hộ chiếu, sổ hàng hải và thị thực. Mỗi chứng chỉ hiển thị tên, mã số, ngày cấp, ngày hết hạn và trạng thái (có hiệu lực, sắp hết hạn, đã hết hạn). Banner cảnh báo xuất hiện nếu có chứng chỉ cần gia hạn.
