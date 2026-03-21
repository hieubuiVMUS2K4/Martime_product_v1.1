using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    public partial class AddSyncKeyRotationGraceWindow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastAcknowledgedKeyVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastKeyVersionAcknowledgedAt",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSigningKey",
                table: "sync_node_trackers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousKeyVersion",
                table: "sync_node_trackers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousKeyGraceUntil",
                table: "sync_node_trackers",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAcknowledgedKeyVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "LastKeyVersionAcknowledgedAt",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousSigningKey",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousKeyVersion",
                table: "sync_node_trackers");

            migrationBuilder.DropColumn(
                name: "PreviousKeyGraceUntil",
                table: "sync_node_trackers");
        }
    }
}