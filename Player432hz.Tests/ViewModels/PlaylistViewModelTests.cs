using System;
using System.ComponentModel;
using System.Linq;
using HanumanInstitute.Player432hz.ViewModels;
using HanumanInstitute.Player432hz.Business;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using Xunit;
using Moq;
using System.Collections.Generic;

namespace Player432hz.Tests.ViewModels
{
    public class PlaylistViewModelTests
    {
        private Mock<IDialogService> _mockDialogService;
        private Mock<IFilesListViewModel> _mockFileList;
        private const string dialogFolderPath = "C:\\";

        private PlaylistViewModel SetupModel()
        {
            _mockDialogService = new Mock<IDialogService>();
            _mockFileList = new Mock<IFilesListViewModel>();
            return new PlaylistViewModel(_mockDialogService.Object, _mockFileList.Object);
        }

        private void SetDialogFolder()
        {
            _mockDialogService.Setup(x => x.ShowFolderBrowserDialog(It.IsAny<INotifyPropertyChanged>(), It.IsAny<FolderBrowserDialogSettings>()))
                .Callback<INotifyPropertyChanged, FolderBrowserDialogSettings>((owner, settings) => settings.SelectedPath = dialogFolderPath);
        }

        private void AddFolders(PlaylistViewModel model, int count)
        {
            for (var i = 0; i < count; i++) {
                model.Folders.List.Add(i.ToString());
            }
        }

        [Fact]
        public void CanAddFolderCommand_ReturnsTrue()
        {
            var model = SetupModel();

            var result = model.AddFolderCommand.CanExecute(null);

            Assert.True(result);
        }

        [Fact]
        public void AddFolderCommand_Execute_CallsDialogService()
        {
            var model = SetupModel();

            model.AddFolderCommand.Execute(null);

            _mockDialogService.Verify(x => x.ShowFolderBrowserDialog(It.IsAny<INotifyPropertyChanged>(), It.IsAny<FolderBrowserDialogSettings>()));
        }

        [Fact]
        public void AddFolderCommand_ExecuteCancel_NoAction()
        {
            var model = SetupModel();
            var listCount = model.Folders.List.Count;
            // _mockDialogService.ShowFolderBrowserDialog() returns false by default.

            model.AddFolderCommand.Execute(null);

            Assert.Equal(listCount, model.Folders.List.Count);
        }

        [Fact]
        public void AddFolderCommand_SelectDir_FolderAddedToList()
        {
            var model = SetupModel();
            var listCount = model.Folders.List.Count;
            SetDialogFolder();

            model.AddFolderCommand.Execute(null);

            Assert.Equal(listCount + 1, model.Folders.List.Count);
            Assert.Equal(dialogFolderPath, model.Folders.List.Last());
        }

        [Fact]
        public void AddFolderCommand_SelectDir_FilesListSetPathsCalled()
        {
            var model = SetupModel();
            var listCount = model.Folders.List.Count;
            SetDialogFolder();

            model.AddFolderCommand.Execute(null);

            _mockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
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
            var model = SetupModel();
            AddFolders(model, 2); // List contains 2 elements.
            model.Folders.SelectedIndex = selectedIndex;

            var result = model.RemoveFolderCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void RemoveFolderCommand_NoSelectedIndex_NoAction()
        {
            var listCount = 2;
            var model = SetupModel();
            AddFolders(model, listCount);

            model.RemoveFolderCommand.Execute(null);

            Assert.Equal(listCount, model.Folders.List.Count);
        }

        [Fact]
        public void RemoveFolderCommand_EmptyList_NoAction()
        {
            var model = SetupModel();

            model.RemoveFolderCommand.Execute(null);

            Assert.Empty(model.Folders.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void RemoveFolderCommand_ValidSelectedIndex_RemoveAtSelectedIndex(int selectedIndex)
        {
            var listCount = 3;
            var model = SetupModel();
            AddFolders(model, listCount);
            model.Folders.SelectedIndex = selectedIndex;
            var selectedItem = model.Folders.SelectedItem;

            model.RemoveFolderCommand.Execute(null);

            Assert.Equal(listCount - 1, model.Folders.List.Count);
            Assert.DoesNotContain(selectedItem, model.Folders.List);
        }

        [Theory]
        [InlineData(1, 0, -1)]
        [InlineData(2, 0, 0)]
        [InlineData(3, 2, 1)]
        public void RemoveFolderCommand_LastSelected_SetValidSelectedIndex(int count, int sel, int newSel)
        {
            var model = SetupModel();
            AddFolders(model, count);
            model.Folders.SelectedIndex = sel;

            model.RemoveFolderCommand.Execute(null);

            Assert.Equal(newSel, model.Folders.SelectedIndex);
        }

        [Fact]
        public void RemoveFolderCommand_ValidSelectedIndex_FilesListSetPathsCalled()
        {
            var model = SetupModel();
            AddFolders(model, 1);
            model.Folders.SelectedIndex = 0;
            _mockFileList.Reset();

            model.RemoveFolderCommand.Execute(null);

            _mockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        //private void AddFiles(PlaylistViewModel model, int count)
        //{
        //    for (var i = 0; i < count; i++)
        //    {
        //        model.Files.Add(i.ToString());
        //    }
        //}

        //[Fact]
        //public void LoadFiles_HasFolders_FileLocatorCalledWithFolders()
        //{
        //    var model = SetupModel();
        //    AddFolders(model, 1);

        //    model.LoadFiles();

        //    _mockFileLocator.Verify(x => x.GetAudioFiles(model.Folders));
        //}

        //[Fact]
        //public void LoadFiles_NoParam_FilesListContainsResult()
        //{
        //    var model = SetupModel();
        //    var files = new[] { "a", "b", "c" };
        //    _mockFileLocator.Setup(x => x.GetAudioFiles(It.IsAny<IEnumerable<string>>())).Returns(files);

        //    model.LoadFiles();

        //    Assert.Equal(files.Select(x => x), model.Files.Select(x => x));
        //}
    }
}
