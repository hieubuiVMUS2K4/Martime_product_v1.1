# Script to insert sample TaskTypes into PostgreSQL database
# Run this from edge-services directory

$env:PGPASSWORD = "admin123"
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "maritime_edge"
$dbUser = "postgres"

Write-Host "Inserting sample TaskTypes into database..." -ForegroundColor Cyan

try {
    # Check if psql is available
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        Write-Host "ERROR: psql not found. Please install PostgreSQL client tools." -ForegroundColor Red
        exit 1
    }

    # Execute the SQL file
    psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f "init-scripts\insert-task-types.sql"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Successfully inserted TaskTypes!" -ForegroundColor Green
        Write-Host "`nVerifying data..." -ForegroundColor Cyan
        
        # Count inserted records
        $countQuery = "SELECT COUNT(*) as total FROM `"TaskTypes`" WHERE `"IsActive`" = true;"
        $result = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c $countQuery
        
        Write-Host "Total active TaskTypes: $($result.Trim())" -ForegroundColor Green
    } else {
        Write-Host "`n❌ Failed to insert TaskTypes" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
    exit 1
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
