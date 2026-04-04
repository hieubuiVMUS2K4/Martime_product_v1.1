# Phase 3 Round 1 Sample Results

## Muc tieu

Tai lieu nay rut gon ket qua Phase 3 Round 1 thanh cac bang tong hop chinh de dua truc tiep vao bao cao. Phien ban nay chi giu lai cau hinh testbed, ket qua cot loi va cac gioi han cua dot thuc nghiem.

## Bang 1. Cau hinh testbed va pham vi Round 1

| Hang muc | Gia tri |
|---|---|
| Shore backend | Local process tai `http://localhost:5000` |
| Edge backend | Local process tai `http://localhost:5001` |
| Shore DB | PostgreSQL local container, `productdb` |
| Edge DB | PostgreSQL local container, `maritime_edge` |
| Node test | `8765432` |
| Protocol version | `2` |
| Key version baseline | `1` |
| Network profile | LAN, 4G-like, VSAT-like, offline/recovery |
| Cong cu emulation | Toxiproxy local proxy tai `localhost:8666` |
| Scenario da chay | `B1`, `B2`, `B3`, `B4`, `S1`, `S5`, `S6`, `S8`, `O1`, `O2` |

## Bang 2. Ket qua baseline va operational

| Scenario | Dieu kien | Chi so chinh | Ket qua | Nhan xet ngan |
|---|---|---|---|---|
| B1 | LAN | Push `2-4` item, heartbeat, pull | Pass | Signed sync hoat dong end-to-end, khong co reject bat thuong |
| B2 | 4G-like | POST `/api/sync` ~ `202.355 ms`, pull ~ `54.113 ms`, throughput ~ `49.4 item/s` | Pass | Batch `10` item van duoc chap nhan on dinh tren profile 4G-like |
| B3 | VSAT-like | Batch `10`: `853.282 ms`; batch `50`: `1204.888 ms`; batch `100`: `1699.162 ms` | Pass | Latency tang theo kich thuoc batch nhung signed sync khong vo flow |
| B4 | Offline/recovery | Backlog `100` item, `time to consistency = 7828.044 ms` | Pass | Backlog duoc giu lai khi mat mang va day het sau khi khoi phuc |
| O1 | Burst read dashboard | `140` request, `29` request `200`, `111` request `429`, `P95 = 27.342 ms` | Pass | Rate limiting observability kich hoat dung, khong co `5xx` |
| O2 | Queue saturation VSAT-like | Backlog `500` item, `5` batch x `100`, `queue drain = 32074.701 ms`, throughput ~ `15.589 item/s` | Pass | Hang doi lon duoc giai phong het, khong mat du lieu |

## Bang 3. Ket qua security va nhan xet tong hop

| Scenario | Muc tieu | Bang chung chinh | Ket qua |
|---|---|---|---|
| S1 | Kiem chung anti-replay | Lan 1 `200`, replay `409`, co `audit_logs` voi `replay_detected` | Pass |
| S5 | Kiem chung revoke path | Node revoke xong bi tu choi `403`, activate lai thanh cong | Pass |
| S6 | Kiem chung grace window | Rotate sang key version `2`, key cu version `1` van duoc chap nhan trong grace | Pass |
| S8 | Kiem chung rollback key | `rolledBack = true`, key version quay lai `1`, signed request duoc chap nhan lai | Pass |

## Bang 4. Ket qua file-flow live sau khi sua sync metadata/FK

| Scenario | Dataset | Chi so chinh | Ket qua | Nhan xet ngan |
|---|---|---|---|---|
| F1 - Large image preprocess | `crew_certificate`, anh goc `1023525 B` | Trigger `2403.58 ms`, file Shore `943138 B`, giam `80387 B` (~`7.85%`), `is_preprocessed = true`, profile `image-optimized` | Pass | Path preprocess/compress hoat dong end-to-end sau khi doi `recordKey` sang `CertificateNumber` va bo sung crew business identifiers |
| F2 - Small file bundle | `3 x health_document` PDF (`14648 B`, `16696 B`, `18744 B`) | Trigger `254.64 ms`, tong du lieu Shore `50088 B`, `3/3` manifest verified, `edgeUploadChunkSessionCount = 0`, `shoreUploadChunkSessionCount = 0` | Pass | Bundle path dong bo on dinh voi metadata-first/file verification; khong can chunk session cho tap nho |
| F3 - Delta transfer | `travel_document`, file `1572896 B` | Baseline trigger `362.64 ms`, modified trigger `302.01 ms`, `7` chunk, `is_delta_session = false`, `changedChunkCount = 0` | Pass co ghi chu | Scenario thanh cong ve mat dong bo tep, nhung dot do nay chua kich hoat duoc block-delta thuc su ma dang fallback ve full upload |

### Nhan xet tong hop

- Round 1 da hoan tat day du chuoi `B1 -> B4 -> S1 -> S5 -> S6 -> S8 -> O1 -> O2`.
- Bo file-flow live run `campaign-rerun-20260322-fileflow-v5` da bo sung bang chung thuc te cho preprocess anh lon, small-file path va delta-path.
- Lop secure sync giu dung hanh vi duoi baseline, adverse network emulation, replay, revoke, grace rotation, rollback, burst observability va queue saturation.
- Dot nay da du de dua vao phan thuc nghiem cua bao cao voi tu cach bang chung Round 1.
- Gioi han hien tai:
  - network emulation moi bao gom latency, jitter va bandwidth co ban
  - packet loss fidelity chua du sat moi truong bien thuc te
  - O1 moi do burst tu mot client, chua mo rong thanh multi-client concurrent load
  - F3 hien moi xac nhan fallback full upload; can mot dot rieng de ep block-delta path va ghi nhan `requested_chunk_indexes_json` khac `null`
