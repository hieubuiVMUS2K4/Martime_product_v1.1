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
            // Safely add columns that may already exist from prior schema drift
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- sync_file_transfer_requests
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_transfer_requests' AND column_name='delta_block_size_bytes') THEN ALTER TABLE public.sync_file_transfer_requests ADD COLUMN delta_block_size_bytes integer; END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_transfer_requests' AND column_name='prefer_delta_transfer') THEN ALTER TABLE public.sync_file_transfer_requests ADD COLUMN prefer_delta_transfer boolean NOT NULL DEFAULT false; END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_transfer_requests' AND column_name='receiver_base_sha256') THEN ALTER TABLE public.sync_file_transfer_requests ADD COLUMN receiver_base_sha256 character varying(64); END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_transfer_requests' AND column_name='receiver_block_hashes_json') THEN ALTER TABLE public.sync_file_transfer_requests ADD COLUMN receiver_block_hashes_json text; END IF;

                    -- sync_file_manifests
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_manifests' AND column_name='is_preprocessed') THEN ALTER TABLE public.sync_file_manifests ADD COLUMN is_preprocessed boolean NOT NULL DEFAULT false; END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_manifests' AND column_name='original_size_bytes') THEN ALTER TABLE public.sync_file_manifests ADD COLUMN original_size_bytes bigint; END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_manifests' AND column_name='preprocess_profile') THEN ALTER TABLE public.sync_file_manifests ADD COLUMN preprocess_profile character varying(50); END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_manifests' AND column_name='transport_encoding') THEN ALTER TABLE public.sync_file_manifests ADD COLUMN transport_encoding character varying(30) NOT NULL DEFAULT ''; END IF;

                    -- sync_file_chunk_sessions
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_chunk_sessions' AND column_name='is_delta_session') THEN ALTER TABLE public.sync_file_chunk_sessions ADD COLUMN is_delta_session boolean NOT NULL DEFAULT false; END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_chunk_sessions' AND column_name='receiver_base_sha256') THEN ALTER TABLE public.sync_file_chunk_sessions ADD COLUMN receiver_base_sha256 character varying(64); END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='sync_file_chunk_sessions' AND column_name='requested_chunk_indexes_json') THEN ALTER TABLE public.sync_file_chunk_sessions ADD COLUMN requested_chunk_indexes_json text; END IF;
                END;
                $$;
            ");

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
            migrationBuilder.Sql(@"
                ALTER TABLE public.sync_file_transfer_requests DROP COLUMN IF EXISTS delta_block_size_bytes;
                ALTER TABLE public.sync_file_transfer_requests DROP COLUMN IF EXISTS prefer_delta_transfer;
                ALTER TABLE public.sync_file_transfer_requests DROP COLUMN IF EXISTS receiver_base_sha256;
                ALTER TABLE public.sync_file_transfer_requests DROP COLUMN IF EXISTS receiver_block_hashes_json;
                ALTER TABLE public.sync_file_manifests DROP COLUMN IF EXISTS is_preprocessed;
                ALTER TABLE public.sync_file_manifests DROP COLUMN IF EXISTS original_size_bytes;
                ALTER TABLE public.sync_file_manifests DROP COLUMN IF EXISTS preprocess_profile;
                ALTER TABLE public.sync_file_manifests DROP COLUMN IF EXISTS transport_encoding;
                ALTER TABLE public.sync_file_chunk_sessions DROP COLUMN IF EXISTS is_delta_session;
                ALTER TABLE public.sync_file_chunk_sessions DROP COLUMN IF EXISTS receiver_base_sha256;
                ALTER TABLE public.sync_file_chunk_sessions DROP COLUMN IF EXISTS requested_chunk_indexes_json;
            ");
        }
    }
}
