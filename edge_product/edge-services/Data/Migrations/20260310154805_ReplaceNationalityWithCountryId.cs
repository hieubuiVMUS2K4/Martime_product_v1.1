using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceNationalityWithCountryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nationality",
                schema: "public",
                table: "crew_members");

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            // Set default country_id = 1 (Vietnam) for all existing crew members
            migrationBuilder.Sql("UPDATE \"public\".\"crew_members\" SET \"country_id\" = 1 WHERE \"country_id\" IS NULL;");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7188), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7192) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7196), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7196) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7197), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7198) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7198), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7199) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7200), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7200) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7201), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7201) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7202), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7202) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7203), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7203) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7204), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7205) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7206), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7206) });

            migrationBuilder.CreateIndex(
                name: "idx_crew_country_id",
                schema: "public",
                table: "crew_members",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_crew_members_countries_country_id",
                schema: "public",
                table: "crew_members",
                column: "country_id",
                principalSchema: "public",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_crew_members_countries_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropIndex(
                name: "idx_crew_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7861), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7863) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7864), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7865) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7866), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7866) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7867), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7867) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7868), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7868) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7869), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7869) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7870), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7871) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7871), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7872) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7873), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7873) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7874), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7874) });
        }
    }
}
