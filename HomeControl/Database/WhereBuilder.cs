using HomeControl.Helpers;
using HomeControl.Modeling;
using System.Linq.Expressions;
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

        public abstract class WhereElement<T> where T : Model
        {
            private WhereElement<T> _parent;

            private WhereElement<T> _nextElement;

            public string BuildWhere(out Dictionary<string, object> parameterValues)
            {
                if (_parent != null) return _parent.BuildWhere(out parameterValues);

                var whereStringBuilder = new StringBuilder();

                var parameters = new ParameterCollection();

                BuildWhereCore(whereStringBuilder, parameters);

                parameterValues = parameters.GetParameterValues();

                return whereStringBuilder.ToString();
            }

            private void BuildWhereCore(StringBuilder whereStringBuilder, ParameterCollection parameters)
            {
                if (_nextElement == null) return;

                switch (_nextElement)
                {
                    case LogicalOperator<T> combination: AppendLogicalOperator(whereStringBuilder, combination); break;
                    case Statement<T> combinable: AppendStatement(whereStringBuilder, combinable, parameters); break;
                }

                _nextElement.BuildWhereCore(whereStringBuilder, parameters);
            }

            protected TChild SetNextElement<TChild>(TChild child) where TChild : WhereElement<T>
            {
                child._parent = this;

                _nextElement = child;

                return child;
            }

            private void AppendLogicalOperator(StringBuilder whereStringBuilder, LogicalOperator<T> combination)
            {
                if (_nextElement == null) throw new InvalidOperationException("No next Element Provided.");

                switch (combination)
                {
                    case RootElement<T>: break;
                    case And<T>: whereStringBuilder.Append(" AND "); break;
                    case Or<T>: whereStringBuilder.Append(" OR "); break;
                }
            }

            private void AppendStatement(StringBuilder whereStringBuilder, Statement<T> combinable, ParameterCollection parameters)
            {
                switch (combinable)
                {
                    case Brackets<T> brackets:
                        whereStringBuilder.Append('(');
                        brackets.BuildWhereCore(whereStringBuilder, parameters);
                        whereStringBuilder.Append(')');
                        break;
                    case Comparison<T> comparison:
                        AppendComparison(whereStringBuilder, comparison, parameters);
                        break;
                }
            }

            private static void AppendComparison(StringBuilder whereStringBuilder, Comparison<T> comparison, ParameterCollection parameters)
            {
                comparison.AppendComparison(whereStringBuilder, parameters);
            }
        }

        private sealed class RootElement<T> : LogicalOperator<T> where T : Model
        {

        }

        public abstract class LogicalOperator<T> : WhereElement<T> where T : Model
        {
            public Statement<T> Compare<TProperty>(Expression<Func<T, TProperty>> propertySelector, ComparisonOperator comparisonOperator, TProperty value)
            {
                return SetNextElement(new ValueComparison<T, TProperty>(propertySelector, comparisonOperator, value));
            }
        }

        private sealed class And<T> : LogicalOperator<T> where T : Model
        {

        }

        private sealed class Or<T> : LogicalOperator<T> where T : Model
        {

        }

        public abstract class Statement<T> : WhereElement<T> where T : Model
        {
            public LogicalOperator<T> And() => SetNextElement(new And<T>());

            public LogicalOperator<T> Or() => SetNextElement(new Or<T>());
        }

        public sealed class Brackets<T> : Statement<T> where T : Model
        {

        }

        public abstract class Comparison<T> : Statement<T> where T : Model
        {
            public abstract void AppendComparison(StringBuilder whereStringBuilder, ParameterCollection parameters);
        }

        public sealed class ValueComparison<T, TProperty> : Comparison<T> where T : Model
        {
            public ValueComparison(Expression<Func<T, TProperty>> propertySelector, ComparisonOperator comparisonOperator, TProperty value)
            {
                _propertyName = LinqHelper.GetExpressionMemberName(propertySelector);

                ArgumentNullException.ThrowIfNull(_propertyName, nameof(propertySelector));

                _comparisonOperator = comparisonOperator;
                _value = value;
            }

            private readonly string _propertyName;
            private readonly ComparisonOperator _comparisonOperator;
            private readonly TProperty _value;

            public override void AppendComparison(StringBuilder whereStringBuilder, ParameterCollection parameters)
            {
                whereStringBuilder.Append($"[{_propertyName}] ");

                switch (_comparisonOperator)
                {
                    case ComparisonOperator.Equals: whereStringBuilder.Append('='); break;
                    case ComparisonOperator.NotEquals: whereStringBuilder.Append("<>"); break;
                    case ComparisonOperator.GreaterThan: whereStringBuilder.Append('>'); break;
                    case ComparisonOperator.GreaterThanOrEqual: whereStringBuilder.Append(">="); break;
                    case ComparisonOperator.SmallerThan: whereStringBuilder.Append('<'); break;
                    case ComparisonOperator.SmallerThanOrEqual: whereStringBuilder.Append("<="); break;
                }

                whereStringBuilder.Append($" {parameters.AddParameter(_value)}");
            }
        }

        public static LogicalOperator<T> Where<T>() where T : Model
        {
            return new RootElement<T>();
        }
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