using HomeControl.Helpers;
using NTIH.Database;
using NTIH.Database.Modeling;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace HomeControl.Database
{
    public static class Extensions
    {
        public static void LeftJoin<T, TProperty>(this IJoinable<T> query, Expression<Func<T, TProperty>> selectorExpression) where T : DatabaseModel
        {
            ArgumentNullException.ThrowIfNull(selectorExpression, nameof(selectorExpression));

            query.LeftJoin(LinqHelper.GetExpressionMemberName(selectorExpression));
        }

        public static IStatement<T> Compare<T, TProperty>(this ILogicalOperator<T> logicalOperator, Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value) where T : DatabaseModel
        {
            return logicalOperator.Compare(LinqHelper.GetExpressionMemberName(selectorExpression), comparisonOperator, value);
        }
    }
}
