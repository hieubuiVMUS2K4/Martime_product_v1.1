using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselProvisioningToSyncNodeTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigDownloadCount",
                table: "sync_node_trackers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstHandshakeAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConfigDownloadedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastConfigDownloadedBy",
                table: "sync_node_trackers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastConfigDownloadedIp",
                table: "sync_node_trackers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHandshakeAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NodeApiToken",
                table: "sync_node_trackers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NodeApiTokenHash",
                table: "sync_node_trackers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NodeApiTokenRotatedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NodeApiTokenVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisionedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisioningStatus",
                table: "sync_node_trackers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_NodeApiTokenHash",
                table: "sync_node_trackers",
                column: "NodeApiTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_ProvisioningStatus",
                table: "sync_node_trackers",
                column: "ProvisioningStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sync_node_trackers_NodeApiTokenHash",
                table: "sync_node_trackers");

            migrationBuilder.DropIndex(
                name: "IX_sync_node_trackers_ProvisioningStatus",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "ConfigDownloadCount",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "FirstHandshakeAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastConfigDownloadedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastConfigDownloadedBy",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastConfigDownloadedIp",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastHandshakeAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "NodeApiToken",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "NodeApiTokenHash",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "NodeApiTokenRotatedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "NodeApiTokenVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "ProvisionedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "ProvisioningStatus",
                table: "sync_node_trackers");
        }
    }
}
