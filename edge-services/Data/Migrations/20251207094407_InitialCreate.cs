using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty migration - database schema already exists from init-scripts/01-init-schema.sql
            // This migration serves as a baseline snapshot for EF Core
            // All tables are created by Docker entrypoint script on first container startup
            // Future schema changes should use new migrations with proper ALTER TABLE statements
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty down migration - do not drop any tables
            // Tables are managed by init-scripts for initial setup
        }
    }
}
