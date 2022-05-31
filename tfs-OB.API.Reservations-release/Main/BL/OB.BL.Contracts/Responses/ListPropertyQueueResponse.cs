using OB.Reservation.BL.Contracts.Data.ProactiveActions;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response class that provides a list of found PropertyQueue data transfer objects.
    /// </summary>
    [DataContract]
    public class ListPropertyQueueResponse : ListGenericPagedResponse<PropertyQueue>
    {
    }
}