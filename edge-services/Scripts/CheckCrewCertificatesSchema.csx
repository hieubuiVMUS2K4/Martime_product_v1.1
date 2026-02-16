#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;
using System;

var connStr = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

var conn = new NpgsqlConnection(connStr);
conn.Open();

Console.WriteLine("\n=== CREW_CERTIFICATES TABLE SCHEMA ===\n");

var cmd = new NpgsqlCommand(@"
    SELECT column_name, data_type, character_maximum_length, is_nullable
    FROM information_schema.columns 
    WHERE table_schema = 'public' AND table_name = 'crew_certificates' 
    ORDER BY ordinal_position;
", conn);

var reader = cmd.ExecuteReader();
Console.WriteLine("Column Name                | Data Type          | Max Length | Nullable");
Console.WriteLine("---------------------------+--------------------+------------+---------");
while (reader.Read())
{
    var colName = reader.GetString(0).PadRight(26);
    var dataType = reader.GetString(1).PadRight(19);
    var maxLen = reader.IsDBNull(2) ? "-".PadRight(11) : reader.GetInt32(2).ToString().PadRight(11);
    var nullable = reader.GetString(3);
    Console.WriteLine($"{colName}| {dataType}| {maxLen}| {nullable}");
}
reader.Close();

conn.Close();

Console.WriteLine("\n✅ Schema check completed!");
