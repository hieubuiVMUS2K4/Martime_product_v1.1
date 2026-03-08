using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vessel_manning_standards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_manning_standards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manning_positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManningStandardId = table.Column<Guid>(type: "uuid", nullable: false),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false),
                    AllowEquivalent = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manning_positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_manning_positions_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_manning_positions_vessel_manning_standards_ManningStandardId",
                        column: x => x.ManningStandardId,
                        principalTable: "vessel_manning_standards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    ManningPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinPortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JoinPortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeavePortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LeavePortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsEquivalentRank = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalRankId = table.Column<int>(type: "integer", nullable: true),
                    EquivalentRankJustification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ComplianceResult = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ComplianceEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_assignments_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crew_assignments_manning_positions_ManningPositionId",
                        column: x => x.ManningPositionId,
                        principalTable: "manning_positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crew_assignments_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignment_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_comments_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_confirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Response = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeclineReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_confirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_confirmations_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_conflicts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolutionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_conflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_conflicts_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_status_history_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assignment_comments_AssignmentId",
                table: "assignment_comments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_confirmations_AssignmentId",
                table: "assignment_confirmations",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_conflicts_AssignmentId",
                table: "assignment_conflicts",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_conflicts_AssignmentId_IsResolved",
                table: "assignment_conflicts",
                columns: new[] { "AssignmentId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_assignment_status_history_AssignmentId",
                table: "assignment_status_history",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_CrewMemberId",
                table: "crew_assignments",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_CrewMemberId_Status",
                table: "crew_assignments",
                columns: new[] { "CrewMemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_ManningPositionId",
                table: "crew_assignments",
                column: "ManningPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_PlannedStartDate_PlannedEndDate",
                table: "crew_assignments",
                columns: new[] { "PlannedStartDate", "PlannedEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_RankId",
                table: "crew_assignments",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_Status",
                table: "crew_assignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_VesselId",
                table: "crew_assignments",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_assignments_VesselId_Status",
                table: "crew_assignments",
                columns: new[] { "VesselId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_manning_positions_ManningStandardId",
                table: "manning_positions",
                column: "ManningStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_manning_positions_RankId",
                table: "manning_positions",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_manning_standards_IsActive",
                table: "vessel_manning_standards",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_manning_standards_VesselId",
                table: "vessel_manning_standards",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignment_comments");

            migrationBuilder.DropTable(
                name: "assignment_confirmations");

            migrationBuilder.DropTable(
                name: "assignment_conflicts");

            migrationBuilder.DropTable(
                name: "assignment_status_history");

            migrationBuilder.DropTable(
                name: "crew_assignments");

            migrationBuilder.DropTable(
                name: "manning_positions");

            migrationBuilder.DropTable(
                name: "vessel_manning_standards");
        }
    }
}
