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
        Task<VesselDto?> UpdateCommercialDataAsync(Guid id, UpdateCommercialDataDto commercialDto);
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
                .AsNoTracking()
                .Include(v => v.Positions.OrderByDescending(p => p.Timestamp).Take(1))
                .Include(v => v.Alerts.Where(a => !a.IsAcknowledged))
                .AsSplitQuery()
                .ToListAsync();

            return vessels.Select(MapToDto);
        }

        public async Task<VesselDto?> GetVesselByIdAsync(Guid id)
        {
            var vessel = await _context.Vessels
                .AsNoTracking()
                .Include(v => v.Positions.OrderByDescending(p => p.Timestamp).Take(100))
                .Include(v => v.FuelRecords.OrderByDescending(f => f.ReportDate).Take(100))
                .Include(v => v.PortCalls.OrderByDescending(p => p.ArrivalTime).Take(50))
                .Include(v => v.Alerts.Where(a => !a.IsAcknowledged))
                .AsSplitQuery()
                .FirstOrDefaultAsync(v => v.Id == id);

            return vessel != null ? MapToDto(vessel) : null;
        }

        public async Task<VesselDto?> GetVesselByIMOAsync(string imo)
        {
            var vessel = await _context.Vessels
                .AsNoTracking()
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

        public async Task<VesselDto?> UpdateCommercialDataAsync(Guid id, UpdateCommercialDataDto commercialDto)
        {
            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel == null) return null;

            // Update Shipowner fields (Shore Master)
            vessel.ShipownerName = commercialDto.ShipownerName;
            vessel.ShipownerStreet = commercialDto.ShipownerStreet;
            vessel.ShipownerCountry = commercialDto.ShipownerCountry;
            vessel.ShipownerZip = commercialDto.ShipownerZip;
            vessel.ShipownerCity = commercialDto.ShipownerCity;
            vessel.ShipownerPhone = commercialDto.ShipownerPhone;
            vessel.ShipownerFax = commercialDto.ShipownerFax;
            vessel.ShipownerEmail = commercialDto.ShipownerEmail;
            vessel.ShipownerContactPerson = commercialDto.ShipownerContactPerson;

            vessel.ManagingOwnerName = commercialDto.ManagingOwnerName;
            vessel.ManagingOwnerEmail = commercialDto.ManagingOwnerEmail;
            vessel.ManagingOwnerContactPerson = commercialDto.ManagingOwnerContactPerson;

            vessel.OperatorName = commercialDto.OperatorName;
            vessel.OperatorEmail = commercialDto.OperatorEmail;
            vessel.OperatorContactPerson = commercialDto.OperatorContactPerson;

            vessel.CsoFirstName = commercialDto.CsoFirstName;
            vessel.CsoLastName = commercialDto.CsoLastName;
            vessel.CsoEmail = commercialDto.CsoEmail;
            vessel.CsoPhone24h = commercialDto.CsoPhone24h;

            vessel.DpaFirstName = commercialDto.DpaFirstName;
            vessel.DpaLastName = commercialDto.DpaLastName;
            vessel.DpaEmail = commercialDto.DpaEmail;
            vessel.DpaPhone24h = commercialDto.DpaPhone24h;

            // Update Charterer fields (Shore Master)
            vessel.ChartererName = commercialDto.ChartererName;
            vessel.ChartererStreet = commercialDto.ChartererStreet;
            vessel.ChartererCountry = commercialDto.ChartererCountry;
            vessel.ChartererZip = commercialDto.ChartererZip;
            vessel.ChartererCity = commercialDto.ChartererCity;
            vessel.ChartererPhone = commercialDto.ChartererPhone;
            vessel.ChartererEmail = commercialDto.ChartererEmail;
            vessel.ChartererContactPerson = commercialDto.ChartererContactPerson;

            vessel.BareboatChartererName = commercialDto.BareboatChartererName;
            vessel.BareboatChartererEmail = commercialDto.BareboatChartererEmail;
            vessel.BareboatChartererContactPerson = commercialDto.BareboatChartererContactPerson;

            // Update Insurance fields (Shore Master)
            vessel.PiClubName = commercialDto.PiClubName;
            vessel.PiClubStreet = commercialDto.PiClubStreet;
            vessel.PiClubCountry = commercialDto.PiClubCountry;
            vessel.PiClubZip = commercialDto.PiClubZip;
            vessel.PiClubCity = commercialDto.PiClubCity;
            vessel.PiClubPhone = commercialDto.PiClubPhone;
            vessel.PiClubEmail = commercialDto.PiClubEmail;
            vessel.PiClubContactPerson = commercialDto.PiClubContactPerson;

            vessel.HmClubName = commercialDto.HmClubName;
            vessel.HmClubEmail = commercialDto.HmClubEmail;
            vessel.HmClubContactPerson = commercialDto.HmClubContactPerson;

            // Update sync metadata
            vessel.LastShoreSyncAt = DateTime.UtcNow;
vessel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated commercial data for vessel: {IMO} - {Name}", vessel.IMO, vessel.Name);

            // TODO: Enqueue to SyncOutbox for Shore → Edge replication
            // await EnqueueCommercialDataToSyncOutbox(vessel);

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
                .AsNoTracking()
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
                .AsNoTracking()
                .Where(fc => fc.VesselId == vesselId);

            if (fromDate.HasValue)
            {
                query = query.Where(fc => fc.ReportDate >= fromDate.Value);
            }

            var fuelRecords = await query
                .OrderByDescending(fc => fc.ReportDate)
                .Take(500) // Limit results
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
                UnacknowledgedAlerts = vessel.Alerts?.Count(a => !a.IsAcknowledged) ?? 0,

                // Extended Basic Data
                OfficialNumber = vessel.OfficialNumber,
                PortOfRegistry = vessel.PortOfRegistry,
                PreviousName = vessel.PreviousName,
                PreviousFlag = vessel.PreviousFlag,
                MmsiNumber = vessel.MmsiNumber,
                ClassNotation = vessel.ClassNotation,
                ClassRegisterNumber = vessel.ClassRegisterNumber,
                ShipyardCountry = vessel.ShipyardCountry,
                ShipyardName = vessel.ShipyardName,
                YardNo = vessel.YardNo,
                CompanyImoNumber = vessel.CompanyImoNumber,
                SuezCanalIdNumber = vessel.SuezCanalIdNumber,
                KeelLaidDate = vessel.KeelLaidDate,
                YearBuilt = vessel.YearBuilt,
                DateOfRegistry = vessel.DateOfRegistry,
                OwnerImoNumber = vessel.OwnerImoNumber,
                PanamaCanalIdNumber = vessel.PanamaCanalIdNumber,
                MaxPersonsAllowedOB = vessel.MaxPersonsAllowedOB,
                ServiceSpeedKts = vessel.ServiceSpeedKts,
                VrpNumber = vessel.VrpNumber,
                VrpType = vessel.VrpType,
                NoOfCrewSafeManning = vessel.NoOfCrewSafeManning,
                MaxPassengersAllowedOB = vessel.MaxPassengersAllowedOB,

                // Dimensions
                Loa = vessel.Loa,
                Lbp = vessel.Lbp,
                BreadthMoulded = vessel.BreadthMoulded,
                DepthMoulded = vessel.DepthMoulded,
                DraftMoulded = vessel.DraftMoulded,
                DraftScantling = vessel.DraftScantling,
                DraftFullBallast = vessel.DraftFullBallast,
                HMaxAirdraft = vessel.HMaxAirdraft,
                LightShip = vessel.LightShip,
                BlockCoefficient = vessel.BlockCoefficient,
                TpcAtSummerDraft = vessel.TpcAtSummerDraft,
                GrossTonnageInternational = vessel.GrossTonnageInternational,
                GrossTonnageSuezCanal = vessel.GrossTonnageSuezCanal,
                GrossTonnagePanamaCanal = vessel.GrossTonnagePanamaCanal,
                NettTonnageInternational = vessel.NettTonnageInternational,

                // Machinery
                AnchorChainPort = vessel.AnchorChainPort,
                AnchorChainStarboard = vessel.AnchorChainStarboard,
                AnchorChainStern = vessel.AnchorChainStern,
                HarbourGeneratorMaker = vessel.HarbourGeneratorMaker,
                HarbourGeneratorMaxPowerKW = vessel.HarbourGeneratorMaxPowerKW,
                AzimuthEngFwdCount = vessel.AzimuthEngFwdCount,
                AzimuthEngFwdMaxPowerKW = vessel.AzimuthEngFwdMaxPowerKW,

                // Shipowner
                ShipownerName = vessel.ShipownerName,
                ShipownerStreet = vessel.ShipownerStreet,
                ShipownerCountry = vessel.ShipownerCountry,
                ShipownerZip = vessel.ShipownerZip,
                ShipownerCity = vessel.ShipownerCity,
                ShipownerPhone = vessel.ShipownerPhone,
                ShipownerFax = vessel.ShipownerFax,
                ShipownerEmail = vessel.ShipownerEmail,
                ShipownerContactPerson = vessel.ShipownerContactPerson,
                ManagingOwnerName = vessel.ManagingOwnerName,
                ManagingOwnerEmail = vessel.ManagingOwnerEmail,
                ManagingOwnerContactPerson = vessel.ManagingOwnerContactPerson,
                OperatorName = vessel.OperatorName,
                OperatorEmail = vessel.OperatorEmail,
                OperatorContactPerson = vessel.OperatorContactPerson,
                CsoFirstName = vessel.CsoFirstName,
                CsoLastName = vessel.CsoLastName,
                CsoEmail = vessel.CsoEmail,
                CsoPhone24h = vessel.CsoPhone24h,
                DpaFirstName = vessel.DpaFirstName,
                DpaLastName = vessel.DpaLastName,
                DpaEmail = vessel.DpaEmail,
                DpaPhone24h = vessel.DpaPhone24h,

                // Charterer
                ChartererName = vessel.ChartererName,
                ChartererStreet = vessel.ChartererStreet,
                ChartererCountry = vessel.ChartererCountry,
                ChartererZip = vessel.ChartererZip,
                ChartererCity = vessel.ChartererCity,
                ChartererPhone = vessel.ChartererPhone,
                ChartererEmail = vessel.ChartererEmail,
                ChartererContactPerson = vessel.ChartererContactPerson,
                BareboatChartererName = vessel.BareboatChartererName,
                BareboatChartererEmail = vessel.BareboatChartererEmail,
                BareboatChartererContactPerson = vessel.BareboatChartererContactPerson,

                // Class / Flag State
                ClassSocietyName = vessel.ClassSocietyName,
                ClassSocietyCountry = vessel.ClassSocietyCountry,
                ClassSocietyEmail = vessel.ClassSocietyEmail,
                ClassSocietyContactPerson = vessel.ClassSocietyContactPerson,
                FlagStateName = vessel.FlagStateName,
                FlagStateCountry = vessel.FlagStateCountry,
                FlagStateEmail = vessel.FlagStateEmail,
                FlagStateContactPerson = vessel.FlagStateContactPerson,

                // Insurance
                PiClubName = vessel.PiClubName,
                PiClubStreet = vessel.PiClubStreet,
                PiClubCountry = vessel.PiClubCountry,
                PiClubZip = vessel.PiClubZip,
                PiClubCity = vessel.PiClubCity,
                PiClubPhone = vessel.PiClubPhone,
                PiClubEmail = vessel.PiClubEmail,
                PiClubContactPerson = vessel.PiClubContactPerson,
                HmClubName = vessel.HmClubName,
                HmClubEmail = vessel.HmClubEmail,
                HmClubContactPerson = vessel.HmClubContactPerson,

                // Radio Communication
                InmarsatPhone1 = vessel.InmarsatPhone1,
                InmarsatPhone2 = vessel.InmarsatPhone2,
                InmarsatFax1 = vessel.InmarsatFax1,
                EmailAddress1 = vessel.EmailAddress1,
                EmailAddress2 = vessel.EmailAddress2,
                GsmPhone = vessel.GsmPhone,
                SeaAreaA1 = vessel.SeaAreaA1,
                SeaAreaA2 = vessel.SeaAreaA2,
                SeaAreaA3 = vessel.SeaAreaA3,
                SeaAreaA4 = vessel.SeaAreaA4,
                Ais = vessel.Ais,
                Navtex = vessel.Navtex,
                EpirbNumber = vessel.EpirbNumber,
                EpirbMaker = vessel.EpirbMaker,

                // Tanks & Cargo
                HfoCbm = vessel.HfoCbm,
                MdoCbm = vessel.MdoCbm,
                LubOilCbm = vessel.LubOilCbm,
                FreshWaterCbm = vessel.FreshWaterCbm,
                BallastWaterCbm = vessel.BallastWaterCbm,
                NoOfBallastTanks = vessel.NoOfBallastTanks,
                TeuTotal = vessel.TeuTotal,
                TeuOnDeck = vessel.TeuOnDeck,
                TeuUnderDeck = vessel.TeuUnderDeck,
                GrainCbm = vessel.GrainCbm,
                BalesCbm = vessel.BalesCbm,
                NoOfCargoHolds = vessel.NoOfCargoHolds,
                NoOfHatches = vessel.NoOfHatches,

                // Sync Metadata
                LastEdgeSyncAt = vessel.LastEdgeSyncAt,
                LastShoreSyncAt = vessel.LastShoreSyncAt
            };
        }
    }
}