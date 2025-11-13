using MaritimeEdge.DTOs;

namespace MaritimeEdge.Services;

/// <summary>
/// Maritime Business Validation Service
/// Validates maritime data according to industry standards and common sense
/// </summary>
public class MaritimeValidationService
{
    /// <summary>
    /// Validate noon report data for maritime accuracy
    /// </summary>
    public static (bool IsValid, List<string> Errors, List<string> Warnings) ValidateNoonReport(CreateNoonReportDto dto)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. Position Validation
        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            // Check for Null Island (0,0) - likely data error
            if (Math.Abs(dto.Latitude.Value) < 0.01 && Math.Abs(dto.Longitude.Value) < 0.01)
            {
                errors.Add("Invalid position: Coordinates near (0,0) 'Null Island' - likely GPS error");
            }

            // Check if coordinates are within valid range (already done by DTO validation)
            // But add business logic check
            if (dto.Latitude.Value == 0 && dto.Longitude.Value == 0)
            {
                errors.Add("Invalid position: Exactly (0,0) is highly suspicious");
            }
        }

        // 2. Speed vs Distance Validation
        if (dto.SpeedOverGround.HasValue && dto.DistanceTraveled.HasValue)
        {
            // If speed is zero but distance traveled, something is wrong
            if (dto.SpeedOverGround.Value < 0.1 && dto.DistanceTraveled.Value > 10)
            {
                errors.Add("Inconsistent data: Speed near zero but significant distance traveled");
            }

            // If distance is zero but speed is high, warning
            if (dto.DistanceTraveled.Value < 1 && dto.SpeedOverGround.Value > 5)
            {
                warnings.Add("Unusual: High speed reported but minimal distance traveled");
            }

            // Check if average speed matches distance
            var expectedDistance = dto.SpeedOverGround.Value * 24; // 24 hours
            var deviation = Math.Abs(expectedDistance - dto.DistanceTraveled.Value) / expectedDistance;
            if (deviation > 0.3) // 30% deviation
            {
                warnings.Add($"Speed/distance mismatch: Expected {expectedDistance:F1} nm based on SOG, but reported {dto.DistanceTraveled.Value:F1} nm");
            }
        }

        // 3. Fuel Consumption Validation
        if (dto.FuelOilConsumed.HasValue)
        {
            // Negative consumption
            if (dto.FuelOilConsumed.Value < 0)
            {
                errors.Add("Invalid fuel consumption: Cannot be negative");
            }

            // Unreasonably high consumption (>100 MT/day for most vessels)
            if (dto.FuelOilConsumed.Value > 100)
            {
                warnings.Add($"Unusually high fuel consumption: {dto.FuelOilConsumed.Value:F1} MT/day");
            }

            // Zero consumption while vessel is moving
            if (dto.FuelOilConsumed.Value == 0 && dto.SpeedOverGround.HasValue && dto.SpeedOverGround.Value > 1)
            {
                errors.Add("Invalid: Zero fuel consumption while vessel is underway");
            }

            // Check fuel consumption rate
            if (dto.SpeedOverGround.HasValue && dto.SpeedOverGround.Value > 0)
            {
                var consumptionRate = dto.FuelOilConsumed.Value / dto.SpeedOverGround.Value;
                if (consumptionRate > 10) // Very inefficient
                {
                    warnings.Add($"High specific fuel consumption: {consumptionRate:F2} MT per knot");
                }
            }
        }

        // 4. ROB (Remaining On Board) Validation
        if (dto.FuelOilROB.HasValue && dto.FuelOilConsumed.HasValue)
        {
            // ROB cannot be negative
            if (dto.FuelOilROB.Value < 0)
            {
                errors.Add("Invalid: Fuel ROB cannot be negative");
            }

            // ROB suspiciously low with high consumption
            if (dto.FuelOilROB.Value < dto.FuelOilConsumed.Value * 2)
            {
                warnings.Add($"Low fuel ROB: Only {dto.FuelOilROB.Value:F1} MT remaining, consumed {dto.FuelOilConsumed.Value:F1} MT today");
            }
        }

        // 5. Time Validation - Noon report should be near noon
        var reportHour = dto.ReportDate.ToLocalTime().Hour;
        if (reportHour < 10 || reportHour > 14)
        {
            warnings.Add($"Noon report time unusual: Reported at {reportHour:D2}:00 local time (expected 11:00-13:00)");
        }

        // 6. Weather Data Consistency
        if (dto.WindSpeed.HasValue && dto.SeaState != null)
        {
            // High wind but calm sea - unusual
            if (dto.WindSpeed.Value > 30 && dto.SeaState.Contains("Calm"))
            {
                warnings.Add("Inconsistent weather: High wind speed but calm sea state");
            }
        }

        // 7. Temperature Checks
        if (dto.AirTemperature.HasValue)
        {
            if (dto.AirTemperature.Value < -50 || dto.AirTemperature.Value > 50)
            {
                warnings.Add($"Extreme air temperature: {dto.AirTemperature.Value}°C");
            }
        }

        if (dto.SeaTemperature.HasValue)
        {
            if (dto.SeaTemperature.Value < -2 || dto.SeaTemperature.Value > 35)
            {
                warnings.Add($"Unusual sea temperature: {dto.SeaTemperature.Value}°C");
            }
        }

        // 8. Barometric Pressure
        if (dto.BarometricPressure.HasValue)
        {
            if (dto.BarometricPressure.Value < 950)
            {
                warnings.Add($"Very low pressure: {dto.BarometricPressure.Value} hPa - possible typhoon/hurricane");
            }
            if (dto.BarometricPressure.Value > 1040)
            {
                warnings.Add($"Very high pressure: {dto.BarometricPressure.Value} hPa");
            }
        }

        return (errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// Validate departure report data
    /// </summary>
    public static (bool IsValid, List<string> Errors, List<string> Warnings) ValidateDepartureReport(CreateDepartureReportDto dto)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. Port validation
        if (string.IsNullOrWhiteSpace(dto.PortName))
        {
            errors.Add("Port name is required for departure report");
        }

        // 2. Destination port validation
        if (string.IsNullOrWhiteSpace(dto.DestinationPort))
        {
            warnings.Add("Destination port not specified");
        }

        // 3. Same port departure and arrival
        if (!string.IsNullOrWhiteSpace(dto.PortName) && !string.IsNullOrWhiteSpace(dto.DestinationPort))
        {
            if (dto.PortName.Equals(dto.DestinationPort, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add("Departure and destination ports are the same");
            }
        }

        // 4. ETA validation
        if (dto.EstimatedArrival.HasValue)
        {
            var voyageDuration = dto.EstimatedArrival.Value - dto.DepartureDateTime;
            if (voyageDuration.TotalHours < 0)
            {
                errors.Add("ETA cannot be before departure time");
            }
            if (voyageDuration.TotalDays > 60)
            {
                warnings.Add($"Very long voyage: {voyageDuration.TotalDays:F1} days to destination");
            }
        }

        // 5. Draft validation
        if (dto.DraftForward.HasValue && dto.DraftAft.HasValue)
        {
            if (dto.DraftForward.Value < 0 || dto.DraftAft.Value < 0)
            {
                errors.Add("Draft cannot be negative");
            }

            var trim = Math.Abs(dto.DraftAft.Value - dto.DraftForward.Value);
            if (trim > 3)
            {
                warnings.Add($"Large trim: {trim:F2} meters difference between forward and aft drafts");
            }
        }

        // 6. ROB validation
        if (dto.FuelOilROB.HasValue && dto.FuelOilROB.Value < 10)
        {
            warnings.Add($"Very low fuel ROB at departure: {dto.FuelOilROB.Value:F1} MT");
        }

        return (errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// Validate bunker report data
    /// </summary>
    public static (bool IsValid, List<string> Errors, List<string> Warnings) ValidateBunkerReport(CreateBunkerReportDto dto)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. Quantity validation
        if (dto.QuantityReceived <= 0)
        {
            errors.Add("Bunker quantity must be positive");
        }
        if (dto.QuantityReceived > 5000)
        {
            warnings.Add($"Very large bunker quantity: {dto.QuantityReceived:F0} MT");
        }

        // 2. Fuel type validation
        if (string.IsNullOrWhiteSpace(dto.FuelType))
        {
            errors.Add("Fuel type is required for bunker report");
        }
        else
        {
            var validFuelTypes = new[] { "HFO", "VLSFO", "ULSFO", "MGO", "MDO", "LNG", "LSMGO" };
            if (!validFuelTypes.Contains(dto.FuelType.ToUpper()))
            {
                warnings.Add($"Unusual fuel type: {dto.FuelType} (Common types: HFO, VLSFO, MGO)");
            }
        }

        // 3. Sulfur content validation (MARPOL Annex VI) - note: DTO uses 'SulphurContent'
        if (dto.SulphurContent.HasValue)
        {
            if (dto.SulphurContent.Value < 0)
            {
                errors.Add("Sulphur content cannot be negative");
            }
            
            // MARPOL 2020: Global sulfur limit 0.5%
            if (dto.SulphurContent.Value > 0.5)
            {
                errors.Add($"Sulphur content {dto.SulphurContent.Value}% exceeds MARPOL 2020 global limit of 0.50% (unless in ECA or using scrubber)");
            }

            // ECA limit is 0.1%
            if (dto.SulphurContent.Value > 0.1)
            {
                warnings.Add($"Sulphur content {dto.SulphurContent.Value}% exceeds ECA limit of 0.10%");
            }
        }

        // 4. Density validation
        if (dto.Density.HasValue)
        {
            // Marine fuel density typically 0.8-1.0 kg/L
            if (dto.Density.Value < 0.7 || dto.Density.Value > 1.1)
            {
                warnings.Add($"Unusual fuel density: {dto.Density.Value} kg/L (typical range: 0.8-1.0)");
            }
        }

        // 5. Viscosity validation
        if (dto.Viscosity.HasValue)
        {
            // Typical range for marine fuel: 10-700 cSt
            if (dto.Viscosity.Value < 1 || dto.Viscosity.Value > 1000)
            {
                warnings.Add($"Unusual viscosity: {dto.Viscosity.Value} cSt");
            }
        }

        // 6. ROB validation
        if (dto.ROBBefore.HasValue && dto.ROBAfter.HasValue)
        {
            var expectedROBAfter = dto.ROBBefore.Value + dto.QuantityReceived;
            if (Math.Abs(dto.ROBAfter.Value - expectedROBAfter) > 10)
            {
                warnings.Add($"ROB mismatch: Expected {expectedROBAfter:F1} MT after bunkering (Before: {dto.ROBBefore.Value:F1} + Received: {dto.QuantityReceived:F1}), but reported {dto.ROBAfter.Value:F1} MT");
            }
        }

        return (errors.Count == 0, errors, warnings);
    }

    /// <summary>
    /// Validate position report data
    /// </summary>
    public static (bool IsValid, List<string> Errors, List<string> Warnings) ValidatePositionReport(CreatePositionReportDto dto)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. Position Validation - Critical for safety
        // Check for Null Island (0,0) - likely GPS error
        if (Math.Abs(dto.Latitude) < 0.01 && Math.Abs(dto.Longitude) < 0.01)
        {
            errors.Add("Invalid position: Coordinates near (0,0) 'Null Island' - likely GPS error");
        }

        if (dto.Latitude == 0 && dto.Longitude == 0)
        {
            errors.Add("Exact coordinates (0,0) detected - GPS malfunction");
        }

        // 2. Speed validation
        if (dto.SpeedOverGround.HasValue)
        {
            if (dto.SpeedOverGround.Value < 0 || dto.SpeedOverGround.Value > 50)
            {
                errors.Add($"Invalid speed: {dto.SpeedOverGround.Value} knots (must be 0-50)");
            }

            if (dto.SpeedOverGround.Value > 30)
            {
                warnings.Add($"High speed reported: {dto.SpeedOverGround.Value} knots - verify accuracy");
            }
        }

        // 3. Course validation
        if (dto.CourseOverGround.HasValue)
        {
            if (dto.CourseOverGround.Value < 0 || dto.CourseOverGround.Value > 360)
            {
                errors.Add($"Invalid course: {dto.CourseOverGround.Value}° (must be 0-360)");
            }
        }

        // 4. Report reason validation
        var validReasons = new[] { "ROUTINE", "EMERGENCY", "REQUEST", "SPECIAL_AREA", "PIRACY_AREA", "ECA_ZONE" };
        if (!validReasons.Contains(dto.ReportReason?.ToUpper()))
        {
            warnings.Add($"Unusual report reason: {dto.ReportReason}. Standard reasons: {string.Join(", ", validReasons)}");
        }

        // 5. Future date check
        if (dto.ReportDateTime > DateTime.UtcNow.AddHours(1))
        {
            errors.Add("Report datetime cannot be in the future");
        }

        // 6. ETA validation
        if (dto.ETA.HasValue)
        {
            if (dto.ETA.Value < dto.ReportDateTime)
            {
                errors.Add("ETA cannot be before position report time");
            }

            if (dto.ETA.Value < DateTime.UtcNow)
            {
                warnings.Add("ETA is in the past - may need update");
            }
        }

        return (errors.Count == 0, errors, warnings);
    }
}
