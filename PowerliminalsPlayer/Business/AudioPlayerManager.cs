using System;
using System.Collections.Generic;
using System.Linq;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Manages the playback of a list of media files.
/// </summary>
public class AudioPlayerManager : IAudioPlayerManager
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;

    public AudioPlayerManager(IAppPathService appPathService, IFileSystemService fileSystemService)
    {
        _appPath = appPathService.CheckNotNull(nameof(_appPath));
        _fileSystem = fileSystemService.CheckNotNull(nameof(_fileSystem));
    }

    /// <summary>
    /// Returns the list of files currently playing.
    /// </summary>
    public List<string> Files { get; private set; } = new List<string>();
    /// <summary>
    /// Returns the path of the file currently playing.
    /// </summary>
    public string NowPlaying { get; set; } = string.Empty;
    /// <summary>
    /// Occurs when a media file starts to play.
    /// </summary>
    public event EventHandler<PlayingEventArgs>? StartPlaying;
    private readonly Random _random = new Random();

    /// <summary>
    /// Starts the playback of specified list of media files.
    /// </summary>
    /// <param name="list">The list of file paths to play.</param>
    /// <param name="current">If specified, playback will start with specified file.</param>
    public void Play(IEnumerable<string> list, string current)
    {
        Files.Clear();
        Files.AddRange(list);
        NowPlaying = current;
        if (!string.IsNullOrEmpty(current))
        {
            StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
        }
        else
        {
            PlayNext();
        }
    }

    /// <summary>
    /// Starts playing the next media file from the list.
    /// </summary>
    public void PlayNext()
    {
        if (Files.Any())
        {
            var pos = _random.Next(Files.Count);
            if (Files[pos] == NowPlaying)
            {
                pos = _random.Next(Files.Count);
            }

            NowPlaying = Files[pos];
            StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
        }
    }
}