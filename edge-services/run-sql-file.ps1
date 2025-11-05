# Script to run SQL file and insert roles/users into database
# Usage: .\run-sql-file.ps1 -SqlFile "insert-roles-and-users.sql"

param(
    [Parameter(Mandatory=$false)]
    [string]$SqlFile = "insert-roles-and-users.sql"
)

$env:PGPASSWORD = "admin123"
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "maritime_edge"
$dbUser = "postgres"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Running SQL file: $SqlFile" -ForegroundColor Cyan
Write-Host "Database: $dbName @ $dbHost:$dbPort" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

try {
    # Check if psql is available
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        Write-Host "❌ ERROR: psql not found!" -ForegroundColor Red
        Write-Host "`nPlease install PostgreSQL or add psql to PATH" -ForegroundColor Yellow
        Write-Host "Or use pgAdmin 4 to run the SQL file manually" -ForegroundColor Yellow
        exit 1
    }

    # Check if file exists
    if (-not (Test-Path $SqlFile)) {
        Write-Host "❌ ERROR: File not found: $SqlFile" -ForegroundColor Red
        exit 1
    }

    Write-Host "Executing SQL file..." -ForegroundColor Yellow
    
    # Execute the SQL file
    psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f $SqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n================================" -ForegroundColor Green
        Write-Host "✅ SQL file executed successfully!" -ForegroundColor Green
        Write-Host "================================`n" -ForegroundColor Green
        
        Write-Host "Checking inserted data..." -ForegroundColor Cyan
        
        # Count roles
        $rolesCount = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c "SELECT COUNT(*) FROM roles;"
        Write-Host "  Roles: $($rolesCount.Trim())" -ForegroundColor White
        
        # Count users
        $usersCount = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c "SELECT COUNT(*) FROM users;"
        Write-Host "  Users: $($usersCount.Trim())" -ForegroundColor White
        
        Write-Host "`n✅ Done! You can now use the application." -ForegroundColor Green
        
    } else {
        Write-Host "`n❌ Failed to execute SQL file" -ForegroundColor Red
        Write-Host "Check the error messages above" -ForegroundColor Yellow
        exit 1
    }
    
} catch {
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
    exit 1
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
