using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedRanksData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "ranks",
                columns: new[] { "id", "is_active", "rank_code", "rank_name" },
                values: new object[,]
                {
                    { 1, true, "MAST", "Master (Captain)" },
                    { 2, true, "C/O", "Chief Officer" },
                    { 3, true, "2/O", "Second Officer" },
                    { 4, true, "3/O", "Third Officer" },
                    { 5, true, "C/E", "Chief Engineer" },
                    { 6, true, "2/E", "Second Engineer" },
                    { 7, true, "BOSN", "Bosun" },
                    { 8, true, "AB", "Able Seaman" },
                    { 9, true, "OILR", "Oiler" },
                    { 10, true, "COOK", "Chief Cook" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10);
        }
    }
}
