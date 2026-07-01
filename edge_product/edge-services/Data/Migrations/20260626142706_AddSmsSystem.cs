using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ism_elements",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    chapter_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ism_elements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sms_procedures",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ism_element_id = table.Column<int>(type: "integer", nullable: false),
                    procedure_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    publish_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    obsolete_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    watermark_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sms_procedures", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sms_procedures_ism_elements_ism_element_id",
                        column: x => x.ism_element_id,
                        principalSchema: "public",
                        principalTable: "ism_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_form_templates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sms_procedure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content_schema = table.Column<string>(type: "text", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sms_form_templates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sms_form_templates__sms_procedures_sms_procedure_id",
                        column: x => x.sms_procedure_id,
                        principalSchema: "public",
                        principalTable: "sms_procedures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_procedure_acknowledgements",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sms_procedure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rank = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sms_procedure_acknowledgements", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sms_procedure_acknowledgements_sms_procedures_sms_procedure~",
                        column: x => x.sms_procedure_id,
                        principalSchema: "public",
                        principalTable: "sms_procedures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_filled_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sms_form_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vessel_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    filled_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    filled_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    filled_data = table.Column<string>(type: "text", nullable: false),
                    digital_signatures = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sms_filled_records", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sms_filled_records__sms_form_templates_sms_form_template_id",
                        column: x => x.sms_form_template_id,
                        principalSchema: "public",
                        principalTable: "sms_form_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3801), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3802) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3803), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3803) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3804), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3804) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3805), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3805) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3806), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3807) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3807), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3808) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3809), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3809) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3810), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3810) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3811), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3811) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3812), new DateTime(2026, 6, 26, 14, 27, 3, 157, DateTimeKind.Utc).AddTicks(3812) });

            migrationBuilder.CreateIndex(
                name: "IX_sms_filled_records_sms_form_template_id",
                schema: "public",
                table: "sms_filled_records",
                column: "sms_form_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_sms_form_templates_sms_procedure_id",
                schema: "public",
                table: "sms_form_templates",
                column: "sms_procedure_id");

            migrationBuilder.CreateIndex(
                name: "IX_sms_procedure_acknowledgements_sms_procedure_id",
                schema: "public",
                table: "sms_procedure_acknowledgements",
                column: "sms_procedure_id");

            migrationBuilder.CreateIndex(
                name: "IX_sms_procedures_ism_element_id",
                schema: "public",
                table: "sms_procedures",
                column: "ism_element_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_filled_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sms_procedure_acknowledgements",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sms_form_templates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sms_procedures",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ism_elements",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(508), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(510) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(511), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(512) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(512), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(513) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(514), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(514) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(515), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(515) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(516), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(516) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(517), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(517) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(518), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(519) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(519), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(520) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(521), new DateTime(2026, 6, 21, 5, 5, 15, 251, DateTimeKind.Utc).AddTicks(521) });
        }
    }
}
