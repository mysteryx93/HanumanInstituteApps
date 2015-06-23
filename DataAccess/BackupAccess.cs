//using System;
//using System.Data;
//using System.Data.SqlClient;

//namespace DataAccess {
//    public sealed class BackupAccess {
//        private BackupAccess() { }

//        public static void BackupDatabase(string database, string destPath) {
//            SqlConnection Conn = SqlServerManager.GetSqlConnection();
//            try {
//                string CommandText = string.Format("BACKUP DATABASE [{0}] TO DISK = '{1}'",
//                    database, destPath);
//                SqlCommand Command = new SqlCommand(CommandText, Conn);
//                Command.ExecuteNonQuery();
//            }
//            finally {
//                Conn.Close();
//            }
//        }

//        public static void RestoreDatabase(string database, string sourcePath, string destDataPath, string destLogPath) {
//            SqlConnection Conn = SqlServerManager.GetSqlBlankConnection();
//            try {
//                string CommandText = string.Format("RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH MOVE 'NaturalGroundingVideos_Data' TO '{2}', MOVE 'NaturalGroundingVideos_Log' TO '{3}', REPLACE",
//                    Conn.Database, sourcePath, destDataPath, destLogPath);
//                SqlCommand Command = new SqlCommand(CommandText, Conn);
//                Command.CommandTimeout = 120;
//                Command.ExecuteNonQuery();
//            }
//            finally {
//                Conn.Close();
//            }
//        }
//    }
//}