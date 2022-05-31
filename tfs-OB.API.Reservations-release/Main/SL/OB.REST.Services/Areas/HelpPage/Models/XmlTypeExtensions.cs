using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OB.REST.Services.Areas.HelpPage.Models
{
    public static class XmlTypeExtensions
    {
        private static readonly Dictionary<Type, string> TypeMappings = new Dictionary<Type, string>()     
     {         
         // TODO - need to verify the type mappings!         
         {typeof(string), "string" },
        {typeof(bool), "boolean" },
        {typeof(decimal), "decimal" },
        {typeof(float), "float" },
        {typeof(double), "double" },
        {typeof(int), "int" },
        {typeof(DateTime), "datetime" },
        {typeof(Guid), "guid" },
     };
        public static string ToXmlTypeString(this Type type)
        {
            string typeString;
            if (TypeMappings.TryGetValue(type, out typeString))
            {
                return typeString;
            }
            return null;
        }
    }
}