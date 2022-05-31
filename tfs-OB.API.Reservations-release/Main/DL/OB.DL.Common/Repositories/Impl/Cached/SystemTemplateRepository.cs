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
    internal class SystemTemplateRepository : Repository<SystemTemplate>, ISystemTemplateRepository
    {
        private static readonly TimeSpan _refreshTime = TimeSpan.FromHours(6);
        private static readonly string __cacheKey = "_SystemTemplates_Repository_Cache";

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public SystemTemplateRepository(IObjectContext context, ICacheProvider cacheProvider)
            : base(context, cacheProvider)
        {
            var connectionString = context.Context.Database.Connection.ConnectionString;
            CacheProvider.Set(__cacheKey, new CacheEntry
            {
                CacheKey = __cacheKey,
                GetDataCallback = () => GetDataToCache(connectionString),
                UpdateInterval = _refreshTime
            });
        }

        #region Cache Methods

        private List<SystemTemplate> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<SystemTemplate>("select * from SystemTemplates where IsDeleted=0 or IsDeleted is null order by UID").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get SystemTemplate by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<SystemTemplate> GetAll()
        {
            var result = CacheProvider.Get<IEnumerable<SystemTemplate>>(__cacheKey);

            return result;
        }

        public override SystemTemplate FirstOrDefault(Expression<Func<SystemTemplate, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override SystemTemplate Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the SystemTemplate");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods

        #region Finders

        #endregion Finders
    }
}