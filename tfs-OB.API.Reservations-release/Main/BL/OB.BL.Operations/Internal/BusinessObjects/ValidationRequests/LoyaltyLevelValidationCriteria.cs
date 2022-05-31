using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects.ValidationRequests
{
    public class LoyaltyLevelValidationCriteria
    {
        public LoyaltyLevelValidationCriteria()
        {
            RoomsToValidate = new List<LoyaltyRoomToValidate>();
        }

        public int? LoyaltyLevelLimitsPeriodicityValue { get; set; }
        public long? Guest_UID { get; set; }
        public bool IsForNumberOfReservationsActive { get; set; }
        public int? IsForNumberOfReservationsValue { get; set; }
        public bool IsForNightsRoomActive { get; set; }
        public int? IsForNightsRoomValue { get; set; }
        public bool IsForTotalReservationsActive { get; set; }
        public long? LoyaltyLevelBaseCurrency_UID { get; set; }
        public decimal? IsForTotalReservationsValue { get; set; }
        public long? LoyaltyLevel_UID { get; set; }
        public List<LoyaltyRoomToValidate> RoomsToValidate { get; set; }
        public decimal? IsForReservationValue { get; set; }
        public int? IsForReservationRoomNightsValue { get; set; }
        public bool IsForReservationActive { get; set; }
        public bool IsLimitsForPeriodicityActive { get; set; }
        public int? LoyaltyLevelLimitsPeriodicity_UID { get; set; }
        public long? PropertyId { get; set; }
        public long? PropertyCurrencyId { get; set; }
    }

    public class LoyaltyRoomToValidate
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalAmount { get; set; }
        public long LoyaltyLevel_UID { get; set; }
    }

    public class AggregatedGuestPastReservationsValues
    {
        public int ReservationsCount { get; set; }
        public int RoomNightsCount { get; set; }
        public decimal ReservationsTotalAmount { get; set; }
    }
}
