using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// The request sent to Export to FTP.
    /// </summary>
    public class ExportReservationsToFtpRequest : RequestBase
    {
        /// <summary>
        /// The external source code marking to which external source should be exported.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ExternalSourceCode { get; set; }

        /// <summary>
        /// The Property Id used to generate the data.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        /// <summary>
        /// The Date From used as start date to generate the data.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The Date To used as ending date to generate the data.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTo { get; set; }
    }
}
