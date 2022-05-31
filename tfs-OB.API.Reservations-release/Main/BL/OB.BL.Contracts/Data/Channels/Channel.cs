using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Channels
{
    [DataContract]
    public partial class Channel : ContractBase
    {
        public Channel()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsBookingEngine { get; set; }

        [DataMember]
        public Nullable<bool> IsExtended { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public Nullable<int> Type { get; set; }

        [DataMember]
        public string IATA_Number { get; set; }

        [DataMember]
        public string IATA_Name { get; set; }

        [DataMember]
        public byte[] Revision { get; set; }

        [DataMember]
        public int OperatorType { get; set; }

        [DataMember]
        public string OperatorCode { get; set; }

        [DataMember]
        public string ChannelCode { get; set; }

        [DataMember]
        public Nullable<int> RealTimeType { get; set; }
    }
}