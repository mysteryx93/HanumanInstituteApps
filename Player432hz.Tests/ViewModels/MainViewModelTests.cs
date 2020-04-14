using System;
using System.Collections.Generic;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Xunit;
using Moq;
using System.Windows.Input;

namespace Player432hz.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private Mock<IFilesListViewModel> _mockFileList;
        private FakeSettingsProvider _mockSettings;
        PlaylistViewModelFactory _factory;

        private MainViewModel SetupModel()
        {
            _mockFileList = new Mock<IFilesListViewModel>();
            _factory = new PlaylistViewModelFactory(null, null);
            _mockSettings = new FakeSettingsProvider();
            return new MainViewModel(_factory, _mockSettings, _mockFileList.Object);
        }

        private void AddPlaylists(MainViewModel model, int count)
        {
            for (var i = 0; i < count; i++)
            {
                model.Playlists.List.Add(_factory.Create());
            }
        }

        [Fact]
        public void CanAddPlaylistCommand_ReturnsTrue()
        {
            var model = SetupModel();

            var result = model.AddPlaylistCommand.CanExecute(null);

            Assert.True(result);
        }

        [Fact]
        public void AddPlaylistCommand_Execute_SelectedIndexSet()
        {
            var model = SetupModel();

            model.AddPlaylistCommand.Execute(null);

            Assert.Equal(0, model.Playlists.SelectedIndex);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(int.MinValue, false)]
        [InlineData(int.MaxValue, false)]
        public void CanDeletePlaylistCommand_WithSelectedIndex_ReturnsTrueIfSelectedIndexValid(int selectedIndex, bool expected)
        {
            var model = SetupModel();
            AddPlaylists(model, 2); // List contains 2 elements.
            model.Playlists.SelectedIndex = selectedIndex;

            var result = model.DeletePlaylistCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DeletePlaylistCommand_NoSelectedIndex_NoAction()
        {
            var listCount = 2;
            var model = SetupModel();
            AddPlaylists(model, listCount);

            model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(listCount, model.Playlists.List.Count);
        }

        [Fact]
        public void DeletePlaylistCommand_EmptyList_NoAction()
        {
            var model = SetupModel();

            model.DeletePlaylistCommand.Execute(null);

            Assert.Empty(model.Playlists.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void DeletePlaylistCommand_ValidSelectedIndex_RemoveAtSelectedIndex(int selectedIndex)
        {
            var listCount = 3;
            var model = SetupModel();
            AddPlaylists(model, listCount);
            model.Playlists.SelectedIndex = selectedIndex;
            var selectedItem = model.Playlists.SelectedItem;

            model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(listCount - 1, model.Playlists.List.Count);
            Assert.DoesNotContain(selectedItem, model.Playlists.List);
        }

        [Theory]
        [InlineData(1, 0, -1)]
        [InlineData(2, 0, 0)]
        [InlineData(3, 2, 1)]
        public void DeletePlaylistCommand_LastSelected_SetValidSelectedIndex(int count, int sel, int newSel)
        {
            var model = SetupModel();
            AddPlaylists(model, count);
            model.Playlists.SelectedIndex = sel;

            model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(newSel, model.Playlists.SelectedIndex);
        }

        [Fact]
        public void DeletePlaylistCommand_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            var model = SetupModel();
            AddPlaylists(model, 1);
            model.Playlists.SelectedIndex = 0;
            _mockFileList.Reset();

            model.DeletePlaylistCommand.Execute(null);

            _mockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void Playlists_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            var model = SetupModel();
            AddPlaylists(model, 1);

            model.Playlists.SelectedIndex = 0;

            _mockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void Playlists_RemoveSelection_FilesListSetPathsCalled()
        {
            var model = SetupModel();
            AddPlaylists(model, 1);
            model.Playlists.SelectedIndex = 0;
            _mockFileList.Reset();

            model.Playlists.SelectedIndex = -1;

            _mockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void PlayCommand_Get_ReturnsCommand()
        {
            var model = SetupModel();
            _mockFileList.Setup(x => x.PlayCommand).Returns(Mock.Of<ICommand>());

            var result = model.PlayCommand;

            Assert.NotNull(result);
            Assert.IsAssignableFrom<ICommand>(result);
        }

        [Fact]
        public void Settings_Loaded_FillPlaylists()
        {
            var model = SetupModel();
            AddPlaylists(model, 1); // make sure it gets removed
            _mockSettings.Current.Playlists.Add(new SettingsPlaylistItem("P1"));
            _mockSettings.Current.Playlists.Add(new SettingsPlaylistItem("P2"));

            _mockSettings.Load();

            Assert.Equal(2, model.Playlists.List.Count);
        }

        [Fact]
        public void Settings_LoadedWithFolders_FillPlaylistFolders()
        {
            var model = SetupModel();
            var folders = new List<string>() { "a", "b", "c" };
            _mockSettings.Current.Playlists.Add(new SettingsPlaylistItem("P1", folders));

            _mockSettings.Load();

            Assert.NotEmpty(model.Playlists.List);
            Assert.Equal(3, model.Playlists.List[0].Folders.List.Count);
        }

        [Fact]
        public void Settings_Saved_FillSettings()
        {
            var model = SetupModel();
            AddPlaylists(model, 2);

            _mockSettings.Save();

            Assert.Equal(2, _mockSettings.Current.Playlists.Count);
        }

        [Fact]
        public void Settings_SavedWithFolders_FillSettingsFolders()
        {
            var model = SetupModel();
            AddPlaylists(model, 1);
            model.Playlists.List[0].Folders.List.Add("a");

            _mockSettings.Save();

            Assert.NotNull(_mockSettings.Current.Playlists);
            Assert.Single(_mockSettings.Current.Playlists[0].Folders);
        }
    }
}
