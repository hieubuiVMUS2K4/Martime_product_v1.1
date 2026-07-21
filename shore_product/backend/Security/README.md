# Security — Xác thực nội bộ, ký request Sync, mã hoá dữ liệu nhạy cảm

## Mục đích

Thư mục này KHÔNG chứa xác thực JWT chính (đó là cấu hình `AddAuthentication().AddJwtBearer(...)` trong `Program.cs`) — nó chứa 3 mảnh bảo mật bổ sung, chuyên biệt cho quan hệ Shore↔Edge: (1) một policy ủy quyền nội bộ dùng cho các endpoint quản trị/giám sát, (2) middleware xác thực chữ ký HMAC cho toàn bộ request `/api/sync/*` (chống giả mạo tàu, chống replay), và (3) dịch vụ mã hoá AES-GCM để lưu trữ khoá ký/tệp nhạy cảm ở trạng thái nghỉ (at rest).

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `InternalAccessRequirement.cs` | `IAuthorizationRequirement` rỗng — chỉ là "thẻ đánh dấu" để gắn vào policy `"InternalAccess"` khai báo trong `Program.cs` |
| `InternalAccessHandler.cs` | `AuthorizationHandler<InternalAccessRequirement>` — logic thật quyết định policy `InternalAccess` có pass hay không |
| `SyncRequestVerificationMiddleware.cs` | `IMiddleware` xác thực chữ ký HMAC-SHA256 + chống replay + kiểm tra node đã đăng ký cho các request `/api/sync/*` |
| `DataEncryptionService.cs` | `IDataEncryptionService` — mã hoá/giải mã AES-GCM 1 chuỗi hoặc 1 mảng byte, hỗ trợ nhiều "phiên bản khoá" (key rotation) đồng thời |

## Luồng hoạt động chính

### Policy `InternalAccess` (dùng bởi nhiều Controller — xem `Controllers/README.md`)

```
[Authorize(Policy = "InternalAccess")]
        │
        ▼
InternalAccessHandler.HandleRequirementAsync:
  1. Security:RequireInternalAccess == false (MẶC ĐỊNH trong cả appsettings.json
     VÀ appsettings.Development.json) → context.Succeed() NGAY LẬP TỨC, bỏ qua mọi
     kiểm tra bên dưới
  2. (nếu true) User đã authenticate bằng JWT (bất kỳ role nào) → Succeed
  3. (nếu true) Môi trường Development VÀ request đến từ loopback (localhost) → Succeed
  4. (nếu true) Header "X-Internal-Api-Key" khớp cấu hình "InternalAccess:ApiKey" → Succeed
  5. Không thoả điều kiện nào → Fail (mặc định của AuthorizationHandler)
```

### Xác thực chữ ký request Sync (`SyncRequestVerificationMiddleware`)

```
Program.cs:
  app.UseWhen(
      context => SyncRequestVerificationMiddleware.IsProtectedSyncRequest(context.Request),
      branch => branch.UseMiddleware<SyncRequestVerificationMiddleware>());
        │  (chỉ áp dụng cho danh sách cố định các route /api/sync/* — POST /, /heartbeat,
        │   /acknowledge, GET /pull, và toàn bộ /file-*)
        ▼
InvokeAsync:
  0. SyncSecurity:RequireSignedRequests == false (MẶC ĐỊNH trong cả 2 file appsettings)
     → next(context) NGAY LẬP TỨC, bỏ qua toàn bộ bước dưới
  1. (nếu true) Đọc 7 header bắt buộc: X-Sync-Node-Id, X-Sync-Timestamp, X-Sync-Nonce,
     X-Sync-Key-Version, X-Sync-Signature, X-Sync-Content-SHA256, X-Sync-Protocol
     — thiếu bất kỳ header nào → 401
  2. Kiểm tra protocol version khớp cấu hình, timestamp trong khoảng lệch giờ cho phép
     (mặc định 300 giây)
  3. Chống replay: tra cứu "sync-nonce:{nodeId}:{nonce}" trong IMemoryCache — đã tồn tại
     → 409 (KHÔNG dùng bảng SyncNonceRegistry ở Services/Sync/ — xem ghi chú quan trọng)
  4. Tra SyncNodeTracker theo NodeId — chưa đăng ký (IsRegistered=false) → 401;
     đã bị thu hồi (IsRevoked=true) → 403
  5. Giải mã khoá ký của node (qua IDataEncryptionService) — ưu tiên khoá hiện tại
     (KeyVersion), chấp nhận khoá TRƯỚC ĐÓ nếu còn trong "grace period"
     (PreviousKeyGraceUntil) để hỗ trợ xoay khoá không gây gián đoạn
  6. So khớp SHA-256 của body request với header X-Sync-Content-SHA256
  7. Dựng "canonical string" = Method + Path+Query + NodeId + Timestamp + Nonce +
     KeyVersion + ContentHash + Protocol, nối bằng "\n"
  8. Tính HMAC-SHA256(canonical, sharedKey) — so khớp (constant-time) với
     X-Sync-Signature — sai → 401
  9. Ghi nonce vào cache (chống replay), cập nhật SyncNodeTracker
     (LastSignedRequestAt, LastAcknowledgedKeyVersion...) → next(context)
```

Xoay khoá ký của từng node (`SigningKey`/`PreviousSigningKey`/`KeyVersion`/`PreviousKeyGraceUntil`) được thao tác qua `Controllers/SyncDashboardController.cs` (`PUT/POST .../security`, `.../rollback-key`, `.../revoke`, `.../activate`).

### Mã hoá dữ liệu nhạy cảm (`DataEncryptionService`)

Dùng **AES-GCM** (mã hoá xác thực — vừa mã hoá vừa chống chỉnh sửa) cho 2 trường hợp: (a) chuỗi (`Encrypt`/`Decrypt`, ví dụ `SyncNodeTracker.SigningKey`/`PreviousSigningKey` lưu trong DB), và (b) mảng byte (`EncryptBytes`/`DecryptBytes`, dùng bởi `Services/Sync/SyncFileStorageService.cs` cho file trong các thư mục nhạy cảm: avatar, chứng chỉ, tài liệu thuyền viên). Hỗ trợ **nhiều phiên bản khoá cùng lúc** (`DataProtection:EncryptionKeys` — dictionary theo version) để xoay khoá mã hoá mà không làm hỏng dữ liệu đã mã hoá bằng khoá cũ (mỗi payload tự ghi kèm version khoá đã dùng để mã hoá nó).

## Liên kết với phần khác

- `InternalAccessHandler` được đăng ký làm `IAuthorizationHandler` trong `Program.cs`, gắn với policy `"InternalAccess"` — policy DUY NHẤT thật sự được dùng trong toàn bộ `[Authorize(Policy = ...)]` của codebase (xem `Controllers/README.md`).
- `SyncRequestVerificationMiddleware` được đăng ký `AddScoped` rồi gắn có điều kiện qua `app.UseWhen(...)` trong `Program.cs`, đứng TRƯỚC `UseAuthentication()`/`UseAuthorization()` trong pipeline.
- `IDataEncryptionService` được `Services/Sync/SyncFileStorageService.cs` (mã hoá file) và `Security/SyncRequestVerificationMiddleware.cs` (giải mã khoá ký lưu trong `SyncNodeTracker`) sử dụng.
- Đọc `Services/Sync/README.md` để hiểu vì sao chống-replay ở đây (dùng `IMemoryCache`) lại tách biệt với bảng `SyncNonceRegistry` được xây riêng cho mục đích tương tự.

## Ghi chú khi đọc/dạy

- **Theo cấu hình mặc định hiện tại, CẢ HAI cơ chế bảo mật trong thư mục này đều tắt.** `appsettings.json` VÀ `appsettings.Development.json` đều đặt `Security:RequireInternalAccess = false` và `SyncSecurity:RequireSignedRequests = false`. Nghĩa là: mọi `[Authorize(Policy = "InternalAccess")]` hiện luôn pass, và mọi request tới `/api/sync/*` hiện không cần chữ ký HMAC nào. Đây là điều **quan trọng nhất** cần biết trước khi đánh giá mức độ an toàn của hệ thống hoặc trước khi demo/triển khai thật — bật 2 cờ này lên là bước bắt buộc trước khi đưa ra môi trường không tin cậy.
- **Chống replay có 2 cơ chế song song, KHÔNG dùng chung dữ liệu.** `SyncRequestVerificationMiddleware` dùng `IMemoryCache` (theo từng instance Shore, mất khi restart, không hoạt động đúng nếu chạy nhiều instance sau load balancer). `Services/Sync/SyncNonceRegistryService.cs` (bảng DB `sync_nonce_registry`, xem `Models/Sync/README.md`) được xây riêng để giải quyết đúng vấn đề đó nhưng **chưa được middleware này gọi tới** — đã xác minh bằng cách tìm toàn bộ backend. Đừng nhầm tưởng 2 cơ chế đang phối hợp với nhau.
- **`InternalAccessHandler` không kiểm tra ROLE nào cả**, kể cả khi `RequireInternalAccess=true` — bất kỳ user đã đăng nhập (JWT hợp lệ) với bất kỳ role nào đều pass. Nếu cần phân quyền theo role thật sự (ví dụ chỉ `Admin` mới được xoay khoá ký), phải tự thêm logic, `InternalAccessHandler` hiện không làm việc đó.
- **`X-Internal-Api-Key` là "cửa sau" luôn khả dụng** khi `RequireInternalAccess=true` — bất kỳ ai có đúng giá trị cấu hình `InternalAccess:ApiKey` đều qua được policy mà không cần JWT. Cần bảo vệ giá trị cấu hình này như một secret thật sự.
- **`ResolveAcceptedSigningKey` chấp nhận CẢ khoá hiện tại lẫn khoá liền trước** (trong thời gian `PreviousKeyGraceUntil` còn hiệu lực) — đây là thiết kế cố ý để xoay khoá không làm gián đoạn tàu đang dùng khoá cũ, nhưng cũng nghĩa là ngay sau khi xoay khoá, cả 2 khoá đều còn hợp lệ trong một khoảng thời gian — không phải bug nếu thấy request ký bằng khoá "cũ" vẫn được chấp nhận ngay sau khi rotate.
- Khi debug lỗi "sync request bị từ chối", `RejectAndAuditAsync` luôn ghi kèm 1 dòng vào `AuditLogs` (qua `IAuditService`, từ `Services/CrewManagement/`) với `entityType="SyncSecurity"` — đây là nơi đầu tiên nên tra khi cần biết chính xác lý do bị từ chối (`reasonCode`: `missing_headers`, `unsupported_protocol`, `invalid_timestamp`, `clock_skew`, `invalid_key_version`, `replay_detected`, `unknown_node`, `revoked_node`, `missing_node_key`, `payload_hash_mismatch`, `invalid_signature`).
