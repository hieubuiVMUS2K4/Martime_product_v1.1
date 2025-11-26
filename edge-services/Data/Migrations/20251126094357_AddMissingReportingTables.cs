using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingReportingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_task_details_task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details");

            migrationBuilder.DropIndex(
                name: "idx_task_detail_type_id",
                schema: "public",
                table: "task_details");

            migrationBuilder.DropIndex(
                name: "idx_task_detail_type_order",
                schema: "public",
                table: "task_details");

            migrationBuilder.DropIndex(
                name: "idx_sync_table_record",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "task_type_id",
                schema: "public",
                table: "task_details");

            migrationBuilder.DropColumn(
                name: "record_id",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.RenameColumn(
                name: "r_o_b_before",
                schema: "public",
                table: "bunker_reports",
                newName: "r_o_before");

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "weekly_performance_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "weekly_performance_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "watchkeeping_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "watchkeeping_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "voyage_records",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "tank_levels",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "tank_levels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "tank_levels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "action_type",
                schema: "public",
                table: "sync_queue",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "record_key",
                schema: "public",
                table: "sync_queue",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "safety_alarms",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "safety_alarms",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "safety_alarms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_workflow_histories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "report_workflow_histories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_transmission_logs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "report_transmission_logs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_attachments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "report_attachments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "report_attachments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "report_attachments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "original_report_id",
                schema: "public",
                table: "report_amendments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "report_amendments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "position_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "position_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "position_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "position_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "position_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "oil_record_books",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "oil_record_books",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "oil_record_books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "noon_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "noon_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "navigation_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "navigation_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "navigation_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "monthly_summary_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "material_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "material_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "material_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "maritime_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "maritime_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "maritime_reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "maintenance_tasks",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "maintenance_task_details",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "generator_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "generator_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "generator_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "fuel_consumption",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "fuel_consumption",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "fuel_consumption",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "environmental_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "environmental_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "environmental_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "engine_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "engine_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "engine_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "departure_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "departure_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "departure_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "crew_members",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "cargo_operations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "cargo_operations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "cargo_operations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "cargo_operations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "bunker_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "bunker_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "arrival_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "maritime_report_id",
                schema: "public",
                table: "arrival_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "arrival_reports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "ais_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "ais_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "ais_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "task_type_task_details",
                schema: "public",
                columns: table => new
                {
                    TaskTypeId = table.Column<int>(type: "integer", nullable: false),
                    TaskDetailId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_type_task_details", x => new { x.TaskTypeId, x.TaskDetailId });
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_details_TaskDetailId",
                        column: x => x.TaskDetailId,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_types_TaskTypeId",
                        column: x => x.TaskTypeId,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                column: "TaskDetailId1");

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_detail_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskDetailId");

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_type_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_task_details_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                column: "TaskDetailId1",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_maintenance_task_details__task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "f_k_maintenance_task_details__task_types_task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_task_details_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropForeignKey(
                name: "f_k_maintenance_task_details__task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropForeignKey(
                name: "f_k_maintenance_task_details__task_types_task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropTable(
                name: "task_type_task_details",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_maintenance_task_details_task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropIndex(
                name: "IX_maintenance_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "tank_levels");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "tank_levels");

            migrationBuilder.DropColumn(
                name: "action_type",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "record_key",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "report_attachments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "report_attachments");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "position_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "position_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "navigation_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "navigation_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "maritime_reports");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropColumn(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "generator_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "generator_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "environmental_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "environmental_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "engine_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "engine_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "ais_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "ais_data");

            migrationBuilder.RenameColumn(
                name: "r_o_before",
                schema: "public",
                table: "bunker_reports",
                newName: "r_o_b_before");

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "weekly_performance_reports",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "weekly_performance_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "voyage_records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "task_details",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "tank_levels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "record_id",
                schema: "public",
                table: "sync_queue",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "safety_alarms",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_workflow_histories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "report_workflow_histories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_transmission_logs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "report_transmission_logs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "report_attachments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "report_attachments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "original_report_id",
                schema: "public",
                table: "report_amendments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "report_amendments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "position_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "position_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "position_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "oil_record_books",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "noon_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "noon_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "navigation_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "monthly_summary_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "material_items",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "maritime_reports",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "maritime_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "maintenance_tasks",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "generator_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "fuel_consumption",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "environmental_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "engine_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "departure_reports",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "departure_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "departure_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "crew_members",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "cargo_operations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "cargo_operations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "bunker_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "bunker_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "arrival_reports",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "maritime_report_id",
                schema: "public",
                table: "arrival_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "arrival_reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "ais_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_type_id",
                schema: "public",
                table: "task_details",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_type_order",
                schema: "public",
                table: "task_details",
                columns: new[] { "task_type_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_table_record",
                schema: "public",
                table: "sync_queue",
                columns: new[] { "table_name", "record_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_task_details_task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
