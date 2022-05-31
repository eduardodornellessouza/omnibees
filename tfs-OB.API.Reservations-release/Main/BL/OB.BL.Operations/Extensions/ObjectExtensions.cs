using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


//TMOREIRA:Use same namespace to make the extension available everywhere easily.
// ReSharper disable CheckNamespace
namespace System
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Extension that returns a PropertyName from its expression.
    /// </summary>
    public static class ObjectExtension
    {

        public static string GetPropertyName<T>(Expression<Func<T, object>> exp) where T : class
        {
            return GetMemberExpressionPropertyName(exp.Body);
        }

        public static string GetPropertyName(this object owner, Expression<Func<object>> exp)
        {
            return GetMemberExpressionPropertyName(exp.Body);
        }

        public static MethodInfo GetPropertySetter(this object owner, string propertyName)
        {
            var propInfo = owner.GetType().GetProperty(propertyName);
            return propInfo.GetSetMethod();
        }

        private static string GetMemberExpressionPropertyName(System.Linq.Expressions.Expression bodyExpression)
        {
            MemberExpression body = bodyExpression as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)bodyExpression;
                body = ubody.Operand as MemberExpression;
            }
            return body.Member.Name;
        }

        public static string GetPropertyName<T>(this T owner, Expression<Func<T, object>> exp) where T : class
        {
            return GetMemberExpressionPropertyName(exp.Body);
        }

        /// <summary>
        /// Serializes the given object to JSON format.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="indent">To add indentation to the output (linebreaks , etc)</param>
        /// <returns></returns>
        public static string ToJSON(this object obj, bool indent = false)
        {
            if (obj != null)
            {
                var stringBuider = new StringBuilder();
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {                    
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = indent ? Formatting.Indented : Formatting.None
                });
                serializer.Serialize(new StringWriter(stringBuider), obj);

                return stringBuider.ToString();
            }

            return null;
        }

        /// <summary>
        /// Deserializes the given string to object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="indent">To add indentation to the output (linebreaks , etc)</param>
        /// <returns></returns>
        public static T FromJSON<T>(this string obj)
        {
            if (!string.IsNullOrEmpty(obj))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj);

            return default(T);
        }

        /// <summary>
        /// Compare properties of two class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static bool AreEqual<T>(this T self, T to, params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (!ignoreList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == to;
        }

    }
}
