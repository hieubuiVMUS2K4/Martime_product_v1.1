using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionsFieldsToDrillSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_crew_member_required",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_document_required",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_fixed_interval",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_mandatory_sign_on_evaluation",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_secure_history",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_crew_member_required",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "is_document_required",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "is_fixed_interval",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "is_mandatory_sign_on_evaluation",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "is_secure_history",
                schema: "public",
                table: "drill_schedules");
        }
    }
}
