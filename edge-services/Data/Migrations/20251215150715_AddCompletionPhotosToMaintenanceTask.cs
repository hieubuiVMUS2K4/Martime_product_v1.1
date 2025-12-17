using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionPhotosToMaintenanceTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "completion_photos",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(100000)",
                maxLength: 100000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completion_photos",
                schema: "public",
                table: "maintenance_tasks");
        }
    }
}
