using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAbstractLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "abstract_log_voyages",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ship_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    i_m_o_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    master_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    chief_engineer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    report_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_of_last_docking = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    propeller_pitch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    commencement_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completion_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    grand_total_hours = table.Column<double>(type: "double precision", nullable: true),
                    fo_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    fo_received = table.Column<double>(type: "double precision", nullable: true),
                    fo_consumed_total = table.Column<double>(type: "double precision", nullable: true),
                    fo_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    do_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    do_received = table.Column<double>(type: "double precision", nullable: true),
                    do_consumed_total = table.Column<double>(type: "double precision", nullable: true),
                    do_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    cyl_oil_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    cyl_oil_received = table.Column<double>(type: "double precision", nullable: true),
                    cyl_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    cyl_oil_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    sys_oil_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    sys_oil_received = table.Column<double>(type: "double precision", nullable: true),
                    sys_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    sys_oil_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    gen_oil_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    gen_oil_received = table.Column<double>(type: "double precision", nullable: true),
                    gen_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    gen_oil_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    fw_rob_previous = table.Column<double>(type: "double precision", nullable: true),
                    fw_produced = table.Column<double>(type: "double precision", nullable: true),
                    fw_consumed = table.Column<double>(type: "double precision", nullable: true),
                    fw_rob_current = table.Column<double>(type: "double precision", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_abstract_log_voyages", x => x.id);
                    table.ForeignKey(
                        name: "f_k_abstract_log_voyages__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "abstract_log_legs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    abstract_log_voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leg_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    departure_port = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    departure_draft_fore = table.Column<double>(type: "double precision", nullable: true),
                    departure_draft_aft = table.Column<double>(type: "double precision", nullable: true),
                    departure_draft_mean = table.Column<double>(type: "double precision", nullable: true),
                    arrival_port = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arrival_draft_fore = table.Column<double>(type: "double precision", nullable: true),
                    arrival_draft_aft = table.Column<double>(type: "double precision", nullable: true),
                    arrival_draft_mean = table.Column<double>(type: "double precision", nullable: true),
                    hours_propelling = table.Column<double>(type: "double precision", nullable: true),
                    hours_under_way = table.Column<double>(type: "double precision", nullable: true),
                    hours_drifting = table.Column<double>(type: "double precision", nullable: true),
                    hours_anchor = table.Column<double>(type: "double precision", nullable: true),
                    hours_port = table.Column<double>(type: "double precision", nullable: true),
                    distance_prop = table.Column<double>(type: "double precision", nullable: true),
                    distance_log = table.Column<double>(type: "double precision", nullable: true),
                    distance_o_g = table.Column<double>(type: "double precision", nullable: true),
                    speed_log = table.Column<double>(type: "double precision", nullable: true),
                    speed_o_g = table.Column<double>(type: "double precision", nullable: true),
                    slip_percent = table.Column<double>(type: "double precision", nullable: true),
                    shaft_revolutions = table.Column<double>(type: "double precision", nullable: true),
                    me_foc_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    me_foc_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    me_foc_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    de_foc_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    de_foc_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    de_foc_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    boiler_foc_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    boiler_foc_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    boiler_foc_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_abstract_log_legs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_abstract_log_legs__abstract_log_voyages_abstract_log_voyage_id",
                        column: x => x.abstract_log_voyage_id,
                        principalSchema: "public",
                        principalTable: "abstract_log_voyages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "abstract_log_daily_entries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    abstract_log_leg_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    noon_latitude = table.Column<double>(type: "double precision", nullable: true),
                    noon_longitude = table.Column<double>(type: "double precision", nullable: true),
                    wind_direction_true = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    wind_direction_relative = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    wind_force_beaufort = table.Column<int>(type: "integer", nullable: true),
                    sea_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hours_under_way = table.Column<double>(type: "double precision", nullable: true),
                    hours_propelling = table.Column<double>(type: "double precision", nullable: true),
                    hours_drifting = table.Column<double>(type: "double precision", nullable: true),
                    hours_anchor = table.Column<double>(type: "double precision", nullable: true),
                    hours_port = table.Column<double>(type: "double precision", nullable: true),
                    time_zone_change = table.Column<double>(type: "double precision", nullable: true),
                    distance_engine = table.Column<double>(type: "double precision", nullable: true),
                    distance_log = table.Column<double>(type: "double precision", nullable: true),
                    distance_o_g = table.Column<double>(type: "double precision", nullable: true),
                    speed_log = table.Column<double>(type: "double precision", nullable: true),
                    speed_o_g = table.Column<double>(type: "double precision", nullable: true),
                    slip_percent = table.Column<double>(type: "double precision", nullable: true),
                    avg_r_p_m = table.Column<double>(type: "double precision", nullable: true),
                    hp_me_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_me_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_me_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    hp_de_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_de_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_de_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    hp_boiler_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_boiler_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    hp_boiler_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    dt_me_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_me_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_me_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    dt_de_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_de_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_de_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    dt_boiler_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_boiler_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    dt_boiler_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    port_me_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_me_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_me_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    port_de_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_de_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_de_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    port_boiler_hsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_boiler_vlsfo = table.Column<double>(type: "double precision", nullable: true),
                    port_boiler_lsmgo = table.Column<double>(type: "double precision", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_abstract_log_daily_entries", x => x.id);
                    table.ForeignKey(
                        name: "f_k_abstract_log_daily_entries__abstract_log_legs_abstract_log_leg~",
                        column: x => x.abstract_log_leg_id,
                        principalSchema: "public",
                        principalTable: "abstract_log_legs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_alde_leg",
                schema: "public",
                table: "abstract_log_daily_entries",
                column: "abstract_log_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_alde_leg_date_unique",
                schema: "public",
                table: "abstract_log_daily_entries",
                columns: new[] { "abstract_log_leg_id", "entry_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_all_voyage_leg_unique",
                schema: "public",
                table: "abstract_log_legs",
                columns: new[] { "abstract_log_voyage_id", "leg_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_alv_status",
                schema: "public",
                table: "abstract_log_voyages",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_alv_voyage_unique",
                schema: "public",
                table: "abstract_log_voyages",
                column: "voyage_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abstract_log_daily_entries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "abstract_log_legs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "abstract_log_voyages",
                schema: "public");
        }
    }
}
