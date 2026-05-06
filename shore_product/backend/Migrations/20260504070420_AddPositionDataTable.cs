using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionData",
                table: "PositionData");

            migrationBuilder.RenameTable(
                name: "PositionData",
                newName: "position_data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_position_data",
                table: "position_data",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_position_data_OriginNode",
                table: "position_data",
                column: "OriginNode");

            migrationBuilder.CreateIndex(
                name: "IX_position_data_OriginNode_Timestamp",
                table: "position_data",
                columns: new[] { "OriginNode", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_position_data_Timestamp",
                table: "position_data",
                column: "Timestamp",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_position_data",
                table: "position_data");

            migrationBuilder.DropIndex(
                name: "IX_position_data_OriginNode",
                table: "position_data");

            migrationBuilder.DropIndex(
                name: "IX_position_data_OriginNode_Timestamp",
                table: "position_data");

            migrationBuilder.DropIndex(
                name: "IX_position_data_Timestamp",
                table: "position_data");

            migrationBuilder.RenameTable(
                name: "position_data",
                newName: "PositionData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionData",
                table: "PositionData",
                column: "Id");
        }
    }
}
