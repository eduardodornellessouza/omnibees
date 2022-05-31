using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// A Request sent for approving or cancelling a on request reservation.
    /// </summary>
    [DataContract]
    public class ApproveOrRefuseOnRequestReservationRequest : RequestBase
    {
        /// <summary>
        /// The reservation ID that will be canceled or aprooved.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long ReservationId { get; set; }
        /// <summary>
        /// Is the reservation should be either Approved or Canceled.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public bool? IsApprove { get; set; }
        /// <summary>
        /// The User Id making this decision.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long UserId { get; set; }
        /// <summary>
        /// The Channel ID of the reservation.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long? ChannelId { get; set; }
        /// <summary>
        /// The transaction Id of the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TransactionId { get; set; }

        /// <summary>
        /// The User Type making this decision.
        /// </summary>
        [DataMember]
        public int? OBUserType { get; set; }

    }
}
