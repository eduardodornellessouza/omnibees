using OB.DL.Common.QueryResultObjects;
using System.Collections.Generic;
using contracts = OB.Reservation.BL.Contracts.Data.Reservations;
using OBCRMContracts = OB.BL.Contracts.Data.CRM;
using domain = OB.Domain.Reservations;
using OBRateContracts = OB.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Requests;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class TreatBEReservationParameters
    {
        public contracts.Reservation Reservation { get; set; }
        public List<contracts.ReservationRoom> Rooms { get; set; }
        public List<contracts.ReservationRoomChild> ReservationRoomChilds { get; set; }
        public List<contracts.ReservationRoomDetail> ReservationRoomDetails { get; set; }
        public List<contracts.ReservationRoomExtra> ReservationRoomExtras { get; set; }
        public ReservationDataContext ReservationContext { get; set; }
        public OBCRMContracts.Guest Guest { get; set; }
        public domain.GroupRule GroupRule { get; set; }
        public OBRateContracts.PromotionalCode PromotionalCode { get; set; }
        public ReservationBaseRequest ReservationRequest { get; set; }
    }
}
