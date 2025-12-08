using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckpointFieldsToTaskChecklistItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "checkpoint_description",
                schema: "public",
                table: "task_checklist_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "normal_range_max",
                schema: "public",
                table: "task_checklist_items",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "normal_range_min",
                schema: "public",
                table: "task_checklist_items",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_reading",
                schema: "public",
                table: "task_checklist_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "unit",
                schema: "public",
                table: "task_checklist_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checkpoint_description",
                schema: "public",
                table: "task_checklist_items");

            migrationBuilder.DropColumn(
                name: "normal_range_max",
                schema: "public",
                table: "task_checklist_items");

            migrationBuilder.DropColumn(
                name: "normal_range_min",
                schema: "public",
                table: "task_checklist_items");

            migrationBuilder.DropColumn(
                name: "requires_reading",
                schema: "public",
                table: "task_checklist_items");

            migrationBuilder.DropColumn(
                name: "unit",
                schema: "public",
                table: "task_checklist_items");
        }
    }
}
