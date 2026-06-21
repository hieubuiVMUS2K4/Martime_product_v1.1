using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHsqeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hsqe_documents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    current_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    obsolete_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_controlled = table.Column<bool>(type: "boolean", nullable: false),
                    watermark_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    digital_signature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    edit_count = table.Column<int>(type: "integer", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_incidents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    incident_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vessel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    occurrence_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    immediate_actions = table.Column<string>(type: "text", nullable: true),
                    root_cause = table.Column<string>(type: "text", nullable: true),
                    why1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    why2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    why3 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    why4 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    why5 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_incidents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_risk_assessments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    job_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    assessment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    steps_json = table.Column<string>(type: "text", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_risk_assessments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_work_permits",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    permit_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vessel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    duration_hours = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    risk_assessment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    gas_test_o2 = table.Column<double>(type: "double precision", nullable: false),
                    gas_test_l_e_l = table.Column<double>(type: "double precision", nullable: false),
                    gas_test_c_o = table.Column<double>(type: "double precision", nullable: false),
                    gas_test_h2_s = table.Column<double>(type: "double precision", nullable: false),
                    precautions_json = table.Column<string>(type: "text", nullable: false),
                    chief_officer_signed = table.Column<bool>(type: "boolean", nullable: false),
                    captain_approved = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_work_permits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_document_revisions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    change_summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_snapshot = table.Column<string>(type: "text", nullable: false),
                    diff_content = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_document_revisions", x => x.id);
                    table.ForeignKey(
                        name: "f_k_hsqe_document_revisions_hsqe_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "public",
                        principalTable: "hsqe_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_document_sync_statuses",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    received = table.Column<bool>(type: "boolean", nullable: false),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trained = table.Column<bool>(type: "boolean", nullable: false),
                    trained_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_document_sync_statuses", x => x.id);
                    table.ForeignKey(
                        name: "f_k_hsqe_document_sync_statuses_hsqe_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "public",
                        principalTable: "hsqe_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_capas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capa_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    assignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed = table.Column<bool>(type: "boolean", nullable: false),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_details = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_capas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_hsqe_capas__hsqe_incidents_incident_id",
                        column: x => x.incident_id,
                        principalSchema: "public",
                        principalTable: "hsqe_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7477), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7479) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7481), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7481) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7482), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7483) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7484), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7484) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7485), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7485) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7486), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7487) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7488), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7488) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7489), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7489) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7490), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7491) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7491), new DateTime(2026, 6, 20, 14, 54, 0, 955, DateTimeKind.Utc).AddTicks(7492) });

            migrationBuilder.CreateIndex(
                name: "IX_hsqe_capas_incident_id",
                schema: "public",
                table: "hsqe_capas",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_hsqe_document_revisions_document_id",
                schema: "public",
                table: "hsqe_document_revisions",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_hsqe_document_sync_statuses_document_id",
                schema: "public",
                table: "hsqe_document_sync_statuses",
                column: "document_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hsqe_capas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_document_revisions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_document_sync_statuses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_risk_assessments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_work_permits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_incidents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_documents",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8231), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8232) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8234), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8234) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8235), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8235) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8257), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8257) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8258), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8258) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8259), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8260) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8260), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8261) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8262), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8262) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8263), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8263) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8264), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8264) });
        }
    }
}
