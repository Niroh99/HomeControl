
using Microsoft.Data.Sqlite;

namespace HomeControl.Sql
{
    public static class Database
    {
        public const string RowIdColumnName = "ROWID";

        public static string ConnectionString { get; set; }

        private static SqliteConnection _connection = null;

        public static void Connect(Action action)
        {
            _ = Connect<object>((sqlConnection) =>
            {
                action();
                return null;
            }); 
        }

        public static T Connect<T>(Func<T> function)
        {
            return Connect((sqlConnection) => function());
        }

        public static T Connect<T>(Func<SqliteConnection, T> function)
        {
            using (_connection = new SqliteConnection(ConnectionString))
            {
                _connection.Open();

                var result = function(_connection);

                _connection.Close();

                _connection = null;

                return result;
            }
        }

        public static SqliteConnection GeRunningConnection()
        {
            if (ConnectionString == null) throw new Exception("ConnectionString not specified.");
            if (_connection == null) throw new Exception("Not Database Connection.");

            return _connection;
        }
    }
}