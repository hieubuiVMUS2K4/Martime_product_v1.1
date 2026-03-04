using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================================
// SHIP'S DATA MODULE - DTOs
// ============================================================

#region Response DTOs

/// <summary>
/// Full Ship Data response DTO (includes all tabs + child collections)
/// </summary>
public class ShipDataDto
{
    public Guid Id { get; set; }

    // === Basic Data ===
    public string ImoNumber { get; set; } = string.Empty;
    public string? OfficialNumber { get; set; }
    public string? CallSign { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public string PortOfRegistry { get; set; } = string.Empty;
    public string? PreviousName { get; set; }
    public string? PreviousFlag { get; set; }
    public string? MmsiNumber { get; set; }
    public string? TypeOfVessel { get; set; }
    public string? ClassNotation { get; set; }
    public string? ClassRegisterNumber { get; set; }
    public string? ShipyardCountry { get; set; }
    public string? ShipyardName { get; set; }
    public string? YardNo { get; set; }
    public string? CompanyImoNumber { get; set; }
    public string? SuezCanalIdNumber { get; set; }
    public DateTime? KeelLaidDate { get; set; }
    public int? YearBuilt { get; set; }
    public DateTime? DateOfRegistry { get; set; }
    public string? OwnerImoNumber { get; set; }
    public string? PanamaCanalIdNumber { get; set; }
    public int? MaxPersonsAllowedOB { get; set; }
    public double? ServiceSpeedKts { get; set; }
    public string? VrpNumber { get; set; }
    public string? VrpType { get; set; }
    public int? NoOfCrewSafeManning { get; set; }
    public int? MaxPassengersAllowedOB { get; set; }

    // === Dimensions ===
    public double? Loa { get; set; }
    public double? DepthMoulded { get; set; }
    public double? HMaxAirdraft { get; set; }
    public double? ParallelBodyBallast { get; set; }
    public double? ParallelBodyLoaded { get; set; }
    public double? Lbp { get; set; }
    public double? DraftMoulded { get; set; }
    public double? DDistance { get; set; }
    public double? BridgeToAft { get; set; }
    public double? BridgeToBow { get; set; }
    public double? BowToBulbousBow { get; set; }
    public double? BreadthMoulded { get; set; }
    public double? DraftScantling { get; set; }
    public double? AirdraftReductionMastFouled { get; set; }
    public double? LightShip { get; set; }
    public double? DraftFullBallast { get; set; }
    public bool BlockCoefficientNA { get; set; }
    public double? BlockCoefficient { get; set; }
    public double? TpcAtSummerDraft { get; set; }
    public double? FreshWaterAllowanceFwa { get; set; }
    public double? GrossTonnageInternational { get; set; }
    public double? GrossTonnageSuezCanal { get; set; }
    public double? GrossTonnagePanamaCanal { get; set; }
    public double? NettTonnageInternational { get; set; }
    public double? NettTonnageSuezCanal { get; set; }
    public double? NettTonnagePanamaCanal { get; set; }
    public double? ManifoldToWaterlineBallast { get; set; }
    public double? ManifoldToWaterlineLoaded { get; set; }
    public double? DeckToManifold { get; set; }
    public double? SternToManifold { get; set; }
    public double? ShipsideToManifold { get; set; }
    public double? BowToManifold { get; set; }
    public double? ManifoldToKeel { get; set; }
    public double? ManifoldToBridge { get; set; }
    public double? MaxLoadingRateShip { get; set; }
    public int? NumberOfLines { get; set; }
    public double? MaxAllowablePressurePsi { get; set; }
    public string? VentingSystemShip { get; set; }

    // === Machinery fixed fields ===
    public int? AnchorChainPort { get; set; }
    public int? AnchorChainStarboard { get; set; }
    public int? AnchorChainStern { get; set; }
    public bool AnchorChainSternNA { get; set; }
    public bool BowthrusterNA { get; set; }
    public bool SternthrusterNA { get; set; }
    public bool ShaftGeneratorNA { get; set; }
    public string? HarbourGeneratorMaker { get; set; }
    public double? HarbourGeneratorMaxPowerKW { get; set; }
    public int? AzimuthEngFwdCount { get; set; }
    public double? AzimuthEngFwdMaxPowerKW { get; set; }
    public int? AzimuthEngAftCount { get; set; }
    public double? AzimuthEngAftMaxPowerKW { get; set; }

    // === Shipowner ===
    public string? ShipownerName { get; set; }
    public string? ShipownerStreet { get; set; }
    public string? ShipownerCountry { get; set; }
    public string? ShipownerZip { get; set; }
    public string? ShipownerCity { get; set; }
    public string? ShipownerPhone { get; set; }
    public string? ShipownerFax { get; set; }
    public string? ShipownerTlx { get; set; }
    public string? ShipownerEmail { get; set; }
    public string? ShipownerContactPerson { get; set; }
    public string? ManagingOwnerName { get; set; }
    public string? ManagingOwnerStreet { get; set; }
    public string? ManagingOwnerCountry { get; set; }
    public string? ManagingOwnerZip { get; set; }
    public string? ManagingOwnerCity { get; set; }
    public string? ManagingOwnerPhone { get; set; }
    public string? ManagingOwnerFax { get; set; }
    public string? ManagingOwnerTlx { get; set; }
    public string? ManagingOwnerEmail { get; set; }
    public string? ManagingOwnerContactPerson { get; set; }
    public string? OperatorName { get; set; }
    public string? OperatorStreet { get; set; }
    public string? OperatorCountry { get; set; }
    public string? OperatorZip { get; set; }
    public string? OperatorCity { get; set; }
    public string? OperatorPhone { get; set; }
    public string? OperatorFax { get; set; }
    public string? OperatorTlx { get; set; }
    public string? OperatorEmail { get; set; }
    public string? OperatorContactPerson { get; set; }
    public string? CsoTitle { get; set; }
    public string? CsoFirstName { get; set; }
    public string? CsoLastName { get; set; }
    public string? CsoStreet { get; set; }
    public string? CsoCountry { get; set; }
    public string? CsoZip { get; set; }
    public string? CsoCity { get; set; }
    public string? CsoPhone24h { get; set; }
    public string? CsoFax { get; set; }
    public string? CsoTlx { get; set; }
    public string? CsoEmail { get; set; }
    public string? DpaTitle { get; set; }
    public string? DpaFirstName { get; set; }
    public string? DpaLastName { get; set; }
    public string? DpaStreet { get; set; }
    public string? DpaCountry { get; set; }
    public string? DpaZip { get; set; }
    public string? DpaCity { get; set; }
    public string? DpaPhone24h { get; set; }
    public string? DpaFax { get; set; }
    public string? DpaTlx { get; set; }
    public string? DpaEmail { get; set; }
    public string? QiUsaTitle { get; set; }
    public string? QiUsaFirstName { get; set; }
    public string? QiUsaLastName { get; set; }
    public string? QiUsaStreet { get; set; }
    public string? QiUsaCountry { get; set; }
    public string? QiUsaZip { get; set; }
    public string? QiUsaCity { get; set; }
    public string? QiUsaPhone24h { get; set; }
    public string? QiUsaFax { get; set; }
    public string? QiUsaTlx { get; set; }
    public string? QiUsaEmail { get; set; }
    public string? QiPanamaTitle { get; set; }
    public string? QiPanamaFirstName { get; set; }
    public string? QiPanamaLastName { get; set; }
    public string? QiPanamaStreet { get; set; }
    public string? QiPanamaCountry { get; set; }
    public string? QiPanamaZip { get; set; }
    public string? QiPanamaCity { get; set; }
    public string? QiPanamaPhone24h { get; set; }
    public string? QiPanamaFax { get; set; }
    public string? QiPanamaTlx { get; set; }
    public string? QiPanamaEmail { get; set; }

    // === Charterer ===
    public string? ChartererName { get; set; }
    public string? ChartererStreet { get; set; }
    public string? ChartererCountry { get; set; }
    public string? ChartererZip { get; set; }
    public string? ChartererCity { get; set; }
    public string? ChartererPhone { get; set; }
    public string? ChartererFax { get; set; }
    public string? ChartererTlx { get; set; }
    public string? ChartererEmail { get; set; }
    public string? ChartererContactPerson { get; set; }
    public string? BareboatChartererName { get; set; }
    public string? BareboatChartererStreet { get; set; }
    public string? BareboatChartererCountry { get; set; }
    public string? BareboatChartererZip { get; set; }
    public string? BareboatChartererCity { get; set; }
    public string? BareboatChartererPhone { get; set; }
    public string? BareboatChartererFax { get; set; }
    public string? BareboatChartererTlx { get; set; }
    public string? BareboatChartererEmail { get; set; }
    public string? BareboatChartererContactPerson { get; set; }

    // === Class / Flag State ===
    public string? ClassSocietyName { get; set; }
    public string? ClassSocietyStreet { get; set; }
    public string? ClassSocietyCountry { get; set; }
    public string? ClassSocietyZip { get; set; }
    public string? ClassSocietyCity { get; set; }
    public string? ClassSocietyPhone { get; set; }
    public string? ClassSocietyFax { get; set; }
    public string? ClassSocietyTlx { get; set; }
    public string? ClassSocietyEmail { get; set; }
    public string? ClassSocietyContactPerson { get; set; }
    public string? FlagStateName { get; set; }
    public string? FlagStateStreet { get; set; }
    public string? FlagStateCountry { get; set; }
    public string? FlagStateZip { get; set; }
    public string? FlagStateCity { get; set; }
    public string? FlagStatePhone { get; set; }
    public string? FlagStateFax { get; set; }
    public string? FlagStateTlx { get; set; }
    public string? FlagStateEmail { get; set; }
    public string? FlagStateContactPerson { get; set; }

    // === Insurance ===
    public string? PiClubName { get; set; }
    public string? PiClubStreet { get; set; }
    public string? PiClubCountry { get; set; }
    public string? PiClubZip { get; set; }
    public string? PiClubCity { get; set; }
    public string? PiClubPhone { get; set; }
    public string? PiClubFax { get; set; }
    public string? PiClubTlx { get; set; }
    public string? PiClubEmail { get; set; }
    public string? PiClubContactPerson { get; set; }
    public string? HmClubName { get; set; }
    public string? HmClubStreet { get; set; }
    public string? HmClubCountry { get; set; }
    public string? HmClubZip { get; set; }
    public string? HmClubCity { get; set; }
    public string? HmClubPhone { get; set; }
    public string? HmClubFax { get; set; }
    public string? HmClubTlx { get; set; }
    public string? HmClubEmail { get; set; }
    public string? HmClubContactPerson { get; set; }

    // === Radio Communication ===
    public string? InmarsatTelex1 { get; set; }
    public string? InmarsatTelex2 { get; set; }
    public string? InmarsatPhone1 { get; set; }
    public string? InmarsatPhone2 { get; set; }
    public string? InmarsatFax1 { get; set; }
    public string? InmarsatFax2 { get; set; }
    public string? EmailAddress1 { get; set; }
    public string? EmailAddress2 { get; set; }
    public string? GsmPhone { get; set; }
    public bool SeaAreaA1 { get; set; }
    public bool SeaAreaA2 { get; set; }
    public bool SeaAreaA3 { get; set; }
    public bool SeaAreaA4 { get; set; }
    public bool DscHF { get; set; }
    public bool DscMF { get; set; }
    public bool DscVHF { get; set; }
    public bool RadiotelephoneHF { get; set; }
    public bool RadiotelephoneMF { get; set; }
    public bool RadiotelephoneVHF { get; set; }
    public bool RadiotelegraphHF { get; set; }
    public bool RadiotelegraphMF { get; set; }
    public bool RadiotelegraphVHF { get; set; }
    public bool Navtex { get; set; }
    public bool Ais { get; set; }
    public bool SartTransponder { get; set; }
    public bool Radiotelex { get; set; }
    public string? OtherRadioEquipment { get; set; }
    public string? EpirbNumber { get; set; }
    public string? EpirbOperatingSystem { get; set; }
    public string? EpirbMaker { get; set; }
    public string? EpirbModel { get; set; }
    public string? EpirbFrequency { get; set; }

    // === Tanks & Cargo Spaces ===
    public double? HfoCbm { get; set; }
    public double? MdoCbm { get; set; }
    public double? LubOilCbm { get; set; }
    public double? SludgeCbm { get; set; }
    public double? BilgeWaterCbm { get; set; }
    public double? SewageCbm { get; set; }
    public double? FreshWaterCbm { get; set; }
    public double? BallastWaterCbm { get; set; }
    public int? NoOfBallastTanks { get; set; }
    public int? TeuTotal { get; set; }
    public int? TeuOnDeck { get; set; }
    public int? TeuUnderDeck { get; set; }
    public double? GrainCbm { get; set; }
    public double? BalesCbm { get; set; }
    public int? NoOfCargoHolds { get; set; }
    public int? NoOfHatches { get; set; }

    // === Child collections ===
    public List<ShipMainEngineDto> MainEngines { get; set; } = new();
    public List<ShipAuxiliaryEngineDto> AuxiliaryEngines { get; set; } = new();
    public List<ShipPropellerDto> Propellers { get; set; } = new();
    public List<ShipBowthrusterDto> Bowthrusters { get; set; } = new();
    public List<ShipSternthrusterDto> Sternthrusters { get; set; } = new();
    public List<ShipRudderDto> Rudders { get; set; } = new();
    public List<ShipShaftGeneratorDto> ShaftGenerators { get; set; } = new();
    public List<ShipBoilerDto> Boilers { get; set; } = new();
    public List<ShipLoadLineDto> LoadLines { get; set; } = new();
    public List<ShipPilotCardDataDto> PilotCardData { get; set; } = new();

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region Child DTOs

public class ShipMainEngineDto
{
    public Guid? Id { get; set; }
    public string? MeType { get; set; }
    public string? MeFuelGrade { get; set; }
    public double? MePowerKW { get; set; }
    public double? McrKW { get; set; }
    public int SortOrder { get; set; }
}

public class ShipAuxiliaryEngineDto
{
    public Guid? Id { get; set; }
    public string? AeType { get; set; }
    public string? AeFuelGrade { get; set; }
    public double? AePowerKW { get; set; }
    public int SortOrder { get; set; }
}

public class ShipPropellerDto
{
    public Guid? Id { get; set; }
    public string? PropellerType { get; set; }
    public int? NumberOfBlades { get; set; }
    public string? Rotation { get; set; }
    public double? DiameterMm { get; set; }
    public double? PropellerPitchGeometricMm { get; set; }
    public double? PitchRatio { get; set; }
    public int SortOrder { get; set; }
}

public class ShipBowthrusterDto
{
    public Guid? Id { get; set; }
    public double? PowerKW { get; set; }
    public int SortOrder { get; set; }
}

public class ShipSternthrusterDto
{
    public Guid? Id { get; set; }
    public double? PowerKW { get; set; }
    public int SortOrder { get; set; }
}

public class ShipRudderDto
{
    public Guid? Id { get; set; }
    public string? RudderType { get; set; }
    public int SortOrder { get; set; }
}

public class ShipShaftGeneratorDto
{
    public Guid? Id { get; set; }
    public double? MaxPowerKW { get; set; }
    public int SortOrder { get; set; }
}

public class ShipBoilerDto
{
    public Guid? Id { get; set; }
    public string? BoilerType { get; set; }
    public string? Model { get; set; }
    public int SortOrder { get; set; }
}

public class ShipLoadLineDto
{
    public Guid? Id { get; set; }
    public string LoadLineType { get; set; } = string.Empty;
    public double? DraftM { get; set; }
    public double? FreeboardM { get; set; }
    public double? DisplacementMt { get; set; }
    public double? DeadweightMt { get; set; }
    public int SortOrder { get; set; }
}

public class ShipPilotCardDataDto
{
    public Guid? Id { get; set; }
    public string EngineOrder { get; set; } = string.Empty;
    public double? MainEngineRPM { get; set; }
    public double? SpeedLoadedKts { get; set; }
    public double? SpeedBallastKts { get; set; }
    public int SortOrder { get; set; }
}

#endregion

#region Request DTOs

/// <summary>
/// DTO for creating/updating the full Ship Data (all tabs at once)
/// </summary>
public class SaveShipDataDto
{
    // === Basic Data ===
    [Required] [MaxLength(20)]
    public string ImoNumber { get; set; } = string.Empty;
    public string? OfficialNumber { get; set; }
    public string? CallSign { get; set; }
    [Required] [MaxLength(200)]
    public string ShipName { get; set; } = string.Empty;
    [Required] [MaxLength(100)]
    public string Flag { get; set; } = string.Empty;
    [Required] [MaxLength(200)]
    public string PortOfRegistry { get; set; } = string.Empty;
    public string? PreviousName { get; set; }
    public string? PreviousFlag { get; set; }
    public string? MmsiNumber { get; set; }
    public string? TypeOfVessel { get; set; }
    public string? ClassNotation { get; set; }
    public string? ClassRegisterNumber { get; set; }
    public string? ShipyardCountry { get; set; }
    public string? ShipyardName { get; set; }
    public string? YardNo { get; set; }
    public string? CompanyImoNumber { get; set; }
    public string? SuezCanalIdNumber { get; set; }
    public DateTime? KeelLaidDate { get; set; }
    public int? YearBuilt { get; set; }
    public DateTime? DateOfRegistry { get; set; }
    public string? OwnerImoNumber { get; set; }
    public string? PanamaCanalIdNumber { get; set; }
    public int? MaxPersonsAllowedOB { get; set; }
    public double? ServiceSpeedKts { get; set; }
    public string? VrpNumber { get; set; }
    public string? VrpType { get; set; }
    public int? NoOfCrewSafeManning { get; set; }
    public int? MaxPassengersAllowedOB { get; set; }

    // === Dimensions ===
    public double? Loa { get; set; }
    public double? DepthMoulded { get; set; }
    public double? HMaxAirdraft { get; set; }
    public double? ParallelBodyBallast { get; set; }
    public double? ParallelBodyLoaded { get; set; }
    public double? Lbp { get; set; }
    public double? DraftMoulded { get; set; }
    public double? DDistance { get; set; }
    public double? BridgeToAft { get; set; }
    public double? BridgeToBow { get; set; }
    public double? BowToBulbousBow { get; set; }
    public double? BreadthMoulded { get; set; }
    public double? DraftScantling { get; set; }
    public double? AirdraftReductionMastFouled { get; set; }
    public double? LightShip { get; set; }
    public double? DraftFullBallast { get; set; }
    public bool BlockCoefficientNA { get; set; }
    public double? BlockCoefficient { get; set; }
    public double? TpcAtSummerDraft { get; set; }
    public double? FreshWaterAllowanceFwa { get; set; }
    public double? GrossTonnageInternational { get; set; }
    public double? GrossTonnageSuezCanal { get; set; }
    public double? GrossTonnagePanamaCanal { get; set; }
    public double? NettTonnageInternational { get; set; }
    public double? NettTonnageSuezCanal { get; set; }
    public double? NettTonnagePanamaCanal { get; set; }
    public double? ManifoldToWaterlineBallast { get; set; }
    public double? ManifoldToWaterlineLoaded { get; set; }
    public double? DeckToManifold { get; set; }
    public double? SternToManifold { get; set; }
    public double? ShipsideToManifold { get; set; }
    public double? BowToManifold { get; set; }
    public double? ManifoldToKeel { get; set; }
    public double? ManifoldToBridge { get; set; }
    public double? MaxLoadingRateShip { get; set; }
    public int? NumberOfLines { get; set; }
    public double? MaxAllowablePressurePsi { get; set; }
    public string? VentingSystemShip { get; set; }

    // === Machinery ===
    public int? AnchorChainPort { get; set; }
    public int? AnchorChainStarboard { get; set; }
    public int? AnchorChainStern { get; set; }
    public bool AnchorChainSternNA { get; set; }
    public bool BowthrusterNA { get; set; }
    public bool SternthrusterNA { get; set; }
    public bool ShaftGeneratorNA { get; set; }
    public string? HarbourGeneratorMaker { get; set; }
    public double? HarbourGeneratorMaxPowerKW { get; set; }
    public int? AzimuthEngFwdCount { get; set; }
    public double? AzimuthEngFwdMaxPowerKW { get; set; }
    public int? AzimuthEngAftCount { get; set; }
    public double? AzimuthEngAftMaxPowerKW { get; set; }

    // === Shipowner ===
    public string? ShipownerName { get; set; }
    public string? ShipownerStreet { get; set; }
    public string? ShipownerCountry { get; set; }
    public string? ShipownerZip { get; set; }
    public string? ShipownerCity { get; set; }
    public string? ShipownerPhone { get; set; }
    public string? ShipownerFax { get; set; }
    public string? ShipownerTlx { get; set; }
    public string? ShipownerEmail { get; set; }
    public string? ShipownerContactPerson { get; set; }
    public string? ManagingOwnerName { get; set; }
    public string? ManagingOwnerStreet { get; set; }
    public string? ManagingOwnerCountry { get; set; }
    public string? ManagingOwnerZip { get; set; }
    public string? ManagingOwnerCity { get; set; }
    public string? ManagingOwnerPhone { get; set; }
    public string? ManagingOwnerFax { get; set; }
    public string? ManagingOwnerTlx { get; set; }
    public string? ManagingOwnerEmail { get; set; }
    public string? ManagingOwnerContactPerson { get; set; }
    public string? OperatorName { get; set; }
    public string? OperatorStreet { get; set; }
    public string? OperatorCountry { get; set; }
    public string? OperatorZip { get; set; }
    public string? OperatorCity { get; set; }
    public string? OperatorPhone { get; set; }
    public string? OperatorFax { get; set; }
    public string? OperatorTlx { get; set; }
    public string? OperatorEmail { get; set; }
    public string? OperatorContactPerson { get; set; }
    public string? CsoTitle { get; set; }
    public string? CsoFirstName { get; set; }
    public string? CsoLastName { get; set; }
    public string? CsoStreet { get; set; }
    public string? CsoCountry { get; set; }
    public string? CsoZip { get; set; }
    public string? CsoCity { get; set; }
    public string? CsoPhone24h { get; set; }
    public string? CsoFax { get; set; }
    public string? CsoTlx { get; set; }
    public string? CsoEmail { get; set; }
    public string? DpaTitle { get; set; }
    public string? DpaFirstName { get; set; }
    public string? DpaLastName { get; set; }
    public string? DpaStreet { get; set; }
    public string? DpaCountry { get; set; }
    public string? DpaZip { get; set; }
    public string? DpaCity { get; set; }
    public string? DpaPhone24h { get; set; }
    public string? DpaFax { get; set; }
    public string? DpaTlx { get; set; }
    public string? DpaEmail { get; set; }
    public string? QiUsaTitle { get; set; }
    public string? QiUsaFirstName { get; set; }
    public string? QiUsaLastName { get; set; }
    public string? QiUsaStreet { get; set; }
    public string? QiUsaCountry { get; set; }
    public string? QiUsaZip { get; set; }
    public string? QiUsaCity { get; set; }
    public string? QiUsaPhone24h { get; set; }
    public string? QiUsaFax { get; set; }
    public string? QiUsaTlx { get; set; }
    public string? QiUsaEmail { get; set; }
    public string? QiPanamaTitle { get; set; }
    public string? QiPanamaFirstName { get; set; }
    public string? QiPanamaLastName { get; set; }
    public string? QiPanamaStreet { get; set; }
    public string? QiPanamaCountry { get; set; }
    public string? QiPanamaZip { get; set; }
    public string? QiPanamaCity { get; set; }
    public string? QiPanamaPhone24h { get; set; }
    public string? QiPanamaFax { get; set; }
    public string? QiPanamaTlx { get; set; }
    public string? QiPanamaEmail { get; set; }

    // === Charterer ===
    public string? ChartererName { get; set; }
    public string? ChartererStreet { get; set; }
    public string? ChartererCountry { get; set; }
    public string? ChartererZip { get; set; }
    public string? ChartererCity { get; set; }
    public string? ChartererPhone { get; set; }
    public string? ChartererFax { get; set; }
    public string? ChartererTlx { get; set; }
    public string? ChartererEmail { get; set; }
    public string? ChartererContactPerson { get; set; }
    public string? BareboatChartererName { get; set; }
    public string? BareboatChartererStreet { get; set; }
    public string? BareboatChartererCountry { get; set; }
    public string? BareboatChartererZip { get; set; }
    public string? BareboatChartererCity { get; set; }
    public string? BareboatChartererPhone { get; set; }
    public string? BareboatChartererFax { get; set; }
    public string? BareboatChartererTlx { get; set; }
    public string? BareboatChartererEmail { get; set; }
    public string? BareboatChartererContactPerson { get; set; }

    // === Class / Flag State ===
    public string? ClassSocietyName { get; set; }
    public string? ClassSocietyStreet { get; set; }
    public string? ClassSocietyCountry { get; set; }
    public string? ClassSocietyZip { get; set; }
    public string? ClassSocietyCity { get; set; }
    public string? ClassSocietyPhone { get; set; }
    public string? ClassSocietyFax { get; set; }
    public string? ClassSocietyTlx { get; set; }
    public string? ClassSocietyEmail { get; set; }
    public string? ClassSocietyContactPerson { get; set; }
    public string? FlagStateName { get; set; }
    public string? FlagStateStreet { get; set; }
    public string? FlagStateCountry { get; set; }
    public string? FlagStateZip { get; set; }
    public string? FlagStateCity { get; set; }
    public string? FlagStatePhone { get; set; }
    public string? FlagStateFax { get; set; }
    public string? FlagStateTlx { get; set; }
    public string? FlagStateEmail { get; set; }
    public string? FlagStateContactPerson { get; set; }

    // === Insurance ===
    public string? PiClubName { get; set; }
    public string? PiClubStreet { get; set; }
    public string? PiClubCountry { get; set; }
    public string? PiClubZip { get; set; }
    public string? PiClubCity { get; set; }
    public string? PiClubPhone { get; set; }
    public string? PiClubFax { get; set; }
    public string? PiClubTlx { get; set; }
    public string? PiClubEmail { get; set; }
    public string? PiClubContactPerson { get; set; }
    public string? HmClubName { get; set; }
    public string? HmClubStreet { get; set; }
    public string? HmClubCountry { get; set; }
    public string? HmClubZip { get; set; }
    public string? HmClubCity { get; set; }
    public string? HmClubPhone { get; set; }
    public string? HmClubFax { get; set; }
    public string? HmClubTlx { get; set; }
    public string? HmClubEmail { get; set; }
    public string? HmClubContactPerson { get; set; }

    // === Radio Communication ===
    public string? InmarsatTelex1 { get; set; }
    public string? InmarsatTelex2 { get; set; }
    public string? InmarsatPhone1 { get; set; }
    public string? InmarsatPhone2 { get; set; }
    public string? InmarsatFax1 { get; set; }
    public string? InmarsatFax2 { get; set; }
    public string? EmailAddress1 { get; set; }
    public string? EmailAddress2 { get; set; }
    public string? GsmPhone { get; set; }
    public bool SeaAreaA1 { get; set; }
    public bool SeaAreaA2 { get; set; }
    public bool SeaAreaA3 { get; set; }
    public bool SeaAreaA4 { get; set; }
    public bool DscHF { get; set; }
    public bool DscMF { get; set; }
    public bool DscVHF { get; set; }
    public bool RadiotelephoneHF { get; set; }
    public bool RadiotelephoneMF { get; set; }
    public bool RadiotelephoneVHF { get; set; }
    public bool RadiotelegraphHF { get; set; }
    public bool RadiotelegraphMF { get; set; }
    public bool RadiotelegraphVHF { get; set; }
    public bool Navtex { get; set; }
    public bool Ais { get; set; }
    public bool SartTransponder { get; set; }
    public bool Radiotelex { get; set; }
    public string? OtherRadioEquipment { get; set; }
    public string? EpirbNumber { get; set; }
    public string? EpirbOperatingSystem { get; set; }
    public string? EpirbMaker { get; set; }
    public string? EpirbModel { get; set; }
    public string? EpirbFrequency { get; set; }

    // === Tanks & Cargo ===
    public double? HfoCbm { get; set; }
    public double? MdoCbm { get; set; }
    public double? LubOilCbm { get; set; }
    public double? SludgeCbm { get; set; }
    public double? BilgeWaterCbm { get; set; }
    public double? SewageCbm { get; set; }
    public double? FreshWaterCbm { get; set; }
    public double? BallastWaterCbm { get; set; }
    public int? NoOfBallastTanks { get; set; }
    public int? TeuTotal { get; set; }
    public int? TeuOnDeck { get; set; }
    public int? TeuUnderDeck { get; set; }
    public double? GrainCbm { get; set; }
    public double? BalesCbm { get; set; }
    public int? NoOfCargoHolds { get; set; }
    public int? NoOfHatches { get; set; }

    // === Child collections (sent together with the save) ===
    public List<ShipMainEngineDto> MainEngines { get; set; } = new();
    public List<ShipAuxiliaryEngineDto> AuxiliaryEngines { get; set; } = new();
    public List<ShipPropellerDto> Propellers { get; set; } = new();
    public List<ShipBowthrusterDto> Bowthrusters { get; set; } = new();
    public List<ShipSternthrusterDto> Sternthrusters { get; set; } = new();
    public List<ShipRudderDto> Rudders { get; set; } = new();
    public List<ShipShaftGeneratorDto> ShaftGenerators { get; set; } = new();
    public List<ShipBoilerDto> Boilers { get; set; } = new();
    public List<ShipLoadLineDto> LoadLines { get; set; } = new();
    public List<ShipPilotCardDataDto> PilotCardData { get; set; } = new();
}

#endregion
