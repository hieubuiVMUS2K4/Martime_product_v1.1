#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;
using System;

var connStr = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

var conn = new NpgsqlConnection(connStr);
conn.Open();

Console.WriteLine("\n🚢 === SEEDING CREW CERTIFICATES === 🚢\n");

// Get crew members with their ranks
var crewData = new List<(Guid id, string crewId, string name, int? rankId)>();
var cmdCrew = new NpgsqlCommand(@"
    SELECT id, crew_id, full_name, rank_id 
    FROM crew_members 
    ORDER BY crew_id;
", conn);

var readerCrew = cmdCrew.ExecuteReader();
while (readerCrew.Read())
{
    crewData.Add((
        readerCrew.GetGuid(0),
        readerCrew.GetString(1),
        readerCrew.GetString(2),
        readerCrew.IsDBNull(3) ? null : readerCrew.GetInt32(3)
    ));
}
readerCrew.Close();

Console.WriteLine($"Found {crewData.Count} crew members\n");

// Certificate data to insert
// Format: (CrewMemberId, CertificateId, CertNumber, IssueDate, ExpiryDate, Status, Authority, Notes)
var certificatesToInsert = new List<(Guid, int, string, DateTime, DateTime, string, string, string)>();

var now = DateTime.UtcNow;
var countryId = 1; // Vietnam

// CREW-001: Master (Rank 1) - Fully compliant
var crew001 = crewData.FirstOrDefault(c => c.crewId == "CREW-001");
if (crew001.id != Guid.Empty)
{
    certificatesToInsert.Add((crew001.id, 1, "STCW-M-001234", now.AddYears(-3), now.AddYears(2), "VALID", "VN Maritime Admin", "CoC - Master"));
    certificatesToInsert.Add((crew001.id, 3, "MED-001234", now.AddMonths(-12), now.AddMonths(12), "VALID", "Maritime Medical", "Annual medical"));
    certificatesToInsert.Add((crew001.id, 4, "BST-001234", now.AddYears(-4), now.AddYears(1), "VALID", "Training Center", "Basic Safety"));
    certificatesToInsert.Add((crew001.id, 5, "GMDSS-001234", now.AddYears(-2), now.AddYears(3), "VALID", "GMDSS Center", "General Operator"));
}

// CREW-002: Chief Officer (Rank 2) - Partially compliant (missing one cert)
var crew002 = crewData.FirstOrDefault(c => c.crewId == "CREW-002");
if (crew002.id != Guid.Empty)
{
    certificatesToInsert.Add((crew002.id, 2, "STCW-CO-002345", now.AddYears(-2), now.AddYears(3), "VALID", "VN Maritime Admin", "CoC - Chief Officer"));
    certificatesToInsert.Add((crew002.id, 3, "MED-002345", now.AddMonths(-6), now.AddMonths(18), "VALID", "Maritime Medical", "Medical valid"));
    // Missing BASIC_SAFETY for testing partial compliance
}

// CREW-003: Chief Engineer (Rank 5) - Has expired certificate
var crew003 = crewData.FirstOrDefault(c => c.crewId == "CREW-003");
if (crew003.id != Guid.Empty)
{
    certificatesToInsert.Add((crew003.id, 1, "STCW-CE-003456", now.AddYears(-4), now.AddYears(1), "VALID", "VN Maritime Admin", "Engineering Cert"));
    certificatesToInsert.Add((crew003.id, 3, "MED-003456", now.AddMonths(-26), now.AddMonths(-2), "EXPIRED", "Maritime Medical", "Medical expired"));
    certificatesToInsert.Add((crew003.id, 4, "BST-003456", now.AddYears(-3), now.AddYears(2), "VALID", "Training Center", "Basic Safety"));
}

// CREW-004: Second Officer (Rank 3) - Expiring soon
var crew004 = crewData.FirstOrDefault(c => c.crewId == "CREW-004");
if (crew004.id != Guid.Empty)
{
    certificatesToInsert.Add((crew004.id, 2, "STCW-2O-004567", now.AddYears(-2), now.AddYears(3), "VALID", "VN Maritime Admin", "Second Officer Cert"));
    certificatesToInsert.Add((crew004.id, 3, "MED-004567", now.AddMonths(-22), now.AddMonths(2), "VALID", "Maritime Medical", "Expiring soon"));
    certificatesToInsert.Add((crew004.id, 4, "BST-004567", now.AddYears(-5), now.AddMonths(6), "VALID", "Training Center", "BST expiring"));
}

// CREW-005: Able Seaman (Rank 8) - Basic certificates only
var crew005 = crewData.FirstOrDefault(c => c.crewId == "CREW-005");
if (crew005.id != Guid.Empty)
{
    certificatesToInsert.Add((crew005.id, 3, "MED-005678", now.AddMonths(-8), now.AddMonths(16), "VALID", "Maritime Medical", "Medical cert"));
    certificatesToInsert.Add((crew005.id, 4, "BST-005678", now.AddYears(-2), now.AddYears(3), "VALID", "Training Center", "Basic Safety"));
}

// CREW-006: Second Engineer (Rank 6) - Fully compliant with tanker training
var crew006 = crewData.FirstOrDefault(c => c.crewId == "CREW-006");
if (crew006.id != Guid.Empty)
{
    certificatesToInsert.Add((crew006.id, 1, "STCW-2E-006789", now.AddYears(-1), now.AddYears(4), "VALID", "VN Maritime Admin", "Second Engineer"));
    certificatesToInsert.Add((crew006.id, 3, "MED-006789", now.AddMonths(-4), now.AddMonths(20), "VALID", "Maritime Medical", "Medical valid"));
    certificatesToInsert.Add((crew006.id, 4, "BST-006789", now.AddYears(-3), now.AddYears(2), "VALID", "Training Center", "Basic Safety"));
    certificatesToInsert.Add((crew006.id, 6, "TANK-006789", now.AddYears(-1), now.AddYears(4), "VALID", "Tanker Center", "Tanker Training"));
}

// CREW-007: Bosun (Rank 7) - No certificates (non-compliant)
var crew007 = crewData.FirstOrDefault(c => c.crewId == "CREW-007");
// Intentionally leaving empty for testing non-compliance

Console.WriteLine($"Preparing to insert {certificatesToInsert.Count} crew certificates...\n");

// Insert certificates
int successCount = 0;
foreach (var cert in certificatesToInsert)
{
    try
    {
        var cmdInsert = new NpgsqlCommand(@"
            INSERT INTO crew_certificates (
                crew_member_id, certificate_id, certificate_number, 
                issue_date, expiry_date, issuing_authority, 
                country_id, status, notes, 
                is_synced, origin_node, created_at, updated_at
            )
            VALUES (
                @crewMemberId, @certificateId, @certificateNumber,
                @issueDate, @expiryDate, @issuingAuthority,
                @countryId, @status, @notes,
                false, 'SHIP_01', @createdAt, @updatedAt
            )
            RETURNING id;
        ", conn);
        
        cmdInsert.Parameters.AddWithValue("crewMemberId", cert.Item1);
        cmdInsert.Parameters.AddWithValue("certificateId", cert.Item2);
        cmdInsert.Parameters.AddWithValue("certificateNumber", cert.Item3);
        cmdInsert.Parameters.AddWithValue("issueDate", cert.Item4);
        cmdInsert.Parameters.AddWithValue("expiryDate", cert.Item5);
        cmdInsert.Parameters.AddWithValue("issuingAuthority", cert.Item6);
        cmdInsert.Parameters.AddWithValue("countryId", countryId);
        cmdInsert.Parameters.AddWithValue("status", cert.Item7);
        cmdInsert.Parameters.AddWithValue("notes", cert.Item8);
        cmdInsert.Parameters.AddWithValue("createdAt", now);
        cmdInsert.Parameters.AddWithValue("updatedAt", now);
        
        var newId = cmdInsert.ExecuteScalar();
        Console.WriteLine($"✅ Inserted certificate ID {newId}: {cert.Item3} for crew member");
        successCount++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to insert {cert.Item3}: {ex.Message}");
    }
}

Console.WriteLine($"\n📊 Summary:");
Console.WriteLine($"   Total certificates inserted: {successCount}/{certificatesToInsert.Count}");

// Verify inserted data
var cmdVerify = new NpgsqlCommand(@"
    SELECT 
        cm.crew_id, 
        cm.full_name,
        COUNT(cc.id) as cert_count,
        SUM(CASE WHEN cc.status = 'VALID' THEN 1 ELSE 0 END) as valid_count,
        SUM(CASE WHEN cc.status = 'EXPIRED' THEN 1 ELSE 0 END) as expired_count
    FROM crew_members cm
    LEFT JOIN crew_certificates cc ON cm.id = cc.crew_member_id
    GROUP BY cm.crew_id, cm.full_name
    ORDER BY cm.crew_id;
", conn);

Console.WriteLine($"\n📋 Verification - Certificates by Crew Member:");
Console.WriteLine("CrewID   | Name             | Total | Valid | Expired");
Console.WriteLine("---------+------------------+-------+-------+--------");

var readerVerify = cmdVerify.ExecuteReader();
while (readerVerify.Read())
{
    var crewId = readerVerify.GetString(0);
    var name = readerVerify.GetString(1).PadRight(17);
    var total = readerVerify.GetInt64(2);
    var valid = readerVerify.GetInt64(3);
    var expired = readerVerify.GetInt64(4);
    Console.WriteLine($"{crewId} | {name} | {total,5} | {valid,5} | {expired,7}");
}
readerVerify.Close();

conn.Close();

Console.WriteLine("\n✅ Crew certificates seeding completed successfully!");
Console.WriteLine("\n💡 Test scenarios created:");
Console.WriteLine("   - CREW-001: Fully compliant (all certificates valid)");
Console.WriteLine("   - CREW-002: Partially compliant (missing certificate)");
Console.WriteLine("   - CREW-003: Has expired certificate");
Console.WriteLine("   - CREW-004: Certificates expiring soon");
Console.WriteLine("   - CREW-005: Basic certificates only");
Console.WriteLine("   - CREW-006: Fully compliant with extra tanker training");
Console.WriteLine("   - CREW-007: No certificates (non-compliant)");
