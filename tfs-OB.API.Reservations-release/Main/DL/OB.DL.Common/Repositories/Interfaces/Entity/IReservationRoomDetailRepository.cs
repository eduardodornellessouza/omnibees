using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IReservationRoomDetailRepository : IRepository<ReservationRoomDetail>
    {
        /// <summary>
        /// Finds the ReservationRoomDetail instances (ReservationRoomDetailQR1 instances) given the reservationUID and LanguageUID.
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <param name="languageUID"></param>
        /// <param name="cancelledStatus"></param>
        /// <returns>A list of ReservatiomRoomDetailQR1 which are QueryResult objects that are not part of any Entity Model.</returns>
        List<ReservationRoomDetailQR1> GetReservationRoomDetailsQR1(long reservationUID, long languageUID, int cancelledStatus);
    }
}