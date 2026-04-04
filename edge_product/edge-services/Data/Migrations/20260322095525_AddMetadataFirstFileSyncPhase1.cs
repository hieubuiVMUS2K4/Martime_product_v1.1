using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataFirstFileSyncPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_file_manifests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    table_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transfer_priority = table.Column<int>(type: "integer", nullable: false),
                    transfer_status = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    captured_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sync_file_manifests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sync_file_transfer_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    supplier_node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sync_file_transfer_requests", x => x.id);
                    table.ForeignKey(
                        name: "f_k_sync_file_transfer_requests_sync_file_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalSchema: "public",
                        principalTable: "sync_file_manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_manifest_id",
                schema: "public",
                table: "sync_file_manifests",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_manifest_owner_record",
                schema: "public",
                table: "sync_file_manifests",
                columns: new[] { "owner_node_id", "table_name", "record_key" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_manifest_receiver_status",
                schema: "public",
                table: "sync_file_manifests",
                columns: new[] { "receiver_node_id", "transfer_status" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_manifest_sha256",
                schema: "public",
                table: "sync_file_manifests",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_request_requester_status",
                schema: "public",
                table: "sync_file_transfer_requests",
                columns: new[] { "requester_node_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_file_request_supplier_status",
                schema: "public",
                table: "sync_file_transfer_requests",
                columns: new[] { "supplier_node_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_transfer_requests_manifest_id",
                schema: "public",
                table: "sync_file_transfer_requests",
                column: "manifest_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_file_transfer_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sync_file_manifests",
                schema: "public");
        }
    }
}
