using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthSessionAndSystemLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                schema: "public",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_until",
                schema: "public",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                schema: "public",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_changed_at",
                schema: "public",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_salt",
                schema: "public",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "login_attempts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_login_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    duration_ms = table.Column<int>(type: "integer", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_system_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    refresh_token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    logout_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    refresh_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    termination_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_login_attempt_ip",
                schema: "public",
                table: "login_attempts",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "idx_login_attempt_time",
                schema: "public",
                table: "login_attempts",
                column: "attempted_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_login_attempt_user_time",
                schema: "public",
                table: "login_attempts",
                columns: new[] { "username", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "idx_login_attempt_username",
                schema: "public",
                table: "login_attempts",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_action",
                schema: "public",
                table: "system_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_category",
                schema: "public",
                table: "system_logs",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_category_timestamp",
                schema: "public",
                table: "system_logs",
                columns: new[] { "category", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_syslog_level",
                schema: "public",
                table: "system_logs",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_session_id",
                schema: "public",
                table: "system_logs",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_synced",
                schema: "public",
                table: "system_logs",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_syslog_timestamp",
                schema: "public",
                table: "system_logs",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_syslog_user_category_time",
                schema: "public",
                table: "system_logs",
                columns: new[] { "user_id", "category", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_syslog_user_id",
                schema: "public",
                table: "system_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_session_access_token",
                schema: "public",
                table: "user_sessions",
                column: "access_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_session_active",
                schema: "public",
                table: "user_sessions",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_session_active_expiry",
                schema: "public",
                table: "user_sessions",
                columns: new[] { "is_active", "access_token_expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_session_login_at",
                schema: "public",
                table: "user_sessions",
                column: "login_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_session_refresh_token",
                schema: "public",
                table: "user_sessions",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_session_user_id",
                schema: "public",
                table: "user_sessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_attempts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "system_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_until",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_changed_at",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_salt",
                schema: "public",
                table: "users");
        }
    }
}
