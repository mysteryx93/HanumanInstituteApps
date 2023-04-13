using System.Net.Http;
using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.Apps;

public class HanumanInstituteHttpClient : HttpClient
{
    public void QueryVersion()
    {
        
    }

    public AdInfo GetAds()
    {
        return new AdInfo();
    }
}
