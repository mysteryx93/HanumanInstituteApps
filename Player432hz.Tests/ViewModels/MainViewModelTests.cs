using System;
using System.Collections.Generic;
using System.Windows.Input;
using MvvmDialogs;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp.Tests;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;
using Xunit;

namespace Player432hz.Tests.ViewModels
{
    public class MainViewModelTests
    {
        public Mock<IFilesListViewModel> MockFileList => _mockFileList ?? (_mockFileList = new Mock<IFilesListViewModel>());
        private Mock<IFilesListViewModel>? _mockFileList;

        public ISettingsProvider<SettingsData> MockSettings => _mockSettings ?? (_mockSettings = new FakeSettingsProvider<SettingsData>());
        private ISettingsProvider<SettingsData>? _mockSettings;

        public PlaylistViewModelFactory Factory => _factory ?? (_factory = new PlaylistViewModelFactory(Mock.Of<IDialogService>(), Mock.Of<IFilesListViewModel>()));
        private PlaylistViewModelFactory? _factory;

        public MainViewModel Model => _model ?? (_model = new MainViewModel(Factory, MockSettings, MockFileList.Object));
        private MainViewModel? _model;


        private void AddPlaylists(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Model.Playlists.List.Add(_factory!.Create());
            }
        }

        [Fact]
        public void CanAddPlaylistCommand_ReturnsTrue()
        {
            var result = Model.AddPlaylistCommand.CanExecute(null);

            Assert.True(result);
        }

        [Fact]
        public void AddPlaylistCommand_Execute_SelectedIndexSet()
        {
            Model.AddPlaylistCommand.Execute(null);

            Assert.Equal(0, Model.Playlists.SelectedIndex);
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
            AddPlaylists(2); // List contains 2 elements.
            Model.Playlists.SelectedIndex = selectedIndex;

            var result = Model.DeletePlaylistCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DeletePlaylistCommand_NoSelectedIndex_NoAction()
        {
            var listCount = 2;
            AddPlaylists(listCount);

            Model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(listCount, Model.Playlists.List.Count);
        }

        [Fact]
        public void DeletePlaylistCommand_EmptyList_NoAction()
        {
            Model.DeletePlaylistCommand.Execute(null);

            Assert.Empty(Model.Playlists.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void DeletePlaylistCommand_ValidSelectedIndex_RemoveAtSelectedIndex(int selectedIndex)
        {
            var listCount = 3;
            AddPlaylists(listCount);
            Model.Playlists.SelectedIndex = selectedIndex;
            var selectedItem = Model.Playlists.SelectedItem;

            Model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(listCount - 1, Model.Playlists.List.Count);
            Assert.DoesNotContain(selectedItem, Model.Playlists.List);
        }

        [Theory]
        [InlineData(1, 0, -1)]
        [InlineData(2, 0, 0)]
        [InlineData(3, 2, 1)]
        public void DeletePlaylistCommand_LastSelected_SetValidSelectedIndex(int count, int sel, int newSel)
        {
            AddPlaylists(count);
            Model.Playlists.SelectedIndex = sel;

            Model.DeletePlaylistCommand.Execute(null);

            Assert.Equal(newSel, Model.Playlists.SelectedIndex);
        }

        [Fact]
        public void DeletePlaylistCommand_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            AddPlaylists(1);
            Model.Playlists.SelectedIndex = 0;
            MockFileList.Reset();

            Model.DeletePlaylistCommand.Execute(null);

            MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void Playlists_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            AddPlaylists(1);

            Model.Playlists.SelectedIndex = 0;

            MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void Playlists_RemoveSelection_FilesListSetPathsCalled()
        {
            AddPlaylists(1);
            Model.Playlists.SelectedIndex = 0;
            MockFileList.Reset();

            Model.Playlists.SelectedIndex = -1;

            MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void PlayCommand_Get_ReturnsCommand()
        {
            MockFileList.Setup(x => x.PlayCommand).Returns(Mock.Of<ICommand>());

            var result = Model.PlayCommand;

            Assert.NotNull(result);
            Assert.IsAssignableFrom<ICommand>(result);
        }

        [Fact]
        public void Settings_Loaded_FillPlaylists()
        {
            AddPlaylists(1); // make sure it gets removed
            MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P1"));
            MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P2"));

            MockSettings.Load();

            Assert.Equal(2, Model.Playlists.List.Count);
        }

        [Fact]
        public void Settings_LoadedWithFolders_FillPlaylistFolders()
        {
            var folders = new List<string>() { "a", "b", "c" };
            MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P1", folders));

            MockSettings.Load();

            Assert.NotEmpty(Model.Playlists.List);
            Assert.Equal(3, Model.Playlists.List[0].Folders.List.Count);
        }

        [Fact]
        public void SaveSettings_WithPlaylists_FillSettings()
        {
            AddPlaylists(2);

            Model.SaveSettings();

            Assert.Equal(2, MockSettings.Value.Playlists.Count);
        }

        [Fact]
        public void SaveSettings_WithFolders_FillSettingsFolders()
        {
            AddPlaylists(1);
            Model.Playlists.List[0].Folders.List.Add("a");

            Model.SaveSettings();

            Assert.NotNull(MockSettings.Value.Playlists);
            Assert.Single(MockSettings.Value.Playlists[0].Folders);
        }
    }
}
