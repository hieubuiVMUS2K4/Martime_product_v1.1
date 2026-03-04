using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRankIdToCrewMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop index only if it exists
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.idx_crew_position;
            ");

            // Drop column only if it exists
            migrationBuilder.Sql(@"
                ALTER TABLE public.crew_members 
                DROP COLUMN IF EXISTS position;
            ");

            // Add rank_id column if not exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'crew_members' 
                        AND column_name = 'rank_id'
                    ) THEN
                        ALTER TABLE public.crew_members 
                        ADD COLUMN rank_id INTEGER NULL;
                    END IF;
                END $$;
            ");

            // Create index if not exists
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_crew_rank_id 
                ON public.crew_members(rank_id);
            ");

            // Add foreign key if not exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'f_k_crew_members__ranks_rank_id'
                        AND table_name = 'crew_members'
                    ) THEN
                        ALTER TABLE public.crew_members 
                        ADD CONSTRAINT f_k_crew_members__ranks_rank_id 
                        FOREIGN KEY (rank_id) 
                        REFERENCES public.ranks(id);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_crew_members__ranks_rank_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropIndex(
                name: "idx_crew_rank_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "rank_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.AddColumn<string>(
                name: "position",
                schema: "public",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_crew_position",
                schema: "public",
                table: "crew_members",
                column: "position");
        }
    }
}
