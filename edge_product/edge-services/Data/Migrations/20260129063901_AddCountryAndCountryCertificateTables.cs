using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryAndCountryCertificateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    country_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code_alpha2 = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "country_certificates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    certificate_id = table.Column<int>(type: "integer", nullable: false),
                    is_recognized = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_country_certificates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_country_certificates_certificates_certificate_id",
                        column: x => x.certificate_id,
                        principalSchema: "public",
                        principalTable: "certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_country_certificates_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_country_active",
                schema: "public",
                table: "countries",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_country_code_alpha2",
                schema: "public",
                table: "countries",
                column: "country_code_alpha2");

            migrationBuilder.CreateIndex(
                name: "idx_country_code_unique",
                schema: "public",
                table: "countries",
                column: "country_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_country_cert_certificate_id",
                schema: "public",
                table: "country_certificates",
                column: "certificate_id");

            migrationBuilder.CreateIndex(
                name: "idx_country_cert_country_id",
                schema: "public",
                table: "country_certificates",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "idx_country_cert_recognized",
                schema: "public",
                table: "country_certificates",
                column: "is_recognized",
                filter: "is_recognized = true");

            migrationBuilder.CreateIndex(
                name: "idx_country_cert_unique",
                schema: "public",
                table: "country_certificates",
                columns: new[] { "country_id", "certificate_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_certificates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "public");

            migrationBuilder.RenameIndex(
                name: "idx_crew_cert_expiry",
                schema: "public",
                table: "crew_certificates",
                newName: "idx_crew_cert_expiry_date");
        }
    }
}
