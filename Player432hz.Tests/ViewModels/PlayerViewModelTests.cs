using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.AvisynthScriptBuilder;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp.Tests;
using HanumanInstitute.Encoder;
using HanumanInstitute.Player432hz.ViewModels;
using HanumanInstitute.Player432hz.Business;
using Xunit;
using Moq;

namespace Player432hz.Tests.ViewModels
{
    public class PlayerViewModelTests
    {
        private IPlaylistPlayer _playlistPlayer;
        private Mock<IChangePitchBusiness> _mockChangePitch;
        private Mock<FakeFileSystemService> _mockFileSystem;
        private Mock<IMediaInfoReader> _mockInfoReader;
        private const string FileName1 = "file1", FileName2 = "file2", FileName3 = "file3";
        private int NowPlayingChanged = 0;

        private IPlayerViewModel SetupModel()
        {
            _mockFileSystem = new Mock<FakeFileSystemService>();
            _mockChangePitch = new Mock<IChangePitchBusiness>();
            var appPath = new FakeAppPathService();
            _mockInfoReader = new Mock<IMediaInfoReader>();
            var playlistPlayerBase = new PlaylistPlayer(_mockFileSystem.Object);
            _playlistPlayer = new PlaylistPlayer432hz(playlistPlayerBase, _mockChangePitch.Object, appPath, _mockInfoReader.Object);
            var model = new PlayerViewModel(_playlistPlayer);

            model.Player.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.Player.NowPlaying))
                {
                    NowPlayingChanged++;

                    if (model.Player.NowPlaying == null)
                    {
                        model.MediaFinished();
                    }
                }
            };
            return model;
        }

        [Fact]
        public void Constructor_VerifyInitialState()
        {
            var model = SetupModel();

            Assert.NotNull(model.Player);
            Assert.NotNull(model.Player.Files);
            Assert.Empty(model.Player.Files);
            Assert.Null(model.Player.NowPlaying);
            Assert.Null(model.Player.NowPlayingTitle);
        }

        [Fact]
        public void Play_ListNoCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(new[] { FileName1 }, null);

            Assert.Equal(1, model.Player.Files.Count);
            Assert.NotEmpty(model.Player.NowPlaying);
            Assert.Equal(FileName1, model.Player.NowPlayingTitle);
            Assert.Equal(1, NowPlayingChanged);
        }

        [Fact]
        public void Play_ListSetCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(new[] { FileName1 }, FileName2);

            Assert.Equal(1, model.Player.Files.Count);
            Assert.NotEmpty(model.Player.NowPlaying);
            Assert.Equal(FileName2, model.Player.NowPlayingTitle);
            Assert.Equal(1, NowPlayingChanged);
        }

        [Fact]
        public void Play_NoListSetCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(null, FileName2);

            Assert.Empty(model.Player.Files);
            Assert.NotEmpty(model.Player.NowPlaying);
            Assert.Equal(FileName2, model.Player.NowPlayingTitle);
            Assert.Equal(1, NowPlayingChanged);
        }

        [Fact]
        public void Play_NoListNoCurrent_NowPlayingNull()
        {
            var model = SetupModel();

            model.Player.Play(null, null);

            Assert.Empty(model.Player.Files);
            Assert.Null(model.Player.NowPlaying);
            Assert.Null(model.Player.NowPlayingTitle);
            Assert.Equal(0, NowPlayingChanged);
        }

        [Fact]
        public void PlayNext_ListNoCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(new[] { FileName1 }, null);
            model.Player.PlayNext();

            Assert.Equal(1, model.Player.Files.Count);
            Assert.NotNull(model.Player.NowPlaying);
            Assert.Equal(FileName1, model.Player.NowPlayingTitle);
            Assert.Equal(3, NowPlayingChanged);
        }

        [Fact]
        public void PlayNext_ListSetCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(new[] { FileName1, FileName2, FileName3 }, FileName1);
            model.Player.PlayNext();

            Assert.Equal(3, model.Player.Files.Count);
            Assert.NotNull(model.Player.NowPlaying);
            Assert.NotEqual(FileName1, model.Player.NowPlayingTitle);
            Assert.Equal(3, NowPlayingChanged);
        }

        [Fact]
        public void PlayNext_NoListSetCurrent_StartPlayback()
        {
            var model = SetupModel();

            model.Player.Play(null, FileName2);
            model.Player.PlayNext();

            Assert.Empty(model.Player.Files);
            Assert.NotNull(model.Player.NowPlaying);
            Assert.Equal(FileName2, model.Player.NowPlayingTitle);
            Assert.Equal(1, NowPlayingChanged);
        }

        [Fact]
        public void PlayNext_NoListNoCurrent_NowPlayingNull()
        {
            var model = SetupModel();

            model.Player.Play(null, null);
            model.Player.PlayNext();

            Assert.Empty(model.Player.Files);
            Assert.Null(model.Player.NowPlaying);
            Assert.Null(model.Player.NowPlayingTitle);
            Assert.Equal(0, NowPlayingChanged);
        }

        [Fact]
        public void PlayTwice_List_RestartsPlayback()
        {
            var model = SetupModel();

            model.Player.Play(new[] { FileName1 }, null);
            model.Player.Play(new[] { FileName2 }, null);

            Assert.Equal(1, model.Player.Files.Count);
            Assert.NotNull(model.Player.NowPlaying);
            Assert.Equal(FileName2, model.Player.NowPlayingTitle);
            Assert.Equal(3, NowPlayingChanged);
        }
    }
}
