using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_tasks_task_types_task_type_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropTable(
                name: "task_type_task_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_types",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_task_type_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_mtd_task_detail_id",
                schema: "public",
                table: "maintenance_task_details");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "task_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    detail_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detail_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    max_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    min_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    requires_photo = table.Column<bool>(type: "boolean", nullable: false),
                    requires_signature = table.Column<bool>(type: "boolean", nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    default_priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    estimated_duration_hours = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    required_certification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    requires_approval = table.Column<bool>(type: "boolean", nullable: false),
                    type_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_types", x => x.id);
                });

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
                name: "idx_maintenance_task_type_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id");

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
                name: "idx_task_detail_active",
                schema: "public",
                table: "task_details",
                column: "is_active",
                filter: "is_active = true");

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

            migrationBuilder.CreateIndex(
                name: "idx_task_type_active",
                schema: "public",
                table: "task_types",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_task_type_category",
                schema: "public",
                table: "task_types",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_task_type_code_unique",
                schema: "public",
                table: "task_types",
                column: "type_code",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_tasks_task_types_task_type_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_type_id",
                principalSchema: "public",
                principalTable: "task_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
