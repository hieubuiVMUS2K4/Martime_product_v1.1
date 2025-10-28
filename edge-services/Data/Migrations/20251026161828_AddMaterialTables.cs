using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    parent_category_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_categories_material_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalSchema: "public",
                        principalTable: "material_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "material_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    specification = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    on_hand_quantity = table.Column<double>(type: "numeric(14,3)", nullable: false),
                    min_stock = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    max_stock = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    reorder_level = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    reorder_quantity = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    supplier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    batch_tracked = table.Column<bool>(type: "boolean", nullable: false),
                    serial_tracked = table.Column<bool>(type: "boolean", nullable: false),
                    expiry_required = table.Column<bool>(type: "boolean", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_items_material_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "material_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_material_category_active",
                schema: "public",
                table: "material_categories",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_material_category_code_unique",
                schema: "public",
                table: "material_categories",
                column: "category_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_material_category_parent",
                schema: "public",
                table: "material_categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_category_synced",
                schema: "public",
                table: "material_categories",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_active",
                schema: "public",
                table: "material_items",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_barcode",
                schema: "public",
                table: "material_items",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_category",
                schema: "public",
                table: "material_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_code_unique",
                schema: "public",
                table: "material_items",
                column: "item_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_material_item_synced",
                schema: "public",
                table: "material_items",
                column: "is_synced",
                filter: "is_synced = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_categories",
                schema: "public");
        }
    }
}
