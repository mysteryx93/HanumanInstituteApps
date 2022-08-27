using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <inheritdoc cref="IPlayerViewModel" />
public class PlayerViewModel : ReactiveObject, IPlayerViewModel
{
    /// <inheritdoc />
    public IPlaylistPlayer Player { get; }

    public PlayerViewModel(IPlaylistPlayer player)
    {
        Player = player;
    }

    /// <inheritdoc />
    public RxCommandUnit PlayNextCommand => _playNextCommand ??= ReactiveCommand.Create(PlayNextImpl);
    private RxCommandUnit? _playNextCommand; 
    private void PlayNextImpl() => Player.PlayNext();
}
