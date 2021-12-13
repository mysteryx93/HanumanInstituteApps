using System;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Exposes the Windows API.
/// </summary>
public interface IWindowsApiService
{
    /// <summary>
    /// Returns the hWnd of the window that is in the foreground.
    /// </summary>
    IntPtr GetForegroundWindow();
    /// <summary>
    /// Moves specified window into the foreground.
    /// </summary>
    /// <param name="hWnd">The hWnd of the window to show.</param>
    bool SetForegroundWindow(IntPtr hWnd);
    /// <summary>
    /// Returns the DOS-format short path name for specified path.
    /// </summary>
    /// <param name="path">The path to get the short name for.</param>
    /// <returns>The short path name.</returns>
    string GetShortPathName(string path);
    /// <summary>
    /// Sends a file operation command via the Windows API.
    /// </summary>
    /// <param name="fileOperation">The file operation to perform.</param>
    /// <param name="path">The path on which to perform the operation.</param>
    /// <param name="flags">Additional options, FOF_ALLOWUNDO will be automatically added.</param>
    void SHFileOperation(ApiFileOperationType fileOperation, string path, ApiFileOperationFlags flags);
}