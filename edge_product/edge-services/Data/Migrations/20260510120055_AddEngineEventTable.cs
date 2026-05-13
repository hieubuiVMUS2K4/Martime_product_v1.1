using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely add columns that may already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'maintenance_tasks' AND column_name = 'require_inspection_report'
                    ) THEN
                        ALTER TABLE public.maintenance_tasks ADD COLUMN require_inspection_report boolean NOT NULL DEFAULT false;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'maintenance_tasks' AND column_name = 'require_risk_assessment'
                    ) THEN
                        ALTER TABLE public.maintenance_tasks ADD COLUMN require_risk_assessment boolean NOT NULL DEFAULT false;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'maintenance_schedules' AND column_name = 'require_inspection_report'
                    ) THEN
                        ALTER TABLE public.maintenance_schedules ADD COLUMN require_inspection_report boolean NOT NULL DEFAULT false;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'maintenance_schedules' AND column_name = 'require_risk_assessment'
                    ) THEN
                        ALTER TABLE public.maintenance_schedules ADD COLUMN require_risk_assessment boolean NOT NULL DEFAULT false;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'engine_data' AND column_name = 'is_running'
                    ) THEN
                        ALTER TABLE public.engine_data ADD COLUMN is_running boolean NOT NULL DEFAULT false;
                    END IF;
                END;
                $$;
            ");

            migrationBuilder.CreateTable(
                name: "engine_events",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    engine_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    event_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    rpm = table.Column<double>(type: "double precision", nullable: true),
                    load_percent = table.Column<double>(type: "double precision", nullable: true),
                    trigger_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_engine_events", x => x.id);
                });

            // Index for querying engine events by origin_node + timestamp
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""ix_engine_events_origin_node_timestamp""
                ON public.engine_events (origin_node, timestamp DESC);
            ");

            // task_inspection_reports and task_risk_assessments may already exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.task_inspection_reports (
                    id uuid NOT NULL,
                    task_id character varying(100) NOT NULL,
                    ship_name character varying(200),
                    equipment_name character varying(200),
                    equipment_code character varying(50),
                    maintenance_type character varying(30),
                    maintenance_date timestamp with time zone,
                    job_items_json text,
                    post_maintenance_status character varying(30),
                    recommendations text,
                    operator_signature character varying(100),
                    chief_engineer_signature character varying(100),
                    overall_result character varying(10),
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    created_by character varying(50),
                    CONSTRAINT p_k_task_inspection_reports PRIMARY KEY (id)
                );

                CREATE TABLE IF NOT EXISTS public.task_risk_assessments (
                    id uuid NOT NULL,
                    task_id character varying(100) NOT NULL,
                    job_name character varying(200),
                    equipment_name character varying(200),
                    location character varying(200),
                    assessment_date timestamp with time zone,
                    personnel character varying(500),
                    ra_number character varying(50),
                    hazard_mechanical boolean NOT NULL DEFAULT false,
                    hazard_electrical boolean NOT NULL DEFAULT false,
                    hazard_chemical boolean NOT NULL DEFAULT false,
                    hazard_environmental boolean NOT NULL DEFAULT false,
                    hazard_notes text,
                    initial_severity character varying(20),
                    initial_likelihood character varying(20),
                    initial_risk_level character varying(20),
                    control_loto boolean NOT NULL DEFAULT false,
                    control_ptw boolean NOT NULL DEFAULT false,
                    control_ppe boolean NOT NULL DEFAULT false,
                    control_ventilation boolean NOT NULL DEFAULT false,
                    control_notes text,
                    residual_severity character varying(20),
                    residual_likelihood character varying(20),
                    residual_risk_level character varying(20),
                    residual_risk_notes text,
                    is_approved_to_proceed boolean NOT NULL DEFAULT false,
                    worker_signature character varying(100),
                    supervisor_signature character varying(100),
                    chief_engineer_approval character varying(100),
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    created_by character varying(50),
                    CONSTRAINT p_k_task_risk_assessments PRIMARY KEY (id)
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_events",
                schema: "public");

            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS public.task_inspection_reports;
                DROP TABLE IF EXISTS public.task_risk_assessments;
                ALTER TABLE public.maintenance_tasks DROP COLUMN IF EXISTS require_inspection_report;
                ALTER TABLE public.maintenance_tasks DROP COLUMN IF EXISTS require_risk_assessment;
                ALTER TABLE public.maintenance_schedules DROP COLUMN IF EXISTS require_inspection_report;
                ALTER TABLE public.maintenance_schedules DROP COLUMN IF EXISTS require_risk_assessment;
                ALTER TABLE public.engine_data DROP COLUMN IF EXISTS is_running;
            ");
        }
    }
}
