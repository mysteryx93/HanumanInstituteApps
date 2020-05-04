using System;
using HanumanInstitute.CommonWpfApp.Tests;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;
using Xunit;

namespace Player432hz.Tests.ViewModels
{
    public class PlayerViewModelTests
    {
        public FakeFileSystemService MockFileSystem => _mockFileSystem ?? (_mockFileSystem = new FakeFileSystemService());
        private FakeFileSystemService? _mockFileSystem;

        public IPlaylistPlayer PlaylistPlayer => _playlistPlayer ?? (_playlistPlayer = new PlaylistPlayer(MockFileSystem));
        private IPlaylistPlayer? _playlistPlayer;

        public IPlayerViewModel Model => _model ?? (_model = SetupModel());
        private IPlayerViewModel? _model;

        private const string FileName1 = "file1", FileName2 = "file2", FileName3 = "file3";
        private int _nowPlayingChanged = 0;

        private IPlayerViewModel SetupModel()
        {
            var result = new PlayerViewModel(PlaylistPlayer);
            result.Player.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(result.Player.NowPlaying))
                {
                    _nowPlayingChanged++;

                    if (result.Player.NowPlaying == null)
                    {
                        result.MediaFinished();
                    }
                }
            };
            return result;
        }

        [Fact]
        public void Constructor_VerifyInitialState()
        {
            Assert.NotNull(Model.Player);
            Assert.NotNull(Model.Player.Files);
            Assert.Empty(Model.Player.Files);
            Assert.Empty(Model.Player.NowPlaying);
            Assert.Empty(Model.Player.NowPlayingTitle);
        }

        [Fact]
        public void Play_ListNoCurrent_StartPlayback()
        {
            Model.Player.Play(new[] { FileName1 }, null);

            Assert.Equal(1, Model.Player.Files.Count);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName1, Model.Player.NowPlayingTitle);
            Assert.Equal(1, _nowPlayingChanged);
        }

        [Fact]
        public void Play_ListSetCurrent_StartPlayback()
        {
            Model.Player.Play(new[] { FileName1 }, FileName2);

            Assert.Equal(1, Model.Player.Files.Count);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName2, Model.Player.NowPlayingTitle);
            Assert.Equal(1, _nowPlayingChanged);
        }

        [Fact]
        public void Play_NoListSetCurrent_StartPlayback()
        {
            Model.Player.Play(null, FileName2);

            Assert.Empty(Model.Player.Files);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName2, Model.Player.NowPlayingTitle);
            Assert.Equal(1, _nowPlayingChanged);
        }

        [Fact]
        public void Play_NoListNoCurrent_NowPlayingEmpty()
        {
            Model.Player.Play(null, null);

            Assert.Empty(Model.Player.Files);
            Assert.Empty(Model.Player.NowPlaying);
            Assert.Empty(Model.Player.NowPlayingTitle);
            Assert.Equal(0, _nowPlayingChanged);
        }

        [Fact]
        public void PlayNext_ListNoCurrent_StartPlayback()
        {
            Model.Player.Play(new[] { FileName1 }, null);
            Model.Player.PlayNext();

            Assert.Equal(1, Model.Player.Files.Count);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName1, Model.Player.NowPlayingTitle);
            Assert.Equal(3, _nowPlayingChanged);
        }

        [Fact]
        public void PlayNext_ListSetCurrent_StartPlayback()
        {
            Model.Player.Play(new[] { FileName1, FileName2, FileName3 }, FileName1);
            Model.Player.PlayNext();

            Assert.Equal(3, Model.Player.Files.Count);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.NotEqual(FileName1, Model.Player.NowPlayingTitle);
            Assert.Equal(3, _nowPlayingChanged);
        }

        [Fact]
        public void PlayNext_NoListSetCurrent_StartPlayback()
        {
            Model.Player.Play(null, FileName2);
            Model.Player.PlayNext();

            Assert.Empty(Model.Player.Files);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName2, Model.Player.NowPlayingTitle);
            Assert.Equal(1, _nowPlayingChanged);
        }

        [Fact]
        public void PlayNext_NoListNoCurrent_NowPlayingEmpty()
        {
            Model.Player.Play(null, null);
            Model.Player.PlayNext();

            Assert.Empty(Model.Player.Files);
            Assert.Empty(Model.Player.NowPlaying);
            Assert.Empty(Model.Player.NowPlayingTitle);
            Assert.Equal(0, _nowPlayingChanged);
        }

        [Fact]
        public void PlayTwice_List_RestartsPlayback()
        {
            Model.Player.Play(new[] { FileName1 }, null);
            Model.Player.Play(new[] { FileName2 }, null);

            Assert.Equal(1, Model.Player.Files.Count);
            Assert.NotEmpty(Model.Player.NowPlaying);
            Assert.Equal(FileName2, Model.Player.NowPlayingTitle);
            Assert.Equal(3, _nowPlayingChanged);
        }
    }
}
