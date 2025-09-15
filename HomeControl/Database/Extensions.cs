using HomeControl.Helpers;
using NTIH.Database;
using NTIH.Database.Modeling;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace HomeControl.Database
{
    public static class Extensions
    {
        public static ISelectSingle<T> LeftJoin<T, TProperty>(this ISelectSingle<T> query, Expression<Func<T, TProperty>> selectorExpression) where T : DatabaseTableModel
        {
            ArgumentNullException.ThrowIfNull(selectorExpression, nameof(selectorExpression));

            return query.LeftJoin(LinqHelper.GetExpressionMemberName(selectorExpression));
        }

        public static ISelectMany<T> LeftJoin<T, TProperty>(this ISelectMany<T> query, Expression<Func<T, TProperty>> selectorExpression) where T : DatabaseTableModel
        {
            ArgumentNullException.ThrowIfNull(selectorExpression, nameof(selectorExpression));

            return query.LeftJoin(LinqHelper.GetExpressionMemberName(selectorExpression));
        }

        public static IStatement<T, ISelectSingle<T>> Compare<T, TProperty>(this ILogicalOperator<T, ISelectSingle<T>> logicalOperator, Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value) where T : DatabaseTableModel
        {
            return logicalOperator.Compare(LinqHelper.GetExpressionMemberName(selectorExpression), comparisonOperator, value);
        }

        public static IStatement<T, ISelectMany<T>> Compare<T, TProperty>(this ILogicalOperator<T, ISelectMany<T>> logicalOperator, Expression<Func<T, TProperty>> selectorExpression, ComparisonOperator comparisonOperator, TProperty value) where T : DatabaseTableModel
        {
            return logicalOperator.Compare(LinqHelper.GetExpressionMemberName(selectorExpression), comparisonOperator, value);
        }
    }
}
