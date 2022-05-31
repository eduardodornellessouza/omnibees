using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace OB.DL.Common.Impl
{
    internal class PaymentGatewayConfigurationRepository : Repository<PaymentGatewayConfiguration>, IPaymentGatewayConfigurationRepository
    {
        private static TimeSpan _refreshTime = TimeSpan.FromMinutes(5);
        private static readonly string __cacheKey = "_PaymentGatewayConfiguration_Repository_Cache";

        public PaymentGatewayConfigurationRepository(IObjectContext context, ICacheProvider cacheProvider)
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

        private List<PaymentGatewayConfiguration> GetDataToCache(string connectionString)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                var data = newConnection.Query<PaymentGatewayConfiguration>("select * from PaymentGatewayConfiguration").ToList();
                return data;
            }
        }

        /// <summary>
        /// Desc :: To Get PaymentGatewayConfiguration by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override IEnumerable<PaymentGatewayConfiguration> GetAll()
        {
            return CacheProvider.Get<IEnumerable<PaymentGatewayConfiguration>>(__cacheKey);
        }

        public override PaymentGatewayConfiguration FirstOrDefault(Expression<Func<PaymentGatewayConfiguration, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override PaymentGatewayConfiguration Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the PaymentGatewayConfiguration");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        public List<PaymentGatewayConfiguration> FindByCriteria(out int totalRecords, List<long> propertyIds, List<long> gatewayIds, 
            List<string> gatewayNames, List<string> gatewayCodes,
            bool returnTotal = false, int pageIndex = 0, int pageSize = 0)
        {
            var result = GetAll().AsQueryable();
            totalRecords = -1;

            if (gatewayIds != null && gatewayIds.Count > 0)
                result = result.Where(x => x.GatewayUID.HasValue && gatewayIds.Contains(x.GatewayUID.Value));

            if (gatewayCodes != null && gatewayCodes.Count > 0)
                result = result.Where(x => gatewayCodes.Contains(x.GatewayCode));

            if (gatewayNames != null && gatewayNames.Count > 0)
                result = result.Where(x => gatewayNames.Contains(x.GatewayName));

            if (propertyIds != null && propertyIds.Count > 0)
                result = result.Where(x => propertyIds.Contains(x.PropertyUID));

            if (returnTotal)
                totalRecords = result.Count();

            if (pageIndex > 0 && pageSize > 0)
                result = result.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result.ToList();
        }

        #endregion Cache Methods

        /// <summary>
        /// Get payment Gateway Configurations
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="currencyId"></param>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        public PaymentGatewayQR1 GetActivePaymentGatewayConfiguration(long propertyId, long currencyId, long paymentMethodId)
        {
            var config = new PaymentGatewayQR1();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@propertyId", propertyId, DbType.Int64);
            parameters.Add("@currencyId", currencyId, DbType.Int64);
            parameters.Add("@paymentMethodId", paymentMethodId, DbType.Int64);

            var paymentGateway = this.Context.Context.Database.Connection.Query<PaymentGatewayQR1>(GET_ACTIVE_PAYMENT_GATEWAY_CONFIGURATION, parameters).FirstOrDefault();

            //if (paymentGateways.Count() > 0)
            //    return paymentGateways.FirstOrDefault();
            //else
            return paymentGateway;
        }

        /// <summary>
        /// Get payment Gateway Configurations
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="currencyId"></param>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        public PaymentGatewayQR1 GetActivePaymentGatewayConfigurationReduced(long propertyId, long currencyId)
        {
            var config = new PaymentGatewayQR1();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@propertyId", propertyId, DbType.Int64);
            parameters.Add("@currencyId", currencyId, DbType.Int64);

            var paymentGateway = this.Context.Context.Database.Connection.Query<PaymentGatewayQR1>(GET_ACTIVE_PAYMENT_GATEWAY_CONFIGURATION_REDUCED, parameters).FirstOrDefault();

            //if (paymentGateways.Count() > 0)
            //    return paymentGateways.FirstOrDefault();
            //else
            return paymentGateway;
        }

        private static readonly string GET_ACTIVE_PAYMENT_GATEWAY_CONFIGURATION = @"
                SELECT [pgc].[UID], [pgc].[ApiSignatureKey], COALESCE([pgc].[PaymentAuthorizationTypeId],1) AS [PaymentAuthorizationType],
                COALESCE([pgc].[InstallmentPaymentPlanId],1) AS [InstallmentPaymentPlan], COALESCE([pgc].[Comission],0) AS [Comission], [country].[CountryIsoCode], 
                [country].[countrycode] AS CountryIsoCode2Letters,
                [currency].[Symbol] AS [CurrencyIsoCode], [pgc].[GatewayCode], COALESCE([pgc].[GatewayUID],0) AS [GatewayId], [pgc].[GatewayName], [pgc].[IsActive], [pgc].[MerchantID] AS [MerchantId],
                [pgc].[MerchantKey], COALESCE([pgc].[ProcessorCode],1) AS [ProcessorCode], [pgc].[ProcessorName], [p].[UID] AS [PropertyId], [p].[Name] AS [PropertyName],
                CONVERT(smallint,(CASE
                    WHEN [t7].[test] IS NOT NULL THEN [t7].[Code]
                    ELSE 0
                END)) AS [PaymentMethodCode], [pg].[ApiVersion], [pgc].[MerchantAccount], [pgc].[IsAntiFraudeControlEnable]
                FROM [Properties] AS [p]
                CROSS JOIN [PaymentGatewayConfiguration] AS [pgc]
                INNER JOIN [PaymentGateways] AS [pg] ON [pgc].[GatewayUID] = ([pg].[UID])
                CROSS JOIN [Currencies] AS [currency]
                CROSS JOIN [Countries] AS [country]
                LEFT OUTER JOIN [PaymentProcessors] AS [paymentProcessor] ON ([paymentProcessor].[ProcessorCode] = ([pgc].[ProcessorCode])) AND ([paymentProcessor].[PaymentGatewayUID] = [pg].[UID])
                LEFT OUTER JOIN (
                    SELECT 1 AS [test], [pmpc].[PaymentGatewayId], [pmpc].[PaymentProcessorId], [pmpc].[PaymentMethodId], [pmpc].[Code]
                    FROM [PaymentMethodProcessorCode] AS [pmpc]
                    ) AS [t7] ON (([t7].[PaymentGatewayId]) = [pgc].[GatewayUID]) AND ([t7].[PaymentProcessorId] = [paymentProcessor].[UID]) AND ([t7].[PaymentMethodId] = @paymentMethodId)
                WHERE ([p].[UID] = @propertyId) AND ([pgc].[IsActive] = 1) AND ([pgc].[PropertyUID] = [p].[UID]) AND ([currency].[UID] = @currencyId) AND ([country].[UID] = [p].[Country_UID])";

        private static readonly string GET_ACTIVE_PAYMENT_GATEWAY_CONFIGURATION_REDUCED = @"
                SELECT [t0].[UID], [t0].[ApiSignatureKey], COALESCE([t0].[PaymentAuthorizationTypeId],1) AS [PaymentAuthorizationType],
                COALESCE([t0].[InstallmentPaymentPlanId],1) AS [InstallmentPaymentPlan], COALESCE([t0].[Comission],0) AS [Comission], [t0].[GatewayCode],
                COALESCE([t0].[GatewayUID],0) AS [GatewayId], [t0].[GatewayName], [t0].[IsActive], [t0].[MerchantID] AS [MerchantId], [t0].[MerchantKey],
                COALESCE([t0].[ProcessorCode],1) AS [ProcessorCode], [t0].[ProcessorName], [t1].[ApiVersion], [t0].[PropertyUID] AS [PropertyId], [t2].[Symbol] AS [CurrencyIsoCode],
                [t0].[MerchantAccount]
                FROM [PaymentGatewayConfiguration] AS [t0]
                INNER JOIN [PaymentGateways] AS [t1] ON [t0].[GatewayUID] = ([t1].[UID])
                CROSS JOIN [Currencies] AS [t2]
                LEFT OUTER JOIN [PaymentProcessors] AS [t3] ON [t3].[ProcessorCode] = ([t0].[ProcessorCode])
                WHERE ([t0].[PropertyUID] = @propertyId) AND ([t0].[IsActive] = 1) AND ([t2].[UID] = @currencyId)";

        /// <summary>
        /// Invalidates the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="forceUpdate">if set to <c>true</c> [force update].</param>
        public void Invalidate(bool forceUpdate = false) 
        {
            CacheProvider.Invalidate(__cacheKey, forceUpdate);
        } 
    }
}