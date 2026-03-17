using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageSyncCoreMirror : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VoyageRecords",
                table: "VoyageRecords");

            migrationBuilder.RenameTable(
                name: "VoyageRecords",
                newName: "voyage_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "VesselId",
                table: "PortCalls",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "BerthNumber",
                table: "PortCalls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallType",
                table: "PortCalls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CargoOpsCompleted",
                table: "PortCalls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "PortCalls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PortCalls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "DraftAft",
                table: "PortCalls",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DraftFore",
                table: "PortCalls",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "PortCalls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginNode",
                table: "PortCalls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PilotOffBoard",
                table: "PortCalls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PilotOnBoard",
                table: "PortCalls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PortId",
                table: "PortCalls",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "PortCalls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "PortCalls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SyncVersion",
                table: "PortCalls",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PortCalls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "VoyageId",
                table: "PortCalls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoyagePlanLegId",
                table: "PortCalls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlarmSummaryJson",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CertificatesExpiringSoon",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceSummaryJson",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ActualProfitMargin",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrivalPortCode",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallSign",
                table: "voyage_records",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CharterType",
                table: "voyage_records",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommencedAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeparturePortCode",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedProfitMargin",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinancialClosedAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialClosedBy",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialStatus",
                table: "voyage_records",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "voyage_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "OutstandingBalance",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedAverageSpeed",
                table: "voyage_records",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedDistance",
                table: "voyage_records",
                type: "double precision",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedDurationHours",
                table: "voyage_records",
                type: "double precision",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedFuelConsumption",
                table: "voyage_records",
                type: "double precision",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousPortCode",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousPortName",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyAt",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SyncVersion",
                table: "voyage_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "TotalActualCost",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalActualRevenue",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalAdvanced",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalDisbursed",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalEstimatedCost",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalEstimatedRevenue",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselFlag",
                table: "voyage_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselIMO",
                table: "voyage_records",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselName",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoyageInstructions",
                table: "voyage_records",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_voyage_records",
                table: "voyage_records",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "cargo_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: true),
                    VoyagePlanLegId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CargoType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CargoDescription = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", precision: 15, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LoadingPort = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DischargePort = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LoadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DischargedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Shipper = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Consignee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BillOfLading = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SealNumbers = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cargo_operations_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "voyage_crew_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RankId = table.Column<int>(type: "integer", nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmbarkPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    EmbarkPortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EmbarkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisembarkPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    DisembarkPortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DisembarkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WatchSchedule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_crew_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_crew_assignments_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_voyage_crew_assignments_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_crew_assignments_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: true),
                    VoyagePlanLegId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EventDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventDateTimeLocal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    PortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PortLocode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PortCountry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BerthNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DistanceToGo = table.Column<double>(type: "double precision", nullable: true),
                    DistanceFromLast = table.Column<double>(type: "double precision", nullable: true),
                    TotalVoyageDistance = table.Column<double>(type: "double precision", nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", nullable: true),
                    SpeedOverGround = table.Column<double>(type: "double precision", nullable: true),
                    PilotName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PilotStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OfficerOnWatch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MasterSignature = table.Column<string>(type: "text", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_log_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_log_entries_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_CallType",
                table: "PortCalls",
                column: "CallType");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_IsSynced",
                table: "PortCalls",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_PortCode",
                table: "PortCalls",
                column: "PortCode");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_VoyageId_Sequence",
                table: "PortCalls",
                columns: new[] { "VoyageId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_DepartureTime",
                table: "voyage_records",
                column: "DepartureTime");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_IsSynced",
                table: "voyage_records",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_VesselIMO",
                table: "voyage_records",
                column: "VesselIMO");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_VoyageNumber",
                table: "voyage_records",
                column: "VoyageNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_records_VoyageStatus",
                table: "voyage_records",
                column: "VoyageStatus");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_BillOfLading",
                table: "cargo_operations",
                column: "BillOfLading");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_CargoType",
                table: "cargo_operations",
                column: "CargoType");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_IsSynced",
                table: "cargo_operations",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_OperationId",
                table: "cargo_operations",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_Status",
                table: "cargo_operations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_operations_VoyageId",
                table: "cargo_operations",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_CrewMemberId",
                table: "voyage_crew_assignments",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_IsSynced",
                table: "voyage_crew_assignments",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_RankId",
                table: "voyage_crew_assignments",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_Status",
                table: "voyage_crew_assignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_VoyageId_CrewMemberId",
                table: "voyage_crew_assignments",
                columns: new[] { "VoyageId", "CrewMemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_log_entries_EventDateTime",
                table: "voyage_log_entries",
                column: "EventDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_log_entries_EventType",
                table: "voyage_log_entries",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_log_entries_IsSynced",
                table: "voyage_log_entries",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_log_entries_PortLocode",
                table: "voyage_log_entries",
                column: "PortLocode");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_log_entries_VoyageId",
                table: "voyage_log_entries",
                column: "VoyageId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortCalls_voyage_records_VoyageId",
                table: "PortCalls",
                column: "VoyageId",
                principalTable: "voyage_records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortCalls_voyage_records_VoyageId",
                table: "PortCalls");

            migrationBuilder.DropTable(
                name: "cargo_operations");

            migrationBuilder.DropTable(
                name: "voyage_crew_assignments");

            migrationBuilder.DropTable(
                name: "voyage_log_entries");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_CallType",
                table: "PortCalls");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_IsSynced",
                table: "PortCalls");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_PortCode",
                table: "PortCalls");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_VoyageId_Sequence",
                table: "PortCalls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_voyage_records",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "IX_voyage_records_DepartureTime",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "IX_voyage_records_IsSynced",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "IX_voyage_records_VesselIMO",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "IX_voyage_records_VoyageNumber",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "IX_voyage_records_VoyageStatus",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "BerthNumber",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "CallType",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "CargoOpsCompleted",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "DraftAft",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "DraftFore",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "OriginNode",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "PilotOffBoard",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "PilotOnBoard",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "PortId",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "SyncVersion",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "VoyagePlanLegId",
                table: "PortCalls");

            migrationBuilder.DropColumn(
                name: "AlarmSummaryJson",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "CertificatesExpiringSoon",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "MaintenanceSummaryJson",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "ActualProfitMargin",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "ArrivalPortCode",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "ArrivedAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "CallSign",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "CharterType",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "CommencedAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "DeparturePortCode",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "EstimatedProfitMargin",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "FinancialClosedAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "FinancialClosedBy",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "FinancialStatus",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "OutstandingBalance",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PlannedAverageSpeed",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PlannedDistance",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PlannedDurationHours",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PlannedFuelConsumption",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PreviousPortCode",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "PreviousPortName",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "ReadyAt",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "SyncVersion",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalActualCost",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalActualRevenue",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalAdvanced",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalDisbursed",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalEstimatedCost",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "TotalEstimatedRevenue",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "VesselFlag",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "VesselIMO",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "VesselName",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "VoyageInstructions",
                table: "voyage_records");

            migrationBuilder.RenameTable(
                name: "voyage_records",
                newName: "VoyageRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "VesselId",
                table: "PortCalls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VoyageRecords",
                table: "VoyageRecords",
                column: "Id");
        }
    }
}
