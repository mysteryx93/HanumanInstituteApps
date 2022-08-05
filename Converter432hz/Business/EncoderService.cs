using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI.Validation.Helpers;

namespace HanumanInstitute.Converter432hz.Business;

/// <inheritdoc cref="IEncoderService"/>
public class EncoderService : ReactiveValidationObject, IEncoderService
{
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;
    private readonly IAudioEncoder _audioEncoder;
    private readonly IDispatcher _dispatcher;
    private CancellationTokenSource? _cancelTokenSource;

    public EncoderService(IFileSystemService fileSystem, IDialogService dialogService, IAudioEncoder audioEncoder, IDispatcher dispatcher)
    {
        _fileSystem = fileSystem;
        _dialogService = dialogService;
        _audioEncoder = audioEncoder;
        _dispatcher = dispatcher;

        // var sourceNotEmpty = Sources
        //     .ToObservableChangeSet(x => x.Path)
        //     .ToCollection()
        //     .Select(x => x.Any());
        // this.ValidationRule(x => x.Sources,
        //     sourceNotEmpty,
        //     "You must select source files or folders to encode");
        // Sources.Clear();
        //
        // this.ValidationRule(x => x.Destination,
        //     x => !string.IsNullOrWhiteSpace(x),
        //     "You must specify a valid destination folder");
        // this.ValidationRule(x => x.Destination,
        //     x => string.IsNullOrWhiteSpace(x) || _fileSystem.Directory.Exists(x),
        //     "Destination directory does not exist");

        // _isValid = this.IsValid().ToProperty(this, x => x.IsValidValue);
    }

    /// <summary>
    /// Gets or sets the ViewModel owning this service.
    /// </summary>
    public INotifyPropertyChanged Owner { get; set; } = default!;

    // readonly ObservableAsPropertyHelper<bool> _isValid;
    // public bool IsValidValue
    // {
    //     get { return _isValid.Value; }
    // }

    /// <inheritdoc />
    public ObservableCollection<FileItem> Sources { get; } = new();

    /// <inheritdoc />
    [Reactive]
    public string Destination { get; set; } = string.Empty;

    /// <inheritdoc />
    [Reactive]
    public EncodeSettings Settings { get; set; } = new();

    /// <inheritdoc />
    [Reactive]
    public FileExistsAction FileExistsAction { get; set; } = FileExistsAction.Ask;

    /// <inheritdoc />
    [Reactive]
    public bool IsProcessing { get; set; }

    /// <inheritdoc />
    [Reactive]
    public int DelayBeforeStart { get; set; } = 150;

    /// <inheritdoc />
    public ObservableCollection<ProcessingItem> ProcessingFiles { get; } = new();

    /// <inheritdoc />
    public event EventHandler<FileItem>? FileCompleted;

    /// <inheritdoc />
    public async Task RunAsync()
    {
        // if (!IsValidValue)
        // {
        //     var errors = string.Join(Environment.NewLine, GetErrors(null).Cast<string>());
        //     throw new ValidationException(errors);
        // }

        if (!IsProcessing)
        {
            var err = Validate();
            if (err != null)
            {
                await _dialogService.ShowMessageBoxAsync(Owner, err, "Validation", MessageBoxButton.Ok, MessageBoxImage.Exclamation);
            }
            else
            {
                IsProcessing = true;
                _cancelTokenSource = new CancellationTokenSource();

                await EncodeJobAsync(_cancelTokenSource.Token).ConfigureAwait(false);

                _cancelTokenSource = null;
                IsProcessing = false;
            }
        }
    }

    private string? Validate()
    {
        if (!Sources.Any())
        {
            return "You must select source files or folders to encode.";
        }
        if (string.IsNullOrWhiteSpace(Destination))
        {
            return "You must specify a valid destination folder.";
        }
        else if (!_fileSystem.Directory.Exists(Destination))
        {
            return "Destination directory does not exist";
        }
        return null;
    }

    /// <inheritdoc />
    public void Cancel() => _cancelTokenSource?.Cancel();

    /// <summary>
    /// Processes the batch job.
    /// </summary>
    private async Task EncodeJobAsync(CancellationToken cancellationToken = default)
    {
        var pool = new List<Task>();

        var source = Sources.FirstOrDefault();
        while (source != null)
        {
            if (source is FolderItem folder)
            {
                foreach (var item in folder.Files.ToList())
                {
                    await AddToPoolAsync(item).ConfigureAwait(false);

                    // Stop processing folder if it is deleted from sources.
                    if (folder != Sources.FirstOrDefault() || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            else
            {
                await AddToPoolAsync(source).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                source = null;
            }
            else
            {
                Sources.Remove(source);
                source = Sources.FirstOrDefault();
            }
        }
        await Task.WhenAll(pool).ConfigureAwait(false);

        async Task AddToPoolAsync(FileItem item)
        {
            int poolCount;
            lock (pool)
            {
                pool.Add(EncodeFileAsync(item, cancellationToken));
                poolCount = pool.Count;
            }
            if (poolCount >= Settings.MaxThreads)
            {
                await Task.WhenAny(pool.ToArray()).ConfigureAwait(false);
            }
            else
            {
                // If OperationCanceledException is thrown here it won't be caught.
                await Task.Delay(DelayBeforeStart, CancellationToken.None);
            }
            lock (pool)
            {
                pool.RemoveAll(x => x.IsCompleted);
            }
        }
    }

    /// <summary>
    /// Processes a file.
    /// </summary>
    /// <param name="fileItem">The file to encode.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task EncodeFileAsync(FileItem fileItem, CancellationToken cancellationToken = default)
    {
        var file = new ProcessingItem(fileItem);
        try
        {
            await _dispatcher.InvokeAsync(() => ProcessingFiles.Add(file)).ConfigureAwait(false);

            await SetDestination(file, cancellationToken).ConfigureAwait(false);

            if (file.Status != EncodeStatus.None || string.IsNullOrEmpty(file.Destination))
            {
                return;
            }
            file.Status = EncodeStatus.Processing;
            file.IsFileCreated = true;

            await _audioEncoder.StartAsync(file, Settings, cancellationToken);
            file.ProgressPercent = 0;
        }
        catch (OperationCanceledException)
        {
            file.Status = EncodeStatus.Cancelled;
        }
        catch (Exception)
        {
            file.Status = EncodeStatus.Error;
        }
        finally
        {
            if (file.IsFileCreated && (file.Status == EncodeStatus.Cancelled || file.Status == EncodeStatus.Error))
            {
                _fileSystem.DeleteFileSilent(file.Destination);
            }
            FileCompleted?.Invoke(this, file);
        }
    }

    /// <summary>
    /// Calculates the destination path.
    /// </summary>
    /// <param name="item">The item to process.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task SetDestination(ProcessingItem item, CancellationToken cancellationToken = default)
    {
        var destPath = _fileSystem.Path.Combine(Destination, item.RelativePath);
        destPath = ChangeExtension(destPath, Settings.Format);
        var destExists = _fileSystem.File.Exists(destPath);
        item.Destination = destPath;
        if (destExists && FileExistsAction == FileExistsAction.Ask)
        {
            await ShowAskFileActionUnique(item, cancellationToken);
        }
        else if (destExists)
        {
            ApplyFileExistsAction(item, FileExistsAction, cancellationToken);
        }
    }

    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Shows the AskFileAction dialog only once even if there are multiple threads.
    /// Applies the action before releasing the semaphore. 
    /// </summary>
    /// <param name="item">The file to process.</param>
    /// <param name="cancellationToken"></param>
    private async Task ShowAskFileActionUnique(ProcessingItem item, CancellationToken cancellationToken)
    {
        // Allow a single instance at the same time.
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // A first dialog can be applied to all, don't show another dialog.
            if (FileExistsAction != FileExistsAction.Ask || cancellationToken.IsCancellationRequested)
            {
                ApplyFileExistsAction(item, FileExistsAction, cancellationToken);
                return;
            }
        
            var vm = _dialogService.CreateViewModel<AskFileActionViewModel>();
            vm.FilePath = item.Path;
            await _dialogService.ShowDialogAsync(Owner, vm).ConfigureAwait(false);

            // Cancel action must be applied before semaphore is released to avoid showing an extra dialog.
            if (vm.DialogResult == true && vm.ApplyToAll)
            {
                FileExistsAction = vm.Action;
            }
            ApplyFileExistsAction(item, vm.Action, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string ChangeExtension(string path, EncodeFormat format)
    {
        return _fileSystem.GetPathWithoutExtension(path) +
               format switch
               {
                   EncodeFormat.Mp3 => ".mp3",
                   EncodeFormat.Flac => ".flac",
                   EncodeFormat.Ogg => ".ogg",
                   EncodeFormat.Opus => ".opus",
                   EncodeFormat.Wav => ".wav",
                   EncodeFormat.Aac => ".aac",
                   _ => ""
               };
    }

    private void ApplyFileExistsAction(ProcessingItem item, FileExistsAction action, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            item.Status = EncodeStatus.Cancelled;
        }
        else if (action == FileExistsAction.Cancel)
        {
            item.Status = EncodeStatus.Cancelled;
            Cancel();
        }
        else if (action == FileExistsAction.Overwrite)
        {
        }
        else if (action == FileExistsAction.Skip || action == FileExistsAction.Ask)
        {
            item.Status = EncodeStatus.Skip;
            item.Destination = string.Empty;
        }
        else if (action == FileExistsAction.Rename)
        {
            var destPathFile = _fileSystem.GetPathWithoutExtension(item.Destination);
            var destPathExt = _fileSystem.Path.GetExtension(item.Destination);
            var destPathRename = string.Empty;
            var i = 1;
            do
            {
                i++;
                destPathRename = $"{destPathFile} ({i}){destPathExt}";
            }
            while (_fileSystem.File.Exists(destPathRename));
            item.Destination = destPathRename;
        }
    }
}
