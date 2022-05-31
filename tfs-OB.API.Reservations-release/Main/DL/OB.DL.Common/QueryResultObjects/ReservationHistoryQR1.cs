using System;

namespace OB.DL.Common.QueryResultObjects
{
    public class ReservationHistoryQR1
    {
        public long UID { get; set; }

        public Nullable<long> ReservationUID { get; set; }

        public string ReservationNumber { get; set; }

        public string UserName { get; set; }

        public string Channel { get; set; }

        public Nullable<System.DateTime> ChangedDate { get; set; }

        public long? StatusUID { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }
    }
}