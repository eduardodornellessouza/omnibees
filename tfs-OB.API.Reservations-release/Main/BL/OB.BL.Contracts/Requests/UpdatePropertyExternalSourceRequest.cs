using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the UpdatePropertyExternalSources RESTfull operation.
    /// The PropertyExternalSource Request object that contains all the required objects
    /// (PropertyExternalSource)
    /// to create and insert a PropertyExternalSource and its permissions into the database.
    /// The relationship between the objects are maintained using the UIDs despite the fact that they are not Database UIDs but logical UIDs (valid in the scope of the object graph).
    /// </summary>
    [DataContract]
    public class UpdatePropertyExternalSourceRequest : RequestBase
    {
        [DataMember]
        public List<PropertiesExternalSource> PropertyExternalSource { get; set; }
    }
}
