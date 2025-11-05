namespace MaritimeEdge.Services;

/// <summary>
/// Helper class for maritime unit conversions
/// </summary>
public static class UnitConversionHelper
{
    #region Length / Distance
    
    /// <summary>
    /// Convert meters to nautical miles
    /// </summary>
    public static double MetersToNauticalMiles(double meters) => meters / 1852.0;
    
    /// <summary>
    /// Convert nautical miles to meters
    /// </summary>
    public static double NauticalMilesToMeters(double nauticalMiles) => nauticalMiles * 1852.0;
    
    #endregion

    #region Speed
    
    /// <summary>
    /// Convert meters per second to knots
    /// 1 knot = 1.94384 m/s
    /// </summary>
    public static double MpsToKnots(double mps) => mps * 1.94384;
    
    /// <summary>
    /// Convert knots to meters per second
    /// </summary>
    public static double KnotsToMps(double knots) => knots / 1.94384;
    
    #endregion

    #region Angle
    
    /// <summary>
    /// Convert radians to degrees
    /// </summary>
    public static double RadiansToDegrees(double radians) => radians * (180.0 / Math.PI);
    
    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    public static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
    
    /// <summary>
    /// Normalize angle to 0-360 range
    /// </summary>
    public static double NormalizeAngle(double degrees)
    {
        degrees %= 360;
        if (degrees < 0) degrees += 360;
        return degrees;
    }
    
    #endregion

    #region Temperature
    
    /// <summary>
    /// Convert Kelvin to Celsius
    /// Returns 0 if input is invalid (≤0)
    /// </summary>
    public static double KelvinToCelsius(double kelvin) 
        => kelvin > 0 ? kelvin - 273.15 : 0;
    
    /// <summary>
    /// Convert Celsius to Kelvin
    /// </summary>
    public static double CelsiusToKelvin(double celsius) => celsius + 273.15;
    
    /// <summary>
    /// Convert Celsius to Fahrenheit
    /// </summary>
    public static double CelsiusToFahrenheit(double celsius) 
        => (celsius * 9.0 / 5.0) + 32.0;
    
    /// <summary>
    /// Convert Fahrenheit to Celsius
    /// </summary>
    public static double FahrenheitToCelsius(double fahrenheit) 
        => (fahrenheit - 32.0) * 5.0 / 9.0;
    
    #endregion

    #region Pressure
    
    /// <summary>
    /// Convert Pascals to Bar
    /// 1 bar = 100,000 Pa
    /// </summary>
    public static double PascalToBar(double pascal) => pascal / 100000.0;
    
    /// <summary>
    /// Convert Bar to Pascals
    /// </summary>
    public static double BarToPascal(double bar) => bar * 100000.0;
    
    /// <summary>
    /// Convert Pascals to hectoPascals (hPa) / millibars (mbar)
    /// 1 hPa = 100 Pa
    /// </summary>
    public static double PascalToHPa(double pascal) => pascal / 100.0;
    
    /// <summary>
    /// Convert hPa to Pascals
    /// </summary>
    public static double HPaToPascal(double hPa) => hPa * 100.0;
    
    #endregion

    #region Volume / Flow Rate
    
    /// <summary>
    /// Convert cubic meters per second to liters per hour
    /// 1 m³ = 1000 liters
    /// </summary>
    public static double CubicMetersPerSecondToLitersPerHour(double m3s) 
        => m3s * 3600000.0;
    
    /// <summary>
    /// Convert liters per hour to cubic meters per second
    /// </summary>
    public static double LitersPerHourToCubicMetersPerSecond(double lph) 
        => lph / 3600000.0;
    
    /// <summary>
    /// Convert cubic meters to liters
    /// </summary>
    public static double CubicMetersToLiters(double m3) => m3 * 1000.0;
    
    /// <summary>
    /// Convert liters to cubic meters
    /// </summary>
    public static double LitersToCubicMeters(double liters) => liters / 1000.0;
    
    #endregion

    #region Percentage / Ratio
    
    /// <summary>
    /// Convert decimal ratio to percentage (0-100)
    /// </summary>
    public static double RatioToPercentage(double ratio) => ratio * 100.0;
    
    /// <summary>
    /// Convert percentage to decimal ratio (0-1)
    /// </summary>
    public static double PercentageToRatio(double percentage) => percentage / 100.0;
    
    #endregion

    #region Time
    
    /// <summary>
    /// Convert seconds to hours
    /// </summary>
    public static double SecondsToHours(double seconds) => seconds / 3600.0;
    
    /// <summary>
    /// Convert hours to seconds
    /// </summary>
    public static double HoursToSeconds(double hours) => hours * 3600.0;
    
    #endregion
}
