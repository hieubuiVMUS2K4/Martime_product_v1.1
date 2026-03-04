using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCountryIdFromRankCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_rank_certificates_countries_country_id",
                schema: "public",
                table: "rank_certificates");

            migrationBuilder.DropIndex(
                name: "IX_rank_certificates_country_id",
                schema: "public",
                table: "rank_certificates");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "public",
                table: "rank_certificates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "country_id",
                schema: "public",
                table: "rank_certificates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_country_id",
                schema: "public",
                table: "rank_certificates",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_rank_certificates_countries_country_id",
                schema: "public",
                table: "rank_certificates",
                column: "country_id",
                principalSchema: "public",
                principalTable: "countries",
                principalColumn: "id");
        }
    }
}
