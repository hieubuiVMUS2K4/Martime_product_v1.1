# 🔧 TROUBLESHOOTING GUIDE - Maritime Edge Server

## ❌ Vấn Đề 1: pgAdmin không khởi động (Email validation error)

### **Triệu chứng:**
```
'admin@edge.local' does not appear to be a valid email address
```

### **Nguyên nhân:**
pgAdmin 4 không chấp nhận email với domain `.local` (không phải top-level domain hợp lệ)

### **Giải pháp:**
1. Sửa file `.env`:
```env
# Thay đổi từ:
EDGE_PGADMIN_EMAIL=admin@edge.local

# Thành:
EDGE_PGADMIN_EMAIL=admin@maritime.com
```

2. Tạo lại containers:
```powershell
docker-compose down
docker-compose up -d
```

3. Kiểm tra logs:
```powershell
docker logs maritime-edge-pgadmin --tail 20
```

✅ **Kết quả:** Thấy message "pgAdmin 4 - Application Initialisation" mà không có lỗi email

---

## ❌ Vấn Đề 2: Container restart liên tục

### **Triệu chứng:**
```powershell
docker-compose ps
# Shows: Restarting (1) 10 seconds ago
```

### **Giải pháp:**
```powershell
# Xem logs để tìm lỗi
docker logs maritime-edge-postgres
docker logs maritime-edge-pgadmin

# Nếu lỗi về password
# Kiểm tra .env và appsettings.json có khớp nhau không
```

---

## ❌ Vấn Đề 3: Không kết nối được PostgreSQL từ host

### **Triệu chứng:**
```
Connection refused on localhost:5433
```

### **Giải pháp:**
```powershell
# 1. Kiểm tra container có chạy không
docker-compose ps

# 2. Kiểm tra port mapping
docker port maritime-edge-postgres
# Output: 5432/tcp -> 0.0.0.0:5433

# 3. Test connection
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT 1;"

# 4. Kiểm tra firewall Windows
# Control Panel > Windows Defender Firewall > Allow an app
```

---

## ❌ Vấn Đề 4: EF Core Migration fails

### **Triệu chứng:**
```
Unable to create an object of type 'EdgeDbContext'
```

### **Giải pháp:**
```powershell
# 1. Verify connection string trong appsettings.json
# 2. Đảm bảo PostgreSQL container đang chạy
docker-compose ps

# 3. Thử migration với connection string explicit
dotnet ef database update --connection "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!"
```

---

## ❌ Vấn Đề 5: pgAdmin không load database

### **Triệu chứng:**
pgAdmin UI mở được nhưng không thấy database `maritime_edge`

### **Giải pháp:**
1. Login vào pgAdmin: http://localhost:5050
   - Email: `admin@maritime.com`
   - Password: `admin`

2. **Add New Server:**
   - General Tab:
     - Name: `Maritime Edge`
   
   - Connection Tab:
     - **CRITICAL:** Host phải là `edge-postgres` (không phải `localhost`)
     - Port: `5432` (internal port, không phải 5433)
     - Database: `maritime_edge`
     - Username: `edge_user`
     - Password: `ChangeMe_EdgePassword123!`

3. Click **Save**

✅ **Lý do:** pgAdmin chạy trong Docker network, phải dùng service name `edge-postgres` để resolve

---

## ❌ Vấn Đề 6: Database empty sau migration

### **Triệu chứng:**
Migration chạy thành công nhưng `\dt` không thấy tables

### **Giải pháp:**
```powershell
# 1. Kiểm tra migration history
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT * FROM \"__EFMigrationsHistory\";"

# 2. Nếu không có records, chạy lại migration
cd edge-services
dotnet ef database update --context EdgeDbContext

# 3. Verify tables
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"
```

---

## ❌ Vấn Đề 7: Port 5433/5050 already in use

### **Triệu chứng:**
```
Error: Port 5433 is already allocated
```

### **Giải pháp:**
```powershell
# 1. Tìm process sử dụng port
netstat -ano | findstr :5433
netstat -ano | findstr :5050

# 2. Kill process (replace <PID>)
taskkill /PID <PID> /F

# Hoặc thay đổi port trong .env
EDGE_POSTGRES_PORT=5434
EDGE_PGADMIN_PORT=5051

# Sau đó recreate containers
docker-compose down
docker-compose up -d
```

---

## ❌ Vấn Đề 8: Docker Compose version warning

### **Triệu chứng:**
```
level=warning msg="the attribute `version` is obsolete"
```

### **Giải pháp:**
Xóa dòng `version: '3.8'` ở đầu file `docker-compose.yml` (warning không ảnh hưởng chức năng)

---

## ❌ Vấn Đề 9: Data mất sau restart

### **Triệu chứng:**
Insert data vào database, sau khi `docker-compose down` thì mất hết

### **Giải pháp:**
```powershell
# Kiểm tra volumes
docker volume ls

# Phải thấy:
# edge-services_postgres-data

# Nếu không có, edit docker-compose.yml:
volumes:
  postgres-data:  # Phải có section này
  
services:
  edge-postgres:
    volumes:
      - postgres-data:/var/lib/postgresql/data  # Named volume
```

---

## ❌ Vấn Đề 10: snake_case vs PascalCase mismatch

### **Triệu chứng:**
```
Column "IsSynced" does not exist
```

### **Giải pháp:**
Đảm bảo `EdgeDbContext.OnModelCreating()` có implement `ToSnakeCase()` method:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply snake_case naming convention
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        // Convert table names
        entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
        
        // Convert column names
        foreach (var property in entity.GetProperties())
        {
            property.SetColumnName(property.Name.ToSnakeCase());
        }
        
        // Convert primary keys
        foreach (var key in entity.GetKeys())
        {
            key.SetName(key.GetName()?.ToSnakeCase());
        }
        
        // Convert foreign keys
        foreach (var fk in entity.GetForeignKeys())
        {
            fk.SetConstraintName(fk.GetConstraintName()?.ToSnakeCase());
        }
        
        // Convert indexes
        foreach (var index in entity.GetIndexes())
        {
            index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
        }
    }
}
```

---

## 🔍 Useful Commands

### Check Container Health:
```powershell
docker-compose ps
docker stats maritime-edge-postgres
docker stats maritime-edge-pgadmin
```

### View Logs:
```powershell
# Real-time logs
docker-compose logs -f

# Last 50 lines
docker logs maritime-edge-postgres --tail 50
docker logs maritime-edge-pgadmin --tail 50
```

### Database Access:
```powershell
# psql interactive shell
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge

# Single query
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT COUNT(*) FROM position_data;"
```

### Clean Rebuild:
```powershell
# Stop and remove everything
docker-compose down -v  # WARNING: -v removes volumes (data loss!)

# Rebuild from scratch
docker-compose up -d --build

# Reapply migrations
cd edge-services
dotnet ef database update
```

---

## 📞 Support Checklist

Khi gặp lỗi, thu thập các thông tin sau:

1. ✅ Container status: `docker-compose ps`
2. ✅ Container logs: `docker logs <container_name> --tail 100`
3. ✅ Environment variables: `docker exec <container> env | grep EDGE`
4. ✅ Network connectivity: `docker network inspect edge-services_edge-network`
5. ✅ Database schema: `docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"`
6. ✅ Migration history: Check `Data/Migrations/*.cs` files

---

*Last Updated: October 19, 2025* 🚢🔧
