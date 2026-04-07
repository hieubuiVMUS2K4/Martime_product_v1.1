using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class FixVoyageNumberUniquePerVessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the global unique constraint on VoyageNumber alone.
            // VoyageNumber only needs to be unique per vessel, not across the entire fleet.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_voyage_records_VoyageNumber"";");

            // Re-create as composite unique: (VesselIMO, VoyageNumber)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_voyage_records_VesselIMO_VoyageNumber""
                ON voyage_records (""VesselIMO"", ""VoyageNumber"")
                WHERE ""VesselIMO"" IS NOT NULL AND ""VoyageNumber"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_voyage_records_VesselIMO_VoyageNumber",
                table: "voyage_records");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_VoyageNumber",
                table: "voyage_records",
                column: "VoyageNumber",
                unique: true);
        }
    }
}
