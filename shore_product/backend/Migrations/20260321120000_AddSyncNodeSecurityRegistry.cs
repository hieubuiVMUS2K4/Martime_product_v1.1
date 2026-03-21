using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    public partial class AddSyncNodeSecurityRegistry : Migration
    {
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
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastKeyRotatedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignedRequestAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
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

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_IsRegistered",
                table: "sync_node_trackers",
                column: "IsRegistered");

            migrationBuilder.CreateIndex(
                name: "IX_sync_node_trackers_IsRevoked",
                table: "sync_node_trackers",
                column: "IsRevoked");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "LastKeyRotatedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastSignedRequestAt",
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