using Microsoft.Data.Sqlite;

namespace MyBackEndApp.Utils
{
    public class DbHelper : IDisposable
    {
        private static SqliteConnection? connection;
        private const string dbConnectionString = @"Data Source=db\sql_lite.db";

        public static async Task<SqliteConnection> GetConnection()
        {
            if (connection is null)
            {
                connection = new SqliteConnection(dbConnectionString);
                await connection.OpenAsync();
                var pragmaCommand = new SqliteCommand
                {
                    Connection = connection,
                    CommandText =
                        "PRAGMA journal_mode = OFF;\r\n" +
                        "PRAGMA synchronous = 0;\r\n" +
                        "PRAGMA cache_size = 1000000;\r\n" +
                        "PRAGMA locking_mode = EXCLUSIVE;\r\n" +
                        "PRAGMA temp_store = MEMORY;"
                };
                await pragmaCommand.ExecuteNonQueryAsync();
            }
            return connection;
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
