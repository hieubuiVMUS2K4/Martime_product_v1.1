using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddPitchRollToNoonReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Pitch",
                table: "noon_reports",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Roll",
                table: "noon_reports",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pitch",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "Roll",
                table: "noon_reports");
        }
    }
}
