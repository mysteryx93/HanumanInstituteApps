using System;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Manages the creation of IAvisynthScriptBuilder instances.
    /// </summary>
    public interface IScriptFactory
    {
        /// <summary>
        /// Creates a new instance of IScriptBuilderAvisynth.
        /// </summary>
        IScriptBuilderAvisynth CreateAvisynthScript();
    }
}
