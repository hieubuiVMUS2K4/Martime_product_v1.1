using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOverdueDeferralFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_overdue_deferral",
                schema: "public",
                table: "task_deferral_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "preventive_measures",
                schema: "public",
                table: "task_deferral_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "root_cause",
                schema: "public",
                table: "task_deferral_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "task_status_at_request",
                schema: "public",
                table: "task_deferral_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_overdue_deferral",
                schema: "public",
                table: "task_deferral_requests");

            migrationBuilder.DropColumn(
                name: "preventive_measures",
                schema: "public",
                table: "task_deferral_requests");

            migrationBuilder.DropColumn(
                name: "root_cause",
                schema: "public",
                table: "task_deferral_requests");

            migrationBuilder.DropColumn(
                name: "task_status_at_request",
                schema: "public",
                table: "task_deferral_requests");
        }
    }
}
