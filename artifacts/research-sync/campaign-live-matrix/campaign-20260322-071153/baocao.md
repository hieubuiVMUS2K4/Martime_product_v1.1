## Tong hop thuc nghiem bao mat va dong bo

Tai lieu nay tong hop toan bo ket qua thuc nghiem da chay va da co artifact doi chieu trong dot lam viec hien tai. Thay vi chi tach rieng tung nhom ket qua, ban tong hop nay dua chung ve mot tai lieu duy nhat de co the chen truc tiep vao chuong “Thuc nghiem va danh gia” cua bao cao. Bon nhom bang chung duoc dua vao gom: (i) secure sync Round 1 de kiem chung cac co che bao mat tren kenh Edge - Shore; (ii) campaign dong bo dinh luong moi nhat tren ba profile `LAN`, `4G`, `VSAT`; (iii) dot file-flow live sau khi sua metadata/FK va logic ghi nhan ket qua sync; va (iv) benchmark metadata-first voi corpus `1000` file.

## 1. Moi truong va phuong phap thuc nghiem

Cum thuc nghiem su dung Shore backend local tai `http://localhost:5000`, Edge backend local tai `http://localhost:5001`, PostgreSQL rieng cho Shore va Edge, va Toxiproxy lam lop mo phong mang tai `localhost:8666`. Edge duoc cau hinh dong bo qua proxy nay de heartbeat, push sync, pull sync va acknowledge deu di qua lop mo phong mang truoc khi toi Shore. Cac artifact record-sync moi nhat duoc luu tai `artifacts/research-sync/campaign-live-matrix/campaign-20260322-071153`, trong khi artifact file-flow live duoc luu tai `artifacts/research-sync/campaign-live-matrix/campaign-rerun-20260322-fileflow-v5`. Benchmark metadata-first duoc luu tai `artifacts/research-sync/metadata-first-benchmark-1000`.

Ve bao mat, secure sync su dung signed request tren cac endpoint dong bo voi cac thanh phan xac thuc gom `nodeId`, `timestamp`, `nonce`, `keyVersion`, `content hash` va `HMAC signature`. Ve dong bo ban ghi, he thong duoc do bang benchmark queue tren `sync_queue`, trong do moi batch benchmark duoc gan `runId` rieng de tach biet voi queue van hanh thong thuong. Trong campaign record-sync moi nhat, harness chi tiep tuc retry khi benchmark queue thuc su giam hoac so ban ghi benchmark da sync tang len, nham tranh do “spin time” vo ich.

## 2. Nguon bang chung da tong hop

| Nhom ket qua | Artifact chinh | Vai tro trong bao cao |
|---|---|---|
| Secure sync Round 1 | `docs/PHASE3_ROUND1_SAMPLE_RESULTS.md`, `docs/THUC_NGHIEM_VA_DANH_GIA_ROUND1.md` | Tong hop baseline, security va operational scenario |
| Record-sync campaign moi nhat | `campaign-20260322-071153/thesis-summary.csv`, `campaign-20260322-071153/campaign-summary.csv` | Bang thong ke dinh luong theo mang va quy mo du lieu |
| File-flow live | `campaign-rerun-20260322-fileflow-v5/summary.json` | Bang chung preprocess, bundle path va delta fallback |
| Metadata-first benchmark | `metadata-first-benchmark-1000/summary.json` | Danh gia kich thuoc payload metadata so voi inline full-content |

## 3. Cau hinh profile mang da dung

| Profile | Pham vi su dung | Cach ap dung | Thong so chinh |
|---|---|---|---|
| LAN | Record-sync campaign, secure sync baseline | Khong them toxic | Duong truyen baseline, khong gioi han bo sung |
| 4G | Record-sync campaign, secure sync 4G-like | `latency` hai chieu | Upstream: `23 ms`, jitter `6 ms`; Downstream: `22 ms`, jitter `6 ms` |
| VSAT | Record-sync campaign, secure sync VSAT-like | `latency` + `bandwidth` hai chieu | Upstream: `345 ms`, jitter `43 ms`, rate `250`; Downstream: `345 ms`, jitter `42 ms`, rate `250` |
| OFFLINE | Secure sync baseline B4 | `timeout` | Cat ket noi downstream trong `60000 ms` |

## 4. Tong hop thuc nghiem bao mat Round 1

### 4.1. Muc tieu

Cum thuc nghiem bao mat duoc dung de kiem chung rang signed sync khong chi ton tai tren thiet ke ma thuc su phong ve duoc cac de doa cot loi: replay, node revoke, key rotation grace va rollback key. Ngoai ra, baseline va operational scenario duoc chay kem de xac nhan rang lop bao mat khong lam vo dong bo trong dieu kien mang bat loi va backlog lon.

### 4.2. Bang ket qua bao mat va van hanh lien quan

| Nhom | Scenario | Muc tieu | Bang chung chinh | Ket qua |
|---|---|---|---|---|
| Baseline | B1 | Xac nhan signed sync hop le tren LAN | Push, heartbeat va pull thanh cong end-to-end | Pass |
| Baseline | B2 | Do overhead signed sync tren 4G-like | POST `/api/sync` xap xi `202.355 ms`, throughput ~ `49.4 item/s` | Pass |
| Baseline | B3 | Kiem tra signed sync tren VSAT-like | Batch `10`: `853.282 ms`; `50`: `1204.888 ms`; `100`: `1699.162 ms` | Pass |
| Baseline | B4 | Kiem chung eventual consistency sau offline | Backlog `100` item, `time to consistency = 7828.044 ms` | Pass |
| Security | S1 | Kiem chung anti-replay | Lan 1 `200`, replay `409`, `audit_logs` co `replay_detected` | Pass |
| Security | S5 | Kiem chung revoke node | Node da revoke bi tu choi `403`, activate lai thanh cong | Pass |
| Security | S6 | Kiem chung grace window khi rotate key | Key cu version `1` van duoc chap nhan trong grace sau khi rotate sang version `2` | Pass |
| Security | S8 | Kiem chung rollback key co kiem soat | `rolledBack = true`, key version quay lai `1`, signed request hop le tro lai | Pass |
| Operations | O1 | Kiem chung burst read tren dashboard | `140` request, `111` request bi `429`, `P95 = 27.342 ms`, khong co `5xx` | Pass |
| Operations | O2 | Kiem chung queue saturation duoi VSAT-like | Backlog `500` item, `queue drain = 32074.701 ms`, throughput ~ `15.589 item/s` | Pass |

### 4.3. Nhan xet tu cum bao mat

Ket qua Round 1 cho thay secure sync da dap ung dung cac muc tieu bao mat cot loi trong pham vi dot danh gia dau tien. Anti-replay hoat dong dung, node revoke duoc enforce, grace rotation khong lam gian doan dong bo, va rollback key co the thuc hien co kiem soat. Dong thoi, signed sync van giu duoc baseline hop le tren LAN, 4G-like, VSAT-like va trong tinh huong offline/recovery. Nhu vay, cum bang chung nay xac nhan lop bao mat khong phai chi la claim kien truc ma da duoc kiem chung bang test scenario co audit trail.

## 5. Tong hop campaign dong bo dinh luong moi nhat

### 5.1. Cau hinh campaign

Campaign dong bo dinh luong moi nhat duoc chay tren ba profile `LAN`, `4G`, `VSAT` voi so lan lap tuong ung `10`, `10`, `15`. Moi profile duoc danh gia tren bon muc quy mo du lieu `100`, `1000`, `5000`, `10000` ban ghi. Ket qua tong hop duoc sinh ra tu `thesis-summary.csv`, trong khi `campaign-summary.csv` luu ket qua chi tiet tung repetition. Toan bo `12` to hop kich ban trong dot nay deu co `success = True` o muc repetition-level.

### 5.2. Bang tong hop ket qua dong bo

| Kich ban | Mang | So ban ghi | So lan lap | Ti le thanh cong (%) | Queue drain TB (s) | Queue drain Median (s) | Queue drain SD | Queue drain 95% CI | Trigger TB (ms) | Trigger Median (ms) | Trigger SD | Trigger 95% CI | Retry TB | Retry Median | Retry SD | Retry 95% CI | CPU TB (%) | RAM TB (MB) |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Dong bo ban ghi | 4G | 100 | 10 | 100 | 3.66 | 3.661 | 0.051 | 0.032 | 632.6 | 622.807 | 51.932 | 32.188 | 0 | 0 | 0 | 0 | 0.181 | 253.61 |
| Dong bo ban ghi | 4G | 1000 | 10 | 100 | 9.285 | 9.231 | 0.275 | 0.17 | 6234.19 | 6192.058 | 302.299 | 187.367 | 0 | 0 | 0 | 0 | 0.458 | 261.64 |
| Dong bo ban ghi | 4G | 5000 | 10 | 100 | 34.398 | 34.305 | 0.336 | 0.208 | 31344.717 | 31248.245 | 328.526 | 203.622 | 0 | 0 | 0 | 0 | 0.737 | 292.02 |
| Dong bo ban ghi | 4G | 10000 | 10 | 100 | 65.875 | 65.888 | 0.508 | 0.315 | 62761.156 | 62770.297 | 524.631 | 325.17 | 0 | 0 | 0 | 0 | 0.162 | 366.9 |
| Dong bo ban ghi | LAN | 100 | 10 | 100 | 3.603 | 3.606 | 0.114 | 0.071 | 628.663 | 654.56 | 113.352 | 70.256 | 0 | 0 | 0 | 0 | 0.047 | 169.65 |
| Dong bo ban ghi | LAN | 1000 | 10 | 100 | 9.224 | 9.25 | 0.24 | 0.149 | 6223.827 | 6233.57 | 243.118 | 150.686 | 0 | 0 | 0 | 0 | 0.79 | 170.97 |
| Dong bo ban ghi | LAN | 5000 | 10 | 100 | 34.58 | 34.168 | 1.493 | 0.925 | 31501.209 | 31071.365 | 1484.207 | 919.921 | 0 | 0 | 0 | 0 | 0.84 | 181.66 |
| Dong bo ban ghi | LAN | 10000 | 10 | 100 | 65.484 | 65.516 | 0.547 | 0.339 | 62439 | 62540.408 | 597.818 | 370.532 | 0 | 0 | 0 | 0 | 0.206 | 234.94 |
| Dong bo ban ghi | VSAT | 100 | 15 | 100 | 3.79 | 3.778 | 0.064 | 0.033 | 653.278 | 653.981 | 45.481 | 23.017 | 0 | 0 | 0 | 0 | 0.647 | 403.353 |
| Dong bo ban ghi | VSAT | 1000 | 15 | 100 | 9.596 | 9.64 | 0.188 | 0.095 | 6440.429 | 6436.679 | 180.528 | 91.36 | 0 | 0 | 0 | 0 | 0.361 | 416.707 |
| Dong bo ban ghi | VSAT | 5000 | 15 | 100 | 35.095 | 35.124 | 0.351 | 0.178 | 31894.452 | 31988.018 | 324.893 | 164.418 | 0 | 0 | 0 | 0 | 0.403 | 455.067 |
| Dong bo ban ghi | VSAT | 10000 | 15 | 100 | 67.736 | 67.71 | 0.229 | 0.116 | 64477.208 | 64502.883 | 246.596 | 124.795 | 0 | 0 | 0 | 0 | 0.518 | 515 |

### 5.3. Nhan xet tu campaign dong bo

Campaign moi nhat cho thay ti le thanh cong dat `100%` tren toan bo `12` to hop scenario. Queue drain time tang gan nhu tuyen tinh theo quy mo du lieu: khoang `3.6 - 3.8 s` cho `100` ban ghi, `9.2 - 9.6 s` cho `1000` ban ghi, `34.4 - 35.1 s` cho `5000` ban ghi, va `65.5 - 67.7 s` cho `10000` ban ghi. Trigger latency cung tang ro rang theo quy mo batch, dat muc xap xi `62 - 64 s` cho `10000` ban ghi.

Neu doi chieu theo profile mang, `LAN` la baseline nhanh nhat o quy mo `100` va `1000`, trong khi `4G` rat gan baseline. `VSAT` co do tre va RAM cao hon, nhung khong lam giam ti le thanh cong. Cac cot retry trong campaign nay deu bang `0`, cho thay harness moi nhat khong phai dua vao retry lap lai de dat thanh cong, ma phan anh sat hon hanh vi dong bo thuc te.

## 6. Ket qua file-flow live sau khi sua metadata/FK

### 6.1. Muc tieu va boi canh

Sau khi sua hai nhom loi runtime tren code live, gom (i) Edge khong con danh dau `synced_at` cho item batch bi fail mot phan va (ii) Shore co the resolve dung FK cho `health_document` va `crew_certificate` dua tren crew business identifiers, bo script `Invoke-ResearchFileFlowScenarios.ps1` da duoc chay lai thanh cong voi output tai `campaign-rerun-20260322-fileflow-v5/summary.json`.

### 6.2. Bang ket qua file-flow live

| Scenario | Dataset | Chi so chinh | Ket qua | Nhan xet ngan |
|---|---|---|---|---|
| F1 - Large image preprocess | `crew_certificate`, anh goc `1023525 B` | Trigger `2403.58 ms`, file Shore `943138 B`, giam `80387 B` (~`7.85%`), `is_preprocessed = true`, profile `image-optimized` | Pass | Path preprocess/compress hoat dong end-to-end sau khi doi `recordKey` sang `CertificateNumber` va bo sung crew business identifiers |
| F2 - Small file bundle | `3 x health_document` PDF (`14648 B`, `16696 B`, `18744 B`) | Trigger `254.64 ms`, tong du lieu Shore `50088 B`, `3/3` manifest verified, `edgeUploadChunkSessionCount = 0`, `shoreUploadChunkSessionCount = 0` | Pass | Bundle path dong bo on dinh voi metadata-first/file verification; khong can chunk session cho tap nho |
| F3 - Delta transfer | `travel_document`, file `1572896 B` | Baseline trigger `362.64 ms`, modified trigger `302.01 ms`, `7` chunk, `is_delta_session = false`, `changedChunkCount = 0` | Pass co ghi chu | Scenario thanh cong ve mat dong bo tep, nhung dot do nay chua kich hoat duoc block-delta thuc su ma dang fallback ve full upload |

### 6.3. Nhan xet tu file-flow live

Ket qua file-flow live bo sung bang chung quan trong ma campaign record-sync khong bao phu: preprocess anh lon da hoat dong thuc su tren du lieu that, small-file path da dong bo on dinh ma khong can chunk session, va delta-transfer da di het luong file-sync nhung chua kich hoat duoc block-delta thuc su. Vi vay, doi voi phan bao cao, can dien giai F3 la “dong bo tep thanh cong, nhung round nay moi xac nhan fallback full upload” thay vi khang dinh da chung minh duoc changed-block transfer.

## 7. Benchmark metadata-first voi corpus 1000 file

### 7.1. Cau hinh benchmark

De tang co mau cho danh gia co che dong bo tep theo huong metadata-first, mot corpus thuc nghiem `1000` file da duoc tao trong `artifacts/research-sync/metadata-first-benchmark-1000/generated-corpus`. Corpus nay duoc sinh bang cach lay mau co lap tu cac tep that dang ton tai trong `edge_product/edge-services/uploads/crew`, giu nguyen kich thuoc tep, loai tep, bang nghiep vu va muc uu tien truyen file cua he thong hien tai. Cach lam nay khong thay doi du lieu nghiep vu goc, dong thoi cho phep danh gia payload tren co mau lon hon va lap lai duoc voi seed co dinh.

### 7.2. Bang tong hop metadata-first

| Chi so | Gia tri |
|---|---:|
| So file trong corpus | 1000 |
| Tong kich thuoc tep goc | 335,670,833 bytes |
| Tong kich thuoc payload metadata-only | 483,604 bytes |
| Tong kich thuoc payload inline full-content | 448,050,265 bytes |
| Giam payload khoi tao so voi mo hinh inline full-content | 447,566,661 bytes |
| Ty le giam payload khoi tao so voi mo hinh inline full-content | 99.8921% |
| Ti le trung binh `full-content / metadata-only` | 942.81 lan |
| Ti le trung vi `full-content / metadata-only` | 745.76 lan |

| Nhom file | So file | Payload metadata-only (bytes) | Payload full-content (bytes) | Ty le giam khoi tao (%) | Ti le TB (lan) |
|---|---:|---:|---:|---:|---:|
| crew_certificate | 89 | 39,516 | 121,500,219 | 99.9675 | 3074.71 |
| crew_member | 350 | 168,236 | 28,487,050 | 99.4094 | 176.13 |
| health_document | 167 | 82,032 | 117,398,476 | 99.9301 | 1429.85 |
| travel_document | 394 | 193,820 | 180,664,520 | 99.8927 | 935.86 |

### 7.3. Dien giai hoc thuat

Can dien giai than trong rang chi so `99.8921%` o day chi do muc giam cua `payload khoi tao` khi so sanh co che metadata-first voi mo hinh doi chung “dua ca noi dung file vao JSON dong bo ngay tu dau”. Day khong phai la muc tiet kiem tong bang thong cuoi cung trong moi truong hop, boi phan byte that cua file van co the duoc tai o buoc sau neu Shore chua co ban sao hop le. Cach trinh bay dung hon la: metadata-first tach buoc “quang ba metadata” khoi buoc “truyen byte tep”, nhờ do giam rat manh kich thuoc goi dong bo ban dau va cho phep quyet dinh tai tep theo nhu cau thuc te.

Trong artifact benchmark, profile `Cellular_4G` cho thay co `439` file du dieu kien truyen ngay va `561` file bi tri hoan, trong khi Shore van nhan du metadata cua toan bo `1000` file chi trong `483,604 bytes`. Neu so voi mo hinh inline full-content, nhom file du dieu kien truyen ngay tren `4G` tuong ung `149,987,269 bytes` payload noi dung, con nhom bi tri hoan tuong ung `298,062,996 bytes` payload noi dung. Nhu vay, metadata-first khong chi giam kich thuoc goi dong bo ban dau ma con tao co so de tri hoan phan payload tep uu tien thap tren lien ket han che bang thong.

## 8. Danh gia tong hop giua bao mat va dong bo

Khi dat bon cum ket qua canh nhau, co the rut ra bon nhan xet chinh. Thu nhat, signed sync khong gay vo flow dong bo: secure sync Round 1 da xac nhan signed request van hoat dong tren LAN, 4G-like, VSAT-like va offline/recovery; campaign dong bo moi nhat tiep tuc cung cap bang chung dinh luong voi `success rate = 100%` tren `LAN`, `4G`, `VSAT`. Thu hai, cac co che bao mat cot loi nhu anti-replay, revoke, grace rotation va rollback da duoc kiem chung ma khong lam mat tinh san sang cua he thong. Thu ba, file-flow live cho thay preprocessing va bundle path da duoc kich hoat dung tren du lieu that sau khi sua loi FK/runtime. Thu tu, benchmark metadata-first bo sung bang chung rang thiet ke tach metadata va byte-content mang lai muc giam payload khoi tao rat lon.

Neu doi chieu theo profile mang, `LAN` la baseline nhanh nhat, `4G` rat gan baseline, con `VSAT` ton chi phi nhieu hon ve do tre va RAM nhung van giu duoc thanh cong `100%`. Tu do co the ket luan rang secure sync da dat duoc muc tieu thiet ke: bao toan tinh xac thuc, toan ven va truy vet ma van duy tri kha nang dong bo thuc dung tren cac profile mang dai dien cho bai toan hang hai.

## 9. Gioi han cua bo thuc nghiem

- Network emulation hien tai dua tren Toxiproxy, chu yeu mo phong latency, jitter, bandwidth va timeout; packet loss fidelity chua dat muc sat thuc dia ngoai bien.
- Cum security Round 1 moi bao gom mot tap scenario uu tien cao, chua phu kin toan bo nhom negative test trong test matrix nhu payload tamper file chi tiet, ownership binding cho moi domain va concurrent multi-client load.
- Benchmark metadata-first `1000` file duoc sinh bang cach lay mau co lap tu corpus tep hien co, vi vay phu hop de danh gia payload va chinh sach tri hoan, nhung chua thay the hoan toan cho bo du lieu thuc dia phong phu hon.
- CPU va RAM duoc lay theo snapshot trong moi repetition, phu hop cho so sanh tuong doi trong cung testbed, nhung khong nen dien giai nhu benchmark he thong tuyet doi.
- Scenario delta trong file-flow live moi xac nhan fallback full upload; can mot dot rieng de ep block-delta path va ghi nhan `requested_chunk_indexes_json` khac `null` neu muon ket luan sau hon ve changed-block transfer.

## 10. Ket luan cho bao cao

Tu toan bo bang chung hien co, co the ket luan rang he thong Edge - Shore da dat duoc hai muc tieu lon cua de tai. Ve bao mat, secure sync da kiem chung thanh cong cac co che anti-replay, node revoke, grace rotation va rollback key, dong thoi duy tri duoc audit trail va traceability. Ve dong bo ban ghi, campaign dinh luong moi nhat xac nhan he thong dat `100%` thanh cong tren `LAN`, `4G`, `VSAT` voi bo lap `10/10/15`, va cac thong ke `median`, `std dev`, `95% CI` cho thay ket qua on dinh, lap lai duoc. Ve dong bo tep, dot file-flow live da bo sung bang chung preprocess anh lon, small-file bundle path va delta fallback. Bo sung them, benchmark metadata-first tren corpus `1000` file cho thay co che nay cat giam rat manh `payload khoi tao`, dong thoi cho phep tri hoan mot phan lon payload tep tren lien ket han che bang thong ma van giu du metadata ngay o lan dong bo dau. Nhu vay, bo ket qua nay da du co so de dua vao phan “Thuc nghiem va danh gia” voi lap luan rang co che dong bo an toan de xuat khong chi dung ve mat thiet ke ma con dung trong thuc nghiem co kiem soat.

## 11. Cau hinh va ket qua kiem thu tai Shore public

### 11.1. Muc tieu va pham vi

Ngoai cac dot thuc nghiem local cho secure sync va record-sync, mot bai kiem thu tai rieng da duoc thuc hien truc tiep tren Shore public tai `https://shcdvmu.site` de danh gia kha nang dap ung khi co `100 - 150` nguoi dung truy cap dong thoi. De tranh tac dong den du lieu van hanh that, kich ban chi su dung cac `GET` endpoint public, khong thuc hien login, khong ghi du lieu, khong tao moi ho so va khong goi cac luong mutate nghiep vu.

Do deployment public hien tai tra `404` cho `POST /api/auth/login`, dot benchmark nay duoc gioi han trong pham vi public read-only. Script da duoc bo sung kha nang bearer token de su dung trong cac dot sau, nhung ket qua duoi day la ket qua cua public read mix tren he thong dang deploy.

### 11.2. Cau hinh bai kiem thu

| Thanh phan | Cau hinh da dung |
|---|---|
| Dich vu dich | `https://shcdvmu.site` |
| Cong cu | `k6` |
| Kich ban | `ramping-vus` |
| Mo hinh truy cap | Public read-only, weighted mix theo hanh vi dashboard/browse |
| Think time | Ngau nhien trong khoang `800 - 2500 ms` |
| Header | Mac dinh public, khong bearer token trong dot nay |
| Nhom endpoint | `/api/health`, `/api/vessels`, `/api/vessels/fleet-summary`, `/api/voyages`, `/api/voyages/{id}`, `/api/reports/vessels`, `/api/reports/vessel/{id}` |
| Threshold danh gia | `http_req_failed < 2%`, `checks > 98%`, global `p95 < 2000 ms`, global `p99 < 5000 ms`, kem nguong `p95` theo tung endpoint |

Ba muc tai da duoc chay gom `100 VU`, `150 VU` va mot dot `200 VU` ngan de xac nhan diem gay. Cau hinh cu the cho tung muc nhu sau.

| Muc tai | Ramp-up | Hold | Ramp-down | Muc dich |
|---|---:|---:|---:|---|
| `100 VU` | `45 s` | `120 s` | `30 s` | Muc tai muc tieu thap hon trong yeu cau ban dau |
| `150 VU` | `60 s` | `120 s` | `30 s` | Muc tai can tren trong yeu cau ban dau |
| `200 VU` | `45 s` | `60 s` | `30 s` | Dot stress probe ngan de xac nhan nguong bao hoa |

### 11.3. Ket qua tong hop

| Muc tai | Request loi (%) | Checks dat (%) | P95 tong the | P99 tong the | Thong luong TB (req/s) | Nhan xet |
|---|---:|---:|---:|---:|---:|---|
| `100 VU` | `0.00` | `100.00` | `1.64 s` | `3.63 s` | `35.85` | He thong van phuc vu on dinh, chi truot threshold chat cua `health` |
| `150 VU` | `0.55` | `99.20` | `7.01 s` | `21.01 s` | `34.33` | Do tre tang manh, da vao vung qua tai thuc te |
| `200 VU` | `43.82` | `61.16` | `30.00 s` | `30.00 s` | `19.34` | He thong suy giam ro ret, khong con la muc van hanh an toan |

### 11.4. Chi tiet ket qua theo tung muc tai

| Muc tai | Chi tiet chinh |
|---|---|
| `100 VU` | `7111` request, `7106` iterations, `http_req_failed = 0.00%`, `checks = 100.00%`, `p95 = 1.64 s`, `p99 = 3.63 s`; chi threshold `health p95 < 500 ms` bi truot voi gia tri thuc te xap xi `1.42 s` |
| `150 VU` | `7294` request, `7289` iterations, `http_req_failed = 0.55%`, `checks = 99.20%`, `p95 = 7.01 s`, `p99 = 21.01 s`; cac endpoint `vessels`, `voyages-list`, `voyage-detail`, `reports-vessels`, `reports-vessel-detail`, `fleet-summary`, `health` deu truot threshold `p95` |
| `200 VU` | `2937` request, `2932` iterations, `http_req_failed = 43.82%`, `checks = 61.16%`, `p95 = 30.00 s`, `p99 = 30.00 s`; nhieu request cham moc timeout va throughput giam manh xuong con xap xi `19.34 req/s` |

### 11.5. Dien giai ket qua

Ket qua benchmark public cho thay cum Shore dang deploy co the dap ung muc truy cap tuong duong `100` nguoi dung dong thoi voi public read mix da chon, du do tre da tang so voi cac nguong chat theo endpoint. Khi tang len `150` nguoi dung dong thoi, throughput khong tang them ma latency lai tang vot len `p95 = 7.01 s` va `p99 = 21.01 s`, cho thay he thong da cham nguong bao hoa tai nguyen o tang API, DB hoac ca hai. Dot probe `200 VU` xac nhan diem gay nay mot cach ro rang hon: ty le loi vuot `43%`, p95/p99 cham tran `30 s`, va thong luong thuc te giam manh.

Tu cac so lieu nay, co the dien giai thuc nghiem nhu sau: muc van hanh an toan hien tai nam quanh `100 concurrent users`; muc `150 concurrent users` van con dap ung mot phan yeu cau chuc nang nhung khong con dat muc chat luong phan hoi tot; va muc `200 concurrent users` vuot kha nang phuc vu on dinh cua deployment hien tai. Day la bang chung bo sung huu ich cho phan danh gia he thong khi chuyen tu testbed local sang dich vu Shore public dang van hanh.

### 11.6. Artifact doi chieu

| Muc tai | Artifact |
|---|---|
| `100 VU` | `artifacts/load-test/direct-20260324-155648-100vu/summary.json` |
| `150 VU` | `artifacts/load-test/direct-20260324-160158-150vu/summary.json` |
| `200 VU` | `artifacts/load-test/direct-20260324-160653-200vu/summary.json` |
