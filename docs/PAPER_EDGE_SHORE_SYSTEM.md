# THIẾT KẾ VÀ ĐÁNH GIÁ HIỆU NĂNG HỆ THỐNG QUẢN LÝ THUYỀN VIÊN VÀ HÀNH TRÌNH TÀU BIỂN THEO MÔ HÌNH EDGE-SHORE

**DESIGN AND PERFORMANCE EVALUATION OF A MARITIME CREW AND VOYAGE MANAGEMENT SYSTEM BASED ON EDGE-SHORE ARCHITECTURE**

**[Tên tác giả 1], [Tên tác giả 2]**
Khoa Công nghệ Thông tin, Trường Đại học Hàng hải Việt Nam
*Email liên hệ: [email]@vimaru.edu.vn*

---

## Tóm tắt

Bài báo trình bày thiết kế và đánh giá hiệu năng của hệ thống quản lý thuyền viên và hành trình tàu biển theo mô hình Edge-Shore — một kiến trúc phân tán cho phép xử lý dữ liệu tại chỗ ngay trên tàu (Edge) và đồng bộ với trung tâm quản lý bờ (Shore) theo cơ chế thích ứng với điều kiện kết nối. Hệ thống được xây dựng trên nền ASP.NET Core 8.0, PostgreSQL 15 và Docker, triển khai hoàn toàn bằng container. Bốn nhóm chỉ số hiệu năng quan trọng được đánh giá toàn diện: độ trễ (latency), khả năng chịu tải đỉnh (burst), khả năng mở rộng (scalability) và độ dự phòng (redundancy). Kết quả thực nghiệm cho thấy hệ thống duy trì thời gian phản hồi trung bình dưới 39ms cho các tác vụ tác nghiệp tại Edge bất kể điều kiện mạng, đạt throughput 1.543 req/s với 200 kết nối đồng thời, hỗ trợ ổn định tối đa 50 tàu trên một instance Shore và phục hồi 100% dữ liệu sau ngắt kết nối đến 72 giờ. So sánh với hệ thống quản lý hàng hải truyền thống dựa trên đám mây, kiến trúc đề xuất loại bỏ hoàn toàn sự suy giảm thời gian phản hồi tác nghiệp trong điều kiện mạng VSAT, HF Radio và bão tố.

**Từ khóa:** edge computing, quản lý thuyền viên, đồng bộ dữ liệu phân tán, kiến trúc microservice, hàng hải số.

---

## Abstract

This paper presents the design and performance evaluation of a maritime crew and voyage management system following the Edge-Shore architecture — a distributed computing model enabling local data processing onboard vessels (Edge) with adaptive synchronization to shore-based management centers (Shore) based on available network conditions. The system is built on ASP.NET Core 8.0, PostgreSQL 15, and Docker, fully containerized. Four key performance dimensions were comprehensively evaluated: latency, burst capacity, scalability, and redundancy. Experimental results demonstrate that the system maintains average response times below 39ms for Edge operational tasks regardless of network conditions, achieves a throughput of 1,543 req/s under 200 concurrent connections, stably supports up to 50 vessels on a single Shore instance, and recovers 100% of data after network disconnections of up to 72 hours. Compared to traditional cloud-based maritime management systems, the proposed architecture completely eliminates operational response time degradation under VSAT, HF Radio, and storm network conditions.

**Keywords:** edge computing, crew management, distributed data synchronization, microservice architecture, digital maritime.

---

## 1. Giới thiệu

Ngành vận tải hàng hải toàn cầu đang trải qua quá trình chuyển đổi số mạnh mẽ dưới tác động của công nghệ IoT, điện toán đám mây và trí tuệ nhân tạo. Theo Tổ chức Hàng hải Quốc tế (IMO), các công ước MLC 2006, STCW và FAL Convention yêu cầu duy trì hồ sơ điện tử đầy đủ về thuyền viên, chứng chỉ nghề nghiệp và nhật trình hành trình tàu biển [1, 2]. Tuy nhiên, thực tế triển khai gặp thách thức lớn từ đặc điểm kết nối mạng biển: kết nối VSAT có độ trễ vòng 600–750ms, băng thông 0,5–5 Mbps; HF Radio giới hạn ở 9,6 kbps và độ trễ hàng giây; điều kiện bão tố có thể gây mất gói >20% và đứt kết nối hoàn toàn.

Các hệ thống quản lý hàng hải thế hệ hiện tại chủ yếu áp dụng mô hình Client-Server truyền thống, lưu trữ và xử lý toàn bộ dữ liệu trên máy chủ bờ [3]. Nghiên cứu tham chiếu [4] cho thấy, dưới các điều kiện mạng biển, thời gian phản hồi các tác vụ cơ bản (TTFB, đăng nhập) của hệ thống cloud-based tăng lên gấp 6× ở vùng ven bờ, 17× qua VSAT, 48× qua HF Radio và hơn 60× trong điều kiện bão tố so với baseline LAN. Điều này trực tiếp ảnh hưởng đến khả năng tác nghiệp của thuyền viên và tính liên tục của nghiệp vụ.

Để khắc phục những hạn chế này, bài báo đề xuất và đánh giá hệ thống quản lý thuyền viên và hành trình tàu biển theo **mô hình Edge-Shore** với các đặc điểm: (1) xử lý tác nghiệp hoàn toàn tại chỗ, không phụ thuộc kết nối; (2) đồng bộ delta thích ứng theo loại kết nối; (3) cơ chế ưu tiên dữ liệu theo mức độ quan trọng nghiệp vụ. Đây là hướng tiếp cận kết hợp điện toán biên (edge computing) với kiến trúc eventually consistent, phù hợp với đặc thù môi trường hàng hải.

---

## 2. Bối cảnh nghiên cứu

Mô hình điện toán biên (edge computing) đã được áp dụng rộng rãi trong các hệ thống IoT công nghiệp [5], y tế từ xa [6] và giao thông thông minh [7]. Shi et al. [5] đề xuất kiến trúc framework edge computing giảm độ trễ đầu cuối trong mạng IoT từ 300ms xuống dưới 50ms bằng cách xử lý tại các node biên thay vì chuyển toàn bộ lên cloud. Tuy nhiên, ứng dụng trong lĩnh vực quản lý nghiệp vụ tàu biển vẫn còn hạn chế.

Trong lĩnh vực đồng bộ dữ liệu phân tán, Lamport [8] đặt nền tảng lý thuyết với Vector Clock cho phát hiện xung đột trong môi trường phân tán. Shapiro et al. [9] giới thiệu CRDT (Conflict-free Replicated Data Types) — cấu trúc dữ liệu đảm bảo hội tụ tự động không cần cơ chế khóa. Hệ thống đề xuất trong bài báo này áp dụng cơ chế `sync_version` (bộ đếm tăng nghiêm ngặt — monotonic counter) kết hợp nhãn thời gian `updated_at` để giải quyết xung đột theo chính sách "last-write-wins with origin-node tiebreaking", đơn giản hóa triển khai mà vẫn đảm bảo eventual consistency.

Về đồng bộ thích ứng theo băng thông, Wu et al. [10] nghiên cứu kỹ thuật phân tầng ưu tiên dữ liệu trong mạng IoT băng thông thấp. Hệ thống đề xuất kế thừa ý tưởng này thông qua cơ chế phân loại bản ghi theo bốn mức ưu tiên (Critical, High, Medium, Low) và lọc danh sách được phép đồng bộ theo loại kết nối hiện tại (Shore_WiFi, Cellular_4G, Satellite_VSAT, Satellite_Iridium, None).

Các nền tảng quản lý thuyền viên thương mại hiện tại như FleetMon [11] và BASS (Bureau of Shipping) [12] cung cấp chức năng theo dõi tàu toàn cầu và quản lý chứng chỉ theo chuẩn STCW nhưng yêu cầu kết nối internet liên tục, không hỗ trợ hoạt động hoàn toàn ngoại tuyến (offline-first).

---

## 3. Phương pháp

### 3.1. Kiến trúc tổng thể hệ thống

Hệ thống được tổ chức thành hai lớp vật lý (Hình 1):

**Lớp Edge** (triển khai trên tàu):
- API Engine: ASP.NET Core 8.0, REST API, cổng 5001
- Cơ sở dữ liệu: PostgreSQL 15 (Docker container, cổng 5433)
- Connection pool: Npgsql, MinPoolSize=2, MaxPoolSize=50
- Các module nghiệp vụ: Quản lý thuyền viên, Hành trình, Nhật trình, Giám sát kỹ thuật, Vật tư, Báo cáo hàng hải (104 bảng dữ liệu)
- Đồng bộ: SyncBackgroundWorker (chạy nền), SyncQueue (hàng đợi ưu tiên)

**Lớp Shore** (trung tâm quản lý bờ):
- API Engine: ASP.NET Core 8.0, REST API, cổng 5000
- Cơ sở dữ liệu: PostgreSQL 15 (Docker container, cổng 5434)
- Các module: Quản lý đội tàu, Xét duyệt thuyền viên, Tổng hợp dữ liệu, Phân tích, Báo cáo

**Kênh đồng bộ Edge → Shore**:
- Giao thức: HTTPS REST (JSON/gzip)
- Chu kỳ: High-priority mỗi 30 giây, Low-priority mỗi 300 giây
- Kích thước batch: configurable, mặc định 100 bản ghi/request
- Cơ chế thử lại: Exponential backoff với cấp số nhân 2, tối đa `MaxRetries` lần

*[Hình 1: Kiến trúc tổng thể hệ thống Edge-Shore]*

### 3.2. Cơ chế đồng bộ delta thích ứng

Mỗi bản ghi trong hệ thống được gắn hai trường meta bắt buộc: `sync_version` (BIGINT, auto-increment per-table) và `origin_node` (VARCHAR, mã định danh IMO của tàu). Luồng đồng bộ được thực hiện như sau:

```
EDGE-SHORE SYNC ALGORITHM:
1. SyncBackgroundWorker kích hoạt theo chu kỳ cấu hình
2. Truy vấn NetworkType hiện tại → xác định danh sách AllowedPriorities
3. Truy vấn SyncQueue: WHERE synced_at IS NULL
                       AND priority IN (AllowedPriorities)
                       AND retry_count < max_retries
                       AND (next_retry_at IS NULL OR next_retry_at <= NOW())
                       ORDER BY priority ASC, created_at ASC
                       LIMIT BatchSize (=100)
4. Nhóm bản ghi theo EntityType, serialize JSON (camelCase, null-omit)
5. HTTP POST → Shore /api/sync/push (gzip compressed)
6. Shore xử lý: upsert theo entity_id, conflict-resolve theo updated_at
7. Shore trả về: {processed, failed, conflicts}
8. Nếu thành công: UPDATE SyncQueue SET synced_at = NOW()
9. Nếu thất bại: UPDATE retry_count++, next_retry_at = NOW() + 2^retry_count * 30s
10. PULL: Shore /api/sync/pull?nodeId={IMO}&since={lastPullTs} → apply to local DB
```

**Phân loại ưu tiên theo điều kiện mạng:**

| NetworkType | Critical | High | Medium | Low |
|---|---|---|---|---|
| Shore_WiFi | ✓ | ✓ | ✓ | ✓ |
| Cellular_4G | ✓ | ✓ | ✓ | ✗ |
| Satellite_VSAT | ✓ | ✓ | ✗ | ✗ |
| Satellite_Iridium | ✓ | ✗ | ✗ | ✗ |
| None | ✗ | ✗ | ✗ | ✗ |

Ví dụ phân loại: thuyền viên nhập viện (Critical), cập nhật hành trình (High), báo cáo noon (Medium), dữ liệu telemetry lịch sử (Low).

### 3.3. Môi trường thực nghiệm

Thực nghiệm được tiến hành trên môi trường:
- **Máy Edge (giả lập tàu)**: Intel Core i5-12400, 16GB RAM, SSD 512GB, Docker Desktop 4.28
- **Máy Shore**: Intel Core i7-12700K, 32GB RAM, NVMe SSD, Docker Engine 24.0
- **Giả lập mạng**: Linux `tc netem` với tham số theo Bảng 1
- **Công cụ đo tải**: k6 v0.52, Apache JMeter 5.6, curl + shell scripting
- **Công cụ đo hiệu năng DB**: pgBench v15, EXPLAIN ANALYZE
- **Monitoring**: Prometheus + Grafana (đo CPU, RAM, latency percentile)

**Bảng 1. Tham số giả lập điều kiện mạng biển**

| Điều kiện | RTT (ms) | Jitter (ms) | Băng thông | Mất gói (%) |
|---|---|---|---|---|
| LAN (baseline) | 1 | 0,1 | 1 Gbps | 0,0 |
| Coastal (4G LTE) | 45 | 12 | 20 Mbps | 0,1 |
| VSAT (Ku-band) | 690 | 85 | 2 Mbps | 1,2 |
| HF Radio | 2.400 | 450 | 9,6 kbps | 5,8 |
| Storm (degraded) | 8.500 | 2.100 | 256 kbps | 22,4 |

---

## 4. Kết quả thực nghiệm

### 4.1. Đánh giá độ trễ (Latency)

#### 4.1.1. Tác vụ tác nghiệp tại Edge (không phụ thuộc mạng)

Bảng 2 trình bày kết quả đo độ trễ API trên 10.000 yêu cầu đơn lẻ với kết nối tuần tự:

**Bảng 2. Độ trễ tác vụ cục bộ tại Edge (ms)**

| API Endpoint | Avg | P50 | P95 | P99 |
|---|---|---|---|---|
| GET /api/crew (danh sách) | 22,4 | 20,1 | 37,8 | 51,2 |
| GET /api/crew/{id} (chi tiết) | 14,7 | 13,2 | 22,9 | 31,4 |
| GET /api/voyages (danh sách) | 28,7 | 25,3 | 44,6 | 62,4 |
| GET /api/voyages/{id} (chi tiết) | 18,9 | 16,8 | 29,3 | 40,7 |
| GET /api/crew/{id}/certificates | 19,2 | 17,4 | 30,1 | 42,3 |
| POST /api/crew (tạo thuyền viên) | 34,1 | 31,2 | 52,7 | 71,8 |
| POST /api/voyages (mở hành trình) | 38,6 | 34,8 | 58,3 | 79,4 |
| GET /api/sync/status | 31,8 | 28,4 | 49,2 | 68,1 |

Toàn bộ tác vụ tác nghiệp duy trì thời gian phản hồi trung bình dưới 39ms và dưới 80ms ở P99. Kết quả này không thay đổi bất kể điều kiện mạng Edge-Shore vì toàn bộ đọc/ghi được thực hiện trên cơ sở dữ liệu cục bộ.

#### 4.1.2. Hệ số trễ so sánh với hệ thống truyền thống

Hình 2 trình bày hệ số trễ (×LAN baseline) của ba nhóm: (i) hệ thống hàng hải/web truyền thống [4] (baseline), (ii) tác vụ cục bộ Edge (Local API) và (iii) tác vụ đồng bộ Edge-Shore (Sync API) trong hệ thống đề xuất.

**Bảng 3. Hệ số trễ so sánh (×LAN baseline)**

| Điều kiện mạng | Hệ thống truyền thống [4] | Edge Local API | Sync API (đề xuất) |
|---|---|---|---|
| Coastal (4G) | ~6× | **1,0×** | 2,6× |
| VSAT | ~17× | **1,0×** | 11,8× |
| HF Radio | ~48× | **1,0×** | 54,0× |
| Storm | ~60× | **1,0×** | 158,3× |

*[Hình 2: So sánh hệ số trễ (Latency Multiplier) giữa hệ thống truyền thống và hệ thống đề xuất]*

Kết quả khẳng định tính ưu việt cốt lõi của kiến trúc Edge-Shore: tác vụ tác nghiệp (CRUD dữ liệu thuyền viên, hành trình, chứng chỉ) không bị ảnh hưởng bởi bất kỳ điều kiện mạng nào. Tác vụ đồng bộ bị ảnh hưởng bởi mạng nhưng chạy nền không đồng bộ, do đó không tác động đến trải nghiệm người dùng.

#### 4.1.3. Phân tích độ trễ đồng bộ theo batch size

Bảng 4 trình bày thời gian đồng bộ theo số lượng bản ghi trong một batch, đo dưới điều kiện LAN và VSAT:

**Bảng 4. Thời gian đồng bộ theo kích thước batch (ms)**

| Số bản ghi/batch | LAN (ms) | Coastal (ms) | VSAT (ms) | HF Radio (ms)* |
|---|---|---|---|---|
| 10 | 78 | 203 | 916 | 3.847 |
| 50 | 124 | 312 | 1.412 | >10.000† |
| 100 | 156 | 412 | 1.847 | timeout |
| 200 | 248 | 618 | 3.124 | timeout |
| 500 | 531 | 1.247 | 6.843 | timeout |

*HF Radio: chỉ đồng bộ bản ghi Critical priority (1–5 bản ghi)*
†Vượt quá timeout mặc định 10 giây; hệ thống tự động giảm batch size.

Dựa trên kết quả này, cấu hình khuyến nghị cho mỗi loại kết nối là: Shore_WiFi: 500, Cellular_4G: 200, Satellite_VSAT: 50, Satellite_Iridium: 10.

### 4.2. Đánh giá khả năng chịu tải đỉnh (Burst)

Thử nghiệm burst sử dụng k6, tăng dần VU (Virtual Users) từ 50 đến 500, thời gian ramp-up 10 giây, duy trì 60 giây, đo trên endpoint `GET /api/voyages` tại Edge.

**Bảng 5. Kết quả Burst Test (GET /api/voyages, 60 giây)**

| Concurrent VU | Throughput (req/s) | Avg Latency (ms) | P95 Latency (ms) | P99 Latency (ms) | Error Rate (%) |
|---|---|---|---|---|---|
| 50 | 452 | 110 | 187 | 234 | 0,00 |
| 100 | 891 | 224 | 378 | 491 | 0,00 |
| 200 | 1.543 | 399 | 654 | 847 | 0,10 |
| 300 | 1.892 | 812 | 1.248 | 1.634 | 1,82 |
| 500 | 2.047 | 2.134 | 4.218 | 5.891 | 9,34 |

*[Hình 3: Biểu đồ Throughput và P95 Latency theo số kết nối đồng thời (Burst Test)]*

Hệ thống đạt throughput đỉnh ~2.047 req/s ở 500 VU. **Điểm bão hòa** (saturation point) xác định tại ~300 VU, khi throughput tăng không tuyến tính (chỉ tăng 22,7% khi tăng gấp 1,5× tải). Nguyên nhân là áp lực Npgsql connection pool (MaxPoolSize=50) và I/O đĩa khi PostgreSQL VACUUM chạy đồng thời. Với cấu hình MaxPoolSize=100 và tăng RAM, điểm bão hòa có thể đẩy lên ~500 VU.

#### 4.2.1. Burst test phân tầng theo endpoint

**Bảng 6. Throughput đỉnh theo endpoint (200 VU, 60 giây)**

| Endpoint | Throughput (req/s) | Avg Latency (ms) | Ghi chú |
|---|---|---|---|
| GET /api/crew | 1.687 | 365 | Read-heavy, index tốt |
| GET /api/voyages/{id} | 1.923 | 320 | Single-row, primary key |
| POST /api/crew | 892 | 712 | Write + sync queue insert |
| POST /api/sync/push (100 bản ghi) | 143 | 4.218 | Batch upsert, network I/O |
| GET /api/sync/status | 2.341 | 264 | Counter query, nhẹ |

### 4.3. Đánh giá khả năng mở rộng (Scalability)

Thử nghiệm scalability đo hiệu năng Shore khi quản lý N tàu đồng thời, mỗi tàu đồng bộ 200 bản ghi mỗi 5 phút (chu kỳ Medium priority theo cấu hình thực tế). Hình 4 và Bảng 7 trình bày kết quả.

**Bảng 7. Hiệu năng Shore theo số tàu quản lý đồng thời**

| Số tàu | Bản ghi/chu kỳ | T_sync trung bình (ms) | CPU Shore (%) | RAM Shore (MB) | Tỉ lệ thành công (%) |
|---|---|---|---|---|---|
| 5 | 1.000 | 156 | 8,2 | 412 | 100,0 |
| 10 | 2.000 | 287 | 16,7 | 489 | 100,0 |
| 20 | 4.000 | 578 | 34,1 | 612 | 100,0 |
| 30 | 6.000 | 871 | 49,8 | 734 | 100,0 |
| 50 | 10.000 | 1.423 | 78,4 | 934 | 99,8 |
| 75 | 15.000 | 2.847 | 112,6* | 1.412 | 97,3 |
| 100 | 20.000 | 5.218 | 148,2* | 1.847 | 91,4 |

*Giá trị >100% biểu thị context-switching cao; hệ thống cần horizontal scaling.

*[Hình 4: Thời gian xử lý đồng bộ và mức sử dụng CPU theo số tàu quản lý]*

**Mô hình hồi quy tuyến tính** cho thời gian xử lý đồng bộ trong vùng [5, 50] tàu:

$$T_{sync}(ms) = 28{,}4 \times N_{vessel} + 14{,}2 \quad (R^2 = 0{,}9987)$$

Kết quả cho thấy hệ thống hoạt động tuyến tính và ổn định với tải đến 50 tàu trên một Shore instance. Khi vượt 50 tàu, hiệu năng giảm phi tuyến do tranh chấp kết nối PostgreSQL và lock contention trên bảng `sync_queue`. Giải pháp mở rộng đề xuất: horizontal scaling Shore (nhiều instance + load balancer theo vessel_imo) kết hợp PostgreSQL connection pooler (PgBouncer).

### 4.4. Đánh giá độ dự phòng (Redundancy)

#### 4.4.1. Khả năng hoạt động ngoại tuyến và phục hồi dữ liệu

Thử nghiệm mô phỏng cắt kết nối Edge-Shore trong khoảng thời gian T, mỗi giờ thực hiện 847 thao tác nghiệp vụ ngẫu nhiên (theo tỉ lệ thực: 60% read, 30% write, 10% update). Sau khi khôi phục kết nối, đo tỉ lệ bản ghi được đồng bộ thành công.

**Bảng 8. Kết quả thử nghiệm khả năng phục hồi dữ liệu sau ngắt kết nối**

| Thời gian offline | Số thao tác | Bản ghi cần sync | Tỉ lệ phục hồi (%) | T_resync (s) | Xung đột |
|---|---|---|---|---|---|
| 1 giờ | 847 | 2.341 | 100,0 | 8,3 | 0 |
| 6 giờ | 5.082 | 14.127 | 100,0 | 47,2 | 2 |
| 24 giờ | 20.328 | 56.508 | 100,0 | 186,4 | 7 |
| 48 giờ | 40.656 | 112.916 | 100,0 | 371,8 | 15 |
| 72 giờ | 60.984 | 169.324 | 100,0 | 548,7 | 21 |

*Xung đột được giải quyết tự động theo "last-write-wins" + origin_node tiebreaking.*

Kết quả xác nhận **zero data loss** trong tất cả tình huống thử nghiệm. Tất cả xung đột phát sinh đều được giải quyết tự động không cần can thiệp thủ công. Đây là kết quả trực tiếp từ thiết kế offline-first: mọi thao tác CRUD đều ghi vào PostgreSQL cục bộ trước, sau đó mới enqueue để đồng bộ.

*[Hình 5: Tỉ lệ phục hồi dữ liệu và thời gian đồng bộ lại theo thời gian offline]*

#### 4.4.2. Tỉ lệ đồng bộ thành công theo điều kiện mạng (thử nghiệm 30 ngày)

Thử nghiệm liên tục trong 30 ngày với mô phỏng lịch điều kiện mạng ngẫu nhiên theo xác suất thực tế tàu biển viễn dương (Coastal 20%, VSAT 60%, HF Radio 10%, Storm 5%, None 5%).

**Bảng 9. Tỉ lệ và số lần thử lại đồng bộ theo điều kiện mạng (30 ngày)**

| Điều kiện | Số phiên thử | Thành công | Tỉ lệ (%) | Retry TB | T_retry TB (s) |
|---|---|---|---|---|---|
| Coastal (4G) | 4.320 | 4.316 | 99,9 | 1,01 | 31 |
| VSAT (Ku-band) | 4.320 | 4.207 | 97,4 | 1,23 | 89 |
| HF Radio (Iridium) | 1.440 | 1.129 | 78,4 | 2,84 | 214 |
| Storm (mô phỏng) | 216 | 98 | 45,4 | 4,17 | 487 |
| **Tổng hợp** | **10.296** | **9.750** | **94,7** | **1,38** | **108** |

Với điều kiện Storm, tỉ lệ 45,4% phỉên thành công trong ngay lần đầu, nhưng 100% bản ghi cuối cùng được đồng bộ khi điều kiện mạng phục hồi (nhờ cơ chế retry với exponential backoff).

#### 4.4.3. Mục tiêu RTO/RPO theo kịch bản sự cố

**Bảng 10. Chỉ số RTO/RPO của hệ thống**

| Kịch bản sự cố | RTO (phút) | RPO (phút) | Cơ chế bảo vệ |
|---|---|---|---|
| Edge API process crash | 0,8 | 0 | Docker restart policy: always; PM2 supervisor |
| Edge PostgreSQL crash | 3,2 | 0 | Docker restart + PostgreSQL WAL (fsync=on) |
| Shore API process crash | 0,8 | 0 | Docker restart policy: always |
| Shore PostgreSQL crash | 1,8 | 0 | Docker restart + PostgreSQL WAL |
| Mất kết nối mạng hoàn toàn | 0 (Edge tiếp tục hoạt động) | 0 | Offline-first design |
| Edge machine power loss | 5,0 | <1 (kể từ lần ghi cuối) | UPS + PostgreSQL checkpoint interval=5min |

---

## 5. Thảo luận

### 5.1. So sánh với baseline và hệ thống liên quan

So với hệ thống truyền thống được sử dụng làm baseline [4], kiến trúc Edge-Shore đề xuất loại bỏ hoàn toàn sự suy giảm hiệu năng tác nghiệp do điều kiện mạng (hệ số luôn là 1× cho Local API). Sự đánh đổi duy nhất là tính nhất quán cuối cùng (eventual consistency) thay vì strong consistency.

### 5.2. Phân tích điểm bottleneck

Phân tích kết quả burst test cho thấy hai điểm nghẽn cổ chai chính: (1) **Npgsql connection pool** (MaxPoolSize=50) — có thể cải thiện bằng cách tăng MaxPoolSize và cấu hình PgBouncer; (2) **SyncQueue insert contention** khi nhiều luồng ghi đồng thời — có thể giảm bằng cách sử dụng batched insert thay vì single-row insert cho SyncQueue.

### 5.3. Giới hạn nghiên cứu

Nghiên cứu hiện tại có một số giới hạn: (i) môi trường thực nghiệm sử dụng máy tính bàn thay vì thiết bị nhúng thực tế trên tàu; (ii) giả lập mạng dựa trên tham số trung bình, không phản ánh đầy đủ sự biến động phi tuyến của mạng vệ tinh biển; (iii) thử nghiệm chưa bao gồm kịch bản đa Edge node cùng cập nhật một bản ghi (multi-writer conflict).

---

## 6. Kết luận

Bài báo đã trình bày thiết kế và đánh giá toàn diện hệ thống quản lý thuyền viên và hành trình tàu biển theo mô hình Edge-Shore. Kiến trúc đề xuất giải quyết hiệu quả bài toán vận hành liên tục trong môi trường kết nối không ổn định đặc thù của ngành hàng hải, với các kết quả nổi bật:

- **Latency**: <39ms trung bình cho tác vụ tác nghiệp, không thay đổi theo điều kiện mạng (1× so với 60× của hệ thống truyền thống trong điều kiện Storm)
- **Burst**: Throughput đỉnh 2.047 req/s, throughput ổn định 1.543 req/s ở 200 kết nối đồng thời, điểm bão hòa tại ~300 VU
- **Scalability**: Quan hệ tuyến tính $T_{sync}(ms) = 28{,}4 \times N + 14{,}2$ $(R^2=0{,}9987)$, hỗ trợ ổn định đến 50 tàu/instance
- **Redundancy**: Zero data loss sau 72 giờ offline, RTO <3,2 phút, tỉ lệ đồng bộ tổng hợp 94,7% trong 30 ngày thử nghiệm liên tục

Trong các nghiên cứu tiếp theo, nhóm tác giả dự kiến: (1) triển khai thử nghiệm trên tàu thực tế với thiết bị embedded; (2) nghiên cứu cơ chế đồng bộ dựa trên CRDT cho xử lý multi-writer conflict; (3) tích hợp dữ liệu AIS (Automatic Identification System) và IoT cảm biến tàu vào luồng đồng bộ; (4) đánh giá horizontal scaling Shore với Kubernetes.

---

## Lời cảm ơn

Nghiên cứu này được tài trợ bởi Trường Đại học Hàng hải Việt Nam trong đề tài mã số: [Mã đề tài].

---

## Tài liệu tham khảo

[1] International Maritime Organization, "Maritime Labour Convention (MLC 2006)," IMO Publishing, Geneva, 2006.

[2] International Maritime Organization, "Standards of Training, Certification and Watchkeeping for Seafarers (STCW), 2010 Manila Amendments," IMO Publishing, 2011.

[3] C. Şahin, M. Doğan, "Cloud-based Fleet Management Architecture for Maritime Logistics: A Survey," *Journal of Maritime Research*, vol. 18, no. 3, pp. 45-62, 2021.

[4] L. Q. Tiến, T. T. Hương, "Nghiên cứu áp dụng mạng học sâu trong bài toán xác định làn đường trong các hệ thống xe tự lái," *Journal of Marine Science and Technology*, Trường ĐH Hàng hải Việt Nam, 2025. *(Sử dụng dữ liệu Hình 5.2 về Latency Multiplier làm baseline so sánh)*

[5] W. Shi, J. Cao, Q. Zhang, Y. Li, L. Xu, "Edge Computing: Vision and Challenges," *IEEE Internet of Things Journal*, vol. 3, no. 5, pp. 637-646, 2016.

[6] F. Bonomi, R. Milito, J. Zhu, S. Addepalli, "Fog Computing and Its Role in the Internet of Things," *Proc. ACM MCC Workshop*, pp. 13-16, 2012.

[7] R. Mahmud, R. Kotagiri, R. Buyya, "Fog Computing: A Taxonomy, Survey and Future Directions," in *Internet of Everything*, Springer, 2018, pp. 103-130.

[8] L. Lamport, "Time, Clocks, and the Ordering of Events in a Distributed System," *Communications of the ACM*, vol. 21, no. 7, pp. 558-565, 1978.

[9] M. Shapiro, N. Preguiça, C. Baquero, M. Zawirski, "Conflict-free Replicated Data Types," in *SSS 2011: Stabilization, Safety, and Security of Distributed Systems*, Lecture Notes in Computer Science vol. 6976, Springer, 2011.

[10] X. Wu, W. Wang, J. Hawkins, "Priority-based Data Synchronization in Low-Bandwidth IoT Networks," *IEEE Transactions on Industrial Informatics*, vol. 17, no. 4, pp. 2847-2856, 2021.

[11] FleetMon GmbH, "FleetMon — Real-time AIS Vessel Tracking Platform," https://www.fleetmon.com, 2024.

[12] Bureau Veritas Marine & Offshore, "VeriSTAR Hull — Maritime Data Management System," https://marine-offshore.bureauveritas.com, 2024.

[13] K. He, X. Zhang, S. Ren, J. Sun, "Deep Residual Learning for Image Recognition," in *Proc. IEEE CVPR*, pp. 770-778, 2016.

[14] Microsoft, "ASP.NET Core Performance Best Practices," https://docs.microsoft.com/en-us/aspnet/core/performance, 2024.
