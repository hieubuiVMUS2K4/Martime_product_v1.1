using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselIdToMaterialItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "MaintenanceTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "material_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropIndex(
                name: "IX_material_items_ItemCode",
                table: "material_items");

            migrationBuilder.CreateIndex(
                name: "IX_material_items_VesselId_ItemCode",
                table: "material_items",
                columns: new[] { "VesselId", "ItemCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_material_items_VesselId_ItemCode",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "material_items");

            migrationBuilder.CreateIndex(
                name: "IX_material_items_ItemCode",
                table: "material_items",
                column: "ItemCode",
                unique: true);
        }
    }
}
