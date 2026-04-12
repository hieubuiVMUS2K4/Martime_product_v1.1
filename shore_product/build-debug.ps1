#!/usr/bin/env pwsh
param(
    [switch]$Clean = $false
)

Set-Location "F:\NCKH\Product\Martime_product_v1.1\shore_product\backend"

if ($Clean) {
    Write-Host "=== CLEANING ===" -ForegroundColor Green
    Remove-Item -Recurse -Force ".\bin" -ErrorAction SilentlyContinue | Out-Null
    Remove-Item -Recurse -Force ".\obj" -ErrorAction SilentlyContinue | Out-Null
    Remove-Item -Recurse -Force ".\artifacts" -ErrorAction SilentlyContinue | Out-Null
    dotnet clean | Out-Null
    Write-Host "✓ Cleaned" -ForegroundColor Green
}

Write-Host "=== BUILDING ===" -ForegroundColor Cyan
$buildOutput = dotnet build product-api.csproj -c Release 2>&1
$errorCount = ($buildOutput | Select-String "error MSB" | Measure-Object).Count
$buildSuccessMsg = $buildOutput | Select-String "Build succeeded|Build failed|error.*|[0-9]* Error"

Write-Host "Error count: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host "Build output summary:" -ForegroundColor Cyan
$buildSuccessMsg | ForEach-Object { Write-Host $_ }

if ($errorCount -eq 0) {
    Write-Host "`n✓ BUILD SUCCESSFUL" -ForegroundColor Green
} else {
    Write-Host "`n✗ BUILD FAILED with $errorCount errors" -ForegroundColor Red
    Write-Host "`nFirst error:" -ForegroundColor Yellow
    ($buildOutput | Select-String "error" | Select-Object -First 1) | Write-Host
}

exit $(if ($errorCount -eq 0) { 0 } else { 1 })
