using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsRecognizedAndNotesFromCountryCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_country_cert_recognized",
                schema: "public",
                table: "country_certificates");

            migrationBuilder.DropColumn(
                name: "is_recognized",
                schema: "public",
                table: "country_certificates");

            migrationBuilder.DropColumn(
                name: "notes",
                schema: "public",
                table: "country_certificates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_recognized",
                schema: "public",
                table: "country_certificates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                schema: "public",
                table: "country_certificates",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_country_cert_recognized",
                schema: "public",
                table: "country_certificates",
                column: "is_recognized",
                filter: "is_recognized = true");
        }
    }
}
