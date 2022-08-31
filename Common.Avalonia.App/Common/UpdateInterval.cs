namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Specifies when to automatically check for updates.
/// </summary>
public enum UpdateInterval
{
    /// <summary>
    /// Every day.
    /// </summary>
    Daily,
    /// <summary>
    /// Twice per week.
    /// </summary>
    Biweekly,
    /// <summary>
    /// Every week.
    /// </summary>
    Weekly,
    /// <summary>
    /// Twice per month.
    /// </summary>
    Bimonthly,
    /// <summary>
    /// Every month.
    /// </summary>
    Monthly,
    /// <summary>
    /// Do not check for updates automatically.
    /// </summary>
    Never
}
