using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGarbagePartIAndPartII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.CreateTable(
                name: "garbage_record_part_i",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    category = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    estimated_amount_discharged_to_sea = table.Column<double>(type: "double precision", nullable: true),
                    estimated_amount_to_reception_facilities = table.Column<double>(type: "double precision", nullable: true),
                    estimated_amount_incinerated = table.Column<double>(type: "double precision", nullable: true),
                    discharge_latitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    discharge_longitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reception_facility_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    incineration_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    incineration_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    incinerator_details = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    exceptional_discharge_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    officer_in_charge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_garbage_record_part_is", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "garbage_record_part_ii",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    category = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    start_latitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    start_longitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    end_latitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    end_longitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    estimated_amount_discharged_to_sea = table.Column<double>(type: "double precision", nullable: true),
                    estimated_amount_to_reception_facilities = table.Column<double>(type: "double precision", nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reception_facility_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cargo_description = table.Column<string>(type: "text", nullable: false),
                    hold_numbers_washed = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    officer_in_charge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_garbage_record_part_i_is", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_i_category",
                schema: "public",
                table: "garbage_record_part_i",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_i_date",
                schema: "public",
                table: "garbage_record_part_i",
                column: "operation_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_i_synced",
                schema: "public",
                table: "garbage_record_part_i",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_ii_category",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_ii_date",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "operation_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_ii_synced",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.AddForeignKey(
                name: "f_k_material_receipt_items__material_items_material_item_id",
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
                name: "f_k_material_receipt_items__material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items");

            migrationBuilder.DropTable(
                name: "garbage_record_part_i",
                schema: "public");

            migrationBuilder.DropTable(
                name: "garbage_record_part_ii",
                schema: "public");

            migrationBuilder.AddForeignKey(
                name: "f_k_material_receipt_items_material_items_material_item_id",
                schema: "public",
                table: "material_receipt_items",
                column: "material_item_id",
                principalSchema: "public",
                principalTable: "material_items",
                principalColumn: "id");
        }
    }
}
