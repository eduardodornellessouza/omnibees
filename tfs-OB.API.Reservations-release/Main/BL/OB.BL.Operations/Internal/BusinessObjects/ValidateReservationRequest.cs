using OB.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ValidateReservationRequest : ValidationBaseRequest
    {
        //public Reservation Reservation { get; set; }
        public bool ValidateGuarantee { get; set; }
        public bool ValidateCancelationCosts { get; set; }
        public bool ValidateAllotment { get; set; }
        public bool OnlyChangeGuestName { get; set; }
        public List<ChildTerm> ChildTerms { get; set; }
        public OB.Domain.Reservations.ReservationsAdditionalData ReservationAdditionalData { get; set; }
        
        public bool IsModifyReservation { get; set; }
        public List<string> RoomNumberToValidate { get; set; }

        public ValidateReservationRequest ()
	    {
            ChildTerms = new List<ChildTerm>();
            RoomNumberToValidate = new List<string>();
	    }
    }
}
