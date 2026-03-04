using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertLegTypeToDynamicMultiLeg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop old unique index (on leg_type)
            migrationBuilder.DropIndex(
                name: "idx_all_voyage_leg_unique",
                schema: "public",
                table: "abstract_log_legs");

            // Step 2: Add new columns BEFORE dropping leg_type so we can migrate data
            migrationBuilder.AddColumn<string>(
                name: "leg_label",
                schema: "public",
                table: "abstract_log_legs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "leg_number",
                schema: "public",
                table: "abstract_log_legs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Step 3: Migrate existing data — LEG_OUT → 1, LEG_HOME → 2, others → sequence
            migrationBuilder.Sql(@"
                UPDATE ""public"".""abstract_log_legs""
                SET ""leg_number"" = CASE
                    WHEN ""leg_type"" = 'LEG_OUT' THEN 1
                    WHEN ""leg_type"" = 'LEG_HOME' THEN 2
                    ELSE ""sequence""
                END,
                ""leg_label"" = CASE
                    WHEN ""leg_type"" = 'LEG_OUT' THEN 'Outbound'
                    WHEN ""leg_type"" = 'LEG_HOME' THEN 'Homebound'
                    ELSE 'Leg ' || ""sequence""
                END;
            ");

            // Step 4: Now drop the old column
            migrationBuilder.DropColumn(
                name: "leg_type",
                schema: "public",
                table: "abstract_log_legs");

            // Step 5: Create new unique index (on sequence)
            migrationBuilder.CreateIndex(
                name: "idx_all_voyage_leg_unique",
                schema: "public",
                table: "abstract_log_legs",
                columns: new[] { "abstract_log_voyage_id", "sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_all_voyage_leg_unique",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.DropColumn(
                name: "leg_label",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.DropColumn(
                name: "leg_number",
                schema: "public",
                table: "abstract_log_legs");

            migrationBuilder.AddColumn<string>(
                name: "leg_type",
                schema: "public",
                table: "abstract_log_legs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_all_voyage_leg_unique",
                schema: "public",
                table: "abstract_log_legs",
                columns: new[] { "abstract_log_voyage_id", "leg_type" },
                unique: true);
        }
    }
}
