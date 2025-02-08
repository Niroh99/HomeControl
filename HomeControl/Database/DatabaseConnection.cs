using HomeControl.Modeling;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace HomeControl.Database
{
    public interface IDatabaseConnection
    {
        Task<T> SelectAsync<T>(int id) where T : IdentityKeyModel;

        Task<T> SelectAsync<T>(string id) where T : StringKeyModel;

        Task<List<T>> SelectAllAsync<T>() where T : Model;

        Task InsertAsync<T>(T instance) where T : Model;

        Task DeleteAsync<T>(T instance) where T : Model;

        Task CommitTransactionAsync();
    }

    public class DatabaseConnection : IDatabaseConnection, IDisposable
    {
        public const string RowIdColumnName = "ROWID";

        static DatabaseConnection()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var modelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(Model))))
            {
                var metadata = new ModelMetadata<DatabaseField>();

                var tableAttribute = modelType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

                if (tableAttribute == null) metadata.TableName = modelType.Name;
                else metadata.TableName = tableAttribute.Name;

                foreach (var property in modelType.GetProperties().Where(x => x.CanRead && x.CanWrite))
                {
                    var columnAttribute = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                    if (columnAttribute == null) continue;

                    var keyAttribute = property.GetCustomAttribute(typeof(KeyAttribute));

                    if (keyAttribute == null) metadata.Fields.Add(new DatabaseField(property.Name, columnAttribute.Name));
                    else
                    {
                        if (modelType.IsAssignableTo(typeof(IdentityKeyModel))) metadata.Fields.Add(new PrimaryKeyField(property.Name, true, columnAttribute.Name));
                        else metadata.Fields.Add(new PrimaryKeyField(property.Name, false, columnAttribute.Name));
                    }
                }

                ModelMetadatas[modelType] = metadata;
            }
        }

        public SqliteConnection SqlConnection { get; }

        private SqliteTransaction _sqlTransaction;

        public DatabaseConnection(string connectionString)
        {
            SqlConnection = new SqliteConnection(connectionString);

            SqlConnection.Open();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        private static Dictionary<Type, ModelMetadata<DatabaseField>> ModelMetadatas { get; } = new Dictionary<Type, ModelMetadata<DatabaseField>>();

        public async Task<T> SelectAsync<T>(string id) where T : StringKeyModel
        {
            return await SelectAsyncCore<T, string>(id);
        }

        public async Task<T> SelectAsync<T>(int id) where T : IdentityKeyModel
        {
            return await SelectAsyncCore<T, int>(id);
        }

        private async Task<T> SelectAsyncCore<T, TKey>(TKey id) where T : Model
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            var instance = Activator.CreateInstance(modelType) as T;

            using (var command = SqlConnection.CreateCommand())
            {
                var commandStringBuilder = BuildSelect(modelMetadata);

                var primaryKey = GetPrimaryKey(modelMetadata);

                if (primaryKey != null && id != null)
                {
                    commandStringBuilder.Append($" WHERE [{primaryKey.ColumnName}] = $Id");

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "$Id";
                    parameter.Value = id;

                    command.Parameters.Add(parameter);
                }

                command.CommandText = commandStringBuilder.ToString();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        ApplyFields(instance, reader);

                        return instance;
                    }
                    else return null;
                }
            }
        }

        public async Task<List<T>> SelectAllAsync<T>() where T : Model
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using (var command = SqlConnection.CreateCommand())
            {
                var commandStringBuilder = BuildSelect(modelMetadata);

                command.CommandText = commandStringBuilder.ToString();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<T>();

                    while (await reader.ReadAsync())
                    {
                        var instance = Activator.CreateInstance(modelType) as T;

                        ApplyFields(instance, reader);

                        result.Add(instance);
                    }

                    return result;
                }
            }
        }

        public async Task InsertAsync<T>(T instance) where T : Model
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using (var command = SqlConnection.CreateCommand())
            {
                var commandStringBuilder = new StringBuilder("INSERT INTO ")
                    .Append($"[{modelMetadata.TableName}]")
                    .Append('(');

                commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.Where(x => !IsIdentity(x)).Select(x => $"[{x.ColumnName}]")));

                commandStringBuilder.Append(')');

                commandStringBuilder.Append(" VALUES")
                    .Append('(');

                commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.Where(x => !IsIdentity(x)).Select(x => InsertField(x, instance, command))));

                commandStringBuilder.Append(')');

                var primaryKey = GetPrimaryKey(modelMetadata);

                var isIdentity = primaryKey != null && primaryKey.IsIdentity;

                if (isIdentity) commandStringBuilder.Append("; SELECT LAST_INSERT_ROWID()");

                command.CommandText = commandStringBuilder.ToString();

                if (isIdentity)
                {
                    var id = await command.ExecuteScalarAsync();

                    instance.Set(id, primaryKey.Name);
                }
                else await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync<T>(T instance) where T : Model
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using (var command = SqlConnection.CreateCommand())
            {
                var commandStringBuilder = new StringBuilder("DELETE FROM ")
                    .Append($"[{modelMetadata.TableName}]")
                    .Append(" WHERE ");

                var primaryKey = GetPrimaryKey(modelMetadata);

                if (primaryKey != null && primaryKey.IsIdentity) commandStringBuilder.Append(DeleteField(primaryKey, instance, command));
                else commandStringBuilder.Append(string.Join(" AND ", modelMetadata.Fields.Select(x => DeleteField(x, instance, command))));

                command.CommandText = commandStringBuilder.ToString();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            await _sqlTransaction.CommitAsync();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        private static PrimaryKeyField GetPrimaryKey(ModelMetadata<DatabaseField> modelMetadata)
        {
            return modelMetadata?.Fields.OfType<PrimaryKeyField>().FirstOrDefault();
        }

        private static bool IsIdentity(DatabaseField field)
        {
            return field is PrimaryKeyField primaryKeyField && primaryKeyField.IsIdentity;
        }

        private static string SelectField(DatabaseField field)
        {
            return $"[{field.ColumnName}] AS [{field.Name}]";
        }

        private static StringBuilder BuildSelect(ModelMetadata<DatabaseField> modelMetadata)
        {
            var commandStringBuilder = new StringBuilder("SELECT ");

            commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.Select(x => SelectField(x))));

            return commandStringBuilder.Append(" FROM ").Append($"[{modelMetadata.TableName}]");
        }

        private static void ApplyFields<T>(T instance, SqliteDataReader reader) where T : Model
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);

                instance.Set(fieldValue, fieldName);
            }
        }

        private static string InsertField<T>(DatabaseField field, T instance, SqliteCommand command) where T : Model
        {
            var parameterName = $"${field.Name}";

            command.Parameters.AddWithValue(parameterName, instance.Get<object>(field.Name));

            return parameterName;
        }

        private static string DeleteField<T>(DatabaseField field, T instance, SqliteCommand command) where T : Model
        {
            var parameterName = $"${field.Name}";

            command.Parameters.AddWithValue(parameterName, instance.Get<object>(field.Name));

            return $"[{field.ColumnName}] = {parameterName}";
        }

        public async void Dispose()
        {
            await _sqlTransaction.CommitAsync();

            await SqlConnection.CloseAsync();
            await SqlConnection.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}