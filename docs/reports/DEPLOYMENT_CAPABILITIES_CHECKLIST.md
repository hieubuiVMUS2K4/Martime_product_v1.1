# 🚀 DEPLOYMENT CAPABILITIES ASSESSMENT
## Edge (Ship) + Shore (Shore) Systems

**Assessment Date:** April 4, 2026  
**Assessment Scope:** Deployment readiness for both subsystems  
**Overall Status:** ✅ **DEPLOYMENT READY** (with caveats noted below)

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Shore System Deployment](#shore-system-deployment)
3. [Edge System Deployment](#edge-system-deployment)
4. [Network & Sync Integration](#network--sync-integration)
5. [Pre-Deployment Checklist](#pre-deployment-checklist)
6. [Deployment Commands Reference](#deployment-commands-reference)
7. [Production Considerations](#production-considerations)

---

## EXECUTIVE SUMMARY

### ✅ What's Ready
- **Infrastructure:** Both systems have complete Docker Compose configurations
- **Architecture:** Dual-system edge-shore pattern fully implemented
- **Sync Protocol:** Store-and-forward with heartbeat, priority queuing, conflict resolution
- **Data Models:** 110+ Shore tables, 60+ Edge tables all synchronized
- **Security:** JWT tokens, internal API keys, configurable HTTPS
- **Network Awareness:** Supports Iridium/VSAT/4G/WiFi with priority-based sync

### ⚠️ What Needs Attention
- **Secrets Management:** Production credentials must be set in `.env` files
- **TLS/Certificates:** HTTPS needs reverse proxy (Nginx) configuration
- **Database Backups:** Backup scripts exist but need scheduling
- **Monitoring:** Health endpoints ready but require integration with monitoring tools
- **Load Testing:** Baseline established (100 concurrent users safe, 150+ approaching saturation)

### 📊 Key Metrics
| Metric | Value | Status |
|--------|-------|--------|
| Shore Backend | ASP.NET Core 8 | ✅ Ready |
| Shore Frontend | React 19 + Vite | ✅ Ready |
| Edge Backend | ASP.NET Core 8 | ✅ Ready |
| Edge Frontend | React 19 + Vite | ✅ Ready |
| Edge Mobile | Flutter 3 | ✅ Ready |
| Shore Database | PostgreSQL 15 | ✅ Ready |
| Edge Database | PostgreSQL 15 | ✅ Ready |
| Docker Compose | v2 | ✅ Ready |
| Containers | 5+ services | ✅ Ready |

---

## SHORE SYSTEM DEPLOYMENT

### 🏗️ Architecture Overview

```
┌─────────────────────────────────────────┐
│    SHORE PRODUCTION (Server IP: 27.71.17.165)
├─────────────────────────────────────────┤
│  Port 80 (HTTP)                         │
│  ├─ Nginx Reverse Proxy                 │
│  │  ├─ Frontend React 19 (80)          │
│  │  └─ Backend API Proxy → 5000         │
│  │                                      │
│  Port 5001 (Dev/Internal)              │
│  └─ Backend API (.NET 8)               │
│     ├─ JWT Auth                         │
│     ├─ 29 Controllers                   │
│     ├─ 35+ Services                     │
│     └─ EF Core + PostgreSQL 15          │
│                                         │
│  Port 5434 (Database - Private)        │
│  └─ PostgreSQL 15                       │
│     ├─ 110+ tables                      │
│     ├─ SyncOutbox (→ Edge)              │
│     ├─ SyncLog (audit trail)            │
│     └─ Crew + Vessel + Voyage data     │
│                                         │
│  Port 8081 (Optional - pgAdmin)        │
│  └─ Database Administration UI          │
└─────────────────────────────────────────┘
```

### 📦 Deployment Files

| File | Purpose | Status |
|------|---------|--------|
| `docker-compose.prod.yml` | Production orchestration | ✅ Ready |
| `build-production.ps1` | PowerShell build script | ✅ Ready |
| `backend/Dockerfile` | Backend container image | ✅ Ready |
| `frontend/Dockerfile` | Frontend + Nginx image | ✅ Ready |
| `.env.production` | Production secrets template | ⚠️ Needs values |
| `init-scripts/` | Database initialization | ✅ Ready |

### 🔧 Service Definitions

#### 1. **PostgreSQL Database**  
```yaml
Service: postgres:15
Container: shore-postgres
Port: 5434 → 5432
Volumes: postgres-data (persistent)
Health: pg_isready check (10s interval)
```

**Status:** ✅ Fully configured + health checking + auto-restart

#### 2. **Backend API (.NET 8)**  
```yaml
Service: backend (.NET 8)
Build: backend/Dockerfile (Release mode)
Port: 5001 → 5000 (internal)
Env: Production (ASPNETCORE_ENVIRONMENT=Production)
Depends: postgres (health check)
Volumes: backend-uploads (persistent)
```

**Configuration:**
```env
ASPNETCORE_ENVIRONMENT: Production
JWT__Key: [REQUIRED - 32+ random chars]
JWT__Issuer: product
JWT__Audience: product-users
Security__EnforceHttps: true (REQUIRES reverse proxy with TLS)
Security__RequireInternalAccess: true
InternalAccess__ApiKey: [REQUIRED - internal proxy key]
DataProtection__EncryptionKey: [REQUIRED - 32+ random chars]
SyncSecurity__RequireSignedRequests: true
SyncSecurity__ProtocolVersion: 2
```

**Status:** ✅ Ready (secrets must be injected)

#### 3. **Frontend + Nginx Reverse Proxy**  
```yaml
Service: frontend (React 19 + Nginx)
Build: frontend/Dockerfile
Port: 80 → 80 (HTTP)
Env: INTERNAL_API_KEY injected for Nginx
Config: Backend proxy to http://backend:5000/api
```

**Features:**
- ✅ Nginx reverse proxy with `/api` rewrite
- ✅ Serves React SPA static files
- ✅ Injects `X-Internal-Api-Key` header → Backend
- ✅ CORS handling

**Status:** ✅ Ready (needs TLS reverse proxy for HTTPS)

#### 4. **pgAdmin (Optional)**  
```yaml
Service: dpage/pgadmin4
Port: 8081 (optional, in tools profile)
```

**Status:** ✅ Ready for admin access (optional profile)

### 📋 Shore Deployment Checklist

```ini
PRE-DEPLOYMENT
[_] Docker Engine 20+ installed
[_] Docker Compose v2 installed
[_] Server has 2GB+ RAM
[_] Ports 80, 5434, 8081 open on firewall

ENVIRONMENT SETUP
[_] Copy .env.production → .env
[_] Set POSTGRES_USER (e.g., "product")
[_] Set POSTGRES_PASSWORD (32+ random chars, not default)
[_] Set JWT_KEY (32+ random chars)
[_] Set INTERNAL_API_KEY (random string for proxy)
[_] Set DATA_PROTECTION__ENCRYPTION_KEY (32+ random chars)
[_] Set PGADMIN_PASSWORD (for database admin)

CONTAINER BUILD & START
[_] Run: docker compose -f docker-compose.prod.yml up -d --build
[_] Verify: docker compose ps (all containers running)
[_] Check: docker compose logs backend (no errors)
[_] Check: docker compose logs frontend (no errors)

VERIFICATION
[_] Test: curl http://localhost/api/health → 200 OK
[_] Test: curl http://localhost/ → React app loads
[_] Test: chrome http://localhost → login page visible
[_] Database: Login to pgAdmin (port 8081) → see data
[_] Sync: Check SyncOutbox table (non-empty)
[_] Health: GET /api/health/ready returns 200

TLS/HTTPS SETUP (OPTIONAL)
[_] Obtain SSL certificate (Let's Encrypt or commercial)
[_] Mount certificate into Nginx container
[_] Update nginx.conf to serve on 443
[_] Redirect 80 → 443 (optional)
[_] Validate HTTPS: curl https://27.71.17.165

MONITORING & BACKUPS
[_] Schedule docker-backup.ps1 (daily)
[_] Setup alerts on container restart
[_] Test database restore procedure
[_] Monitor disk space for postgres-data volume
```

---

## EDGE SYSTEM DEPLOYMENT

### 🚢 Architecture Overview

```
┌──────────────────────────────────────┐
│    EDGE PRODUCTION (Ship Server)
├──────────────────────────────────────┤
│  Port 5001 (API)                     │
│  ├─ Backend API (.NET 8)             │
│  │  ├─ 51 Controllers                │
│  │  ├─ Sync Service                  │
│  │  │  ├─ SyncBackgroundWorker       │
│  │  │  ├─ SyncQueue (store-forward)  │
│  │  │  └─ Telemetry collector        │
│  │  └─ 35+ Domain Services           │
│  │                                   │
│  Port 3000 (Frontend - Served via API) │
│  └─ React 19 Dashboard               │
│     ├─ 55+ routes                    │
│     ├─ Offline-first with Zustand    │
│     └─ Export Excel/PDF              │
│                                      │
│  Port 5433 (Database - Local)        │
│  └─ PostgreSQL 15                    │
│     ├─ 60+ tables                    │
│     ├─ SyncQueue (outgoing)          │
│     ├─ Telemetry data                │
│     └─ Voyage/Crew/Maintenance       │
│                                      │
│  Serial/USB (Hardware Integration)   │
│  └─ SignalK data collector           │
│     ├─ GPS, Engine, Navigation       │
│     └─ Real-time telemetry           │
│                                      │
│  Optional: Mobile App (Flutter)      │
│  └─ Tasks, Alarms, Schedule          │
└──────────────────────────────────────┘
```

### 📦 Deployment Files

| File | Purpose | Status |
|------|---------|--------|
| `edge-services/docker-compose.yml` | Dev orchestration | ✅ Ready |
| `edge-services/Dockerfile` | Container image (if .NET ready) | ⚠️ Partial |
| `edge-services/appsettings.Production.json` | Production config | ✅ Ready |
| `.env` | Edge secrets | ⚠️ Needs values |
| `init-scripts/` | Database initialization | ✅ Ready |

### 🔧 Deployment Options

#### **Option 1: Standalone .NET Runtime (RECOMMENDED FOR PRODUCTION)**

**Deployment Steps:**
```bash
# 1. On ship server (Linux/Windows)
cd edge_product/edge-services

# 2. Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:5001
export Database__ConnectionString="Host=localhost;...password"
export Auth__TokenSigningKey="[32+ random]"
export InternalAccess__ApiKey="[random]"

# 3. Run .NET 8 runtime
dotnet edge-services.dll --urls "http://+:5001"

# 4. Setup PostgreSQL 15 separately (system package)
sudo apt-get install postgresql-15
```

**Advantages:**
- ✅ Lower resource overhead (no Docker daemon)
- ✅ Direct hardware access (serial ports, GPUs)
- ✅ Simpler deployment in restricted environments
- ✅ Can run from USB or portable storage

**Status:** ✅ Ready for production

#### **Option 2: Docker Compose (FOR DEVELOPMENT/TESTING)**

**Deployment Steps:**
```bash
cd edge_product/edge-services
docker compose up -d --build
```

**Services Started:**
1. PostgreSQL 15
2. pgAdmin (optional)
3. EdgeCollector service

**Note:** Requires Docker Engine on ship — may not be ideal for restricted maritime environments.

**Status:** ✅ Ready for development

### 🔧 Service Definitions (Standalone Runtime)

#### **Backend API (.NET 8)**
```
Port: 5001
Protocol: HTTP (use Nginx reverse proxy for HTTPS on ship if needed)
Controllers: 51 (Auth, Voyage, Crew, Logbook, Reports, PMS, etc.)
Services: 35+ (Sync, Telemetry, Maintenance, etc.)
Database: PostgreSQL 15 (port 5433)
```

**Configuration:**
```env
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_URLS: http://+:5001
Database__ConnectionString: Host=localhost;Port=5433;Database=maritime_edge;...
Auth__TokenSigningKey: [REQUIRED - 32+ random]
Security__EnforceHttps: false (or true with reverse proxy)
Security__RequireInternalAccess: false (or true for internal networks)
InternalAccess__ApiKey: [if enforcement enabled]
SyncSecurity__Enabled: false (or true with Shore key exchange)
SyncSecurity__NodeId: [ship IMO number, e.g., "9123456"]
SyncSecurity__SigningKey: [if sync security enabled]
```

**Status:** ✅ Ready

#### **Frontend Dashboard (React 19)**

**Deployment Option A: Standalone (with Node.js)**
```bash
cd edge_product/frontend-edge
npm install
npm run build
npm run preview  # or use nginx to serve dist/

# Or use lightweight HTTP server
npx serve dist/ -l 3000
```

**Deployment Option B: Bundled with Backend**
- Serve `dist/` folder via Backend static file middleware
- Request flow: `http://ship-server:5001/` → Static files
- API requests: `http://ship-server:5001/api/...` → Backend endpoints

**Status:** ✅ Ready for both options

#### **Database - PostgreSQL 15**

**Installation:**
```bash
# Ubuntu/Debian
sudo apt-get install postgresql-15

# Create database
sudo -u postgres psql
create database maritime_edge;
create user edge_user with password 'strong-password';
grant all privileges on database maritime_edge to edge_user;
```

**Port:** 5433 (default 5432, mapped in docker-compose)  
**Status:** ✅ Ready

#### **Mobile App (Flutter)**

**Deployment:**
```bash
cd edge_product/frontend-mobile
flutter pub get
flutter build apk --release          # Android
flutter build ios --release          # iOS
```

**Status:** ✅ Ready (optional)

### 📋 Edge Deployment Checklist

```ini
PRE-DEPLOYMENT (Ship Server)
[_] Linux or Windows with .NET 8 SDK/Runtime
[_] PostgreSQL 15 system package (or Docker)
[_] RAM: 1GB+ (target 2GB recommended)
[_] Storage: 50GB+ (for telemetry data)
[_] Network: Ensure outbound HTTPS to Shore (27.71.17.165)
[_] Serial ports available for SignalK hardware (if physical)

DATABASE SETUP
[_] Install PostgreSQL 15
[_] Create maritime_edge database
[_] Create edge_user with strong password
[_] Run init-scripts to seed schema + data
[_] Verify: psql maritime_edge -c "SELECT COUNT(*) FROM vessel;"

BACKEND CONFIGURATION
[_] Copy edge-services/appsettings.Production.json
[_] Set Auth__TokenSigningKey (32+ random chars)
[_] Set InternalAccess__ApiKey (internal API key)
[_] Set SyncSecurity__NodeId (ship IMO, e.g., "9123456")
[_] Optional: Set SyncSecurity__SigningKey (for signed sync)
[_] Verify: dotnet build -c Release (0 errors)

BACKEND STARTUP
[_] Run: dotnet edge-services.dll --urls "http://+:5001"
[_] Verify: curl http://localhost:5001/api/health → 200 OK
[_] Verify: curl http://localhost:5001/api/health/ready → 200 OK
[_] Check logs: Look for "Application started" message

FRONTEND BUILD & DEPLOY
[_] npm install (or use pre-built dist/)
[_] npm run build (creates dist/ folder)
[_] Configure: Serve dist/ statically or via backend
[_] Verify: http://localhost:5001/ → login page visible

SYNC VERIFICATION
[_] Check: SELECT * FROM sync_queue (should show pending items)
[_] Monitor: SyncBackgroundWorker logs (push/pull every 30s)
[_] Network: curl http://27.71.17.165/api/sync/health (Shore reachable)
[_] Heartbeat: Edge → Shore heartbeat POST every 60s

HARDWARE INTEGRATION (OPTIONAL)
[_] Connect SignalK device via serial port
[_] Verify: Check telemetry data flowing into position_data table
[_] Configure: SignalKDataCollectorService polling interval
[_] Monitor: Real-time vessel position/engine data

OFFLINE OPERATION TEST
[_] Simulate network down: Block traffic to Shore
[_] Verify: SyncQueue accumulates items (store-and-forward)
[_] Verify: Application continues normal operation
[_] Verify: UI remains responsive (offline-first)
[_] Resume: Restore network → SyncQueue drains

SYSTEMD SERVICE (FOR AUTO-RESTART)
[_] Create /etc/systemd/system/edge-collector.service
[_] Enable: systemctl enable edge-collector
[_] Start: systemctl start edge-collector
[_] Verify: systemctl status edge-collector
```

**Example systemd service file:**
```ini
[Unit]
Description=Maritime Edge Collector
After=network.target postgresql.service

[Service]
Type=simple
User=www-data
WorkingDirectory=/opt/edge-services
ExecStart=/usr/bin/dotnet /opt/edge-services/edge-services.dll
Restart=always
RestartSec=10
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://+:5001"

[Install]
WantedBy=multi-user.target
```

---

## NETWORK & SYNC INTEGRATION

### 🌐 Connectivity Architecture

```
                    EDGE (Ship)                     SHORE (Server)
                    ═══════════                     ══════════════

  ┌─────────────┐                                ┌──────────────┐
  │   API       │                                │   API        │
  │ :5001       │                                │   :80/443    │
  │  Store-     │    ┌──────────────────┐        │  Sync        │
  │ Forward     ├───►│  Network Layer   │───────►│ Outbox       │
  │  Queue      │    └──────────────────┘        └──────────────┘
  │             │         ▼ ▲                          │
  │ Heartbeat ──┤    HTTP/REST                  ▲─────┘
  │ every 60s   │    Push: 30s
  │             │    Pull: 30s
  │             │    ◄────────────────────────
  └─────────────┘
    Port 5001


  NETWORK TYPES SUPPORTED:
  ┌────────────────┬───────────┬──────────────┬─────────────────┐
  │ Connection     │ Bandwidth │ Priority Lvl │ Sync Behavior   │
  ├────────────────┼───────────┼──────────────┼─────────────────┤
  │ Iridium Sat    │ 0.12-1kbps│ Critical ★★★ │ Critical only   │
  │ VSAT Sat       │ 64k-2Mbps │ Operational  │ Critical +      │
  │ 4G/LTE Mobile  │ 1-100Mbps │ Operational  │ Critical + Ops  │
  │ Shore WiFi     │ 10+ Mbps  │ Low ★        │ All priority    │
  │ Offline        │ 0         │ N/A          │ Queue stored    │
  └────────────────┴───────────┴──────────────┴─────────────────┘

  PRIORITIES:
  🔴 Critical (Safety):  Alarms, Distress signals → Always send
  🟡 Operational:        Reports, Crew, Positions → Smart queue
  🟢 Low:                Logbooks, Inventory → Queue in good conditions
```

### 📊 Sync Flow Diagram

```
PUSH (Edge → Shore): POST /api/sync
───────────────────────────────────
1. App changes data → EF Core change tracker
2. SyncableEntity auto-enqueue to SyncQueue
3. SyncBackgroundWorker batches (30s interval)
4. Filter by network type (Iridium=Critical only)
5. POST to /api/sync with batch
6. Shore SyncInboxService receives
7. ConflictResolverService applies rules
8. Save to database + SyncLog
9. Return: { success, itemsProcessed, errors }
10. Edge marks SyncedAt on successful items

PULL (Shore → Edge): GET /api/sync/pull
────────────────────────────────────────
1. Edge polls GET /api/sync/pull?since={lastSync}&cursor={id}
2. Shore queries SyncOutbox for this node
3. Return: { items, nextCursor, hasMore }
4. Edge applies via SyncConflictHandler
5. Edge sends POST /api/sync/acknowledge
6. Shore marks DeliveredAt in SyncOutbox
7. Clean old acknowledged items (retention policy)
```

### 🔐 Security Model

**Authentication:**
```
SHORE:
  JWT Token (Bearer)
  Roles: Admin, HRAdmin, CrewCoordinator, FleetManager, etc.
  Header: Authorization: Bearer {jwt_token}

EDGE:
  JWT Token (Bearer) for UI access
  Internal API Key for backend services
  Header: X-Internal-Api-Key: {key} (for internal calls)
  Header: Authorization: Bearer {jwt_token} (for user auth)
```

**Sync Security (Optional v2):**
```
POST /api/sync {
  "originalSignature": "HMAC-SHA256(payload, sharedKey)",
  "payloadVersion": 2,
  "data": {...}
}

Shore validates HMAC before processing.
Prevents man-in-the-middle replay attacks.
```

**Status:** ✅ Ready (signatures optional per environment)

---

## PRE-DEPLOYMENT CHECKLIST

### ✅ Infrastructure

```ini
BOTH SYSTEMS
[_] Docker Engine 20+ installed (if using containers)
[_] Docker Compose v2 installed
[_] .NET 8 SDK/Runtime available
[_] PostgreSQL 15 installed
[_] Git repository cloned and up-to-date
[_] All appsettings.json files committed (no secrets)
[_] .env files created from .env.example (with real secrets)

SHORE SERVER
[_] IP: 27.71.17.165 (or your dynamic IP)
[_] Ports open: 80 (HTTP), 443 (HTTPS)
[_] 2GB+ RAM available
[_] 50GB+ disk space for postgres-data
[_] Firewall allows outbound HTTPS from Edge nodes

EDGE SHIP SERVER
[_] Network: Outbound HTTPS to Shore IP
[_] RAM: 1GB+ available
[_] Storage: 50GB+ for telemetry
[_] Serial ports: Available if using SignalK hardware
[_] .NET 8 Runtime installed (or Docker)
[_] PostgreSQL 15 available
```

### 🔐 Secrets Setup

```ini
REQUIRED ENVIRONMENT VARIABLES

SHORE (.env):
[_] POSTGRES_USER=product
[_] POSTGRES_PASSWORD={32+ random chars}
[_] POSTGRES_DB=productdb
[_] JWT_KEY={32+ random chars}
[_] INTERNAL_API_KEY={random string}
[_] DATA_PROTECTION__ENCRYPTION_KEY={32+ random chars}
[_] PGADMIN_PASSWORD={strong password}
[_] SECURITY_ENFORCE_HTTPS=true (with reverse proxy)
[_] SECURITY_REQUIRE_INTERNAL_ACCESS=true
[_] SYNC_REQUIRE_SIGNED_REQUESTS=true

EDGE (.env):
[_] EDGE_POSTGRES_PASSWORD={32+ random chars}
[_] EDGE_AUTH_TOKEN_SIGNING_KEY={32+ random chars}
[_] EDGE_INTERNAL_API_KEY={random string}
[_] EDGE_SYNC_NODE_ID={ship IMO, e.g., "9123456"}
[_] EDGE_SYNC_SECURITY_ENABLED=true (if sync signing)
[_] EDGE_SYNC_SIGNING_KEY={shared with Shore}
[_] EDGE_SECURITY_ENFORCE_HTTPS=false (or true with proxy)
```

**Generation Tool:**
```bash
# Generate random 32+ char string
openssl rand -base64 32

# Example
POSTGRES_PASSWORD=abcdef123456/+xyz==
JWT_KEY=ijklmn789012XYZ/+abc==
```

### 📊 Database Verification

```bash
# SHORE: Verify schema created
docker compose exec postgres psql -U product -d productdb -c "\dt" | head -20

# EDGE: Verify schema created
docker compose exec edge-postgres psql -U edge_user -d maritime_edge -c "\dt" | head -20

# Both should show: crew, vessel, voyage, sync_queue, sync_outbox, etc.
```

### 🧪 Health Check Commands

```bash
# SHORE API
curl http://localhost/api/health
# Expected: { "status": "healthy", "timestamp": "2026-04-04T...", "version": "1.0" }

curl http://localhost/api/health/ready
# Expected: { "ready": true, "components": {...} }

# EDGE API
curl http://localhost:5001/api/health
# Expected: { "status": "healthy" }

# SHORE-EDGE Connectivity
curl -H "Authorization: Bearer {token}" http://localhost/api/sync/status
# Expected: { "edgeNodes": [...], "lastSync": "...", "queueSize": N }
```

---

## DEPLOYMENT COMMANDS REFERENCE

### SHORE DEPLOYMENT (Docker Compose)

```bash
# 1. Navigate to shore_product
cd shore_product

# 2. Prepare environment
cp .env.production .env
nano .env  # Edit secrets!

# 3. Build and start all services
docker compose -f docker-compose.prod.yml up -d --build

# 4. Monitor startup
docker compose -f docker-compose.prod.yml logs -f backend

# 5. Verify all healthy
docker compose -f docker-compose.prod.yml ps
# All should show "healthy" or "running"

# 6. Test API
curl http://localhost/api/health

# 7. View database
docker compose -f docker-compose.prod.yml exec postgres \
  psql -U product -d productdb -c "SELECT COUNT(*) as total_tables FROM information_schema.tables WHERE table_schema='public';"

# To stop
docker compose -f docker-compose.prod.yml down

# To view logs
docker compose -f docker-compose.prod.yml logs -f [service: postgres, backend, frontend, pgadmin]

# Database backup
docker compose -f docker-compose.prod.yml exec postgres \
  pg_dump -U product -d productdb > shore_backup_$(date +%s).sql

# Database restore
docker compose -f docker-compose.prod.yml exec -T postgres \
  psql -U product -d productdb < shore_backup_1234567890.sql
```

### EDGE DEPLOYMENT (Standalone .NET)

```bash
# 1. Navigate to edge-services
cd edge_product/edge-services

# 2. Setup database
sudo systemctl start postgresql-15  # or docker run postgres:15
psql -U postgres -c "CREATE DATABASE maritime_edge;"
psql -U postgres -c "CREATE USER edge_user WITH PASSWORD 'password';"
psql -U postgres maritime_edge < init-scripts/01-schema.sql

# 3. Publish release build
dotnet publish -c Release -o ./publish

# 4. Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:5001
export Database__ConnectionString="Host=localhost;Port=5432;Database=maritime_edge;Username=edge_user;Password=password"
export Auth__TokenSigningKey="$(openssl rand -base64 32)"
export InternalAccess__ApiKey="internalkey123"
export SyncSecurity__NodeId="9123456"

# 5. Run backend
cd publish
dotnet edge-services.dll

# 6. Build frontend (separate terminal, or serve via backend)
cd ../frontend-edge
npm install
npm run build
# Serve dist/ via Nginx or backend static file middleware

# 7. Test
curl http://localhost:5001/api/health

# To run as service
sudo systemctl enable edge-collector
sudo systemctl start edge-collector
sudo systemctl status edge-collector
```

### EDGE DEPLOYMENT (Docker Compose)

```bash
# 1. Navigate to edge-services
cd edge_product/edge-services

# 2. Prepare environment
cp .env.example .env
nano .env  # Edit secrets!

# 3. Start services
docker compose up -d --build

# 4. Monitor
docker compose logs -f edge-collector

# 5. Verify
docker compose ps
curl http://localhost:5001/api/health

# Database
docker compose exec edge-postgres psql -U edge_user -d maritime_edge -c "\dt"

# Stop
docker compose down
```

---

## PRODUCTION CONSIDERATIONS

### 🔒 Security Hardening

```ini
BEFORE GOING LIVE:

Authentication
[_] JWT tokens expire (set exp claim: 15 minutes)
[_] Refresh tokens rotate (7 days max)
[_] Password policy enforced (Shore login)
[_] Rate limiting on auth endpoints (10 attempts/min)
[_] HTTPS enforced (redirect 80 → 443)

API Security
[_] CORS policy set (only allow expected origins)
[_] Content Security Policy headers added
[_] X-Frame-Options, X-Content-Type-Options set
[_] SQL injection protection (EF Core parameterized)
[_] Input validation on all endpoints
[_] API key rotation schedule (quarterly)

Database
[_] PostgreSQL password: 16+ chars, random
[_] Database backups encrypted
[_] Backup stored off-server (S3, external drive)
[_] Connection strings never logged
[_] Least privilege: edge_user has only needed rights
[_] Public schema: No access from untrusted networks

Network
[_] TLS 1.3 enforced (no TLS 1.0/1.1)
[_] Certificates: Self-signed OK for internal, CA for public
[_] Certificate pinning on Edge ↔ Shore
[_] Firewall: Block unexpected ports
[_] VPN/SSH tunneling for management access

Deployment
[_] Secrets NOT in git (use .env files)
[_] Docker registries: Private (not Docker Hub)
[_] Container scanning for CVEs
[_] Log aggregation to external service
[_] Audit logging enabled (SyncLog, API activity)
```

### 📈 Performance & Monitoring

```ini
MONITORING

Logs
[_] Centralized logging (ELK, Splunk, or CloudWatch)
[_] Retention: 30 days minimum
[_] Alerts on ERROR, CRITICAL severity
[_] Sync failures logged with retry count

Metrics
[_] Container CPU/RAM usage
[_] Database query performance (pg_stat_statements)
[_] API response time (p50, p95, p99)
[_] Sync queue size (target < 1000 items)
[_] Disk space (alert at 80%)
[_] Network bandwidth (Iridium/VSAT usage)

Health Checks
[_] GET /api/health every 30 seconds
[_] GET /api/health/ready every 60 seconds
[_] Database connectivity test
[_] Sync service heartbeat
[_] Alert if any check fails 3 times

Alarms (Recommended)
[_] CPU > 80% for 5 minutes
[_] Memory > 90% for 5 minutes
[_] Disk > 85% capacity
[_] Database connection pool exhausted
[_] Sync queue > 5000 items
[_] API error rate > 1%
[_] Response time p99 > 5 seconds

SCALING STRATEGY

Horizontal (Add more servers)
[_] Deploy multiple Edge instances (one per ship)
[_] Load balance Shore with HAProxy/Nginx
[_] Database replication for load distribution

Vertical (Bigger server)
[_] Current capacity: ~100 concurrent users (Shore)
[_] Upgrade to: 4GB RAM, 2 CPU cores for 150+ users
[_] Database: Consider read replicas beyond 200 users

Caching Layer (Optional)
[_] Redis for session caching
[_] Varnish for static frontend files
[_] Query result caching for crew master data
```

### 🔄 Backup & Disaster Recovery

```bash
# SHORE: Daily backup (run via cron)
#!/bin/bash
BACKUP_DIR="/backups/shore"
DATE=$(date +%Y%m%d_%H%M%S)
docker compose -f docker-compose.prod.yml exec -T postgres \
  pg_dump -U product -d productdb | gzip > $BACKUP_DIR/shore_$DATE.sql.gz
# Retain last 30 days
find $BACKUP_DIR -mtime +30 -delete

# EDGE: Daily backup
#!/bin/bash
BACKUP_DIR="/backups/edge"
DATE=$(date +%Y%m%d_%H%M%S)
docker compose exec -T edge-postgres \
  pg_dump -U edge_user -d maritime_edge | gzip > $BACKUP_DIR/edge_$DATE.sql.gz
find $BACKUP_DIR -mtime +30 -delete

# Test restore (monthly)
gunzip -c shore_20260401_060000.sql.gz | psql -U product -d productdb_test

# Disaster Recovery: 4-hour RTO
[_] Restore from latest backup (15 min)
[_] Replay sync logs if available (30 min)
[_] Verify data integrity (15 min)
[_] Sync Edge nodes to get current state (30 min)
```

### 🚀 Rollback Strategy

```ini
If deployment fails:

1. IMMEDIATE (< 1 min)
   [_] docker compose -f docker-compose.prod.yml down
   [_] Restore previous docker image tag
   [_] docker compose -f docker-compose.prod.yml up -d

2. DATABASE ROLLBACK (if schema changed)
   [_] Restore from backup: gunzip | psql
   [_] Run reverse migration (if available)
   [_] Manual data correction if needed

3. DATA CONSISTENCY
   [_] Resync Edge nodes to get consistent data
   [_] Manually correct any missed updates
   [_] Verify SyncLog for gaps
   [_] Test affected workflows end-to-end
```

---

## SUMMARY SCORECARD

| Component | Status | Notes |
|-----------|--------|-------|
| **Infrastructure** | ✅ Ready | Docker, .NET SDK, PostgreSQL |
| **Shore Backend** | ✅ Ready | 29 controllers, 35+ services |
| **Shore Frontend** | ✅ Ready | React 19 + Vite optimized |
| **Edge Backend** | ✅ Ready | 51 controllers, 35+ services |
| **Edge Frontend** | ✅ Ready | React 19 + Vite optimized |
| **Edge Mobile** | ✅ Ready | Flutter 3.x |
| **Database Sync** | ✅ Ready | Store-and-forward, conflict resolution |
| **Network Awareness** | ✅ Ready | Iridium/VSAT/4G/WiFi support |
| **Security** | ⚠️ Partial | Needs: TLS certs, secrets injection |
| **Monitoring** | ⚠️ Partial | Needs: Log aggregation, alerting |
| **Backup Schedule** | ⚠️ Partial | Needs: Cron jobs, off-site storage |
| **Load Testing** | ✅ Ready | 100 concurrent users baseline |

---

## 🎯 NEXT STEPS

1. **Choose Deployment Option:**
   - [ ] Shore: Docker Compose (recommended)
   - [ ] Edge: Standalone .NET (recommended) OR Docker Compose

2. **Prepare Secrets:**
   - [ ] Generate strong random passwords
   - [ ] Populate .env files
   - [ ] Store securely (not in Git)

3. **Test in Staging:**
   - [ ] Deploy to test server first
   - [ ] Run full integration test suite
   - [ ] Load test with target concurrent users

4. **Monitor & Iterate:**
   - [ ] Setup logging aggregation
   - [ ] Configure alerts
   - [ ] Document runbooks for operational team

5. **Go Live:**
   - [ ] Deploy to production
   - [ ] Verify all health checks
   - [ ] Have rollback plan ready
   - [ ] Monitor first 24 hours closely

---

**Document Version:** 1.0  
**Last Updated:** 2026-04-04  
**Author:** Maritime Product Team
