using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreLocationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "store_locations",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(167), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(193) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(222), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(222) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(224), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(224) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(225), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(225) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(226), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(226) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(227), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(228) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(228), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(229) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(235), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(236) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(236), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(237) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(237), new DateTime(2026, 3, 10, 16, 41, 59, 201, DateTimeKind.Utc).AddTicks(238) });
        }
    }
}
