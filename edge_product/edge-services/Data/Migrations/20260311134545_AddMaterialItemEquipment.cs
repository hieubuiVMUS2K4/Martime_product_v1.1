using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialItemEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_item_equipments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_item_equipments", x => x.id);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(20), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(24) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(33), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(33) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(34), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(34) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(35), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(35) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(36), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(38) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(38), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(39) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(40), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(40) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(41), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(41) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(42), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(43) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(44), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(44) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_item_equipments",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5233), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5237) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5242), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5242) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5243), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5244) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5253), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5253) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5254), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5255) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5256), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5256) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5257), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5257) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5258), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5258) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5259), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5259) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5260), new DateTime(2026, 3, 11, 5, 10, 21, 198, DateTimeKind.Utc).AddTicks(5261) });
        }
    }
}
