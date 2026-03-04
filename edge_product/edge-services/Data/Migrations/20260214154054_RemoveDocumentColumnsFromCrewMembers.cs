using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentColumnsFromCrewMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "passport_expiry",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "passport_number",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "rank",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "seaman_book_number",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "visa_expiry",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "visa_number",
                schema: "public",
                table: "crew_members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "passport_expiry",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passport_number",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rank",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seaman_book_number",
                schema: "public",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "visa_expiry",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visa_number",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
