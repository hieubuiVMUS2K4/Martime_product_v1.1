# Test i18n implementation
Write-Host "🌍 Testing i18n Implementation..." -ForegroundColor Cyan

$projectPath = "f:\NCKH\Product\Martime_product_v1.1\frontend-mobile"
Set-Location $projectPath

Write-Host "`n✅ Step 1: Running flutter pub get to generate l10n code..." -ForegroundColor Yellow
flutter pub get

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ flutter pub get failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Step 2: Checking generated l10n files..." -ForegroundColor Yellow
$genPath = ".dart_tool\flutter_gen\gen_l10n"
if (Test-Path $genPath) {
    Write-Host "✅ Found generated l10n files:" -ForegroundColor Green
    Get-ChildItem $genPath -Filter "*.dart" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Green
    }
} else {
    Write-Host "❌ Generated l10n files not found!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Step 3: Checking .arb files..." -ForegroundColor Yellow
$arbFiles = Get-ChildItem "lib\l10n" -Filter "*.arb"
Write-Host "✅ Found $($arbFiles.Count) language files:" -ForegroundColor Green
foreach ($file in $arbFiles) {
    $content = Get-Content $file.FullName | ConvertFrom-Json
    $locale = $content.'@@locale'
    $keyCount = ($content.PSObject.Properties | Where-Object { $_.Name -notlike '@*' -and $_.Name -ne '@@locale' }).Count
    Write-Host "  - $locale`: $keyCount keys" -ForegroundColor Green
}

Write-Host "`n✅ Step 4: Verifying key consistency..." -ForegroundColor Yellow
$enKeys = (Get-Content "lib\l10n\app_en.arb" | ConvertFrom-Json).PSObject.Properties | Where-Object { $_.Name -notlike '@*' -and $_.Name -ne '@@locale' } | Select-Object -ExpandProperty Name
$viKeys = (Get-Content "lib\l10n\app_vi.arb" | ConvertFrom-Json).PSObject.Properties | Where-Object { $_.Name -notlike '@*' -and $_.Name -ne '@@locale' } | Select-Object -ExpandProperty Name
$filKeys = (Get-Content "lib\l10n\app_fil.arb" | ConvertFrom-Json).PSObject.Properties | Where-Object { $_.Name -notlike '@*' -and $_.Name -ne '@@locale' } | Select-Object -ExpandProperty Name

$missingInVi = $enKeys | Where-Object { $_ -notin $viKeys }
$missingInFil = $enKeys | Where-Object { $_ -notin $filKeys }

if ($missingInVi.Count -eq 0 -and $missingInFil.Count -eq 0) {
    Write-Host "✅ All keys are consistent across all languages!" -ForegroundColor Green
} else {
    if ($missingInVi.Count -gt 0) {
        Write-Host "⚠️  Missing in Vietnamese: $($missingInVi -join ', ')" -ForegroundColor Yellow
    }
    if ($missingInFil.Count -gt 0) {
        Write-Host "⚠️  Missing in Filipino: $($missingInFil -join ', ')" -ForegroundColor Yellow
    }
}

Write-Host "`n✅ Step 5: Building project to verify no errors..." -ForegroundColor Yellow
flutter build windows --debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Please check the errors above." -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 i18n Implementation Test PASSED!" -ForegroundColor Green
Write-Host "`nℹ️  Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: flutter run -d windows" -ForegroundColor White
Write-Host "  2. Go to Settings → Language Settings" -ForegroundColor White
Write-Host "  3. Change language and verify UI updates" -ForegroundColor White
Write-Host "  4. Check MIGRATION_GUIDE.md to update remaining screens" -ForegroundColor White
