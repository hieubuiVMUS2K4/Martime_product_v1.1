using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialReceiptItemSnapshotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "material_item_id",
                schema: "public",
                table: "material_receipt_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "item_code",
                schema: "public",
                table: "material_receipt_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_name",
                schema: "public",
                table: "material_receipt_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "specification",
                schema: "public",
                table: "material_receipt_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit",
                schema: "public",
                table: "material_receipt_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items",
                column: "material_item_id",
                principalSchema: "public",
                principalTable: "material_items",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.DropColumn(
                name: "item_code",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.DropColumn(
                name: "item_name",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.DropColumn(
                name: "specification",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.DropColumn(
                name: "unit",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "material_item_id",
                schema: "public",
                table: "material_receipt_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items",
                column: "material_item_id",
                principalSchema: "public",
                principalTable: "material_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
