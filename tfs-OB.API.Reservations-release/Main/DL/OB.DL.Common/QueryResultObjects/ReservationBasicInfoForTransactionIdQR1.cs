using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// For result of QUERY_GetReservationBasicInfoForTransactionID in ReservationRepository
    /// </summary>
    [Serializable]
    public partial class ReservationBasicInfoForTransactionIdQR1
    {
        public long UID { get; set; }

        public long ReservationCurrency_UID { get; set; }

        public long Channel_UID { get; set; }

    }
}