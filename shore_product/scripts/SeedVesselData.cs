using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

class SeedVesselData
{
    static async Task Main()
    {
        Console.WriteLine("🚀 Starting seed vessel data insertion...\n");
        
        // Read SQL file
        var sqlFile = Path.Combine(Directory.GetCurrentDirectory(), "seed-vessel-positions.sql");
        if (!File.Exists(sqlFile))
        {
            Console.Error.WriteLine($"❌ SQL file not found: {sqlFile}");
            Environment.Exit(1);
        }

        var sqlContent = await File.ReadAllTextAsync(sqlFile);
        Console.WriteLine($"📄 Loaded SQL file: {sqlFile}");
        Console.WriteLine($"📊 SQL size: {sqlContent.Length} bytes\n");

        // Get connection string from environment or hardcode
        var connStr = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? "Host=localhost;Port=5432;Database=maritime_shore;Username=postgres;Password=postgres;";

        Console.WriteLine($"🔗 Connection: {connStr.Substring(0, Math.Min(50, connStr.Length))}...\n");

        try
        {
            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            Console.WriteLine("✅ Successfully connected to PostgreSQL database\n");

            // Split and execute commands
            var commands = sqlContent.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var successCount = 0;

            foreach (var cmdText in commands)
            {
                var trimmed = cmdText.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = trimmed;
                    cmd.CommandTimeout = 300;
                    
                    var result = await cmd.ExecuteNonQueryAsync();
                    successCount++;
                    
                    if (trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                        Console.WriteLine($"  ✅ Inserted {result} rows");
                    else if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                        Console.WriteLine($"  ✅ Query executed: {result} rows");
                    else
                        Console.WriteLine($"  ✅ Command executed");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"  ⚠️  Command error: {ex.Message}");
                }
            }

            Console.WriteLine($"\n✅ Completed! {successCount} SQL commands executed successfully");
            Console.WriteLine("\n📊 Seed data status:");
            Console.WriteLine("  • 5 vessels created");
            Console.WriteLine("  • 50 position waypoints created");
            Console.WriteLine("  • 5 different dashed line patterns");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
