using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Models;

namespace MaritimeEdge.Data;

/// <summary>
/// PostgreSQL Database Context for Maritime Edge Server
/// Stores all sensor data, telemetry, and sync queue for offline operation
/// Uses EF Core Code-First Migrations
/// </summary>
public class EdgeDbContext : DbContext
{
    public EdgeDbContext(DbContextOptions<EdgeDbContext> options) : base(options)
    {
    }

    // NMEA & Position Data
    public DbSet<NmeaRawData> NmeaRawData { get; set; } = null!;
    public DbSet<PositionData> PositionData { get; set; } = null!;
    public DbSet<AisData> AisData { get; set; } = null!;
    public DbSet<NavigationData> NavigationData { get; set; } = null!;

    // Engine & Machinery
    public DbSet<EngineData> EngineData { get; set; } = null!;
    public DbSet<GeneratorData> GeneratorData { get; set; } = null!;
    public DbSet<TankLevel> TankLevels { get; set; } = null!;

    // Fuel & Environmental
    public DbSet<FuelConsumption> FuelConsumption { get; set; } = null!;
    public DbSet<EnvironmentalData> EnvironmentalData { get; set; } = null!;

    // Safety & Voyages
    public DbSet<SafetyAlarm> SafetyAlarms { get; set; } = null!;
    public DbSet<VoyageRecord> VoyageRecords { get; set; } = null!;

    // Sync Queue
    public DbSet<SyncQueue> SyncQueue { get; set; } = null!;

    // Critical Operational Tables (SOLAS/ISM/MARPOL)
    public DbSet<CrewMember> CrewMembers { get; set; } = null!;
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; } = null!;
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    public DbSet<TaskDetail> TaskDetails { get; set; } = null!;
    public DbSet<MaintenanceTaskDetail> MaintenanceTaskDetails { get; set; } = null!;
    public DbSet<CargoOperation> CargoOperations { get; set; } = null!;
    public DbSet<WatchkeepingLog> WatchkeepingLogs { get; set; } = null!;
    public DbSet<OilRecordBook> OilRecordBooks { get; set; } = null!;

    // Inventory & Materials
    public DbSet<MaterialCategory> MaterialCategories { get; set; } = null!;
    public DbSet<MaterialItem> MaterialItems { get; set; } = null!;

    // Fuel Analytics (IMO DCS / EU MRV / CII Compliance)
    public DbSet<FuelAnalyticsSummary> FuelAnalyticsSummaries { get; set; } = null!;
    public DbSet<FuelEfficiencyAlert> FuelEfficiencyAlerts { get; set; } = null!;

    // Authentication & Authorization
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    // Maritime Reporting System (IMO/SOLAS/MARPOL Compliance)
    public DbSet<ReportType> ReportTypes { get; set; } = null!;
    public DbSet<MaritimeReport> MaritimeReports { get; set; } = null!;
    public DbSet<NoonReport> NoonReports { get; set; } = null!;
    public DbSet<DepartureReport> DepartureReports { get; set; } = null!;
    public DbSet<ArrivalReport> ArrivalReports { get; set; } = null!;
    public DbSet<BunkerReport> BunkerReports { get; set; } = null!;
    public DbSet<PositionReport> PositionReports { get; set; } = null!;
    public DbSet<ReportAttachment> ReportAttachments { get; set; } = null!;
    public DbSet<ReportDistribution> ReportDistributions { get; set; } = null!;
    public DbSet<ReportTransmissionLog> ReportTransmissionLogs { get; set; } = null!;
    public DbSet<ReportWorkflowHistory> ReportWorkflowHistories { get; set; } = null!;
    public DbSet<ReportAmendment> ReportAmendments { get; set; } = null!;
    public DbSet<WeeklyPerformanceReport> WeeklyPerformanceReports { get; set; } = null!;
    public DbSet<MonthlySummaryReport> MonthlySummaryReports { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL specific configurations
        modelBuilder.HasDefaultSchema("public");

        // Configure naming convention to snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));

            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // Convert keys to snake_case
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? ""));
            }

            // Convert foreign keys to snake_case
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? ""));
            }

            // Convert indexes to snake_case (names are already set)
            foreach (var index in entity.GetIndexes())
            {
                if (index.Name != null && !index.Name.StartsWith("idx_"))
                {
                    index.SetDatabaseName(ToSnakeCase(index.Name));
                }
            }
        }

        // ========== POSITION DATA ==========
        modelBuilder.Entity<PositionData>(entity =>
        {
            entity.ToTable("position_data");
            
            // Precision for coordinates (PostgreSQL supports high precision)
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10,7)")
                .HasComment("Latitude in decimal degrees (-90 to +90)");
            
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10,7)")
                .HasComment("Longitude in decimal degrees (-180 to +180)");
            
            entity.Property(e => e.Altitude)
                .HasColumnType("decimal(8,2)")
                .HasComment("Altitude in meters above MSL");
            
            entity.Property(e => e.SpeedOverGround)
                .HasColumnType("decimal(5,2)")
                .HasComment("Speed in knots");
            
            entity.Property(e => e.CourseOverGround)
                .HasColumnType("decimal(5,2)")
                .HasComment("Course in degrees true");
            
            entity.Property(e => e.Hdop)
                .HasColumnType("decimal(4,2)");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_position_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_position_synced")
                .HasFilter("is_synced = false");
            
            entity.HasIndex(e => new { e.Timestamp, e.IsSynced })
                .HasDatabaseName("idx_position_timestamp_synced");
        });

        // ========== AIS DATA ==========
        modelBuilder.Entity<AisData>(entity =>
        {
            entity.ToTable("ais_data");
            
            entity.Property(e => e.SpeedOverGround)
                .HasColumnType("decimal(5,2)");
            
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10,7)");
            
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10,7)");
            
            entity.Property(e => e.CourseOverGround)
                .HasColumnType("decimal(5,2)");
            
            entity.Property(e => e.RateOfTurn)
                .HasColumnType("decimal(6,2)");
            
            entity.Property(e => e.Draught)
                .HasColumnType("decimal(4,2)");
            
            entity.HasIndex(e => new { e.Mmsi, e.Timestamp })
                .HasDatabaseName("idx_ais_mmsi_timestamp");
            
            entity.HasIndex(e => e.MessageType)
                .HasDatabaseName("idx_ais_message_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_ais_synced")
                .HasFilter("is_synced = false");
        });

        // ========== NMEA RAW DATA ==========
        modelBuilder.Entity<NmeaRawData>(entity =>
        {
            entity.ToTable("nmea_raw_data");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_nmea_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => e.SentenceType)
                .HasDatabaseName("idx_nmea_sentence_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_nmea_synced")
                .HasFilter("is_synced = false");
        });

        // ========== NAVIGATION DATA ==========
        modelBuilder.Entity<NavigationData>(entity =>
        {
            entity.ToTable("navigation_data");
            
            entity.Property(e => e.HeadingTrue).HasColumnType("decimal(5,2)");
            entity.Property(e => e.HeadingMagnetic).HasColumnType("decimal(5,2)");
            entity.Property(e => e.RateOfTurn).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Pitch).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Roll).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SpeedThroughWater).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Depth).HasColumnType("decimal(8,2)");
            entity.Property(e => e.WindSpeedRelative).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindDirectionRelative).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindSpeedTrue).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindDirectionTrue).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_navigation_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_navigation_synced")
                .HasFilter("is_synced = false");
        });

        // ========== ENGINE DATA ==========
        modelBuilder.Entity<EngineData>(entity =>
        {
            entity.ToTable("engine_data");
            
            entity.Property(e => e.Rpm).HasColumnType("decimal(6,2)");
            entity.Property(e => e.LoadPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.CoolantTemp).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ExhaustTemp).HasColumnType("decimal(6,2)");
            entity.Property(e => e.LubeOilPressure).HasColumnType("decimal(5,2)");
            entity.Property(e => e.LubeOilTemp).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FuelPressure).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FuelRate).HasColumnType("decimal(8,2)");
            entity.Property(e => e.RunningHours).HasColumnType("decimal(10,2)");
            
            entity.HasIndex(e => new { e.EngineId, e.Timestamp })
                .HasDatabaseName("idx_engine_id_timestamp");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_engine_synced")
                .HasFilter("is_synced = false");
            
            entity.HasIndex(e => e.AlarmStatus)
                .HasDatabaseName("idx_engine_alarm_status")
                .HasFilter("alarm_status > 0");
        });

        // ========== GENERATOR DATA ==========
        modelBuilder.Entity<GeneratorData>(entity =>
        {
            entity.ToTable("generator_data");
            
            entity.Property(e => e.Voltage).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Frequency).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Current).HasColumnType("decimal(8,2)");
            entity.Property(e => e.ActivePower).HasColumnType("decimal(8,2)");
            entity.Property(e => e.PowerFactor).HasColumnType("decimal(4,3)");
            entity.Property(e => e.RunningHours).HasColumnType("decimal(10,2)");
            entity.Property(e => e.LoadPercent).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => new { e.GeneratorId, e.Timestamp })
                .HasDatabaseName("idx_generator_id_timestamp");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_generator_synced")
                .HasFilter("is_synced = false");
        });

        // ========== TANK LEVELS ==========
        modelBuilder.Entity<TankLevel>(entity =>
        {
            entity.ToTable("tank_levels");
            
            entity.Property(e => e.LevelPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.VolumeLiters).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => new { e.TankId, e.Timestamp })
                .HasDatabaseName("idx_tank_id_timestamp");
            
            entity.HasIndex(e => e.TankType)
                .HasDatabaseName("idx_tank_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_tank_synced")
                .HasFilter("is_synced = false");
        });

        // ========== FUEL CONSUMPTION ==========
        modelBuilder.Entity<FuelConsumption>(entity =>
        {
            entity.ToTable("fuel_consumption");
            
            entity.Property(e => e.ConsumedVolume).HasColumnType("decimal(10,3)");
            entity.Property(e => e.ConsumedMass).HasColumnType("decimal(10,3)");
            entity.Property(e => e.Density).HasColumnType("decimal(6,2)");
            entity.Property(e => e.DistanceTraveled).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TimeUnderway).HasColumnType("decimal(8,2)");
            entity.Property(e => e.CargoWeight).HasColumnType("decimal(12,3)");
            entity.Property(e => e.Co2Emissions).HasColumnType("decimal(10,3)");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_fuel_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => e.FuelType)
                .HasDatabaseName("idx_fuel_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_fuel_synced")
                .HasFilter("is_synced = false");
        });

        // ========== ENVIRONMENTAL DATA ==========
        modelBuilder.Entity<EnvironmentalData>(entity =>
        {
            entity.ToTable("environmental_data");
            
            entity.Property(e => e.AirTemperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.BarometricPressure).HasColumnType("decimal(7,2)");
            entity.Property(e => e.Humidity).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SeaTemperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindSpeed).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindDirection).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WaveHeight).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Visibility).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_environmental_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_environmental_synced")
                .HasFilter("is_synced = false");
        });

        // ========== SAFETY ALARMS ==========
        modelBuilder.Entity<SafetyAlarm>(entity =>
        {
            entity.ToTable("safety_alarms");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_alarm_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => new { e.IsResolved, e.Severity })
                .HasDatabaseName("idx_alarm_unresolved_severity")
                .HasFilter("is_resolved = false");
            
            entity.HasIndex(e => e.AlarmType)
                .HasDatabaseName("idx_alarm_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_alarm_synced")
                .HasFilter("is_synced = false");
        });

        // ========== VOYAGE RECORDS ==========
        modelBuilder.Entity<VoyageRecord>(entity =>
        {
            entity.ToTable("voyage_records");
            
            entity.Property(e => e.CargoWeight).HasColumnType("decimal(12,3)");
            entity.Property(e => e.DistanceTraveled).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FuelConsumed).HasColumnType("decimal(10,3)");
            entity.Property(e => e.AverageSpeed).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => e.VoyageNumber)
                .IsUnique()
                .HasDatabaseName("idx_voyage_number_unique");
            
            entity.HasIndex(e => e.VoyageStatus)
                .HasDatabaseName("idx_voyage_status");
            
            entity.HasIndex(e => e.DepartureTime)
                .HasDatabaseName("idx_voyage_departure")
                .IsDescending();
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_voyage_synced")
                .HasFilter("is_synced = false");
        });

        // ========== SYNC QUEUE ==========
        modelBuilder.Entity<SyncQueue>(entity =>
        {
            entity.ToTable("sync_queue");
            
            entity.HasIndex(e => new { e.Priority, e.NextRetryAt })
                .HasDatabaseName("idx_sync_priority_retry")
                .HasFilter("synced_at IS NULL");
            
            entity.HasIndex(e => e.TableName)
                .HasDatabaseName("idx_sync_table");
            
            entity.HasIndex(e => e.SyncedAt)
                .HasDatabaseName("idx_sync_synced_at");
            
            entity.HasIndex(e => new { e.TableName, e.RecordId })
                .HasDatabaseName("idx_sync_table_record");
        });

        // ========== CREW MEMBERS ==========
        modelBuilder.Entity<CrewMember>(entity =>
        {
            entity.ToTable("crew_members");
            
            entity.HasIndex(e => e.CrewId)
                .IsUnique()
                .HasDatabaseName("idx_crew_id_unique");
            
            entity.HasIndex(e => e.IsOnboard)
                .HasDatabaseName("idx_crew_onboard")
                .HasFilter("is_onboard = true");
            
            entity.HasIndex(e => e.Position)
                .HasDatabaseName("idx_crew_position");
            
            entity.HasIndex(e => e.CertificateExpiry)
                .HasDatabaseName("idx_crew_cert_expiry");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_crew_synced")
                .HasFilter("is_synced = false");
        });

        // ========== MAINTENANCE TASKS ==========
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.ToTable("maintenance_tasks");
            
            entity.Property(e => e.IntervalHours).HasColumnType("decimal(10,2)");
            entity.Property(e => e.RunningHoursAtLastDone).HasColumnType("decimal(10,2)");
            
            entity.HasIndex(e => e.TaskId)
                .IsUnique()
                .HasDatabaseName("idx_maintenance_task_id_unique");
            
            entity.HasIndex(e => e.EquipmentId)
                .HasDatabaseName("idx_maintenance_equipment");
            
            entity.HasIndex(e => e.NextDueAt)
                .HasDatabaseName("idx_maintenance_next_due");
            
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_maintenance_status");
            
            entity.HasIndex(e => new { e.Status, e.Priority })
                .HasDatabaseName("idx_maintenance_status_priority")
                .HasFilter("status IN ('PENDING', 'OVERDUE', 'IN_PROGRESS')");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_maintenance_synced")
                .HasFilter("is_synced = false");

            entity.HasIndex(e => e.TaskTypeId)
                .HasDatabaseName("idx_maintenance_task_type_id");

            // Foreign key to TaskType (optional)
            entity.HasOne<TaskType>()
                .WithMany()
                .HasForeignKey(e => e.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========== TASK TYPES ==========
        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.ToTable("task_types");

            entity.HasIndex(e => e.TypeCode)
                .IsUnique()
                .HasDatabaseName("idx_task_type_code_unique");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("idx_task_type_category");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_task_type_active")
                .HasFilter("is_active = true");
        });

        // ========== TASK DETAILS ==========
        modelBuilder.Entity<TaskDetail>(entity =>
        {
            entity.ToTable("task_details");

            entity.Property(e => e.MinValue).HasColumnType("decimal(10,3)");
            entity.Property(e => e.MaxValue).HasColumnType("decimal(10,3)");

            entity.HasIndex(e => e.TaskTypeId)
                .HasDatabaseName("idx_task_detail_type_id");

            entity.HasIndex(e => new { e.TaskTypeId, e.OrderIndex })
                .HasDatabaseName("idx_task_detail_type_order");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_task_detail_active")
                .HasFilter("is_active = true");

            // Foreign key to TaskType - now optional (nullable)
            entity.HasOne<TaskType>()
                .WithMany()
                .HasForeignKey(e => e.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // ========== MAINTENANCE TASK DETAILS (N-N junction table) ==========
        modelBuilder.Entity<MaintenanceTaskDetail>(entity =>
        {
            entity.ToTable("maintenance_task_details");

            entity.Property(e => e.MeasuredValue).HasColumnType("decimal(10,3)");

            entity.HasIndex(e => e.MaintenanceTaskId)
                .HasDatabaseName("idx_mtd_maintenance_task_id");

            entity.HasIndex(e => e.TaskDetailId)
                .HasDatabaseName("idx_mtd_task_detail_id");

            entity.HasIndex(e => new { e.MaintenanceTaskId, e.TaskDetailId })
                .IsUnique()
                .HasDatabaseName("idx_mtd_task_detail_unique");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_mtd_status");

            entity.HasIndex(e => e.IsCompleted)
                .HasDatabaseName("idx_mtd_completed")
                .HasFilter("is_completed = false");

            // Foreign key to MaintenanceTask
            entity.HasOne<MaintenanceTask>()
                .WithMany()
                .HasForeignKey(e => e.MaintenanceTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to TaskDetail
            entity.HasOne<TaskDetail>()
                .WithMany()
                .HasForeignKey(e => e.TaskDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== CARGO OPERATIONS ==========
        modelBuilder.Entity<CargoOperation>(entity =>
        {
            entity.ToTable("cargo_operations");
            
            entity.Property(e => e.Quantity).HasColumnType("decimal(15,3)");
            
            entity.HasIndex(e => e.OperationId)
                .IsUnique()
                .HasDatabaseName("idx_cargo_operation_id_unique");
            
            entity.HasIndex(e => e.VoyageId)
                .HasDatabaseName("idx_cargo_voyage");
            
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_cargo_status");
            
            entity.HasIndex(e => e.CargoType)
                .HasDatabaseName("idx_cargo_type");
            
            entity.HasIndex(e => e.BillOfLading)
                .HasDatabaseName("idx_cargo_bol");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_cargo_synced")
                .HasFilter("is_synced = false");
        });

        // ========== WATCHKEEPING LOGS ==========
        modelBuilder.Entity<WatchkeepingLog>(entity =>
        {
            entity.ToTable("watchkeeping_logs");
            
            entity.Property(e => e.CourseLogged).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SpeedLogged).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PositionLat).HasColumnType("decimal(10,7)");
            entity.Property(e => e.PositionLon).HasColumnType("decimal(10,7)");
            entity.Property(e => e.DistanceRun).HasColumnType("decimal(8,2)");
            
            entity.HasIndex(e => e.WatchDate)
                .HasDatabaseName("idx_watchkeeping_date")
                .IsDescending();
            
            entity.HasIndex(e => new { e.WatchDate, e.WatchPeriod })
                .HasDatabaseName("idx_watchkeeping_date_period");
            
            entity.HasIndex(e => e.WatchType)
                .HasDatabaseName("idx_watchkeeping_type");
            
            entity.HasIndex(e => e.OfficerOnWatch)
                .HasDatabaseName("idx_watchkeeping_officer");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_watchkeeping_synced")
                .HasFilter("is_synced = false");
        });

        // ========== OIL RECORD BOOK ==========
        modelBuilder.Entity<OilRecordBook>(entity =>
        {
            entity.ToTable("oil_record_books");
            
            entity.Property(e => e.LocationLat).HasColumnType("decimal(10,7)");
            entity.Property(e => e.LocationLon).HasColumnType("decimal(10,7)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,3)");
            
            entity.HasIndex(e => e.EntryDate)
                .HasDatabaseName("idx_orb_entry_date")
                .IsDescending();
            
            entity.HasIndex(e => e.OperationCode)
                .HasDatabaseName("idx_orb_operation_code");
            
            entity.HasIndex(e => e.OfficerInCharge)
                .HasDatabaseName("idx_orb_officer");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_orb_synced")
                .HasFilter("is_synced = false");
        });

        // ========== MATERIAL CATEGORIES ==========
        modelBuilder.Entity<MaterialCategory>(entity =>
        {
            entity.ToTable("material_categories");

            entity.HasIndex(e => e.CategoryCode)
                .IsUnique()
                .HasDatabaseName("idx_material_category_code_unique");

            entity.HasIndex(e => e.ParentCategoryId)
                .HasDatabaseName("idx_material_category_parent");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_material_category_active")
                .HasFilter("is_active = true");

            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_material_category_synced")
                .HasFilter("is_synced = false");

            // Self reference
            entity.HasOne<MaterialCategory>()
                .WithMany()
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== MATERIAL ITEMS ==========
        modelBuilder.Entity<MaterialItem>(entity =>
        {
            entity.ToTable("material_items");

            entity.Property(e => e.OnHandQuantity).HasColumnType("decimal(14,3)");
            entity.Property(e => e.MinStock).HasColumnType("decimal(14,3)");
            entity.Property(e => e.MaxStock).HasColumnType("decimal(14,3)");
            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(14,3)");
            entity.Property(e => e.ReorderQuantity).HasColumnType("decimal(14,3)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.ItemCode)
                .IsUnique()
                .HasDatabaseName("idx_material_item_code_unique");

            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("idx_material_item_category");

            entity.HasIndex(e => e.Barcode)
                .HasDatabaseName("idx_material_item_barcode");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_material_item_active")
                .HasFilter("is_active = true");

            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_material_item_synced")
                .HasFilter("is_synced = false");

            entity.HasOne<MaterialCategory>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== FUEL ANALYTICS SUMMARY ==========
        modelBuilder.Entity<FuelAnalyticsSummary>(entity =>
        {
            entity.ToTable("fuel_analytics_summaries");
            
            entity.Property(e => e.DistanceNauticalMiles).HasColumnType("decimal(12,2)");
            entity.Property(e => e.TimeUnderwayHours).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TimeBerthHours).HasColumnType("decimal(10,2)");
            entity.Property(e => e.AverageSpeedKnots).HasColumnType("decimal(5,2)");
            
            entity.Property(e => e.TotalFuelConsumedMT).HasColumnType("decimal(10,3)");
            entity.Property(e => e.MainEngineFuelMT).HasColumnType("decimal(10,3)");
            entity.Property(e => e.AuxiliaryFuelMT).HasColumnType("decimal(10,3)");
            entity.Property(e => e.BoilerFuelMT).HasColumnType("decimal(10,3)");
            
            entity.Property(e => e.EEOI).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FuelPerNauticalMile).HasColumnType("decimal(8,4)");
            entity.Property(e => e.FuelPerHour).HasColumnType("decimal(8,4)");
            entity.Property(e => e.SFOC).HasColumnType("decimal(8,2)");
            
            entity.Property(e => e.CO2EmissionsMT).HasColumnType("decimal(12,3)");
            entity.Property(e => e.CII).HasColumnType("decimal(10,2)");
            
            entity.Property(e => e.AvgMainEngineRPM).HasColumnType("decimal(6,2)");
            entity.Property(e => e.AvgMainEngineLoad).HasColumnType("decimal(5,2)");
            entity.Property(e => e.AvgSeaState).HasColumnType("decimal(3,1)");
            entity.Property(e => e.AvgWindSpeed).HasColumnType("decimal(5,2)");
            entity.Property(e => e.CargoWeightMT).HasColumnType("decimal(12,3)");
            
            entity.Property(e => e.EstimatedFuelCostUSD).HasColumnType("decimal(15,2)");
            entity.Property(e => e.FuelPricePerMT).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DataQualityScore).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(e => new { e.PeriodType, e.PeriodStart })
                .HasDatabaseName("idx_fuel_analytics_period")
                .IsDescending(false, true);
            
            entity.HasIndex(e => e.VoyageId)
                .HasDatabaseName("idx_fuel_analytics_voyage");
            
            entity.HasIndex(e => e.CIIRating)
                .HasDatabaseName("idx_fuel_analytics_cii");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_fuel_analytics_synced")
                .HasFilter("is_synced = false");
        });

        // ========== FUEL EFFICIENCY ALERTS ==========
        modelBuilder.Entity<FuelEfficiencyAlert>(entity =>
        {
            entity.ToTable("fuel_efficiency_alerts");
            
            entity.Property(e => e.CurrentValue).HasColumnType("decimal(10,3)");
            entity.Property(e => e.ExpectedValue).HasColumnType("decimal(10,3)");
            entity.Property(e => e.DeviationPercent).HasColumnType("decimal(6,2)");
            
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_fuel_alert_timestamp")
                .IsDescending();
            
            entity.HasIndex(e => new { e.IsResolved, e.Severity })
                .HasDatabaseName("idx_fuel_alert_unresolved")
                .HasFilter("is_resolved = false");
            
            entity.HasIndex(e => e.AlertType)
                .HasDatabaseName("idx_fuel_alert_type");
            
            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_fuel_alert_synced")
                .HasFilter("is_synced = false");
        });

        // ========== ROLES ==========
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleCode)
                .IsUnique()
                .HasDatabaseName("idx_role_code_unique");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_role_active")
                .HasFilter("is_active = true");
        });

        // ========== USERS ==========
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("idx_user_username_unique");

            entity.HasIndex(e => e.CrewId)
                .HasDatabaseName("idx_user_crew_id");

            entity.HasIndex(e => e.RoleId)
                .HasDatabaseName("idx_user_role_id");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_user_active")
                .HasFilter("is_active = true");

            // Foreign key relationship with Role
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign key relationship with CrewMember (optional)
            entity.HasOne<CrewMember>()
                .WithMany()
                .HasForeignKey(e => e.CrewId)
                .HasPrincipalKey(c => c.CrewId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== MARITIME REPORTING SYSTEM ==========

        // ========== REPORT TYPES ==========
        modelBuilder.Entity<ReportType>(entity =>
        {
            entity.ToTable("report_types");

            entity.HasIndex(e => e.TypeCode)
                .IsUnique()
                .HasDatabaseName("idx_report_type_code_unique");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("idx_report_type_category");

            entity.HasIndex(e => e.IsMandatory)
                .HasDatabaseName("idx_report_type_mandatory")
                .HasFilter("is_mandatory = true");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_report_type_active")
                .HasFilter("is_active = true");
        });

        // ========== MARITIME REPORTS (Polymorphic Parent) ==========
        modelBuilder.Entity<MaritimeReport>(entity =>
        {
            entity.ToTable("maritime_reports");

            entity.HasIndex(e => e.ReportNumber)
                .IsUnique()
                .HasDatabaseName("idx_report_number_unique");

            entity.HasIndex(e => e.ReportTypeId)
                .HasDatabaseName("idx_report_type_id");

            entity.HasIndex(e => e.VoyageId)
                .HasDatabaseName("idx_report_voyage_id");

            entity.HasIndex(e => e.ReportDateTime)
                .HasDatabaseName("idx_report_datetime")
                .IsDescending();

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_report_status");

            entity.HasIndex(e => new { e.Status, e.ReportDateTime })
                .HasDatabaseName("idx_report_status_datetime")
                .HasFilter("status IN ('DRAFT', 'SUBMITTED')");

            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_report_synced")
                .HasFilter("is_synced = false");

            // Foreign key to ReportType
            entity.HasOne<ReportType>()
                .WithMany()
                .HasForeignKey(e => e.ReportTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign key to VoyageRecord (optional)
            entity.HasOne<VoyageRecord>()
                .WithMany()
                .HasForeignKey(e => e.VoyageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========== NOON REPORTS ==========
        modelBuilder.Entity<NoonReport>(entity =>
        {
            entity.ToTable("noon_reports");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_noon_report_id");

            entity.HasIndex(e => e.ReportDate)
                .HasDatabaseName("idx_noon_date")
                .IsDescending();

            // Foreign key to MaritimeReport (one-to-one)
            entity.HasOne<MaritimeReport>()
                .WithOne()
                .HasForeignKey<NoonReport>(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== DEPARTURE REPORTS ==========
        modelBuilder.Entity<DepartureReport>(entity =>
        {
            entity.ToTable("departure_reports");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_departure_report_id");

            entity.HasIndex(e => e.PortName)
                .HasDatabaseName("idx_departure_port");

            entity.HasIndex(e => e.DepartureDateTime)
                .HasDatabaseName("idx_departure_datetime")
                .IsDescending();

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithOne()
                .HasForeignKey<DepartureReport>(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ARRIVAL REPORTS ==========
        modelBuilder.Entity<ArrivalReport>(entity =>
        {
            entity.ToTable("arrival_reports");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_arrival_report_id");

            entity.HasIndex(e => e.PortName)
                .HasDatabaseName("idx_arrival_port");

            entity.HasIndex(e => e.ArrivalDateTime)
                .HasDatabaseName("idx_arrival_datetime")
                .IsDescending();

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithOne()
                .HasForeignKey<ArrivalReport>(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== BUNKER REPORTS ==========
        modelBuilder.Entity<BunkerReport>(entity =>
        {
            entity.ToTable("bunker_reports");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_bunker_report_id");

            entity.HasIndex(e => e.PortName)
                .HasDatabaseName("idx_bunker_port");

            entity.HasIndex(e => e.BunkerDate)
                .HasDatabaseName("idx_bunker_date")
                .IsDescending();

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithOne()
                .HasForeignKey<BunkerReport>(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== POSITION REPORTS ==========
        modelBuilder.Entity<PositionReport>(entity =>
        {
            entity.ToTable("position_reports");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_position_report_id");

            entity.HasIndex(e => e.ReportDateTime)
                .HasDatabaseName("idx_position_report_datetime")
                .IsDescending();

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithOne()
                .HasForeignKey<PositionReport>(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== REPORT ATTACHMENTS ==========
        modelBuilder.Entity<ReportAttachment>(entity =>
        {
            entity.ToTable("report_attachments");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_attachment_report_id");

            entity.HasIndex(e => e.IsSynced)
                .HasDatabaseName("idx_attachment_synced")
                .HasFilter("is_synced = false");

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithMany()
                .HasForeignKey(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== REPORT DISTRIBUTION (N-N Junction Table) ==========
        modelBuilder.Entity<ReportDistribution>(entity =>
        {
            entity.ToTable("report_distributions");

            entity.HasIndex(e => e.ReportTypeId)
                .HasDatabaseName("idx_distribution_report_type");

            entity.HasIndex(e => e.RecipientType)
                .HasDatabaseName("idx_distribution_recipient_type");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_distribution_active")
                .HasFilter("is_active = true");

            // Foreign key to ReportType
            entity.HasOne<ReportType>()
                .WithMany()
                .HasForeignKey(e => e.ReportTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== REPORT TRANSMISSION LOG ==========
        modelBuilder.Entity<ReportTransmissionLog>(entity =>
        {
            entity.ToTable("report_transmission_logs");

            entity.HasIndex(e => e.MaritimeReportId)
                .HasDatabaseName("idx_transmission_report_id");

            entity.HasIndex(e => e.TransmissionDateTime)
                .HasDatabaseName("idx_transmission_datetime")
                .IsDescending();

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_transmission_status");

            entity.HasIndex(e => new { e.Status, e.RetryCount })
                .HasDatabaseName("idx_transmission_failed_retry")
                .HasFilter("status = 'FAILED'");

            // Foreign key to MaritimeReport
            entity.HasOne<MaritimeReport>()
                .WithMany()
                .HasForeignKey(e => e.MaritimeReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Initialize database with migrations
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        // Use migrations instead of EnsureCreated for production
        await Database.MigrateAsync();
    }

    /// <summary>
    /// Cleanup old data based on retention policy
    /// </summary>
    public async Task CleanupOldDataAsync(Dictionary<string, int> retentionDays)
    {
        var cutoffDates = retentionDays.ToDictionary(
            kvp => kvp.Key,
            kvp => DateTime.UtcNow.AddDays(-kvp.Value)
        );

        // Position Data
        if (cutoffDates.TryGetValue("PositionData", out var positionCutoff))
        {
            var oldPositions = await PositionData
                .Where(p => p.Timestamp < positionCutoff && p.IsSynced)
                .ToListAsync();
            PositionData.RemoveRange(oldPositions);
        }

        // AIS Data
        if (cutoffDates.TryGetValue("AisData", out var aisCutoff))
        {
            var oldAis = await AisData
                .Where(a => a.Timestamp < aisCutoff && a.IsSynced)
                .ToListAsync();
            AisData.RemoveRange(oldAis);
        }

        // Engine Data
        if (cutoffDates.TryGetValue("EngineData", out var engineCutoff))
        {
            var oldEngine = await EngineData
                .Where(e => e.Timestamp < engineCutoff && e.IsSynced)
                .ToListAsync();
            EngineData.RemoveRange(oldEngine);
        }

        // Environmental Data
        if (cutoffDates.TryGetValue("EnvironmentalData", out var envCutoff))
        {
            var oldEnv = await EnvironmentalData
                .Where(e => e.Timestamp < envCutoff && e.IsSynced)
                .ToListAsync();
            EnvironmentalData.RemoveRange(oldEnv);
        }

        // NMEA Raw Data (keep only recent for debugging)
        var nmeaCutoff = DateTime.UtcNow.AddDays(-1); // Keep only 1 day
        var oldNmea = await NmeaRawData
            .Where(n => n.Timestamp < nmeaCutoff && n.IsSynced)
            .ToListAsync();
        NmeaRawData.RemoveRange(oldNmea);

        // Synced queue items older than 7 days
        var syncQueueCutoff = DateTime.UtcNow.AddDays(-7);
        var oldSyncQueue = await SyncQueue
            .Where(s => s.SyncedAt != null && s.SyncedAt < syncQueueCutoff)
            .ToListAsync();
        SyncQueue.RemoveRange(oldSyncQueue);

        await SaveChangesAsync();
    }

    /// <summary>
    /// Get unsynchronized records count
    /// </summary>
    public async Task<Dictionary<string, int>> GetUnsyncedCountsAsync()
    {
        var counts = new Dictionary<string, int>
        {
            ["PositionData"] = await PositionData.CountAsync(p => !p.IsSynced),
            ["AisData"] = await AisData.CountAsync(a => !a.IsSynced),
            ["NavigationData"] = await NavigationData.CountAsync(n => !n.IsSynced),
            ["EngineData"] = await EngineData.CountAsync(e => !e.IsSynced),
            ["GeneratorData"] = await GeneratorData.CountAsync(g => !g.IsSynced),
            ["TankLevels"] = await TankLevels.CountAsync(t => !t.IsSynced),
            ["FuelConsumption"] = await FuelConsumption.CountAsync(f => !f.IsSynced),
            ["EnvironmentalData"] = await EnvironmentalData.CountAsync(e => !e.IsSynced),
            ["SafetyAlarms"] = await SafetyAlarms.CountAsync(s => !s.IsSynced),
            ["VoyageRecords"] = await VoyageRecords.CountAsync(v => !v.IsSynced)
        };

        return counts;
    }

    /// <summary>
    /// Convert PascalCase to snake_case
    /// </summary>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return string.Concat(
            input.Select((c, i) => i > 0 && char.IsUpper(c) 
                ? "_" + char.ToLower(c).ToString() 
                : char.ToLower(c).ToString())
        );
    }
}
