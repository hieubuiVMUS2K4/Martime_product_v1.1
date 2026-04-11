using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class FixSyncTransferSchemaDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check and add columns to sync_file_transfer_requests if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_transfer_requests' AND column_name = 'DeltaBlockSizeBytes') THEN
                        ALTER TABLE sync_file_transfer_requests ADD COLUMN ""DeltaBlockSizeBytes"" integer;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_transfer_requests' AND column_name = 'PreferDeltaTransfer') THEN
                        ALTER TABLE sync_file_transfer_requests ADD COLUMN ""PreferDeltaTransfer"" boolean DEFAULT false;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_transfer_requests' AND column_name = 'ReceiverBaseSha256') THEN
                        ALTER TABLE sync_file_transfer_requests ADD COLUMN ""ReceiverBaseSha256"" character varying(64);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_transfer_requests' AND column_name = 'ReceiverBlockHashesJson') THEN
                        ALTER TABLE sync_file_transfer_requests ADD COLUMN ""ReceiverBlockHashesJson"" text;
                    END IF;
                END $$;
            ");

            // Check and add columns to sync_file_manifests if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_manifests' AND column_name = 'IsPreprocessed') THEN
                        ALTER TABLE sync_file_manifests ADD COLUMN ""IsPreprocessed"" boolean DEFAULT false;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_manifests' AND column_name = 'OriginalSizeBytes') THEN
                        ALTER TABLE sync_file_manifests ADD COLUMN ""OriginalSizeBytes"" bigint;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_manifests' AND column_name = 'PreprocessProfile') THEN
                        ALTER TABLE sync_file_manifests ADD COLUMN ""PreprocessProfile"" character varying(50);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_manifests' AND column_name = 'TransportEncoding') THEN
                        ALTER TABLE sync_file_manifests ADD COLUMN ""TransportEncoding"" character varying(30) DEFAULT '';
                    END IF;
                END $$;
            ");

            // Check and add columns to sync_file_chunk_sessions if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_chunk_sessions' AND column_name = 'IsDeltaSession') THEN
                        ALTER TABLE sync_file_chunk_sessions ADD COLUMN ""IsDeltaSession"" boolean DEFAULT false;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_chunk_sessions' AND column_name = 'ReceiverBaseSha256') THEN
                        ALTER TABLE sync_file_chunk_sessions ADD COLUMN ""ReceiverBaseSha256"" character varying(64);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sync_file_chunk_sessions' AND column_name = 'RequestedChunkIndexesJson') THEN
                        ALTER TABLE sync_file_chunk_sessions ADD COLUMN ""RequestedChunkIndexesJson"" text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeltaBlockSizeBytes",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "PreferDeltaTransfer",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "ReceiverBaseSha256",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "ReceiverBlockHashesJson",
                table: "sync_file_transfer_requests");

            migrationBuilder.DropColumn(
                name: "IsPreprocessed",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "OriginalSizeBytes",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "PreprocessProfile",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "TransportEncoding",
                table: "sync_file_manifests");

            migrationBuilder.DropColumn(
                name: "IsDeltaSession",
                table: "sync_file_chunk_sessions");

            migrationBuilder.DropColumn(
                name: "ReceiverBaseSha256",
                table: "sync_file_chunk_sessions");

            migrationBuilder.DropColumn(
                name: "RequestedChunkIndexesJson",
                table: "sync_file_chunk_sessions");
        }
    }
}
