using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EngineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RpmAtEvent = table.Column<double>(type: "double precision", nullable: true),
                    TriggerSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineEvents", x => x.Id);
                });

            // Index cho query lọc theo tàu (OriginNode) và sắp xếp theo thời gian
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_EngineEvents_OriginNode_Timestamp""
                ON public.""EngineEvents"" (""OriginNode"", ""Timestamp"" DESC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineEvents");
        }
    }
}
