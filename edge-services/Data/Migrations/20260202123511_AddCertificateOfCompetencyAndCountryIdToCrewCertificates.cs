using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateOfCompetencyAndCountryIdToCrewCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "certificate_of_competency",
                schema: "public",
                table: "crew_certificates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                schema: "public",
                table: "crew_certificates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_country_id",
                schema: "public",
                table: "crew_certificates",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_crew_certificates_countries_country_id",
                schema: "public",
                table: "crew_certificates",
                column: "country_id",
                principalSchema: "public",
                principalTable: "countries",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_crew_certificates_countries_country_id",
                schema: "public",
                table: "crew_certificates");

            migrationBuilder.DropIndex(
                name: "IX_crew_certificates_country_id",
                schema: "public",
                table: "crew_certificates");

            migrationBuilder.DropColumn(
                name: "certificate_of_competency",
                schema: "public",
                table: "crew_certificates");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "public",
                table: "crew_certificates");
        }
    }
}
