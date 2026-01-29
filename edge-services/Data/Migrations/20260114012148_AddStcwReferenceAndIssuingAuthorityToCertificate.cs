using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStcwReferenceAndIssuingAuthorityToCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "issuing_authority",
                schema: "public",
                table: "certificates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stcw_reference",
                schema: "public",
                table: "certificates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "issuing_authority",
                schema: "public",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "stcw_reference",
                schema: "public",
                table: "certificates");
        }
    }
}
