# Shore Database Backup Script
# Run this BEFORE applying optimization changes

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "productdb_backup_$timestamp.sql"
$container = "martime_product_v11-postgres-1"
$database = "productdb"
$user = "product"

Write-Host "Starting Shore Database Backup..." -ForegroundColor Cyan
Write-Host "Timestamp: $timestamp" -ForegroundColor Gray

# Check if container is running
$containerRunning = docker ps --filter "name=$container" --format "{{.Names}}"
if (-not $containerRunning) {
    Write-Host "ERROR: Container $container is not running!" -ForegroundColor Red
    Write-Host "Run: docker-compose up -d" -ForegroundColor Yellow
    exit 1
}

# Create backup using pg_dump
Write-Host "Creating backup: $backupFile" -ForegroundColor Yellow
docker exec -t $container pg_dump -U $user -d $database --clean --if-exists --format=plain | Out-File -FilePath $backupFile -Encoding UTF8

if ($LASTEXITCODE -eq 0) {
    $fileSize = (Get-Item $backupFile).Length / 1MB
    $fileSizeRounded = [math]::Round($fileSize, 2)
    Write-Host "Backup completed successfully!" -ForegroundColor Green
    Write-Host "File: $backupFile (Size: $fileSizeRounded MB)" -ForegroundColor Green
    Write-Host ""
    Write-Host "To restore, run command:" -ForegroundColor Cyan
    Write-Host "docker exec -i $container psql -U $user -d $database -f /backup/$backupFile" -ForegroundColor White
} else {
    Write-Host "Backup failed!" -ForegroundColor Red
    exit 1
}
