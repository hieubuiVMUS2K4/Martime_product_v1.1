using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class VoyageCrewPortLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "arrival_port_code",
                schema: "public",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "call_sign",
                schema: "public",
                table: "voyage_records",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "departure_port_code",
                schema: "public",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_port_code",
                schema: "public",
                table: "voyage_records",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_port_name",
                schema: "public",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_flag",
                schema: "public",
                table: "voyage_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_i_m_o",
                schema: "public",
                table: "voyage_records",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_name",
                schema: "public",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voyage_crew_assignments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rank_id = table.Column<int>(type: "integer", nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    embark_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    embark_port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    embark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disembark_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    disembark_port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    disembark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    watch_schedule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_crew_assignments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_assignments__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_assignments_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_assignments_ranks_rank_id",
                        column: x => x.rank_id,
                        principalSchema: "public",
                        principalTable: "ranks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "port_calls",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    port_id = table.Column<int>(type: "integer", nullable: true),
                    port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    call_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    berth_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pilot_on_board = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pilot_off_board = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    draft_fore = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    draft_aft = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    cargo_ops_completed = table.Column<bool>(type: "boolean", nullable: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_port_calls", x => x.id);
                    table.ForeignKey(
                        name: "f_k_port_calls__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_port_calls_ports_port_id",
                        column: x => x.port_id,
                        principalSchema: "public",
                        principalTable: "ports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_arr_port_code",
                schema: "public",
                table: "voyage_records",
                column: "arrival_port_code");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_dep_port_code",
                schema: "public",
                table: "voyage_records",
                column: "departure_port_code");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_vessel_imo",
                schema: "public",
                table: "voyage_records",
                column: "vessel_i_m_o");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_arrival",
                schema: "public",
                table: "port_calls",
                column: "arrival_time");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_port_code",
                schema: "public",
                table: "port_calls",
                column: "port_code");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_synced",
                schema: "public",
                table: "port_calls",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_type",
                schema: "public",
                table: "port_calls",
                column: "call_type");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_voyage_seq",
                schema: "public",
                table: "port_calls",
                columns: new[] { "voyage_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_port_calls_port_id",
                schema: "public",
                table: "port_calls",
                column: "port_id");

            migrationBuilder.CreateIndex(
                name: "idx_port_active",
                schema: "public",
                table: "ports",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_port_code_unique",
                schema: "public",
                table: "ports",
                column: "port_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_port_country_code",
                schema: "public",
                table: "ports",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "idx_port_name",
                schema: "public",
                table: "ports",
                column: "port_name");

            migrationBuilder.CreateIndex(
                name: "idx_vca_crew_member",
                schema: "public",
                table: "voyage_crew_assignments",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "idx_vca_status",
                schema: "public",
                table: "voyage_crew_assignments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_vca_synced",
                schema: "public",
                table: "voyage_crew_assignments",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_vca_voyage_crew_unique",
                schema: "public",
                table: "voyage_crew_assignments",
                columns: new[] { "voyage_id", "crew_member_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_assignments_rank_id",
                schema: "public",
                table: "voyage_crew_assignments",
                column: "rank_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_cargo_operations__voyage_records_voyage_record_id",
                schema: "public",
                table: "cargo_operations",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_voyage_log_entries__voyage_records_voyage_id",
                schema: "public",
                table: "voyage_log_entries",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_cargo_operations__voyage_records_voyage_record_id",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropForeignKey(
                name: "f_k_voyage_log_entries__voyage_records_voyage_id",
                schema: "public",
                table: "voyage_log_entries");

            migrationBuilder.DropTable(
                name: "port_calls",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_crew_assignments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ports",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_voyage_arr_port_code",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "idx_voyage_dep_port_code",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropIndex(
                name: "idx_voyage_vessel_imo",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "arrival_port_code",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "call_sign",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "departure_port_code",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "previous_port_code",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "previous_port_name",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "vessel_flag",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "vessel_i_m_o",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "vessel_name",
                schema: "public",
                table: "voyage_records");
        }
    }
}
