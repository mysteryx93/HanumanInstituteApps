using System;
using HanumanInstitute.Player432hz.ViewModels;
using MvvmDialogs;
using Xunit;
using Moq;

namespace Player432hz.Tests.ViewModels
{
    public class PlaylistViewModelFactoryTests
    {
        public IPlaylistViewModelFactory Model => _model ?? (_model = new PlaylistViewModelFactory(Mock.Of<IDialogService>(), Mock.Of<IFilesListViewModel>()));
        private IPlaylistViewModelFactory? _model;

        private const string DefaultPlaylistName = "New Playlist";

        [Fact]
        public void Create_NoParam_ReturnsNewInstanceWithDefaultName()
        {
            var obj = Model.Create();

            Assert.IsAssignableFrom<PlaylistViewModel>(obj);
            Assert.Equal(DefaultPlaylistName, obj.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        [InlineData(DefaultPlaylistName)]
        public void Create_WithName_ReturnsNewInstanceWithName(string name)
        {
            var obj = Model.Create(name);

            Assert.IsAssignableFrom<PlaylistViewModel>(obj);
            Assert.Equal(name, obj.Name);
        }
    }
}
