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
        public DbSet<Certificate> Certificates { get; set; } = null!;

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

            // Configure Certificate
            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.HasOne(c => c.Vessel)
                    .WithMany()
                    .HasForeignKey(c => c.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.VesselId, c.ExpiryDate });
                entity.HasIndex(c => c.CertificateNumber).IsUnique();
            });
        }
    }
}
