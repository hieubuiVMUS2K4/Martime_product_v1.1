using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageMirrorPlanningCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    PortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "voyage_plan_legs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    LegType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    FromPortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    ToPortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlannedDepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedDistance = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    PlannedDurationHours = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    PlannedAverageSpeed = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    CargoActivity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CrewChangePlanned = table.Column<bool>(type: "boolean", nullable: false),
                    BunkerSupplyPlanned = table.Column<bool>(type: "boolean", nullable: false),
                    PlannedFuelConsumption = table.Column<double>(type: "double precision", precision: 10, scale: 3, nullable: true),
                    WeatherRoutingNotes = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_plan_legs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_plan_legs_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_status_history_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_PortId",
                table: "PortCalls",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_VoyagePlanLegId",
                table: "PortCalls",
                column: "VoyagePlanLegId");

            migrationBuilder.CreateIndex(
                name: "IX_ports_CountryCode",
                table: "ports",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_ports_IsActive",
                table: "ports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ports_IsSynced",
                table: "ports",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_ports_PortCode",
                table: "ports",
                column: "PortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ports_PortName",
                table: "ports",
                column: "PortName");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_plan_legs_IsSynced",
                table: "voyage_plan_legs",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_plan_legs_LegType",
                table: "voyage_plan_legs",
                column: "LegType");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_plan_legs_VoyageId_Sequence",
                table: "voyage_plan_legs",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_status_history_IsSynced",
                table: "voyage_status_history",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_status_history_ToStatus",
                table: "voyage_status_history",
                column: "ToStatus");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_status_history_VoyageId_ChangedAt",
                table: "voyage_status_history",
                columns: new[] { "VoyageId", "ChangedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_PortCalls_ports_PortId",
                table: "PortCalls",
                column: "PortId",
                principalTable: "ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PortCalls_voyage_plan_legs_VoyagePlanLegId",
                table: "PortCalls",
                column: "VoyagePlanLegId",
                principalTable: "voyage_plan_legs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortCalls_ports_PortId",
                table: "PortCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_PortCalls_voyage_plan_legs_VoyagePlanLegId",
                table: "PortCalls");

            migrationBuilder.DropTable(
                name: "ports");

            migrationBuilder.DropTable(
                name: "voyage_plan_legs");

            migrationBuilder.DropTable(
                name: "voyage_status_history");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_PortId",
                table: "PortCalls");

            migrationBuilder.DropIndex(
                name: "IX_PortCalls_VoyagePlanLegId",
                table: "PortCalls");
        }
    }
}
