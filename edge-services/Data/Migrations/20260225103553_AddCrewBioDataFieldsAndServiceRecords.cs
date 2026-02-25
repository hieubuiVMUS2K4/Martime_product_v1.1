using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewBioDataFieldsAndServiceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "blood_group",
                schema: "public",
                table: "crew_members",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "catering_size",
                schema: "public",
                table: "crew_members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "clothing_size",
                schema: "public",
                table: "crew_members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "education_course",
                schema: "public",
                table: "crew_members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "education_graduation_year",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "education_institution",
                schema: "public",
                table: "crew_members",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "education_period_years",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "id_card_number",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_covid_vaccinated",
                schema: "public",
                table: "crew_members",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_smoker",
                schema: "public",
                table: "crew_members",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marital_status",
                schema: "public",
                table: "crew_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "next_of_kin_address",
                schema: "public",
                table: "crew_members",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "next_of_kin_name",
                schema: "public",
                table: "crew_members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "next_of_kin_phone",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "next_of_kin_relation",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                schema: "public",
                table: "crew_members",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "place_of_birth",
                schema: "public",
                table: "crew_members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shoe_size",
                schema: "public",
                table: "crew_members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                schema: "public",
                table: "crew_members",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vessel_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vessel_flag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vessel_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vessel_grt = table.Column<decimal>(type: "numeric", nullable: true),
                    vessel_dwt = table.Column<decimal>(type: "numeric", nullable: true),
                    vessel_year_built = table.Column<int>(type: "integer", nullable: true),
                    trade_area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    main_engine_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    main_engine_power_kw = table.Column<int>(type: "integer", nullable: true),
                    main_engine_maker = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    boiler_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    has_exhaust_gas_scrubber = table.Column<bool>(type: "boolean", nullable: true),
                    ecdis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rank_at_time = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    boarding_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    disembark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    boarding_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    boarding_port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    disembark_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    disembark_port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    boarding_records = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_service_records", x => x.id);
                    table.ForeignKey(
                        name: "f_k_service_records_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_records_crew_member_id",
                schema: "public",
                table: "service_records",
                column: "crew_member_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_records",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "blood_group",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "catering_size",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "clothing_size",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "education_course",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "education_graduation_year",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "education_institution",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "education_period_years",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "height",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "id_card_number",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "is_covid_vaccinated",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "is_smoker",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "marital_status",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "next_of_kin_address",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "next_of_kin_name",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "next_of_kin_phone",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "next_of_kin_relation",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "photo_url",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "place_of_birth",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "shoe_size",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "weight",
                schema: "public",
                table: "crew_members");
        }
    }
}
