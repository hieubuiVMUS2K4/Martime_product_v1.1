using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataFirstFileSyncPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_file_manifests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceiverNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourcePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransferPriority = table.Column<int>(type: "integer", nullable: false),
                    TransferStatus = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRequestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_file_manifests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_file_transfer_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_file_transfer_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sync_file_transfer_requests_sync_file_manifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "sync_file_manifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_manifests_OwnerNodeId_TableName_RecordKey",
                table: "sync_file_manifests",
                columns: new[] { "OwnerNodeId", "TableName", "RecordKey" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_manifests_ReceiverNodeId_TransferStatus",
                table: "sync_file_manifests",
                columns: new[] { "ReceiverNodeId", "TransferStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_manifests_Sha256",
                table: "sync_file_manifests",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_transfer_requests_ManifestId",
                table: "sync_file_transfer_requests",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_transfer_requests_RequesterNodeId_Status",
                table: "sync_file_transfer_requests",
                columns: new[] { "RequesterNodeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_transfer_requests_SupplierNodeId_Status",
                table: "sync_file_transfer_requests",
                columns: new[] { "SupplierNodeId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_file_transfer_requests");

            migrationBuilder.DropTable(
                name: "sync_file_manifests");
        }
    }
}
