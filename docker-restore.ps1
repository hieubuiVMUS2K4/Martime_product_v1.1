# ===================================================================
# Docker-based Restore Script
# Không cần cài PostgreSQL client tools
# ===================================================================

param(
    [string]$ZipFile = ""
)

Write-Host "`n🔄 Docker Restore - Maritime Product" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Tìm ZIP file mới nhất nếu không chỉ định
if ([string]::IsNullOrEmpty($ZipFile)) {
    $ZipFile = Get-ChildItem "backup_*.zip" | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1 -ExpandProperty FullName
    
    if ([string]::IsNullOrEmpty($ZipFile)) {
        Write-Host "❌ Không tìm thấy file backup nào!" -ForegroundColor Red
        Write-Host "Sử dụng: .\docker-restore.ps1 -ZipFile 'backup_20240203_143022.zip'" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "📦 Found: $(Split-Path $ZipFile -Leaf)" -ForegroundColor Green
}

if (-not (Test-Path $ZipFile)) {
    Write-Host "❌ File không tồn tại: $ZipFile" -ForegroundColor Red
    exit 1
}

# Kiểm tra Docker
Write-Host "🔍 Checking Docker containers..." -ForegroundColor Cyan
$postgresContainer = docker ps --filter "name=postgres" --format "{{.Names}}" | Select-Object -First 1

if ([string]::IsNullOrEmpty($postgresContainer)) {
    Write-Host "❌ PostgreSQL container không chạy!" -ForegroundColor Red
    Write-Host "Chạy: docker-compose up -d postgres" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Found container: $postgresContainer`n" -ForegroundColor Green

# Xác nhận
Write-Host "⚠️  CẢNH BÁO: Toàn bộ dữ liệu hiện tại sẽ bị xóa!" -ForegroundColor Yellow
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
$shoreFile = Get-ChildItem "$tempDir\*shore*.sql" | Select-Object -First 1
$edgeFile = Get-ChildItem "$tempDir\*edge*.sql" | Select-Object -First 1

if (-not $shoreFile) {
    Write-Host "❌ Không tìm thấy shore backup file" -ForegroundColor Red
    Remove-Item $tempDir -Recurse -Force
    exit 1
}

# Restore Shore Database
Write-Host "📥 Restoring Shore Database..." -NoNewline
try {
    Get-Content $shoreFile.FullName | docker exec -i $postgresContainer psql -U product -d productdb 2>$null | Out-Null
    Write-Host " ✅" -ForegroundColor Green
} catch {
    Write-Host " ❌" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

# Restore Edge Database (nếu có)
if ($edgeFile) {
    $edgeContainer = docker ps --filter "name=edge" --format "{{.Names}}" | Select-Object -First 1
    
    if (-not [string]::IsNullOrEmpty($edgeContainer)) {
        Write-Host "📥 Restoring Edge Database..." -NoNewline
        try {
            Get-Content $edgeFile.FullName | docker exec -i $edgeContainer psql -U edge_user -d maritime_edge 2>$null | Out-Null
            Write-Host " ✅" -ForegroundColor Green
        } catch {
            Write-Host " ❌" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️  Edge Database container không tìm thấy (skipped)" -ForegroundColor Yellow
    }
}

# Cleanup
Remove-Item $tempDir -Recurse -Force

Write-Host "`n✨ Restore completed!`n" -ForegroundColor Green

# Verify
Write-Host "🔍 Verifying data..." -ForegroundColor Cyan
$crewCount = docker exec $postgresContainer psql -U product -d productdb -t -c "SELECT COUNT(*) FROM `"Crews`";" 2>$null | Select-String "\d+" | ForEach-Object { $_.Matches.Value }
if ($crewCount) {
    Write-Host "   Crews: $crewCount records" -ForegroundColor Green
}

Write-Host ""
