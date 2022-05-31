namespace OB.DL.Common.QueryResultObjects
{
    public class PaymentGatewayQR1
    {
        public long UID { get; set; }

        public long PropertyId { get; set; }

        public string PropertyName { get; set; }

        public long GatewayId { get; set; }

        public string GatewayName { get; set; }

        public string GatewayCode { get; set; }

        public int ProcessorCode { get; set; }

        public string ProcessorName { get; set; }

        public string MerchantId { get; set; }

        public string MerchantKey { get; set; }

        public bool IsActive { get; set; }

        public decimal Comission { get; set; }

        public string ApiSignatureKey { get; set; }

        public string CurrencyIsoCode { get; set; }

        public string ApiVersion { get; set; }

        public string CountryIsoCode { get; set; }
        public string CountryIsoCode2Letters { get; set; }

        public short PaymentMethodCode { get; set; }

        public string PaymentGatewayTransactionStatusDescription { get; set; }

        public string MerchantAccount { get; set; }

        public long PaymentAuthorizationType { get; set; }

        public int InstallmentPaymentPlan { get; set; }

        public bool IsAntiFraudeControlEnable { get; set; }

        //public string StateCode { get; set; }
    }
}