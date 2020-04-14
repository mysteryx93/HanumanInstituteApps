using System;
using System.IO.Abstractions;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Manages the creation of IAvisynthScriptBuilder instances.
    /// </summary>
    public class ScriptFactory : IScriptFactory
    {
        private readonly IScriptPathService scriptPath;
        private readonly IFileSystem fileSystem;
        private readonly IShortFileNameService shortFileName;

        public ScriptFactory() : this(new ScriptPathService(), new FileSystem(), new ShortFileNameService()) { }

        public ScriptFactory(IScriptPathService scriptPathService, IFileSystem fileSystemService, IShortFileNameService shortFileNameService)
        {
            this.scriptPath = scriptPathService ?? throw new ArgumentNullException(nameof(scriptPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.shortFileName = shortFileNameService ?? throw new ArgumentNullException(nameof(shortFileNameService));
        }

        /// <summary>
        /// Creates a new instance of IScriptBuilderAvisynth.
        /// </summary>
        public IScriptBuilderAvisynth CreateAvisynthScript()
        {
            return new ScriptBuilderAvisynth(scriptPath, fileSystem, shortFileName);
        }
    }
}
