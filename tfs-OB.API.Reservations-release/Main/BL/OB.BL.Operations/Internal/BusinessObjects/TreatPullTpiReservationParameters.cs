using OB.BL.Contracts.Data.CRM;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using System.Collections.Generic;
using OBRateContracts = OB.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Data;
using OB.Reservation.BL.Contracts.Requests;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class TreatPullTpiReservationParameters
    {
        public contractsReservation.Reservation Reservation { get; set; }
        public List<contractsReservation.ReservationRoom> Rooms { get; set; }
        public List<contractsReservation.ReservationRoomChild> ReservationRoomChilds { get; set; }
        public List<contractsReservation.ReservationRoomDetail> ReservationRoomDetails { get; set; }
        public contractsReservation.ReservationsAdditionalData AddicionalData { get; set; }
        public ReservationDataContext ReservationContext { get; set; }
        public Guest Guest { get; set; }
        public GroupRule GroupRule { get; set; }
        public List<contractsReservation.ReservationRoomExtra> ReservationRoomExtras { get; set; }
        public bool IsInsert { get; set; }
        public Version Version { get; set; }
        public OBRateContracts.PromotionalCode PromotionalCode { get; set; }
        public ReservationBaseRequest ReservationRequest { get; set; }
        public System.Guid? RequestGuid { get; set; }
        /// <summary>
        /// Set <c>true</c> if payment method type of reservation is not configured in rate.
        /// </summary>
        public bool PaymentMethodTypeNotConfiguredInRate { get; set; }
    }
}
