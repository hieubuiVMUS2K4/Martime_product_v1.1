#!/usr/bin/env pwsh

# Test script để kiểm tra API endpoint chat AI đã hoạt động chưa

$apiUrl = "http://localhost:5000/api/reports/chat/vessels"
$vesselId = "9876543-0000-0000-0000-000000000001"  # Thay bằng ID tàu thực tế

$body = @{
    message = "Phân tích báo cáo hôm nay"
    vesselId = $vesselId
} | ConvertTo-Json

Write-Host "📤 Gửi yêu cầu đến API chat..."
Write-Host "URL: $apiUrl/$vesselId"
Write-Host "Body: $body"
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "$apiUrl/$vesselId" `
        -Method POST `
        -Headers @{ "Content-Type" = "application/json" } `
        -Body $body `
        -SkipCertificateCheck

    Write-Host "✅ Thành công! Status: $($response.StatusCode)"
    Write-Host ""
    Write-Host "📦 Response:"
    $response.Content | ConvertFrom-Json | Format-List
}
catch {
    Write-Host "❌ Lỗi: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)"
        Write-Host "Response Body: $($_.Exception.Response | Get-Member)"
    }
}
