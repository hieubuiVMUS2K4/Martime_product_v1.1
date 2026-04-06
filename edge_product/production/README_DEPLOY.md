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
- Production uses dump-first bootstrap from init-scripts.
- `00-edge-dump.sql` is treated as the migration baseline (contains aligned `__EFMigrationsHistory`).
- Production compose controls `Database__AutoMigrate` via env toggle.
- Default template is ON for convenience deploys after baseline alignment.
- Note: init-scripts run only when postgres volume is empty.

Schema change rollout (recommended for production)
--------------------------------------------------
- Add new EF migration in source code for each schema change.
- Build artifacts, transfer production folder, then run `docker compose up -d --build`.
- With auto-migrate ON and aligned baseline, new migrations are applied on startup.
- For high-risk schema changes, prefer manual SQL rollout (backup -> apply -> verify -> deploy).

Baseline alignment check (before enabling auto-migrate)
-------------------------------------------------------
- From `edge_product` run:
  - `./scripts/check-ef-baseline.ps1`
- This compares source migrations in `edge-services/Data/Migrations` with dump history in `production/init-scripts/00-edge-dump.sql`.
- It generates an idempotent history-stamp SQL file at:
  - `artifacts/tmp-build/edge-migration-baseline-fix.sql`
- Apply that SQL on the target DB if there are missing migration IDs, then re-check and only then consider temporary auto-migrate enablement.

One-time rebaseline workflow (when dump schema is already canonical)
--------------------------------------------------------------------
- Use this only when you intentionally reset migration lineage and treat current schema as baseline.
- From `edge_product` run preview first:
  - `./scripts/rebaseline-edge-migrations.ps1`
- Execute for real only after backup + branch commit:
  - `./scripts/rebaseline-edge-migrations.ps1 -Execute -BaselineName BaselineSchema`
- Script actions:
  - Archives old files from `edge-services/Data/Migrations`.
  - Creates a new baseline migration in `edge-services/Data/Migrations`.
  - Generates baseline history stamp SQL in `artifacts/tmp-build/edge-baseline-history-stamp.sql`.
- On existing DB, apply only the generated history stamp SQL.
- Do NOT run `dotnet ef database update` directly against populated production DB during rebaseline.

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
