#region Namespace

//using OB.BL.Resources;

#endregion Namespace

namespace OB.Reservation.BL.Contracts.Validators
{
    public class CustomValidator
    {
        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 11 Oct 2010
        ///// Description :: This method is used in Amenity class to validate AmenityCategory_UID property.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateAmenityCategory_UID(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblAmenityCategorySelect, new string[] { "AmenityCategory_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 08 Nov 2010
        ///// Desc :: To check whether Country is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateCountry(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblGuestSelectCountry, new string[] { "Country_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 08 Nov 2010
        ///// Desc :: To check whether Currency is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateCurrency(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblGuestSelectCurrency, new string[] { "Currency_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 08 Nov 2010
        ///// Desc :: To check whether Guest Category is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateGuestCategory(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblGuestSelectCategory, new string[] { "GuestCategory_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 08 Nov 2010
        ///// Desc :: To check whether Langueage is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateLanguage(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblguestSelectLanguages, new string[] { "Language_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 12 Jan 2011
        ///// Desc :: To check whether Property is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidatePropertySelection(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(Resources.MetadataErrorMessages.lblRequiredProperty, new string[] { "Property_UID" });

        //    return (result);
        //}

        //public static ValidationResult ValidateRateTierSelection(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(Resources.MetadataErrorMessages.lblRequiredRateTier, new string[] { "RateTier_UID" });

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 08 Nov 2010
        ///// Desc :: To check whether pass value greater than zero or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateForDDL(long value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0)
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidateString(string value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (string.IsNullOrEmpty(value))
        //        result = new ValidationResult(CustomErrorMessage.lblRequired, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Tabrez Shaikh
        ///// Created Date :: 25 Nov 2010
        ///// Desc :: To check number must be greater then zero.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult IsValueGreaterThanZero(int value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult("");
        //    return (result);
        //}

        //public static ValidationResult IsValidRateRoomCustomPrice(RateRoomCustom rate)
        //{
        //    ValidationResult result = null;

        //    Nullable<decimal> Price = rate.Price;

        //    if ((Price == null) || (Price != null && Price.Value <= 0))
        //    {
        //        List<string> properties = new List<string>() { "Price" };
        //        result = new ValidationResult(CustomErrorMessage.lblRoomPriceGreterthenzero, properties);

        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 04 jan 2011
        ///// Desc :: To check whether PropertyTemplate is selected or not.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidatePropertyTemplate(long value, ValidationContext context)
        //{
        //    ValidationResult result = (value > 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblSurveyPropertyTemplate, new string[] { "PropertyTemplates_UID" });

        //    return (result);
        //}

        //public static ValidationResult IsValidRateRoomCustomAllotment(RateRoomCustom rate)
        //{
        //    ValidationResult result = null;

        //    Nullable<int> Allotment = rate.Allotment;

        //    if ((Allotment == null) || (Allotment != null && Allotment.Value <= 0))
        //    {
        //        List<string> properties = new List<string>() { "Allotment" };
        //        result = new ValidationResult(CustomErrorMessage.lblRoomAllomentGreterthenzero, properties);
        //    }

        //    return result;
        //}
        //public static ValidationResult IsValidRateChannelCustomValue(RatesChannelCustom rate)
        //{
        //    ValidationResult result = null;

        //    Nullable<decimal> Value = rate.Value;

        //    if (rate.Channel_UID != rate.BookingEngine_Channel_UID && ((Value == null) || (Value != null && Value.Value < 0)))
        //    {
        //        List<string> properties = new List<string>() { "Value" };
        //        result = new ValidationResult(CustomErrorMessage.lblChannelvalueGreaterthanzero, properties);
        //    }

        //    if (rate.IsPercentage != null && rate.IsPercentage.Value == true)
        //    {
        //        if (rate.Value != null && rate.Value > 100)
        //        {
        //            List<string> properties = new List<string>() { "Value" };
        //            result = new ValidationResult(CustomErrorMessage.lblChannelvalueLessthan100, properties);
        //        }
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidBaseRoomTypeValue(BaseRoomTypeCustom room)
        //{
        //    ValidationResult result = null;

        //    if (room.IsPercentage != null && room.IsPercentage.Value == true)
        //    {
        //        if (room.Value != null && room.Value > 100)
        //        {
        //            List<string> properties = new List<string>() { "Value" };
        //            result = new ValidationResult(CustomErrorMessage.lblRoomvalueLessthan100, properties);
        //        }
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidOccupancyFrom(RateLevel rate)
        //{
        //    ValidationResult result = null;

        //    int val = rate.OccupancyFrom;

        //    if (val < 0)
        //    {
        //        List<string> properties = new List<string>() { "OccupancyFrom" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelOccupancyFromGreaterthanzero, properties);
        //    }
        //    else if (val > 100)
        //    {
        //        List<string> properties = new List<string>() { "OccupancyFrom" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelOccupancyFromLessthan100, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidOccupancyTo(RateLevel rate)
        //{
        //    ValidationResult result = null;

        //    int val = rate.OccupancyTo;

        //    if (val <= 0)
        //    {
        //        List<string> properties = new List<string>() { "OccupancyTo" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelOccupancyToGreaterthanzero, properties);
        //    }
        //    else if (val > 100)
        //    {
        //        List<string> properties = new List<string>() { "OccupancyTo" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelOccupancyToLessthan100, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidRateLevelValue(RateLevelDetail rate)
        //{
        //    ValidationResult result = null;

        //    decimal val = rate.Value;
        //    bool IsPercentage = rate.IsPercentage;

        //    if (val < 0)
        //    {
        //        List<string> properties = new List<string>() { "Value" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueGreaterthanzero, properties);
        //    }
        //    else if (IsPercentage && val > 100)
        //    {
        //        List<string> properties = new List<string>() { "Value" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueLessthan100, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidRateLevelDetailValue(RateLevelDetailCustom rate)
        //{
        //    ValidationResult result = null;

        //    decimal val = rate.Value;
        //    bool IsPercentage = rate.IsPercentage;

        //    if (val < 0)
        //    {
        //        List<string> properties = new List<string>() { "Value" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueGreaterthanzero, properties);
        //    }
        //    else if (IsPercentage && val > 100)
        //    {
        //        List<string> properties = new List<string>() { "Value" };
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueLessthan100, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidDateFrom(RatesIncentiveCustom rate)
        //{
        //    ValidationResult result = null;

        //    DateTime? val = rate.DateFrom;

        //    if (val == null)
        //    {
        //        List<string> properties = new List<string>() { "DateFrom" };
        //        result = new ValidationResult(CustomErrorMessage.lblRatesIncentiveDateFrom, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult IsValidDateTo(RatesIncentiveCustom rate)
        //{
        //    ValidationResult result = null;

        //    DateTime? _dateTo = rate.DateTo;

        //    if (_dateTo == null)
        //    {
        //        List<string> properties = new List<string>() { "DateTo" };
        //        result = new ValidationResult(CustomErrorMessage.lblRatesIncentiveDateTo, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateReservationRoomQty(ReservationRoomTypeCustom objResRoom)
        //{
        //    ValidationResult result = null;

        //    int Qty = objResRoom.RoomQty;
        //    if (objResRoom.IsSelected && objResRoom.RoomQty <= 0)
        //    {
        //        List<string> properties = new List<string>() { "RoomQty" };
        //        result = new ValidationResult(CustomErrorMessage.lblReservationRoomQuantityGreaterthanzero, properties);
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateTaxPolicy(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    bool isPercentage = ((TaxPolicy)(context.ObjectInstance)).IsPercentage;
        //    if (value > 100 && isPercentage)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblTaxPolicyValueLessthan100, new string[] { "Value" });
        //    }
        //    else if (value < 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblTaxPolicyValidValue, new string[] { "Value" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateChildTerms(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    bool isPercentage = ((ChildTerm)(context.ObjectInstance)).IsPercentage.Value;
        //    if (value > 100 && isPercentage)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChildTermsValueLessthan100, new string[] { "Value" });
        //    }
        //    else if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChildTermsValidValue, new string[] { "Value" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateChildAge(int value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblReservationChildAge, new string[] { "Age" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateStatus(int value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblStatus, new string[] { "Status" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateReservationGuest(int value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblReservationGuest, new string[] { "Guest_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateChildAgeFrom(long value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChildTermsAgeFrom, new string[] { "AgeFrom" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateChildAgeTo(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    long AgeFrom = ((ChildTerm)(context.ObjectInstance)).AgeFrom;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChildTermsAgeTo, new string[] { "AgeTo" });
        //    }
        //    else if (value < AgeFrom)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChildTermsAgeToGreterthenAgeFrom, new string[] { "AgeTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateCancellationPolicy(int? value, ValidationContext context)
        //{
        //    ValidationResult result = (value >= 0) ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblCanecellationPolicyValidDay, new string[] { "Days" });

        //    return (result);
        //}

        //public static ValidationResult ValidateLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((DepositPoliciesLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((DepositPoliciesLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblDepositPolicyValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);

        //}

        //public static ValidationResult ValidateOtherPolicyLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((OtherPoliciesLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((OtherPoliciesLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblOtherPolicyValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);

        //}

        //public static ValidationResult ValidateGroupCodeLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((GroupCodeLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((GroupCodeLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblOtherPolicyValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateChildTermLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((ChildTermLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((ChildTermLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblOtherPolicyValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);

        //}

        //public static ValidationResult ValidateCancellationPolicyLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((CancellationPoliciesLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((CancellationPoliciesLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblCancellationPolicyValidateLanguageName, new string[1] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);

        //}

        //public static ValidationResult ValidateTaxPolicyLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((TaxPoliciesLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((TaxPoliciesLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblTaxPolicyValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateExtraLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((ExtrasLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((ExtrasLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblExtraServiceValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateAttractionLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((AttractionLanguage)(context.ObjectInstance)).Name;
        //    string Desciption = ((AttractionLanguage)(context.ObjectInstance)).Description;

        //    if (string.IsNullOrEmpty(Name) && !(string.IsNullOrEmpty(Desciption)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblAttractionValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateRoomTypeLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((RoomTypeLanguage)(context.ObjectInstance)).Name;
        //    string ShortDescription = ((RoomTypeLanguage)(context.ObjectInstance)).ShortDescription;
        //    string Description = ((RoomTypeLanguage)(context.ObjectInstance)).Description;

        //    if ((!string.IsNullOrEmpty(Description) || !string.IsNullOrEmpty(ShortDescription)) && string.IsNullOrEmpty(Name))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRoomTypeValidateLanguageName, new string[] { "Name" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateRateLanguages(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    string Name = ((RateLanguage)(context.ObjectInstance)).RateName;
        //    string Description = ((RateLanguage)(context.ObjectInstance)).RateDescription;

        //    if (!string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Name))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRateValidateLangName, new string[] { "RateName" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidatePasswordExpiryDate(DateTime? value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    //DateTime? PasswordExpiryDate = ((User)(context.ObjectInstance)).PasswordExpiryDate;

        //    if (value != null && value < DateTime.Today)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblUserPasswordExpirydate, new string[] { "PasswordExpiryDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ////*//
        //public static ValidationResult ValidateClientComments(Boolean value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    string Comments = ((Client)(context.ObjectInstance)).Comments;

        //    if (value && string.IsNullOrEmpty(Comments))
        //    {
        //        result = new ValidationResult("Please Enter Comments.");
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateChannelProperty(object value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    bool isPercentage = ((ChannelsPropertyCustom)(context.ObjectInstance)).IsPercentage;
        //    Decimal Value = ((ChannelsPropertyCustom)(context.ObjectInstance)).Value;

        //    if (Value > 100 && isPercentage)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChannelServiceValueLassthan100, new string[] { "Value" });
        //    }
        //    else if (Value < 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblChannelServiceValidateValue, new string[] { "Value" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateRateTierDateRange(DateTime value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    DateTime dtFrom = ((RateTiersDateRange)(context.ObjectInstance)).DateFrom;
        //    DateTime dtTo = ((RateTiersDateRange)(context.ObjectInstance)).DateTo;
        //    if (dtTo < dtFrom)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRateTiersDateRange, new string[] { "DateTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateAttCatValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblAttractionCategory, new string[] { "AttractionCategory_UID" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 14 Dec 2010
        ///// Desc :: To check Rate value for Package
        ///// </summary>
        //public static ValidationResult ValidatePackageRateValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageRateValidate, new string[] { "Rate_UID" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidatePropertyCatValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPropertyCategory, new string[] { "PropertyCategory_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        //public static ValidationResult ValidateCurrencyDropDown(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblCurrencySelect, new string[] { "BaseCurrency_UID" });

        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        //public static ValidationResult ValidatePropertyAccManValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblAccountManagerValidateProperty, new string[] { "AccountManager_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        //public static ValidationResult ValidatePropertyClientValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPropertyValidateClient, new string[] { "Client_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        //public static ValidationResult ValidateProCodeDates(PromotionalCode objPromotionalCode, ValidationContext context)
        //{
        //    ValidationResult result;
        //    var dtFrom = objPromotionalCode.ValidFrom;
        //    var dtTo = objPromotionalCode.ValidTo;
        //    var Code = objPromotionalCode.Code;

        //    if (!string.IsNullOrEmpty(Code) && dtTo == null)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidTo, new string[] { "ValidTo" });
        //    }

        //    if (!string.IsNullOrEmpty(Code) && dtFrom == null)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidFrom, new string[] { "ValidFrom" });
        //    }
        //    else if (((dtFrom != null && dtTo == null) || (dtFrom == null && dtTo != null) || dtFrom > dtTo))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPromotionCodesValidateDateValidFrom, new string[] { "ValidFrom" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateProCodeDatesTo(PromotionalCode objPromotionalCode, ValidationContext context)
        //{
        //    ValidationResult result;
        //    var dtFrom = objPromotionalCode.ValidFrom;
        //    var dtTo = objPromotionalCode.ValidTo;
        //    var Code = objPromotionalCode.Code;

        //    if (!string.IsNullOrEmpty(Code) && dtTo == null)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidTo, new string[] { "ValidTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateProCodeURL(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    var URL = ((PromotionalCode)(context.ObjectInstance)).URL;
        //    bool IsRegisterTPI = (bool)((PromotionalCode)(context.ObjectInstance)).IsRegisterTPI;

        //    if (IsRegisterTPI && string.IsNullOrEmpty(URL))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidTo, new string[] { "URL" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateProCodeCommitionProgram(string value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    long? Promotioncode = ((PromotionalCode)(context.ObjectInstance)).UID;
        //    bool IsRegisterTPI = (bool)((PromotionalCode)(context.ObjectInstance)).IsRegisterTPI;

        //    if (IsRegisterTPI && (Promotioncode == null || Promotioncode <= 0))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { "PromotionalCode_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateActivityCategoryValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblActivityCategory, new string[] { "ActivityCategory_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        //public static ValidationResult ValidateCampaignProperty(bool? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    decimal? discount = ((Campaign)(context.ObjectInstance)).DiscountValue;
        //    bool Ispersentage = (bool)value;

        //    if (discount > 100 && Ispersentage)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueLessthan100, new string[] { "DiscountValue" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidatePromotionCodeProperty(bool value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    decimal? discount = ((PromotionalCode)(context.ObjectInstance)).DiscountValue;
        //    bool Ispersentage = value;

        //    if (discount > 99 && Ispersentage)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRateLevelValueLessthan100, new string[] { "DiscountValue" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //#region InvoiceSettings
        //public static ValidationResult validateRecurrence(long value, ValidationContext context)
        //{
        //    ValidationResult result = null;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingValidateRecurrence, new string[] { "Recurrence" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateVAT(long value, ValidationContext context)
        //{
        //    ValidationResult result = null;

        //    if (value > 100)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingVatLassthan100, new string[] { "VAT1" });
        //    }
        //    else if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingValidateVat, new string[] { "VAT1" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateInvoiceSettingVAT(long value, ValidationContext context)
        //{
        //    ValidationResult result = null;

        //    if (value == null || (value != null && value <= 0))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateVAT, new string[] { "VAT_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return result;
        //}

        ////public static ValidationResult InvoicesettingDetailValue(bool value, ValidationContext context)
        ////{
        ////    ValidationResult result = null;
        ////    bool isPercentage = ((InvoiceSettingsDetail)(context.ObjectInstance)).IsPercentage;
        ////    decimal val = ((InvoiceSettingsDetail)(context.ObjectInstance)).Value;

        ////    if (val > 100 && isPercentage)
        ////    {
        ////        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingDetailValueLessthan100, new string[] { "Value" });
        ////    }
        ////    else if (val <= 0)
        ////    {
        ////        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingDetailValidateValue, new string[] { "Value" });
        ////    }
        ////    else
        ////    {
        ////        result = ValidationResult.Success;
        ////    }

        ////    return result;
        ////}

        //public static ValidationResult ValidateProperty(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblInvoiceSettingValidateProperty, new string[] { "Property_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        ////public static ValidationResult ValidateInvoiceValue(object value, ValidationContext context)
        ////{
        ////    ValidationResult result = null;
        ////    if (((InvoicesDetail)value).Value <= 0)
        ////    {
        ////        List<string> properties = new List<string>() { "Value" };
        ////        result = new ValidationResult(CustomErrorMessage.lblInvoiceValidateValue, properties);
        ////    }

        ////    return result;
        ////}

        ////public static ValidationResult ValidateInvoiceQuantity(object value, ValidationContext context)
        ////{
        ////    ValidationResult result = null;
        ////    if (((InvoicesDetail)value).Quantity <= 0)
        ////    {
        ////        List<string> properties = new List<string>() { "Quantity" };
        ////        result = new ValidationResult(CustomErrorMessage.lblInvoiceValidateQuantity, properties);
        ////    }

        ////    return result;
        ////}

        //#endregion
        //public static ValidationResult ValidateStartEndDate(Campaign value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((Campaign)(context.ObjectInstance)).StartDate;
        //    DateTime? EndDate = ((Campaign)(context.ObjectInstance)).EndDate;

        //    if (EndDate < StartDate)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblCampaignValidateEndDate, new string[] { "EndDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateResSummaryFromToDate(DateTime value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((DashboardReservationCustom)(context.ObjectInstance)).FromDate;
        //    DateTime? EndDate = ((DashboardReservationCustom)(context.ObjectInstance)).ToDate;

        //    if (EndDate < StartDate)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageValidateStartEndDate, new string[] { "ToDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ////TMOREIRA
        ////public static ValidationResult ValidateSurveyStartEndDate(DateTime? value, ValidationContext context)
        ////{
        ////    ValidationResult result;

        ////    DateTime? StartDate = ((Survey)(context.ObjectInstance)).StartDate;
        ////    DateTime? EndDate = ((Survey)(context.ObjectInstance)).EndDate;

        ////    if (EndDate < StartDate)
        ////    {
        ////        result = new ValidationResult(CustomErrorMessage.lblCampaignValidateEndDate, new string[] { "EndDate" });
        ////    }
        ////    else
        ////    {
        ////        result = ValidationResult.Success;
        ////    }
        ////    return (result);
        ////}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 14 Dec 2010
        ///// Desc :: To Check From and TO date for Package
        ///// </summary>
        //public static ValidationResult ValidatePackageStartEndDate(Package objPackage, ValidationContext context)
        //{
        //    ValidationResult result;

        //    //DateTime? StartDate = ((Package)(context.ObjectInstance)).DateFrom;
        //    //DateTime? EndDate = ((Package)(context.ObjectInstance)).DateTo;

        //    if (objPackage.DateFrom.HasValue && objPackage.DateTo.HasValue && objPackage.DateTo.Value < objPackage.DateFrom.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageValidateStartEndDate, new string[] { "DateTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 04 feb 2011
        ///// Desc :: To Check Begin End date must be greater then Begin Start date in package
        ///// </summary>
        //public static ValidationResult ValidatePackageSaleBeginEndDate(Package objPackage, ValidationContext context)
        //{
        //    ValidationResult result;

        //    //DateTime? StartDate = ((Package)(context.ObjectInstance)).BeginSale;
        //    //DateTime? EndDate = ((Package)(context.ObjectInstance)).EndSale;

        //    if (objPackage.EndSale.HasValue && objPackage.BeginSale.HasValue && objPackage.EndSale.Value < objPackage.BeginSale.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblBeginEndSaleDate, new string[] { "EndSale" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ////TMOREIRA
        /////// <summary>
        /////// Created By :: Ronak Shah
        /////// Created Date :: 04 feb 2011
        /////// Desc :: To Check Begin End date must be greater then Begin Start date in Rate
        /////// </summary>
        ////public static ValidationResult ValidateRateSaleBeginEndDate(DateTime? value, ValidationContext context)
        ////{
        ////    ValidationResult result;

        ////    DateTime? StartDate = ((Rate)(context.ObjectInstance)).BeginSale;
        ////    DateTime? EndDate = ((Rate)(context.ObjectInstance)).EndSale;

        ////    if (EndDate.HasValue && StartDate.HasValue && EndDate.Value < StartDate.Value)
        ////    {
        ////        result = new ValidationResult(CustomErrorMessage.lblBeginEndSaleDate, new string[] { "EndSale" });
        ////    }
        ////    else
        ////    {
        ////        result = ValidationResult.Success;
        ////    }
        ////    return (result);
        ////}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 13 Dec 2010
        ///// Desc :: To check SalesDocumentType is Selected or mot
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateSalesDocumentSeriesValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblSalesDocumentSeriesValidateDocumentType, new string[] { "SalesDocumentType_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 13 Dec 2010
        ///// Desc :: To check SystemTemplatesCategory
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateSystemTemplatesCategoryValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblSystemTemplateValidateCategory, new string[] { "SystemTemplatesCategorie_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);

        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 13 Dec 2010
        ///// Desc :: To check StartDate and EndDate
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateSalesDocumentStartEndDate(DateTime? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((SalesDocumentsSery)(context.ObjectInstance)).StartDate;
        //    DateTime? EndDate = ((SalesDocumentsSery)(context.ObjectInstance)).EndDate;

        //    if (EndDate < StartDate)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblSalesDocumentSeriesValidateStartEndDate, new string[] { "EndDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 07 Dec 2010
        ///// Desc :: To check StartDate is valid or not
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateDateGreaterThanCurrent(DateTime? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value != null && value < System.DateTime.Today)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblVatValidateDate, new string[] { context.DisplayName });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 18 Dec 2010
        ///// Desc :: To check MaxValue and MinValue
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateRoomTypeMaxvalue(Decimal? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    Decimal? MaxValue = ((RoomType)(context.ObjectInstance)).MaxValue;
        //    Decimal? MinValue = ((RoomType)(context.ObjectInstance)).MinValue;

        //    if (MaxValue != null && MinValue != null && MaxValue < MinValue)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRoomTypeValidateMaxValue, new string[] { "MaxValue" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 18 Dec 2010
        ///// Desc :: To check MaxValue and MinValue
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateRoomTypeAdultMaxOccupancy(Int32? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    Int32? MaxOccupancy = ((RoomType)(context.ObjectInstance)).AdultMaxOccupancy;
        //    Int32? MinOccupancy = ((RoomType)(context.ObjectInstance)).AdultMinOccupancy;

        //    if (MaxOccupancy != null && MinOccupancy != null && MaxOccupancy < MinOccupancy)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRoomTypeValidateMaxOccupancy, new string[] { "AdultMaxOccupancy" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 18 Dec 2010
        ///// Desc :: To check MaxValue and MinValue
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        /////
        //public static ValidationResult ValidateRoomTypeChildMaxOccupancy(RoomType objRoomType, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (objRoomType.AcceptsChildren.HasValue && objRoomType.AcceptsChildren.Value && (!objRoomType.ChildMaxOccupancy.HasValue || (objRoomType.ChildMaxOccupancy.HasValue && objRoomType.ChildMaxOccupancy.Value < 0)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRoomTypeValidateMaxOccupancy, new string[] { "ChildMaxOccupancy" });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 15 Dec 2010
        ///// Desc :: To check any date for Past Date.
        //public static ValidationResult ValidatePastDate(DateTime? date, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (date.HasValue && date.Value < DateTime.Today.Date)
        //        result = new ValidationResult(CustomErrorMessage.lblPackagServiceValidateDate, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Pathak Gaurang A
        ///// Created Date :: 22 May 2012
        //public static ValidationResult ValidateValue(decimal? Value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    bool IsDerivedRate = ((Rate)(context.ObjectInstance)).Rate_UID > 0;

        //    if (IsDerivedRate && (Value == null || (Value.HasValue && Value <= 0)))
        //        result = new ValidationResult(CustomErrorMessage.lblValidateValue, new string[] { "Value" });
        //    else
        //        result = ValidationResult.Success;

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Hitesh Patel
        ///// Created Date :: 01 Dec 2011
        ///// Desc :: To check any date for Past Date of package.
        //public static ValidationResult ValidatePastDatePackage(Package packg, ValidationContext context)
        //{
        //    ValidationResult result;

        //    //if (date.HasValue && date.Value < DateTime.Today.Date)
        //    //    result = new ValidationResult(CustomErrorMessage.lblPackagServiceValidateDate, new string[] { context.DisplayName });
        //    //else
        //    //    result = ValidationResult.Success;

        //    if (packg.UID == 0 && packg.DateFrom.HasValue && packg.DateFrom.Value < DateTime.Today.Date)
        //        result = new ValidationResult(CustomErrorMessage.lblPackagServiceValidateDate, new string[] { "DateFrom" });
        //    else if (packg.DateTo.HasValue && packg.DateTo.Value < DateTime.Today.Date)
        //        result = new ValidationResult(CustomErrorMessage.lblPackagServiceValidateDate, new string[] { "DateTo" });
        //    else if (packg.EndSale.HasValue && packg.EndSale.Value < DateTime.Today.Date)
        //        result = new ValidationResult(CustomErrorMessage.lblPackagServiceValidateDate, new string[] { "EndSale" });
        //    else
        //        result = ValidationResult.Success;

        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 17 Dec 2010
        ///// Desc :: To check any date for Past Date.
        //public static ValidationResult ValidateEventConditionValue(EventConditionCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.Value != null)
        //    {
        //        if (obj.UID == (int)Constants.SystemEvents.TweetWithKeyword)
        //            result = ValidationResult.Success;
        //        else
        //        {
        //            try
        //            {
        //                int value = Convert.ToInt32(obj.Value);
        //                result = ValidationResult.Success;
        //            }
        //            catch (Exception)
        //            {
        //                List<string> properties = new List<string>() { "Value" };
        //                result = new ValidationResult(CustomErrorMessage.lblSystemDefultEventValidateValue, properties);
        //            }
        //        }
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 15 Dec 2010
        ///// Desc :: To check dropdown default value.
        //public static ValidationResult ValidateDropDown(long? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (!value.HasValue || (value.HasValue && value <= 0))
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidateTPIPromotioncodeDropDown(long? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (value <= 0 || value == null)
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidatePropertyEventDropDown(long? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    long Action = ((PropertyEventCustom)(context.ObjectInstance)).Action_UID;

        //    if (Action == (Int64)Constants.SystemAction.SendMail && value <= 0)
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidateEventCondition(int? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    string SystemEvent_Coed = ((PropertyEventCustom)(context.ObjectInstance)).SystemEvent_Code;

        //    if (SystemEvent_Coed == ((Int32)Constants.SystemEventsCode.Availability).ToString() && value <= 0)
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ////public static ValidationResult ValidateEventConditionValue(long value, ValidationContext context)
        ////{
        ////    ValidationResult result;

        ////    string SystemEvent_Coed = ((PropertyEventCustom)(context.ObjectInstance)).SystemEvent_Code;

        ////    if (Convert.ToInt32(SystemEvent_Coed) == (Int32)Constants.SystemEventsCode.Availability && value == null)
        ////        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        ////    else
        ////        result = ValidationResult.Success;
        ////    return (result);
        ////}

        //public static ValidationResult ValidateEventValue(string value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    string SystemEvent_Code = ((PropertyEventCustom)(context.ObjectInstance)).SystemEvent_Code;

        //   // if ((SystemEvent_Code == ((Int32)Constants.SystemEventsCode.PreStay).ToString() || Convert.ToInt32(SystemEvent_Code) == (Int32)Constants.SystemEventsCode.PostStay) && (value == null || (value != null && Convert.ToInt32(value) <= 0)))
        //    if ((SystemEvent_Code == ((Int32)Constants.SystemEventsCode.PreStay).ToString() &&  (value == null || (value != null && Convert.ToInt32(value) <= 0)) || Convert.ToInt32(SystemEvent_Code) == (Int32)Constants.SystemEventsCode.PostStay) && (value == null || (value != null && Convert.ToInt32(value) < 0)))
        //        result = new ValidationResult(Resources.MetadataErrorMessages.lblInvalidDays, new string[] { context.DisplayName });
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created by :: Tabrez Shaikh
        ///// Created Date :: 30 Dec 2010
        ///// Modify by :: Pathak Gaurang A
        ///// Modify Date :: 08 Aug 2010
        ///// Desc :: To check email address entered or not, if system action selected as Send Mail(1).
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidatePropertyEventEmail(string value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    long Action = ((PropertyEventCustom)(context.ObjectInstance)).Action_UID;
        //    bool IsOther = ((PropertyEventCustom)(context.ObjectInstance)).IsOther;

        //    if (Action != null && Action == (Int64)Helper.Constants.SystemAction.SendMail && IsOther)
        //    {
        //        if (value == null || (value != null && value.Length == 0))
        //            result = new ValidationResult(CustomErrorMessage.lblValidateEmailaddress, new string[] { context.DisplayName });
        //        else
        //            result = ValidationResult.Success;
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidatePropertyEventMessages(string value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    long Action = ((PropertyEventCustom)(context.ObjectInstance)).Action_UID;

        //    if (Action != null && (Action == (Int64)Helper.Constants.SystemAction.SendFaceBook || Action == (Int64)Helper.Constants.SystemAction.SendTweet))
        //    {
        //        if (value == null || (value != null && value.Length == 0))
        //            result = new ValidationResult(CustomErrorMessage.lblValidateMessages, new string[] { context.DisplayName });
        //        else
        //            result = ValidationResult.Success;
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidatePropertyEventSubject(string value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    long Action = ((PropertyEventCustom)(context.ObjectInstance)).Action_UID;

        //    if (Action != null && (Action == (Int64)Helper.Constants.SystemAction.SendMail))
        //    {
        //        if (value == null || (value != null && value.Length == 0))
        //            result = new ValidationResult(CustomErrorMessage.lblValidateSubject, new string[] { context.DisplayName });
        //        else
        //            result = ValidationResult.Success;
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created by :: Ronak Shah
        ///// Created Date :: 05 Jan 2011
        ///// Desc :: To check whether new password & confirm password match or not.
        ///// </summary>
        //public static ValidationResult ValidateUserPassword(UserPasswordCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.ConfirmPassword != null)
        //    {
        //        if (obj.NewPassword == obj.ConfirmPassword)
        //            result = ValidationResult.Success;
        //        else
        //            result = new ValidationResult(CustomErrorMessage.lblPswdNotMatch, new string[] { "ConfirmPassword" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        //public static ValidationResult ValidateConfirmpassword(string Values, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    string Password = ((ConfirmpPsswodCustom)(context.ObjectInstance)).UserPassword;
        //    string ConfirmPassword = ((ConfirmpPsswodCustom)(context.ObjectInstance)).Confirmpassword;

        //    if (Password != null && ConfirmPassword != null && Password != ConfirmPassword)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidatePasswordandRepeatPassword, new string[] { "Confirmpassword" });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 10 Jan 2011
        ///// Desc :: To check Sales document detail value for Credit document detail
        ///// </summary>
        //public static ValidationResult ValidateSalesDocDetailValue(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblSalesDocDetailValidate, new string[] { "SalesDocumentDetails_UID" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 10 Jan 2011
        ///// Desc :: To check Sales document detail value for greater than zero
        ///// </summary>
        //public static ValidationResult ValidateValueGreaterZero(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValueGreaterZero, new string[] { "Value" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        //public static ValidationResult ValidateSalesDocumentValueGreaterZero(decimal value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValueGreaterZero, new string[] { "Value" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Pathak Gaurang A
        ///// Created Date :: 23 August 2011
        ///// Desc :: To check SalesDocumentDatail value
        ///// </summary>
        //public static ValidationResult ValidateSalesDocumentDetailValue(decimal value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    decimal Actualvalue = ((SalesDocumentsDetailCustom)context.ObjectInstance).ActualValue;

        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValueGreaterZero, new string[] { "Value" });
        //    }
        //    else if (value > Actualvalue)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValueExceed, new string[] { "Value" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 10 Jan 2011
        ///// Desc :: To check Property value for sales document
        ///// </summary>
        //public static ValidationResult ValidatePropertyForSalesDoc(long value, ValidationContext context)
        //{
        //    ValidationResult result;
        //    if (value <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblActivityCategory, new string[] { "Property_UID" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 14 Dec 2010
        ///// Desc :: To Check From and TO date for Package
        ///// </summary>
        //public static ValidationResult ValidateCalendarEventDates(DateTime value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((CalenderEvent)(context.ObjectInstance)).StartDate;
        //    DateTime? EndDate = ((CalenderEvent)(context.ObjectInstance)).EndDate;

        //    if (EndDate < StartDate)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblCampaignValidateEndDate, new string[] { "EndDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 17 Feb 2011
        ///// Desc :: To Check From and TO date for Saft pt
        ///// </summary>
        //public static ValidationResult ValidateSaftPtDates(DateTime value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((SaftPtDocDates)(context.ObjectInstance)).StartDate;
        //    DateTime? EndDate = ((SaftPtDocDates)(context.ObjectInstance)).EndDate;

        //    if (EndDate < StartDate)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblCampaignValidateEndDate, new string[] { "EndDate" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 14 Dec 2010
        ///// Desc :: To check atleast one day should be selected for Bulk update
        ///// </summary>
        //public static ValidationResult ValidateDatesForBulkUpdate(RateTierDateRangeCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.DateTo < obj.DateFrom)
        //        result = new ValidationResult(CustomErrorMessage.lblRateTiersDateRange, new string[] { "DateTo" });
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 14 Dec 2010
        ///// Desc :: To check atleast one day should be selected for Bulk update
        ///// </summary>
        //public static ValidationResult ValidateWeekDaysForBulkUpdate(RateRoomCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (!obj.IsFriday && !obj.IsMonday && !obj.IsSaturday && !obj.IsSunday && !obj.IsThursday && !obj.IsTuesday && !obj.IsWednesday)
        //        result = new ValidationResult(CustomErrorMessage.lblSelectOneDay, new string[] { "IsSunday" });
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 18 Mar 2011
        ///// Desc :: To check atleast one day should be selected for Bulk update
        ///// </summary>
        //public static ValidationResult ValidateStayThroughForRateRest(RatesRateRestrictionCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (((obj.MinDays.HasValue && obj.MinDays.Value > 0) || (obj.MaxDays.HasValue && obj.MaxDays.Value > 0)) && (obj.StayThrough.HasValue && obj.StayThrough.Value > 0))
        //        result = new ValidationResult(CustomErrorMessage.lblStayThroughMsg, new string[] { "StayThrough" });
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Gaurang Pathak
        ///// Created Date :: 15 Nov 2011
        ///// </summary>
        //public static ValidationResult ValidateRestRestricationMinMax(RatesRateRestrictionCustom obj, ValidationContext context)
        //{
        //    ValidationResult result;
        //    int Minday = obj.MinDays.HasValue ? (int)obj.MinDays : 0;
        //    int Maxday = obj.MaxDays.HasValue ? (int)obj.MaxDays : 0;

        //    if (obj.MinDays.HasValue && obj.MaxDays.HasValue && Minday > Maxday)
        //        result = new ValidationResult(CustomErrorMessage.lblValidateRateRestriction, new string[] { "MaxDays" });
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 06 Apr 2011
        ///// Desc :: To Check From and TO date for Group Code
        ///// </summary>
        //public static ValidationResult ValidateGroupCodeStartEndDate(DateTime value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((GroupCode)(context.ObjectInstance)).DateFrom;
        //    DateTime? EndDate = ((GroupCode)(context.ObjectInstance)).DateTo;

        //    if (EndDate.HasValue && StartDate.HasValue && EndDate.Value < StartDate.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblGroupCodeValidateDateTo, new string[] { "DateTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Ronak Shah
        ///// Created Date :: 06 Apr 2011
        ///// Desc :: To Check Begin and End Sell date for Group Code
        ///// </summary>
        //public static ValidationResult ValidateGroupCodeBeginEndSaleDate(DateTime? value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    DateTime? StartDate = ((GroupCode)(context.ObjectInstance)).BeginSell;
        //    DateTime? EndDate = ((GroupCode)(context.ObjectInstance)).EndSell;

        //    if (EndDate.HasValue && StartDate.HasValue && EndDate.Value < StartDate.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblGroupCodeValidateEndSell, new string[] { "EndSell" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created by :: Ronak Shah
        ///// Created Date :: 05 Jan 2011
        ///// Desc :: To check whether new password & confirm password match or not.
        ///// </summary>
        //public static ValidationResult ValidateGroupCodeRate(GroupCode obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.Rate_UID == null || obj.Rate_UID <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateGroupCodeRate, new string[] { "Rate_UID" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created by :: Ronak Shah
        ///// Created Date :: 18 Apr 2011
        ///// Desc :: To check credit card number length according to card type.
        ///// </summary>
        //public static ValidationResult ValidateResPaymentCCNumber(ReservationPaymentDetail obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.PaymentMethod_UID == (long)Constants.CCTypes.AmericanExpress && obj.CardNumber.Length != 15)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblResCardNumberLength, new string[] { "CardNumber" });
        //    }
        //    else if (obj.CardNumber.Length != 16)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblResCardNumberLength, new string[] { "CardNumber" });
        //    }
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        ///// Created by :: Ronak Shah
        ///// Created Date :: 28 Apr 2011
        ///// Desc :: To check Max Days & Extended Rate validation for package
        ///// </summary>
        //public static ValidationResult ValidatePackageObject(Package obj, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (obj.IsPackageExtensible && (!(obj.MaxDays.HasValue) || (obj.MaxDays.HasValue && obj.MaxDays.Value <= 0)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageMaxDays, new string[] { "MaxDays" });
        //    }
        //    else if (obj.IsStayExtensible && (!(obj.RateForExtendedStay_UID.HasValue) || (obj.RateForExtendedStay_UID.HasValue && obj.RateForExtendedStay_UID.Value <= 0)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageExtendedRate, new string[] { "RateForExtendedStay_UID" });
        //    }
        //    //else if (obj.MaxDays.HasValue && obj.MaxDays.Value > obj.Days)
        //    //{
        //    //    result = new ValidationResult(CustomErrorMessage.lblPackageDays, new string[] { "MaxDays" });
        //    //}
        //    else
        //        result = ValidationResult.Success;
        //    return result;
        //}

        ///// <summary>
        /////  Validate RoomType Quantity
        ///// </summary>
        ///// <param name="values"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateRoomQty(long values, ValidationContext context)
        //{
        //    return (values > 0 ? ValidationResult.Success : new ValidationResult(CustomErrorMessage.lblValidateRoomTypeQty, new string[] { "Qty" }));
        //}

        //public static ValidationResult ValidateIncentivediscount(long value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (((Incentive)(context.ObjectInstance)).IncentiveType_UID != (long)ProturRIAServices.Web.Helper.Constants.IncentiveType.FreeNights)
        //    {
        //        //decimal? discount = ((Incentive)(context.ObjectInstance)).DiscountPercentage;
        //        if (value > 99)
        //        {
        //            result = new ValidationResult(CustomErrorMessage.lblValidateDiscountLessthan100, new string[] { "DiscountPercentage" });
        //        }
        //        else if (value <= 0)
        //        {
        //            result = new ValidationResult(CustomErrorMessage.lblValidateDiscountGreaterthanZero, new string[] { "DiscountPercentage" });
        //        }
        //        else
        //        {
        //            result = ValidationResult.Success;
        //        }
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        //public static ValidationResult ValidateIncentiveFreeDays(Nullable<int> value, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (((Incentive)(context.ObjectInstance)) != null && ((Incentive)(context.ObjectInstance)).IncentiveType_UID == (long)ProturRIAServices.Web.Helper.Constants.IncentiveType.FreeNights && ((Incentive)(context.ObjectInstance)).FreeDays.HasValue)
        //    {
        //        if (((Incentive)(context.ObjectInstance)).FreeDays > ((Incentive)(context.ObjectInstance)).Days)
        //        {
        //            result = new ValidationResult(CustomErrorMessage.lblValidateIncentiveFreeDays, new string[] { context.DisplayName });
        //        }
        //        else
        //        {
        //            result = ValidationResult.Success;
        //        }
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }

        //    return (result);
        //}

        ///// <summary>
        /////  Validate Property CheckIn-CheckOut Houres
        ///// </summary>
        ///// <param name="Value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateCheckInHoures(DateTime? Value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    DateTime? CheckInHours = ((Property)(context.ObjectInstance)).CheckinHour;
        //    DateTime? CheckOutHours = ((Property)(context.ObjectInstance)).CheckoutHour;

        //    //DateTime? BreakfastTo = ((Property)(context.ObjectInstance)).BreakfeastHours;

        //    if (CheckInHours != null && CheckOutHours != null && CheckOutHours.Value.TimeOfDay < CheckInHours.Value.TimeOfDay)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateCheckInHours, new string[] { "CheckoutHour" });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Validate Proeprty Reception Open-Close Houres
        ///// </summary>
        ///// <param name="Value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateReceptionHoures(DateTime? Value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    DateTime? Rec_OpenHoures = ((Property)(context.ObjectInstance)).ReceptionOpenHours;
        //    DateTime? Rec_CloseHoures = ((Property)(context.ObjectInstance)).ReceptionClosedHours;

        //    if (Rec_OpenHoures != null && Rec_CloseHoures != null && Rec_CloseHoures.Value.TimeOfDay < Rec_OpenHoures.Value.TimeOfDay)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateReceptionHours, new string[] { "ReceptionClosedHours" });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Validate Property Closed From-To Date
        ///// </summary>
        ///// <param name="Value"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static ValidationResult ValidateCloseDate(object Value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (Value != null && ((Property)(Value)).CloseFrom.HasValue == true && ((Property)(Value)).CloseTo.HasValue == true && ((Property)(Value)).CloseTo.Value < ((Property)(Value)).CloseFrom.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateCloseDate, new string[] { "CloseTo" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateBreakfeastHoures(DateTime? Value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    DateTime? BreakfeastFrom = ((Property)(context.ObjectInstance)).BreakfeastHours;
        //    DateTime? BreakfeastTo = ((Property)(context.ObjectInstance)).BreakfeastTo;

        //    if (BreakfeastTo != null && BreakfeastFrom != null && BreakfeastTo.Value.TimeOfDay < BreakfeastFrom.Value.TimeOfDay)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateBreakFeastHours, new string[] { "BreakfeastTo" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateChannelUserName(object Values, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;
        //    string UserName = ((ChannelsPropertyCustom)(context.ObjectInstance)).UserName;
        //    string UserPassword = ((ChannelsPropertyCustom)(context.ObjectInstance)).UserPassword;

        //    if ((string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserPassword)))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateUserName, new string[] { "UserName" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateChannelUserPassword(object Values, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;
        //    string UserName = ((ChannelsPropertyCustom)(context.ObjectInstance)).UserName;
        //    string UserPassword = ((ChannelsPropertyCustom)(context.ObjectInstance)).UserPassword;

        //    if ((!string.IsNullOrEmpty(UserName) && (string.IsNullOrEmpty(UserPassword))))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateUserPassword, new string[] { "UserPassword" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateUseroldPassword(string Values, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    string UseroldPassword = ((UserchangePassword)(context.ObjectInstance)).UseroldPassword;
        //    string UserPassword = ((UserchangePassword)(context.ObjectInstance)).UserPassword;

        //    if (UseroldPassword != null && UserPassword != null && UseroldPassword != UserPassword)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateOldPassword, new string[] { "UserPassword" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateUserConfirmPassword(string Values, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    string NewPassword = ((UserchangePassword)(context.ObjectInstance)).NewPassword;
        //    string ConfirmPassword = ((UserchangePassword)(context.ObjectInstance)).ConfirmPassword;

        //    if (NewPassword != null && ConfirmPassword != null && NewPassword != ConfirmPassword)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateNewPasswordandConfirmPassword, new string[] { "ConfirmPassword" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateOccupancyQuantity(long quantiy, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (quantiy < 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblRoomQuantiy, new string[] { "RoomQuantity" });
        //    }

        //    return result;
        //}

        //public static ValidationResult ValidateExtraPrice(long value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (value < 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateExtraPrice, new string[] { "Value" });
        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateBoardType(bool value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;
        //    long? BoardType_UID = ((Extra)(context.ObjectInstance)).BoardType_UID;

        //    if (value && (BoardType_UID == null || BoardType_UID <= 0))
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { "BoardType_UID" });
        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateGeoPoints(Property property, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (property != null)
        //    {
        //        //validate GeoLocationLat
        //        if (!string.IsNullOrEmpty(property.GeoLocationLat))
        //        {
        //            double lat = 0;
        //            try
        //            {
        //                lat = Convert.ToDouble(property.GeoLocationLat.Replace(".", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));

        //                //if (!(lat >= -86 && lat <= 86))
        //                if (!(lat >= -180 && lat <= 180))
        //                {
        //                    result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLat" });
        //                }
        //            }
        //            catch
        //            {
        //                result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLat" });
        //            }
        //        }

        //        //validate GeoLocationLng
        //        if (!string.IsNullOrEmpty(property.GeoLocationLng))
        //        {
        //            double lat = 0;
        //            try
        //            {
        //                lat = Convert.ToDouble(property.GeoLocationLng.Replace(".", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));

        //                //if (!(lat >= -86 && lat <= 86))
        //                if (!(lat >= -180 && lat <= 180))
        //                {
        //                    result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLng" });
        //                }
        //            }
        //            catch
        //            {
        //                result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLng" });
        //            }
        //        }

        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateGeoLat(string value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (value != null && value.Trim().Length > 0)
        //    {
        //        double lat = 0;
        //        try
        //        {
        //            string strValue = value;
        //            lat = Convert.ToDouble(strValue.Replace(".", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));

        //            //if (!(lat >= -86 && lat <= 86))
        //            if (!(lat >= -180 && lat <= 180))
        //            {
        //                result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLat" });
        //            }
        //        }
        //        catch
        //        {
        //            result = new ValidationResult(CustomErrorMessage.lblValidateLat, new string[] { "GeoLocationLat" });
        //        }
        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateGeoLon(string value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (value != null && value.Trim().Length > 0)
        //    {
        //        double lat = 0;
        //        try
        //        {
        //            string strValue = value;

        //            lat = Convert.ToDouble(strValue.Replace(".", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));

        //            if (!(lat >= -180 && lat <= 180))
        //            {
        //                result = new ValidationResult(CustomErrorMessage.lblValidateLon, new string[] { "GeoLocationLng" });
        //            }
        //        }
        //        catch
        //        {
        //            result = new ValidationResult(CustomErrorMessage.lblValidateLon, new string[] { "GeoLocationLng" });
        //        }
        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateBuildYear(object value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (value != null && (int)value > DateTime.Now.Year)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidatePropertyBuildYear, new string[] { "BuildYear" });
        //    }
        //    return result;
        //}

        //public static ValidationResult ValidateRenovationYear(object value, ValidationContext context)
        //{
        //    ValidationResult result = ValidationResult.Success;

        //    if (value != null && (int)value > DateTime.Now.Year)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidatePropertyRenYear, new string[] { "RenovationYear" });
        //    }
        //    return result;
        //}

        ////public static ValidationResult ValidateGroupPrice(object value, ValidationContext context)
        ////{
        ////    ValidationResult result = ValidationResult.Success;

        ////    ChildTerm childTerm = (ChildTerm)value;
        ////    bool isChild = (!childTerm.CountsAsAdult && (childTerm.IsFree.HasValue ? !childTerm.IsFree.Value : true)) ? true : false;
        ////    if (childTerm != null && !childTerm.CountsAsAdult && !isChild && (!childTerm.IsFree.HasValue || (childTerm.IsFree.HasValue && !childTerm.IsFree.Value)))
        ////    {
        ////        result = new ValidationResult(CustomErrorMessage.lblChildTermPriceRequired, new string[] { "CountsAsAdult" });
        ////    }
        ////    return result;
        ////}

        ///// <summary>
        ///// Created By :: Hitesh Patel
        ///// Created Date :: 01 June 2012
        ///// Desc :: To Check From and TO date for Package Period
        ///// </summary>
        //public static ValidationResult ValidatePackagesPeriodStartEndDate(PackagesPeriodCustom objPackagesPeriod, ValidationContext context)
        //{
        //    ValidationResult result;

        //    //DateTime? StartDate = ((Package)(context.ObjectInstance)).DateFrom;
        //    //DateTime? EndDate = ((Package)(context.ObjectInstance)).DateTo;

        //    if (objPackagesPeriod.DateFrom.HasValue && objPackagesPeriod.DateTo.HasValue && objPackagesPeriod.DateTo.Value < objPackagesPeriod.DateFrom.Value)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblPackageValidateStartEndDate, new string[] { "DateTo" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        //public static ValidationResult ValidateRoomType(long roomTypeId, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (roomTypeId <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateRoomType, new string[] { "RoomType_UID" });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Hitesh Patel
        ///// Created Date :: 15 Aug 2012
        ///// Desc :: To validate Parcel >= 2
        ///// </summary>
        //public static ValidationResult ValidateNoOfParcel(int parcel, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (parcel < 2)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Created By :: Hitesh Patel
        ///// Created Date :: 21 Aug 2012
        ///// Desc :: To validate RoomTypeId > 0
        ///// </summary>
        //public static ValidationResult ValidateRoomTypeUID(int roomtypeid, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (roomtypeid <= 0)
        //    {
        //        result = new ValidationResult(CustomErrorMessage.lblValidateDropDown, new string[] { context.DisplayName });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}

        ///// <summary>
        ///// Validate rate currency
        ///// </summary>
        //public static ValidationResult ValidateRateCurrencyUID(long currencyUID, ValidationContext context)
        //{
        //    ValidationResult result;

        //    if (currencyUID <= 0)
        //    {
        //        result = new ValidationResult(string.Format(CustomErrorMessage.lblRequired,context.DisplayName), new string[] { context.DisplayName });
        //    }
        //    else
        //    {
        //        result = ValidationResult.Success;
        //    }
        //    return (result);
        //}
    }
}