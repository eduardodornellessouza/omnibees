using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Users.
    /// </summary>
    [DataContract]
    public class ListUserRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Users to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of ClientUID's for which to list the Users.
        /// </summary>
        [DataMember]
        public List<long> ClientUIDs { get; set; }

        /// <summary>
        /// List of FirstNames for which to list the Users.
        /// </summary>
        [DataMember]
        public List<string> FirstNames { get; set; }

        /// <summary>
        /// List of LastNames for which to list the Users.
        /// </summary>
        [DataMember]
        public List<string> LastNames { get; set; }

        /// <summary>
        /// List of user's CategoryUID's for which to list the Users.
        /// </summary>
        [DataMember]
        public List<long> userCategoryUIDs { get; set; }
    }
}