using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeafarerEmploymentHealthDocumentsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employment_documents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_employment_documents", x => x.id);
                    table.ForeignKey(
                        name: "f_k_employment_documents_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "f_k_employment_documents_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_documents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_health_documents", x => x.id);
                    table.ForeignKey(
                        name: "f_k_health_documents_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seafarer_documents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_seafarer_documents", x => x.id);
                    table.ForeignKey(
                        name: "f_k_seafarer_documents_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "f_k_seafarer_documents_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employment_documents_country_id",
                schema: "public",
                table: "employment_documents",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_documents_crew_member_id",
                schema: "public",
                table: "employment_documents",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_documents_crew_member_id",
                schema: "public",
                table: "health_documents",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_seafarer_documents_country_id",
                schema: "public",
                table: "seafarer_documents",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_seafarer_documents_crew_member_id",
                schema: "public",
                table: "seafarer_documents",
                column: "crew_member_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employment_documents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "health_documents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "seafarer_documents",
                schema: "public");
        }
    }
}
