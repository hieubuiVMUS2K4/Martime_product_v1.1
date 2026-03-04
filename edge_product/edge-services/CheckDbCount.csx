using System;
using Npgsql;

var connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!;Include Error Detail=true";

try 
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    
    Console.WriteLine("=== DATABASE RECORD COUNTS ===\n");
    
    // Count certificates
    using var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM certificates", connection);
    var certCount = cmd1.ExecuteScalar();
    Console.WriteLine($"Certificates: {certCount}");
    
    // Count crew_certificates
    using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM crew_certificates", connection);
    var crewCertCount = cmd2.ExecuteScalar();
    Console.WriteLine($"Crew_Certificates: {crewCertCount}\n");
    
    Console.WriteLine("=== CERTIFICATES TABLE DATA ===");
    using var cmd3 = new NpgsqlCommand(@"
        SELECT id, certificate_name, certificate_code, issuing_authority, stcw_reference 
        FROM certificates 
        ORDER BY id", connection);
    using var reader3 = cmd3.ExecuteReader();
    
    int count = 0;
    while (reader3.Read()) 
    {
        count++;
        Console.WriteLine($"{count}. ID: {reader3["id"]}, Name: {reader3["certificate_name"]}, Code: {reader3["certificate_code"]}, Authority: {reader3["issuing_authority"]}, STCW: {reader3["stcw_reference"]}");
    }
    
    reader3.Close();
    
    Console.WriteLine("\n=== CREW_CERTIFICATES TABLE DATA ===");
    using var cmd4 = new NpgsqlCommand(@"
        SELECT id, crew_id, certificate_id, issue_date, expiry_date 
        FROM crew_certificates 
        ORDER BY id 
        LIMIT 20", connection);
    using var reader4 = cmd4.ExecuteReader();
    
    count = 0;
    while (reader4.Read()) 
    {
        count++;
        Console.WriteLine($"{count}. ID: {reader4["id"]}, CrewID: {reader4["crew_id"]}, CertID: {reader4["certificate_id"]}, Issue: {reader4["issue_date"]}, Expiry: {reader4["expiry_date"]}");
    }
    
    connection.Close();
} 
catch (Exception ex) 
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
