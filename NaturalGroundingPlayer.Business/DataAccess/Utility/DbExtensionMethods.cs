using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {
    public static class ExtensionMethods {
        /// <summary>
        /// Binds a function to specified SQLite connection.
        /// </summary>
        /// <param name="connection">The connection to bind to.</param>
        /// <param name="function">The function to bind.</param>
        public static void BindFunction(this SQLiteConnection connection, SQLiteFunction function) {
            var attributes = function.GetType().GetCustomAttributes(typeof(SQLiteFunctionAttribute), true).Cast<SQLiteFunctionAttribute>().ToArray();
            if (attributes.Length == 0) {
                throw new InvalidOperationException("SQLiteFunction doesn't have SQLiteFunctionAttribute");
            }
            connection.BindFunction(attributes[0], function);
        }

        /// <summary>
        /// Does an OrElse operation between two expressions.
        /// </summary>
        /// <typeparam name="T">The input type of the expression.</typeparam>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>An expression that combines both.</returns>
        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(left, right), parameter);
        }

        /// <summary>
        /// Does an AndAlso operation between two expressions.
        /// </summary>
        /// <typeparam name="T">The input type of the expression.</typeparam>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>An expression that combines both.</returns>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

        /// <summary>
        /// Internal class used by the Expression.OrElse function.
        /// </summary>
        private class ReplaceExpressionVisitor
            : ExpressionVisitor {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue) {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node) {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}
