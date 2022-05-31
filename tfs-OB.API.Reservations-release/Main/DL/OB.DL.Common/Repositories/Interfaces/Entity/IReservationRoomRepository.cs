using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IReservationRoomRepository : IRepository<ReservationRoom>
    {
        /// <summary>
        /// Finds the ReservationRooms (ReservationRoomQR1 instances) given the reservationUID and LanguageUID.
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <param name="languageUID"></param>
        /// <returns>A list of ReservatiomRoomQR1 which are QueryResult objects that are not part of any Entity Model.</returns>
        List<ReservationRoomQR1> GetReservationRoomsQR1(long reservationUID, long languageUID);

        /// <summary>
        /// Finds the ReservationRooms (ReservationRoom instances) given a list of Ids.
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns>A list of ReservatiomRoom</returns>
        List<ReservationRoom> FindByCriteria(List<long> Ids);
    }
}