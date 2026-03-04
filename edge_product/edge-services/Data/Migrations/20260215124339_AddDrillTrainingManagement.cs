using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDrillTrainingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drill_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drill_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    drill_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    drill_name_local = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    regulation_source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    regulation_period = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    frequency_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    frequency_days = table.Column<int>(type: "integer", nullable: true),
                    trigger_condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    trigger_within_days = table.Column<int>(type: "integer", nullable: true),
                    warning_days_before = table.Column<int>(type: "integer", nullable: false),
                    assigned_to_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    instruction_content = table.Column<string>(type: "text", nullable: true),
                    is_fixed_interval = table.Column<bool>(type: "boolean", nullable: false),
                    is_document_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_secure_history = table.Column<bool>(type: "boolean", nullable: false),
                    is_crew_member_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory_sign_on_evaluation = table.Column<bool>(type: "boolean", nullable: false),
                    has_no_expiry = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drill_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drill_schedules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drill_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vessel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    schedule_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    scheduled_month = table.Column<int>(type: "integer", nullable: false),
                    scheduled_year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    overdue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_to_crew_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_executed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    execution_count = table.Column<int>(type: "integer", nullable: false),
                    timeline_label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_auto_generated = table.Column<bool>(type: "boolean", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drill_schedules", x => x.id);
                    table.ForeignKey(
                        name: "f_k_drill_schedules__drill_types_drill_type_id",
                        column: x => x.drill_type_id,
                        principalSchema: "public",
                        principalTable: "drill_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_drill_schedules_crew_members_assigned_to_crew_id",
                        column: x => x.assigned_to_crew_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "drill_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drill_schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    drill_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    execution_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    execution_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weather_condition = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    overall_assessment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    participants = table.Column<string>(type: "jsonb", nullable: true),
                    total_participants = table.Column<int>(type: "integer", nullable: false),
                    new_crew_count = table.Column<int>(type: "integer", nullable: false),
                    findings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    corrective_actions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    general_remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    lessons_learned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    conducted_by_crew_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_by_crew_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    attachment_urls = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drill_logs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_drill_logs__drill_schedules_drill_schedule_id",
                        column: x => x.drill_schedule_id,
                        principalSchema: "public",
                        principalTable: "drill_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_drill_logs__drill_types_drill_type_id",
                        column: x => x.drill_type_id,
                        principalSchema: "public",
                        principalTable: "drill_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_drill_logs_crew_members_conducted_by_crew_id",
                        column: x => x.conducted_by_crew_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_drill_logs_crew_members_verified_by_crew_id",
                        column: x => x.verified_by_crew_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_conducted_by_crew_id",
                schema: "public",
                table: "drill_logs",
                column: "conducted_by_crew_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_drill_schedule_id",
                schema: "public",
                table: "drill_logs",
                column: "drill_schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_drill_type_id",
                schema: "public",
                table: "drill_logs",
                column: "drill_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_execution_date",
                schema: "public",
                table: "drill_logs",
                column: "execution_date");

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_is_locked",
                schema: "public",
                table: "drill_logs",
                column: "is_locked");

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_log_code",
                schema: "public",
                table: "drill_logs",
                column: "log_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drill_logs_verified_by_crew_id",
                schema: "public",
                table: "drill_logs",
                column: "verified_by_crew_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_assigned_to_crew_id",
                schema: "public",
                table: "drill_schedules",
                column: "assigned_to_crew_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_drill_type_id",
                schema: "public",
                table: "drill_schedules",
                column: "drill_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_due_date",
                schema: "public",
                table: "drill_schedules",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_schedule_code",
                schema: "public",
                table: "drill_schedules",
                column: "schedule_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_scheduled_year_scheduled_month",
                schema: "public",
                table: "drill_schedules",
                columns: new[] { "scheduled_year", "scheduled_month" });

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_status",
                schema: "public",
                table: "drill_schedules",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_drill_schedules_vessel_id",
                schema: "public",
                table: "drill_schedules",
                column: "vessel_id");

            migrationBuilder.CreateIndex(
                name: "IX_drill_types_category_display_order",
                schema: "public",
                table: "drill_types",
                columns: new[] { "category", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_drill_types_drill_code",
                schema: "public",
                table: "drill_types",
                column: "drill_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drill_types_frequency_type",
                schema: "public",
                table: "drill_types",
                column: "frequency_type");

            migrationBuilder.CreateIndex(
                name: "IX_drill_types_is_active",
                schema: "public",
                table: "drill_types",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drill_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "drill_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "drill_types",
                schema: "public");
        }
    }
}
