using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSyncVersionAndRankFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5419), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5425) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5426), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5430), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5431), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5432) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5433), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5433) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5434), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5434) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5475), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5475) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5476), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5477) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5477), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5478) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5479), new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5479) });
        }
    }
}
