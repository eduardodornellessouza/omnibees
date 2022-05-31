using System;

namespace OB.DL.Common.QueryResultObjects
{
    [Serializable]
    public partial class ReservationRoomExtraQR1
    {
        public long UID { get; set; }

        public long ReservationRoom_UID { get; set; }

        public long ExtraID { get; set; }

        public long ExtraCount { get; set; }

        public Nullable<Decimal> ExtraPrice { get; set; }

        public string ExtraPriceFormated { get; set; }

        public string ExtraName { get; set; }

        public string ExtraNameWithCount { get; set; }

        public Nullable<Int16> VAT { get; set; }

        public Decimal TotalVat { get; set; }

        public long ReservationRoomExtraUID { get; set; }

        //TMOREIRA: Entity Framework Objects should NEVER EVER be used in here. It can blow up with the Database and IIS worker process because
        //This class is serialized into DB/Session during the booking process.
        //public List<ReservationRoomExtrasSchedule> ReservationRoomExtrasSchedule { get; set; }

        public bool ExtraIncluded { get; set; }

        public string ExtraScheduleInfo { get; set; }

        public string ExtraAvailableDatesInfo { get; set; }

        public string ExtraAvailableDatesInfoForBE { get; set; }

        public string ExtraAddedOrIncuded { get; set; }

        public bool IsExtraScheduleVisible { get; set; }
    }
}