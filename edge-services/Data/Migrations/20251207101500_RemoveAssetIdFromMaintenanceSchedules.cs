using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssetIdFromMaintenanceSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove old asset_id column - schedules now use equipment_group_id
            migrationBuilder.DropColumn(
                name: "asset_id",
                schema: "public",
                table: "maintenance_schedules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add asset_id column if rollback needed
            migrationBuilder.AddColumn<Guid>(
                name: "asset_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: true); // Nullable to allow existing rows
        }
    }
}
