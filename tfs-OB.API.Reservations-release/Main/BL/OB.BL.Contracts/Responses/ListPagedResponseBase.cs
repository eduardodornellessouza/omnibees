using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPagedResponseBase : PagedResponseBase
    {
        [DataMember]
        public string ResultTypeName { get; set; }

        private IList<ContractBase> result;

        [DataMember]
        public IList<ContractBase> Result
        {
            get
            {
                return result;
            }
            set
            {
                if (string.IsNullOrEmpty(ResultTypeName))
                {
                    if (value != null && value.Count > 0)
                        ResultTypeName = value.First().GetType().FullName;
                }
                result = value;
            }
        }
    }
}