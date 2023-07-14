using System;
using HanumanInstitute.NaturalGroundingPlayer.Models;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the NaturalGroundingPlayer application settings.
    /// </summary>
    public interface IAppSettingsProvider : ISettingsProvider<AppSettingsData>
    { }
}
