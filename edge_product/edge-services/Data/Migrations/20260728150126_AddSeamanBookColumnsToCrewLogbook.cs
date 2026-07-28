using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeamanBookColumnsToCrewLogbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "approval_history",
                schema: "public",
                table: "crew_logbook_entries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approved_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assignment_id",
                schema: "public",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "call_sign",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conduct",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "deadweight",
                schema: "public",
                table: "crew_logbook_entries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "endorsed_at",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endorsed_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "entry_source",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gross_tonnage",
                schema: "public",
                table: "crew_logbook_entries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imo_number",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_manually_edited",
                schema: "public",
                table: "crew_logbook_entries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_edited_at",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_edited_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "main_engine_maker",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "main_engine_power_kw",
                schema: "public",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "main_engine_type",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "master_name",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rank_at_time",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rank_id",
                schema: "public",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "record_status",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejected_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "sign_off_date",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_port_code",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_port_name",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_reason",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_request_reason",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "sign_off_requested_at",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_off_requested_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_on_by",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "sign_on_date",
                schema: "public",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_on_port_code",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sign_on_port_name",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trade_area",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_flag",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "vessel_id",
                schema: "public",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_name",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_type",
                schema: "public",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "year_built",
                schema: "public",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approval_history",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "approved_at",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "approved_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "assignment_id",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "call_sign",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "conduct",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "deadweight",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "endorsed_at",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "endorsed_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "entry_source",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "gross_tonnage",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "imo_number",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "is_manually_edited",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "last_edited_at",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "last_edited_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "main_engine_maker",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "main_engine_power_kw",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "main_engine_type",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "master_name",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "rank_at_time",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "rank_id",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "record_status",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "rejected_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_date",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_port_code",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_port_name",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_reason",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_request_reason",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_requested_at",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_off_requested_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_on_by",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_on_date",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_on_port_code",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "sign_on_port_name",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "trade_area",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "vessel_flag",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "vessel_id",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "vessel_name",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "vessel_type",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "year_built",
                schema: "public",
                table: "crew_logbook_entries");

        }
    }
}
