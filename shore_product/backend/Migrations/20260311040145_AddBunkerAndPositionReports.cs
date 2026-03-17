using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddBunkerAndPositionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bunker_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaritimeReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    BunkerDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BDNNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FuelType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FuelGrade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QuantityReceived = table.Column<double>(type: "double precision", nullable: true),
                    Density = table.Column<double>(type: "double precision", nullable: true),
                    SulphurContent = table.Column<double>(type: "double precision", nullable: true),
                    Viscosity = table.Column<double>(type: "double precision", nullable: true),
                    FlashPoint = table.Column<double>(type: "double precision", nullable: true),
                    ROBefore = table.Column<double>(type: "double precision", nullable: true),
                    ROBAfter = table.Column<double>(type: "double precision", nullable: true),
                    TanksLoaded = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SealNumbers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ChiefEngineerSignature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bunker_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "position_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaritimeReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", nullable: true),
                    SpeedOverGround = table.Column<double>(type: "double precision", nullable: true),
                    WeatherConditions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeaState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WindSpeed = table.Column<double>(type: "double precision", nullable: true),
                    WindDirection = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FuelOilROB = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilROB = table.Column<double>(type: "double precision", nullable: true),
                    NextPort = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ETA = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DistanceToGo = table.Column<double>(type: "double precision", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bunker_reports_MaritimeReportId",
                table: "bunker_reports",
                column: "MaritimeReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_reports_MaritimeReportId",
                table: "position_reports",
                column: "MaritimeReportId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bunker_reports");

            migrationBuilder.DropTable(
                name: "position_reports");
        }
    }
}
