using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess {
    public class VersionAccess {
        public static Version GetVersionInfo() {
            using (Entities context = new Entities()) {
                DbVersion Result = context.DbVersions.Single();
                return new Version(Result.Major, Result.Minor, Result.Build, Result.Revision);
            }
        }

        public async static Task<Version> GetVersionInfoAsync() {
            try {
                return await Task.Run(() => {
                    using (Entities context = new Entities()) {
                        // We must query manually because querying entity objects would fail on outdated databases.
                        DbVersion Result = context.Database.SqlQuery<DbVersion>("SELECT Major, Minor, Build, Revision FROM Version").FirstOrDefault();
                        //DbVersion Result = context.DbVersions.Single();
                        return new Version(Result.Major, Result.Minor, Result.Build, Result.Revision);
                    }
                });
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public static void UpdateVersionInfo(Version version) {
            using (Entities context = new Entities()) {
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