using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using Xunit;

namespace Player432hz.Tests.ViewModels
{
    public class PlaylistViewModelTests
    {
        public Mock<IDialogService> MockDialogService => _mockDialogService ?? (_mockDialogService = new Mock<IDialogService>());
        private Mock<IDialogService>? _mockDialogService;

        public Mock<IFilesListViewModel> MockFileList => _mockFileList ?? (_mockFileList = new Mock<IFilesListViewModel>());
        private Mock<IFilesListViewModel>? _mockFileList;

        public PlaylistViewModel Model => _model ?? (_model = new PlaylistViewModel(MockDialogService.Object, MockFileList.Object));
        private PlaylistViewModel? _model;

        private const string DialogFolderPath = "C:\\";

        private void SetDialogFolder()
        {
            MockDialogService.Setup(x => x.ShowFolderBrowserDialog(It.IsAny<INotifyPropertyChanged>(), It.IsAny<FolderBrowserDialogSettings>()))
                .Callback<INotifyPropertyChanged, FolderBrowserDialogSettings>((owner, settings) => settings.SelectedPath = DialogFolderPath);
        }

        private void AddFolders(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Model.Folders.List.Add(i.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Fact]
        public void CanAddFolderCommand_ReturnsTrue()
        {
            var result = Model.AddFolderCommand.CanExecute(null);

            Assert.True(result);
        }

        [Fact]
        public void AddFolderCommand_Execute_CallsDialogService()
        {
            Model.AddFolderCommand.Execute(null);

            MockDialogService.Verify(x => x.ShowFolderBrowserDialog(It.IsAny<INotifyPropertyChanged>(), It.IsAny<FolderBrowserDialogSettings>()));
        }

        [Fact]
        public void AddFolderCommand_ExecuteCancel_NoAction()
        {
            var listCount = Model.Folders.List.Count;
            // MockDialogService.ShowFolderBrowserDialog() returns false by default.

            Model.AddFolderCommand.Execute(null);

            Assert.Equal(listCount, Model.Folders.List.Count);
        }

        [Fact]
        public void AddFolderCommand_SelectDir_FolderAddedToList()
        {
            var listCount = Model.Folders.List.Count;
            SetDialogFolder();

            Model.AddFolderCommand.Execute(null);

            Assert.Equal(listCount + 1, Model.Folders.List.Count);
            Assert.Equal(DialogFolderPath, Model.Folders.List.Last());
        }

        [Fact]
        public void AddFolderCommand_SelectDir_FilesListSetPathsCalled()
        {
            var listCount = Model.Folders.List.Count;
            SetDialogFolder();

            Model.AddFolderCommand.Execute(null);

            MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(int.MinValue, false)]
        [InlineData(int.MaxValue, false)]
        public void CanRemoveFolderCommand_WithSelectedIndex_ReturnsTrueIfSelectedIndexValid(int selectedIndex, bool expected)
        {
            AddFolders(2); // List contains 2 elements.
            Model.Folders.SelectedIndex = selectedIndex;

            var result = Model.RemoveFolderCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void RemoveFolderCommand_NoSelectedIndex_NoAction()
        {
            var listCount = 2;
            AddFolders(listCount);

            Model.RemoveFolderCommand.Execute(null);

            Assert.Equal(listCount, Model.Folders.List.Count);
        }

        [Fact]
        public void RemoveFolderCommand_EmptyList_NoAction()
        {
            Model.RemoveFolderCommand.Execute(null);

            Assert.Empty(Model.Folders.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void RemoveFolderCommand_ValidSelectedIndex_RemoveAtSelectedIndex(int selectedIndex)
        {
            var listCount = 3;
            AddFolders(listCount);
            Model.Folders.SelectedIndex = selectedIndex;
            var selectedItem = Model.Folders.SelectedItem;

            Model.RemoveFolderCommand.Execute(null);

            Assert.Equal(listCount - 1, Model.Folders.List.Count);
            Assert.DoesNotContain(selectedItem, Model.Folders.List);
        }

        [Theory]
        [InlineData(1, 0, -1)]
        [InlineData(2, 0, 0)]
        [InlineData(3, 2, 1)]
        public void RemoveFolderCommand_LastSelected_SetValidSelectedIndex(int count, int sel, int newSel)
        {
            AddFolders(count);
            Model.Folders.SelectedIndex = sel;

            Model.RemoveFolderCommand.Execute(null);

            Assert.Equal(newSel, Model.Folders.SelectedIndex);
        }

        [Fact]
        public void RemoveFolderCommand_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            AddFolders(1);
            Model.Folders.SelectedIndex = 0;
            MockFileList.Reset();

            Model.RemoveFolderCommand.Execute(null);

            MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        //private void AddFiles(PlaylistViewModel model, int count)
        //{
        //    for (var i = 0; i < count; i++)
        //    {
        //        Model.Files.Add(i.ToString());
        //    }
        //}

        //[Fact]
        //public void LoadFiles_HasFolders_FileLocatorCalledWithFolders()
        //{
        //    var model = SetupModel();
        //    AddFolders(model, 1);

        //    Model.LoadFiles();

        //    _mockFileLocator.Verify(x => x.GetAudioFiles(Model.Folders));
        //}

        //[Fact]
        //public void LoadFiles_NoParam_FilesListContainsResult()
        //{
        //    var model = SetupModel();
        //    var files = new[] { "a", "b", "c" };
        //    _mockFileLocator.Setup(x => x.GetAudioFiles(It.IsAny<IEnumerable<string>>())).Returns(files);

        //    Model.LoadFiles();

        //    Assert.Equal(files.Select(x => x), Model.Files.Select(x => x));
        //}
    }
}
