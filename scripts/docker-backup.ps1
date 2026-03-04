# Docker-based Backup Script
# Khong can cai PostgreSQL client tools

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "database-backups"

Write-Host ""
Write-Host "Docker Backup - Maritime Product" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Tao thu muc backup
if (-not (Test-Path $backupDir)) {
    New-Item -Path $backupDir -ItemType Directory | Out-Null
}

# Kiem tra Docker containers
Write-Host "Checking Docker containers..." -ForegroundColor Cyan
$postgresContainer = docker ps --filter "name=postgres" --format "{{.Names}}" | Select-Object -First 1

if ([string]::IsNullOrEmpty($postgresContainer)) {
    Write-Host "PostgreSQL container khong chay!" -ForegroundColor Red
    Write-Host "Chay: docker-compose up -d postgres" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found container: $postgresContainer" -ForegroundColor Green
Write-Host ""

# Shore Database
Write-Host "Backing up Shore Database..." -NoNewline
try {
    docker exec $postgresContainer pg_dump -U product -d productdb --clean --if-exists | 
        Out-File -FilePath "$backupDir\shore_$timestamp.sql" -Encoding UTF8
    Write-Host " Done" -ForegroundColor Green
} catch {
    Write-Host " Failed" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

# Edge Database
Write-Host "Backing up Edge Database..." -NoNewline

$edgeContainer = docker ps --filter "name=edge" --format "{{.Names}}" | Select-Object -First 1

if (-not [string]::IsNullOrEmpty($edgeContainer)) {
    try {
        docker exec $edgeContainer pg_dump -U edge_user -d maritime_edge --clean --if-exists |
            Out-File -FilePath "$backupDir\edge_$timestamp.sql" -Encoding UTF8
        Write-Host " Done" -ForegroundColor Green
    } catch {
        Write-Host " Failed" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host " Skipped (not in Docker)" -ForegroundColor Yellow
    "-- Edge Database backup skipped" | Out-File -FilePath "$backupDir\edge_$timestamp.sql" -Encoding UTF8
}

# Tao ZIP
Write-Host ""
Write-Host "Creating ZIP file..." -NoNewline
try {
    $filesToZip = Get-ChildItem "$backupDir\*_$timestamp.sql"
    if ($filesToZip) {
        Compress-Archive -Path $filesToZip -DestinationPath "backup_$timestamp.zip" -Force
        Write-Host " Done" -ForegroundColor Green
        
        $size = (Get-Item "backup_$timestamp.zip").Length / 1MB
        Write-Host ""
        Write-Host "Backup completed: backup_$timestamp.zip" -ForegroundColor Green
        Write-Host "Size: $([math]::Round($size, 2)) MB" -ForegroundColor Green
    }
} catch {
    Write-Host " Failed" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

# Hien thi danh sach backups
Write-Host ""
Write-Host "Recent backups:" -ForegroundColor Cyan
$backups = Get-ChildItem "backup_*.zip" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 5
if ($backups) {
    foreach ($backup in $backups) {
        $sizeMB = [math]::Round($backup.Length / 1MB, 2)
        Write-Host "  $($backup.Name) - $sizeMB MB - $($backup.LastWriteTime)" -ForegroundColor Gray
    }
} else {
    Write-Host "  No backups found" -ForegroundColor Gray
}

Write-Host ""
