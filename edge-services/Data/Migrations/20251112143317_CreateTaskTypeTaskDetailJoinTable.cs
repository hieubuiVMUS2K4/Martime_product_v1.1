using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTaskTypeTaskDetailJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_detail_task_type",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "task_type_task_details",
                schema: "public",
                columns: table => new
                {
                    TaskTypeId = table.Column<int>(type: "integer", nullable: false),
                    TaskDetailId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_type_task_details", x => new { x.TaskTypeId, x.TaskDetailId });
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_details_TaskDetailId",
                        column: x => x.TaskDetailId,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_types_TaskTypeId",
                        column: x => x.TaskTypeId,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_detail_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskDetailId");

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_type_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_type_task_details",
                schema: "public");

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
                name: "IX_task_detail_task_type_task_types_id",
                schema: "public",
                table: "task_detail_task_type",
                column: "task_types_id");
        }
    }
}
