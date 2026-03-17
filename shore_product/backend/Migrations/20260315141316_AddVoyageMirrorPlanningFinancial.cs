using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageMirrorPlanningFinancial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "voyage_actual_revenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevenueNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RevenueCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<double>(type: "double precision", nullable: false),
                    AmountUsd = table.Column<double>(type: "double precision", nullable: false),
                    PayerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_actual_revenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_actual_revenues_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_advance_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvanceNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdvanceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<double>(type: "double precision", nullable: false),
                    AmountUsd = table.Column<double>(type: "double precision", nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SettledAmount = table.Column<double>(type: "double precision", nullable: false),
                    UnsettledBalance = table.Column<double>(type: "double precision", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_advance_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_advance_payments_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_bunker_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanLegId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlannedQuantity = table.Column<double>(type: "double precision", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EstimatedCostUsd = table.Column<double>(type: "double precision", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_bunker_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_bunker_plans_voyage_plan_legs_PlanLegId",
                        column: x => x.PlanLegId,
                        principalTable: "voyage_plan_legs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_bunker_plans_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_cargo_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanLegId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CargoType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CargoDescription = table.Column<string>(type: "text", nullable: true),
                    PlannedQuantity = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ShipperName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConsigneeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_cargo_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_cargo_plans_voyage_plan_legs_PlanLegId",
                        column: x => x.PlanLegId,
                        principalTable: "voyage_plan_legs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_cargo_plans_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_cost_estimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CostCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstimatedAmount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_cost_estimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_cost_estimates_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_crew_change_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanLegId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    RankId = table.Column<int>(type: "integer", nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacementReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_crew_change_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_crew_change_plans_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_crew_change_plans_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_crew_change_plans_voyage_plan_legs_PlanLegId",
                        column: x => x.PlanLegId,
                        principalTable: "voyage_plan_legs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_crew_change_plans_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_expense_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AllocationScope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedAmount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<double>(type: "double precision", nullable: false),
                    RequestedAmountUsd = table.Column<double>(type: "double precision", nullable: false),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VendorReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PortCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAmount = table.Column<double>(type: "double precision", nullable: true),
                    ApprovedAmountUsd = table.Column<double>(type: "double precision", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupportingDocuments = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_expense_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_expense_requests_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_revenue_estimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RevenueCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstimatedAmount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_revenue_estimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_revenue_estimates_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalExpenseApproved = table.Column<double>(type: "double precision", nullable: false),
                    TotalAdvanced = table.Column<double>(type: "double precision", nullable: false),
                    TotalDisbursed = table.Column<double>(type: "double precision", nullable: false),
                    TotalRevenue = table.Column<double>(type: "double precision", nullable: false),
                    NetResult = table.Column<double>(type: "double precision", nullable: false),
                    AdvanceBalance = table.Column<double>(type: "double precision", nullable: false),
                    FinalSettlementAmount = table.Column<double>(type: "double precision", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    PreparedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PreparedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_settlements_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_disbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdvancePaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisbursementNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AllocationScope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<double>(type: "double precision", nullable: false),
                    AmountUsd = table.Column<double>(type: "double precision", nullable: false),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PortCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupportingDocuments = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voyage_disbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voyage_disbursements_voyage_advance_payments_AdvancePayment~",
                        column: x => x.AdvancePaymentId,
                        principalTable: "voyage_advance_payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_disbursements_voyage_expense_requests_ExpenseRequest~",
                        column: x => x.ExpenseRequestId,
                        principalTable: "voyage_expense_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_voyage_disbursements_voyage_records_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "voyage_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_voyage_actual_revenues_IsSynced",
                table: "voyage_actual_revenues",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_actual_revenues_RevenueCategory",
                table: "voyage_actual_revenues",
                column: "RevenueCategory");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_actual_revenues_RevenueNumber",
                table: "voyage_actual_revenues",
                column: "RevenueNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_actual_revenues_Status",
                table: "voyage_actual_revenues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_actual_revenues_VoyageId",
                table: "voyage_actual_revenues",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_advance_payments_AdvanceNumber",
                table: "voyage_advance_payments",
                column: "AdvanceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_advance_payments_IsSynced",
                table: "voyage_advance_payments",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_advance_payments_Status",
                table: "voyage_advance_payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_advance_payments_VoyageId",
                table: "voyage_advance_payments",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_bunker_plans_IsSynced",
                table: "voyage_bunker_plans",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_bunker_plans_PlanLegId",
                table: "voyage_bunker_plans",
                column: "PlanLegId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_bunker_plans_VoyageId_Sequence",
                table: "voyage_bunker_plans",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cargo_plans_IsSynced",
                table: "voyage_cargo_plans",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cargo_plans_PlanLegId",
                table: "voyage_cargo_plans",
                column: "PlanLegId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cargo_plans_VoyageId_Sequence",
                table: "voyage_cargo_plans",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cost_estimates_CostCategory",
                table: "voyage_cost_estimates",
                column: "CostCategory");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cost_estimates_IsSynced",
                table: "voyage_cost_estimates",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_cost_estimates_VoyageId_Sequence",
                table: "voyage_cost_estimates",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_CrewMemberId",
                table: "voyage_crew_change_plans",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_IsSynced",
                table: "voyage_crew_change_plans",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_PlanLegId",
                table: "voyage_crew_change_plans",
                column: "PlanLegId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_RankId",
                table: "voyage_crew_change_plans",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_VoyageId_Sequence",
                table: "voyage_crew_change_plans",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_AdvancePaymentId",
                table: "voyage_disbursements",
                column: "AdvancePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_CostCategory",
                table: "voyage_disbursements",
                column: "CostCategory");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_DisbursementNumber",
                table: "voyage_disbursements",
                column: "DisbursementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_ExpenseRequestId",
                table: "voyage_disbursements",
                column: "ExpenseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_IsSynced",
                table: "voyage_disbursements",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_Status",
                table: "voyage_disbursements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_disbursements_VoyageId",
                table: "voyage_disbursements",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_expense_requests_CostCategory",
                table: "voyage_expense_requests",
                column: "CostCategory");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_expense_requests_IsSynced",
                table: "voyage_expense_requests",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_expense_requests_RequestNumber",
                table: "voyage_expense_requests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_expense_requests_Status",
                table: "voyage_expense_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_expense_requests_VoyageId",
                table: "voyage_expense_requests",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_revenue_estimates_IsSynced",
                table: "voyage_revenue_estimates",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_revenue_estimates_RevenueCategory",
                table: "voyage_revenue_estimates",
                column: "RevenueCategory");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_revenue_estimates_VoyageId_Sequence",
                table: "voyage_revenue_estimates",
                columns: new[] { "VoyageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_settlements_IsSynced",
                table: "voyage_settlements",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_settlements_SettlementNumber",
                table: "voyage_settlements",
                column: "SettlementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voyage_settlements_Status",
                table: "voyage_settlements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_voyage_settlements_VoyageId",
                table: "voyage_settlements",
                column: "VoyageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voyage_actual_revenues");

            migrationBuilder.DropTable(
                name: "voyage_bunker_plans");

            migrationBuilder.DropTable(
                name: "voyage_cargo_plans");

            migrationBuilder.DropTable(
                name: "voyage_cost_estimates");

            migrationBuilder.DropTable(
                name: "voyage_crew_change_plans");

            migrationBuilder.DropTable(
                name: "voyage_disbursements");

            migrationBuilder.DropTable(
                name: "voyage_revenue_estimates");

            migrationBuilder.DropTable(
                name: "voyage_settlements");

            migrationBuilder.DropTable(
                name: "voyage_advance_payments");

            migrationBuilder.DropTable(
                name: "voyage_expense_requests");
        }
    }
}
