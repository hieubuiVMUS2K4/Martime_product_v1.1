# Task Management Testing Guide
# Test the new Task Management System

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Task Management System - Quick Test Guide" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Prerequisites:" -ForegroundColor Yellow
Write-Host "1. Backend API running on http://localhost:5000" -ForegroundColor White
Write-Host "2. Frontend running on http://localhost:3001" -ForegroundColor White
Write-Host "3. PostgreSQL database with sample data" -ForegroundColor White
Write-Host ""

Write-Host "Testing Steps:" -ForegroundColor Green
Write-Host ""

Write-Host "Step 1: Check Health" -ForegroundColor Cyan
Write-Host "curl http://localhost:5000/api/task-management/health" -ForegroundColor Gray
Write-Host ""

Write-Host "Step 2: Get All Task Details (Library)" -ForegroundColor Cyan
Write-Host "curl http://localhost:5000/api/task-management/task-details/all" -ForegroundColor Gray
Write-Host ""

Write-Host "Step 3: Get All Task Types" -ForegroundColor Cyan
Write-Host "curl http://localhost:5000/api/task-management/task-types" -ForegroundColor Gray
Write-Host ""

Write-Host "Step 4: Create a New Task Detail" -ForegroundColor Cyan
$detailJson = @"
{
  "taskTypeId": 0,
  "detailCode": "CHECK_ENGINE_TEMP",
  "detailName": "Kiểm tra nhiệt độ động cơ",
  "detailType": "MEASUREMENT",
  "description": "Đo nhiệt độ nước làm mát động cơ",
  "isMandatory": true,
  "minValue": 80,
  "maxValue": 95,
  "unit": "°C"
}
"@
Write-Host 'curl -X POST http://localhost:5000/api/task-management/task-details `' -ForegroundColor Gray
Write-Host '  -H "Content-Type: application/json" `' -ForegroundColor Gray
Write-Host "  -d '$detailJson'" -ForegroundColor Gray
Write-Host ""

Write-Host "Step 5: Create a New Task Type" -ForegroundColor Cyan
$typeJson = @"
{
  "taskTypeCode": "DAILY_ENGINE_CHECK",
  "typeName": "Kiểm tra động cơ hàng ngày",
  "category": "ENGINE",
  "description": "Kiểm tra tổng thể động cơ chính hàng ngày",
  "priority": "HIGH",
  "estimatedDurationMinutes": 30,
  "requiresApproval": false
}
"@
Write-Host 'curl -X POST http://localhost:5000/api/task-management/task-types `' -ForegroundColor Gray
Write-Host '  -H "Content-Type: application/json" `' -ForegroundColor Gray
Write-Host "  -d '$typeJson'" -ForegroundColor Gray
Write-Host ""

Write-Host "Step 6: Frontend Testing" -ForegroundColor Cyan
Write-Host "1. Navigate to: http://localhost:3001/task-management" -ForegroundColor White
Write-Host "2. Tab '1️⃣ Chi Tiết Công Việc': Create new task details" -ForegroundColor White
Write-Host "3. Tab '2️⃣ Loại Công Việc': Create task types and select details" -ForegroundColor White
Write-Host ""

Write-Host "Current Database Sample Data:" -ForegroundColor Green
Write-Host "- 12 Task Types (ENGINE, SAFETY, DECK, ELECTRICAL categories)" -ForegroundColor White
Write-Host "- 13 Task Details (Engine Oil Change, Fire Equipment, Safety Drill workflows)" -ForegroundColor White
Write-Host ""

Write-Host "Quick Commands:" -ForegroundColor Yellow
Write-Host ""

# Function to run a test
function Test-Endpoint {
    param (
        [string]$Name,
        [string]$Url
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        Write-Host "✅ SUCCESS" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 3 | Write-Host
    }
    catch {
        Write-Host "❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Ask if user wants to run tests
$runTests = Read-Host "Do you want to run API tests now? (y/n)"

if ($runTests -eq 'y' -or $runTests -eq 'Y') {
    Write-Host ""
    Write-Host "Running API Tests..." -ForegroundColor Yellow
    Write-Host ""
    
    Test-Endpoint -Name "Health Check" -Url "http://localhost:5000/api/task-management/health"
    Test-Endpoint -Name "Get All Task Details" -Url "http://localhost:5000/api/task-management/task-details/all?activeOnly=true"
    Test-Endpoint -Name "Get All Task Types" -Url "http://localhost:5000/api/task-management/task-types?activeOnly=true"
    Test-Endpoint -Name "Get Categories" -Url "http://localhost:5000/api/task-management/categories"
    Test-Endpoint -Name "Get Priorities" -Url "http://localhost:5000/api/task-management/priorities"
    Test-Endpoint -Name "Get Detail Types" -Url "http://localhost:5000/api/task-management/detail-types"
    
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "Tests completed!" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Cyan
}
else {
    Write-Host "Tests skipped. Run this script again to test." -ForegroundColor Yellow
}
