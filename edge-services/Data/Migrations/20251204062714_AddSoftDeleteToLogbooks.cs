using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToLogbooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "watchkeeping_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "watchkeeping_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "watchkeeping_logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "oil_record_books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "oil_record_books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "oil_record_books",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "garbage_record_books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "garbage_record_books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "garbage_record_books",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "engine_log_books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "engine_log_books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "engine_log_books",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "deck_log_books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "deck_log_books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "deck_log_books",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "ballast_water_record_books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "ballast_water_record_books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "ballast_water_record_books",
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
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "ballast_water_record_books");
        }
    }
}
