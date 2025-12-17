using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EdgeCollector.Scripts
{
    public class SeedCrewData
    {
        public static async Task Execute(string connectionString)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            Console.WriteLine("🚢 Starting crew data seed...\n");

            // Delete existing test data
            Console.WriteLine("Cleaning up existing test data...");
            await ExecuteNonQuery(conn, @"DELETE FROM ""Users"" WHERE ""Username"" LIKE 'CREW%'");
            await ExecuteNonQuery(conn, @"DELETE FROM ""CrewMembers"" WHERE ""CrewId"" LIKE 'CREW%'");
            Console.WriteLine("✅ Cleanup complete\n");

            // Read and execute SQL file
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed-crew-and-users.sql");
            
            if (!File.Exists(sqlFile))
            {
                sqlFile = @"D:\Martime_product_v1\edge-services\seed-crew-and-users.sql";
            }

            if (!File.Exists(sqlFile))
            {
                Console.WriteLine("❌ SQL file not found!");
                return;
            }

            var sql = await File.ReadAllTextAsync(sqlFile);
            
            // Split by INSERT statements and execute
            var statements = sql.Split(new[] { "INSERT INTO" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.Trim().Length > 0)
                .Select(s => "INSERT INTO" + s)
                .ToList();

            Console.WriteLine($"Found {statements.Count} INSERT statements\n");

            int crewCount = 0;
            int userCount = 0;

            foreach (var statement in statements)
            {
                if (statement.Contains(@"""CrewMembers"""))
                {
                    await ExecuteNonQuery(conn, statement);
                    crewCount++;
                    Console.Write(".");
                }
                else if (statement.Contains(@"""Users"""))
                {
                    await ExecuteNonQuery(conn, statement);
                    userCount++;
                    Console.Write(".");
                }
            }

            Console.WriteLine($"\n\n✅ Seed completed!");
            Console.WriteLine($"   Crew Members: {crewCount}");
            Console.WriteLine($"   Users: {userCount}");

            // Verification
            Console.WriteLine("\n📊 Verification:");
            await VerifyData(conn);

            await conn.CloseAsync();
        }

        private static async Task ExecuteNonQuery(NpgsqlConnection conn, string sql)
        {
            try
            {
                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"SQL: {sql.Substring(0, Math.Min(100, sql.Length))}...");
            }
        }

        private static async Task VerifyData(NpgsqlConnection conn)
        {
            // Count by department
            var sql = @"
                SELECT 
                    ""Department"", 
                    COUNT(*) as ""CrewCount""
                FROM ""CrewMembers""
                WHERE ""CrewId"" LIKE 'CREW%'
                GROUP BY ""Department""
                ORDER BY ""Department""";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            Console.WriteLine("\nCrew by Department:");
            while (await reader.ReadAsync())
            {
                var dept = reader.GetString(0);
                var count = reader.GetInt32(1);
                Console.WriteLine($"  {dept}: {count}");
            }

            await reader.CloseAsync();

            // Count key positions
            sql = @"
                SELECT COUNT(*)
                FROM ""CrewMembers""
                WHERE ""Rank"" IN ('MASTER', 'C/E', 'C/O', '2/E', '3/E', 'E/O', 'BOSUN')
                  AND ""CrewId"" LIKE 'CREW%'";

            await using var cmd2 = new NpgsqlCommand(sql, conn);
            var keyCount = await cmd2.ExecuteScalarAsync();
            Console.WriteLine($"\nKey Positions: {keyCount}");
        }
    }
}
