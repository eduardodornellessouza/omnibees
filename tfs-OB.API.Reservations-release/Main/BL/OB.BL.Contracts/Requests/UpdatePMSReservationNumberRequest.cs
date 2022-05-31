using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.PMS;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdatePMSReservationNumberRequest : RequestBase
    {
        public UpdatePMSReservationNumberRequest()
        {
            PMSReservationNumberByReservationRoomUID = new Dictionary<long, PMSHistoryRoomData>();
            PMSReservationNumberByReservationUID = new Dictionary<long, string>();
        }

        [DataMember]
        public long? PmsId { get; set; }

        [DataMember]
        public long? ClientId { get; set; }

        [DataMember]
        public Dictionary<long, PMSHistoryRoomData> PMSReservationNumberByReservationRoomUID { get; set; }

        [DataMember]
        public Dictionary<long, string> PMSReservationNumberByReservationUID { get; set; }
    }
}