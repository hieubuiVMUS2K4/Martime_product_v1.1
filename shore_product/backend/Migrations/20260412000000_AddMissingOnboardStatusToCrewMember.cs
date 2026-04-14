using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingOnboardStatusToCrewMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OnboardStatus was added to the EF Core model (shared CrewMember.cs)
            // but was never included in any prior migration, causing a schema drift
            // between the EF model snapshot and the actual deployed database.
            // This migration fixes that gap using IF NOT EXISTS for idempotency
            // (in case the column was already added manually on some environments).
            migrationBuilder.Sql(@"
                ALTER TABLE crew_members
                ADD COLUMN IF NOT EXISTS ""OnboardStatus"" character varying(20) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardStatus",
                table: "crew_members");
        }
    }
}
