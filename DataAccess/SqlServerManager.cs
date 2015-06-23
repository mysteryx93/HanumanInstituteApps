//using System;
//using System.Collections.Generic;
//using System.Data.Entity.Core.EntityClient;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Common;
//using DataAccess;

//namespace DataAccess {
//    public static class SqlServerManager {
//        /// <summary>
//        /// Runs specified script on SQL Server.
//        /// </summary>
//        /// <param name="script">The content of the script to run.</param>
//        public static void RunScript(string script) {
//            Server server = GetSmoConnection();
//            server.ConnectionContext.ExecuteNonQuery(script);
//        }

//        /// <summary>
//        /// Detaches current database from the server.
//        /// </summary>
//        public static void DetachDatabase() {
//            string DbName = GetSqlConnection().Database;
//            Server server = GetSmoConnection();
//            server.KillAllProcesses(DbName);
//            server.DetachDatabase(DbName, true);
//        }

//        /// <summary>
//        /// Returns a SqlServer.Management.Smo connection.
//        /// </summary>
//        public static Server GetSmoConnection() {
//            return new Server(new ServerConnection(GetSqlConnection()));
//        }

//        /// <summary>
//        /// Returns a SqlConnection linked to the database.
//        /// </summary>
//        public static SqlConnection GetSqlConnection() {
//            using (Entities context = new Entities()) {
//                string ConnString = (context.Database.Connection as SqlConnection).ConnectionString;
//                return new SqlConnection(ConnString);
//            }
//        }

//        /// <summary>
//        /// Returns a SqlConnection linked to the server without attaching the database..
//        /// </summary>
//        public static SqlConnection GetSqlBlankConnection() {
//            using (Entities context = new Entities()) {
//                string ConnString = (context.Database.Connection as EntityConnection).StoreConnection.ConnectionString;
//                ConnString = RemoveParameter(ConnString, "AttachDbFilename=");
//                ConnString = RemoveParameter(ConnString, "Initial Catalog=");
//                return new SqlConnection(ConnString);
//            }
//        }

//        /// <summary>
//        /// Removes specified parameter from the connection string.
//        /// </summary>
//        /// <param name="connectionString">The connection string.</param>
//        /// <param name="parameter">The parameter to remove.</param>
//        /// <returns>A connection string without specified parameter.</returns>
//        private static string RemoveParameter(string connectionString, string parameter) {
//            int ParamPos = connectionString.IndexOf(parameter);
//            if (ParamPos > -1) {
//                string Result = connectionString.Substring(0, ParamPos);
//                int ParamEndPos = connectionString.Substring(ParamPos).IndexOf(";");
//                if (ParamEndPos > -1)
//                    Result += connectionString.Substring(ParamPos + ParamEndPos + 1);
//                return Result;
//            } else
//                return connectionString;
//        }
//    }
//}
