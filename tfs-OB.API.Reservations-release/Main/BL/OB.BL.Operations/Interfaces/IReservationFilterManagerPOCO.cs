using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Interfaces
{
    public interface IReservationFilterManagerPOCO : IBusinessPOCOBase
    {
        /// <summary>
        /// Insert ReservationFilter
        /// </summary>
        /// <param name="reservationFilter"></param>
        /// <returns></returns>
        Domain.Reservations.ReservationFilter InsertReservationFilter(Domain.Reservations.ReservationFilter reservationFilter);

        /// <summary>
        /// Update ReservationFilter
        /// </summary>
        /// <param name="reservationFilter"></param>
        /// <returns></returns>
        Domain.Reservations.ReservationFilter UpdateReservationFilter(Domain.Reservations.ReservationFilter reservationFilter);
    }
}
