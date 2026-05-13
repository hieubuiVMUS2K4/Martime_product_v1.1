using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using System.Text.Json;

namespace MaritimeEdge.Services.Reporting;

/// <summary>
/// Base class for all report generation services.
/// Consolidates common patterns: validation, numbering, duplicate checking, 
/// transaction handling, status workflow, and logging.
/// Template method pattern - override specific steps in subclasses.
/// </summary>
/// <typeparam name="TDto">Input DTO type (e.g., CreateNoonReportDto)</typeparam>
/// <typeparam name="TReport">Report model type (e.g., NoonReport)</typeparam>
public abstract class ReportGeneratorBase<TDto, TReport>
    where TReport : class
{
    protected readonly EdgeDbContext Context;
    protected readonly ILogger Logger;

    protected ReportGeneratorBase(EdgeDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    /// <summary>
    /// Template method for report creation workflow.
    /// Orchestrates validation → duplicate check → number generation → 
    /// transaction → persistence → logging.
    /// </summary>
    public async Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> CreateReportAsync(
        TDto dto,
        string reportTypeCode,
        string? username = null)
    {
        try
        {
            // Step 1: Validate input according to maritime rules
            var validationResult = await ValidateReportAsync(dto);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("Report validation failed for {ReportType}: {Errors}",
                    reportTypeCode, string.Join("; ", validationResult.Errors));
                return (false, string.Empty, null, string.Join("; ", validationResult.Errors));
            }

            // Log warnings if any
            if (validationResult.Warnings.Any())
            {
                Logger.LogWarning("Report generation warnings for {ReportType}: {Warnings}",
                    reportTypeCode, string.Join("; ", validationResult.Warnings));
            }

            // Step 2: Get report type configuration
            var reportType = await GetReportTypeAsync(reportTypeCode);
            if (reportType == null)
            {
                var error = $"Report type {reportTypeCode} not found. Please seed report types.";
                Logger.LogError(error);
                return (false, string.Empty, null, error);
            }

            // Step 3: Check for duplicates (business rule validation)
            var duplicateCheckResult = await CheckForDuplicatesAsync(dto, reportType);
            if (!duplicateCheckResult.IsValid)
            {
                Logger.LogWarning("Duplicate report check failed: {Error}", duplicateCheckResult.Error);
                return (false, string.Empty, null, duplicateCheckResult.Error);
            }

            // Step 4: Generate unique report number
            var reportNumber = await GenerateReportNumberAsync(reportTypeCode);

            // Step 5: Build remarks with validation warnings
            var remarks = GetReportRemarks(dto);
            if (validationResult.Warnings.Any())
            {
                remarks = $"[VALIDATION WARNINGS]\n{string.Join("\n", validationResult.Warnings)}\n\n{remarks}";
            }

            // Step 6: Execute in transaction for atomicity
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var (maritimeReportId, report) = await CreateReportInTransactionAsync(
                    dto, reportType, reportNumber, remarks, username);

                await Context.SaveChangesAsync();
                await transaction.CommitAsync();

                Logger.LogInformation("Report {ReportNumber} (ID: {ReportId}) created successfully",
                    reportNumber, maritimeReportId);

                return (true, reportNumber, maritimeReportId, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError(ex, "Error creating report {ReportNumber}: {Message}",
                    reportNumber, ex.Message);
                return (false, string.Empty, null, $"Database error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in report creation: {Message}", ex.Message);
            return (false, string.Empty, null, $"System error: {ex.Message}");
        }
    }

    // ============================================================
    // ABSTRACT METHODS - OVERRIDE IN SUBCLASS
    // ============================================================

    /// <summary>
    /// Validate report-specific business rules.
    /// Override to implement validation logic specific to each report type.
    /// </summary>
    protected abstract Task<ValidationResult> ValidateReportAsync(TDto dto);

    /// <summary>
    /// Check for business rule violations (e.g., duplicate Noon Report for same date).
    /// Override to implement duplicate or conflicting record checks.
    /// </summary>
    protected abstract Task<DuplicateCheckResult> CheckForDuplicatesAsync(TDto dto, ReportType reportType);

    /// <summary>
    /// Extract remarks from the DTO.
    /// Override to return report-specific remarks field.
    /// </summary>
    protected abstract string GetReportRemarks(TDto dto);

    /// <summary>
    /// Create the report entity and populate the maritime report record.
    /// Override to implement report-specific entity creation.
    /// </summary>
    protected abstract Task<(Guid MaritimeReportId, TReport Report)> CreateReportInTransactionAsync(
        TDto dto,
        ReportType reportType,
        string reportNumber,
        string remarks,
        string? username);

    /// <summary>
    /// Get report by ID from database.
    /// Override in subclass to retrieve the specific report model type.
    /// </summary>
    protected abstract Task<object?> GetReportByIdAsync(Guid reportId);

    /// <summary>
    /// Add status history entry when report transitions between states.
    /// Override in subclass to implement model-specific history tracking.
    /// </summary>
    protected abstract void AddStatusHistoryAsync(object report, string fromStatus, string toStatus, string notes);

    /// <summary>
    /// Create amendment record for report corrections.
    /// Override in subclass to implement amendment tracking specific to report type.
    /// </summary>
    protected abstract void CreateAmendmentAsync(object report, string changedBy, string description);

    // ============================================================
    // PROTECTED HELPER METHODS - REUSABLE ACROSS REPORT TYPES
    // ============================================================

    /// <summary>
    /// Retrieve report type from database with caching.
    /// </summary>
    protected async Task<ReportType?> GetReportTypeAsync(string typeCode)
    {
        return await Context.ReportTypes
            .FirstOrDefaultAsync(rt => rt.TypeCode == typeCode && rt.IsActive);
    }

    /// <summary>
    /// Generate unique report number with format: RPTTYPE-YYYY-NNNNNN.
    /// </summary>
    protected async Task<string> GenerateReportNumberAsync(string reportTypeCode)
    {
        // Get current year
        var year = DateTime.UtcNow.Year;

        // Count existing reports for this type and year
        var count = await Context.MaritimeReports
            .Where(mr => mr.ReportNumber.StartsWith($"{reportTypeCode}-{year}-"))
            .CountAsync();

        // Format: REPORTTYPE-YYYY-NNNNNN (sequential)
        return $"{reportTypeCode}-{year}-{(count + 1):D6}";
    }

    /// <summary>
    /// Serialize DTO as JSON for audit trail storage.
    /// </summary>
    protected string SerializeDto(TDto dto)
    {
        return JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = false });
    }

    // ============================================================
    // RESULT TYPES
    // ============================================================

    protected sealed record ValidationResult(
        bool IsValid,
        List<string> Errors,
        List<string> Warnings)
    {
        public static ValidationResult Success(List<string>? warnings = null)
            => new(true, new(), warnings ?? new());

        public static ValidationResult Failure(params string[] errors)
            => new(false, errors.ToList(), new());

        public static ValidationResult FailureWithWarnings(List<string> errors, List<string> warnings)
            => new(false, errors, warnings);
    }

    protected sealed record DuplicateCheckResult(bool IsValid, string? Error)
    {
        public static DuplicateCheckResult Success()
            => new(true, null);

        public static DuplicateCheckResult Failure(string error)
            => new(false, error);
    }
}
