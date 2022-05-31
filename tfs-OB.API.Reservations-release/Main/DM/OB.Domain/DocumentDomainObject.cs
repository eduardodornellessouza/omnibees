using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain
{
    /// <summary>
    /// Class that represents a DomainObject which can be serialized into a JSON/NoSQL Document.
    /// </summary>
    [Serializable]
    public class DocumentDomainObject : DomainObject
    {
        /// <summary>
        /// Id of the document in the NoSQL database.
        /// </summary>
        public string DocumentId { get; set; }
    }
}
