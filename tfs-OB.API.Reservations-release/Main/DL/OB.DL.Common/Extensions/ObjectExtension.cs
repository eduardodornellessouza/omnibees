using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    static class ObjectExtension
    {  
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
    }
}
