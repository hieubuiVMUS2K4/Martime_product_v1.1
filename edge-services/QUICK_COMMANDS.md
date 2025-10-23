# 🔧 Quick Commands - Maritime Edge Database

## 📦 Backup & Restore

### **Quick Backup (Recommended):**
```powershell
# Create backup directory
mkdir -Force backups

# Backup database to SQL file
docker exec maritime-edge-postgres pg_dump -U edge_user maritime_edge > "backups\maritime_edge_$(Get-Date -Format yyyyMMdd_HHmmss).sql"

# ✅ Output: backups\maritime_edge_20251019_190536.sql (66 KB)
```

### **Quick Restore:**
```powershell
# Restore from latest backup
$latestBackup = Get-ChildItem backups -Filter "*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $latestBackup.FullName | docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge
```

### **Compressed Backup (Save Space):**
```powershell
# Backup with gzip compression (70-90% smaller)
docker exec maritime-edge-postgres pg_dump -U edge_user maritime_edge | gzip > "backups\maritime_edge_$(Get-Date -Format yyyyMMdd_HHmmss).sql.gz"

# Restore from compressed backup
gunzip -c backups\maritime_edge_20251019_190536.sql.gz | docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge
```

---

## 🐳 Docker Commands

### **Start/Stop Containers:**
```powershell
# Start all services
docker-compose up -d

# Stop all services (KEEPS DATA ✅)
docker-compose down

# Stop all services (DELETES DATA ❌ DANGEROUS!)
docker-compose down -v

# Restart single service
docker-compose restart edge-postgres

# View logs
docker-compose logs -f edge-postgres
```

### **Rebuild After Code Changes:**
```powershell
# Rebuild and restart (KEEPS DATA ✅)
docker-compose up -d --build

# Force recreate containers (KEEPS DATA ✅)
docker-compose up -d --force-recreate
```

---

## 🗄️ Database Access

### **psql Interactive Shell:**
```powershell
# Connect to database
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge

# Once inside psql:
\dt                    # List all tables
\d position_data       # Describe table structure
\d+ position_data      # Describe with details
\di                    # List indexes
\q                     # Quit
```

### **Execute Single Query:**
```powershell
# List tables
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"

# Count records
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT 'position_data' as table, COUNT(*) FROM position_data;"

# Get database size
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT pg_size_pretty(pg_database_size('maritime_edge'));"
```

---

## 🔧 EF Core Migrations

### **Create Migration:**
```powershell
cd edge-services
dotnet ef migrations add MigrationName --context EdgeDbContext --output-dir Data/Migrations
```

### **Apply Migration:**
```powershell
# Apply all pending migrations
dotnet ef database update --context EdgeDbContext

# Apply specific migration
dotnet ef database update MigrationName --context EdgeDbContext
```

### **Rollback Migration:**
```powershell
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --context EdgeDbContext

# Remove last migration (if not applied)
dotnet ef migrations remove --context EdgeDbContext --force
```

### **List Migrations:**
```powershell
# List all migrations
dotnet ef migrations list --context EdgeDbContext

# Check migration history in database
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c 'SELECT * FROM "__EFMigrationsHistory";'
```

---

## 🔍 Troubleshooting

### **Container Won't Start:**
```powershell
# Check logs
docker logs maritime-edge-postgres --tail 50

# Check volume mounts
docker inspect maritime-edge-postgres | Select-String "Mounts" -Context 0,15

# Check if port is in use
netstat -ano | findstr :5433
```

### **Cannot Connect to Database:**
```powershell
# Test connection from host
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT 1;"

# Check if port is exposed
docker port maritime-edge-postgres
# Expected: 5432/tcp -> 0.0.0.0:5433
```

### **Data Disappeared:**
```powershell
# Check if volume exists
docker volume ls | Select-String "edge-postgres"

# Check if volume is mounted
docker inspect maritime-edge-postgres --format '{{json .Mounts}}' | ConvertFrom-Json

# Check tables
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"
```

---

## 🧹 Cleanup Commands

### **Remove Old Backups:**
```powershell
# Delete backups older than 30 days
Get-ChildItem backups -Filter "*.sql" | 
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | 
    Remove-Item -Force
```

### **Clean Docker Resources:**
```powershell
# Remove stopped containers
docker container prune -f

# Remove unused images
docker image prune -a -f

# Remove unused networks
docker network prune -f

# Remove unused volumes (⚠️ CAREFUL - check before running!)
docker volume ls | Select-String "edge"  # Check first
docker volume prune -f  # Only if sure!
```

---

## 📊 Health Check Script

```powershell
# Save as: health-check.ps1
Write-Host "🔍 Maritime Edge Database Health Check"
Write-Host "======================================="

# 1. Check container
$running = docker ps --filter "name=maritime-edge-postgres" --format "{{.Names}}"
if ($running) {
    Write-Host "✅ Container: Running"
} else {
    Write-Host "❌ Container: Not running"
    exit 1
}

# 2. Check volume
$volume = docker volume ls --format "{{.Name}}" | Select-String "edge-postgres-data"
if ($volume) {
    Write-Host "✅ Volume: $volume"
} else {
    Write-Host "❌ Volume: Not found"
}

# 3. Check tables
$tables = docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -tAc "SELECT COUNT(*) FROM pg_tables WHERE schemaname='public';" 2>$null
Write-Host "✅ Tables: $tables"

# 4. Check database size
$size = docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -tAc "SELECT pg_size_pretty(pg_database_size('maritime_edge'));" 2>$null
Write-Host "✅ Database size: $size"

# 5. Check connectivity
$test = docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -tAc "SELECT 1;" 2>$null
if ($test -eq "1") {
    Write-Host "✅ Connection: OK"
} else {
    Write-Host "❌ Connection: Failed"
}

# 6. Check last backup
$lastBackup = Get-ChildItem backups -Filter "*.sql*" -ErrorAction SilentlyContinue | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1
if ($lastBackup) {
    $age = [math]::Round(((Get-Date) - $lastBackup.LastWriteTime).TotalHours, 1)
    Write-Host "✅ Last backup: $($lastBackup.Name) ($age hours ago)"
    
    if ($age -gt 24) {
        Write-Host "⚠️  Warning: Backup is older than 24 hours!"
    }
} else {
    Write-Host "⚠️  No backups found"
}

Write-Host "`n🎯 Status: System Healthy"
```

---

## 🚀 Daily Workflow

### **Morning Routine:**
```powershell
# 1. Check system health
docker-compose ps

# 2. View overnight logs
docker-compose logs --tail 100

# 3. Check database size
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT pg_size_pretty(pg_database_size('maritime_edge'));"
```

### **Before Making Changes:**
```powershell
# 1. Backup current state
docker exec maritime-edge-postgres pg_dump -U edge_user maritime_edge > "backups\pre-change-backup.sql"

# 2. Make changes (migrations, code updates, etc.)

# 3. Test changes

# 4. If problems, restore backup
Get-Content backups\pre-change-backup.sql | docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge
```

### **End of Day:**
```powershell
# 1. Create daily backup
docker exec maritime-edge-postgres pg_dump -U edge_user maritime_edge > "backups\maritime_edge_$(Get-Date -Format yyyyMMdd).sql"

# 2. Check logs for errors
docker-compose logs | Select-String "ERROR"

# 3. View resource usage
docker stats --no-stream maritime-edge-postgres
```

---

## 📞 Emergency Commands

### **Database Corrupted:**
```powershell
# 1. Stop container
docker-compose stop edge-postgres

# 2. Restore from latest backup
$latest = Get-ChildItem backups -Filter "*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

# 3. Start container
docker-compose start edge-postgres

# 4. Restore data
Get-Content $latest.FullName | docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge
```

### **Volume Corrupted:**
```powershell
# 1. Stop all services
docker-compose down

# 2. Remove corrupted volume
docker volume rm edge-services_edge-postgres-data

# 3. Create new volume
docker volume create edge-services_edge-postgres-data

# 4. Start PostgreSQL
docker-compose up -d edge-postgres

# 5. Restore from backup
$latest = Get-ChildItem backups -Filter "*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $latest.FullName | docker exec -i maritime-edge-postgres psql -U edge_user maritime_edge
```

---

*Keep these commands handy for daily operations!* 🛠️🚢
