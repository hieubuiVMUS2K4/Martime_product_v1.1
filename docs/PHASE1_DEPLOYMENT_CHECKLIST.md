# Phase 1 Deployment Checklist

## 1. Secrets va bien moi truong

- Dat `POSTGRES_PASSWORD` bang chuoi manh, khong dung gia tri mac dinh cu.
- Dat `JWT_KEY` bang chuoi ngau nhien dai toi thieu 32 ky tu.
- Dat `INTERNAL_API_KEY` bang chuoi ngau nhien rieng cho reverse proxy.
- Dat `EDGE_AUTH_TOKEN_SIGNING_KEY` bang chuoi ngau nhien dai toi thieu 32 ky tu.
- Dat `EDGE_INTERNAL_API_KEY` bang chuoi ngau nhien rieng cho Edge.
- Dat `SHORE_API_KEY` neu Edge dung API key khi goi ve Shore.

## 2. Shore production

- Xac nhan frontend Shore duoc deploy qua Nginx container hoac reverse proxy co inject `X-Internal-Api-Key`.
- Xac nhan backend Shore nhan cung mot `InternalAccess__ApiKey`.
- Giu `SECURITY_REQUIRE_INTERNAL_ACCESS=true` trong production sau khi xac nhan frontend da gui header noi bo.
- Giu `SECURITY_ENFORCE_HTTPS=true` trong production khi Shore da co TLS/reverse proxy HTTPS.
- Khong expose backend Shore truc tiep ra Internet neu frontend proxy la diem vao duy nhat.

## 3. Edge deployment

- Dat `Auth__TokenSigningKey` qua environment, khong de trong `appsettings.json`.
- Dat `InternalAccess__ApiKey` qua environment.
- Xac nhan cac endpoint van hanh Edge chi duoc goi noi bo hoac qua giao dien da xac thuc.
- Bat `EDGE_SECURITY_REQUIRE_INTERNAL_ACCESS=true` khi da co header/key noi bo dung.
- Bat `EDGE_SECURITY_ENFORCE_HTTPS=true` khi da co TLS hoac reverse proxy hop le.

## 4. Frontend security

- Edge frontend luu refresh token trong `sessionStorage`, khong persist access token qua restart tab.
- Kiem tra logout, refresh token va session timeout van hoat dong dung.
- Neu mo rong Shore auth UI o phase sau, khong dua token vao `localStorage` neu khong that su can thiet.

## 5. Verification sau deploy

- Thu `GET /api/health` phai tra ve 200.
- Thu `GET /api/health/ready` khong co internal key phai bi tu choi khi enforcement da bat.
- Thu cac man hinh Shore crew/compliance/assignment/travel/onboard qua frontend proxy.
- Thu Edge `sync/status`, `trigger`, `reset-errors` qua giao dien sau khi bat enforcement.
- Chay `dotnet build` cho Shore backend va Edge backend sau moi dot chinh sua.

## 6. Khong duoc bo qua

- Khong commit lai secret that vao `.env`, `.env.production`, `appsettings*.json`, `docker-compose*.yml`.
- Khong bat `RequireInternalAccess` neu frontend proxy chua duoc cap `INTERNAL_API_KEY`.
- Khong bat `EnforceHttps` neu reverse proxy hoac TLS chua hoan tat.