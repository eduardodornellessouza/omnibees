using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// For result of QUERY_GetReservationTransactionStatusBasicInfoForReservationUID in ReservationRepository
    /// </summary>
    
    [Serializable]
    public partial class ReservationTransactionStatusBasicInfoForReservationUidQR1
    {
        public string TransactionUID { get; set; }

        public int TransactionState { get; set; }

        public long HangfireID { get; set; }
    }
}