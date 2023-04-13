using System.IO;

namespace HanumanInstitute.Apps.Tests;

public static class Extensions
{
    /// <summary>
    /// Replaces '/' with '\' on Windows, allowing to generate valid paths for either Windows or Linux.
    /// Input string should separate paths with '/'.
    /// </summary>
    public static string ReplaceDirectorySeparator(this string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
}
