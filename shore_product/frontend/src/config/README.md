# config/ — Cấu hình ứng dụng

## Mục đích

Gom các hằng số cấu hình (base URL API, tên app, timeout...) vào một chỗ thay vì rải rác trong code.

## Cấu trúc & vai trò

| File | Export | Nội dung |
|---|---|---|
| `app.config.ts` | `API_CONFIG` (`BASE_URL: '/api'`, `TIMEOUT: 30000`), `VESSEL_CONFIG` (`VESSEL_NAME: 'Shore Office'`) | Cấu hình dùng bởi `services/api.client.ts` (base URL + timeout) và một vài nơi cần tên hiển thị mặc định cho "văn phòng bờ". |
| `env.ts` | `ENV` (`API_BASE_URL`, `APP_NAME: 'Maritime Shore Management'`, `APP_SHORT_NAME: 'MSM'`, `VERSION: '1.0.0'`) | `API_BASE_URL` đọc từ biến môi trường Vite `VITE_API_BASE_URL`, mặc định `'/api'` nếu không set. Đây là hằng số **được dùng nhiều nhất** trong toàn bộ `services/` (import `{ ENV } from '../config/env'` rồi dùng `${ENV.API_BASE_URL}/...`). |

Cả hai file đều export object `as const` thuần, không có logic, không có validation.

## Luồng hoạt động chính

```
.env (VITE_API_BASE_URL, nếu có) ──► import.meta.env ──► config/env.ts (ENV.API_BASE_URL)
                                                              │
                                          services/*.service.ts dùng ENV.API_BASE_URL
                                          services/api.client.ts dùng API_CONFIG.BASE_URL (config/app.config.ts)
```

Cả `ENV.API_BASE_URL` và `API_CONFIG.BASE_URL` **đều mặc định là `/api`** (giá trị tương đối) — việc forward sang `http://localhost:5000` là do proxy của Vite dev server khai báo trong `vite.config.ts`, **không** phải do file cấu hình này quyết định host thật.

## Liên kết với phần khác

- **services/**: gần như mọi file `*.service.ts` import `ENV` từ đây để dựng URL gọi API; `api.client.ts` (và các service dùng `apiClient`) dùng `API_CONFIG`.
- **vite.config.ts** (ngoài `src/`): định nghĩa alias `@` → `src/` và proxy `/api`, `/uploads`, `/vietmap`, `/maps` — phối hợp với `API_BASE_URL = '/api'` ở đây để request đi đúng đường.

## Ghi chú khi đọc/dạy

- Có **2 hằng số base-URL trùng chức năng** (`API_CONFIG.BASE_URL` và `ENV.API_BASE_URL`) định nghĩa ở 2 file khác nhau, cùng giá trị mặc định `'/api'` nhưng độc lập nhau (đổi một cái không tự đổi cái kia). Phần lớn service mới dùng `ENV.API_BASE_URL`; chỉ `api.client.ts` (và các service qua nó) dùng `API_CONFIG`.
- Không có file `config/index.ts` gộp — import phải chỉ đích danh `config/env` hoặc `config/app.config`.
- Không có cấu hình theo `NODE_ENV`/multi-environment (staging/prod) ở đây — mọi khác biệt môi trường đi qua biến `VITE_*` lúc build (xem `Dockerfile`, `docker-entrypoint.sh` ở gốc `frontend/`).
