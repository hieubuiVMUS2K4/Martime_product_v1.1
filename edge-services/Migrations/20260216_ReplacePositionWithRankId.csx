#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;
using System;

// Database connection string
var connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

Console.WriteLine("🔄 Starting migration: Replace Position with RankId in crew_members table");
Console.WriteLine("=" .PadRight(70, '='));

try
{
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    Console.WriteLine("✅ Connected to database");

    using var transaction = conn.BeginTransaction();
    
    try
    {
        // Step 1: Check if position column exists
        Console.WriteLine("\n📋 Step 1: Checking existing schema...");
        var checkPositionCmd = new NpgsqlCommand(@"
            SELECT column_name 
            FROM information_schema.columns 
            WHERE table_name = 'crew_members' 
            AND column_name = 'position'
        ", conn, transaction);
        
        var hasPosition = checkPositionCmd.ExecuteScalar() != null;
        Console.WriteLine($"   Position column exists: {hasPosition}");
        
        var checkRankIdCmd = new NpgsqlCommand(@"
            SELECT column_name 
            FROM information_schema.columns 
            WHERE table_name = 'crew_members' 
            AND column_name = 'rank_id'
        ", conn, transaction);
        
        var hasRankId = checkRankIdCmd.ExecuteScalar() != null;
        Console.WriteLine($"   RankId column exists: {hasRankId}");
        
        if (hasRankId)
        {
            Console.WriteLine("⚠️  RankId column already exists. Migration may have been run before.");
            Console.WriteLine("   Skipping migration to avoid data loss.");
            transaction.Rollback();
            return 0;
        }
        
        // Step 2: Add rank_id column (nullable for now)
        if (!hasRankId)
        {
            Console.WriteLine("\n📋 Step 2: Adding rank_id column...");
            var addRankIdCmd = new NpgsqlCommand(@"
                ALTER TABLE crew_members 
                ADD COLUMN rank_id INTEGER NULL
            ", conn, transaction);
            
            addRankIdCmd.ExecuteNonQuery();
            Console.WriteLine("✅ Added rank_id column");
        }
        
        // Step 3: Create mapping from position string to rank_id based on common patterns
        Console.WriteLine("\n📋 Step 3: Mapping existing position data to ranks...");
        
        // Get all crews with their positions
        var getCrewsCmd = new NpgsqlCommand(@"
            SELECT id, position 
            FROM crew_members 
            WHERE position IS NOT NULL AND position != ''
        ", conn, transaction);
        
        var crews = new List<(Guid id, string position)>();
        using (var reader = getCrewsCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                crews.Add((reader.GetGuid(0), reader.GetString(1)));
            }
        }
        
        Console.WriteLine($"   Found {crews.Count} crew members with position data");
        
        // Map positions to rank_ids
        var positionToRankMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Master", 1 },
            { "Captain", 1 },
            { "Chief Officer", 2 },
            { "C/O", 2 },
            { "First Mate", 2 },
            { "Second Officer", 3 },
            { "2/O", 3 },
            { "Third Officer", 4 },
            { "3/O", 4 },
            { "Chief Engineer", 5 },
            { "C/E", 5 },
            { "Second Engineer", 6 },
            { "2/E", 6 },
            { "Bosun", 7 },
            { "Boatswain", 7 },
            { "Able Seaman", 8 },
            { "AB", 8 },
            { "Oiler", 9 },
            { "Chief Cook", 10 }
        };
        
        int updatedCount = 0;
        int unmappedCount = 0;
        
        foreach (var crew in crews)
        {
            int? rankId = null;
            
            // Try exact match first
            if (positionToRankMap.TryGetValue(crew.position, out var exactRankId))
            {
                rankId = exactRankId;
            }
            else
            {
                // Try partial match
                foreach (var kvp in positionToRankMap)
                {
                    if (crew.position.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        rankId = kvp.Value;
                        break;
                    }
                }
            }
            
            if (rankId.HasValue)
            {
                var updateCmd = new NpgsqlCommand(@"
                    UPDATE crew_members 
                    SET rank_id = @rankId 
                    WHERE id = @id
                ", conn, transaction);
                
                updateCmd.Parameters.AddWithValue("rankId", rankId.Value);
                updateCmd.Parameters.AddWithValue("id", crew.id);
                updateCmd.ExecuteNonQuery();
                
                updatedCount++;
            }
            else
            {
                Console.WriteLine($"   ⚠️  Unmapped position: '{crew.position}' for crew {crew.id}");
                unmappedCount++;
            }
        }
        
        Console.WriteLine($"✅ Mapped {updatedCount} crew members to ranks");
        if (unmappedCount > 0)
        {
            Console.WriteLine($"⚠️  {unmappedCount} crew members have unmapped positions (rank_id will be NULL)");
        }
        
        // Step 4: Add foreign key constraint
        Console.WriteLine("\n📋 Step 4: Adding foreign key constraint...");
        var addFkCmd = new NpgsqlCommand(@"
            ALTER TABLE crew_members 
            ADD CONSTRAINT fk_crew_members_rank 
            FOREIGN KEY (rank_id) 
            REFERENCES ranks(id) 
            ON DELETE SET NULL
        ", conn, transaction);
        
        addFkCmd.ExecuteNonQuery();
        Console.WriteLine("✅ Added foreign key constraint");
        
        // Step 5: Create index for better query performance
        Console.WriteLine("\n📋 Step 5: Creating index on rank_id...");
        var createIndexCmd = new NpgsqlCommand(@"
            CREATE INDEX IF NOT EXISTS idx_crew_members_rank_id 
            ON crew_members(rank_id)
        ", conn, transaction);
        
        createIndexCmd.ExecuteNonQuery();
        Console.WriteLine("✅ Created index");
        
        // Step 6: Remove position column
        if (hasPosition)
        {
            Console.WriteLine("\n📋 Step 6: Removing position column...");
            var dropPositionCmd = new NpgsqlCommand(@"
                ALTER TABLE crew_members 
                DROP COLUMN IF EXISTS position
            ", conn, transaction);
            
            dropPositionCmd.ExecuteNonQuery();
            Console.WriteLine("✅ Removed position column");
        }
        
        // Commit transaction
        transaction.Commit();
        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine("✅ Migration completed successfully!");
        Console.WriteLine($"   - Added rank_id column with FK to ranks table");
        Console.WriteLine($"   - Migrated {updatedCount} position values to rank_id");
        Console.WriteLine($"   - Removed position column");
        
        return 0;
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        Console.WriteLine($"\n❌ Migration failed: {ex.Message}");
        Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        throw;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Fatal error: {ex.Message}");
    return 1;
}
