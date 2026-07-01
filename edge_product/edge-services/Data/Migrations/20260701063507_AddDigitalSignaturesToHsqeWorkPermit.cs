using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalSignaturesToHsqeWorkPermit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "digital_signatures",
                schema: "public",
                table: "hsqe_work_permits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2417), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2422) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2423), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2423) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2424), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2425) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2425), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2426) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2427), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2427) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2428), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2428) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2429), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2431), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2432), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2433) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "digital_signatures",
                schema: "public",
                table: "hsqe_work_permits");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3063), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3068) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3069), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3069) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3070), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3070) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3091), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3091) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3092), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3092) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3093), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3093) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3094), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3095) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3096), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3096) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3097), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3097) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3098), new DateTime(2026, 6, 26, 14, 54, 2, 875, DateTimeKind.Utc).AddTicks(3098) });
        }
    }
}
