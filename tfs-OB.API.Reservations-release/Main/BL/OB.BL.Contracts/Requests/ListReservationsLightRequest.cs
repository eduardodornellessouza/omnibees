using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ReservationsExternalSources.
    /// </summary>
    [DataContract]
    public class ListReservationsLightRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of UID's for which to list the ReservationsUIDs.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of Channel UID's for which to list the ReservationsUIDs.
        /// </summary>
        [DataMember]
        public List<long> ChannelsUIDs { get; set; }

        /// <summary>
        /// The list of Property UID's for which to list the ReservationsUIDs.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }


        /// <summary>
        /// List of DateFrom for which to search the ReservationsUIDs.
        /// </summary>
        [DataMember]
        public DateTime? DateFrom { get; set; }
        
        /// <summary>
        /// List of DateTo for which to search the ReservationsUIDs.
        /// </summary>
        [DataMember]
        public DateTime? DateTo { get; set; }
        
        /// <summary>
        /// Boolean to indicate if IsPropertyActive to include only reservations for active properties.
        /// </summary>
        [DataMember]
        public bool IsPropertyActive { get; set; }

        /// <summary>
        /// Boolean to indicate if DateFrom > DateTo is to search for ReservationsUIDs Modified or Created within that period.
        /// </summary>
        [DataMember]
        public bool IsDateFindModifications { get; set; }

        
        /// <summary>
        /// Boolean to indicate if DateFrom > DateTo is to search for ReservationsUIDs, with Arrivals which intersect with that period.
        /// </summary>
        [DataMember]
        public bool isDateFindArrivals { get; set; }
                       
        
        /// <summary>
        /// Boolean to indicate if DateFrom > DateTo is to search for ReservationsUIDs, with Room Stays which intersect with that period.
        /// </summary>
        [DataMember]
        public bool IsDateFindStays { get; set; }


        /// <summary>
        /// Boolean to indicate if ReservationsRoomLight are to include in response.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRooms { get; set; }

    }
}