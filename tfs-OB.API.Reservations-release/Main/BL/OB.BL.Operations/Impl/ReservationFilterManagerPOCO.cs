using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.BL.Operations.Internal.TypeConverters;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Exceptions;

namespace OB.BL.Operations.Impl
{
    public partial class ReservationFilterManagerPOCO : BusinessPOCOBase, IReservationFilterManagerPOCO
    {
        public ReservationFilterManagerPOCO() { }

        #region RESERVATION FILTER CRUD

        /// <summary>
        /// Insert ReservationFilter
        /// </summary>
        /// <param name="reservationFilter"></param>
        /// <returns></returns>
        public Domain.Reservations.ReservationFilter InsertReservationFilter(Domain.Reservations.ReservationFilter reservationFilter)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                repo.Add(reservationFilter);
                unitOfWork.Save();
            }
            return reservationFilter;
        }

        /// <summary>
        /// Update ReservationFilter
        /// </summary>
        /// <param name="reservationFilter"></param>
        /// <returns></returns>
        public Domain.Reservations.ReservationFilter UpdateReservationFilter(Domain.Reservations.ReservationFilter reservationFilter)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                repo.AttachAsModified(reservationFilter);
                unitOfWork.Save();
            }
            return reservationFilter;
        }

        #endregion
    }
}
