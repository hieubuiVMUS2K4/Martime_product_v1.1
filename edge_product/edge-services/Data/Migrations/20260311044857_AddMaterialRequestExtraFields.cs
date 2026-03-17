using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialRequestExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachments",
                schema: "public",
                table: "material_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_name",
                schema: "public",
                table: "material_requests",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "material_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "voyage_name",
                schema: "public",
                table: "material_requests",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attachments",
                schema: "public",
                table: "material_requests");

            migrationBuilder.DropColumn(
                name: "vessel_name",
                schema: "public",
                table: "material_requests");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "material_requests");

            migrationBuilder.DropColumn(
                name: "voyage_name",
                schema: "public",
                table: "material_requests");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4817), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4822) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4829), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4829) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4831), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4831) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4838), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4838) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4839), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4839) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4840), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4841) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4842), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4842) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4843), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4843) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4844), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4845) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4845), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4846) });
        }
    }
}
