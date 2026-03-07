using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

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
        }
    }
}
