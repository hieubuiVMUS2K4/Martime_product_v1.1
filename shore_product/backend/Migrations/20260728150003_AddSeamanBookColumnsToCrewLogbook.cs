using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddSeamanBookColumnsToCrewLogbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalHistory",
                table: "crew_logbook_entries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallSign",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conduct",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Deadweight",
                table: "crew_logbook_entries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndorsedAt",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndorsedBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntrySource",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossTonnage",
                table: "crew_logbook_entries",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImoNumber",
                table: "crew_logbook_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyEdited",
                table: "crew_logbook_entries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastEditedBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainEngineMaker",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MainEnginePowerKw",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainEngineType",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MasterName",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RankAtTime",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RankId",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordStatus",
                table: "crew_logbook_entries",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "crew_logbook_entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignOffDate",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffPortCode",
                table: "crew_logbook_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffPortName",
                table: "crew_logbook_entries",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffReason",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffRequestReason",
                table: "crew_logbook_entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignOffRequestedAt",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffRequestedBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOnBy",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignOnDate",
                table: "crew_logbook_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOnPortCode",
                table: "crew_logbook_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOnPortName",
                table: "crew_logbook_entries",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeArea",
                table: "crew_logbook_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselFlag",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VesselId",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselName",
                table: "crew_logbook_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselType",
                table: "crew_logbook_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoyageId",
                table: "crew_logbook_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "crew_logbook_entries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalHistory",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "CallSign",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "Conduct",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "Deadweight",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "EndorsedAt",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "EndorsedBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "EntrySource",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "GrossTonnage",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "ImoNumber",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "IsManuallyEdited",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "MainEngineMaker",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "MainEnginePowerKw",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "MainEngineType",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "MasterName",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RankAtTime",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RankId",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RecordStatus",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffDate",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffPortCode",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffPortName",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffReason",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffRequestReason",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffRequestedAt",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOffRequestedBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOnBy",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOnDate",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOnPortCode",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "SignOnPortName",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "TradeArea",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "VesselFlag",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "VesselName",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "VesselType",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "crew_logbook_entries");

            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "crew_logbook_entries");
        }
    }
}
