$connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!"

Add-Type -AssemblyName "Npgsql"
$conn = New-Object Npgsql.NpgsqlConnection($connectionString)

try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT id, crew_id, full_name, position FROM crew_members ORDER BY id LIMIT 10"
    $reader = $cmd.ExecuteReader()
    
    Write-Host "`nCrew Members in Database:" -ForegroundColor Green
    Write-Host "ID`tCrew ID`t`tFull Name`t`tPosition" -ForegroundColor Yellow
    Write-Host "------------------------------------------------------------"
    
    while ($reader.Read()) {
        Write-Host "$($reader[0])`t$($reader[1])`t`t$($reader[2])`t`t$($reader[3])"
    }
    $reader.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
finally {
    if ($conn.State -eq 'Open') {
        $conn.Close()
    }
}
