# Kế hoạch Triển khai Hệ thống Đồng bộ Tàu - Bờ (Maritime Sync System)
*Tiêu chuẩn: IMO MSC.428(98) (Cyber Risk), ISO 27001, MARPOL Annex I (Electronic Record Books)*

Tài liệu này chi tiết hóa các bước xây dựng hệ thống đồng bộ dữ liệu tối ưu cho môi trường mạng vệ tinh (VSAT/FBB/Iridium), đảm bảo tính toàn vẹn, bảo mật và tuân thủ pháp lý hàng hải.

## Giai đoạn 1: Chuẩn hóa Database & Tuân thủ (Compliance Foundation)

Mục tiêu: Đảm bảo cấu trúc dữ liệu hỗ trợ định danh duy nhất toàn cầu và tuân thủ quy định về "Nhật ký điện tử" (không được xóa/sửa không vết).

1.  **Định danh & Khóa chính (Identity Management):** [COMPLETED]
    *   **Chuyển đổi PK:** Sử dụng `UUID/Guid` cho các bảng dữ liệu sinh ra phân tán (Tàu & Bờ). (Đã hoàn thành: Migration V2)
    *   **Origin Tagging:** Thêm cột `OriginNode` (e.g., `IMO:9876543`) để biết dữ liệu từ tàu nào. (Đã hoàn thành: Default 'SHIP_01')

2.  **Cơ chế Audit & Immutable Logs (Quan trọng cho MARPOL/SOLAS):**
    *   Với các bảng nhạy cảm (*OilRecordBook, NavigationLog*):
        *   **Cấm Update/Delete vật lý:** Mọi chỉnh sửa phải là một bản ghi mới với `ReferenceId` trỏ về bản ghi cũ và đánh dấu `IsAmendment = true`.
        *   **Digital Signature:** Lưu hash chữ ký số của Thuyền trưởng/Máy trưởng vào bản ghi.

3.  **Cấu trúc bảng `SyncQueue` nâng cao:** [COMPLETED]
    *   Thêm `Priority` (Độ ưu tiên): (Đã cập nhật Enum SyncPriority và DB)
        *   `P1 (Critical)`: Distress Alert, Safety Alarms (Gửi ngay lập tức).
        *   `P2 (Operational)`: Noon Report, Position Data (Gửi theo lịch/Batch).
        *   `P3 (Log/Low)`: Crew Logs, Inventory (Gửi khi băng thông rỗi).

## Giai đoạn 2: Logic "Smart Delta Sync" tại Edge (Tàu) [IN PROGRESS]

Mục tiêu: Tối thiểu hóa dung lượng gửi đi (Byte-saving) và xử lý thông minh.

1.  **Change Tracking thông minh (Override `SaveChanges`):** [COMPLETED]
    *   Tự động phát hiện thay đổi. (Đã implement trong EdgeDbContext)
    *   **Delta Serialization:** Chỉ đóng gói các trường thay đổi (Ví dụ: `{"Status": "COMPLETED"}` thay vì cả object). (Đã implement)
    *   **Data Filtering:** Loại bỏ các trường không cần thiết (như ảnh raw, log debug) khỏi payload đồng bộ. (Đã implement logic bỏ qua IsSynced/UpdatedAt)

2.  **Cơ chế Prioritization & Throttling:** [COMPLETED]
    *   Xây dựng `SyncManager` biết phân loại dữ liệu. (Đã tạo `SyncService.cs`)
    *   Nếu đang dùng **Iridium/Backup Link** (cước phí cao): Chỉ gửi P1. (Đã implement logic `GetAllowedPriorities`)
    *   Nếu đang dùng **VSAT/4G** (cước phí thấp): Gửi P1, P2, P3. (Đã implement logic)

## Giai đoạn 3: Transport Layer & Resilience (Giao vận tin cậy)

Mục tiêu: Đảm bảo dữ liệu đi qua đường truyền "gập ghềnh" mà không bị hỏng hoặc mất.

1.  **Nén & Mã hóa (Compression & Encryption):**
    *   **Compression:** Sử dụng thuật toán **Brotli** (hiệu quả hơn Gzip cho text/JSON).
    *   **Encryption:** Mã hóa AES-256 cho payload trước khi gửi (ngoài lớp HTTPS) để đảm bảo bí mật thương mại.

2.  **Chunking & Resume (Cho File/Report lớn):**
    *   Cắt nhỏ các file đính kèm (PDF, Ảnh sự cố) thành các chunk 512KB.
    *   Cơ chế "Checkpoint": Nếu đứt mạng ở chunk 5, lần sau gửi tiếp từ chunk 6.

3.  **Data Integrity Check:**
    *   Tính **MD5/SHA256 Checksum** cho mỗi gói tin. Server Bờ sẽ tính lại và so sánh, nếu sai lệch 1 bit -> Yêu cầu gửi lại.

## Giai đoạn 4: Shore-Side Processing (Xử lý tại Bờ)

1.  **Conflict Resolution (Xử lý xung đột):**
    *   **Operational Data:** Last Write Wins (Dựa trên `UpdatedAt` từ Tàu - *Lưu ý: Phải đồng bộ thời gian UTC chuẩn*).
    *   **Compliance Data:** Append Only (Không ghi đè, lưu thành phiên bản lịch sử).

2.  **Acknowledgement (ACK):**
    *   Chỉ khi Bờ đã lưu thành công vào DB và verify checksum -> Gửi ACK về Tàu.
    *   Tàu nhận ACK mới được phép xóa/đánh dấu `IsSynced = true` trong `SyncQueue`.

## Giai đoạn 5: Bảo mật & Cyber Risk (IMO MSC.428(98))

1.  **Authentication:**
    *   Sử dụng **mTLS (Mutual TLS)**: Cả Server và Tàu đều phải có chứng chỉ số để xác thực nhau (Chống giả mạo tàu).
2.  **Access Control:**
    *   API chỉ chấp nhận IP từ dải IP vệ tinh đăng ký (Whitelist) nếu có thể.

---
**Checklist kiểm tra:**
- [ ] Đã xử lý trường hợp mất mạng giữa chừng khi đang gửi báo cáo dài?
- [ ] Đã đảm bảo dữ liệu Oil Record Book không bị sửa đổi trái phép?
- [ ] Đã tối ưu chi phí vệ tinh (không gửi rác)?
- [ ] Đã có cơ chế cảnh báo khi đồng bộ thất bại quá 24h?
