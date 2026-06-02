using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewLogbookEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crew_logbook_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ShoreActivity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TrainingCourse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShoreLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Supervisor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WatchDuty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NavigationPhase = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IncidentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeatherConditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VesselPosition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperationalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EdgeDeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EdgeLocalCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_logbook_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_logbook_entries_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crew_logbook_entries_CrewMemberId",
                table: "crew_logbook_entries",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_logbook_entries_EntryDate",
                table: "crew_logbook_entries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_crew_logbook_entries_EntryOrigin",
                table: "crew_logbook_entries",
                column: "EntryOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_crew_logbook_entries_IsSynced",
                table: "crew_logbook_entries",
                column: "IsSynced");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crew_logbook_entries");
        }
    }
}
