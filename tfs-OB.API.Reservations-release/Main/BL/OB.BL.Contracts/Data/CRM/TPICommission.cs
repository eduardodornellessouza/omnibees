using System;
using System.Runtime.Serialization;


namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class TPICommission : ContractBase
    {
        [DataMember]
        public Nullable<decimal> PercentageCommission { get; set; }

        [DataMember]
        public Nullable<bool> CommissionIsPercentage { get; set; }
    }
}
