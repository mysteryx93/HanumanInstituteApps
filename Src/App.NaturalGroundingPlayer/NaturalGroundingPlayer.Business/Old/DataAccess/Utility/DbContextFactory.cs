using System;
using System.Data.Entity;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Creates instances of an Entity Framework data context.
    /// </summary>
    /// <typeparam name="T">The type of Entity Framework data context to create.</typeparam>
    public interface IDbContextFactory<out T> where T: DbContext {
        /// <summary>
        /// Creates a DbContext.
        /// </summary>
        T Create();
    }

    #endregion

    /// <summary>
    /// Creates instances of an Entity Framework data context.
    /// </summary>
    /// <typeparam name="T">The type of Entity Framework data context to create.</typeparam>
    public class DbContextFactory<T> : IDbContextFactory<T> where T : DbContext, new() {
        /// <summary>
        /// Creates a DbContext.
        /// </summary>
        public T Create() {
            T Context = new T();
#if DEBUG
            Context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
#endif
            return Context;
        }
    }
}
