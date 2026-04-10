using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddReportEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRegistered",
                table: "sync_node_trackers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "sync_node_trackers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KeyVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastAcknowledgedKeyVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastKeyRotatedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastKeyVersionAcknowledgedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignedRequestAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousKeyGraceUntil",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousKeyVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSigningKey",
                table: "sync_node_trackers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevokedReason",
                table: "sync_node_trackers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SigningKey",
                table: "sync_node_trackers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentVi = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportEvaluations_noon_reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "noon_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sync_dlq_items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ItemId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ItemContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    ErrorReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    LastFailedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MovedToDlqAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginEdgeNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DiagnosticInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsApprovedForManualReplay = table.Column<bool>(type: "boolean", nullable: false),
                    ManualReviewNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_dlq_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_file_chunk_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierNodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChunkSizeBytes = table.Column<int>(type: "integer", nullable: false),
                    TotalChunks = table.Column<int>(type: "integer", nullable: false),
                    NextChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    CommittedBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsDeltaSession = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedChunkIndexesJson = table.Column<string>(type: "text", nullable: true),
                    ReceiverBaseSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourcePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StagingPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResumeToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_file_chunk_sessions", x => x.Id);
                });

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
                    OriginalSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransportEncoding = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsPreprocessed = table.Column<bool>(type: "boolean", nullable: false),
                    PreprocessProfile = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                name: "sync_nonce_registry",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nonce = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginTimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequesterIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    EndpointPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_nonce_registry", x => x.Id);
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
                    PreferDeltaTransfer = table.Column<bool>(type: "boolean", nullable: false),
                    DeltaBlockSizeBytes = table.Column<int>(type: "integer", nullable: true),
                    ReceiverBaseSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReceiverBlockHashesJson = table.Column<string>(type: "text", nullable: true),
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
                name: "IX_sync_node_trackers_IsRegistered",
                table: "sync_node_trackers",
                column: "IsRegistered");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_IsRevoked",
                table: "sync_node_trackers",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_ReportEvaluations_ReportId",
                table: "ReportEvaluations",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sync_dlq_items_IsApprovedForManualReplay_MovedToDlqAtUtc",
                table: "sync_dlq_items",
                columns: new[] { "IsApprovedForManualReplay", "MovedToDlqAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_dlq_items_MovedToDlqAtUtc",
                table: "sync_dlq_items",
                column: "MovedToDlqAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sync_dlq_items_OriginEdgeNode",
                table: "sync_dlq_items",
                column: "OriginEdgeNode");

            migrationBuilder.CreateIndex(
                name: "IX_sync_dlq_items_Priority",
                table: "sync_dlq_items",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_chunk_sessions_ExpiresAtUtc",
                table: "sync_file_chunk_sessions",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_chunk_sessions_ManifestId_Direction_Status",
                table: "sync_file_chunk_sessions",
                columns: new[] { "ManifestId", "Direction", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_chunk_sessions_RequesterNodeId_SupplierNodeId_Dir~",
                table: "sync_file_chunk_sessions",
                columns: new[] { "RequesterNodeId", "SupplierNodeId", "Direction", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_file_chunk_sessions_ResumeToken",
                table: "sync_file_chunk_sessions",
                column: "ResumeToken",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_sync_nonce_registry_ExpiresAtUtc",
                table: "sync_nonce_registry",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sync_nonce_registry_Nonce",
                table: "sync_nonce_registry",
                column: "Nonce",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sync_nonce_registry_OriginNode_RegisteredAtUtc",
                table: "sync_nonce_registry",
                columns: new[] { "OriginNode", "RegisteredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportEvaluations");

            migrationBuilder.DropTable(
                name: "sync_dlq_items");

            migrationBuilder.DropTable(
                name: "sync_file_chunk_sessions");

            migrationBuilder.DropTable(
                name: "sync_file_transfer_requests");

            migrationBuilder.DropTable(
                name: "sync_nonce_registry");

            migrationBuilder.DropTable(
                name: "sync_file_manifests");

            migrationBuilder.DropIndex(
                name: "IX_sync_node_trackers_IsRegistered",
                table: "sync_node_trackers");

            migrationBuilder.DropIndex(
                name: "IX_sync_node_trackers_IsRevoked",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "IsRegistered",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "KeyVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastAcknowledgedKeyVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastKeyRotatedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastKeyVersionAcknowledgedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastSignedRequestAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousKeyGraceUntil",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousKeyVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousSigningKey",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "SigningKey",
                table: "sync_node_trackers");
        }
    }
}
