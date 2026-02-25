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
            migrationBuilder.DropIndex(
                name: "idx_crew_position",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "position",
                schema: "public",
                table: "crew_members");

            migrationBuilder.AddColumn<int>(
                name: "rank_id",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_crew_rank_id",
                schema: "public",
                table: "crew_members",
                column: "rank_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_crew_members__ranks_rank_id",
                schema: "public",
                table: "crew_members",
                column: "rank_id",
                principalSchema: "public",
                principalTable: "ranks",
                principalColumn: "id");
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
