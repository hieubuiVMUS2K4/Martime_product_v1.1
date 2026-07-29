using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSyncedToMaterialLogisticsAndPms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "stock_receipts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "stock_receipt_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "schedule_spare_parts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "schedule_checklist_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "material_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "material_request_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "inventory_stock",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "equipment_group_members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6193), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6200) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6203), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6204) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6205), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6205) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6213), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6213) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6215), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6215) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6216), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6216) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6217), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6218) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6219), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6219) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6220), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6220) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6229), new DateTime(2026, 7, 29, 8, 40, 20, 126, DateTimeKind.Utc).AddTicks(6229) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "stock_receipt_items");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "schedule_spare_parts");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "schedule_checklist_templates");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "material_requests");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "material_request_items");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "inventory_stock");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "equipment_group_members");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7569), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7573) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7577), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7578) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7579), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7579) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7580), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7581) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7582), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7582) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7584), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7584) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7585), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7586) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7586), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7587) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7588), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7588) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7591), new DateTime(2026, 7, 28, 15, 1, 23, 730, DateTimeKind.Utc).AddTicks(7591) });
        }
    }
}
