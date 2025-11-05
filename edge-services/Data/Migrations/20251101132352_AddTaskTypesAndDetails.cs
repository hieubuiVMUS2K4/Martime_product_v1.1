using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTypesAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "task_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    default_priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estimated_duration_hours = table.Column<int>(type: "integer", nullable: true),
                    required_certification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    requires_approval = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    task_type_id = table.Column<int>(type: "integer", nullable: false),
                    detail_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    detail_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    min_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    max_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    requires_photo = table.Column<bool>(type: "boolean", nullable: false),
                    requires_signature = table.Column<bool>(type: "boolean", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_task_details_task_types_task_type_id",
                        column: x => x.task_type_id,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_task_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maintenance_task_id = table.Column<long>(type: "bigint", nullable: false),
                    task_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    measured_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    check_result = table.Column<bool>(type: "boolean", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    signature_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_task_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_task_details_maintenance_tasks_maintenance_task~",
                        column: x => x.maintenance_task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_task_details_task_details_task_detail_id",
                        column: x => x.task_detail_id,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_task_type_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_completed",
                schema: "public",
                table: "maintenance_task_details",
                column: "is_completed",
                filter: "is_completed = false");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "maintenance_task_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_status",
                schema: "public",
                table: "maintenance_task_details",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_task_detail_unique",
                schema: "public",
                table: "maintenance_task_details",
                columns: new[] { "maintenance_task_id", "task_detail_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_active",
                schema: "public",
                table: "task_details",
                column: "is_active",
                filter: "is_active = true");

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
                name: "FK_maintenance_tasks_task_types_task_type_id",
                schema: "public",
                table: "maintenance_tasks",
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
                name: "FK_maintenance_tasks_task_types_task_type_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropTable(
                name: "maintenance_task_details",
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

            migrationBuilder.DropColumn(
                name: "task_type_id",
                schema: "public",
                table: "maintenance_tasks");
        }
    }
}
