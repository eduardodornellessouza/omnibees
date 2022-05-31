using OB.Reservation.BL.Contracts.Validators;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [CustomValidation(typeof(CustomValidator), "ValidateProCodeDates")]
    [CustomValidation(typeof(CustomValidator), "ValidateProCodeDatesTo")]
    internal sealed class PromotionalCode
    {
        public PromotionalCode()
        {
        }

        public string Code { get; set; }

        public Nullable<decimal> DiscountValue { get; set; }

        public bool IsDeleted { get; set; }

        [CustomValidation(typeof(CustomValidator), "ValidatePromotionCodeProperty")]
        public bool IsPercentage { get; set; }

        public bool IsValid { get; set; }

        public Nullable<int> MaxReservations { get; set; }

        public string Name { get; set; }

        public long Property_UID { get; set; }

        public Nullable<int> ReservationsCompleted { get; set; }

        public long UID { get; set; }

        public Nullable<DateTime> ValidFrom { get; set; }

        public Nullable<DateTime> ValidTo { get; set; }

        public bool IsCommission { get; set; }

        public bool IsRegisterTPI { get; set; }

        [CustomValidation(typeof(CustomValidator), "ValidateProCodeURL")]
        //TODO:
        //[RegularExpression(@"^(((ht|f){1}(tp(s?)\:[][/]){1})|((www.){1}))[-a-zA-Z0-9@:%_\+.~#?&/=]+$", ErrorMessageResourceName = "lblAttractionValidURL", ErrorMessageResourceType = typeof(MetadataErrorMessages))]
        public string URL { get; set; }

        [CustomValidation(typeof(CustomValidator), "ValidateProCodeCommitionProgram")]
        public long PromotionalCode_UID { get; set; }

        //[Include]
        public ICollection<PromotionalCodesCurrency> PromotionalCodesCurrencies { get; set; }
    }
}