using Couchbase;
using OB.Domain;
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
namespace OB.DL.Common.Infrastructure
{
    public static class EntityExtensions
    {
        internal static IDocument<T> Wrap<T>(this T entity) where T : ICouchBaseEntity
        {
            return new Document<T>
            {
                Id = entity.Id,
                Cas = entity.Cas,
                Content = entity
            };
        }

        internal static ICouchBaseEntity<T> UnWrap<T>(this IDocument<ICouchBaseEntity<T>> document) where T : DomainObject
        {
            ICouchBaseEntity<T> result = null;

            result.DomainObject = document.Content.DomainObject;
            result.Cas = document.Cas;
            result.Id = document.Id;
            return (ICouchBaseEntity<T>)result;
        }
    }
}
