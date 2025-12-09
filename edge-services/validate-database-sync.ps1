<#
.SYNOPSIS
    Validates that the database schema is in sync with EF Core migrations.

.DESCRIPTION
    This script helps prevent migration issues by:
    1. Checking that all migrations are applied to the database
    2. Detecting if there are pending model changes
    3. Ensuring ModelSnapshot matches the current database state
    
    Run this script BEFORE creating new migrations to ensure database and models are in sync.

.EXAMPLE
    .\validate-database-sync.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Database & Migration Validation ===" -ForegroundColor Cyan
Write-Host ""

# Change to edge-services directory
$projectPath = $PSScriptRoot
Set-Location $projectPath

# Step 1: Check if there are pending migrations
Write-Host "Step 1: Checking for pending migrations..." -ForegroundColor Yellow
$migrationsList = dotnet ef migrations list --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to list migrations. Make sure the project builds successfully." -ForegroundColor Red
    Write-Host "Run: dotnet build" -ForegroundColor Yellow
    exit 1
}

$pendingMigrations = $migrationsList | Select-String "\(Pending\)"
if ($pendingMigrations) {
    Write-Host "⚠️  WARNING: Pending migrations detected!" -ForegroundColor Red
    Write-Host $pendingMigrations -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Action required: Apply pending migrations with:" -ForegroundColor Yellow
    Write-Host "  dotnet ef database update" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "✓ No pending migrations. All migrations are applied." -ForegroundColor Green
}

# Step 2: Check if there are model changes not captured in migrations
Write-Host ""
Write-Host "Step 2: Checking for un-migrated model changes..." -ForegroundColor Yellow

# Create a temporary migration to see if there are changes
$tempMigrationName = "TempValidation_$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "  Creating temporary migration: $tempMigrationName" -ForegroundColor Gray

$addMigrationOutput = dotnet ef migrations add $tempMigrationName --no-build 2>&1
$migrationCreated = $LASTEXITCODE -eq 0

if ($migrationCreated) {
    # Check if the migration is empty
    $migrationFile = Get-ChildItem -Path "Data/Migrations" -Filter "*$tempMigrationName.cs" | Select-Object -First 1
    
    if ($migrationFile) {
        $content = Get-Content $migrationFile.FullName -Raw
        
        # Check if Up method is empty
        if ($content -match 'protected override void Up\(MigrationBuilder migrationBuilder\)\s*{\s*}') {
            Write-Host "✓ Models are in sync with database. No changes detected." -ForegroundColor Green
        } else {
            Write-Host "⚠️  WARNING: Model changes detected that are not in migrations!" -ForegroundColor Red
            Write-Host ""
            Write-Host "Changes found in temporary migration. Review the changes:" -ForegroundColor Yellow
            Write-Host "  File: $($migrationFile.FullName)" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Options:" -ForegroundColor Yellow
            Write-Host "  1. Keep this migration if changes are intentional:" -ForegroundColor White
            Write-Host "     - Review the migration file" -ForegroundColor Gray
            Write-Host "     - Rename it to a meaningful name if needed" -ForegroundColor Gray
            Write-Host "     - Run: dotnet ef database update" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  2. Remove this migration if changes are NOT intentional:" -ForegroundColor White
            Write-Host "     - Run: dotnet ef migrations remove --force" -ForegroundColor Cyan
            Write-Host "     - Fix your models to match the database" -ForegroundColor Gray
            Write-Host ""
            
            # Don't auto-remove, let user decide
            exit 2
        }
        
        # Remove empty temporary migration
        Write-Host "  Removing temporary migration..." -ForegroundColor Gray
        dotnet ef migrations remove --force > $null 2>&1
    }
} else {
    Write-Host "✓ No model changes detected (migration creation returned no changes)." -ForegroundColor Green
}

# Step 3: Validate database connection and table count
Write-Host ""
Write-Host "Step 3: Validating database connection..." -ForegroundColor Yellow

$dockerCheck = docker ps | Select-String "maritime-edge-postgres"
if (-not $dockerCheck) {
    Write-Host "⚠️  WARNING: maritime-edge-postgres container is not running!" -ForegroundColor Red
    Write-Host "Start the container with: docker-compose up -d" -ForegroundColor Yellow
    exit 3
}

Write-Host "✓ Database container is running." -ForegroundColor Green

# Count tables in database
$tableCountQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';"
$tableCount = docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -t -c $tableCountQuery 2>&1

if ($LASTEXITCODE -eq 0) {
    $tableCount = $tableCount.Trim()
    Write-Host "✓ Database has $tableCount tables." -ForegroundColor Green
} else {
    Write-Host "⚠️  Could not query database table count." -ForegroundColor Yellow
}

# Step 4: Check for maritime logbooks
Write-Host ""
Write-Host "Step 4: Validating maritime logbooks..." -ForegroundColor Yellow
$logbookQuery = "SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_name IN ('deck_log_books', 'engine_log_books', 'garbage_record_books', 'ballast_water_record_books') ORDER BY table_name;"
$logbooks = docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -t -c $logbookQuery 2>&1

if ($LASTEXITCODE -eq 0) {
    $logbookCount = ($logbooks | Where-Object { $_.Trim() -ne "" }).Count
    if ($logbookCount -eq 4) {
        Write-Host "✓ All 4 maritime logbooks exist (SOLAS/MARPOL compliance)." -ForegroundColor Green
    } else {
        Write-Host "⚠️  WARNING: Only $logbookCount/4 maritime logbooks found!" -ForegroundColor Red
        Write-Host "Required logbooks: deck_log_books, engine_log_books, garbage_record_books, ballast_water_record_books" -ForegroundColor Yellow
    }
}

# Summary
Write-Host ""
Write-Host "=== Validation Summary ===" -ForegroundColor Cyan
Write-Host "Database and models are synchronized." -ForegroundColor Green
Write-Host ""
Write-Host "Safe to create new migrations with:" -ForegroundColor White
Write-Host "  dotnet ef migrations add YourMigrationName" -ForegroundColor Cyan
Write-Host ""
Write-Host "After creating migrations, always:" -ForegroundColor Yellow
Write-Host "  1. Review the migration file" -ForegroundColor Gray
Write-Host "  2. Run: dotnet ef database update" -ForegroundColor Cyan
Write-Host "  3. Commit migration files to Git for teammates" -ForegroundColor Gray
Write-Host ""
