using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalRequestsAndTravel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    AgencyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AgencyEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false),
                    NationalityPreference = table.Column<string>(type: "text", nullable: true),
                    RequiredByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseSlaDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    MandatoryDocuments = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_external_requests_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_external_requests_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "travel_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TravelType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeparturePort = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ArrivalPort = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    BaggageNotes = table.Column<string>(type: "text", nullable: true),
                    VisaRequirements = table.Column<string>(type: "text", nullable: true),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BookingReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_travel_requests_crew_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "crew_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_travel_requests_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RankId = table.Column<int>(type: "integer", nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComplianceResult = table.Column<string>(type: "text", nullable: true),
                    ProfileSummary = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedBy = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    LinkedCrewMemberId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_external_candidates_crew_members_LinkedCrewMemberId",
                        column: x => x.LinkedCrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_external_candidates_external_requests_ExternalRequestId",
                        column: x => x.ExternalRequestId,
                        principalTable: "external_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_external_candidates_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "external_request_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_request_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_external_request_messages_external_requests_ExternalRequest~",
                        column: x => x.ExternalRequestId,
                        principalTable: "external_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "travel_segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TravelRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceOrder = table.Column<int>(type: "integer", nullable: false),
                    SegmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Origin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Destination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FlightNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_travel_segments_travel_requests_TravelRequestId",
                        column: x => x.TravelRequestId,
                        principalTable: "travel_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "travel_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TravelRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_travel_status_history_travel_requests_TravelRequestId",
                        column: x => x.TravelRequestId,
                        principalTable: "travel_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_candidates_ExternalRequestId",
                table: "external_candidates",
                column: "ExternalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_external_candidates_LinkedCrewMemberId",
                table: "external_candidates",
                column: "LinkedCrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_external_candidates_RankId",
                table: "external_candidates",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_external_candidates_Status",
                table: "external_candidates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_external_request_messages_ExternalRequestId",
                table: "external_request_messages",
                column: "ExternalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_external_requests_AssignmentId",
                table: "external_requests",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_external_requests_RankId",
                table: "external_requests",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_external_requests_Status",
                table: "external_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_external_requests_VesselId",
                table: "external_requests",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_travel_requests_AssignmentId",
                table: "travel_requests",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_travel_requests_CrewMemberId",
                table: "travel_requests",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_travel_requests_Status",
                table: "travel_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_travel_segments_TravelRequestId",
                table: "travel_segments",
                column: "TravelRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_travel_status_history_TravelRequestId",
                table: "travel_status_history",
                column: "TravelRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_candidates");

            migrationBuilder.DropTable(
                name: "external_request_messages");

            migrationBuilder.DropTable(
                name: "travel_segments");

            migrationBuilder.DropTable(
                name: "travel_status_history");

            migrationBuilder.DropTable(
                name: "external_requests");

            migrationBuilder.DropTable(
                name: "travel_requests");
        }
    }
}
