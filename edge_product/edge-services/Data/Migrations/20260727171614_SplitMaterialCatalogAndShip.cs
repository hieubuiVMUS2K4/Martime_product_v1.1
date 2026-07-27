using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitMaterialCatalogAndShip : Migration
    {
        // ============================================================
        // Tách vật tư trên EDGE (giống Shore):
        //   material_items      → DANH MỤC dùng chung (mới, đồng bộ từ Shore)
        //   material_item_ship  → vật tư theo tàu (đổi tên từ material_items cũ, giữ Id)
        //
        // Hướng RENAME để giữ nguyên dữ liệu + Id (các FK inventory_stock,
        // stock_receipt_items, material_receipt_items... không gãy). Danh mục là bảng MỚI.
        // Relink schedule_spare_parts / material_item_equipments / material_request_items → danh mục.
        // ============================================================

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Gỡ FK material_receipt_items → material_items (sẽ trỏ lại material_item_ship).
            migrationBuilder.Sql(@"ALTER TABLE material_receipt_items DROP CONSTRAINT IF EXISTS ""f_k_material_receipt_items__material_items_material_item_id"";");

            // 2. Đổi tên bảng cũ → material_item_ship (giữ data, Id, PK p_k_material_items).
            migrationBuilder.Sql(@"ALTER TABLE material_items RENAME TO material_item_ship;");

            // 3. Đổi tên FK loại + các index sang tên _ship_.
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship RENAME CONSTRAINT ""FK_material_items_material_categories_category_id"" TO ""FK_material_item_ship_material_categories_category_id"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_code_unique"" RENAME TO ""idx_material_item_ship_code_unique"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_category"" RENAME TO ""idx_material_item_ship_category"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_barcode"" RENAME TO ""idx_material_item_ship_barcode"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_active"" RENAME TO ""idx_material_item_ship_active"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_synced"" RENAME TO ""idx_material_item_ship_synced"";");

            // 4. Thêm cột material_item_code (FK danh mục), backfill = item_code.
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship ADD COLUMN material_item_code varchar(50) NULL;");
            migrationBuilder.Sql(@"UPDATE material_item_ship SET material_item_code = item_code;");
            migrationBuilder.Sql(@"CREATE INDEX ""idx_material_item_ship_catalog_code"" ON material_item_ship (material_item_code);");

            // 5. Tạo bảng DANH MỤC material_items (mới).
            migrationBuilder.Sql(@"
                CREATE TABLE material_items (
                    id uuid NOT NULL,
                    item_code varchar(50) NOT NULL,
                    name varchar(200) NOT NULL,
                    category_id bigint NOT NULL,
                    unit_price numeric(18,4) NULL,
                    is_active boolean NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""p_k_material_catalog_items"" PRIMARY KEY (id),
                    CONSTRAINT ""AK_material_items_item_code"" UNIQUE (item_code),
                    CONSTRAINT ""FK_material_items_material_categories_category_id"" FOREIGN KEY (category_id) REFERENCES material_categories (id) ON DELETE RESTRICT
                );");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ""idx_material_catalog_code_unique"" ON material_items (item_code);");
            migrationBuilder.Sql(@"CREATE INDEX ""idx_material_catalog_category"" ON material_items (category_id);");
            migrationBuilder.Sql(@"CREATE INDEX ""idx_material_catalog_active"" ON material_items (is_active) WHERE is_active = true;");

            // 6. Nạp danh mục từ dữ liệu ship (gộp trùng theo mã).
            migrationBuilder.Sql(@"
                INSERT INTO material_items (id, item_code, name, category_id, unit_price, is_active, created_at, updated_at)
                SELECT gen_random_uuid(), s.item_code, MIN(s.name), MIN(s.category_id), MIN(s.unit_cost), true, now(), now()
                FROM material_item_ship s
                GROUP BY s.item_code;");

            // 7. FK ship.material_item_code → material_items.item_code.
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship ADD CONSTRAINT ""FK_material_item_ship_material_items_material_item_code"" FOREIGN KEY (material_item_code) REFERENCES material_items (item_code) ON DELETE SET NULL;");

            // 8. Relink sang DANH MỤC: remap material_item_id (ship.id → catalog.id) + FK + index.
            // schedule_spare_parts (FK bắt buộc)
            migrationBuilder.Sql(@"UPDATE schedule_spare_parts p SET material_item_id = c.id FROM material_item_ship s, material_items c WHERE p.material_item_id = s.id AND c.item_code = s.item_code;");
            migrationBuilder.Sql(@"DELETE FROM schedule_spare_parts WHERE material_item_id NOT IN (SELECT id FROM material_items);");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_schedule_spare_parts_material_item_id"" ON schedule_spare_parts (material_item_id);");
            migrationBuilder.Sql(@"ALTER TABLE schedule_spare_parts ADD CONSTRAINT ""FK_schedule_spare_parts_material_items_material_item_id"" FOREIGN KEY (material_item_id) REFERENCES material_items (id) ON DELETE RESTRICT;");

            // material_item_equipments (FK bắt buộc)
            migrationBuilder.Sql(@"UPDATE material_item_equipments e SET material_item_id = c.id FROM material_item_ship s, material_items c WHERE e.material_item_id = s.id AND c.item_code = s.item_code;");
            migrationBuilder.Sql(@"DELETE FROM material_item_equipments WHERE material_item_id NOT IN (SELECT id FROM material_items);");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_material_item_equipments_material_item_id"" ON material_item_equipments (material_item_id);");
            migrationBuilder.Sql(@"ALTER TABLE material_item_equipments ADD CONSTRAINT ""FK_material_item_equipments_material_items_material_item_id"" FOREIGN KEY (material_item_id) REFERENCES material_items (id) ON DELETE RESTRICT;");

            // material_request_items (FK nullable)
            migrationBuilder.Sql(@"UPDATE material_request_items r SET material_item_id = c.id FROM material_item_ship s, material_items c WHERE r.material_item_id = s.id AND c.item_code = s.item_code;");
            migrationBuilder.Sql(@"UPDATE material_request_items SET material_item_id = NULL WHERE material_item_id IS NOT NULL AND material_item_id NOT IN (SELECT id FROM material_items);");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_material_request_items_material_item_id"" ON material_request_items (material_item_id);");
            migrationBuilder.Sql(@"ALTER TABLE material_request_items ADD CONSTRAINT ""FK_material_request_items_material_items_material_item_id"" FOREIGN KEY (material_item_id) REFERENCES material_items (id) ON DELETE SET NULL;");

            // 9. Trỏ lại FK material_receipt_items → material_item_ship (dữ liệu vốn là ship.id).
            migrationBuilder.Sql(@"ALTER TABLE material_receipt_items ADD CONSTRAINT ""f_k_material_receipt_items__material_items_material_item_id"" FOREIGN KEY (material_item_id) REFERENCES material_item_ship (id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE material_receipt_items DROP CONSTRAINT IF EXISTS ""f_k_material_receipt_items__material_items_material_item_id"";");
            migrationBuilder.Sql(@"ALTER TABLE schedule_spare_parts DROP CONSTRAINT IF EXISTS ""FK_schedule_spare_parts_material_items_material_item_id"";");
            migrationBuilder.Sql(@"ALTER TABLE material_item_equipments DROP CONSTRAINT IF EXISTS ""FK_material_item_equipments_material_items_material_item_id"";");
            migrationBuilder.Sql(@"ALTER TABLE material_request_items DROP CONSTRAINT IF EXISTS ""FK_material_request_items_material_items_material_item_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_schedule_spare_parts_material_item_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_item_equipments_material_item_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_request_items_material_item_id"";");

            migrationBuilder.Sql(@"ALTER TABLE material_item_ship DROP CONSTRAINT IF EXISTS ""FK_material_item_ship_material_items_material_item_code"";");

            // Bỏ danh mục, khôi phục material_item_ship về material_items cũ.
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS material_items;");
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship DROP COLUMN IF EXISTS material_item_code;");
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship RENAME CONSTRAINT ""FK_material_item_ship_material_categories_category_id"" TO ""FK_material_items_material_categories_category_id"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_ship_code_unique"" RENAME TO ""idx_material_item_code_unique"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_ship_category"" RENAME TO ""idx_material_item_category"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_ship_barcode"" RENAME TO ""idx_material_item_barcode"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_ship_active"" RENAME TO ""idx_material_item_active"";");
            migrationBuilder.Sql(@"ALTER INDEX ""idx_material_item_ship_synced"" RENAME TO ""idx_material_item_synced"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""idx_material_item_ship_catalog_code"";");
            migrationBuilder.Sql(@"ALTER TABLE material_item_ship RENAME TO material_items;");
            migrationBuilder.Sql(@"ALTER TABLE material_receipt_items ADD CONSTRAINT ""f_k_material_receipt_items__material_items_material_item_id"" FOREIGN KEY (material_item_id) REFERENCES material_items (id);");
        }
    }
}
