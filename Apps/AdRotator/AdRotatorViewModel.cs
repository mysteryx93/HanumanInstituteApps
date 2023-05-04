using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Timers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.Apps.AdRotator;

/// <inheritdoc cref="IAdRotatorViewModel" />
public class AdRotatorViewModel : ReactiveObject, IAdRotatorViewModel
{
    private readonly IProcessService _processService;
    private readonly ISerializationService _serialization;
    private readonly IJsonTypeInfoResolver _serializerContext;
    private readonly IAppPathServiceBase _appPaths;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IHanumanInstituteHttpClient _httpClient;
    private readonly Timer _timer;

    /// <summary>
    /// Initializes a new instance of the AdRotator class.
    /// </summary>
    public AdRotatorViewModel(IProcessService processService, ISerializationService serialization, IJsonTypeInfoResolver serializerContext,
        IAppPathServiceBase appPaths, IRandomGenerator randomGenerator, IHanumanInstituteHttpClient httpClient)
    {
        _processService = processService;
        _serialization = serialization;
        _serializerContext = serializerContext;
        _appPaths = appPaths;
        _randomGenerator = randomGenerator;
        _httpClient = httpClient;

        // Change ad every minutes.
        _timer = new Timer(TimeSpan.FromMinutes(2));
        _timer.Elapsed += (_, _) => SetRandomAd();
        _timer.Start();
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (httpClient == null) { return; }
        
        var _ = LoadAsync().ContinueWith(x =>
        {
            if (x.Exception != null)
            {
                GlobalErrorHandler.ShowErrorLog(x.Exception);
            }
        });
    }

    /// <inheritdoc />
    public bool Enabled
    {
        get => _enabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _enabled, value);
            _timer.Enabled = value;
        }
    }
    private bool _enabled = true;

    /// <inheritdoc />
    [Reactive]
    public AdInfo AdInfo { get; private set; } = new();

    /// <inheritdoc />
    [Reactive]
    public AdItem? Current { get; set; }

    /// <inheritdoc />
    public RxCommandUnit OpenLink => _openLink ??= ReactiveCommand.Create(OpenLinkImpl);
    private RxCommandUnit? _openLink;
    private void OpenLinkImpl()
    {
        if (Current != null)
        {
            var url = _httpClient.GetLinkTrackerUrl(Current.Id);
            _processService.OpenBrowserUrl(url);
        }
    }

    private async Task LoadAsync()
    {
        if (!Enabled) { return; }

        // try
        // {
        AdInfo = await _serialization.DeserializeFromFileAsync<AdInfo>(_appPaths.AdInfoPath, _serializerContext);
        // }
        // catch
        // {
        //     await LoadFromServerAsync();
        // }
        if (!AdInfo.Ads.Any())
        {
            await LoadDefaultAsync();
        }
        SetRandomAd();
    }

    public async Task LoadFromServerAsync()
    {
        if (!Enabled) { return; }

        try
        {
            var result = await _httpClient.GetAdsAsync();
            if (result != null)
            {
                AdInfo = result;
                await _serialization.SerializeToFileAsync(AdInfo, _appPaths.AdInfoPath, _serializerContext);
            }
        }
        catch
        {
            // ignored
        }
    }

    private async Task LoadDefaultAsync()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HanumanInstitute.Apps.AdRotator.DefaultAds.json");
        AdInfo = await _serialization.DeserializeAsync<AdInfo>(stream!, _serializerContext);
        await _serialization.SerializeToFileAsync(AdInfo, _appPaths.AdInfoPath, _serializerContext);
    }

    private void SetRandomAd()
    {
        var newAd = GetRandomAd();
        while (AdInfo.Ads.Count > 1 && newAd == Current)
        {
            newAd = GetRandomAd();
        }
        Current = newAd;
        // Current = AdInfo.Ads.FirstOrDefault(x => x.Id == 101);
    }
    
    private AdItem? GetRandomAd() => AdInfo.Ads.Any() ? AdInfo.Ads[_randomGenerator.GetInt(AdInfo.Ads.Count)] : null;
}
