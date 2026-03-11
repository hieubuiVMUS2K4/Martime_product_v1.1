1. Chu trình PMS: Từ Kế hoạch đến Dữ liệu lịch sử
Dựa trên sơ đồ "Quy trình tổng quát PMS", hệ thống của bạn vận hành như sau:

Bước 1: Lập kế hoạch (Văn phòng + Thuyền viên): * Giao diện "Danh mục thiết bị" và "Cấu hình" là nơi định nghĩa các đầu việc bảo trì định kỳ/đột xuất.

Kết quả là các dải màu trên Gantt Chart và Calendar và Kanbanboard. Tại đây, Văn phòng và Máy trưởng sẽ chốt: Làm việc gì? Khi nào làm? Ai làm? Cần vật tư gì?

Bước 2: Thực hiện (Thuyền viên):

Thuyền viên nhìn vào Calendar để biết lịch. Khi thực hiện, họ mở màn hình "Danh sách công việc" để chuyển trạng thái sang "Đang thực hiện".

Bước 3: Giám sát & Kiểm tra (Văn phòng + Thuyền viên):

Khi tàu xong việc, họ điền vào giao diện "Báo cáo công việc".

Điểm mấu chốt: Văn phòng (Shore) sẽ nhận được báo cáo này để "Kiểm tra và phê duyệt". Nếu kết quả đo đạc (trong biên bản BBKT) không đạt, quy trình sẽ quay lại bước thực hiện.

Bước 4: Báo cáo & Thống kê:

Hệ thống tự động lưu vào "Lịch sử bảo trì". Đây là dữ liệu "vàng" để giải trình với Đăng kiểm hoặc chủ tàu.

2. Chu trình Vật tư: Từ Nhu cầu đến ROB (Tồn kho thực tế)
Dựa trên sơ đồ "Quy trình tổng quát Vật tư", logic trong code của bạn cần xử lý:

Bước 1: Yêu cầu vật tư (Tàu lập):

Sĩ quan dùng màn hình "Tạo yêu cầu vật tư".

Logic thông minh: Nếu một PMS ở trên yêu cầu thay thế linh kiện mà kho báo "Hết", hệ thống phải tự động gợi ý tạo phiếu yêu cầu này (Dưới tồn kho tối thiểu).

Bước 2: Mua sắm (Văn phòng làm chủ):

Văn phòng nhận yêu cầu, check lịch trình tàu để chọn cảng nhận hàng phù hợp nhất (tránh phí vận chuyển cao).

Bước 3: Nhập kho (Tàu thực hiện):

Khi hàng về, tàu dùng giao diện "Tạo phiếu nhập kho" để đưa hàng vào các "Vị trí kho" cụ thể.

Bước 4: Quản lý tồn kho (Đồng bộ Bờ - Tàu):

Giao diện "Tồn kho giá trị" giúp văn phòng biết chính xác tài sản trên tàu.

Mối liên kết: Khi báo cáo PMS xong -> Tồn kho tự trừ -> Giá trị tồn kho giảm xuống.

3. "Sợi dây" liên kết giữa 2 Workflow (Cực kỳ quan trọng)
Đây là phần bạn cần chú ý nhất khi nâng cấp module Shore:

PMS tiêu thụ Vật tư - Vật tư phục vụ PMS

Tính dự báo: Khi nhìn vào Gantt Chart của 3 tháng tới, văn phòng phải biết được tổng lượng vật tư cần thiết để chuẩn bị mua sắm tập trung (giúp tiết kiệm chi phí hơn mua lẻ tẻ).

Tính minh bạch: Mọi vật tư xuất kho đều phải gắn với một Mã công việc (Work Order ID). Điều này giúp bạn trả lời câu hỏi: "Tại sao tháng này tàu dùng hết 10 cái lọc dầu?" -> "Vì có 10 đợt bảo trì máy phát điện trong lịch sử".

Số giờ chạy (Running Hours): Giao diện "Counter" sẽ cập nhật giờ chạy máy. Khi giờ chạy đạt ngưỡng, nó sẽ "đánh thức" cả 2 workflow: Vừa tạo việc bảo trì, vừa kiểm tra sẵn sàng vật tư.

Dưới đây là bản diễn giải nghiệp vụ có kèm theo chỉ định File ảnh cụ thể cho từng bước:1. Chu trình PMS: Từ Kế hoạch đến Dữ liệu lịch sử(Dựa trên Workflow tại file: Screenshot 2026-03-10 223637.png)BướcHoạt động nghiệp vụGiao diện tương ứng (File ảnh)B1: Lập kế hoạchĐịnh nghĩa thiết bị, cấu trúc cây và các hạng mục bảo trì định kỳ.Screenshot 2026-03-10 213757.png (Danh mục thiết bị) & image_5202f6.png (Cấu trúc Tree).B1.1: Chốt lịchXem tổng thể tiến độ, tránh chồng chéo việc và dự trù vật tư.Screenshot 2026-03-10 213946.png (Calendar) & Screenshot 2026-03-10 214000.png (Gantt Chart).B2: Thực hiệnThuyền viên nhận việc, kiểm tra danh sách các đầu việc cần làm hôm nay.Screenshot 2026-03-10 213842.jpg (Danh sách công việc - dạng bảng).B3: Giám sátThuyền viên điền thông số, đính kèm biên bản kiểm tra, ảnh chụp thực tế.Screenshot 2026-03-10 213851.png (Báo cáo công việc).B4: Phê duyệtVăn phòng (Shore) kiểm tra báo cáo từ xa. Nếu đạt, dữ liệu chuyển vào lịch sử.Dữ liệu cập nhật ngược lại Screenshot 2026-03-10 213757.png (Cột Trạng thái chuyển sang "Hoạt động").2. Chu trình Vật tư: Từ Nhu cầu đến ROB (Tồn kho thực tế)(Dựa trên Workflow tại file: Screenshot 2026-03-10 223655.png)BướcHoạt động nghiệp vụGiao diện tương ứng (File ảnh)B1: Yêu cầuLập phiếu yêu cầu vật tư khi kho sắp hết hoặc cần cho bảo trì lớn.Screenshot 2026-03-10 213909.png (Tạo yêu cầu vật tư).B2: Quản lý DMKiểm tra thông tin chuẩn của vật tư (Mã, tên, hãng) trước khi mua.Screenshot 2026-03-10 213816.png (Danh mục vật tư).B3: Nhập khoHàng về tàu, thuyền viên kiểm đếm và đưa vào các vị trí lưu trữ.Screenshot 2026-03-10 213921.png (Tạo phiếu nhập kho) & Screenshot 2026-03-10 213829.png (Danh mục vị trí kho).B4: Theo dõiVăn phòng xem tổng giá trị tài sản và số lượng tồn kho thực tế (ROB).Screenshot 2026-03-10 213931.png (Tồn kho vật tư giá trị).3. Các điểm "Chạm" (Integration Points) giữa 2 ModuleĐây là logic quan trọng nhất để Agent AI không làm rời rạc hệ thống của bạn:Từ PMS sang Vật tư: Trong giao diện Báo cáo công việc (Screenshot 2026-03-10 213851.png), khi người dùng mở tab "Vật tư", Agent AI phải gọi dữ liệu từ Danh mục vật tư (Screenshot 2026-03-10 213816.png) để người dùng chọn.Từ Counter sang PMS: Giao diện cập nhật giờ chạy máy (Counter) sẽ tác động trực tiếp lên Calendar (Screenshot 2026-03-10 213946.png). Nếu giờ chạy vượt ngưỡng, ô lịch đó phải tự động hiện màu đỏ hoặc tạo một Work Order mới.Từ Kho sang PMS: Trong Gantt Chart (Screenshot 2026-03-10 214000.png), nếu một công việc sắp diễn ra nhưng vật tư tương ứng trong Tồn kho (Screenshot 2026-03-10 213931.png) bằng 0, hệ thống phải hiện cảnh báo thiếu hụt ngay trên biểu đồ.