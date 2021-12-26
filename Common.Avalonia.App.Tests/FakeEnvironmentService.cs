using System;
using System.Collections.Generic;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeEnvironmentService : IEnvironmentService
{
    public IEnumerable<string> CommandLineArguments => Array.Empty<string>();

    public Version AppVersion => new Version(1, 0);

    public string AppFriendlyName => "TestApp";

    public string ApplicationDataPath => @"C:\TestAppData\";

    public string AppDirectory => @"C:\TestApp\";

    public string SystemRootDirectory => @"C:\";

    public string ProgramFilesX86 => @"C:\Program Files\";

    public char DirectorySeparatorChar => '\\';

    public DateTime Now => new DateTime(2019, 01, 01);

    public DateTime UtcNow => new DateTime(2019, 01, 01);
}