using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeTaskTypeIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "task_details",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "task_details",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
