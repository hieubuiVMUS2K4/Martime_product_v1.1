using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeProvisioningProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "edge_provisioning_profile",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    node_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vessel_imo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    vessel_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vessel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    shore_base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    node_api_token = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    signing_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    key_version = table.Column<int>(type: "integer", nullable: false),
                    protocol_version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    security_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    batch_size = table.Column<int>(type: "integer", nullable: false),
                    sync_interval_sec = table.Column<int>(type: "integer", nullable: false),
                    network_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    schema_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    imported_from = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_handshake_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    handshake_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    last_handshake_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_edge_provisioning_profiles", x => x.id);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3117), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3121) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3126), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3127) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3128), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3128) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3129), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3129) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3130), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3130) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3131), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3132) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3133), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3134) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3134), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3135) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3135), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3136) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3136), new DateTime(2026, 7, 28, 9, 34, 37, 607, DateTimeKind.Utc).AddTicks(3137) });

            migrationBuilder.CreateIndex(
                name: "idx_epp_is_active",
                schema: "public",
                table: "edge_provisioning_profile",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edge_provisioning_profile",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2417), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2422) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2423), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2423) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2424), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2425) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2425), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2426) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2427), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2427) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2428), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2428) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2429), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2431), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2432), new DateTime(2026, 7, 1, 6, 35, 4, 961, DateTimeKind.Utc).AddTicks(2433) });
        }
    }
}
