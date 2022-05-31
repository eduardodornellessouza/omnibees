using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Images.
    /// </summary>
    [DataContract]
    public class ListImageRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Images to return
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of PropertyUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }
        
        /// <summary>
        /// List of RoomTypeUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> RoomTypeUIDs { get; set; }
        
        /// <summary>
        /// List of PackageUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> PackageUIDs { get; set; }
        
        /// <summary>
        /// List of AttractionUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> AttractionUIDs { get; set; }
        
        /// <summary>
        /// List of ExtrasUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> ExtrasUIDs { get; set; }
                
        /// <summary>
        /// List of ClientUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> ClientUIDs { get; set; }
                
        /// <summary>
        /// List of CalenderEventsUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> CalenderEventsUIDs { get; set; }
                
        /// <summary>
        /// List of ThirdPartyIntermediaryUID's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> ThirdPartyIntermediaryUIDs { get; set; }
                
        /// <summary>
        /// List of MediaCategoryUIDs's for which to list the Images.
        /// </summary>
        [DataMember]
        public List<long> MediaCategoryUIDs { get; set; }
                


        /// <summary>
        /// Boolean to Include the Images File in results.
        /// </summary>
        [DataMember]
        public bool? IncludeImage { get; set; }

        /// <summary>
        /// Boolean to select only IsBeDeafultImages from results.
        /// </summary>
        [DataMember]
        public bool? IsBeDefault { get; set; }
        
        /// <summary>
        /// Boolean to select to exclude deleted Rooms from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

    }
}