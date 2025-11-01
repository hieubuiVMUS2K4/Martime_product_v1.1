using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskDetailForeignKeyToOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details");

            migrationBuilder.AddForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details");

            migrationBuilder.AddForeignKey(
                name: "FK_task_details_task_types_task_type_id",
                schema: "public",
                table: "task_details",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
