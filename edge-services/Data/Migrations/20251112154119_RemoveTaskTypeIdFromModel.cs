using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskTypeIdFromModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: task_type_id column and related constraints were already manually removed from database
            // This migration only updates the EF model to match the current database state
            // No actual database changes needed - this is a model-only migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "task_details",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_type_id",
                schema: "public",
                table: "task_details",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_type_order",
                schema: "public",
                table: "task_details",
                columns: new[] { "task_type_id", "order_index" });

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
    }
}
