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
    public RxCommandUnit PlayNext => _playNext ??= ReactiveCommand.CreateFromTask(PlayNextImplAsync);
    private RxCommandUnit? _playNext; 
    private Task PlayNextImplAsync() => Player.PlayNextAsync();
}
