# 📋 MIGRATION & SETUP GUIDE FOR TEAMMATES

## ✅ What Has Been Done

### 1. **DbContextDesignTimeFactory Created**
- File: `Data/AppDbContextDesignTimeFactory.cs`
- Purpose: Enables EF Core CLI tools to generate migrations without running the application
- Impact: You can now safely run `dotnet ef` commands during development

### 2. **Migration Files Generated**
- New migration: `20260401110009_AddVoyageDateTimeUtcConverters.cs`
- Designer snapshot: `20260401110009_AddVoyageDateTimeUtcConverters.Designer.cs`
- Status: ✅ Checked in to version control

### 3. **UTC DateTime Converters Configured**
- Location: `Data/AppDbContext.cs` (OnModelCreating method)
- Coverage: All voyage entity datetime fields
- Entities: VoyageRecord, VoyagePlanLeg, VoyageStatusHistory, VoyageCrewAssignment, CargoOperation, VoyageLogEntry, VoyageCargoPlan, Port, PortCall
- Effect: Prevents Npgsql "DateTime.Kind=Local" exceptions at runtime

---

## 🚀 What Teammates Need To Do

### When Pulling Code For The First Time:
```bash
# 1. Install dependencies
dotnet restore

# 2. Build the project
dotnet build

# 3. Apply ALL pending migrations to your database
dotnet ef database update

# Done! ✅
```

### That's It!
No need to create migrations manually. The migration files are already committed to the repository.

---

## 📝 Key Points

| Aspect | Details |
|--------|---------|
| **Connection String** | Read from `appsettings.json` or `appsettings.Development.json` |
| **Default Dev Database** | `maritime_shore_dev` on localhost:5432 |
| **Migration Command** | `dotnet ef database update` (applies all pending migrations) |
| **New Models Added?** | Run: `dotnet ef migrations add "YourMigrationName"` then commit the files |
| **Designer Factory** | Already configured via `AppDbContextDesignTimeFactory.cs` |

---

## ⚠️ Important

### ✅ DO THIS:
1. Keep migration files in version control (they're checked in)
2. Run `dotnet ef database update` when you pull new code
3. If you add new entities or modify DbContext:
   - Generate migration: `dotnet ef migrations add "MigrationName"`
   - Commit BOTH the `.cs` and `.Designer.cs` files
   - Colleagues will auto-apply via `dotnet ef database update`

### ❌ DON'T DO THIS:
1. Don't manually run migrations (use `dotnet ef database update`)
2. Don't skip migration files when checking in code
3. Don't create separate migration scripts - EF handles it

---

## 🔍 Verify Setup

To check if everything is ready without applying migrations:
```bash
# List pending migrations
dotnet ef migrations list

# Check database is up-to-date
dotnet ef migrations has-pending-changes
```

---

## 🆘 Troubleshooting

### **"Migration not found" error**
- Ensure you ran `dotnet restore` first
- Check connection string in `appsettings.json`
- Delete `bin/` and `obj/` folders, rebuild

### **"DateTime.Kind=Local" runtime error**
- Migrations have UTC converters configured
- Run `dotnet ef database update` to apply them
- Restart the application

### **"Cannot connect to database"**
- Ensure PostgreSQL is running locally on port 5432
- Update connection string in `appsettings.Development.json`
- Or create env var: `ConnectionStrings__DefaultConnection=YourConnectionString`

---

## 📦 Deployment

When deploying to production/staging:
```bash
# Apply all pending migrations
dotnet ef database update --environment Production

# Or via container startup hooks (recommended)
# See Startup.cs for automatic migration application
```

---

## 📚 References

- Migration snapshot: `Migrations/AppDbContextModelSnapshot.cs`
- Latest migration: `Migrations/20260401110009_AddVoyageDateTimeUtcConverters.cs`
- DbContext config: `Data/AppDbContext.cs`
- EF Core docs: https://docs.microsoft.com/ef/core/migrations

---

**Last Updated:** 2026-04-01  
**Status:** ✅ All migrations configured for smooth team adoption
