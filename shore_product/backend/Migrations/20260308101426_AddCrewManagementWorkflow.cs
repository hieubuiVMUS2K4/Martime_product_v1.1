using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewManagementWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PoolStatus",
                table: "crew_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "crew_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusChangedBy",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Actor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceChannel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BeforeState = table.Column<string>(type: "text", nullable: true),
                    AfterState = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "crew_document_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingCountryId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActiveSubmission = table.Column<bool>(type: "boolean", nullable: false),
                    SensitivityLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_document_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_document_submissions_countries_IssuingCountryId",
                        column: x => x.IssuingCountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crew_document_submissions_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_status_history_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceVesselId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceVesselName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselGroupCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FlagState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboarding_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_onboarding_cases_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew_document_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileChecksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActiveVersion = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_document_versions_crew_document_submissions_Submission~",
                        column: x => x.SubmissionId,
                        principalTable: "crew_document_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequiredDocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequiredCertificateId = table.Column<int>(type: "integer", nullable: true),
                    SourceRuleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WaivedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WaiverReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboarding_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_onboarding_checklist_items_onboarding_cases_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalTable: "onboarding_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_verification_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_verification_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_verification_tasks_crew_document_submissions_Submi~",
                        column: x => x.SubmissionId,
                        principalTable: "crew_document_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_verification_tasks_crew_document_versions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "crew_document_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_verification_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_verification_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_verification_actions_document_verification_tasks_T~",
                        column: x => x.TaskId,
                        principalTable: "document_verification_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Actor",
                table: "audit_logs",
                column: "Actor");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CorrelationId",
                table: "audit_logs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Timestamp",
                table: "audit_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_submissions_CrewMemberId",
                table: "crew_document_submissions",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_submissions_CrewMemberId_DocumentType_IsActiv~",
                table: "crew_document_submissions",
                columns: new[] { "CrewMemberId", "DocumentType", "IsActiveSubmission" });

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_submissions_IssuingCountryId",
                table: "crew_document_submissions",
                column: "IssuingCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_submissions_Status",
                table: "crew_document_submissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_versions_SubmissionId",
                table: "crew_document_versions",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_document_versions_SubmissionId_IsActiveVersion",
                table: "crew_document_versions",
                columns: new[] { "SubmissionId", "IsActiveVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_crew_status_history_ChangedAt",
                table: "crew_status_history",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_crew_status_history_CrewMemberId",
                table: "crew_status_history",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_actions_TaskId",
                table: "document_verification_actions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_tasks_AssignedTo",
                table: "document_verification_tasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_tasks_DueAt",
                table: "document_verification_tasks",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_tasks_Status_Priority",
                table: "document_verification_tasks",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_tasks_SubmissionId",
                table: "document_verification_tasks",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_document_verification_tasks_VersionId",
                table: "document_verification_tasks",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_cases_CreatedAt",
                table: "onboarding_cases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_cases_CrewMemberId",
                table: "onboarding_cases",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_cases_Status",
                table: "onboarding_cases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_checklist_items_OnboardingCaseId",
                table: "onboarding_checklist_items",
                column: "OnboardingCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_checklist_items_OnboardingCaseId_Status",
                table: "onboarding_checklist_items",
                columns: new[] { "OnboardingCaseId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "crew_status_history");

            migrationBuilder.DropTable(
                name: "document_verification_actions");

            migrationBuilder.DropTable(
                name: "onboarding_checklist_items");

            migrationBuilder.DropTable(
                name: "document_verification_tasks");

            migrationBuilder.DropTable(
                name: "onboarding_cases");

            migrationBuilder.DropTable(
                name: "crew_document_versions");

            migrationBuilder.DropTable(
                name: "crew_document_submissions");

            migrationBuilder.DropColumn(
                name: "PoolStatus",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "StatusChangedBy",
                table: "crew_members");
        }
    }
}
