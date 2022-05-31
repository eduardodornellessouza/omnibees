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
    internal class ReservationRoomRepository : Repository<ReservationRoom>, IReservationRoomRepository
    {
        public ReservationRoomRepository(IObjectContext context)
            : base(context)
        {
        }

        public List<ReservationRoomQR1> GetReservationRoomsQR1(long reservationUID, long languageUID)
        {
            List<ReservationRoomQR1> result = new List<ReservationRoomQR1>();

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@reservationUID", reservationUID);
            parameters[1] = new SqlParameter("@languagesId", languageUID);

            var connection = _context.Context.Database.Connection;

            result = connection.Query<ReservationRoomQR1>("[dbo].[GetReservation_GetReservationRoom]", parameters).ToList();

            return result;
        }

        public List<ReservationRoom> FindByCriteria(List<long> Ids)
        {
            var result = this.GetQuery();

            if (Ids?.Any() == true)
                result = result.Where(x => Ids.Contains(x.UID));

            return result.ToList();
        }
    }
}