using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageLogEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "voyage_log_entries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    event_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_date_time_local = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_zone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    port_locode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    port_country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    berth_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    distance_to_go = table.Column<double>(type: "double precision", nullable: true),
                    distance_from_last = table.Column<double>(type: "double precision", nullable: true),
                    total_voyage_distance = table.Column<double>(type: "double precision", nullable: true),
                    course_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    speed_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    pilot_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pilot_station = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    officer_on_watch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "text", nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_log_entries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_event_datetime",
                schema: "public",
                table: "voyage_log_entries",
                column: "event_date_time");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_event_type",
                schema: "public",
                table: "voyage_log_entries",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_port_locode",
                schema: "public",
                table: "voyage_log_entries",
                column: "port_locode");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_synced",
                schema: "public",
                table: "voyage_log_entries",
                column: "is_synced");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_voyage_id",
                schema: "public",
                table: "voyage_log_entries",
                column: "voyage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voyage_log_entries",
                schema: "public");
        }
    }
}
