namespace OB.DL.Common.Criteria
{
    public class UpdateCreditCriteria
    {
        public long TpiUid { get; set; }
        public long ChannelUid { get; set; }
        public long PropertyUid { get; set; }
        public decimal IncrementValue { get; set; }
        public bool UpdateCreditUsed { get; set; }
        public bool UpdatePrePaidCreditUsed { get; set; }
    }
}
