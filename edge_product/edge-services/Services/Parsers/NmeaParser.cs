using System.Globalization;
using System.Text.RegularExpressions;
using MaritimeEdge.Models;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Parsers;

/// <summary>
/// NMEA 0183 Sentence Parser
/// Supports: GGA, RMC, VTG, GLL, HDT, ROT, DPT, DBT, MTW, MWV, MWD, VHW, VDM, VDO
/// </summary>
public partial class NmeaParser
{
    private readonly ILogger<NmeaParser> _logger;

    public NmeaParser(ILogger<NmeaParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse NMEA sentence and return appropriate data object
    /// </summary>
    public object? ParseSentence(string sentence)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return null;

            sentence = sentence.Trim();

            // Validate checksum
            if (!ValidateChecksum(sentence))
            {
                _logger.LogWarning("Invalid checksum for sentence: {Sentence}", sentence);
                return null;
            }

            // Extract sentence type (e.g., "GPGGA" -> "GGA")
            var match = SentenceTypeRegex().Match(sentence);
            if (!match.Success)
                return null;

            var sentenceType = match.Groups[1].Value;

            // Parse based on sentence type
            return sentenceType switch
            {
                "GGA" => ParseGGA(sentence),
                "RMC" => ParseRMC(sentence),
                "VTG" => ParseVTG(sentence),
                // "GLL" => ParseGLL(sentence), // TODO: Implement
                "HDT" => ParseHDT(sentence),
                "ROT" => ParseROT(sentence),
                "DPT" => ParseDPT(sentence),
                "DBT" => ParseDBT(sentence),
                "MTW" => ParseMTW(sentence),
                "MWV" => ParseMWV(sentence),
                "MWD" => ParseMWD(sentence),
                "VHW" => ParseVHW(sentence),
                "VDM" or "VDO" => ParseAIS(sentence),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing NMEA sentence: {Sentence}", sentence);
            return null;
        }
    }

    /// <summary>
    /// Parse GGA - Global Positioning System Fix Data
    /// Example: $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
    /// </summary>
    private PositionData? ParseGGA(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 15)
            return null;

        var time = ParseTime(fields[1]);
        var latitude = ParseLatitude(fields[2], fields[3]);
        var longitude = ParseLongitude(fields[4], fields[5]);
        var fixQuality = ParseInt(fields[6]) ?? 0;
        var satellites = ParseInt(fields[7]) ?? 0;
        var hdop = ParseDouble(fields[8]);
        var altitude = ParseDouble(fields[9]);

        if (!latitude.HasValue || !longitude.HasValue)
            return null;

        return new PositionData
        {
            Timestamp = time ?? DateTime.UtcNow,
            Latitude = latitude.Value,
            Longitude = longitude.Value,
            Altitude = altitude,
            FixQuality = fixQuality,
            SatellitesUsed = satellites,
            Hdop = hdop,
            Source = "GPS"
        };
    }

    /// <summary>
    /// Parse RMC - Recommended Minimum Navigation Information
    /// Example: $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
    /// </summary>
    private PositionData? ParseRMC(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 12)
            return null;

        var time = ParseTime(fields[1]);
        var status = fields[2]; // A=valid, V=invalid
        var latitude = ParseLatitude(fields[3], fields[4]);
        var longitude = ParseLongitude(fields[5], fields[6]);
        var speedKnots = ParseDouble(fields[7]);
        var courseTrue = ParseDouble(fields[8]);
        var date = ParseDate(fields[9]);
        var magneticVar = ParseDouble(fields[10]);

        if (status != "A" || !latitude.HasValue || !longitude.HasValue)
            return null;

        var timestamp = CombineDateTime(date, time);

        return new PositionData
        {
            Timestamp = timestamp,
            Latitude = latitude.Value,
            Longitude = longitude.Value,
            SpeedOverGround = speedKnots,
            CourseOverGround = courseTrue,
            MagneticVariation = magneticVar,
            FixQuality = 1, // RMC implies valid fix
            Source = "GPS"
        };
    }

    /// <summary>
    /// Parse VTG - Track Made Good and Ground Speed
    /// Example: $GPVTG,054.7,T,034.4,M,005.5,N,010.2,K*48
    /// </summary>
    private PositionData? ParseVTG(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 9)
            return null;

        var courseTrue = ParseDouble(fields[1]);
        var courseMagnetic = ParseDouble(fields[3]);
        var speedKnots = ParseDouble(fields[5]);
        var speedKmh = ParseDouble(fields[7]);

        return new PositionData
        {
            Timestamp = DateTime.UtcNow,
            CourseOverGround = courseTrue,
            SpeedOverGround = speedKnots,
            Source = "GPS"
        };
    }

    /// <summary>
    /// Parse HDT - Heading True
    /// Example: $HCHDT,274.07,T*03
    /// </summary>
    private NavigationData? ParseHDT(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 3)
            return null;

        var headingTrue = ParseDouble(fields[1]);

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            HeadingTrue = headingTrue
        };
    }

    /// <summary>
    /// Parse ROT - Rate of Turn
    /// Example: $TIROT,12.5,A*25
    /// </summary>
    private NavigationData? ParseROT(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 3)
            return null;

        var rot = ParseDouble(fields[1]);
        var status = fields[2].Split('*')[0]; // A=valid, V=invalid

        if (status != "A")
            return null;

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            RateOfTurn = rot
        };
    }

    /// <summary>
    /// Parse DPT - Depth
    /// Example: $SDDPT,12.5,0.0,*5E
    /// </summary>
    private NavigationData? ParseDPT(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 3)
            return null;

        var depth = ParseDouble(fields[1]);

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            Depth = depth
        };
    }

    /// <summary>
    /// Parse DBT - Depth Below Transducer
    /// Example: $SDDBT,12.5,f,3.8,M,2.1,F*31
    /// </summary>
    private NavigationData? ParseDBT(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 6)
            return null;

        var depthMeters = ParseDouble(fields[3]);

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            Depth = depthMeters
        };
    }

    /// <summary>
    /// Parse MTW - Water Temperature
    /// Example: $IIMTW,15.5,C*14
    /// </summary>
    private EnvironmentalData? ParseMTW(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 3)
            return null;

        var temp = ParseDouble(fields[1]);

        return new EnvironmentalData
        {
            Timestamp = DateTime.UtcNow,
            SeaTemperature = temp
        };
    }

    /// <summary>
    /// Parse MWV - Wind Speed and Angle
    /// Example: $IIMWV,045,R,12.5,N,A*2E
    /// </summary>
    private NavigationData? ParseMWV(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 6)
            return null;

        var windAngle = ParseDouble(fields[1]);
        var reference = fields[2]; // R=Relative, T=True
        var windSpeed = ParseDouble(fields[3]);
        var speedUnit = fields[4]; // N=knots, M=m/s, K=km/h
        var status = fields[5].Split('*')[0]; // A=valid

        if (status != "A")
            return null;

        // Convert to knots if needed
        if (speedUnit == "M" && windSpeed.HasValue)
            windSpeed = windSpeed.Value * 1.94384; // m/s to knots
        else if (speedUnit == "K" && windSpeed.HasValue)
            windSpeed = windSpeed.Value * 0.539957; // km/h to knots

        if (reference == "R")
        {
            return new NavigationData
            {
                Timestamp = DateTime.UtcNow,
                WindSpeedRelative = windSpeed,
                WindDirectionRelative = windAngle
            };
        }
        else
        {
            return new NavigationData
            {
                Timestamp = DateTime.UtcNow,
                WindSpeedTrue = windSpeed,
                WindDirectionTrue = windAngle
            };
        }
    }

    /// <summary>
    /// Parse MWD - Wind Direction and Speed
    /// Example: $IIMWD,045,T,032,M,12.5,N,6.4,M*5A
    /// </summary>
    private NavigationData? ParseMWD(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 9)
            return null;

        var windDirTrue = ParseDouble(fields[1]);
        var windSpeedKnots = ParseDouble(fields[5]);

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            WindDirectionTrue = windDirTrue,
            WindSpeedTrue = windSpeedKnots
        };
    }

    /// <summary>
    /// Parse VHW - Water Speed and Heading
    /// Example: $IIVHW,274,T,270,M,12.5,N,23.2,K*51
    /// </summary>
    private NavigationData? ParseVHW(string sentence)
    {
        var fields = sentence.Split(',');
        if (fields.Length < 9)
            return null;

        var headingTrue = ParseDouble(fields[1]);
        var headingMagnetic = ParseDouble(fields[3]);
        var speedKnots = ParseDouble(fields[5]);

        return new NavigationData
        {
            Timestamp = DateTime.UtcNow,
            HeadingTrue = headingTrue,
            HeadingMagnetic = headingMagnetic,
            SpeedThroughWater = speedKnots
        };
    }

    /// <summary>
    /// Parse AIS VDM/VDO messages (simplified - full AIS parsing is complex)
    /// Example: !AIVDM,1,1,,A,13aEOK?P00PD2wVMdLDRhgvL289?,0*26
    /// </summary>
    private AisData? ParseAIS(string sentence)
    {
        // This is a simplified parser - full AIS decoding requires bit manipulation
        // For production, use a dedicated AIS library
        var fields = sentence.Split(',');
        if (fields.Length < 7)
            return null;

        // Extract payload
        var payload = fields[5];
        
        // AIS decoding would happen here
        // For now, just store raw data
        _logger.LogDebug("AIS message received: {Payload}", payload);

        return null; // TODO: Implement full AIS decoder
    }

    // ========== HELPER METHODS ==========

    [GeneratedRegex(@"\$[A-Z]{2}([A-Z]{3}),")]
    private static partial Regex SentenceTypeRegex();

    /// <summary>
    /// Validate NMEA checksum
    /// </summary>
    private static bool ValidateChecksum(string sentence)
    {
        if (!sentence.Contains('*'))
            return false;

        var parts = sentence.Split('*');
        if (parts.Length != 2)
            return false;

        var data = parts[0].TrimStart('$', '!');
        var checksumStr = parts[1].Substring(0, Math.Min(2, parts[1].Length));

        if (!int.TryParse(checksumStr, NumberStyles.HexNumber, null, out var checksum))
            return false;

        var calculated = data.Aggregate(0, (current, c) => current ^ c);

        return calculated == checksum;
    }

    private static DateTime? ParseTime(string timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr) || timeStr.Length < 6)
            return null;

        var hour = int.Parse(timeStr.Substring(0, 2));
        var minute = int.Parse(timeStr.Substring(2, 2));
        var second = int.Parse(timeStr.Substring(4, 2));

        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, now.Day, hour, minute, second, DateTimeKind.Utc);
    }

    private static DateTime? ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr) || dateStr.Length != 6)
            return null;

        var day = int.Parse(dateStr.Substring(0, 2));
        var month = int.Parse(dateStr.Substring(2, 2));
        var year = 2000 + int.Parse(dateStr.Substring(4, 2));

        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime CombineDateTime(DateTime? date, DateTime? time)
    {
        if (date.HasValue && time.HasValue)
        {
            return new DateTime(
                date.Value.Year, date.Value.Month, date.Value.Day,
                time.Value.Hour, time.Value.Minute, time.Value.Second,
                DateTimeKind.Utc
            );
        }

        return time ?? date ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Parse latitude from NMEA format (ddmm.mmmm,N/S)
    /// </summary>
    private static double? ParseLatitude(string latStr, string nsIndicator)
    {
        if (string.IsNullOrWhiteSpace(latStr) || latStr.Length < 4)
            return null;

        var degrees = int.Parse(latStr.Substring(0, 2));
        var minutes = double.Parse(latStr.Substring(2), CultureInfo.InvariantCulture);

        var decimalDegrees = degrees + (minutes / 60.0);

        if (nsIndicator == "S")
            decimalDegrees = -decimalDegrees;

        return decimalDegrees;
    }

    /// <summary>
    /// Parse longitude from NMEA format (dddmm.mmmm,E/W)
    /// </summary>
    private static double? ParseLongitude(string lonStr, string ewIndicator)
    {
        if (string.IsNullOrWhiteSpace(lonStr) || lonStr.Length < 5)
            return null;

        var degrees = int.Parse(lonStr.Substring(0, 3));
        var minutes = double.Parse(lonStr.Substring(3), CultureInfo.InvariantCulture);

        var decimalDegrees = degrees + (minutes / 60.0);

        if (ewIndicator == "W")
            decimalDegrees = -decimalDegrees;

        return decimalDegrees;
    }

    private static double? ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) 
            ? result 
            : null;
    }

    private static int? ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }
}
