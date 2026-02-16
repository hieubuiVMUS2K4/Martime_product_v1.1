using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCrewIdFromTravelDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crew_id",
                schema: "public",
                table: "travel_documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "crew_id",
                schema: "public",
                table: "travel_documents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
