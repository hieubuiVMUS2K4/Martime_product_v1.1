#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Restore database schema from backup for teammates
.DESCRIPTION
    This script drops and recreates the maritime_edge database with the correct schema
.EXAMPLE
    .\restore-database.ps1
#>

Write-Host "=== Maritime Edge Database Restore ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$containerName = "maritime-edge-postgres"
$dbName = "maritime_edge"
$dbUser = "edge_user"
$backupFile = "database-schema-backup.sql"

# Check if container is running
Write-Host "[1/5] Checking Docker container..." -ForegroundColor Yellow
$containerExists = docker ps --filter "name=$containerName" --format "{{.Names}}"
if ($containerExists -ne $containerName) {
    Write-Host "ERROR: Container '$containerName' is not running!" -ForegroundColor Red
    Write-Host "Please start docker-compose first: cd .. && docker-compose up -d" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Container is running" -ForegroundColor Green

# Check if backup file exists
Write-Host "[2/5] Checking backup file..." -ForegroundColor Yellow
if (-not (Test-Path $backupFile)) {
    Write-Host "ERROR: Backup file '$backupFile' not found!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Backup file found ($((Get-Item $backupFile).Length / 1KB) KB)" -ForegroundColor Green

# Confirm with user
Write-Host ""
Write-Host "WARNING: This will DROP and RECREATE the '$dbName' database!" -ForegroundColor Red
Write-Host "All existing data will be LOST!" -ForegroundColor Red
$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Aborted by user." -ForegroundColor Yellow
    exit 0
}

# Drop existing database
Write-Host ""
Write-Host "[3/5] Dropping existing database..." -ForegroundColor Yellow
docker exec $containerName psql -U postgres -c "DROP DATABASE IF EXISTS $dbName;"
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to drop database!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Database dropped" -ForegroundColor Green

# Create new database
Write-Host "[4/5] Creating new database..." -ForegroundColor Yellow
docker exec $containerName psql -U postgres -c "CREATE DATABASE $dbName OWNER $dbUser;"
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create database!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Database created" -ForegroundColor Green

# Restore schema
Write-Host "[5/5] Restoring database schema..." -ForegroundColor Yellow
Get-Content $backupFile | docker exec -i $containerName psql -U $dbUser -d $dbName
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore schema!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Schema restored" -ForegroundColor Green

# Verify
Write-Host ""
Write-Host "=== Verification ===" -ForegroundColor Cyan
$tableCount = docker exec $containerName psql -U $dbUser -d $dbName -t -c "SELECT COUNT(*) FROM pg_tables WHERE schemaname = 'public';"
Write-Host "Total tables: $($tableCount.Trim())" -ForegroundColor Green

Write-Host ""
Write-Host "=== Database Restore Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run migrations: dotnet ef database update" -ForegroundColor White
Write-Host "2. Build project: dotnet build" -ForegroundColor White
Write-Host "3. Run application: dotnet run" -ForegroundColor White
