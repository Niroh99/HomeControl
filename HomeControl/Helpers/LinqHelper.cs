using System.Linq.Expressions;

namespace HomeControl.Helpers
{
    public class LinqHelper
    {
        public static string GetExpressionMemberName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.Body is MemberExpression memberExpression) return memberExpression.Member.Name;
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression OperandMemberExpression) return OperandMemberExpression.Member.Name;

            return null;
        }
    }
}