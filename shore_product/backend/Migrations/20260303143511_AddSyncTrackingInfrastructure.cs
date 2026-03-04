using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncTrackingInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_idempotency_records",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_idempotency_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_node_trackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShipName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ImoNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastPushAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalReceivedCount = table.Column<long>(type: "bigint", nullable: false),
                    LastPushBatchSize = table.Column<int>(type: "integer", nullable: false),
                    LastReceivedVersion = table.Column<long>(type: "bigint", nullable: false),
                    LastPullAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalDeliveredCount = table.Column<long>(type: "bigint", nullable: false),
                    PendingOutboxCount = table.Column<int>(type: "integer", nullable: false),
                    LastAcknowledgedId = table.Column<long>(type: "bigint", nullable: false),
                    LastHeartbeatAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentNetworkType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastErrorAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_node_trackers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_table_stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalSynced = table.Column<long>(type: "bigint", nullable: false),
                    TotalConflicts = table.Column<long>(type: "bigint", nullable: false),
                    TotalFailed = table.Column<long>(type: "bigint", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SnapshotAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_table_stats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sync_idempotency_records_IdempotencyKey",
                table: "sync_idempotency_records",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sync_idempotency_records_ProcessedAt",
                table: "sync_idempotency_records",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_IsOnline",
                table: "sync_node_trackers",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_NodeId",
                table: "sync_node_trackers",
                column: "NodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sync_table_stats_NodeId_TableName",
                table: "sync_table_stats",
                columns: new[] { "NodeId", "TableName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_idempotency_records");

            migrationBuilder.DropTable(
                name: "sync_node_trackers");

            migrationBuilder.DropTable(
                name: "sync_table_stats");
        }
    }
}
