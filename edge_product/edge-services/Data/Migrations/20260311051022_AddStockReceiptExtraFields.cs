using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockReceiptExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachments",
                schema: "public",
                table: "stock_receipts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_name",
                schema: "public",
                table: "stock_receipts",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "stock_receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "voyage_name",
                schema: "public",
                table: "stock_receipts",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attachments",
                schema: "public",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "vessel_name",
                schema: "public",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "voyage_name",
                schema: "public",
                table: "stock_receipts");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2205), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2218) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2228), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2228) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2229), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2230) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2231), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2239) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2240), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2240) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2241), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2241) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2242), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2242) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2243), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2246) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2247), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2247) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2248), new DateTime(2026, 3, 11, 4, 48, 56, 329, DateTimeKind.Utc).AddTicks(2248) });
        }
    }
}
