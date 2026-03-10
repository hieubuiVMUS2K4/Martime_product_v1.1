using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddMaritimeReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportTypes",
                table: "ReportTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NoonReports",
                table: "NoonReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaritimeReports",
                table: "MaritimeReports");

            migrationBuilder.RenameTable(
                name: "ReportTypes",
                newName: "report_types");

            migrationBuilder.RenameTable(
                name: "NoonReports",
                newName: "noon_reports");

            migrationBuilder.RenameTable(
                name: "MaritimeReports",
                newName: "maritime_reports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_report_types",
                table: "report_types",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_noon_reports",
                table: "noon_reports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_maritime_reports",
                table: "maritime_reports",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "arrival_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaritimeReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortLocode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ArrivalDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalDateTimeLocal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ArrivalLat = table.Column<double>(type: "double precision", nullable: true),
                    ArrivalLon = table.Column<double>(type: "double precision", nullable: true),
                    VoyageDistance = table.Column<double>(type: "double precision", nullable: true),
                    VoyageDurationHours = table.Column<double>(type: "double precision", nullable: true),
                    AverageSpeedKnots = table.Column<double>(type: "double precision", nullable: true),
                    DraftFore = table.Column<double>(type: "double precision", nullable: true),
                    DraftAft = table.Column<double>(type: "double precision", nullable: true),
                    DraftMidship = table.Column<double>(type: "double precision", nullable: true),
                    FuelOilConsumed = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilConsumed = table.Column<double>(type: "double precision", nullable: true),
                    FuelOilROB = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilROB = table.Column<double>(type: "double precision", nullable: true),
                    LubOilROB = table.Column<double>(type: "double precision", nullable: true),
                    FreshWaterROB = table.Column<double>(type: "double precision", nullable: true),
                    CargoOnBoard = table.Column<double>(type: "double precision", nullable: true),
                    CargoDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PersonsOnBoard = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arrival_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "departure_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaritimeReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortLocode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DepartureDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepartureDateTimeLocal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DepartureLat = table.Column<double>(type: "double precision", nullable: true),
                    DepartureLon = table.Column<double>(type: "double precision", nullable: true),
                    DraftFore = table.Column<double>(type: "double precision", nullable: true),
                    DraftAft = table.Column<double>(type: "double precision", nullable: true),
                    DraftMidship = table.Column<double>(type: "double precision", nullable: true),
                    FuelOilROB = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilROB = table.Column<double>(type: "double precision", nullable: true),
                    LubOilROB = table.Column<double>(type: "double precision", nullable: true),
                    FreshWaterROB = table.Column<double>(type: "double precision", nullable: true),
                    DistanceToNextPort = table.Column<double>(type: "double precision", nullable: true),
                    ETA = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextPort = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NextPortLocode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CargoOnBoard = table.Column<double>(type: "double precision", nullable: true),
                    CargoDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PersonsOnBoard = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departure_reports", x => x.Id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO report_types (""Id"", ""Category"", ""CreatedAt"", ""Description"", ""Frequency"", ""IsActive"", ""IsMandatory"", ""RegulationReference"", ""RequiresMasterSignature"", ""TemplateSchema"", ""TypeCode"", ""TypeName"")
                VALUES
                    (1, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'DAILY', TRUE, TRUE, 'SOLAS V/28', TRUE, NULL, 'NOON', 'Noon Report'),
                    (2, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'EVENT_BASED', TRUE, TRUE, NULL, TRUE, NULL, 'DEPARTURE', 'Departure Report'),
                    (3, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'EVENT_BASED', TRUE, TRUE, NULL, TRUE, NULL, 'ARRIVAL', 'Arrival Report'),
                    (4, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'DAILY', TRUE, FALSE, NULL, FALSE, NULL, 'DAILY', 'Daily Report'),
                    (5, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'EVENT_BASED', TRUE, FALSE, NULL, FALSE, NULL, 'BUNKER', 'Bunker Report'),
                    (6, 'VOYAGE', '2026-01-01T00:00:00Z', NULL, 'EVENT_BASED', TRUE, FALSE, NULL, FALSE, NULL, 'POSITION', 'Position Report')
                ON CONFLICT (""Id"") DO NOTHING;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_report_types_TypeCode",
                table: "report_types",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_noon_reports_MaritimeReportId",
                table: "noon_reports",
                column: "MaritimeReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maritime_reports_OriginNode",
                table: "maritime_reports",
                column: "OriginNode");

            migrationBuilder.CreateIndex(
                name: "IX_maritime_reports_ReportDateTime",
                table: "maritime_reports",
                column: "ReportDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_maritime_reports_ReportNumber",
                table: "maritime_reports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maritime_reports_Status",
                table: "maritime_reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_arrival_reports_MaritimeReportId",
                table: "arrival_reports",
                column: "MaritimeReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departure_reports_MaritimeReportId",
                table: "departure_reports",
                column: "MaritimeReportId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arrival_reports");

            migrationBuilder.DropTable(
                name: "departure_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_report_types",
                table: "report_types");

            migrationBuilder.DropIndex(
                name: "IX_report_types_TypeCode",
                table: "report_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_noon_reports",
                table: "noon_reports");

            migrationBuilder.DropIndex(
                name: "IX_noon_reports_MaritimeReportId",
                table: "noon_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_maritime_reports",
                table: "maritime_reports");

            migrationBuilder.DropIndex(
                name: "IX_maritime_reports_OriginNode",
                table: "maritime_reports");

            migrationBuilder.DropIndex(
                name: "IX_maritime_reports_ReportDateTime",
                table: "maritime_reports");

            migrationBuilder.DropIndex(
                name: "IX_maritime_reports_ReportNumber",
                table: "maritime_reports");

            migrationBuilder.DropIndex(
                name: "IX_maritime_reports_Status",
                table: "maritime_reports");

            migrationBuilder.Sql(@"DELETE FROM report_types WHERE ""Id"" IN (1,2,3,4,5,6);");

            migrationBuilder.RenameTable(
                name: "report_types",
                newName: "ReportTypes");

            migrationBuilder.RenameTable(
                name: "noon_reports",
                newName: "NoonReports");

            migrationBuilder.RenameTable(
                name: "maritime_reports",
                newName: "MaritimeReports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportTypes",
                table: "ReportTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoonReports",
                table: "NoonReports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaritimeReports",
                table: "MaritimeReports",
                column: "Id");
        }
    }
}
