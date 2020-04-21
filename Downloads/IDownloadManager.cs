using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HanumanInstitute.Downloads
{

    /// <summary>
    /// Manages media file downloads.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Gets the list of active and pending downloads.
        /// </summary>
        ObservableCollection<DownloadTaskInfo> DownloadsList { get; }
        /// <summary>
        /// Occurs when a new download task is added to the list.
        /// </summary>
        event EventHandler DownloadAdded;
        /// <summary>
        /// Occurs before performing the muxing operation.
        /// </summary>
        event DownloadTaskEventHandler BeforeMuxing;
        /// <summary>
        /// Occurs when the download is completed.
        /// </summary>
        event DownloadTaskEventHandler Completed;
        /// <summary>
        /// Starts a new download task and add it to the list.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        Task AddDownloadAsync(DownloadTaskInfo downloadTask);
        /// <summary>
        /// Returns the file extensions that can be created by the downloader.
        /// </summary>
        //IList<string> DownloadedExtensions { get; }
        /// <summary>
        /// Returns specified path without its file extension.
        /// </summary>
        /// <param name="path">The path to truncate extension from.</param>
        /// <returns>A file path with no file extension.</returns>
        string GetPathNoExt(string path);
        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        Task<string> GetVideoTitleAsync(Uri url);
        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        Task<VideoInfo> GetDownloadInfoAsync(Uri url);
        /// <summary>
        /// Returns whether specified URL is already being downloaded.
        /// </summary>
        /// <param name="url">The URL to check for.</param>
        /// <returns>Whether the URL is already in the list of downloads.</returns>
        bool IsDownloadDuplicate(Uri url);
        /// <summary>
        /// Returns whether specified file exists and contains data (at least 500KB).
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>Whether the file contains data.</returns>
        bool FileHasContent(string fileName);
    }
}
