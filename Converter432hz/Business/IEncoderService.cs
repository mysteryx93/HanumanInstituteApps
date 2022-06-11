﻿using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Provides audio encoding functions.
/// </summary>
public interface IEncoderService : IReactiveObject, IValidatableViewModel
{
    /// <summary>
    /// Gets or sets the ViewModel owning this service.
    /// </summary>
    public INotifyPropertyChanged Owner { get; set; }
    /// <summary>
    /// Gets or sets the list of files and folders to encode.
    /// </summary>
    ObservableCollection<FileItem> Sources { get; }
    /// <summary>
    /// Gets or sets the destination folder where to encode the files.
    /// </summary>
    string Destination { get; set; }
    /// <summary>
    /// Gets or sets encoding settings.
    /// </summary>
    EncodeSettings Settings { get; set; }
    /// <summary>
    /// Gets or sets the action to take when the destination file already exists.
    /// </summary>
    FileExistsAction FileExistsAction { get; set; }
    /// <summary>
    /// Gets or sets whether encoding is in progress.
    /// </summary>
    bool IsProcessing { get; }
    /// <summary>
    /// Gets the list of completed files.
    /// </summary>
    ObservableCollection<ProcessingItem> ProcessingFiles { get; }
    /// <summary>
    /// Starts the encoding batch job.
    /// </summary>
    Task RunAsync();
    /// <summary>
    /// Cancels the encoding batch job.
    /// </summary>
    void Cancel();
}
