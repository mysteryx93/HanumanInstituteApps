using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using HanumanInstitute.Common.Services;
using Moq;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeFileSystemService : FileSystemService
{
    public FakeFileSystemService() : 
        this(new Dictionary<string, MockFileData>())
    { }

    public FakeFileSystemService(IDictionary<string, MockFileData> files, string currentDirectory = "") :
        base(new MockFileSystem(files, currentDirectory), Mock.Of<IWindowsApiService>())
    { }

    // public virtual void DeleteFileSilent(string path) { }
    //
    // public virtual void EnsureDirectoryExists(string path) => Directory.CreateDirectory(Path.GetDirectoryName(path));
    //
    // public virtual IEnumerable<string> GetFilesByExtensions(string path, IEnumerable<string> extensions, 
    //     SearchOption searchOption = SearchOption.TopDirectoryOnly) => Array.Empty<string>();
    //
    // public virtual string GetPathWithoutExtension(string path) => string.Empty;
    //
    // public virtual void MoveToRecycleBin(string path) { }
    //
    // public virtual void MoveToRecycleBin(string path, bool displayWarning) { }
}
