namespace MaritimeEdge.DTOs;

// =============================================
// Abstract Log DTOs
// =============================================

// ── Response DTOs ──

public class AbstractLogVoyageDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public string? IMONumber { get; set; }
    public string? MasterName { get; set; }
    public string? ChiefEngineerName { get; set; }
    public DateTime? ReportDate { get; set; }
    public DateTime? DateOfLastDocking { get; set; }
    public string? PropellerPitch { get; set; }

    // Time Summary
    public DateTime? CommencementTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public double? GrandTotalHours { get; set; }

    // Fuel ROB Reconciliation
    public double? FoRobPrevious { get; set; }
    public double? FoReceived { get; set; }
    public double? FoConsumedTotal { get; set; }
    public double? FoRobCurrent { get; set; }
    public double? DoRobPrevious { get; set; }
    public double? DoReceived { get; set; }
    public double? DoConsumedTotal { get; set; }
    public double? DoRobCurrent { get; set; }
    public double? CylOilRobPrevious { get; set; }
    public double? CylOilReceived { get; set; }
    public double? CylOilConsumed { get; set; }
    public double? CylOilRobCurrent { get; set; }
    public double? SysOilRobPrevious { get; set; }
    public double? SysOilReceived { get; set; }
    public double? SysOilConsumed { get; set; }
    public double? SysOilRobCurrent { get; set; }
    public double? GenOilRobPrevious { get; set; }
    public double? GenOilReceived { get; set; }
    public double? GenOilConsumed { get; set; }
    public double? GenOilRobCurrent { get; set; }
    public double? FwRobPrevious { get; set; }
    public double? FwProduced { get; set; }
    public double? FwConsumed { get; set; }
    public double? FwRobCurrent { get; set; }

    public string? Remarks { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? MasterSignedAt { get; set; }
    public string? ChiefEngineerSignature { get; set; }
    public DateTime? ChiefEngineerSignedAt { get; set; }
    public string Status { get; set; } = "DRAFT";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Nested data
    public List<AbstractLogLegDto> Legs { get; set; } = new();
}

public class AbstractLogLegDto
{
    public Guid Id { get; set; }
    public Guid AbstractLogVoyageId { get; set; }
    public int LegNumber { get; set; }
    public int Sequence { get; set; }
    public string? LegLabel { get; set; }

    public string? DeparturePort { get; set; }
    public DateTime? DepartureTime { get; set; }
    public double? DepartureDraftFore { get; set; }
    public double? DepartureDraftAft { get; set; }
    public double? DepartureDraftMean { get; set; }

    public string? ArrivalPort { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public double? ArrivalDraftFore { get; set; }
    public double? ArrivalDraftAft { get; set; }
    public double? ArrivalDraftMean { get; set; }

    public double? HoursPropelling { get; set; }
    public double? HoursUnderWay { get; set; }
    public double? HoursDrifting { get; set; }
    public double? HoursAnchor { get; set; }
    public double? HoursPort { get; set; }

    public double? DistanceProp { get; set; }
    public double? DistanceLog { get; set; }
    public double? DistanceOG { get; set; }
    public double? SpeedLog { get; set; }
    public double? SpeedOG { get; set; }
    public double? SlipPercent { get; set; }
    public double? ShaftRevolutions { get; set; }

    // Cargo
    public string? CargoType { get; set; }
    public double? CargoQuantity { get; set; }
    public string? LoadCondition { get; set; }

    // FOC totals
    public double? MeFocHsfo { get; set; }
    public double? MeFocVlsfo { get; set; }
    public double? MeFocLsmgo { get; set; }
    public double? DeFocHsfo { get; set; }
    public double? DeFocVlsfo { get; set; }
    public double? DeFocLsmgo { get; set; }
    public double? BoilerFocHsfo { get; set; }
    public double? BoilerFocVlsfo { get; set; }
    public double? BoilerFocLsmgo { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<AbstractLogDailyEntryDto> DailyEntries { get; set; } = new();
}

public class AbstractLogDailyEntryDto
{
    public Guid Id { get; set; }
    public Guid AbstractLogLegId { get; set; }
    public int DayNumber { get; set; }
    public DateTime EntryDate { get; set; }
    public double? NoonLatitude { get; set; }
    public double? NoonLongitude { get; set; }

    public string? WindDirectionTrue { get; set; }
    public string? WindDirectionRelative { get; set; }
    public int? WindForceBeaufort { get; set; }
    public string? SeaState { get; set; }

    public double? HoursUnderWay { get; set; }
    public double? HoursPropelling { get; set; }
    public double? HoursDrifting { get; set; }
    public double? HoursAnchor { get; set; }
    public double? HoursPort { get; set; }
    public double? TimeZoneChange { get; set; }

    public double? DistanceEngine { get; set; }
    public double? DistanceLog { get; set; }
    public double? DistanceOG { get; set; }
    public double? SpeedLog { get; set; }
    public double? SpeedOG { get; set; }
    public double? SlipPercent { get; set; }
    public double? AvgRPM { get; set; }

    // FOC During Propelling
    public double? HpMeHsfo { get; set; }
    public double? HpMeVlsfo { get; set; }
    public double? HpMeLsmgo { get; set; }
    public double? HpDeHsfo { get; set; }
    public double? HpDeVlsfo { get; set; }
    public double? HpDeLsmgo { get; set; }
    public double? HpBoilerHsfo { get; set; }
    public double? HpBoilerVlsfo { get; set; }
    public double? HpBoilerLsmgo { get; set; }

    // FOC During Detention/Drifting
    public double? DtMeHsfo { get; set; }
    public double? DtMeVlsfo { get; set; }
    public double? DtMeLsmgo { get; set; }
    public double? DtDeHsfo { get; set; }
    public double? DtDeVlsfo { get; set; }
    public double? DtDeLsmgo { get; set; }
    public double? DtBoilerHsfo { get; set; }
    public double? DtBoilerVlsfo { get; set; }
    public double? DtBoilerLsmgo { get; set; }

    // FOC In Port
    public double? PortMeHsfo { get; set; }
    public double? PortMeVlsfo { get; set; }
    public double? PortMeLsmgo { get; set; }
    public double? PortDeHsfo { get; set; }
    public double? PortDeVlsfo { get; set; }
    public double? PortDeLsmgo { get; set; }
    public double? PortBoilerHsfo { get; set; }
    public double? PortBoilerVlsfo { get; set; }
    public double? PortBoilerLsmgo { get; set; }

    // Lub Oil & Fresh Water (daily)
    public double? CylOilConsumed { get; set; }
    public double? SysOilConsumed { get; set; }
    public double? FwProduced { get; set; }
    public double? FwConsumed { get; set; }

    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ── Create/Update DTOs ──

public class CreateAbstractLogDto
{
    public Guid VoyageId { get; set; }
}

public class UpdateAbstractLogVoyageDto
{
    public string? MasterName { get; set; }
    public string? ChiefEngineerName { get; set; }
    public DateTime? ReportDate { get; set; }
    public DateTime? DateOfLastDocking { get; set; }
    public string? PropellerPitch { get; set; }

    // Fuel ROB Reconciliation
    public double? FoRobPrevious { get; set; }
    public double? FoReceived { get; set; }
    public double? FoConsumedTotal { get; set; }
    public double? FoRobCurrent { get; set; }
    public double? DoRobPrevious { get; set; }
    public double? DoReceived { get; set; }
    public double? DoConsumedTotal { get; set; }
    public double? DoRobCurrent { get; set; }
    public double? CylOilRobPrevious { get; set; }
    public double? CylOilReceived { get; set; }
    public double? CylOilConsumed { get; set; }
    public double? CylOilRobCurrent { get; set; }
    public double? SysOilRobPrevious { get; set; }
    public double? SysOilReceived { get; set; }
    public double? SysOilConsumed { get; set; }
    public double? SysOilRobCurrent { get; set; }
    public double? GenOilRobPrevious { get; set; }
    public double? GenOilReceived { get; set; }
    public double? GenOilConsumed { get; set; }
    public double? GenOilRobCurrent { get; set; }
    public double? FwRobPrevious { get; set; }
    public double? FwProduced { get; set; }
    public double? FwConsumed { get; set; }
    public double? FwRobCurrent { get; set; }

    public string? Remarks { get; set; }
    public string? Status { get; set; }
    public string? MasterSignature { get; set; }
    public string? ChiefEngineerSignature { get; set; }
}

public class CreateAbstractLogLegDto
{
    /// <summary>Optional departure port for initialization</summary>
    public string? DeparturePort { get; set; }
    public DateTime? DepartureTime { get; set; }
    public double? DepartureDraftFore { get; set; }
    public double? DepartureDraftAft { get; set; }
    public string? ArrivalPort { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public double? ArrivalDraftFore { get; set; }
    public double? ArrivalDraftAft { get; set; }
    public string? CargoType { get; set; }
    public double? CargoQuantity { get; set; }
    public string? LoadCondition { get; set; }
}

public class UpdateAbstractLogLegDto
{
    public string? DeparturePort { get; set; }
    public DateTime? DepartureTime { get; set; }
    public double? DepartureDraftFore { get; set; }
    public double? DepartureDraftAft { get; set; }
    public double? DepartureDraftMean { get; set; }
    public string? ArrivalPort { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public double? ArrivalDraftFore { get; set; }
    public double? ArrivalDraftAft { get; set; }
    public double? ArrivalDraftMean { get; set; }

    public double? HoursPropelling { get; set; }
    public double? HoursUnderWay { get; set; }
    public double? HoursDrifting { get; set; }
    public double? HoursAnchor { get; set; }
    public double? HoursPort { get; set; }

    public double? DistanceProp { get; set; }
    public double? DistanceLog { get; set; }
    public double? DistanceOG { get; set; }
    public double? SpeedLog { get; set; }
    public double? SpeedOG { get; set; }
    public double? SlipPercent { get; set; }
    public double? ShaftRevolutions { get; set; }

    public double? MeFocHsfo { get; set; }
    public double? MeFocVlsfo { get; set; }
    public double? MeFocLsmgo { get; set; }
    public double? DeFocHsfo { get; set; }
    public double? DeFocVlsfo { get; set; }
    public double? DeFocLsmgo { get; set; }
    public double? BoilerFocHsfo { get; set; }
    public double? BoilerFocVlsfo { get; set; }
    public double? BoilerFocLsmgo { get; set; }
    public string? CargoType { get; set; }
    public double? CargoQuantity { get; set; }
    public string? LoadCondition { get; set; }
}

public class CreateAbstractLogDailyEntryDto
{
    public DateTime EntryDate { get; set; }
    public double? NoonLatitude { get; set; }
    public double? NoonLongitude { get; set; }

    public string? WindDirectionTrue { get; set; }
    public string? WindDirectionRelative { get; set; }
    public int? WindForceBeaufort { get; set; }
    public string? SeaState { get; set; }

    public double? HoursUnderWay { get; set; }
    public double? HoursPropelling { get; set; }
    public double? HoursDrifting { get; set; }
    public double? HoursAnchor { get; set; }
    public double? HoursPort { get; set; }
    public double? TimeZoneChange { get; set; }

    public double? DistanceEngine { get; set; }
    public double? DistanceLog { get; set; }
    public double? DistanceOG { get; set; }
    public double? SpeedLog { get; set; }
    public double? SpeedOG { get; set; }
    public double? SlipPercent { get; set; }
    public double? AvgRPM { get; set; }

    // FOC Propelling
    public double? HpMeHsfo { get; set; }
    public double? HpMeVlsfo { get; set; }
    public double? HpMeLsmgo { get; set; }
    public double? HpDeHsfo { get; set; }
    public double? HpDeVlsfo { get; set; }
    public double? HpDeLsmgo { get; set; }
    public double? HpBoilerHsfo { get; set; }
    public double? HpBoilerVlsfo { get; set; }
    public double? HpBoilerLsmgo { get; set; }

    // FOC Detention/Drifting
    public double? DtMeHsfo { get; set; }
    public double? DtMeVlsfo { get; set; }
    public double? DtMeLsmgo { get; set; }
    public double? DtDeHsfo { get; set; }
    public double? DtDeVlsfo { get; set; }
    public double? DtDeLsmgo { get; set; }
    public double? DtBoilerHsfo { get; set; }
    public double? DtBoilerVlsfo { get; set; }
    public double? DtBoilerLsmgo { get; set; }

    // FOC In Port
    public double? PortMeHsfo { get; set; }
    public double? PortMeVlsfo { get; set; }
    public double? PortMeLsmgo { get; set; }
    public double? PortDeHsfo { get; set; }
    public double? PortDeVlsfo { get; set; }
    public double? PortDeLsmgo { get; set; }
    public double? PortBoilerHsfo { get; set; }
    public double? PortBoilerVlsfo { get; set; }
    public double? PortBoilerLsmgo { get; set; }

    // Lub Oil & FW daily
    public double? CylOilConsumed { get; set; }
    public double? SysOilConsumed { get; set; }
    public double? FwProduced { get; set; }
    public double? FwConsumed { get; set; }

    public string? Remarks { get; set; }
}

public class UpdateAbstractLogDailyEntryDto : CreateAbstractLogDailyEntryDto
{
    // Inherits all fields — all nullable so partial update works
}

/// <summary>List item (lightweight, no nested legs)</summary>
public class AbstractLogListItemDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public DateTime? CommencementTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public double? GrandTotalHours { get; set; }
    public string Status { get; set; } = "DRAFT";
    public int LegCount { get; set; }
    public int DailyEntryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
