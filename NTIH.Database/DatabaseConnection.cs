using NTIH.Database.Modeling;
using NTIH.Database.Metadata;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using NTIH.Database.Exceptions;

namespace NTIH.Database
{
    public class DatabaseConnection : IDisposable
    {
        public const char JoinedFieldsAliasSeparator = '-';

        public const string BaseTableAlias = "-Base-";

        internal static Dictionary<Type, DatabaseTableModelMetadata> TableModelMetadatas { get; } = [];

        internal static Dictionary<Type, DatabaseModelMetadata> ModelMetadatas { get; } = [];

        public SqliteConnection SqlConnection { get; }

        private SqliteTransaction _sqlTransaction;

        public DatabaseConnection(string connectionString)
        {
            SqlConnection = new SqliteConnection(connectionString);

            SqlConnection.Open();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        public static void RegisterDatabaseModelType<T>() where T : DatabaseModel
        {
            RegisterDatabaseModelType(typeof(T));
        }

        public static void RegisterDatabaseModelType(Type databaseModelType)
        {
            if (TableModelMetadatas.ContainsKey(databaseModelType)) return;

            if (!databaseModelType.IsAssignableTo(typeof(DatabaseModel))) return;

            AddMetadata(databaseModelType, GenerateModelMetadata(databaseModelType));
        }

        private static void AddMetadata(Type databaseModelType, DatabaseModelMetadata metadata)
        {
            ModelMetadatas[databaseModelType] = metadata;

            if (metadata is DatabaseTableModelMetadata tableModelMetadata) TableModelMetadatas[databaseModelType] = tableModelMetadata;
        }

        public static void RegisterDatabaseModelTypesFromAssembly(Assembly assembly)
        {
            foreach (var databaseModelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(DatabaseModel)) && !TableModelMetadatas.ContainsKey(x)))
            {
                AddMetadata(databaseModelType, GenerateModelMetadata(databaseModelType));
            }
        }

        private static DatabaseModelMetadata GenerateModelMetadata(Type databaseModelType)
        {
            DatabaseModelMetadata metadata;

            if (databaseModelType.GetCustomAttribute(typeof(TableAttribute)) is TableAttribute tableAttribute)
            {
                metadata = new DatabaseTableModelMetadata()
                {
                    TableName = tableAttribute.Name,
                };
            }
            else metadata = new DatabaseModelMetadata();

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

        public static bool TryGetTableModelMetadata(string modelName, out Type modelType, out DatabaseTableModelMetadata metadata)
        {
            var metadataKeyValuePair = TableModelMetadatas.FirstOrDefault(x => x.Key.Name == modelName);

            modelType = metadataKeyValuePair.Key;
            metadata = metadataKeyValuePair.Value;

            return modelType != null && metadata != null;
        }

        public static bool TryGetTableModelMetadata<T>(out DatabaseTableModelMetadata metadata) where T : DatabaseTableModel
        {
            return TryGetTableModelMetadata(typeof(T), out metadata);
        }

        public static bool TryGetTableModelMetadata(Type modelType, out DatabaseTableModelMetadata metadata)
        {
            return TableModelMetadatas.TryGetValue(modelType, out metadata);
        }

        public static bool TryGetModelMetadata(string fullName, out Type modelType, out DatabaseModelMetadata metadata)
        {
            var metadataKeyValuePair = ModelMetadatas.FirstOrDefault(x => x.Key.FullName == fullName);

            modelType = metadataKeyValuePair.Key;
            metadata = metadataKeyValuePair.Value;

            return modelType != null && metadata != null;
        }

        public static bool TryGetModelMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseTableModel
        {
            return TryGetModelMetadata(typeof(T), out metadata);
        }

        public static bool TryGetModelMetadata(Type modelType, out DatabaseModelMetadata metadata)
        {
            return ModelMetadatas.TryGetValue(modelType, out metadata);
        }

        public IQuery Insert<T>(T instance) where T : DatabaseTableModel
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

        public ISelectMany<T> Select<T>() where T : DatabaseTableModel
        {
            return new SelectMany<T>(this);
        }

        public IQuery Update<T>(T instance) where T : DatabaseTableModel
        {
            return new UpdateQuery<T>(instance, this);
        }

        public IQuery Delete<T>(T instance) where T : DatabaseTableModel
        {
            return new DeleteQuery<T>(instance, this);
        }

        public async Task CommitTransactionAsync()
        {
            await _sqlTransaction.CommitAsync();

            _sqlTransaction = SqlConnection.BeginTransaction();
        }

        public virtual async Task OnInserted(DatabaseTableModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnSelected(DatabaseTableModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnUpdated(DatabaseTableModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnDeleted(DatabaseTableModel model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task<object> DeserializeJsonField(string valueJson)
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(valueJson));

            var jsonDocument = await System.Text.Json.JsonDocument.ParseAsync(memoryStream);

            var typeName = jsonDocument.RootElement.GetProperty(nameof(DatabaseTableModel.TypeName)).GetString();

            if (!TryGetModelMetadata(typeName, out var modelType, out var metadata)) throw new Exception($"Unable to Deserialize database model field");

            memoryStream.Seek(0, SeekOrigin.Begin);

            return await System.Text.Json.JsonSerializer.DeserializeAsync(memoryStream, modelType);
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

    public interface IQueryInfo
    {
        public bool HasJoin { get; }
    }

    public interface IQuery : IQueryInfo
    {
        Task ExecuteAsync();
    }

    public interface IResultQuery : IQueryInfo
    {
        Task<DatabaseTableModel> ExecuteAsync();
    }

    public interface IResultQuery<T> : IResultQuery
    {
        new Task<T> ExecuteAsync();
    }

    internal abstract class Query<T>(DatabaseConnection databaseConnection) where T : DatabaseTableModel
    {
        public SqliteConnection SqlConnection { get => DatabaseConnection.SqlConnection; }

        protected DatabaseConnection DatabaseConnection { get; } = databaseConnection;

        protected DatabaseTableModelMetadata ModelMetadata { get; } = TryGetModelMetadataAndThrow();

        private static DatabaseTableModelMetadata TryGetModelMetadataAndThrow()
        {
            if (!DatabaseConnection.TryGetTableModelMetadata<T>(out var modelMetadata)) throw new Exception("Unable to find Model Metadata.");

            return modelMetadata;
        }

        protected PrimaryKeyField GetPrimaryKey()
        {
            return GetPrimaryKey(ModelMetadata);
        }

        protected static PrimaryKeyField GetPrimaryKey(DatabaseTableModelMetadata modelMetadata)
        {
            return modelMetadata?.Fields.OfType<PrimaryKeyField>().FirstOrDefault();
        }

        protected async Task<object> ConvertDatabaseValue(DatabaseColumnField field, object databaseValue)
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

        protected static IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(DatabaseTableModelMetadata modelMetadata, Action<StringBuilder, DatabaseColumnField> append)
        {
            return EnumerateDatabaseColumnFields(modelMetadata, null, append);
        }

        protected static IEnumerable<StringBuilder> EnumerateDatabaseColumnFields(DatabaseTableModelMetadata modelMetadata, Func<DatabaseColumnField, bool> condition, Action<StringBuilder, DatabaseColumnField> append)
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

        protected static string AddFieldValueParameter(DatabaseColumnField field, object fieldValue, SqliteCommand command)
        {
            var parameterName = $"${field.Name}";

            if (fieldValue == null) fieldValue = DBNull.Value;
            else if (field.IsJson && fieldValue is not string) fieldValue = System.Text.Json.JsonSerializer.Serialize(fieldValue);

            command.Parameters.AddWithValue(parameterName, fieldValue);

            return parameterName;
        }
    }

    public interface ISelectSingle : IResultQuery
    {
        ISelectSingle LeftJoin(string propertyName);

        ISelectSingle LeftJoin(DatabaseNavigationField databaseNavigationField);
    }

    public interface ISelectSingle<T> : ISelectSingle, IResultQuery<T>
    {
        new ISelectSingle<T> LeftJoin(string propertyName);

        new ISelectSingle<T> LeftJoin(DatabaseNavigationField databaseNavigationField);
    }

    internal abstract class SelectQuery<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection), IQueryInfo where T : DatabaseTableModel
    {
        private bool _hasJoin = false;
        bool IQueryInfo.HasJoin => _hasJoin;

        private class Join(DatabaseConnection databaseConnection, DatabaseNavigationField navigationField)
        {
            public DatabaseConnection DatabaseConnection { get; } = databaseConnection;

            public DatabaseNavigationField NavigationField { get; } = navigationField;

            public Type ModelType { get => NavigationField.PropertyInfo.PropertyType; }

            private DatabaseTableModelMetadata _modelMetadata;
            public DatabaseTableModelMetadata ModelMetadata
            {
                get
                {
                    if (_modelMetadata == null)
                    {
                        if (!DatabaseConnection.TryGetTableModelMetadata(NavigationField.PropertyInfo.PropertyType, out _modelMetadata)) throw new Exception($"Unable to join {NavigationField.Name}");
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

        protected void LeftJoinCore(string propertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            var field = ModelMetadata.Fields.OfType<DatabaseNavigationField>().FirstOrDefault(field => field.Name == propertyName) ?? throw new Exception($"Unable to join {propertyName}.");

            _joins.Add(new Join(DatabaseConnection, field));

            _hasJoin = true;
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
                if (!DatabaseConnection.TryGetTableModelMetadata(join.ModelType, out var joinedModelMetadata)) throw new Exception($"Unable to join {join.NavigationField.Name}");

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
            var joinedFields = new Dictionary<string, DatabaseTableModel>();

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
                        joinedFieldValue = (DatabaseTableModel)Activator.CreateInstance(join.ModelType);

                        joinedFieldValue.Track(DatabaseConnection);

                        joinedFields[joinedFieldName] = joinedFieldValue;

                        await ApplyField(instance, ModelMetadata, joinedFieldName, joinedFieldValue);
                    }

                    await ApplyField(joinedFieldValue, join.ModelMetadata, joinedFieldFieldName, fieldValue);
                }
                else await ApplyField(instance, ModelMetadata, fieldName, fieldValue);
            }
        }

        private async Task ApplyField(DatabaseTableModel instance, DatabaseTableModelMetadata modelMetadata, string fieldName, object fieldValue)
        {
            if (fieldValue == DBNull.Value) fieldValue = null;
            else
            {
                var field = modelMetadata.Fields.OfType<DatabaseColumnField>().First(field => field.Name == fieldName);

                fieldValue = await ConvertDatabaseValue(field, fieldValue);
            }

            instance.Set(fieldValue, fieldName);
        }
    }

    internal abstract class SelectSingle<T>(DatabaseConnection databaseConnection) : SelectQuery<T>(databaseConnection), ISelectSingle<T> where T : DatabaseTableModel
    {
        public ISelectSingle<T> LeftJoin(string propertyName)
        {
            LeftJoinCore(propertyName);

            return this;
        }

        public ISelectSingle<T> LeftJoin(DatabaseNavigationField databaseNavigationField)
        {
            ArgumentNullException.ThrowIfNull(nameof(databaseNavigationField));

            return LeftJoin(databaseNavigationField.Name);
        }

        ISelectSingle ISelectSingle.LeftJoin(string propertyName) => LeftJoin(propertyName);

        ISelectSingle ISelectSingle.LeftJoin(DatabaseNavigationField databaseNavigationField) => LeftJoin(databaseNavigationField);

        public abstract Task<T> ExecuteAsync();

        async Task<DatabaseTableModel> IResultQuery.ExecuteAsync() => await ExecuteAsync();

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

    internal sealed class SelectSingleIdentityKeyModel<T>(int id, DatabaseConnection databaseConnection) : SelectSingle<T>(databaseConnection) where T : DatabaseTableModel
    {
        public int Id => id;

        public override async Task<T> ExecuteAsync() => await SelectSingleAsync(Id);
    }

    internal sealed class SelectSingleStringKeyModel<T>(string id, DatabaseConnection databaseConnection) : SelectSingle<T>(databaseConnection) where T : DatabaseTableModel
    {
        public string Id => id;

        public override async Task<T> ExecuteAsync() => await SelectSingleAsync(Id);
    }

    public interface IResultsQuery : IQueryInfo
    {
        Task<List<DatabaseTableModel>> ExecuteAsync();

        IAsyncEnumerable<DatabaseTableModel> QueryAsync();
    }

    public interface IResultsQuery<T> : IResultsQuery
    {
        new Task<List<T>> ExecuteAsync();

        new IAsyncEnumerable<T> QueryAsync();
    }

    public interface ISelectMany : IResultsQuery
    {
        ISelectMany LeftJoin(string propertyName);

        ISelectMany LeftJoin(DatabaseNavigationField databaseNavigationField);

        ILogicalOperator<ISelectMany> StartWhere();
    }

    public interface ISelectMany<T> : ISelectMany, IResultsQuery<T> where T : DatabaseTableModel
    {
        new ISelectMany<T> LeftJoin(string propertyName);

        new ISelectMany<T> LeftJoin(DatabaseNavigationField databaseNavigationField);

        ILogicalOperator<T, ISelectMany<T>> BeginWhere();
    }

    internal sealed class SelectMany<T>(DatabaseConnection databaseConnection) : SelectQuery<T>(databaseConnection), ISelectMany<T> where T : DatabaseTableModel
    {
        public ISelectMany<T> LeftJoin(string propertyName)
        {
            LeftJoinCore(propertyName);

            return this;
        }

        public ISelectMany<T> LeftJoin(DatabaseNavigationField databaseNavigationField)
        {
            ArgumentNullException.ThrowIfNull(nameof(databaseNavigationField));

            return LeftJoin(databaseNavigationField.Name);
        }

        ISelectMany ISelectMany.LeftJoin(string propertyName) => LeftJoin(propertyName);

        private ILogicalOperator<T, ISelectMany<T>> _where;

        public ILogicalOperator<T, ISelectMany<T>> BeginWhere() => _where = WhereBuilder.Where<T, ISelectMany<T>>(this);

        ISelectMany ISelectMany.LeftJoin(DatabaseNavigationField databaseNavigationField) => LeftJoin(databaseNavigationField);

        ILogicalOperator<ISelectMany> ISelectMany.StartWhere() => (ILogicalOperator<ISelectMany>)BeginWhere();

        public async Task<List<T>> ExecuteAsync()
        {
            CreateQueryData(out var modelType, out var command);

            return await ReadMany(modelType, command);
        }

        async Task<List<DatabaseTableModel>> IResultsQuery.ExecuteAsync() => [.. await ExecuteAsync()];

        public async IAsyncEnumerable<T> QueryAsync()
        {
            CreateQueryData(out var modelType, out var command);

            await foreach (var instance in QueryMany(modelType, command)) yield return instance;
        }

        IAsyncEnumerable<DatabaseTableModel> IResultsQuery.QueryAsync() => QueryAsync();

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

    internal abstract class ModifingQuery<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection), IQuery, IQueryInfo where T : DatabaseTableModel
    {
        public bool HasJoin => false;

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

            foreach (var uniqueField in ModelMetadata.Fields.OfType<DatabaseColumnField>().Where(x => x.IsUnique && x != primaryKey))
            {
                if (modifiedProperties.Length > 0 && Array.BinarySearch(modifiedProperties, uniqueField.Name) < 0) continue;

                var value = uniqueField.PropertyInfo.GetValue(instance);

                if (value == null) continue;

                var select = DatabaseConnection.Select<T>();

                var where = select.BeginWhere().Compare(uniqueField, ComparisonOperator.Equals, value);

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

    internal sealed class InsertQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseTableModel
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

    internal sealed class UpdateQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseTableModel
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

    internal sealed class DeleteQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseTableModel
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