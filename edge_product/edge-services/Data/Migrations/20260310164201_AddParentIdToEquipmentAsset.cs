using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentIdToEquipmentAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                schema: "public",
                table: "equipment_assets",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(167), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(193) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(222), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(222) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(224), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(224) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(225), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(225) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(226), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(226) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(227), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(228) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(228), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(229) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(235), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(236) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(236), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(237) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(237), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(238) });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_equipment_assets_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets",
                column: "parent_id",
                principalSchema: "public",
                principalTable: "equipment_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_equipment_assets_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets");

            migrationBuilder.DropIndex(
                name: "IX_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets");

            migrationBuilder.DropColumn(
                name: "parent_id",
                schema: "public",
                table: "equipment_assets");

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
