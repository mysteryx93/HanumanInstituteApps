using System.Linq;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class AppSettingsProviderTests
{
    protected AppSettingsData Settings { get; set; } = new();

    protected FakeEnvironmentService MockEnvironment => _mockEnvironment ??= new FakeEnvironmentService();
    private FakeEnvironmentService _mockEnvironment;

    protected FakeFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService();
    private FakeFileSystemService _mockFileSystem;

    protected IAppPathService AppPath => _appPath ??= new AppPathService(MockEnvironment, MockFileSystem, Mock.Of<IBassDevice>());
    private IAppPathService _appPath;

    protected ISerializationService Serialization => _serialization ??= new SerializationService(MockFileSystem);
    private ISerializationService _serialization;

    protected AppSettingsProvider Model => _model ??= new AppSettingsProvider(Serialization, AppPath, MockFileSystem);
    private AppSettingsProvider _model;

    protected string SetPath() => System.IO.Path.DirectorySeparatorChar == '\\' ? SetWindowsPath() : SetLinuxPath();

    protected string SetWindowsPath()
    {
        MockEnvironment.ApplicationDataPath = @"C:\AppData";
        MockEnvironment.DirectorySeparatorChar = '\\';
        MockEnvironment.AltDirectorySeparatorChar = '/';
        return @"C:\AppData\Hanuman Institute\PowerliminalsConfig.xml";
    }

    protected string SetLinuxPath()
    {
        MockEnvironment.ApplicationDataPath = @"/AppData";
        MockEnvironment.DirectorySeparatorChar = '/';
        MockEnvironment.AltDirectorySeparatorChar = '/';
        return @"/AppData/Hanuman Institute/PowerliminalsConfig.xml";
    }

    // Load
    // Test load path
    // Test load valid XML / invalid XML / no file

    // [Fact]
    // public void Load_Default_WindowsPathValid()
    // {
    //     var mockSerialization = new Mock<ISerializationService>();
    //     _serialization = mockSerialization.Object;
    //     var path = SetWindowsPath();
    //     var outPath = string.Empty;
    //     mockSerialization.Setup(x => x.DeserializeFromFile<string>(It.IsAny<string>())).Callback<string>(x => outPath = x);
    //
    //     Model.Load();
    //
    //     mockSerialization.Verify(x => x.DeserializeFromFile<AppSettingsData>(path), Times.Once);
    // }

    [Fact]
    public void Load_Default_LinuxPathValid()
    {
        var mockSerialization = new Mock<ISerializationService>();
        _serialization = mockSerialization.Object;
        var path = SetLinuxPath();

        var _ = Model; // Init object, Load is called in constructor.
        // Model.Load();

        mockSerialization.Verify(x => x.DeserializeFromFile<AppSettingsData>(path), Times.Once);
    }

    [Fact]
    public void Load_ValidXml_SettingsLoaded()
    {
        var path = SetPath();
        MockFileSystem.EnsureDirectoryExists(path);
        using (var file = MockFileSystem.File.CreateText(path))
        {
            file.Write(ValidXml);
        }

        Model.Load();

        Assert.NotEmpty(Model.Value.Folders);
        Assert.NotEmpty(Model.Value.Presets);
        Assert.Equal(100, Model.Value.Width);
        Assert.NotEmpty(Model.Value.Presets.First().Files);
    }

    // [Fact]
    // public void Load_InvalidXml_DefaultSettings()
    // {
    //     var path = SetPath();
    //     MockFileSystem.EnsureDirectoryExists(path);
    //     using (var file = MockFileSystem.File.CreateText(path))
    //     {
    //         file.Write(InvalidXml);
    //     }
    //
    //     Model.Load();
    //
    //     Assert.Empty(Model.Value.Folders);
    //     Assert.Empty(Model.Value.Presets);
    //     Assert.Equal(AppSettingsProvider.DefaultWidth, Model.Value.Width);
    // }
    //
    // [Fact]
    // public void Load_NoFile_DefaultSettings()
    // {
    //     SetPath();
    //
    //     Model.Load();
    //
    //     Assert.Empty(Model.Value.Folders);
    //     Assert.Empty(Model.Value.Presets);
    //     Assert.Equal(AppSettingsProvider.DefaultWidth, Model.Value.Width);
    // }

    [Fact]
    public void Save_Default_CreateSettingsFile()
    {
        var path = SetPath();

        Model.Save();

        Assert.True(MockFileSystem.File.Exists(path));
    }
    
    [Fact]
    public void Save_WithPreset_CreateSettingsFile()
    {
        var path = SetPath();
        Model.Value.Folders.Add("C:\\");
        Model.Value.Presets.Add(new PresetItem("Preset"));

        Model.Save();

        Assert.True(MockFileSystem.File.Exists(path));
    }
    
    [Fact]
    public void Save_ExistingFile_OverwriteFile()
    {
        var path = SetPath();
        MockFileSystem.EnsureDirectoryExists(path);
        using (var file = MockFileSystem.File.CreateText(path))
        {
            file.Write(InvalidXml);
        }
        
        Model.Save();

        Assert.True(MockFileSystem.File.Exists(path));
    }

    public const string ValidXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<PowerliminalsPlayer>
	<Folder>/run/media/hanuman/SSD/Audios</Folder>
	<Presets>
		<PresetItem>
			<Name>Purple &amp; Gold</Name>
			<File>
				<FullPath>/run/media/hanuman/SSD/Audios/AmethystRain.wav</FullPath>
				<Volume>97.99306840741613</Volume>
				<IsPlaying>true</IsPlaying>
				<Speed>3</Speed>
				<Rate>1.375</Rate>
			</File>
			<File>
				<FullPath>/run/media/hanuman/SSD/Audios/AmethystRain.wav</FullPath>
				<Volume>97.99306840741613</Volume>
				<IsPlaying>true</IsPlaying>
				<Speed>2</Speed>
				<Rate>1.25</Rate>
			</File>
			<MasterVolume>45.92161540743671</MasterVolume>
		</PresetItem>
		<PresetItem>
			<Name>Ruby Night</Name>
			<File>
				<FullPath>/run/media/hanuman/SSD/Audios/RubyRain.wav</FullPath>
				<Volume>99.3148848629637</Volume>
				<IsPlaying>true</IsPlaying>
				<Speed>2</Speed>
				<Rate>1.25</Rate>
			</File>
			<MasterVolume>37.25524129746836</MasterVolume>
		</PresetItem>
	</Presets>
	<FoldersExpanded>false</FoldersExpanded>
	<Width>100</Width>
	<Height>100</Height>
</PowerliminalsPlayer>";

    public const string InvalidXml = "<INVALID XML />";
}
