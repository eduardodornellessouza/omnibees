using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class CancellationPolicyQR1
    {
        public long UID { get; set; }
        public string CancelPolicyName { get; set; }
        public string CancellationPolicy_Description { get; set; }
        public string TranslatedCancelPolicyName { get; set; }
        public string TranslatedCancellationPolicy_Description { get; set; }
        public int CancellationDays { get; set; }
        public Nullable<long> Property_UID { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsCancellationAllowed { get; set; }
        public bool CancellationCosts { get; set; }
        public Nullable<decimal> Value { get; set; }
        public Nullable<int> PaymentModel { get; set; }
        public Nullable<int> NrNights { get; set; }
        public Nullable<long> RateUID { get; set; }
        public int SortOrder { get; set; }
    }
}