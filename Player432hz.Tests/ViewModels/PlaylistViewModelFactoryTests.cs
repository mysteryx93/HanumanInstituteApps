using System;
using HanumanInstitute.Player432hz.ViewModels;
using HanumanInstitute.Player432hz.Business;
using MvvmDialogs;
using Xunit;
using Moq;

namespace Player432hz.Tests.ViewModels
{
    public class PlaylistViewModelFactoryTests
    {
        private const string DefaultPlaylistName = "New Playlist";

        public IPlaylistViewModelFactory SetupModel()
        {
            var fakeDialogService = Mock.Of<IDialogService>();
            var fakeFileList = Mock.Of<IFilesListViewModel>();
            return new PlaylistViewModelFactory(fakeDialogService, fakeFileList);
        }

        [Fact]
        public void Create_NoParam_ReturnsNewInstanceWithDefaultName()
        {
            var factory = SetupModel();

            var obj = factory.Create();

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
            var factory = SetupModel();

            var obj = factory.Create(name);

            Assert.IsAssignableFrom<PlaylistViewModel>(obj);
            Assert.Equal(name, obj.Name);
        }
    }
}
