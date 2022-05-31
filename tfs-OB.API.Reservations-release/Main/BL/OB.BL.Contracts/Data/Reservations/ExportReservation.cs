using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ExportReservation : ContractBase
    {
        public ExportReservation()
        {
        }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public int NumberOfDays { get; set; }

        [DataMember]
        public int NumberOfAdults { get; set; }

        [DataMember]
        public int NumberOfChildren { get; set; }
        
        [DataMember]
        public int NumberOfRooms { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public string CreatedDate { get; set; }

        [DataMember]
        public string GuestFirstName { get; set; }

        [DataMember]
        public string GuestLastName { get; set; }

        [DataMember]
        public string DateFrom { get; set; }

        [DataMember]
        public string DateTo { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public string CommissionRate { get; set; }

        [DataMember]
        public string CommissionableRevenue { get; set; }

        [DataMember]
        public string CommissionAmount { get; set; }

        [DataMember]
        public string TransactionCode { get; set; }

        [DataMember]
        public string PayeeId { get; set; }

        [DataMember]
        public string PayeeLegalName { get; set; }

        [DataMember]
        public string PayeeName { get; set; }

        [DataMember]
        public string PayeeAddress { get; set; }

        [DataMember]
        public string PayeeCity { get; set; }

        [DataMember]
        public string PayeeStateCode { get; set; }

        [DataMember]
        public string PayeePostalCode { get; set; }

        [DataMember]
        public string PayeeCountryCode { get; set; }
    }
}