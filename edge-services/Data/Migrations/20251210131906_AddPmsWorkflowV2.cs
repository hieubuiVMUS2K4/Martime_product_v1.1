using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPmsWorkflowV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.AlterColumn<string>(
                name: "rejection_reason",
                schema: "public",
                table: "maintenance_tasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "actual_duration",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "actual_running_hours",
                schema: "public",
                table: "maintenance_tasks",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assigned_department",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                schema: "public",
                table: "maintenance_tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancelled_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "checklist_completed",
                schema: "public",
                table: "maintenance_tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "deferral_count",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "estimated_duration",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_pending_deferral",
                schema: "public",
                table: "maintenance_tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_cms",
                schema: "public",
                table: "maintenance_tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_deferred_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_deferred_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_rejected_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_rejected_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "photos_uploaded",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rejection_count",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "rejection_history",
                schema: "public",
                table: "maintenance_tasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "required_photos",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "started_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "submitted_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "synced_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_notes",
                schema: "public",
                table: "maintenance_tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_result",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verified_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "task_deferral_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    current_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    proposed_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deferral_days = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attachments = table.Column<string>(type: "jsonb", nullable: true),
                    is_cms_item = table.Column<bool>(type: "boolean", nullable: false),
                    class_permission_letter = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_deferral_requests", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_deferral_requests_maintenance_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_status_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    device_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_status_histories_maintenance_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_assigned_to",
                schema: "public",
                table: "maintenance_tasks",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_department",
                schema: "public",
                table: "maintenance_tasks",
                column: "assigned_department");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_pending_deferral",
                schema: "public",
                table: "maintenance_tasks",
                column: "has_pending_deferral",
                filter: "has_pending_deferral = true");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks",
                columns: new[] { "status", "priority" },
                filter: "status IN ('SCHEDULED', 'DUE', 'OVERDUE', 'IN_PROGRESS', 'PENDING_APPROVAL', 'RECTIFY')");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_pending",
                schema: "public",
                table: "task_deferral_requests",
                columns: new[] { "status", "requested_at" },
                filter: "status = 'PENDING'");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_requested_by",
                schema: "public",
                table: "task_deferral_requests",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_status",
                schema: "public",
                table: "task_deferral_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_synced",
                schema: "public",
                table: "task_deferral_requests",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_task_id",
                schema: "public",
                table: "task_deferral_requests",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_changed_at",
                schema: "public",
                table: "task_status_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_task_id",
                schema: "public",
                table: "task_status_history",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_task_time",
                schema: "public",
                table: "task_status_history",
                columns: new[] { "task_id", "changed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_deferral_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_status_history",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_assigned_to",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_department",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_pending_deferral",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "actual_duration",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "actual_running_hours",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "assigned_department",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "cancelled_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "checklist_completed",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "deferral_count",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "estimated_duration",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "has_pending_deferral",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "is_cms",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "last_deferred_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "last_deferred_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "last_rejected_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "last_rejected_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "photos_uploaded",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "rejection_count",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "rejection_history",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "required_photos",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "started_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "submitted_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "synced_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "verification_notes",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "verification_result",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "verified_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "verified_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.AlterColumn<string>(
                name: "rejection_reason",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks",
                columns: new[] { "status", "priority" },
                filter: "status IN ('PENDING', 'OVERDUE', 'IN_PROGRESS')");
        }
    }
}
