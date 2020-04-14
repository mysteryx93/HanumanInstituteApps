using System;
using DataAccess;
using EmergenceGuardian.DownloadManager;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    public interface IDownloadMediaFactory {
        /// <summary>
        /// Creates a new IDownloadMediaInfo initialized with specified values.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <param name="title">The title of the downloaded file.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to download the audio stream.</param>
        /// <param name="callback">The function to be called once download is completed.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The new IDownloadMediaInfo instance.</returns>
        DownloadMediaInfo Create(Media media, int queuePos, string url, string destination, string title, bool downloadVideo, bool downloadAudio, DownloadTaskEventHandler callback, DownloadOptions options);
    }

    /// <summary>
    /// Replaces the default factory to produce instances of the derived class DownloadMediaInfo instead of DownloadTaskInfo.
    /// </summary>
    public class DownloadMediaFactory : IDownloadMediaFactory {
        /// <summary>
        /// Creates a new IDownloadMediaInfo initialized with specified values.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <param name="title">The title of the downloaded file.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to download the audio stream.</param>
        /// <param name="callback">The function to be called once download is completed.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The new IDownloadMediaInfo instance.</returns>
        public DownloadMediaInfo Create(Media media, int queuePos, string url, string destination, string title, bool downloadVideo, bool downloadAudio, DownloadTaskEventHandler callback, DownloadOptions options) {
            return new DownloadMediaInfo(url, destination, title, downloadVideo, downloadAudio, callback, options) {
                Media = media,
                QueuePos = queuePos
            };
        }
    }
}
