namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System.Runtime.Serialization;

    [DataContract]
    public class BillingInfo : ContractBase
    {
        public BillingInfo()
        {
        }

        /// <summary>
        /// Address1
        /// </summary>
        [DataMember]
        public string Address1 { get; set; }

        /// <summary>
        /// Address2
        /// </summary>
        [DataMember]
        public string Address2 { get; set; }

        /// <summary>
        /// Contact Name
        /// </summary>
        [DataMember]
        public string ContactName { get; set; }

        /// <summary>
        /// Postal Code
        /// </summary>
        [DataMember]
        public string PostalCode { get; set; }

        /// <summary>
        /// City associated with billing information
        /// </summary>
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// Country id associated with billing information
        /// </summary>
        [DataMember]
        public long? CountryId { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        [DataMember]
        public string Phone { get; set; }

        /// <summary>
        /// State id associated with billing information
        /// </summary>
        [DataMember]
        public long? StateId { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Tax Card Number
        /// </summary>
        [DataMember]
        public string TaxCardNumber { get; set; }
    }
}