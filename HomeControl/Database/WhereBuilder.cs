using HomeControl.Helpers;
using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HomeControl.Database
{
    public static class WhereBuilder
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

        public abstract class WhereElement : IWhereElement
        {
            private WhereElement _parent;

            private WhereElement _nextElement;

            public WhereElement Parent { get => _parent; }

            public WhereElement NextElement { get => _nextElement; }

            public string BuildWhere(out Dictionary<string, object> parameterValues)
            {
                if (Parent != null) return Parent.BuildWhere(out parameterValues);

                var whereStringBuilder = new StringBuilder();

                var parameters = new ParameterCollection();

                BuildWhereCore(this, whereStringBuilder, parameters);

                parameterValues = parameters.GetParameterValues();

                return whereStringBuilder.ToString();
            }

            protected TChild SetNextElement<TChild>(TChild child) where TChild : WhereElement
            {
                child._parent = this;

                _nextElement = child;

                return child;
            }

            public abstract void Append(StringBuilder builder, ParameterCollection parameters);
        }

        private abstract class WhereElement<T> : WhereElement
        {

        }

        private abstract class LogicalOperator : WhereElement, ILogicalOperator
        {
            public IStatement Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value)
            {
                return SetNextElement(new ValueComparison(databaseColumnField, comparisonOperator, value));
            }

            public IStatement Brackets(Action<ILogicalOperator> buildChild)
            {
                var brackets = new Brackets();

                buildChild(brackets.Child);

                return brackets;
            }
        }

        private abstract class LogicalOperator<T> : WhereElement<T>, ILogicalOperator<T> where T : Model
        {
            IStatement ILogicalOperator.Compare(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value)
            {
                return SetNextElement(new ValueComparison(databaseColumnField, comparisonOperator, value));
            }

            IStatement ILogicalOperator.Brackets(Action<ILogicalOperator> buildChild)
            {
                return Brackets(buildChild);
            }

            public IStatement<T> Compare<TProperty>(Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value)
            {
                return SetNextElement(new ValueComparison<T, TProperty>(selectorExpression, comparisonOperator, value));
            }

            public static IStatement<T> Brackets(Action<ILogicalOperator<T>> buildChild)
            {
                var brackets = new Brackets<T>();

                buildChild(brackets.Child);

                return brackets;
            }
        }

        private sealed class RootElement : LogicalOperator
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {

            }
        }

        private sealed class RootElement<T> : LogicalOperator<T> where T : Model
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {

            }
        }

        private sealed class And : LogicalOperator
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendAnd(builder);
            }
        }

        private sealed class And<T> : LogicalOperator<T> where T : Model
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendAnd(builder);
            }
        }

        private sealed class Or : LogicalOperator
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendOr(builder);
            }
        }

        private sealed class Or<T> : LogicalOperator<T> where T : Model
        {
            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendOr(builder);
            }
        }

        private abstract class Statement : WhereElement, IStatement
        {
            public ILogicalOperator And() => SetNextElement(new And());

            public ILogicalOperator Or() => SetNextElement(new Or());
        }

        private abstract class Statement<T> : WhereElement<T>, IStatement<T> where T : Model
        {
            ILogicalOperator IStatement.And() => And();

            ILogicalOperator IStatement.Or() => Or();

            public ILogicalOperator<T> And() => SetNextElement(new And<T>());

            public ILogicalOperator<T> Or() => SetNextElement(new Or<T>());
        }

        private sealed class Brackets : Statement
        {
            public Brackets()
            {
                _child = new RootElement();
            }

            private readonly RootElement _child;
            public ILogicalOperator Child { get => _child; }

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendBrackets(_child, builder, parameters);
            }
        }

        private sealed class Brackets<T> : Statement<T>, IStatement<T> where T : Model
        {
            public Brackets()
            {
                _child = new RootElement<T>();
            }

            private readonly RootElement<T> _child;
            public ILogicalOperator<T> Child { get => _child; }

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendBrackets(_child, builder, parameters);
            }
        }

        private class ValueComparison : Statement
        {
            public ValueComparison(DatabaseColumnField databaseColumnField, ComparisonOperator comparisonOperator, object value)
            {
                ArgumentNullException.ThrowIfNull(databaseColumnField, nameof(databaseColumnField));

                _columnName = databaseColumnField.ColumnName;
                _comparisonOperator = comparisonOperator;
                _value = value;
            }

            private readonly string _columnName;
            private readonly ComparisonOperator _comparisonOperator;
            private readonly object _value;

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendValueComparison(builder, parameters, _columnName, _comparisonOperator, _value);
            }
        }

        private class ValueComparison<T, TProperty> : Statement<T> where T : Model
        {
            public ValueComparison(Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value)
            {
                ArgumentNullException.ThrowIfNull(selectorExpression, nameof(selectorExpression));

                _propertyName = LinqHelper.GetExpressionMemberName(selectorExpression);
                _comparisonOperator = comparisonOperator;
                _value = value;
            }

            private readonly string _propertyName;
            private readonly ComparisonOperator _comparisonOperator;
            private readonly object _value;

            public override void Append(StringBuilder builder, ParameterCollection parameters)
            {
                AppendValueComparison(builder, parameters, _propertyName, _comparisonOperator, _value);
            }
        }

        public static ILogicalOperator Where()
        {
            return new RootElement();
        }

        public static ILogicalOperator<T> Where<T>() where T : Model
        {
            return new RootElement<T>();
        }

        private static void AppendAnd(StringBuilder builder)
        {
            builder.Append(" AND ");
        }

        private static void AppendOr(StringBuilder builder)
        {
            builder.Append(" OR ");
        }

        private static void AppendBrackets(WhereElement child, StringBuilder builder, ParameterCollection parameters)
        {
            builder.Append('(');
            BuildWhereCore(child, builder, parameters);
            builder.Append(')');
        }

        private static void AppendValueComparison(StringBuilder builder, ParameterCollection parameters, string columnName, ComparisonOperator comparisonOperator, object value)
        {
            builder.Append($"[{columnName}] ");

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals: builder.Append('='); break;
                case ComparisonOperator.NotEquals: builder.Append("<>"); break;
                case ComparisonOperator.GreaterThan: builder.Append('>'); break;
                case ComparisonOperator.GreaterThanOrEqual: builder.Append(">="); break;
                case ComparisonOperator.SmallerThan: builder.Append('<'); break;
                case ComparisonOperator.SmallerThanOrEqual: builder.Append("<="); break;
            }

            builder.Append($" {parameters.AddParameter(value)}");
        }

        private static void BuildWhereCore(WhereElement element, StringBuilder whereStringBuilder, ParameterCollection parameters)
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

        IStatement Brackets(Action<ILogicalOperator> buildChild);
    }

    public interface ILogicalOperator<T> : ILogicalOperator where T : Model
    {
        IStatement<T> Compare<TProperty>(Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value);
    }

    public interface IStatement : IWhereElement
    {
        ILogicalOperator And();

        ILogicalOperator Or();
    }

    public interface IStatement<T> : IStatement where T : Model
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