using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateRoomDetails : ContractBase
    {
        public RateRoomDetails()
        {
        }


        [DataMember]
        public long RateRoom_UID;

        [DataMember]
        public IList<RateRoomDetail> Details;
    }
}
