using HomeControl.Sql;
using Microsoft.Data.Sqlite;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using HomeControl.Modeling;

namespace HomeControl.Models
{
    public abstract class SqLiteModel : Model
    {
        static SqLiteModel()
        {
            var modelTypes = new List<Type>
            {
                typeof(Device)
            };

            foreach (var modelType in modelTypes)
            {
                var metadata = new ModelMetadata<SqlField>();

                var tableAttribute = modelType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

                if (tableAttribute == null) metadata.TableName = modelType.Name;
                else metadata.TableName = tableAttribute.Name;

                foreach (var property in modelType.GetProperties().Where(x => x.CanRead && x.CanWrite))
                {
                    var columnAttribute = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                    if (columnAttribute == null) continue;

                    var keyAttribute = property.GetCustomAttribute(typeof(KeyAttribute));

                    if (keyAttribute == null) metadata.Fields.Add(new SqlField(property.Name, columnAttribute.Name));
                    else
                    {
                        var identityAttribute = property.GetCustomAttribute(typeof(IdentityAttribute));

                        if (identityAttribute == null) metadata.Fields.Add(new PrimaryKeyField(property.Name, false, columnAttribute.Name));
                        else metadata.Fields.Add(new PrimaryKeyField(property.Name, true, columnAttribute.Name));
                    }
                }

                ModelMetadatas[modelType] = metadata;
            }
        }

        private static Dictionary<Type, ModelMetadata<SqlField>> ModelMetadatas { get; } = new Dictionary<Type, ModelMetadata<SqlField>>();

        protected static T Select<T>(object id) where T : SqLiteModel
        {
            return Database.Connect((sqlConnection) =>
            {
                var modelType = typeof(T);

                var modelMetadata = ModelMetadatas[modelType];

                var instance = Activator.CreateInstance(modelType) as T;

                using (var command = sqlConnection.CreateCommand())
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

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ApplyFields(instance, reader);

                            return instance;
                        }
                        else return null;
                    }
                }
            });
        }

        protected static List<T> SelectAll<T>() where T : SqLiteModel
        {
            return Database.Connect((sqlConnection) =>
            {
                var modelType = typeof(T);

                var modelMetadata = ModelMetadatas[modelType];

                using (var command = sqlConnection.CreateCommand())
                {
                    var commandStringBuilder = BuildSelect(modelMetadata);

                    command.CommandText = commandStringBuilder.ToString();

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<T>();

                        while (reader.Read())
                        {
                            var instance = Activator.CreateInstance(modelType) as T;

                            ApplyFields(instance, reader);

                            result.Add(instance);
                        }

                        return result;
                    }
                }
            });
        }

        protected static void Insert<T>(T instance) where T : SqLiteModel
        {
            Database.Connect((sqlConnection) =>
            {
                var modelType = typeof(T);

                var modelMetadata = ModelMetadatas[modelType];

                using (var command = sqlConnection.CreateCommand())
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
                        var id = command.ExecuteScalar();

                        instance.Set(id, primaryKey.Name);
                    }
                    else command.ExecuteNonQuery();
                }
            });
        }

        public void Delete()
        {
            Database.Connect((sqlConnection) =>
            {
                var modelType = GetType();

                var modelMetadata = ModelMetadatas[modelType];

                using (var command = sqlConnection.CreateCommand())
                {
                    var commandStringBuilder = new StringBuilder("DELETE FROM ")
                        .Append($"[{modelMetadata.TableName}]")
                        .Append(" WHERE ");

                    var primaryKey = GetPrimaryKey(modelMetadata);

                    if (primaryKey != null && primaryKey.IsIdentity) commandStringBuilder.Append(DeleteField(primaryKey, command));
                    else commandStringBuilder.Append(string.Join(" AND ", modelMetadata.Fields.Select(x => DeleteField(x, command))));

                    command.CommandText = commandStringBuilder.ToString();

                    command.ExecuteNonQuery();
                }
            });
        }

        private static PrimaryKeyField GetPrimaryKey(ModelMetadata<SqlField> modelMetadata)
        {
            return modelMetadata?.Fields.OfType<PrimaryKeyField>().FirstOrDefault();
        }

        private static bool IsIdentity(SqlField field)
        {
            return field is PrimaryKeyField primaryKeyField && primaryKeyField.IsIdentity;
        }

        private static string SelectField(SqlField field)
        {
            return $"[{field.ColumnName}] AS [{field.Name}]";
        }

        private static StringBuilder BuildSelect(ModelMetadata<SqlField> modelMetadata)
        {
            var commandStringBuilder = new StringBuilder("SELECT ");

            commandStringBuilder.Append(string.Join(", ", modelMetadata.Fields.Select(x => SelectField(x))));

            return commandStringBuilder.Append(" FROM ").Append($"[{modelMetadata.TableName}]");
        }

        private static void ApplyFields<T>(T instance, SqliteDataReader reader) where T : SqLiteModel
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);

                instance.Set(fieldValue, fieldName);
            }
        }

        private static string InsertField<T>(SqlField field, T instance, SqliteCommand command) where T : SqLiteModel
        {
            var parameterName = $"${field.Name}";

            command.Parameters.AddWithValue(parameterName, instance.Get<object>(field.Name));

            return parameterName;
        }

        private string DeleteField(SqlField field, SqliteCommand command)
        {
            var parameterName = $"${field.Name}";

            command.Parameters.AddWithValue(parameterName, Get<object>(field.Name));

            return $"[{field.ColumnName}] = {parameterName}" ;
        }
    }
}