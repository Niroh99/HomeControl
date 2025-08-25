using NTIH.Database.Modeling;
using NTIH.Database.Metadata;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NTIH.Database.Exceptions;

namespace NTIH.Database
{
    public class DatabaseConnection : IDisposable
    {
        public const char JoinedFieldsAliasSeparator = '-';

        public const string BaseTableAlias = "-Base-";

        internal static Dictionary<Type, DatabaseModelMetadata> ModelMetadatas { get; } = [];

        public SqliteConnection SqlConnection { get; }

        private SqliteTransaction _sqlTransaction;

        public DatabaseConnection(string connectionString)
        {
            SqlConnection = new SqliteConnection(connectionString);

            SqlConnection.Open();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        public static void RegisterModelType<T>()
        {
            var modelType = typeof(T);
            RegisterDatabaseModelType(modelType);
        }

        public static void RegisterDatabaseModelType(Type databaseModelType)
        {
            if (ModelMetadatas.ContainsKey(databaseModelType)) return;

            if (!databaseModelType.IsAssignableTo(typeof(DatabaseModel))) return;

            ModelMetadatas[databaseModelType] = GenerateModelMetadata(databaseModelType);
        }

        public static void RegisterDatabaseModelTypesFromAssembly(Assembly assembly)
        {
            foreach (var databaseModelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(DatabaseModel)) && !ModelMetadatas.ContainsKey(x)))
            {
                ModelMetadatas[databaseModelType] = GenerateModelMetadata(databaseModelType);
            }
        }

        private static DatabaseModelMetadata GenerateModelMetadata(Type databaseModelType)
        {
            var metadata = new DatabaseModelMetadata();

            var tableAttribute = databaseModelType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

            if (tableAttribute != null) metadata.TableName = tableAttribute.Name;

            foreach (var property in databaseModelType.GetProperties().Where(x => x.CanRead))
            {
                var columnAttribute = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                var foreignKeyAttribute = property.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;

                if (columnAttribute == null && foreignKeyAttribute == null) metadata.Fields.Add(new DatabaseField(property));
                if (columnAttribute != null)
                {
                    var keyAttribute = property.GetCustomAttribute(typeof(KeyAttribute));

                    if (keyAttribute == null) metadata.Fields.Add(new DatabaseColumnField(property, columnAttribute.Name));
                    else metadata.Fields.Add(new PrimaryKeyField(property, columnAttribute.Name));
                }
                else if (foreignKeyAttribute != null)
                {
                    metadata.Fields.Add(new DatabaseNavigationField(property, foreignKeyAttribute.Name));
                }
            }

            return metadata;
        }

        public static bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata)
        {
            var metadataKeyValuePair = ModelMetadatas.FirstOrDefault(x => x.Key.Name == modelName);

            modelType = metadataKeyValuePair.Key;
            metadata = metadataKeyValuePair.Value;

            return modelType != null && metadata != null;
        }

        public static bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel
        {
            return TryGetMetadata(typeof(T), out metadata);
        }

        public static bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata)
        {
            return ModelMetadatas.TryGetValue(modelType, out metadata);
        }

        public IQuery Insert<T>(T instance) where T : DatabaseModel
        {
            return new InsertQuery<T>(instance, this);
        }

        public ISelectSingle<T> SelectSingle<T>(string id) where T : StringKeyModel
        {
            return new SelectSingleStringKeyModel<T>(id, this);
        }

        public ISelectSingle<T> SelectSingle<T>(int id) where T : IdentityKeyModel
        {
            return new SelectSingleIdentityKeyModel<T>(id, this);
        }

        public ISelectMany<T> Select<T>() where T : DatabaseModel
        {
            return new SelectMany<T>(this);
        }

        public IQuery Update<T>(T instance) where T : DatabaseModel
        {
            return new UpdateQuery<T>(instance, this);
        }

        public IQuery Delete<T>(T instance) where T : DatabaseModel
        {
            return new DeleteQuery<T>(instance, this);
        }

        public async Task CommitTransactionAsync()
        {
            await _sqlTransaction.CommitAsync();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        public virtual async Task OnInserted(DatabaseModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnSelected(DatabaseModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnUpdated(DatabaseModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnDeleted(DatabaseModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task<object> DeserializeJsonField(string valueJson)
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(valueJson));

            var jsonDocument = await System.Text.Json.JsonDocument.ParseAsync(memoryStream);

            var typeName = jsonDocument.RootElement.GetProperty(nameof(DatabaseModel.TypeName)).GetString();

            var jsonObjectType = Assembly.GetExecutingAssembly().GetType(typeName);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return await System.Text.Json.JsonSerializer.DeserializeAsync(memoryStream, jsonObjectType);
        }

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            _sqlTransaction.Commit();

            SqlConnection.Close();
            SqlConnection.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public interface IQuery
    {
        Task ExecuteAsync();
    }

    public interface IResultQuery
    {
        Task<object> ExecuteAsync();
    }

    public interface IResultQuery<T> : IResultQuery
    {
        new Task<T> ExecuteAsync();
    }

    internal abstract class Query<T> where T : DatabaseModel
    {
        public Query(DatabaseConnection databaseConnection)
        {
            DatabaseConnection = databaseConnection;

            ModelMetadata = TryGetModelMetadataAndThrow();
        }

        public SqliteConnection SqlConnection { get => DatabaseConnection.SqlConnection; }

        protected DatabaseConnection DatabaseConnection { get; }

        protected DatabaseModelMetadata ModelMetadata { get; }

        private DatabaseModelMetadata TryGetModelMetadataAndThrow()
        {
            if (!DatabaseConnection.TryGetMetadata<T>(out var modelMetadata)) throw new Exception("Unable to find Model Metadata.");

            return modelMetadata;
        }

        protected PrimaryKeyField GetPrimaryKey()
        {
            return GetPrimaryKey(ModelMetadata);
        }

        protected PrimaryKeyField GetPrimaryKey(DatabaseModelMetadata modelMetadata)
        {
            return modelMetadata?.Fields.OfType<PrimaryKeyField>().FirstOrDefault();
        }

        protected async Task<object> ConvertDatabaseValue(DatabaseField field, object databaseValue)
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
                return await DatabaseConnection.DeserializeJsonField(valueJson);
            }

            if (propertyType.IsEnum && valueType == typeof(string))
            {
                return Enum.Parse(propertyType, (string)databaseValue);
            }
            else if (propertyType == typeof(int) && databaseValue is long longValue)
            {
                return (int)longValue;
            }
            else if (propertyType == typeof(decimal))
            {
                if (databaseValue is long decimalLongValue) return (decimal)decimalLongValue;
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

        protected IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(Action<StringBuilder, DatabaseColumnField> append)
        {
            return EnumerateDatabaseColumnFields(ModelMetadata, null, append);
        }

        protected IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(Func<DatabaseColumnField, bool> condition, Action<StringBuilder, DatabaseColumnField> append)
        {
            return EnumerateDatabaseColumnFields(ModelMetadata, condition, append);
        }

        protected IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(DatabaseModelMetadata modelMetadata, Action<StringBuilder, DatabaseColumnField> append)
        {
            return EnumerateDatabaseColumnFields(modelMetadata, null, append);
        }

        protected IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(DatabaseModelMetadata modelMetadata, Func<DatabaseColumnField, bool> condition, Action<StringBuilder, DatabaseColumnField> append)
        {
            var fieldStringBuilder = new StringBuilder();

            IEnumerable<DatabaseColumnField> fields;

            if (condition == null) fields = modelMetadata.Fields.OfType<DatabaseColumnField>();
            else fields = modelMetadata.Fields.OfType<DatabaseColumnField>().Where(condition);

            foreach (var field in fields)
            {
                append(fieldStringBuilder, field);

                yield return fieldStringBuilder;

                fieldStringBuilder.Clear();
            }
        }

        protected string AddFieldValueParameter(DatabaseColumnField field, object fieldValue, SqliteCommand command)
        {
            var parameterName = $"${field.Name}";

            if (fieldValue == null) fieldValue = DBNull.Value;
            else if (field.IsJson && fieldValue is not string) fieldValue = System.Text.Json.JsonSerializer.Serialize(fieldValue);

            command.Parameters.AddWithValue(parameterName, fieldValue);

            return parameterName;
        }
    }

    public interface IJoinable
    {
        void LeftJoin(string propertyName);
    }

    public interface IJoinable<T> : IJoinable
    {

    }

    public interface ISelectSingle : IJoinable, IResultQuery
    {

    }

    public interface ISelectSingle<T> : ISelectSingle, IJoinable<T>, IResultQuery<T>
    {

    }

    internal abstract class SelectQuery<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection), IJoinable<T> where T : DatabaseModel
    {
        private class Join(DatabaseConnection databaseConnection, DatabaseNavigationField navigationField)
        {
            public DatabaseConnection DatabaseConnection { get; } = databaseConnection;

            public DatabaseNavigationField NavigationField { get; } = navigationField;

            public Type ModelType { get => NavigationField.PropertyInfo.PropertyType; }

            private DatabaseModelMetadata _modelMetadata;
            public DatabaseModelMetadata ModelMetadata
            {
                get
                {
                    if (_modelMetadata == null)
                    {
                        if (!DatabaseConnection.TryGetMetadata(NavigationField.PropertyInfo.PropertyType, out _modelMetadata)) throw new Exception($"Unable to join {NavigationField.Name}");
                    }

                    return _modelMetadata;
                }
            }

            public override int GetHashCode()
            {
                return NavigationField.Name.GetHashCode();
            }
        }

        private readonly HashSet<Join> _joins = [];

        public void LeftJoin(string propertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            var field = ModelMetadata.Fields.OfType<DatabaseNavigationField>().FirstOrDefault(field => field.Name == propertyName) ?? throw new Exception($"Unable to join {propertyName}.");

            _joins.Add(new Join(DatabaseConnection, field));
        }

        protected StringBuilder BuildSelect()
        {
            var commandStringBuilder = new StringBuilder("SELECT ");

            var fields = EnumerateDatabaseColumnFields((fieldStringBuilder, field) =>
                fieldStringBuilder.Append('[').Append(DatabaseConnection.BaseTableAlias)
                    .Append("].[")
                    .Append(field.ColumnName)
                    .Append("] AS [")
                    .Append(field.Name)
                    .Append(']'));

            var joinsStringBuilder = new StringBuilder();

            foreach (var join in _joins)
            {
                if (!DatabaseConnection.TryGetMetadata(join.ModelType, out var joinedModelMetadata)) throw new Exception($"Unable to join {join.NavigationField.Name}");

                var foreignKeyField = ModelMetadata.Fields.OfType<DatabaseColumnField>().FirstOrDefault(x => x.Name == join.NavigationField.ForeignKeyFieldName) ?? throw new Exception($"Unable to join {join.NavigationField.Name}");

                var joinedModelPrimaryKey = GetPrimaryKey(joinedModelMetadata);

                joinsStringBuilder.Append(" LEFT JOIN [")
                    .Append(joinedModelMetadata.TableName)
                    .Append("] AS [")
                    .Append(join.NavigationField.Name)
                    .Append("] ON [")
                    .Append(foreignKeyField.ColumnName)
                    .Append("] = [")
                    .Append(join.NavigationField.Name)
                    .Append("].[")
                    .Append(joinedModelPrimaryKey.ColumnName)
                    .Append(']');

                fields = fields.Concat(EnumerateDatabaseColumnFields(joinedModelMetadata, (fieldStringBuilder, field) =>
                    fieldStringBuilder.Append('[')
                        .Append(join.NavigationField.Name)
                        .Append("].[")
                        .Append(field.ColumnName)
                        .Append("] AS [")
                        .Append(join.NavigationField.Name)
                        .Append(DatabaseConnection.JoinedFieldsAliasSeparator)
                        .Append(field.Name)
                        .Append(']')));
            }

            commandStringBuilder.AppendJoin(", ", fields);

            commandStringBuilder.Append(" FROM [")
                .Append(ModelMetadata.TableName)
                .Append("] AS [")
                .Append(DatabaseConnection.BaseTableAlias)
                .Append(']');

            commandStringBuilder.Append(joinsStringBuilder);

            return commandStringBuilder;
        }

        protected async Task<T> ReadSingle(Type modelType, SqliteCommand command)
        {
            using var reader = await command.ExecuteReaderAsync();

            var instance = Activator.CreateInstance(modelType) as T;

            instance.Track(DatabaseConnection);

            if (await reader.ReadAsync())
            {
                await ApplyFields(instance, reader);

                instance.ApplyChanges();

                await DatabaseConnection.OnSelected(instance);

                return instance;
            }
            else return null;
        }

        protected async IAsyncEnumerable<T> QueryMany(Type modelType, SqliteCommand command)
        {
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var instance = Activator.CreateInstance(modelType) as T;

                instance.Track(DatabaseConnection);

                await ApplyFields(instance, reader);

                instance.ApplyChanges();

                await DatabaseConnection.OnSelected(instance);

                yield return instance;
            }
        }

        protected async Task<List<T>> ReadMany(Type modelType, SqliteCommand command)
        {
            var result = new List<T>();

            await foreach (var instance in QueryMany(modelType, command))
            {
                result.Add(instance);
            }

            return result;
        }

        private async Task ApplyFields(T instance, SqliteDataReader reader)
        {
            var joinedFields = new Dictionary<string, DatabaseModel>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);

                if (fieldName.Contains(DatabaseConnection.JoinedFieldsAliasSeparator, StringComparison.Ordinal))
                {
                    var splitFieldName = fieldName.Split(DatabaseConnection.JoinedFieldsAliasSeparator);

                    if (splitFieldName.Length != 2) throw new Exception($"Something went wrong with field {fieldName}");

                    var joinedFieldName = splitFieldName[0];

                    var join = _joins.FirstOrDefault(x => x.NavigationField.Name == joinedFieldName) ?? throw new Exception($"Something went wrong with field {fieldName}");

                    var joinedFieldFieldName = splitFieldName[1];

                    if (!joinedFields.TryGetValue(joinedFieldName, out var joinedFieldValue))
                    {
                        joinedFieldValue = (DatabaseModel)Activator.CreateInstance(join.ModelType);

                        joinedFieldValue.Track(DatabaseConnection);

                        joinedFields[joinedFieldName] = joinedFieldValue;

                        await ApplyField(instance, ModelMetadata, joinedFieldName, joinedFieldValue);
                    }

                    await ApplyField(joinedFieldValue, join.ModelMetadata, joinedFieldFieldName, fieldValue);
                }
                else await ApplyField(instance, ModelMetadata, fieldName, fieldValue);
            }
        }

        private async Task ApplyField(DatabaseModel instance, DatabaseModelMetadata modelMetadata, string fieldName, object fieldValue)
        {
            if (fieldValue == DBNull.Value) fieldValue = null;
            else
            {
                var field = modelMetadata.Fields.First(field => field.Name == fieldName);

                fieldValue = await ConvertDatabaseValue(field, fieldValue);
            }

            instance.Set(fieldValue, fieldName);
        }
    }

    internal abstract class SelectSingle<T>(DatabaseConnection databaseConnection) : SelectQuery<T>(databaseConnection), ISelectSingle<T> where T : DatabaseModel
    {
        public abstract Task<T> ExecuteAsync();

        async Task<object> IResultQuery.ExecuteAsync() => await ExecuteAsync();

        protected async Task<T> SelectSingleAsync<TKey>(TKey id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var modelType = typeof(T);

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = BuildSelect();

            var primaryKey = GetPrimaryKey() ?? throw new Exception("Something went wrong.");

            commandStringBuilder.Append(" WHERE [")
                .Append(DatabaseConnection.BaseTableAlias)
                .Append("].[")
                .Append(primaryKey.ColumnName)
                .Append("] = ")
                .Append(AddFieldValueParameter(primaryKey, id, command));

            command.CommandText = commandStringBuilder.ToString();

            return await ReadSingle(modelType, command);
        }
    }

    internal sealed class SelectSingleIdentityKeyModel<T>(int id, DatabaseConnection databaseConnection) : SelectSingle<T>(databaseConnection) where T : DatabaseModel
    {
        public int Id => id;

        public override async Task<T> ExecuteAsync() => await SelectSingleAsync(Id);
    }

    internal sealed class SelectSingleStringKeyModel<T>(string id, DatabaseConnection databaseConnection) : SelectSingle<T>(databaseConnection) where T : DatabaseModel
    {
        public string Id => id;

        public override async Task<T> ExecuteAsync() => await SelectSingleAsync(Id);
    }

    public interface IResultsQuery
    {
        Task<List<object>> ExecuteAsync();

        IAsyncEnumerable<object> QueryAsync();
    }

    public interface IResultsQuery<T> : IResultsQuery
    {
        new Task<List<T>> ExecuteAsync();

        new IAsyncEnumerable<T> QueryAsync();
    }

    public interface ISelectMany : IJoinable, IResultsQuery
    {
        ILogicalOperator Where();
    }

    public interface ISelectMany<T> : ISelectMany, IJoinable<T>, IResultsQuery<T> where T : DatabaseModel
    {
        new ILogicalOperator<T> Where();
    }

    internal sealed class SelectMany<T>(DatabaseConnection databaseConnection) : SelectQuery<T>(databaseConnection), ISelectMany<T> where T : DatabaseModel
    {
        private ILogicalOperator<T> _where;

        public ILogicalOperator<T> Where() => _where = WhereBuilder.Where<T>();

        ILogicalOperator ISelectMany.Where() => Where();

        public async Task<List<T>> ExecuteAsync()
        {
            CreateQueryData(out var modelType, out var command);

            return await ReadMany(modelType, command);
        }

        async Task<List<object>> IResultsQuery.ExecuteAsync() => [.. await ExecuteAsync()];

        public async IAsyncEnumerable<T> QueryAsync()
        {
            CreateQueryData(out var modelType, out var command);

            await foreach (var instance in QueryMany(modelType, command)) yield return instance;
        }

        IAsyncEnumerable<object> IResultsQuery.QueryAsync() => QueryAsync();

        private void CreateQueryData(out Type modelType, out SqliteCommand command)
        {
            modelType = typeof(T);
            command = SqlConnection.CreateCommand();

            var commandStringBuilder = BuildSelect();

            if (_where != null)
            {
                commandStringBuilder.Append(" WHERE ");
                commandStringBuilder.Append(_where.BuildWhere(out var parameterValues));

                foreach (var parameterValue in parameterValues) command.Parameters.AddWithValue(parameterValue.Key, parameterValue.Value);
            }

            command.CommandText = commandStringBuilder.ToString();
        }
    }

    internal abstract class ModifingQuery<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection), IQuery where T : DatabaseModel
    {
        protected void AppendInstanceWhere(StringBuilder commandStringBuilder, T instance, SqliteCommand command)
        {
            commandStringBuilder.Append(" WHERE ");

            var primaryKey = GetPrimaryKey();

            if (primaryKey != null)
            {
                commandStringBuilder.Append('[')
                    .Append(primaryKey.ColumnName)
                    .Append("] = ")
                    .Append(AddFieldParameter(primaryKey, instance, command));
            }
            else
            {
                commandStringBuilder.AppendJoin(" AND ", EnumerateDatabaseColumnFields((fieldStringBuilder, field) =>
                    fieldStringBuilder.Append('[')
                        .Append(primaryKey.ColumnName)
                        .Append("] = ")
                        .Append(AddFieldParameter(field, instance, command))));
            }
        }

        protected async Task CheckUniqueFieldViolationAndThrow(T instance)
        {
            await CheckUniqueFieldViolationAndThrow(instance, []);
        }

        protected async Task CheckUniqueFieldViolationAndThrow(T instance, string[] modifiedProperties)
        {
            var primaryKey = GetPrimaryKey();

            foreach (var uniqueField in ModelMetadata.Fields.Where(x => x.IsUnique && x != primaryKey).OfType<DatabaseColumnField>())
            {
                if (modifiedProperties.Length > 0 && Array.BinarySearch(modifiedProperties, uniqueField.Name) < 0) continue;

                var value = uniqueField.PropertyInfo.GetValue(instance);

                if (value == null) continue;

                var select = DatabaseConnection.Select<T>();

                var where = select.Where().Compare(uniqueField, ComparisonOperator.Equals, value);

                if (instance.IsTracked) where.And().Compare(primaryKey, ComparisonOperator.NotEquals, primaryKey.Get(instance));

                var existingModels = await select.ExecuteAsync();

                if (existingModels.Count > 0) throw new UniqueFieldViolationException(uniqueField.Name);
            }
        }

        protected bool IsIdentity(DatabaseColumnField field)
        {
            return field is PrimaryKeyField primaryKeyField && primaryKeyField.IsIdentity;
        }

        protected string AddFieldParameter(DatabaseColumnField field, T instance, SqliteCommand command)
        {
            var fieldValue = field.Get(instance);

            return AddFieldValueParameter(field, fieldValue, command);
        }

        public abstract Task ExecuteAsync();
    }

    internal sealed class InsertQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
    {
        public async override Task ExecuteAsync()
        {
            instance.OnInserting();

            await CheckUniqueFieldViolationAndThrow(instance);

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder("INSERT INTO ")
                .Append('[')
                .Append(ModelMetadata.TableName)
                .Append(']')
                .Append('(');

            commandStringBuilder.AppendJoin(", ", EnumerateDatabaseColumnFields(field => !IsIdentity(field), (fieldStringBuilder, field) =>
                fieldStringBuilder.Append('[')
                    .Append(field.ColumnName)
                    .Append(']')));

            commandStringBuilder.Append(')');

            commandStringBuilder.Append(" VALUES")
                .Append('(');

            commandStringBuilder.AppendJoin(", ", EnumerateDatabaseColumnFields(field => !IsIdentity(field), (fieldStringBuilder, field) =>
                fieldStringBuilder.Append(AddFieldParameter(field, instance, command))));

            commandStringBuilder.Append(')');

            var primaryKey = GetPrimaryKey();

            var isIdentity = primaryKey != null && primaryKey.IsIdentity;

            if (isIdentity) commandStringBuilder.Append("; SELECT LAST_INSERT_ROWID()");

            command.CommandText = commandStringBuilder.ToString();

            if (isIdentity)
            {
                var id = await command.ExecuteScalarAsync();

                id = await ConvertDatabaseValue(primaryKey, id);

                instance.Set(id, primaryKey.Name);
            }
            else await command.ExecuteNonQueryAsync();

            instance.ApplyChanges();

            await DatabaseConnection.OnInserted(instance);
        }
    }

    internal sealed class UpdateQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
    {
        public async override Task ExecuteAsync()
        {
            instance.OnUpdating();

            var modfiedProperies = instance.GetModifiedProperties();

            if (modfiedProperies.Length == 0) return;

            await CheckUniqueFieldViolationAndThrow(instance, modfiedProperies);

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder("UPDATE [")
                .Append(ModelMetadata.TableName)
                .Append("] SET ");

            for (int i = 0; i < modfiedProperies.Length; i++)
            {
                var modifiedProperty = modfiedProperies[i];

                var field = ModelMetadata.Fields.OfType<DatabaseColumnField>().First(f => f.Name == modifiedProperty);

                commandStringBuilder.Append('[')
                    .Append(field.ColumnName)
                    .Append("] = ")
                    .Append(AddFieldParameter(field, instance, command));

                if (i < modfiedProperies.Length - 1) commandStringBuilder.Append(", ");
            }

            AppendInstanceWhere(commandStringBuilder, instance, command);

            command.CommandText = commandStringBuilder.ToString();

            await command.ExecuteNonQueryAsync();

            instance.ApplyChanges();

            await DatabaseConnection.OnUpdated(instance);
        }
    }

    internal sealed class DeleteQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
    {
        public async override Task ExecuteAsync()
        {
            instance.OnDeleting();

            using var command = SqlConnection.CreateCommand();

            var commandStringBuilder = new StringBuilder("DELETE FROM ")
                .Append('[')
                .Append(ModelMetadata.TableName)
                .Append(']');

            AppendInstanceWhere(commandStringBuilder, instance, command);

            command.CommandText = commandStringBuilder.ToString();

            await command.ExecuteNonQueryAsync();

            await DatabaseConnection.OnDeleted(instance);
        }
    }
}