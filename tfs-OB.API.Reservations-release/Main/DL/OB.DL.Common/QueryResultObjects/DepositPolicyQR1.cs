using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class DepositPolicyQR1
    {
        public long UID { get; set; }
        public string DepositPolicyName { get; set; }
        public string DepositPolicyDescription { get; set; }
        public string TranslatedDepositPolicyName { get; set; }
        public string TranslatedDepositPolicyDescription { get; set; }
        public int DepositPolicyDays { get; set; }
        public bool IsDepositCostsAllowed { get; set; }
        public bool DepositCosts { get; set; }
        public Nullable<decimal> Value { get; set; }
        public Nullable<int> PaymentModel { get; set; }
        public Nullable<int> NrNights { get; set; }
        public Nullable<long> RateUID { get; set; }
        public int SortOrder { get; set; }
    }
}