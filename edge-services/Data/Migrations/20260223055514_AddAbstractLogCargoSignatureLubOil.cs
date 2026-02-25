using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAbstractLogCargoSignatureLubOil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "chief_engineer_signature",
                schema: "public",
                table: "abstract_log_voyages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "chief_engineer_signed_at",
                schema: "public",
                table: "abstract_log_voyages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "master_signature",
                schema: "public",
                table: "abstract_log_voyages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "master_signed_at",
                schema: "public",
                table: "abstract_log_voyages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "cargo_quantity",
                schema: "public",
                table: "abstract_log_legs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cargo_type",
                schema: "public",
                table: "abstract_log_legs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "load_condition",
                schema: "public",
                table: "abstract_log_legs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "cyl_oil_consumed",
                schema: "public",
                table: "abstract_log_daily_entries",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "fw_consumed",
                schema: "public",
                table: "abstract_log_daily_entries",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "fw_produced",
                schema: "public",
                table: "abstract_log_daily_entries",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "sys_oil_consumed",
                schema: "public",
                table: "abstract_log_daily_entries",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chief_engineer_signature",
                schema: "public",
                table: "abstract_log_voyages");

            migrationBuilder.DropColumn(
                name: "chief_engineer_signed_at",
                schema: "public",
                table: "abstract_log_voyages");

            migrationBuilder.DropColumn(
                name: "master_signature",
                schema: "public",
                table: "abstract_log_voyages");

            migrationBuilder.DropColumn(
                name: "master_signed_at",
                schema: "public",
                table: "abstract_log_voyages");

            migrationBuilder.DropColumn(
                name: "cargo_quantity",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.DropColumn(
                name: "cargo_type",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.DropColumn(
                name: "load_condition",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.DropColumn(
                name: "cyl_oil_consumed",
                schema: "public",
                table: "abstract_log_daily_entries");

            migrationBuilder.DropColumn(
                name: "fw_consumed",
                schema: "public",
                table: "abstract_log_daily_entries");

            migrationBuilder.DropColumn(
                name: "fw_produced",
                schema: "public",
                table: "abstract_log_daily_entries");

            migrationBuilder.DropColumn(
                name: "sys_oil_consumed",
                schema: "public",
                table: "abstract_log_daily_entries");
        }
    }
}
