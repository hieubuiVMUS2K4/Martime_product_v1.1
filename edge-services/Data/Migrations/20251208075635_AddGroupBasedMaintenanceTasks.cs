using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupBasedMaintenanceTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "equipment_name",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "equipment_id",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "equipment_group_id",
                schema: "public",
                table: "maintenance_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "equipment_group_name",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_maintenance_tasks_task_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_id");

            migrationBuilder.CreateTable(
                name: "task_checklist_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    asset_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reading_value = table.Column<double>(type: "double precision", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_abnormal = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_checklist_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_checklist_items_equipment_assets_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "public",
                        principalTable: "equipment_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_task_checklist_items_maintenance_tasks_task_id1",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_tasks_equipment_group_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "equipment_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_checklist_items_asset_id",
                schema: "public",
                table: "task_checklist_items",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_checklist_items_task_id",
                schema: "public",
                table: "task_checklist_items",
                column: "task_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_maintenance_tasks_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "equipment_group_id",
                principalSchema: "public",
                principalTable: "equipment_groups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_maintenance_tasks_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropTable(
                name: "task_checklist_items",
                schema: "public");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_maintenance_tasks_task_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "IX_maintenance_tasks_equipment_group_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_group_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_group_name",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.AlterColumn<string>(
                name: "equipment_name",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "equipment_id",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
