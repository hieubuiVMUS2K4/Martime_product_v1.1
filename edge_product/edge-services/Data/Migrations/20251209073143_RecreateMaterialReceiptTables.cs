using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecreateMaterialReceiptTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing MaterialReceiptItems first (foreign key constraint)
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MaterialReceiptItems\" CASCADE;");
            
            // Drop existing MaterialReceipts
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MaterialReceipts\" CASCADE;");
            
            // Create MaterialReceipts table
            migrationBuilder.CreateTable(
                name: "MaterialReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiptCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImportSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImportFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReceipts", x => x.Id);
                });

            // Create MaterialReceiptItems table
            migrationBuilder.CreateTable(
                name: "MaterialReceiptItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiptId = table.Column<int>(type: "integer", nullable: false),
                    MaterialItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReceiptItems_MaterialReceipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "MaterialReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialReceiptItems_material_items_MaterialItemId",
                        column: x => x.MaterialItemId,
                        principalSchema: "public",
                        principalTable: "material_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceipts_ReceiptCode",
                table: "MaterialReceipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceiptItems_ReceiptId",
                table: "MaterialReceiptItems",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceiptItems_MaterialItemId",
                table: "MaterialReceiptItems",
                column: "MaterialItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order
            migrationBuilder.DropTable(name: "MaterialReceiptItems");
            migrationBuilder.DropTable(name: "MaterialReceipts");
        }
    }
}
