#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"
#nullable enable

using Npgsql;
using System;
using System.Linq;

var connStr = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

Console.WriteLine("\n🔍 === CHECKING BIO-DATA SCHEMA & DATA === 🔍\n");

var conn = new NpgsqlConnection(connStr);
try
{
conn.Open();

// ============================================
// 1. CHECK CREW_MEMBERS NEW COLUMNS
// ============================================
Console.WriteLine("📋 Step 1: Checking crew_members table schema...\n");

var checkColumnsCmd = new NpgsqlCommand(@"
    SELECT column_name, data_type, character_maximum_length
    FROM information_schema.columns 
    WHERE table_name = 'crew_members' 
    AND column_name IN (
        'place_of_birth', 'id_card_number', 'marital_status',
        'height', 'weight', 'blood_group',
        'clothing_size', 'shoe_size', 'catering_size',
        'is_smoker', 'is_covid_vaccinated', 'photo_url',
        'next_of_kin_name', 'next_of_kin_relation', 'next_of_kin_phone', 'next_of_kin_address',
        'education_institution', 'education_course', 'education_period_years', 'education_graduation_year'
    )
    ORDER BY column_name;
", conn);

using (var reader = checkColumnsCmd.ExecuteReader())
{
    Console.WriteLine("New BIO-DATA columns:");
    while (reader.Read())
    {
        var colName = reader.GetString(0);
        var dataType = reader.GetString(1);
        var maxLength = reader.IsDBNull(2) ? "" : $"({reader.GetInt32(2)})";
        Console.WriteLine($"  ✅ {colName} -> {dataType}{maxLength}");
    }
}

Console.WriteLine();

// ============================================
// 2. CHECK SERVICE_RECORDS TABLE
// ============================================
Console.WriteLine("📋 Step 2: Checking service_records table...\n");

var checkTableCmd = new NpgsqlCommand(@"
    SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = 'public' AND table_name = 'service_records';
", conn);

var tableExists = (long)checkTableCmd.ExecuteScalar()! > 0;

if (tableExists)
{
    Console.WriteLine("✅ service_records table exists!");
    
    var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM public.service_records", conn);
    var recordCount = (long)countCmd.ExecuteScalar()!;
    Console.WriteLine($"   Current records: {recordCount}\n");
}
else
{
    Console.WriteLine("❌ service_records table NOT found!\n");
    return 1;
}

// ============================================
// 3. CHECK EXISTING CREW DATA
// ============================================
Console.WriteLine("📋 Step 3: Checking existing crew members...\n");

var getCrewCmd = new NpgsqlCommand(@"
    SELECT id, crew_id, full_name, nationality, date_of_birth,
           place_of_birth, height, blood_group, education_institution
    FROM public.crew_members 
    LIMIT 10;
", conn);

var crewList = new List<(Guid id, string crewId, string name, string? nationality, DateTime? dob, 
                         string? pob, int? height, string? blood, string? edu)>();

using (var reader = getCrewCmd.ExecuteReader())
{
    while (reader.Read())
    {
        crewList.Add((
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetInt32(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8)
        ));
    }
}

Console.WriteLine($"Found {crewList.Count} crew members");
Console.WriteLine("\nCurrent data sample:");
foreach (var crew in crewList.Take(3))
{
    Console.WriteLine($"  • {crew.crewId} - {crew.name}");
    Console.WriteLine($"    Place of Birth: {crew.pob ?? "(null)"}");
    Console.WriteLine($"    Height: {crew.height?.ToString() ?? "(null)"} cm");
    Console.WriteLine($"    Blood Group: {crew.blood ?? "(null)"}");
    Console.WriteLine($"    Education: {crew.edu ?? "(null)"}");
    Console.WriteLine();
}

// ============================================
// 4. SEED SAMPLE BIO-DATA
// ============================================
Console.WriteLine("\n📝 Step 4: Seeding sample bio-data...\n");

var sampleData = new[]
{
    new {
        PlaceOfBirth = "Hà Nội", IdCard = "001234567890", Marital = "Single",
        Height = 170, Weight = 65m, Blood = "O+",
        Clothing = "L", Shoe = "42", Catering = "M",
        Smoker = false, Covid = true,
        KinName = "Nguyễn Văn An", KinRelation = "Father", KinPhone = "0901234567", KinAddress = "Hà Nội",
        EduInstitution = "Vietnam Maritime University", EduCourse = "Deck", EduYears = 4, EduGrad = 2020
    },
    new {
        PlaceOfBirth = "Hải Phòng", IdCard = "001987654321", Marital = "Married",
        Height = 175, Weight = 70m, Blood = "A+",
        Clothing = "XL", Shoe = "43", Catering = "L",
        Smoker = false, Covid = true,
        KinName = "Trần Thị Bình", KinRelation = "Spouse", KinPhone = "0907654321", KinAddress = "Hải Phòng",
        EduInstitution = "Vietnam Maritime University", EduCourse = "Engine", EduYears = 4, EduGrad = 2019
    },
    new {
        PlaceOfBirth = "Thanh Hóa", IdCard = "038694024325", Marital = "Single",
        Height = 163, Weight = 52m, Blood = "B",
        Clothing = "160", Shoe = "24", Catering = "24",
        Smoker = true, Covid = true,
        KinName = "Hoàng Văn Thùy", KinRelation = "Father", KinPhone = "0386147308", KinAddress = "TDP Nam Thanh, phường Ngọc Sơn, tỉnh Thanh Hóa",
        EduInstitution = "Vietnam Maritime University", EduCourse = "Engine", EduYears = 6, EduGrad = 2019
    }
};

int updatedCount = 0;

for (int i = 0; i < Math.Min(sampleData.Length, crewList.Count); i++)
{
    var crew = crewList[i];
    var data = sampleData[i];
    
    var updateCmd = new NpgsqlCommand(@"
        UPDATE public.crew_members SET
            place_of_birth = @pob,
            id_card_number = @idcard,
            marital_status = @marital,
            height = @height,
            weight = @weight,
            blood_group = @blood,
            clothing_size = @clothing,
            shoe_size = @shoe,
            catering_size = @catering,
            is_smoker = @smoker,
            is_covid_vaccinated = @covid,
            next_of_kin_name = @kinname,
            next_of_kin_relation = @kinrelation,
            next_of_kin_phone = @kinphone,
            next_of_kin_address = @kinaddress,
            education_institution = @eduinst,
            education_course = @educourse,
            education_period_years = @eduyears,
            education_graduation_year = @edugrad,
            updated_at = NOW()
        WHERE id = @id;
    ", conn);
    
    updateCmd.Parameters.AddWithValue("pob", data.PlaceOfBirth);
    updateCmd.Parameters.AddWithValue("idcard", data.IdCard);
    updateCmd.Parameters.AddWithValue("marital", data.Marital);
    updateCmd.Parameters.AddWithValue("height", data.Height);
    updateCmd.Parameters.AddWithValue("weight", data.Weight);
    updateCmd.Parameters.AddWithValue("blood", data.Blood);
    updateCmd.Parameters.AddWithValue("clothing", data.Clothing);
    updateCmd.Parameters.AddWithValue("shoe", data.Shoe);
    updateCmd.Parameters.AddWithValue("catering", data.Catering);
    updateCmd.Parameters.AddWithValue("smoker", data.Smoker);
    updateCmd.Parameters.AddWithValue("covid", data.Covid);
    updateCmd.Parameters.AddWithValue("kinname", data.KinName);
    updateCmd.Parameters.AddWithValue("kinrelation", data.KinRelation);
    updateCmd.Parameters.AddWithValue("kinphone", data.KinPhone);
    updateCmd.Parameters.AddWithValue("kinaddress", data.KinAddress);
    updateCmd.Parameters.AddWithValue("eduinst", data.EduInstitution);
    updateCmd.Parameters.AddWithValue("educourse", data.EduCourse);
    updateCmd.Parameters.AddWithValue("eduyears", data.EduYears);
    updateCmd.Parameters.AddWithValue("edugrad", data.EduGrad);
    updateCmd.Parameters.AddWithValue("id", crew.id);
    
    updateCmd.ExecuteNonQuery();
    updatedCount++;
    
    Console.WriteLine($"✅ Updated crew: {crew.crewId} - {crew.name}");
}

Console.WriteLine($"\n✅ Updated {updatedCount} crew members with bio-data!\n");

// ============================================
// 5. SEED SAMPLE SERVICE RECORDS
// ============================================
Console.WriteLine("📝 Step 5: Seeding sample service records...\n");

var sampleServiceRecords = new[]
{
    new {
        VesselName = "MV PACIFIC STAR", VesselFlag = "Singapore", VesselType = "BULK",
        Grt = 25000m, Dwt = 35000m, YearBuilt = 2015, TradeArea = "Worldwide",
        EngineType = "MAN B&W", EnginePowerKw = 8500, EngineMaker = "MAN Energy Solutions",
        BoilerType = "Composite Boiler", HasScrubber = true, Ecdis = "JRC JAN-901B",
        RankAtTime = "Third Officer", BoardingDate = new DateTime(2022, 1, 15), DisembarkDate = (DateTime?)new DateTime(2022, 12, 20),
        BoardingPort = "SGSIN", BoardingPortName = "Singapore", DisembarkPort = "VNVUT", DisembarkPortName = "Vung Tau"
    },
    new {
        VesselName = "MV OCEAN GLORY", VesselFlag = "Panama", VesselType = "CONTAINER",
        Grt = 45000m, Dwt = 55000m, YearBuilt = 2018, TradeArea = "Asia-Europe",
        EngineType = "Wartsila RT-flex", EnginePowerKw = 12000, EngineMaker = "Wartsila",
        BoilerType = "Exhaust Gas Boiler", HasScrubber = true, Ecdis = "Furuno FEA-2107",
        RankAtTime = "Second Engineer", BoardingDate = new DateTime(2023, 3, 10), DisembarkDate = (DateTime?)new DateTime(2024, 2, 15),
        BoardingPort = "VNHPH", BoardingPortName = "Hai Phong", DisembarkPort = "NLRTM", DisembarkPortName = "Rotterdam"
    },
    new {
        VesselName = "BEACON REZHA10", VesselFlag = "Panama", VesselType = "BULK",
        Grt = 12875m, Dwt = 20000m, YearBuilt = 2015, TradeArea = "V/V",
        EngineType = "MAN B&W 6S35MC", EnginePowerKw = 3750, EngineMaker = "MAN",
        BoilerType = "", HasScrubber = false, Ecdis = "",
        RankAtTime = "WPR", BoardingDate = new DateTime(2024, 11, 19), DisembarkDate = (DateTime?)null,
        BoardingPort = "VNVUT", BoardingPortName = "Vung Tau", DisembarkPort = "", DisembarkPortName = ""
    }
};

int serviceRecordCount = 0;

for (int i = 0; i < Math.Min(sampleServiceRecords.Length, crewList.Count); i++)
{
    var crew = crewList[i];
    var sr = sampleServiceRecords[i];
    
    // Check if already exists
    var checkCmd = new NpgsqlCommand(@"
        SELECT COUNT(*) FROM public.service_records 
        WHERE crew_member_id = @crewId AND vessel_name = @vesselName;
    ", conn);
    checkCmd.Parameters.AddWithValue("crewId", crew.id);
    checkCmd.Parameters.AddWithValue("vesselName", sr.VesselName);
    
    var exists = (long)checkCmd.ExecuteScalar()! > 0;
    
    if (!exists)
    {
        var insertCmd = new NpgsqlCommand(@"
            INSERT INTO public.service_records (
                id, crew_member_id, vessel_name, vessel_flag, vessel_type,
                vessel_grt, vessel_dwt, vessel_year_built, trade_area,
                main_engine_type, main_engine_power_kw, main_engine_maker,
                boiler_type, has_exhaust_gas_scrubber, ecdis,
                rank_at_time, boarding_date, disembark_date,
                boarding_port_code, boarding_port_name,
                disembark_port_code, disembark_port_name,
                is_synced, origin_node, created_at, updated_at
            ) VALUES (
                gen_random_uuid(), @crewId, @vesselName, @vesselFlag, @vesselType,
                @grt, @dwt, @yearBuilt, @tradeArea,
                @engineType, @enginePowerKw, @engineMaker,
                @boilerType, @hasScrubber, @ecdis,
                @rankAtTime, @boardingDate, @disembarkDate,
                @boardingPort, @boardingPortName,
                @disembarkPort, @disembarkPortName,
                false, 'SHIP_01', NOW(), NOW()
            );
        ", conn);
        
        insertCmd.Parameters.AddWithValue("crewId", crew.id);
        insertCmd.Parameters.AddWithValue("vesselName", sr.VesselName);
        insertCmd.Parameters.AddWithValue("vesselFlag", sr.VesselFlag);
        insertCmd.Parameters.AddWithValue("vesselType", sr.VesselType);
        insertCmd.Parameters.AddWithValue("grt", sr.Grt);
        insertCmd.Parameters.AddWithValue("dwt", sr.Dwt);
        insertCmd.Parameters.AddWithValue("yearBuilt", sr.YearBuilt);
        insertCmd.Parameters.AddWithValue("tradeArea", sr.TradeArea);
        insertCmd.Parameters.AddWithValue("engineType", string.IsNullOrEmpty(sr.EngineType) ? DBNull.Value : sr.EngineType);
        insertCmd.Parameters.AddWithValue("enginePowerKw", sr.EnginePowerKw);
        insertCmd.Parameters.AddWithValue("engineMaker", string.IsNullOrEmpty(sr.EngineMaker) ? DBNull.Value : sr.EngineMaker);
        insertCmd.Parameters.AddWithValue("boilerType", string.IsNullOrEmpty(sr.BoilerType) ? DBNull.Value : sr.BoilerType);
        insertCmd.Parameters.AddWithValue("hasScrubber", sr.HasScrubber);
        insertCmd.Parameters.AddWithValue("ecdis", string.IsNullOrEmpty(sr.Ecdis) ? DBNull.Value : sr.Ecdis);
        insertCmd.Parameters.AddWithValue("rankAtTime", sr.RankAtTime);
        insertCmd.Parameters.AddWithValue("boardingDate", sr.BoardingDate);
        insertCmd.Parameters.AddWithValue("disembarkDate", (object?)sr.DisembarkDate ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("boardingPort", string.IsNullOrEmpty(sr.BoardingPort) ? DBNull.Value : sr.BoardingPort);
        insertCmd.Parameters.AddWithValue("boardingPortName", string.IsNullOrEmpty(sr.BoardingPortName) ? DBNull.Value : sr.BoardingPortName);
        insertCmd.Parameters.AddWithValue("disembarkPort", string.IsNullOrEmpty(sr.DisembarkPort) ? DBNull.Value : sr.DisembarkPort);
        insertCmd.Parameters.AddWithValue("disembarkPortName", string.IsNullOrEmpty(sr.DisembarkPortName) ? DBNull.Value : sr.DisembarkPortName);
        
        insertCmd.ExecuteNonQuery();
        serviceRecordCount++;
        
        Console.WriteLine($"✅ Added service record: {crew.name} on {sr.VesselName}");
    }
    else
    {
        Console.WriteLine($"⏭️  Skipped (exists): {crew.name} on {sr.VesselName}");
    }
}

Console.WriteLine($"\n✅ Added {serviceRecordCount} service records!\n");

// ============================================
// 6. VERIFY FINAL DATA
// ============================================
Console.WriteLine("📋 Step 6: Verifying final data...\n");

var verifyCmd = new NpgsqlCommand(@"
    SELECT c.crew_id, c.full_name, c.place_of_birth, c.height, c.blood_group,
           c.education_institution, c.next_of_kin_name,
           COUNT(sr.id) as service_record_count
    FROM public.crew_members c
    LEFT JOIN public.service_records sr ON sr.crew_member_id = c.id
    GROUP BY c.id, c.crew_id, c.full_name, c.place_of_birth, c.height, 
             c.blood_group, c.education_institution, c.next_of_kin_name
    LIMIT 5;
", conn);

Console.WriteLine("Final crew data with service records:\n");
using (var reader = verifyCmd.ExecuteReader())
{
    while (reader.Read())
    {
        var crewId = reader.GetString(0);
        var name = reader.GetString(1);
        var pob = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
        var height = reader.IsDBNull(3) ? "N/A" : reader.GetInt32(3).ToString() + " cm";
        var blood = reader.IsDBNull(4) ? "N/A" : reader.GetString(4);
        var edu = reader.IsDBNull(5) ? "N/A" : reader.GetString(5);
        var kin = reader.IsDBNull(6) ? "N/A" : reader.GetString(6);
        var srCount = reader.GetInt64(7);
        
        Console.WriteLine($"  👤 {crewId} - {name}");
        Console.WriteLine($"     Place of Birth: {pob}");
        Console.WriteLine($"     Height: {height} | Blood: {blood}");
        Console.WriteLine($"     Education: {edu}");
        Console.WriteLine($"     Next of Kin: {kin}");
        Console.WriteLine($"     Service Records: {srCount}");
        Console.WriteLine();
    }
}

Console.WriteLine("✅ =========================");
Console.WriteLine("✅ BIO-DATA SEEDING COMPLETE!");
Console.WriteLine("✅ =========================\n");

}
finally
{
    conn?.Close();
    conn?.Dispose();
}

return 0;
