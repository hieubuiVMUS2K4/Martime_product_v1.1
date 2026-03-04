using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCountryCodeAlpha2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_country_code_alpha2",
                schema: "public",
                table: "countries");

            migrationBuilder.DropColumn(
                name: "country_code_alpha2",
                schema: "public",
                table: "countries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country_code_alpha2",
                schema: "public",
                table: "countries",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_country_code_alpha2",
                schema: "public",
                table: "countries",
                column: "country_code_alpha2");
        }
    }
}
