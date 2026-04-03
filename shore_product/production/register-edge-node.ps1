# ============================================================
# register-edge-node.ps1
# Đăng ký node edge (tàu) trên shore server
# Chạy script này SAU KHI đã restart shore với .env mới
# ============================================================

$SHORE_URL       = "http://27.71.17.165"
$INTERNAL_KEY    = "ShoreInternal_NCKH_Maritime_Admin_2026_VietHoang"
$NODE_ID         = "8765432"
$SHIP_NAME       = "MV MEKONG SPIRIT"
$IMO             = "8765432"
$SIGNING_KEY     = "MekongSpirit_SyncKey_Node8765432_HMAC_SHA256_Maritime_NCKH_2026_Prod"
$KEY_VERSION     = 1

$body = @{
    shipName     = $SHIP_NAME
    imoNumber    = $IMO
    signingKey   = $SIGNING_KEY
    keyVersion   = $KEY_VERSION
} | ConvertTo-Json

Write-Host "Đăng ký node $NODE_ID trên $SHORE_URL ..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod `
        -Method Put `
        -Uri "$SHORE_URL/api/sync/dashboard/nodes/$NODE_ID/security" `
        -Headers @{ "X-Internal-Api-Key" = $INTERNAL_KEY } `
        -ContentType "application/json" `
        -Body $body

    Write-Host "✓ Đăng ký thành công!" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "✗ Lỗi: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  → Kiểm tra shore server đang chạy và .env đã được cập nhật" -ForegroundColor Yellow
}
