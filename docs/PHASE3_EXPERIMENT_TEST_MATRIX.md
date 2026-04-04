# Phase 3 Experiment Test Matrix

## Muc tieu

Tai lieu nay chot bo scenario de thu thap bang chung thuc nghiem tren ban Shore/Edge da deploy va da freeze. Moi scenario phai chay tren cung mot commit, cung env va cung node provisioning state da duoc ghi nhan trong `ONE_DEPLOY_FREEZE_CHECKLIST`.

## 1. Nguyen tac thuc hien

- Chay baseline truoc, moi scenario tan cong sau.
- Sau moi scenario, khoi phuc ve trang thai clean hoac provision lai node neu can.
- Moi lan chay phai luu: timestamp, commit hash, env version, node id, key version, network profile.
- Khong sua runtime code trong suot dot do metric.

## 2. Cau hinh testbed toi thieu

| Thanh phan | So luong toi thieu | Ghi chu |
|---|---|---|
| Shore backend | 1 | ban da deploy va freeze |
| Edge node | 2 | 1 node hop le, 1 node dung cho negative scenario |
| PostgreSQL | 2 | Shore DB va Edge DB theo deployment that |
| Network profile | 4 | LAN, 4G, VSAT, degraded |
| Dataset | 4 nhom | crew, certificate, voyage/report, file sync |

## 3. Profile mang de lap lai

| Profile | RTT (ms) | Jitter (ms) | Bandwidth | Packet loss (%) | Muc dich |
|---|---|---|---|---|---|
| LAN | 1 | 0.1 | 1 Gbps | 0.0 | baseline local lab |
| 4G | 45 | 12 | 20 Mbps | 0.1 | coastal operation |
| VSAT | 690 | 85 | 2 Mbps | 1.2 | offshore practical |
| Degraded | 8500 | 2100 | 256 kbps | 22.4 | storm / unstable link |

## 4. Benchmark dataset

| Dataset | Khoi luong goi y | Noi dung |
|---|---|---|
| Crew basic | 100 ban ghi | crew profile + master data lien quan |
| Crew compliance | 200 ban ghi | certificate, travel docs, health docs |
| Voyage/report | 100 ban ghi | reports, voyage mirror, planning subset |
| File sync | 20 tep | avatar, certificate scan, travel document |

## 5. Test matrix

| ID | Nhom | Scenario | Muc tieu | Cach thuc hien | Ky vong | Metric chinh |
|---|---|---|---|---|---|---|
| B1 | Baseline | Sync hop le tren LAN | Xac nhan he thong chay dung khi khong co tan cong | Provision node hop le, trigger push/pull/ack | Batch thanh cong, khong co reject | success rate, P50/P95/P99 latency |
| B2 | Baseline | Sync hop le tren 4G | Do overhead signed sync trong mang thuc te | Lap lai B1 tren profile 4G | Thanh cong, latency tang nhung chap nhan duoc | sync latency, throughput |
| B3 | Baseline | Sync hop le tren VSAT | Do resilience va batch sizing | Trigger sync voi batch 10/50/100 | Batch lon tang latency, batch nho van thanh cong | latency theo batch, recovery time |
| B4 | Baseline | Offline dai ngay roi hoi phuc | Do eventual consistency | Edge queue du lieu khi mat mang, phuc hoi mang roi trigger sync | Hang doi duoc day len Shore, ack day du | time to consistency, success rate |
| S1 | Security | Replay batch cu | Kiem chung anti-replay | Gui lai cung signed request/nonce | Shore reject, co audit `replay_detected` | replay rejection rate |
| S2 | Security | Sua payload sau khi ky | Kiem chung payload integrity | Giua nguyen header/signature, sua body | Shore reject | invalid signature rate, payload hash mismatch count |
| S3 | Security | Sua file checksum | Kiem chung file integrity | Sua `FileData` hoac `FileChecksumSha256` | Node nhan khong ghi file, request fail | file tamper detection rate |
| S4 | Security | Gia mao node khac | Kiem chung ownership binding | Gui signed request cua node A nhung body/query node B | Shore reject hoac normalize theo node da verify | spoofing rejection rate |
| S5 | Security | Dung node da revoke | Kiem chung revoke path | Revoke node roi trigger sync lai | Shore tra 403 | revoke enforcement rate |
| S6 | Security | Dung key cu trong grace | Kiem chung staged rotation | Rotate key, giu Edge o key cu trong grace | Sync duoc chap nhan trong grace | transition success rate |
| S7 | Security | Dung key cu sau grace | Kiem chung grace boundary | Het grace roi trigger bang key cu | Shore reject | expired-key rejection rate |
| S8 | Security | Rollback key co kiem soat | Kiem chung rollback path | Rotate key, rollback, trigger sync bang key cu | Sync hoi phuc neu rollback hop le | rollback recovery time |
| O1 | Operations | Burst read on sync dashboard | Do tac dong observability | Goi lap lai endpoint trang thai/dashboard | Rate limit hoat dong, khong sap he thong | P95 latency, 429 ratio |
| O2 | Operations | Sync queue saturation | Do hanh vi khi backlog cao | Tao backlog 500 item roi sync duoi VSAT | Hoan tat theo nhieu batch, khong mat du lieu | throughput, queue drain time |

## 6. Metric can thu thap

### 6.1. Metric thanh cong

| Metric | Cong thuc | Nguon |
|---|---|---|
| Sync success rate | so batch thanh cong / tong so batch | Edge log, Shore log |
| Replay rejection rate | so replay reject / tong replay attempt | `audit_logs` |
| Invalid signature rejection rate | so invalid signature reject / tong tamper attempt | `audit_logs` |
| Time to consistency | thoi gian tu luc du lieu duoc queue den luc Shore/Edge dong bo xong | queue state + timestamps |

### 6.2. Metric hieu nang

| Metric | Don vi | Ghi chu |
|---|---|---|
| P50/P95/P99 sync latency | ms | tinh theo scenario va network profile |
| Throughput | item/s hoac batch/s | tach theo push, pull, ack |
| Queue drain time | s | quan trong cho offline recovery |
| CPU Shore/Edge | % | lay theo dot 30s hoac 60s |
| RAM Shore/Edge | MB | lay cung thoi diem burst |
| Message overhead | bytes, % | so sanh signed sync voi baseline khong signing neu can |

## 7. Bang chung phai luu cho moi scenario

- Log Shore.
- Log Edge.
- Trich xuat `audit_logs` theo `correlationId`, `reason`, `node`.
- Snapshot node security state truoc va sau scenario.
- Anh chup hoac export metric CPU, RAM, network condition.
- Bang ket qua CSV/Markdown cho P50, P95, P99, throughput, rejection rate.

## 8. Mau bang ghi ket qua

| Scenario ID | Network | Dataset | Key version | Result | P50 | P95 | P99 | Success % | Reject count | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| B1 | LAN | Crew basic | 1 | Pass | 0 | 0 | 0 | 0 | 0 | |

## 9. Dieu kien pass cua dot Phase 3 dau tien

- Toan bo baseline B1-B4 chay thanh cong.
- S1-S5 co bang chung reject dung theo ky vong.
- S6-S8 xac nhan key rotation grace va rollback khong can tat enforcement.
- Co du du lieu de dien bang P50/P95/P99, throughput, queue drain time, replay rejection rate, invalid signature rejection rate.

## 10. Thu tu khuyen nghi khi chay

1. B1 -> B4.
2. S1 -> S5.
3. S6 -> S8.
4. O1 -> O2.

Thu tu nay giup co baseline truoc, sau do moi chay negative test va operational stress test.