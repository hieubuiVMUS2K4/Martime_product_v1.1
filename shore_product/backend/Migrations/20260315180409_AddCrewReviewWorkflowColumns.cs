using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewReviewWorkflowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EdgeChanges",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EdgeChangesViewed",
                table: "crew_members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardStatusChangedAt",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardStatusChangedBy",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewChecklist",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "crew_members",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EdgeChanges",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "EdgeChangesViewed",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "OnboardStatusChangedAt",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "OnboardStatusChangedBy",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "ReviewChecklist",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "crew_members");
        }
    }
}
