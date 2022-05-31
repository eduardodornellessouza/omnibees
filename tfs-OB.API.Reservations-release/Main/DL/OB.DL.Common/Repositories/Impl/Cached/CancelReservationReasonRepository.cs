using Dapper;
using OB.Api.Core;
using OB.DL.Common.Cache;
using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace OB.DL.Common.Repositories.Impl.Cached
{
    internal class CancelReservationReasonRepository : CachedRepository<CancelReservationReason>, ICancelReservationReasonRepository
    {
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public CancelReservationReasonRepository(IObjectContext context, ICacheProvider cacheProvider)
            : base(context, cacheProvider, TimeSpan.FromMinutes(int.MaxValue))
        {
            var connectionString = context.Context.Database.Connection.ConnectionString;
            SetCacheProvider(() => GetDataToCache(connectionString));
        }

        #region Cache Methods

        private List<CancelReservationReason> GetDataToCache(string connectionString)
        {
            const string query = @"SELECT
	                        c.UID,
	                        c.Name,
	                        cl.UID AS _UID,
	                        cl.Name AS _Name,
	                        cl.CancelReservationReason_UID AS _CancelReservationReason_UID,
	                        cl.Language_UID AS _Language_UID
                        FROM CancelReservationReason c
	                        LEFT JOIN CancelReservationReasonLanguage cl ON c.UID = cl.CancelReservationReason_UID
            ";

            List<dynamic> queryResults;
            using (var newConnection = new SqlConnection(connectionString))
                queryResults = newConnection.Query(query).ToList();

            return queryResults
                .GroupBy(x => x.UID)
                .Select(g =>
                {
                    var first = g.First();

                    return new CancelReservationReason
                    {
                        UID = first.UID,
                        Name = first.Name,
                        CancelReservationReasonLanguages = g.Where(x => x._UID != null)
                            .Select(x => new CancelReservationReasonLanguage
                            {
                                UID = x._UID,
                                Name = x._Name,
                                CancelReservationReason_UID = x._CancelReservationReason_UID,
                                Language_UID = x._Language_UID
                            }).ToList()
                    };
                })
                .ToList();
        }

        public override CancelReservationReason FirstOrDefault(Expression<Func<CancelReservationReason, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override CancelReservationReason Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the CancelReservationReason");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods
    }
}
