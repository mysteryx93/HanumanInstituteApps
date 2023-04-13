namespace HanumanInstitute.Services;

/// <summary>
/// Generates random numbers in a thread-safe way.
/// </summary>
/// <remarks>This class should be registered as Singleton.</remarks>
public interface IRandomGenerator
{
    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than MaxValue.</returns>
    int GetInt();
    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue. However, if maxValue equals 0, maxValue is returned.</returns>
    int GetInt(int maxValue);
    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
    int GetInt(int minValue, int maxValue);
    /// <summary>
    /// Returns a non-negative random integer with specified amount of digits.
    /// </summary>
    /// <param name="digits">The amount of digits to generate.</param>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and has up to specified number of digits.</returns>
    int GetDigits(int digits);
    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    double GetDouble();
    /// <summary>
    /// Returns an array of bytes with a cryptographically strong sequence of random values.
    /// </summary>
    /// <param name="length">The length of the byte array to generate and return.</param>
    /// <returns>An array of cryptographically strong random bytes.</returns>
    byte[] GetSecureBytes(int length);
    /// <summary>
    /// Returns a cryptographically strong random byte value.
    /// </summary>
    public byte GetSecureByte();
    /// <summary>
    /// Returns a cryptographically strong random Int32 value.
    /// </summary>
    int GetSecureInt32();
    /// <summary>
    /// Returns a cryptographically strong random Int64 value.
    /// </summary>
    long GetSecureInt64();
    /// <summary>
    /// Returns a cryptographically strong random Single value.
    /// </summary>
    float GetSecureSingle();
    /// <summary>
    /// Returns a cryptographically strong random Double value.
    /// </summary>
    double GetSecureDouble();
    /// <summary>
    /// Returns a cryptographically strong token of alphanumerical values of specified length.
    /// </summary>
    /// <param name="length">The length of the token to generate.</param>
    /// <returns>A cryptographically strong random token.</returns>
    string GetSecureToken(int length);
}
