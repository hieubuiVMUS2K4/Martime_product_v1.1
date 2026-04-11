using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncNonceRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "sync_dlq_items");

            migrationBuilder.DropTable(
                name: "sync_nonce_registry");
        }
    }
}
