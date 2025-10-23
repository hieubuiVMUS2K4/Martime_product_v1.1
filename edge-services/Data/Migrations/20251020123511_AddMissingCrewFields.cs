using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingCrewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "public",
                table: "crew_members",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "certificate_issue",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_end",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "public",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "public",
                table: "crew_members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "join_date",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "medical_issue",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                schema: "public",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "passport_expiry",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "certificate_issue",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "contract_end",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "department",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "join_date",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "medical_issue",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "notes",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "passport_expiry",
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
    }
}
