using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialWorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "material_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    urgency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    needed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
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
                    supplier_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
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
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4817), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4822) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4829), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4829) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4831), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4831) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4838), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4838) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4839), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4839) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4840), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4841) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4842), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4842) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4843), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4843) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4844), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4845) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4845), new DateTime(2026, 3, 11, 3, 34, 44, 794, DateTimeKind.Utc).AddTicks(4846) });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_stock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_request_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipt_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_requests",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6906), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6915), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6915) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6916), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6917) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6917), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6918) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6919), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6919) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6920), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6920) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6921), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6921) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6922), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6922) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6923), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6923) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6924), new DateTime(2026, 3, 11, 2, 36, 0, 170, DateTimeKind.Utc).AddTicks(6924) });
        }
    }
}
