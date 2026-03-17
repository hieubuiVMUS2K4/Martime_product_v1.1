# VMS Phase 1 Implementation

## Scope

Phase 1 standardizes the voyage domain so the current Edge voyage module can evolve into a business-grade VMS without breaking the existing operational workflow.

This phase does not yet implement voyage economics, estimate calculation, or a full planning cockpit. It adds the backend lifecycle and planning foundation that later phases can build on.

## Lifecycle

The voyage lifecycle is now:

- `PLANNING`
- `APPROVED`
- `READY`
- `UNDERWAY`
- `ARRIVED`
- `COMPLETED`
- `CANCELLED`

Transition rules:

- `PLANNING -> APPROVED, CANCELLED`
- `APPROVED -> READY, PLANNING, CANCELLED`
- `READY -> UNDERWAY, PLANNING, CANCELLED`
- `UNDERWAY -> ARRIVED, CANCELLED`
- `ARRIVED -> COMPLETED, CANCELLED`
- `CANCELLED -> PLANNING`

Editing rules:

- Full edit: `PLANNING`, `APPROVED`, `READY`
- Limited edit: `UNDERWAY`, `ARRIVED`
- Read only: `COMPLETED`, `CANCELLED`

## Data Foundation

`voyage_records` now carries Phase 1 planning fields:

- `charter_type`
- `planned_distance`
- `planned_duration_hours`
- `planned_average_speed`
- `planned_fuel_consumption`
- `voyage_instructions`
- lifecycle milestone timestamps (`approved_at`, `ready_at`, `commenced_at`, `arrived_at`, `completed_at`, `cancelled_at`)

New tables:

- `voyage_plan_legs`: ordered route/planning legs for passage, port, bunker, and crew-change intent
- `voyage_status_history`: auditable lifecycle transition log

## API Impact

Voyage detail now includes:

- planning summary fields
- `planLegs`
- `statusHistory`

Create/update voyage DTOs now accept:

- planning summary fields
- optional `planLegs`
- expanded lifecycle statuses

## Migration

Edge migration generated for this phase:

- `edge_product/edge-services/Data/Migrations/20260311130142_Phase1VoyageLifecycleFoundation.cs`

Apply it with the normal Edge database migration flow before testing status transitions or planning legs.

## Next Phase Dependencies

Phase 2 can now build on this foundation to add:

- route planning UI
- estimated voyage efficiency by charter type
- bunker and crew-change planning workflows
- plan vs actual tracking per voyage leg