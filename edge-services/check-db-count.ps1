# PowerShell script to check database counts using dotnet-ef or direct SQL
$connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!;"

# Using dotnet script approach
$code = @"
using System;
using Npgsql;

var connectionString = "$connectionString";

try {
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    
    Console.WriteLine("=== Database Record Counts ===\n");
    
    // Count certificates
    using var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM certificates", connection);
    var certCount = cmd1.ExecuteScalar();
    Console.WriteLine("Certificates: {0}", certCount);
    
    // Count crew_certificates
    using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM crew_certificates", connection);
    var crewCertCount = cmd2.ExecuteScalar();
    Console.WriteLine("Crew_Certificates: {0}", crewCertCount);
    
    Console.WriteLine("\n=== Certificates Data ===");
    using var cmd3 = new NpgsqlCommand("SELECT id, certificate_name, certificate_code FROM certificates", connection);
    using var reader = cmd3.ExecuteReader();
    while (reader.Read()) {
        Console.WriteLine("  ID: {0}, Name: {1}, Code: {2}", reader[0], reader[1], reader[2]);
    }
} catch (Exception ex) {
    Console.WriteLine("Error: {0}", ex.Message);
}
"@

# Save the code to a temp file
$tempFile = [System.IO.Path]::GetTempFileName()
$tempCsFile = $tempFile + ".csx"
$code | Out-File -FilePath $tempCsFile -Encoding UTF8

# Try to run with dotnet-script if available
if (Get-Command "dotnet-script" -ErrorAction SilentlyContinue) {
    dotnet-script $tempCsFile
} else {
    Write-Host "dotnet-script not installed. Installing..."
    dotnet tool install -g dotnet-script
    dotnet-script $tempCsFile
}

# Clean up
Remove-Item $tempCsFile -ErrorAction SilentlyContinue
