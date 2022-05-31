using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.PortalOperadoras
{
    [DataContract]
    public class SellRule : ContractBase
    {
        [DataMember]
        public decimal TaxCurrencyValue { get; set; }
        [DataMember]
        public bool TaxIsPercentage { get; set; }
        [DataMember]
        public decimal Tax { get; set; }
        [DataMember]
        public bool CommissionIsPercentage { get; set; }
        [DataMember]
        public decimal Commission { get; set; }
        [DataMember]
        public ExternalApplianceType CommissionType { get; set; }
        [DataMember]
        public decimal MarkupCurrencyValue { get; set; }
        [DataMember]
        public long CurrencyBaseUID { get; set; }
        [DataMember]
        public bool MarkupIsPercentage { get; set; }
        [DataMember]
        public ExternalApplianceType MarkupType { get; set; }
        [DataMember]
        public ExternalRatesTypeTarget RatesTypeTarget { get; set; }
        [DataMember]
        public int RuleType { get; set; }
        [DataMember]
        public long PosCode { get; set; }
        [DataMember]
        public string ExternalName { get; set; }
        [DataMember]
        public int KeeperType { get; set; }
        [DataMember]
        public long KeeperUid { get; set; }
        [DataMember]
        public decimal Markup { get; set; }
        [DataMember]
        public long CurrencyValueUID { get; set; }
    }
}
