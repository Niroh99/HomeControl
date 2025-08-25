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

        private abstract class WhereElement<T> : IWhereElement where T : DatabaseModel
        {
            private WhereElement<T> _parent;

            private WhereElement<T> _nextElement;

            public WhereElement<T> Parent { get => _parent; }

            public WhereElement<T> NextElement { get => _nextElement; }

            public string BuildWhere(out Dictionary<string, object> parameterValues)
            {
                if (Parent != null) return Parent.BuildWhere(out parameterValues);

                var whereStringBuilder = new StringBuilder();

                var parameters = new ParameterCollection();

                BuildWhereCore(this, whereStringBuilder, parameters);

                parameterValues = parameters.GetParameterValues();

                return whereStringBuilder.ToString();
            }

            protected TChild SetNextElement<TChild>(TChild child) where TChild : WhereElement<T>
            {
                child._parent = this;

                _nextElement = child;

                return child;
            }

            public abstract void Append(StringBuilder builder, ParameterCollection parameters);
        }

        private abstract class LogicalOperator<T> : WhereElement<T>, ILogicalOperator<T> where T : DatabaseModel
        {
            IStatement ILogicalOperator.Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value) => Compare(databaseColumnField, comparisonOperator, value);

            IStatement ILogicalOperator.Compare(string columnName, ComparisonOperator comparisonOperator, object value) => Compare(columnName, comparisonOperator, value);

            IStatement ILogicalOperator.IsNull(DatabaseColumnField databaseColumnField) => IsNull(databaseColumnField);

            IStatement ILogicalOperator.IsNull(string columnName) => IsNull(columnName);

            IStatement ILogicalOperator.IsNotNull(DatabaseColumnField databaseColumnField) => IsNotNull(databaseColumnField);

            IStatement ILogicalOperator.IsNotNull(string columnName) => IsNotNull(columnName);

            IStatement ILogicalOperator.Brackets(Action<ILogicalOperator> buildChild) => Brackets(buildChild);

            public IStatement<T> Compare<TProperty>(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, TProperty value)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return Compare(databaseColumnField.Name, comparisonOperator, value);
            }

            public IStatement<T> Compare<TProperty>(string columnName, ComparisonOperator comparisonOperator, TProperty value)
            {
                return SetNextElement(new ValueComparison<T>(columnName, comparisonOperator, value));
            }

            public IStatement<T> IsNull(DatabaseColumnField databaseColumnField)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return IsNull(databaseColumnField.ColumnName);
            }

            public IStatement<T> IsNull(string columnName)
            {
                return SetNextElement(new NullComparison<T>(columnName, false));
            }

            public IStatement<T> IsNotNull(DatabaseColumnField databaseColumnField)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                return IsNotNull(databaseColumnField.ColumnName);
            }

            public IStatement<T> IsNotNull(string columnName)
            {
                return SetNextElement(new NullComparison<T>(columnName, true));
            }

            public static IStatement<T> Brackets(Action<ILogicalOperator<T>> buildChild)
            {
                var brackets = new Brackets<T>();

                buildChild(brackets.Child);

                return brackets;
            }
        }

        private sealed class RootElement<T> : LogicalOperator<T> where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {

            }
        }

        private sealed class And<T> : LogicalOperator<T> where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append(" AND ");
            }
        }

        private sealed class Or<T> : LogicalOperator<T> where T : DatabaseModel
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append(" OR ");
            }
        }

        private abstract class Statement<T> : WhereElement<T>, IStatement<T> where T : DatabaseModel
        {
            ILogicalOperator IStatement.And() => And();

            ILogicalOperator IStatement.Or() => Or();

            public ILogicalOperator<T> And() => SetNextElement(new And<T>());

            public ILogicalOperator<T> Or() => SetNextElement(new Or<T>());
        }

        private sealed class Brackets<T> : Statement<T>, IStatement<T> where T : DatabaseModel
        {
            public Brackets()
            {
                _child = new RootElement<T>();
            }

            private readonly RootElement<T> _child;
            public ILogicalOperator<T> Child { get => _child; }

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                builder.Append('(');
                BuildWhereCore(_child, builder, parameters);
                builder.Append(')');
            }
        }

        private class ValueComparison<T> : Statement<T> where T : DatabaseModel
        {
            public ValueComparison(string columnName, ComparisonOperator comparisonOperator, object value)
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

        private class NullComparison<T> : Statement<T> where T : DatabaseModel
        {
            public NullComparison(string columnName, bool isInverted)
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

        public static ILogicalOperator Where()
        {
            return new RootElement<DatabaseModel>();
        }

        public static ILogicalOperator<T> Where<T>() where T : DatabaseModel
        {
            return new RootElement<T>();
        }

        private static void BuildWhereCore<T>(WhereElement<T> element, StringBuilder whereStringBuilder, ParameterCollection parameters) where T : DatabaseModel
        {
            if (element == null) return;

            element.Append(whereStringBuilder, parameters);

            BuildWhereCore(element.NextElement, whereStringBuilder, parameters);
        }
    }

    public interface IWhereElement
    {
        string BuildWhere(out Dictionary<string, object> parameterValues);

        
    }

    public interface ILogicalOperator : IWhereElement
    {
        IStatement Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value);

        IStatement Compare(string columnName, ComparisonOperator comparisonOperator, object value);

        IStatement Brackets(Action<ILogicalOperator> buildChild);

        IStatement IsNull(DatabaseColumnField databaseColumnField);

        IStatement IsNull(string columnName);

        IStatement IsNotNull(DatabaseColumnField databaseColumnField);

        IStatement IsNotNull(string columnName);
    }

    public interface ILogicalOperator<T> : ILogicalOperator where T : DatabaseModel
    {
        IStatement<T> Compare<TProperty>(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, TProperty value);

        IStatement<T> Compare<TProperty>(string columnName, ComparisonOperator comparisonOperator, TProperty value);

        new IStatement<T> IsNull(DatabaseColumnField databaseColumnField);

        new IStatement<T> IsNull(string columnName);

        new IStatement<T> IsNotNull(DatabaseColumnField databaseColumnField);

        new IStatement<T> IsNotNull(string columnName);
    }

    public interface IStatement : IWhereElement
    {
        ILogicalOperator And();

        ILogicalOperator Or();
    }

    public interface IStatement<T> : IStatement where T : DatabaseModel
    {
        new ILogicalOperator<T> And();

        new ILogicalOperator<T> Or();
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