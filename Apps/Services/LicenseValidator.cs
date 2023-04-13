using AppSoftware.LicenceEngine.KeyVerification;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class LicenseValidator : ILicenseValidator
{
    private readonly IAppInfo _appInfo;

    /// <summary>
    /// Initializes a new instance of the LicenseValidator class.
    /// </summary>
    public LicenseValidator(IAppInfo appInfo)
    {
        _appInfo = appInfo;
    }

    /// <inheritdoc />
    public bool Validate(string key)
    {
        var verifier = new PkvKeyVerifier();
        return verifier.VerifyKey(key, _appInfo.KeyByteSets, _appInfo.TotalKeyByteSets, Array.Empty<string>()) == PkvKeyVerificationResult.KeyIsValid;
    }
}
