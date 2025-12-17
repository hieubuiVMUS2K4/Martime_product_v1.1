# Project Rules & Guidelines

## 1. Maritime Standards & Compliance (Quy chuẩn Hàng hải)
- **Tuân thủ tuyệt đối:** Mọi tính năng phải tuân theo quy định của IMO (SOLAS, MARPOL, STCW, ISM Code).
- **Tính toàn vẹn dữ liệu (Data Integrity):** 
  - Các bút toán nhật ký (Log entries) sau khi ký (signed) là **bất biến**. 
  - Không được phép xóa vật lý (Hard Delete). Nếu sai, phải tạo bút toán sửa sai (Correction Entry) tham chiếu đến bút toán cũ.
- **Thuật ngữ:** Sử dụng chính xác thuật ngữ hàng hải (VD: dùng "Draft" thay vì "Depth", "Port/Starboard" thay vì "Left/Right").

## 2. Database & Performance
- **Hiệu năng:** Tối ưu hóa query cho dữ liệu lớn (Time-series data). Sử dụng Index hợp lý cho các trường hay query (Date, SyncedStatus).
- **Migrations:** Mọi thay đổi cấu trúc DB phải thông qua EF Core Migrations. Tuyệt đối không sửa schema thủ công trên production.
- **Offline-First:** Hệ thống phải hoạt động 100% khi mất kết nối. Cơ chế đồng bộ (Sync) phải xử lý xung đột dữ liệu dựa trên `origin_node` và `timestamp`.

## 3. API Design
- **Single Responsibility:** Không tái sử dụng endpoint API cho các chức năng nghiệp vụ khác nhau. Tách biệt Controller rõ ràng (VD: `DeckLogbookController` riêng, `EngineLogbookController` riêng).
- **Validation:** Validate dữ liệu chặt chẽ tại Server-side trước khi lưu xuống DB.
- **Standard:** Sử dụng chuẩn RESTful, Async/Await cho mọi tác vụ I/O.

## 4. UI/UX Design (Industrial Maritime Style)
- **Phong cách Công nghiệp:** 
  - **Không bo tròn góc (No border-radius)**. Thiết kế vuông vức, sắc cạnh.
  - **Mật độ thông tin cao (High Density):** Không để thừa khoảng trống vô ích. Tối ưu hóa không gian màn hình.
- **Hiển thị:** 
  - Font chữ rõ ràng, độ tương phản cao (dễ đọc trong điều kiện ánh sáng yếu hoặc chói).
  - Hỗ trợ Dark Mode (Night Mode) cho giao diện trên buồng lái (Bridge).
- **Trải nghiệm:** Tối ưu cho thao tác nhanh, ít click chuột.

## 5. Code Quality
- **Clean Code:** Tuân thủ SOLID principles.
- **Logging:** Ghi log đầy đủ các hành động quan trọng (đặc biệt là các thao tác ghi/sửa nhật ký).