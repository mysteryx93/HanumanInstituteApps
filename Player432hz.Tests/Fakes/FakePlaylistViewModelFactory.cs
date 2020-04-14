using System;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;

namespace Player432hz.Tests
{
    public class FakePlaylistViewModelFactory : IPlaylistViewModelFactory
    {
        public IPlaylistViewModel Create() => Mock.Of<IPlaylistViewModel>();

        public IPlaylistViewModel Create(string name) => Create();

        public IPlaylistViewModel Create(SettingsPlaylistItem data) => Create();
    }
}
