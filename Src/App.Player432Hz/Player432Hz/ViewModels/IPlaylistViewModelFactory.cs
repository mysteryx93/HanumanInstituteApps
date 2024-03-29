﻿using System.ComponentModel;

namespace HanumanInstitute.Player432Hz.ViewModels;

/// <summary>
/// Creates new instances of IPlaylistViewModel.
/// </summary>
public interface IPlaylistViewModelFactory
{
    /// <summary>
    /// Gets or sets the ViewModel of the View hosting this factory. Must be set before calling Create.
    /// </summary>
    INotifyPropertyChanged? OwnerViewModel { get; set; }
    
    /// <summary>
    /// Returns a new instance of PlaylistViewModel with default playlist name.
    /// </summary>
    /// <returns>A new IPlaylistViewModel instance.</returns>
    IPlaylistViewModel Create();

    /// <summary>
    /// Returns a new instance of PlaylistViewModel with specified playlist name.
    /// </summary>
    /// <param name="name">The name of the new playlist.</param>
    /// <returns>A new IPlaylistViewModel instance.</returns>
    IPlaylistViewModel Create(string name);

    /// <summary>
    /// Returns a new instance of PlaylistViewModel from settings data.
    /// </summary>
    /// <param name="data">A playlist element within the settings file.</param>
    /// <returns>A new IPlaylistViewModel instance.</returns>
    IPlaylistViewModel Create(SettingsPlaylistItem data);
}
