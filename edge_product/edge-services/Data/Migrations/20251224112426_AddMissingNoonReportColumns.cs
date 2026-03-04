using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingNoonReportColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "crew_on_board",
                schema: "public",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maintenance_remarks",
                schema: "public",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "passengers_on_board",
                schema: "public",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "safety_drills_conducted",
                schema: "public",
                table: "noon_reports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "safety_incidents",
                schema: "public",
                table: "noon_reports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crew_on_board",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "maintenance_remarks",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "passengers_on_board",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "safety_drills_conducted",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "safety_incidents",
                schema: "public",
                table: "noon_reports");
        }
    }
}
