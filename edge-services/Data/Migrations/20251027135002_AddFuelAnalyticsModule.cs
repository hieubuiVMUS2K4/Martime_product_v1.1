using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelAnalyticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fuel_analytics_summaries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    distance_nautical_miles = table.Column<double>(type: "numeric(12,2)", nullable: false),
                    time_underway_hours = table.Column<double>(type: "numeric(10,2)", nullable: false),
                    time_berth_hours = table.Column<double>(type: "numeric(10,2)", nullable: false),
                    average_speed_knots = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    total_fuel_consumed_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    main_engine_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    auxiliary_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    boiler_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    e_e_o_i = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    fuel_per_nautical_mile = table.Column<double>(type: "numeric(8,4)", nullable: false),
                    fuel_per_hour = table.Column<double>(type: "numeric(8,4)", nullable: false),
                    s_f_o_c = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    c_o2_emissions_m_t = table.Column<double>(type: "numeric(12,3)", nullable: false),
                    c_i_i = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    c_i_i_rating = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    avg_main_engine_r_p_m = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    avg_main_engine_load = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    avg_sea_state = table.Column<double>(type: "numeric(3,1)", nullable: true),
                    avg_wind_speed = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    cargo_weight_m_t = table.Column<double>(type: "numeric(12,3)", nullable: true),
                    estimated_fuel_cost_u_s_d = table.Column<double>(type: "numeric(15,2)", nullable: true),
                    fuel_price_per_m_t = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    voyage_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_points_count = table.Column<int>(type: "integer", nullable: false),
                    data_quality_score = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_analytics_summaries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_efficiency_alerts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    current_value = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    expected_value = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    deviation_percent = table.Column<double>(type: "numeric(6,2)", nullable: false),
                    recommended_action = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_efficiency_alerts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_cii",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "c_i_i_rating");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_period",
                schema: "public",
                table: "fuel_analytics_summaries",
                columns: new[] { "period_type", "period_start" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_synced",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_voyage",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_synced",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_timestamp",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_type",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "alert_type");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_unresolved",
                schema: "public",
                table: "fuel_efficiency_alerts",
                columns: new[] { "is_resolved", "severity" },
                filter: "is_resolved = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuel_analytics_summaries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fuel_efficiency_alerts",
                schema: "public");
        }
    }
}
