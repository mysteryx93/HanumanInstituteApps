using System.Security.Cryptography;
using System.Text;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.Services;

/// <inheritdoc />
public class RandomGenerator : IRandomGenerator
{
    // This implementation solves the problems with multi-threading explained here
    // http://web.archive.org/web/20160326010328/http://blogs.msdn.com/b/pfxteam/archive/2009/02/19/9434171.aspx

    /// <summary>
    /// The seeds will be obtained in a more secure way using crypto, ensuring multiple seeds remain unique even if created at the same time.
    /// </summary>
    private static readonly RandomNumberGenerator s_global = RandomNumberGenerator.Create();
    private Random? _local;

    /// <summary>
    /// Returns an instance of Random in a thread-safe way.
    /// </summary>
    private Random GetInstance() => _local ??= new Random(GetSecureInt32());

    /// <inheritdoc />
    public int GetInt() => GetInstance().Next();

    /// <inheritdoc />
    public int GetInt(int maxValue) => GetInstance().Next(maxValue);

    /// <inheritdoc />
    public int GetInt(int minValue, int maxValue) => GetInstance().Next(minValue, maxValue);

    /// <inheritdoc />
    public int GetDigits(int digits) => GetInstance().Next(10 ^ digits);

    /// <inheritdoc />
    public double GetDouble() => GetInstance().NextDouble();

    /// <inheritdoc />
    public byte[] GetSecureBytes(int length)
    {
        var buffer = new byte[length];
        s_global.GetBytes(buffer);
        return buffer;
    }

    /// <inheritdoc />
    public byte GetSecureByte() => GetSecureBytes(1)[0];

    /// <inheritdoc />
    public int GetSecureInt32() => BitConverter.ToInt32(GetSecureBytes(4));

    /// <inheritdoc />
    public long GetSecureInt64() => BitConverter.ToInt64(GetSecureBytes(8));

    /// <inheritdoc />
    public float GetSecureSingle() => BitConverter.ToSingle(GetSecureBytes(4));

    /// <inheritdoc />
    public double GetSecureDouble() => BitConverter.ToDouble(GetSecureBytes(8));

    /// <inheritdoc />
    public string GetSecureToken(int length)
    {
        var data = GetSecureBytes(4 * length);
        var result = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % s_chars.Length;
            result.Append(s_chars[idx]);
        }
        return result.ToString();
    }

    private static readonly char[] s_chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
}
