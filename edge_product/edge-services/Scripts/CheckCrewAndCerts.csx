#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;
using System;

var connStr = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

var conn = new NpgsqlConnection(connStr);
conn.Open();

// Get crew members
Console.WriteLine("\n=== CREW MEMBERS ===");
var cmdCrew = new NpgsqlCommand(@"
    SELECT cm.id, cm.crew_id, cm.full_name, cm.rank_id, r.rank_name, r.rank_code
    FROM crew_members cm
    LEFT JOIN ranks r ON cm.rank_id = r.id
    ORDER BY cm.rank_id, cm.crew_id;
", conn);

var readerCrew = cmdCrew.ExecuteReader();
Console.WriteLine("ID                                   | CrewID | Name           | RankID | Rank");
Console.WriteLine("-------------------------------------+--------+----------------+--------+------------------");
while (readerCrew.Read())
{
    var id = readerCrew.GetGuid(0);
    var crewId = readerCrew.GetString(1);
    var name = readerCrew.GetString(2).PadRight(15);
    var rankId = readerCrew.IsDBNull(3) ? "NULL" : readerCrew.GetInt32(3).ToString();
    var rankName = readerCrew.IsDBNull(4) ? "-" : readerCrew.GetString(4);
    var rankCode = readerCrew.IsDBNull(5) ? "-" : readerCrew.GetString(5);
    Console.WriteLine($"{id} | {crewId} | {name} | {rankId}      | {rankCode} - {rankName}");
}
readerCrew.Close();

// Get certificates
Console.WriteLine("\n\n=== CERTIFICATES ===");
var cmdCert = new NpgsqlCommand(@"
    SELECT id, certificate_code, certificate_name, category, validity_period_months
    FROM certificates
    ORDER BY id;
", conn);

var readerCert = cmdCert.ExecuteReader();
Console.WriteLine("ID | Code              | Name                              | Category    | Validity");
Console.WriteLine("---+-------------------+-----------------------------------+-------------+---------");
while (readerCert.Read())
{
    var id = readerCert.GetInt32(0);
    var code = readerCert.GetString(1).PadRight(18);
    var name = readerCert.GetString(2).PadRight(34);
    var category = readerCert.IsDBNull(3) ? "-".PadRight(12) : readerCert.GetString(3).PadRight(12);
    var validity = readerCert.IsDBNull(4) ? "-" : readerCert.GetInt32(4).ToString();
    Console.WriteLine($"{id,2} | {code} | {name} | {category} | {validity} months");
}
readerCert.Close();

// Get rank-certificates
Console.WriteLine("\n\n=== RANK-CERTIFICATES MAPPING ===");
var cmdRankCert = new NpgsqlCommand(@"
    SELECT rc.id, rc.rank_id, r.rank_code, r.rank_name, rc.certificate_id, c.certificate_code, c.certificate_name
    FROM rank_certificates rc
    JOIN ranks r ON rc.rank_id = r.id
    JOIN certificates c ON rc.certificate_id = c.id
    ORDER BY rc.rank_id, rc.certificate_id;
", conn);

var readerRC = cmdRankCert.ExecuteReader();
Console.WriteLine("ID | RankID | Rank Code | Rank Name          | CertID | Cert Code         | Cert Name");
Console.WriteLine("---+--------+-----------+--------------------+--------+-------------------+-------------------------");
while (readerRC.Read())
{
    var id = readerRC.GetInt32(0);
    var rankId = readerRC.GetInt32(1);
    var rankCode = readerRC.GetString(2).PadRight(10);
    var rankName = readerRC.GetString(3).PadRight(19);
    var certId = readerRC.GetInt32(4);
    var certCode = readerRC.GetString(5).PadRight(18);
    var certName = readerRC.GetString(6);
    Console.WriteLine($"{id,2} | {rankId,6} | {rankCode} | {rankName} | {certId,6} | {certCode} | {certName}");
}
readerRC.Close();

// Get existing crew certificates
Console.WriteLine("\n\n=== EXISTING CREW CERTIFICATES ===");
var cmdCrewCert = new NpgsqlCommand(@"
    SELECT cc.id, cm.crew_id, cm.full_name, cc.certificate_id, c.certificate_code, cc.certificate_number, cc.status
    FROM crew_certificates cc
    JOIN crew_members cm ON cc.crew_member_id = cm.id
    JOIN certificates c ON cc.certificate_id = c.id
    ORDER BY cm.crew_id, cc.certificate_id;
", conn);

var readerCC = cmdCrewCert.ExecuteReader();
Console.WriteLine("ID | CrewID | Name           | CertID | Cert Code         | Cert Number | Status");
Console.WriteLine("---+--------+----------------+--------+-------------------+-------------+--------");
int count = 0;
while (readerCC.Read())
{
    count++;
    var id = readerCC.GetInt32(0);
    var crewId = readerCC.GetString(1);
    var name = readerCC.GetString(2).PadRight(15);
    var certId = readerCC.GetInt32(3);
    var certCode = readerCC.GetString(4).PadRight(18);
    var certNum = readerCC.GetString(5);
    var status = readerCC.GetString(6);
    Console.WriteLine($"{id,2} | {crewId} | {name} | {certId,6} | {certCode} | {certNum} | {status}");
}
if (count == 0)
{
    Console.WriteLine("(No data)");
}
readerCC.Close();

conn.Close();

Console.WriteLine("\n✅ Query completed successfully!");
