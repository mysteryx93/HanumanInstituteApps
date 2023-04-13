using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using HanumanInstitute.Services.Properties;

namespace HanumanInstitute.Services;

/// <inheritdoc />
public class WindowsApiService : IWindowsApiService
{
    private const int ResultOk = 0;

    /// <inheritdoc />
    public nint GetForegroundWindow() => NativeMethods.GetForegroundWindow();

    /// <inheritdoc />
    public bool SetForegroundWindow(nint hWnd) => NativeMethods.SetForegroundWindow(hWnd);

    /// <inheritdoc />
    public string GetShortPathName(string path)
    {
        const int MaxPathLength = 255;
        var result = new StringBuilder(MaxPathLength);
        ValidateHResult(NativeMethods.GetShortPathName(path, result, MaxPathLength), "GetShortPathName");
        return result.ToString();
    }

    /// <inheritdoc />
    public void ShFileOperation(ApiFileOperationType fileOperation, string path, ApiFileOperationFlags flags)
    {
        var fs = new NativeMethods.ShFileOpStruct
        {
            Func = ApiFileOperationType.Delete,
            From = path + '\0' + '\0',
            Flags = flags
        };
        ValidateHResult(NativeMethods.SHFileOperation(ref fs), "SHFileOperation");
    }

    private static void ValidateHResult(int hresult, string apiName)
    {
        if (hresult != ResultOk)
        {
            throw new ExternalException(
                string.Format(CultureInfo.CurrentCulture, Resources.ApiInvocationError, apiName, hresult),
                hresult);
        }
    }

    /// <summary>
    /// Provides PInvoke calls for Windows API.
    /// </summary>
    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder shortPath, int shortPathLength);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern int SHFileOperation(ref ShFileOpStruct fileOp);

        /// <summary>
        /// SHFileOperation COM data structure.
        /// </summary>
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We must use fields instead of properties for API calls.")]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        internal struct ShFileOpStruct
        {
            public nint Hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public ApiFileOperationType Func;
            public string From;
            public string To;
            public ApiFileOperationFlags Flags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool AnyOperationsAborted;
            public nint NameMappings;
            public string ProgressTitle;
        }
    }
}

/// <summary>
/// File Operation Function Type for SHFileOperation
/// </summary>
[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "For Windows API")]
public enum ApiFileOperationType : uint
{
    /// <summary>
    /// Move the objects
    /// </summary>
    Move = 0x0001,
    /// <summary>
    /// Copy the objects
    /// </summary>
    Copy = 0x0002,
    /// <summary>
    /// Delete (or recycle) the objects
    /// </summary>
    Delete = 0x0003,
    /// <summary>
    /// Rename the object(s)
    /// </summary>
    Rename = 0x0004,
}

/// <summary>
/// Possible flags for the SHFileOperation method.
/// </summary>
[Flags]
[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "For Windows API")]
public enum ApiFileOperationFlags : ushort
{
    /// <summary>
    /// Do not show a dialog during the process
    /// </summary>
    Silent = 0x0004,
    /// <summary>
    /// Do not ask the user to confirm selection
    /// </summary>
    NoConfirmation = 0x0010,
    /// <summary>
    /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
    /// </summary>
    AllowUndo = 0x0040,
    /// <summary>
    /// Do not show the names of the files or folders that are being recycled.
    /// </summary>
    SimpleProgress = 0x0100,
    /// <summary>
    /// Suppress errors, if any occur during the process.
    /// </summary>
    NoErrorUi = 0x0400,
    /// <summary>
    /// Warn if files are too big to fit in the recycle bin and will need
    /// to be deleted completely.
    /// </summary>
    WantNukeWarning = 0x4000,
}
