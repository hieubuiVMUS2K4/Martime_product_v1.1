# =====================================================
# STEP 1: BACKUP DATABASE & MIGRATIONS
# Safe backup before migration reset
# =====================================================

param(
    [string]$BackupFolder = ".\migration-reset-backup"
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "STEP 1: BACKUP BEFORE RESET" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Create backup folder
if (-not (Test-Path $BackupFolder)) {
    New-Item -ItemType Directory -Path $BackupFolder | Out-Null
    Write-Host "[OK] Created backup folder: $BackupFolder" -ForegroundColor Green
}

$backupPath = Join-Path $BackupFolder $timestamp
New-Item -ItemType Directory -Path $backupPath | Out-Null
Write-Host "[OK] Backup location: $backupPath" -ForegroundColor Green
Write-Host ""

# ===== STEP 1.1: BACKUP DATABASE =====
Write-Host "Step 1.1: Backing up database..." -ForegroundColor Yellow

$dbBackupFile = Join-Path $backupPath "maritime_edge_backup.sql"
$containerName = "maritime-edge-postgres"
$dbUser = "edge_user"
$dbName = "maritime_edge"

try {
    # Check if container is running
    $containerStatus = docker ps --filter "name=$containerName" --format "{{.Status}}"
    if (-not $containerStatus) {
        throw "Container '$containerName' is not running. Please start it first."
    }
    Write-Host "  - Container '$containerName' is running" -ForegroundColor DarkGreen
    
    # Perform database backup
    Write-Host "  - Creating database dump (this may take a minute)..." -ForegroundColor DarkGreen
    docker exec $containerName pg_dump -U $dbUser -d $dbName > $dbBackupFile
    
    if (-not (Test-Path $dbBackupFile)) {
        throw "Backup file was not created"
    }
    
    $fileSize = (Get-Item $dbBackupFile).Length / 1MB
    Write-Host "  [OK] Database backed up: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Green
}
catch {
    Write-Host "  [FAIL] Database backup failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "BACKUP FAILED - STOPPING" -ForegroundColor Red
    exit 1
}
Write-Host ""

# ===== STEP 1.2: BACKUP MIGRATIONS =====
Write-Host "Step 1.2: Backing up migrations..." -ForegroundColor Yellow

$migrationsSource = ".\Data\Migrations"
$migrationsBackup = Join-Path $backupPath "Migrations_Backup"

try {
    if (Test-Path $migrationsSource) {
        Copy-Item -Path $migrationsSource -Destination $migrationsBackup -Recurse -Force
        $migrationCount = (Get-ChildItem $migrationsBackup -Filter "*.cs" | Measure-Object).Count
        Write-Host "  [OK] Backed up $migrationCount migration files" -ForegroundColor Green
    }
    else {
        Write-Host "  [WARN] No migrations folder found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  [FAIL] Migrations backup failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "BACKUP FAILED - STOPPING" -ForegroundColor Red
    exit 1
}
Write-Host ""

# ===== STEP 1.3: BACKUP APPSETTINGS =====
Write-Host "Step 1.3: Backing up configuration..." -ForegroundColor Yellow

try {
    $configFiles = @("appsettings.json", "appsettings.Development.json", "docker-compose.yml")
    
    foreach ($config in $configFiles) {
        if (Test-Path $config) {
            Copy-Item $config -Destination (Join-Path $backupPath $config) -Force
            Write-Host "  [OK] Backed up $config" -ForegroundColor Green
        }
    }
}
catch {
    Write-Host "  [WARN] Config backup failed: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# ===== STEP 1.4: CREATE METADATA =====
Write-Host "Step 1.4: Creating backup metadata..." -ForegroundColor Yellow

$metadata = @{
    BackupDate = $timestamp
    DatabaseName = $dbName
    ContainerName = $containerName
    MigrationCount = $migrationCount
    BackupFolder = $backupPath
} | ConvertTo-Json

$metadataFile = Join-Path $backupPath "backup-info.json"
$metadata | Out-File $metadataFile -Encoding UTF8

Write-Host "  [OK] Metadata saved" -ForegroundColor Green
Write-Host ""

# ===== VERIFICATION =====
Write-Host "Step 1.5: Verifying backup..." -ForegroundColor Yellow

$isValid = $true

# Check database backup
if (-not (Test-Path $dbBackupFile)) {
    Write-Host "  [FAIL] Database backup file missing" -ForegroundColor Red
    $isValid = $false
}
elseif ((Get-Item $dbBackupFile).Length -lt 1000) {
    Write-Host "  [FAIL] Database backup file too small (likely corrupted)" -ForegroundColor Red
    $isValid = $false
}
else {
    Write-Host "  [OK] Database backup verified" -ForegroundColor Green
}

# Check migrations backup
if (-not (Test-Path $migrationsBackup)) {
    Write-Host "  [WARN] Migrations backup missing" -ForegroundColor Yellow
}
else {
    Write-Host "  [OK] Migrations backup verified" -ForegroundColor Green
}

Write-Host ""

if ($isValid) {
    Write-Host "================================" -ForegroundColor Green
    Write-Host "BACKUP COMPLETED SUCCESSFULLY" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Backup location: $backupPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "NEXT STEP: Run '.\2-reset-migrations.ps1'" -ForegroundColor Yellow
    Write-Host ""
    
    # Save backup path for next script
    $backupPath | Out-File ".\last-backup-path.txt" -Encoding UTF8
    
    exit 0
}
else {
    Write-Host "================================" -ForegroundColor Red
    Write-Host "BACKUP VERIFICATION FAILED" -ForegroundColor Red
    Write-Host "================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check errors above and try again" -ForegroundColor Red
    exit 1
}
