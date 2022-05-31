using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.Domain.General;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace OB.DL.Common.Impl
{
    internal class ExchangeRateRepository : Repository<OB.Domain.General.ExchangeRate>, IExchangeRateRepository
    {
        private static TimeSpan _refreshTime = TimeSpan.FromMinutes(1440);
        private static readonly string __cacheKey = "_ExchangeRate_Repository_Cache";

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        /// <param name="languageRepository">Injected</param>
        public ExchangeRateRepository(IObjectContext context, ICacheProvider cacheProvider)
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

        private List<ExchangeRate> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<ExchangeRate>(@"SELECT er.UID
	                                                        ,er.Currency_UID
	                                                        ,er.Rate
	                                                        ,[Date]
                                                        FROM dbo.ExchangeRates er
                                                        INNER JOIN (
	                                                        SELECT Currency_UID
		                                                        ,max(er2.[Date]) AS MaxDate
	                                                        FROM dbo.ExchangeRates er2
	                                                        GROUP BY er2.Currency_UID
	                                                        ) tm ON er.Currency_UID = tm.Currency_UID
	                                                        AND er.[Date] = tm.MaxDate").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get All AppSettings .
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<ExchangeRate> GetAll()
        {
            var result = CacheProvider.Get<IEnumerable<ExchangeRate>>(__cacheKey);

            return result;
        }

        public override ExchangeRate FirstOrDefault(Expression<Func<ExchangeRate, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        #endregion Cache Methods

        #region Finders

        public IEnumerable<ExchangeRate> GetByCurrencyIds(List<long> currencyIds)
        {
            if (currencyIds == null)
                return Enumerable.Empty<ExchangeRate>();

            var exchangeRateList = this.GetAll().Where(x => currencyIds.Contains(x.Currency_UID));

            return exchangeRateList.OrderByDescending(x => x.Date);
        }

        #endregion Finders
    }
}