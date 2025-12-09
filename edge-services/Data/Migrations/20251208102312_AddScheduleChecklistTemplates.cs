using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleChecklistTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedule_checklist_templates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    checkpoint_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requires_reading = table.Column<bool>(type: "boolean", nullable: false),
                    normal_range_min = table.Column<double>(type: "double precision", nullable: true),
                    normal_range_max = table.Column<double>(type: "double precision", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_schedule_checklist_templates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_schedule_checklist_templates_maintenance_schedules_schedule~",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "maintenance_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_checklist_templates_schedule_id",
                schema: "public",
                table: "schedule_checklist_templates",
                column: "schedule_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_checklist_templates",
                schema: "public");
        }
    }
}
