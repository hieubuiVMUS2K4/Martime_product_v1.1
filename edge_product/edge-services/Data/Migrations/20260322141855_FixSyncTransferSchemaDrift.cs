using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSyncTransferSchemaDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "delta_block_size_bytes",
                schema: "public",
                table: "sync_file_transfer_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "prefer_delta_transfer",
                schema: "public",
                table: "sync_file_transfer_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "receiver_base_sha256",
                schema: "public",
                table: "sync_file_transfer_requests",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receiver_block_hashes_json",
                schema: "public",
                table: "sync_file_transfer_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_preprocessed",
                schema: "public",
                table: "sync_file_manifests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "original_size_bytes",
                schema: "public",
                table: "sync_file_manifests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preprocess_profile",
                schema: "public",
                table: "sync_file_manifests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transport_encoding",
                schema: "public",
                table: "sync_file_manifests",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_delta_session",
                schema: "public",
                table: "sync_file_chunk_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "receiver_base_sha256",
                schema: "public",
                table: "sync_file_chunk_sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "requested_chunk_indexes_json",
                schema: "public",
                table: "sync_file_chunk_sessions",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7144), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7148) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7149), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7150) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7150), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7151) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7158), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7159) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7160), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7160) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7161), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7162) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7163), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7163) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7166), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7167) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7168), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7168) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7171), new DateTime(2026, 3, 22, 14, 18, 54, 311, DateTimeKind.Utc).AddTicks(7172) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delta_block_size_bytes",
                schema: "public",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "prefer_delta_transfer",
                schema: "public",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "receiver_base_sha256",
                schema: "public",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "receiver_block_hashes_json",
                schema: "public",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "is_preprocessed",
                schema: "public",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "original_size_bytes",
                schema: "public",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "preprocess_profile",
                schema: "public",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "transport_encoding",
                schema: "public",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "is_delta_session",
                schema: "public",
                table: "sync_file_chunk_sessions");

            migrationBuilder.DropColumn(
                name: "receiver_base_sha256",
                schema: "public",
                table: "sync_file_chunk_sessions");

            migrationBuilder.DropColumn(
                name: "requested_chunk_indexes_json",
                schema: "public",
                table: "sync_file_chunk_sessions");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3479), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3482) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3484), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3484) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3485), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3485) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3495), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3495) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3496), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3496) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3497), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3498) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3499), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3499) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3500), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3500) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3501), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3501) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3505), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3505) });
        }
    }
}
