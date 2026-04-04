using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncFileChunkSessionsPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_file_chunk_sessions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    supplier_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    table_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    chunk_size_bytes = table.Column<int>(type: "integer", nullable: false),
                    total_chunks = table.Column<int>(type: "integer", nullable: false),
                    next_chunk_index = table.Column<int>(type: "integer", nullable: false),
                    committed_bytes = table.Column<long>(type: "bigint", nullable: false),
                    source_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    staging_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resume_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_activity_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sync_file_chunk_sessions", x => x.id);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6973), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6976) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6977), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6977) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6978), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6978) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6984), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6985) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6985), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6986) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6987), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6987) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6988), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6988) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6989), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6989) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6990), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6990) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6991), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6992) });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_chunk_session_expires",
                schema: "public",
                table: "sync_file_chunk_sessions",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_chunk_session_manifest_status",
                schema: "public",
                table: "sync_file_chunk_sessions",
                columns: new[] { "manifest_id", "direction", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_chunk_session_nodes_status",
                schema: "public",
                table: "sync_file_chunk_sessions",
                columns: new[] { "requester_node_id", "supplier_node_id", "direction", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_chunk_session_resume_token",
                schema: "public",
                table: "sync_file_chunk_sessions",
                column: "resume_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_file_chunk_sessions",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8620), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8623) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8624), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8624) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8625), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8625) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8632), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8632) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8633), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8633) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8634), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8634) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8635), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8636) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8636), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8637) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8638), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8638) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8639), new DateTime(2026, 3, 22, 9, 55, 23, 852, DateTimeKind.Utc).AddTicks(8639) });
        }
    }
}
