using System;
using System.Threading.Tasks;
using HanumanInstitute.Downloads;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Manages extra aspects of media file downloads specific to this application such as file path management and database entries.
    /// </summary>
    public interface IDownloadService
    {
        /// <summary>
        /// Returns the information of an active download with specified mediaId, or null.
        /// </summary>
        /// <param name="mediaId">The ID of the media to look for.</param>
        /// <returns>The download information of the requested media.</returns>
        DownloadMediaInfo GetActiveDownloadByMediaId(Guid mediaId);
        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        Task<DownloadStatus> DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback);
        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        Task<DownloadStatus> DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback, bool downloadVideo, bool downloadAudio, DownloadOptions options);
        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        string GetFinalExtension(string video, string audio);
    }
}
