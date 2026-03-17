using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AlignReportFieldsWithEdge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "departure_reports");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "arrival_reports");

            migrationBuilder.RenameColumn(
                name: "PortLocode",
                table: "departure_reports",
                newName: "PortCode");

            migrationBuilder.RenameColumn(
                name: "PersonsOnBoard",
                table: "departure_reports",
                newName: "PassengersOnBoard");

            migrationBuilder.RenameColumn(
                name: "NextPortLocode",
                table: "departure_reports",
                newName: "NextPortCode");

            migrationBuilder.RenameColumn(
                name: "ETA",
                table: "departure_reports",
                newName: "PilotOnBoardTime");

            migrationBuilder.RenameColumn(
                name: "DraftFore",
                table: "departure_reports",
                newName: "DraftForward");

            migrationBuilder.RenameColumn(
                name: "DepartureLon",
                table: "departure_reports",
                newName: "DepartureLongitude");

            migrationBuilder.RenameColumn(
                name: "DepartureLat",
                table: "departure_reports",
                newName: "DepartureLatitude");

            migrationBuilder.RenameColumn(
                name: "DepartureDateTimeLocal",
                table: "departure_reports",
                newName: "LastLineAshoreTime");

            migrationBuilder.RenameColumn(
                name: "VoyageDurationHours",
                table: "arrival_reports",
                newName: "VoyageDuration");

            migrationBuilder.RenameColumn(
                name: "PortLocode",
                table: "arrival_reports",
                newName: "PortCode");

            migrationBuilder.RenameColumn(
                name: "PersonsOnBoard",
                table: "arrival_reports",
                newName: "PassengersOnBoard");

            migrationBuilder.RenameColumn(
                name: "FuelOilConsumed",
                table: "arrival_reports",
                newName: "TotalFuelConsumed");

            migrationBuilder.RenameColumn(
                name: "DraftFore",
                table: "arrival_reports",
                newName: "DraftForward");

            migrationBuilder.RenameColumn(
                name: "DieselOilConsumed",
                table: "arrival_reports",
                newName: "TotalDieselConsumed");

            migrationBuilder.RenameColumn(
                name: "AverageSpeedKnots",
                table: "arrival_reports",
                newName: "AverageSpeed");

            migrationBuilder.RenameColumn(
                name: "ArrivalLon",
                table: "arrival_reports",
                newName: "ArrivalLongitude");

            migrationBuilder.RenameColumn(
                name: "ArrivalLat",
                table: "arrival_reports",
                newName: "ArrivalLatitude");

            migrationBuilder.RenameColumn(
                name: "ArrivalDateTimeLocal",
                table: "arrival_reports",
                newName: "PilotOnBoardTime");

            migrationBuilder.AddColumn<double>(
                name: "CargoOnBoard",
                table: "position_reports",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewOnBoard",
                table: "position_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPort",
                table: "position_reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportReason",
                table: "position_reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewOnBoard",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceRemarks",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassengersOnBoard",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyDrillsConducted",
                table: "noon_reports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyIncidents",
                table: "noon_reports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NextPort",
                table: "departure_reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewOnBoard",
                table: "departure_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedTimeOfArrival",
                table: "departure_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoyageId",
                table: "departure_reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewOnBoard",
                table: "arrival_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstLineAshoreTime",
                table: "arrival_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoyageId",
                table: "arrival_reports",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoOnBoard",
                table: "position_reports");

            migrationBuilder.DropColumn(
                name: "CrewOnBoard",
                table: "position_reports");

            migrationBuilder.DropColumn(
                name: "LastPort",
                table: "position_reports");

            migrationBuilder.DropColumn(
                name: "ReportReason",
                table: "position_reports");

            migrationBuilder.DropColumn(
                name: "CrewOnBoard",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "MaintenanceRemarks",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "PassengersOnBoard",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "SafetyDrillsConducted",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "SafetyIncidents",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "CrewOnBoard",
                table: "departure_reports");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeOfArrival",
                table: "departure_reports");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "departure_reports");

            migrationBuilder.DropColumn(
                name: "CrewOnBoard",
                table: "arrival_reports");

            migrationBuilder.DropColumn(
                name: "FirstLineAshoreTime",
                table: "arrival_reports");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "arrival_reports");

            migrationBuilder.RenameColumn(
                name: "PortCode",
                table: "departure_reports",
                newName: "PortLocode");

            migrationBuilder.RenameColumn(
                name: "PilotOnBoardTime",
                table: "departure_reports",
                newName: "ETA");

            migrationBuilder.RenameColumn(
                name: "PassengersOnBoard",
                table: "departure_reports",
                newName: "PersonsOnBoard");

            migrationBuilder.RenameColumn(
                name: "NextPortCode",
                table: "departure_reports",
                newName: "NextPortLocode");

            migrationBuilder.RenameColumn(
                name: "LastLineAshoreTime",
                table: "departure_reports",
                newName: "DepartureDateTimeLocal");

            migrationBuilder.RenameColumn(
                name: "DraftForward",
                table: "departure_reports",
                newName: "DraftFore");

            migrationBuilder.RenameColumn(
                name: "DepartureLongitude",
                table: "departure_reports",
                newName: "DepartureLon");

            migrationBuilder.RenameColumn(
                name: "DepartureLatitude",
                table: "departure_reports",
                newName: "DepartureLat");

            migrationBuilder.RenameColumn(
                name: "VoyageDuration",
                table: "arrival_reports",
                newName: "VoyageDurationHours");

            migrationBuilder.RenameColumn(
                name: "TotalFuelConsumed",
                table: "arrival_reports",
                newName: "FuelOilConsumed");

            migrationBuilder.RenameColumn(
                name: "TotalDieselConsumed",
                table: "arrival_reports",
                newName: "DieselOilConsumed");

            migrationBuilder.RenameColumn(
                name: "PortCode",
                table: "arrival_reports",
                newName: "PortLocode");

            migrationBuilder.RenameColumn(
                name: "PilotOnBoardTime",
                table: "arrival_reports",
                newName: "ArrivalDateTimeLocal");

            migrationBuilder.RenameColumn(
                name: "PassengersOnBoard",
                table: "arrival_reports",
                newName: "PersonsOnBoard");

            migrationBuilder.RenameColumn(
                name: "DraftForward",
                table: "arrival_reports",
                newName: "DraftFore");

            migrationBuilder.RenameColumn(
                name: "AverageSpeed",
                table: "arrival_reports",
                newName: "AverageSpeedKnots");

            migrationBuilder.RenameColumn(
                name: "ArrivalLongitude",
                table: "arrival_reports",
                newName: "ArrivalLon");

            migrationBuilder.RenameColumn(
                name: "ArrivalLatitude",
                table: "arrival_reports",
                newName: "ArrivalLat");

            migrationBuilder.AlterColumn<string>(
                name: "NextPort",
                table: "departure_reports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "departure_reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "arrival_reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
