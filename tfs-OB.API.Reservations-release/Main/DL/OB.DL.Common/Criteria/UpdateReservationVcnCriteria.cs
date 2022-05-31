using System;

namespace OB.DL.Common.Criteria
{
    public class UpdateReservationVcnCriteria
    {
        public long ReservationId { get; set; }
        public string VcnReservationId { get; set; }
        public string VcnToken { get; set; }
        public string CreditCardHolderName { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardCVV { get; set; }
        public DateTime CreditCardExpirationDate { get; set; }
        public byte[] CreditCardHashCode { get; set; }

        public UpdateReservationVcnCriteria()
        {
            
        }
    }
}
