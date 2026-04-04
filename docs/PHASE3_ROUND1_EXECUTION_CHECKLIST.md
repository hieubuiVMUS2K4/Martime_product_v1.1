# Phase 3 Round 1 Execution Checklist

## Muc tieu

Checklist nay dung cho dot thuc nghiem Phase 3 dau tien tren ban Shore/Edge da deploy va freeze. Thu tu scenario bat buoc la:

1. B1
2. B2
3. B3
4. B4
5. S1
6. S5
7. S6
8. S8
9. O1
10. O2

Checklist nay phai duoc dung cung voi:

- [docs/PHASE3_EXPERIMENT_TEST_MATRIX.md](docs/PHASE3_EXPERIMENT_TEST_MATRIX.md)
- [docs/PHASE3_METRIC_COLLECTION_RUNBOOK.md](docs/PHASE3_METRIC_COLLECTION_RUNBOOK.md)
- [docs/PHASE2_DEPLOY_VERIFY_CHECKLIST.md](docs/PHASE2_DEPLOY_VERIFY_CHECKLIST.md)
- [docs/ONE_DEPLOY_FREEZE_CHECKLIST.md](docs/ONE_DEPLOY_FREEZE_CHECKLIST.md)

## 1. Pre-flight truoc khi vao dot 1

### 1.1. Chot phien ban va moi truong

- [ ] Ghi commit hash Shore, Edge, shared dang deploy.
- [ ] Ghi ngay deploy va nguoi phu trach dot test.
- [ ] Ghi `SYNC_PROTOCOL_VERSION`, `EDGE_SYNC_NODE_ID`, `EDGE_SYNC_KEY_VERSION`.
- [ ] Xac nhan khong co thay doi runtime code sau khi da freeze.

### 1.2. Xac nhan he thong san sang

- [ ] Shore backend dang chay on dinh.
- [ ] Edge backend dang chay on dinh.
- [ ] Node test da duoc provision va `isRegistered = true`.
- [ ] Node test dang `isRevoked = false`.
- [ ] `hasSigningKey = true` va `keyVersion` dung voi Edge.
- [ ] `dotnet ef migrations list` hoac trang thai DB da xac nhan migration moi nhat da san sang/applied theo deployment.

### 1.3. Chuan bi thu muc bang chung

- [ ] Tao thu muc evidence cho dot 1.
- [ ] Chot quy tac dat ten file theo runbook.
- [ ] Bat dau thu Shore log va Edge log.
- [ ] Chup state node security ban dau.

### 1.4. Chuan bi dataset

- [ ] Crew basic dataset san sang cho B1/B2.
- [ ] File sync dataset san sang cho B3/B4 neu can.
- [ ] Backlog data san sang cho O2.

## 2. Thu tu va quy tac chung khi chay

- [ ] Chi chay scenario tiep theo khi scenario hien tai da luu du bang chung.
- [ ] Sau moi scenario, ghi pass/fail va ly do.
- [ ] Sau moi scenario co thay doi node state, khoi phuc node ve state clean truoc khi sang scenario tiep theo.
- [ ] Sau moi scenario, luu: Shore log, Edge log, audit extract, node security snapshot, metric table.

## 3. Dot 1 - Baseline

### B1. Sync hop le tren LAN

Muc tieu: xac nhan signed sync hop le tren moi truong baseline.

- [ ] Dat network profile = LAN.
- [ ] Xac nhan node test dang active va key version dung.
- [ ] Trigger `push/pull/ack` signed theo checklist Phase 2.
- [ ] Ghi lai `lastSignedRequestAt` truoc va sau scenario.
- [ ] Xac nhan khong co `invalid signature`, `payload hash mismatch`, `replay`, `revoked node`.
- [ ] Luu metric P50/P95/P99, throughput, success rate.
- [ ] Danh dau B1 pass neu batch thanh cong va khong co reject bat thuong.

### B2. Sync hop le tren 4G

Muc tieu: do overhead signed sync trong mang thuc te gan bo.

- [ ] Dat network profile = 4G.
- [ ] Lap lai luong B1 tren cung dataset hoac dataset tuong duong.
- [ ] Ghi lai latency va throughput so voi B1.
- [ ] Xac nhan signed sync van thanh cong, khong co reject sai.
- [ ] Luu metric P50/P95/P99, throughput, success rate.
- [ ] Danh dau B2 pass neu sync thanh cong va metric duoc ghi day du.

### B3. Sync hop le tren VSAT

Muc tieu: do resilience va batch sizing duoi RTT cao.

- [ ] Dat network profile = VSAT.
- [ ] Chay batch 10.
- [ ] Chay batch 50.
- [ ] Chay batch 100 neu ha tang cho phep.
- [ ] Ghi lai queue drain time va latency theo batch size.
- [ ] Xac nhan batch nho thanh cong va batch lon neu fail thi co ly do hop le theo mang.
- [ ] Danh dau B3 pass neu thu du du lieu de so sanh batch size va khong vo chain signed sync.

### B4. Offline dai ngay roi hoi phuc

Muc tieu: do eventual consistency va recovery time.

- [ ] Ngat ket noi Edge -> Shore theo profile offline.
- [ ] Tao du lieu pending tai Edge trong thoi gian offline.
- [ ] Ghi so item backlog truoc khi phuc hoi mang.
- [ ] Khoi phuc ket noi.
- [ ] Trigger sync lai.
- [ ] Ghi thoi gian tu luc phuc hoi mang den luc backlog ve 0.
- [ ] Xac nhan Shore nhan batch day du va ACK hoan tat.
- [ ] Danh dau B4 pass neu backlog duoc day het va co time-to-consistency ro rang.

## 4. Dot 1 - Security

### S1. Replay batch cu

Muc tieu: kiem chung anti-replay.

- [ ] Dung mot signed request hop le da luu tu baseline.
- [ ] Gui lai cung nonce/timestamp trong cua so test.
- [ ] Xac nhan Shore reject request replay.
- [ ] Trich `audit_logs` va tim reason `replay_detected`.
- [ ] Luu response code, log, audit extract.
- [ ] Danh dau S1 pass neu replay bi tu choi dung va co bang chung audit.

### S5. Dung node da revoke

Muc tieu: kiem chung revoke enforcement.

- [ ] Revoke node test tren Shore.
- [ ] Chup state node security sau revoke.
- [ ] Trigger sync tu Edge bang node vua bi revoke.
- [ ] Xac nhan Shore tra 403 hoac reject dung theo ky vong.
- [ ] Trich `audit_logs` va log Shore cho scenario nay.
- [ ] Sau khi luu bang chung, reactivate node de chuan bi cho S6.
- [ ] Chup state node security sau activate.
- [ ] Danh dau S5 pass neu node revoked khong sync duoc va node co the khoi phuc ve state active.

### S6. Dung key cu trong grace window

Muc tieu: kiem chung staged rotation khong gay dung sync.

- [ ] Rotate node sang key moi voi `previousKeyGraceMinutes` hop le.
- [ ] Giu Edge dang dung key cu trong grace window.
- [ ] Trigger sync tu Edge.
- [ ] Xac nhan request duoc chap nhan trong grace.
- [ ] Ghi state `previousKeyVersion`, `previousKeyGraceUntil`, `lastSignedRequestAt`.
- [ ] Luu metric transition success rate.
- [ ] Danh dau S6 pass neu key cu van duoc chap nhan trong cua so grace.

### S8. Rollback key co kiem soat

Muc tieu: kiem chung rollback path hoat dong dung.

- [ ] Khi van con trong grace window, goi rollback key tren Shore.
- [ ] Xac nhan `rolledBack = true` va `keyVersion` quay lai muc truoc do.
- [ ] Trigger sync lai bang key cu tu Edge.
- [ ] Xac nhan sync hoi phuc thanh cong.
- [ ] Luu state node security, log, audit neu co.
- [ ] Danh dau S8 pass neu rollback thanh cong va Edge sync lai duoc ma khong tat enforcement.

## 5. Dot 1 - Operations

### O1. Burst read on sync dashboard

Muc tieu: do tac dong observability va rate limiting.

- [ ] Chot endpoint se test: `/api/sync/status` hoac `/api/sync/dashboard/nodes`.
- [ ] Chay burst read voi tai tang dan theo ke hoach team chot.
- [ ] Ghi P95 latency va ti le 429.
- [ ] Xac nhan Shore van on dinh, khong mat kha nang phuc vu sync chinh.
- [ ] Luu metric CPU/RAM neu co.
- [ ] Danh dau O1 pass neu rate limit hoat dong dung va he thong khong sap.

### O2. Sync queue saturation duoi VSAT

Muc tieu: do backlog handling va queue drain time khi hang doi lon.

- [ ] Dat network profile = VSAT.
- [ ] Tao backlog 500 item tai Edge.
- [ ] Ghi queue size ban dau.
- [ ] Trigger sync va theo doi theo tung dot batch.
- [ ] Ghi throughput, queue drain time, reject count neu co.
- [ ] Xac nhan khong mat du lieu va queue giam dan ve 0 hoac muc hop le.
- [ ] Danh dau O2 pass neu backlog duoc xu ly theo nhieu batch ma khong vo signed sync flow.

## 6. Post-run sau khi chay xong dot 1

- [ ] Tong hop bang ket qua cho B1, B2, B3, B4, S1, S5, S6, S8, O1, O2.
- [ ] Danh dau scenario pass/fail.
- [ ] Tong hop P50/P95/P99, throughput, success rate, reject rate, queue drain time.
- [ ] Tong hop audit evidence cho replay, revoke, rotation, rollback.
- [ ] Neu co scenario fail, ghi ro fail do bug runtime hay do test setup.
- [ ] Neu fail do runtime code, dung dot test hien tai theo `ONE_DEPLOY_FREEZE_CHECKLIST`.

## 7. Tieu chi xem la hoan tat dot 1

- [ ] Co day du bang chung cho 10 scenario theo dung thu tu da chot.
- [ ] B1-B4 cho du baseline metric.
- [ ] S1, S5, S6, S8 cho du security evidence.
- [ ] O1-O2 cho du operational metric.
- [ ] Co the dung ngay bo evidence nay de dien chuong thuc nghiem va danh gia.