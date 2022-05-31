using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for ChannelRoomCodes.
    /// </summary>
    [DataContract]
    public class ListChannelRoomCodeRequest : PagedRequestBase
    {
        /// <summary>
        /// ChannelUID used for filtering, this is required!
        /// </summary>
        [DataMember]
        public long ChannelUID { get; set; }

        /// <summary>
        /// List of PropertyUIDs (optional). If given it will filter by the given ChannelUID AND any of the given PropertyUIDs
        /// and any of the given codes.
        /// For example, if a ChannelUID=1, PropertyUIDs=[100,101,102], Codes=[A,B,C] it will return the ChannelRoomCode instances
        /// for codes A or B or C associated with Properties 100 or 101 or 102 for Channel with UID=1.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// List of ChannelRoomCode.Code values (optional). If given it will filter by the given ChannelUID AND any of the given PropertyUIDs
        /// and any of the given Codes.
        /// For example, if a ChannelUID=1, PropertyUIDs=[100,101,102], Codes=[A,B,C] it will return the ChannelRoomCode instances
        /// for codes A or B or C associated with Properties 100 or 101 or 102 for Channel with UID=1.
        /// </summary>
        [DataMember]
        public List<string> Codes { get; set; }
    }
}