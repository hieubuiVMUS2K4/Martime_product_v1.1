EDGE Production Deployment
==========================

This folder is intended to be transferred to the server as-is.

Quick start on server
---------------------
1. Upload the whole production directory to server.
2. Run:
   - cd production
   - docker compose up -d --build

Database bootstrap policy (same direction as shore)
---------------------------------------------------
- Production uses dump-first bootstrap from init-scripts, not full EF migration replay.
- Production compose does not override Database__AutoMigrate (same behavior as shore).
- Note: init-scripts run only when postgres volume is empty.

If you need to re-initialize DB from dump
-----------------------------------------
1. Stop stack and remove volumes:
  - docker compose down -v
2. Start again:
  - docker compose up -d --build

If DB volume already has correct data
-------------------------------------
- Do NOT run down -v.
- Use normal restart/update only:
  - docker compose up -d --build

Optional tools
--------------
- Enable pgAdmin profile only when needed:
  - docker compose --profile tools up -d pgadmin

What must exist before transfer
-------------------------------
- backend/MaritimeEdgeServer.dll
- frontend/dist/index.html
- .env

How to generate this package locally
------------------------------------
From edge_product root:
- .\build-production.ps1

The script will:
- publish backend into production/backend
- build frontend into production/frontend/dist
- copy init scripts into production/init-scripts
- copy edge-services/.env into production/.env
- create zip in artifacts/production
