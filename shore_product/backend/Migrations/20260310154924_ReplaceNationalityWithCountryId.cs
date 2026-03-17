using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceNationalityWithCountryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "crew_members");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "crew_members",
                type: "integer",
                nullable: true);

            // Set default country_id = 1 (Vietnam) for all existing crew members
            migrationBuilder.Sql("UPDATE \"crew_members\" SET \"CountryId\" = 1 WHERE \"CountryId\" IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_CountryId",
                table: "crew_members",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_crew_members_countries_CountryId",
                table: "crew_members",
                column: "CountryId",
                principalTable: "countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_crew_members_countries_CountryId",
                table: "crew_members");

            migrationBuilder.DropIndex(
                name: "IX_crew_members_CountryId",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "crew_members");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
