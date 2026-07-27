using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class RelinkMaterialFksToCatalog : Migration
    {
        // ============================================================
        // Chuyển 3 khoá ngoại từ material_item_ship → DANH MỤC material_items:
        //   - schedule_spare_parts.MaterialItemId     (định mức phụ tùng theo loại)
        //   - material_item_equipments.MaterialItemId (vật tư dùng cho thiết bị)
        //   - material_request_items.MaterialItemId   (yêu cầu theo loại, nullable)
        //
        // Trước đó các cột này lưu material_item_ship.Id. Remap sang material_items.Id
        // qua mã vật tư: material_item_ship.MaterialItemCode = material_items.ItemCode.
        // Dòng không map được (mồ côi) bị xoá (FK bắt buộc) hoặc set NULL (FK nullable).
        // ============================================================

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Remap dữ liệu ship.Id → catalog.Id ──
            migrationBuilder.Sql(@"
                UPDATE schedule_spare_parts p
                SET ""MaterialItemId"" = c.""Id""
                FROM material_item_ship s, material_items c
                WHERE p.""MaterialItemId"" = s.""Id"" AND c.""ItemCode"" = s.""MaterialItemCode"";");
            migrationBuilder.Sql(@"
                DELETE FROM schedule_spare_parts
                WHERE ""MaterialItemId"" NOT IN (SELECT ""Id"" FROM material_items);");

            migrationBuilder.Sql(@"
                UPDATE material_item_equipments e
                SET ""MaterialItemId"" = c.""Id""
                FROM material_item_ship s, material_items c
                WHERE e.""MaterialItemId"" = s.""Id"" AND c.""ItemCode"" = s.""MaterialItemCode"";");
            migrationBuilder.Sql(@"
                DELETE FROM material_item_equipments
                WHERE ""MaterialItemId"" NOT IN (SELECT ""Id"" FROM material_items);");

            migrationBuilder.Sql(@"
                UPDATE material_request_items r
                SET ""MaterialItemId"" = c.""Id""
                FROM material_item_ship s, material_items c
                WHERE r.""MaterialItemId"" = s.""Id"" AND c.""ItemCode"" = s.""MaterialItemCode"";");
            migrationBuilder.Sql(@"
                UPDATE material_request_items
                SET ""MaterialItemId"" = NULL
                WHERE ""MaterialItemId"" IS NOT NULL
                  AND ""MaterialItemId"" NOT IN (SELECT ""Id"" FROM material_items);");

            // ── 2. Index cho các FK mới ──
            migrationBuilder.CreateIndex(
                name: "IX_schedule_spare_parts_MaterialItemId",
                table: "schedule_spare_parts",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_material_request_items_MaterialItemId",
                table: "material_request_items",
                column: "MaterialItemId");

            // ── 3. Thêm khoá ngoại trỏ danh mục ──
            migrationBuilder.AddForeignKey(
                name: "FK_material_item_equipments_material_items_MaterialItemId",
                table: "material_item_equipments",
                column: "MaterialItemId",
                principalTable: "material_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_material_request_items_material_items_MaterialItemId",
                table: "material_request_items",
                column: "MaterialItemId",
                principalTable: "material_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_schedule_spare_parts_material_items_MaterialItemId",
                table: "schedule_spare_parts",
                column: "MaterialItemId",
                principalTable: "material_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_item_equipments_material_items_MaterialItemId",
                table: "material_item_equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_material_request_items_material_items_MaterialItemId",
                table: "material_request_items");

            migrationBuilder.DropForeignKey(
                name: "FK_schedule_spare_parts_material_items_MaterialItemId",
                table: "schedule_spare_parts");

            migrationBuilder.DropIndex(
                name: "IX_schedule_spare_parts_MaterialItemId",
                table: "schedule_spare_parts");

            migrationBuilder.DropIndex(
                name: "IX_material_request_items_MaterialItemId",
                table: "material_request_items");

            // Lưu ý: không remap ngược catalog.Id → ship.Id (dữ liệu gốc có thể đã mất do
            // xoá dòng mồ côi ở Up). Down chỉ gỡ index + khoá ngoại.
        }
    }
}
