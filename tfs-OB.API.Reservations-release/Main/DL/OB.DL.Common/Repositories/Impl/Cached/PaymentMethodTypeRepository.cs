using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.Domain.Payments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;

namespace OB.DL.Common.Impl
{
    internal class PaymentMethodTypeRepository : Repository<PaymentMethodType>, IPaymentMethodTypeRepository
    {
        private static TimeSpan _refreshTime = TimeSpan.FromMinutes(1450);
        private static readonly string __cacheKey = "_PaymentMethodType_Repository_Cache";

        public PaymentMethodTypeRepository(IObjectContext context, ICacheProvider cacheProvider)
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

        /// <summary>
        /// Desc :: To Get PaymentMethodType by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public PaymentMethodType FindPaymentMethodTypeByCode(int code)
        {
            return GetQuery().SingleOrDefault(x => x.Code == code);
        }

        private IEnumerable<PaymentMethodType> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<PaymentMethodType>("select * from PaymentMethodTypes");
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get PaymentGatewayConfiguration by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<PaymentMethodType> GetAll()
        {
            IEnumerable<PaymentMethodType> result = CacheProvider.Get<IEnumerable<PaymentMethodType>>(__cacheKey);

            return result;
        }

        public override IQueryable<PaymentMethodType> GetQuery()
        {
            return GetAll().AsQueryable();
        }

        public override PaymentMethodType Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the PaymentMethodType");

            return GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }
    }
}