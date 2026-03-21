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
            // Use IF NOT EXISTS to handle cases where column was added manually
            migrationBuilder.Sql(@"ALTER TABLE ""MaintenanceTasks"" ADD COLUMN IF NOT EXISTS ""VesselId"" uuid NULL;");
            migrationBuilder.Sql(@"ALTER TABLE ""material_items"" ADD COLUMN IF NOT EXISTS ""VesselId"" uuid NULL;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_items_ItemCode"";");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_material_items_VesselId_ItemCode"" ON ""material_items"" (""VesselId"", ""ItemCode"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_items_VesselId_ItemCode"";");
            migrationBuilder.Sql(@"ALTER TABLE ""material_items"" DROP COLUMN IF EXISTS ""VesselId"";");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_material_items_ItemCode"" ON ""material_items"" (""ItemCode"");");
            migrationBuilder.Sql(@"ALTER TABLE ""MaintenanceTasks"" DROP COLUMN IF EXISTS ""VesselId"";");
        }
    }
}
