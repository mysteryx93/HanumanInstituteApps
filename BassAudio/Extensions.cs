using System.Text.RegularExpressions;
using ManagedBass;

namespace HanumanInstitute.BassAudio;

/// <summary>
/// Provides extension methods for BASS.
/// </summary>
internal static class BassExtensions
{
    /// <summary>
    /// Checks whether a BASS handle is valid and throws an exception if it is 0.
    /// </summary>
    /// <param name="handle">The BASS handle to validate.</param>
    /// <returns>The same value.</returns>
    /// <exception cref="InvalidOperationException">BASS handle is null</exception>
    internal static int Valid(this int handle)
    {
        if (handle == 0)
        {
            throw new BassException(Bass.LastError);
        }

        return handle;
    }

    /// <summary>
    /// Checks whether specified is true and throws an exception if false.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <exception cref="InvalidOperationException">Value is false.</exception>
    internal static void Valid(this bool value)
    {
        if (!value)
        {
            throw new BassException(Bass.LastError);
        }
    }

    /// <summary>
    /// Adds 'param value' to specified command string if value is not empty.
    /// Param and value will be separated by a space unless param ends with '='.
    /// </summary>
    /// <param name="command">The string to append to.</param>
    /// <param name="param">The parameter name.</param>
    /// <param name="value">The value to append if not empty.</param>
    /// <returns>The command with appended parameter value.</returns>
    internal static string AddTag(this string command, string param, string? value) =>
         string.IsNullOrEmpty(value) ? command : 
            "{0} {1}{2}{3}".FormatInvariant(command, param, param.EndsWith('=') ? "" : " ", EncodeParameterArgument(value));
    
    /// <summary>
    /// Encodes an argument for passing into a program.
    /// </summary>
    /// <param name="original">The value that should be received by the program.</param>
    /// <returns>The value which needs to be passed to the program for the original value 
    /// to come through.</returns>
    private static string EncodeParameterArgument(string original)
    {
        // from: https://stackoverflow.com/a/12364234/3960200
        if (!original.HasValue()) { return string.Empty; }
        
        var value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
        value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"", RegexOptions.Singleline);
        return value;
    }
}
