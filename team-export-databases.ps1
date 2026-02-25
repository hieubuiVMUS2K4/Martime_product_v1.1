# Simple Database Export Script for Team Members
# No special characters or emojis

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "team-database-backups"
$host.UI.RawUI.OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host ""
Write-Host "Maritime Product - Database Export" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Create backup directory
if (-not (Test-Path $backupDir)) {
    Write-Host "Creating backup directory..." -ForegroundColor Yellow
    New-Item -Path $backupDir -ItemType Directory | Out-Null
    Write-Host "Directory created: $backupDir" -ForegroundColor Green
}

Write-Host ""
Write-Host "Timestamp: $timestamp" -ForegroundColor Cyan
Write-Host ""

# Export Shore Database (productdb)
Write-Host "[1/2] Exporting Shore Database (productdb)..." -ForegroundColor Cyan
$shoreFile = "$backupDir\shore_productdb_$timestamp.sql"

try {
    $env:PGPASSWORD = "123"
    & "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe" `
        --host=localhost `
        --port=5432 `
        --username=product `
        --dbname=productdb `
        --clean `
        --if-exists `
        --no-owner `
        --no-privileges `
        --file="$shoreFile"
    
    if ($LASTEXITCODE -eq 0) {
        $fileSize = (Get-Item $shoreFile).Length / 1KB
        Write-Host "SUCCESS: Shore database exported" -ForegroundColor Green
        Write-Host "File: $shoreFile" -ForegroundColor Gray
        Write-Host "Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
    } else {
        Write-Host "FAILED: Shore database export failed (exit code: $LASTEXITCODE)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Export Edge Database (maritime_edge)
Write-Host "[2/2] Exporting Edge Database (maritime_edge)..." -ForegroundColor Cyan
$edgeFile = "$backupDir\edge_maritime_edge_$timestamp.sql"

try {
    $env:PGPASSWORD = "123"
    & "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe" `
        --host=localhost `
        --port=5433 `
        --username=edge_user `
        --dbname=maritime_edge `
        --clean `
        --if-exists `
        --no-owner `
        --no-privileges `
        --file="$edgeFile"
    
    if ($LASTEXITCODE -eq 0) {
        $fileSize = (Get-Item $edgeFile).Length / 1KB
        Write-Host "SUCCESS: Edge database exported" -ForegroundColor Green
        Write-Host "File: $edgeFile" -ForegroundColor Gray
        Write-Host "Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
    } else {
        Write-Host "FAILED: Edge database export failed (exit code: $LASTEXITCODE)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Export completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Backup files location: $backupDir\" -ForegroundColor Yellow
Write-Host ""
Write-Host "To import these files, your teammates should run:" -ForegroundColor Yellow
Write-Host "  psql -U product -d productdb -f shore_productdb_$timestamp.sql" -ForegroundColor Gray
Write-Host "  psql -U edge_user -d maritime_edge -p 5433 -f edge_maritime_edge_$timestamp.sql" -ForegroundColor Gray
Write-Host ""
