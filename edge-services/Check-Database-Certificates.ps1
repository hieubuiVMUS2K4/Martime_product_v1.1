using namespace System.Data
using namespace Npgsql

# Connection string
$connString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!;"

try {
    Write-Host "=== DATABASE CHECK ===" -ForegroundColor Green
    Write-Host ""
    
    # Load Npgsql from the project's assemblies
    $dllPath = "E:\NCKH\Martime_product_v1.1\edge-services\bin\Debug\net8.0\Npgsql.dll"
    if (Test-Path $dllPath) {
        Add-Type -Path $dllPath
    } else {
        Write-Host "Loading Npgsql from NuGet..." -ForegroundColor Yellow
        $nugetPath = "$env:USERPROFILE\.nuget\packages\npgsql\8.0.1\lib\net8.0\Npgsql.dll"
        if (Test-Path $nugetPath) {
            Add-Type -Path $nugetPath
        } else {
            throw "Cannot find Npgsql.dll"
        }
    }
    
    # Create connection
    $conn = New-Object Npgsql.NpgsqlConnection($connString)
    $conn.Open()
    
    Write-Host "Connected to database!" -ForegroundColor Green
    Write-Host ""
    
    # Count certificates
    $cmd1 = $conn.CreateCommand()
    $cmd1.CommandText = "SELECT COUNT(*) FROM certificates"
    $certCount = $cmd1.ExecuteScalar()
    Write-Host "Total Certificates: $certCount" -ForegroundColor Cyan
    
    # Count crew_certificates
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT COUNT(*) FROM crew_certificates"
    $crewCertCount = $cmd2.ExecuteScalar()
    Write-Host "Total Crew_Certificates: $crewCertCount" -ForegroundColor Cyan
    Write-Host ""
    
    # Show some sample certificates
    Write-Host "=== SAMPLE CERTIFICATES (first 5) ===" -ForegroundColor Yellow
    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = @"
SELECT id, certificate_code, certificate_name, category, is_mandatory 
FROM certificates 
ORDER BY id 
LIMIT 5
"@
    $reader = $cmd3.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host ("ID: {0} | Code: {1} | Name: {2} | Category: {3} | Mandatory: {4}" -f 
            $reader["id"], 
            $reader["certificate_code"], 
            $reader["certificate_name"], 
            $reader["category"], 
            $reader["is_mandatory"])
    }
    $reader.Close()
    Write-Host ""
    
    # Show some sample crew certificates
    Write-Host "=== SAMPLE CREW_CERTIFICATES (first 5) ===" -ForegroundColor Yellow
    $cmd4 = $conn.CreateCommand()
    $cmd4.CommandText = @"
SELECT cc.id, cc.crew_id, c.certificate_name, cc.issue_date, cc.expiry_date 
FROM crew_certificates cc
JOIN certificates c ON cc.certificate_id = c.id
ORDER BY cc.id 
LIMIT 5
"@
    $reader2 = $cmd4.ExecuteReader()
    
    while ($reader2.Read()) {
        Write-Host ("ID: {0} | CrewID: {1} | Certificate: {2} | Issued: {3:yyyy-MM-dd} | Expires: {4:yyyy-MM-dd}" -f 
            $reader2["id"], 
            $reader2["crew_id"], 
            $reader2["certificate_name"], 
            $reader2["issue_date"], 
            $reader2["expiry_date"])
    }
    $reader2.Close()
    
    $conn.Close()
    Write-Host ""
    Write-Host "=== COMPLETED ===" -ForegroundColor Green
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack: $($_.Exception.StackTrace)" -ForegroundColor Red
}
