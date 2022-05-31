using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Domain
{
    public class ChannelProperties
    {
        public long UID { get; set; }
        public long Channel_UID { get; set; }
        public long Property_UID { get; set; }
        public string UserPassword { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public long? RateModel_UID { get; set; }
        public decimal Value { get; set; }
        public bool IsPercentage { get; set; }
        public byte[] ContractPdf { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPendingRequest { get; set; }
        public long? ChannelCommissionCategory_UID { get; set; }
        public string ContractName { get; set; }
        public string IncomingOfficeCode { get; set; }
        public string Sequence { get; set; }
        public string HotelId { get; set; }
        public string ChainId { get; set; }
        public byte[] Revision { get; set; }
        public long? OperatorBillingType { get; set; }
        public decimal? OperatorCreditLimit { get; set; }
        public decimal? OperatorCreditUsed { get; set; }
        public bool IsOperatorsCreditLimit { get; set; }
        public long? Commission { get; set; }
        public long? Markup { get; set; }
        public long? Package { get; set; }
        public bool IsActivePrePaymentCredit { get; set; }
        public decimal? PrePaymentCreditLimit { get; set; }
        public decimal? PrePaymentCreditUsed { get; set; }
        public bool IsOnRequestEnable { get; set; }
        public decimal? PriceVariation { get; set; }
    }
}
