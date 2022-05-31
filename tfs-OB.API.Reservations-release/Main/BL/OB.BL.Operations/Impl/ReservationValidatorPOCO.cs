using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.TypeConverters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using contractsRates = OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Operations.Internal.BusinessObjects;
using System.Diagnostics.Contracts;
using Dapper;
using System.Data;
using OB.BL.Operations.Helper;
using OB.Domain.Reservations;
using OB.DL.Common.Extensions;
using OB.BL.Operations.Extensions;
using OB.DL.Common.QueryResultObjects;
using OB.BL.Operations.Internal.BusinessObjects.Enums;
using OB.BL.Operations.Internal.BusinessObjects.ValidationRequests;
using domainReservations = OB.Domain.Reservations;
using System.Data.SqlClient;
using Castle.Core.Internal;

namespace OB.BL.Operations.Impl
{
    internal partial class ReservationValidatorPOCO : BusinessPOCOBase, IReservationValidatorPOCO
    {

        public ReservationValidatorPOCO()
        {

        }

        /// <summary>
        /// Implementation of the ValidateReservation operation.
        /// Validate a Reservation into the Datastore given it's associated objects.
        /// </summary>
        /// <param>Need the flags and the object reservation to validate a reservation.</param>
        /// <returns>A ObservableCollection that contains the errors of validation.</returns>
        public ObservableCollection<Error> ValidateReservation(contractsReservations.Reservation reservation, ValidateReservationRequest request)
        {
            if (reservation == null)
                throw Errors.ReservationError.ToBusinessLayerException();

            var result = new ObservableCollection<Error>();

            #region Cancelation Costs

            if (request.ValidateCancelationCosts)
            {
                ValidateCancelationCosts(reservation);
            }

            #endregion

            #region Guarantee

            if (request.ValidateGuarantee)
            {
                result = ValidateGuarantee(reservation);
                if (result.Count > 0)
                    return result;
            }

            #endregion

            #region Restrictions
            //TODO: REMOVE THIS ASSIGNMENT AFTER CORRECT THE PROBLEMS ON THE SP (THIS CHANGES ARE ONLY ON STABLE AND RELEASE)
            if (!request.OnlyChangeGuestName && (false/*request.GroupRule != null*/ && request.GroupRule.BusinessRules.HasFlag(BusinessRules.ValidateRestrictions)))
            {
                ValidateRestrictions(reservation, request);
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Implementation of the ValidateReservation operation.
        /// Validate a Reservation into the Datastore given it's associated objects.
        /// </summary>
        /// <param>Need the flags and the object reservation to validate a reservation.</param>
        /// <returns>A ObservableCollection that contains the errors of validation.</returns>
        public void ValidateModifyReservation(OB.Domain.Reservations.Reservation newReservation, contractsReservations.Reservation oldReservation, ValidateReservationRequest request,
            List<contractsReservations.UpdateRoom> roomsToUpdate)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRoomChildRepo = RepositoryFactory.GetRepository<OB.Domain.Reservations.ReservationRoomChild>(unitOfWork);

                int j = 0;
                foreach (var room in roomsToUpdate)
                {
                    var tmpRoom = newReservation.ReservationRooms.FirstOrDefault(x => x.ReservationRoomNo == room.Number);

                    if (tmpRoom == null && !string.IsNullOrEmpty(room.Number))
                        throw Errors.InvalidRoom.ToBusinessLayerException();

                    if (tmpRoom == null)
                    {
                        // Add new Rooms
                        if (!room.Rate_UID.HasValue || !room.RoomType_UID.HasValue || !room.AdultCount.HasValue || room.AdultCount <= 0
                            || !room.DateFrom.HasValue || !room.DateTo.HasValue)
                            throw Errors.InvalidRoom.ToBusinessLayerException();

                        var roomNumber = newReservation.Number + "/" + (newReservation.ReservationRooms.Count() + 1);
                        var newRoom = new OB.Domain.Reservations.ReservationRoom
                        {
                            ReservationRoomNo = roomNumber,
                            Rate_UID = room.Rate_UID,
                            RoomType_UID = room.RoomType_UID,
                            UID = -1 * j
                        };

                        newReservation.ReservationRooms.Add(newRoom);
                        room.Number = roomNumber;
                        tmpRoom = newRoom;
                    }
                    else
                    {
                        if (tmpRoom.Status == (int)Constants.ReservationStatus.Cancelled
                            || tmpRoom.Status == (int)Constants.ReservationStatus.CancelledOnRequest)
                            throw Errors.RoomIsCancelledError.ToBusinessLayerException();
                    }

                    if (room.Guest != null)
                    {
                        request.OnlyChangeGuestName = true;
                        if (!string.IsNullOrEmpty(room.Guest.FirstName) || !string.IsNullOrEmpty(room.Guest.LastName))
                            tmpRoom.GuestName = room.Guest.FirstName + " " + room.Guest.LastName;
                    }

                    if ((room.RoomType_UID.HasValue && room.RoomType_UID != tmpRoom.RoomType_UID) || tmpRoom.UID < 0)
                    {
                        request.OnlyChangeGuestName = false;
                        request.ValidateAllotment = true;
                        tmpRoom.RoomType_UID = room.RoomType_UID;
                    }

                    if (room.AdultCount.HasValue && room.AdultCount != tmpRoom.AdultCount && room.AdultCount.Value > 0)
                    {
                        request.OnlyChangeGuestName = false;
                        tmpRoom.AdultCount = room.AdultCount;
                    }

                    #region CHILDREN
                    if (!room.ChildCount.HasValue)
                        room.ChildCount = 0;

                    if ((room.ChildCount > 0 && (room.ChildAges == null || !room.ChildAges.Any()))
                        || (room.ChildAges != null && room.ChildCount != room.ChildAges.Count))
                        throw Errors.ChildrenAgesMissing.ToBusinessLayerException();

                    if (room.ChildAges == null)
                        room.ChildAges = new List<int>();

                    List<int> currentChildAges = tmpRoom.ReservationRoomChilds != null ? tmpRoom.ReservationRoomChilds.Where(x => x.Age.HasValue).Select(x => x.Age.Value).ToList() : new List<int>();
                    if (room.ChildCount != tmpRoom.ChildCount || room.ChildAges.Except(currentChildAges).Any())
                    {
                        request.OnlyChangeGuestName = false;
                        tmpRoom.ChildCount = room.ChildCount;
                        room.ChildAges.Reverse();

                        if (room.ChildAges.Count == tmpRoom.ReservationRoomChilds.Count)
                        {
                            for (int i = 0; i < room.ChildAges.Count; i++)
                                tmpRoom.ReservationRoomChilds.ElementAt(i).Age = room.ChildAges[i];
                        }
                        else
                        {
                            reservationRoomChildRepo.Delete(tmpRoom.ReservationRoomChilds);
                            for (int i = 1; i <= room.ChildCount; i++)
                            {
                                tmpRoom.ReservationRoomChilds.Add(new OB.Domain.Reservations.ReservationRoomChild
                                {
                                    Age = room.ChildAges[i - 1],
                                    ChildNo = i,
                                    ReservationRoom_UID = tmpRoom.UID,
                                    UID = -i
                                });
                            }
                        }
                    }
                    #endregion CHILDREN

                    if ((room.DateFrom.HasValue && room.DateFrom.Value.Date != tmpRoom.DateFrom?.Date)
                        || room.DateTo.HasValue && (room.DateTo.Value.Date != tmpRoom.DateTo?.Date))
                    {
                        request.OnlyChangeGuestName = false;
                        request.ValidateAllotment = true;
                        tmpRoom.DateFrom = room.DateFrom;
                        tmpRoom.DateTo = room.DateTo;
                    }

                    if (!request.OnlyChangeGuestName)
                        request.RoomNumberToValidate.Add(tmpRoom.ReservationRoomNo);

                    j++;
                }

                if (!request.OnlyChangeGuestName)
                {
                    // Validate Cancelation Costs
                    ValidateCancelationCosts(oldReservation);

                    // Validate Deposit Costs
                    ValidateDepositCosts(oldReservation, request.ReservationAdditionalData);
                }
                else
                {
                    request.ValidateAllotment = false;
                    request.ValidateCancelationCosts = false;
                    request.ValidateGuarantee = false;
                }

                // Create contracts reservation for validations
                var contractReservation = DomainToBusinessObjectTypeConverter.Convert(newReservation, new ListReservationRequest
                {
                    IncludeReservationRoomChilds = true,
                    IncludeReservationRoomDetails = true,
                    IncludeReservationRoomDetailsAppliedIncentives = true,
                    IncludeReservationRoomExtras = true,
                    IncludeReservationRooms = true,
                    IncludeReservationRoomTaxPolicies = true
                });

                request.ValidateCancelationCosts = false;
                request.IsModifyReservation = true;

                bool validateAllotment = request.ValidateAllotment;
                request.ValidateAllotment = false; // set validateAllotment to false because cannot be validated on SP
                ValidateReservation(contractReservation, request);
                request.ValidateAllotment = validateAllotment;
            }
        }

        #region VALIDATE RESTRICTIONS

        /// <summary>
        /// Validate
        /// </summary>
        /// <param name="reservation">the reservation</param>
        /// <para>No allotment available Error = <b>-500</b></para> 
        /// <para>Channel is not on property Error = <b>-501</b></para> 
        /// <para>Channel is not on rate Error = <b>-502</b></para> 
        /// <para>Property channel mapping not has this combination Error = <b>-503</b></para> 
        /// <para>Invalid Agency Error = <b>-504</b></para> 
        /// <para>Invalid company code Error = <b>-505</b></para> 
        /// <para>Missig days in date range Error = <b>-506</b></para> 
        /// <para>Worng currencie Error = <b>-507</b></para> 
        /// <para>Bortype type is different in request Error = <b>-509</b></para> 
        /// <para>No occupation available Error = <b>-510</b></para> 
        /// <para>Max Occupancy Exceeded in one or more rooms Error = <b>-511</b></para> 
        /// <para>Max Adult Exceeded in one or more rooms Error = <b>-512</b></para> 
        /// <para>Max child Exceeded in one or more rooms Error = <b>-513</b></para> 
        /// <para>One or more selected days are closed Error = <b>-514</b></para>
        /// <para>Minimum Length Of Stay Restriction Error = <b>-515</b></para>
        /// <para>Maximum Length Of Stay Restriction Error = <b>-516</b></para>
        /// <para>StayTrought Restriction Error = <b>-517</b></para>
        /// <para>Realease days Restriction Error = <b>-518</b></para>
        /// <para>Close On Arrival Restriction Error = <b>-519</b></para>
        /// <para>Close On Departure Restriction Error = <b>-520</b></para>
        /// <para>Rate is not for selling Error = <b>-523</b></para>
        /// <para>PaymentMehtod not allowed Error = <b>-524</b></para>
        /// <para>Invalid Group code Error = <b>-526</b></para>
        /// <para>Rate is exculisive for groupcode Error = <b>-527</b></para>
        /// <para>Rate is exculisive for promo code Error = <b>-528</b></para>
        /// <returns></returns>
        public void ValidateRestrictions(contractsReservations.Reservation reservation, ValidateReservationRequest request)
        {
            var result = new ObservableCollection<Error>();
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {                
                var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                var reservationPricesPOCO = this.Resolve<IReservationPricesCalculationPOCO>();

            //Get Child Terms
            if (request.ChildTerms == null || !request.ChildTerms.Any())
            {
                request.ChildTerms = new List<ChildTerm>();
                var childRepo = this.RepositoryFactory.GetOBChildTermsRepository();
                var childTermsResponse = childRepo.ListChildTerms(new Contracts.Requests.ListChildTermsRequest
                {
                    PropertyUIDs = new List<long>() { reservation.Property_UID },
                    IncludeChildTermCurrencies = true
                });
            }
            List<ChildTerm> propertyChildTerms = request.ChildTerms;

            foreach (var room in reservation.ReservationRooms)
            {
                if (request.IsModifyReservation && !request.RoomNumberToValidate.Contains(room.ReservationRoomNo))
                    continue;

                    int childCountForOccupancy = 0;
                    int adultCount = room.AdultCount ?? 0;
                    int adultChildsCount = 0;
                    var ages = room.ReservationRoomChilds?.Where(x => x.Age.HasValue).Select(x => x.Age.Value).ToList();
                    var childTermsForSearch = reservationPricesPOCO.GetGuestCountAfterApplyChildTerms(reservation.ReservationBaseCurrency_UID,
                            propertyChildTerms, room.ChildCount, ages);

                    if (childTermsForSearch != null && childTermsForSearch.Any())
                    {
                        adultChildsCount = childTermsForSearch.Where(c => c.PriceType == ChildPriceType.Adult && c.IsAccountableForOccupancy).Sum(c => c.NumberOfChilds);
                        childCountForOccupancy = childTermsForSearch.Where(c => c.IsAccountableForOccupancy).Sum(c => c.NumberOfChilds);
                    }

                    childCountForOccupancy = childCountForOccupancy - adultChildsCount;
                    adultCount = adultCount + adultChildsCount;

                    var error = reservationsRepo.ValidateReservation(new DL.Common.Criteria.ValidateReservationCriteria
                    {
                        DateFrom = room.DateFrom,
                        DateTo = room.DateTo,
                        Channel_UID = reservation.Channel_UID,
                        Property_UID = reservation.Property_UID,
                        Rate_UID = room.Rate_UID,
                        RoomType_UID = room.RoomType_UID,
                        NumAdults = adultCount,
                        NumChilds = childCountForOccupancy,
                        ChildAges = null,
                        PCC = null,
                        CompanyCode = null,
                        Currency = null,
                        BoardType_UID = null,
                        NumberOfRooms = 1,
                        HigherDayPrice = null,
                        PaymentMethodType = reservation.PaymentMethodType_UID,
                        GroupCode = reservation.GroupCode_UID,
                        PromoCode = reservation.PromotionalCode_UID,
                        ValidateAllotment = request.ValidateAllotment,
                        Tpi_UID = reservation.TPI_UID
                    });

                    if (error < 0)
                    {
                        BuildError(ref result, error);
                    }
                }
            }
        }

        #endregion

        #region VALIDATE GUARANTEE

        /// <summary>
        /// Validate guarantee through reservation object.
        /// </summary>
        /// <param>Need the object reservation to validate guarantee.</param>
        /// <returns>A ObservableCollection that contains the errors of validation.
        /// 
        /// Guarantee Error code:
        /// <para/>Invalid Credit Card error= -4000 
        /// <para/>Error must occur when CardNumber field is empty= -4100 
        /// <para/>Invalid guarantee type, DepositInformation is empty= -4101
        /// <para/>Guarantee type is not selected in OB= -4102
        /// <para/>Error must occur when PaymentMethodType_UID field is null= -4103
        /// <para/>ReservationError - Reservation detail must be provided= -4104
        /// <para/>
        /// </returns>
        private ObservableCollection<Error> ValidateGuarantee(contractsReservations.Reservation reservation)
        {
            var result = new ObservableCollection<Error>();

            #region Validate required fields

            //Validate if deposit information is fullfield
            //Validate if credit card data is passed inf Guarantee Type is credit card
            foreach (contractsReservations.ReservationRoom room in reservation.ReservationRooms)
            {
                if (!string.IsNullOrEmpty(room.DeposityGuaranteeType) && string.IsNullOrEmpty(room.DepositInformation))
                {
                    #region CREDIT CARD GUARANTEE

                    if (room.DeposityGuaranteeType == ((int)Constants.GuaranteeType.CreditCard).ToString())
                    {
                        if (reservation.PaymentMethodType_UID.HasValue)
                        {
                            var objReservationPaymentDetail = reservation.ReservationPaymentDetail;
                            if (objReservationPaymentDetail != null && !string.IsNullOrEmpty(objReservationPaymentDetail.CardNumber))
                            {
                                // Check if Credit card number is valid
                                var securityRepo = this.RepositoryFactory.GetOBSecurityRepository();
                                string cardNumber = securityRepo.DecryptCreditCards(
                                        new Contracts.Requests.ListCreditCardRequest { CreditCards = new List<string> { objReservationPaymentDetail.CardNumber } })
                                        .FirstOrDefault();

                                if (!Helper.ValidationHelper.CardCheckDigit(cardNumber)
                                && !Helper.ValidationHelper.CardCheck(cardNumber, objReservationPaymentDetail.PaymentMethod_UID))
                                {
                                    BuildError(out result, Contracts.Responses.ErrorType.InvalidCreditCard, "Error making payment on gateway - Error Code = -4000", -4000);
                                    break;
                                }
                            }
                            else
                            {
                                BuildError(out result, Contracts.Responses.ErrorType.InvalidCreditCard, "Error must occur when CardNumber field is empty - Error Code = -4100", -4100);
                                break;
                            }
                        }
                        else
                        {
                            result.Add(Errors.InvalidPaymentMethod.ToContractError());
                            break;
                        }
                    }

                    #endregion

                    #region OTHERS GUARANTEES

                    else
                    {
                        result.Add(Errors.GuaranteeTypeInformationError.ToContractError());
                        break;
                    }

                    #endregion
                }
            }
            #endregion

            if (!GuaranteeTypeIsSelected(reservation))
            {
                result.Add(Errors.GuaranteeTypeNotSelectedError.ToContractError());
                return result;
            }

            return result;
        }

        /// <summary>
        /// Validate if guarantee is selected.
        /// </summary>
        /// <param>Need ID deposit policy and typecode of guarantee to validate.</param>
        /// <returns>A boolean with result of validation, true is selected, false is not selected.</returns>
        private bool GuaranteeTypeIsSelected(contractsReservations.Reservation reservation)
        {
            var depositRepo = this.RepositoryFactory.GetOBDepositPolicyRepository();
            var depositPoliciesGarantee = new Dictionary<long, long>();

            foreach (var room in reservation.ReservationRooms)
            {
                if (room.DepositPolicy_UID == 0)
                    continue;

                long guaranteeTypeCode = 0;
                if (!string.IsNullOrEmpty(room.DeposityGuaranteeType))
                {
                    if (!long.TryParse(room.DeposityGuaranteeType, out guaranteeTypeCode))
                    {
                        return false;
                    }
                    else if (!depositPoliciesGarantee.ContainsKey(room.DepositPolicy_UID))
                        depositPoliciesGarantee.Add(room.DepositPolicy_UID, guaranteeTypeCode);
                }
            }

            if (!depositPoliciesGarantee.Any())
                return true;

            var lstActiveDepositGuaranteesTypeIds = depositRepo.ListDepositPoliciesGuaranteeTypes(
                new Contracts.Requests.ListDepositPoliciesGuaranteeTypesRequest { DepositPoliciesIDs = depositPoliciesGarantee.Keys.ToList(), IsActive = true })
                .Select(x => x.GuaranteeType_UID).ToList();

            var guaranteesTypes = depositPoliciesGarantee.Select(x => x.Value).ToList();
            var lstGuaranteeTypesIds = depositRepo.ListGuaranteeTypesFilter(new Contracts.Requests.ListGuaranteeTypesFilterRequest { TypeCodes = guaranteesTypes }).Select(x => x.GuaranteeType_UID).ToList();

            if (lstActiveDepositGuaranteesTypeIds == null || !lstActiveDepositGuaranteesTypeIds.Any())
                return true;
            else
            {
                if (!lstGuaranteeTypesIds.Any() || lstGuaranteeTypesIds.Except(lstActiveDepositGuaranteesTypeIds).Any())
                    return false;
                else
                    return true;
            }
        }

        #endregion

        #region VALIDATE CANCELATION COSTS

        /// <summary>
        /// Validate Cancelation Costs
        /// if costs are applied an error is returned
        /// </summary>
        /// <param name="reservation">the reservation</param>
        /// <para>One or more Rooms have cancelation costs applied Error = <b>-544</b></para> 
        /// <returns></returns>
        private void ValidateCancelationCosts(contractsReservations.Reservation reservation)
        {
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            var result = new ObservableCollection<Error>();
            var costs = reservationHelper.GetCancelationCosts(reservation);
            if (costs.Any() && costs.Sum(x => x.CancelationCosts) > 0)
                throw Errors.CancelationCostsAppliedError.ToBusinessLayerException();
        }

        /// <summary>
        /// Validate Deposit Costs
        /// if costs are applied an error is returned
        /// </summary>
        /// <param name="reservation">the reservation</param>
        /// <para>One or more Rooms have deposit costs applied Error = <b>-544</b></para> 
        /// <returns></returns>
        private void ValidateDepositCosts(contractsReservations.Reservation reservation, Domain.Reservations.ReservationsAdditionalData additionalData)
        {
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            var result = new ObservableCollection<Error>();
            var costs = reservationHelper.GetDepositCosts(reservation, additionalData);
            if (costs.Any() && costs.Sum(x => x.DepositCosts) > 0)
                throw Errors.DepositCostsAppliedError.ToBusinessLayerException();
        }

        #endregion

        #region VALIDATE RESERVATION RESTRICTIONS

        public bool ValidateReservationRestrictions(OB.BL.Contracts.Data.Rates.RateRoomDetail rateRoomDetail, DateTime checkIn, DateTime checkOut, long channelId)
        {
            if (rateRoomDetail.MinimumLengthOfStay.HasValue
                && !ValidateMinimumLengthOfStay(checkIn, checkOut, rateRoomDetail.MinimumLengthOfStay.Value))
                return false;

            if (rateRoomDetail.MaximumLengthOfStay.HasValue
               && !ValidateMaximumLengthOfStay(checkIn, checkOut, rateRoomDetail.MaximumLengthOfStay.Value))
                return false;

            if (rateRoomDetail.StayThrough.HasValue
              && !ValidateStayThrough(checkIn, checkOut, rateRoomDetail.StayThrough.Value))
                return false;

            if (rateRoomDetail.ClosedOnArrival.HasValue
             && !ValidateClosedOnArrival(checkIn, rateRoomDetail.Date, rateRoomDetail.ClosedOnArrival.Value))
                return false;

            if (rateRoomDetail.ClosedOnDeparture.HasValue
             && !ValidateClosedOnDeparture(checkOut, rateRoomDetail.Date, rateRoomDetail.ClosedOnDeparture.Value))
                return false;

            if (!rateRoomDetail.BlockedChannelsListIds.IsNullOrEmpty()
            && !ValidateBlockedChannels(rateRoomDetail.BlockedChannelsListIds, channelId))
                return false;

            return true;
        }

        /// <summary>
        /// Total days between dateFrom and dateTo cannot be less than minimumLengthOfStay
        /// </summary>
        /// <param name="datefrom">checkin date</param>
        /// <param name="dateTo">checkout date</param>
        /// <param name="minimumLengthOfStay">minimum Length Of Stay</param>
        /// <returns>Boolean</returns>
        public bool ValidateMinimumLengthOfStay(DateTime datefrom, DateTime dateTo, int minimumLengthOfStay)
        {
            var totalDays = (dateTo - datefrom).Days;

            return minimumLengthOfStay <= totalDays;
        }

        /// <summary>
        /// Total days between dateFrom and dateTo cannot be bigger than maximumLengthOfStay
        /// </summary>
        /// <param name="datefrom">checkin date</param>
        /// <param name="dateTo">checkout date</param>
        /// <param name="maximumLengthOfStay">maximum Length Of Stay</param>
        /// <returns>Boolean</returns>
        public bool ValidateMaximumLengthOfStay(DateTime datefrom, DateTime dateTo, int maximumLengthOfStay)
        {
            var totalDays = (dateTo - datefrom).Days;

            return maximumLengthOfStay >= totalDays;
        }

        /// <summary>
        /// Total days between dateFrom and dateTo must be equal stayThrough
        /// </summary>
        /// <param name="datefrom">checkin date</param>
        /// <param name="dateTo">checkout date</param>
        /// <param name="stayThrough">stay Through</param>
        /// <returns>Boolean</returns>
        public bool ValidateStayThrough(DateTime datefrom, DateTime dateTo, int stayThrough)
        {
            var totalDays = (dateTo - datefrom).Days;

            return stayThrough == totalDays;
        }

        /// <summary>
        /// CheckIn day cannot be equal the dateRateRoom when closedOnArrival = true
        /// </summary>
        /// <param name="datefrom">Checkin date</param>
        /// <param name="dateRateRoom">RateRoom date</param>
        /// <param name="closedOnArrival">closeOnarrivel</param>
        /// <returns>boolean</returns>
        public bool ValidateClosedOnArrival(DateTime datefrom, DateTime dateRateRoom, bool closedOnArrival)
        {
            return !(closedOnArrival && dateRateRoom == datefrom);
        }

        /// <summary>
        /// Checkout day cannot be equal the dateRateRoom when closedOnDeparture = true
        /// </summary>
        /// <param name="dateTo">Checkout date</param>
        /// <param name="dateRateRoom">RateRoom date</param>
        /// <param name="closedOnDeparture">closedOnDeparture</param>
        /// <returns>boolean</returns>
        public bool ValidateClosedOnDeparture(DateTime dateTo, DateTime dateRateRoom, bool closedOnDeparture)
        {
            return !(closedOnDeparture && dateRateRoom == dateTo);
        }

        /// <summary>
        /// Channel ID must be not on blocked channel list
        /// </summary>
        /// <param name="blockedChannelsListIds"> blocked channels ids</param>
        /// <param name="channelId">channel id</param>
        /// <returns>boolean</returns>
        public bool ValidateBlockedChannels(List<long> blockedChannelsListIds, long channelId)
        {
            return !(blockedChannelsListIds.Contains(channelId));
        }

        #endregion

        #region LOYALTY LEVELS

        /// <summary>
        /// Calculates the guest past reservations values like count reservations, count total value of reservation and total room/nights.
        /// </summary>
        /// <returns></returns>
        public AggregatedGuestPastReservationsValues CalculateGuestPastReservationsValues(long guestId, int periodicityLimitType, int periodicityLimitValue, long loyaltyBaseCurrencyUID)
        {
            var response = new AggregatedGuestPastReservationsValues();
            var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            var startDate = DateTime.UtcNow;
            var endDate = startDate;

            switch (periodicityLimitType)
            {
                case (int)LoyaltyLevelLimitsPeriodicityEnum.Annual:
                    startDate = startDate.AddYears(-1 * periodicityLimitValue); // Subtract Years
                    break;
                case (int)LoyaltyLevelLimitsPeriodicityEnum.Monthly:
                    startDate = startDate.AddMonths(-1 * periodicityLimitValue); // Subtract Months
                    break;
            }

            // Get all guests reservation in UTC
            var guestReservationsRooms = reservationHelper.GetGuestsReservationRoomsWithLoyaltyDiscount(guestId, startDate, endDate);

            // Filter by hotel timezone
            guestReservationsRooms = guestReservationsRooms.Where(x =>
                        propertyRepo.ConvertToTimeZone(x.CreatedDate, x.TimeZone_UID) > propertyRepo.ConvertToTimeZone(startDate, x.TimeZone_UID)
                        && propertyRepo.ConvertToTimeZone(x.CreatedDate, x.TimeZone_UID) < propertyRepo.ConvertToTimeZone(endDate, x.TimeZone_UID)).ToList();

            var groupByReservation = guestReservationsRooms.GroupBy(x => x.ReservationId);

            // Get exchange rates
            var currenciesExchangeRates = new Dictionary<long, decimal>();
            var currencies = guestReservationsRooms.Select(x =>
                new
                {
                    CurrencyId = x.BaseCurrency_UID,
                    PropertyId = x.PropertyId
                }).Distinct().ToList();
            foreach (var item in currencies)
            {
                // Get Exchange rate from Hotel Currency to loyalty currency
                if (!currenciesExchangeRates.ContainsKey(item.CurrencyId))
                    currenciesExchangeRates.Add(item.CurrencyId, reservationHelper.GetExchangeRateBetweenCurrenciesByPropertyId(item.CurrencyId, loyaltyBaseCurrencyUID, item.PropertyId));
            }

            response.ReservationsCount = groupByReservation.Count(); // Count Reservations
            response.RoomNightsCount = groupByReservation.Sum(x => (x.Sum(y => (y.DateTo - y.DateFrom).Days))); // Count Rooms Nights
            response.ReservationsTotalAmount = groupByReservation.Sum(x => x.First().TotalAmount / currenciesExchangeRates[x.First().BaseCurrency_UID]); // Calculate total of reservations in base currency of property

            return response;
        }

        /// <summary>
        /// Validade Guest Loyalty Discount per reservation room
        /// </summary>
        /// <param name="guestId"></param>
        /// <param name="reservationRooms"></param>
        /// <returns>dictionary where key is roomNumber and value is a boolean indication if reservation room is valid for loyalty discount</returns>
        public bool HaveGuestExceededLoyaltyDiscount(long guestId)
        {
            var guestRepo = RepositoryFactory.GetOBCRMRepository();
            var result = false;

            var guest =
                guestRepo.ListGuestsByLightCriteria(new Contracts.Requests.ListGuestLightRequest
                {
                    UIDs = new List<long>() { guestId }
                }).FirstOrDefault();
            var guestLoyaltyId = guest != null ? guest.LoyaltyLevel_UID : null;

            if (!guestLoyaltyId.HasValue)
                throw Errors.GuestLoyaltyLevelIsDiferentFromRegistered.ToBusinessLayerException();

            var crmRepo = RepositoryFactory.GetOBCRMRepository();

            var loyaltyProgram =
                crmRepo.ListLoyaltyPrograms(new Contracts.Requests.ListLoyaltyProgramRequest
                {
                    IncludeLoyaltyLevels = true,
                    LoyaltyLevel_Uids = new List<long>() { guestLoyaltyId.Value }
                }).Results.FirstOrDefault();
            var loyaltyLevel = loyaltyProgram != null
                ? loyaltyProgram.LoyaltyLevels.Select(
                    x => new { LoyaltyLevel = x, PropertyId = loyaltyProgram.CreatedByPropertyUID }).FirstOrDefault()
                : null;

            if (loyaltyLevel == null)
                throw Errors.GuestLoyaltyLevelIsDiferentFromRegistered.ToBusinessLayerException();

            long? propertyCurrencyId = null;
            var currencyRepo = RepositoryFactory.GetOBCurrencyRepository();
            var dictCurrencies = currencyRepo.ListPropertyBaseCurrencyByPropertyUID(new Contracts.Requests.ListPropertyBaseCurrencyByPropertyUIDRequest
            {
                PropertyUIDs = new List<long> { loyaltyLevel.PropertyId }
            });

            if (dictCurrencies.ContainsKey(loyaltyLevel.PropertyId))
                propertyCurrencyId = dictCurrencies[loyaltyLevel.PropertyId].UID;

            var criteria = new LoyaltyLevelValidationCriteria
            {
                LoyaltyLevelLimitsPeriodicityValue = loyaltyLevel.LoyaltyLevel.LoyaltyLevelLimitsPeriodicityValue,
                Guest_UID = guestId,
                IsForNumberOfReservationsActive = loyaltyLevel.LoyaltyLevel.IsForNumberOfReservationsActive,
                IsForNumberOfReservationsValue = loyaltyLevel.LoyaltyLevel.IsForNumberOfReservationsValue,
                IsForNightsRoomActive = loyaltyLevel.LoyaltyLevel.IsForNightsRoomActive,
                IsForNightsRoomValue = loyaltyLevel.LoyaltyLevel.IsForNightsRoomValue,
                IsForTotalReservationsActive = loyaltyLevel.LoyaltyLevel.IsForTotalReservationsActive,
                LoyaltyLevelBaseCurrency_UID = propertyCurrencyId,
                IsForTotalReservationsValue = loyaltyLevel.LoyaltyLevel.IsForTotalReservationsValue,
                LoyaltyLevel_UID = loyaltyLevel.LoyaltyLevel.UID,
                IsForReservationValue = loyaltyLevel.LoyaltyLevel.IsForReservationValue,
                IsForReservationRoomNightsValue = loyaltyLevel.LoyaltyLevel.IsForReservationRoomNightsValue,
                IsForReservationActive = false,
                IsLimitsForPeriodicityActive = loyaltyLevel.LoyaltyLevel.IsLimitsForPeriodicityActive,
                LoyaltyLevelLimitsPeriodicity_UID = loyaltyLevel.LoyaltyLevel.LoyaltyLevelLimitsPeriodicity_UID
            };

            // Validade limits to all guest reservation
            try
            {
                ValidateGuestLoyaltyLevel(criteria);
            }
            catch (BusinessLayerException ex)
            {
                if (ex.ErrorCode == (int)Errors.GuestLoyaltyLevelTotalReservationValueExcedded
                    || ex.ErrorCode == (int)Errors.GuestLoyaltyLevelNumberOfReservationRoomNightExcedded
                    || ex.ErrorCode == (int)Errors.GuestLoyaltyLevelNumberOfReservationsExcedded)
                    result = true;
                else
                    throw;
            }

            return result;
        }

        /// <summary>
        /// Validade Guest Loyalty Discount per reservation
        /// </summary>
        /// <param name="reservationContext"></param>
        /// <param name="guest"></param>
        /// <param name="reservation"></param>
        /// <param name="rooms"></param>
        public bool ValidateGuestLoyaltyLevel(LoyaltyLevelValidationCriteria parameters)
        {
            ValidateCriteriaParameters(parameters);

            // Validade Limits per reservation
            if (parameters.IsForReservationActive)
            {
                if (!parameters.RoomsToValidate.Any())
                    return true;

                if (parameters.PropertyId == null)
                    throw Errors.PropertyIdIsRequired.ToBusinessLayerException();

                if (parameters.PropertyCurrencyId == null)
                    throw Errors.PropertyCurrencyIsRequired.ToBusinessLayerException();

                if (parameters.RoomsToValidate.Any(x => x.LoyaltyLevel_UID != parameters.LoyaltyLevel_UID))
                    throw Errors.GuestLoyaltyLevelIsDiferentFromRegistered.ToBusinessLayerException();

                ValidateGuestLoyaltyLevelCurrentReservation(parameters);
            }

            // Validade limits to all guest reservation
            if (parameters.IsLimitsForPeriodicityActive)
                ValidateGuestLoyaltyLevelForPeriod(parameters);

            return true;
        }

        private bool ValidateGuestLoyaltyLevelCurrentReservation(LoyaltyLevelValidationCriteria validationCriteria)
        {
            var reservationHelper = Resolve<IReservationHelperPOCO>();

            var roomsToValidate = validationCriteria.RoomsToValidate;

            // validate reservation room nights
            if (roomsToValidate.Any(x => !x.CheckIn.HasValue || !x.CheckOut.HasValue))
                throw Errors.GuestLoyaltyLevelCheckInAndCheckOutMustBeDefined.ToBusinessLayerException();

            // Get Exchange rate from loyalty currency to Hotel Currency
            var loyaltyLevelExchangeRate = reservationHelper.GetExchangeRateBetweenCurrenciesByPropertyId(validationCriteria.LoyaltyLevelBaseCurrency_UID.Value,
                    validationCriteria.PropertyCurrencyId.Value, validationCriteria.PropertyId.Value);

            // validate reservation total amount
            var reservationTotalAmountToPropertyCurrency = (roomsToValidate.Sum(x => x.TotalAmount) ?? 0);
            var limitValueToPropertyCurrency = validationCriteria.IsForReservationValue / loyaltyLevelExchangeRate;

            if (reservationTotalAmountToPropertyCurrency > limitValueToPropertyCurrency)
                throw Errors.GuestLoyaltyLevelTotalReservationValueExcedded.ToBusinessLayerException();

            // Validate room night
            if (roomsToValidate.Any(x => (x.CheckOut.Value - x.CheckIn.Value).Days > validationCriteria.IsForReservationRoomNightsValue))
                throw Errors.GuestLoyaltyLevelNumberOfReservationRoomNightExcedded.ToBusinessLayerException();

            return true;
        }

        private bool ValidateGuestLoyaltyLevelForPeriod(LoyaltyLevelValidationCriteria validationCriteria)
        {
            if (!validationCriteria.IsLimitsForPeriodicityActive)
                return true;

            var pastReservationsValues = this.CalculateGuestPastReservationsValues(validationCriteria.Guest_UID.Value, validationCriteria.LoyaltyLevelLimitsPeriodicity_UID.Value,
                validationCriteria.LoyaltyLevelLimitsPeriodicityValue.Value, validationCriteria.LoyaltyLevelBaseCurrency_UID.Value);

            // Validade Number of Reservations
            if (validationCriteria.IsForNumberOfReservationsActive)
            {
                // Sum Current Reservation to past reservations
                var numberOfReservations = pastReservationsValues.ReservationsCount + 1;
                if (numberOfReservations > validationCriteria.IsForNumberOfReservationsValue)
                    throw Errors.GuestLoyaltyLevelNumberOfReservationsExcedded.ToBusinessLayerException();
            }

            // Validade Room Nights
            if (validationCriteria.IsForNightsRoomActive)
            {
                // Sum Current Reservation rooms/night to past reservations
                var numberOfRoomNights = pastReservationsValues.RoomNightsCount + validationCriteria.RoomsToValidate.Sum(x => (x.CheckOut.Value - x.CheckIn.Value).Days);
                if (numberOfRoomNights > validationCriteria.IsForNightsRoomValue)
                    throw Errors.GuestLoyaltyLevelNumberOfReservationRoomNightExcedded.ToBusinessLayerException();
            }

            // Validade by total reservations value
            if (validationCriteria.IsForTotalReservationsActive)
            {
                // Sum Current Reservation Total Amount to past reservations
                var reservationTotalValueInLoyaltyCurrency = pastReservationsValues.ReservationsTotalAmount + validationCriteria.RoomsToValidate.Sum(x => x.TotalAmount);

                if (reservationTotalValueInLoyaltyCurrency > validationCriteria.IsForTotalReservationsValue)
                    throw Errors.GuestLoyaltyLevelTotalReservationValueExcedded.ToBusinessLayerException();
            }

            return true;
        }

        private void ValidateCriteriaParameters(LoyaltyLevelValidationCriteria validationCriteria)
        {
            if (!validationCriteria.Guest_UID.HasValue)
                throw Errors.GuestIdIsRequired.ToBusinessLayerException();

            if (!validationCriteria.LoyaltyLevel_UID.HasValue)
                throw Errors.LoyaltyLevelIdIsRequired.ToBusinessLayerException();

            if (!validationCriteria.LoyaltyLevelBaseCurrency_UID.HasValue)
                throw Errors.GuestLoyaltyLevelLoyaltyLevelBaseCurrencyIsRequired.ToBusinessLayerException();
        }

        #endregion

        private void BuildError(ref ObservableCollection<Error> result, int errorCode)
        {
            switch (errorCode)
            {
                case -500:
                    throw Errors.AllotmentNotAvailable.ToBusinessLayerException();
                case -501:
                    throw Errors.InvalidPropertyChannel.ToBusinessLayerException();
                case -502:
                    throw Errors.InvalidRateChannel.ToBusinessLayerException();
                case -503:
                    throw Errors.InvalidPropertyChannelMapping.ToBusinessLayerException();
                case -504:
                    throw Errors.InvalidAgency.ToBusinessLayerException();
                case -505:
                    throw Errors.InvalidCompanyCode.ToBusinessLayerException();
                case -506:
                    throw Errors.DateRangeDaysError.ToBusinessLayerException();
                case -507:
                    throw Errors.InvalidCurrency.ToBusinessLayerException();
                case -509:
                    throw Errors.InvalidBoardType.ToBusinessLayerException();
                case -510:
                    throw Errors.OccupancyNotAvailable.ToBusinessLayerException();
                case -511:
                    throw Errors.MaxOccupancyExceeded.ToBusinessLayerException();
                case -512:
                    throw Errors.MaxAdultsExceeded.ToBusinessLayerException();
                case -513:
                    throw Errors.MaxChildrenExceeded.ToBusinessLayerException();
                case -514:
                    throw Errors.ClosedDayRestrictionError.ToBusinessLayerException();
                case -515:
                    throw Errors.MinimumLengthOfStayRestrictionError.ToBusinessLayerException();
                case -516:
                    throw Errors.MaximumLengthOfStayRestrictionError.ToBusinessLayerException();
                case -517:
                    throw Errors.StayTroughtRestrictionError.ToBusinessLayerException();
                case -518:
                    throw Errors.ReleaseDaysRestrictionError.ToBusinessLayerException();
                case -519:
                    throw Errors.ClosedOnArrivalRestrictionError.ToBusinessLayerException();
                case -520:
                    throw Errors.ClosedOnDepartureRestrictionError.ToBusinessLayerException();
                case -523:
                    throw Errors.RateIsNotForSale.ToBusinessLayerException();
                case -524:
                    throw Errors.InvalidPaymentMethod.ToBusinessLayerException();
                case -526:
                    throw Errors.InvalidGroupCode.ToBusinessLayerException();
                case -527:
                    throw Errors.RateIsExclusiveForGroupCodes.ToBusinessLayerException();
                case -528:
                    throw Errors.RateIsExclusiveForPromoCodes.ToBusinessLayerException();
                case -535:
                    throw Errors.InvalidRate.ToBusinessLayerException();
                case -536:
                    throw Errors.InvalidCheckIn.ToBusinessLayerException();
                default:
                    break;
            }
        }

        private void BuildError(out ObservableCollection<Error> result, string errorType, string errorDescription, int errorCode)
        {
            result = new ObservableCollection<Error>();

            var error = new OB.Reservation.BL.Contracts.Responses.Error();
            error.ErrorType = errorType;
            error.Description = errorDescription;
            error.ErrorCode = errorCode;

            result.Add(error);
        }
    }
}
