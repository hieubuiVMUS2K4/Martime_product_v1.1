## BẢN TỔNG HỢP AUDIT: VẤN ĐỀ, HẬU QUẢ VÀ HƯỚNG KHẮC PHỤC

Ngày rà soát: 2025-10-27

Tài liệu này tập hợp các vấn đề tôi đã rà soát trong repository, mô tả hậu quả tiềm ẩn và hướng khắc phục ưu tiên. Mục tiêu: cung cấp checklist hành động để đưa hệ thống an toàn và ổn định trước khi deploy.

---

## Tóm tắt ngắn

- Tổng quan: kiến trúc tốt (Edge + Shore) nhưng có nhiều vấn đề bảo mật, cấu hình và vận hành cần sửa trước môi trường production.
- Các hạng mục quan trọng: secrets lộ, CORS quá rộng, edge API không có authentication, xung đột cổng DB, thiếu retry/transaction cho sync, thiếu healthchecks, thiếu caching/pagination.

---

## Các vấn đề chi tiết (theo mức độ ưu tiên)

### CRITICAL - Cần fix ngay

1) Hardcoded secrets & weak credentials
   - Vị trí: `backend/appsettings.json`, `edge-services/appsettings.json`, `docker-compose.yml`, nhiều file docs.
   - Hậu quả: lộ khóa JWT, mật khẩu DB/MQTT, rò rỉ quyền truy cập → compromise toàn bộ hệ thống.
   - Khắc phục:
     - Di chuyển tất cả secrets sang environment variables hoặc secret manager (Vault/Azure KeyVault).
     - Thêm `.env.example` (không commit `.env`).
     - Thay JWT key bằng chuỗi ngẫu nhiên >= 32 ký tự.

2) Edge Services thiếu authentication/authorization
   - Vị trí: `edge-services/Program.cs` (không cấu hình authentication) và controllers không require `[Authorize]`.
   - Hậu quả: bất kỳ ai trong LAN có thể đọc/ghi dữ liệu nhạy cảm trên tàu.
   - Khắc phục:
     - Thêm JWT-based authentication cho Edge API.
     - Tạo bảng `Users` + `RefreshTokens` và endpoint login/refresh.
     - Bật [Authorize] trên controller cần bảo vệ.

3) CORS cấu hình quá mở
   - Vị trí: `backend/Program.cs` (AllowAnyOrigin)
   - Hậu quả: Web độc hại có thể gọi API từ trình duyệt của user.
   - Khắc phục: whitelist origin cụ thể (domain/IP), không sử dụng AllowAnyOrigin trong production.

4) Database port / docker network inconsistency
   - Vị trí: root `docker-compose.yml` và `edge-services/docker-compose.yml` có port/ network khác nhau (5432 vs 5433, product-net vs edge-network).
   - Hậu quả: các container không giao tiếp, developer confusion, port conflict trên host.
   - Khắc phục:
     - Chuẩn hoá port mapping; dùng env `EDGE_POSTGRES_PORT` trong một nơi duy nhất.
     - Dùng external docker network hoặc hợp nhất compose files khi cần.

---

### HIGH - Nên fix trước khi production

5) Sync logic thiếu retry/exponential backoff và transaction
   - Vị trí: `edge-services` Sync code (một số model có `RetryCount` nhưng không thấy retry service rõ ràng).
   - Hậu quả: dữ liệu sync thất bại dễ bị bỏ sót hoặc lặp, inconsistent state.
   - Khắc phục:
     - Implement SyncBackgroundService với exponential backoff, max retries, alert nếu quá nhiều failure.
     - Dùng transaction khi cập nhật nhiều bảng liên quan (task status + audit + syncqueue).

6) No health checks & monitoring
   - Vị trí: docker-compose thiếu healthcheck cho backend; code thiếu endpoints health.
   - Hậu quả: Orchestrator không biết service ready → lỗi start ordering.
   - Khắc phục: thêm `/health` endpoint, và healthcheck vào compose.

7) No caching & heavy DB queries, no pagination
   - Vị trí: Frontend fetch all alerts (`/api/vessels/alerts/all...`) và backend chưa dùng Redis cache.
   - Hậu quả: performance degradation, frontend memory crashes on large result sets.
   - Khắc phục: server-side pagination & filtering; cache hot queries in Redis.

8) Dependency/version inconsistencies
   - Vị trí: `frontend/package.json` và `frontend-edge/package.json` dùng khác phiên bản Vite; `frontend` dùng rolldown-vite alias.
   - Hậu quả: build issues, unpredictable dev experience.
   - Khắc phục: thống nhất version, test build pipeline.

---

### MEDIUM - Cần cải thiện

9) Logging & error aggregation thiếu
   - Hậu quả: khó điều tra incidents.
   - Khắc phục: dùng Serilog + Seq/Grafana/Loki; thêm Application Insights or Sentry.

10) No unit/integration tests for critical business logic
    - Khắc phục: thêm xUnit tests cho sync, alert rules, auth.

11) TLS/HTTPS absent for Edge API
    - Hậu quả: traffic nội bộ không mã hoá, MITM risk.
    - Khắc phục: enable HTTPS with self-signed certs on edge devices; mobile pinning.

12) Cleanup jobs running in same thread as collectors (race conditions)
    - Vị trí: `TelemetrySimulatorService` runs deletes during inserts.
    - Khắc phục: separate DataCleanupService; use transactions/isolation.

---

### LOW - Gợi ý/tiện ích

- mDNS/Bonjour for auto-discovery of Edge server from mobile app.
- Implement rate limiting for public shore API.
- Add documented deployment checklist (secrets, migrations, backups).

---

## Prioritized Quick-Fix Plan (tổng quan hành động hàng loạt)

1) Immediate (day 0-2):
   - Move secrets to env; rotate keys. Create `.env.example`.
   - Lock down CORS to dev/staging domains.
   - Add healthcheck endpoint and compose healthchecks.

2) Short term (week 1):
   - Add JWT auth to `edge-services` and protect controllers.
   - Fix DB port/network inconsistencies in compose files.
   - Implement basic Sync retry with max attempts and alerting.

3) Medium term (week 2-3):
   - Add Redis caching and pagination on heavy endpoints.
   - Add structured logging & error tracking.
   - Write unit/integration tests for critical flows.

4) Longer term (week 3+):
   - TLS/HTTPS & certificate pinning for mobile.
   - Load testing & performance tuning.
   - Monitoring dashboards and SLOs.

---

## Ví dụ nhanh: secure secrets

1. Remove `JWT` key from `appsettings.json` and use environment variable `JWT__Key`.
2. Add to `docker-compose.yml` (example):

```yaml
environment:
  - JWT__Key=${JWT__Key}
```

3. Create `.env.example` with placeholder values (do not commit real secrets).

---

## Next steps tôi có thể làm cho bạn

- Tạo PR cụ thể để: (a) trích secrets thành variables, (b) thêm healthchecks, (c) khóa CORS — bạn muốn bắt đầu ở mục nào?
- Hoặc tôi có thể tạo một series PRs (mỗi PR 1 thay đổi nhỏ) và chạy tests/build.

---

Tài liệu này dựa trên rà soát mã nguồn và file cấu hình trong repository ngày 2025-10-27. Nếu bạn muốn tôi tự động thực hiện một hoặc nhiều sửa chữa (ví dụ: di chuyển secrets sang env, thêm endpoint health), hãy chọn hành động đầu tiên và tôi sẽ thực hiện từng bước và mở PR sửa đổi.

-- Kết thúc bản ghi.
