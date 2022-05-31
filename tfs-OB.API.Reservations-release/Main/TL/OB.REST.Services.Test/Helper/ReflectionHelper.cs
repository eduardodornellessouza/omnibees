using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OB.REST.Services.Test.Helper
{
    public class ReflectionHelper
    {
        public static string GetMethodName<T, U>(Expression<Func<T, U>> expression)
        {
            var method = expression.Body as MethodCallExpression;
            if (method != null)
                return method.Method.Name;

            throw new ArgumentException("Expression is wrong");
        }
    }
}
