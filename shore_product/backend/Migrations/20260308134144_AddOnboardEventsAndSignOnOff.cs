using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProductApi.Data;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260308134144_AddOnboardEventsAndSignOnOff")]
    public partial class AddOnboardEventsAndSignOnOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crew_access_grants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GrantedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RevokedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_access_grants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_access_grants_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crew_access_grants_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboard_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConfirmedByRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignOffReason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OriginalEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboard_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_onboard_events_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_onboard_events_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_onboard_events_onboard_events_OriginalEventId",
                        column: x => x.OriginalEventId,
                        principalTable: "onboard_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sign_on_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    SignOnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SignedOnBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OnboardEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sign_on_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sign_on_records_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sign_on_records_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sign_on_records_onboard_events_OnboardEventId",
                        column: x => x.OnboardEventId,
                        principalTable: "onboard_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sign_on_records_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sign_off_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    SignOffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SignedOffBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OnboardEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignOnRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sign_off_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sign_off_records_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sign_off_records_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sign_off_records_onboard_events_OnboardEventId",
                        column: x => x.OnboardEventId,
                        principalTable: "onboard_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sign_off_records_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sign_off_records_sign_on_records_SignOnRecordId",
                        column: x => x.SignOnRecordId,
                        principalTable: "sign_on_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crew_access_grants_AssignmentId",
                table: "crew_access_grants",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_access_grants_CrewMemberId",
                table: "crew_access_grants",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_access_grants_Status",
                table: "crew_access_grants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_crew_access_grants_VesselId",
                table: "crew_access_grants",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_AssignmentId",
                table: "onboard_events",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_CrewMemberId",
                table: "onboard_events",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_EventTimestamp",
                table: "onboard_events",
                column: "EventTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_EventType",
                table: "onboard_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_OriginalEventId",
                table: "onboard_events",
                column: "OriginalEventId");

            migrationBuilder.CreateIndex(
                name: "IX_onboard_events_VesselId",
                table: "onboard_events",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_AssignmentId",
                table: "sign_off_records",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_CrewMemberId",
                table: "sign_off_records",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_OnboardEventId",
                table: "sign_off_records",
                column: "OnboardEventId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_RankId",
                table: "sign_off_records",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_SignOffDate",
                table: "sign_off_records",
                column: "SignOffDate");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_SignOnRecordId",
                table: "sign_off_records",
                column: "SignOnRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_off_records_VesselId",
                table: "sign_off_records",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_AssignmentId",
                table: "sign_on_records",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_CrewMemberId",
                table: "sign_on_records",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_OnboardEventId",
                table: "sign_on_records",
                column: "OnboardEventId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_RankId",
                table: "sign_on_records",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_SignOnDate",
                table: "sign_on_records",
                column: "SignOnDate");

            migrationBuilder.CreateIndex(
                name: "IX_sign_on_records_VesselId",
                table: "sign_on_records",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crew_access_grants");

            migrationBuilder.DropTable(
                name: "sign_off_records");

            migrationBuilder.DropTable(
                name: "sign_on_records");

            migrationBuilder.DropTable(
                name: "onboard_events");
        }
    }
}
