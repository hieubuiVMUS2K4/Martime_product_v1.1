using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliance_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Authority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliance_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverallResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TotalRules = table.Column<int>(type: "integer", nullable: false),
                    RulesMet = table.Column<int>(type: "integer", nullable: false),
                    RulesNotMet = table.Column<int>(type: "integer", nullable: false),
                    RulesWarning = table.Column<int>(type: "integer", nullable: false),
                    RulesWaived = table.Column<int>(type: "integer", nullable: false),
                    EvaluationDetails = table.Column<string>(type: "jsonb", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EvaluationStage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NextExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliance_snapshots_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RequirementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiredCertificateId = table.Column<int>(type: "integer", nullable: true),
                    RequiredDocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvaluationStage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MinDaysBeforeExpiry = table.Column<int>(type: "integer", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "integer", nullable: true),
                    RenewWindowDays = table.Column<int>(type: "integer", nullable: true),
                    WaiverAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    WaiverApproverRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AllowEquivalent = table.Column<bool>(type: "boolean", nullable: false),
                    EquivalentCertificateIds = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UiMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExplainabilityText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliance_rules_compliance_rule_sets_RuleSetId",
                        column: x => x.RuleSetId,
                        principalTable: "compliance_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_dimensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Operator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_dimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliance_dimensions_compliance_rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "compliance_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_waivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_waivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliance_waivers_compliance_rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "compliance_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_compliance_waivers_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_dimensions_DimensionType_Value",
                table: "compliance_dimensions",
                columns: new[] { "DimensionType", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_dimensions_RuleId",
                table: "compliance_dimensions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rule_sets_Code",
                table: "compliance_rule_sets",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rule_sets_IsActive",
                table: "compliance_rule_sets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_EvaluationStage",
                table: "compliance_rules",
                column: "EvaluationStage");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_IsActive",
                table: "compliance_rules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_RequiredCertificateId",
                table: "compliance_rules",
                column: "RequiredCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_RuleSetId",
                table: "compliance_rules",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_Severity",
                table: "compliance_rules",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_CrewMemberId",
                table: "compliance_snapshots",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_CrewMemberId_VesselId",
                table: "compliance_snapshots",
                columns: new[] { "CrewMemberId", "VesselId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_EvaluatedAt",
                table: "compliance_snapshots",
                column: "EvaluatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_OverallResult",
                table: "compliance_snapshots",
                column: "OverallResult");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_waivers_CrewMemberId",
                table: "compliance_waivers",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_waivers_CrewMemberId_RuleId_Status",
                table: "compliance_waivers",
                columns: new[] { "CrewMemberId", "RuleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_waivers_RuleId",
                table: "compliance_waivers",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_waivers_Status",
                table: "compliance_waivers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliance_dimensions");

            migrationBuilder.DropTable(
                name: "compliance_snapshots");

            migrationBuilder.DropTable(
                name: "compliance_waivers");

            migrationBuilder.DropTable(
                name: "compliance_rules");

            migrationBuilder.DropTable(
                name: "compliance_rule_sets");
        }
    }
}
