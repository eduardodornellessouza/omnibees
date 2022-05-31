using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Base class for Requests that get data for a single property and use Paging.
    /// </summary>
    [DataContract]
    public class ListForPropertyPagedRequest : ListPagedRequest
    {
        /// <summary>
        /// Gets or sets the UID of the Property to search for data
        /// </summary>
        [DataMember(IsRequired = true)]
        public long Property_UID { get; set; }
    }
}