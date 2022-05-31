using System;

namespace OB.DL.Common.QueryResultObjects
{
    [Serializable]
    public partial class ReservationRoomIncentiveQR1
    {
        public int Days { get; set; }

        public int DiscountPercentage { get; set; }

        public int? FreeDays { get; set; }

        public string IncentiveType { get; set; }

        public long IncentiveType_UID { get; set; }

        public bool IsDeleted { get; set; }

        public bool? IsFreeDaysAtBegin { get; set; }

        public string Name { get; set; }

        public long Property_UID { get; set; }

        public long Rate_UID { get; set; }

        public long UID { get; set; }
    }
}