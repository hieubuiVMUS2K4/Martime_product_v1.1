# Maritime Edge - Teammate Setup Guide

## Database Setup for New Teammates

### Option 1: Fresh Database (Recommended for New Setup)

**Step 1: Start Docker containers**
```bash
cd edge-services
docker-compose up -d
```

**Step 2: Initialize database schema**
Docker will automatically run `init-scripts/01-init-schema.sql` on first startup.

**Step 3: Verify EF Migrations**
```bash
dotnet ef migrations list
# Should show: 20251207094407_InitialCreate (Applied)
```

### Option 2: Restore from Backup

If you need production data:

```bash
# 1. Start database
docker-compose up -d

# 2. Restore backup
docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge < backup_full_20251207_160427.sql
```

## Working with EF Migrations

### For Existing Database (First Time)

Your database is already created via SQL scripts. EF Migrations is marked as "applied" but schema exists.

**Verify status:**
```bash
dotnet ef migrations list
# Expected: 20251207094407_InitialCreate (Applied)
```

### Adding New Schema Changes

**Step 1: Modify models**
```csharp
// Example: Add column to MaintenanceSchedule
public class MaintenanceSchedule
{
    // ... existing properties ...
    
    [MaxLength(500)]
    public string? MaintenanceNotes { get; set; }  // NEW
}
```

**Step 2: Generate migration**
```bash
dotnet ef migrations add AddMaintenanceNotes --output-dir Data/Migrations
```

**Step 3: Review generated SQL**
Check `Data/Migrations/20251207XXXXXX_AddMaintenanceNotes.cs` to verify SQL is correct.

**Step 4: Apply migration**
```bash
dotnet ef database update
```

**Step 5: Commit to Git**
```bash
git add Data/Migrations/
git commit -m "feat: Add MaintenanceNotes column to MaintenanceSchedule"
git push
```

### Teammate Updates Code

When teammate pulls code with new migrations:

```bash
# 1. Pull latest code
git pull origin feature/hieu

# 2. Apply pending migrations
cd edge-services
dotnet ef database update

# 3. Verify
dotnet ef migrations list
# All migrations should show (Applied)
```

## Important Notes

### Initial Setup Strategy

This project uses a **hybrid approach**:

1. **Initial Schema**: Created via SQL script (`init-scripts/01-init-schema.sql`)
   - Docker auto-runs on first container startup
   - Creates all tables with proper structure
   - Marks EF migration as "applied" in `__efmigrationshistory`

2. **Future Changes**: Use EF Migrations
   - All schema changes after initial setup use `dotnet ef migrations add`
   - Teammates apply via `dotnet ef database update`
   - Standard .NET workflow for team collaboration

### Why This Approach?

**Problem**: Database was initially created via SQL scripts, not EF. EF Migrations cannot cleanly retrofit existing databases.

**Solution**:
- ✅ SQL script for baseline (one-time setup)
- ✅ EF Migrations for incremental changes (ongoing)
- ✅ Teammates get consistent experience (`dotnet ef database update`)

### Troubleshooting

**Migration shows "Pending" but schema exists:**
```bash
# Manually mark as applied (one-time fix)
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c \
  "INSERT INTO __efmigrationshistory (migration_id, product_version) VALUES ('20251207094407_InitialCreate', '9.0.10');"
```

**EF tries to CREATE existing tables:**
- Check `__efmigrationshistory` table
- Verify migration ID matches file name
- Run `dotnet ef migrations list` to confirm status

**Database connection issues:**
```bash
# Check container is running
docker ps | grep maritime-edge-postgres

# Test connection
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT version();"
```

## Common Workflows

### Add New Table

```csharp
// 1. Add model in Models/EdgeModels.cs
public class NewEntity
{
    [Key]
    public Guid Id { get; set; }
    // ... properties ...
}

// 2. Add DbSet in Data/EdgeDbContext.cs
public DbSet<NewEntity> NewEntities { get; set; }

// 3. Generate migration
dotnet ef migrations add AddNewEntityTable --output-dir Data/Migrations

// 4. Apply
dotnet ef database update
```

### Modify Existing Column

```csharp
// 1. Change property in model
[MaxLength(500)]  // Changed from 200
public string Name { get; set; }

// 2. Generate migration
dotnet ef migrations add IncreaseNameLength --output-dir Data/Migrations

// 3. Review SQL (should be ALTER COLUMN)
// 4. Apply
dotnet ef database update
```

### Remove Column

```csharp
// 1. Remove property from model (or add [NotMapped])
// [NotMapped]  // To keep property in code but remove from DB
// public string OldProperty { get; set; }

// 2. Generate migration
dotnet ef migrations add RemoveOldProperty --output-dir Data/Migrations

// 3. Review SQL (should be DROP COLUMN)
// 4. Apply
dotnet ef database update
```

## Database Connection Strings

**Edge Service (Local Docker):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=edge_pass_123"
}
```

**Shore API (Remote):**
```json
"ConnectionStrings": {
  "ShoreConnection": "Host=your-shore-db.com;Port=5432;Database=shore_db;Username=shore_user;Password=shore_pass"
}
```

## Quick Reference

```bash
# List all migrations
dotnet ef migrations list

# Add new migration
dotnet ef migrations add MigrationName --output-dir Data/Migrations

# Apply migrations
dotnet ef database update

# Remove last migration (only if not applied)
dotnet ef migrations remove

# Generate SQL script (for manual review)
dotnet ef migrations script

# Revert to specific migration
dotnet ef database update MigrationName
```

## Support

For issues with database setup, check:
1. `docker ps` - Ensure containers running
2. `dotnet ef migrations list` - Check migration status
3. `Data/Migrations/` - Review generated SQL
4. `__efmigrationshistory` table in database
