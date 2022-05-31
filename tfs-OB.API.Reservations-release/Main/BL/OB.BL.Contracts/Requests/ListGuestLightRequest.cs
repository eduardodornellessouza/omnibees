using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Guests.
    /// </summary>
    [DataContract]
    public class ListGuestLightRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of PropertyUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// The list of CountryUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> CountryUIDs { get; set; }

        /// <summary>
        /// The list of UserName's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> UserNames { get; set; }

        /// <summary>
        /// The list of CurrencyUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> CurrencyUIDs { get; set; }

        /// <summary>
        /// The list of LanguageUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> LanguageUIDs { get; set; }

        /// <summary>
        /// The list of Email's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Emails { get; set; }

        /// <summary>
        /// The list of FirstNames for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> FirstNames { get; set; }

        /// <summary>
        /// The list of LastNames for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> LastNames { get; set; }

        /// <summary>
        /// The status of IsActive for the Guests to return.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// The status of AllowMarketing for the Guests to return.
        /// </summary>
        [DataMember]
        public bool AllowMarketing { get; set; }

        /// <summary>
        /// The list of Gender's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Genders { get; set; }

        /// <summary>
        /// The list of ClientUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> ClientUIDs { get; set; }

        [DataMember]
        public List<long> LoyaltyLevelUIDs { get; set; }

        [DataMember]
        public bool ExcludeDeleted { get; set; }
    }
}