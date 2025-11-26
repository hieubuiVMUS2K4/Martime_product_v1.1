using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskTypeTaskDetailManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_task_details_task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.AddColumn<long>(
                name: "TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "task_detail_task_type",
                schema: "public",
                columns: table => new
                {
                    task_details_id = table.Column<long>(type: "bigint", nullable: false),
                    task_types_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_detail_task_type", x => new { x.task_details_id, x.task_types_id });
                    table.ForeignKey(
                        name: "f_k_task_detail_task_type_task_details_task_details_id",
                        column: x => x.task_details_id,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_task_detail_task_type_task_types_task_types_id",
                        column: x => x.task_types_id,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                column: "TaskDetailId1");

            migrationBuilder.CreateIndex(
                name: "IX_task_detail_task_type_task_types_id",
                schema: "public",
                table: "task_detail_task_type",
                column: "task_types_id");

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_task_details_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                column: "TaskDetailId1",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_maintenance_task_details__task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "f_k_maintenance_task_details__task_types_task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_task_details_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropForeignKey(
                name: "f_k_maintenance_task_details__task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropForeignKey(
                name: "f_k_maintenance_task_details__task_types_task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropTable(
                name: "task_detail_task_type",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_maintenance_task_details_task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropIndex(
                name: "IX_maintenance_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropColumn(
                name: "TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.DropColumn(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_task_details");

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_task_details_task_details_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id",
                principalSchema: "public",
                principalTable: "task_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
