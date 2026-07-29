using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselIdToShipMaterialsAndPms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "store_locations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "stock_receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "material_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "inventory_stocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "equipment_groups",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "store_locations");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "material_requests");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "inventory_stocks");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "equipment_groups");
        }
    }
}
