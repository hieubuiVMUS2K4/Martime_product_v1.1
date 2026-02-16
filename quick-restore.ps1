# ===================================================================
# Quick Restore Script - Đơn giản và nhanh
# ===================================================================
# Script nhanh để restore database từ ZIP file
# ===================================================================

param(
    [string]$ZipFile = ""
)

Write-Host "`n🔄 Quick Restore - Maritime Product" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Tìm ZIP file mới nhất nếu không chỉ định
if ([string]::IsNullOrEmpty($ZipFile)) {
    $ZipFile = Get-ChildItem "backup_*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
    
    if ([string]::IsNullOrEmpty($ZipFile)) {
        Write-Host "❌ Không tìm thấy file backup nào!" -ForegroundColor Red
        Write-Host "Sử dụng: .\quick-restore.ps1 -ZipFile 'backup_20240203_143022.zip'" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "📦 Found: $(Split-Path $ZipFile -Leaf)" -ForegroundColor Green
}

if (-not (Test-Path $ZipFile)) {
    Write-Host "❌ File không tồn tại: $ZipFile" -ForegroundColor Red
    exit 1
}

# Xác nhận
Write-Host "`n⚠️  CẢNH BÁO: Toàn bộ dữ liệu hiện tại sẽ bị xóa!" -ForegroundColor Yellow
Write-Host "Tiếp tục? (yes/no): " -NoNewline
$confirm = Read-Host

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""

# Extract
$tempDir = "temp_restore_$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "📂 Extracting..." -NoNewline
Expand-Archive -Path $ZipFile -DestinationPath $tempDir -Force
Write-Host " ✅" -ForegroundColor Green

# Tìm file SQL
$shoreFile = Get-ChildItem "$tempDir\*shore*.sql" | Select-Object -First 1 -ExpandProperty FullName
$edgeFile = Get-ChildItem "$tempDir\*edge*.sql" | Select-Object -First 1 -ExpandProperty FullName

if (-not $shoreFile -or -not $edgeFile) {
    Write-Host "❌ Không tìm thấy file SQL trong ZIP" -ForegroundColor Red
    Remove-Item $tempDir -Recurse -Force
    exit 1
}

# Restore Shore
Write-Host "📥 Restoring Shore Database..." -NoNewline
$env:PGPASSWORD = "productpwd"
psql -h localhost -p 5432 -U product -d productdb -f $shoreFile 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ✅" -ForegroundColor Green
} else {
    Write-Host " ❌" -ForegroundColor Red
}

# Restore Edge
Write-Host "📥 Restoring Edge Database..." -NoNewline
$env:PGPASSWORD = "ChangeMe_EdgePassword123!"
psql -h localhost -p 5433 -U edge_user -d maritime_edge -f $edgeFile 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ✅" -ForegroundColor Green
} else {
    Write-Host " ❌" -ForegroundColor Red
}

# Cleanup
$env:PGPASSWORD = $null
Remove-Item $tempDir -Recurse -Force

Write-Host "`n✨ Restore completed!`n" -ForegroundColor Green
