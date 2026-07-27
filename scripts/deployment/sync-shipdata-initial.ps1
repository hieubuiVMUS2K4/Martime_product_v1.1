# Script để đồng bộ ship_data lần đầu từ Edge → Shore

Write-Host "=== Đồng Bộ Ship Data Lần Đầu (Edge → Shore) ===" -ForegroundColor Cyan
Write-Host ""

# Bước 1: Initialize ShipData từ appsettings.json config
Write-Host "[1/3] Initializing ship data from config..." -ForegroundColor Yellow
try {
    $response1 = Invoke-WebRequest -Uri "http://localhost:5001/api/ship-data/initialize-from-config" `
        -Method POST `
        -UseBasicParsing `
        -ErrorAction Stop

    $result1 = $response1.Content | ConvertFrom-Json
    
    Write-Host "✅ Ship data initialized!" -ForegroundColor Green
    Write-Host "   IMO: $($result1.imo)" -ForegroundColor White
    Write-Host "   Name: $($result1.name)" -ForegroundColor White
    Write-Host "   Ship Data ID: $($result1.shipDataId)" -ForegroundColor White
    Write-Host "   Message: $($result1.message)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "❌ Unauthorized - Edge API requires authentication" -ForegroundColor Red
        Write-Host "   Solution: Remove [Authorize] attribute from InitializeFromConfig endpoint" -ForegroundColor Yellow
    }
    else {
        Write-Host "❌ Failed to initialize: $_" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Response.Content)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "Note: If initialization failed, ship data may already exist or there's a config issue." -ForegroundColor Yellow
    Write-Host "Continuing to trigger sync anyway..." -ForegroundColor Yellow
    Write-Host ""
}

# Bước 2: Trigger manual sync
Write-Host "[2/3] Triggering manual sync from Edge to Shore..." -ForegroundColor Yellow
try {
    $response2 = Invoke-WebRequest -Uri "http://localhost:5001/api/sync/trigger" `
        -Method POST `
        -UseBasicParsing `
        -ErrorAction Stop

    $result2 = $response2.Content | ConvertFrom-Json
    
    Write-Host "✅ Manual sync completed!" -ForegroundColor Green
    Write-Host "   Total Synced: $($result2.totalSynced)" -ForegroundColor White
    Write-Host "   Pending: $($result2.pendingRecords)" -ForegroundColor White
    Write-Host "   Message: $($result2.message)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "❌ Unauthorized - Edge API requires authentication" -ForegroundColor Red
        Write-Host "   Solution: Remove [Authorize] attribute from TriggerSync endpoint" -ForegroundColor Yellow
    }
    else {
        Write-Host "❌ Failed to trigger sync: $_" -ForegroundColor Red
    }
    Write-Host ""
    exit 1
}

# Bước 3: Verify trên Shore
Write-Host "[3/3] Checking Shore for vessel data..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $response3 = Invoke-WebRequest -Uri "http://localhost:5000/api/vessels" `
        -UseBasicParsing `
        -ErrorAction Stop

    $vessels = $response3.Content | ConvertFrom-Json
    
    if ($vessels.Count -gt 0) {
        Write-Host "✅ Shore has $($vessels.Count) vessel(s)!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Vessel Details:" -ForegroundColor Cyan
        foreach ($vessel in $vessels) {
            Write-Host "   IMO: $($vessel.imo)" -ForegroundColor White
            Write-Host "   Name: $($vessel.name)" -ForegroundColor White
            Write-Host "   Call Sign: $($vessel.callSign)" -ForegroundColor White
            Write-Host "   Type: $($vessel.vesselType)" -ForegroundColor White
            Write-Host "   Flag: $($vessel.flag)" -ForegroundColor White
            if ($vessel.originNode) {
                Write-Host "   Origin: $($vessel.originNode) (synced from Edge)" -ForegroundColor Green
            }
            Write-Host ""
        }
    }
    else {
        Write-Host "⚠️  Shore has no vessels yet" -ForegroundColor Yellow
        Write-Host "   This may mean:" -ForegroundColor Gray
        Write-Host "   - Sync is still in progress (check logs)" -ForegroundColor Gray
        Write-Host "   - Shore rejected the sync (check Shore logs for errors)" -ForegroundColor Gray
        Write-Host "   - ShipData wasn't queued for sync (check Edge SyncQueue)" -ForegroundColor Gray
    }
}
catch {
    Write-Host "❌ Failed to check Shore: $_" -ForegroundColor Red
    Write-Host "   Make sure Shore backend is running on port 5000" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Check Shore UI - Vessel Management to see the vessel" -ForegroundColor White
Write-Host "2. Edit vessel details in Edge - it will auto-sync to Shore" -ForegroundColor White
Write-Host "3. Check sync logs: GET http://localhost:5000/api/sync/status" -ForegroundColor White
Write-Host ""
Write-Host "Troubleshooting:" -ForegroundColor Yellow
Write-Host "- Edge logs (terminal): Check for [EDGE-SYNC] messages" -ForegroundColor Gray
Write-Host "- Shore logs (terminal): Check for ship_data processing" -ForegroundColor Gray
Write-Host "- Shore API: http://localhost:5000/api/sync/status (recent logs)" -ForegroundColor Gray
