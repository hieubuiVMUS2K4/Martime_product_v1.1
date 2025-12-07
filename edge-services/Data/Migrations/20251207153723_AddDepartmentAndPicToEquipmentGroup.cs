using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndPicToEquipmentGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "public",
                table: "equipment_groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pic_crew_id",
                schema: "public",
                table: "equipment_groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pic_role",
                schema: "public",
                table: "equipment_groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "department",
                schema: "public",
                table: "equipment_groups");

            migrationBuilder.DropColumn(
                name: "pic_crew_id",
                schema: "public",
                table: "equipment_groups");

            migrationBuilder.DropColumn(
                name: "pic_role",
                schema: "public",
                table: "equipment_groups");
        }
    }
}
