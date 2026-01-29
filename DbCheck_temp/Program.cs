using System;
using System.IO;
using Npgsql;

var connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!;";

try 
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    
    Console.WriteLine("=== SEEDING SAMPLE DATA ===\n");
    
    // Read SQL file
    var sqlFile = @"E:\NCKH\Martime_product_v1.1\edge-services\seed-sample-data.sql";
    var sql = File.ReadAllText(sqlFile);
    
    using var cmd = new NpgsqlCommand(sql, connection);
    cmd.ExecuteNonQuery();
    
    Console.WriteLine("✓ Data seeded successfully!\n");
    
    // Verify counts
    using var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM countries", connection);
    Console.WriteLine($"Countries: {cmd1.ExecuteScalar()}");
    
    using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM certificates", connection);
    Console.WriteLine($"Certificates: {cmd2.ExecuteScalar()}");
    
    using var cmd3 = new NpgsqlCommand("SELECT COUNT(*) FROM crew_certificates", connection);
    Console.WriteLine($"Crew_Certificates: {cmd3.ExecuteScalar()}");
    
    using var cmd4 = new NpgsqlCommand("SELECT COUNT(*) FROM country_certificates", connection);
    Console.WriteLine($"Country_Certificates: {cmd4.ExecuteScalar()}");
    
    Console.WriteLine("\n=== COMPLETED ===");
} 
catch (Exception ex) 
{
    Console.WriteLine($"ERROR: {ex.Message}");
}
