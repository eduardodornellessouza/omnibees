using Newtonsoft.Json;
using OB.BL.Contracts.Data.Channels;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Operations.Helper;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using PaymentGatewaysLibrary.PaypalClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using contractsChannel = OB.Reservation.BL.Contracts.Data.Channels;
using contractsPayment = OB.Reservation.BL.Contracts.Data.Payments;
using dynLinq = Kendo.DynamicLinq;

namespace OB.BL.Operations.Internal.TypeConverters
{
    public class OtherConverter
    {
        public static OB.DL.Common.Filter.FilterByInfo Convert(RequestFilterBase obj)
        {
            var newObj = new OB.DL.Common.Filter.FilterByInfo();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(RequestFilterBase obj, OB.DL.Common.Filter.FilterByInfo objDestination)
        {
            objDestination.Operator = (DL.Common.Filter.FilterOperator)((int)obj.Filter);
            objDestination.FilterBy = obj.FilterBy;
            objDestination.Value = obj.Value != null ? Newtonsoft.Json.JsonConvert.DeserializeObject(obj.Value, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }) : obj.Value;
            objDestination.Conjunction = (DL.Common.Filter.FilterConjunction)((int)obj.Operator);
        }


        #region Kendo Filter

        public static dynLinq.Filter Convert(IEnumerable<RequestFilterBase> objs)
        {
            var newObj = new dynLinq.Filter();

            if (objs != null && objs.Any())
            {
                // Convert to Kendo Filter
                var flatFiltersList = new List<dynLinq.Filter>();
                foreach (var obj in objs)
                {
                    var objDestination = new dynLinq.Filter();
                    objDestination.Field = obj.FilterBy;
                    objDestination.Value = obj.Value != null ? Newtonsoft.Json.JsonConvert.DeserializeObject(obj.Value, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }) : obj.Value;
                    objDestination.Operator = Convert(obj.Filter);
                    objDestination.Logic = Convert(obj.Operator);
                    flatFiltersList.Add(objDestination);
                }
                flatFiltersList.Last().Logic = dynLinq.FilterLogic.Or;

                // Build Nesting flat filter according list
                var orFilters = new List<dynLinq.Filter>();
                List<dynLinq.Filter> currentAndFilters = null;

                if (flatFiltersList[0].Logic == dynLinq.FilterLogic.Or)
                    orFilters.Add(flatFiltersList[0]);
                else
                {
                    orFilters.Add(new dynLinq.Filter() { Logic = dynLinq.FilterLogic.And, Filters = new List<dynLinq.Filter>() }); // Creates new And Filter
                    currentAndFilters = (List<dynLinq.Filter>)orFilters.Last().Filters;
                    currentAndFilters.Add(flatFiltersList[0]);
                }

                for (int i = 1; i < flatFiltersList.Count; i++)
                {
                    // OR
                    if (flatFiltersList[i - 1].Logic == dynLinq.FilterLogic.Or)
                    {
                        if (flatFiltersList[i].Logic == dynLinq.FilterLogic.And)
                        {
                            orFilters.Add(new dynLinq.Filter() { Logic = dynLinq.FilterLogic.And, Filters = new List<dynLinq.Filter>() }); // Creates new And Filter
                            currentAndFilters = (List<dynLinq.Filter>)orFilters.Last().Filters;
                            currentAndFilters.Add(flatFiltersList[i]);
                        }
                        else
                            orFilters.Add(flatFiltersList[i]);
                    }

                    // AND
                    else
                        currentAndFilters.Add(flatFiltersList[i]);
                }
                newObj.Logic = dynLinq.FilterLogic.Or;
                newObj.Filters = orFilters;
            }

            return newObj;
        }

        public static dynLinq.Filter Convert(NestedFilterRequestBase obj)
        {
            var newObj = new dynLinq.Filter();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(NestedFilterRequestBase obj, dynLinq.Filter objDestination)
        {
            objDestination.Field = obj.FilterBy;
            objDestination.Value = obj.Value != null ? Newtonsoft.Json.JsonConvert.DeserializeObject(obj.Value, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }) : obj.Value;
            objDestination.Operator = Convert(obj.Descriptor);
            objDestination.Logic = Convert(obj.Operator);
            objDestination.IsEnumerableField = obj.IsEnumerable;

            if (obj.Filters != null && obj.Filters.Any())
                objDestination.Filters = obj.Filters.Select(x => Convert(x)).ToList();
        }

        private static dynLinq.FilterOperator Convert(FilterDescriptor obj)
        {
            var filterOperator = (DL.Common.Filter.FilterOperator)((int)obj);

            switch (filterOperator)
            {
                case DL.Common.Filter.FilterOperator.IsLessThan: return dynLinq.FilterOperator.IsLessThan;
                case DL.Common.Filter.FilterOperator.IsLessThanOrEqualTo: return dynLinq.FilterOperator.IsLessThanOrEqualTo;
                case DL.Common.Filter.FilterOperator.IsEqualTo: return dynLinq.FilterOperator.IsEqualTo;
                case DL.Common.Filter.FilterOperator.IsNotEqualTo: return dynLinq.FilterOperator.IsNotEqualTo;
                case DL.Common.Filter.FilterOperator.IsGreaterThanOrEqualTo: return dynLinq.FilterOperator.IsGreaterThanOrEqualTo;
                case DL.Common.Filter.FilterOperator.IsGreaterThan: return dynLinq.FilterOperator.IsGreaterThan;
                case DL.Common.Filter.FilterOperator.StartsWith: return dynLinq.FilterOperator.StartsWith;
                case DL.Common.Filter.FilterOperator.EndsWith: return dynLinq.FilterOperator.EndsWith;
                case DL.Common.Filter.FilterOperator.Contains: return dynLinq.FilterOperator.Contains;
                case DL.Common.Filter.FilterOperator.DoesNotContain: return dynLinq.FilterOperator.DoesNotContain;
                case DL.Common.Filter.FilterOperator.IsContainedIn: return dynLinq.FilterOperator.IsContainedIn;
                case DL.Common.Filter.FilterOperator.IsNotContainedIn: return dynLinq.FilterOperator.IsNotContainedIn;
                case DL.Common.Filter.FilterOperator.IsNull: return dynLinq.FilterOperator.IsNull;
                case DL.Common.Filter.FilterOperator.IsNotNull: return dynLinq.FilterOperator.IsNotNull;
                default: return default(dynLinq.FilterOperator);
            }
        }

        private static dynLinq.FilterLogic Convert(FilterOperator obj)
        {
            var filterConjunction = (DL.Common.Filter.FilterConjunction)((int)obj);

            switch (filterConjunction)
            {
                case DL.Common.Filter.FilterConjunction.AND: return dynLinq.FilterLogic.And;
                case DL.Common.Filter.FilterConjunction.OR: return dynLinq.FilterLogic.Or;
                default: return default(dynLinq.FilterLogic);
            }
        }

        #endregion

        #region ReservationAdditionalData

        public static Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData Convert(OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction obj)
        {
            var newObj = new Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData();

            Map(obj, newObj);

            return newObj;
        }

        public static Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData Convert(OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction obj, Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData objDestination)
        {
            Map(obj, objDestination);
            return objDestination;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction obj, Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData objDestination)
        {
            objDestination.PaymentGatewayDetails = new List<Reservation.BL.Contracts.Data.Reservations.PaymentGatewayDetail>();
            foreach (var payment in obj.PaymentDetails)
            {
                var paymentGatewayDetail = new Reservation.BL.Contracts.Data.Reservations.PaymentGatewayDetail
                {
                    Id = payment.Id,
                    Installments = payment.Installments,
                    AuthorizationCode = payment.AuthorizationCode,
                    Nsu = payment.Nsu,
                    Tid = payment.Tid,
                    Authentication3DS = payment.Authentication3DS,
                    ProviderReturnMessage = payment.ProviderReturnMessage,
                    CaptureDate = payment.CaptureDate,
                    CancellationDate = payment.CancellationDate,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Provider = payment.Provider,
                    Status = payment.Status.ToString(),
                };

                objDestination.PaymentGatewayDetails.Add(paymentGatewayDetail);
            }
        }

        #endregion ReservationPaymentDetail

        public static OB.DL.Common.Filter.SortByInfo Convert(RequestOrderBase obj)
        {
            var newObj = new OB.DL.Common.Filter.SortByInfo();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(RequestOrderBase obj, OB.DL.Common.Filter.SortByInfo objDestination)
        {
            objDestination.Direction = (DL.Common.Filter.SortDirection)((int)obj.Direction);
            objDestination.OrderBy = obj.OrderBy;
            objDestination.Initial = obj.Initial;
        }

        public static OB.DL.Common.ListReservationCriteria Convert(ListReservationRequest obj)
        {
            var newObj = new OB.DL.Common.ListReservationCriteria();

            Map(obj, newObj);
            return newObj;
        }

        public static void Map(ListReservationRequest obj, OB.DL.Common.ListReservationCriteria objDestination)
        {
            objDestination.ReservationUIDs = obj.ReservationUIDs;
            objDestination.PropertyUIDs = obj.PropertyUIDs;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.ReservationNumbers = obj.ReservationNumbers;
            objDestination.ReservationStatusCodes = obj.ReservationStatusCodes;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.ModifiedFrom = obj.ModifiedFrom;
            objDestination.ModifiedTo = obj.ModifiedTo;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.LanguageUID = obj.LanguageUID;
            objDestination.IncludeReservationRooms = obj.IncludeReservationRooms;
            objDestination.IncludeReservationRoomDetails = obj.IncludeReservationRoomDetails;
            objDestination.IncludeReservationRoomChilds = obj.IncludeReservationRoomChilds;
            objDestination.IncludeReservationRoomExtras = obj.IncludeReservationRoomExtras;
            objDestination.IncludeReservationRoomExtrasSchedules = obj.IncludeReservationRoomExtrasSchedules;
            objDestination.IncludeReservationRoomExtrasAvailableDates = obj.IncludeReservationRoomExtrasAvailableDates;
            objDestination.IncludeReservationRoomTaxPolicies = obj.IncludeReservationRoomTaxPolicies;
            objDestination.IncludeReservationPaymentDetail = obj.IncludeReservationPaymentDetail;
            objDestination.IncludeReservationPartialPaymentDetails = obj.IncludeReservationPartialPaymentDetails;
            objDestination.IncludeGuests = obj.IncludeGuests;
            objDestination.IncludeTaxPolicies = obj.IncludeTaxPolicies;
            objDestination.IncludeRates = obj.IncludeRates;
            objDestination.IncludeRateCategories = obj.IncludeRateCategories;
            objDestination.IncludeOtaCodes = obj.IncludeOtaCodes;
            objDestination.IncludeRoomTypes = obj.IncludeRoomTypes;
            objDestination.IncludePromotionalCodes = obj.IncludePromotionalCodes;
            objDestination.IncludeReservationRoomDetailsAppliedIncentives = obj.IncludeReservationRoomDetailsAppliedIncentives;
            objDestination.IncludeReservationRoomIncentivePeriods = obj.IncludeReservationRoomIncentivePeriods;
            objDestination.IncludeReservationRoomDetailsAppliedPromotionalCode = obj.IncludeReservationRoomDetailsAppliedPromotionalCode;
            objDestination.ReservationStatus = obj.ReservationStatus;
            objDestination.PartnerIds = obj.PartnerIds;
            objDestination.PartnerReservationNumbers = obj.PartnerReservationNumbers;
            objDestination.TpiIds = obj.TpiIds;
            objDestination.IncludeExtras = obj.IncludeExtras;
            objDestination.IncludeExtrasBillingTypes = obj.IncludeExtrasBillingTypes;
            objDestination.IncludeIncentives = obj.IncludeIncentives;
            objDestination.IncludeGroupCodes = obj.IncludeGroupCodes;
            objDestination.IncludeReservationAddicionalData = obj.IncludeReservationAddicionalData;
            objDestination.IncludeGuestActivities = obj.IncludeGuestActivities;
            objDestination.IncludeBESpecialRequests = obj.IncludeBESpecialRequests;
            objDestination.IncludeTransferLocation = obj.IncludeTransferLocation;
            objDestination.IncludeReservationBaseCurrency = obj.IncludeReservationBaseCurrency;
            objDestination.IncludeCancelationCosts = obj.IncludeCancelationCosts;
            objDestination.IncludeReservationStatusName = obj.IncludeReservationStatusName;
            objDestination.IncludeChannel = obj.IncludeChannel;
            objDestination.IncludeChannelOperator = obj.IncludeChannelOperator;
            objDestination.IncludePropertyBaseCurrency = obj.IncludePropertyBaseCurrency;
            objDestination.IncludeReservationReadStatus = obj.IncludeReservationReadStatus;
            objDestination.IncludeTPIName = obj.IncludeTPIName;
            objDestination.IncludeTPILanguageUID = obj.IncludeTPILanguageUID;
            objDestination.IncludeCompanyName = obj.IncludeCompanyName;
            objDestination.IncludeReservationCurrency = obj.IncludeReservationCurrency;
            objDestination.IncludePaymentMethodType = obj.IncludePaymentMethodType;
            objDestination.IncludeBillingCountryName = obj.IncludeBillingCountryName;
            objDestination.IncludeBillingStateName = obj.IncludeBillingStateName;
            objDestination.IncludeOnRequestDecisionUser = obj.IncludeOnRequestDecisionUser;
            objDestination.IncludeExternalSource = obj.IncludeExternalSource;
            objDestination.IncludeReferralSource = obj.IncludeReferralSource;
            objDestination.IncludeCommissionTypeName = obj.IncludeCommissionTypeName;
            objDestination.IncludeGuestCountryName = obj.IncludeGuestCountryName;
            objDestination.IncludeGuestStateName = obj.IncludeGuestStateName;
            objDestination.IncludeGuestPrefixName = obj.IncludeGuestPrefixName;
            objDestination.IncludeTPICommissions = obj.IncludeTPICommissions;
            objDestination.IncludePropertyCountry = obj.IncludePropertyCountryCode;

            if (obj.IncludeReservationResumeInfo)
                objDestination.IncludeReservationRooms = true;

            //Manage Filters
            Kendo.DynamicLinq.Filter kendoFilter = null;
            List<DL.Common.Filter.FilterByInfo> oldFilters = null;
            ManageFilteringHelper.ManageOldAndNewFilters(out kendoFilter, out oldFilters, obj);

            if (kendoFilter != null)
                objDestination.NestedFilters = kendoFilter;
            if (oldFilters != null)
                objDestination.Filters = oldFilters;

            if (obj.Orders != null)
                objDestination.Orders = obj.Orders.Select(x => OtherConverter.Convert(x)).ToList();

        }

        public static void Map(ListReservationRequest obj, OB.DL.Common.ListReservationFilterCriteria objDestination)
        {
            objDestination.ReservationUIDs = obj.ReservationUIDs;
            objDestination.ReservationNumbers = obj.ReservationNumbers;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.PropertyUIDs = obj.PropertyUIDs;
            objDestination.PropertyNames = obj.PropertyNames;
            objDestination.ChannelNames = obj.ChannelNames;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.IsOnRequest = obj.IsOnRequest;
            objDestination.IsReaded = obj.IsReaded;
            objDestination.ModifiedFrom = obj.ModifiedFrom;
            objDestination.ModifiedTo = obj.ModifiedTo;
            objDestination.GuestName = obj.GuestName;
            objDestination.NumberOfNights = obj.NumberOfNights;
            objDestination.NumberOfAdults = obj.NumberOfAdults;
            objDestination.NumberOfChildren = obj.NumberOfChildren;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.PaymentTypeIds = obj.PaymentTypeIds;
            objDestination.GuestIds = obj.GuestIds;
            objDestination.TpiIds = obj.TpiIds;
            objDestination.TPINames = obj.TPINames;
            objDestination.ReservationStatus = obj.ReservationStatus;
            objDestination.TotalAmount = obj.TotalAmount;
            objDestination.ExternalTotalAmount = obj.ExternalTotalAmount;
            objDestination.ExternalCommissionValue = obj.ExternalCommissionValue;
            objDestination.ExternalIsPaid = obj.ExternalIsPaid;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.IsPaid = obj.IsPaid;
            objDestination.ReservationDate = obj.ReservationDate;
            objDestination.ExternalChannelUids = obj.ExternalChannelIds;
            objDestination.ExternalTPIUids = obj.ExternalTpiIds;
            objDestination.ExternalNames = obj.ExternalNames;
            objDestination.PartnerIds = obj.PartnerIds;
            objDestination.PartnerReservationNumbers = obj.PartnerReservationNumbers;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.EmployeeIds = obj.EmployeeIds;

            objDestination.IncludeReservationRoomsFilter = obj.IncludeReservationRooms;

            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckInTo = obj.CheckInTo;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.CheckOutFrom = obj.CheckOutFrom;
            objDestination.ApplyDepositPolicy = obj.ApplyDepositPolicy;

            objDestination.PageIndex = obj.PageIndex;
            objDestination.PageSize = obj.PageSize;

            objDestination.BigPullAuthRequestorUIDs = obj.BigPullAuthRequestorUIDs;
            objDestination.BigPullAuthOwnerUIDs = obj.BigPullAuthOwnerUIDs;

            //Manage Filters
            Kendo.DynamicLinq.Filter kendoFilter = null;
            List<DL.Common.Filter.FilterByInfo> oldFilters = null;
            ManageFilteringHelper.ManageOldAndNewFilters(out kendoFilter, out oldFilters, obj);

            if (kendoFilter != null)
                objDestination.NestedFilters = kendoFilter;
            else if (oldFilters != null)
                objDestination.Filters = oldFilters;

            if (obj.Orders != null)
                objDestination.Orders = obj.Orders.Select(x => OtherConverter.Convert(x)).ToList();

        }

        public static void Map(ListReservationsFilterRequest obj, OB.DL.Common.ListReservationFilterCriteria objDestination)
        {
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ReservationNumbers = obj.ReservationNumbers;
            objDestination.PropertyUIDs = obj.PropertyUIDs;
            objDestination.PropertyNames = obj.PropertyNames;
            objDestination.ChannelNames = obj.ChannelNames;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.IsOnRequest = obj.IsOnRequest;
            objDestination.IsReaded = obj.IsReaded;
            objDestination.ModifiedFrom = obj.ModifiedFrom;
            objDestination.ModifiedTo = obj.ModifiedTo;
            objDestination.GuestName = obj.GuestName;
            objDestination.NumberOfNights = obj.NumberOfNights;
            objDestination.NumberOfAdults = obj.NumberOfAdults;
            objDestination.NumberOfChildren = obj.NumberOfChildren;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.PaymentTypeIds = obj.PaymentTypeIds;
            objDestination.TpiIds = obj.TpiIds;
            objDestination.TPINames = obj.TPINames;
            objDestination.ReservationStatus = obj.ReservationStatus;
            objDestination.TotalAmount = obj.TotalAmount;
            objDestination.ExternalTotalAmount = obj.ExternalTotalAmount;
            objDestination.ExternalCommissionValue = obj.ExternalCommissionValue;
            objDestination.ExternalIsPaid = obj.ExternalIsPaid;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.IsPaid = obj.IsPaid;
            objDestination.ReservationDate = obj.ReservationDate;
            objDestination.ExternalChannelUids = obj.ExternalChannelIds;
            objDestination.ExternalTPIUids = obj.ExternalTpiIds;
            objDestination.ExternalNames = obj.ExternalNames;
            objDestination.PartnerIds = obj.PartnerIds;
            objDestination.PartnerReservationNumbers = obj.PartnerReservationNumbers;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;

            objDestination.IncludeReservationRoomsFilter = obj.IncludeReservationRoomsFilters;

            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.ApplyDepositPolicy = obj.ApplyDepositPolicy;
            objDestination.FilterQtyExcludeReservationRoomsCancelled = obj.FilterQtyExcludeReservationRoomsCancelled;

            objDestination.PageIndex = obj.PageIndex;
            objDestination.PageSize = obj.PageSize;

            //Manage Filters
            Kendo.DynamicLinq.Filter kendoFilter = null;
            List<DL.Common.Filter.FilterByInfo> oldFilters = null;
            ManageFilteringHelper.ManageOldAndNewFilters(out kendoFilter, out oldFilters, obj);

            if (kendoFilter != null)
                objDestination.NestedFilters = kendoFilter;
            else if (oldFilters != null)
                objDestination.Filters = oldFilters;

            if (obj.Orders != null)
                objDestination.Orders = obj.Orders.Select(x => OtherConverter.Convert(x)).ToList();

        }


        public static ReservationRoomCost Convert(OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomCancelationCost obj)
        {
            var newObj = new ReservationRoomCost();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomCancelationCost obj, ReservationRoomCost objDestination)
        {
            objDestination.RoomCost = obj.CancelationCosts;
            objDestination.RoomNumber = obj.Number;
            objDestination.RoomStatus = obj.Status;
            objDestination.CurrencyUid = obj.CurrencyUid;
        }


        public static void Map2(ListReservationRequest obj, OB.DL.Common.ListReservationFilterCriteria objDestination)
        {
            objDestination.PropertyUIDs = obj.PropertyUIDs;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.ReservationNumbers = obj.ReservationNumbers;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOutFrom = obj.CheckOutFrom;
            objDestination.CheckInTo = obj.CheckInTo;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.ChannelUIDs = obj.ChannelUIDs;
            objDestination.TpiIds = obj.TpiIds;
            objDestination.PaymentTypeIds = new List<long> { 4 };
            objDestination.IsPaid = false;
            objDestination.ReservationStatus = new List<long> { 1, 3, 4, 5, 8, 9, 10 };
            objDestination.ReservationUIDs = obj.ReservationUIDs;
        }
        /// <summary>
        /// Converts RoomInventory list to a DataTable. 
        /// Used for UpdateInventory Stored Procedure.
        /// </summary>
        /// <param name="inventoriesToUpdate">The inventories to update.</param>
        /// <returns>The DataTable parameter</returns>
        public static List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable> ConvertToRoomInventoryDataTable(List<OB.BL.Contracts.Data.Properties.RoomInventory> inventoriesToUpdate)
        {
            var roomsInventory = new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>();

            if (!inventoriesToUpdate.Any())
                return roomsInventory;

            foreach (var distinctInventoryToUpdate in inventoriesToUpdate.Where(x => x != null).GroupBy(x => new { x.RoomTypeUID, x.StartDate, x.EndDate, x.Type, x.Count }))
            {
                var inventoryToUpdate = distinctInventoryToUpdate.First();
                for (DateTime date = inventoryToUpdate.StartDate.Date; date <= inventoryToUpdate.EndDate.Date; date = date.AddDays(1))
                {
                    roomsInventory.Add(new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                    {
                        RoomTypeId = inventoryToUpdate.RoomTypeUID,
                        Date = date,
                        Type = (int)inventoryToUpdate.Type,
                        Count = inventoryToUpdate.Count
                    });
                }
            }

            // Return distinct values
            return roomsInventory.GroupBy(x => new { x.RoomTypeId, x.Date, x.Type, x.Count }).Select(x => x.First()).ToList();
        }

        #region Guest
        public static OB.Reservation.BL.Contracts.Data.CRM.Guest Convert(OB.BL.Contracts.Data.CRM.Guest obj)
        {
            if (obj == null)
                return null;

            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.Guest();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.CRM.Guest obj, OB.Reservation.BL.Contracts.Data.CRM.Guest objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Prefix = obj.Prefix;
            objDestination.FirstName = obj.FirstName;
            objDestination.LastName = obj.LastName;
            objDestination.Address1 = obj.Address1;
            objDestination.Address2 = obj.Address2;
            objDestination.City = obj.City;
            objDestination.PostalCode = obj.PostalCode;
            objDestination.BillingAddress1 = obj.BillingAddress1;
            objDestination.BillingAddress2 = obj.BillingAddress2;
            objDestination.BillingCity = obj.BillingCity;
            objDestination.BillingPostalCode = obj.BillingPostalCode;
            objDestination.BillingPhone = obj.BillingPhone;
            objDestination.BillingExt = obj.BillingExt;
            objDestination.BillingCountry_UID = obj.BillingCountry_UID;
            objDestination.Country_UID = obj.Country_UID;
            objDestination.Phone = obj.Phone;
            objDestination.PhoneExt = obj.PhoneExt;
            objDestination.MobilePhone = obj.MobilePhone;
            objDestination.UserName = obj.UserName;
            objDestination.UserPassword = obj.UserPassword;
            objDestination.PasswordHint = obj.PasswordHint;
            objDestination.GuestCategory_UID = obj.GuestCategory_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Currency_UID = obj.Currency_UID;
            objDestination.Language_UID = obj.Language_UID;
            objDestination.Email = obj.Email;
            objDestination.Birthday = obj.Birthday;
            objDestination.CreatedByTPI_UID = obj.CreatedByTPI_UID;
            objDestination.IsActive = obj.IsActive;
            objDestination.CreateDate = obj.CreateDate;
            objDestination.LastLoginDate = obj.LastLoginDate;
            objDestination.FacebookUser = obj.FacebookUser;
            objDestination.TwitterUser = obj.TwitterUser;
            objDestination.TripAdvisorUser = obj.TripAdvisorUser;
            objDestination.AllowMarketing = obj.AllowMarketing;
            objDestination.CreateBy = obj.CreateBy;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifyBy = obj.ModifyBy;
            objDestination.ModifyDate = obj.ModifyDate;
            objDestination.Question_UID = obj.Question_UID;
            objDestination.State = obj.State;
            objDestination.BillingState = obj.BillingState;
            objDestination.IsFacebookFan = obj.IsFacebookFan;
            objDestination.IsTwitterFollower = obj.IsTwitterFollower;
            objDestination.IDCardNumber = obj.IDCardNumber;
            objDestination.BillingState_UID = obj.BillingState_UID;
            objDestination.State_UID = obj.State_UID;
            objDestination.BillingEmail = obj.BillingEmail;
            objDestination.Client_UID = obj.Client_UID;
            objDestination.BillingContactName = obj.BillingContactName;
            objDestination.BillingTaxCardNumber = obj.BillingTaxCardNumber;
            objDestination.UseDifferentBillingInfo = obj.UseDifferentBillingInfo;
            objDestination.IsImportedFromExcel = obj.IsImportedFromExcel;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.Gender = obj.Gender;
            objDestination.LoyaltyLevel_UID = obj.LoyaltyLevel_UID;
            objDestination.PrefixName = obj.PrefixName;

            if (obj.GuestSocialProviders != null && obj.GuestSocialProviders.Count > 0)
                objDestination.GuestSocialProviders = obj.GuestSocialProviders.Select(x => Convert(x)).ToList();

            if (obj.GuestActivities != null && obj.GuestActivities.Count > 0)
                objDestination.GuestActivities = obj.GuestActivities.Select(x => Convert(x)).ToList();

            if (obj.GuestFavoriteExtras != null && obj.GuestFavoriteExtras.Count > 0)
                objDestination.GuestFavoriteExtras = obj.GuestFavoriteExtras.Select(x => Convert(x)).ToList();

            if (obj.GuestFavoriteSpecialRequests != null && obj.GuestFavoriteSpecialRequests.Count > 0)
                objDestination.GuestFavoriteSpecialRequests = obj.GuestFavoriteSpecialRequests.Select(x => Convert(x)).ToList();
        }
        #endregion Guest

        #region GuestSocialProviders
        public static OB.Reservation.BL.Contracts.Data.CRM.GuestSocialProvider Convert(OB.BL.Contracts.Data.CRM.GuestSocialProvider obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.GuestSocialProvider();
            Map(obj, newObj);
            return newObj;
        }
        public static void Map(OB.BL.Contracts.Data.CRM.GuestSocialProvider obj, OB.Reservation.BL.Contracts.Data.CRM.GuestSocialProvider objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.SocialProvider_UID = obj.SocialProvider_UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.ProviderUserID = obj.ProviderUserID;
            objDestination.Revision = obj.Revision;
        }
        #endregion

        #region GuestActivity
        public static OB.Reservation.BL.Contracts.Data.CRM.GuestActivity Convert(OB.BL.Contracts.Data.CRM.GuestActivity obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.GuestActivity();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.CRM.GuestActivity obj, OB.Reservation.BL.Contracts.Data.CRM.GuestActivity objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.Activity_UID = obj.Activity_UID;

        }
        #endregion GuestActivity

        #region GuestFavoriteExtra
        public static OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteExtra Convert(OB.BL.Contracts.Data.CRM.GuestFavoriteExtra obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteExtra();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.CRM.GuestFavoriteExtra obj, OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteExtra objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.Extras_UID = obj.Extras_UID;

        }
        #endregion GuestFavoriteExtra

        #region GuestFavoriteSpecialRequest
        public static OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest Convert(OB.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest obj, OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.BESpecialRequests_UID = obj.BESpecialRequests_UID;

        }
        #endregion GuestFavoriteSpecialRequest

        #region PromotionalCodes
        public static OB.Reservation.BL.Contracts.Data.Rates.PromotionalCode Convert(OB.BL.Contracts.Data.Rates.PromotionalCode obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.PromotionalCode();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.PromotionalCode obj, OB.Reservation.BL.Contracts.Data.Rates.PromotionalCode objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.Code = obj.Code;
            objDestination.DiscountValue = obj.DiscountValue;
            objDestination.IsCommission = obj.IsCommission;

            objDestination.IsPromotionalCodeVisibleRate = obj.IsPromotionalCodeVisibleRate;
            objDestination.IsRegisterTPI = obj.IsRegisterTPI;
            objDestination.IsValid = obj.IsValid;
            objDestination.MaxReservations = obj.MaxReservations;
            objDestination.Name = obj.Name;

            objDestination.PromotionalCode_UID = obj.PromotionalCode_UID;
            objDestination.ReservationsCompleted = obj.ReservationsCompleted;
            objDestination.URL = obj.URL;
            objDestination.ValidFrom = obj.ValidFrom;
            objDestination.ValidTo = obj.ValidTo;
        }
        #endregion PromotionalCodes

        #region Group Codes
        public static OB.Reservation.BL.Contracts.Data.Rates.GroupCode Convert(OB.BL.Contracts.Data.Rates.GroupCode obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.GroupCode();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.GroupCode obj, OB.Reservation.BL.Contracts.Data.Rates.GroupCode objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.InternalCode = obj.InternalCode;
            objDestination.GroupCode1 = obj.GroupCode1;
            objDestination.Name = obj.Name;
            objDestination.Description = obj.Description;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.BeginSell = obj.BeginSell;
            objDestination.EndSell = obj.EndSell;
            objDestination.Rate_UID = obj.Rate_UID;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.IsActive = obj.IsActive;
        }
        #endregion

        #region Rate
        public static OB.Reservation.BL.Contracts.Data.Rates.Rate Convert(OB.BL.Contracts.Data.Rates.Rate obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.Rate();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(Rate obj, Reservation.BL.Contracts.Data.Rates.Rate objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.BeginSale = obj.BeginSale;
            objDestination.CancellationPolicy_UID = obj.CancellationPolicy_UID;
            objDestination.Currency_UID = obj.Currency_UID;
            objDestination.DepositPolicy_UID = obj.DepositPolicy_UID;
            objDestination.EndSale = obj.EndSale;
            objDestination.GDSSabreRateName = obj.GDSSabreRateName;
            objDestination.IsAllExtrasIncluded = obj.IsAllExtrasIncluded;
            objDestination.IsAvailableToTPI = obj.IsAvailableToTPI;
            objDestination.IsParity = obj.IsParity;
            objDestination.IsPercentage = obj.IsPercentage;
            objDestination.IsPriceDerived = obj.IsPriceDerived;
            objDestination.IsValueDecrease = obj.IsValueDecrease;
            objDestination.IsYielding = obj.IsYielding;
            objDestination.Name = obj.Name;
            objDestination.OtherPolicy_UID = obj.OtherPolicy_UID;
            objDestination.PriceModel = obj.PriceModel;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Rate_UID = obj.Rate_UID;
            objDestination.Value = obj.Value;
            objDestination.Description = obj.Description;
            objDestination.AvailabilityType = obj.AvailabilityType;
            objDestination.InternalName = obj.InternalName;
            objDestination.RateCategoryId = obj.RateCategoryId;
        }
        #endregion Rate

        #region RoomType
        public static OB.Reservation.BL.Contracts.Data.Properties.RoomType Convert(OB.BL.Contracts.Data.Properties.RoomType obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Properties.RoomType();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Properties.RoomType obj, OB.Reservation.BL.Contracts.Data.Properties.RoomType objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.Description = obj.Description;
            objDestination.ShortDescription = obj.ShortDescription;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Qty = obj.Qty;
            objDestination.AdultMaxOccupancy = obj.AdultMaxOccupancy;
            objDestination.AdultMinOccupancy = obj.AdultMinOccupancy;
            objDestination.AcceptsChildren = obj.AcceptsChildren;
            objDestination.ChildMaxOccupancy = obj.ChildMaxOccupancy;
            objDestination.ChildMinOccupancy = obj.ChildMinOccupancy;
            objDestination.AcceptsExtraBed = obj.AcceptsExtraBed;
            objDestination.MaxOccupancy = obj.MaxOccupancy;
            objDestination.MaxFreeChild = obj.MaxFreeChild;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.BasePrice = obj.BasePrice;
            objDestination.IsBase = obj.IsBase;
            objDestination.Value = obj.Value;
            objDestination.IsPercentage = obj.IsPercentage;
            objDestination.IsValueDecrease = obj.IsValueDecrease;
            objDestination.MaxValue = obj.MaxValue;
            objDestination.MinValue = obj.MinValue;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
        }
        #endregion RoomType

        #region Extra

        public static OB.Reservation.BL.Contracts.Data.Rates.Extra Convert(OB.BL.Contracts.Data.Rates.Extra obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.Extra();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.Extra obj, OB.Reservation.BL.Contracts.Data.Rates.Extra objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;


            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.Description = obj.Description;
            objDestination.Value = obj.Value;
            objDestination.Image_UID = obj.Image_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.ExtraBillingType_UID = obj.ExtraBillingType_UID;
            objDestination.VAT = obj.VAT;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.NotificationEmail = obj.NotificationEmail;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.IsBoardType = obj.IsBoardType;
            objDestination.BoardType_UID = obj.BoardType_UID;
            objDestination.IsActive = obj.IsActive;
            objDestination.Revision = obj.Revision;
            objDestination.ExtraOrder = obj.ExtraOrder;
        }
        #endregion Extra

        #region TaxPolicy

        public static OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy Convert(OB.BL.Contracts.Data.Rates.TaxPolicy obj)
        {
            if (obj == null)
                return null;

            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.TaxPolicy obj, OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.Description = obj.Description;
            objDestination.IsPercentage = obj.IsPercentage ?? false;
            objDestination.Name = obj.Name;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Value = obj.Value;
            objDestination.IsDeleted = obj.IsDeleted;
        }

        #endregion TaxPolicy

        #region Incentive
        public static OB.Reservation.BL.Contracts.Data.Properties.Incentive Convert(OB.BL.Contracts.Data.Properties.Incentive obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Properties.Incentive();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Properties.Incentive obj, OB.Reservation.BL.Contracts.Data.Properties.Incentive objDestination)
        {
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.Days = obj.Days;
            objDestination.DiscountPercentage = obj.DiscountPercentage;
            objDestination.FreeDays = obj.FreeDays;
            objDestination.Hours = obj.Hours;
            objDestination.IncentiveType = (OB.Reservation.BL.Constants.IncentiveType)obj.IncentiveType_UID;
            objDestination.IsBetweenNights = obj.IsBetweenNights;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.IsFreeDaysAtBegin = obj.IsFreeDaysAtBegin;
            objDestination.IsLastMinuteInHours = obj.IsLastMinuteInHours;
            objDestination.MaxDays = obj.MaxDays;
            objDestination.MinDays = obj.MinDays;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.Name = obj.Name;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Revision = obj.Revision;
            objDestination.UID = obj.UID;
        }
        #endregion Incentive

        #region Guest
        public static OB.BL.Contracts.Data.CRM.Guest Convert(OB.Reservation.BL.Contracts.Data.CRM.Guest obj)
        {
            if (obj == null)
                return null;

            var newObj = new OB.BL.Contracts.Data.CRM.Guest();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.CRM.Guest obj, OB.BL.Contracts.Data.CRM.Guest objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Prefix = obj.Prefix;
            objDestination.FirstName = obj.FirstName;
            objDestination.LastName = obj.LastName;
            objDestination.Address1 = obj.Address1;
            objDestination.Address2 = obj.Address2;
            objDestination.City = obj.City;
            objDestination.PostalCode = obj.PostalCode;
            objDestination.BillingAddress1 = obj.BillingAddress1;
            objDestination.BillingAddress2 = obj.BillingAddress2;
            objDestination.BillingCity = obj.BillingCity;
            objDestination.BillingPostalCode = obj.BillingPostalCode;
            objDestination.BillingPhone = obj.BillingPhone;
            objDestination.BillingExt = obj.BillingExt;
            objDestination.BillingCountry_UID = obj.BillingCountry_UID;
            objDestination.Country_UID = obj.Country_UID;
            objDestination.Phone = obj.Phone;
            objDestination.PhoneExt = obj.PhoneExt;
            objDestination.MobilePhone = obj.MobilePhone;
            objDestination.UserName = obj.UserName;
            objDestination.UserPassword = obj.UserPassword;
            objDestination.PasswordHint = obj.PasswordHint;
            objDestination.GuestCategory_UID = obj.GuestCategory_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Currency_UID = obj.Currency_UID;
            objDestination.Language_UID = obj.Language_UID;
            objDestination.Email = obj.Email;
            objDestination.Birthday = obj.Birthday;
            objDestination.CreatedByTPI_UID = obj.CreatedByTPI_UID;
            objDestination.IsActive = obj.IsActive;
            objDestination.CreateDate = obj.CreateDate;
            objDestination.LastLoginDate = obj.LastLoginDate;
            objDestination.FacebookUser = obj.FacebookUser;
            objDestination.TwitterUser = obj.TwitterUser;
            objDestination.TripAdvisorUser = obj.TripAdvisorUser;
            objDestination.AllowMarketing = obj.AllowMarketing;
            objDestination.CreateBy = obj.CreateBy;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifyBy = obj.ModifyBy;
            objDestination.ModifyDate = obj.ModifyDate;
            objDestination.Question_UID = obj.Question_UID;
            objDestination.State = obj.State;
            objDestination.BillingState = obj.BillingState;
            objDestination.IsFacebookFan = obj.IsFacebookFan;
            objDestination.IsTwitterFollower = obj.IsTwitterFollower;
            objDestination.IDCardNumber = obj.IDCardNumber;
            objDestination.BillingState_UID = obj.BillingState_UID;
            objDestination.State_UID = obj.State_UID;
            objDestination.BillingEmail = obj.BillingEmail;
            objDestination.Client_UID = obj.Client_UID;
            objDestination.BillingContactName = obj.BillingContactName;
            objDestination.BillingTaxCardNumber = obj.BillingTaxCardNumber;
            objDestination.UseDifferentBillingInfo = obj.UseDifferentBillingInfo;
            objDestination.IsImportedFromExcel = obj.IsImportedFromExcel;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.Gender = obj.Gender;
            objDestination.LoyaltyLevel_UID = obj.LoyaltyLevel_UID;
            objDestination.LanguageId = obj.Language_UID;

            if (obj.GuestSocialProviders != null && obj.GuestSocialProviders.Count > 0)
                objDestination.GuestSocialProviders = obj.GuestSocialProviders.Select(x => Convert(x)).ToList();

            if (obj.GuestActivities != null && obj.GuestActivities.Count > 0)
                objDestination.GuestActivities = obj.GuestActivities.Select(x => Convert(x)).ToList();

            if (obj.GuestFavoriteExtras != null && obj.GuestFavoriteExtras.Count > 0)
                objDestination.GuestFavoriteExtras = obj.GuestFavoriteExtras.Select(x => Convert(x)).ToList();

            if (obj.GuestFavoriteSpecialRequests != null && obj.GuestFavoriteSpecialRequests.Count > 0)
                objDestination.GuestFavoriteSpecialRequests = obj.GuestFavoriteSpecialRequests.Select(x => Convert(x)).ToList();
        }
        #endregion Guest

        #region GuestSocialProviders
        public static OB.BL.Contracts.Data.CRM.GuestSocialProvider Convert(OB.Reservation.BL.Contracts.Data.CRM.GuestSocialProvider obj)
        {
            var newObj = new OB.BL.Contracts.Data.CRM.GuestSocialProvider();
            Map(obj, newObj);
            return newObj;
        }
        public static void Map(OB.Reservation.BL.Contracts.Data.CRM.GuestSocialProvider obj, OB.BL.Contracts.Data.CRM.GuestSocialProvider objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.SocialProvider_UID = obj.SocialProvider_UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.ProviderUserID = obj.ProviderUserID;
            objDestination.Revision = obj.Revision;
        }
        #endregion

        #region GuestActivity
        public static OB.BL.Contracts.Data.CRM.GuestActivity Convert(OB.Reservation.BL.Contracts.Data.CRM.GuestActivity obj)
        {
            var newObj = new OB.BL.Contracts.Data.CRM.GuestActivity();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.CRM.GuestActivity obj, OB.BL.Contracts.Data.CRM.GuestActivity objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.Activity_UID = obj.Activity_UID;

        }
        #endregion GuestActivity

        #region GuestFavoriteExtra
        public static OB.BL.Contracts.Data.CRM.GuestFavoriteExtra Convert(OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteExtra obj)
        {
            var newObj = new OB.BL.Contracts.Data.CRM.GuestFavoriteExtra();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteExtra obj, OB.BL.Contracts.Data.CRM.GuestFavoriteExtra objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.Extras_UID = obj.Extras_UID;

        }
        #endregion GuestFavoriteExtra

        #region GuestFavoriteSpecialRequest
        public static OB.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest Convert(OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest obj)
        {
            var newObj = new OB.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest obj, OB.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.BESpecialRequests_UID = obj.BESpecialRequests_UID;

        }
        #endregion GuestFavoriteSpecialRequest

        #region GuestFavoriteSpecialRequest
        public static OB.Reservation.BL.Contracts.Data.General.BillingType Convert(OB.BL.Contracts.Data.Rates.ExtrasBillingType obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.General.BillingType();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.ExtrasBillingType obj, OB.Reservation.BL.Contracts.Data.General.BillingType objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Type = obj.Type;
            objDestination.Name = obj.Name;
        }
        #endregion GuestFavoriteSpecialRequest

        #region Currency
        public static OB.Reservation.BL.Contracts.Data.General.Currency Convert(OB.BL.Contracts.Data.General.Currency obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.General.Currency();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.General.Currency obj, OB.Reservation.BL.Contracts.Data.General.Currency objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.Symbol = obj.Symbol;
            objDestination.CurrencySymbol = obj.CurrencySymbol;
            objDestination.DefaultPositionNumber = obj.DefaultPositionNumber;
            objDestination.PaypalCurrencyCode = obj.PaypalCurrencyCode;
        }
        #endregion

        #region RateChannel

        #endregion

        #region RatesExtraPeriod
        public static RatesExtrasPeriod Convert(ReservationRoomExtrasAvailableDate obj)
        {
            var newObj = new RatesExtrasPeriod();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ReservationRoomExtrasAvailableDate obj, RatesExtrasPeriod objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.RatesExtras_UID = obj.ReservationRoomExtra_UID;
        }
        #endregion

        #region Channel
        public static contractsChannel.Channel Convert(Channel obj)
        {
            var newObj = new contractsChannel.Channel();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(Channel obj, contractsChannel.Channel objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.IsBookingEngine = obj.IsBookingEngine;
            objDestination.IsExtended = obj.IsExtended;
            objDestination.Description = obj.Description;
            objDestination.Enabled = obj.Enabled;
            objDestination.Type = obj.Type;
            objDestination.IATA_Number = obj.IATA_Number;
            objDestination.IATA_Name = obj.IATA_Name;
            objDestination.Revision = obj.Revision;
            objDestination.OperatorType = obj.OperatorType;
            objDestination.OperatorCode = obj.OperatorCode;
            objDestination.ChannelCode = obj.ChannelCode;
            objDestination.RealTimeType = obj.RealTimeType;
        }
        #endregion

        #region ChannelOperator
        public static contractsChannel.ChannelOperator Convert(ChannelOperator obj)
        {
            var newObj = new contractsChannel.ChannelOperator();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ChannelOperator obj, contractsChannel.ChannelOperator objDestination)
        {
            objDestination.Channel_UID = obj.Channel_UID;
            objDestination.Name = obj.Name;
            objDestination.CNPJ = obj.CNPJ;
            objDestination.CreditUsed = obj.CreditUsed;
            objDestination.Address1 = obj.Address1;
            objDestination.Address2 = obj.Address2;
            objDestination.PostalCode = obj.PostalCode;
            objDestination.Country_UID = obj.Country_UID;
            objDestination.CountryName = obj.CountryName;
            objDestination.State_UID = obj.State_UID;
            objDestination.StateName = obj.StateName;
            objDestination.City_UID = obj.City_UID;
            objDestination.CityName = obj.CityName;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.CreatedBy = obj.CreatedBy;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.ModifiedBy = obj.ModifiedBy;
            objDestination.Email = obj.Email;
        }
        #endregion

        #region PaymentMethodType
        public static contractsPayment.PaymentMethodType Convert(PaymentMethodType obj)
        {
            var newObj = new contractsPayment.PaymentMethodType();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(PaymentMethodType obj, contractsPayment.PaymentMethodType objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.PaymentType = obj.PaymentType;
            objDestination.Ordering = obj.Ordering;
            objDestination.IsBilled = obj.IsBilled;
            objDestination.Code = obj.Code;
            objDestination.AllowParcialPayments = obj.AllowParcialPayments;
        }
        #endregion

        #region Policies
        public static DepositPolicyQR1 Convert(DepositPolicy obj)
        {
            var newObj = new DepositPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(DepositPolicy obj, DepositPolicyQR1 objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.DepositPolicyName = obj.Name;
            objDestination.DepositPolicyDescription = obj.Description;
            objDestination.TranslatedDepositPolicyName = obj.TranslatedName;
            objDestination.TranslatedDepositPolicyDescription = obj.TranslatedDescription;
            objDestination.DepositPolicyDays = obj.Days.GetValueOrDefault();
            objDestination.IsDepositCostsAllowed = obj.IsDepositCostsAllowed;
            objDestination.DepositCosts = obj.DepositCosts;
            objDestination.Value = obj.Value;
            objDestination.PaymentModel = obj.PaymentModel;
            objDestination.NrNights = obj.NrNights;
            objDestination.RateUID = obj.RateUID;
            objDestination.SortOrder = obj.SortOrder;
        }

        public static CancellationPolicyQR1 Convert(CancellationPolicy obj)
        {
            var newObj = new CancellationPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(CancellationPolicy obj, CancellationPolicyQR1 objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.CancelPolicyName = obj.Name;
            objDestination.CancellationPolicy_Description = obj.Description;
            objDestination.TranslatedCancelPolicyName = obj.TranslatedName;
            objDestination.TranslatedCancellationPolicy_Description = obj.TranslatedDescription;
            objDestination.CancellationDays = obj.Days.GetValueOrDefault();
            objDestination.Property_UID = obj.Property_UID;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.IsCancellationAllowed = obj.IsCancellationAllowed.GetValueOrDefault();
            objDestination.CancellationCosts = obj.CancellationCosts ?? false;
            objDestination.Value = obj.Value;
            objDestination.PaymentModel = obj.PaymentModel;
            objDestination.NrNights = obj.NrNights;
            objDestination.RateUID = obj.RateUID;
            objDestination.SortOrder = obj.SortOrder;
        }

        public static OtherPolicyQR1 Convert(OtherPolicy obj)
        {
            var newObj = new OtherPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OtherPolicy obj, OtherPolicyQR1 objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.Name = obj.OtherPolicy_Name;
            objDestination.Description = obj.OtherPolicy_Description;
            objDestination.TranslatedName = obj.TranslatedName;
            objDestination.TranslatedDescription = obj.TranslatedDescription;
            objDestination.Language = obj.Language;
            objDestination.IsDelete = obj.IsDeleted;
            objDestination.IsSelected = obj.IsSelected;
        }
        #endregion

        #region User
        public static OB.Reservation.BL.Contracts.Data.General.User Convert(OB.BL.Contracts.Data.General.User obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.General.User();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.General.User obj, OB.Reservation.BL.Contracts.Data.General.User objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.FirstName = obj.FirstName;
            objDestination.LastName = obj.LastName;
            objDestination.Email = obj.Email;
            objDestination.Phone = obj.Phone;
            objDestination.PhoneExt = obj.PhoneExt;
            objDestination.UserName = obj.UserName;
            objDestination.Language_UID = obj.Language_UID;
            objDestination.IsActive = obj.IsActive;
            objDestination.Category_UID = obj.Category_UID;
            objDestination.PasswordExpiryDate = obj.PasswordExpiryDate;
            objDestination.CreateDate = obj.CreateDate;
            objDestination.Birthday = obj.Birthday;
            objDestination.Client_UID = obj.Client_UID;
            objDestination.LastLogin = obj.LastLogin;
            objDestination.IsLocked = obj.IsLocked;
            objDestination.CreateBy = obj.CreateBy;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifyBy = obj.ModifyBy;
            objDestination.ModifyDate = obj.ModifyDate;
            objDestination.UserSettings = obj.UserSettings;
            objDestination.RoleType = obj.RoleType;
            objDestination.IsDeleted = obj.IsDeleted;
            objDestination.IsDummyUser = obj.IsDummyUser;
        }
        #endregion

        #region TPICommission
        public static OB.Reservation.BL.Contracts.Data.CRM.TPICommission Convert(OB.BL.Contracts.Data.CRM.TPICommission obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.CRM.TPICommission();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.CRM.TPICommission obj, OB.Reservation.BL.Contracts.Data.CRM.TPICommission objDestination)
        {
            objDestination.CommissionIsPercentage = obj.CommissionIsPercentage;
            objDestination.PercentageCommission = obj.PercentageCommission;
        }
        #endregion

        #region ListRateRoomDetailsRequest
        public static Contracts.Requests.ListRateRoomDetailsRequest Convert(Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions obj, long propertyId)
        {
            var newObj = new Contracts.Requests.ListRateRoomDetailsRequest();

            Map(obj, newObj, propertyId);

            return newObj;
        }

        public static void Map(Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions obj, Contracts.Requests.ListRateRoomDetailsRequest objDestination, long propertyId)
        {
            objDestination.PropertyId = propertyId;
            objDestination.RrdFromDate = obj.CheckIn;
            objDestination.RrdToDate = obj.CheckOut;
            objDestination.RateRoomAssociation = new List<OB.BL.Contracts.Data.Rates.RateRoomAssociation>
            {
                new OB.BL.Contracts.Data.Rates.RateRoomAssociation
                {
                     RateId = obj.RateId,
                     RoomTypeId = obj.RoomTypeId
                }
            };
        }
        #endregion


        public static SetExpressCheckoutResponse Convert(Reservation.BL.Contracts.Data.Payments.SetExpressCheckoutResponse obj)
        {
            if (obj == null)
                return null;

            var newObj = new SetExpressCheckoutResponse();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(Reservation.BL.Contracts.Data.Payments.SetExpressCheckoutResponse obj, SetExpressCheckoutResponse objDestination)
        {
            objDestination.Token = obj.Token;
            if (obj.Errors != null && obj.Errors.Any())
                objDestination.Errors = obj.Errors.Select(x => Convert(x)).ToList();

        }

        public static PaypalError Convert(Reservation.BL.Contracts.Data.Payments.PaypalError obj)
        {
            if (obj == null)
                return null;

            var newObj = new PaypalError();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(Reservation.BL.Contracts.Data.Payments.PaypalError obj, PaypalError objDestination)
        {
            objDestination.ErrorCode = obj.ErrorCode;
            objDestination.LongMessage = obj.LongMessage;
        }
    }
}
