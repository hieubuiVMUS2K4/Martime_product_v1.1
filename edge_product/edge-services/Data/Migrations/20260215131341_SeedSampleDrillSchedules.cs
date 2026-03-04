using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleDrillSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create sample drill schedules for 2026 timeline
            // These will appear as bars in the Gantt chart
            
            // February 2026: Abandon ship drill (completed)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202602-ABANDON_SHIP-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    2, 2026,
                    '2026-02-01'::timestamp with time zone,
                    '2026-02-28'::timestamp with time zone,
                    '2026-03-07'::timestamp with time zone,
                    'COMPLETED',
                    '✓',
                    1,
                    false,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'ABANDON_SHIP_MONTHLY' LIMIT 1;
            ");
            
            // March 2026: Fire drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202603-FIRE_DRILL-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    3, 2026,
                    '2026-03-01'::timestamp with time zone,
                    '2026-03-31'::timestamp with time zone,
                    '2026-04-07'::timestamp with time zone,
                    'SCHEDULED',
                    '1 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'FIRE_DRILL_MONTHLY' LIMIT 1;
            ");
            
            // April 2026: Rescue boat drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202604-RESCUE_BOAT-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    4, 2026,
                    '2026-04-01'::timestamp with time zone,
                    '2026-04-30'::timestamp with time zone,
                    '2026-05-07'::timestamp with time zone,
                    'SCHEDULED',
                    '2 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'RESCUE_BOAT_QUARTERLY' LIMIT 1;
            ");
            
            // May 2026: Abandon ship drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202605-ABANDON_SHIP-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    5, 2026,
                    '2026-05-01'::timestamp with time zone,
                    '2026-05-31'::timestamp with time zone,
                    '2026-06-07'::timestamp with time zone,
                    'SCHEDULED',
                    '3 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'ABANDON_SHIP_MONTHLY' LIMIT 1;
            ");
            
            // June 2026: Security drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202606-SECURITY-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    6, 2026,
                    '2026-06-01'::timestamp with time zone,
                    '2026-06-30'::timestamp with time zone,
                    '2026-07-07'::timestamp with time zone,
                    'SCHEDULED',
                    '4 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'SECURITY_DRILL_QUARTERLY' LIMIT 1;
            ");
            
            // July 2026: Fire drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202607-FIRE_DRILL-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    7, 2026,
                    '2026-07-01'::timestamp with time zone,
                    '2026-07-31'::timestamp with time zone,
                    '2026-08-07'::timestamp with time zone,
                    'SCHEDULED',
                    '5 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'FIRE_DRILL_MONTHLY' LIMIT 1;
            ");
            
            // August 2026: Enclosed space entry (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202608-ENCLOSED_SPACE-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    8, 2026,
                    '2026-08-01'::timestamp with time zone,
                    '2026-08-31'::timestamp with time zone,
                    '2026-09-07'::timestamp with time zone,
                    'SCHEDULED',
                    '6 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'ENCLOSED_SPACE_BIMONTHLY' LIMIT 1;
            ");
            
            // September 2026: Emergency response drill (scheduled)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202609-EMERGENCY-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    9, 2026,
                    '2026-09-01'::timestamp with time zone,
                    '2026-09-30'::timestamp with time zone,
                    '2026-10-07'::timestamp with time zone,
                    'SCHEDULED',
                    '7 m',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'EMERGENCY_RESPONSE_ANNUAL' LIMIT 1;
            ");
            
            // January 2026: Overdue drill (to show RED bar)
            migrationBuilder.Sql(@"
                INSERT INTO drill_schedules (id, schedule_code, drill_type_id, scheduled_month, scheduled_year, 
                    start_date, due_date, overdue_date, status, timeline_label, execution_count, is_auto_generated,
                    origin_node, is_synced, created_at, updated_at)
                SELECT 
                    gen_random_uuid(),
                    'DS-202601-FIRE_DRILL-' || substring(gen_random_uuid()::text, 1, 8),
                    id,
                    1, 2026,
                    '2026-01-01'::timestamp with time zone,
                    '2026-01-31'::timestamp with time zone,
                    '2026-02-07'::timestamp with time zone,
                    'OVERDUE',
                    '-2 w',
                    0,
                    true,
                    'EDGE',
                    false,
                    NOW(),
                    NOW()
                FROM drill_types WHERE drill_code = 'FIRE_DRILL_MONTHLY' LIMIT 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove sample drill schedules
            migrationBuilder.Sql(@"
                DELETE FROM drill_schedules 
                WHERE schedule_code LIKE 'DS-2026%';
            ");
        }
    }
}
