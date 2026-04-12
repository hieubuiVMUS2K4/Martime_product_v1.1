#!/usr/bin/env pwsh

# Script kiểm tra báo cáo và evaluation trong database

$connectionString = "Host=localhost;Port=5434;Database=productdb;Username=product;Password=productpwd"

# Tạo C# script để query
$csharpCode = @"
using Npgsql;
using System;

var connString = \"$connectionString\";

try
{
    using (var conn = new NpgsqlConnection(connString))
    {
        conn.Open();
        
        // Check tables exist
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = \"\"\"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name IN ('NoonReports', 'ReportEvaluations', 'maritime_reports')
            \"\"\";
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine(\"=== Tables Found ===\");
                while (reader.Read())
                {
                    Console.WriteLine($\"  - {reader[\"table_name\"]}\");
                }
            }
        }
        
        // Count NoonReports
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = \"SELECT COUNT(*) as count FROM \\\"NoonReports\\\"\";
            var count = (long)cmd.ExecuteScalar();
            Console.WriteLine($\"\\n=== NoonReports Count ===\");
            Console.WriteLine($\"  Total: {count}\");
        }
        
        // Count ReportEvaluations
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = \"SELECT COUNT(*) as count FROM \\\"ReportEvaluations\\\"\";
            var count = (long)cmd.ExecuteScalar();
            Console.WriteLine($\"\\n=== ReportEvaluations Count ===\");
            Console.WriteLine($\"  Total: {count}\");
        }
        
        // List latest NoonReports
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = \"\"\"
                SELECT id, \"ReportDate\", \"MaritimeReportId\"
                FROM \\\"NoonReports\\\"
                ORDER BY \"ReportDate\" DESC
                LIMIT 10
            \"\"\";
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine($\"\\n=== Latest 10 NoonReports ===\");
                while (reader.Read())
                {
                    Console.WriteLine($\"  ID: {reader[\"id\"]} | Date: {reader[\"ReportDate\"]} | Maritime: {reader[\"MaritimeReportId\"]}\");
                }
            }
        }
        
        // List evaluations
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = \"\"\"
                SELECT \\\"ReportId\\\", \\\"Status\\\"
                FROM \\\"ReportEvaluations\\\"
                ORDER BY id DESC
                LIMIT 10
            \"\"\";
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine($\"\\n=== Latest 10 Evaluations ===\");
                while (reader.Read())
                {
                    Console.WriteLine($\"  Report: {reader[\"ReportId\"]} | Status: {reader[\"Status\"]}\");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($\"Error: {ex.Message}\");
}
"@

# Chạy script
dotnet script <(Write-Output $csharpCode)
