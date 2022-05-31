using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Payments
{
    [DataContract]
    public class PaymentGatewayConfiguration
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> GatewayUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GatewayName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GatewayCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ProcessorCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProcessorName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MerchantID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MerchantKey { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Comission { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CreatedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifiedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> DefaultAuthorizationOnly { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> DefaultAuthorizationSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ApiSignatureKey { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MerchantAccount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> PaymentAuthorizationTypeId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> InstallmentPaymentPlanId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ClientSideEncryptionPublicKey { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAntiFraudeControlEnable { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public AntiFraudTypeEnum AntiFraudType { get; set; }
    }

    public enum AntiFraudTypeEnum : byte
    {
        RedShield = 1,
        Cybersource = 2
    } 
}
