using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a light set of Rates.
    /// </summary>
    [DataContract]
    public class ListRateLightRequest : GridPagedRequest
    {
        /// <summary>
        /// The list of UID's for the Rates to return
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of PropertyUID's for which to list the Rates.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// List of Name's for which to list the Rates.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

        /// <summary>
        /// Boolean to select returning also the RoomTypes within each Rate.
        /// </summary>
        [DataMember]
        public bool? IncludeRateRooms { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted Rates / RoomTypes from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }


        /// <summary>
        /// Boolean to select to include Extras in results.
        /// </summary>
        [DataMember]
        public bool? IncludeExtras { get; set; }
        
        /// <summary>
        /// Boolean to select to include Incentives in results.
        /// </summary>
        [DataMember]
        public bool? IncludeIncentives{ get; set; }
        
        /// <summary>
        /// Boolean to select to include Policies in results.
        /// </summary>
        [DataMember]
        public bool? IncludePolicies { get; set; }
        

        /// <summary>
        /// Boolean to select to include RateBuyerGroups in results.
        /// </summary>
        [DataMember]
        public bool? IncludeRateBuyerGroups { get; set; }

        /// <summary>
        /// Boolean to select to include RateCategories in results.
        /// </summary>
        [DataMember]
        public bool? IncludeRateCategories { get; set; }

        /// <summary>
        /// List of TPI_UID's to have inside the RateBuyerGroups's in the Rates to return.
        /// </summary>
        [DataMember]
        public List<long> TPI_UIDs { get; set; }
    }
}