using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.Domain.General;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace OB.DL.Common.Impl
{
    internal class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        private static readonly string __cacheKey = "_Currency_Repository_Cache";

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public CurrencyRepository(IObjectContext context, ICacheProvider cacheProvider)
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

        private List<Currency> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<Currency>("select * from Currencies").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get Currency by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<Currency> GetAll()
        {
            var result = CacheProvider.Get<IEnumerable<Currency>>(__cacheKey);

            return result;
        }

        public override Currency FirstOrDefault(Expression<Func<Currency, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override Currency Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the Currency");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods

        #region Finders

        public CultureInfo GetCultureInfoByCurrencyUID(long currency_UID)
        {
            var currency = this.Get(currency_UID);

            var symbol = currency != null ? currency.Symbol : null;

            var currentCulture = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(s => new RegionInfo(s.LCID).ISOCurrencySymbol == symbol).FirstOrDefault();

            if (currentCulture == null)
            {
                currency = this.Get(109);

                symbol = currency != null ? currency.Symbol : null;

                currentCulture = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(s => new RegionInfo(s.LCID).ISOCurrencySymbol == symbol).FirstOrDefault();
            }

            return currentCulture;
        }


        public IEnumerable<Currency> FindCurrencyByCriteria(out int totalRecords, List<long> UIDs = null, List<string> Names = null, List<string> Symbols = null, List<string> CurrencySymbols = null, List<string> PaypalCurrencyCodes = null,
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            var result = GetQuery();
            totalRecords = -1;

            if (UIDs != null && UIDs.Count > 0)
                result = result.Where(x => UIDs.Contains(x.UID));

            if (Names != null && Names.Count > 0)
                result = result.Where(x => Names.Contains(x.Name));

            if (Symbols != null && Symbols.Count > 0)
                result = result.Where(x => x.Symbol != null && Symbols.Contains(x.Symbol));

            if (CurrencySymbols != null && CurrencySymbols.Count > 0)
                result = result.Where(x => x.CurrencySymbol != null && CurrencySymbols.Contains(x.CurrencySymbol));

            if (PaypalCurrencyCodes != null && PaypalCurrencyCodes.Count > 0)
                result = result.Where(x => x.PaypalCurrencyCode != null && PaypalCurrencyCodes.Contains(x.PaypalCurrencyCode));

            if (returnTotal)
                totalRecords = result.Count();

            if (pageIndex > 0 && pageSize > 0)
                result = result.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }

        #endregion Finders
    }
}