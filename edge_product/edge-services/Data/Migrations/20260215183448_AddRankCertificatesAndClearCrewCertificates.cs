using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRankCertificatesAndClearCrewCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rank_certificates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rank_id = table.Column<int>(type: "integer", nullable: false),
                    certificate_id = table.Column<int>(type: "integer", nullable: false),
                    country_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_rank_certificates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_rank_certificates_certificates_certificate_id",
                        column: x => x.certificate_id,
                        principalSchema: "public",
                        principalTable: "certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_rank_certificates_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "f_k_rank_certificates_ranks_rank_id",
                        column: x => x.rank_id,
                        principalSchema: "public",
                        principalTable: "ranks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_certificate_id",
                schema: "public",
                table: "rank_certificates",
                column: "certificate_id");

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_country_id",
                schema: "public",
                table: "rank_certificates",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_rank_id",
                schema: "public",
                table: "rank_certificates",
                column: "rank_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rank_certificates",
                schema: "public");
        }
    }
}
