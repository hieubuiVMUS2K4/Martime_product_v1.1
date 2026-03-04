using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class TestSyncVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_roles_role_id",
                schema: "public",
                table: "users");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "service_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "ranks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "public",
                table: "ranks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                schema: "public",
                table: "ranks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "ranks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "crew_members",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "crew_certificates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "flag_image_url",
                schema: "public",
                table: "countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5419), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5425) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5426), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5430), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5431), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5432) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5433), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5433) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5434), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5434) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5475), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5475) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5476), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5477) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5477), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5478) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "department", "sort_order", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5479), "DECK", 0, new DateTime(2026, 3, 3, 14, 4, 48, 319, DateTimeKind.Utc).AddTicks(5479) });

            migrationBuilder.AddForeignKey(
                name: "f_k_users_roles_role_id",
                schema: "public",
                table: "users",
                column: "role_id",
                principalSchema: "public",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_users_roles_role_id",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "service_records");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "ranks");

            migrationBuilder.DropColumn(
                name: "department",
                schema: "public",
                table: "ranks");

            migrationBuilder.DropColumn(
                name: "sort_order",
                schema: "public",
                table: "ranks");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "ranks");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "crew_certificates");

            migrationBuilder.DropColumn(
                name: "flag_image_url",
                schema: "public",
                table: "countries");

            migrationBuilder.AddForeignKey(
                name: "FK_users_roles_role_id",
                schema: "public",
                table: "users",
                column: "role_id",
                principalSchema: "public",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
