using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToDrillSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "delete_reason",
                schema: "public",
                table: "drill_schedules",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "drill_schedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "drill_schedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "drill_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delete_reason",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "drill_schedules");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "drill_schedules");
        }
    }
}
