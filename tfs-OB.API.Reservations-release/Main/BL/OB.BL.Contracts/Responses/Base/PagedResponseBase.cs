using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Base class for RESTful responses that support pagination.
    /// </summary>
    [DataContract]
    public class PagedResponseBase : ResponseBase
    {
        [DataMember]
        public int TotalRecords { get; set; }
    }
}