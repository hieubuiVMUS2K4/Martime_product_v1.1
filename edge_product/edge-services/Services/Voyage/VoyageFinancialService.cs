using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Constants;
using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;

namespace MaritimeEdge.Services.Voyage;

public interface IVoyageFinancialService
{
    // Overview
    Task<VoyageFinancialOverviewDto?> GetFinancialOverviewAsync(Guid voyageId);

    // Expense Requests
    Task<List<VoyageExpenseRequestDto>> GetExpenseRequestsAsync(Guid voyageId);
    Task<VoyageExpenseRequestDto?> GetExpenseRequestAsync(Guid id);
    Task<VoyageExpenseRequestDto> CreateExpenseRequestAsync(Guid voyageId, CreateExpenseRequestDto dto, string user);
    Task<VoyageExpenseRequestDto?> UpdateExpenseRequestAsync(Guid id, UpdateExpenseRequestDto dto);
    Task<bool> TransitionExpenseRequestAsync(Guid id, string newStatus, string user, ApproveExpenseRequestDto? approval = null);
    Task<bool> DeleteExpenseRequestAsync(Guid id);

    // Advance Payments
    Task<List<VoyageAdvancePaymentDto>> GetAdvancePaymentsAsync(Guid voyageId);
    Task<VoyageAdvancePaymentDto> CreateAdvancePaymentAsync(Guid voyageId, CreateAdvancePaymentDto dto);
    Task<VoyageAdvancePaymentDto?> UpdateAdvancePaymentAsync(Guid id, UpdateAdvancePaymentDto dto);
    Task<bool> MarkAdvancePaidAsync(Guid id, string paidBy, string? paymentRef);
    Task<bool> DeleteAdvancePaymentAsync(Guid id);

    // Disbursements
    Task<List<VoyageDisbursementDto>> GetDisbursementsAsync(Guid voyageId);
    Task<VoyageDisbursementDto> CreateDisbursementAsync(Guid voyageId, CreateDisbursementDto dto);
    Task<VoyageDisbursementDto?> UpdateDisbursementAsync(Guid id, UpdateDisbursementDto dto);
    Task<bool> TransitionDisbursementAsync(Guid id, string newStatus, string user, string? paymentRef = null);
    Task<bool> DeleteDisbursementAsync(Guid id);

    // Actual Revenue
    Task<List<VoyageActualRevenueDto>> GetActualRevenuesAsync(Guid voyageId);
    Task<VoyageActualRevenueDto> CreateActualRevenueAsync(Guid voyageId, CreateActualRevenueDto dto);
    Task<VoyageActualRevenueDto?> UpdateActualRevenueAsync(Guid id, UpdateActualRevenueDto dto);
    Task<bool> TransitionRevenueAsync(Guid id, string newStatus, string? paymentRef = null);
    Task<bool> DeleteActualRevenueAsync(Guid id);

    // Settlements
    Task<List<VoyageSettlementDto>> GetSettlementsAsync(Guid voyageId);
    Task<VoyageSettlementDto> CreateSettlementAsync(Guid voyageId, CreateSettlementDto dto, string user);
    Task<VoyageSettlementDto?> UpdateSettlementAsync(Guid id, UpdateSettlementDto dto);
    Task<bool> TransitionSettlementAsync(Guid id, string newStatus, string user, string? notes = null);

    // Financial Closing
    Task<bool> CloseVoyageFinancialsAsync(Guid voyageId, string user, string? notes = null);
}

public class VoyageFinancialService : IVoyageFinancialService
{
    private readonly EdgeDbContext _db;

    public VoyageFinancialService(EdgeDbContext db)
    {
        _db = db;
    }

    // ================================================================
    // FINANCIAL OVERVIEW
    // ================================================================

    public async Task<VoyageFinancialOverviewDto?> GetFinancialOverviewAsync(Guid voyageId)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.CostEstimates)
            .Include(v => v.RevenueEstimates)
            .Include(v => v.ExpenseRequests)
            .Include(v => v.AdvancePayments)
            .Include(v => v.Disbursements)
            .Include(v => v.ActualRevenues)
            .Include(v => v.Settlements)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return null;

        var totalEstCost = voyage.CostEstimates.Sum(c => c.EstimatedAmount);
        var totalEstRevenue = voyage.RevenueEstimates.Sum(r => r.EstimatedAmount);
        var totalActualCost = voyage.Disbursements.Sum(d => d.AmountUsd);
        var totalActualRevenue = voyage.ActualRevenues.Sum(r => r.AmountUsd);
        var totalAdvanced = voyage.AdvancePayments
            .Where(a => a.Status == AdvancePaymentStatus.PAID || a.Status == AdvancePaymentStatus.SETTLED)
            .Sum(a => a.AmountUsd);
        var totalDisbursed = voyage.Disbursements
            .Where(d => d.Status == DisbursementStatus.PAID)
            .Sum(d => d.AmountUsd);

        return new VoyageFinancialOverviewDto
        {
            VoyageId = voyage.Id,
            VoyageNumber = voyage.VoyageNumber,
            VoyageStatus = voyage.VoyageStatus,
            FinancialStatus = voyage.FinancialStatus,

            TotalEstimatedCost = totalEstCost,
            TotalEstimatedRevenue = totalEstRevenue,
            EstimatedProfitMargin = totalEstRevenue - totalEstCost,

            TotalActualCost = totalActualCost,
            TotalActualRevenue = totalActualRevenue,
            ActualProfitMargin = totalActualRevenue - totalActualCost,

            CostVariance = totalActualCost - totalEstCost,
            RevenueVariance = totalActualRevenue - totalEstRevenue,
            ProfitVariance = (totalActualRevenue - totalActualCost) - (totalEstRevenue - totalEstCost),

            TotalAdvanced = totalAdvanced,
            TotalDisbursed = totalDisbursed,
            OutstandingBalance = totalAdvanced - totalDisbursed,

            ExpenseRequestCount = voyage.ExpenseRequests.Count,
            PendingExpenseCount = voyage.ExpenseRequests.Count(e =>
                e.Status == ExpenseRequestStatus.SUBMITTED),
            AdvancePaymentCount = voyage.AdvancePayments.Count,
            DisbursementCount = voyage.Disbursements.Count,
            RevenueCount = voyage.ActualRevenues.Count,
            SettlementCount = voyage.Settlements.Count,

            EstimatedCostBreakdown = BuildCostBreakdown(
                voyage.CostEstimates.GroupBy(c => c.CostCategory)
                    .Select(g => (g.Key, g.Sum(c => c.EstimatedAmount))).ToList(), totalEstCost),
            ActualCostBreakdown = BuildCostBreakdown(
                voyage.Disbursements.GroupBy(d => d.CostCategory)
                    .Select(g => (g.Key, g.Sum(d => d.AmountUsd))).ToList(), totalActualCost),
            EstimatedRevenueBreakdown = voyage.RevenueEstimates.GroupBy(r => r.RevenueCategory)
                .Select(g => new RevenueBreakdownItem
                {
                    Category = g.Key,
                    Amount = g.Sum(r => r.EstimatedAmount),
                    Percentage = totalEstRevenue > 0 ? g.Sum(r => r.EstimatedAmount) / totalEstRevenue * 100 : 0,
                }).ToList(),
            ActualRevenueBreakdown = voyage.ActualRevenues.GroupBy(r => r.RevenueCategory)
                .Select(g => new RevenueBreakdownItem
                {
                    Category = g.Key,
                    Amount = g.Sum(r => r.AmountUsd),
                    Percentage = totalActualRevenue > 0 ? g.Sum(r => r.AmountUsd) / totalActualRevenue * 100 : 0,
                }).ToList(),
            CostAllocationBreakdown = voyage.Disbursements.GroupBy(d => d.AllocationScope)
                .Select(g => new AllocationBreakdownItem
                {
                    Scope = g.Key,
                    Amount = g.Sum(d => d.AmountUsd),
                    Percentage = totalActualCost > 0 ? g.Sum(d => d.AmountUsd) / totalActualCost * 100 : 0,
                }).ToList(),

            FinancialClosedAt = voyage.FinancialClosedAt,
            FinancialClosedBy = voyage.FinancialClosedBy,
        };
    }

    private static List<CostBreakdownItem> BuildCostBreakdown(
        List<(string Key, double Sum)> groups, double total)
    {
        return groups.Select(g => new CostBreakdownItem
        {
            Category = g.Key,
            Amount = g.Sum,
            Percentage = total > 0 ? g.Sum / total * 100 : 0,
        }).ToList();
    }

    // ================================================================
    // EXPENSE REQUESTS
    // ================================================================

    public async Task<List<VoyageExpenseRequestDto>> GetExpenseRequestsAsync(Guid voyageId)
    {
        return await _db.VoyageExpenseRequests
            .Where(e => e.VoyageId == voyageId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapExpenseRequest(e))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VoyageExpenseRequestDto?> GetExpenseRequestAsync(Guid id)
    {
        var e = await _db.VoyageExpenseRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return e == null ? null : MapExpenseRequest(e);
    }

    public async Task<VoyageExpenseRequestDto> CreateExpenseRequestAsync(
        Guid voyageId, CreateExpenseRequestDto dto, string user)
    {
        var count = await _db.VoyageExpenseRequests.CountAsync(e => e.VoyageId == voyageId);
        var entity = new VoyageExpenseRequest
        {
            VoyageId = voyageId,
            RequestNumber = $"EXP-{count + 1:D4}",
            CostCategory = dto.CostCategory,
            AllocationScope = dto.AllocationScope,
            Description = dto.Description,
            RequestedAmount = dto.RequestedAmount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            RequestedAmountUsd = dto.RequestedAmount * dto.ExchangeRate,
            VendorName = dto.VendorName,
            VendorReference = dto.VendorReference,
            PortCode = dto.PortCode,
            PortName = dto.PortName,
            Status = ExpenseRequestStatus.DRAFT,
            RequestedBy = user,
            RequestedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            SupportingDocuments = dto.SupportingDocuments,
        };

        _db.VoyageExpenseRequests.Add(entity);
        await _db.SaveChangesAsync();
        return MapExpenseRequest(entity);
    }

    public async Task<VoyageExpenseRequestDto?> UpdateExpenseRequestAsync(
        Guid id, UpdateExpenseRequestDto dto)
    {
        var entity = await _db.VoyageExpenseRequests.FindAsync(id);
        if (entity == null) return null;
        if (entity.Status != ExpenseRequestStatus.DRAFT && entity.Status != ExpenseRequestStatus.REJECTED)
            return null; // can only edit in DRAFT or REJECTED

        if (dto.CostCategory != null) entity.CostCategory = dto.CostCategory;
        if (dto.AllocationScope != null) entity.AllocationScope = dto.AllocationScope;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.RequestedAmount.HasValue)
        {
            entity.RequestedAmount = dto.RequestedAmount.Value;
            entity.RequestedAmountUsd = dto.RequestedAmount.Value * (dto.ExchangeRate ?? entity.ExchangeRate);
        }
        if (dto.Currency != null) entity.Currency = dto.Currency;
        if (dto.ExchangeRate.HasValue)
        {
            entity.ExchangeRate = dto.ExchangeRate.Value;
            entity.RequestedAmountUsd = entity.RequestedAmount * dto.ExchangeRate.Value;
        }
        if (dto.VendorName != null) entity.VendorName = dto.VendorName;
        if (dto.VendorReference != null) entity.VendorReference = dto.VendorReference;
        if (dto.PortCode != null) entity.PortCode = dto.PortCode;
        if (dto.PortName != null) entity.PortName = dto.PortName;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.SupportingDocuments != null) entity.SupportingDocuments = dto.SupportingDocuments;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapExpenseRequest(entity);
    }

    public async Task<bool> TransitionExpenseRequestAsync(
        Guid id, string newStatus, string user, ApproveExpenseRequestDto? approval = null)
    {
        var entity = await _db.VoyageExpenseRequests.FindAsync(id);
        if (entity == null) return false;

        if (!ExpenseRequestStatus.ValidTransitions.TryGetValue(entity.Status, out var valid)
            || !valid.Contains(newStatus))
            return false;

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        if (newStatus == ExpenseRequestStatus.APPROVED && approval != null)
        {
            entity.ApprovedBy = user;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovedAmount = approval.ApprovedAmount;
            entity.ApprovedAmountUsd = approval.ApprovedAmount * entity.ExchangeRate;
            entity.ApprovalNotes = approval.ApprovalNotes;
        }

        if (newStatus == ExpenseRequestStatus.REJECTED)
        {
            entity.ApprovedBy = user;
            entity.ApprovedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return true;
    }

    public async Task<bool> DeleteExpenseRequestAsync(Guid id)
    {
        var entity = await _db.VoyageExpenseRequests.FindAsync(id);
        if (entity == null) return false;
        if (entity.Status != ExpenseRequestStatus.DRAFT) return false;

        var voyageId = entity.VoyageId;
        _db.VoyageExpenseRequests.Remove(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return true;
    }

    // ================================================================
    // ADVANCE PAYMENTS
    // ================================================================

    public async Task<List<VoyageAdvancePaymentDto>> GetAdvancePaymentsAsync(Guid voyageId)
    {
        return await _db.VoyageAdvancePayments
            .Where(a => a.VoyageId == voyageId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => MapAdvancePayment(a))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VoyageAdvancePaymentDto> CreateAdvancePaymentAsync(
        Guid voyageId, CreateAdvancePaymentDto dto)
    {
        var count = await _db.VoyageAdvancePayments.CountAsync(a => a.VoyageId == voyageId);
        var entity = new VoyageAdvancePayment
        {
            VoyageId = voyageId,
            AdvanceNumber = $"ADV-{count + 1:D4}",
            AdvanceType = dto.AdvanceType,
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            AmountUsd = dto.Amount * dto.ExchangeRate,
            RecipientName = dto.RecipientName,
            PortCode = dto.PortCode,
            PortName = dto.PortName,
            Status = AdvancePaymentStatus.PENDING,
            UnsettledBalance = dto.Amount * dto.ExchangeRate,
            Notes = dto.Notes,
        };

        _db.VoyageAdvancePayments.Add(entity);
        await _db.SaveChangesAsync();
        return MapAdvancePayment(entity);
    }

    public async Task<VoyageAdvancePaymentDto?> UpdateAdvancePaymentAsync(
        Guid id, UpdateAdvancePaymentDto dto)
    {
        var entity = await _db.VoyageAdvancePayments.FindAsync(id);
        if (entity == null) return null;
        if (entity.Status != AdvancePaymentStatus.PENDING) return null;

        if (dto.AdvanceType != null) entity.AdvanceType = dto.AdvanceType;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Amount.HasValue)
        {
            entity.Amount = dto.Amount.Value;
            entity.AmountUsd = dto.Amount.Value * (dto.ExchangeRate ?? entity.ExchangeRate);
            entity.UnsettledBalance = entity.AmountUsd - entity.SettledAmount;
        }
        if (dto.Currency != null) entity.Currency = dto.Currency;
        if (dto.ExchangeRate.HasValue)
        {
            entity.ExchangeRate = dto.ExchangeRate.Value;
            entity.AmountUsd = entity.Amount * dto.ExchangeRate.Value;
            entity.UnsettledBalance = entity.AmountUsd - entity.SettledAmount;
        }
        if (dto.RecipientName != null) entity.RecipientName = dto.RecipientName;
        if (dto.PortCode != null) entity.PortCode = dto.PortCode;
        if (dto.PortName != null) entity.PortName = dto.PortName;
        if (dto.Notes != null) entity.Notes = dto.Notes;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapAdvancePayment(entity);
    }

    public async Task<bool> MarkAdvancePaidAsync(Guid id, string paidBy, string? paymentRef)
    {
        var entity = await _db.VoyageAdvancePayments.FindAsync(id);
        if (entity == null || entity.Status != AdvancePaymentStatus.PENDING) return false;

        entity.Status = AdvancePaymentStatus.PAID;
        entity.PaidAt = DateTime.UtcNow;
        entity.PaidBy = paidBy;
        entity.PaymentReference = paymentRef;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return true;
    }

    public async Task<bool> DeleteAdvancePaymentAsync(Guid id)
    {
        var entity = await _db.VoyageAdvancePayments.FindAsync(id);
        if (entity == null || entity.Status != AdvancePaymentStatus.PENDING) return false;

        var voyageId = entity.VoyageId;
        _db.VoyageAdvancePayments.Remove(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return true;
    }

    // ================================================================
    // DISBURSEMENTS
    // ================================================================

    public async Task<List<VoyageDisbursementDto>> GetDisbursementsAsync(Guid voyageId)
    {
        return await _db.VoyageDisbursements
            .Where(d => d.VoyageId == voyageId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => MapDisbursement(d))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VoyageDisbursementDto> CreateDisbursementAsync(
        Guid voyageId, CreateDisbursementDto dto)
    {
        var count = await _db.VoyageDisbursements.CountAsync(d => d.VoyageId == voyageId);
        var entity = new VoyageDisbursement
        {
            VoyageId = voyageId,
            ExpenseRequestId = dto.ExpenseRequestId,
            AdvancePaymentId = dto.AdvancePaymentId,
            DisbursementNumber = $"DIS-{count + 1:D4}",
            CostCategory = dto.CostCategory,
            AllocationScope = dto.AllocationScope,
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            AmountUsd = dto.Amount * dto.ExchangeRate,
            VendorName = dto.VendorName,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            PortCode = dto.PortCode,
            PortName = dto.PortName,
            Status = DisbursementStatus.RECORDED,
            Notes = dto.Notes,
            SupportingDocuments = dto.SupportingDocuments,
        };

        _db.VoyageDisbursements.Add(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return MapDisbursement(entity);
    }

    public async Task<VoyageDisbursementDto?> UpdateDisbursementAsync(
        Guid id, UpdateDisbursementDto dto)
    {
        var entity = await _db.VoyageDisbursements.FindAsync(id);
        if (entity == null) return null;
        if (entity.Status == DisbursementStatus.PAID) return null;

        if (dto.CostCategory != null) entity.CostCategory = dto.CostCategory;
        if (dto.AllocationScope != null) entity.AllocationScope = dto.AllocationScope;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Amount.HasValue)
        {
            entity.Amount = dto.Amount.Value;
            entity.AmountUsd = dto.Amount.Value * (dto.ExchangeRate ?? entity.ExchangeRate);
        }
        if (dto.Currency != null) entity.Currency = dto.Currency;
        if (dto.ExchangeRate.HasValue)
        {
            entity.ExchangeRate = dto.ExchangeRate.Value;
            entity.AmountUsd = entity.Amount * dto.ExchangeRate.Value;
        }
        if (dto.VendorName != null) entity.VendorName = dto.VendorName;
        if (dto.InvoiceNumber != null) entity.InvoiceNumber = dto.InvoiceNumber;
        if (dto.InvoiceDate.HasValue) entity.InvoiceDate = dto.InvoiceDate;
        if (dto.DueDate.HasValue) entity.DueDate = dto.DueDate;
        if (dto.PortCode != null) entity.PortCode = dto.PortCode;
        if (dto.PortName != null) entity.PortName = dto.PortName;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.SupportingDocuments != null) entity.SupportingDocuments = dto.SupportingDocuments;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return MapDisbursement(entity);
    }

    public async Task<bool> TransitionDisbursementAsync(
        Guid id, string newStatus, string user, string? paymentRef = null)
    {
        var entity = await _db.VoyageDisbursements.FindAsync(id);
        if (entity == null) return false;

        if (!DisbursementStatus.ValidTransitions.TryGetValue(entity.Status, out var valid)
            || !valid.Contains(newStatus))
            return false;

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        if (newStatus == DisbursementStatus.VERIFIED)
        {
            entity.VerifiedBy = user;
            entity.VerifiedAt = DateTime.UtcNow;
        }
        if (newStatus == DisbursementStatus.PAID)
        {
            entity.PaidAt = DateTime.UtcNow;
            entity.PaymentReference = paymentRef;

            // If linked to an advance, settle against it
            if (entity.AdvancePaymentId.HasValue)
            {
                var advance = await _db.VoyageAdvancePayments.FindAsync(entity.AdvancePaymentId.Value);
                if (advance != null && advance.Status == AdvancePaymentStatus.PAID)
                {
                    advance.SettledAmount += entity.AmountUsd;
                    advance.UnsettledBalance = advance.AmountUsd - advance.SettledAmount;
                    if (advance.UnsettledBalance <= 0)
                    {
                        advance.Status = AdvancePaymentStatus.SETTLED;
                        advance.UnsettledBalance = 0;
                    }
                    advance.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return true;
    }

    public async Task<bool> DeleteDisbursementAsync(Guid id)
    {
        var entity = await _db.VoyageDisbursements.FindAsync(id);
        if (entity == null || entity.Status != DisbursementStatus.RECORDED) return false;

        var voyageId = entity.VoyageId;
        _db.VoyageDisbursements.Remove(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return true;
    }

    // ================================================================
    // ACTUAL REVENUE
    // ================================================================

    public async Task<List<VoyageActualRevenueDto>> GetActualRevenuesAsync(Guid voyageId)
    {
        return await _db.VoyageActualRevenues
            .Where(r => r.VoyageId == voyageId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapActualRevenue(r))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VoyageActualRevenueDto> CreateActualRevenueAsync(
        Guid voyageId, CreateActualRevenueDto dto)
    {
        var count = await _db.VoyageActualRevenues.CountAsync(r => r.VoyageId == voyageId);
        var entity = new VoyageActualRevenue
        {
            VoyageId = voyageId,
            RevenueNumber = $"REV-{count + 1:D4}",
            RevenueCategory = dto.RevenueCategory,
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            AmountUsd = dto.Amount * dto.ExchangeRate,
            PayerName = dto.PayerName,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            Status = "INVOICED",
            Notes = dto.Notes,
        };

        _db.VoyageActualRevenues.Add(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return MapActualRevenue(entity);
    }

    public async Task<VoyageActualRevenueDto?> UpdateActualRevenueAsync(
        Guid id, UpdateActualRevenueDto dto)
    {
        var entity = await _db.VoyageActualRevenues.FindAsync(id);
        if (entity == null) return null;

        if (dto.RevenueCategory != null) entity.RevenueCategory = dto.RevenueCategory;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Amount.HasValue)
        {
            entity.Amount = dto.Amount.Value;
            entity.AmountUsd = dto.Amount.Value * (dto.ExchangeRate ?? entity.ExchangeRate);
        }
        if (dto.Currency != null) entity.Currency = dto.Currency;
        if (dto.ExchangeRate.HasValue)
        {
            entity.ExchangeRate = dto.ExchangeRate.Value;
            entity.AmountUsd = entity.Amount * dto.ExchangeRate.Value;
        }
        if (dto.PayerName != null) entity.PayerName = dto.PayerName;
        if (dto.InvoiceNumber != null) entity.InvoiceNumber = dto.InvoiceNumber;
        if (dto.InvoiceDate.HasValue) entity.InvoiceDate = dto.InvoiceDate;
        if (dto.Notes != null) entity.Notes = dto.Notes;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return MapActualRevenue(entity);
    }

    public async Task<bool> TransitionRevenueAsync(Guid id, string newStatus, string? paymentRef = null)
    {
        var entity = await _db.VoyageActualRevenues.FindAsync(id);
        if (entity == null) return false;

        // Simple transitions: INVOICED → RECEIVED, INVOICED → DISPUTED, DISPUTED → INVOICED
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            { "INVOICED", new() { "RECEIVED", "DISPUTED" } },
            { "DISPUTED", new() { "INVOICED" } },
            { "RECEIVED", new() { } },
        };

        if (!validTransitions.TryGetValue(entity.Status, out var valid) || !valid.Contains(newStatus))
            return false;

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        if (newStatus == "RECEIVED")
        {
            entity.ReceivedAt = DateTime.UtcNow;
            entity.PaymentReference = paymentRef;
        }

        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(entity.VoyageId);
        return true;
    }

    public async Task<bool> DeleteActualRevenueAsync(Guid id)
    {
        var entity = await _db.VoyageActualRevenues.FindAsync(id);
        if (entity == null) return false;

        var voyageId = entity.VoyageId;
        _db.VoyageActualRevenues.Remove(entity);
        await _db.SaveChangesAsync();
        await RecalculateFinancialSummaryAsync(voyageId);
        return true;
    }

    // ================================================================
    // SETTLEMENTS
    // ================================================================

    public async Task<List<VoyageSettlementDto>> GetSettlementsAsync(Guid voyageId)
    {
        return await _db.VoyageSettlements
            .Where(s => s.VoyageId == voyageId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapSettlement(s))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VoyageSettlementDto> CreateSettlementAsync(
        Guid voyageId, CreateSettlementDto dto, string user)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.ExpenseRequests)
            .Include(v => v.AdvancePayments)
            .Include(v => v.Disbursements)
            .Include(v => v.ActualRevenues)
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) throw new InvalidOperationException("Voyage not found");

        var count = await _db.VoyageSettlements.CountAsync(s => s.VoyageId == voyageId);

        var totalExpenseApproved = voyage.ExpenseRequests
            .Where(e => e.Status == ExpenseRequestStatus.APPROVED)
            .Sum(e => e.ApprovedAmountUsd ?? e.RequestedAmountUsd);
        var totalAdvanced = voyage.AdvancePayments
            .Where(a => a.Status == AdvancePaymentStatus.PAID || a.Status == AdvancePaymentStatus.SETTLED)
            .Sum(a => a.AmountUsd);
        var totalDisbursed = voyage.Disbursements
            .Where(d => d.Status == DisbursementStatus.PAID)
            .Sum(d => d.AmountUsd);
        var totalRevenue = voyage.ActualRevenues.Sum(r => r.AmountUsd);

        var entity = new VoyageSettlement
        {
            VoyageId = voyageId,
            SettlementNumber = $"STL-{count + 1:D4}",
            Status = "DRAFT",
            TotalExpenseApproved = totalExpenseApproved,
            TotalAdvanced = totalAdvanced,
            TotalDisbursed = totalDisbursed,
            TotalRevenue = totalRevenue,
            NetResult = totalRevenue - totalDisbursed,
            AdvanceBalance = totalAdvanced - totalDisbursed,
            Summary = dto.Summary,
            PreparedBy = user,
            PreparedAt = DateTime.UtcNow,
            Notes = dto.Notes,
        };

        _db.VoyageSettlements.Add(entity);

        // Update voyage financial status
        voyage.FinancialStatus = VoyageFinancialStatus.PENDING_SETTLEMENT;
        voyage.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapSettlement(entity);
    }

    public async Task<VoyageSettlementDto?> UpdateSettlementAsync(
        Guid id, UpdateSettlementDto dto)
    {
        var entity = await _db.VoyageSettlements.FindAsync(id);
        if (entity == null) return null;
        if (entity.Status != "DRAFT") return null;

        if (dto.FinalSettlementAmount.HasValue) entity.FinalSettlementAmount = dto.FinalSettlementAmount;
        if (dto.Summary != null) entity.Summary = dto.Summary;
        if (dto.Notes != null) entity.Notes = dto.Notes;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapSettlement(entity);
    }

    public async Task<bool> TransitionSettlementAsync(
        Guid id, string newStatus, string user, string? notes = null)
    {
        var entity = await _db.VoyageSettlements.FindAsync(id);
        if (entity == null) return false;

        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            { "DRAFT", new() { "SUBMITTED" } },
            { "SUBMITTED", new() { "REVIEWED", "REJECTED" } },
            { "REVIEWED", new() { "APPROVED", "REJECTED" } },
            { "APPROVED", new() { } },
            { "REJECTED", new() { "DRAFT" } },
        };

        if (!validTransitions.TryGetValue(entity.Status, out var valid) || !valid.Contains(newStatus))
            return false;

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        if (newStatus == "REVIEWED")
        {
            entity.ReviewedBy = user;
            entity.ReviewedAt = DateTime.UtcNow;
        }
        if (newStatus == "APPROVED")
        {
            entity.ApprovedBy = user;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovalNotes = notes;

            // Mark voyage as settled
            var voyage = await _db.VoyageRecords.FindAsync(entity.VoyageId);
            if (voyage != null)
            {
                voyage.FinancialStatus = VoyageFinancialStatus.SETTLED;
                voyage.UpdatedAt = DateTime.UtcNow;
            }
        }
        if (newStatus == "REJECTED")
        {
            entity.ApprovalNotes = notes;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // FINANCIAL CLOSING
    // ================================================================

    public async Task<bool> CloseVoyageFinancialsAsync(Guid voyageId, string user, string? notes = null)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.Settlements)
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return false;

        // Must have at least one approved settlement to close
        var hasApprovedSettlement = voyage.Settlements.Any(s => s.Status == "APPROVED");
        if (!hasApprovedSettlement) return false;

        // Cannot close if already closed
        if (voyage.FinancialStatus == VoyageFinancialStatus.CLOSED) return false;

        voyage.FinancialStatus = VoyageFinancialStatus.CLOSED;
        voyage.FinancialClosedAt = DateTime.UtcNow;
        voyage.FinancialClosedBy = user;
        voyage.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    // ================================================================
    // RECALCULATE FINANCIAL SUMMARY
    // ================================================================

    private async Task RecalculateFinancialSummaryAsync(Guid voyageId)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.Disbursements)
            .Include(v => v.ActualRevenues)
            .Include(v => v.AdvancePayments)
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return;

        var totalActualCost = voyage.Disbursements.Sum(d => d.AmountUsd);
        var totalActualRevenue = voyage.ActualRevenues.Sum(r => r.AmountUsd);
        var totalAdvanced = voyage.AdvancePayments
            .Where(a => a.Status == AdvancePaymentStatus.PAID || a.Status == AdvancePaymentStatus.SETTLED)
            .Sum(a => a.AmountUsd);
        var totalDisbursed = voyage.Disbursements
            .Where(d => d.Status == DisbursementStatus.PAID)
            .Sum(d => d.AmountUsd);

        voyage.TotalActualCost = totalActualCost;
        voyage.TotalActualRevenue = totalActualRevenue;
        voyage.ActualProfitMargin = totalActualRevenue - totalActualCost;
        voyage.TotalAdvanced = totalAdvanced;
        voyage.TotalDisbursed = totalDisbursed;
        voyage.OutstandingBalance = totalAdvanced - totalDisbursed;
        voyage.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ================================================================
    // MAPPING HELPERS
    // ================================================================

    private static VoyageExpenseRequestDto MapExpenseRequest(VoyageExpenseRequest e) => new()
    {
        Id = e.Id,
        VoyageId = e.VoyageId,
        RequestNumber = e.RequestNumber,
        CostCategory = e.CostCategory,
        AllocationScope = e.AllocationScope,
        Description = e.Description,
        RequestedAmount = e.RequestedAmount,
        Currency = e.Currency,
        ExchangeRate = e.ExchangeRate,
        RequestedAmountUsd = e.RequestedAmountUsd,
        VendorName = e.VendorName,
        VendorReference = e.VendorReference,
        PortCode = e.PortCode,
        PortName = e.PortName,
        Status = e.Status,
        RequestedBy = e.RequestedBy,
        RequestedAt = e.RequestedAt,
        ApprovedBy = e.ApprovedBy,
        ApprovedAt = e.ApprovedAt,
        ApprovedAmount = e.ApprovedAmount,
        ApprovedAmountUsd = e.ApprovedAmountUsd,
        ApprovalNotes = e.ApprovalNotes,
        Notes = e.Notes,
        SupportingDocuments = e.SupportingDocuments,
        CreatedAt = e.CreatedAt,
    };

    private static VoyageAdvancePaymentDto MapAdvancePayment(VoyageAdvancePayment a) => new()
    {
        Id = a.Id,
        VoyageId = a.VoyageId,
        AdvanceNumber = a.AdvanceNumber,
        AdvanceType = a.AdvanceType,
        Description = a.Description,
        Amount = a.Amount,
        Currency = a.Currency,
        ExchangeRate = a.ExchangeRate,
        AmountUsd = a.AmountUsd,
        RecipientName = a.RecipientName,
        PortCode = a.PortCode,
        PortName = a.PortName,
        Status = a.Status,
        PaidAt = a.PaidAt,
        PaidBy = a.PaidBy,
        PaymentReference = a.PaymentReference,
        SettledAmount = a.SettledAmount,
        UnsettledBalance = a.UnsettledBalance,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt,
    };

    private static VoyageDisbursementDto MapDisbursement(VoyageDisbursement d) => new()
    {
        Id = d.Id,
        VoyageId = d.VoyageId,
        ExpenseRequestId = d.ExpenseRequestId,
        AdvancePaymentId = d.AdvancePaymentId,
        DisbursementNumber = d.DisbursementNumber,
        CostCategory = d.CostCategory,
        AllocationScope = d.AllocationScope,
        Description = d.Description,
        Amount = d.Amount,
        Currency = d.Currency,
        ExchangeRate = d.ExchangeRate,
        AmountUsd = d.AmountUsd,
        VendorName = d.VendorName,
        InvoiceNumber = d.InvoiceNumber,
        InvoiceDate = d.InvoiceDate,
        DueDate = d.DueDate,
        PortCode = d.PortCode,
        PortName = d.PortName,
        Status = d.Status,
        VerifiedBy = d.VerifiedBy,
        VerifiedAt = d.VerifiedAt,
        PaidAt = d.PaidAt,
        PaymentReference = d.PaymentReference,
        Notes = d.Notes,
        SupportingDocuments = d.SupportingDocuments,
        CreatedAt = d.CreatedAt,
    };

    private static VoyageActualRevenueDto MapActualRevenue(VoyageActualRevenue r) => new()
    {
        Id = r.Id,
        VoyageId = r.VoyageId,
        RevenueNumber = r.RevenueNumber,
        RevenueCategory = r.RevenueCategory,
        Description = r.Description,
        Amount = r.Amount,
        Currency = r.Currency,
        ExchangeRate = r.ExchangeRate,
        AmountUsd = r.AmountUsd,
        PayerName = r.PayerName,
        InvoiceNumber = r.InvoiceNumber,
        InvoiceDate = r.InvoiceDate,
        Status = r.Status,
        ReceivedAt = r.ReceivedAt,
        PaymentReference = r.PaymentReference,
        Notes = r.Notes,
        CreatedAt = r.CreatedAt,
    };

    private static VoyageSettlementDto MapSettlement(VoyageSettlement s) => new()
    {
        Id = s.Id,
        VoyageId = s.VoyageId,
        SettlementNumber = s.SettlementNumber,
        Status = s.Status,
        TotalExpenseApproved = s.TotalExpenseApproved,
        TotalAdvanced = s.TotalAdvanced,
        TotalDisbursed = s.TotalDisbursed,
        TotalRevenue = s.TotalRevenue,
        NetResult = s.NetResult,
        AdvanceBalance = s.AdvanceBalance,
        FinalSettlementAmount = s.FinalSettlementAmount,
        Summary = s.Summary,
        PreparedBy = s.PreparedBy,
        PreparedAt = s.PreparedAt,
        ReviewedBy = s.ReviewedBy,
        ReviewedAt = s.ReviewedAt,
        ApprovedBy = s.ApprovedBy,
        ApprovedAt = s.ApprovedAt,
        ApprovalNotes = s.ApprovalNotes,
        Notes = s.Notes,
        CreatedAt = s.CreatedAt,
    };
}
