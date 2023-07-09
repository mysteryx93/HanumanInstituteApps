using System;
using System.Data.SQLite;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Creates instances of the NaturalGroundingPlayer database data context.
    /// </summary>
    public interface INgpContextFactory : IDbContextFactory<Entities> {
        /// <summary>
        /// Creates a DbContext.
        /// </summary>
        /// <param name="bindFunctions">If true, custom functions will be bound to the database context.</param>
        Entities Create(bool bindFunctions);
    }

    #endregion

    /// <summary>
    /// Creates instances of the NaturalGroundingPlayer database data context.
    /// </summary>
    public class NgpContextFactory : DbContextFactory<Entities>, INgpContextFactory {
        /// <summary>
        /// Initializes system variables for data access.
        /// </summary>
        public NgpContextFactory() {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Environment.SetEnvironmentVariable("AppendManifestToken_SQLiteProviderManifest", ";BinaryGUID=True;");
        }

        /// <summary>
        /// Creates a DbContext.
        /// </summary>
        /// <param name="bindFunctions">If true, custom functions will be bound to the database context.</param>
        public Entities Create(bool bindFunctions) {
            var Context = Create();
            SQLiteConnection Conn = (SQLiteConnection)Context.Database.Connection;
            Conn.Open();
            if (bindFunctions) {
                Conn.BindFunction(new DbGetRatingValueClass());
                Conn.BindFunction(new DbCompareValuesClass());
            }
            return Context;
        }
    }
}
