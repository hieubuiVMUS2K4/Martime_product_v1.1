# ⚡ QUICK START - Maritime Management System

## 🚀 5-Minute Setup

### Prerequisites
- Docker Desktop installed and running
- Git installed
- 8GB+ RAM available

### Installation
```bash
# 1. Clone repository
git clone https://github.com/hieubuiVMUS2K4/sampleProduct.git
cd sampleProduct

# 2. Start system (will auto-build)
docker compose up -d

# 3. Wait for healthy status (2-3 minutes)
docker compose ps
```

### Access URLs
| Service | URL | Credentials |
|---------|-----|-------------|
| 🌐 **Frontend Web** | http://localhost:3000 | - |
| 🔧 **Backend API** | http://localhost:5000 | - |
| 📚 **API Docs** | http://localhost:5000/swagger | - |
| 🗃️ **Database Admin** | http://localhost:8081 | `admin@example.com` / `admin` |

### Database Connection (pgAdmin)
```
Host: postgres
Port: 5432
Database: productdb
Username: product
Password: productpwd
```

### Common Commands
```bash
# View logs
docker compose logs backend --follow

# Stop system
docker compose down

# Restart specific service
docker compose restart frontend

# Rebuild after changes
docker compose build --no-cache [service_name]
```

### Troubleshooting
```bash
# If ports are busy
docker compose down
# Change ports in docker-compose.yml if needed

# If build fails
docker compose down --remove-orphans
docker compose build --no-cache

# Reset everything
docker compose down -v
docker system prune -a
```

## 🎯 Next Steps
1. Check out the full [README.md](README.md) for detailed features
2. Read [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md) for development setup
3. Visit http://localhost:3000 to start using the system!

**🚢 Happy Maritime Management!**