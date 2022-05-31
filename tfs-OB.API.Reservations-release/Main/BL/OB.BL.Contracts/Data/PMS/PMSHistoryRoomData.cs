using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.PMS
{
    [DataContract]
    public class PMSHistoryRoomData : ContractBase
    {
        public PMSHistoryRoomData()
        {
        }

        [DataMember]
        public string PMSNumber { get; set; }

        [DataMember]
        public Guid Guid { get; set; }


    }
}