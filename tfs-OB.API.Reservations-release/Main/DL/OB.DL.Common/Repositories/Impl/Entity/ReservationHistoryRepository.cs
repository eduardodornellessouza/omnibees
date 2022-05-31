using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class ReservationHistoryRepository : Repository<ReservationsHistory>, IReservationHistoryRepository
    {
        public ReservationHistoryRepository(IObjectContext context)
            : base(context)
        {
        }

        /// <summary>
        /// This method Finds all ReservationHistoryLight instances given a search criteria.
        /// Instances returned by this method don't support Tracking.
        ///
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="reservationHistoryUIDs"></param>
        /// <param name="reservationNumbers"></param>
        /// <param name="statuses"></param>
        /// <param name="minChangedDate"></param>
        /// <param name="maxChangedDate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        public IEnumerable<ReservationHistoryQR1> FindByCriteria(out int totalRecords, List<long> reservationHistoryUIDs, List<long> reservationUids, List<string> reservationNumbers,
            List<string> statuses,
            DateTime? minChangedDate,
            DateTime? maxChangedDate,
            int pageIndex = -1, int pageSize = -1,
            bool returnTotal = false)
        {
            var query = this.GetQuery();
            totalRecords = -1;

            if (reservationHistoryUIDs != null && reservationHistoryUIDs.Count > 0)
                query = query.Where(x => reservationHistoryUIDs.Contains(x.UID));

            if (reservationUids != null && reservationUids.Count > 0)
                query = query.Where(x => x.ReservationUID.HasValue &&  reservationUids.Contains(x.ReservationUID.Value));

            if (reservationNumbers != null && reservationNumbers.Count > 0)
                query = query.Where(x => reservationNumbers.Contains(x.ReservationNumber));

            if (statuses != null && statuses.Count > 0)
                query = query.Where(x => statuses.Contains(x.Status));

            if (minChangedDate.HasValue && minChangedDate != DateTime.MinValue)
                query = query.Where(x => x.ChangedDate.Value >= minChangedDate.Value);

            if (maxChangedDate.HasValue && maxChangedDate != DateTime.MinValue)
                query = query.Where(x => x.ChangedDate.Value <= maxChangedDate.Value);

            if (returnTotal)
                totalRecords = query.Count();

            if (pageIndex > 0 && pageSize > 0)
                query = query.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
            {
                query = query.Take(pageSize);
            }

            return query.Select(x => new ReservationHistoryQR1
            {
                UID = x.UID,
                ReservationUID = x.ReservationUID,
                ReservationNumber = x.ReservationNumber,
                UserName = x.UserName,
                Channel = x.Channel,
                ChangedDate = x.ChangedDate,      
                StatusUID = x.StatusUID,
                Status = x.Status,
                Message = (!string.IsNullOrEmpty(x.Message) && x.Message.StartsWith("{")) ? x.Message : string.Empty

            }).ToList();
        }
    }
}