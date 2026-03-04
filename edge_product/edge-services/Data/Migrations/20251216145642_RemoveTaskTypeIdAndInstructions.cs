using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskTypeIdAndInstructions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_schedules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
