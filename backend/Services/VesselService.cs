using ProductApi.Data;
using ProductApi.Models;
using ProductApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Services
{
    public interface IVesselService
    {
        Task<IEnumerable<VesselDto>> GetAllVesselsAsync();
        Task<VesselDto?> GetVesselByIdAsync(Guid id);
        Task<VesselDto?> GetVesselByIMOAsync(string imo);
        Task<VesselDto> CreateVesselAsync(CreateVesselDto vesselDto);
        Task<VesselDto?> UpdateVesselAsync(Guid id, UpdateVesselDto vesselDto);
        Task<bool> DeleteVesselAsync(Guid id);
        Task<VesselPositionDto> AddPositionAsync(Guid vesselId, CreateVesselPositionDto positionDto);
        Task<IEnumerable<VesselPositionDto>> GetVesselPositionsAsync(Guid vesselId, DateTime? fromDate = null);
        Task<FuelConsumptionDto> AddFuelRecordAsync(Guid vesselId, CreateFuelConsumptionDto fuelDto);
        Task<IEnumerable<FuelConsumptionDto>> GetVesselFuelRecordsAsync(Guid vesselId, DateTime? fromDate = null);
    }

    public class VesselService : IVesselService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VesselService> _logger;

        public VesselService(AppDbContext context, ILogger<VesselService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VesselDto>> GetAllVesselsAsync()
        {
            var vessels = await _context.Vessels
                .Include(v => v.Positions.OrderByDescending(p => p.Timestamp).Take(1))
                .Include(v => v.Alerts.Where(a => !a.IsAcknowledged))
                .ToListAsync();

            return vessels.Select(MapToDto);
        }

        public async Task<VesselDto?> GetVesselByIdAsync(Guid id)
        {
            var vessel = await _context.Vessels
                .Include(v => v.Positions.OrderByDescending(p => p.Timestamp))
                .Include(v => v.FuelRecords.OrderByDescending(f => f.ReportDate))
                .Include(v => v.PortCalls.OrderByDescending(p => p.ArrivalTime))
                .Include(v => v.Alerts.Where(a => !a.IsAcknowledged))
                .FirstOrDefaultAsync(v => v.Id == id);

            return vessel != null ? MapToDto(vessel) : null;
        }

        public async Task<VesselDto?> GetVesselByIMOAsync(string imo)
        {
            var vessel = await _context.Vessels
                .Include(v => v.Positions.OrderByDescending(p => p.Timestamp).Take(1))
                .FirstOrDefaultAsync(v => v.IMO == imo);

            return vessel != null ? MapToDto(vessel) : null;
        }

        public async Task<VesselDto> CreateVesselAsync(CreateVesselDto vesselDto)
        {
            var vessel = new Vessel
            {
                Id = Guid.NewGuid(),
                IMO = vesselDto.IMO,
                Name = vesselDto.Name,
                CallSign = vesselDto.CallSign,
                VesselType = vesselDto.VesselType,
                GrossTonnage = vesselDto.GrossTonnage,
                DeadWeight = vesselDto.DeadWeight,
                BuildDate = vesselDto.BuildDate,
                Flag = vesselDto.Flag,
                IsActive = true
            };

            _context.Vessels.Add(vessel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new vessel: {IMO} - {Name}", vessel.IMO, vessel.Name);
            return MapToDto(vessel);
        }

        public async Task<VesselDto?> UpdateVesselAsync(Guid id, UpdateVesselDto vesselDto)
        {
            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel == null) return null;

            vessel.Name = vesselDto.Name;
            vessel.CallSign = vesselDto.CallSign;
            vessel.VesselType = vesselDto.VesselType;
            vessel.GrossTonnage = vesselDto.GrossTonnage;
            vessel.DeadWeight = vesselDto.DeadWeight;
            vessel.Flag = vesselDto.Flag;
            vessel.IsActive = vesselDto.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated vessel: {IMO} - {Name}", vessel.IMO, vessel.Name);
            return MapToDto(vessel);
        }

        public async Task<bool> DeleteVesselAsync(Guid id)
        {
            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel == null) return false;

            vessel.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated vessel: {IMO} - {Name}", vessel.IMO, vessel.Name);
            return true;
        }

        public async Task<VesselPositionDto> AddPositionAsync(Guid vesselId, CreateVesselPositionDto positionDto)
        {
            var position = new VesselPosition
            {
                Id = Guid.NewGuid(),
                VesselId = vesselId,
                Latitude = positionDto.Latitude,
                Longitude = positionDto.Longitude,
                Speed = positionDto.Speed,
                Course = positionDto.Course,
                Timestamp = positionDto.Timestamp ?? DateTime.UtcNow,
                Source = positionDto.Source ?? "GPS"
            };

            _context.VesselPositions.Add(position);
            await _context.SaveChangesAsync();

            return new VesselPositionDto
            {
                Id = position.Id,
                VesselId = position.VesselId,
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                Speed = position.Speed,
                Course = position.Course,
                Timestamp = position.Timestamp,
                Source = position.Source
            };
        }

        public async Task<IEnumerable<VesselPositionDto>> GetVesselPositionsAsync(Guid vesselId, DateTime? fromDate = null)
        {
            var query = _context.VesselPositions
                .Where(vp => vp.VesselId == vesselId);

            if (fromDate.HasValue)
            {
                query = query.Where(vp => vp.Timestamp >= fromDate.Value);
            }

            var positions = await query
                .OrderByDescending(vp => vp.Timestamp)
                .Take(1000) // Limit to last 1000 positions
                .ToListAsync();

            return positions.Select(p => new VesselPositionDto
            {
                Id = p.Id,
                VesselId = p.VesselId,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Speed = p.Speed,
                Course = p.Course,
                Timestamp = p.Timestamp,
                Source = p.Source
            });
        }

        public async Task<FuelConsumptionDto> AddFuelRecordAsync(Guid vesselId, CreateFuelConsumptionDto fuelDto)
        {
            var fuelRecord = new FuelConsumption
            {
                Id = Guid.NewGuid(),
                VesselId = vesselId,
                ReportDate = fuelDto.ReportDate,
                FuelConsumed = fuelDto.FuelConsumed,
                FuelType = fuelDto.FuelType,
                DistanceTraveled = fuelDto.DistanceTraveled,
                AverageSpeed = fuelDto.AverageSpeed,
                FuelEfficiency = fuelDto.FuelConsumed / Math.Max(fuelDto.DistanceTraveled, 0.1)
            };

            _context.FuelConsumptions.Add(fuelRecord);
            await _context.SaveChangesAsync();

            return new FuelConsumptionDto
            {
                Id = fuelRecord.Id,
                VesselId = fuelRecord.VesselId,
                ReportDate = fuelRecord.ReportDate,
                FuelConsumed = fuelRecord.FuelConsumed,
                FuelType = fuelRecord.FuelType,
                DistanceTraveled = fuelRecord.DistanceTraveled,
                AverageSpeed = fuelRecord.AverageSpeed,
                FuelEfficiency = fuelRecord.FuelEfficiency
            };
        }

        public async Task<IEnumerable<FuelConsumptionDto>> GetVesselFuelRecordsAsync(Guid vesselId, DateTime? fromDate = null)
        {
            var query = _context.FuelConsumptions
                .Where(fc => fc.VesselId == vesselId);

            if (fromDate.HasValue)
            {
                query = query.Where(fc => fc.ReportDate >= fromDate.Value);
            }

            var fuelRecords = await query
                .OrderByDescending(fc => fc.ReportDate)
                .ToListAsync();

            return fuelRecords.Select(f => new FuelConsumptionDto
            {
                Id = f.Id,
                VesselId = f.VesselId,
                ReportDate = f.ReportDate,
                FuelConsumed = f.FuelConsumed,
                FuelType = f.FuelType,
                DistanceTraveled = f.DistanceTraveled,
                AverageSpeed = f.AverageSpeed,
                FuelEfficiency = f.FuelEfficiency
            });
        }

        private static VesselDto MapToDto(Vessel vessel)
        {
            return new VesselDto
            {
                Id = vessel.Id,
                IMO = vessel.IMO,
                Name = vessel.Name,
                CallSign = vessel.CallSign,
                VesselType = vessel.VesselType,
                GrossTonnage = vessel.GrossTonnage,
                DeadWeight = vessel.DeadWeight,
                BuildDate = vessel.BuildDate,
                Flag = vessel.Flag,
                IsActive = vessel.IsActive,
                LastPosition = vessel.Positions?.FirstOrDefault() != null 
                    ? new VesselPositionDto
                    {
                        Id = vessel.Positions.First().Id,
                        VesselId = vessel.Positions.First().VesselId,
                        Latitude = vessel.Positions.First().Latitude,
                        Longitude = vessel.Positions.First().Longitude,
                        Speed = vessel.Positions.First().Speed,
                        Course = vessel.Positions.First().Course,
                        Timestamp = vessel.Positions.First().Timestamp,
                        Source = vessel.Positions.First().Source
                    } : null,
                UnacknowledgedAlerts = vessel.Alerts?.Count(a => !a.IsAcknowledged) ?? 0
            };
        }
    }
}