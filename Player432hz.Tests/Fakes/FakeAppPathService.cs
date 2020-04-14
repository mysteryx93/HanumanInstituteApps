using System;
using HanumanInstitute.CommonWpfApp.Tests;
using HanumanInstitute.Player432hz.Business;

namespace Player432hz.Tests
{
    public class FakeAppPathService : AppPathService
    {
        public FakeAppPathService() : base(new FakeEnvironmentService(), new FakeFileSystemService())
        { }
    }
}
