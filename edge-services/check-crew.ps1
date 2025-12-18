$query = @"
SELECT crew_id, full_name, position, is_active 
FROM crew 
WHERE crew_id = 'CREW003';
"@

dotnet user-secrets list --project . | Select-String "Database"

Write-Host "`nChecking CREW003 in database..." -ForegroundColor Yellow
Write-Host $query
