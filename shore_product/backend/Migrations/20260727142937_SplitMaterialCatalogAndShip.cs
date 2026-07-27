using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class SplitMaterialCatalogAndShip : Migration
    {
        // ============================================================
        // Tách bảng vật tư thành 2:
        //   material_items       → DANH MỤC dùng chung toàn fleet (mới)
        //   material_item_ship   → vật tư theo từng tàu/kho (đổi tên từ material_items cũ)
        //
        // Quan trọng: bảng material_items CŨ (đang chứa dữ liệu ship + được
        // InventoryStock / material_item_equipments / ScheduleSparePart tham chiếu
        // qua Id) được ĐỔI TÊN thành material_item_ship để GIỮ NGUYÊN Id,
        // tránh làm gãy các khoá ngoại hiện có. Danh mục material_items là bảng MỚI.
        // ============================================================

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Gỡ các index cũ trên material_items (dù được tạo bằng EF hay raw SQL).
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_items_VesselId_ItemCode"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_material_items_ItemCode"";");

            // 2. Đổi tên bảng cũ → material_item_ship (giữ nguyên toàn bộ dữ liệu + Id).
            migrationBuilder.RenameTable(name: "material_items", newName: "material_item_ship");
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_material_items') THEN
                        ALTER TABLE material_item_ship RENAME CONSTRAINT ""PK_material_items"" TO ""PK_material_item_ship"";
                    END IF;
                END $$;");

            // 3. Cột mã vật tư cũ (ItemCode) → mã vật tư trên tàu (ShipItemCode).
            migrationBuilder.RenameColumn(name: "ItemCode", table: "material_item_ship", newName: "ShipItemCode");

            // 4. Thêm cột MaterialItemCode (mã vật tư — FK về danh mục), backfill = mã cũ.
            migrationBuilder.AddColumn<string>(
                name: "MaterialItemCode", table: "material_item_ship",
                type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "");
            migrationBuilder.Sql(@"UPDATE material_item_ship SET ""MaterialItemCode"" = ""ShipItemCode"";");

            // 5. Tạo bảng danh mục material_items (MỚI).
            migrationBuilder.CreateTable(
                name: "material_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_items", x => x.Id);
                    table.UniqueConstraint("AK_material_items_ItemCode", x => x.ItemCode);
                });

            // 6. Nạp danh mục từ dữ liệu ship: gộp trùng theo mã, tên lấy từ cột Name cũ,
            //    đơn giá danh mục lấy từ UnitCost.
            migrationBuilder.Sql(@"
                INSERT INTO material_items (""Id"", ""ItemCode"", ""Name"", ""CategoryId"", ""UnitPrice"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                SELECT gen_random_uuid(), s.""ShipItemCode"", MIN(s.""Name""), MIN(s.""CategoryId""), MIN(s.""UnitCost""), true, now(), now()
                FROM material_item_ship s
                GROUP BY s.""ShipItemCode"";");

            // 7. Cột tên cũ trên ship đã chuyển vào danh mục → bỏ đi (vai trò thay bằng MaterialItemCode).
            migrationBuilder.DropColumn(name: "Name", table: "material_item_ship");

            // 8. Index + khoá ngoại.
            migrationBuilder.CreateIndex(
                name: "IX_material_items_ItemCode", table: "material_items",
                column: "ItemCode", unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_item_ship_MaterialItemCode", table: "material_item_ship",
                column: "MaterialItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_material_item_ship_VesselId_ShipItemCode", table: "material_item_ship",
                columns: new[] { "VesselId", "ShipItemCode" }, unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_material_item_ship_material_items_MaterialItemCode",
                table: "material_item_ship", column: "MaterialItemCode",
                principalTable: "material_items", principalColumn: "ItemCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_item_ship_material_items_MaterialItemCode", table: "material_item_ship");
            migrationBuilder.DropIndex(name: "IX_material_item_ship_MaterialItemCode", table: "material_item_ship");
            migrationBuilder.DropIndex(name: "IX_material_item_ship_VesselId_ShipItemCode", table: "material_item_ship");

            // Khôi phục cột Name từ danh mục trước khi bỏ danh mục.
            migrationBuilder.AddColumn<string>(
                name: "Name", table: "material_item_ship",
                type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "");
            migrationBuilder.Sql(@"
                UPDATE material_item_ship s SET ""Name"" = c.""Name""
                FROM material_items c WHERE s.""MaterialItemCode"" = c.""ItemCode"";");

            migrationBuilder.DropTable(name: "material_items");

            migrationBuilder.DropColumn(name: "MaterialItemCode", table: "material_item_ship");
            migrationBuilder.RenameColumn(name: "ShipItemCode", table: "material_item_ship", newName: "ItemCode");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_material_item_ship') THEN
                        ALTER TABLE material_item_ship RENAME CONSTRAINT ""PK_material_item_ship"" TO ""PK_material_items"";
                    END IF;
                END $$;");
            migrationBuilder.RenameTable(name: "material_item_ship", newName: "material_items");

            migrationBuilder.CreateIndex(
                name: "IX_material_items_VesselId_ItemCode", table: "material_items",
                columns: new[] { "VesselId", "ItemCode" }, unique: true);
        }
    }
}
