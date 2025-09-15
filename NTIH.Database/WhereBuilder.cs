using NTIH.Modeling;
using NTIH.Database.Metadata;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NTIH.Database.Modeling;

namespace NTIH.Database
{
    internal static class WhereBuilder
    {
        public const char ParameterIndicator = '$';

        public class ParameterCollection
        {
            private readonly List<object> _parameterValues = [];

            public string AddParameter(object value)
            {
                _parameterValues.Add(value);

                return GetParameterName(_parameterValues.Count - 1);
            }

            public Dictionary<string, object> GetParameterValues()
            {
                if (_parameterValues.Count == 0) return [];

                var result = new Dictionary<string, object>();

                for (int i = 0; i < _parameterValues.Count; i++)
                {
                    result[GetParameterName(i)] = _parameterValues[i];
                }

                return result;
            }

            private static string GetParameterName(int index)
            {
                return $"{ParameterIndicator}{index}";
            }
        }

        private abstract class WhereElement<T, TQuery>(TQuery query) : IWhereElement<TQuery> where T : DatabaseModel
        {
            private WhereElement<T, TQuery> _parent;

            private WhereElement<T, TQuery> _nextElement;

            public WhereElement<T, TQuery> Parent { get => _parent; }

            public WhereElement<T, TQuery> NextElement { get => _nextElement; }

            public string BuildWhere(out Dictionary<string, object> parameterValues)
            {
                if (Parent != null) return Parent.BuildWhere(out parameterValues);

                var whereStringBuilder = new StringBuilder();

                var parameters = new ParameterCollection();

                BuildWhereCore(this, whereStringBuilder, parameters);

                parameterValues = parameters.GetParameterValues();

                return whereStringBuilder.ToString();
            }

            public TQuery EndWhere()
            {
                return query;
            }

            protected TChild SetNextElement<TChild>(TChild child) where TChild : WhereElement<T, TQuery>
            {
                child._parent = this;

                _nextElement = child;

                return child;
            }

            public abstract void Append(StringBuilder builder, ParameterCollection parameters);
        }

        private abstract class LogicalOperator<T, TQuery>(TQuery query) : WhereElement<T, TQuery>(query), ILogicalOperator<T, TQuery> where T : DatabaseModel
        {
            IStatement<TQuery> ILogicalOperator<TQuery>.Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value) => Compare(databaseColumnField, comparisonOperator, value);

            IStatement<TQuery> ILogicalOperator<TQuery>.Compare(string columnName, ComparisonOperator comparisonOperator, object value) => Compare(columnName, comparisonOperator, value);

            IStatement<TQuery> ILogicalOperator<TQuery>.IsNull(DatabaseColumnField databaseColumnField) => IsNull(databaseColumnField);

            IStatement<TQuery> ILogicalOperator<TQuery>.IsNull(string columnName) => IsNull(columnName);

            IStatement<TQuery> ILogicalOperator<TQuery>.IsNotNull(DatabaseColumnField databaseColumnField) => IsNotNull(databaseColumnField);

            IStatement<TQuery> ILogicalOperator<TQuery>.IsNotNull(string columnName) => IsNotNull(columnName);

            IStatement<TQuery> ILogicalOperator<TQuery>.Brackets(Action<ILogicalOperator<TQuery>> buildChild) => Brackets(buildChild);

            public IStatement<T, TQuery> Compare<TProperty>(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, TProperty value)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return Compare(databaseColumnField.Name, comparisonOperator, value);
            }

            public IStatement<T, TQuery> Compare<TProperty>(string columnName, ComparisonOperator comparisonOperator, TProperty value)
            {
                return SetNextElement(new ValueComparison<T, TQuery>(query, columnName, comparisonOperator, value));
            }

            public IStatement<T, TQuery> IsNull(DatabaseColumnField databaseColumnField)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return IsNull(databaseColumnField.ColumnName);
            }

            public IStatement<T, TQuery> IsNull(string columnName)
            {
                return SetNextElement(new NullComparison<T, TQuery>(query, columnName, false));
            }

            public IStatement<T, TQuery> IsNotNull(DatabaseColumnField databaseColumnField)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return IsNotNull(databaseColumnField.ColumnName);
            }

            public IStatement<T, TQuery> IsNotNull(string columnName)
            {
                return SetNextElement(new NullComparison<T, TQuery>(query, columnName, true));
            }

            public IStatement<T, TQuery> Brackets(Action<ILogicalOperator<TQuery>> buildChild)
            {
                var brackets = new Brackets<T, TQuery>(query);

                buildChild(brackets.Child);

                return SetNextElement(brackets);
            }
        }

        private sealed class RootElement<T, TQuery>(TQuery query) : LogicalOperator<T, TQuery>(query) where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {

            }
        }

        private sealed class And<T, TQuery>(TQuery query) : LogicalOperator<T, TQuery>(query) where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append(" AND ");
            }
        }

        private sealed class Or<T, TQuery>(TQuery query) : LogicalOperator<T, TQuery>(query) where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append(" OR ");
            }
        }

        private abstract class Statement<T, TQuery>(TQuery query) : WhereElement<T, TQuery>(query), IStatement<T, TQuery> where T : DatabaseModel
        {
            ILogicalOperator<TQuery> IStatement<TQuery>.And() => And();

            ILogicalOperator<TQuery> IStatement<TQuery>.Or() => Or();

            public ILogicalOperator<T, TQuery> And() => SetNextElement(new And<T, TQuery>(query));

            public ILogicalOperator<T, TQuery> Or() => SetNextElement(new Or<T, TQuery>(query));
        }

        private sealed class Brackets<T, TQuery> : Statement<T, TQuery>, IStatement<T, TQuery> where T : DatabaseModel
        {
            public Brackets(TQuery query) : base(query)
            {
                _child = new RootElement<T, TQuery>(query);
            }

            private readonly RootElement<T, TQuery> _child;
            public ILogicalOperator<T, TQuery> Child { get => _child; }

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append('(');
                BuildWhereCore(_child, builder, parameters);
                builder.Append(')');
            }
        }

        private class ValueComparison<T, TQuery> : Statement<T, TQuery> where T : DatabaseModel
        {
            public ValueComparison(TQuery query, string columnName, ComparisonOperator comparisonOperator, object value) : base(query)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(columnName, nameof(columnName));

                _columnName = columnName;
                _comparisonOperator = comparisonOperator;
                _value = value;
            }

            private readonly string _columnName;
            private readonly ComparisonOperator _comparisonOperator;
            private readonly object _value;

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append($"[{_columnName}] ");

                switch (_comparisonOperator)
                {
                    case ComparisonOperator.Equals: builder.Append('='); break;
                    case ComparisonOperator.NotEquals: builder.Append("<>"); break;
                    case ComparisonOperator.GreaterThan: builder.Append('>'); break;
                    case ComparisonOperator.GreaterThanOrEqual: builder.Append(">="); break;
                    case ComparisonOperator.SmallerThan: builder.Append('<'); break;
                    case ComparisonOperator.SmallerThanOrEqual: builder.Append("<="); break;
                }

                builder.Append($" {parameters.AddParameter(_value)}");
            }
        }

        private class NullComparison<T, TQuery> : Statement<T, TQuery> where T : DatabaseModel
        {
            public NullComparison(TQuery query, string columnName, bool isInverted) : base(query)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(columnName, nameof(columnName));

                _columnName = columnName;
                _isInverted = isInverted;
            }

            private readonly string _columnName;
            private readonly bool _isInverted;

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append($"[{_columnName}] IS ");

                if (_isInverted) builder.Append("NOT ");

                builder.Append("NULL");
            }
        }

        public static ILogicalOperator<T, TQuery> Where<T, TQuery>(TQuery query) where T : DatabaseModel
        {
            return new RootElement<T, TQuery>(query);
        }

        private static void BuildWhereCore<T, TQuery>(WhereElement<T, TQuery> element, StringBuilder whereStringBuilder, ParameterCollection parameters) where T : DatabaseModel
        {
            if (element == null) return;

            element.Append(whereStringBuilder, parameters);

            BuildWhereCore(element.NextElement, whereStringBuilder, parameters);
        }
    }

    public interface IWhereElement<TQuery>
    {
        string BuildWhere(out Dictionary<string, object> parameterValues);

        TQuery EndWhere();
    }

    public interface ILogicalOperator<TQuery> : IWhereElement<TQuery>
    {
        IStatement<TQuery> Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value);

        IStatement<TQuery> Compare(string columnName, ComparisonOperator comparisonOperator, object value);

        IStatement<TQuery> Brackets(Action<ILogicalOperator<TQuery>> buildChild);

        IStatement<TQuery> IsNull(DatabaseColumnField databaseColumnField);

        IStatement<TQuery> IsNull(string columnName);

        IStatement<TQuery> IsNotNull(DatabaseColumnField databaseColumnField);

        IStatement<TQuery> IsNotNull(string columnName);
    }

    public interface ILogicalOperator<T, TQuery> : ILogicalOperator<TQuery> where T : DatabaseModel
    {
        IStatement<T, TQuery> Compare<TProperty>(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, TProperty value);

        IStatement<T, TQuery> Compare<TProperty>(string columnName, ComparisonOperator comparisonOperator, TProperty value);

        new IStatement<T, TQuery> IsNull(DatabaseColumnField databaseColumnField);

        new IStatement<T, TQuery> IsNull(string columnName);

        new IStatement<T, TQuery> IsNotNull(DatabaseColumnField databaseColumnField);

        new IStatement<T, TQuery> IsNotNull(string columnName);
    }

    public interface IStatement<TQuery> : IWhereElement<TQuery>
    {
        ILogicalOperator<TQuery> And();

        ILogicalOperator<TQuery> Or();
    }

    public interface IStatement<T, TQuery> : IStatement<TQuery> where T : DatabaseModel
    {
        new ILogicalOperator<T, TQuery> And();

        new ILogicalOperator<T, TQuery> Or();
    }

    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        SmallerThan,
        SmallerThanOrEqual,
    }
}