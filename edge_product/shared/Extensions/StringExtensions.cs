namespace Maritime.Shared.Extensions;

/// <summary>
/// String comparison helpers to enforce case-insensitive, culture-invariant comparisons.
/// Use these instead of == for enum-like string values to prevent case sensitivity bugs.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Case-insensitive, ordinal string comparison (culture-invariant)
    /// Use for status codes, enum-like values, and business logic strings.
    /// </summary>
    /// <example>
    /// ✅ CORRECT: if (status.IsSameAs("ONBOARD")) {...}
    /// ❌ WRONG:   if (status == "ONBOARD") {...}  // fails on "onboard"
    /// </example>
    public static bool IsSameAs(this string? value, string? compare)
    {
        if (value == null && compare == null) return true;
        if (value == null || compare == null) return false;
        
        return value.Equals(compare, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if string starts with another string (case-insensitive, ordinal)
    /// </summary>
    /// <example>
    /// ✅ CORRECT: if (code.StartsWithOrdinal("STCW")) {...}
    /// </example>
    public static bool StartsWithOrdinal(this string? value, string? prefix)
    {
        if (value == null || prefix == null) return false;
        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if string contains another string (case-insensitive, ordinal)
    /// </summary>
    public static bool ContainsOrdinal(this string? value, string? substring)
    {
        if (value == null || substring == null) return false;
        return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }
}
