using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Manages extra aspects of media file downloads specific to this application such as file path management and database entries.
    /// </summary>
    public class DownloadService : IDownloadService
    {
        private readonly IDownloadManager _download;
        private readonly IAppSettingsProvider _settings;
        private readonly IFileSystemService _fileSystem;
        private readonly IAppPathService _appPath;
        private readonly IDefaultMediaPathService _defaultPath;
        private readonly IMediaInfoReader _mediaInfo;
        private readonly IMediaMuxer _mediaMuxer;
        private readonly IMediaRepository _repository;

        public DownloadService(IDownloadManager downloadManager, IAppSettingsProvider settings, IFileSystemService fileSystemService, IAppPathService appPathService, IDefaultMediaPathService defaultMediaPath, IMediaInfoReader mediaInfo, IMediaMuxer mediaMuxer, IMediaRepository mediaRepository)
        {
            _download = downloadManager;
            _settings = settings;
            _fileSystem = fileSystemService;
            _appPath = appPathService;
            _defaultPath = defaultMediaPath;
            _mediaInfo = mediaInfo;
            _mediaMuxer = mediaMuxer;
            _repository = mediaRepository;
        }

        private readonly List<DownloadMediaInfo> _list = new List<DownloadMediaInfo>();

        /// <summary>
        /// Returns the information of an active download with specified mediaId, or null.
        /// </summary>
        /// <param name="mediaId">The ID of the media to look for.</param>
        /// <returns>The download information of the requested media.</returns>
        public DownloadMediaInfo GetActiveDownloadByMediaId(Guid mediaId)
        {
            var searchStatus = new[] { DownloadStatus.Waiting, DownloadStatus.Initializing, DownloadStatus.Downloading, DownloadStatus.Finalizing };
            return _list.FirstOrDefault(d => (d as DownloadMediaInfo).Media.MediaId == mediaId &&
                searchStatus.Contains(d.Download.Status)) as DownloadMediaInfo;
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task<DownloadStatus> DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback)
        {
            return await DownloadVideoAsync(video, queuePos, callback, true, true, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task<DownloadStatus> DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback, bool downloadVideo, bool downloadAudio, DownloadOptions? options)
        {
            video.CheckNotNull(nameof(video));
            video.DownloadUrl.CheckNotNullOrEmpty(nameof(video.DownloadUrl));

            // Store in the Temp folder.
            var fileName = _defaultPath.GetDefaultFileName(video.Artist, video.Title, (MediaType)video.MediaTypeId, "");
            var destination = _fileSystem.Path.Combine(_appPath.DownloaderTempPath, fileName);
            _fileSystem.EnsureDirectoryExists(destination);

            DownloadMediaInfo? downloadInfo = null;
            var result = await _download.DownloadAsync(new Uri(video.DownloadUrl), destination, downloadVideo, downloadAudio, options,
                (sender, e) =>
                {
                    e.Download.Muxing += DownloadMuxing;
                    downloadInfo = new DownloadMediaInfo(video, e.Download)
                    {
                        Destination = destination,
                        Title = fileName,
                        QueuePos = queuePos
                    };
                    _list.Add(downloadInfo);

                    callback?.Invoke(this, e);
                }
            ).ConfigureAwait(false);

            if (result == DownloadStatus.Success)
            {
                DownloadCompleted(downloadInfo!);
            }
            return result;
        }


        /// <summary>
        /// Allows upgrading audio or video of existing local file when only one of the two streams has been downloaded.
        /// </summary>
        /// <param name="e">Contains information about the download task.</param>
        private void DownloadMuxing(object sender, DownloadTaskEventArgs e)
        {
            // Check if we need to upgrade an existing local media file.
            var downloadTask = (DownloadMediaInfo)e.Download;
            var srcFile = downloadTask.Media.FileName != null ? _settings.Value.NaturalGroundingFolder + downloadTask.Media.FileName : null;
            var videoFile = e.Download.Files.FirstOrDefault(f => f.HasVideo);
            var audioFile = e.Download.Files.FirstOrDefault(f => f.HasAudio);
            if (srcFile != null && downloadTask.Media.FileName != null && _fileSystem.File.Exists(srcFile) && (videoFile == null || audioFile == null))
            {
                // Upgrade audio or video.
                var mInfo = _mediaInfo.GetFileInfo(srcFile);
                var videoFormat = videoFile != null ? _fileSystem.Path.GetExtension(videoFile.Destination).TrimStart('.') : mInfo.VideoStream?.Format;
                var audioFormat = audioFile != null ? _fileSystem.Path.GetExtension(audioFile.Destination).TrimStart('.') : mInfo.AudioStream?.Format;
                var videoDestExt = GetFinalExtension(videoFormat, audioFormat);
                e.Download.Destination = _fileSystem.GetPathWithoutExtension(e.Download.Destination) + videoDestExt;
                var result = _mediaMuxer.Muxe(videoFile?.Destination ?? srcFile, audioFile?.Destination ?? srcFile, e.Download.Destination);

                // Cleanup.
                if (result == CompletionStatus.Success && _fileSystem.File.Exists(srcFile))
                {
                    _fileSystem.MoveToRecycleBin(srcFile);
                }
                else
                {
                    e.Download.Fail();
                }
            }
        }

        /// <summary>
        /// When download is completed, move it into a new location and add it into the database.
        /// </summary>
        /// <param name="e">Contains information about the download task.</param>
        private void DownloadCompleted(DownloadMediaInfo downloadInfo)
        {
            var video = downloadInfo.Media;
            var src = downloadInfo.Destination;
            var srcExt = _fileSystem.Path.GetExtension(src);
            src = _fileSystem.GetPathWithoutExtension(src);

            // Get final file name.
            var dst = _defaultPath.GetDefaultFileName(video.Artist, video.Title, video.MediaTypeId);
            _fileSystem.EnsureDirectoryExists(_settings.Value.NaturalGroundingFolder + dst);
            video.FileName = dst + srcExt;

            // Move file and overwrite.
            var dstFile = _settings.Value.NaturalGroundingFolder + video.FileName;
            if (_fileSystem.File.Exists(dstFile))
            {
                _fileSystem.MoveToRecycleBin(dstFile);
            }

            _fileSystem.File.Move(src + srcExt, dstFile);

            // Add to database
            var existingData = _repository.GetMediaById(video.MediaId);
            if (existingData != null)
            {
                // Edit video info.
                existingData.FileName = video.FileName;
                existingData.Length = null;
                existingData.Height = null;
                _repository.Save();
            }
            else
            {
                // Add new video info.
                _repository.AddMedia(video);
                _repository.Save();
            }
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetFinalExtension(string? video, string? audio)
        {
            video = video?.ToUpperInvariant();
            audio = audio?.ToUpperInvariant();
            if (video == "VP8" || video == "VP9")
            {
                video = "webm";
            }

            if (video == "H264")
            {
                video = "mp4";
            }

            if (audio == "OPUS" || audio == "VORBIS")
            {
                audio = "webm";
            }

            if (audio == "AAC")
            {
                audio = "mp4";
            }

            if (video == "WEBM" && audio == "WEBM")
            {
                return ".webm";
            }
            else if (video == "MP4" && audio == "MP4")
            {
                return ".mp4";
            }
            else
            {
                return ".mkv";
            }
        }
    }
}
