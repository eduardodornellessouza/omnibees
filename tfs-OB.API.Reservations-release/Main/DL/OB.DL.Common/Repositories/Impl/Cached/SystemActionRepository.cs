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
    internal class SystemActionRepository : Repository<SystemAction>, ISystemActionRepository
    {
        private static readonly string __cacheKey = "_SystemActions_Repository_Cache";

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public SystemActionRepository(IObjectContext context, ICacheProvider cacheProvider)
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

        private List<SystemAction> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<SystemAction>("select * from SystemActions").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get SystemAction by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<SystemAction> GetAll()
        {
            var result = CacheProvider.Get<IEnumerable<SystemAction>>(__cacheKey);

            return result;
        }

        public override SystemAction FirstOrDefault(Expression<Func<SystemAction, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override SystemAction Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the SystemAction");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods

        #region Finders

        public SystemAction FindByName(string name)
        {
            return GetAll().FirstOrDefault(x => x.Name == name);
        }

        public SystemAction FindByActionCode(int code)
        {
            string codeStr = code.ToString();
            return GetAll().FirstOrDefault(x => x.Code == codeStr);
        }

        public SystemAction FindByActionCode(string code)
        {
            return GetAll().FirstOrDefault(x => x.Code == code);
        }

        #endregion Finders
    }
}