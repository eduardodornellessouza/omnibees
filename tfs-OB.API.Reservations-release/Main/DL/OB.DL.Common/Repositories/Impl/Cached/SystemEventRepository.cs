using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.Domain.ProactiveActions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace OB.DL.Common.Impl
{
    internal class SystemEventRepository : Repository<SystemEvent>, ISystemEventRepository
    {
        private static readonly string __cacheKey = "_SystemEvents_Repository_Cache";

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public SystemEventRepository(IObjectContext context, ICacheProvider cacheProvider)
            : base(context, cacheProvider)
        {
            var connectionString = context.Context.Database.Connection.ConnectionString;
            CacheProvider.Set(__cacheKey, new CacheEntry
            {
                CacheKey = __cacheKey,
                GetDataCallback = () => GetDataToCache(connectionString),
                UpdateInterval = TimeSpan.MaxValue
            });
        }

        #region Cache Methods

        private List<SystemEvent> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<SystemEvent>("select * from SystemEvents order by SEOrder").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get SystemEvent by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<SystemEvent> GetAll()
        {
            var result = CacheProvider.Get<IEnumerable<SystemEvent>>(__cacheKey);

            return result;
        }

        public override SystemEvent FirstOrDefault(Expression<Func<SystemEvent, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override SystemEvent Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the SystemEvent");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods

        #region Finders

        public SystemEvent FindByEventCode(string code)
        {
            return GetAll().FirstOrDefault(x => x.Code == code);
        }

        public SystemEvent FindByEventCode(int code)
        {
            string codeStr = code.ToString();
            return GetAll().FirstOrDefault(x => x.Code == codeStr);
        }

        #endregion Finders
    }
}