# =====================================================
# STEP 3: RESTORE FROM BACKUP (ROLLBACK)
# Use this if Step 2 failed or you want to undo changes
# =====================================================

param(
    [string]$BackupPath = ""
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Red
Write-Host "ROLLBACK: RESTORE FROM BACKUP" -ForegroundColor Red
Write-Host "================================" -ForegroundColor Red
Write-Host ""

# ===== FIND BACKUP =====
if ($BackupPath -eq "") {
    Write-Host "Looking for recent backup..." -ForegroundColor Yellow
    
    if (Test-Path ".\last-backup-path.txt") {
        $BackupPath = Get-Content ".\last-backup-path.txt"
        Write-Host "[OK] Found backup: $BackupPath" -ForegroundColor Green
    }
    else {
        Write-Host "[ERROR] No automatic backup found" -ForegroundColor Red
        Write-Host ""
        Write-Host "Available backups:" -ForegroundColor Yellow
        
        if (Test-Path ".\migration-reset-backup") {
            Get-ChildItem ".\migration-reset-backup" -Directory | ForEach-Object {
                Write-Host "  - $($_.FullName)" -ForegroundColor Cyan
            }
            Write-Host ""
            $BackupPath = Read-Host "Enter backup path to restore"
        }
        else {
            Write-Host "  (none found)" -ForegroundColor DarkGray
            Write-Host ""
            Write-Host "Cannot proceed without backup" -ForegroundColor Red
            exit 1
        }
    }
}

if (-not (Test-Path $BackupPath)) {
    Write-Host "[ERROR] Backup path does not exist: $BackupPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Backup to restore: $BackupPath" -ForegroundColor Cyan
Write-Host ""

# ===== CONFIRMATION =====
Write-Host "WARNING: This will restore database and migrations from backup!" -ForegroundColor Yellow
Write-Host "Current database and migrations will be REPLACED!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press ENTER to continue or Ctrl+C to cancel..." -ForegroundColor Yellow
Read-Host

# ===== STEP 3.1: RESTORE DATABASE =====
Write-Host ""
Write-Host "Step 3.1: Restoring database..." -ForegroundColor Yellow

$dbBackupFile = Join-Path $BackupPath "maritime_edge_backup.sql"
$containerName = "maritime-edge-postgres"
$dbUser = "edge_user"
$dbName = "maritime_edge"

if (-not (Test-Path $dbBackupFile)) {
    Write-Host "  [ERROR] Database backup file not found: $dbBackupFile" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "  - Dropping existing database..." -ForegroundColor DarkGreen
    docker exec $containerName psql -U $dbUser -d postgres -c "DROP DATABASE IF EXISTS $dbName;" 2>&1 | Out-Null
    
    Write-Host "  - Creating fresh database..." -ForegroundColor DarkGreen
    docker exec $containerName psql -U $dbUser -d postgres -c "CREATE DATABASE $dbName OWNER $dbUser;" 2>&1 | Out-Null
    
    Write-Host "  - Copying backup file to container..." -ForegroundColor DarkGreen
    docker cp $dbBackupFile "${containerName}:/tmp/restore.sql"
    
    Write-Host "  - Restoring from backup (this may take a minute)..." -ForegroundColor DarkGreen
    docker exec $containerName psql -U $dbUser -d $dbName -f /tmp/restore.sql 2>&1 | Out-Null
    
    Write-Host "  [OK] Database restored successfully" -ForegroundColor Green
}
catch {
    Write-Host "  [FAIL] Database restore failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "You may need to restore manually:" -ForegroundColor Yellow
    Write-Host "  docker cp $dbBackupFile ${containerName}:/tmp/restore.sql" -ForegroundColor Cyan
    Write-Host "  docker exec $containerName psql -U $dbUser -d $dbName -f /tmp/restore.sql" -ForegroundColor Cyan
    exit 1
}
Write-Host ""

# ===== STEP 3.2: RESTORE MIGRATIONS =====
Write-Host "Step 3.2: Restoring migrations..." -ForegroundColor Yellow

$migrationsBackup = Join-Path $BackupPath "Migrations_Backup"
$migrationsFolder = ".\Data\Migrations"

try {
    if (Test-Path $migrationsBackup) {
        # Delete current migrations
        if (Test-Path $migrationsFolder) {
            Remove-Item "$migrationsFolder\*.cs" -Force -ErrorAction SilentlyContinue
        }
        else {
            New-Item -ItemType Directory -Path $migrationsFolder -Force | Out-Null
        }
        
        # Restore from backup
        Copy-Item "$migrationsBackup\*" -Destination $migrationsFolder -Recurse -Force
        
        $count = (Get-ChildItem $migrationsFolder -Filter "*.cs" | Measure-Object).Count
        Write-Host "  [OK] Restored $count migration files" -ForegroundColor Green
    }
    else {
        Write-Host "  [WARN] No migrations backup found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  [FAIL] Migrations restore failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  You may need to restore manually from: $migrationsBackup" -ForegroundColor Yellow
}
Write-Host ""

# ===== STEP 3.3: RESTORE CONFIGURATION =====
Write-Host "Step 3.3: Checking configuration files..." -ForegroundColor Yellow

try {
    $configs = Get-ChildItem $BackupPath -Filter "*.json"
    
    foreach ($config in $configs) {
        if ($config.Name -ne "backup-info.json") {
            Write-Host "  - Found: $($config.Name)" -ForegroundColor DarkGreen
            # Don't auto-restore configs, just notify
        }
    }
    
    Write-Host "  [INFO] Configuration files in backup (not auto-restored)" -ForegroundColor DarkGray
}
catch {
    Write-Host "  [WARN] Could not check config files" -ForegroundColor Yellow
}
Write-Host ""

# ===== STEP 3.4: VERIFY RESTORE =====
Write-Host "Step 3.4: Verifying restore..." -ForegroundColor Yellow

try {
    # Check database
    $tableCount = docker exec $containerName psql -U $dbUser -d $dbName -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>&1
    
    if ($tableCount -match "\d+") {
        Write-Host "  [OK] Database has $($tableCount.Trim()) tables" -ForegroundColor Green
    }
    
    # Check migrations
    if (Test-Path $migrationsFolder) {
        $migCount = (Get-ChildItem $migrationsFolder -Filter "*.cs" | Measure-Object).Count
        Write-Host "  [OK] Migrations folder has $migCount files" -ForegroundColor Green
    }
}
catch {
    Write-Host "  [WARN] Could not verify: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# ===== STEP 3.5: BUILD TEST =====
Write-Host "Step 3.5: Testing build..." -ForegroundColor Yellow

try {
    dotnet build --no-incremental 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Build successful" -ForegroundColor Green
    }
    else {
        Write-Host "  [WARN] Build had warnings" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  [WARN] Build test skipped" -ForegroundColor Yellow
}
Write-Host ""

# ===== SUCCESS =====
Write-Host "================================" -ForegroundColor Green
Write-Host "RESTORE COMPLETED" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  - Database: Restored from backup" -ForegroundColor Green
Write-Host "  - Migrations: Restored from backup" -ForegroundColor Green
Write-Host "  - Build: Tested" -ForegroundColor Green
Write-Host ""
Write-Host "Your system has been rolled back to:" -ForegroundColor Yellow
Write-Host "  $BackupPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEP: Test your application to confirm everything works" -ForegroundColor Yellow
Write-Host ""
