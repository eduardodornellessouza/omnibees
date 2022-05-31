using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListGuaranteeTypeRequest : RequestBase
    {
        /// <summary>
        /// DepositPolicy UID for the Guarantee Types to return
        /// </summary>
        [DataMember]
        public long DepositPolicy_UID { get; set; }


        /// <summary>
        /// Language UID to translate Guarantee Types
        /// </summary>
        [DataMember]
        public long Language_UID { get; set; }

    }
}
