# PowerShell Script to Insert Seed Vessel Data
# This script reads the SQL file and creates a temporary C# program to execute it

$sqlFile = "seed-vessel-positions.sql"
$sqlContent = Get-Content $sqlFile -Raw

if (-not $sqlContent) {
    Write-Error "Could not read SQL file: $sqlFile"
    exit 1
}

# Get connection string from appsettings
$appsettingsPath = "backend\appsettings.json"
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

Write-Host "📊 Inserting seed data into database..."
Write-Host "Connection: $($connectionString.Substring(0, 50))..."

# Create temporary C# program to execute SQL
$tempCsFile = "temp-seed-runner.cs"
$csharpCode = @"
using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main()
    {
        var connStr = "$connectionString";
        try
        {
            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            Console.WriteLine("✅ Connected to database");

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"$($sqlContent -replace '"', '""')";
            cmd.CommandTimeout = 300;
            
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Seed data inserted successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
"@

Set-Content -Path $tempCsFile -Value $csharpCode

Write-Host "🔧 Running temporary seeder program..."
& dotnet run -p $tempCsFile

# Clean up
Remove-Item $tempCsFile -Force -ErrorAction SilentlyContinue

Write-Host "✅ Done!"
