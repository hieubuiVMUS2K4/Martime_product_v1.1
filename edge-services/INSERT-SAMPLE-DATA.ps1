# =====================================================
# INSERT SAMPLE DATA TO MARITIME EDGE DATABASE
# Run this script to populate database with test data
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  MARITIME EDGE - INSERT SAMPLE DATA" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Docker Desktop is not running!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "[OK] Docker is running" -ForegroundColor Green
Write-Host ""

# Check if PostgreSQL container is running
$containerStatus = docker ps --filter "name=maritime-edge-postgres" --format "{{.Status}}"
if (-not $containerStatus) {
    Write-Host "[ERROR] PostgreSQL container is not running!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start the database first:" -ForegroundColor Yellow
    Write-Host "  cd edge-services" -ForegroundColor White
    Write-Host "  docker compose up -d edge-postgres" -ForegroundColor White
    Write-Host ""
    pause
    exit 1
}

Write-Host "[OK] PostgreSQL container is running" -ForegroundColor Green
Write-Host ""

# Database connection details
$DB_NAME = "maritime_edge"
$DB_USER = "edge_user"
# Use the fixed SQL filename present in the repo
$SQL_FILE = "insert-sample-data-fixed.sql"

# Resolve SQL file relative to the script's directory so script works when run from repo root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ScriptDir) { $ScriptDir = Get-Location }
$SQL_PATH = Join-Path $ScriptDir $SQL_FILE

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  INSERTING SAMPLE DATA..." -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Database: $DB_NAME" -ForegroundColor White
Write-Host "User: $DB_USER" -ForegroundColor White
Write-Host "SQL File: $SQL_FILE" -ForegroundColor White
Write-Host ""

# Ensure SQL file exists locally
if (-not (Test-Path -Path $SQL_PATH)) {
    Write-Host "[ERROR] SQL file '$SQL_PATH' not found." -ForegroundColor Red
    Write-Host "Looking in script directory: $ScriptDir" -ForegroundColor Yellow
    Write-Host "Available files in that directory:" -ForegroundColor Yellow
    Get-ChildItem -Path $ScriptDir -File | ForEach-Object { Write-Host "  $_" }
    pause
    exit 1
}

# Copy SQL file to container
Write-Host "[1/4] Copying SQL file to container..." -ForegroundColor Yellow
docker cp $SQL_PATH maritime-edge-postgres:/tmp/insert-sample-data.sql
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to copy SQL file! (docker cp returned exit code $LASTEXITCODE)" -ForegroundColor Red
    pause
    exit 1
}
Write-Host "[OK] SQL file copied" -ForegroundColor Green
Write-Host ""

 # Execute SQL file
Write-Host "[2/4] Executing SQL script..." -ForegroundColor Yellow
Write-Host ""
# Remove -it to avoid TTY blocking; enable ON_ERROR_STOP for psql to fail fast
$startTime = Get-Date
docker exec maritime-edge-postgres psql -U $DB_USER -d $DB_NAME -v ON_ERROR_STOP=1 -f /tmp/insert-sample-data.sql
if ($LASTEXITCODE -ne 0) {
    $elapsed = (Get-Date) - $startTime
    Write-Host "" 
    Write-Host "[ERROR] Failed to execute SQL script (exit $LASTEXITCODE). Elapsed: $($elapsed.TotalSeconds) sec" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  1. Tables already exist with data - Try truncating or dropping before insert" -ForegroundColor White
    Write-Host "  2. Duplicate key violations - Data already inserted" -ForegroundColor White
    Write-Host "  3. Connection issues - Check database is healthy and credentials are correct" -ForegroundColor White
    Write-Host ""
    pause
    exit 1
}
$elapsed = (Get-Date) - $startTime
Write-Host "[OK] SQL executed successfully in $([int]$elapsed.TotalSeconds) seconds" -ForegroundColor Green
Write-Host ""

# Cleanup
Write-Host "[3/4] Cleaning up..." -ForegroundColor Yellow
docker exec maritime-edge-postgres rm -f /tmp/insert-sample-data.sql
Write-Host "[OK] Cleanup complete" -ForegroundColor Green
Write-Host ""

Write-Host "================================================" -ForegroundColor Green
Write-Host "  SAMPLE DATA INSERTED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Sample data inserted:" -ForegroundColor Cyan
Write-Host "  - 6 Crew Members (CM001-CM006)" -ForegroundColor White
Write-Host "  - 10 Maintenance Tasks (9 pending, 1 completed)" -ForegroundColor White
Write-Host "  - 4 Safety Alarms (1 critical, 2 warning, 1 info)" -ForegroundColor White
Write-Host "  - 1 Active Voyage" -ForegroundColor White
Write-Host "  - Sample Telemetry Data (GPS, Engine, Generators, Tanks)" -ForegroundColor White
Write-Host ""

Write-Host "Test Accounts for Mobile App:" -ForegroundColor Cyan
Write-Host "  Crew ID: CM001 | Password: password123 | John Smith (Chief Engineer)" -ForegroundColor White
Write-Host "  Crew ID: CM002 | Password: password123 | David Wilson (2nd Engineer)" -ForegroundColor White
Write-Host "  Crew ID: CM003 | Password: password123 | Mike Johnson (Electrician)" -ForegroundColor White
Write-Host "  Crew ID: CM004 | Password: password123 | Robert Brown (Deck Officer)" -ForegroundColor White
Write-Host "  Crew ID: CM005 | Password: password123 | Carlos Garcia (Fitter)" -ForegroundColor White
Write-Host "  Crew ID: CM006 | Password: password123 | Ahmed Hassan (Oiler)" -ForegroundColor White
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Start Edge Server:" -ForegroundColor Yellow
Write-Host "     dotnet run --urls 'http://0.0.0.0:5001'" -ForegroundColor White
Write-Host ""
Write-Host "  2. Check Swagger UI:" -ForegroundColor Yellow
Write-Host "     http://localhost:5001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "  3. Run Mobile App:" -ForegroundColor Yellow
Write-Host "     cd ..\frontend-mobile" -ForegroundColor White
Write-Host "     flutter run -d windows" -ForegroundColor White
Write-Host ""
Write-Host "  4. Or run Frontend-Edge:" -ForegroundColor Yellow
Write-Host "     cd ..\frontend-edge" -ForegroundColor White
Write-Host "     npm run dev" -ForegroundColor White
Write-Host ""

Write-Host "View data in pgAdmin:" -ForegroundColor Cyan
Write-Host "  http://localhost:5050" -ForegroundColor White
Write-Host "  Email: admin@localhost.com | Password: admin" -ForegroundColor White
Write-Host ""

pause
