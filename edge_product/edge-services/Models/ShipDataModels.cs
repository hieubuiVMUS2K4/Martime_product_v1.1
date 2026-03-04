using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MaritimeEdge.Models;

// ============================================================
// SHIP'S DATA MODULE - Quản lý thông tin tàu
// Tuân thủ tiêu chuẩn quốc tế: IMO, SOLAS, MARPOL
// ============================================================

/// <summary>
/// Ship's Data - Bảng chính chứa toàn bộ thông tin tàu
/// Bao gồm: Basic Data, Dimensions, Shipowner, Charterer,
/// Class/Flag State, Insurance, Radio Communication, Tanks & Cargo
/// </summary>
public class ShipData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ═══════════════════════════════════════════
    // TAB 1: BASIC DATA
    // ═══════════════════════════════════════════

    /// <summary>IMO Number (bắt buộc) - Mã nhận dạng IMO</summary>
    [Required]
    [MaxLength(20)]
    public string ImoNumber { get; set; } = string.Empty;

    /// <summary>Official Number</summary>
    [MaxLength(50)]
    public string? OfficialNumber { get; set; }

    /// <summary>Call Sign</summary>
    [MaxLength(20)]
    public string? CallSign { get; set; }

    /// <summary>Ship's Name (bắt buộc)</summary>
    [Required]
    [MaxLength(200)]
    public string ShipName { get; set; } = string.Empty;

    /// <summary>Flag (bắt buộc) - Quốc kỳ tàu</summary>
    [Required]
    [MaxLength(100)]
    public string Flag { get; set; } = string.Empty;

    /// <summary>Port of Registry (bắt buộc)</summary>
    [Required]
    [MaxLength(200)]
    public string PortOfRegistry { get; set; } = string.Empty;

    /// <summary>Previous Name</summary>
    [MaxLength(200)]
    public string? PreviousName { get; set; }

    /// <summary>Previous Flag</summary>
    [MaxLength(100)]
    public string? PreviousFlag { get; set; }

    /// <summary>MMSI Number - Maritime Mobile Service Identity</summary>
    [MaxLength(20)]
    public string? MmsiNumber { get; set; }

    /// <summary>Type of vessel (dropdown)</summary>
    [MaxLength(200)]
    public string? TypeOfVessel { get; set; }

    /// <summary>Class Notation</summary>
    [MaxLength(200)]
    public string? ClassNotation { get; set; }

    /// <summary>Class Register Number</summary>
    [MaxLength(50)]
    public string? ClassRegisterNumber { get; set; }

    /// <summary>Shipyard / Country</summary>
    [MaxLength(100)]
    public string? ShipyardCountry { get; set; }

    /// <summary>Shipyard / Name</summary>
    [MaxLength(200)]
    public string? ShipyardName { get; set; }

    /// <summary>Yard No</summary>
    [MaxLength(50)]
    public string? YardNo { get; set; }

    /// <summary>Company IMO Number</summary>
    [MaxLength(50)]
    public string? CompanyImoNumber { get; set; }

    /// <summary>Suez Canal ID Number</summary>
    [MaxLength(50)]
    public string? SuezCanalIdNumber { get; set; }

    /// <summary>Keel Laid Date</summary>
    public DateTime? KeelLaidDate { get; set; }

    /// <summary>Year of Built</summary>
    public int? YearBuilt { get; set; }

    /// <summary>Date of Registry</summary>
    public DateTime? DateOfRegistry { get; set; }

    /// <summary>Owner IMO Number</summary>
    [MaxLength(50)]
    public string? OwnerImoNumber { get; set; }

    /// <summary>Panama Canal ID Number</summary>
    [MaxLength(50)]
    public string? PanamaCanalIdNumber { get; set; }

    /// <summary>Max Persons Allowed O/B</summary>
    public int? MaxPersonsAllowedOB { get; set; }

    /// <summary>Service speed (kts)</summary>
    public double? ServiceSpeedKts { get; set; }

    /// <summary>VRP – Vessel Response Plan Number</summary>
    [MaxLength(50)]
    public string? VrpNumber { get; set; }

    /// <summary>VRP Type (Required for US E-NOAID): NONTANK, TANK</summary>
    [MaxLength(50)]
    public string? VrpType { get; set; }

    /// <summary>No of Crew as per Safe Manning</summary>
    public int? NoOfCrewSafeManning { get; set; }

    /// <summary>Max Passengers Allowed O/B</summary>
    public int? MaxPassengersAllowedOB { get; set; }

    // ═══════════════════════════════════════════
    // TAB 2: DIMENSIONS - Section trên
    // ═══════════════════════════════════════════

    /// <summary>LOA - Length Overall (m)</summary>
    public double? Loa { get; set; }

    /// <summary>Depth moulded (m)</summary>
    public double? DepthMoulded { get; set; }

    /// <summary>H – Max Airdraft (m)</summary>
    public double? HMaxAirdraft { get; set; }

    /// <summary>Parallel body (Ballast) (m)</summary>
    public double? ParallelBodyBallast { get; set; }

    /// <summary>Parallel body (Loaded) (m)</summary>
    public double? ParallelBodyLoaded { get; set; }

    /// <summary>LBP - Length Between Perpendiculars (m)</summary>
    public double? Lbp { get; set; }

    /// <summary>Draft moulded (m)</summary>
    public double? DraftMoulded { get; set; }

    /// <summary>D – Distance (m)</summary>
    public double? DDistance { get; set; }

    /// <summary>Bridge to Aft (m)</summary>
    public double? BridgeToAft { get; set; }

    /// <summary>Bridge to Bow (m)</summary>
    public double? BridgeToBow { get; set; }

    /// <summary>Bow to Bulbous Bow (m)</summary>
    public double? BowToBulbousBow { get; set; }

    /// <summary>Breadth moulded (m)</summary>
    public double? BreadthMoulded { get; set; }

    /// <summary>Draft Scantling (m)</summary>
    public double? DraftScantling { get; set; }

    /// <summary>Airdraft Reduction (Mast Fouled) (m)</summary>
    public double? AirdraftReductionMastFouled { get; set; }

    /// <summary>Light Ship (mt)</summary>
    public double? LightShip { get; set; }

    /// <summary>Draft Full Ballast (m)</summary>
    public double? DraftFullBallast { get; set; }

    /// <summary>Block Coefficient N/A checkbox</summary>
    public bool BlockCoefficientNA { get; set; } = false;

    /// <summary>Block Coefficient</summary>
    public double? BlockCoefficient { get; set; }

    /// <summary>TPC at Summer Draft (mt)</summary>
    public double? TpcAtSummerDraft { get; set; }

    /// <summary>Fresh Water Allowance – FWA (mm)</summary>
    public double? FreshWaterAllowanceFwa { get; set; }

    // --- Gross Tonnage ---
    /// <summary>GT International</summary>
    public double? GrossTonnageInternational { get; set; }

    /// <summary>GT Suez Canal</summary>
    public double? GrossTonnageSuezCanal { get; set; }

    /// <summary>GT Panama Canal</summary>
    public double? GrossTonnagePanamaCanal { get; set; }

    // --- Nett Tonnage ---
    /// <summary>NT International</summary>
    public double? NettTonnageInternational { get; set; }

    /// <summary>NT Suez Canal</summary>
    public double? NettTonnageSuezCanal { get; set; }

    /// <summary>NT Panama Canal</summary>
    public double? NettTonnagePanamaCanal { get; set; }

    // --- For Tankers, LNG and LPG Only ---
    /// <summary>Manifold to waterline (Ballast) (m)</summary>
    public double? ManifoldToWaterlineBallast { get; set; }

    /// <summary>Manifold to waterline (Loaded) (m)</summary>
    public double? ManifoldToWaterlineLoaded { get; set; }

    /// <summary>Deck to manifold (m)</summary>
    public double? DeckToManifold { get; set; }

    /// <summary>Stern to manifold (m)</summary>
    public double? SternToManifold { get; set; }

    /// <summary>Shipside to manifold (m)</summary>
    public double? ShipsideToManifold { get; set; }

    /// <summary>Bow to manifold (m)</summary>
    public double? BowToManifold { get; set; }

    /// <summary>Manifold to keel (m)</summary>
    public double? ManifoldToKeel { get; set; }

    /// <summary>Manifold to Bridge (m)</summary>
    public double? ManifoldToBridge { get; set; }

    /// <summary>Max loading rate – ship (mq)</summary>
    public double? MaxLoadingRateShip { get; set; }

    /// <summary>Number of lines</summary>
    public int? NumberOfLines { get; set; }

    /// <summary>Max allowable pressure (psi)</summary>
    public double? MaxAllowablePressurePsi { get; set; }

    /// <summary>Venting system – ship</summary>
    [MaxLength(200)]
    public string? VentingSystemShip { get; set; }

    // ═══════════════════════════════════════════
    // TAB 3: MACHINERY - Fixed fields
    // ═══════════════════════════════════════════

    // --- Anchor Chain ---
    /// <summary>Port (shackles)</summary>
    public int? AnchorChainPort { get; set; }

    /// <summary>Starboard (shackles)</summary>
    public int? AnchorChainStarboard { get; set; }

    /// <summary>Stern (shackles)</summary>
    public int? AnchorChainStern { get; set; }

    /// <summary>Anchor Chain Stern N/A</summary>
    public bool AnchorChainSternNA { get; set; } = false;

    // --- N/A toggles for dynamic sections ---
    /// <summary>Bowthruster N/A toggle</summary>
    public bool BowthrusterNA { get; set; } = false;

    /// <summary>Sternthruster N/A toggle</summary>
    public bool SternthrusterNA { get; set; } = false;

    /// <summary>Shaft Generator N/A toggle</summary>
    public bool ShaftGeneratorNA { get; set; } = false;

    // --- Harbour / Emergency Generator ---
    /// <summary>Harbour Generator Maker</summary>
    [MaxLength(200)]
    public string? HarbourGeneratorMaker { get; set; }

    /// <summary>Harbour Generator Max Power (kW)</summary>
    public double? HarbourGeneratorMaxPowerKW { get; set; }

    // --- Azimuth Engine ---
    /// <summary>No of Azimuth Eng. FWD</summary>
    public int? AzimuthEngFwdCount { get; set; }

    /// <summary>Azimuth Engine FWD Max Power (kW)</summary>
    public double? AzimuthEngFwdMaxPowerKW { get; set; }

    /// <summary>No of Azimuth Eng. AFT</summary>
    public int? AzimuthEngAftCount { get; set; }

    /// <summary>Azimuth Engine AFT Max Power (kW)</summary>
    public double? AzimuthEngAftMaxPowerKW { get; set; }

    // ═══════════════════════════════════════════
    // TAB 4: SHIPOWNER
    // ═══════════════════════════════════════════

    // --- Shipowner ---
    [MaxLength(300)] public string? ShipownerName { get; set; }
    [MaxLength(300)] public string? ShipownerStreet { get; set; }
    [MaxLength(100)] public string? ShipownerCountry { get; set; }
    [MaxLength(20)] public string? ShipownerZip { get; set; }
    [MaxLength(100)] public string? ShipownerCity { get; set; }
    [MaxLength(50)] public string? ShipownerPhone { get; set; }
    [MaxLength(50)] public string? ShipownerFax { get; set; }
    [MaxLength(50)] public string? ShipownerTlx { get; set; }
    [MaxLength(200)] public string? ShipownerEmail { get; set; }
    [MaxLength(200)] public string? ShipownerContactPerson { get; set; }

    // --- Managing Owner ---
    [MaxLength(300)] public string? ManagingOwnerName { get; set; }
    [MaxLength(300)] public string? ManagingOwnerStreet { get; set; }
    [MaxLength(100)] public string? ManagingOwnerCountry { get; set; }
    [MaxLength(20)] public string? ManagingOwnerZip { get; set; }
    [MaxLength(100)] public string? ManagingOwnerCity { get; set; }
    [MaxLength(50)] public string? ManagingOwnerPhone { get; set; }
    [MaxLength(50)] public string? ManagingOwnerFax { get; set; }
    [MaxLength(50)] public string? ManagingOwnerTlx { get; set; }
    [MaxLength(200)] public string? ManagingOwnerEmail { get; set; }
    [MaxLength(200)] public string? ManagingOwnerContactPerson { get; set; }

    // --- Operator ---
    [MaxLength(300)] public string? OperatorName { get; set; }
    [MaxLength(300)] public string? OperatorStreet { get; set; }
    [MaxLength(100)] public string? OperatorCountry { get; set; }
    [MaxLength(20)] public string? OperatorZip { get; set; }
    [MaxLength(100)] public string? OperatorCity { get; set; }
    [MaxLength(50)] public string? OperatorPhone { get; set; }
    [MaxLength(50)] public string? OperatorFax { get; set; }
    [MaxLength(50)] public string? OperatorTlx { get; set; }
    [MaxLength(200)] public string? OperatorEmail { get; set; }
    [MaxLength(200)] public string? OperatorContactPerson { get; set; }

    // --- Company Security Officer (CSO) ---
    [MaxLength(20)] public string? CsoTitle { get; set; }
    [MaxLength(100)] public string? CsoFirstName { get; set; }
    [MaxLength(100)] public string? CsoLastName { get; set; }
    [MaxLength(300)] public string? CsoStreet { get; set; }
    [MaxLength(100)] public string? CsoCountry { get; set; }
    [MaxLength(20)] public string? CsoZip { get; set; }
    [MaxLength(100)] public string? CsoCity { get; set; }
    [MaxLength(50)] public string? CsoPhone24h { get; set; }
    [MaxLength(50)] public string? CsoFax { get; set; }
    [MaxLength(50)] public string? CsoTlx { get; set; }
    [MaxLength(200)] public string? CsoEmail { get; set; }

    // --- Designated Person Ashore (DPA) ---
    [MaxLength(20)] public string? DpaTitle { get; set; }
    [MaxLength(100)] public string? DpaFirstName { get; set; }
    [MaxLength(100)] public string? DpaLastName { get; set; }
    [MaxLength(300)] public string? DpaStreet { get; set; }
    [MaxLength(100)] public string? DpaCountry { get; set; }
    [MaxLength(20)] public string? DpaZip { get; set; }
    [MaxLength(100)] public string? DpaCity { get; set; }
    [MaxLength(50)] public string? DpaPhone24h { get; set; }
    [MaxLength(50)] public string? DpaFax { get; set; }
    [MaxLength(50)] public string? DpaTlx { get; set; }
    [MaxLength(200)] public string? DpaEmail { get; set; }

    // --- Qualified Individual USA (QI) ---
    [MaxLength(20)] public string? QiUsaTitle { get; set; }
    [MaxLength(100)] public string? QiUsaFirstName { get; set; }
    [MaxLength(100)] public string? QiUsaLastName { get; set; }
    [MaxLength(300)] public string? QiUsaStreet { get; set; }
    [MaxLength(100)] public string? QiUsaCountry { get; set; }
    [MaxLength(20)] public string? QiUsaZip { get; set; }
    [MaxLength(100)] public string? QiUsaCity { get; set; }
    [MaxLength(50)] public string? QiUsaPhone24h { get; set; }
    [MaxLength(50)] public string? QiUsaFax { get; set; }
    [MaxLength(50)] public string? QiUsaTlx { get; set; }
    [MaxLength(200)] public string? QiUsaEmail { get; set; }

    // --- Qualified Individual Panama Canal (QI) ---
    [MaxLength(20)] public string? QiPanamaTitle { get; set; }
    [MaxLength(100)] public string? QiPanamaFirstName { get; set; }
    [MaxLength(100)] public string? QiPanamaLastName { get; set; }
    [MaxLength(300)] public string? QiPanamaStreet { get; set; }
    [MaxLength(100)] public string? QiPanamaCountry { get; set; }
    [MaxLength(20)] public string? QiPanamaZip { get; set; }
    [MaxLength(100)] public string? QiPanamaCity { get; set; }
    [MaxLength(50)] public string? QiPanamaPhone24h { get; set; }
    [MaxLength(50)] public string? QiPanamaFax { get; set; }
    [MaxLength(50)] public string? QiPanamaTlx { get; set; }
    [MaxLength(200)] public string? QiPanamaEmail { get; set; }

    // ═══════════════════════════════════════════
    // TAB 5: CHARTERER
    // ═══════════════════════════════════════════

    // --- Charterer ---
    [MaxLength(300)] public string? ChartererName { get; set; }
    [MaxLength(300)] public string? ChartererStreet { get; set; }
    [MaxLength(100)] public string? ChartererCountry { get; set; }
    [MaxLength(20)] public string? ChartererZip { get; set; }
    [MaxLength(100)] public string? ChartererCity { get; set; }
    [MaxLength(50)] public string? ChartererPhone { get; set; }
    [MaxLength(50)] public string? ChartererFax { get; set; }
    [MaxLength(50)] public string? ChartererTlx { get; set; }
    [MaxLength(200)] public string? ChartererEmail { get; set; }
    [MaxLength(200)] public string? ChartererContactPerson { get; set; }

    // --- Bareboat Charterer ---
    [MaxLength(300)] public string? BareboatChartererName { get; set; }
    [MaxLength(300)] public string? BareboatChartererStreet { get; set; }
    [MaxLength(100)] public string? BareboatChartererCountry { get; set; }
    [MaxLength(20)] public string? BareboatChartererZip { get; set; }
    [MaxLength(100)] public string? BareboatChartererCity { get; set; }
    [MaxLength(50)] public string? BareboatChartererPhone { get; set; }
    [MaxLength(50)] public string? BareboatChartererFax { get; set; }
    [MaxLength(50)] public string? BareboatChartererTlx { get; set; }
    [MaxLength(200)] public string? BareboatChartererEmail { get; set; }
    [MaxLength(200)] public string? BareboatChartererContactPerson { get; set; }

    // ═══════════════════════════════════════════
    // TAB 6: CLASS / FLAG STATE
    // ═══════════════════════════════════════════

    // --- Classification Society ---
    [MaxLength(300)] public string? ClassSocietyName { get; set; }
    [MaxLength(300)] public string? ClassSocietyStreet { get; set; }
    [MaxLength(100)] public string? ClassSocietyCountry { get; set; }
    [MaxLength(20)] public string? ClassSocietyZip { get; set; }
    [MaxLength(100)] public string? ClassSocietyCity { get; set; }
    [MaxLength(50)] public string? ClassSocietyPhone { get; set; }
    [MaxLength(50)] public string? ClassSocietyFax { get; set; }
    [MaxLength(50)] public string? ClassSocietyTlx { get; set; }
    [MaxLength(200)] public string? ClassSocietyEmail { get; set; }
    [MaxLength(200)] public string? ClassSocietyContactPerson { get; set; }

    // --- Flag State ---
    [MaxLength(300)] public string? FlagStateName { get; set; }
    [MaxLength(300)] public string? FlagStateStreet { get; set; }
    [MaxLength(100)] public string? FlagStateCountry { get; set; }
    [MaxLength(20)] public string? FlagStateZip { get; set; }
    [MaxLength(100)] public string? FlagStateCity { get; set; }
    [MaxLength(50)] public string? FlagStatePhone { get; set; }
    [MaxLength(50)] public string? FlagStateFax { get; set; }
    [MaxLength(50)] public string? FlagStateTlx { get; set; }
    [MaxLength(200)] public string? FlagStateEmail { get; set; }
    [MaxLength(200)] public string? FlagStateContactPerson { get; set; }

    // ═══════════════════════════════════════════
    // TAB 7: INSURANCE
    // ═══════════════════════════════════════════

    // --- P&I Club ---
    [MaxLength(300)] public string? PiClubName { get; set; }
    [MaxLength(300)] public string? PiClubStreet { get; set; }
    [MaxLength(100)] public string? PiClubCountry { get; set; }
    [MaxLength(20)] public string? PiClubZip { get; set; }
    [MaxLength(100)] public string? PiClubCity { get; set; }
    [MaxLength(50)] public string? PiClubPhone { get; set; }
    [MaxLength(50)] public string? PiClubFax { get; set; }
    [MaxLength(50)] public string? PiClubTlx { get; set; }
    [MaxLength(200)] public string? PiClubEmail { get; set; }
    [MaxLength(200)] public string? PiClubContactPerson { get; set; }

    // --- H&M Club ---
    [MaxLength(300)] public string? HmClubName { get; set; }
    [MaxLength(300)] public string? HmClubStreet { get; set; }
    [MaxLength(100)] public string? HmClubCountry { get; set; }
    [MaxLength(20)] public string? HmClubZip { get; set; }
    [MaxLength(100)] public string? HmClubCity { get; set; }
    [MaxLength(50)] public string? HmClubPhone { get; set; }
    [MaxLength(50)] public string? HmClubFax { get; set; }
    [MaxLength(50)] public string? HmClubTlx { get; set; }
    [MaxLength(200)] public string? HmClubEmail { get; set; }
    [MaxLength(200)] public string? HmClubContactPerson { get; set; }

    // ═══════════════════════════════════════════
    // TAB 8: RADIO COMMUNICATION EQUIPMENT
    // ═══════════════════════════════════════════

    // --- INMARSAT ---
    [MaxLength(50)] public string? InmarsatTelex1 { get; set; }
    [MaxLength(50)] public string? InmarsatTelex2 { get; set; }
    [MaxLength(50)] public string? InmarsatPhone1 { get; set; }
    [MaxLength(50)] public string? InmarsatPhone2 { get; set; }
    [MaxLength(50)] public string? InmarsatFax1 { get; set; }
    [MaxLength(50)] public string? InmarsatFax2 { get; set; }
    [MaxLength(200)] public string? EmailAddress1 { get; set; }
    [MaxLength(200)] public string? EmailAddress2 { get; set; }
    [MaxLength(50)] public string? GsmPhone { get; set; }

    // --- Sea Areas (Regulation IV/2) ---
    public bool SeaAreaA1 { get; set; } = false;
    public bool SeaAreaA2 { get; set; } = false;
    public bool SeaAreaA3 { get; set; } = false;
    public bool SeaAreaA4 { get; set; } = false;

    // --- Radio Equipment O/B ---
    public bool DscHF { get; set; } = false;
    public bool DscMF { get; set; } = false;
    public bool DscVHF { get; set; } = false;
    public bool RadiotelephoneHF { get; set; } = false;
    public bool RadiotelephoneMF { get; set; } = false;
    public bool RadiotelephoneVHF { get; set; } = false;
    public bool RadiotelegraphHF { get; set; } = false;
    public bool RadiotelegraphMF { get; set; } = false;
    public bool RadiotelegraphVHF { get; set; } = false;
    public bool Navtex { get; set; } = false;
    public bool Ais { get; set; } = false;
    public bool SartTransponder { get; set; } = false;
    public bool Radiotelex { get; set; } = false;
    [MaxLength(500)] public string? OtherRadioEquipment { get; set; }

    // --- EPIRB ---
    [MaxLength(50)] public string? EpirbNumber { get; set; }
    [MaxLength(50)] public string? EpirbOperatingSystem { get; set; }
    [MaxLength(100)] public string? EpirbMaker { get; set; }
    [MaxLength(100)] public string? EpirbModel { get; set; }
    [MaxLength(50)] public string? EpirbFrequency { get; set; }

    // ═══════════════════════════════════════════
    // TAB 9: TANKS & CARGO SPACES
    // ═══════════════════════════════════════════

    // --- Tanks Capacity (100%) ---
    /// <summary>HFO (cbm)</summary>
    public double? HfoCbm { get; set; }

    /// <summary>MDO (cbm)</summary>
    public double? MdoCbm { get; set; }

    /// <summary>Lub. Oil (cbm)</summary>
    public double? LubOilCbm { get; set; }

    /// <summary>Sludge (cbm)</summary>
    public double? SludgeCbm { get; set; }

    /// <summary>Bilge Water (cbm)</summary>
    public double? BilgeWaterCbm { get; set; }

    /// <summary>Sewage (cbm)</summary>
    public double? SewageCbm { get; set; }

    /// <summary>Fresh Water (cbm)</summary>
    public double? FreshWaterCbm { get; set; }

    /// <summary>Ballast Water (cbm)</summary>
    public double? BallastWaterCbm { get; set; }

    /// <summary>No of Ballast Tanks</summary>
    public int? NoOfBallastTanks { get; set; }

    // --- Cargo Capacity ---
    /// <summary>TEU Total</summary>
    public int? TeuTotal { get; set; }

    /// <summary>TEU on Deck</summary>
    public int? TeuOnDeck { get; set; }

    /// <summary>TEU Under Deck</summary>
    public int? TeuUnderDeck { get; set; }

    /// <summary>Grain (cbm)</summary>
    public double? GrainCbm { get; set; }

    /// <summary>Bales (cbm)</summary>
    public double? BalesCbm { get; set; }

    /// <summary>No of Cargo Holds</summary>
    public int? NoOfCargoHolds { get; set; }

    /// <summary>No of Hatches</summary>
    public int? NoOfHatches { get; set; }

    // ═══════════════════════════════════════════
    // METADATA
    // ═══════════════════════════════════════════

    public bool IsSynced { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    // ═══════════════════════════════════════════
    // NAVIGATION PROPERTIES (1-to-Many)
    // ═══════════════════════════════════════════

    [JsonIgnore]
    public virtual ICollection<ShipMainEngine> MainEngines { get; set; } = new List<ShipMainEngine>();

    [JsonIgnore]
    public virtual ICollection<ShipAuxiliaryEngine> AuxiliaryEngines { get; set; } = new List<ShipAuxiliaryEngine>();

    [JsonIgnore]
    public virtual ICollection<ShipPropeller> Propellers { get; set; } = new List<ShipPropeller>();

    [JsonIgnore]
    public virtual ICollection<ShipBowthruster> Bowthrusters { get; set; } = new List<ShipBowthruster>();

    [JsonIgnore]
    public virtual ICollection<ShipSternthruster> Sternthrusters { get; set; } = new List<ShipSternthruster>();

    [JsonIgnore]
    public virtual ICollection<ShipRudder> Rudders { get; set; } = new List<ShipRudder>();

    [JsonIgnore]
    public virtual ICollection<ShipShaftGenerator> ShaftGenerators { get; set; } = new List<ShipShaftGenerator>();

    [JsonIgnore]
    public virtual ICollection<ShipBoiler> Boilers { get; set; } = new List<ShipBoiler>();

    [JsonIgnore]
    public virtual ICollection<ShipLoadLine> LoadLines { get; set; } = new List<ShipLoadLine>();

    [JsonIgnore]
    public virtual ICollection<ShipPilotCardData> PilotCardData { get; set; } = new List<ShipPilotCardData>();
}

// ============================================================
// CHILD TABLES (1-to-Many from ShipData)
// ============================================================

/// <summary>
/// Main Engine(s) - Động cơ chính
/// </summary>
public class ShipMainEngine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>ME Type - e.g. "MAN B&W 11G95ME-"</summary>
    [MaxLength(200)]
    public string? MeType { get; set; }

    /// <summary>ME Fuel Grade: HFO, MDO, MGO, LNG, etc.</summary>
    [MaxLength(50)]
    public string? MeFuelGrade { get; set; }

    /// <summary>ME Power (kW) - HP auto-calculated on frontend</summary>
    public double? MePowerKW { get; set; }

    /// <summary>MCR - Maximum Continuous Rating (kW)</summary>
    public double? McrKW { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Auxiliary Engine(s) - Động cơ phụ
/// </summary>
public class ShipAuxiliaryEngine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>AE Type - e.g. "Yanmar 6EY26L"</summary>
    [MaxLength(200)]
    public string? AeType { get; set; }

    /// <summary>AE Fuel Grade: HFO, MDO, MGO, etc.</summary>
    [MaxLength(50)]
    public string? AeFuelGrade { get; set; }

    /// <summary>AE Power (kW) - HP auto-calculated on frontend</summary>
    public double? AePowerKW { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Propeller(s) - Chân vịt
/// </summary>
public class ShipPropeller
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Type: "(FPP) Fixed Pitch Propeller", "(CPP) Controllable Pitch Propeller"</summary>
    [MaxLength(100)]
    public string? PropellerType { get; set; }

    /// <summary>Number of Blades</summary>
    public int? NumberOfBlades { get; set; }

    /// <summary>Rotation: "Clockwise" / "Counter-Clockwise"</summary>
    [MaxLength(20)]
    public string? Rotation { get; set; }

    /// <summary>Diameter (mm)</summary>
    public double? DiameterMm { get; set; }

    /// <summary>Propeller pitch (Geometric) (mm)</summary>
    public double? PropellerPitchGeometricMm { get; set; }

    /// <summary>Pitch Ratio (auto-calc: Pitch / Diameter)</summary>
    public double? PitchRatio { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Bowthruster(s) - Chân vịt mũi
/// </summary>
public class ShipBowthruster
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Power (kW) - HP auto-calculated on frontend</summary>
    public double? PowerKW { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Sternthruster(s) - Chân vịt lái
/// </summary>
public class ShipSternthruster
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Power (kW) - HP auto-calculated on frontend</summary>
    public double? PowerKW { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Rudder(s) - Bánh lái
/// </summary>
public class ShipRudder
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Rudder Type: "Semi-balanced Spade", "Balanced", "Flap Rudder", etc.</summary>
    [MaxLength(100)]
    public string? RudderType { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Shaft Generator(s) - Máy phát trục
/// </summary>
public class ShipShaftGenerator
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Max Power (kW) - HP auto-calculated on frontend</summary>
    public double? MaxPowerKW { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Boiler(s) - Nồi hơi
/// </summary>
public class ShipBoiler
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Boiler Type - e.g. "Exhaust Gas Boiler"</summary>
    [MaxLength(200)]
    public string? BoilerType { get; set; }

    /// <summary>Model - e.g. "Aalborg XS-2V"</summary>
    [MaxLength(200)]
    public string? Model { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Load Lines Particulars - Dấu mạn khô
/// Types: Tropic Fresh (TF), Tropic (T), Fresh (F), Summer (S), Winter (W), Winter N.Atl. (WNA)
/// </summary>
public class ShipLoadLine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Load Line Type: "TF", "T", "F", "S", "W", "WNA"</summary>
    [Required]
    [MaxLength(50)]
    public string LoadLineType { get; set; } = string.Empty;

    /// <summary>Draft (m)</summary>
    public double? DraftM { get; set; }

    /// <summary>Freeboard (m) – chỉ có ở Summer (S)</summary>
    public double? FreeboardM { get; set; }

    /// <summary>Displacement (mt)</summary>
    public double? DisplacementMt { get; set; }

    /// <summary>Deadweight (mt)</summary>
    public double? DeadweightMt { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}

/// <summary>
/// Data for Pilot Card - Dữ liệu thẻ hoa tiêu
/// Engine Orders: Full Ahead Manoeuvring, Half Ahead, Slow Ahead, Dead Slow Ahead,
///                Dead Slow Astern, Slow Astern, Half Astern, Full Astern
/// </summary>
public class ShipPilotCardData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShipDataId { get; set; }

    /// <summary>Engine Order name</summary>
    [Required]
    [MaxLength(50)]
    public string EngineOrder { get; set; } = string.Empty;

    /// <summary>Main Engine Order RPM (/MIN)</summary>
    public double? MainEngineRPM { get; set; }

    /// <summary>Speed Loaded (KTS)</summary>
    public double? SpeedLoadedKts { get; set; }

    /// <summary>Speed Ballast (KTS)</summary>
    public double? SpeedBallastKts { get; set; }

    public int SortOrder { get; set; } = 0;

    [ForeignKey("ShipDataId")]
    [JsonIgnore]
    public virtual ShipData? ShipData { get; set; }
}
