using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStopTimeAndWaterDepthToGarbageRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "operation_end_time",
                schema: "public",
                table: "garbage_record_part_ii",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "operation_end_time",
                schema: "public",
                table: "garbage_record_part_i",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "water_depth",
                schema: "public",
                table: "garbage_record_part_i",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "operation_end_time",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropColumn(
                name: "operation_end_time",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropColumn(
                name: "water_depth",
                schema: "public",
                table: "garbage_record_part_i");
        }
    }
}
