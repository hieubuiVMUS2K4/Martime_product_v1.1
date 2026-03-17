using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewReviewWorkflowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8727), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8729) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8734), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8734) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8735), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8736) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8737), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8737) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8738), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8739) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8740), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8740) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8741), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8742) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8742), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8743) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8744), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8744) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8745), new DateTime(2026, 3, 15, 18, 6, 45, 708, DateTimeKind.Utc).AddTicks(8746) });

            migrationBuilder.CreateIndex(
                name: "idx_crew_onboard_status",
                schema: "public",
                table: "crew_members",
                column: "onboard_status",
                filter: "onboard_status IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_crew_onboard_status",
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

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7188), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7192) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7196), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7196) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7197), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7198) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7198), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7199) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7200), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7200) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7201), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7201) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7202), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7202) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7203), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7203) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7204), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7205) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7206), new DateTime(2026, 3, 10, 15, 48, 3, 710, DateTimeKind.Utc).AddTicks(7206) });
        }
    }
}
