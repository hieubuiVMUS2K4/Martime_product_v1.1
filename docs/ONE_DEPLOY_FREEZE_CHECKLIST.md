# One-Deploy Freeze Checklist

## Muc tieu

Checklist nay dung khi muon deploy 1 lan cho dot thuc nghiem nghien cuu khoa hoc, sau do freeze code va env de thu metric lap lai duoc.

## 1. Dieu kien pre-deploy bat buoc

- Phase 1 hardening da chot va build lai thanh cong cho Shore va Edge.
- Phase 2 runtime da chot: signed sync, replay protection, payload hash, file checksum/provenance, verified node binding, revoke/reactivate, key rotation grace window, rollback key.
- Migrations Shore DB da san sang apply tren moi truong test.
- `.env`, compose va reverse proxy da du gia tri that, khong con placeholder.

## 2. Truoc khi an nut deploy

- Chot branch/code commit dung de lam ban thuc nghiem.
- Ghi lai hash commit Shore, Edge va shared library vao notebook nghien cuu.
- Chot bo env se dung cho dot test: internal key, signing key, key version, protocol version.
- Xac nhan khong con ke hoach sua them logic runtime trong dot test.

## 3. Deploy

- Deploy Shore.
- Apply Shore migration.
- Deploy Edge.
- Provision node security state tren Shore.
- Cau hinh Edge dung `EDGE_SYNC_NODE_ID`, `EDGE_SYNC_SIGNING_KEY`, `EDGE_SYNC_KEY_VERSION`.

## 4. Verify ngay sau deploy

- Chay [docs/PHASE1_DEPLOYMENT_CHECKLIST.md](docs/PHASE1_DEPLOYMENT_CHECKLIST.md).
- Chay [docs/PHASE2_DEPLOY_VERIFY_CHECKLIST.md](docs/PHASE2_DEPLOY_VERIFY_CHECKLIST.md).
- Luu log xac minh ban dau va snapshot node security state.

## 5. Freeze window

- Khong doi code Shore, Edge, shared trong suot dot test metric.
- Khong doi env, signing key, key version, protocol version neu khong co bien co bat buoc.
- Neu buoc phai doi key de test rotation, ghi ro moc thoi gian va tach thanh mot scenario rieng.

## 6. Du lieu can thu thap trong dot thuc nghiem

- Log Shore va Edge cua tung scenario.
- `audit_logs` cho invalid signature, replay, revoked node va reject khac.
- Node security state truoc va sau tung scenario.
- Metric latency, throughput, rejection rate va recovery time.

## 7. Tieu chi de khong phai redeploy lai

- Moi bug phat hien chi thuoc test data, config test hoac kịch ban van hanh, khong phai loi runtime code.
- Moi scenario nghien cuu deu chay tren cung mot ban code va env da freeze.
- Mọi thay doi sau do chi la tai lieu, bang ket qua, do thi va phan tich.

## 8. Neu bat buoc pha freeze

- Neu phai sua code Shore/Edge/runtime config, dung dot test metric hien tai.
- Tang version dot thuc nghiem.
- Deploy lai va chay lai toan bo baseline verification truoc khi tiep tuc thu metric.