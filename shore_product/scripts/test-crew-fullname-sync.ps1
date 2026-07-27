# Test script to verify CrewMember FullName sync from Edge → Shore
# This simulates an Edge ship sending a crew member UPDATE with FullName change

$syncPayload = @{
    items = @(
        @{
            tableName = "crew_member"
            recordKey = "00000000-0000-0000-0000-000000000001"  # Replace with actual CrewMember ID
            actionType = "UPDATE"
            syncVersion = 2
            originNode = "9876543"
            payload = @{
                FullName = "Tran Duc Thanh 9999"
                UpdatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            } | ConvertTo-Json
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "=== Testing CrewMember FullName Sync ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Payload:" -ForegroundColor Yellow
$syncPayload | Write-Host
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/sync" `
        -Method POST `
        -ContentType "application/json" `
        -Body $syncPayload `
        -UseBasicParsing

    Write-Host "✅ Sync request successful!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Yellow
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10 | Write-Host
    Write-Host ""
    Write-Host "Check backend logs for:" -ForegroundColor Cyan
    Write-Host "  - 'CrewMember conflict resolution' log with timestamps" -ForegroundColor White
    Write-Host "  - 'CrewMember FullName change' log showing old → new value" -ForegroundColor White
    Write-Host "  - 'shouldApply=True' indicating the change was applied" -ForegroundColor White
}
catch {
    Write-Host "❌ Sync request failed!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure:" -ForegroundColor Yellow
    Write-Host "  1. Backend is running on http://localhost:5000" -ForegroundColor White
    Write-Host "  2. CrewMember with ID exists in database" -ForegroundColor White
    Write-Host "  3. Replace 'recordKey' with actual CrewMember.Id" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Query database to verify FullName was updated:" -ForegroundColor White
Write-Host '   SELECT "Id", "CrewId", "FullName", "UpdatedAt" FROM crew_members WHERE "FullName" LIKE ''%Tran Duc Thanh%'';' -ForegroundColor Gray
Write-Host ""
Write-Host "2. Check sync_logs table for processing details:" -ForegroundColor White
Write-Host '   SELECT * FROM sync_logs WHERE table_name = ''crew_member'' ORDER BY processed_at DESC LIMIT 10;' -ForegroundColor Gray
