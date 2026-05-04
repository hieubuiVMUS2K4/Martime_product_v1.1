using System;
using System.Linq;
using System.Text.Json;
using Npgsql;
namespace ConsoleApp {
    public class Program {
        public static void Main() {
            try {
                using var conn = new NpgsqlConnection("Host=localhost;Port=5434;Database=productdb;Username=product;Password=REPLACE_WITH_LOCAL_DB_PASSWORD;");
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT \"Timestamp\", \"Latitude\", \"Longitude\" FROM position_data ORDER BY \"Timestamp\" DESC LIMIT 15", conn);
                using var reader = cmd.ExecuteReader();
                Console.WriteLine("SHORE POSITIONS:");
                while (reader.Read()) {
                    Console.WriteLine($"{reader.GetDateTime(0):O} | {reader.GetDouble(1)} | {reader.GetDouble(2)}");
                }
            } catch (Exception ex) { Console.WriteLine("Shore Error: " + ex.Message); }
            
            try {
                using var conn2 = new NpgsqlConnection("Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=edge_password;");
                conn2.Open();
                // Edge table name might just be lowercase or same as model?
                using var cmd2 = new NpgsqlCommand("SELECT \"Timestamp\", \"Latitude\", \"Longitude\" FROM position_data ORDER BY \"Timestamp\" DESC LIMIT 15", conn2);
                using var reader2 = cmd2.ExecuteReader();
                Console.WriteLine("\nEDGE POSITIONS:");
                while (reader2.Read()) {
                    Console.WriteLine($"{reader2.GetDateTime(0):O} | {reader2.GetDouble(1)} | {reader2.GetDouble(2)}");
                }
            } catch (Exception ex) { Console.WriteLine("Edge Error: " + ex.Message); }
        }
    }
}
