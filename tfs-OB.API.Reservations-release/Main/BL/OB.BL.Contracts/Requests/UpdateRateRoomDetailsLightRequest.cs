using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for updating room prices.
    /// Lightweigth Object that contains all the required objects
    /// (RateRoomDetailsDataWithChildRecordsCustom) to update and insert the room prices, allotments and close sales into the database.
    /// </summary>
    [DataContract]
    public class UpdateRateRoomDetailsLightRequest : RequestBase
    {
        /// <summary>
        /// List of all RateRoomDetails price combinations including allotments and close sales operations.
        /// </summary>
        [DataMember]
        public List<RateRoomDetailsLightDataWithChildRecordsCustom> RateList { get; set; }

        /// <summary>
        /// UID of the Hotel.
        /// </summary>
        [DataMember]
        public long Property_UID { get; set; }


        #region Excluded Fields
        ///// <summary>
        ///// correlationUID that identifies the update
        ///// </summary>
        //[DataMember]
        //public string correlationUID { get; set; }

        ///// <summary>
        ///// Flag that indicates that the Rate is per Occupancy.
        ///// </summary>
        //[DataMember]
        //public bool? IsRatePerOccupancy { get; set; }
        #endregion Excluded Fields
    }
}