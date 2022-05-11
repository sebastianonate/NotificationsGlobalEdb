using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Core.Helpers
{
    public class BuildExpressionHelper
    {
        public static Expression Contains(Expression left, Expression right)
        {
            MethodInfo mi = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            Expression call = Expression.Call(left, mi, right);
            return call;
        }



        public static Expression<Func<T, bool>> GetFilter<T>(List<FilterDescriptor> filters)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            Expression finalBody = null;
            foreach (var item in filters)
            {
                var currentBody = Combine(item.Operation,
                    parameter,
                    Expression.Constant(item.Value));
                if (finalBody == null)
                    finalBody = currentBody;
                else
                    finalBody = Expression.AndAlso(finalBody, currentBody);
            }
            var predicate = Expression.Lambda<Func<T, bool>>(
                finalBody, parameter);
            return predicate;
        }

        public static Expression Combine(string op, Expression left, Expression right)
        {
            switch (op)
            {
                case "=":
                    return Expression.Equal(left, right);
                case "<":
                    return Expression.LessThan(left, right);
                case ">":
                    return Expression.GreaterThan(left, right);
                case "contains":
                    return Contains(left, right);
            }
            return null;
        }
    }
    public record FilterDescriptor(string FieldName, object Value, string Operation);

}
