using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeNoteToSmsProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "change_note",
                schema: "public",
                table: "sms_procedures",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "change_note",
                schema: "public",
                table: "sms_procedures");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3801), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3802) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3803), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3803) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3804), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3804) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3805), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3805) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3806), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3807) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3807), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3808) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3809), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3809) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3810), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3810) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3811), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3811) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3812), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3812) });
        }
    }
}
