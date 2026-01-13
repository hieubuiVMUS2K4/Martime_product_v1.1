using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "certificates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    certificate_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    certificate_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    validity_period_months = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_certificates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crew_certificates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificate_id = table.Column<int>(type: "integer", nullable: false),
                    certificate_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    document_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_crew_certificates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_crew_certificates__crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_crew_certificates_certificates_certificate_id",
                        column: x => x.certificate_id,
                        principalSchema: "public",
                        principalTable: "certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_certificate_active",
                schema: "public",
                table: "certificates",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_certificate_category",
                schema: "public",
                table: "certificates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_certificate_code_unique",
                schema: "public",
                table: "certificates",
                column: "certificate_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_crew_expiry",
                schema: "public",
                table: "crew_certificates",
                columns: new[] { "crew_member_id", "expiry_date" });

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_crew_id",
                schema: "public",
                table: "crew_certificates",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_expiry",
                schema: "public",
                table: "crew_certificates",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_number_unique",
                schema: "public",
                table: "crew_certificates",
                column: "certificate_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_status",
                schema: "public",
                table: "crew_certificates",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_synced",
                schema: "public",
                table: "crew_certificates",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_type_id",
                schema: "public",
                table: "crew_certificates",
                column: "certificate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crew_certificates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "certificates",
                schema: "public");

            migrationBuilder.AddColumn<DateTime>(
                name: "certificate_expiry",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "certificate_issue",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "certificate_number",
                schema: "public",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "medical_expiry",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "medical_issue",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_expiry",
                schema: "public",
                table: "crew_members",
                column: "certificate_expiry");
        }
    }
}
