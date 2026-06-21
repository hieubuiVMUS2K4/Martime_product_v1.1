using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHsqeAttachmentsAndLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hsqe_document_attachments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uploaded_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_document_attachments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_hsqe_document_attachments_hsqe_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "public",
                        principalTable: "hsqe_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hsqe_document_read_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rank = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_hsqe_document_read_logs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_hsqe_document_read_logs_hsqe_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "public",
                        principalTable: "hsqe_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_hsqe_document_attachments_document_id",
                schema: "public",
                table: "hsqe_document_attachments",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_hsqe_document_read_logs_document_id",
                schema: "public",
                table: "hsqe_document_read_logs",
                column: "document_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hsqe_document_attachments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hsqe_document_read_logs",
                schema: "public");

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
        }
    }
}
