using System;
using System.Linq;
using System.Threading.Tasks;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for database version information.
    /// </summary>
    public interface IVersionAccess {
        /// <summary>
        /// Returns the version information of the database.
        /// </summary>
        Version GetVersionInfo();
        /// <summary>
        /// Updates the version information of the database.
        /// </summary>
        /// <param name="version">The new version information to store.</param>
        void UpdateVersionInfo(Version version);
    }

    #endregion

    /// <summary>
    /// Provides data access for database version information.
    /// </summary>
    public class VersionAccess : IVersionAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public VersionAccess() : this (new NgpContextFactory()) { }

        public VersionAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Returns the version information of the database.
        /// </summary>
        public Version GetVersionInfo() {
            using (Entities context = contextFactory.Create()) {
                // We must query manually because querying entity objects would fail on outdated databases.
                DbVersion Result = context.Database.SqlQuery<DbVersion>("SELECT Major, Minor, Build, Revision FROM Version").FirstOrDefault();
                return new Version(Result.Major, Result.Minor, Result.Build, Result.Revision);
            }
        }

        /// <summary>
        /// Updates the version information of the database.
        /// </summary>
        /// <param name="version">The new version information to store.</param>
        public void UpdateVersionInfo(Version version) {
            using (Entities context = contextFactory.Create()) {
                DbVersion Row = context.DbVersions.Single();
                Row.Major = version.Major;
                Row.Minor = version.Minor;
                Row.Build = version.Build;
                Row.Revision = version.Revision;
                context.SaveChanges();
            }
        }
    }
}