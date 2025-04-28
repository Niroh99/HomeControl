using HomeControl.Modeling;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        private readonly IServiceProvider _serviceProvider;

        public DatabaseConnection(string connectionString, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

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

                id = await ConvertDatabaseValue(primaryKey, id);

                instance.Set(id, primaryKey.Name);

                instance.ApplyChanges();

                await instance.CreateDisplay(_serviceProvider);
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

        private async Task<T> SelectSingleCoreAsync<T, TKey>(TKey id) where T : DatabaseModel
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

            await instance.CreateDisplay(_serviceProvider);
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

        private static void AppendInstanceWhere<T>(StringBuilder commandStringBuilder, T instance, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : DatabaseModel
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

        private async Task ApplyFields<T>(T instance, DatabaseModelMetadata modelMetadata, SqliteDataReader reader) where T : DatabaseModel
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);

                if (fieldValue == DBNull.Value) fieldValue = null;
                else
                {
                    var field = modelMetadata.Fields.First(field => field.Name == fieldName);

                    fieldValue = await ConvertDatabaseValue(field, fieldValue);
                }

                instance.Set(fieldValue, fieldName);
            }
        }

        private static string AddFieldParameter<T>(DatabaseColumnField field, T instance, SqliteCommand command) where T : DatabaseModel
        {
            var fieldValue = field.Get(instance);

            return AddFieldValueParameter(field, fieldValue, command);
        }

        private static string AddFieldValueParameter(DatabaseColumnField field, object fieldValue, SqliteCommand command)
        {
            var parameterName = $"${field.Name}";

            if (fieldValue == null) fieldValue = DBNull.Value;
            else if (field.IsJson && fieldValue is not string) fieldValue = System.Text.Json.JsonSerializer.Serialize(fieldValue);

            command.Parameters.AddWithValue(parameterName, fieldValue);

            return parameterName;
        }

        private async Task<object> ConvertDatabaseValue(DatabaseField field, object databaseValue)
        {
            ArgumentNullException.ThrowIfNull(databaseValue, nameof(databaseValue));
            ArgumentNullException.ThrowIfNull(field, nameof(field));

            var valueType = databaseValue.GetType();

            var propertyType = field.PropertyInfo.PropertyType;

            if (propertyType.IsGenericType)
            {
                var genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    if (databaseValue == null) return null;

                    propertyType = propertyType.GetGenericArguments()[0];
                }
            }

            if (valueType == propertyType) return databaseValue;

            if (field.IsJson && databaseValue is string valueJson)
            {
                var jsonDocument = System.Text.Json.JsonDocument.Parse(valueJson);

                var typeName = jsonDocument.RootElement.GetProperty(nameof(DatabaseModel.TypeName)).GetString();

                var jsonObjectType = Assembly.GetExecutingAssembly().GetType(typeName);

                var value = System.Text.Json.JsonSerializer.Deserialize(valueJson, jsonObjectType);

                if (value is Model modelValue) await modelValue.CreateDisplay(_serviceProvider);

                return value;
            }

            if (propertyType.IsEnum && valueType == typeof(string))
            {
                return Enum.Parse(propertyType, (string)databaseValue);
            }
            else if (propertyType == typeof(int) && databaseValue is long longValue)
            {
                return (int)longValue;
            }
            else if (propertyType == typeof(bool) && databaseValue is long boolValue)
            {
                return boolValue != 0;
            }
            else if (propertyType == typeof(DateTime) && databaseValue is string dateTimeValue)
            {
                return DateTime.Parse(dateTimeValue);
            }

            throw new InvalidOperationException("Unable to Convert Database Type.");
        }

        private async Task<T> ReadSingle<T>(Type modelType, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : DatabaseModel
        {
            using var reader = await command.ExecuteReaderAsync();

            var instance = Activator.CreateInstance(modelType) as T;

            instance.Track(this);

            if (await reader.ReadAsync())
            {
                await ApplyFields(instance, modelMetadata, reader);

                instance.ApplyChanges();

                await instance.CreateDisplay(_serviceProvider);
                
                return instance;
            }
            else return null;
        }

        private async Task<List<T>> ReadMany<T>(Type modelType, DatabaseModelMetadata modelMetadata, SqliteCommand command) where T : DatabaseModel
        {
            using var reader = await command.ExecuteReaderAsync();

            var result = new List<T>();

            while (await reader.ReadAsync())
            {
                var instance = Activator.CreateInstance(modelType) as T;

                instance.Track(this);

                await ApplyFields(instance, modelMetadata, reader);

                instance.ApplyChanges();

                await instance.CreateDisplay(_serviceProvider);
                
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