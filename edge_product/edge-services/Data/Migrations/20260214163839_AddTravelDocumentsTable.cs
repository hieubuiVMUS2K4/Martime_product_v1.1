using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "travel_documents",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crew_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_travel_documents", x => x.id);
                    table.ForeignKey(
                        name: "f_k_travel_documents_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "public",
                        principalTable: "countries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "f_k_travel_documents_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_travel_documents_country_id",
                schema: "public",
                table: "travel_documents",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_travel_documents_crew_member_id",
                schema: "public",
                table: "travel_documents",
                column: "crew_member_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "travel_documents",
                schema: "public");
        }
    }
}
