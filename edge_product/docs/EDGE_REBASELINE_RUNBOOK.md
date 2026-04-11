# Edge EF Rebaseline Runbook

This runbook resets migration lineage while keeping the current database schema as source-of-truth.

## Preconditions
- Current production dump schema is accepted as canonical.
- A full DB backup exists.
- You have a dedicated git branch for rebaseline.

## Steps
1. Preview the rebaseline actions:
   - `./scripts/rebaseline-edge-migrations.ps1`
2. Execute rebaseline locally:
   - `./scripts/rebaseline-edge-migrations.ps1 -Execute -BaselineName BaselineSchema`
3. Review git diff and confirm:
   - Old migration files are archived.
   - A single new baseline migration + snapshot is present in Data/Migrations.
4. Build and deploy application package as normal.
5. On existing DB, apply generated stamp SQL:
   - `artifacts/tmp-build/edge-baseline-history-stamp.sql`
6. Keep `EDGE_DATABASE_AUTO_MIGRATE=false` in normal operation.
7. For later schema changes, create incremental migrations only.

## Notes
- Do not run `dotnet ef database update` against populated production DB during the rebaseline rollout.
- The baseline migration represents schema at rebaseline time. Future migrations should only include new deltas.
