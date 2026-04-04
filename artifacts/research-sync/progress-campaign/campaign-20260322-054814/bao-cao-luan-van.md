## Mo ta phuong phap gia lap mang

Trong dot thuc nghiem nay, he thong su dung Toxiproxy de gia lap dieu kien mang giua Edge va Shore. Mot proxy `shore` duoc mo tai `localhost:8666` va chuyen tiep den Shore backend tai `host.docker.internal:5000`. Edge backend duoc cau hinh gui request dong bo den dia chi proxy nay de toan bo heartbeat, push sync va pull sync deu di qua lop mo phong mang truoc khi den Shore.

Profile `LAN` duoc xem la baseline, tuc proxy khong ap them toxic nao. Cac profile `4G`, `VSAT` va `HF` duoc mo phong bang cach them `latency`, `jitter`, `bandwidth` hoac `timeout` tren Toxiproxy. Vi vay, cac so lieu duoi day phan anh hieu nang dong bo cua he thong trong moi truong mo phong co kiem soat, phu hop cho muc dich danh gia nghien cuu trong repo hien tai.

Luu y: day la lop harness nghien cuu toi thieu co the chay tren may Windows/local workspace. Ket qua co gia tri so sanh giua cac profile mang trong moi truong noi bo, nhung khong thay the cho do dac hien truong hay mo phong vat ly day du.

## Bang cau hinh profile mang da dung

| Profile | Cong cu gia lap | Cach ap dung | Thong so chinh |
|---|---|---|---|
| LAN | Toxiproxy | Khong them toxic | Duong truyen baseline, khong gioi han bo sung |
| 4G | Toxiproxy | `latency` hai chieu | Upstream: 23 ms, jitter 6 ms; Downstream: 22 ms, jitter 6 ms |
| VSAT | Toxiproxy | `latency` + `bandwidth` hai chieu | Upstream: 345 ms, jitter 43 ms, rate 250; Downstream: 345 ms, jitter 42 ms, rate 250 |
| HF | Toxiproxy | `latency` + `bandwidth` hai chieu | Upstream: 1200 ms, jitter 250 ms, rate 8; Downstream: 1200 ms, jitter 250 ms, rate 8 |
| OFFLINE | Toxiproxy | `timeout` | Cat ket noi downstream trong 60000 ms |

## Bang tong hop ket qua

| Kich ban | Mang | So ban ghi | So lan lap | Ti le thanh cong (%) | Queue drain TB (s) | Queue drain Median (s) | Queue drain SD | Queue drain 95% CI | Trigger TB (ms) | Trigger Median (ms) | Trigger SD | Trigger 95% CI | Retry TB | Retry Median | Retry SD | Retry 95% CI | CPU TB (%) | RAM TB (MB) |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Dong bo ban ghi | LAN | 10 | 1 | 0 | 5.464 | 5.464 | 0 | 0 | 25.781 | 25.781 | 0 | 0 | 10 | 10 | 0 | 0 | 0.49 | 52.64 |
