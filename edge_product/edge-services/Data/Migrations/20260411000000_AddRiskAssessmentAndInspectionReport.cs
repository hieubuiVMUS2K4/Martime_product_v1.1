using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskAssessmentAndInspectionReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Add flags to maintenance_schedules (idempotent) ──
            migrationBuilder.Sql("ALTER TABLE public.maintenance_schedules ADD COLUMN IF NOT EXISTS require_risk_assessment boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE public.maintenance_schedules ADD COLUMN IF NOT EXISTS require_inspection_report boolean NOT NULL DEFAULT false;");

            // ── Add flags to maintenance_tasks (idempotent) ──
            migrationBuilder.Sql("ALTER TABLE public.maintenance_tasks ADD COLUMN IF NOT EXISTS require_risk_assessment boolean NOT NULL DEFAULT false;");
            migrationBuilder.Sql("ALTER TABLE public.maintenance_tasks ADD COLUMN IF NOT EXISTS require_inspection_report boolean NOT NULL DEFAULT false;");

            // ── Create task_risk_assessments table (idempotent) ──
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS public.task_risk_assessments (
    id uuid NOT NULL PRIMARY KEY,
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
    is_approved_to_proceed boolean NOT NULL DEFAULT true,
    worker_signature character varying(100),
    supervisor_signature character varying(100),
    chief_engineer_approval character varying(100),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(50)
);
CREATE INDEX IF NOT EXISTS ""IX_task_risk_assessments_task_id"" ON public.task_risk_assessments(task_id);
");

            // ── Create task_inspection_reports table (idempotent) ──
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS public.task_inspection_reports (
    id uuid NOT NULL PRIMARY KEY,
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
    created_by character varying(50)
);
CREATE INDEX IF NOT EXISTS ""IX_task_inspection_reports_task_id"" ON public.task_inspection_reports(task_id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "task_risk_assessments", schema: "public");
            migrationBuilder.DropTable(name: "task_inspection_reports", schema: "public");

            migrationBuilder.DropColumn(name: "require_risk_assessment", schema: "public", table: "maintenance_schedules");
            migrationBuilder.DropColumn(name: "require_inspection_report", schema: "public", table: "maintenance_schedules");
            migrationBuilder.DropColumn(name: "require_risk_assessment", schema: "public", table: "maintenance_tasks");
            migrationBuilder.DropColumn(name: "require_inspection_report", schema: "public", table: "maintenance_tasks");
        }
    }
}
