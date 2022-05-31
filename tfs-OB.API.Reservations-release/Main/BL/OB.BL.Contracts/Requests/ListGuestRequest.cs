using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Guests.
    /// </summary>
    [DataContract]
    public class ListGuestRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of PrefixUIDs for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> PrefixUIDs { get; set; }

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
        /// The list of Address1's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Address1s { get; set; }

        /// <summary>
        /// The list of Address2's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Address2s { get; set; }

        /// <summary>
        /// The list of Cities for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Cities { get; set; }

        /// <summary>
        /// The list of PostalCodes for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> PostalCodes { get; set; }

        /// <summary>
        /// The list of BillingAddress1's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingAddress1s { get; set; }

        /// <summary>
        /// The list of BillingAddress2's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingAddress2s { get; set; }

        /// <summary>
        /// The list of BillingCity's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingCitys { get; set; }

        /// <summary>
        /// The list of BillingPostalCode's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingPostalCodes { get; set; }

        /// <summary>
        /// The list of BillingPhone's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingPhones { get; set; }

        /// <summary>
        /// The list of BillingExt's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingExts { get; set; }

        /// <summary>
        /// The list of BillingCountryUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> BillingCountryUIDs { get; set; }

        /// <summary>
        /// The list of CountryUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> CountryUIDs { get; set; }

        /// <summary>
        /// The list of Phone's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Phones { get; set; }

        /// <summary>
        /// The list of PhoneExt's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> PhoneExts { get; set; }

        /// <summary>
        /// The list of MobilePhone's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> MobilePhones { get; set; }

        /// <summary>
        /// The list of UserName's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> UserNames { get; set; }

        /// <summary>
        /// The list of PasswordHint's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> PasswordHints { get; set; }

        /// <summary>
        /// The list of GuestCategoryUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> GuestCategoryUIDs { get; set; }

        /// <summary>
        /// The list of PropertyUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

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
        /// The list of Birthday's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<System.DateTime> Birthdays { get; set; }

        /// <summary>
        /// The status of IsActive for the Guests to return.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// The list of CreatedByTPI_UID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> CreatedByTPI_UIDs { get; set; }

        /// <summary>
        /// The list of FacebookUser's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> FacebookUsers { get; set; }

        /// <summary>
        /// The list of TwitterUser's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> TwitterUsers { get; set; }

        /// <summary>
        /// The list of TripAdvisorUser's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> TripAdvisorUsers { get; set; }

        /// <summary>
        /// The status of AllowMarketing for the Guests to return.
        /// </summary>
        [DataMember]
        public bool AllowMarketing { get; set; }

        /// <summary>
        /// The list of QuestionUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> QuestionUIDs { get; set; }

        /// <summary>
        /// The list of State's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> States { get; set; }

        /// <summary>
        /// The status of IsFacebookFan for the Guests to return.
        /// </summary>
        [DataMember]
        public bool IsFacebookFan { get; set; }

        /// <summary>
        /// The status of IsTwitterFollower for the Guests to return.
        /// </summary>
        [DataMember]
        public bool IsTwitterFollower { get; set; }

        /// <summary>
        /// The list of IDCardNumber's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> IDCardNumbers { get; set; }

        /// <summary>
        /// The list of BillingStateUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> BillingStateUIDs { get; set; }

        /// <summary>
        /// The list of StateUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> StateUIDs { get; set; }

        /// <summary>
        /// The list of BillingEmail's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingEmails { get; set; }

        /// <summary>
        /// The list of ClientUID's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<long> ClientUIDs { get; set; }

        /// <summary>
        /// The list of BillingContactName's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingContactNames { get; set; }

        /// <summary>
        /// The list of BillingTaxCardNumber's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingTaxCardNumbers { get; set; }

        /// <summary>
        /// The status of UseDifferentBillingInfo for the Guests to return.
        /// </summary>
        [DataMember]
        public bool UseDifferentBillingInfo { get; set; }

        /// <summary>
        /// The status of IsImportedFromExcel for the Guests to return.
        /// </summary>
        [DataMember]
        public bool IsImportedFromExcel { get; set; }

        /// <summary>
        /// The list of BillingState's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> BillingStates { get; set; }

        /// <summary>
        /// The list of Gender's for the Guests to return.
        /// </summary>
        [DataMember]
        public List<string> Genders { get; set; }
    }
}