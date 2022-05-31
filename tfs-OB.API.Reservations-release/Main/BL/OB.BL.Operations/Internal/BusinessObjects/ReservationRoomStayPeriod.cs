using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ReservationRoomStayPeriod
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public long RateUID { get; set; }

        /// <summary>
        /// This property will be filled after validate promocode
        /// </summary>
        public readonly bool IsAssociatedToPromocode;

        public ReservationRoomStayPeriod(bool isAssociatedToPromocode = false)
        {
            IsAssociatedToPromocode = isAssociatedToPromocode;
        }
    }
}