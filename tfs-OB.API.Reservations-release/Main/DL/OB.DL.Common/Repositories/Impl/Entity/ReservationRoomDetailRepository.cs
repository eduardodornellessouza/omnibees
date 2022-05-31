using Dapper;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class ReservationRoomDetailRepository : Repository<ReservationRoomDetail>, IReservationRoomDetailRepository
    {
        public ReservationRoomDetailRepository(IObjectContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Finds the ReservationRoomDetail instances (ReservationRoomDetailQR1 instances) given the reservationUID and LanguageUID.
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <param name="languageUID"></param>
        /// <param name="cancelledStatus"></param>
        /// <returns>A list of ReservatiomRoomDetailQR1 which are QueryResult objects that are not part of any Entity Model.</returns>
        public List<ReservationRoomDetailQR1> GetReservationRoomDetailsQR1(long reservationUID, long languageUID, int cancelledStatus)
        {
            List<ReservationRoomDetailQR1> result = new List<ReservationRoomDetailQR1>();

            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@reservationUID", reservationUID);
            parameters[1] = new SqlParameter("@languagesId", languageUID);
            parameters[2] = new SqlParameter("@cancelledStatus", cancelledStatus);
            var connection = _context.Context.Database.Connection;

            result = connection.Query<ReservationRoomDetailQR1>("[dbo].[GetReservation_GetReservationRoomDetails]", parameters).ToList();

            return result;
        }
    }
}