using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowPerEquipmentScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.RenameIndex(
                name: "IX_maintenance_schedules_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                newName: "idx_schedule_equipment_group");

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

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7495), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7500) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7507), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7507) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7508), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7508) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7527), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7527) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7528), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7528) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7529), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7531), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7531) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7532), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7532) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7533), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7533) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7534), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7535) });

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
                name: "FK_maintenance_schedules_equipment_assets_equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_equipment_asset",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_schedule_equipment_asset",
                schema: "public",
                table: "maintenance_schedules");

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

            migrationBuilder.RenameIndex(
                name: "idx_schedule_equipment_group",
                schema: "public",
                table: "maintenance_schedules",
                newName: "IX_maintenance_schedules_equipment_group_id");

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

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(415), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(419) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(425), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(426) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(427), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(427) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(428), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(428) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(429), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(430), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(430) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(431), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(432), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(432) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(433), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(433) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(434), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(434) });

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
