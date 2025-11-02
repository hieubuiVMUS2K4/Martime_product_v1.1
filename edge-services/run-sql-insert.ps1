# =====================================================
# Script chay SQL files vao PostgreSQL Database
# Maritime Edge System
# =====================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Maritime Edge - Database Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$DB_NAME = "maritime_edge"
$DB_USER = "edge_user"

# Kiem tra Docker container
Write-Host "[1/4] Checking Docker container..." -ForegroundColor Yellow
$containerStatus = docker ps --filter "name=maritime-edge-postgres" --format "{{.Status}}"
if ($containerStatus -match "Up") {
    Write-Host "OK - PostgreSQL container is running" -ForegroundColor Green
} else {
    Write-Host "ERROR - Container not running!" -ForegroundColor Red
    Write-Host "Please start the container first" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Insert Roles and Users
Write-Host "[2/4] Inserting Roles and Users..." -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Gray
if (Test-Path ".\insert-roles-and-users.sql") {
    $sql1 = Get-Content ".\insert-roles-and-users.sql" -Raw -Encoding UTF8
    $result1 = $sql1 | docker exec -i maritime-edge-postgres psql -U $DB_USER -d $DB_NAME 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK - Roles and Users inserted" -ForegroundColor Green
        $success1 = $true
    } else {
        Write-Host "ERROR - Failed to insert Roles and Users" -ForegroundColor Red
        Write-Host $result1 -ForegroundColor Red
        $success1 = $false
    }
} else {
    Write-Host "ERROR - File not found: insert-roles-and-users.sql" -ForegroundColor Red
    $success1 = $false
}
Write-Host ""

# Insert Materials and Maintenance
Write-Host "[3/4] Inserting Materials and Maintenance..." -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Gray
if (Test-Path ".\insert-10-more-data.sql") {
    $sql2 = Get-Content ".\insert-10-more-data.sql" -Raw -Encoding UTF8
    $result2 = $sql2 | docker exec -i maritime-edge-postgres psql -U $DB_USER -d $DB_NAME 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK - Materials and Maintenance inserted" -ForegroundColor Green
        $success2 = $true
    } else {
        Write-Host "ERROR - Failed to insert Materials and Maintenance" -ForegroundColor Red
        Write-Host $result2 -ForegroundColor Red
        $success2 = $false
    }
} else {
    Write-Host "ERROR - File not found: insert-10-more-data.sql" -ForegroundColor Red
    $success2 = $false
}
Write-Host ""

# Summary
Write-Host "[4/4] Summary" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Gray
if ($success1 -and $success2) {
    Write-Host "SUCCESS - All data inserted!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Default Login Credentials:" -ForegroundColor Cyan
    Write-Host "  Admin User:" -ForegroundColor White
    Write-Host "    Username: admin" -ForegroundColor Yellow
    Write-Host "    Password: admin123" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Crew Members:" -ForegroundColor White
    Write-Host "    Username: CM001, CM002, etc." -ForegroundColor Yellow
    Write-Host "    Password: ddMMyyyy or 123456" -ForegroundColor Yellow
} else {
    Write-Host "FAILED - Some insertions failed" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Done!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Read-Host "Press Enter to exit"
