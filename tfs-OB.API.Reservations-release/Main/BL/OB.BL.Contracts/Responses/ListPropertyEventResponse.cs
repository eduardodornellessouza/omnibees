using OB.Reservation.BL.Contracts.Data.ProactiveActions;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response class that provides a list of found PropertyEvent data transfer objects.
    /// </summary>
    [DataContract]
    public class ListPropertyEventResponse : ListGenericPagedResponse<PropertyEvent>
    {
    }
}