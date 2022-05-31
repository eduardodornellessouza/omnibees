using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    /// <summary>
    /// This class is a copy from OB.Events.Contracts
    /// </summary>
    [DataContract]
    public class BaseLogDetail
    {

        public BaseLogDetail()
        {
            EntityGroups = new List<EntityGroup>();
            Identifiers = new List<Identifier>();
        }

        /// <summary>
        /// Action is for example: Update Rates, Rate Code Setup (translated)
        /// </summary>
        [DataMember]
        public string Action { get; set; }

        /// <summary>
        /// Summary is the Rate Name for Example or "Multi Rates".
        /// It is a summary of the Log!
        /// </summary>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Created Date of the Log
        /// </summary>
        [DataMember]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Log Created By floule for example (Username)
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public List<Identifier> Identifiers { get; set; }

        [DataMember]
        public List<EntityGroup> EntityGroups { get; set; }
    }
}
