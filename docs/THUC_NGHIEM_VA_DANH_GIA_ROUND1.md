# Thuc nghiem va danh gia

## 1. Muc tieu thuc nghiem

Muc tieu cua dot thuc nghiem Round 1 la kiem chung rang co che secure sync giua Edge va Shore van hoat dong dung trong ba nhom dieu kien chinh: dieu kien co so, tinh huong an toan thong tin va tinh huong van hanh. Cum thuc nghiem nay tap trung tra loi cac cau hoi sau: (i) signed sync co van duoc chap nhan va duy tri kha nang dong bo tren cac profile mang dai dien; (ii) cac co che phong ve nhu anti-replay, revoke node, grace window va rollback key co thuc su hoat dong dung; (iii) he thong co giu duoc do on dinh khi dashboard bi burst read va khi hang doi dong bo tang cao trong dieu kien VSAT-like hay khong.

## 2. Moi truong va phuong phap thuc nghiem

He thong duoc trien khai tren testbed local gom Shore backend tai `http://localhost:5000`, Edge backend tai `http://localhost:5001`, PostgreSQL rieng cho Shore va Edge, va node thu nghiem chinh mang dinh danh `8765432`. Giao thuc signed sync su dung protocol version `2`, key version baseline `1`. De mo phong dieu kien mang, dot nay su dung Toxiproxy lam lop proxy trung gian tai `localhost:8666`. Tren co so do, cac profile LAN, 4G-like, VSAT-like va offline/recovery duoc tai hien thong qua latency, jitter va bandwidth shaping. Cach lam nay cho phep giu nguyen runtime code trong suot dot do, tu do bao dam ket qua phan anh hanh vi that cua he thong da freeze.

Ve quy trinh, cac scenario duoc chay theo thu tu tu baseline den security va operations. Moi scenario deu luu bang chung toi thieu gom log Edge, log Shore, truy van `sync_queue` hoac `audit_logs`, va trang thai node security khi can. Doi voi cac scenario baseline va operational, cac chi so chinh duoc su dung la do tre dong bo, throughput, queue drain time va ty le phan hoi. Doi voi nhom security, chi so tap trung vao ma loi, audit evidence va kha nang khoi phuc ve trang thai clean sau khi ket thuc test.

## 3. Ket qua baseline

Ket qua baseline cho thay signed sync van hoat dong dung trong ca LAN, 4G-like, VSAT-like va tinh huong offline roi hoi phuc. O LAN, cac batch nho va heartbeat deu duoc chap nhan, xac nhan duong dong bo end-to-end hoat dong on dinh. O 4G-like, batch `10` item dat do tre POST `/api/sync` khoang `202.355 ms` va throughput xap xi `49.4 item/s`, cho thay overhead ky request van o muc chap nhan duoc. O VSAT-like, cac batch `10`, `50` va `100` deu duoc chap nhan thanh cong; do tre tang theo kich thuoc batch, cu the tu `853.282 ms` den `1699.162 ms`, nhung khong lam vo signed sync flow. Trong tinh huong offline, backlog `100` item van duoc giu tai Edge, sau do day het len Shore sau khi khoi phuc ket noi voi `time to consistency` khoang `7828.044 ms`.

Tu ket qua tren co the nhan thay kien truc dong bo hien tai dap ung tot muc tieu offline-first trong pham vi Round 1. He thong khong chi dong bo duoc o dieu kien mang binh thuong, ma con giu duoc kha nang hoi phuc sau mat ket noi ma khong mat du lieu. Day la bang chung thuc nghiem quan trong cho phan danh gia baseline cua giai phap.

## 4. Ket qua security

Cum security tap trung vao cac co che bao ve cot loi cua secure sync. O scenario S1, request dau tien hop le duoc chap nhan voi ma `200`, trong khi request replay cung nonce bi tu choi voi ma `409`; dong thoi `audit_logs` ghi nhan ly do `replay_detected`. O scenario S5, sau khi node bi revoke, signed heartbeat lap tuc bi tu choi voi ma `403`, va node co the duoc kich hoat lai de tro ve trang thai clean. O scenario S6, he thong xac nhan key cu van duoc chap nhan trong grace window sau khi rotate sang key version `2`, cho thay co che staged rotation khong lam gian doan dong bo. O scenario S8, thao tac rollback key thanh cong, key version quay lai `1` va signed request bang key cu tiep tuc duoc chap nhan.

Ket qua nay xac nhan lop bao ve cua secure sync khong chi ton tai tren thiet ke ma da hoat dong dung trong thuc nghiem. Anti-replay, revoke enforcement, grace rotation va controlled rollback deu co bang chung ro rang tu ma loi, log va audit trail. Vi vay, co the ket luan rang he thong da dat duoc muc bao ve can thiet cho mot co che dong bo giua Edge va Shore trong dieu kien van hanh phan tan.

## 5. Ket qua van hanh

O scenario O1, endpoint `GET /api/sync/dashboard/overview` duoc burst `140` lan lien tiep tu cung mot client noi bo. Ket qua cho thay `29` request duoc phuc vu thanh cong va `111` request bi gioi han voi ma `429`, trong khi nhom request duoc chap nhan van duy tri `P95 = 27.342 ms` va khong co loi `5xx`. Dieu nay cho thay co che observability rate limiting da hoat dong dung va dashboard khong sap duoi burst read.

O scenario O2, backlog `500` item duoc tao duoi profile VSAT-like, sau do duoc giai phong theo `5` batch, moi batch `100` item. Tong thoi gian drain queue theo du lieu trong database la `32074.701 ms`, throughput trung binh dat xap xi `15.589 item/s`. Cac batch dong bo deu duoc chap nhan, queue giam dan ve `0` va khong ghi nhan mat du lieu. Ket qua nay cho thay he thong giu duoc kha nang xu ly backlog lon tren lien ket do tre cao.

## 5A. Ket qua file-flow live

Sau khi sua hai nhom loi runtime tren code live, gom (i) Edge khong con danh dau `synced_at` cho item batch bi fail mot phan va (ii) Shore co the resolve dung FK cho `health_document` va `crew_certificate` dua tren crew business identifiers, bo script `Invoke-ResearchFileFlowScenarios.ps1` da duoc chay lai thanh cong voi output tai `artifacts/research-sync/campaign-live-matrix/campaign-rerun-20260322-fileflow-v5/summary.json`. Run nay cung sua logic do queue de chi cho cac record do harness tao ra, tranh bi nhieu boi `position_data` nen.

O scenario large-image preprocess, he thong tao `crew_certificate` voi `CertificateNumber = RS-LARGE-20260322163110` va dong bo file anh lon tu Edge sang Shore thanh cong. File goc co kich thuoc `1023525 B`, trong khi manifest tai Shore ghi nhan file sau preprocess/cong nen chi con `943138 B`, giam `80387 B`, tuong duong xap xi `7.85%`. Manifest cung ghi ro `is_preprocessed = true` va `preprocess_profile = image-optimized`, cho thay pipeline toi uu hoa anh da duoc kich hoat trong lan chay thuc.

O scenario small-file bundle, ba `health_document` duoc tao voi cac kich thuoc lan luot `14648 B`, `16696 B` va `18744 B`. Trigger sync mat `254.64 ms`, sau do ca `3/3` manifest tai Shore deu dat `transfer_status = 2` va tong du lieu xac nhan la `50088 B`. Ca Edge va Shore deu ghi nhan `chunk session count = 0`, phu hop voi ky vong rang nhom file nho duoc xu ly gon nhe, khong can mo session chunk rieng.

O scenario delta-transfer, `travel_document` co kich thuoc `1572896 B` duoc dong bo thanh cong ca o lan baseline va lan cap nhat file. Trigger baseline mat `362.64 ms`, trigger lan sua doi mat `302.01 ms`; ca hai lan deu ket thuc voi chunk session `7` chunk, `chunk_size_bytes = 262144` va `transfer_status = 2`. Tuy nhien, metric `is_delta_session = false` va `changedChunkCount = 0` cho thay trong Round 1 live run nay he thong da fallback ve full upload thay vi kich hoat duoc block-delta thuc su. Vi vay, can tach rieng mot dot do tiep theo neu muc tieu la chung minh hieu qua delta theo nghia chi truyen cac block thay doi.

## 6. Danh gia tong hop

Tong hop cac ket qua tren, Round 1 da hoan tat day du chuoi `B1 -> B4 -> S1 -> S5 -> S6 -> S8 -> O1 -> O2` va bo sung them mot dot file-flow live cho `F1 -> F3`. He thong da the hien ba tinh chat quan trong. Thu nhat, signed sync hoat dong dung tren nhieu profile mang dai dien va giu duoc kha nang hoi phuc sau mat ket noi. Thu hai, cac co che bao ve cot loi cua secure sync da duoc kiem chung bang bang chung thuc te tu log va audit trail. Thu ba, o muc file-sync, path preprocess anh lon va small-file transfer da duoc xac nhan tren du lieu that sau khi sua cac loi FK/runtime quan sat trong moi truong live.

Tuy nhien, can ghi ro gioi han cua dot nay. Network emulation moi bao gom latency, jitter va bandwidth co ban, chua mo phong sat packet loss va bien dong thuc dia ngoai bien. O1 moi thuc hien burst tu mot client, chua mo rong sang multi-client concurrent load. Rieng F3, dot live nay chua kich hoat duoc block-delta ma van fallback ve full upload. Do do, ket qua Round 1 nen duoc dien giai la bang chung thuc nghiem ban dau co do tin cay cao cho muc bao cao va danh gia ky thuat, dong thoi la co so de mo rong sang cac dot thuc nghiem sau voi packet loss fidelity cao hon, kich ban tai dong thoi phuc tap hon va mot chien dich rieng de ep delta-session.

## 7. Ket luan cho bao cao

Tu bo so lieu hien tai, co the ket luan rang giai phap secure sync de xuat da dap ung muc tieu thuc nghiem Round 1. Kien truc Edge-Shore duy tri duoc dong bo trong dieu kien LAN, 4G-like, VSAT-like va offline/recovery; dong thoi dam bao cac co che bao ve nhu anti-replay, revoke, grace rotation va rollback key hoat dong dung. O khia canh van hanh, he thong cung cho thay kha nang bao ve dashboard va xu ly backlog lon ma khong xuat hien mat du lieu. Nhu vay, bo ket qua nay da du de dua truc tiep vao muc “Thuc nghiem va danh gia” cua bao cao.