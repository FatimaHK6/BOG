namespace BOG.DbModel.Constants;

/// <summary>
/// Constants for ApplyingMethod lookup table IDs.
/// </summary>
public static class ApplyingMethodIds
{
    /// <summary>
    /// Request submitted in person at the court (من خلال المحكمة)
    /// </summary>
    public const int ThroughCourt = 1;

    /// <summary>
    /// Request submitted through the online portal (من خلال البوابة)
    /// </summary>
    public const int ThroughPortal = 2;
}
