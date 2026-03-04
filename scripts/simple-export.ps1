# Simple database export - Try all methods
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "team-database-backups"

if (-not (Test-Path $backupDir)) {
    New-Item -Path $backupDir -ItemType Directory | Out-Null
}

Write-Host "=== Maritime Database Export ===" -ForegroundColor Cyan
Write-Host ""

# 1. Export shore database (productdb on localhost:5432)
Write-Host "[1/2] Shore Database (productdb)..." -ForegroundColor Yellow

# Try method 1: Direct pg_dump with PGPASSWORD
$shoreFile = "$backupDir\shore_productdb_$timestamp.sql"
$env:PGPASSWORD = "123"

# Find pg_dump
$pgDumpPaths = @(
    "pg_dump",
    "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
    "C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
    "C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
    "C:\PostgreSQL\16\bin\pg_dump.exe"
)

$pgDump = $null
foreach ($path in $pgDumpPaths) {
    if (Get-Command $path -ErrorAction SilentlyContinue) {
        $pgDump = $path
        break
    }
}

if ($pgDump) {
    Write-Host "  Using: $pgDump" -ForegroundColor Gray
    & $pgDump -h localhost -p 5432 -U product -d productdb --clean --if-exists --no-owner --no-privileges -f $shoreFile 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0 -and (Test-Path $shoreFile)) {
        $size = [math]::Round((Get-Item $shoreFile).Length / 1KB, 2)
        Write-Host "  SUCCESS: $size KB" -ForegroundColor Green
    } else {
        Write-Host "  FAILED" -ForegroundColor Red
    }
} else {
    Write-Host "  ERROR: pg_dump not found" -ForegroundColor Red
    Write-Host "  Please add PostgreSQL bin directory to PATH" -ForegroundColor Yellow
}

Write-Host ""

# 2. Export edge database (maritime_edge from Docker)
Write-Host "[2/2] Edge Database (maritime_edge)..." -ForegroundColor Yellow

$edgeFile = "$backupDir\edge_maritime_edge_$timestamp.sql"
docker exec e83c9374df19 pg_dump -U edge_user -d maritime_edge --clean --if-exists --no-owner --no-privileges > $edgeFile 2>&1

if ($LASTEXITCODE -eq 0 -and (Test-Path $edgeFile)) {
    $size = [math]::Round((Get-Item $edgeFile).Length / 1KB, 2)
    Write-Host "  SUCCESS: $size KB" -ForegroundColor Green
} else {
    Write-Host "  FAILED" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Completed ===" -ForegroundColor Cyan
Write-Host "Files in: $backupDir\" -ForegroundColor Yellow
Write-Host ""

# Show all files
Get-ChildItem $backupDir\*$timestamp*.sql | ForEach-Object {
    $size = [math]::Round($_.Length / 1KB, 2)
    Write-Host "  $($_.Name) - $size KB" -ForegroundColor Gray
}
