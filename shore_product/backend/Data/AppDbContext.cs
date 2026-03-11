using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using Maritime.Shared.Models.CrewManagement;

namespace ProductApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Ship> Ships { get; set; } = null!;
        public DbSet<CrewMember> CrewMembers { get; set; } = null!;
        public DbSet<MaintenanceTask> MaintenanceTasks { get; set; } = null!;
        
        // Maritime entities
        public DbSet<Vessel> Vessels { get; set; } = null!;
        public DbSet<VesselPosition> VesselPositions { get; set; } = null!;
        public DbSet<FuelConsumption> FuelConsumptions { get; set; } = null!;
        public DbSet<PortCall> PortCalls { get; set; } = null!;
        public DbSet<VesselAlert> VesselAlerts { get; set; } = null!;
        public DbSet<VesselCertificate> VesselCertificates { get; set; } = null!;

        // Edge Sync Models (Optimized for Shore - Essential Data Only)
        // REMOVED: NmeaRawData (debug only), NavigationData (realtime only), EnvironmentalData (in NoonReport)
        public DbSet<PositionData> PositionData { get; set; } = null!;
        public DbSet<AisData> AisData { get; set; } = null!;
        public DbSet<EngineData> EngineData { get; set; } = null!;
        public DbSet<FuelConsumptionData> FuelConsumptionData { get; set; } = null!;
        public DbSet<TankLevel> TankLevels { get; set; } = null!;
        public DbSet<GeneratorData> GeneratorData { get; set; } = null!;
        public DbSet<SafetyAlarm> SafetyAlarms { get; set; } = null!;
        public DbSet<VoyageRecord> VoyageRecords { get; set; } = null!;
        public DbSet<ReportType> ReportTypes { get; set; } = null!;
        public DbSet<MaritimeReport> MaritimeReports { get; set; } = null!;
        public DbSet<NoonReport> NoonReports { get; set; } = null!;
        public DbSet<DepartureReport> DepartureReports { get; set; } = null!;
        public DbSet<ArrivalReport> ArrivalReports { get; set; } = null!;

        // ============================================================
        // CREW MANAGEMENT (Maritime.Shared models via SharedTypeAliases)
        // ============================================================
        public DbSet<Certificate> CrewCertificateTypes { get; set; } = null!;
        public DbSet<CrewCertificate> CrewCertificates { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Rank> Ranks { get; set; } = null!;
        public DbSet<RankCertificate> RankCertificates { get; set; } = null!;
        public DbSet<CountryCertificate> CountryCertificates { get; set; } = null!;
        public DbSet<ServiceRecord> ServiceRecords { get; set; } = null!;
        public DbSet<VesselCertificateAssignment> VesselCertificateAssignments { get; set; } = null!;

        // Crew Documents
        public DbSet<TravelDocument> TravelDocuments { get; set; } = null!;
        public DbSet<SeafarerDocument> SeafarerDocuments { get; set; } = null!;
        public DbSet<EmploymentDocument> EmploymentDocuments { get; set; } = null!;
        public DbSet<HealthDocument> HealthDocuments { get; set; } = null!;

        // ============================================================
        // SYNC INFRASTRUCTURE
        // ============================================================
        public DbSet<SyncOutbox> SyncOutbox { get; set; } = null!;
        public DbSet<SyncLog> SyncLogs { get; set; } = null!;
        public DbSet<SyncNodeTracker> SyncNodeTrackers { get; set; } = null!;
        public DbSet<SyncIdempotencyRecord> SyncIdempotencyRecords { get; set; } = null!;
        public DbSet<SyncTableStats> SyncTableStats { get; set; } = null!;

        // ============================================================
        // CREW MANAGEMENT WORKFLOW (Phase 1A)
        // ============================================================
        public DbSet<CrewStatusHistory> CrewStatusHistory { get; set; } = null!;
        public DbSet<OnboardingCase> OnboardingCases { get; set; } = null!;
        public DbSet<OnboardingChecklistItem> OnboardingChecklistItems { get; set; } = null!;
        public DbSet<CrewDocumentSubmission> DocumentSubmissions { get; set; } = null!;
        public DbSet<CrewDocumentVersion> DocumentVersions { get; set; } = null!;
        public DbSet<DocumentVerificationTask> VerificationTasks { get; set; } = null!;
        public DbSet<DocumentVerificationAction> VerificationActions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        // ============================================================
        // COMPLIANCE MATRIX (Phase 2)
        // ============================================================
        public DbSet<ComplianceRuleSet> ComplianceRuleSets { get; set; } = null!;
        public DbSet<ComplianceRule> ComplianceRules { get; set; } = null!;
        public DbSet<ComplianceDimension> ComplianceDimensions { get; set; } = null!;
        public DbSet<ComplianceWaiver> ComplianceWaivers { get; set; } = null!;
        public DbSet<ComplianceSnapshot> ComplianceSnapshots { get; set; } = null!;

        // ============================================================
        // PLANNING & ASSIGNMENT (Phase 5)
        // ============================================================
        public DbSet<VesselManningStandard> VesselManningStandards { get; set; } = null!;
        public DbSet<ManningPosition> ManningPositions { get; set; } = null!;
        public DbSet<CrewAssignment> CrewAssignments { get; set; } = null!;
        public DbSet<AssignmentConfirmation> AssignmentConfirmations { get; set; } = null!;
        public DbSet<AssignmentConflict> AssignmentConflicts { get; set; } = null!;
        public DbSet<AssignmentComment> AssignmentComments { get; set; } = null!;
        public DbSet<AssignmentStatusHistory> AssignmentStatusHistory { get; set; } = null!;

        // ============================================================
        // EXTERNAL REQUESTS & TRAVEL (Phase 6)
        // ============================================================
        public DbSet<ExternalRequest> ExternalRequests { get; set; } = null!;
        public DbSet<ExternalCandidate> ExternalCandidates { get; set; } = null!;
        public DbSet<ExternalRequestMessage> ExternalRequestMessages { get; set; } = null!;
        public DbSet<TravelRequest> TravelRequests { get; set; } = null!;
        public DbSet<TravelSegment> TravelSegments { get; set; } = null!;
        public DbSet<TravelStatusHistory> TravelStatusHistory { get; set; } = null!;

        // ============================================================
        // ONBOARD EVENTS, ACCESS & SIGN-ON/SIGN-OFF (Phase 7)
        // ============================================================
        public DbSet<OnboardEvent> OnboardEvents { get; set; } = null!;
        public DbSet<CrewAccessGrant> CrewAccessGrants { get; set; } = null!;
        public DbSet<SignOnRecord> SignOnRecords { get; set; } = null!;
        public DbSet<SignOffRecord> SignOffRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Vessel
            modelBuilder.Entity<Vessel>(entity =>
            {
                entity.HasIndex(v => v.IMO).IsUnique();
                entity.Property(v => v.GrossTonnage).HasPrecision(10, 2);
                entity.Property(v => v.DeadWeight).HasPrecision(10, 2);
            });

            // Configure VesselPosition
            modelBuilder.Entity<VesselPosition>(entity =>
            {
                entity.HasOne(vp => vp.Vessel)
                    .WithMany(v => v.Positions)
                    .HasForeignKey(vp => vp.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(vp => vp.Latitude).HasPrecision(10, 7);
                entity.Property(vp => vp.Longitude).HasPrecision(10, 7);
                entity.Property(vp => vp.Speed).HasPrecision(5, 2);
                entity.Property(vp => vp.Course).HasPrecision(5, 2);
                
                entity.HasIndex(vp => new { vp.VesselId, vp.Timestamp });
            });

            // Configure FuelConsumption
            modelBuilder.Entity<FuelConsumption>(entity =>
            {
                entity.HasOne(fc => fc.Vessel)
                    .WithMany(v => v.FuelRecords)
                    .HasForeignKey(fc => fc.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(fc => fc.FuelConsumed).HasPrecision(8, 3);
                entity.Property(fc => fc.DistanceTraveled).HasPrecision(8, 2);
                entity.Property(fc => fc.AverageSpeed).HasPrecision(5, 2);
                entity.Property(fc => fc.FuelEfficiency).HasPrecision(6, 4);
            });

            // Configure PortCall
            modelBuilder.Entity<PortCall>(entity =>
            {
                entity.HasOne(pc => pc.Vessel)
                    .WithMany(v => v.PortCalls)
                    .HasForeignKey(pc => pc.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(pc => pc.PortFees).HasPrecision(12, 2);
                entity.Property(pc => pc.CargoQuantity).HasPrecision(10, 3);
                
                entity.HasIndex(pc => new { pc.VesselId, pc.ArrivalTime });
            });

            // Configure VesselAlert
            modelBuilder.Entity<VesselAlert>(entity =>
            {
                entity.HasOne(va => va.Vessel)
                    .WithMany(v => v.Alerts)
                    .HasForeignKey(va => va.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(va => new { va.VesselId, va.Timestamp });
                entity.HasIndex(va => va.IsAcknowledged);
            });

            // Configure VesselCertificate (renamed from Certificate to avoid conflict with shared crew Certificate)
            modelBuilder.Entity<VesselCertificate>(entity =>
            {
                entity.ToTable("Certificates"); // Keep the same table name for backward compatibility
                entity.HasOne(c => c.Vessel)
                    .WithMany()
                    .HasForeignKey(c => c.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.VesselId, c.ExpiryDate });
                entity.HasIndex(c => c.CertificateNumber).IsUnique();
            });

            // ============================================================
            // CREW MANAGEMENT ENTITY CONFIGURATIONS
            // ============================================================
            
            // Configure CrewMember
            modelBuilder.Entity<CrewMember>(entity =>
            {
                entity.ToTable("crew_members");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewId).IsUnique();
                entity.HasIndex(e => e.FullName);
                entity.HasIndex(e => e.IsOnboard);
                entity.HasIndex(e => e.IsSynced);
                
                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Country)
                    .WithMany()
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.VesselId);

                entity.Property(e => e.Weight).HasPrecision(5, 2);
            });

            // Configure Certificate (Crew Certificate Types)
            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.ToTable("certificates");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CertificateCode).IsUnique();
            });

            // Configure CrewCertificate
            modelBuilder.Entity<CrewCertificate>(entity =>
            {
                entity.ToTable("crew_certificates");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CrewMemberId, e.CertificateId });
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.IsSynced);

                entity.HasOne(e => e.CrewMember)
                    .WithMany(c => c.Certificates)
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Certificate)
                    .WithMany(c => c.CrewCertificates)
                    .HasForeignKey(e => e.CertificateId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Country)
                    .WithMany(c => c.CrewCertificates)
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Country
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("countries");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CountryCode).IsUnique();
            });

            // Configure Rank
            modelBuilder.Entity<Rank>(entity =>
            {
                entity.ToTable("ranks");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RankCode).IsUnique();
            });

            // Configure RankCertificate
            modelBuilder.Entity<RankCertificate>(entity =>
            {
                entity.ToTable("rank_certificates");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RankId, e.CertificateId }).IsUnique();

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Certificate)
                    .WithMany()
                    .HasForeignKey(e => e.CertificateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CountryCertificate
            modelBuilder.Entity<CountryCertificate>(entity =>
            {
                entity.ToTable("country_certificates");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CountryId, e.CertificateId }).IsUnique();

                entity.HasOne(e => e.Country)
                    .WithMany(c => c.CountryCertificates)
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Certificate)
                    .WithMany(c => c.CountryCertificates)
                    .HasForeignKey(e => e.CertificateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure VesselCertificateAssignment
            modelBuilder.Entity<VesselCertificateAssignment>(entity =>
            {
                entity.ToTable("vessel_certificate_assignments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.VesselId, e.CertificateId }).IsUnique();
                entity.HasIndex(e => e.IsSynced);

                entity.HasOne(e => e.Certificate)
                    .WithMany()
                    .HasForeignKey(e => e.CertificateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ServiceRecord
            modelBuilder.Entity<ServiceRecord>(entity =>
            {
                entity.ToTable("service_records");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.IsSynced);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.VesselGrt).HasPrecision(12, 2);
                entity.Property(e => e.VesselDwt).HasPrecision(12, 2);
            });

            // Configure TravelDocument
            modelBuilder.Entity<TravelDocument>(entity =>
            {
                entity.ToTable("travel_documents");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);

                entity.HasOne(e => e.CrewMember)
                    .WithMany(c => c.TravelDocuments)
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Country)
                    .WithMany()
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure SeafarerDocument
            modelBuilder.Entity<SeafarerDocument>(entity =>
            {
                entity.ToTable("seafarer_documents");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);

                entity.HasOne(e => e.CrewMember)
                    .WithMany(c => c.SeafarerDocuments)
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Country)
                    .WithMany()
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure EmploymentDocument
            modelBuilder.Entity<EmploymentDocument>(entity =>
            {
                entity.ToTable("employment_documents");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);

                entity.HasOne(e => e.CrewMember)
                    .WithMany(c => c.EmploymentDocuments)
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Country)
                    .WithMany()
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure HealthDocument
            modelBuilder.Entity<HealthDocument>(entity =>
            {
                entity.ToTable("health_documents");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);

                entity.HasOne(e => e.CrewMember)
                    .WithMany(c => c.HealthDocuments)
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MaritimeReport
            modelBuilder.Entity<MaritimeReport>(entity =>
            {
                entity.ToTable("maritime_reports");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReportNumber).IsUnique();
                entity.HasIndex(e => e.OriginNode);
                entity.HasIndex(e => e.ReportDateTime);
                entity.HasIndex(e => e.Status);
            });

            // Configure ReportType
            modelBuilder.Entity<ReportType>(entity =>
            {
                entity.ToTable("report_types");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TypeCode).IsUnique();
            });

            // Configure NoonReport
            modelBuilder.Entity<NoonReport>(entity =>
            {
                entity.ToTable("noon_reports");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.MaritimeReportId).IsUnique();
            });

            // Configure DepartureReport
            modelBuilder.Entity<DepartureReport>(entity =>
            {
                entity.ToTable("departure_reports");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.MaritimeReportId).IsUnique();
            });

            // Configure ArrivalReport
            modelBuilder.Entity<ArrivalReport>(entity =>
            {
                entity.ToTable("arrival_reports");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.MaritimeReportId).IsUnique();
            });

            // Seed ReportTypes
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<ReportType>().HasData(
                new ReportType { Id = 1, TypeCode = "NOON",      TypeName = "Noon Report",      Category = "VOYAGE", Frequency = "DAILY",       IsMandatory = true,  RequiresMasterSignature = true,  IsActive = true, RegulationReference = "SOLAS V/28", CreatedAt = seedDate },
                new ReportType { Id = 2, TypeCode = "DEPARTURE", TypeName = "Departure Report", Category = "VOYAGE", Frequency = "EVENT_BASED", IsMandatory = true,  RequiresMasterSignature = true,  IsActive = true, CreatedAt = seedDate },
                new ReportType { Id = 3, TypeCode = "ARRIVAL",   TypeName = "Arrival Report",   Category = "VOYAGE", Frequency = "EVENT_BASED", IsMandatory = true,  RequiresMasterSignature = true,  IsActive = true, CreatedAt = seedDate },
                new ReportType { Id = 4, TypeCode = "DAILY",     TypeName = "Daily Report",     Category = "VOYAGE", Frequency = "DAILY",       IsMandatory = false, RequiresMasterSignature = false, IsActive = true, CreatedAt = seedDate },
                new ReportType { Id = 5, TypeCode = "BUNKER",    TypeName = "Bunker Report",    Category = "VOYAGE", Frequency = "EVENT_BASED", IsMandatory = false, RequiresMasterSignature = false, IsActive = true, CreatedAt = seedDate },
                new ReportType { Id = 6, TypeCode = "POSITION",  TypeName = "Position Report",  Category = "VOYAGE", Frequency = "EVENT_BASED", IsMandatory = false, RequiresMasterSignature = false, IsActive = true, CreatedAt = seedDate }
            );

            // Configure SyncOutbox (shore → ship)
            modelBuilder.Entity<SyncOutbox>(entity =>
            {
                entity.ToTable("sync_outbox");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DeliveredAt);
                entity.HasIndex(e => new { e.TableName, e.RecordKey });
            });

            // Configure SyncLog
            modelBuilder.Entity<SyncLog>(entity =>
            {
                entity.ToTable("sync_logs");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProcessedAt);
                entity.HasIndex(e => e.OriginNode);
            });

            // Configure SyncNodeTracker
            modelBuilder.Entity<SyncNodeTracker>(entity =>
            {
                entity.ToTable("sync_node_trackers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NodeId).IsUnique();
                entity.HasIndex(e => e.IsOnline);
            });

            // Configure SyncIdempotencyRecord
            modelBuilder.Entity<SyncIdempotencyRecord>(entity =>
            {
                entity.ToTable("sync_idempotency_records");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdempotencyKey).IsUnique();
                entity.HasIndex(e => e.ProcessedAt);
            });

            // Configure SyncTableStats
            modelBuilder.Entity<SyncTableStats>(entity =>
            {
                entity.ToTable("sync_table_stats");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.NodeId, e.TableName });
            });

            // ============================================================
            // CREW MANAGEMENT WORKFLOW CONFIGURATIONS (Phase 1A)
            // ============================================================

            // Configure CrewStatusHistory
            modelBuilder.Entity<CrewStatusHistory>(entity =>
            {
                entity.ToTable("crew_status_history");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.ChangedAt);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OnboardingCase
            modelBuilder.Entity<OnboardingCase>(entity =>
            {
                entity.ToTable("onboarding_cases");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OnboardingChecklistItem
            modelBuilder.Entity<OnboardingChecklistItem>(entity =>
            {
                entity.ToTable("onboarding_checklist_items");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OnboardingCaseId);
                entity.HasIndex(e => new { e.OnboardingCaseId, e.Status });

                entity.HasOne(e => e.OnboardingCase)
                    .WithMany(c => c.ChecklistItems)
                    .HasForeignKey(e => e.OnboardingCaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CrewDocumentSubmission
            modelBuilder.Entity<CrewDocumentSubmission>(entity =>
            {
                entity.ToTable("crew_document_submissions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => new { e.CrewMemberId, e.DocumentType, e.IsActiveSubmission });
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.IssuingCountry)
                    .WithMany()
                    .HasForeignKey(e => e.IssuingCountryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure CrewDocumentVersion
            modelBuilder.Entity<CrewDocumentVersion>(entity =>
            {
                entity.ToTable("crew_document_versions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SubmissionId);
                entity.HasIndex(e => new { e.SubmissionId, e.IsActiveVersion });

                entity.HasOne(e => e.Submission)
                    .WithMany(s => s.Versions)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DocumentVerificationTask
            modelBuilder.Entity<DocumentVerificationTask>(entity =>
            {
                entity.ToTable("document_verification_tasks");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SubmissionId);
                entity.HasIndex(e => new { e.Status, e.Priority });
                entity.HasIndex(e => e.AssignedTo);
                entity.HasIndex(e => e.DueAt);

                entity.HasOne(e => e.Submission)
                    .WithMany(s => s.VerificationTasks)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Version)
                    .WithMany()
                    .HasForeignKey(e => e.VersionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DocumentVerificationAction
            modelBuilder.Entity<DocumentVerificationAction>(entity =>
            {
                entity.ToTable("document_verification_actions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TaskId);

                entity.HasOne(e => e.Task)
                    .WithMany(t => t.Actions)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasIndex(e => e.Actor);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.CorrelationId);
            });

            // ============================================================
            // COMPLIANCE MATRIX CONFIGURATIONS (Phase 2)
            // ============================================================

            modelBuilder.Entity<ComplianceRuleSet>(entity =>
            {
                entity.ToTable("compliance_rule_sets");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<ComplianceRule>(entity =>
            {
                entity.ToTable("compliance_rules");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RuleSetId);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.EvaluationStage);
                entity.HasIndex(e => e.RequiredCertificateId);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.RuleSet)
                    .WithMany(rs => rs.Rules)
                    .HasForeignKey(e => e.RuleSetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ComplianceDimension>(entity =>
            {
                entity.ToTable("compliance_dimensions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RuleId);
                entity.HasIndex(e => new { e.DimensionType, e.Value });

                entity.HasOne(e => e.Rule)
                    .WithMany(r => r.Dimensions)
                    .HasForeignKey(e => e.RuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ComplianceWaiver>(entity =>
            {
                entity.ToTable("compliance_waivers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.RuleId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.CrewMemberId, e.RuleId, e.Status });

                entity.HasOne(e => e.Rule)
                    .WithMany()
                    .HasForeignKey(e => e.RuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ComplianceSnapshot>(entity =>
            {
                entity.ToTable("compliance_snapshots");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => new { e.CrewMemberId, e.VesselId });
                entity.HasIndex(e => e.OverallResult);
                entity.HasIndex(e => e.EvaluatedAt);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================================
            // PLANNING & ASSIGNMENT CONFIGURATIONS (Phase 5)
            // ============================================================

            modelBuilder.Entity<VesselManningStandard>(entity =>
            {
                entity.ToTable("vessel_manning_standards");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<ManningPosition>(entity =>
            {
                entity.ToTable("manning_positions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ManningStandardId);
                entity.HasIndex(e => e.RankId);

                entity.HasOne(e => e.ManningStandard)
                    .WithMany(s => s.Positions)
                    .HasForeignKey(e => e.ManningStandardId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CrewAssignment>(entity =>
            {
                entity.ToTable("crew_assignments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.CrewMemberId, e.Status });
                entity.HasIndex(e => new { e.VesselId, e.Status });
                entity.HasIndex(e => new { e.PlannedStartDate, e.PlannedEndDate });

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ManningPosition)
                    .WithMany()
                    .HasForeignKey(e => e.ManningPositionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AssignmentConfirmation>(entity =>
            {
                entity.ToTable("assignment_confirmations");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssignmentId);

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Confirmations)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssignmentConflict>(entity =>
            {
                entity.ToTable("assignment_conflicts");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssignmentId);
                entity.HasIndex(e => new { e.AssignmentId, e.IsResolved });

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Conflicts)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssignmentComment>(entity =>
            {
                entity.ToTable("assignment_comments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssignmentId);

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssignmentStatusHistory>(entity =>
            {
                entity.ToTable("assignment_status_history");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssignmentId);

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.StatusHistory)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================================
            // EXTERNAL REQUESTS & TRAVEL CONFIGURATIONS (Phase 6)
            // ============================================================

            modelBuilder.Entity<ExternalRequest>(entity =>
            {
                entity.ToTable("external_requests");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ExternalCandidate>(entity =>
            {
                entity.ToTable("external_candidates");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ExternalRequestId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.ExternalRequest)
                    .WithMany(r => r.Candidates)
                    .HasForeignKey(e => e.ExternalRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ExternalRequestMessage>(entity =>
            {
                entity.ToTable("external_request_messages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ExternalRequestId);

                entity.HasOne(e => e.ExternalRequest)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(e => e.ExternalRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TravelRequest>(entity =>
            {
                entity.ToTable("travel_requests");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssignmentId);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.EstimatedCost).HasPrecision(12, 2);
            });

            modelBuilder.Entity<TravelSegment>(entity =>
            {
                entity.ToTable("travel_segments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TravelRequestId);

                entity.HasOne(e => e.TravelRequest)
                    .WithMany(t => t.Segments)
                    .HasForeignKey(e => e.TravelRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TravelStatusHistory>(entity =>
            {
                entity.ToTable("travel_status_history");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TravelRequestId);

                entity.HasOne(e => e.TravelRequest)
                    .WithMany(t => t.StatusHistory)
                    .HasForeignKey(e => e.TravelRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================================
            // PHASE 7: ONBOARD EVENTS, ACCESS & SIGN-ON/SIGN-OFF
            // ============================================================

            modelBuilder.Entity<OnboardEvent>(entity =>
            {
                entity.ToTable("onboard_events");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.EventTimestamp);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.OriginalEvent)
                    .WithMany()
                    .HasForeignKey(e => e.OriginalEventId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CrewAccessGrant>(entity =>
            {
                entity.ToTable("crew_access_grants");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SignOnRecord>(entity =>
            {
                entity.ToTable("sign_on_records");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.SignOnDate);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.OnboardEvent)
                    .WithMany()
                    .HasForeignKey(e => e.OnboardEventId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SignOffRecord>(entity =>
            {
                entity.ToTable("sign_off_records");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CrewMemberId);
                entity.HasIndex(e => e.VesselId);
                entity.HasIndex(e => e.SignOffDate);

                entity.HasOne(e => e.CrewMember)
                    .WithMany()
                    .HasForeignKey(e => e.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rank)
                    .WithMany()
                    .HasForeignKey(e => e.RankId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.OnboardEvent)
                    .WithMany()
                    .HasForeignKey(e => e.OnboardEventId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SignOnRecord)
                    .WithMany()
                    .HasForeignKey(e => e.SignOnRecordId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
