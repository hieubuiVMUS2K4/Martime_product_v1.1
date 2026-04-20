# 🔄 CƠ CHẾ ĐỒNG BỘ DỮ LIỆU TÀU–BỜ — SLIDE TRÌNH BÀY

> **Chủ đề:** Giải pháp kỹ thuật đồng bộ dữ liệu bất đồng bộ trong môi trường hàng hải
> **Ngày:** 17/04/2026

---

> **2.2** — SyncQueue · Store-and-Forward · Delta Sync
>
> **2.3** — Priority · Exponential Backoff · Resume
>
> **2.4** — Metadata-First · SHA-256 · Chunked Transfer
>
> **2.5** — Signed Sync · HMAC · AES-GCM

---

## SLIDE 1 — 2.2 CƠ CHẾ ĐỒNG BỘ BẤT ĐỒNG BỘ

### Offline-First + Store-and-Forward

```
  App Edge SaveChanges
       │
       ▼
  SyncQueue (local DB) ──── 30s ────► Shore Backend
  • Payload = JSON delta              • Idempotency check
  • Priority tag                      • Conflict resolve
  • RetryCount                        • ACK → SyncedAt ✓
       ↑
  Mất mạng → Queue giữ nguyên, tự retry khi có mạng
```

### Delta Sync — tiết kiệm băng thông

$$
O(|\Delta D|) \ll O(|D|)
$$

- Chỉ gửi **phần thay đổi** kể từ lần sync gần nhất
- Mỗi entity có `UpdatedAt` + `SyncVersion` để track

---

## SLIDE 2 — 2.3 ƯU TIÊN & THỬ LẠI

### Ma trận Priority × Kết nối

```
                 🔴 Critical   🟡 Operational   🟢 Low
  Iridium          ✅ Gửi         ⏳ Queue         ⏳ Queue
  VSAT             ✅ Gửi         ✅ Gửi            ⏳ Queue
  4G / WiFi        ✅ Gửi         ✅ Gửi            ✅ Gửi
  Không mạng       ❌ Queue       ❌ Queue          ❌ Queue
```

### Exponential Backoff

$$
t_{\text{wait}}(n) = n^2 + \varepsilon_n \quad \text{(phút)}
$$

- Tránh flood server khi mạng kém
- $\varepsilon_n$ = nhiễu ngẫu nhiên — tránh trùng pha

### Resume Transfer

- Mất kết nối giữa chừng → lưu `resume_token` + `next_chunk_index`
- Khi mạng trở lại → tiếp tục từ đúng vị trí, không truyền lại từ đầu

---

## SLIDE 3 — 2.4 TỐI ƯU TRUYỀN TẢI TỆP

### Metadata-First

```
  Bước 1: Gửi metadata { filename, size, hash }
       │
       ▼
  Shore kiểm tra hash
  ├─ Đã tồn tại → Chỉ cập nhật tham chiếu (KHÔNG truyền file)
  └─ Chưa có    → Gửi nội dung (chunked upload)
```

**Tiết kiệm:** $\eta_f = \frac{C^{\text{direct}} - C^{\text{proposed}}}{C^{\text{direct}}} \times 100\%$ — càng nhiều file trùng, tiết kiệm càng cao

### SHA-256 toàn vẹn

- Tính hash **trước gửi · trong khi truyền · sau khi nhận**
- Phát hiện: thiếu bytes · sai thứ tự chunk · bị sửa đổi
- Phát hiện trùng lặp: cùng hash → bỏ qua, chỉ cập nhật metadata

---

## SLIDE 4 — 2.5 BẢO MẬT KÊNH ĐỒNG BỘ

### Signed Sync (HMAC-SHA256)

```json
{
  "nodeId":      "SHIP-IMO-1234567",
  "timestamp":   "2026-04-16T10:30:00Z",
  "nonce":       "a3f8c2e1...",
  "contentHash": "sha256:abc123...",
  "hmac":        "HMAC-SHA256(nodeId+timestamp+nonce+hash, key)"
}
```

| Rủi ro | Cơ chế phòng thủ |
|---|---|
| Giả mạo nút | `nodeId` + `hmac` |
| Replay attack | `nonce` một lần + `timestamp` TTL |
| Sửa đổi nội dung | `contentHash` SHA-256 |

### AES-256-GCM (AEAD)

- Mã hóa: khóa sync + file nghiệp vụ (chứng chỉ, ảnh, tài liệu)
- Vừa **bí mật** (encryption) vừa **toàn vẹn** (authentication tag)
- Nonce dùng một lần — chống phát lại

### Hỗ trợ thêm

- Key Rotation + Grace Window + Rollback
- Revoke Node · Audit Log từ chối sync
- *(Chưa có: Zero Trust · MFA · mTLS)*
