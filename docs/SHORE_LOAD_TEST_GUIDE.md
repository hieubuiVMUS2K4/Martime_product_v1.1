# Shore Load Test Guide

## Muc tieu

Bo script nay duoc tao de test kha nang chiu tai cua shore server da deploy tai `https://shcdvmu.site` theo kieu **read-only**, khong ghi du lieu vao production.

Kich ban mo phong `100` den `150` nguoi dung truy cap dong thoi vao cac man hinh tong quan quan trong cua shore frontend/backend:

- `GET /api/health`
- `GET /api/vessels`
- `GET /api/vessels/fleet-summary`
- `GET /api/voyages?page=...`
- `GET /api/voyages/{id}`
- `GET /api/reports/vessels`
- `GET /api/reports/vessel/{vesselId}`

Tat ca request deu la `GET`, khong tao moi, khong sua, khong xoa du lieu.

Mac dinh script chay o che do `public`. Neu sau nay co san bearer token hop le, co the truyen them `ACCESS_TOKEN` de gui kem header `Authorization: Bearer ...` tren cung tap endpoint.

## File moi trong repo

- `scripts/load-test/shore-public-load.js`: kich ban k6 chinh.
- `scripts/load-test/run-shore-public-load.ps1`: wrapper PowerShell de chay nhanh profile `100`, `150`, `200`, `300`, `500` hoac `custom`.

## Cach mo phong nguoi dung

Kich ban su dung `ramping-vus` va co `think time` mac dinh `0.8s` den `2.5s` giua hai request. Nhu vay no gan voi hanh vi nguoi dung that hon la spam request lien tuc.

Phan bo tai mac dinh:

- `5%` health
- `25%` vessels list
- `20%` fleet summary
- `25%` voyages list
- `10%` voyage detail
- `10%` reports by vessels
- `5%` vessel reports detail

## Nguong danh gia mac dinh

Kich ban khai bao threshold k6 nhu sau:

- `http_req_failed < 2%`
- `checks > 98%`
- `p95 http_req_duration < 2000ms`
- `p99 http_req_duration < 5000ms`

Ngoai ra co threshold rieng cho tung nhom endpoint de de xac dinh man hinh nao bi cham.

## Cai dat k6 tren Windows

Neu may chua co `k6`, cai nhanh bang:

```powershell
winget install k6.k6
```

Neu dung Chocolatey:

```powershell
choco install k6
```

Sau khi cai xong, mo terminal moi.

## Chay smoke test truoc

Nen chay mot bai nho truoc khi tang len `100-150` user:

```powershell
k6 run `
  -e BASE_URL=https://shcdvmu.site `
  -e TARGET_VUS=10 `
  -e RAMP_UP=30s `
  -e HOLD=1m `
  -e RAMP_DOWN=20s `
  scripts/load-test/shore-public-load.js
```

## Chay profile 100 nguoi dung dong thoi

```powershell
.\scripts\load-test\run-shore-public-load.ps1 -LoadProfile 100
```

## Chay profile 150 nguoi dung dong thoi

```powershell
.\scripts\load-test\run-shore-public-load.ps1 -LoadProfile 150
```

## Chay profile 200, 300, 500 de tim diem bao hoa

```powershell
.\scripts\load-test\run-shore-public-load.ps1 -LoadProfile 200
.\scripts\load-test\run-shore-public-load.ps1 -LoadProfile 300
.\scripts\load-test\run-shore-public-load.ps1 -LoadProfile 500
```

## Chay profile tuy chinh

Vi du `120` user, giu tai `15` phut:

```powershell
.\scripts\load-test\run-shore-public-load.ps1 `
  -LoadProfile custom `
  -Vus 120 `
  -Hold 15m
```

## Chay voi bearer token co san

Neu sau nay shore co co che cap token hoac ban da co san JWT hop le, co the chay:

```powershell
.\scripts\load-test\run-shore-public-load.ps1 `
  -LoadProfile 150 `
  -AccessToken '<jwt-token>'
```

Luu y: tai deployment `https://shcdvmu.site` hien tai, `POST /api/auth/login` tra `404`, va trong codebase khong co endpoint `GET` nao cua shore vua chi doc vua bat buoc JWT. Vi vay authenticated flow thuc su hien chua the benchmark end-to-end tren deployment nay neu khong co them co che cap token hoac endpoint chi doc duoc bao ve bang JWT.

## Artifact ket qua

Moi lan chay se tao mot thu muc duoi `artifacts/load-test`, vi du:

```text
artifacts/load-test/shore-public-20260324-151500-100vu/
```

Trong do co:

- `summary.json`: tong hop chi so k6.
- `console.log`: output day du cua k6.
- `run-config.json`: cau hinh thuc te da chay.

## Cach doc ket qua nhanh

Can theo doi it nhat cac chi so sau:

- `http_req_failed`: ti le loi tong.
- `http_req_duration p(95)`: muc tre ma 95% request nam duoi.
- `iterations`: tong so vong lap nguoi dung thuc hien duoc.
- `vus_max`: so user dong thoi toi da da dat.

Neu can danh gia kha nang chiu tai cho bao cao, nen bo sung them metric he thong khi dang chay test:

- CPU, RAM cua container/backend host
- so luong ket noi PostgreSQL
- disk I/O
- reverse proxy/nginx access log
- application log tai shore backend

## Trinh tu khuyen nghi khi test production

1. Chay smoke `10 VU` trong `1-2` phut.
2. Chay `50 VU` trong `5` phut de lay baseline.
3. Chay `100 VU` trong `10` phut.
4. Neu on dinh, chay `150 VU` trong `10-15` phut.
5. Neu can tim diem bao hoa, tang tiep `200`, `300`, `500` theo tung buoc nho.

## Luu y van hanh

- Nen test ngoai gio lam viec neu day la server production that.
- Bai test nay an toan hon vi chi dung `GET`, nhung van co the gay tang tai CPU, DB, cache va bang thong.
- Khong nen chay dong thoi nhieu may client neu chua co ke hoach quan sat he thong.
- Neu can test authenticated flow, nen tao tai khoan test rieng va tach thanh kich ban khac, khong dung chung bo script nay.