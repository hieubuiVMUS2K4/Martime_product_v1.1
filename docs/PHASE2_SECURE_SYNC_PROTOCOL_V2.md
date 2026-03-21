# Phase 2 Secure Sync Protocol v2

## Muc tieu dot khoi dong

Dot khoi dong Phase 2 dua vao he thong mot secure envelope cho cac luong sync Shore - Edge ma khong pha vo flow sync hien co.

## Pham vi da bat dau

- Push Edge -> Shore: `POST /api/sync`
- Heartbeat Edge -> Shore: `POST /api/sync/heartbeat`
- Pull Shore -> Edge: `GET /api/sync/pull`
- Ack Edge -> Shore: `POST /api/sync/acknowledge`

## Header duoc ky

- `X-Sync-Node-Id`
- `X-Sync-Timestamp`
- `X-Sync-Nonce`
- `X-Sync-Key-Version`
- `X-Sync-Content-SHA256`
- `X-Sync-Protocol`
- `X-Sync-Signature`

## Canonical string

Chu ky HMAC-SHA256 duoc tinh tren chuoi:

```text
METHOD
PATH_AND_QUERY
NODE_ID
TIMESTAMP_ISO8601
NONCE
KEY_VERSION
CONTENT_SHA256_HEX
PROTOCOL_VERSION
```

## Cac kiem tra o Shore

- Node phai ton tai trong bang `sync_node_trackers`, duoc `IsRegistered=true`, khong bi revoke va co `SigningKey` hop le.
- Timestamp phai nam trong cua so lech gio cho phep.
- Nonce khong duoc lap lai trong TTL cau hinh.
- `X-Sync-Key-Version` phai khop voi active key hoac previous key con nam trong grace window.
- `X-Sync-Content-SHA256` phai khop body that nhan duoc.
- Chu ky HMAC phai hop le.
- Node trong header phai khop voi node trong payload/query cho push, heartbeat, pull, ack.

## Feature flags

### Shore

- `SyncSecurity:RequireSignedRequests`
- `SyncSecurity:AllowedClockSkewSeconds`
- `SyncSecurity:NonceTtlMinutes`
- `SyncSecurity:ProtocolVersion`

### Edge

- `SyncSecurity:Enabled`
- `SyncSecurity:NodeId`
- `SyncSecurity:SigningKey`
- `SyncSecurity:KeyVersion`
- `SyncSecurity:ProtocolVersion`

## Deployment canh bao

- Shore production compose da duoc noi `SyncSecurity__RequireSignedRequests` va `SyncSecurity__ProtocolVersion` de co the enforce signed sync bang environment variables.
- Edge compose da duoc noi `SyncSecurity__Enabled`, `SyncSecurity__NodeId`, `SyncSecurity__SigningKey`, `SyncSecurity__ProtocolVersion`.
- Cac compose local/prod khong con fallback secret yeu cho JWT, internal API key, database password va pgAdmin password; operator phai dat qua `.env` hoac environment.

## Trang thai hien tai

- Da co request signing o Edge cho push, pull, acknowledge va heartbeat.
- Da co verification middleware o Shore cho push, pull, acknowledge, heartbeat.
- Replay protection hien tai dung memory cache theo nonce TTL.
- Shore da chuyen sang node registry DB-backed tren `sync_node_trackers`.
- Da co API noi bo de provision, rotate key, rollback key, revoke va reactivate node.
- File sync hai chieu da mang them `FileChecksumSha256`, `FileSourceNodeId`, `FileSourcePath`, `FileCapturedAtUtc` va bat buoc verify checksum/provenance truoc khi ghi file tren node nhan.
- Shore da persist audit event vao `audit_logs` cho cac truong hop reject signed sync nhu missing header, sai protocol, clock skew, replay, unknown node, revoked node, payload hash mismatch va invalid signature.
- Shore da normalize ownership inbound theo node da verify, khong con de conflict/persistence tin payload `originNode` nhu mot nguon su that doc lap.
- Rotate key da co version header, previous-key grace window va rollback endpoint de Edge co the doi key ma khong can tat enforcement.

## Viec tiep theo trong Phase 2

1. Chot ownership matrix cap domain cho cac bang nghiep vu phuc tap nhu voyage, maintenance va planning.
2. Mo rong replay registry sang kho ben vung hon neu can scale da instance cho Shore.