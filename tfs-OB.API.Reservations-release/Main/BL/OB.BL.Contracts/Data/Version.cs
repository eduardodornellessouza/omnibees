using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data
{
    [DataContract]
    public class Version
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Patch { get; set; }
    }
}
