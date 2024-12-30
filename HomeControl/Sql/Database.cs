
using Microsoft.Data.Sqlite;

namespace HomeControl.Sql
{
    public static class Database
    {
        public const string RowIdColumnName = "ROWID";

        public static string ConnectionString { get; set; }

        public static void Connect(Action<SqliteConnection> execute)
        {
            _ = Connect<object>((connection) =>
            {
                execute(connection);
                return null;
            });
        }

        public static T Connect<T>(Func<SqliteConnection, T> execute)
        {
            if (ConnectionString == null) throw new Exception("ConnectionString not specified.");

            using (var sqlConnection = new SqliteConnection(ConnectionString))
            {
                sqlConnection.Open();

                var result = execute(sqlConnection);

                sqlConnection.Close();

                return result;
            }
        }
    }
}