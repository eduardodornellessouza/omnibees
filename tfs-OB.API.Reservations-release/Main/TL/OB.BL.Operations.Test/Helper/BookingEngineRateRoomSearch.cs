using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Helper
{
    public partial class BookingEngineRateRoomSearch
    {
        public long UID { get; set; }

        public string RateName { get; set; }

        public long Rate_UID { get; set; }

        public string RoomTypeName { get; set; }

        public long Property_UID { get; set; }

        public string PropertyName { get; set; }

        public long Property_ImageUID { get; set; }

        public string PropertyDescription { get; set; }

        public long RoomType_UID { get; set; }

        public long RateRoom_UID { get; set; }

        public long RateRoomDetail_UID { get; set; }

        public decimal AdultPrice { get; set; }

        public decimal ChildPrice { get; set; }

        public Nullable<Decimal> TotalTax { get; set; }

        public DateTime Date { get; set; }

        public long PromotionalCode_UID { get; set; }

        public bool IsExclusiveForPromoCode { get; set; }

        public int FreeDays { get; set; }

        public bool IsFreeDaysAtBeginning { get; set; }

        public long PackageId { get; set; }

        public long BindingId { get; set; }

        public long RoomNo { get; set; }

        public Nullable<int> Rateorder { get; set; }

        public Nullable<int> ClassinficationStars { get; set; }

        public string address { get; set; }

        public string phone { get; set; }

        public string fax { get; set; }

        public decimal OldAdultPrice { get; set; }

        public decimal OldChildPrice { get; set; }

        public bool IsDiscounted { get; set; }

        public bool AvailableOnRequest { get; set; }

        public bool PropertyAvailableOnRequest { get; set; }

        public List<BookingEngineRateRoomDetailsRateRestrictions> Restrictions { get; set; }

        public int AllotmentAvailable { get; set; }

        public long? RateCategory_UID { get; set; }

        public int MaxAdult { get; set; }

        public int MaxChild { get; set; }

        public long Incentive_UID { get; set; }

        // new fields for v2
        public int? MinimumLengthOfStay { get; set; }
        public int? StayThrough { get; set; }
        public bool? ClosedOnArrival { get; set; }
        public bool? ClosedOnDeparture { get; set; }
        public int? ReleaseDays { get; set; }
        public int? MaximumLengthOfStay { get; set; }
        public bool? isBookingEngineBlocked { get; set; }
        public bool? IsOccupancy { get; set; }

        public bool IsAvailableToTPI { get; set; }
        public bool AcceptsExtraBed { get; set; }
        public decimal ExtraBedPrice { get; set; }
        public List<ExtraCustomMeta> Extras { get; set; }
        public List<long> RoomTypeImage_UID { get; set; }

        public bool IsExclusiveForGroupCode { get; set; }

        public string GetGeneralServicesAmenities { get; set; }

        public decimal PriceForOrder { get; set; }

        public int OrderIndex { get; set; }

        public long GroupCode_UID { get; set; }

        // Cancelation
        public bool IsCancelationAllowed { get; set; }
        public int CancelationDays { get; set; }

        //New Incentives
        public List<ReservationRoomDetailsAppliedIncentive> AppliedIncentives { get; set; }
    }
}
