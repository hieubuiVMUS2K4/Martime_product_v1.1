# Phase 2 Deploy Verify Checklist

## Muc tieu

Checklist nay dung de xac minh signed sync Phase 2 da thuc su duoc enforce sau deploy, khong chi ton tai trong code hoac appsettings.

## Dieu kien truoc khi test

- Shore backend da chay voi `SyncSecurity__RequireSignedRequests=true`.
- Edge da chay voi `SyncSecurity__Enabled=true`, `SyncSecurity__NodeId`, `SyncSecurity__SigningKey`, `SyncSecurity__ProtocolVersion=2`.
- Edge da chay voi `SyncSecurity__KeyVersion` khop voi key version dang active tren Shore.
- Node Edge da duoc provision tren Shore bang signing key trung khop.
- Da co `INTERNAL_API_KEY` cho Shore va `EDGE_INTERNAL_API_KEY` cho Edge de goi cac endpoint operational.

## Bien moi truong can co

### Shore

- `INTERNAL_API_KEY`
- `SYNC_REQUIRE_SIGNED_REQUESTS=true`
- `SYNC_PROTOCOL_VERSION=2`

### Edge

- `EDGE_INTERNAL_API_KEY`
- `EDGE_SYNC_SECURITY_ENABLED=true`
- `EDGE_SYNC_PROTOCOL_VERSION=2`
- `EDGE_SYNC_NODE_ID=<node-id-da-provision>`
- `EDGE_SYNC_SIGNING_KEY=<signing-key-da-provision>`
- `EDGE_SYNC_KEY_VERSION=<key-version-dang-active-tren-shore>`

## Lenh ho tro doc log nhanh

Neu deploy bang Docker Compose, mo hai cua so log rieng trong luc test:

```powershell
docker logs -f shore-backend 2>&1 | Select-String 'sync|signature|replay|revoked|heartbeat'
docker logs -f maritime-edge-collector 2>&1 | Select-String 'sync|triggered|accepted|failed|heartbeat'
```

Neu chay bang `dotnet run`, tim nhanh cac chuoi can theo doi:

```powershell
Get-Content .\logs\*.log -Wait | Select-String 'Missing sync security headers|Invalid sync signature|Unknown sync node|Replay detected|Sync node has been revoked'
```

## Buoc 1. Xac minh Shore dang enforce signed sync

Chay tren may Shore hoac tu host co the vao Shore backend:

```powershell
$shoreBase = 'http://localhost:5000'
$internalKey = 'REPLACE_WITH_INTERNAL_API_KEY'

Invoke-RestMethod `
  -Method Get `
  -Uri "$shoreBase/api/sync/dashboard/nodes" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```

Ket qua mong doi:

- API tra ve 200.
- Node can test xuat hien trong danh sach hoac co the duoc tao/provision o buoc sau.

## Buoc 2. Provision hoac rotate signing key cho node tren Shore

```powershell
$nodeId = 'REPLACE_WITH_EDGE_SYNC_NODE_ID'
$signingKey = 'REPLACE_WITH_EDGE_SYNC_SIGNING_KEY'

Invoke-RestMethod `
  -Method Put `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey } `
  -ContentType 'application/json' `
  -Body (@{
    shipName = 'MV TEST NODE'
    imoNumber = $nodeId
    signingKey = $signingKey
    keyVersion = 1
    previousKeyGraceMinutes = 1440
  } | ConvertTo-Json)
```

Co the luu response de doi chieu nhanh:

```powershell
$securityState = Invoke-RestMethod `
  -Method Put `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey } `
  -ContentType 'application/json' `
  -Body (@{
    shipName = 'MV TEST NODE'
    imoNumber = $nodeId
    signingKey = $signingKey
    keyVersion = 1
    previousKeyGraceMinutes = 1440
  } | ConvertTo-Json)

$securityState | Format-List nodeId,isRegistered,isRevoked,keyVersion,previousKeyVersion,previousKeyGraceUntil,hasSigningKey
```

Ket qua mong doi:

- `isRegistered = true`
- `isRevoked = false`
- `keyVersion` tang dung nhu mong doi
- `hasSigningKey = true`
- Neu dang rotate key, `previousKeyVersion` va `previousKeyGraceUntil` phai co gia tri.

## Buoc 3. Xem state bao mat cua node tren Shore

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```

Ket qua mong doi:

- `hasSigningKey = true`
- `isRegistered = true`
- `isRevoked = false`
- Khi da doi key tren Edge thanh cong, `lastAcknowledgedKeyVersion` phai bang `keyVersion` dang active.

## Buoc 4. Kiem tra Edge co the goi heartbeat signed

Cho background worker chay mot chu ky, hoac trigger sync bang tay de buoc Edge vua push vua pull.

```powershell
$edgeBase = 'http://localhost:5001'
$edgeInternalKey = 'REPLACE_WITH_EDGE_INTERNAL_API_KEY'

Invoke-RestMethod `
  -Method Get `
  -Uri "$edgeBase/api/sync/status" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }

Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/trigger" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }
```

Co the bat loi ro hon bang `try/catch`:

```powershell
try {
  $triggerResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$edgeBase/api/sync/trigger" `
    -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }

  $triggerResponse | Format-List *
}
catch {
  $_.Exception.Response | Format-List *
  throw
}
```

Ket qua mong doi:

- Edge `trigger` tra ve 200.
- Log Edge co `Manual sync triggered via API`.
- Log Shore khong co loi `Missing sync security headers`, `Invalid sync signature`, `Unknown sync node`.
- Neu system da co signed request di qua, `lastSignedRequestAt` cua node se thay doi o buoc 5.

## Buoc 5. Xac minh Shore da nhan signed requests

Sau khi Edge trigger xong, chay lai:

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```

Ket qua mong doi:

- `lastSignedRequestAt` da co gia tri moi.
- `lastKeyRotatedAt` van hop le.
- `lastAcknowledgedKeyVersion` phai bang `keyVersion` khi Edge da chay voi key moi.
- Neu co heartbeat/pull vua xay ra, dashboard node view se co `lastHeartbeat` hoac `pull.lastAt` moi.
- Khong co log Shore chua `Unsupported sync protocol version`, `Sync payload hash mismatch`, `Replay detected for sync request`.

## Buoc 6. Kiem tra push path Edge -> Shore

Neu can tao du lieu pending de test push, co the queue snapshot roi trigger sync:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/snapshot-crew" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }

Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/trigger" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }
```

Ket qua mong doi:

- Edge log co `Posting batch of X items`.
- Shore log nhan `POST /api/sync` thanh cong.
- Edge log co `Shore accepted batch`.
- Khong co log Edge chua `Shore sync failed` hoac `Invalid sync signature`.

## Buoc 7. Kiem tra pull va acknowledge path Shore -> Edge

1. Queue du lieu tu Shore cho node can test.
2. Trigger Edge sync.

Neu can resync cho node:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$shoreBase/api/sync/dashboard/resync/$nodeId" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```

Sau do:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/trigger" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }
```

Ket qua mong doi:

- Shore log co pull thanh cong cho node.
- Shore log hoac outbox state the hien ACK da duoc nhan.
- Edge khong gap `Shore pull failed`.
- Neu can doi chieu nhanh, `lastSignedRequestAt` tiep tuc duoc cap nhat sau dot pull/ack.

## Buoc 8. Kiem tra revoke path

Thu revoke node va trigger sync lai:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/revoke" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey } `
  -ContentType 'application/json' `
  -Body (@{ reason = 'Phase 2 revoke verification' } | ConvertTo-Json)

Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/trigger" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }
```

Co the doc nhanh state sau revoke:

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey } | Format-List nodeId,isRegistered,isRevoked,revokedAt,revokedReason
```

Ket qua mong doi:

- Shore tu choi signed sync cua node bi revoke.
- Edge log nhan response loi tu Shore thay vi bao accepted.
- Shore `GetNodeSecurity` tra ve `isRevoked = true`.
- Log Shore co the xuat hien `Sync node has been revoked`.

## Buoc 9. Reactivate node va xac minh hoi phuc

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/activate" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }

Invoke-RestMethod `
  -Method Post `
  -Uri "$edgeBase/api/sync/trigger" `
  -Headers @{ 'X-Internal-Api-Key' = $edgeInternalKey }
```

Ket qua mong doi:

- Node quay lai `isRegistered = true`, `isRevoked = false`.
- Edge sync lai thanh cong.
- `lastSignedRequestAt` tiep tuc tang lai sau khi trigger sync lai.

## Buoc 10. Kiem tra rotate key, grace window va rollback

Provision key moi cho cung node:

```powershell
$newSigningKey = 'REPLACE_WITH_NEW_EDGE_SYNC_SIGNING_KEY'

Invoke-RestMethod `
  -Method Put `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/security" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey } `
  -ContentType 'application/json' `
  -Body (@{
    shipName = 'MV TEST NODE'
    imoNumber = $nodeId
    signingKey = $newSigningKey
    keyVersion = 2
    previousKeyGraceMinutes = 60
  } | ConvertTo-Json)
```

Ket qua mong doi:

- Shore tra ve `keyVersion = 2`.
- `previousKeyVersion = 1` va `previousKeyGraceUntil` ton tai.
- Neu Edge van chay key cu trong grace window, sync van duoc chap nhan.

Sau khi cap nhat Edge sang `EDGE_SYNC_SIGNING_KEY=$newSigningKey`, `EDGE_SYNC_KEY_VERSION=2` va trigger sync lai:

- `lastAcknowledgedKeyVersion = 2`.
- Khong co log `Invalid sync signature`.

Neu can rollback trong grace window:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "$shoreBase/api/sync/dashboard/nodes/$nodeId/rollback-key" `
  -Headers @{ 'X-Internal-Api-Key' = $internalKey }
```
 
Ket qua mong doi:

- Shore tra ve `rolledBack = true`.
- `keyVersion` quay lai version truoc do.
- Edge dung key cu sync lai duoc neu rollback duoc kich hoat truoc khi grace het han.

## Dau hieu fail va cach doc nhanh

- `Missing sync security headers`: Edge chua bat signing hoac request khong di qua signed path.
- `Unsupported sync protocol version`: Shore va Edge dang lech `ProtocolVersion`.
- `Unknown sync node`: Shore chua provision node hoac node ID khong khop.
- `Sync node has been revoked`: revoke dang hoat dong dung.
- `Invalid sync signature`: signing key hai ben khong khop.
- `Sync payload hash mismatch`: payload bi sua doi hoac body hash bi tinh sai.
- `Replay detected for sync request`: nonce bi tai su dung.
- `Invalid sync key version`: Edge dang gui `EDGE_SYNC_KEY_VERSION` sai hoac chua doi version sau rotation.

## Dieu kien pass cuoi cung

De xem la deploy Phase 2 hop le, can dat du 5 diem sau:

1. Shore node security state hien `hasSigningKey = true`, `isRegistered = true`, `isRevoked = false` truoc khi test luong chinh.
2. Edge `POST /api/sync/trigger` thanh cong khi key dung.
3. Shore `lastSignedRequestAt` cap nhat sau sync.
4. Sau revoke, Edge khong sync duoc.
5. Sau activate, Edge sync lai duoc ma khong can tat enforcement.
6. Sau dot rotate key, `lastAcknowledgedKeyVersion` bang version moi hoac rollback key thanh cong neu can.