# ==============================================================================
# AUTOMATED POSTGRESQL DATABASE BACKUP SCRIPT FOR SHORE SYSTEM
# ==============================================================================
# Usage:
#   powershell -ExecutionPolicy Bypass -File ./scripts/database/backup-shore-db.ps1
# ==============================================================================

param (
    [string]$ContainerName = "shore_product-postgres-1",
    [string]$DbUser = "product",
    [string]$DbName = "productdb",
    [string]$BackupDir = "./dumps/shore_backups",
    [int]$RetentionDays = 14
)

$ErrorActionPreference = "Stop"

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
if (!(Test-Path -Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
}

$backupFile = Join-Path $BackupDir "shore_dump_${timestamp}.sql"
$compressedFile = "${backupFile}.gz"

Write-Host "[BACKUP] Starting automated database backup for Shore System ($DbName)..." -ForegroundColor Cyan

try {
    # Run pg_dump inside docker container
    docker exec $ContainerName pg_dump -U $DbUser $DbName > $backupFile

    if ((Get-Item $backupFile).Length -eq 0) {
        throw "Backup file created is empty. Check database connection and user permissions."
    }

    Write-Host "[BACKUP] Database dump completed successfully: $backupFile" -ForegroundColor Green

    # Retention policy: Purge backups older than RetentionDays
    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    Get-ChildItem -Path $BackupDir -Filter "shore_dump_*.sql*" | Where-Object { $_.LastWriteTime -lt $cutoff } | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "[CLEANUP] Deleted old backup: $($_.Name)" -ForegroundColor Yellow
    }

    Write-Host "[BACKUP] Backup process finished successfully." -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Database backup failed: $_" -ForegroundColor Red
    exit 1
}
