using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineDataMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pitch",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "Roll",
                table: "noon_reports");

            // Add missing columns safely (may already exist from raw SQL migration in Program.cs)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'EngineData' AND column_name = 'IsRunning'
                    ) THEN
                        ALTER TABLE public.""EngineData"" ADD COLUMN ""IsRunning"" boolean NOT NULL DEFAULT false;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'EngineData' AND column_name = 'Rpm'
                    ) THEN
                        ALTER TABLE public.""EngineData"" ADD COLUMN ""Rpm"" double precision;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'EngineData' AND column_name = 'LoadPercent'
                    ) THEN
                        ALTER TABLE public.""EngineData"" ADD COLUMN ""LoadPercent"" double precision;
                    END IF;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.""EngineData"" DROP COLUMN IF EXISTS ""IsRunning"";
                ALTER TABLE public.""EngineData"" DROP COLUMN IF EXISTS ""Rpm"";
                ALTER TABLE public.""EngineData"" DROP COLUMN IF EXISTS ""LoadPercent"";
            ");

            migrationBuilder.AddColumn<double>(
                name: "Pitch",
                table: "noon_reports",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Roll",
                table: "noon_reports",
                type: "double precision",
                nullable: true);
        }
    }
}
