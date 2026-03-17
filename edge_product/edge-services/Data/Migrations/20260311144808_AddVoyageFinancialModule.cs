using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageFinancialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "actual_profit_margin",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "financial_closed_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "financial_closed_by",
                schema: "public",
                table: "voyage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "financial_status",
                schema: "public",
                table: "voyage_records",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "OPEN");

            migrationBuilder.AddColumn<double>(
                name: "outstanding_balance",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_actual_cost",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_actual_revenue",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_advanced",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_disbursed",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "voyage_actual_revenues",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revenue_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revenue_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<double>(type: "double precision", nullable: false),
                    amount_usd = table.Column<double>(type: "double precision", nullable: false),
                    payer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_actual_revenues", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_actual_revenues__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_advance_payments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    advance_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    advance_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<double>(type: "double precision", nullable: false),
                    amount_usd = table.Column<double>(type: "double precision", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payment_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    settled_amount = table.Column<double>(type: "double precision", nullable: false),
                    unsettled_balance = table.Column<double>(type: "double precision", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_advance_payments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_advance_payments__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_expense_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cost_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    allocation_scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    requested_amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<double>(type: "double precision", nullable: false),
                    requested_amount_usd = table.Column<double>(type: "double precision", nullable: false),
                    vendor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vendor_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_amount = table.Column<double>(type: "double precision", nullable: true),
                    approved_amount_usd = table.Column<double>(type: "double precision", nullable: true),
                    approval_notes = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    supporting_documents = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_expense_requests", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_expense_requests__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_settlements",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_expense_approved = table.Column<double>(type: "double precision", nullable: false),
                    total_advanced = table.Column<double>(type: "double precision", nullable: false),
                    total_disbursed = table.Column<double>(type: "double precision", nullable: false),
                    total_revenue = table.Column<double>(type: "double precision", nullable: false),
                    net_result = table.Column<double>(type: "double precision", nullable: false),
                    advance_balance = table.Column<double>(type: "double precision", nullable: false),
                    final_settlement_amount = table.Column<double>(type: "double precision", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    prepared_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    prepared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approval_notes = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_settlements", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_settlements_voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_disbursements",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    advance_payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    disbursement_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cost_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    allocation_scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<double>(type: "double precision", nullable: false),
                    amount_usd = table.Column<double>(type: "double precision", nullable: false),
                    vendor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    supporting_documents = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_disbursements", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_disbursements__voyage_expense_requests_expense_request_~",
                        column: x => x.expense_request_id,
                        principalSchema: "public",
                        principalTable: "voyage_expense_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_voyage_disbursements__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_voyage_disbursements_voyage_advance_payments_advance_paymen~",
                        column: x => x.advance_payment_id,
                        principalSchema: "public",
                        principalTable: "voyage_advance_payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2834), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2836) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2837), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2838) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2876), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2876) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2892), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2892) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2893), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2893) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2894), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2895) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2895), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2896) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2897), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2897) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2898), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2898) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2899), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2899) });

            migrationBuilder.CreateIndex(
                name: "idx_actual_revenue_category",
                schema: "public",
                table: "voyage_actual_revenues",
                column: "revenue_category");

            migrationBuilder.CreateIndex(
                name: "idx_actual_revenue_number_unique",
                schema: "public",
                table: "voyage_actual_revenues",
                column: "revenue_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_actual_revenue_status",
                schema: "public",
                table: "voyage_actual_revenues",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_actual_revenue_voyage",
                schema: "public",
                table: "voyage_actual_revenues",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_advance_payment_number_unique",
                schema: "public",
                table: "voyage_advance_payments",
                column: "advance_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_advance_payment_status",
                schema: "public",
                table: "voyage_advance_payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_advance_payment_voyage",
                schema: "public",
                table: "voyage_advance_payments",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_advance_payment",
                schema: "public",
                table: "voyage_disbursements",
                column: "advance_payment_id");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_category",
                schema: "public",
                table: "voyage_disbursements",
                column: "cost_category");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_expense_request",
                schema: "public",
                table: "voyage_disbursements",
                column: "expense_request_id");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_number_unique",
                schema: "public",
                table: "voyage_disbursements",
                column: "disbursement_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_status",
                schema: "public",
                table: "voyage_disbursements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_voyage",
                schema: "public",
                table: "voyage_disbursements",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_expense_request_category",
                schema: "public",
                table: "voyage_expense_requests",
                column: "cost_category");

            migrationBuilder.CreateIndex(
                name: "idx_expense_request_number_unique",
                schema: "public",
                table: "voyage_expense_requests",
                column: "request_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_expense_request_status",
                schema: "public",
                table: "voyage_expense_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_expense_request_voyage",
                schema: "public",
                table: "voyage_expense_requests",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_settlement_number_unique",
                schema: "public",
                table: "voyage_settlements",
                column: "settlement_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_settlement_status",
                schema: "public",
                table: "voyage_settlements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_settlement_voyage",
                schema: "public",
                table: "voyage_settlements",
                column: "voyage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voyage_actual_revenues",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_disbursements",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_settlements",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_expense_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_advance_payments",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "actual_profit_margin",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "financial_closed_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "financial_closed_by",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "financial_status",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "outstanding_balance",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_actual_cost",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_actual_revenue",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_advanced",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_disbursed",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(6), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(7) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(9), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(9) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(10), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(10) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(11), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(11) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(12), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(12) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(13), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(14) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(14), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(15) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(16), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(16) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(17), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(17) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(18), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(18) });
        }
    }
}
