using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    public partial class AddQuantityRequiredToMaterialItemEquipment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "quantity_required",
                schema: "public",
                table: "material_item_equipments",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantity_required",
                schema: "public",
                table: "material_item_equipments");
        }
    }
}
