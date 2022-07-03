
namespace Common.Services.Tests.Integration;

public class UpdateServiceTests : TestsBase
{
    public UpdateServiceTests(ITestOutputHelper output) : base(output)
    {
    }

    public IUpdateService Model => _model ??= new UpdateService(FeedService); 
    private IUpdateService _model;

    public ISyndicationFeedService FeedService => _feedService ??= new SyndicationFeedService();
    private ISyndicationFeedService _feedService;

    [Fact]
    public void GetLatestVersion_NoFileFormat_ThrowsException()
    {
        Version Act() => Model.GetLatestVersion();

        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void GetLatestVersion_Valid_ReturnsVersion()
    {
        Model.GitRepo = "https://github.com/mysteryx93/NaturalGroundingPlayer";
        Model.FileFormat = "Player432hz-{0}_Win_x64.zip";

        var version = Model.GetLatestVersion();
        
        Output.WriteLine(version?.ToString());
        Assert.NotNull(version);
    }
    
    [Fact]
    public void GetLatestVersion_InvalidFile_ReturnsNull()
    {
        Model.GitRepo = "https://github.com/mysteryx93/NaturalGroundingPlayer";
        Model.FileFormat = "Invalid-{0}.zip";

        var version = Model.GetLatestVersion();
        
        Assert.Null(version);
    }
}
