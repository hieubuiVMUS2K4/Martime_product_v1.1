# ============================================================
# Script khởi động Edge (tàu) - chạy mỗi lần bật máy
# ============================================================

Write-Host "=== STARTING MARITIME EDGE SYSTEM ===" -ForegroundColor Cyan

# 1. Khởi động database (Docker)
Write-Host "`n[1/3] Starting Edge database..." -ForegroundColor Yellow
Set-Location "E:\NCKH\Martime_product_v1.1\edge_product\edge-services"
docker compose up -d edge-postgres
Start-Sleep -Seconds 5

# Chờ database sẵn sàng
$ready = $false
for ($i = 1; $i -le 15; $i++) {
    $check = docker exec maritime-edge-postgres pg_isready -U edge_user -d maritime_edge 2>&1
    if ($check -match "accepting connections") {
        Write-Host "  Database ready!" -ForegroundColor Green
        $ready = $true
        break
    }
    Write-Host "  Waiting for database... ($i/15)"
    Start-Sleep -Seconds 2
}
if (-not $ready) {
    Write-Host "  WARNING: Database may not be ready yet" -ForegroundColor Red
}

# 2. Khởi động Edge Server (.NET)
Write-Host "`n[2/3] Starting Edge Server on port 5001..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location 'E:\NCKH\Martime_product_v1.1\edge_product\edge-services'; Write-Host 'Edge Server running at http://localhost:5001' -ForegroundColor Green; dotnet run --project EdgeCollector.csproj --no-build --urls 'http://0.0.0.0:5001'"
) -WindowStyle Normal
Start-Sleep -Seconds 4

# 3. Khởi động Emulator Android
Write-Host "`n[3/3] Starting Android Emulator..." -ForegroundColor Yellow
$emulators = & "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -list-avds 2>&1
$firstEmulator = ($emulators | Where-Object { $_ -match '\w' } | Select-Object -First 1).Trim()
if ($firstEmulator) {
    Write-Host "  Starting emulator: $firstEmulator"
    Start-Process "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -ArgumentList "-avd", $firstEmulator -WindowStyle Normal
    Write-Host "  Waiting for emulator to boot (~30s)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
} else {
    Write-Host "  No emulator found. Open Android Studio and start emulator manually." -ForegroundColor Red
}

Write-Host "`n=== READY! ===" -ForegroundColor Green
Write-Host "Now run Flutter app with:" -ForegroundColor Cyan
Write-Host "  cd E:\NCKH\Martime_product_v1.1\edge_product\frontend-mobile" -ForegroundColor White
Write-Host "  flutter run -d emulator-5554" -ForegroundColor White
Write-Host ""
Write-Host "Or press Enter to run Flutter app now..."
$input = Read-Host
if ($input -eq "" -or $input -ne "n") {
    Set-Location "E:\NCKH\Martime_product_v1.1\edge_product\frontend-mobile"
    flutter run -d emulator-5554
}
