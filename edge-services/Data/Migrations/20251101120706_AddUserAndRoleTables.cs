using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndRoleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_crew_members_crew_id",
                schema: "public",
                table: "crew_members",
                column: "crew_id");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_crew_members_crew_id",
                        column: x => x.crew_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "crew_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_role_active",
                schema: "public",
                table: "roles",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_role_code_unique",
                schema: "public",
                table: "roles",
                column: "role_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_active",
                schema: "public",
                table: "users",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_user_crew_id",
                schema: "public",
                table: "users",
                column: "crew_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_role_id",
                schema: "public",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_username_unique",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_crew_members_crew_id",
                schema: "public",
                table: "crew_members");
        }
    }
}
