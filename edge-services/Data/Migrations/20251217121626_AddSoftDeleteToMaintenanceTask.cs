using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToMaintenanceTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                schema: "public",
                table: "maintenance_tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "maintenance_tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "deletion_reason",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "maintenance_tasks");
        }
    }
}
