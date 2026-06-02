# Hướng Dẫn Sử Dụng & In Ấn Poster Nghiên Cứu Khoa Học (60x160 cm)

Thư mục này chứa mã nguồn thiết kế poster chất lượng cao chuẩn **Báo cáo Nghiên cứu Khoa học & Công nghệ** cho dự án **Hệ Thống Quản Lý Quá Trình Vận Hành Tàu Thủy Thông Minh**.

## 1. Các Cải Tiến Bố Cục & Đồ Họa Đột Phá Mới Nhất
Để khắc phục hoàn toàn các điểm chưa ổn (từ viết tắt gây khó hiểu, CSDL thô sâu về code, giao diện chụp màn hình bị mờ/stacked, và các lỗi dính chữ/font tiếng Việt), chúng tôi đã thực hiện nâng cấp lớn:
* **Kiến trúc Hệ thống (Mục III) Rõ Ràng & Không Viết Tắt:** Việt hóa 100% sơ đồ khối CSS vector. Mọi từ viết tắt kỹ thuật khó hiểu đều được diễn nghĩa sang tiếng Việt rõ ràng, giúp người ngoài ngành dễ dàng thấu hiểu luồng đi của dữ liệu từ Edge lên Shore.
* **Thiết kế Dữ liệu (Mục IV) Trực Quan Hóa:** Loại bỏ hoàn toàn các hộp vẽ CSDL thô đơn điệu và các ký hiệu kỹ thuật sâu (PK, FK, Voyage Records...). Thay thế bằng **Hệ thống thẻ thủy tinh (Glassmorphism Cards Grid)** với 4 phân hệ cốt lõi trang bị các Icon SVG vector cao cấp, giải thích rõ cơ chế đồng bộ vi phân giúp tiết kiệm băng thông vệ tinh chập chờn.
* **Giao diện Mockup Vector (Mục V) Siêu Sắc Nét & Chữ To Rõ Ràng:** Loại bỏ hoàn toàn các tệp ảnh chụp màn hình bị mờ và stacked. Thay thế bằng **Mockup Vector dựng bằng HTML/CSS/SVG thuần** trực tiếp trên Poster:
  - **Mockup Edge (Tàu):** Bản đồ hải đồ SVG trực quan, đồng hồ đo vận tốc, vòng tua, lượng dầu và trạng thái ngoại tuyến (Offline) sắc nét.
  - **Mockup Shore (Bờ):** Trung tâm giám sát trực tuyến và **Hộp thoại Trợ lý AI đàm thoại Tiếng Việt** hiển thị chữ cực kỳ to, rõ ràng, đọc được dễ dàng từ khoảng cách 2 mét.
  - **Mockup Mobile (Crew):** Khung iPhone hiển thị danh sách nhiệm vụ bảo trì và nút chụp ảnh báo cáo sự cố nổi bật.
* **Sửa toàn bộ lỗi Font & Typo Tiếng Việt:** Chuẩn hóa toàn bộ văn bản sang mã Unicode dựng sẵn, sửa lỗi hiển thị dấu và dính chữ (TS., ThS. có dấu cách, hướng dẫn, Tàu - Bờ, đắt đỏ, thông minh, khi mất mạng, Báo cáo chính xác 100%).
* **Đạt sự cân bằng thị giác tuyệt đối (Perfect Column Alignment):** Hai cột đối xứng có chiều cao bằng nhau, kết thúc hoàn hảo tại mép lề dưới của poster.

---

## 2. Các Thành Phần File
* `index.html`: Mã nguồn trang poster chính viết bằng HTML5/CSS3.
* `images/architecture.png`: Sơ đồ kiến trúc xử lý phân tán Tàu - Bờ thực tế.
* `images/demo_edge.png`: Giao diện Dashboard Edge trên tàu (quản lý cục bộ offline).
* `images/demo_shore.png`: Giao diện Dashboard Shore ở bờ (tập trung tích hợp trợ lý AI).
* `images/demo_mobile.png`: Giao diện Mobile App dành cho thuyền viên báo cáo công việc.
* `images/qr_code.svg`: Mã QR Code vector cực nét trỏ đến tài liệu báo cáo đề tài hoặc video chạy thử.

---

## 3. Hướng Dẫn Xem Và Xuất File PDF In Ấn (Một Phát Ra Cả Poster)
1. Nhấp đúp chuột vào file [index.html](file:///f:/NCKH/Product/Martime_product_v1.1/poster_design/index.html) để mở trên trình duyệt Google Chrome hoặc Edge.
2. Sử dụng bảng điều khiển nổi ở góc dưới bên phải màn hình **"Bản Vẽ Poster NCKH"**:
   * **Bật Vùng An Toàn:** Để hiển thị viền đứt màu đỏ (biên an toàn 3 cm). Hãy đảm bảo nội dung chữ và hình ảnh nằm hoàn toàn trong viền đỏ này.
   * **In Poster / Xuất PDF:** Click để mở cửa sổ cấu hình in của hệ thống.
3. Tại bảng cấu hình in:
   * **Máy in đích (Destination):** Chọn **Lưu dưới dạng PDF (Save as PDF)**.
   * **Khổ giấy (Paper size):** Trình duyệt sẽ **TỰ ĐỘNG** nhận dạng kích thước `600mm x 1600mm` từ mã CSS `@page` của poster. 
     * *Lưu ý:* Nếu trình duyệt của bạn không tự nhận, bạn chỉ cần chọn **Khổ giấy tùy chỉnh (Custom)** và điền vào chiều rộng `600` mm, chiều cao `1600` mm.
   * **Lề (Margins):** Chọn **Không có (None)** (Rất quan trọng để poster xuất ra trọn vẹn, tràn viền và không bị chia cắt thành từng trang lẻ A4).
   * **Tùy chọn khác (Options):** Tích chọn ô **Đồ họa nền (Background graphics)** để hiển thị toàn bộ màu sắc, hình nền, bảng so sánh và sơ đồ.
4. Bấm **Lưu (Save)** để xuất file PDF một trang duy nhất siêu nét.

---

## 4. Lưu Ý Quan Trọng Khi Đưa Đi Nhà In (CMYK)
* **Độ phân giải (Resolution):** Poster sử dụng 100% chữ vector và SVG logo, ảnh chụp hệ thống thực tế độ phân giải cao nên file PDF xuất ra sẽ tự động đạt độ mịn tối đa khi in khổ lớn (300 DPI).
* **Hệ màu in ấn CMYK:** Trình duyệt hiển thị mặc định bằng hệ màu RGB. Để màu sắc bạt in ra giống hệt thiết kế trên màn hình máy tính:
  * Sau khi xuất file PDF, bạn nên mở file đó bằng **Adobe Photoshop** hoặc **Adobe Acrobat**.
  * Chọn lệnh chuyển đổi Hệ màu (Color Mode) sang **CMYK Color** (Profile: *U.S. Web Coated (SWOP) v2* hoặc *Coated FOGRA39*).
  * Lưu lại dưới dạng file **PDF/X-1a** hoặc định dạng **TIFF** chất lượng cao để gửi cho nhà in bạt.
