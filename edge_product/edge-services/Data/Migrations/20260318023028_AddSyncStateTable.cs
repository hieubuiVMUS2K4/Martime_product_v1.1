using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncStateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules");

            // Use IF EXISTS to safely skip if column was already dropped in a previous migration
            migrationBuilder.Sql("ALTER TABLE public.crew_members DROP COLUMN IF EXISTS nationality;");

            migrationBuilder.RenameIndex(
                name: "IX_maintenance_schedules_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                newName: "idx_schedule_equipment_group");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                schema: "public",
                table: "equipment_assets",
                newName: "parent_id");

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                schema: "public",
                table: "material_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "equipment_asset_name",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maintenance_category",
                schema: "public",
                table: "maintenance_schedules",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                schema: "public",
                table: "crew_members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "edge_changes",
                schema: "public",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "edge_changes_viewed",
                schema: "public",
                table: "crew_members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "onboard_status",
                schema: "public",
                table: "crew_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "onboard_status_changed_at",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onboard_status_changed_by",
                schema: "public",
                table: "crew_members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "review_checklist",
                schema: "public",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "review_notes",
                schema: "public",
                table: "crew_members",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inventory_stock",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    last_receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_inventory_stocks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material_item_equipments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_item_equipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vessel_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    voyage_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    urgency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    needed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    attachments = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "store_locations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    manager_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_store_locations", x => x.id);
                    table.ForeignKey(
                        name: "f_k_store_locations_store_locations_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "public",
                        principalTable: "store_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sync_state",
                schema: "public",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sync_state", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "material_request_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_id = table.Column<int>(type: "integer", nullable: false),
                    equipment_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity_on_hand = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    quantity_requested = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_request_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_material_request_items_material_requests_request_id",
                        column: x => x.request_id,
                        principalSchema: "public",
                        principalTable: "material_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_receipts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vessel_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    voyage_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    supplier_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    attachments = table.Column<string>(type: "text", nullable: true),
                    material_request_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_stock_receipts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_stock_receipts_material_requests_material_request_id",
                        column: x => x.material_request_id,
                        principalSchema: "public",
                        principalTable: "material_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "stock_receipt_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_id = table.Column<int>(type: "integer", nullable: false),
                    store_location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity_requested = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_stock_receipt_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_stock_receipt_items_stock_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalSchema: "public",
                        principalTable: "stock_receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(74), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(77) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(83), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(83) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(84), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(84) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(85), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(85) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(86), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(86) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(87), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(88) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(88), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(89) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(89), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(89) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(90), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(90) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(91), new DateTime(2026, 3, 18, 2, 30, 26, 368, DateTimeKind.Utc).AddTicks(91) });

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_equipment_asset",
                schema: "public",
                table: "maintenance_tasks",
                column: "equipment_asset_id");

            migrationBuilder.CreateIndex(
                name: "idx_schedule_equipment_asset",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "idx_crew_country_id",
                schema: "public",
                table: "crew_members",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "idx_crew_onboard_status",
                schema: "public",
                table: "crew_members",
                column: "onboard_status",
                filter: "onboard_status IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "uk_inventory_material_location",
                schema: "public",
                table: "inventory_stock",
                columns: new[] { "material_item_id", "store_location_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_request_items_request_id",
                schema: "public",
                table: "material_request_items",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_request_status",
                schema: "public",
                table: "material_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "uk_material_request_code",
                schema: "public",
                table: "material_requests",
                column: "request_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_receipt_items_receipt_id",
                schema: "public",
                table: "stock_receipt_items",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_receipt_status",
                schema: "public",
                table: "stock_receipts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_stock_receipts_material_request_id",
                schema: "public",
                table: "stock_receipts",
                column: "material_request_id");

            migrationBuilder.CreateIndex(
                name: "uk_stock_receipt_code",
                schema: "public",
                table: "stock_receipts",
                column: "receipt_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_store_location_active",
                schema: "public",
                table: "store_locations",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_store_location_synced",
                schema: "public",
                table: "store_locations",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "IX_store_locations_parent_id",
                schema: "public",
                table: "store_locations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "uk_store_locations_code",
                schema: "public",
                table: "store_locations",
                column: "location_code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_crew_members_countries_country_id",
                schema: "public",
                table: "crew_members",
                column: "country_id",
                principalSchema: "public",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_schedules_equipment_assets_equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_asset_id",
                principalSchema: "public",
                principalTable: "equipment_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_group_id",
                principalSchema: "public",
                principalTable: "equipment_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_crew_members_countries_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_assets_equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "inventory_stock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_item_equipments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_request_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipt_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "store_locations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sync_state",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_requests",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_equipment_asset",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_schedule_equipment_asset",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropIndex(
                name: "IX_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets");

            migrationBuilder.DropIndex(
                name: "idx_crew_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropIndex(
                name: "idx_crew_onboard_status",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_asset_name",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "maintenance_category",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "edge_changes",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "edge_changes_viewed",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status_changed_at",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status_changed_by",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "review_checklist",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "review_notes",
                schema: "public",
                table: "crew_members");

            migrationBuilder.RenameIndex(
                name: "idx_schedule_equipment_group",
                schema: "public",
                table: "maintenance_schedules",
                newName: "IX_maintenance_schedules_equipment_group_id");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                schema: "public",
                table: "equipment_assets",
                newName: "ParentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8764), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8774) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8779), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8780) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8785), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8786) });

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_group_id",
                principalSchema: "public",
                principalTable: "equipment_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
