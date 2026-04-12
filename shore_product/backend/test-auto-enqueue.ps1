# Test script: Verify auto-enqueue logic in ProcessBatchAsync

Write-Host "=== Testing Auto-Enqueue Logic ===" -ForegroundColor Cyan

# 1. Get current status
Write-Host "`n1. Current Status:" -ForegroundColor Yellow
$status1 = (Invoke-WebRequest -Uri "http://localhost:5000/api/reports/debug/status" -UseBasicParsing).Content | ConvertFrom-Json
Write-Host "   Noon Reports: $($status1.noonReportsTotal)"
Write-Host "   Evaluations: $($status1.evaluationsTotal)"

# 2. Create test sync batch with a new NoonReport
Write-Host "`n2. Simulating Sync Batch with New NoonReport..." -ForegroundColor Yellow

$noonReportId = [guid]::NewGuid()
$maritimeReportId = [guid]::NewGuid()
$now = (Get-Date).ToUniversalTime().ToString("o")

$noonReportPayload = @{
    Id = $noonReportId
    MaritimeReportId = $maritimeReportId
    ReportDate = $now
    Latitude = 12.5
    Longitude = 104.8
    CourseOverGround = 180
    SpeedOverGround = 15.5
    DistanceTraveled = 300
    AirTemperature = 28
    SeaTemperature = 26
    BarometricPressure = 1013
    FuelOilConsumed = 22
    DieselOilConsumed = 3
    MainEngineRPM = 95
    CargoOnBoard = 25000
    OperationalRemarks = "Normal operations"
} | ConvertTo-Json

$maritimeReportPayload = @{
    Id = $maritimeReportId
    ReportNumber = "TEST-AUTO-ENQUEUE-001"
    ReportTypeId = 1
    ReportDateTime = $now
    Status = "TRANSMITTED"
    ReportData = $noonReportPayload
    OriginNode = "9876543"
    IsTransmitted = $true
} | ConvertTo-Json

$batchRequest = @(
    @{
        TableName = "maritime_report"
        RecordKey = $maritimeReportId.ToString()
        ActionType = "CREATE"
        Payload = $maritimeReportPayload
        SyncVersion = 1
        OriginNode = "9876543"
    },
    @{
        TableName = "noon_report"
        RecordKey = $noonReportId.ToString()
        ActionType = "CREATE"
        Payload = $noonReportPayload
        SyncVersion = 1
        OriginNode = "9876543"
    }
) | ConvertTo-Json

Write-Host "   Created test batch for: $($noonReportId)"

# 3. Send sync request
Write-Host "   Sending to /api/sync endpoint..."
try {
    $syncResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/sync" `
        -Method POST `
        -ContentType "application/json" `
        -Body $batchRequest `
        -UseBasicParsing
    
    $syncResult = $syncResponse.Content | ConvertFrom-Json
    Write-Host "   ✓ Sync completed. Succeeded: $($syncResult.Succeeded), Failed: $($syncResult.Failed)"
} catch {
    Write-Host "   ✗ Sync failed: $_" -ForegroundColor Red
    exit
}

# 4. Wait for backend to process and enqueue
Write-Host "`n3. Waiting 15s for backend to process batch and auto-enqueue..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# 5. Verify enqueued
Write-Host "`n4. Checking if report was auto-enqueued..." -ForegroundColor Yellow
$status2 = (Invoke-WebRequest -Uri "http://localhost:5000/api/reports/debug/status" -UseBasicParsing).Content | ConvertFrom-Json
Write-Host "   Noon Reports: $($status2.noonReportsTotal)"
Write-Host "   Evaluations: $($status2.evaluationsTotal)"

if ($status2.noonReportsTotal -gt $status1.noonReportsTotal) {
    Write-Host "`n✓ SUCCESS: New report created!" -ForegroundColor Green
    if ($status2.evaluationsTotal -gt $status1.evaluationsTotal) {
        Write-Host "✓ SUCCESS: Auto-enqueue worked! Evaluation created!" -ForegroundColor Green
    } else {
        Write-Host "⚠ WARNING: Report created but evaluation not yet created (worker may still be processing)" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n✗ FAILED: Report not created" -ForegroundColor Red
}

# 6. Show latest evaluations
Write-Host "`n5. Latest Evaluations:" -ForegroundColor Yellow
$status2.latestEvaluations | ForEach-Object {
    Write-Host "   Report: $($_.reportId) | Status: $($_.status)"
}
