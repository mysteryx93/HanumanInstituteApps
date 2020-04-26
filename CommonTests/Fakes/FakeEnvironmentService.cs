using System;
using System.Collections.Generic;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonTests
{
    public class FakeEnvironmentService : IEnvironmentService
    {
        public IEnumerable<string> CommandLineArguments => null;

        public Version AppVersion => new Version(1, 0);

        public string AppFriendlyName => "TestApp";

        public string CommonApplicationDataPath => @"C:\TestAppData\";

        public string AppDirectory => @"C:\TestApp\";

        public string SystemRootDirectory => @"C:\";

        public string ProgramFilesX86 => @"C:\Program Files\";

        public char DirectorySeparatorChar => '\\';

        public DateTime Now => new DateTime(2019, 01, 01);

        public DateTime UtcNow => new DateTime(2019, 01, 01);
    }
}
