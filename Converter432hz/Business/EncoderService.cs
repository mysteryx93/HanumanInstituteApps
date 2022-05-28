using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace HanumanInstitute.Converter432hz.Business;

/// <inheritdoc cref="IEncoderService"/>
public class EncoderService : ReactiveValidationObject, IEncoderService
{
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cancelTokenSource;

    public EncoderService(IFileSystemService fileSystem, IDialogService dialogService)
    {
        _fileSystem = fileSystem;
        _dialogService = dialogService;

        var sourceNotEmpty = Sources
            .ToObservableChangeSet(x => x.Path)
            .ToCollection()
            .Select(x => x.Any());
        this.ValidationRule(x => x.Sources,
            sourceNotEmpty,
            "You must select source files or folders to encode");
        this.Sources.Clear();

        this.ValidationRule(x => x.Destination,
            x => !string.IsNullOrWhiteSpace(x),
            "You must specify a valid destination folder");
        this.ValidationRule(x => x.Destination,
            x => string.IsNullOrWhiteSpace(x) || _fileSystem.Directory.Exists(x),
            "Destination directory does not exist");

        _isValid = this.IsValid().ToProperty(this, x => x.IsValidValue);
    }

    readonly ObservableAsPropertyHelper<bool> _isValid;
    public bool IsValidValue
    {
        get { return _isValid.Value; }
    }

    /// <inheritdoc />
    public ObservableCollection<FileItem> Sources { get; } = new ObservableCollectionExtended<FileItem>();

    /// <inheritdoc />
    [Reactive]
    public string Destination { get; set; } = string.Empty;

    /// <inheritdoc />
    [Reactive]
    public EncodeFormat Format { get; set; } = EncodeFormat.Mp3;

    /// <inheritdoc />
    [Reactive]
    public int Bitrate { get; set; }

    /// <inheritdoc />
    [Reactive]
    public FileExistsAction FileExistsAction { get; set; } = FileExistsAction.Ask;

    /// <inheritdoc />
    [Reactive]
    public bool IsProcessing { get; set; }

    /// <inheritdoc />
    public ObservableCollection<ProcessingItem> ProcessingFiles { get; } = new ObservableCollection<ProcessingItem>();


    /// <inheritdoc />
    public async Task RunAsync()
    {
        if (!IsProcessing && IsValidValue)
        {
            IsProcessing = true;
            _cancelTokenSource = new CancellationTokenSource();

            await EncodeJobAsync(_cancelTokenSource.Token).ConfigureAwait(false);

            _cancelTokenSource = null;
            IsProcessing = false;
        }
    }

    /// <inheritdoc />
    public void Cancel() => _cancelTokenSource?.Cancel();

    /// <summary>
    /// Processes the batch job.
    /// </summary>
    private async Task EncodeJobAsync(CancellationToken cancellationToken = default)
    {
        var source = Sources.FirstOrDefault();
        while (source != null)
        {
            if (source is FolderItem folder)
            {
                foreach (var item in folder.Files)
                {
                    await EncodeFileAsync(item, cancellationToken).ConfigureAwait(false);

                    // Stop processing folder if it is deleted from sources.
                    if (folder != Sources.FirstOrDefault())
                    {
                        break;
                    }
                }
            }
            else
            {
                await EncodeFileAsync(source, cancellationToken).ConfigureAwait(false);
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
    }

    /// <summary>
    /// Processes a file.
    /// </summary>
    /// <param name="fileItem">The file to encode.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task EncodeFileAsync(FileItem fileItem, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        var processing = new ProcessingItem(fileItem);
        await SetDestination(processing).ConfigureAwait(false);
        ProcessingFiles.Add(processing);
    }

    /// <summary>
    /// Calculates the destination path.
    /// </summary>
    /// <param name="item">The item to process.</param>
    private async Task SetDestination(ProcessingItem item)
    {
        var destPath = _fileSystem.Path.Combine(Destination, item.RelativePath);
        var destExists = _fileSystem.File.Exists(destPath);
        if (!destExists)
        {
            item.Destination = destPath;
        }
        else if (FileExistsAction == FileExistsAction.Ask)
        {
            // Display window to ask.
            var askViewModel = ViewModelLocator.AskFileAction;
            var result = await _dialogService.ShowDialogAsync(this, askViewModel).ConfigureAwait(false);
            ApplyFileExistsAction(item, destPath,
                result == true ? askViewModel.Action : FileExistsAction.Skip);
        }
        else
        {
            ApplyFileExistsAction(item, destPath, FileExistsAction);
        }
    }

    private void ApplyFileExistsAction(ProcessingItem item, string destPath, FileExistsAction action)
    {
        if (FileExistsAction == FileExistsAction.Cancel)
        {
            Cancel();
        }
        else if (FileExistsAction == FileExistsAction.Overwrite)
        {
            item.Destination = destPath;
        }
        else if (FileExistsAction == FileExistsAction.Skip)
        {
            item.Status = EncodeStatus.Skip;
        }
        else if (FileExistsAction == FileExistsAction.Rename)
        {
            var destPathFile = _fileSystem.GetPathWithoutExtension(destPath);
            var destPathExt = _fileSystem.Path.GetExtension(destPath);
            var destPathRename = string.Empty;
            var i = 0;
            do
            {
                i++;
                destPathRename = $"{destPathFile} ({i}).{destPathExt}";
            }
            while (_fileSystem.File.Exists(destPathRename));
            item.Destination = destPathRename;
        }
    }
}
