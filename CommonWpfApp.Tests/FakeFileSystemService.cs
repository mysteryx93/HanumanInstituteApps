using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonWpfApp.Tests
{
    public class FakeFileSystemService : MockFileSystem, IFileSystemService
    {
        public FakeFileSystemService() { }

        public FakeFileSystemService(IDictionary<string, MockFileData> files, string currentDirectory = "") :
            base(files, currentDirectory)
        { }

        public void DeleteFileSilent(string path) { }

        public void EnsureDirectoryExists(string path) { }

        public IEnumerable<string> GetFilesByExtensions(string path, IEnumerable<string> extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly) => Array.Empty<string>();

        public string GetPathWithoutExtension(string path) => string.Empty;

        public void MoveToRecycleBin(string path) { }

        public void MoveToRecycleBin(string path, bool displayWarning) { }
    }
}
