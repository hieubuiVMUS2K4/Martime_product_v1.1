using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

public interface IShipDataService
{
    Task<ShipDataDto?> GetShipDataAsync();
    Task<ShipDataDto> SaveShipDataAsync(SaveShipDataDto dto);
}

public class ShipDataService : IShipDataService
{
    private readonly IShipDataRepository _repository;
    private readonly EdgeDbContext _context;
    private readonly ILogger<ShipDataService> _logger;

    public ShipDataService(
        IShipDataRepository repository,
        EdgeDbContext context,
        ILogger<ShipDataService> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    public async Task<ShipDataDto?> GetShipDataAsync()
    {
        var shipData = await _repository.GetWithChildrenAsync();
        if (shipData == null) return null;
        return MapToDto(shipData);
    }

    public async Task<ShipDataDto> SaveShipDataAsync(SaveShipDataDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _repository.GetAsync();
            ShipData shipData;

            if (existing == null)
            {
                // Create new
                shipData = new ShipData();
                MapFromDto(dto, shipData);
                shipData = await _repository.CreateAsync(shipData);

                // New record: insert all child collections
                await _repository.ReplaceMainEnginesAsync(shipData.Id, MapMainEngines(dto.MainEngines));
                await _repository.ReplaceAuxiliaryEnginesAsync(shipData.Id, MapAuxiliaryEngines(dto.AuxiliaryEngines));
                await _repository.ReplacePropellersAsync(shipData.Id, MapPropellers(dto.Propellers));
                await _repository.ReplaceBowthrusterAsync(shipData.Id, MapBowthrusters(dto.Bowthrusters));
                await _repository.ReplaceSternthrusterAsync(shipData.Id, MapSternthrusters(dto.Sternthrusters));
                await _repository.ReplaceRuddersAsync(shipData.Id, MapRudders(dto.Rudders));
                await _repository.ReplaceShaftGeneratorsAsync(shipData.Id, MapShaftGenerators(dto.ShaftGenerators));
                await _repository.ReplaceBoilersAsync(shipData.Id, MapBoilers(dto.Boilers));
                await _repository.ReplaceLoadLinesAsync(shipData.Id, MapLoadLines(dto.LoadLines));
                await _repository.ReplacePilotCardDataAsync(shipData.Id, MapPilotCardData(dto.PilotCardData));
            }
            else
            {
                // Update existing - EF change tracker will only flag actually modified columns
                shipData = existing;
                MapFromDto(dto, shipData);
                shipData = await _repository.UpdateAsync(shipData);

                // Only replace child collections that have actually changed
                // Load existing children for comparison (lightweight queries, no tracking needed)
                await ReplaceChildrenIfChangedAsync(shipData.Id, dto);
            }

            // 1 lần SaveChanges duy nhất cho toàn bộ parent + children inserts
            await _repository.SaveAllChangesAsync();

            await transaction.CommitAsync();

            // Map trực tiếp từ DTO đã lưu thay vì re-fetch từ DB
            return MapSavedToDto(shipData, dto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving ship data");
            throw;
        }
    }

    /// <summary>
    /// Compare existing child collections with incoming DTO data.
    /// Only replace (delete+re-insert) collections that have actually changed.
    /// This prevents unnecessary audit log entries and reduces DB writes.
    /// </summary>
    private async Task ReplaceChildrenIfChangedAsync(Guid shipDataId, SaveShipDataDto dto)
    {
        // Main Engines
        var existingME = await _context.ShipMainEngines
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.MeType, e.MeFuelGrade, e.MePowerKW, e.McrKW })
            .ToListAsync();
        var incomingME = dto.MainEngines.Select(d => new { d.MeType, d.MeFuelGrade, d.MePowerKW, d.McrKW }).ToList();
        if (!SequenceEqual(existingME, incomingME))
            await _repository.ReplaceMainEnginesAsync(shipDataId, MapMainEngines(dto.MainEngines));

        // Auxiliary Engines
        var existingAE = await _context.ShipAuxiliaryEngines
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.AeType, e.AeFuelGrade, e.AePowerKW })
            .ToListAsync();
        var incomingAE = dto.AuxiliaryEngines.Select(d => new { d.AeType, d.AeFuelGrade, d.AePowerKW }).ToList();
        if (!SequenceEqual(existingAE, incomingAE))
            await _repository.ReplaceAuxiliaryEnginesAsync(shipDataId, MapAuxiliaryEngines(dto.AuxiliaryEngines));

        // Propellers
        var existingProp = await _context.ShipPropellers
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.PropellerType, e.NumberOfBlades, e.Rotation, e.DiameterMm, e.PropellerPitchGeometricMm, e.PitchRatio })
            .ToListAsync();
        var incomingProp = dto.Propellers.Select(d => new { d.PropellerType, d.NumberOfBlades, d.Rotation, d.DiameterMm, d.PropellerPitchGeometricMm, d.PitchRatio }).ToList();
        if (!SequenceEqual(existingProp, incomingProp))
            await _repository.ReplacePropellersAsync(shipDataId, MapPropellers(dto.Propellers));

        // Bowthrusters
        var existingBT = await _context.ShipBowthrusters
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.PowerKW })
            .ToListAsync();
        var incomingBT = dto.Bowthrusters.Select(d => new { d.PowerKW }).ToList();
        if (!SequenceEqual(existingBT, incomingBT))
            await _repository.ReplaceBowthrusterAsync(shipDataId, MapBowthrusters(dto.Bowthrusters));

        // Sternthrusters
        var existingST = await _context.ShipSternthrusters
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.PowerKW })
            .ToListAsync();
        var incomingST = dto.Sternthrusters.Select(d => new { d.PowerKW }).ToList();
        if (!SequenceEqual(existingST, incomingST))
            await _repository.ReplaceSternthrusterAsync(shipDataId, MapSternthrusters(dto.Sternthrusters));

        // Rudders
        var existingRud = await _context.ShipRudders
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.RudderType })
            .ToListAsync();
        var incomingRud = dto.Rudders.Select(d => new { d.RudderType }).ToList();
        if (!SequenceEqual(existingRud, incomingRud))
            await _repository.ReplaceRuddersAsync(shipDataId, MapRudders(dto.Rudders));

        // Shaft Generators
        var existingSG = await _context.ShipShaftGenerators
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.MaxPowerKW })
            .ToListAsync();
        var incomingSG = dto.ShaftGenerators.Select(d => new { d.MaxPowerKW }).ToList();
        if (!SequenceEqual(existingSG, incomingSG))
            await _repository.ReplaceShaftGeneratorsAsync(shipDataId, MapShaftGenerators(dto.ShaftGenerators));

        // Boilers
        var existingBoil = await _context.ShipBoilers
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.BoilerType, e.Model })
            .ToListAsync();
        var incomingBoil = dto.Boilers.Select(d => new { d.BoilerType, d.Model }).ToList();
        if (!SequenceEqual(existingBoil, incomingBoil))
            await _repository.ReplaceBoilersAsync(shipDataId, MapBoilers(dto.Boilers));

        // Load Lines
        var existingLL = await _context.ShipLoadLines
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.LoadLineType, e.DraftM, e.FreeboardM, e.DisplacementMt, e.DeadweightMt })
            .ToListAsync();
        var incomingLL = dto.LoadLines.Select(d => new { d.LoadLineType, d.DraftM, d.FreeboardM, d.DisplacementMt, d.DeadweightMt }).ToList();
        if (!SequenceEqual(existingLL, incomingLL))
            await _repository.ReplaceLoadLinesAsync(shipDataId, MapLoadLines(dto.LoadLines));

        // Pilot Card Data
        var existingPC = await _context.ShipPilotCardData
            .Where(e => e.ShipDataId == shipDataId).OrderBy(e => e.SortOrder)
            .Select(e => new { e.EngineOrder, e.MainEngineRPM, e.SpeedLoadedKts, e.SpeedBallastKts })
            .ToListAsync();
        var incomingPC = dto.PilotCardData.Select(d => new { d.EngineOrder, d.MainEngineRPM, d.SpeedLoadedKts, d.SpeedBallastKts }).ToList();
        if (!SequenceEqual(existingPC, incomingPC))
            await _repository.ReplacePilotCardDataAsync(shipDataId, MapPilotCardData(dto.PilotCardData));
    }

    /// <summary>
    /// Compare two anonymous-type sequences using JSON serialization.
    /// Returns true if both sequences have the same count and identical data in order.
    /// </summary>
    private static bool SequenceEqual<T>(List<T> existing, List<T> incoming)
    {
        if (existing.Count != incoming.Count) return false;
        if (existing.Count == 0) return true;

        for (int i = 0; i < existing.Count; i++)
        {
            if (!existing[i]!.Equals(incoming[i])) return false;
        }
        return true;
    }

    // ═══════════════════════════════════════════
    // MAPPING: Entity → DTO
    // ═══════════════════════════════════════════

    /// <summary>
    /// Map từ ShipData đã lưu + DTO input → response DTO (tránh re-fetch từ DB)
    /// </summary>
    private ShipDataDto MapSavedToDto(ShipData s, SaveShipDataDto dto) => new()
    {
        Id = s.Id,
        // Basic Data
        ImoNumber = s.ImoNumber,
        OfficialNumber = s.OfficialNumber,
        CallSign = s.CallSign,
        ShipName = s.ShipName,
        Flag = s.Flag,
        PortOfRegistry = s.PortOfRegistry,
        PreviousName = s.PreviousName,
        PreviousFlag = s.PreviousFlag,
        MmsiNumber = s.MmsiNumber,
        TypeOfVessel = s.TypeOfVessel,
        ClassNotation = s.ClassNotation,
        ClassRegisterNumber = s.ClassRegisterNumber,
        ShipyardCountry = s.ShipyardCountry,
        ShipyardName = s.ShipyardName,
        YardNo = s.YardNo,
        CompanyImoNumber = s.CompanyImoNumber,
        SuezCanalIdNumber = s.SuezCanalIdNumber,
        KeelLaidDate = s.KeelLaidDate,
        YearBuilt = s.YearBuilt,
        DateOfRegistry = s.DateOfRegistry,
        OwnerImoNumber = s.OwnerImoNumber,
        PanamaCanalIdNumber = s.PanamaCanalIdNumber,
        MaxPersonsAllowedOB = s.MaxPersonsAllowedOB,
        ServiceSpeedKts = s.ServiceSpeedKts,
        VrpNumber = s.VrpNumber,
        VrpType = s.VrpType,
        NoOfCrewSafeManning = s.NoOfCrewSafeManning,
        MaxPassengersAllowedOB = s.MaxPassengersAllowedOB,
        // Dimensions (copy from entity which was mapped from dto)
        Loa = s.Loa, DepthMoulded = s.DepthMoulded, HMaxAirdraft = s.HMaxAirdraft,
        ParallelBodyBallast = s.ParallelBodyBallast, ParallelBodyLoaded = s.ParallelBodyLoaded,
        Lbp = s.Lbp, DraftMoulded = s.DraftMoulded, DDistance = s.DDistance,
        BridgeToAft = s.BridgeToAft, BridgeToBow = s.BridgeToBow,
        BowToBulbousBow = s.BowToBulbousBow,
        BreadthMoulded = s.BreadthMoulded, DraftScantling = s.DraftScantling,
        AirdraftReductionMastFouled = s.AirdraftReductionMastFouled,
        LightShip = s.LightShip, DraftFullBallast = s.DraftFullBallast,
        BlockCoefficientNA = s.BlockCoefficientNA, BlockCoefficient = s.BlockCoefficient,
        TpcAtSummerDraft = s.TpcAtSummerDraft, FreshWaterAllowanceFwa = s.FreshWaterAllowanceFwa,
        GrossTonnageInternational = s.GrossTonnageInternational,
        GrossTonnageSuezCanal = s.GrossTonnageSuezCanal,
        GrossTonnagePanamaCanal = s.GrossTonnagePanamaCanal,
        NettTonnageInternational = s.NettTonnageInternational,
        NettTonnageSuezCanal = s.NettTonnageSuezCanal,
        NettTonnagePanamaCanal = s.NettTonnagePanamaCanal,
        ManifoldToWaterlineBallast = s.ManifoldToWaterlineBallast,
        ManifoldToWaterlineLoaded = s.ManifoldToWaterlineLoaded,
        DeckToManifold = s.DeckToManifold, SternToManifold = s.SternToManifold,
        ShipsideToManifold = s.ShipsideToManifold, BowToManifold = s.BowToManifold,
        ManifoldToKeel = s.ManifoldToKeel, ManifoldToBridge = s.ManifoldToBridge,
        MaxLoadingRateShip = s.MaxLoadingRateShip, NumberOfLines = s.NumberOfLines,
        MaxAllowablePressurePsi = s.MaxAllowablePressurePsi, VentingSystemShip = s.VentingSystemShip,
        // Machinery
        AnchorChainPort = s.AnchorChainPort, AnchorChainStarboard = s.AnchorChainStarboard,
        AnchorChainStern = s.AnchorChainStern, AnchorChainSternNA = s.AnchorChainSternNA,
        BowthrusterNA = s.BowthrusterNA, SternthrusterNA = s.SternthrusterNA,
        ShaftGeneratorNA = s.ShaftGeneratorNA,
        HarbourGeneratorMaker = s.HarbourGeneratorMaker,
        HarbourGeneratorMaxPowerKW = s.HarbourGeneratorMaxPowerKW,
        AzimuthEngFwdCount = s.AzimuthEngFwdCount, AzimuthEngFwdMaxPowerKW = s.AzimuthEngFwdMaxPowerKW,
        AzimuthEngAftCount = s.AzimuthEngAftCount, AzimuthEngAftMaxPowerKW = s.AzimuthEngAftMaxPowerKW,
        // Shipowner
        ShipownerName = s.ShipownerName, ShipownerStreet = s.ShipownerStreet,
        ShipownerCountry = s.ShipownerCountry, ShipownerZip = s.ShipownerZip,
        ShipownerCity = s.ShipownerCity, ShipownerPhone = s.ShipownerPhone,
        ShipownerFax = s.ShipownerFax, ShipownerTlx = s.ShipownerTlx,
        ShipownerEmail = s.ShipownerEmail, ShipownerContactPerson = s.ShipownerContactPerson,
        ManagingOwnerName = s.ManagingOwnerName, ManagingOwnerStreet = s.ManagingOwnerStreet,
        ManagingOwnerCountry = s.ManagingOwnerCountry, ManagingOwnerZip = s.ManagingOwnerZip,
        ManagingOwnerCity = s.ManagingOwnerCity, ManagingOwnerPhone = s.ManagingOwnerPhone,
        ManagingOwnerFax = s.ManagingOwnerFax, ManagingOwnerTlx = s.ManagingOwnerTlx,
        ManagingOwnerEmail = s.ManagingOwnerEmail, ManagingOwnerContactPerson = s.ManagingOwnerContactPerson,
        OperatorName = s.OperatorName, OperatorStreet = s.OperatorStreet,
        OperatorCountry = s.OperatorCountry, OperatorZip = s.OperatorZip,
        OperatorCity = s.OperatorCity, OperatorPhone = s.OperatorPhone,
        OperatorFax = s.OperatorFax, OperatorTlx = s.OperatorTlx,
        OperatorEmail = s.OperatorEmail, OperatorContactPerson = s.OperatorContactPerson,
        CsoTitle = s.CsoTitle, CsoFirstName = s.CsoFirstName, CsoLastName = s.CsoLastName,
        CsoStreet = s.CsoStreet, CsoCountry = s.CsoCountry, CsoZip = s.CsoZip, CsoCity = s.CsoCity,
        CsoPhone24h = s.CsoPhone24h, CsoFax = s.CsoFax, CsoTlx = s.CsoTlx, CsoEmail = s.CsoEmail,
        DpaTitle = s.DpaTitle, DpaFirstName = s.DpaFirstName, DpaLastName = s.DpaLastName,
        DpaStreet = s.DpaStreet, DpaCountry = s.DpaCountry, DpaZip = s.DpaZip, DpaCity = s.DpaCity,
        DpaPhone24h = s.DpaPhone24h, DpaFax = s.DpaFax, DpaTlx = s.DpaTlx, DpaEmail = s.DpaEmail,
        QiUsaTitle = s.QiUsaTitle, QiUsaFirstName = s.QiUsaFirstName, QiUsaLastName = s.QiUsaLastName,
        QiUsaStreet = s.QiUsaStreet, QiUsaCountry = s.QiUsaCountry, QiUsaZip = s.QiUsaZip, QiUsaCity = s.QiUsaCity,
        QiUsaPhone24h = s.QiUsaPhone24h, QiUsaFax = s.QiUsaFax, QiUsaTlx = s.QiUsaTlx, QiUsaEmail = s.QiUsaEmail,
        QiPanamaTitle = s.QiPanamaTitle, QiPanamaFirstName = s.QiPanamaFirstName, QiPanamaLastName = s.QiPanamaLastName,
        QiPanamaStreet = s.QiPanamaStreet, QiPanamaCountry = s.QiPanamaCountry, QiPanamaZip = s.QiPanamaZip, QiPanamaCity = s.QiPanamaCity,
        QiPanamaPhone24h = s.QiPanamaPhone24h, QiPanamaFax = s.QiPanamaFax, QiPanamaTlx = s.QiPanamaTlx, QiPanamaEmail = s.QiPanamaEmail,
        // Charterer
        ChartererName = s.ChartererName, ChartererStreet = s.ChartererStreet,
        ChartererCountry = s.ChartererCountry, ChartererZip = s.ChartererZip, ChartererCity = s.ChartererCity,
        ChartererPhone = s.ChartererPhone, ChartererFax = s.ChartererFax, ChartererTlx = s.ChartererTlx,
        ChartererEmail = s.ChartererEmail, ChartererContactPerson = s.ChartererContactPerson,
        BareboatChartererName = s.BareboatChartererName, BareboatChartererStreet = s.BareboatChartererStreet,
        BareboatChartererCountry = s.BareboatChartererCountry, BareboatChartererZip = s.BareboatChartererZip,
        BareboatChartererCity = s.BareboatChartererCity, BareboatChartererPhone = s.BareboatChartererPhone,
        BareboatChartererFax = s.BareboatChartererFax, BareboatChartererTlx = s.BareboatChartererTlx,
        BareboatChartererEmail = s.BareboatChartererEmail, BareboatChartererContactPerson = s.BareboatChartererContactPerson,
        // Class / Flag State
        ClassSocietyName = s.ClassSocietyName, ClassSocietyStreet = s.ClassSocietyStreet,
        ClassSocietyCountry = s.ClassSocietyCountry, ClassSocietyZip = s.ClassSocietyZip,
        ClassSocietyCity = s.ClassSocietyCity, ClassSocietyPhone = s.ClassSocietyPhone,
        ClassSocietyFax = s.ClassSocietyFax, ClassSocietyTlx = s.ClassSocietyTlx,
        ClassSocietyEmail = s.ClassSocietyEmail, ClassSocietyContactPerson = s.ClassSocietyContactPerson,
        FlagStateName = s.FlagStateName, FlagStateStreet = s.FlagStateStreet,
        FlagStateCountry = s.FlagStateCountry, FlagStateZip = s.FlagStateZip,
        FlagStateCity = s.FlagStateCity, FlagStatePhone = s.FlagStatePhone,
        FlagStateFax = s.FlagStateFax, FlagStateTlx = s.FlagStateTlx,
        FlagStateEmail = s.FlagStateEmail, FlagStateContactPerson = s.FlagStateContactPerson,
        // Insurance
        PiClubName = s.PiClubName, PiClubStreet = s.PiClubStreet,
        PiClubCountry = s.PiClubCountry, PiClubZip = s.PiClubZip, PiClubCity = s.PiClubCity,
        PiClubPhone = s.PiClubPhone, PiClubFax = s.PiClubFax, PiClubTlx = s.PiClubTlx,
        PiClubEmail = s.PiClubEmail, PiClubContactPerson = s.PiClubContactPerson,
        HmClubName = s.HmClubName, HmClubStreet = s.HmClubStreet,
        HmClubCountry = s.HmClubCountry, HmClubZip = s.HmClubZip, HmClubCity = s.HmClubCity,
        HmClubPhone = s.HmClubPhone, HmClubFax = s.HmClubFax, HmClubTlx = s.HmClubTlx,
        HmClubEmail = s.HmClubEmail, HmClubContactPerson = s.HmClubContactPerson,
        // Radio Communication
        InmarsatTelex1 = s.InmarsatTelex1, InmarsatTelex2 = s.InmarsatTelex2,
        InmarsatPhone1 = s.InmarsatPhone1, InmarsatPhone2 = s.InmarsatPhone2,
        InmarsatFax1 = s.InmarsatFax1, InmarsatFax2 = s.InmarsatFax2,
        EmailAddress1 = s.EmailAddress1, EmailAddress2 = s.EmailAddress2,
        GsmPhone = s.GsmPhone,
        SeaAreaA1 = s.SeaAreaA1, SeaAreaA2 = s.SeaAreaA2, SeaAreaA3 = s.SeaAreaA3, SeaAreaA4 = s.SeaAreaA4,
        DscHF = s.DscHF, DscMF = s.DscMF, DscVHF = s.DscVHF,
        RadiotelephoneHF = s.RadiotelephoneHF, RadiotelephoneMF = s.RadiotelephoneMF, RadiotelephoneVHF = s.RadiotelephoneVHF,
        RadiotelegraphHF = s.RadiotelegraphHF, RadiotelegraphMF = s.RadiotelegraphMF, RadiotelegraphVHF = s.RadiotelegraphVHF,
        Navtex = s.Navtex, Ais = s.Ais, SartTransponder = s.SartTransponder, Radiotelex = s.Radiotelex,
        OtherRadioEquipment = s.OtherRadioEquipment,
        EpirbNumber = s.EpirbNumber, EpirbOperatingSystem = s.EpirbOperatingSystem,
        EpirbMaker = s.EpirbMaker, EpirbModel = s.EpirbModel, EpirbFrequency = s.EpirbFrequency,
        // Tanks & Cargo
        HfoCbm = s.HfoCbm, MdoCbm = s.MdoCbm, LubOilCbm = s.LubOilCbm,
        SludgeCbm = s.SludgeCbm, BilgeWaterCbm = s.BilgeWaterCbm, SewageCbm = s.SewageCbm,
        FreshWaterCbm = s.FreshWaterCbm, BallastWaterCbm = s.BallastWaterCbm,
        NoOfBallastTanks = s.NoOfBallastTanks,
        TeuTotal = s.TeuTotal, TeuOnDeck = s.TeuOnDeck, TeuUnderDeck = s.TeuUnderDeck,
        GrainCbm = s.GrainCbm, BalesCbm = s.BalesCbm,
        NoOfCargoHolds = s.NoOfCargoHolds, NoOfHatches = s.NoOfHatches,
        // Children - map trực tiếp từ input DTO (đã được lưu thành công)
        MainEngines = dto.MainEngines.Select((d, i) => new ShipMainEngineDto { MeType = d.MeType, MeFuelGrade = d.MeFuelGrade, MePowerKW = d.MePowerKW, McrKW = d.McrKW, SortOrder = i }).ToList(),
        AuxiliaryEngines = dto.AuxiliaryEngines.Select((d, i) => new ShipAuxiliaryEngineDto { AeType = d.AeType, AeFuelGrade = d.AeFuelGrade, AePowerKW = d.AePowerKW, SortOrder = i }).ToList(),
        Propellers = dto.Propellers.Select((d, i) => new ShipPropellerDto { PropellerType = d.PropellerType, NumberOfBlades = d.NumberOfBlades, Rotation = d.Rotation, DiameterMm = d.DiameterMm, PropellerPitchGeometricMm = d.PropellerPitchGeometricMm, PitchRatio = d.PitchRatio, SortOrder = i }).ToList(),
        Bowthrusters = dto.Bowthrusters.Select((d, i) => new ShipBowthrusterDto { PowerKW = d.PowerKW, SortOrder = i }).ToList(),
        Sternthrusters = dto.Sternthrusters.Select((d, i) => new ShipSternthrusterDto { PowerKW = d.PowerKW, SortOrder = i }).ToList(),
        Rudders = dto.Rudders.Select((d, i) => new ShipRudderDto { RudderType = d.RudderType, SortOrder = i }).ToList(),
        ShaftGenerators = dto.ShaftGenerators.Select((d, i) => new ShipShaftGeneratorDto { MaxPowerKW = d.MaxPowerKW, SortOrder = i }).ToList(),
        Boilers = dto.Boilers.Select((d, i) => new ShipBoilerDto { BoilerType = d.BoilerType, Model = d.Model, SortOrder = i }).ToList(),
        LoadLines = dto.LoadLines.Select((d, i) => new ShipLoadLineDto { LoadLineType = d.LoadLineType, DraftM = d.DraftM, FreeboardM = d.FreeboardM, DisplacementMt = d.DisplacementMt, DeadweightMt = d.DeadweightMt, SortOrder = i }).ToList(),
        PilotCardData = dto.PilotCardData.Select((d, i) => new ShipPilotCardDataDto { EngineOrder = d.EngineOrder, MainEngineRPM = d.MainEngineRPM, SpeedLoadedKts = d.SpeedLoadedKts, SpeedBallastKts = d.SpeedBallastKts, SortOrder = i }).ToList(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
    };

    private ShipDataDto MapToDto(ShipData s) => new()
    {
        Id = s.Id,
        // Basic Data
        ImoNumber = s.ImoNumber,
        OfficialNumber = s.OfficialNumber,
        CallSign = s.CallSign,
        ShipName = s.ShipName,
        Flag = s.Flag,
        PortOfRegistry = s.PortOfRegistry,
        PreviousName = s.PreviousName,
        PreviousFlag = s.PreviousFlag,
        MmsiNumber = s.MmsiNumber,
        TypeOfVessel = s.TypeOfVessel,
        ClassNotation = s.ClassNotation,
        ClassRegisterNumber = s.ClassRegisterNumber,
        ShipyardCountry = s.ShipyardCountry,
        ShipyardName = s.ShipyardName,
        YardNo = s.YardNo,
        CompanyImoNumber = s.CompanyImoNumber,
        SuezCanalIdNumber = s.SuezCanalIdNumber,
        KeelLaidDate = s.KeelLaidDate,
        YearBuilt = s.YearBuilt,
        DateOfRegistry = s.DateOfRegistry,
        OwnerImoNumber = s.OwnerImoNumber,
        PanamaCanalIdNumber = s.PanamaCanalIdNumber,
        MaxPersonsAllowedOB = s.MaxPersonsAllowedOB,
        ServiceSpeedKts = s.ServiceSpeedKts,
        VrpNumber = s.VrpNumber,
        VrpType = s.VrpType,
        NoOfCrewSafeManning = s.NoOfCrewSafeManning,
        MaxPassengersAllowedOB = s.MaxPassengersAllowedOB,
        // Dimensions
        Loa = s.Loa,
        DepthMoulded = s.DepthMoulded,
        HMaxAirdraft = s.HMaxAirdraft,
        ParallelBodyBallast = s.ParallelBodyBallast,
        ParallelBodyLoaded = s.ParallelBodyLoaded,
        Lbp = s.Lbp,
        DraftMoulded = s.DraftMoulded,
        DDistance = s.DDistance,
        BridgeToAft = s.BridgeToAft,
        BridgeToBow = s.BridgeToBow,
        BowToBulbousBow = s.BowToBulbousBow,
        BreadthMoulded = s.BreadthMoulded,
        DraftScantling = s.DraftScantling,
        AirdraftReductionMastFouled = s.AirdraftReductionMastFouled,
        LightShip = s.LightShip,
        DraftFullBallast = s.DraftFullBallast,
        BlockCoefficientNA = s.BlockCoefficientNA,
        BlockCoefficient = s.BlockCoefficient,
        TpcAtSummerDraft = s.TpcAtSummerDraft,
        FreshWaterAllowanceFwa = s.FreshWaterAllowanceFwa,
        GrossTonnageInternational = s.GrossTonnageInternational,
        GrossTonnageSuezCanal = s.GrossTonnageSuezCanal,
        GrossTonnagePanamaCanal = s.GrossTonnagePanamaCanal,
        NettTonnageInternational = s.NettTonnageInternational,
        NettTonnageSuezCanal = s.NettTonnageSuezCanal,
        NettTonnagePanamaCanal = s.NettTonnagePanamaCanal,
        ManifoldToWaterlineBallast = s.ManifoldToWaterlineBallast,
        ManifoldToWaterlineLoaded = s.ManifoldToWaterlineLoaded,
        DeckToManifold = s.DeckToManifold,
        SternToManifold = s.SternToManifold,
        ShipsideToManifold = s.ShipsideToManifold,
        BowToManifold = s.BowToManifold,
        ManifoldToKeel = s.ManifoldToKeel,
        ManifoldToBridge = s.ManifoldToBridge,
        MaxLoadingRateShip = s.MaxLoadingRateShip,
        NumberOfLines = s.NumberOfLines,
        MaxAllowablePressurePsi = s.MaxAllowablePressurePsi,
        VentingSystemShip = s.VentingSystemShip,
        // Machinery
        AnchorChainPort = s.AnchorChainPort,
        AnchorChainStarboard = s.AnchorChainStarboard,
        AnchorChainStern = s.AnchorChainStern,
        AnchorChainSternNA = s.AnchorChainSternNA,
        BowthrusterNA = s.BowthrusterNA,
        SternthrusterNA = s.SternthrusterNA,
        ShaftGeneratorNA = s.ShaftGeneratorNA,
        HarbourGeneratorMaker = s.HarbourGeneratorMaker,
        HarbourGeneratorMaxPowerKW = s.HarbourGeneratorMaxPowerKW,
        AzimuthEngFwdCount = s.AzimuthEngFwdCount,
        AzimuthEngFwdMaxPowerKW = s.AzimuthEngFwdMaxPowerKW,
        AzimuthEngAftCount = s.AzimuthEngAftCount,
        AzimuthEngAftMaxPowerKW = s.AzimuthEngAftMaxPowerKW,
        // Shipowner
        ShipownerName = s.ShipownerName,
        ShipownerStreet = s.ShipownerStreet,
        ShipownerCountry = s.ShipownerCountry,
        ShipownerZip = s.ShipownerZip,
        ShipownerCity = s.ShipownerCity,
        ShipownerPhone = s.ShipownerPhone,
        ShipownerFax = s.ShipownerFax,
        ShipownerTlx = s.ShipownerTlx,
        ShipownerEmail = s.ShipownerEmail,
        ShipownerContactPerson = s.ShipownerContactPerson,
        ManagingOwnerName = s.ManagingOwnerName,
        ManagingOwnerStreet = s.ManagingOwnerStreet,
        ManagingOwnerCountry = s.ManagingOwnerCountry,
        ManagingOwnerZip = s.ManagingOwnerZip,
        ManagingOwnerCity = s.ManagingOwnerCity,
        ManagingOwnerPhone = s.ManagingOwnerPhone,
        ManagingOwnerFax = s.ManagingOwnerFax,
        ManagingOwnerTlx = s.ManagingOwnerTlx,
        ManagingOwnerEmail = s.ManagingOwnerEmail,
        ManagingOwnerContactPerson = s.ManagingOwnerContactPerson,
        OperatorName = s.OperatorName,
        OperatorStreet = s.OperatorStreet,
        OperatorCountry = s.OperatorCountry,
        OperatorZip = s.OperatorZip,
        OperatorCity = s.OperatorCity,
        OperatorPhone = s.OperatorPhone,
        OperatorFax = s.OperatorFax,
        OperatorTlx = s.OperatorTlx,
        OperatorEmail = s.OperatorEmail,
        OperatorContactPerson = s.OperatorContactPerson,
        CsoTitle = s.CsoTitle, CsoFirstName = s.CsoFirstName, CsoLastName = s.CsoLastName,
        CsoStreet = s.CsoStreet, CsoCountry = s.CsoCountry, CsoZip = s.CsoZip, CsoCity = s.CsoCity,
        CsoPhone24h = s.CsoPhone24h, CsoFax = s.CsoFax, CsoTlx = s.CsoTlx, CsoEmail = s.CsoEmail,
        DpaTitle = s.DpaTitle, DpaFirstName = s.DpaFirstName, DpaLastName = s.DpaLastName,
        DpaStreet = s.DpaStreet, DpaCountry = s.DpaCountry, DpaZip = s.DpaZip, DpaCity = s.DpaCity,
        DpaPhone24h = s.DpaPhone24h, DpaFax = s.DpaFax, DpaTlx = s.DpaTlx, DpaEmail = s.DpaEmail,
        QiUsaTitle = s.QiUsaTitle, QiUsaFirstName = s.QiUsaFirstName, QiUsaLastName = s.QiUsaLastName,
        QiUsaStreet = s.QiUsaStreet, QiUsaCountry = s.QiUsaCountry, QiUsaZip = s.QiUsaZip, QiUsaCity = s.QiUsaCity,
        QiUsaPhone24h = s.QiUsaPhone24h, QiUsaFax = s.QiUsaFax, QiUsaTlx = s.QiUsaTlx, QiUsaEmail = s.QiUsaEmail,
        QiPanamaTitle = s.QiPanamaTitle, QiPanamaFirstName = s.QiPanamaFirstName, QiPanamaLastName = s.QiPanamaLastName,
        QiPanamaStreet = s.QiPanamaStreet, QiPanamaCountry = s.QiPanamaCountry, QiPanamaZip = s.QiPanamaZip, QiPanamaCity = s.QiPanamaCity,
        QiPanamaPhone24h = s.QiPanamaPhone24h, QiPanamaFax = s.QiPanamaFax, QiPanamaTlx = s.QiPanamaTlx, QiPanamaEmail = s.QiPanamaEmail,
        // Charterer
        ChartererName = s.ChartererName, ChartererStreet = s.ChartererStreet,
        ChartererCountry = s.ChartererCountry, ChartererZip = s.ChartererZip, ChartererCity = s.ChartererCity,
        ChartererPhone = s.ChartererPhone, ChartererFax = s.ChartererFax, ChartererTlx = s.ChartererTlx,
        ChartererEmail = s.ChartererEmail, ChartererContactPerson = s.ChartererContactPerson,
        BareboatChartererName = s.BareboatChartererName, BareboatChartererStreet = s.BareboatChartererStreet,
        BareboatChartererCountry = s.BareboatChartererCountry, BareboatChartererZip = s.BareboatChartererZip,
        BareboatChartererCity = s.BareboatChartererCity, BareboatChartererPhone = s.BareboatChartererPhone,
        BareboatChartererFax = s.BareboatChartererFax, BareboatChartererTlx = s.BareboatChartererTlx,
        BareboatChartererEmail = s.BareboatChartererEmail, BareboatChartererContactPerson = s.BareboatChartererContactPerson,
        // Class / Flag State
        ClassSocietyName = s.ClassSocietyName, ClassSocietyStreet = s.ClassSocietyStreet,
        ClassSocietyCountry = s.ClassSocietyCountry, ClassSocietyZip = s.ClassSocietyZip,
        ClassSocietyCity = s.ClassSocietyCity, ClassSocietyPhone = s.ClassSocietyPhone,
        ClassSocietyFax = s.ClassSocietyFax, ClassSocietyTlx = s.ClassSocietyTlx,
        ClassSocietyEmail = s.ClassSocietyEmail, ClassSocietyContactPerson = s.ClassSocietyContactPerson,
        FlagStateName = s.FlagStateName, FlagStateStreet = s.FlagStateStreet,
        FlagStateCountry = s.FlagStateCountry, FlagStateZip = s.FlagStateZip,
        FlagStateCity = s.FlagStateCity, FlagStatePhone = s.FlagStatePhone,
        FlagStateFax = s.FlagStateFax, FlagStateTlx = s.FlagStateTlx,
        FlagStateEmail = s.FlagStateEmail, FlagStateContactPerson = s.FlagStateContactPerson,
        // Insurance
        PiClubName = s.PiClubName, PiClubStreet = s.PiClubStreet,
        PiClubCountry = s.PiClubCountry, PiClubZip = s.PiClubZip, PiClubCity = s.PiClubCity,
        PiClubPhone = s.PiClubPhone, PiClubFax = s.PiClubFax, PiClubTlx = s.PiClubTlx,
        PiClubEmail = s.PiClubEmail, PiClubContactPerson = s.PiClubContactPerson,
        HmClubName = s.HmClubName, HmClubStreet = s.HmClubStreet,
        HmClubCountry = s.HmClubCountry, HmClubZip = s.HmClubZip, HmClubCity = s.HmClubCity,
        HmClubPhone = s.HmClubPhone, HmClubFax = s.HmClubFax, HmClubTlx = s.HmClubTlx,
        HmClubEmail = s.HmClubEmail, HmClubContactPerson = s.HmClubContactPerson,
        // Radio Communication
        InmarsatTelex1 = s.InmarsatTelex1, InmarsatTelex2 = s.InmarsatTelex2,
        InmarsatPhone1 = s.InmarsatPhone1, InmarsatPhone2 = s.InmarsatPhone2,
        InmarsatFax1 = s.InmarsatFax1, InmarsatFax2 = s.InmarsatFax2,
        EmailAddress1 = s.EmailAddress1, EmailAddress2 = s.EmailAddress2,
        GsmPhone = s.GsmPhone,
        SeaAreaA1 = s.SeaAreaA1, SeaAreaA2 = s.SeaAreaA2, SeaAreaA3 = s.SeaAreaA3, SeaAreaA4 = s.SeaAreaA4,
        DscHF = s.DscHF, DscMF = s.DscMF, DscVHF = s.DscVHF,
        RadiotelephoneHF = s.RadiotelephoneHF, RadiotelephoneMF = s.RadiotelephoneMF, RadiotelephoneVHF = s.RadiotelephoneVHF,
        RadiotelegraphHF = s.RadiotelegraphHF, RadiotelegraphMF = s.RadiotelegraphMF, RadiotelegraphVHF = s.RadiotelegraphVHF,
        Navtex = s.Navtex, Ais = s.Ais, SartTransponder = s.SartTransponder, Radiotelex = s.Radiotelex,
        OtherRadioEquipment = s.OtherRadioEquipment,
        EpirbNumber = s.EpirbNumber, EpirbOperatingSystem = s.EpirbOperatingSystem,
        EpirbMaker = s.EpirbMaker, EpirbModel = s.EpirbModel, EpirbFrequency = s.EpirbFrequency,
        // Tanks & Cargo
        HfoCbm = s.HfoCbm, MdoCbm = s.MdoCbm, LubOilCbm = s.LubOilCbm,
        SludgeCbm = s.SludgeCbm, BilgeWaterCbm = s.BilgeWaterCbm, SewageCbm = s.SewageCbm,
        FreshWaterCbm = s.FreshWaterCbm, BallastWaterCbm = s.BallastWaterCbm,
        NoOfBallastTanks = s.NoOfBallastTanks,
        TeuTotal = s.TeuTotal, TeuOnDeck = s.TeuOnDeck, TeuUnderDeck = s.TeuUnderDeck,
        GrainCbm = s.GrainCbm, BalesCbm = s.BalesCbm,
        NoOfCargoHolds = s.NoOfCargoHolds, NoOfHatches = s.NoOfHatches,
        // Children
        MainEngines = s.MainEngines.Select(e => new ShipMainEngineDto { Id = e.Id, MeType = e.MeType, MeFuelGrade = e.MeFuelGrade, MePowerKW = e.MePowerKW, McrKW = e.McrKW, SortOrder = e.SortOrder }).ToList(),
        AuxiliaryEngines = s.AuxiliaryEngines.Select(e => new ShipAuxiliaryEngineDto { Id = e.Id, AeType = e.AeType, AeFuelGrade = e.AeFuelGrade, AePowerKW = e.AePowerKW, SortOrder = e.SortOrder }).ToList(),
        Propellers = s.Propellers.Select(e => new ShipPropellerDto { Id = e.Id, PropellerType = e.PropellerType, NumberOfBlades = e.NumberOfBlades, Rotation = e.Rotation, DiameterMm = e.DiameterMm, PropellerPitchGeometricMm = e.PropellerPitchGeometricMm, PitchRatio = e.PitchRatio, SortOrder = e.SortOrder }).ToList(),
        Bowthrusters = s.Bowthrusters.Select(e => new ShipBowthrusterDto { Id = e.Id, PowerKW = e.PowerKW, SortOrder = e.SortOrder }).ToList(),
        Sternthrusters = s.Sternthrusters.Select(e => new ShipSternthrusterDto { Id = e.Id, PowerKW = e.PowerKW, SortOrder = e.SortOrder }).ToList(),
        Rudders = s.Rudders.Select(e => new ShipRudderDto { Id = e.Id, RudderType = e.RudderType, SortOrder = e.SortOrder }).ToList(),
        ShaftGenerators = s.ShaftGenerators.Select(e => new ShipShaftGeneratorDto { Id = e.Id, MaxPowerKW = e.MaxPowerKW, SortOrder = e.SortOrder }).ToList(),
        Boilers = s.Boilers.Select(e => new ShipBoilerDto { Id = e.Id, BoilerType = e.BoilerType, Model = e.Model, SortOrder = e.SortOrder }).ToList(),
        LoadLines = s.LoadLines.Select(e => new ShipLoadLineDto { Id = e.Id, LoadLineType = e.LoadLineType, DraftM = e.DraftM, FreeboardM = e.FreeboardM, DisplacementMt = e.DisplacementMt, DeadweightMt = e.DeadweightMt, SortOrder = e.SortOrder }).ToList(),
        PilotCardData = s.PilotCardData.Select(e => new ShipPilotCardDataDto { Id = e.Id, EngineOrder = e.EngineOrder, MainEngineRPM = e.MainEngineRPM, SpeedLoadedKts = e.SpeedLoadedKts, SpeedBallastKts = e.SpeedBallastKts, SortOrder = e.SortOrder }).ToList(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
    };

    // ═══════════════════════════════════════════
    // MAPPING: DTO → Entity
    // ═══════════════════════════════════════════

    private void MapFromDto(SaveShipDataDto dto, ShipData s)
    {
        // Basic Data
        s.ImoNumber = dto.ImoNumber;
        s.OfficialNumber = dto.OfficialNumber;
        s.CallSign = dto.CallSign;
        s.ShipName = dto.ShipName;
        s.Flag = dto.Flag;
        s.PortOfRegistry = dto.PortOfRegistry;
        s.PreviousName = dto.PreviousName;
        s.PreviousFlag = dto.PreviousFlag;
        s.MmsiNumber = dto.MmsiNumber;
        s.TypeOfVessel = dto.TypeOfVessel;
        s.ClassNotation = dto.ClassNotation;
        s.ClassRegisterNumber = dto.ClassRegisterNumber;
        s.ShipyardCountry = dto.ShipyardCountry;
        s.ShipyardName = dto.ShipyardName;
        s.YardNo = dto.YardNo;
        s.CompanyImoNumber = dto.CompanyImoNumber;
        s.SuezCanalIdNumber = dto.SuezCanalIdNumber;
        s.KeelLaidDate = dto.KeelLaidDate;
        s.YearBuilt = dto.YearBuilt;
        s.DateOfRegistry = dto.DateOfRegistry;
        s.OwnerImoNumber = dto.OwnerImoNumber;
        s.PanamaCanalIdNumber = dto.PanamaCanalIdNumber;
        s.MaxPersonsAllowedOB = dto.MaxPersonsAllowedOB;
        s.ServiceSpeedKts = dto.ServiceSpeedKts;
        s.VrpNumber = dto.VrpNumber;
        s.VrpType = dto.VrpType;
        s.NoOfCrewSafeManning = dto.NoOfCrewSafeManning;
        s.MaxPassengersAllowedOB = dto.MaxPassengersAllowedOB;
        // Dimensions
        s.Loa = dto.Loa; s.DepthMoulded = dto.DepthMoulded;
        s.HMaxAirdraft = dto.HMaxAirdraft;
        s.ParallelBodyBallast = dto.ParallelBodyBallast; s.ParallelBodyLoaded = dto.ParallelBodyLoaded;
        s.Lbp = dto.Lbp; s.DraftMoulded = dto.DraftMoulded; s.DDistance = dto.DDistance;
        s.BridgeToAft = dto.BridgeToAft; s.BridgeToBow = dto.BridgeToBow;
        s.BowToBulbousBow = dto.BowToBulbousBow;
        s.BreadthMoulded = dto.BreadthMoulded; s.DraftScantling = dto.DraftScantling;
        s.AirdraftReductionMastFouled = dto.AirdraftReductionMastFouled;
        s.LightShip = dto.LightShip; s.DraftFullBallast = dto.DraftFullBallast;
        s.BlockCoefficientNA = dto.BlockCoefficientNA; s.BlockCoefficient = dto.BlockCoefficient;
        s.TpcAtSummerDraft = dto.TpcAtSummerDraft; s.FreshWaterAllowanceFwa = dto.FreshWaterAllowanceFwa;
        s.GrossTonnageInternational = dto.GrossTonnageInternational;
        s.GrossTonnageSuezCanal = dto.GrossTonnageSuezCanal;
        s.GrossTonnagePanamaCanal = dto.GrossTonnagePanamaCanal;
        s.NettTonnageInternational = dto.NettTonnageInternational;
        s.NettTonnageSuezCanal = dto.NettTonnageSuezCanal;
        s.NettTonnagePanamaCanal = dto.NettTonnagePanamaCanal;
        s.ManifoldToWaterlineBallast = dto.ManifoldToWaterlineBallast;
        s.ManifoldToWaterlineLoaded = dto.ManifoldToWaterlineLoaded;
        s.DeckToManifold = dto.DeckToManifold; s.SternToManifold = dto.SternToManifold;
        s.ShipsideToManifold = dto.ShipsideToManifold; s.BowToManifold = dto.BowToManifold;
        s.ManifoldToKeel = dto.ManifoldToKeel; s.ManifoldToBridge = dto.ManifoldToBridge;
        s.MaxLoadingRateShip = dto.MaxLoadingRateShip; s.NumberOfLines = dto.NumberOfLines;
        s.MaxAllowablePressurePsi = dto.MaxAllowablePressurePsi; s.VentingSystemShip = dto.VentingSystemShip;
        // Machinery
        s.AnchorChainPort = dto.AnchorChainPort; s.AnchorChainStarboard = dto.AnchorChainStarboard;
        s.AnchorChainStern = dto.AnchorChainStern; s.AnchorChainSternNA = dto.AnchorChainSternNA;
        s.BowthrusterNA = dto.BowthrusterNA; s.SternthrusterNA = dto.SternthrusterNA;
        s.ShaftGeneratorNA = dto.ShaftGeneratorNA;
        s.HarbourGeneratorMaker = dto.HarbourGeneratorMaker;
        s.HarbourGeneratorMaxPowerKW = dto.HarbourGeneratorMaxPowerKW;
        s.AzimuthEngFwdCount = dto.AzimuthEngFwdCount; s.AzimuthEngFwdMaxPowerKW = dto.AzimuthEngFwdMaxPowerKW;
        s.AzimuthEngAftCount = dto.AzimuthEngAftCount; s.AzimuthEngAftMaxPowerKW = dto.AzimuthEngAftMaxPowerKW;
        // Shipowner
        s.ShipownerName = dto.ShipownerName; s.ShipownerStreet = dto.ShipownerStreet;
        s.ShipownerCountry = dto.ShipownerCountry; s.ShipownerZip = dto.ShipownerZip;
        s.ShipownerCity = dto.ShipownerCity; s.ShipownerPhone = dto.ShipownerPhone;
        s.ShipownerFax = dto.ShipownerFax; s.ShipownerTlx = dto.ShipownerTlx;
        s.ShipownerEmail = dto.ShipownerEmail; s.ShipownerContactPerson = dto.ShipownerContactPerson;
        s.ManagingOwnerName = dto.ManagingOwnerName; s.ManagingOwnerStreet = dto.ManagingOwnerStreet;
        s.ManagingOwnerCountry = dto.ManagingOwnerCountry; s.ManagingOwnerZip = dto.ManagingOwnerZip;
        s.ManagingOwnerCity = dto.ManagingOwnerCity; s.ManagingOwnerPhone = dto.ManagingOwnerPhone;
        s.ManagingOwnerFax = dto.ManagingOwnerFax; s.ManagingOwnerTlx = dto.ManagingOwnerTlx;
        s.ManagingOwnerEmail = dto.ManagingOwnerEmail; s.ManagingOwnerContactPerson = dto.ManagingOwnerContactPerson;
        s.OperatorName = dto.OperatorName; s.OperatorStreet = dto.OperatorStreet;
        s.OperatorCountry = dto.OperatorCountry; s.OperatorZip = dto.OperatorZip;
        s.OperatorCity = dto.OperatorCity; s.OperatorPhone = dto.OperatorPhone;
        s.OperatorFax = dto.OperatorFax; s.OperatorTlx = dto.OperatorTlx;
        s.OperatorEmail = dto.OperatorEmail; s.OperatorContactPerson = dto.OperatorContactPerson;
        s.CsoTitle = dto.CsoTitle; s.CsoFirstName = dto.CsoFirstName; s.CsoLastName = dto.CsoLastName;
        s.CsoStreet = dto.CsoStreet; s.CsoCountry = dto.CsoCountry; s.CsoZip = dto.CsoZip; s.CsoCity = dto.CsoCity;
        s.CsoPhone24h = dto.CsoPhone24h; s.CsoFax = dto.CsoFax; s.CsoTlx = dto.CsoTlx; s.CsoEmail = dto.CsoEmail;
        s.DpaTitle = dto.DpaTitle; s.DpaFirstName = dto.DpaFirstName; s.DpaLastName = dto.DpaLastName;
        s.DpaStreet = dto.DpaStreet; s.DpaCountry = dto.DpaCountry; s.DpaZip = dto.DpaZip; s.DpaCity = dto.DpaCity;
        s.DpaPhone24h = dto.DpaPhone24h; s.DpaFax = dto.DpaFax; s.DpaTlx = dto.DpaTlx; s.DpaEmail = dto.DpaEmail;
        s.QiUsaTitle = dto.QiUsaTitle; s.QiUsaFirstName = dto.QiUsaFirstName; s.QiUsaLastName = dto.QiUsaLastName;
        s.QiUsaStreet = dto.QiUsaStreet; s.QiUsaCountry = dto.QiUsaCountry; s.QiUsaZip = dto.QiUsaZip; s.QiUsaCity = dto.QiUsaCity;
        s.QiUsaPhone24h = dto.QiUsaPhone24h; s.QiUsaFax = dto.QiUsaFax; s.QiUsaTlx = dto.QiUsaTlx; s.QiUsaEmail = dto.QiUsaEmail;
        s.QiPanamaTitle = dto.QiPanamaTitle; s.QiPanamaFirstName = dto.QiPanamaFirstName; s.QiPanamaLastName = dto.QiPanamaLastName;
        s.QiPanamaStreet = dto.QiPanamaStreet; s.QiPanamaCountry = dto.QiPanamaCountry; s.QiPanamaZip = dto.QiPanamaZip; s.QiPanamaCity = dto.QiPanamaCity;
        s.QiPanamaPhone24h = dto.QiPanamaPhone24h; s.QiPanamaFax = dto.QiPanamaFax; s.QiPanamaTlx = dto.QiPanamaTlx; s.QiPanamaEmail = dto.QiPanamaEmail;
        // Charterer
        s.ChartererName = dto.ChartererName; s.ChartererStreet = dto.ChartererStreet;
        s.ChartererCountry = dto.ChartererCountry; s.ChartererZip = dto.ChartererZip;
        s.ChartererCity = dto.ChartererCity; s.ChartererPhone = dto.ChartererPhone;
        s.ChartererFax = dto.ChartererFax; s.ChartererTlx = dto.ChartererTlx;
        s.ChartererEmail = dto.ChartererEmail; s.ChartererContactPerson = dto.ChartererContactPerson;
        s.BareboatChartererName = dto.BareboatChartererName; s.BareboatChartererStreet = dto.BareboatChartererStreet;
        s.BareboatChartererCountry = dto.BareboatChartererCountry; s.BareboatChartererZip = dto.BareboatChartererZip;
        s.BareboatChartererCity = dto.BareboatChartererCity; s.BareboatChartererPhone = dto.BareboatChartererPhone;
        s.BareboatChartererFax = dto.BareboatChartererFax; s.BareboatChartererTlx = dto.BareboatChartererTlx;
        s.BareboatChartererEmail = dto.BareboatChartererEmail; s.BareboatChartererContactPerson = dto.BareboatChartererContactPerson;
        // Class / Flag State
        s.ClassSocietyName = dto.ClassSocietyName; s.ClassSocietyStreet = dto.ClassSocietyStreet;
        s.ClassSocietyCountry = dto.ClassSocietyCountry; s.ClassSocietyZip = dto.ClassSocietyZip;
        s.ClassSocietyCity = dto.ClassSocietyCity; s.ClassSocietyPhone = dto.ClassSocietyPhone;
        s.ClassSocietyFax = dto.ClassSocietyFax; s.ClassSocietyTlx = dto.ClassSocietyTlx;
        s.ClassSocietyEmail = dto.ClassSocietyEmail; s.ClassSocietyContactPerson = dto.ClassSocietyContactPerson;
        s.FlagStateName = dto.FlagStateName; s.FlagStateStreet = dto.FlagStateStreet;
        s.FlagStateCountry = dto.FlagStateCountry; s.FlagStateZip = dto.FlagStateZip;
        s.FlagStateCity = dto.FlagStateCity; s.FlagStatePhone = dto.FlagStatePhone;
        s.FlagStateFax = dto.FlagStateFax; s.FlagStateTlx = dto.FlagStateTlx;
        s.FlagStateEmail = dto.FlagStateEmail; s.FlagStateContactPerson = dto.FlagStateContactPerson;
        // Insurance
        s.PiClubName = dto.PiClubName; s.PiClubStreet = dto.PiClubStreet;
        s.PiClubCountry = dto.PiClubCountry; s.PiClubZip = dto.PiClubZip; s.PiClubCity = dto.PiClubCity;
        s.PiClubPhone = dto.PiClubPhone; s.PiClubFax = dto.PiClubFax; s.PiClubTlx = dto.PiClubTlx;
        s.PiClubEmail = dto.PiClubEmail; s.PiClubContactPerson = dto.PiClubContactPerson;
        s.HmClubName = dto.HmClubName; s.HmClubStreet = dto.HmClubStreet;
        s.HmClubCountry = dto.HmClubCountry; s.HmClubZip = dto.HmClubZip; s.HmClubCity = dto.HmClubCity;
        s.HmClubPhone = dto.HmClubPhone; s.HmClubFax = dto.HmClubFax; s.HmClubTlx = dto.HmClubTlx;
        s.HmClubEmail = dto.HmClubEmail; s.HmClubContactPerson = dto.HmClubContactPerson;
        // Radio Communication
        s.InmarsatTelex1 = dto.InmarsatTelex1; s.InmarsatTelex2 = dto.InmarsatTelex2;
        s.InmarsatPhone1 = dto.InmarsatPhone1; s.InmarsatPhone2 = dto.InmarsatPhone2;
        s.InmarsatFax1 = dto.InmarsatFax1; s.InmarsatFax2 = dto.InmarsatFax2;
        s.EmailAddress1 = dto.EmailAddress1; s.EmailAddress2 = dto.EmailAddress2;
        s.GsmPhone = dto.GsmPhone;
        s.SeaAreaA1 = dto.SeaAreaA1; s.SeaAreaA2 = dto.SeaAreaA2; s.SeaAreaA3 = dto.SeaAreaA3; s.SeaAreaA4 = dto.SeaAreaA4;
        s.DscHF = dto.DscHF; s.DscMF = dto.DscMF; s.DscVHF = dto.DscVHF;
        s.RadiotelephoneHF = dto.RadiotelephoneHF; s.RadiotelephoneMF = dto.RadiotelephoneMF; s.RadiotelephoneVHF = dto.RadiotelephoneVHF;
        s.RadiotelegraphHF = dto.RadiotelegraphHF; s.RadiotelegraphMF = dto.RadiotelegraphMF; s.RadiotelegraphVHF = dto.RadiotelegraphVHF;
        s.Navtex = dto.Navtex; s.Ais = dto.Ais; s.SartTransponder = dto.SartTransponder; s.Radiotelex = dto.Radiotelex;
        s.OtherRadioEquipment = dto.OtherRadioEquipment;
        s.EpirbNumber = dto.EpirbNumber; s.EpirbOperatingSystem = dto.EpirbOperatingSystem;
        s.EpirbMaker = dto.EpirbMaker; s.EpirbModel = dto.EpirbModel; s.EpirbFrequency = dto.EpirbFrequency;
        // Tanks & Cargo
        s.HfoCbm = dto.HfoCbm; s.MdoCbm = dto.MdoCbm; s.LubOilCbm = dto.LubOilCbm;
        s.SludgeCbm = dto.SludgeCbm; s.BilgeWaterCbm = dto.BilgeWaterCbm; s.SewageCbm = dto.SewageCbm;
        s.FreshWaterCbm = dto.FreshWaterCbm; s.BallastWaterCbm = dto.BallastWaterCbm;
        s.NoOfBallastTanks = dto.NoOfBallastTanks;
        s.TeuTotal = dto.TeuTotal; s.TeuOnDeck = dto.TeuOnDeck; s.TeuUnderDeck = dto.TeuUnderDeck;
        s.GrainCbm = dto.GrainCbm; s.BalesCbm = dto.BalesCbm;
        s.NoOfCargoHolds = dto.NoOfCargoHolds; s.NoOfHatches = dto.NoOfHatches;
    }

    // ═══════════════════════════════════════════
    // MAPPING: Child DTOs → Entities
    // ═══════════════════════════════════════════

    private List<ShipMainEngine> MapMainEngines(List<ShipMainEngineDto> dtos) =>
        dtos.Select((d, i) => new ShipMainEngine
        {
            MeType = d.MeType, MeFuelGrade = d.MeFuelGrade,
            MePowerKW = d.MePowerKW, McrKW = d.McrKW, SortOrder = i
        }).ToList();

    private List<ShipAuxiliaryEngine> MapAuxiliaryEngines(List<ShipAuxiliaryEngineDto> dtos) =>
        dtos.Select((d, i) => new ShipAuxiliaryEngine
        {
            AeType = d.AeType, AeFuelGrade = d.AeFuelGrade,
            AePowerKW = d.AePowerKW, SortOrder = i
        }).ToList();

    private List<ShipPropeller> MapPropellers(List<ShipPropellerDto> dtos) =>
        dtos.Select((d, i) => new ShipPropeller
        {
            PropellerType = d.PropellerType, NumberOfBlades = d.NumberOfBlades,
            Rotation = d.Rotation, DiameterMm = d.DiameterMm,
            PropellerPitchGeometricMm = d.PropellerPitchGeometricMm,
            PitchRatio = d.PitchRatio, SortOrder = i
        }).ToList();

    private List<ShipBowthruster> MapBowthrusters(List<ShipBowthrusterDto> dtos) =>
        dtos.Select((d, i) => new ShipBowthruster { PowerKW = d.PowerKW, SortOrder = i }).ToList();

    private List<ShipSternthruster> MapSternthrusters(List<ShipSternthrusterDto> dtos) =>
        dtos.Select((d, i) => new ShipSternthruster { PowerKW = d.PowerKW, SortOrder = i }).ToList();

    private List<ShipRudder> MapRudders(List<ShipRudderDto> dtos) =>
        dtos.Select((d, i) => new ShipRudder { RudderType = d.RudderType, SortOrder = i }).ToList();

    private List<ShipShaftGenerator> MapShaftGenerators(List<ShipShaftGeneratorDto> dtos) =>
        dtos.Select((d, i) => new ShipShaftGenerator { MaxPowerKW = d.MaxPowerKW, SortOrder = i }).ToList();

    private List<ShipBoiler> MapBoilers(List<ShipBoilerDto> dtos) =>
        dtos.Select((d, i) => new ShipBoiler { BoilerType = d.BoilerType, Model = d.Model, SortOrder = i }).ToList();

    private List<ShipLoadLine> MapLoadLines(List<ShipLoadLineDto> dtos) =>
        dtos.Select((d, i) => new ShipLoadLine
        {
            LoadLineType = d.LoadLineType, DraftM = d.DraftM, FreeboardM = d.FreeboardM,
            DisplacementMt = d.DisplacementMt, DeadweightMt = d.DeadweightMt, SortOrder = i
        }).ToList();

    private List<ShipPilotCardData> MapPilotCardData(List<ShipPilotCardDataDto> dtos) =>
        dtos.Select((d, i) => new ShipPilotCardData
        {
            EngineOrder = d.EngineOrder, MainEngineRPM = d.MainEngineRPM,
            SpeedLoadedKts = d.SpeedLoadedKts, SpeedBallastKts = d.SpeedBallastKts, SortOrder = i
        }).ToList();
}
