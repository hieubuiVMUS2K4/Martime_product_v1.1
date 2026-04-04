# Research Sync Campaign

## Muc tieu

Tai lieu nay chot bo kich ban nghien cuu de do he thong dong bo Edge-Shore theo dung huong ban muon bao cao: test tren nhieu profile mang, nhieu muc tai ban ghi, co dataset file rieng, co lap lai nhieu lan va co bang chung de tong hop thanh bang ket qua khoa hoc.

Bo campaign duoi day duoc chia thanh 2 lop:

1. Lop muc tieu nghien cuu: bam theo bang cau hinh ban da de xuat.
2. Lop toi thieu co the chay ngay trong repo hien tai: su dung Docker, PowerShell, Toxiproxy, Edge sync queue, Shore API, log va docker stats.

## Cau hinh nghien cuu muc tieu

| Thanh phan | Cau hinh / Ke hoach danh gia |
|---|---|
| Nut Edge | Docker, ASP.NET Core 8.0, PostgreSQL 15 |
| Nut Shore | Docker, ASP.NET Core 8.0, PostgreSQL 15 |
| Gia lap mang | Linux tc netem |
| Kich ban mang | LAN, 4G, VSAT, HF |
| Du lieu ban ghi | 100, 1.000, 5.000, 10.000 ban ghi thay doi |
| Du lieu tep | 300 tep hon hop (anh, PDF, scan, tep trung lap) |
| Cong cu sinh tai | k6, JMeter, curl |
| Cong cu do DB | pgBench, EXPLAIN ANALYZE |
| Monitoring | Prometheus, Grafana |
| So lan lap | >= 10 lan moi kich ban |
| Chi so do | bandwidth, latency, success rate, retry count, resource usage |

## Lop toi thieu co the chay ngay tren repo nay

Workspace hien tai da co:

1. Edge va Shore backend chay tren ASP.NET Core 8 va PostgreSQL 15.
2. Docker Compose cho Edge va Shore.
3. Runtime sync queue, retry, batch sync va signed sync.
4. Tai lieu thuc nghiem Phase 3 va metric runbook.
5. Toxiproxy da tung duoc dung de mo phong LAN, 4G-like, VSAT-like va offline.

Voi muc tieu dung duoc ngay tren may Windows dang mo workspace, bo harness moi trong `scripts/research-sync` chon cach chay sau:

1. Dung Toxiproxy thay cho Linux `tc netem`.
2. Dung PowerShell + curl qua `Invoke-RestMethod` thay cho k6/JMeter o pha dau.
3. Dung `docker stats`, log Shore, log Edge, `sync_queue` va `api/sync/status` de lay metric toi thieu.
4. Dung dataset record stress vao `crew_members` va `sync_queue`, vi day la flow sync da co san va de lap lai.
5. Dung file dataset generator rieng de tao 300 tep co manifest va checksum, san sang cho pha file sync.

## Kich ban nghien cuu de chay

### Nhom A. Record Sync Benchmarks

| Scenario ID | Network | Record count | Lap lai | Muc tieu |
|---|---|---|---|---|
| R1 | LAN | 100 | >= 10 | Baseline latency va success rate |
| R2 | LAN | 1.000 | >= 10 | Do throughput va queue drain time |
| R3 | LAN | 5.000 | >= 10 | Do saturation dau tien |
| R4 | LAN | 10.000 | >= 10 | Do kha nang xu ly backlog lon |
| R5 | 4G | 100 | >= 10 | Do anh huong mang ven bo |
| R6 | 4G | 1.000 | >= 10 | Do throughput duoi shaping trung binh |
| R7 | 4G | 5.000 | >= 10 | Do queue drain time va retry |
| R8 | 4G | 10.000 | >= 10 | Do saturation duoi profile 4G |
| R9 | VSAT | 100 | >= 10 | Baseline VSAT |
| R10 | VSAT | 1.000 | >= 10 | Do batch drain duoi RTT cao |
| R11 | VSAT | 5.000 | >= 10 | Do retry va queue backlog |
| R12 | VSAT | 10.000 | >= 10 | Do gioi han operational |
| R13 | HF | 100 | >= 10 | Do hanh vi tren lien ket rat han che |
| R14 | HF | 1.000 | >= 10 | Do backlog va retry tren profile rat xau |
| R15 | HF | 5.000 | >= 10 | Do hang doi lon duoi profile HF |
| R16 | HF | 10.000 | >= 10 | Chi chay khi ha tang du on dinh |

### Nhom B. File Dataset Benchmarks

| Scenario ID | Network | Dataset file | Lap lai | Muc tieu |
|---|---|---|---|---|
| F1 | LAN | 300 tep | >= 10 | Baseline file movement |
| F2 | 4G | 300 tep | >= 10 | Do kha nang xu ly file duoi 4G |
| F3 | VSAT | 300 tep | >= 10 | Do file sync duoi RTT cao |
| F4 | HF | 300 tep | >= 10 | Chi chay khi co co che shaping on dinh |

## Cau truc metric can luu

| Metric | Nguon toi thieu trong repo nay |
|---|---|
| Trigger latency | do thoi gian `POST /api/sync/trigger` |
| Queue drain time | `pendingRecords` ve `0` |
| Success rate | tong lan thanh cong / tong lan trigger |
| Retry count | tong `retry_count` trong `sync_queue` |
| Pending backlog | `api/sync/status` + `sync_queue` |
| CPU/RAM | `docker stats --no-stream` |
| Edge log evidence | log sync, queue, retry |
| Shore log evidence | log accept, reject, heartbeat, pull, ack |

## Quy trinh thuc hien cho moi scenario

1. Xac nhan Shore backend, Edge backend va Edge Postgres da san sang.
2. Neu co shaping mang, dat profile bang `Set-ToxiproxyProfile.ps1`.
3. Tao dataset record bang `New-ResearchSyncQueueSeed.ps1`.
4. Trigger sync bang `Invoke-ResearchSyncScenario.ps1`.
5. Theo doi `pendingRecords` den khi queue ve `0` hoac dat timeout.
6. Luu CSV trigger timeline, JSON summary, docker stats snapshot, SQL seed da dung.
7. Lap lai den du so lan chay.

## Lenh chay toi thieu

### 1. Tao 300 tep file dataset

```powershell
.\scripts\research-sync\New-ResearchFileDataset.ps1 -OutputDir .\artifacts\research-sync\file-dataset -TotalFiles 300 -DuplicateCount 60
```

### 2. Chay mot scenario don

```powershell
.\scripts\research-sync\Invoke-ResearchSyncScenario.ps1 \
  -ScenarioName R1 \
  -NetworkProfile LAN \
  -RecordCount 100 \
  -Repetitions 3 \
  -EdgeBaseUrl http://localhost:5001 \
  -InternalApiKey <EDGE_INTERNAL_API_KEY>
```

### 3. Chay campaign record day du

```powershell
.\scripts\research-sync\Run-ResearchSyncCampaign.ps1 \
  -EdgeBaseUrl http://localhost:5001 \
  -Preset FullMatrix
```

### 4. Chay smoke test live tren may local

```powershell
.\scripts\research-sync\Invoke-ResearchSyncScenario.ps1 \
  -ScenarioName smoke-live \
  -NetworkProfile LAN \
  -RecordCount 10 \
  -Repetitions 1 \
  -EdgeBaseUrl http://localhost:5001
```

### 5. File tong hop phuc vu bang ket qua luan van

Sau khi chay xong campaign, script se tu dong sinh them:

`artifacts/research-sync/campaign-*/thesis-summary.csv`

Bang nay tong hop theo tung cap `network_profile x record_count`, gom:

1. so lan lap
2. success rate
3. queue drain time trung binh / min / max
4. trigger latency trung binh / max
5. retry count trung binh / max
6. CPU va bo nho trung binh cua container chinh trong luc do

## Dieu kien pass cua bo nghien cuu toi thieu

1. Moi scenario sinh ra duoc bang chung lap lai va file ket qua rieng.
2. Queue drain time va trigger latency duoc xuat ra CSV/JSON.
3. Co the doi chieu metric voi log Edge, log Shore va `sync_queue`.
4. Co dataset file co manifest, checksum va danh dau file trung lap.

## Gioi han hien tai can viet ro trong bao cao

1. Harness toi thieu hien dung Toxiproxy, chua phai Linux `tc netem`.
2. Packet loss fidelity va HF chi la profile mo phong toi thieu, chua phai do dac thuc dia.
3. Monitoring hien dung `docker stats` va log; Prometheus/Grafana la buoc nang cap tiep theo.
4. Record benchmark hien nham vao `crew_member` va `sync_queue`, nghia la do duong sync that, nhung chua phu toan bo domain.

## Huong nang cap sau khi co so lieu vong 1

1. Thay Toxiproxy bang Linux `tc netem`.
2. Them Prometheus/Grafana stack.
3. Them k6 hoac JMeter cho burst va concurrent load.
4. Them pgBench va `EXPLAIN ANALYZE` cho DB path.
5. Them file sync end-to-end cho `crew_certificate`, `travel_document`, `health_document`.