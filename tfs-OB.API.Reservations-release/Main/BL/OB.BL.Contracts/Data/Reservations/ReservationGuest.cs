namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationGuest : ContractBase
    {
        public ReservationGuest()
        {
        }

        /// <summary>
        /// First name 
        /// </summary>
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name 
        /// </summary>
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        [DataMember]
        public string Phone { get; set; }

        /// <summary>
        /// Id Card Number
        /// </summary>
        [DataMember]
        public string IdCardNumber { get; set; }

        /// <summary>
        /// Country id associated with guest
        /// </summary>
        [DataMember]
        public long? CountryId { get; set; }

        /// <summary>
        /// State id associated with guest
        /// </summary>
        [DataMember]
        public long? StateId { get; set; }

        /// <summary>
        /// City associated with guest
        /// </summary>
        [DataMember]
        public string City { get; set; }

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
        /// Postal Code
        /// </summary>
        [DataMember]
        public string PostalCode { get; set; }        
    }
}