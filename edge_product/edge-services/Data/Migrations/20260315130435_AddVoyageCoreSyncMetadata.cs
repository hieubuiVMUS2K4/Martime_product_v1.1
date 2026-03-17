using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageCoreSyncMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "voyage_status_history",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_status_history",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_status_history",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_status_history",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "voyage_status_history",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_plan_legs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_plan_legs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_plan_legs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "ports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "ports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "ports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "voyage_status_history");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_status_history");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_status_history");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_status_history");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "voyage_status_history");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_plan_legs");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_plan_legs");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_plan_legs");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "ports");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "ports");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "ports");
        }
    }
}
