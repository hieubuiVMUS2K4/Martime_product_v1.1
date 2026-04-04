# Phase 3 Metric Collection Runbook

## Muc tieu

Runbook nay chot cach thu thap metric va bang chung cho Phase 3 ma khong sua runtime code. Muc tieu la moi thanh vien trong team co the lap lai mot scenario va thu duoc bang chung cung format.

## 1. Truoc khi chay scenario

- Ghi lai commit hash cua `feature/tinht` dang deploy.
- Ghi lai env version: protocol version, key version, node id, ngay deploy.
- Ghi lai network profile dang dung: LAN, 4G, VSAT hoac degraded.
- Chup state node security tu Shore dashboard endpoint.

## 2. Nguon metric chinh

| Nguon | Du lieu lay |
|---|---|
| Shore logs | request accepted/rejected, pull, ack, heartbeat |
| Edge logs | trigger, batch post, pull apply, retry, heartbeat |
| `audit_logs` | missing header, invalid signature, replay, revoked node, payload mismatch |
| `sync_node_trackers` | lastSignedRequestAt, keyVersion, lastAcknowledgedKeyVersion, revoked state |
| Host monitoring | CPU, RAM, network bandwidth |

## 3. Lenh ho tro doc log

### 3.1. Docker logs

```powershell
docker logs -f shore-backend 2>&1 | Select-String 'sync|signature|replay|revoked|heartbeat|payload'
docker logs -f maritime-edge-collector 2>&1 | Select-String 'sync|triggered|accepted|failed|heartbeat|queue'
```

### 3.2. Dotnet run logs

```powershell
Get-Content .\logs\*.log -Wait | Select-String 'Missing sync security headers|Invalid sync signature|Unknown sync node|Replay detected|Sync node has been revoked|payload hash mismatch'
```

## 4. Lenh truy van bang chung tren Shore

### 4.1. Node security state

```powershell
$shoreBase = 'http://localhost:5000'
$internalKey = 'REPLACE_WITH_INTERNAL_API_KEY'
$nodeId = 'REPLACE_WITH_EDGE_SYNC_NODE_ID'

Invoke-RestMethod `
  -Method Get `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```

### 4.2. Audit log trich xuat

```powershell
psql "$env:SHORE_DB_CONNECTION" -c "
select created_at, action, entity_type, entity_id, source_channel, correlation_id, details
from audit_logs
where entity_type = 'SyncSecurity'
order by created_at desc
limit 50;"
```

### 4.3. Migration va version state

```powershell
Set-Location 'f:\NCKH\Product\Martime_product_v1.1\shore_product\backend'
dotnet ef migrations list --project .\product-api.csproj
```

## 5. Bang mau ghi metric

| Timestamp | Scenario | Network | Dataset | P50 | P95 | P99 | Throughput | Success % | Reject count | Queue drain (s) |
|---|---|---|---|---|---|---|---|---|---|---|

## 6. Cach tinh metric

### 6.1. Latency

- P50: trung vi cua thoi gian hoan thanh request/batch.
- P95: moc 95 phan tram cua phan bo latency.
- P99: moc 99 phan tram cua phan bo latency.

### 6.2. Throughput

- Batch throughput = tong so batch thanh cong / tong thoi gian scenario.
- Item throughput = tong so item dong bo thanh cong / tong thoi gian scenario.

### 6.3. Success va reject rate

- Success rate = so request thanh cong / tong request.
- Reject rate = so reject dung ky vong / tong attempt negative.

## 7. Quy tac dat ten bang chung

- Log Shore: `phase3_<scenario>_<network>_shore.log`
- Log Edge: `phase3_<scenario>_<network>_edge.log`
- Audit export: `phase3_<scenario>_<network>_audit.csv`
- Metric table: `phase3_<scenario>_<network>_metrics.csv`

## 8. Ket thuc moi scenario

- Luu tat ca log va CSV vao cung mot thu muc evidence.
- Ghi ro pass/fail va nguyen nhan.
- Neu scenario la security negative test, chup lai reject reason trong `audit_logs`.
- Neu scenario thay doi state node, khoi phuc ve state clean truoc khi sang scenario tiep theo.

## 9. Dieu kien de metric duoc xem la hop le

- Scenario chay tren ban code da freeze.
- Network profile duoc ghi lai ro rang.
- Co it nhat 3 lan lap lai voi scenario quan trong.
- Bang ket qua co the doi chieu voi Shore log, Edge log va `audit_logs`.