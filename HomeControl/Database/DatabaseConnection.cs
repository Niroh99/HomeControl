using HomeControl.Helpers;
using HomeControl.Modeling;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Database
{
    public interface IDatabaseConnectionService : IDisposable
    {
        bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata);

        bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel;

        bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata);

        InsertQuery<T> Insert<T>(T instance) where T : DatabaseModel;

        SelectSingleIdentityKeyModelQuery<T> SelectSingle<T>(int id) where T : IdentityKeyModel;

        SelectSingleStringKeyModelQuery<T> SelectSingle<T>(string id) where T : StringKeyModel;

        SelectQuery<T> Select<T>() where T : DatabaseModel;

        UpdateQuery<T> Update<T>(T instance) where T : DatabaseModel;

        DeleteQuery<T> Delete<T>(T instance) where T : DatabaseModel;

        Task CommitTransactionAsync();
    }

    public class DatabaseConnectionService(string connectionString, IServiceProvider serviceProvider) : DatabaseConnection(connectionString), IDatabaseConnectionService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public override async Task OnInserted(DatabaseModel model)
        {
            await model.CreateDisplay(_serviceProvider);
        }

        public override async Task OnSelected(DatabaseModel model)
        {
            await model.CreateDisplay(_serviceProvider);
        }

        public override async Task OnUpdated(DatabaseModel model)
        {
            await model.CreateDisplay(_serviceProvider);
        }

        public override async Task<object> DeserializeJsonField(string valueJson)
        {
            var jsonField = await base.DeserializeJsonField(valueJson);

            if (jsonField is Model modelValue) await modelValue.CreateDisplay(_serviceProvider);

            return jsonField;
        }
    }

    public interface IDatabaseConnection : IDisposable
    {
        bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata);

        bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel;

        bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata);

        InsertQuery<T> Insert<T>(T instance) where T : DatabaseModel;

        SelectSingleIdentityKeyModelQuery<T> SelectSingle<T>(int id) where T : IdentityKeyModel;

        SelectSingleStringKeyModelQuery<T> SelectSingle<T>(string id) where T : StringKeyModel;

        SelectQuery<T> Select<T>() where T : DatabaseModel;

        UpdateQuery<T> Update<T>(T instance) where T : DatabaseModel;

        DeleteQuery<T> Delete<T>(T instance) where T : DatabaseModel;

        Task CommitTransactionAsync();
    }

    public class DatabaseConnection : IDatabaseConnection, IDisposable
    {
        static DatabaseConnection()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var modelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(DatabaseModel))))
            {
                var metadata = new DatabaseModelMetadata();

                var tableAttribute = modelType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

                if (tableAttribute != null) metadata.TableName = tableAttribute.Name;

                foreach (var property in modelType.GetProperties().Where(x => x.CanRead))
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

                ModelMetadatas[modelType] = metadata;
            }
        }

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

        public bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata)
        {
            var metadataKeyValuePair = ModelMetadatas.FirstOrDefault(x => x.Key.Name == modelName);

            modelType = metadataKeyValuePair.Key;
            metadata = metadataKeyValuePair.Value;

            return modelType != null && metadata != null;
        }

        public bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel
        {
            return TryGetMetadata(typeof(T), out metadata);
        }

        public bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata)
        {
            return ModelMetadatas.TryGetValue(modelType, out metadata);
        }

        public InsertQuery<T> Insert<T>(T instance) where T : DatabaseModel
        {
            return new InsertQuery<T>(instance, this);
        }

        public SelectSingleStringKeyModelQuery<T> SelectSingle<T>(string id) where T : StringKeyModel
        {
            return new SelectSingleStringKeyModelQuery<T>(id, this);
        }

        public SelectSingleIdentityKeyModelQuery<T> SelectSingle<T>(int id) where T : IdentityKeyModel
        {
            return new SelectSingleIdentityKeyModelQuery<T>(id, this);
        }

        public SelectQuery<T> Select<T>() where T : DatabaseModel
        {
            return new SelectQuery<T>(this);
        }

        public UpdateQuery<T> Update<T>(T instance) where T : DatabaseModel
        {
            return new UpdateQuery<T>(instance, this);
        }

        public DeleteQuery<T> Delete<T>(T instance) where T : DatabaseModel
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

    public abstract class Query<T> where T : DatabaseModel
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

    public abstract class SelectQueryBase<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection) where T : DatabaseModel
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

        public void LeftJoin<TProperty>(Expression<Func<T, TProperty>> selectorExpression)
        {
            ArgumentNullException.ThrowIfNull(selectorExpression, nameof(selectorExpression));

            LeftJoin(LinqHelper.GetExpressionMemberName(selectorExpression));
        }

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

    public abstract class SelectSingleQuery<T>(DatabaseConnection databaseConnection) : SelectQueryBase<T>(databaseConnection), IResultQuery where T : DatabaseModel
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

    public sealed class SelectSingleIdentityKeyModelQuery<T>(int id, DatabaseConnection databaseConnection) : SelectSingleQuery<T>(databaseConnection) where T : DatabaseModel
    {
        public int Id { get => id; }

        public override async Task<T> ExecuteAsync()
        {
            return await SelectSingleAsync(Id);
        }
    }

    public sealed class SelectSingleStringKeyModelQuery<T>(string id, DatabaseConnection databaseConnection) : SelectSingleQuery<T>(databaseConnection) where T : DatabaseModel
    {
        public string Id { get => id; }

        public override async Task<T> ExecuteAsync()
        {
            return await SelectSingleAsync(Id);
        }
    }

    public abstract class SelectManyQuery<T>(DatabaseConnection databaseConnection) : SelectQueryBase<T>(databaseConnection), IResultQuery where T : DatabaseModel
    {
        public abstract Task<List<T>> ExecuteAsync();

        async Task<object> IResultQuery.ExecuteAsync()
        {
            return await ExecuteAsync();
        }

        public abstract IAsyncEnumerable<T> Query();
    }

    public interface ISelectQuery : IResultQuery
    {
        ILogicalOperator Where();
    }

    public sealed class SelectQuery<T>(DatabaseConnection databaseConnection) : SelectManyQuery<T>(databaseConnection), ISelectQuery where T : DatabaseModel
    {
        private ILogicalOperator<T> _where;

        public ILogicalOperator<T> Where()
        {
            return _where = WhereBuilder.Where<T>();
        }

        ILogicalOperator ISelectQuery.Where()
        {
            return Where();
        }

        public override async Task<List<T>> ExecuteAsync()
        {
            CreateQueryData(out var modelType, out var command);

            return await ReadMany(modelType, command);
        }

        public override async IAsyncEnumerable<T> Query()
        {
            CreateQueryData(out var modelType, out var command);

            await foreach (var instance in QueryMany(modelType, command)) yield return instance;
        }

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

    public abstract class ModifingQuery<T>(DatabaseConnection databaseConnection) : Query<T>(databaseConnection), IQuery where T : DatabaseModel
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

    public sealed class InsertQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
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

    public sealed class UpdateQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
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

    public sealed class DeleteQuery<T>(T instance, DatabaseConnection databaseConnection) : ModifingQuery<T>(databaseConnection) where T : DatabaseModel
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