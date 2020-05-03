using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using HanumanInstitute.CommonServices.Properties;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Exposes the Windows API.
    /// </summary>
    public class WindowsApiService : IWindowsApiService
    {
        public WindowsApiService() { }

        protected const int ResultOK = 0;

        /// <summary>
        /// Returns the hWnd of the window that is in the foreground.
        /// </summary>
        public IntPtr GetForegroundWindow() => NativeMethods.GetForegroundWindow();

        /// <summary>
        /// Moves specified window into the foreground.
        /// </summary>
        /// <param name="hWnd">The hWnd of the window to show.</param>
        public bool SetForegroundWindow(IntPtr hWnd) => NativeMethods.SetForegroundWindow(hWnd);

        /// <summary>
        /// Returns the DOS-format short path name for specified path.
        /// </summary>
        /// <param name="path">The path to get the short name for.</param>
        /// <exception cref="ExternalException">Occurs if the API call fails.</exception>
        /// <returns>The short path name.</returns>
        public string GetShortPathName(string path)
        {
            const int MaxPathLenth = 255;
            var result = new StringBuilder(MaxPathLenth);
            ValidateHResult(NativeMethods.GetShortPathName(path, result, MaxPathLenth), "GetShortPathName");
            return result.ToString();
        }

        /// <summary>
        /// Sends a file operation command via the Windows API.
        /// </summary>
        /// <param name="fileOperation">The file operation to perform.</param>
        /// <param name="path">The path on which to perform the operation.</param>
        /// <param name="flags">Additional options.</param>
        /// <exception cref="ExternalException">Occurs if the API call fails.</exception>
        public void SHFileOperation(ApiFileOperationType fileOperation, string path, ApiFileOperationFlags flags)
        {
            var fs = new NativeMethods.SHFILEOPSTRUCT
            {
                Func = ApiFileOperationType.Delete,
                From = path + '\0' + '\0',
                Flags = flags
            };
            ValidateHResult(NativeMethods.SHFileOperation(ref fs), "SHFileOperation");
        }

        protected static void ValidateHResult(int hresult, string apiName)
        {
            if (hresult != ResultOK)
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
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            internal static extern int GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder shortPath, int shortPathLength);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            internal static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);

            /// <summary>
            /// SHFILEOPSTRUCT for SHFileOperation from COM
            /// </summary>
            [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We must use fields instead of properties for API calls.")]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct SHFILEOPSTRUCT
            {
                public IntPtr Hwnd;
                [MarshalAs(UnmanagedType.U4)]
                public ApiFileOperationType Func;
                public string From;
                public string To;
                public ApiFileOperationFlags Flags;
                [MarshalAs(UnmanagedType.Bool)]
                public bool AnyOperationsAborted;
                public IntPtr NameMappings;
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
        /// Surpress errors, if any occur during the process.
        /// </summary>
        NoErrorUI = 0x0400,
        /// <summary>
        /// Warn if files are too big to fit in the recycle bin and will need
        /// to be deleted completely.
        /// </summary>
        WantNukeWarning = 0x4000,
    }
}
