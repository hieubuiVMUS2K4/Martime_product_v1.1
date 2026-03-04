#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;

var connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

try
{
    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    
    using var cmd = new NpgsqlCommand("DELETE FROM crew_certificates", conn);
    var rowsAffected = await cmd.ExecuteNonQueryAsync();
    
    Console.WriteLine($"✅ Successfully deleted {rowsAffected} rows from crew_certificates table");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Environment.Exit(1);
}
