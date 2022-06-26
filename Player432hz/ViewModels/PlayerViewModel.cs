using System.Windows.Input;
using HanumanInstitute.Player432hz.Business;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents the media player.
/// </summary>
public class PlayerViewModel : ReactiveObject, IPlayerViewModel
{
    /// <summary>
    /// Gets an instance of IPlaylistPlayer that can be bound to the UI for playback.
    /// </summary>
    public IPlaylistPlayer Player { get; }

    public PlayerViewModel(IPlaylistPlayer player)
    {
        Player = player;
    }

    /// <summary>
    /// Plays the next file when playback ends.
    /// </summary>
    public ICommand PlayNextCommand => _playNextCommand ??= ReactiveCommand.Create<EventArgs>(OnPlayNext);
    private ICommand? _playNextCommand; 
    private void OnPlayNext(EventArgs e)
    {
        Player.PlayNext();
    }
}
