using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    [DataContract]
    public class UpdateRatesGridLineDetail : BaseGridLineDetail
    {
        public UpdateRatesGridLineDetail()
            : base()
        {

        }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Allotment { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<UpdatePeriod> Dates { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int CloseSales { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Rates { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Rooms { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Prices { get; set; }
    }
}
