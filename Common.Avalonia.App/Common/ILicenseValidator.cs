namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Validates license keys.
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// Returns whether specified license key is valid.
    /// </summary>
    /// <param name="key">The license key to validate.</param>
    /// <returns>Whether the license key is valid.</returns>
    bool Validate(string key);
}
