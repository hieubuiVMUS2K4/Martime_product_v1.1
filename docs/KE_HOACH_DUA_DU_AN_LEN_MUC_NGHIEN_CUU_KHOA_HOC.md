# KE HOACH DUA DU AN LEN MUC NGHIEN CUU KHOA HOC

> Phien ban: 1.0  
> Ngay tao: 2026-03-21  
> Pham vi: Toan bo he thong Maritime Shore - Edge  
> Muc tieu: Dua du an tu muc prototype chuc nang len muc de tai nghien cuu khoa hoc co the bao ve, do luong va cong bo ket qua

---

## 1. Muc tieu tong the

Du an hien da co nen tang nghiep vu va dong bo du lieu giua tau va bo kha day du, nhung chua dat muc mot de tai nghien cuu khoa hoc vi con thieu 4 lop quan trong:

1. Kien truc bao mat xuyen suot va co the chung minh.
2. Giao thuc dong bo an toan, chong gia mao, chong replay va chong sua du lieu.
3. Bo khung kiem thu va danh gia thuc nghiem de luong hieu qua ky thuat.
4. Bo tai lieu hoc thuat va san pham dau ra phuc vu luan van, bao cao, bai bao.

Muc tieu cua ke hoach nay la dua he thong hien tai thanh mot mo hinh nghien cuu co ten goi ro rang, co cau hoi nghien cuu, co giai phap ky thuat, co bo thi nghiem va co ket qua dinh luong.

---

## 2. Dinh huong de tai nghien cuu de nghi

### 2.1. Ten de tai goi y

"Thiet ke va danh gia kien truc Edge - Shore bao mat cho he thong quan ly doi tau voi dong bo bat dong bo, tu cau hinh node va kha nang chiu loi mang"

### 2.2. Bai toan nghien cuu trung tam

Lam the nao de xay dung mot he thong quan ly doi tau theo mo hinh Edge - Shore, trong do:

- Edge tren tau van hoat dong duoc khi mat ket noi dai ngay.
- Du lieu van duoc dong bo bat dong bo len Shore khi co mang.
- Moi node tau duoc dinh danh va xac thuc an toan.
- Du lieu dong bo khong bi gia mao, replay, sua doi hoac ghi de sai.
- He thong van duy tri duoc hieu nang chap nhan duoc tren cac dieu kien mang thuc te nhu VSAT, 4G, Shore WiFi.

### 2.3. Cau hoi nghien cuu can tra loi

1. Mo hinh trust nao phu hop cho he thong shore - edge trong moi truong hang hai phan tan?
2. Giao thuc dong bo nao dam bao authenticity, integrity, idempotency va resilience trong dieu kien mang khong on dinh?
3. Chi phi bao mat bo sung anh huong den do tre, ty le thanh cong va thong luong dong bo den muc nao?
4. He thong de xuat co uu diem gi hon mot mo hinh dong bo HTTP thong thuong khong co xac thuc node manh?

### 2.4. Gia thuyet nghien cuu goi y

Neu he thong edge - shore duoc bo sung mot lop node identity, secure sync protocol, secret management va bo khung danh gia thuc nghiem, thi co the dat duoc dong thoi 3 muc tieu:

- Bao mat hon so voi mo hinh sync thong thuong.
- Van giu duoc kha nang hoat dong offline-first.
- Chi tang overhead o muc chap nhan duoc trong van hanh doi tau.

---

## 3. Danh gia hien trang xuat phat

### 3.1. Nhung thanh phan da co va co the tan dung cho de tai

- Nen tang Shore - Edge da ton tai va da tach duoc hai mien van hanh.
- Da co co che sync queue, inbox, outbox, retry, idempotency, background worker.
- Edge da co auth service, lockout, session validation, audit logging, rate limiting.
- Shore da co RBAC policy declaration, sync dashboard, quan ly nhieu module nghiep vu.
- Da co nhieu du lieu va quy trinh thuc te: crew, certificates, voyage, reports, maintenance, ship data.

### 3.2. Cac diem yeu can bien thanh muc tieu nghien cuu

- Shore chua enforce authorization dong bo tren nhieu API.
- Sync channel chua co mutual authentication va message signing bat buoc.
- Secrets con hardcode trong appsettings va .env.
- Transport giua cac thanh phan con dung HTTP o nhieu noi.
- Frontend con luu token theo cach de bi khai thac neu co XSS.
- Chua co bo test khoa hoc de chung minh kha nang chong tan cong, chiu loi va hieu nang.

### 3.3. Tuyen bo trung thuc nen dung trong bao cao

He thong hien tai la prototype chuc nang manh ve nghiep vu va resilient sync, nhung chua phai mot kien truc cyber-secure edge computing da duoc chung minh bang thuc nghiem. Ke hoach phia duoi se chuyen he thong tu prototype thanh san pham nghien cuu.

---

## 4. Nguyen tac thuc hien

1. Khong viet lai he thong tu dau. Tan dung toi da codebase hien co.
2. Uu tien fix cac diem anh huong den gia tri nghien cuu truoc, khong chay theo tinh nang moi.
3. Moi phase phai co deliverable ky thuat, deliverable hoc thuat va tieu chi dong phase.
4. Moi thay doi bao mat deu phai co metric do luong, khong chi mo ta cam tinh.
5. Muc tieu cuoi cung khong chi la chay duoc, ma la giai thich duoc vi sao thiet ke nay hop ly.

---

## 5. Lo trinh tong the theo 3 giai doan

Ke hoach duoc rut gon thanh 3 giai doan dung theo truc anh da chot. Neu lam tap trung, co the trien khai trong khoang 12 den 16 tuan.

| Giai doan | Trong tam | Muc tieu | Thoi luong goi y |
|---|---|---|---|
| 1 | Hardening nen tang | Dua he thong ve muc secure engineering co ban | 3-4 tuan |
| 2 | Secure sync protocol | Bien dong bo hien tai thanh kenh sync co xac thuc va integrity | 4-6 tuan |
| 3 | Chuan hoa thanh nghien cuu khoa hoc | Tao threat model, testbed, metric va ket qua danh gia | 4-6 tuan |

---

## 6. Giai doan 1 - Hardening nen tang

### 6.1. Muc tieu

Khoa cac lo hong co ban de he thong khong con o muc prototype chuc nang, ma dat duoc secure baseline co the dua vao bao cao nghien cuu.

### 6.2. Pham vi cong viec dung theo muc tieu da chot

- Khoa toan bo API Shore theo policy that, bo cac AllowAnonymous tam thoi.
- Bat HTTPS-only cho Shore va Edge, bo HTTP trong cau hinh trien khai.
- Dua secret ra environment variables hoac secret store, xoa khoa cung trong appsettings.
- Bo sung rate limiting cho Shore, khoa sync dashboard va cac endpoint quan sat.

### 6.3. Cong viec chi tiet theo nhom

#### Shore backend

- Ra soat toan bo controllers va actions, lap bang endpoint auth matrix.
- Chuyen cac endpoint dang tam mo sang Authorize hoac policy phu hop.
- Khoa cac endpoint quan sat nhu sync dashboard, health chi tiet, node overview, logs.
- Them rate limiting cho auth, sync, file upload, dashboard va API tong hop.
- Thay CORS rong bang danh sach origin cu the.
- Bat HTTPS redirection, HSTS va cau hinh reverse proxy production.

#### Edge backend

- Chuyen transport sang HTTPS-only trong cau hinh production.
- Tach token signing key, DB password, MQTT credentials, API key ra environment hoac secret store.
- Ra soat middleware auth va danh sach endpoint duoc bypass.
- Hardening file upload, static file va log output.

#### Frontend Shore va Edge

- Kiem tra cach luu token va session state.
- Giam phu thuoc vao localStorage doi voi token nhay cam neu co the.
- Bo sung cac bien phap giam rui ro XSS va session misuse.
- Chuan hoa hanh vi logout, timeout va forced re-auth cho thao tac nhay cam.

#### Van hanh va tai lieu

- Viet checklist secure deployment cho dev, staging, production.
- Chot baseline truoc va sau hardening de phuc vu so sanh hoc thuat.
- Cap nhat tai lieu kien truc voi trust boundary ban dau.

### 6.4. Deliverable

- Secure baseline cho Shore va Edge.
- Bang endpoint auth matrix truoc va sau khi sua.
- Bo cau hinh production khong con hardcoded secrets.
- Tai lieu deployment toi thieu.

### 6.5. Tieu chi hoan thanh

- Khong con endpoint quan trong anonymous ma khong co ly do nghiep vu.
- Khong con secret production nam trong repo.
- Shore va Edge co the mo ta la HTTPS-first trong production.
- Co so sanh ro rang truoc va sau hardening.

---

## 7. Giai doan 2 - Thiet ke secure sync protocol

### 7.1. Muc tieu

Day la trong tam ky thuat cua de tai: bien co che dong bo hien tai thanh mot giao thuc dong bo an toan, co dinh danh node, co integrity, co anti-replay va co the mo ta nhu mot dong gop nghien cuu.

### 7.2. Pham vi cong viec dung theo muc tieu da chot

- Cap danh tinh rieng cho tung tau hoac node edge.
- Dung mTLS hoac request signing bang khoa rieng tung node.
- Bat buoc timestamp, nonce, payload hash, file checksum, replay detection cho moi goi sync.
- Rang buoc ACK, pull, push vao identity cua node thay vi chi dua vao nodeId trong query hay body.

### 7.3. Kien truc de nghi

Moi node Edge phai co mot identity rieng, duoc Shore quan ly. Moi request sync phai mang du cac thanh phan sau:

- Node identity.
- Timestamp.
- Nonce duy nhat.
- Payload hash.
- File checksum neu co file kem theo.
- Signature hoac TLS client certificate.
- Protocol version.

### 7.4. Cong viec chi tiet theo nhom

#### Identity va node onboarding

- Tao bang quan ly node tren Shore: node id, vessel id, status, key status, revocation status.
- Thiet ke quy trinh node enrollment va rotate key hoac certificate.
- Chot cach dinh danh node: mTLS hay request signing.

#### Shore side

- Them middleware hoac endpoint filter xac thuc sync request theo identity cua node.
- Xac minh timestamp va cua so thoi gian hop le.
- Luu nonce cache hoac replay registry voi TTL.
- Bat buoc payload hash verification, khong con optional integrity check.
- Rang buoc push, pull, ack, heartbeat voi node identity da xac thuc.
- Xac minh file checksum truoc khi ghi file.

#### Edge side

- Sinh request sync duoc ky bang khoa rieng cua node hoac su dung client certificate.
- Ky ca push, pull, acknowledge va heartbeat.
- Tach retry nghiep vu va retry bao mat de khong tai su dung nonce sai cach.
- Cap nhat log va audit trail cho cac su kien invalid signature, replay reject, revoked node.

#### Data governance trong sync

- Chot ownership matrix cho nhung bang quan trong: crew, certificates, reports, voyage, maintenance.
- Cap nhat conflict rules de gan voi ownership thay vi merge mac dinh.
- Them lineage, correlation id, causation id neu can cho truy vet.

### 7.5. Deliverable

- Tai lieu Secure Sync Protocol v2.
- Sequence diagram cho push, pull, acknowledge, heartbeat.
- Bo code xac thuc node, verify integrity, replay detection.
- Bang so sanh he thong cu va he thong moi.

### 7.6. Tieu chi hoan thanh

- Node khong hop le khong the sync.
- Replay batch cu bi tu choi.
- Payload bi sua doi bi phat hien.
- ACK khong the gia mao chi bang nodeId thong thuong.
- File sync co checksum va provenance ro rang.

---

## 8. Giai doan 3 - Chuan hoa thanh nghien cuu khoa hoc

### 8.1. Muc tieu

Tu mot he thong da duoc hardening va co secure sync protocol, chuyen thanh mot mo hinh edge-shore sync co bao mat duoc kiem chung bang threat model va bo thi nghiem dinh luong.

### 8.2. Pham vi cong viec dung theo muc tieu da chot

- Viet threat model: nghe len, gia mao node, replay, sua payload, lo secret, chiem token client.
- Thiet ke bo thi nghiem: sync qua mang yeu, mat ket noi, replay batch cu, sua file, gia tau khac.
- Do metric: ty le phat hien tan cong, ty le tu choi replay, do tre sync, overhead bao mat, kha nang phuc hoi sau gian doan.
- Tu do chung minh day la mo hinh edge-shore sync co bao mat duoc kiem chung, khong chi la phan mem quan ly tau.

### 8.3. Cong viec chi tiet

#### Threat model va mo hinh hoa he thong

- Ve trust boundary diagram cho Shore, Edge, user, network, database, file storage.
- Liet ke asset can bao ve: du lieu crew, voyage reports, file scan, secret, token, node identity.
- Liet ke attacker va kha nang tan cong: network attacker, compromised node, malicious insider, token thief.
- Xac dinh cac loai tan cong chinh va cach he thong doi pho.

#### Testbed va kich ban thi nghiem

- Tao testbed gom 1 Shore va 2-5 Edge nodes mo phong nhieu tau.
- Tao profile mang: Shore WiFi, 4G, VSAT, mat mang tung dot.
- Tao dataset benchmark cho crew, certificate, voyage, report va file sync.
- Xay dung bo kich ban:
  - Sync binh thuong.
  - Offline dai ngay roi hoi phuc.
  - Replay batch cu.
  - Sua payload sau khi ky.
  - Sua file checksum.
  - Gia mao node khac.
  - Dung node da bi revoke.

#### Metric va danh gia

- Do ty le sync thanh cong va time to consistency.
- Do P50, P95, P99 sync latency.
- Do replay rejection rate, invalid signature rejection rate.
- Do message overhead theo byte va theo phan tram.
- Do CPU, memory overhead tren Edge va Shore.
- Do thoi gian phuc hoi sau mat mang va sau su co tam thoi o Shore.

#### Dong goi hoc thuat

- Viet chuong thiet ke kien truc de xuat.
- Viet chuong thi nghiem va danh gia.
- Chot bang so sanh voi baseline ban dau.
- Chuan bi slide va demo scenario ngan gon.

### 8.4. Deliverable

- Threat model day du.
- Test matrix va benchmark dataset.
- Bang ket qua thuc nghiem va do thi tong hop.
- Noi dung cho chuong giai phap, cai dat, thuc nghiem va ket luan.

### 8.5. Tieu chi hoan thanh

- Moi claim bao mat deu co metric hoac bang chung kem theo.
- Co the lap lai thi nghiem nhieu lan de lay ket qua tin cay.
- Co bang so sanh baseline truoc hardening va sau secure sync protocol.
- Co the mo ta he thong nhu mot mo hinh edge-shore sync co bao mat duoc kiem chung.

---

## 9. Backlog chi tiet theo nhom cong viec

### 9.1. Nhom Shore backend

- Dong bo hoa auth policy tren controller va action.
- Them rate limiting cho auth, sync, dashboard, upload.
- Them HTTPS-first va cau hinh production an toan.
- Them sync auth middleware hoac endpoint filter.
- Them replay cache, signature verification va file checksum verification.
- Them node registry, revocation va binding request voi node identity.
- Refactor conflict resolver theo ownership matrix.

### 9.2. Nhom Edge backend

- Tach secret khoi appsettings.
- Tich hop request signing hoac client certificate.
- Ho tro onboarding, rotate key, revoke key.
- Ky push, pull, acknowledge, heartbeat.
- Bo sung audit trail cho security event va sync event.

### 9.3. Nhom Frontend Shore

- Xay dung auth layer day du neu can demo truy cap co kiem soat.
- Tao man hinh node management, revocation, sync security status.
- Tao dashboard metric, incident view va monitoring view.

### 9.4. Nhom Frontend Edge

- Giam cach luu token de bi khai thac.
- Hien thi tinh trang node identity, key status, config version.
- Hien thi canh bao replay reject, invalid signature, revoked node.

### 9.5. Nhom Tai lieu va hoc thuat

- Threat model.
- Trust boundary diagram.
- Secure sync sequence diagram.
- Ownership matrix va conflict rules.
- Test matrix, benchmark report va chuong danh gia.

---

## 10. Timeline goi y 14 tuan

| Tuan | Trong tam |
|---|---|
| 1-4 | Giai doan 1 - Hardening nen tang |
| 5-9 | Giai doan 2 - Secure sync protocol |
| 10-14 | Giai doan 3 - Chuan hoa thanh nghien cuu khoa hoc |

Neu tien do cham, co the keo dai len 16 tuan, nhung van giu dung 3 giai doan tren.

---

## 10.1. Cach chot 1 lan deploy cho nhanh ma van dung logic nghien cuu

Neu muc tieu la giam so lan deploy Shore xuong 1 lan chinh, thi can tach ke hoach thanh 2 nhom moc:

### Nhom A - Bat buoc hoan tat truoc khi deploy

Day la cac moc lam thay doi hanh vi runtime cua he thong. Neu chua xong thi deploy som se lam ket qua thuc nghiem mat gia tri hoac phai deploy lai:

- Toan bo secure baseline cua Giai doan 1 cho Shore va Edge.
- Toan bo secure sync protocol cua Giai doan 2 co anh huong den runtime: signed request, node registry, replay protection, payload hash, file checksum, provenance, revoke/reactivate, signed heartbeat, signed pull/push/ack.
- Migrations va schema can thiet cho Shore DB.
- Cau hinh production/staging da noi du env cho HTTPS, internal access, sync security.
- Cac thay doi con lai cua Phase 2 neu muon giu dung 1 lan deploy:
  - Ownership matrix duoc chuyen thanh conflict rule runtime.
  - Key rotation co version/ACK/rollback an toan neu xem day la mot phan bat buoc cua protocol nghien cuu.

### Nhom B - Lam sau deploy, khong nen doi hoan tat roi moi deploy

Day la cac moc nghien cuu va danh gia. Chung can he thong da deploy on dinh de sinh ra bang chung, nhung ban than chung khong phai la ly do de tri hoan deploy:

- Threat model, trust boundary diagram, sequence diagram.
- Test matrix, benchmark dataset, kich ban tan cong va kich ban mang.
- Chay test lap lai de lay metric P50, P95, P99, replay rejection rate, invalid signature rejection rate.
- Thu thap log, audit trail, bang so sanh baseline va bieu do tong hop.
- Dong goi chuong thiet ke, chuong thuc nghiem, slide va demo scenario.

### Nguyen tac ra quyet dinh

- Neu mot moc thay doi code runtime, schema, env, compose, reverse proxy, auth policy, sync protocol hoac conflict rule, thi moc do thuoc Nhom A va phai xong truoc lan deploy duy nhat.
- Neu mot moc chi tao bang chung, tai lieu, metric hoac ket qua do luong tu he thong da chay, thi moc do thuoc Nhom B va phai lam sau deploy.

### Ket luan de deploy 1 lan

Muong deploy 1 lan cho nhanh thi khong nen dat muc tieu "hoan thanh toan bo Giai doan 3 roi moi deploy". Cach dung la:

1. Chot xong tat ca moc Nhom A.
2. Deploy Shore va Edge 1 lan.
3. Freeze code va env trong suot dot thuc nghiem.
4. Hoan thanh toan bo Nhom B tren ban deploy da freeze de lay ket qua nghien cuu co the lap lai.

Noi cach khac, deploy 1 lan la hop ly neu va chi neu code cua Giai doan 1 va Giai doan 2 da duoc dong bang truoc khi vao dot thi nghiem.

---

## 10.2. Trang thai thuc te de quyet dinh deploy 1 lan

### Da hoan thanh o muc pre-deploy

- Giai doan 1 secure baseline cho Shore va Edge.
- Secure sync signed request cho push, pull, acknowledge, heartbeat.
- Replay protection, payload hash verification va file checksum/provenance verification.
- DB-backed node registry, revoke/reactivate, signed heartbeat, audit event cho sync reject.
- Verified node binding cho inbound Shore runtime thay vi tin payload `originNode` nhu nguon su that doc lap.
- Versioned key rotation voi previous-key grace window va rollback endpoint.
- Checklist deploy xac minh cho Phase 1, Phase 2 va checklist one-deploy freeze.

### Van con la moc sau deploy, khong phai blocker cua lan deploy duy nhat

- Threat model, trust boundary diagram va sequence diagram dung cho bao cao.
- Test matrix, benchmark dataset, kich ban tan cong va kich ban mang.
- Bang ket qua thuc nghiem, do thi tong hop va phan tich metric.
- Chuong giai phap, cai dat, thuc nghiem, ket luan va slide bao ve.

### Da khoi dong ngay sau deploy de vao Phase 3

- Da chot threat model va trust boundary tai `docs/PHASE3_THREAT_MODEL_AND_TRUST_BOUNDARY.md`.
- Da chot bo test matrix va benchmark profile tai `docs/PHASE3_EXPERIMENT_TEST_MATRIX.md`.
- Da chot runbook thu thap metric va bang chung tai `docs/PHASE3_METRIC_COLLECTION_RUNBOOK.md`.

Ba tai lieu nay danh dau moc bat dau chinh thuc cua Phase 3 theo huong post-deploy, khong doi them runtime code tru khi phat hien loi nghiem trong lam vo hieu bang chung thuc nghiem.

### Dieu kien de ra quyet dinh deploy 1 lan ngay

- Build Shore va Edge pass tren commit da chot.
- Migrations Shore da san sang apply.
- Env/compose khong con placeholder cho moi truong test that.
- Team dong y freeze code sau deploy va chi thu thap bang chung nghien cuu tren ban deploy do.

Neu 4 dieu kien tren duoc dap ung, co the deploy 1 lan va chuyen trong tam sang Giai doan 3 ma khong can tiep tuc doi code runtime.

---

## 11. Tieu chi nghiem thu cuoi cung cua de tai

Du an duoc xem la dat muc nghien cuu khoa hoc khi thoa man dong thoi cac dieu kien sau:

1. Co cau hoi nghien cuu va giai phap ky thuat ro rang.
2. Co implementation that tren codebase hien tai, khong chi la mo ta y tuong.
3. Co bo thi nghiem lap lai duoc va metric dinh luong.
4. Co so sanh truoc va sau khi ap dung co che de xuat.
5. Co tai lieu va lap luan du de bao ve truoc hoi dong.

---

## 12. Thu tu uu tien neu can lam tung dot

Neu khong the lam tat ca cung luc, thu tu uu tien nen la:

1. Khoa cac API quan trong va bo secrets ra khoi repo.
2. Hoan thien secure sync protocol.
3. Them node identity va enrollment flow.
4. Tao testbed va metric.
5. Moi sau do moi mo rong them nhieu dashboard va tinh nang phu.

Ly do: 3 buoc dau tao gia tri nghien cuu lon nhat. Neu khong co chung, de tai de bi danh gia la mot he thong CRUD + sync thong thuong.

---

## 13. Ket luan thuc thi

Huong di dung cho du an nay khong phai la tiep tuc them tinh nang nghiep vu ngay lap tuc, ma la chot mot truc nghien cuu ro rang xung quanh secure maritime edge-shore synchronization. Codebase hien tai da du manh de lam nen tang. Viec can lam la chuan hoa, hardening, do luong va dong goi thanh mot cau chuyen hoc thuat co bang chung.

Neu thuc hien theo lo trinh tren, du an co the di tu muc prototype van hanh len muc:

- Co gia tri khoa hoc.
- Co tinh moi ky thuat.
- Co the bao ve duoc.
- Co kha nang tach thanh bai bao hoac mo rong thanh san pham thuc te.