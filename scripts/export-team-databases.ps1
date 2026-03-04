# Team Database Export Script
# Exports both shore and edge databases for team collaboration

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "team-database-backups"

Write-Host ""
Write-Host "Maritime Product - Database Export for Team" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Timestamp: $timestamp" -ForegroundColor Yellow
Write-Host ""

# Create backup directory
if (-not (Test-Path $backupDir)) {
    Write-Host "Creating backup directory..." -ForegroundColor Yellow
    New-Item -Path $backupDir -ItemType Directory | Out-Null
}

# Export Shore Database using Docker (if container exists) or direct connection
Write-Host "[1/2] Exporting Shore Database (productdb on port 5432)..." -ForegroundColor Cyan

$shoreContainer = docker ps --filter "expose=5432" --format "{{.Names}}" 2>$null | Select-Object -First 1

if ($shoreContainer) {
    Write-Host "Found Docker container: $shoreContainer" -ForegroundColor Gray
    $shoreFile = Join-Path $backupDir "shore_productdb_$timestamp.sql"
    docker exec $shoreContainer pg_dump -U product -d productdb --clean --if-exists --no-owner --no-privileges > $shoreFile 2>&1
    
    if ($LASTEXITCODE -eq 0 -and (Test-Path $shoreFile)) {
        $fileSize = (Get-Item $shoreFile).Length / 1KB
        Write-Host "SUCCESS - Shore database exported" -ForegroundColor Green
        Write-Host "  File: $shoreFile" -ForegroundColor Gray
        Write-Host "  Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
    } else {
        Write-Host "FAILED - Could not export shore database" -ForegroundColor Red
    }
} else {
    Write-Host "No Docker container found for shore database" -ForegroundColor Yellow
    Write-Host "Trying direct connection to localhost:5432..." -ForegroundColor Yellow
    
    # Try using psql or pg_dump if available in PATH
    $pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
    
    if ($pgDump) {
        $shoreFile = Join-Path $backupDir "shore_productdb_$timestamp.sql"
        $env:PGPASSWORD = "123"
        pg_dump -h localhost -p 5432 -U product -d productdb --clean --if-exists --no-owner --no-privileges -f $shoreFile 2>&1
        
        if ($LASTEXITCODE -eq 0 -and (Test-Path $shoreFile)) {
            $fileSize = (Get-Item $shoreFile).Length / 1KB
            Write-Host "SUCCESS - Shore database exported" -ForegroundColor Green
            Write-Host "  File: $shoreFile" -ForegroundColor Gray
            Write-Host "  Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
        } else {
            Write-Host "FAILED - Could not export shore database" -ForegroundColor Red
        }
    } else {
        Write-Host "SKIPPED - pg_dump not found in PATH" -ForegroundColor Yellow
        Write-Host "Please install PostgreSQL client tools or use Docker" -ForegroundColor Yellow
    }
}

Write-Host ""

# Export Edge Database from Docker container
Write-Host "[2/2] Exporting Edge Database (maritime_edge on port 5433)..." -ForegroundColor Cyan

$edgeContainer = docker ps --filter "expose=5433" --format "{{.Names}}" 2>$null | Select-Object -First 1

if (-not $edgeContainer) {
    # Try finding by name pattern
    $edgeContainer = docker ps --format "{{.Names}}" 2>$null | Where-Object { $_ -like "*postgres*" -or $_ -like "*edge*" } | Select-Object -First 1
}

if ($edgeContainer) {
    Write-Host "Found Docker container: $edgeContainer" -ForegroundColor Gray
    $edgeFile = Join-Path $backupDir "edge_maritime_edge_$timestamp.sql"
    docker exec $edgeContainer pg_dump -U edge_user -d maritime_edge --clean --if-exists --no-owner --no-privileges > $edgeFile 2>&1
    
    if ($LASTEXITCODE -eq 0 -and (Test-Path $edgeFile)) {
        $fileSize = (Get-Item $edgeFile).Length / 1KB
        Write-Host "SUCCESS - Edge database exported" -ForegroundColor Green
        Write-Host "  File: $edgeFile" -ForegroundColor Gray
        Write-Host "  Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
    } else {
        Write-Host "FAILED - Could not export edge database" -ForegroundColor Red
    }
} else {
    Write-Host "FAILED - No Docker container found for edge database" -ForegroundColor Red
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Export process completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Backup files are in: $backupDir\" -ForegroundColor Yellow
Write-Host ""
Write-Host "Share these files with your teammates." -ForegroundColor Cyan
Write-Host "They can import using:" -ForegroundColor Cyan
Write-Host "  psql -U product -d productdb < shore_productdb_$timestamp.sql" -ForegroundColor Gray
Write-Host "  psql -U edge_user -d maritime_edge -p 5433 < edge_maritime_edge_$timestamp.sql" -ForegroundColor Gray
Write-Host ""
