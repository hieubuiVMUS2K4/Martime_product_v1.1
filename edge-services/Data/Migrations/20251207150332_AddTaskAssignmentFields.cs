using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "assigned_to_crew_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assigned_to_role",
                schema: "public",
                table: "maintenance_schedules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approver_role",
                schema: "public",
                table: "equipment_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_executor_role",
                schema: "public",
                table: "equipment_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assigned_to_crew_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "assigned_to_role",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "approver_role",
                schema: "public",
                table: "equipment_assets");

            migrationBuilder.DropColumn(
                name: "default_executor_role",
                schema: "public",
                table: "equipment_assets");
        }
    }
}
