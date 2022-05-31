using OB.Reservation.BL.Contracts.Data.Channels;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of PropertiesExternalSource configuration objects.
    /// </summary>
    [DataContract]
    public class ListPropertiesExternalSourcePermissionsResponse : PagedResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PropertiesExternalSourcesPermission> Result { get; set; }

        /// <summary>
        /// Dictionary with the Rates definition by Rates UID.
        /// </summary>
        [DataMember]
        public Dictionary<long, RateLight> RatesLookup { get; set; }

        /// <summary>
        /// Dictionary with the RoomTypes definition by RoomTypes UID.
        /// </summary>
        [DataMember]
        public Dictionary<long, RoomType> RoomTypesLookup { get; set; }

        /// <summary>
        /// Dictionary with the RoomTypes definition by RoomTypes UID.
        /// </summary>
        [DataMember]
        public Dictionary<long, Channel> ChannelsLookup { get; set; }
    }
}