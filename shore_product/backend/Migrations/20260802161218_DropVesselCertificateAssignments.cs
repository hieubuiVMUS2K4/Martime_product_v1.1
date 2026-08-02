using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class DropVesselCertificateAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vessel_certificate_assignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vessel_certificate_assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_certificate_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vessel_certificate_assignments_certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vessel_certificate_assignments_CertificateId",
                table: "vessel_certificate_assignments",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_certificate_assignments_IsSynced",
                table: "vessel_certificate_assignments",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_certificate_assignments_VesselId_CertificateId",
                table: "vessel_certificate_assignments",
                columns: new[] { "VesselId", "CertificateId" },
                unique: true);
        }
    }
}
