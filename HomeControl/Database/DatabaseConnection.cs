using HomeControl.Modeling;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace HomeControl.Database
{
    public interface IDatabaseConnection : IDisposable
    {
        Task InsertAsync<T>(T instance) where T : DatabaseModel;

        Task<T> SelectSingleAsync<T>(int id) where T : IdentityKeyModel;

        Task<T> SelectSingleAsync<T>(string id) where T : StringKeyModel;

        Task<List<T>> SelectAsync<T>(WhereBuilder.WhereElement<T> where) where T : DatabaseModel;

        Task<List<T>> SelectAllAsync<T>() where T : DatabaseModel;

        Task UpdateAsync<T>(T instance) where T : DatabaseModel;

        Task DeleteAsync<T>(T instance) where T : DatabaseModel;

        Task CommitTransactionAsync();
    }

    public class DatabaseConnection : IDatabaseConnection, IDisposable
    {
        public const string RowIdColumnName = "ROWID";

        static DatabaseConnection()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var modelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(DatabaseModel))))
            {
                var metadata = new DatabaseModelMetadata();

                var tableAttribute = modelType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

                if (tableAttribute != null) metadata.TableName = tableAttribute.Name;

                foreach (var property in modelType.GetProperties().Where(x => x.CanRead && x.CanWrite))
                {
                    var columnAttribute = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                    if (columnAttribute == null) metadata.Fields.Add(new DatabaseField(property));
                    else
                    {
                        var keyAttribute = property.GetCustomAttribute(typeof(KeyAttribute));

                        if (keyAttribute == null) metadata.Fields.Add(new DatabaseColumnField(property, columnAttribute.Name));
                        else metadata.Fields.Add(new PrimaryKeyField(property, columnAttribute.Name));
                    }
                }

                ModelMetadatas[modelType] = metadata;
            }
        }

        private static Dictionary<Type, DatabaseModelMetadata> ModelMetadatas { get; } = [];

        public SqliteConnection SqlConnection { get; }

        private SqliteTransaction _sqlTransaction;

        public DatabaseConnection(string connectionString)
        {
            SqlConnection = new SqliteConnection(connectionString);

            SqlConnection.Open();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        public async Task InsertAsync<T>(T instance) where T : DatabaseModel
        {
            instance.OnInserting();

            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder("INSERT INTO ")
                .Append($"[{modelMetadata.TableName}]")
                .Append('(');

            commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.OfType<DatabaseColumnField>()
                .Where(x => !IsIdentity(x)).Select(x => $"[{x.ColumnName}]")));

            commandStringBuilder.Append(')');

            commandStringBuilder.Append(" VALUES")
                .Append('(');

            commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.OfType<DatabaseColumnField>()
                .Where(field => !IsIdentity(field)).Select(field => AddFieldParameter(field, instance, command))));

            commandStringBuilder.Append(')');

            var primaryKey = GetPrimaryKey(modelMetadata);

            var isIdentity = primaryKey != null && primaryKey.IsIdentity;

            if (isIdentity) commandStringBuilder.Append("; SELECT LAST_INSERT_ROWID()");

            command.CommandText = commandStringBuilder.ToString();

            if (isIdentity)
            {
                var id = await command.ExecuteScalarAsync();

                ConvertDatabaseValue(primaryKey, ref id);

                instance.Set(id, primaryKey.Name);

                instance.ApplyChanges();
            }
            else await command.ExecuteNonQueryAsync();
        }

        public async Task<T> SelectSingleAsync<T>(string id) where T : StringKeyModel
        {
            return await SelectSingleCoreAsync<T, string>(id);
        }

        public async Task<T> SelectSingleAsync<T>(int id) where T : IdentityKeyModel
        {
            return await SelectSingleCoreAsync<T, int>(id);
        }

        private async Task<T> SelectSingleCoreAsync<T, TKey>(TKey id) where T : Model
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = BuildSelect(modelMetadata);

            var primaryKey = GetPrimaryKey(modelMetadata) ?? throw new Exception("Something went wrong.");

            if (id != null) commandStringBuilder.Append($" WHERE [{primaryKey.ColumnName}] = {AddFieldValueParameter(primaryKey, id, command)}");

            command.CommandText = commandStringBuilder.ToString();

            return await ReadSingle<T>(modelType, modelMetadata, command);
        }

        public async Task<List<T>> SelectAsync<T>(WhereBuilder.WhereElement<T> where) where T : DatabaseModel
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = BuildSelect(modelMetadata);

            commandStringBuilder.Append($" WHERE {where.BuildWhere(out var parameterValues)}");

            command.CommandText = commandStringBuilder.ToString();

            foreach (var parameterValue in parameterValues) command.Parameters.AddWithValue(parameterValue.Key, parameterValue.Value);

            return await ReadMany<T>(modelType, modelMetadata, command);
        }

        public async Task<List<T>> SelectAllAsync<T>() where T : DatabaseModel
        {
            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using (var command = SqlConnection.CreateCommand())
            {
                var commandStringBuilder = BuildSelect(modelMetadata);

                command.CommandText = commandStringBuilder.ToString();

                return await ReadMany<T>(modelType, modelMetadata, command);
            }
        }

        public async Task UpdateAsync<T>(T instance) where T : DatabaseModel
        {
            instance.OnUpdating();

            var modfiedProperies = instance.GetModifiedProperties();

            if (modfiedProperies.Length == 0) return;

            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder($"UPDATE [{modelMetadata.TableName}] SET ");

            for (int i = 0; i < modfiedProperies.Length; i++)
            {
                var modifiedProperty = modfiedProperies[i];

                var field = modelMetadata.Fields.OfType<DatabaseColumnField>().First(f => f.Name == modifiedProperty);
                
                commandStringBuilder.Append($"[{field.ColumnName}] = {AddFieldParameter(field, instance, command)}");

                if (i < modfiedProperies.Length - 1) commandStringBuilder.Append(", ");
            }

            AppendInstanceWhere(commandStringBuilder, instance, modelMetadata, command);

            command.CommandText = commandStringBuilder.ToString();

            await command.ExecuteNonQueryAsync();

            instance.ApplyChanges();
        }

        public async Task DeleteAsync<T>(T instance) where T : DatabaseModel
        {
            instance.OnDeleting();

            var modelType = typeof(T);

            var modelMetadata = ModelMetadatas[modelType];

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder("DELETE FROM ")
                .Append($"[{modelMetadata.TableName}]");

            AppendInstanceWhere(commandStringBuilder, instance, modelMetadata, command);

            command.CommandText = commandStringBuilder.ToString();

            await command.ExecuteNonQueryAsync();
        }

        private static void AppendInstanceWhere<T>(StringBuilder commandStringBuilder, T instance, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : Model
        {
            commandStringBuilder.Append(" WHERE ");

            var primaryKey = GetPrimaryKey(modelMetadata);

            if (primaryKey != null) commandStringBuilder.Append($"[{primaryKey.ColumnName}] = {AddFieldParameter(primaryKey, instance, command)}");
            else commandStringBuilder.Append(string.Join(" AND ", modelMetadata.Fields.OfType<DatabaseColumnField>().Select(field => commandStringBuilder.Append($"[{primaryKey.ColumnName}] = {AddFieldParameter(field, instance, command)}"))));
        }

        public async Task CommitTransactionAsync()
        {
            await _sqlTransaction.CommitAsync();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        private static PrimaryKeyField GetPrimaryKey(DatabaseModelMetadata modelMetadata)
        {
            return modelMetadata?.Fields.OfType<PrimaryKeyField>().FirstOrDefault();
        }

        private static bool IsIdentity(DatabaseColumnField field)
        {
            return field is PrimaryKeyField primaryKeyField && primaryKeyField.IsIdentity;
        }

        private static string SelectField(DatabaseColumnField field)
        {
            return $"[{field.ColumnName}] AS [{field.Name}]";
        }

        private static StringBuilder BuildSelect(DatabaseModelMetadata modelMetadata)
        {
            var commandStringBuilder = new StringBuilder("SELECT ");

            commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.OfType<DatabaseColumnField>().Select(SelectField)));

            return commandStringBuilder.Append(" FROM ").Append($"[{modelMetadata.TableName}]");
        }

        private static void ApplyFields<T>(T instance, DatabaseModelMetadata modelMetadata, SqliteDataReader reader) where T : Model
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);

                if (fieldValue == DBNull.Value) fieldValue = null;
                else
                {
                    var field = modelMetadata.Fields.First(field => field.Name == fieldName);

                    ConvertDatabaseValue(field, ref fieldValue);
                }

                instance.Set(fieldValue, fieldName);
            }
        }

        private static string AddFieldParameter<T>(DatabaseColumnField field, T instance, SqliteCommand command) where T : Model
        {
            var fieldValue = field.Get(instance);

            return AddFieldValueParameter(field, fieldValue, command);
        }

        private static string AddFieldValueParameter(DatabaseColumnField field, object fieldValue, SqliteCommand command)
        {
            var parameterName = $"${field.Name}";

            if (field.IsJson && fieldValue is not string) fieldValue = System.Text.Json.JsonSerializer.Serialize(fieldValue);

            command.Parameters.AddWithValue(parameterName, fieldValue);

            return parameterName;
        }

        private static void ConvertDatabaseValue(DatabaseField field, ref object value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(field, nameof(field));

            var valueType = value.GetType();

            if (valueType == field.PropertyInfo.PropertyType) return;

            if (field.IsJson && value is string valueJson)
            {
                var jsonDocument = System.Text.Json.JsonDocument.Parse(valueJson);

                var typeName = jsonDocument.RootElement.GetProperty(nameof(DatabaseModel.TypeName)).GetString();

                var jsonObjectType = Assembly.GetExecutingAssembly().GetType(typeName);

                value = System.Text.Json.JsonSerializer.Deserialize(valueJson, jsonObjectType);
                return;
            }

            if (field.PropertyInfo.PropertyType.IsEnum && valueType == typeof(string))
            {
                value = Enum.Parse(field.PropertyInfo.PropertyType, (string)value);
                return;
            }
            else if (field.PropertyInfo.PropertyType == typeof(int) && value is long longValue)
            {
                value = (int)longValue;
                return;
            }
            else if (field.PropertyInfo.PropertyType == typeof(bool) && value is long boolValue)
            {
                value = boolValue != 0;
                return;
            }
            else if (field.PropertyInfo.PropertyType == typeof(DateTime) && value is string dateTimeValue)
            {
                value = DateTime.Parse(dateTimeValue);
                return;
            }

            throw new InvalidOperationException("Unable to Convert Database Type.");
        }

        private async Task<T> ReadSingle<T>(Type modelType, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : Model
        {
            using var reader = await command.ExecuteReaderAsync();

            var instance = Activator.CreateInstance(modelType) as T;

            if (await reader.ReadAsync())
            {
                ApplyFields(instance, modelMetadata, reader);

                instance.ApplyChanges();

                return instance;
            }
            else return null;
        }

        private async Task<List<T>> ReadMany<T>(Type modelType, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : Model
        {
            using var reader = await command.ExecuteReaderAsync();

            var result = new List<T>();

            while (await reader.ReadAsync())
            {
                var instance = Activator.CreateInstance(modelType) as T;

                ApplyFields(instance, modelMetadata, reader);

                instance.ApplyChanges();

                result.Add(instance);
            }

            return result;
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