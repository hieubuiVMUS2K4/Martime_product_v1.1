using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncFileChunkSessionsPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_file_chunk_sessions");
        }
    }
}
