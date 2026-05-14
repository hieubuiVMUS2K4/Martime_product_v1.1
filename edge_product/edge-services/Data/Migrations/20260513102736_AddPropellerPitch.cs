using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropellerPitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "propeller_pitch",
                schema: "public",
                table: "engine_data",
                type: "double precision",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8153), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8155) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8157), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8157) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8158), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8158) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8159), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8159) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8161) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8161), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8162) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8163), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8163) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8164), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8164) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8165), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8165) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8166), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8166) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "propeller_pitch",
                schema: "public",
                table: "engine_data");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5668), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5670) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5671), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5671) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5672), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5672) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5684), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5685) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5686), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5686) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5687), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5687) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5688), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5688) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5689), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5689) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5690), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5691) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5699), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5699) });
        }
    }
}
