# Insert Sample Data to PostgreSQL
# Run from edge-services directory

$env:PGPASSWORD = "postgres"

Write-Host "Inserting sample data into maritime_edge database..." -ForegroundColor Cyan

# Execute the SQL file
psql -h localhost -p 5433 -U postgres -d maritime_edge -f insert-sample-data.sql

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Sample data inserted successfully!" -ForegroundColor Green
    
    Write-Host "`nVerifying data..." -ForegroundColor Yellow
    
    # Count records
    $crewCount = psql -h localhost -p 5433 -U postgres -d maritime_edge -t -c "SELECT COUNT(*) FROM crew_members;"
    $positionCount = psql -h localhost -p 5433 -U postgres -d maritime_edge -t -c "SELECT COUNT(*) FROM position_data;"
    $alarmCount = psql -h localhost -p 5433 -U postgres -d maritime_edge -t -c "SELECT COUNT(*) FROM safety_alarms;"
    
    Write-Host "Crew Members: $crewCount" -ForegroundColor White
    Write-Host "Position Data: $positionCount" -ForegroundColor White
    Write-Host "Safety Alarms: $alarmCount" -ForegroundColor White
} else {
    Write-Host "`n❌ Failed to insert data. Check errors above." -ForegroundColor Red
}

$env:PGPASSWORD = $null
