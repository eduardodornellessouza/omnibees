using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace OB.DL.Common.Infrastructure.Impl
{
    /// <summary>
    /// Helper class to get PrimaryKey information from EntityFramework entities.
    /// </summary>
    internal class EntityKeyHelper
    {
        private static readonly Lazy<EntityKeyHelper> LazyInstance = new Lazy<EntityKeyHelper>(() => new EntityKeyHelper());
        private readonly Dictionary<Type, string[]> _dict = new Dictionary<Type, string[]>();

        private EntityKeyHelper()
        {
        }

        public static EntityKeyHelper Instance
        {
            get { return LazyInstance.Value; }
        }

        /// <summary>
        /// Gets an array with the PrimaryKey names of the given entity type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public string[] GetKeyNames<T>(DbContext context) where T : class
        {
            Type t = typeof(T);

            //retreive the base type
            while (t.BaseType != typeof(object))
                t = t.BaseType;

            string[] keys;

            _dict.TryGetValue(t, out keys);
            if (keys != null)
                return keys;

            ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;

            //create method CreateObjectSet with the generic parameter of the base-type
            MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes)
                                                     .MakeGenericMethod(t);
            dynamic objectSet = method.Invoke(objectContext, null);

            IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

            string[] keyNames = keyMembers.Select(k => (string)k.Name).ToArray();

            _dict.Add(t, keyNames);

            return keyNames;
        }

        /// <summary>
        /// Gets an array with the entity PrimaryKey values of the given Entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object[] GetKeys<T>(T entity, DbContext context) where T : class
        {
            var keyNames = GetKeyNames<T>(context);
            Type type = typeof(T);

            object[] keys = new object[keyNames.Length];
            for (int i = 0; i < keyNames.Length; i++)
            {
                keys[i] = type.GetProperty(keyNames[i]).GetValue(entity, null);
            }
            return keys;
        }
    }
}