using System;
using System.Collections.Generic;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeEnvironmentService : IEnvironmentService
{
    public IEnumerable<string> CommandLineArguments { get; set; } = Array.Empty<string>();

    public Version AppVersion { get; set; } = new Version(1, 0);

    public string AppFriendlyName { get; set; } = "TestApp";

    public string ApplicationDataPath { get; set; } = @"C:\TestAppData\";

    public string AppDirectory { get; set; } = @"C:\TestApp\";

    public string SystemRootDirectory { get; set; } = @"C:\";

    public string ProgramFilesX86 { get; set; } = @"C:\Program Files\";

    public char DirectorySeparatorChar { get; set; } = '\\';

    public char AltDirectorySeparatorChar { get; set; } = '/';

    public DateTime Now { get; set; } = new DateTime(2019, 01, 01);

    public DateTime UtcNow { get; set; } = new DateTime(2019, 01, 01);
}
