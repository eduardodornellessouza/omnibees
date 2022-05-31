using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using OB.Api.Core;
using OB.DL.Common.Criteria;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Cached;

namespace OB.DL.Common.Repositories.Impl.Cached
{
    internal class ReservationStatusRepository : CachedRepository<ReservationStatus>, IReservationStatusRepository
    {
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        /// <param name="languageRepository">Injected</param>
        public ReservationStatusRepository(IObjectContext context, ICacheProvider cacheProvider)
            : base(context, cacheProvider, TimeSpan.FromMinutes(int.MaxValue))
        {
            var connectionString = context.Context.Database.Connection.ConnectionString;
            SetCacheProvider(() => GetDataToCache(connectionString));
        }

        #region Cache Methods

        private List<ReservationStatus> GetDataToCache(string connectionString)
        {
            string query = @"
            SELECT * FROM ReservationStatuses;
            SELECT * FROM ReservationStatusLanguage;
            ";

            IEnumerable<ReservationStatus> status;
            IEnumerable<ReservationStatusLanguage> statusLanguages;

            using (var newConnection = new SqlConnection(connectionString))
            using (var multi = newConnection.QueryMultiple(query))
            {
                status = multi.Read<ReservationStatus>();
                statusLanguages = multi.Read<ReservationStatusLanguage>();
            }

            foreach (var s in status)
            {
                s.ReservationStatusLanguages = statusLanguages.Where(l => l.ReservationStatus_UID == s.UID).ToList();
            }

            return status.ToList();
        }

        public override ReservationStatus FirstOrDefault(Expression<Func<ReservationStatus, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override ReservationStatus Get(params object[] values)
        {
            Contract.Assert(values.Length == 1, "UID Primary key required to Get the ReservationStatus");

            return this.GetAll().FirstOrDefault(x => x.UID == (long)values[0]);
        }

        #endregion Cache Methods

        #region Finders
        public IEnumerable<ReservationStatus> FindByCriteria(ListReservationStatusCriteria criteria)
        {
            var result = this.GetAll().AsQueryable();

            if (criteria.UIDs != null && criteria.UIDs.Any())
                result = result.Where(x => criteria.UIDs.Contains(x.UID));

            if (criteria.IncludeLanguages)
                result = result.Include(x => x.ReservationStatusLanguages);

            return result;
        }
        #endregion
    }
}