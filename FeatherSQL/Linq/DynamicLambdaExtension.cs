using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 扩展lambda
    /// </summary>
    public static class DynamicLambdaExtension
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> source, Expression<Func<T, bool>> other)
        {
            DynamicLambda<T> dynamicLambda = new DynamicLambda<T>();
            return dynamicLambda.BuildQueryAnd(source, other);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> source, Expression<Func<T, bool>> other)
        {
            DynamicLambda<T> dynamicLambda = new DynamicLambda<T>();
            return dynamicLambda.BuildQueryOr(source, other);
        }
    }
}
