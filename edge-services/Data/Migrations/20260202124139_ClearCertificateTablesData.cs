using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClearCertificateTablesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear data from 5 tables in correct order (FK constraints)
            
            // 1. Delete crew_certificates first (has FK to certificates and crew_members)
            migrationBuilder.Sql("DELETE FROM crew_certificates;");
            
            // 2. Delete country_certificates (has FK to countries and certificates)
            migrationBuilder.Sql("DELETE FROM country_certificates;");
            
            // 3. Delete crew_members
            migrationBuilder.Sql("DELETE FROM crew_members;");
            
            // 4. Delete certificates
            migrationBuilder.Sql("DELETE FROM certificates;");
            
            // 5. Delete countries
            migrationBuilder.Sql("DELETE FROM countries;");
            
            // Reset identity sequences for tables with integer IDs
            migrationBuilder.Sql("ALTER SEQUENCE crew_certificates_id_seq RESTART WITH 1;");
            migrationBuilder.Sql("ALTER SEQUENCE country_certificates_id_seq RESTART WITH 1;");
            migrationBuilder.Sql("ALTER SEQUENCE certificates_id_seq RESTART WITH 1;");
            migrationBuilder.Sql("ALTER SEQUENCE countries_id_seq RESTART WITH 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
