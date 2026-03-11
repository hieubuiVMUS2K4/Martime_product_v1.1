using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselIdToCrewMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "crew_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_VesselId",
                table: "crew_members",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_crew_members_VesselId",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "crew_members");
        }
    }
}
