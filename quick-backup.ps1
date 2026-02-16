# ===================================================================
# Quick Backup Script - Đơn giản và nhanh
# ===================================================================
# Script nhanh để backup database mà không cần nhiều options
# Sử dụng cho việc backup hàng ngày
# ===================================================================

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "database-backups"

Write-Host "`n🚀 Quick Backup - Maritime Product" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Tạo thư mục backup
if (-not (Test-Path $backupDir)) {
    New-Item -Path $backupDir -ItemType Directory | Out-Null
}

# Shore Database
Write-Host "📦 Backing up Shore Database..." -NoNewline
$env:PGPASSWORD = "productpwd"
pg_dump -h localhost -p 5432 -U product -d productdb -F p --clean --if-exists -f "$backupDir\shore_$timestamp.sql" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ✅" -ForegroundColor Green
} else {
    Write-Host " ❌" -ForegroundColor Red
}

# Edge Database
Write-Host "📦 Backing up Edge Database..." -NoNewline
$env:PGPASSWORD = "ChangeMe_EdgePassword123!"
pg_dump -h localhost -p 5433 -U edge_user -d maritime_edge -F p --clean --if-exists -f "$backupDir\edge_$timestamp.sql" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ✅" -ForegroundColor Green
} else {
    Write-Host " ❌" -ForegroundColor Red
}

# Cleanup
$env:PGPASSWORD = $null

# Tạo ZIP
Write-Host "📦 Creating ZIP file..." -NoNewline
try {
    Compress-Archive -Path "$backupDir\shore_$timestamp.sql","$backupDir\edge_$timestamp.sql" -DestinationPath "backup_$timestamp.zip" -Force
    Write-Host " ✅" -ForegroundColor Green
    
    $size = (Get-Item "backup_$timestamp.zip").Length / 1MB
    Write-Host "`n✨ Backup completed: backup_$timestamp.zip ($('{0:N2}' -f $size) MB)`n" -ForegroundColor Green
} catch {
    Write-Host " ❌" -ForegroundColor Red
}

# Xóa old backups (giữ lại 5 bản gần nhất)
$oldBackups = Get-ChildItem "$backupDir\shore_*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -Skip 5
if ($oldBackups) {
    Write-Host "🧹 Cleaning up old backups..." -ForegroundColor Yellow
    $oldBackups | ForEach-Object {
        $baseName = $_.BaseName -replace "shore_", ""
        Remove-Item "$backupDir\shore_$baseName.sql" -ErrorAction SilentlyContinue
        Remove-Item "$backupDir\edge_$baseName.sql" -ErrorAction SilentlyContinue
        Write-Host "   Removed: $baseName" -ForegroundColor Gray
    }
}

Write-Host ""
