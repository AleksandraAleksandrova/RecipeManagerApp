using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RecipeManager.Core.Utils
{
    public static class FilterExpressionBuilder
    {
        public static Expression<Func<T, bool>> BuildExpression<T>(Dictionary<string, string> filters)
        {
            var param = Expression.Parameter(typeof(T), "x");
            Expression finalExpr = null;

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                var propInfo = typeof(T).GetProperty(filter.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propInfo == null)
                    continue;

                var propExpr = Expression.Property(param, propInfo);
                Expression comparisonExpr = null;

                try
                {
                    if (propInfo.PropertyType == typeof(string))
                    {
                        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                        if (toLowerMethod != null && containsMethod != null)
                        {
                            var lowerPropCall = Expression.Call(propExpr, toLowerMethod);
                            var val = Expression.Constant(filter.Value.ToLower(), typeof(string));

                            var nullCheck = Expression.NotEqual(propExpr, Expression.Constant(null, typeof(string)));
                            var containsCall = Expression.Call(lowerPropCall, containsMethod, val);

                            comparisonExpr = Expression.AndAlso(nullCheck, containsCall);
                        }
                    }
                    else if (propInfo.PropertyType == typeof(int) || propInfo.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(filter.Value, out int intVal))
                        {
                            var val = Expression.Constant(intVal, typeof(int));
                            var left = propInfo.PropertyType == typeof(int?) ? Expression.Property(propExpr, "Value") : propExpr;
                            comparisonExpr = Expression.Equal(left, val);
                        }
                    }
                    else if (propInfo.PropertyType == typeof(DateTime) || propInfo.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(filter.Value, out DateTime dtVal))
                        {
                            var val = Expression.Constant(dtVal.Date, typeof(DateTime));
                            var left = propInfo.PropertyType == typeof(DateTime?) ? Expression.Property(Expression.Property(propExpr, "Value"), "Date") : Expression.Property(propExpr, "Date");
                            comparisonExpr = Expression.Equal(left, val);
                        }
                    }

                    if (comparisonExpr != null)
                    {
                        finalExpr = finalExpr == null ? comparisonExpr : Expression.AndAlso(finalExpr, comparisonExpr);
                    }
                }
                catch
                {
                    // Ignore malformed filters
                }
            }

            if (finalExpr == null)
            {
                return x => true;
            }

            return Expression.Lambda<Func<T, bool>>(finalExpr, param);
        }
    }
}
