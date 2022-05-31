using Dapper;
using Hangfire;
using Hangfire.States;
using OB.Api.Core;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Internal.LogHelper;
using OB.BL.Operations.Internal.TypeConverters;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common.Extensions;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.Services.Jobs.Operations;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Configuration;
using static OB.Reservation.BL.Constants;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;
using contractsGeneral = OB.Reservation.BL.Contracts.Data.General;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;
using OBcontractsRates = OB.BL.Contracts.Data.Rates;

namespace OB.BL.Operations.Impl
{
    /// <summary>
    /// BusinessPOCO class that implements the Admin operations.
    /// </summary>
    public partial class ReservationHelperPOCO : BusinessPOCOBase, IReservationHelperPOCO
    {
        private const string MissedTranlation = "This information does not exist in your language...!";

        #region POLICIES

        /// <summary>
        /// Calculate cancelation cost for each room of the reservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public List<contractsReservations.ReservationRoomCancelationCost> GetCancelationCosts(contractsReservations.Reservation reservation, bool convertToRateCurrency = true, DateTime? cancellationDate = null)
        {
            var roomsCost = new List<contractsReservations.ReservationRoomCancelationCost>();
            if (reservation != null)
            {
                decimal exchangeRate = convertToRateCurrency ? 1 / (reservation.PropertyBaseCurrencyExchangeRate ?? ((decimal)1)) : 1;
                var utcNowDate = DateTime.UtcNow.Date;
                foreach (var item in reservation.ReservationRooms)
                {
                    bool notApply = false;
                    bool applyCancelationRules = true;
                    decimal tempCost = 0;

                    if (item.CancellationPolicyDays != null && item.IsCancellationAllowed == true)
                    {
                        // Impossible to calculate costs for this room
                        if (item.Status == (int)Reservation.BL.Constants.ReservationStatus.Cancelled && !item.CancellationDate.HasValue)
                        {
                            notApply = true;
                            applyCancelationRules = false;
                        }
                        else
                        {
                            if (cancellationDate == null)
                            {
                                cancellationDate = item.CancellationDate ?? utcNowDate;
                            }

                            var date = item.DateFrom.Value.Date.AddDays(-item.CancellationPolicyDays.Value);
                            if (cancellationDate < date)
                                applyCancelationRules = false;
                        }
                    }

                    if (applyCancelationRules)
                    {
                        if (!item.CancellationPaymentModel.HasValue || item.IsCancellationAllowed != true)
                            tempCost = item.ReservationRoomsTotalAmount ?? 0;
                        else
                        {
                            var query = item.ReservationRoomDetails.OrderBy(p => p.Date);

                            var total = (item.ReservationRoomsTotalAmount ?? 0);
                            switch (item.CancellationPaymentModel)
                            {
                                case 3:
                                    tempCost = query.Take((item.CancellationNrNights ?? 0)).Sum(p => p.Price);
                                    if (tempCost > total)
                                        tempCost = total;
                                    break;
                                case 2:
                                    tempCost = total * ((item.CancellationValue ?? 100) / 100);
                                    break;
                                default:
                                    tempCost = (item.CancellationValue ?? 0);
                                    break;
                            }
                        }
                    }

                    roomsCost.Add(new contractsReservations.ReservationRoomCancelationCost
                    {
                        Number = item.ReservationRoomNo,
                        Status = item.Status ?? (int)Constants.ReservationStatus.Modified,
                        CancelationCosts = (tempCost * exchangeRate),
                        NotApply = notApply
                    });
                }
            }

            return roomsCost;
        }

        /// <summary>
        /// Calculate deposit cost for each room of the reservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="reservationAdditionalData"></param>
        /// <returns></returns>
        public List<ReservationRoomDepositCost> GetDepositCosts(contractsReservations.Reservation reservation, domainReservations.ReservationsAdditionalData reservationAdditionalData
            , contractsReservations.ReservationsAdditionalData contractAdditionalDate = null, DateTime? depositDate = null)
        {
            var roomsCost = new List<ReservationRoomDepositCost>();
            if (reservation != null)
            {
                // Get roomAdditionalData
                contractsReservations.ReservationsAdditionalData tmpAdditionalData = null;

                if (contractAdditionalDate != null)
                    tmpAdditionalData = contractAdditionalDate;

                if (reservationAdditionalData != null && !string.IsNullOrEmpty(reservationAdditionalData.ReservationAdditionalDataJSON))
                    tmpAdditionalData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationAdditionalData.ReservationAdditionalDataJSON);

                if (tmpAdditionalData == null || tmpAdditionalData.ReservationRoomList == null)
                    return roomsCost;

                decimal exchangeRate = 1 / (reservation.PropertyBaseCurrencyExchangeRate ?? ((decimal)1));
                foreach (var item in reservation.ReservationRooms)
                {
                    var depositPolicy = tmpAdditionalData.ReservationRoomList.FirstOrDefault(x => x.ReservationRoom_UID == item.UID);
                    if (depositPolicy != null)
                    {
                        var applyDeposit = true;
                        if (depositPolicy.DepositDays != null)
                        {
                            if (depositDate == null)
                            {
                                depositDate = DateTime.Now.Date;
                            }

                            if ((item.DateFrom.Value.Date - depositDate.Value).TotalDays > depositPolicy.DepositDays.Value)
                                applyDeposit = false;
                        }
                        decimal tempCost = 0;
                        if (applyDeposit)
                        {
                            if (!depositPolicy.PaymentModel.HasValue)
                                return roomsCost;
                            else
                            {
                                var query = item.ReservationRoomDetails.OrderBy(p => p.Date);

                                var total = (item.ReservationRoomsTotalAmount ?? 0);
                                switch (depositPolicy.PaymentModel)
                                {
                                    case 3:
                                        tempCost = query.Take((depositPolicy.NrNights ?? 0)).Sum(p => p.Price);
                                        if (tempCost > total)
                                            tempCost = total;
                                        break;
                                    case 2:
                                        tempCost = total * ((depositPolicy.Value ?? 100) / 100);
                                        break;
                                    default:
                                        tempCost = (depositPolicy.Value ?? 0);
                                        break;
                                }
                            }
                        }

                        roomsCost.Add(new ReservationRoomDepositCost
                        {
                            Number = item.ReservationRoomNo,
                            Status = item.Status ?? (int)Constants.ReservationStatus.Modified,
                            DepositCosts = (tempCost * exchangeRate),

                        });
                    }
                }
            }

            return roomsCost;
        }

        /// <summary>
        /// Check if deposit policy is diferent from original reservation
        /// </summary>
        /// <param name="reservationRoomId"></param>
        /// <param name="reservationAdditionalData"></param>
        /// <param name="currentDepositPolicy">current deposit policy</param>
        /// <returns>false if equals, true otherwise</returns>
        public bool CheckIfDepositPolicyChanged(long reservationRoomId, contractsReservations.ReservationsAdditionalData reservationAdditionalData,
            OBcontractsRates.DepositPolicy currentDepositPolicy)
        {
            contractsReservations.ReservationsAdditionalData tmpAdditionalData = null;
            if (reservationAdditionalData != null)
                tmpAdditionalData = reservationAdditionalData;

            if (currentDepositPolicy == null
                && (tmpAdditionalData == null || (!tmpAdditionalData.ReservationRoomList.Any())))
                return false;

            OBcontractsRates.DepositPolicy sentDepositPolicy = null;
            if (tmpAdditionalData != null && tmpAdditionalData.ReservationRoomList.Any())
            {
                var roomData = tmpAdditionalData.ReservationRoomList.FirstOrDefault(x => x.ReservationRoom_UID == reservationRoomId);
                sentDepositPolicy = new OBcontractsRates.DepositPolicy
                {
                    Days = roomData.DepositDays,
                    DepositCosts = roomData.DepositCosts,
                    IsDepositCostsAllowed = roomData.IsDepositCostsAllowed,
                    NrNights = roomData.NrNights,
                    PaymentModel = roomData.PaymentModel,
                    Value = roomData.Value
                };
            }

            if (!CompareDepositPolicies(currentDepositPolicy, sentDepositPolicy))
                return true;

            return false;
        }

        /// <summary>
        /// Compare 2 deposit policies excluding the following fields
        /// "Name", "Description", "Property_UID", "ModifiedDate", "IsDeleted", "UID",
        /// "TranslateName", "TranslatedDescription", "Percentage", "CreatedDate"
        /// </summary>
        /// <param name="currentPolicy"></param>
        /// <param name="newPolicy"></param>
        /// <returns></returns>
        public bool CompareDepositPolicies(OBcontractsRates.DepositPolicy currentPolicy, OBcontractsRates.DepositPolicy newPolicy)
        {
            if (currentPolicy.Days != newPolicy.Days && (currentPolicy.Days == null || !currentPolicy.Days.Equals(newPolicy.Days)))
                return false;
            if (currentPolicy.DepositCosts != newPolicy.DepositCosts)
                return false;
            if (currentPolicy.IsDepositCostsAllowed != newPolicy.IsDepositCostsAllowed)
                return false;
            if (currentPolicy.NrNights != newPolicy.NrNights && (currentPolicy.NrNights == null || !currentPolicy.NrNights.Equals(newPolicy.NrNights)))
                return false;
            if (currentPolicy.PaymentModel != newPolicy.PaymentModel && (currentPolicy.PaymentModel == null || !currentPolicy.PaymentModel.Equals(newPolicy.PaymentModel)))
                return false;
            if (currentPolicy.Value != newPolicy.Value && (currentPolicy.Value == null || !currentPolicy.Value.Equals(newPolicy.Value)))
                return false;

            return true;
        }

        /// <summary>
        /// Check if cancellation policy is diferent from original reservation
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currentCancelationPolicy">current cancellation policy</param>
        /// <returns>false if equals, true otherwise</returns>
        public bool CheckIfCancelationPolicyChanged(domainReservations.ReservationRoom room, OBcontractsRates.CancellationPolicy currentCancelationPolicy)
        {
            var sentCancelationPolicy = new OBcontractsRates.CancellationPolicy
            {
                Days = room.CancellationPolicyDays,
                CancellationCosts = room.CancellationCosts,
                IsCancellationAllowed = room.IsCancellationAllowed,
                NrNights = room.CancellationNrNights,
                PaymentModel = room.CancellationPaymentModel,
                Value = room.CancellationValue
            };

            if (currentCancelationPolicy != null)
            {
                if (currentCancelationPolicy.CancellationCosts != sentCancelationPolicy.CancellationCosts)
                    return true;
                if (currentCancelationPolicy.Days != sentCancelationPolicy.Days && (currentCancelationPolicy.Days == null || !currentCancelationPolicy.Days.Equals(sentCancelationPolicy.Days)))
                    return true;
                if (currentCancelationPolicy.IsCancellationAllowed != sentCancelationPolicy.IsCancellationAllowed && (currentCancelationPolicy.IsCancellationAllowed == null || !currentCancelationPolicy.IsCancellationAllowed.Equals(sentCancelationPolicy.IsCancellationAllowed)))
                    return true;
                if (currentCancelationPolicy.NrNights != sentCancelationPolicy.NrNights && (currentCancelationPolicy.NrNights == null || !currentCancelationPolicy.NrNights.Equals(sentCancelationPolicy.NrNights)))
                    return true;
                if (currentCancelationPolicy.PaymentModel != sentCancelationPolicy.PaymentModel && (currentCancelationPolicy.PaymentModel == null || !currentCancelationPolicy.PaymentModel.Equals(sentCancelationPolicy.PaymentModel)))
                    return true;
                if (currentCancelationPolicy.Value != sentCancelationPolicy.Value && (currentCancelationPolicy.Value == null || !currentCancelationPolicy.Value.Equals(sentCancelationPolicy.Value)))
                    return true;
                return false;
            }
            else if (sentCancelationPolicy.PaymentModel != null)
                return true;

            return false;
        }

        /// <summary>
        /// Set cancellation Policies to reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rrdList">FinalPrice property is used for policies calculation</param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="reservationLanguageUsed"></param>
        /// <param name="propertyBaseCurrencyExchangeRate"></param>
        /// <param name="forceDefaultCancellationpolicy"></param>
        public void SetCancellationPolicies(domainReservations.ReservationRoom room,
            List<OBcontractsRates.RateRoomDetailReservation> rrdList,
            long? reservationBaseCurrency, long? reservationLanguageUsed, decimal? propertyBaseCurrencyExchangeRate = 1,
            bool forceDefaultCancellationpolicy = false)
        {
            // Get Most Restrictive Cancelation Policy
            var cancellationPolicy = GetMostRestrictiveCancelationPolicy(room.DateFrom.Value, room.DateTo.Value, room.Rate_UID.Value,
                reservationBaseCurrency, reservationLanguageUsed, rrdList,
                forceDefaultCancellationpolicy);

            if (cancellationPolicy != null)
            {
                if (!string.IsNullOrEmpty(cancellationPolicy.TranslatedDescription))
                    room.CancellationPolicy = cancellationPolicy.TranslatedName + " : " + cancellationPolicy.TranslatedDescription;
                else
                    room.CancellationPolicy = MissedTranlation + " \n " + cancellationPolicy.Name;

                room.IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
                room.CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
                room.CancellationPolicyDays = cancellationPolicy.Days;
                room.CancellationPaymentModel = cancellationPolicy.PaymentModel;
                room.CancellationNrNights = cancellationPolicy.NrNights;

                room.CancellationValue = cancellationPolicy.Value;
                room.CancellationValue = room.CancellationPaymentModel == 2 ? room.CancellationValue
                                    : room.CancellationValue * propertyBaseCurrencyExchangeRate;
            }
            else // Reset Policy
            {
                room.CancellationPolicy = string.Empty;
                room.IsCancellationAllowed = null;
                room.CancellationCosts = false;
                room.CancellationPolicyDays = null;
                room.CancellationPaymentModel = null;
                room.CancellationNrNights = null;
                room.CancellationValue = null;
            }
        }

        /// <summary>
        /// Set deposit Policies to reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rrdList">FinalPrice property is used for policies calculation</param>
        /// <param name="reservationAdditionalData"></param>
        /// <param name="reservationId"></param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="reservationLanguageUsed"></param>
        /// <param name="propertyBaseCurrencyExchangeRate"></param>
        public void SetDepositPolicies(domainReservations.ReservationRoom room,
            List<OBcontractsRates.RateRoomDetailReservation> rrdList, ref contractsReservations.ReservationsAdditionalData reservationAdditionalData,
            long reservationId, long? reservationBaseCurrency, long? reservationLanguageUsed, decimal? propertyBaseCurrencyExchangeRate = 1)
        {
            string missedTranlation = "This information does not exist in your language...!";

            // Get Most Restrictive DepositPolicy
            var depositPolicy = GetMostRestrictiveDepositPolicy(room.DateFrom.Value, room.DateTo.Value, room.Rate_UID.Value,
                reservationBaseCurrency, reservationLanguageUsed, rrdList);

            if (depositPolicy != null)
            {

                contractsReservations.ReservationRoomAdditionalData roomData = null;
                if (reservationAdditionalData.ReservationRoomList.Any())
                    roomData = reservationAdditionalData.ReservationRoomList.FirstOrDefault(x => x.ReservationRoom_UID == room.UID);

                if (roomData == null)
                {
                    roomData = new contractsReservations.ReservationRoomAdditionalData();
                    reservationAdditionalData.ReservationRoomList.Add(roomData);
                }

                if (!string.IsNullOrEmpty(depositPolicy.TranslatedDescription))
                    room.DepositPolicy = depositPolicy.TranslatedName + " : " + depositPolicy.TranslatedDescription;
                else
                    room.DepositPolicy = missedTranlation + " \n " + depositPolicy.Name;

                roomData.ReservationRoom_UID = room.UID;
                roomData.DepositPolicy_UID = depositPolicy.UID;
                roomData.IsDepositCostsAllowed = depositPolicy.IsDepositCostsAllowed;
                roomData.DepositCosts = depositPolicy.DepositCosts;
                roomData.DepositDays = depositPolicy.Days;
                roomData.PaymentModel = depositPolicy.PaymentModel;
                roomData.Value = depositPolicy.Value;

                roomData.Value = roomData.PaymentModel == 2 ? roomData.Value
                                    : roomData.Value * propertyBaseCurrencyExchangeRate;

                roomData.NrNights = depositPolicy.NrNights;
            }
        }

        /// <summary>
        /// Get Most restriction Cancelation Policy
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="rateId"></param>
        /// <param name="currencyId"></param>
        /// <param name="languageId"></param>
        /// <param name="rrdList">list of rateroomdetail, final price is used for the calculation</param>
        /// <param name="forceDefaultPolicy"></param>
        /// <returns></returns>
        public Contracts.Data.Rates.CancellationPolicy GetMostRestrictiveCancelationPolicy(DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId,
            List<OBcontractsRates.RateRoomDetailReservation> rrdList, bool forceDefaultPolicy = false)
        {
            Contracts.Data.Rates.CancellationPolicy cancellationPolicy = null;
            var cancelationRepo = RepositoryFactory.GetOBCancellationPolicyRepository();
            var cancelationPolicies = cancelationRepo.ListCancelationPolicies(new OB.BL.Contracts.Requests.ListCancellationPoliciesRequest
            {
                CheckIn = checkIn,
                CheckOut = checkOut,
                RateId = rateId,
                CurrencyId = currencyId,
                LanguageUID = languageId,
                ForceDefaultPolicy = forceDefaultPolicy
            });

            if (cancelationPolicies != null)
            {
                var groupedCancellationPolicies = cancelationPolicies.OrderBy(p => p.SortOrder).GroupBy(p => p.SortOrder);

                if (groupedCancellationPolicies.Any())
                {
                    var response = cancelationRepo.CalculateMostRestrictiveCancellationPolicy(new OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest
                    {
                        collection = groupedCancellationPolicies.First().ToList(),
                        rrds = rrdList
                    });

                    cancellationPolicy = response;
                }
            }

            return cancellationPolicy;
        }

        /// <summary>
        /// Get Most restriction Deposit Policy
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="rateId"></param>
        /// <param name="currencyId"></param>
        /// <param name="languageId"></param>
        /// <param name="rrdList">list of rateroomdetail, final price is used for the calculation</param>
        /// <returns></returns>
        public Contracts.Data.Rates.DepositPolicy GetMostRestrictiveDepositPolicy(DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId,
            long? languageId, List<OBcontractsRates.RateRoomDetailReservation> rrdList)
        {
            Contracts.Data.Rates.DepositPolicy depositPolicy = null;
            var depositRepo = RepositoryFactory.GetOBDepositPolicyRepository();

            // Get all policies in search criteria
            var depositPolicies = depositRepo.ListDepositPolicies(new OB.BL.Contracts.Requests.ListDepositPoliciesRequest { CheckIn = checkIn, CheckOut = checkOut, RateId = rateId, CurrencyId = currencyId, LanguageUID = languageId });

            if (depositPolicies != null)
            {
                var groupedDepositPolicies = depositPolicies.OrderBy(p => p.SortOrder).GroupBy(p => p.SortOrder);

                if (groupedDepositPolicies.Any())
                {
                    depositPolicy = depositRepo.CalculateMostRestrictiveDepositPolicy(new Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest
                    {
                        collection = groupedDepositPolicies.First().ToList(),
                        rrds = rrdList
                    });
                }
            }

            return depositPolicy;
        }

        public OBcontractsRates.OtherPolicy GetOtherPolicyByRateId(long? rateId, long? baseLanguage)
        {
            var otherPolicyRepo = RepositoryFactory.GetOBOtherPolicyRepository();
            var otherPolicy = otherPolicyRepo.GetOtherPoliciesByRateId(new Contracts.Requests.GetOtherPoliciesRequest { RateId = rateId, LanguageUID = baseLanguage });
            return otherPolicy;
        }

        /// <summary>
        /// Set Other Policy in reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rateId"></param>
        /// <param name="baseLanguage"></param>
        public void SetOtherPolicy(domainReservations.ReservationRoom room, long? rateId, long? baseLanguage)
        {
            var otherPolicy = GetOtherPolicyByRateId(rateId, baseLanguage);
            if (otherPolicy != null && otherPolicy.UID > 0)
            {
                if (!string.IsNullOrEmpty(otherPolicy.TranslatedName))
                {
                    room.OtherPolicy = otherPolicy.TranslatedName + " : " + otherPolicy.TranslatedDescription;
                }
                else
                    room.OtherPolicy = MissedTranlation + " \n " + otherPolicy.OtherPolicy_Name;

                if (room.OtherPolicy != null && room.OtherPolicy.Length > 4000)
                {
                    room.OtherPolicy = room.OtherPolicy.Substring(0, 3997) + "...";
                }
            }
        }

        #region TAX POLICIES

        public List<OBcontractsRates.TaxPolicy> GetTaxPoliciesByRateIds(List<long> rateIds, long? currencyId, long? languageId)
        {
            var otherPolicyRepo = RepositoryFactory.GetOBOtherPolicyRepository();

            var taxPolicies = otherPolicyRepo.ListTaxPoliciesByRateIds(new Contracts.Requests.ListTaxPoliciesByRateIdsRequest { RateIds = rateIds, CurrencyId = currencyId, LanguageUID = languageId });

            return taxPolicies;
        }

        public decimal CalculateTax(OBcontractsRates.TaxPolicy tax, decimal? roomTotal, int numberOfNights, int? numAdults)
        {
            if (tax == null) return 0;

            decimal taxValue;

            var numPerson = 0;

            if (numAdults != null)
                numPerson += (int)numAdults;

            decimal total = 0;
            decimal mult1 = tax.IsPerNight.HasValue && tax.IsPerNight.Value ? numberOfNights : 1;

            if (tax.IsPercentage == true)
                taxValue = ((decimal)roomTotal / mult1) * ((decimal)tax.Value / 100);
            else
                taxValue = (decimal)tax.Value;

            decimal mult2 = (tax.IsPerPerson == true && !(tax.IsPercentage ?? false)) ? numPerson : 1;

            total += (taxValue * mult1 * mult2);
            return total;
        }

        /// <summary>
        /// Set TaxPolicies
        /// </summary>
        /// <param name="room"></param>
        /// <param name="taxPolicies"></param>
        /// <param name="exchangeRate"></param>
        /// <returns></returns>
        public void SetReservationRoomTaxPolicies(domainReservations.ReservationRoom room, List<OBcontractsRates.TaxPolicy> taxPolicies, decimal exchangeRate, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRoomTaxRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomTaxPolicy>(unitOfWork);

                // Delete Old Tax Policies
                reservationRoomTaxRepo.Delete(x => x.ReservationRoom_UID == room.UID);

                if (room.ReservationRoomTaxPolicies != null)
                    room.ReservationRoomTaxPolicies.Clear();

                var roomTaxPolicies = taxPolicies != null ? taxPolicies.Where(x => x.Rate_UID == room.Rate_UID) : null;
                if (roomTaxPolicies != null && roomTaxPolicies.Any())
                {
                    var roomTotal = room.ReservationRoomsPriceSum ?? 0;
                    var numAdults = room.AdultCount ?? 0;
                    var numChilds = room.ChildCount ?? 0;                    
                    var freeNights = rrdList.Where(w => w.AppliedIncentives?.Any(x => x.IncentiveType == OB.BL.Constants.IncentiveType.FreeNights) == true && w.FinalPrice <= 0).Count();
                    var numberOfNights = room.ReservationRoomDetails.Count - freeNights;
                    decimal totalTax = 0;

                    int i = 0;
                    foreach (var item in roomTaxPolicies)
                    {
                        if (!(item.IsPercentage ?? false))
                            item.Value = item.Value * exchangeRate;

                        var taxPolicy = new domainReservations.ReservationRoomTaxPolicy
                        {
                            BillingType = item.BillingType,
                            TaxDefaultValue = item.Value,
                            TaxId = item.UID,
                            TaxCalculatedValue = CalculateTax(item, roomTotal, numberOfNights, numAdults),
                            TaxDescription = item.Description,
                            TaxIsPercentage = item.IsPercentage,
                            TaxName = item.Name,
                            UID = -1 * i,
                            ReservationRoom_UID = room.UID
                        };
                        totalTax += (decimal)taxPolicy.TaxCalculatedValue;
                        room.ReservationRoomTaxPolicies.Add(taxPolicy);
                        i++;
                    }

                    room.TotalTax = Math.Round(totalTax, 4, MidpointRounding.AwayFromZero);
                }
            }
        }

        public void SetReservationRoomTaxPolicies(contractsReservations.ReservationRoom room, List<OBcontractsRates.TaxPolicy> taxPolicies, decimal exchangeRate, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList)
        {
            room.ReservationRoomTaxPolicies = new List<contractsReservations.ReservationRoomTaxPolicy>();

            var roomTaxPolicies = taxPolicies != null ? taxPolicies.Where(x => x.Rate_UID == room.Rate_UID) : new List<OBcontractsRates.TaxPolicy>();
            if (roomTaxPolicies.Any())
            {
                var roomTotal = room.ReservationRoomsPriceSum ?? 0;
                var numAdults = room.AdultCount ?? 0;                                
                var freeNights = rrdList.Where(w => w.AppliedIncentives?.Any(x => x.IncentiveType == OB.BL.Constants.IncentiveType.FreeNights) == true && w.FinalPrice <= 0).Count();
                var numberOfNights = room.ReservationRoomDetails.Count - freeNights;
                decimal totalTax = 0;

                int i = 0;
                foreach (var inputTax in roomTaxPolicies)
                {
                    var clonedInputTax = inputTax.Clone(); // taxPolicy is cloned to avoid change the tax value in future calculations

                    if (!(clonedInputTax.IsPercentage ?? false))
                        clonedInputTax.Value = clonedInputTax.Value * exchangeRate;

                    var taxPolicy = new contractsReservations.ReservationRoomTaxPolicy
                    {
                        BillingType = clonedInputTax.BillingType,
                        TaxDefaultValue = clonedInputTax.Value,
                        TaxId = clonedInputTax.UID,
                        TaxCalculatedValue = CalculateTax(clonedInputTax, roomTotal, numberOfNights, numAdults),
                        TaxDescription = clonedInputTax.Description,
                        TaxIsPercentage = clonedInputTax.IsPercentage,
                        TaxName = clonedInputTax.Name,
                        UID = -1 * i,
                        ReservationRoom_UID = room.UID
                    };
                    totalTax += (decimal)taxPolicy.TaxCalculatedValue;
                    room.ReservationRoomTaxPolicies.Add(taxPolicy);
                    i++;
                }

                room.TotalTax = Math.Round(totalTax, 4, MidpointRounding.AwayFromZero);
            }
        }

        #endregion

        #endregion

        public void SetReservationRoomDetails(domainReservations.ReservationRoom room, List<OBcontractsRates.RateRoomDetailReservation> rrds, out decimal total)
        {
            var detailList = new List<domainReservations.ReservationRoomDetail>();
            total = 0;
            int j = 0;

            var currentReservationRoomDetailsIds = room.ReservationRoomDetails.Select(x => x.UID).ToList();
            var currentAppliedIncentivesIds = room.ReservationRoomDetails.Where(x => x.ReservationRoomDetailsAppliedIncentives != null)
                .SelectMany(x => x.ReservationRoomDetailsAppliedIncentives).Select(x => x.UID).ToList();
            var currentAppliedPromocodes = room.ReservationRoomDetails.Where(x => x.ReservationRoomDetailsAppliedPromotionalCodes != null)
                .SelectMany(x => x.ReservationRoomDetailsAppliedPromotionalCodes);
            var currentAppliedPromocodesIds = currentAppliedPromocodes.Select(x => x.UID).ToList();

            long promoCodeUID = currentAppliedPromocodes.Select(x => x.PromotionalCode_UID).FirstOrDefault();
            var oldPromocodeDays = currentAppliedPromocodes.Select(x => x.Date.Date).ToList();
            var newPromocodeDays = new List<DateTime>();

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRoomDetailRepo = RepositoryFactory.GetReservationRoomDetailRepository(unitOfWork);
                var appliedIncentivesRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomDetailsAppliedIncentive>(unitOfWork);
                var appliedPromocodeRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>(unitOfWork);

                // Remove old reservation room detail and applied incentives and promocodes
                reservationRoomDetailRepo.Delete(x => currentReservationRoomDetailsIds.Contains(x.UID));
                appliedIncentivesRepo.Delete(x => currentAppliedIncentivesIds.Contains(x.UID));
                appliedPromocodeRepo.Delete(x => currentAppliedPromocodesIds.Contains(x.UID));

                foreach (var item in rrds)
                {
                    var reservRoomDetail = new domainReservations.ReservationRoomDetail();
                    reservRoomDetail.RateRoomDetails_UID = item.UID;
                    reservRoomDetail.ReservationRoom_UID = room.UID;
                    reservRoomDetail.Date = item.Date;
                    reservRoomDetail.Price = item.FinalPrice;
                    reservRoomDetail.Rate_UID = item.Rate_UID;
                    reservRoomDetail.CreatedDate = DateTime.UtcNow;
                    total += reservRoomDetail.Price;
                    reservRoomDetail.UID = 0;

                    #region appliedincentives

                    var incentives = new List<domainReservations.ReservationRoomDetailsAppliedIncentive>();

                    if (item.AppliedIncentives != null && item.AppliedIncentives.Any())
                    {
                        foreach (var incentive in item.AppliedIncentives)
                        {
                            incentives.Add(new domainReservations.ReservationRoomDetailsAppliedIncentive
                            {
                                Days = incentive.Days,
                                DiscountPercentage = incentive.DiscountPercentage,
                                DiscountValue = incentive.DayDiscount[j],
                                FreeDays = incentive.FreeDays,
                                Incentive_UID = incentive.UID,
                                IsFreeDaysAtBegin = incentive.IsFreeDaysAtBegin,
                                Name = incentive.Name,
                                UID = 0
                            });
                        }

                        reservRoomDetail.ReservationRoomDetailsAppliedIncentives = incentives;
                    }

                    #endregion

                    #region Applied Promotional Codes

                    if (item.AppliedPromotionalCode != null)
                    {
                        if (promoCodeUID <= 0)
                            promoCodeUID = item.AppliedPromotionalCode.PromotionalCode_UID;

                        var appliedPromocode = new domainReservations.ReservationRoomDetailsAppliedPromotionalCode()
                        {
                            UID = item.AppliedPromotionalCode.UID,
                            PromotionalCode_UID = item.AppliedPromotionalCode.PromotionalCode_UID,
                            ReservationRoomDetail_UID = item.AppliedPromotionalCode.ReservationRoomDetail_UID,
                            Date = item.AppliedPromotionalCode.Date,
                            DiscountValue = item.AppliedPromotionalCode.DiscountValue,
                            DiscountPercentage = item.AppliedPromotionalCode.DiscountPercentage
                        };
                        reservRoomDetail.ReservationRoomDetailsAppliedPromotionalCodes.Add(appliedPromocode);
                        newPromocodeDays.Add(appliedPromocode.Date);
                    }

                    #endregion

                    detailList.Add(reservRoomDetail);
                    j++;
                }
            }

            // Update reservations completed for each day with discount
            if (promoCodeUID > 0)
                UpdatePromoCodeReservationsCompleted(promoCodeUID, oldPromocodeDays, newPromocodeDays);

            room.ReservationRoomDetails = detailList;
        }

        private void SetReservationRoomDetails(contractsReservations.ReservationRoom room, List<OBcontractsRates.RateRoomDetailReservation> rrds, out decimal total)
        {
            var detailList = new List<contractsReservations.ReservationRoomDetail>();
            long promoCodeUID = 0;
            var discountDays = new List<DateTime>();
            total = 0;

            int j = 0;
            foreach (var item in rrds)
            {
                var reservRoomDetail = new contractsReservations.ReservationRoomDetail();
                reservRoomDetail.RateRoomDetails_UID = item.UID;
                reservRoomDetail.ReservationRoom_UID = room.UID;
                reservRoomDetail.Date = item.Date;
                reservRoomDetail.Price = item.FinalPrice;
                reservRoomDetail.Rate_UID = item.Rate_UID;
                reservRoomDetail.CreatedDate = DateTime.UtcNow;
                total += reservRoomDetail.Price;

                #region appliedincentives

                List<contractsReservations.ReservationRoomDetailsAppliedIncentive> incentives = null;

                if (item.AppliedIncentives != null && item.AppliedIncentives.Any())
                {
                    incentives = new List<contractsReservations.ReservationRoomDetailsAppliedIncentive>();
                    foreach (var incentive in item.AppliedIncentives)
                    {
                        incentives.Add(new contractsReservations.ReservationRoomDetailsAppliedIncentive
                        {
                            Days = incentive.Days,
                            DiscountPercentage = incentive.DiscountPercentage,
                            DiscountValue = incentive.DayDiscount[j],
                            FreeDays = incentive.FreeDays,
                            Incentive_UID = incentive.UID,
                            IsFreeDaysAtBegin = incentive.IsFreeDaysAtBegin,
                            Name = incentive.Name,
                        });
                    }
                }

                reservRoomDetail.ReservationRoomDetailsAppliedIncentives = incentives;

                #endregion

                #region Applied Promotional Codes

                if (item.AppliedPromotionalCode != null)
                {
                    if (promoCodeUID <= 0)
                        promoCodeUID = item.AppliedPromotionalCode.PromotionalCode_UID;

                    var appliedPromocode = new contractsReservations.ReservationRoomDetailsAppliedPromotionalCode()
                    {
                        UID = item.AppliedPromotionalCode.UID,
                        PromotionalCode_UID = item.AppliedPromotionalCode.PromotionalCode_UID,
                        ReservationRoomDetail_UID = item.AppliedPromotionalCode.ReservationRoomDetail_UID,
                        Date = item.AppliedPromotionalCode.Date,
                        DiscountValue = item.AppliedPromotionalCode.DiscountValue,
                        DiscountPercentage = item.AppliedPromotionalCode.DiscountPercentage
                    };
                    reservRoomDetail.ReservationRoomDetailsAppliedPromotionalCode = appliedPromocode;
                    discountDays.Add(appliedPromocode.Date);
                }

                #endregion

                detailList.Add(reservRoomDetail);
                j++;
            }

            // Update reservations completed for each day with discount
            if (promoCodeUID > 0)
                UpdatePromoCodeReservationsCompleted(promoCodeUID, newDays: discountDays.Distinct());

            room.ReservationRoomDetails = detailList;
        }

        /// <summary>
        /// Set included extras in reservation
        /// </summary>
        /// <param name="room"></param>
        /// <param name="languageUid"></param>
        /// <returns></returns>
        public void SetIncludedExtras(domainReservations.ReservationRoom room, long languageUid)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var extraRepo = RepositoryFactory.GetOBExtrasRepository();
                var reservationRoomsExtraRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomExtra>(unitOfWork);
                var reservationRoomsExtraDatesRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomExtrasAvailableDate>(unitOfWork);
                var reservationRoomsExtraSchedulesRepo = RepositoryFactory.GetRepository<domainReservations.ReservationRoomExtrasSchedule>(unitOfWork);

                var extras = extraRepo.ListIncludedRateExtras(new Contracts.Requests.ListIncludedRateExtrasRequest
                {
                    Rate_UID = room.Rate_UID.Value,
                    Packages_UID = room.Package_UID ?? 0,
                    LanguageUID = languageUid,
                    CheckIn = room.DateFrom.Value,
                    CheckOut = room.DateTo.Value
                });
                var reservationRoomIncludedExtras = room.ReservationRoomExtras.Where(x => x.ExtraIncluded).ToList();

                // Remove all the included
                if (!extras.Any())
                {
                    foreach (var extra in reservationRoomIncludedExtras)
                    {
                        reservationRoomsExtraDatesRepo.Delete(extra.ReservationRoomExtrasAvailableDates);
                        reservationRoomsExtraSchedulesRepo.Delete(extra.ReservationRoomExtrasSchedules);
                    }

                    reservationRoomsExtraRepo.Delete(reservationRoomIncludedExtras);
                    return;
                }

                if (!room.ReservationRoomExtras.Any())
                {
                    foreach (var item in extras)
                    {
                        room.ReservationRoomExtras.Add(new domainReservations.ReservationRoomExtra
                        {
                            CreatedDate = DateTime.UtcNow,
                            Extra_UID = item.UID,
                            ExtraIncluded = true,
                            Qty = 1,
                            Total_Price = 0,
                            ReservationRoom_UID = room.UID,
                            UID = 0
                        });
                    }

                    return;
                }

                var includedExtras = reservationRoomIncludedExtras.Select(x => x.Extra_UID).ToList();
                var currentExtras = extras.Select(x => x.UID).ToList();

                var extrasToRemove = includedExtras.Except(currentExtras).ToList();
                var extrasToAdd = currentExtras.Except(includedExtras).ToList();

                var reservationRoomExtrasToRemove = reservationRoomIncludedExtras.Where(x => extrasToRemove.Contains(x.Extra_UID));
                foreach (var extra in reservationRoomExtrasToRemove)
                {
                    reservationRoomsExtraDatesRepo.Delete(extra.ReservationRoomExtrasAvailableDates);
                    reservationRoomsExtraSchedulesRepo.Delete(extra.ReservationRoomExtrasSchedules);
                    reservationRoomsExtraRepo.Delete(extra);
                }

                foreach (var item in extras.Where(x => extrasToAdd.Contains(x.UID)))
                {
                    room.ReservationRoomExtras.Add(new domainReservations.ReservationRoomExtra
                    {
                        CreatedDate = DateTime.UtcNow,
                        Extra_UID = item.UID,
                        ExtraIncluded = true,
                        Qty = 1,
                        Total_Price = 0,
                        ReservationRoom_UID = room.UID,
                        UID = 0
                    });
                }
            }
        }

        #region UPDATE CREDITS (Operators, Tpis)

        /// <summary>
        /// Update Operator Credit
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="channelId"></param>
        /// <param name="isOnRequest">reservation IsORequest</param>
        /// <param name="paymentMethodTypeId">reservation PaymentMethodTypeId</param>
        /// <param name="amount"></param>
        /// <param name="sendCreditLimitExcededEmail">output parameter indicating if credit limit as been exceeded</param>
        /// <param name="channelName">output parameter indicating the channel name</param>
        /// <param name="creditLimit">output parameter indicating the excedeed limit</param>
        /// <returns>number of affected lines</returns>
        public void UpdateOperatorCreditUsed(long propertyId, long? channelId, long? paymentMethodTypeId, bool isOnRequest, decimal amount,
            out bool sendCreditLimitExcededEmail, out string channelName, out decimal creditLimit)
        {
            decimal creditValue = amount;
            sendCreditLimitExcededEmail = false;
            channelName = string.Empty;
            creditLimit = 0;
            var paymentAproved = true;

            if (!channelId.HasValue)
                return;

            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
            var channelRepo = RepositoryFactory.GetOBChannelRepository();

            if (paymentMethodTypeId.HasValue && !isOnRequest)
            {
                var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest
                {
                    UIDs = new List<long>() { paymentMethodTypeId.HasValue ? paymentMethodTypeId.Value : 0 }
                }).FirstOrDefault();

                if (payType != null)
                {
                    var response = channelRepo.UpdateOperatorCreditUsed(new Contracts.Requests.UpdateOperatorCreditUsedRequest
                    {
                        PropertyId = propertyId,
                        ChannelId = channelId.Value,
                        PaymentMethodCode = payType.Code,
                        CreditValue = creditValue,
                        SendCreditLimitExcededEmail = sendCreditLimitExcededEmail,
                        ChannelName = channelName,
                        CreditLimit = creditLimit,
                        PaymentApproved = paymentAproved
                    });

                    sendCreditLimitExcededEmail = response.SendCreditLimitExcededEmail;
                    channelName = response.ChannelName;
                    creditLimit = response.CreditLimit;
                    paymentAproved = response.PaymentApproved;
                }

                if (!paymentAproved)
                    throw Errors.OperatorInvalidPayment.ToBusinessLayerException();
            }
        }

        /// <summary>
        /// Update TPI Credit
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="amount"></param>
        /// <param name="sendCreditLimitExcededEmail">output parameter indicating if credit limit as been exceeded</param>
        /// <param name="tpiId"></param>
        /// <param name="paymentMethodTypeId"></param>
        /// <returns>number of affected lines</returns>
        public void UpdateTPICreditUsed(long propertyId, long? tpiId, long? paymentMethodTypeId, decimal amount, out bool sendCreditLimitExcededEmail)
        {
            decimal creditValue = amount;
            sendCreditLimitExcededEmail = false;
            var paymentAproved = true;

            if (!tpiId.HasValue)
                return;

            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
            var channelRepo = RepositoryFactory.GetOBChannelRepository();

            if (paymentMethodTypeId.HasValue)
            {
                List<long> listUids = paymentMethodTypeId > 0 ? new List<long>() { paymentMethodTypeId.Value } : null;
                var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = listUids }).FirstOrDefault();

                if (payType != null)
                {
                    var response = channelRepo.UpdateTPICreditUsed(new Contracts.Requests.UpdateTPICreditUsedRequest { PropertyId = propertyId, TpiId = tpiId.Value, PaymentMethodCode = payType.Code, CreditValue = creditValue, SendCreditLimitExcededEmail = sendCreditLimitExcededEmail, PaymentApproved = paymentAproved });
                    sendCreditLimitExcededEmail = response.SendCreditLimitExcededEmail;
                    paymentAproved = response.PaymentApproved;
                }

                if (!paymentAproved)
                    throw Errors.TpiInvalidPayment.ToBusinessLayerException();
            }
        }

        #endregion

        /// <summary>
        /// Update reservation/reservationroom status
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="reservationStatus"></param>
        /// <param name="transactionId"></param>
        /// <param name="transactionStatus"></param>
        /// <returns></returns>
        public void UpdateReservationStatus(long reservationId, Reservation.BL.Constants.ReservationStatus reservationStatus, string transactionId,
            Reservation.BL.Constants.ReservationTransactionStatus transactionStatus, long? userId, bool updateReservationHistory = true, string paymentGatewayOrderId = null)
        {
            #region Reservation
            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                           new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    try
                    {
                        var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                        reservationRepo.UpdateReservationStatus(reservationId, (int)reservationStatus, transactionId, (int)transactionStatus, reservationStatus.ToString(),
                            userId, updateReservationHistory, paymentGatewayOrderId);

                        transaction.Complete();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        transaction.Dispose();
                        throw Errors.OperationWasInterrupted.ToBusinessLayerException();
                    }
                    catch (OptimisticConcurrencyException)
                    {
                        transaction.Dispose();
                        throw Errors.OperationWasInterrupted.ToBusinessLayerException();
                    }
                    catch (Exception ex)
                    {
                        transaction.Dispose();
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }
            }
            #endregion

            #region ReservationFilter
            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                           new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    try
                    {
                        var reservationFilterRepo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                        reservationFilterRepo.UpdateReservationFilterStatus(reservationId, (int)reservationStatus, userId);

                        transaction.Complete();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        transaction.Dispose();
                        throw Errors.OperationWasInterrupted.ToBusinessLayerException();
                    }
                    catch (OptimisticConcurrencyException)
                    {
                        transaction.Dispose();
                        throw Errors.OperationWasInterrupted.ToBusinessLayerException();
                    }
                    catch (Exception ex)
                    {
                        transaction.Dispose();
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Get Reservation AdditionalData
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns></returns>
        public domainReservations.ReservationsAdditionalData GetReservationAdditionalData(long reservationId)
        {
            domainReservations.ReservationsAdditionalData reservationAdditionalData;
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationAdditionalDataRepo = RepositoryFactory.GetRepository<domainReservations.ReservationsAdditionalData>(unitOfWork);
                reservationAdditionalData = reservationAdditionalDataRepo.GetQuery().FirstOrDefault(x => x.Reservation_UID == reservationId);
            }
            return reservationAdditionalData;
        }

        /// <summary>
        /// Get Reservation AdditionalData json object
        /// </summary>
        /// <param name="reservationAdditionalData">The reservation additional data.</param>
        /// <param name="reservationUID">The reservation uid.</param>
        /// <returns></returns>
        public OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData GetReservationAdditionalDataJsonObject(ref domainReservations.ReservationsAdditionalData reservationAdditionalData, long reservationUID)
        {
            OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData reservationAdditionalDataJsonObj;
            if (reservationAdditionalData != null && !string.IsNullOrEmpty(reservationAdditionalData.ReservationAdditionalDataJSON))
                reservationAdditionalDataJsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData>(reservationAdditionalData.ReservationAdditionalDataJSON);
            else
            {
                if (reservationAdditionalData == null)
                    reservationAdditionalData = new domainReservations.ReservationsAdditionalData();

                reservationAdditionalData.Reservation_UID = reservationUID;
                reservationAdditionalDataJsonObj = new OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData
                {
                    ReservationRoomList = new List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomAdditionalData>()
                };
            }
            return reservationAdditionalDataJsonObj;
        }

        public void DeleteReservationRooms(ICollection<domainReservations.ReservationRoom> rooms)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRoomRepo = RepositoryFactory.GetReservationRoomRepository(unitOfWork);
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                var roomIds = rooms.Select(x => x.UID).ToList();
                if (roomIds.Any())
                {
                    var extrasIds = rooms.SelectMany(x => x.ReservationRoomExtras).Select(x => x.UID).ToList();
                    var rrdIds = rooms.SelectMany(x => x.ReservationRoomDetails).Select(x => x.UID).ToList();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@roomIds", roomIds.ToDataTable(), DbType.Object);
                    parameters.Add("@extrasIds", extrasIds.ToDataTable(), DbType.Object);
                    parameters.Add("@rrdIds", rrdIds.ToDataTable(), DbType.Object);
                    parameters.Add("@reservationId", rooms.First().Reservation_UID, DbType.Int64);

                    repo.dbContext.Database.Connection.ExecuteScalar<int>("DeleteReservationRooms", parameters, null, null, CommandType.StoredProcedure);

                    reservationRoomRepo.Detach(rooms);
                }
            }
        }

        public void DeleteReservationRoomFilter(long reservationUID)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                var reservationFilterRepo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                var reservationRoomFilterRepo = RepositoryFactory.GetReservationRoomFilterRepository(unitOfWork);

                var reservationFilter = reservationFilterRepo.FindByReservationUIDs(new List<long> { reservationUID }).FirstOrDefault();
                if (reservationFilter != null)
                {
                    var roomIds = reservationFilter.ReservationRoomFilters.Select(x => x.UID).ToList();
                    if (roomIds.Any())
                    {
                        repo.dbContext.Database.ExecuteSqlCommand(string.Format("DELETE FROM ReservationRoomFilter WHERE UID IN({0})", string.Join(",", roomIds)));
                        reservationRoomFilterRepo.Detach(reservationFilter.ReservationRoomFilters);
                    }
                }
            }
        }

        /// <summary>
        /// Get Current Exchange Rate Between two currency from property
        /// </summary>
        /// <param name="baseCurrencyId"></param>
        /// <param name="currencyId"></param>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        public decimal GetExchangeRateBetweenCurrenciesByPropertyId(long baseCurrencyId, long currencyId, long propertyId)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                return repo.GetExchangeRateBetweenCurrenciesByPropertyId(baseCurrencyId, currencyId, propertyId);
            }
        }

        /// <summary>
        /// Get exchange rate between two currencies
        /// </summary>
        /// <param name="baseCurrencyUid"></param>
        /// <param name="currencyUid"></param>
        /// <returns></returns>
        public decimal GetExchangeRateBetweenCurrencies(long baseCurrencyUid, long currencyUid)
        {
            var baseExchangeRate = new OB.BL.Contracts.Data.General.ExchangeRate();
            var exchangeRate = new OB.BL.Contracts.Data.General.ExchangeRate();

            var exchangeRateRepo = RepositoryFactory.GetOBCurrencyRepository();

            if (baseCurrencyUid != 34)
                baseExchangeRate = exchangeRateRepo.ListExchangeRatesByCurrencyIds(new Contracts.Requests.ListExchangeRatesRequest { CurrencyIds = new List<long>() { baseCurrencyUid } }).FirstOrDefault();
            else
                baseExchangeRate.Rate = 1; // euro

            if (currencyUid != 34)
                exchangeRate = exchangeRateRepo.ListExchangeRatesByCurrencyIds(new Contracts.Requests.ListExchangeRatesRequest { CurrencyIds = new List<long>() { currencyUid } }).FirstOrDefault();
            else
                exchangeRate.Rate = 1; // euro

            if (baseExchangeRate != null && exchangeRate != null)
                return exchangeRate.Rate / baseExchangeRate.Rate;
            else
                return 0;
        }

        #region RESERVATION TRANSACTION STATUS

        public void InsertReservationTransaction(string transactionId, long reservationId, string reservationNumber, long reservationStatus,
                                Constants.ReservationTransactionStatus transactionStatus, long channelId, long hangfireId, int retries)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                Retry.Execute<int>(() => repo.InsertReservationTransaction(transactionId, reservationId, reservationNumber, reservationStatus, transactionStatus, channelId, hangfireId, retries),
                     TimeSpan.FromMilliseconds(500));
            }
        }

        /// <summary>
        /// Get Reservation Next Transaction State
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="reservationId"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        public int GetReservationTransactionState(string transactionId, long channelId, out long reservationId, out long hangfireId)
        {
            int currentState = 0;
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                currentState = reservationRepo.GetReservationTransactionState(transactionId, channelId, out reservationId, out hangfireId);
            }
            return currentState;
        }

        /// <summary>
        /// Update Reservation Transaction Retries - Type B
        /// </summary>
        /// <returns>true if less than 3 retries, false if retries limit has been exceeded</returns>
        public bool UpdateReservationTransactionRetries(string transactionId, long channelId, out int retries)
        {
            int result = 0;
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                retries = 0;

                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("channelUID", channelId);
                parameters[1] = new SqlParameter("transactionUID", transactionId);
                parameters[2] = new SqlParameter("retries", retries);
                parameters[2].Direction = ParameterDirection.Output;

                result = repo.dbContext.Database.ExecuteSqlCommand("UpdateReservationTransactionRetries @channelUID, @transactionUID, @retries", parameters);
                retries = result;
            }

            return result > 0;
        }

        /// <summary>
        /// Update Reservation Transaction Status
        /// </summary>
        /// <returns></returns>
        public void UpdateReservationTransactionStatus(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                repo.UpdateReservationTransactionStatus(transactionId, channelId, transactionStatus);
            }
        }

        #endregion RESERVATION TRANSACTION STATUS

        #region HangFire Jobs

        /// <summary>
        /// Set Job To Ignore Reservation after an amount of time if it status remain pending
        /// </summary>
        /// <param name="request"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public long SetJobToIgnorePendingReservation(ReservationBaseRequest request, long jobId)
        {
            if (!UnitTestDetector.IsRunningInUnitTest)
            {
                request.TransactionAction = OB.Reservation.BL.Constants.ReservationTransactionAction.Ignore;

                var appRepo = RepositoryFactory.GetOBAppSettingRepository();
                var secondsToDelayJob = double.Parse(appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string>() { "IgnoreReservationTransactionTime" } }).FirstOrDefault().Value);

                IBackgroundJobClient client = new BackgroundJobClient();
                if (!client.ChangeState(jobId.ToString(), new ScheduledState(DateTime.UtcNow.Add(TimeSpan.FromSeconds(secondsToDelayJob)))))
                {
                    var url = WebConfigurationManager.AppSettings["OB.Reservation.REST.Services.Endpoint"];
                    var id = BackgroundJob.Schedule<CallRestService>(x => x.Call(url, "Reservation", "ModifyReservation", request),
                            TimeSpan.FromSeconds(secondsToDelayJob));

                    long.TryParse(id, out jobId);

                    UpdateHanfireJobInReservationTransactionStatus(request.TransactionId, request.ChannelId, jobId);
                }
            }

            return jobId;
        }

        private void UpdateHanfireJobInReservationTransactionStatus(string transactionId, long channelId, long jobId)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                string query = @"UPDATE ReservationTransactionStatus SET HangfireId = @hangfireId
                                WHERE TransactionUID = @transactionUID AND ChannelUID = @channelId";

                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("transactionUID", transactionId);
                parameters[1] = new SqlParameter("channelId", channelId);
                parameters[2] = new SqlParameter("hangfireId", jobId);

                var result = repo.dbContext.Database.ExecuteSqlCommand(query, parameters);

                if (result <= 0)
                    Logger.Warn("Error update HanfireId on TransactionId: {0}", transactionId);
            }
        }


        /// <summary>
        /// Set Job To Cancel Reservation onrequest after an amount of time if it status remain pending
        /// </summary>
        /// <param name="request"></param>
        /// <param name="secondsToDelayJob"></param>
        /// <returns></returns>
        public long SetJobToCancelReservationOnRequestWithDelay(IUnitOfWork unitOfWork, ReservationBaseRequest request, double secondsToDelayJob)
        {
            long jobId = 0;

            var url = WebConfigurationManager.AppSettings["OB.Reservation.REST.Services.Endpoint"];
            var id = BackgroundJob.Schedule<CallRestService>(x => x.Call(url, "Reservation", "CancelReservation", request), TimeSpan.FromSeconds(secondsToDelayJob));

            long.TryParse(id, out jobId);

            UpdateHanfireJobInReservationTransactionStatus(request.TransactionId, request.ChannelId, jobId);

            return jobId;
        }

        /// <summary>
        /// delete Job To Ignore Reservation after an amount of time if it status remain pending
        /// </summary>
        /// <param name="jobId"></param>
        public void DeleteHangfireJob(long jobId)
        {
            var logger = new Log.DefaultLogger("DeleteHangfireJob");
            try
            {
                // Delete Job do Ignore Changes on modifications
                if (!BackgroundJob.Delete(jobId.ToString(), "Scheduled"))
                {
                    if (!BackgroundJob.Delete(jobId.ToString(), "Enqueued"))
                    {
                        BackgroundJob.Delete(jobId.ToString(), "Failed");
                        logger.Debug("Delete Hangfire Job Failed - JobId: {0}", jobId);
                    }
                    else
                        logger.Debug("Delete Hangfire Job Enqueued - JobId: {0}", jobId);
                }
                else
                    logger.Debug("Delete Hangfire Job Scheduled - JobId: {0}", jobId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Delete Hangfire Job Error - JobId: {0}", jobId));
            }
        }

        /// <summary>
        /// Set Job To Unlock Reservation Transaction after an amount of time (Transaction was lock because excess of retries)
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="transactionStatus"></param>
        public void SetJobToUnlockReservationTransaction(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus)//, long oldJobToDelete)
        {
            var appRepo = RepositoryFactory.GetOBAppSettingRepository();
            var secondsToDelayJob = double.Parse(appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string>() { "IgnoreReservationTransactionTime" } }).FirstOrDefault().Value);

            var url = WebConfigurationManager.AppSettings["OB.Reservation.REST.Services.Endpoint"];

            BackgroundJob.Schedule<CallRestService>(x => x.Call(url, "Reservation", "UpdateReservationTransactionStatus", new UpdateReservationTransactionStatusRequest
            {
                TransactionId = transactionId,
                ChannelId = channelId,
                ReservationTransactionStatus = transactionStatus
            }),
            TimeSpan.FromSeconds(secondsToDelayJob));
        }

        #endregion

        #region Loyalty Level

        /// <summary>
        /// Get all guest reservationroom on a period of time
        /// </summary>
        /// <param name="guest_UID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<GuestLoyaltyReservationQR1> GetGuestsReservationRoomsWithLoyaltyDiscount(long guest_UID, DateTime startDate, DateTime endDate)
        {
            List<GuestLoyaltyReservationQR1> result = new List<GuestLoyaltyReservationQR1>();
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var repo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("guestUID", guest_UID);
                parameters[1] = new SqlParameter("startDate", startDate);
                parameters[2] = new SqlParameter("endDate", endDate);

                result = repo.dbContext.Database.SqlQuery<GuestLoyaltyReservationQR1>("GetGuestsReservationRoomsWithLoyaltyDiscount @guestUID, @startDate, @endDate", parameters).ToList();
            }
            return result;
        }

        #endregion

        #region CONVERT RESERVATION TO HOTEL BASE CURRENCY

        public domainReservations.Reservation ApplyCurrencyExchangeToReservationForConnectors(domainReservations.Reservation newReservation, domainReservations.Reservation existingReservation,
            ReservationDataContext context, contractsReservations.ReservationsAdditionalData reservationAdditionalData = null,
            bool onlyApplyExchangeToAdditionalData = false, List<string> roomNoAdditionalDataToUpdate = null)
        {
            long? propertyCurrencyUID = null;
            long? rateCurrencyUID = null;
            long? reservationCurrencyUID = null;
            long? reservationBaseCurrency_UID = null;


            if (!onlyApplyExchangeToAdditionalData)
            {
                // get property currency
                propertyCurrencyUID = context.PropertyBaseCurrency_UID;

                // get rate currency
                rateCurrencyUID = newReservation.ReservationBaseCurrency_UID;

                // if rate currency is null, rate currency = property currency
                if (rateCurrencyUID == null)
                    rateCurrencyUID = propertyCurrencyUID;

                // get reservation values currency
                reservationCurrencyUID = newReservation.ReservationCurrency_UID ?? rateCurrencyUID;
                reservationBaseCurrency_UID = newReservation.ReservationBaseCurrency_UID ?? rateCurrencyUID;

                // set new object with correct currencies
                newReservation.ReservationBaseCurrency_UID = reservationBaseCurrency_UID; // rate currency
                newReservation.ReservationCurrency_UID = reservationCurrencyUID; // reservation currency

                // set rate to reservation currency exchange rate
                if (existingReservation != null && existingReservation.ReservationCurrencyExchangeRate.HasValue)
                {
                    newReservation.ReservationCurrencyExchangeRate = existingReservation.ReservationCurrencyExchangeRate;
                    newReservation.ReservationCurrencyExchangeRateDate = existingReservation.ReservationCurrencyExchangeRateDate;
                }
                else
                {
                    newReservation.ReservationCurrencyExchangeRate = this.GetExchangeRateBetweenCurrenciesByPropertyId(rateCurrencyUID.Value, reservationCurrencyUID.Value, newReservation.Property_UID);
                    newReservation.ReservationCurrencyExchangeRateDate = DateTime.UtcNow;
                }

                // set rate to property base currency exchange rate
                if (existingReservation != null && existingReservation.PropertyBaseCurrencyExchangeRate.HasValue)
                    newReservation.PropertyBaseCurrencyExchangeRate = existingReservation.PropertyBaseCurrencyExchangeRate;
                else
                    newReservation.PropertyBaseCurrencyExchangeRate = this.GetExchangeRateBetweenCurrenciesByPropertyId(rateCurrencyUID.Value, propertyCurrencyUID.Value, newReservation.Property_UID);

            }
            // get the exchange from reservation to base currency
            decimal exchangeReservationToPropertyBase = newReservation.PropertyBaseCurrencyExchangeRate.Value / newReservation.ReservationCurrencyExchangeRate.Value;

            if (exchangeReservationToPropertyBase == 1)
                return newReservation;

            if (!onlyApplyExchangeToAdditionalData)
            {
                #region Reservation Base Object

                newReservation.RoomsExtras = newReservation.RoomsExtras.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.RoomsExtras.Value, 4) : 0;
                newReservation.RoomsPriceSum = newReservation.RoomsPriceSum.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.RoomsPriceSum.Value, 4) : 0;
                newReservation.RoomsTax = newReservation.RoomsTax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.RoomsTax.Value, 4) : 0;
                newReservation.RoomsTotalAmount = newReservation.RoomsTotalAmount.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.RoomsTotalAmount.Value, 4) : 0;
                newReservation.Tax = newReservation.Tax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.Tax.Value, 4) : 0;
                newReservation.TotalAmount = newReservation.TotalAmount.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.TotalAmount.Value, 4) : 0;
                newReservation.TotalTax = newReservation.TotalTax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.TotalTax.Value, 4) : 0;
                newReservation.PaymentAmountCaptured = newReservation.PaymentAmountCaptured.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.PaymentAmountCaptured.Value, 4) : 0;
                newReservation.ChannelProperties_Value = newReservation.ChannelProperties_Value.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.ChannelProperties_Value.Value, 4) : 0;
                newReservation.InstallmentAmount = newReservation.InstallmentAmount.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.InstallmentAmount.Value, 4) : 0;
                newReservation.SalesmanCommission = newReservation.SalesmanCommission.HasValue ? decimal.Round(exchangeReservationToPropertyBase * newReservation.SalesmanCommission.Value, 4) : 0;

                #endregion

                #region Reservation.ReservationRooms

                foreach (var room in newReservation.ReservationRooms)
                {
                    if (room.CancellationPaymentModel != 2)
                        room.CancellationValue = room.CancellationValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.CancellationValue.Value, 4) : 0;

                    room.ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.ReservationRoomsExtrasSum.Value, 4) : 0;
                    room.ReservationRoomsPriceSum = room.ReservationRoomsPriceSum.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.ReservationRoomsPriceSum.Value, 4) : 0;
                    room.ReservationRoomsTotalAmount = room.ReservationRoomsTotalAmount.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.ReservationRoomsTotalAmount.Value, 4) : 0;
                    room.TPIDiscountValue = room.TPIDiscountValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.TPIDiscountValue.Value, 4) : 0;
                    room.TotalTax = room.TotalTax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * room.TotalTax.Value, 4) : 0;

                    foreach (var detail in room.ReservationRoomDetails)
                    {
                        detail.AdultPrice = detail.AdultPrice.HasValue ? decimal.Round(exchangeReservationToPropertyBase * detail.AdultPrice.Value, 4) : 0;
                        detail.ChildPrice = detail.ChildPrice.HasValue ? decimal.Round(exchangeReservationToPropertyBase * detail.ChildPrice.Value, 4) : 0;
                        detail.Price = decimal.Round(exchangeReservationToPropertyBase * detail.Price, 4);

                        if (detail.ReservationRoomDetailsAppliedIncentives != null && detail.ReservationRoomDetailsAppliedIncentives.Any())
                        {
                            foreach (var incentive in detail.ReservationRoomDetailsAppliedIncentives)
                                incentive.DiscountValue = incentive.DiscountValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * incentive.DiscountValue.Value, 4) : 0;
                        }
                    }

                    foreach (var extra in room.ReservationRoomExtras)
                    {
                        extra.Total_Price = decimal.Round(exchangeReservationToPropertyBase * extra.Total_Price, 4);
                        extra.Total_VAT = decimal.Round(exchangeReservationToPropertyBase * extra.Total_VAT, 4);
                    }

                    foreach (var policy in room.ReservationRoomTaxPolicies)
                    {
                        if (policy.TaxIsPercentage != true)
                            policy.TaxDefaultValue = policy.TaxDefaultValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * policy.TaxDefaultValue.Value, 4) : 0;

                        policy.TaxCalculatedValue = policy.TaxCalculatedValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * policy.TaxCalculatedValue.Value, 4) : 0;
                    }
                }

                #endregion

                #region Reservation.PaymentDetails

                foreach (var payDetail in newReservation.ReservationPaymentDetails)
                    payDetail.Amount = payDetail.Amount.HasValue ? decimal.Round(exchangeReservationToPropertyBase * payDetail.Amount.Value, 4) : 0;

                foreach (var partialPayDetail in newReservation.ReservationPartialPaymentDetails)
                    partialPayDetail.Amount = decimal.Round(exchangeReservationToPropertyBase * partialPayDetail.Amount, 4);

                #endregion
            }

            // TODO: retirar a conversão do additionalData daqui e colocar num método à parte para ser chamado no Save do AdditionalData (feito em background)

            #region Reservation Additional Data
            if (reservationAdditionalData != null)
            {
                //ExternalSellingReservationInformation
                if (reservationAdditionalData.ExternalSellingReservationInformationByRule != null)
                {
                    foreach (var externalSellingReservation in reservationAdditionalData.ExternalSellingReservationInformationByRule)
                    {
                        externalSellingReservation.RoomsPriceSum = decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.RoomsPriceSum, 4);
                        externalSellingReservation.RoomsTotalAmount = decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.RoomsTotalAmount, 4);
                        externalSellingReservation.TotalAmount = decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.TotalAmount, 4);
                        externalSellingReservation.TotalTax = externalSellingReservation.TotalTax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.TotalTax.Value, 4) : 0;
                        externalSellingReservation.TotalPOTax = externalSellingReservation.TotalPOTax.HasValue ? decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.TotalPOTax.Value, 4) : 0;
                        if (externalSellingReservation.KeeperType != PO_KeeperType.Representative || (externalSellingReservation.KeeperType == PO_KeeperType.Representative && externalSellingReservation.CommissionIsPercentage))
                            externalSellingReservation.TotalCommission = decimal.Round(exchangeReservationToPropertyBase * externalSellingReservation.TotalCommission, 4);
                    }
                }
                //ExternalSellingInformation
                if (reservationAdditionalData.ReservationRoomList != null)
                {
                    if (roomNoAdditionalDataToUpdate != null)
                        foreach (var roomNo in roomNoAdditionalDataToUpdate)
                        {
                            var room = reservationAdditionalData.ReservationRoomList.FirstOrDefault(x => x.ReservationRoomNo == roomNo);
                            ApplyExchangeRateToReservationRoomAdditionalData(room, exchangeReservationToPropertyBase);
                        }
                    else
                        foreach (var room in reservationAdditionalData.ReservationRoomList)
                            ApplyExchangeRateToReservationRoomAdditionalData(room, exchangeReservationToPropertyBase);
                }
            }
            #endregion

            return newReservation;
        }

        private void ApplyExchangeRateToReservationRoomAdditionalData(contractsReservations.ReservationRoomAdditionalData room, decimal exchangeReservationToPropertyBase)
        {
            if (room != null && room.ExternalSellingInformationByRule != null)
            {
                foreach (var externalSellingInformation in room.ExternalSellingInformationByRule)
                {
                    externalSellingInformation.ReservationRoomsExtrasSum = externalSellingInformation.ReservationRoomsExtrasSum.HasValue
                        ? decimal.Round(exchangeReservationToPropertyBase * externalSellingInformation.ReservationRoomsExtrasSum.Value, 4)
                        : 0;
                    externalSellingInformation.ReservationRoomsPriceSum = decimal.Round(exchangeReservationToPropertyBase * externalSellingInformation.ReservationRoomsPriceSum, 4);
                    externalSellingInformation.ReservationRoomsTotalAmount = decimal.Round(exchangeReservationToPropertyBase * externalSellingInformation.ReservationRoomsTotalAmount, 4);
                    externalSellingInformation.TotalTax = externalSellingInformation.TotalTax.HasValue
                        ? decimal.Round(exchangeReservationToPropertyBase * externalSellingInformation.TotalTax.Value, 4)
                        : 0;
                    //prices per day
                    if (externalSellingInformation.PricesPerDay != null)
                        foreach (var prices in externalSellingInformation.PricesPerDay)
                            prices.Price = decimal.Round(exchangeReservationToPropertyBase * prices.Price, 4);
                    //taxes per day
                    if (externalSellingInformation.TaxesPerDay != null)
                        foreach (var prices in externalSellingInformation.TaxesPerDay)
                            prices.Price = decimal.Round(exchangeReservationToPropertyBase * prices.Price, 4);
                    //tax policies
                    if (externalSellingInformation.TaxPolicies != null)
                        foreach (var tax in externalSellingInformation.TaxPolicies)
                            tax.TaxCalculatedValue = tax.TaxCalculatedValue.HasValue ? decimal.Round(exchangeReservationToPropertyBase * tax.TaxCalculatedValue.Value, 4) : 0;
                }
            }
        }

        #endregion

        #region CONVERT RESERVATION TO RATE CURRENCY

        public bool ConvertAllValuesToRateCurrency(contractsReservations.Reservation reservationWithoutConvertions)
        {
            if (reservationWithoutConvertions == null || !reservationWithoutConvertions.PropertyBaseCurrencyExchangeRate.HasValue)
                return false;
            if (reservationWithoutConvertions.PropertyBaseCurrencyExchangeRate == 1)
                return true;

            decimal rateExchangePropertyBaseCurrency = 1 / reservationWithoutConvertions.PropertyBaseCurrencyExchangeRate.Value;

            #region Reservation Base Object

            reservationWithoutConvertions.RoomsExtras = reservationWithoutConvertions.RoomsExtras.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.RoomsExtras.Value, 4) : 0;
            reservationWithoutConvertions.RoomsPriceSum = reservationWithoutConvertions.RoomsPriceSum.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.RoomsPriceSum.Value, 4) : 0;
            reservationWithoutConvertions.RoomsTax = reservationWithoutConvertions.RoomsTax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.RoomsTax.Value, 4) : 0;
            reservationWithoutConvertions.RoomsTotalAmount = reservationWithoutConvertions.RoomsTotalAmount.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.RoomsTotalAmount.Value, 4) : 0;
            reservationWithoutConvertions.Tax = reservationWithoutConvertions.Tax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.Tax.Value, 4) : 0;
            reservationWithoutConvertions.TotalAmount = reservationWithoutConvertions.TotalAmount.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.TotalAmount.Value, 4) : 0;
            reservationWithoutConvertions.TotalTax = reservationWithoutConvertions.TotalTax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.TotalTax.Value, 4) : 0;
            reservationWithoutConvertions.PaymentAmountCaptured = reservationWithoutConvertions.PaymentAmountCaptured.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.PaymentAmountCaptured.Value, 4) : 0;
            reservationWithoutConvertions.ChannelProperties_Value = reservationWithoutConvertions.ChannelProperties_Value.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.ChannelProperties_Value.Value, 4) : 0;
            reservationWithoutConvertions.InstallmentAmount = reservationWithoutConvertions.InstallmentAmount.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.InstallmentAmount.Value, 4) : 0;
            reservationWithoutConvertions.SalesmanCommission = reservationWithoutConvertions.SalesmanCommission.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.SalesmanCommission.Value, 4) : 0;

            #endregion

            #region Reservation.ReservationRooms

            if (reservationWithoutConvertions.ReservationRooms != null && reservationWithoutConvertions.ReservationRooms.Any())
                foreach (var room in reservationWithoutConvertions.ReservationRooms)
                {
                    if (room.CancellationPaymentModel != 2)
                        room.CancellationValue = room.CancellationValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.CancellationValue.Value, 4) : 0;

                    room.ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.ReservationRoomsExtrasSum.Value, 4) : 0;
                    room.ReservationRoomsPriceSum = room.ReservationRoomsPriceSum.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.ReservationRoomsPriceSum.Value, 4) : 0;
                    room.ReservationRoomsTotalAmount = room.ReservationRoomsTotalAmount.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.ReservationRoomsTotalAmount.Value, 4) : 0;
                    room.TPIDiscountValue = room.TPIDiscountValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.TPIDiscountValue.Value, 4) : 0;
                    room.TotalTax = room.TotalTax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * room.TotalTax.Value, 4) : 0;

                    if (room.ReservationRoomDetails != null && room.ReservationRoomDetails.Any())
                        foreach (var detail in room.ReservationRoomDetails)
                        {
                            detail.AdultPrice = detail.AdultPrice.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * detail.AdultPrice.Value, 4) : 0;
                            detail.ChildPrice = detail.ChildPrice.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * detail.ChildPrice.Value, 4) : 0;
                            detail.Price = decimal.Round(rateExchangePropertyBaseCurrency * detail.Price, 4);

                            if (detail.ReservationRoomDetailsAppliedIncentives != null && detail.ReservationRoomDetailsAppliedIncentives.Any())
                            {
                                foreach (var incentive in detail.ReservationRoomDetailsAppliedIncentives)
                                    incentive.DiscountValue = incentive.DiscountValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * incentive.DiscountValue.Value, 4) : 0;
                            }
                        }

                    if (room.ReservationRoomExtras != null && room.ReservationRoomExtras.Any())
                        foreach (var extra in room.ReservationRoomExtras)
                        {
                            extra.Total_Price = decimal.Round(rateExchangePropertyBaseCurrency * extra.Total_Price, 4);
                            extra.Total_VAT = decimal.Round(rateExchangePropertyBaseCurrency * extra.Total_VAT, 4);
                        }

                    if (room.ReservationRoomTaxPolicies != null && room.ReservationRoomTaxPolicies.Any())
                        foreach (var policy in room.ReservationRoomTaxPolicies)
                        {
                            if (policy.TaxIsPercentage != true)
                                policy.TaxDefaultValue = policy.TaxDefaultValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * policy.TaxDefaultValue.Value, 4) : 0;

                            policy.TaxCalculatedValue = policy.TaxCalculatedValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * policy.TaxCalculatedValue.Value, 4) : 0;
                        }
                }

            #endregion

            #region Reservation.PaymentDetails

            if (reservationWithoutConvertions.ReservationPaymentDetail != null)
            {
                reservationWithoutConvertions.ReservationPaymentDetail.Amount = reservationWithoutConvertions.ReservationPaymentDetail.Amount.HasValue ?
                    decimal.Round(rateExchangePropertyBaseCurrency * reservationWithoutConvertions.ReservationPaymentDetail.Amount.Value, 4) : 0;
            }

            if (reservationWithoutConvertions.ReservationPartialPaymentDetails != null && reservationWithoutConvertions.ReservationPartialPaymentDetails.Any())
                foreach (var partialPayDetail in reservationWithoutConvertions.ReservationPartialPaymentDetails)
                    partialPayDetail.Amount = decimal.Round(rateExchangePropertyBaseCurrency * partialPayDetail.Amount, 4);

            #endregion

            #region Reservation Additional Data

            if (reservationWithoutConvertions.ReservationAdditionalData != null)
            {
                //ExternalSellingReservationInformation
                var externalResInfoList = reservationWithoutConvertions.ReservationAdditionalData.ExternalSellingReservationInformationByRule;
                if (externalResInfoList != null)
                {
                    foreach (var externalSellingReservation in externalResInfoList)
                    {
                        externalSellingReservation.RoomsPriceSum = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.RoomsPriceSum, 4);
                        externalSellingReservation.RoomsTotalAmount = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.RoomsTotalAmount, 4);
                        externalSellingReservation.TotalAmount = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.TotalAmount, 4);
                        externalSellingReservation.TotalTax = externalSellingReservation.TotalTax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.TotalTax.Value, 4) : 0;
                        externalSellingReservation.TotalPOTax = externalSellingReservation.TotalPOTax.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.TotalPOTax.Value, 4) : 0;
                        if (externalSellingReservation.KeeperType != PO_KeeperType.Representative || (externalSellingReservation.KeeperType == PO_KeeperType.Representative && externalSellingReservation.CommissionIsPercentage))
                            externalSellingReservation.TotalCommission = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingReservation.TotalCommission, 4);
                    }
                }

                //ExternalSellingInformation
                if (reservationWithoutConvertions.ReservationAdditionalData.ReservationRoomList != null)
                    foreach (var room in reservationWithoutConvertions.ReservationAdditionalData.ReservationRoomList.Where(x => x.ExternalSellingInformationByRule != null))
                    {
                        if (room.ExternalSellingInformationByRule == null)
                            continue;

                        foreach (var externalSellingInfo in room.ExternalSellingInformationByRule)
                        {
                            externalSellingInfo.ReservationRoomsExtrasSum = externalSellingInfo.ReservationRoomsExtrasSum.HasValue ?
                                decimal.Round(rateExchangePropertyBaseCurrency * externalSellingInfo.ReservationRoomsExtrasSum.Value, 4) : 0;
                            externalSellingInfo.ReservationRoomsPriceSum = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingInfo.ReservationRoomsPriceSum, 4);
                            externalSellingInfo.ReservationRoomsTotalAmount = decimal.Round(rateExchangePropertyBaseCurrency * externalSellingInfo.ReservationRoomsTotalAmount, 4);
                            externalSellingInfo.TotalTax = externalSellingInfo.TotalTax.HasValue ?
                                decimal.Round(rateExchangePropertyBaseCurrency * externalSellingInfo.TotalTax.Value, 4) : 0;

                            //prices per day
                            if (externalSellingInfo.PricesPerDay != null)
                                foreach (var prices in externalSellingInfo.PricesPerDay)
                                    prices.Price = decimal.Round(rateExchangePropertyBaseCurrency * prices.Price, 4);

                            //taxes per day
                            if (externalSellingInfo.TaxesPerDay != null)
                                foreach (var prices in externalSellingInfo.TaxesPerDay)
                                    prices.Price = decimal.Round(rateExchangePropertyBaseCurrency * prices.Price, 4);

                            //tax policies
                            if (externalSellingInfo.TaxPolicies != null)
                                foreach (var tax in externalSellingInfo.TaxPolicies)
                                    tax.TaxCalculatedValue = tax.TaxCalculatedValue.HasValue ? decimal.Round(rateExchangePropertyBaseCurrency * tax.TaxCalculatedValue.Value, 4) : 0;
                        }
                    }
            }

            #endregion

            return true;
        }

        #endregion

        #region GET RULES FROM PORTAL
        public IEnumerable<SellRule> GetRulesFromPortal(long channelUID, long propertyUID,
            long? externalTpiId, long? externalChannelId, long? tpiId, long? currencyId, Guid? requestGuid = null)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var portalRepo = RepositoryFactory.GetPortalRepository(unitOfWork);
                var isObTpi = !externalTpiId.HasValue;
                var rules = portalRepo.ListMarkupCommissionRules(new PO.BL.Contracts.Requests.ListMarkupCommissionRulesRequest
                {
                    Channel_UID = channelUID,
                    Property_ID = propertyUID,
                    ExternalTPI_UID = externalTpiId,
                    ExternalChannel_UID = externalChannelId,
                    TPI_UID = tpiId,
                    CurrencyUID = currencyId,
                    IsOBTPI = isObTpi,
                    RuleType = PO.BL.Constants.RuleType.Omnibees,
                    RequestGuid = requestGuid ?? Guid.Empty
                });
                return rules;
            }
        }
        #endregion

        #region PULL TPI RESERVATION

        /// <summary>
        /// Treat Reservation B2B - US 30858
        /// </summary>
        /// <param name="parameters"></param>
        public void TreatPullTpiReservation(TreatPullTpiReservationParameters parameters)
        {
            if (!parameters.Reservation.Channel_UID.HasValue)
                throw Errors.InvalidChannel.ToBusinessLayerException();

            if (parameters.ReservationRoomDetails == null || !parameters.ReservationRoomDetails.Any())
                throw Errors.RequiredParameter.ToBusinessLayerException(nameof(parameters.ReservationRoomDetails));

            var reservationHelper = Resolve<IReservationHelperPOCO>();
            var reservationPricesPOCO = Resolve<IReservationPricesCalculationPOCO>();
            var rrdList = new List<OBcontractsRates.RateRoomDetailReservation>();

            OB.BL.Contracts.Data.CRM.LoyaltyProgram loyaltyProgram = null;
            var strFailMargin = WebConfigurationManager.AppSettings["PullReservationErrorMargin"];
            if (!decimal.TryParse(strFailMargin, out decimal failMargin))
            {
                failMargin = 0.99M;
            }

            #region Get rule
            var rules = GetRulesFromPortal(parameters.Reservation.Channel_UID.Value, parameters.Reservation.Property_UID,
                 parameters.AddicionalData.ExternalTpiId, parameters.AddicionalData.ExternalChannelId, parameters.Reservation.TPI_UID, parameters.Reservation.ReservationCurrency_UID, parameters.RequestGuid);
            rules = rules.GroupBy(x => x.KeeperType).Select(x => x.OrderByDescending(j => j.RuleType).FirstOrDefault()).ToList();
            rules = rules.OrderByDescending(x => x.KeeperType);
            #endregion Get rule

            #region SAVE PULL PRICES FROM RESERVATION TEMP
            var externalSellingReservationInfoPullTemp = new contractsReservations.ExternalSellingReservationInformation
            {
                TotalAmount = parameters.Reservation.TotalAmount ?? 0,
                RoomsPriceSum = parameters.Reservation.RoomsPriceSum ?? 0,
                RoomsTotalAmount = parameters.Reservation.RoomsTotalAmount ?? 0,
                TotalTax = parameters.Reservation.TotalTax,
                RoomsTax = parameters.Reservation.RoomsTax
            };
            #endregion SAVE PULL PRICES FROM RESERVATION TEMP

            parameters.AddicionalData.ExternalName = rules != null && rules.Any() ? rules.First().ExternalName : string.Empty;

            if (parameters.AddicionalData.ExternalSellingReservationInformationByRule == null)
                parameters.AddicionalData.ExternalSellingReservationInformationByRule = new List<contractsReservations.ExternalSellingReservationInformation>();

            #region Map configuration of rules
            if (rules != null && rules.Any())
            {
                foreach (var rule in rules)
                {
                    if (rule.MarkupType == 0)
                        rule.MarkupType = ExternalApplianceType.Define;
                    if (rule.CommissionType == 0)
                        rule.CommissionType = ExternalApplianceType.Define;

                    var extSellingReservationInformation = new contractsReservations.ExternalSellingReservationInformation();
                    var ruleInfo = parameters.AddicionalData.ExternalSellingReservationInformationByRule.Where(x => (int)x.KeeperType == rule.KeeperType).FirstOrDefault();
                    extSellingReservationInformation.IsPaid = parameters.IsInsert ? false : ruleInfo != null ? ruleInfo.IsPaid : false;
                    extSellingReservationInformation.KeeperUID = rule.KeeperUid;
                    extSellingReservationInformation.KeeperType = (PO_KeeperType)rule.KeeperType;
                    extSellingReservationInformation.Markup = rule.Markup;
                    extSellingReservationInformation.MarkupType = (int)rule.MarkupType;
                    extSellingReservationInformation.MarkupIsPercentage = rule.MarkupIsPercentage;
                    extSellingReservationInformation.Commission = rule.Commission;
                    extSellingReservationInformation.CommissionType = (int)rule.CommissionType;
                    extSellingReservationInformation.CommissionIsPercentage = rule.CommissionIsPercentage;
                    extSellingReservationInformation.Tax = rule.Tax;
                    extSellingReservationInformation.TaxIsPercentage = rule.TaxIsPercentage;
                    extSellingReservationInformation.CurrencyUID = rule.CurrencyValueUID != 0 ? rule.CurrencyValueUID : rule.CurrencyBaseUID;
                    if (extSellingReservationInformation.CurrencyUID > 0)
                        extSellingReservationInformation.ExchangeRate =
                        this.GetExchangeRateBetweenCurrenciesByPropertyId(parameters.ReservationContext.PropertyBaseCurrency_UID ?? extSellingReservationInformation.CurrencyUID, extSellingReservationInformation.CurrencyUID, parameters.Reservation.Property_UID);

                    parameters.AddicionalData.ExternalSellingReservationInformationByRule.Add(extSellingReservationInformation);
                }
            }
            #endregion Map configuration of rules

            #region Get Loyalty program
            if (parameters.Reservation.Guest_UID > 0 && parameters.GroupRule.BusinessRules.HasFlag(domainReservations.BusinessRules.LoyaltyDiscount))
            {
                var crmRepo = RepositoryFactory.GetOBCRMRepository();
                int total = 0;
                if (parameters.Guest.LoyaltyLevel_UID.HasValue)
                {
                    var loyaltyRsp = crmRepo.ListLoyaltyPrograms(new OB.BL.Contracts.Requests.ListLoyaltyProgramRequest
                    {
                        Client_UIDs = new List<long> { parameters.ReservationContext.Client_UID },
                        IncludeDeleted = false,
                        IncludeDefaultCurrency = true,
                        IncludeDefaultLanguage = false,
                        IncludeLoyaltyLevels = true,
                        IncludeLoyaltyLevelsCurrencies = true,
                        IncludeLoyaltyLevelsLanguages = false,
                        IncludeLoyaltyProgramLanguages = false,
                        ExcludeInactive = true,
                        LoyaltyLevel_Uids = new List<long> { parameters.Guest.LoyaltyLevel_UID.Value }
                    });

                    total = loyaltyRsp.TotalRecords;
                    loyaltyProgram = loyaltyRsp.Results.FirstOrDefault();
                }
            }
            #endregion Get Loyalty program

            #region CALCULATE OB PRICES

            // Get valid rates for promocode
            var validatePromoRequest = new ValidatePromocodeForReservationParameters()
            {
                ReservationRooms = parameters.Rooms.Select(rr => new ReservationRoomStayPeriod()
                {
                    RateUID = rr.Rate_UID.Value,
                    CheckIn = rr.DateFrom.Value,
                    CheckOut = rr.DateTo.Value
                }).ToList(),
                PromocodeUID = parameters.Reservation.PromotionalCode_UID,
                CurrencyUID = parameters.Reservation.ReservationBaseCurrency_UID ?? parameters.ReservationContext.PropertyBaseCurrency_UID.Value
            };
            var validPromocodeResponse = reservationHelper.ValidatePromocodeForReservation(validatePromoRequest);

            if (validPromocodeResponse == null || validPromocodeResponse.RejectReservation)
                throw Errors.InvalidPromocodeForReservation.ToBusinessLayerException();

            // Remove promotionalCode_UID from reservation if doesn't have discounts to apply
            if (validPromocodeResponse.PromoCodeObj == null || !validPromocodeResponse.PromoCodeObj.IsValid ||
                validPromocodeResponse.NewDaysToApplyDiscount == null || !validPromocodeResponse.NewDaysToApplyDiscount.Any())
                parameters.Reservation.PromotionalCode_UID = null;

            parameters.PromotionalCode = validPromocodeResponse?.PromoCodeObj;

            #region RESERVATION ROOMS
            List<contractsReservations.ReservationRoomLight> roomsCalculationsWithExternalMarkup = new List<contractsReservations.ReservationRoomLight>();
            var roomsTemp = parameters.Rooms.Clone();
            foreach (var room in roomsTemp)
            {
                var externalSellingRoomInformationByRule = new List<contractsReservations.ExternalSellingRoomInformation>();

                //reservation room details from pull
                var rrd = parameters.ReservationRoomDetails.Where(x => x.ReservationRoom_UID == room.UID).OrderBy(x => x.Date).ToList();
                room.DateFrom = room.DateFrom.Value.Date;
                room.DateTo = room.DateTo.Value.Date;

                #region Copy prices to reservation room addicional data
                var rrAdditionalData = new contractsReservations.ReservationRoomAdditionalData();
                rrAdditionalData.ReservationRoom_UID = room.UID;
                rrAdditionalData.TaxPolicies = room.ReservationRoomTaxPolicies?.ToList();
                parameters.AddicionalData.ReservationRoomList.Add(rrAdditionalData);
                #endregion Copy prices to reservation room addicional data

                #region SAVE PULL PRICES FROM ROOM TEMP
                var externalSellingRoomInfoPullTemp = new contractsReservations.ExternalSellingRoomInformation()
                {
                    PricesPerDay = rrd.Select(x => new contractsReservations.PriceDay { Date = x.Date.Date, Price = x.Price }).ToList(),
                    ReservationRoomsPriceSum = room.ReservationRoomsPriceSum ?? 0,
                    ReservationRoomsTotalAmount = room.ReservationRoomsTotalAmount ?? 0,
                    ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum,
                    TotalTax = room.TotalTax,
                    TaxPolicies = room.ExternalReservationRoomTaxPolicies
                };
                #endregion SAVE PULL PRICES FROM ROOM TEMP

                var ages = parameters.ReservationRoomChilds != null ?
                            parameters.ReservationRoomChilds.Where(x => x.ReservationRoom_UID == room.UID).Where(x => x.Age.HasValue).Select(x => x.Age.Value).ToList()
                            : new List<int>();

                int? rateModelId = null; //OB gets rateModelId value when a reservation has filled the externalTpiId (TPI PO)
                if (!parameters.AddicionalData.ExternalTpiId.HasValue)
                {
                    rateModelId = (int?)room.CommissionType;
                    rrAdditionalData.CommissionType = rateModelId ?? 0;
                }

                var pricesParameters = new CalculateFinalPriceParameters
                {
                    CheckIn = room.DateFrom.Value,
                    CheckOut = room.DateTo.Value,
                    BaseCurrency = parameters.Reservation.ReservationBaseCurrency_UID ?? parameters.ReservationContext.PropertyBaseCurrency_UID.Value,
                    AdultCount = room.AdultCount.Value,
                    ChildCount = room.ChildCount ?? 0,
                    Ages = ages,
                    GroupRule = parameters.GroupRule,
                    //ExchangeRate = parameters.Reservation.PropertyBaseCurrencyExchangeRate ?? 1, 
                    ExchangeRate = 1, // Para que os valores sejam calculados com a moeda da tarifa para ser poder validar com os valores dos pull
                    IsModify = true,
                    PropertyId = parameters.Reservation.Property_UID,
                    RateId = room.Rate_UID.Value,
                    RoomTypeId = room.RoomType_UID.Value,
                    TpiId = parameters.Reservation.TPI_UID,
                    RateModelId = rateModelId,
                    ChannelId = parameters.ReservationContext.Channel_UID,
                    TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                    TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                    TPIDiscountValue = room.TPIDiscountValue,
                    LoyaltyProgram = loyaltyProgram,
                    ValidPromocodeParameters = validPromocodeResponse
                };

                rrdList = reservationPricesPOCO.CalculateReservationRoomPrices(pricesParameters);
                rrdList = rrdList.OrderBy(x => x.Date).ToList();

                if (parameters.AddicionalData.ExternalTpiId.HasValue)
                {
                    room.CommissionType = rrdList.First().IsMarkup ? (int)RateModels.Markup : rrdList.First().IsCommission ? (int)RateModels.Commissionable : (int)RateModels.Package;
                    room.CommissionValue = rrdList.First().RateModelValue;
                    rrAdditionalData.CommissionType = (int)RateModels.Commissionable;//reservation has filled the externalTpiId (TPI PO) the commissiontype always is commission (PVP)
                }

                var rrdListClone = rrdList.Clone();
                //if there are no rules
                var rrdWithExtMarkupRuleList = rrdListClone;

                //map values with markup
                decimal? totalTaxWithMarkup = room.TotalTax;
                decimal? rrExtrasSumWithMarkup = room.ReservationRoomsExtrasSum;

                #region CALCULATE AND MAP PRICES
                // SET RESERVATION ROOM DETAILS
                decimal total = 0;
                this.SetReservationRoomDetails(room, rrdList, out total);
                room.ReservationRoomsPriceSum = total;

                //only when have rule change the reservationroomdetails, otherwise keep the pull prices
                if (rules != null && rules.Any())
                {
                    parameters.ReservationRoomDetails.RemoveAll(x => x.ReservationRoom_UID == room.UID);
                    parameters.ReservationRoomDetails.AddRange(room.ReservationRoomDetails);
                }

                #region CALCULATE TAX POLICIES

                var roomTaxPolicies = reservationHelper.GetTaxPoliciesByRateIds(new List<long> { room.Rate_UID.Value }, parameters.Reservation.ReservationBaseCurrency_UID ?? parameters.ReservationContext.PropertyBaseCurrency_UID.Value, parameters.ReservationRequest.LanguageUID);
                SetReservationRoomTaxPolicies(room, roomTaxPolicies, 1, rrdList);

                #endregion

                //calculate totaltax to room
                if (room.ReservationRoomTaxPolicies != null)
                    room.TotalTax = room.ReservationRoomTaxPolicies.Sum(x => x.TaxCalculatedValue);

                #endregion CALCULATE AND MAP PRICES

                #region CALCULATE WITH EXTERNAL MARKUP RULES
                if (rules != null && rules.Any())
                {
                    #region CALCULATE ROOM WITH EXTERNAL MARKUP BY RULE
                    int i = 0;
                    var rulesTemp = rules.Clone();
                    foreach (var rule in rulesTemp)
                    {
                        if (i > 0 && (rule.KeeperType == (int)PO_KeeperType.Channel || rule.KeeperType == (int)PO_KeeperType.TPI_PO))
                        {
                            rule.Markup = rule.Markup + rules.First().Markup;
                            rule.Tax = rule.Tax + rules.First().Tax;
                            rule.MarkupCurrencyValue = rule.MarkupCurrencyValue + rules.First().MarkupCurrencyValue;
                            rule.TaxCurrencyValue = rule.TaxCurrencyValue + rules.First().TaxCurrencyValue;
                        }

                        rrdWithExtMarkupRuleList = reservationPricesPOCO.CalculateExternalMarkup(rrdListClone, parameters.GroupRule, rule);

                        var calculatedRRTotalAmountWithMarkupTemp = (rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice) + (rrExtrasSumWithMarkup ?? 0) + (room.TotalTax ?? 0));

                        #region taxes per day
                        var taxesPerDay = new List<contractsReservations.PriceDay>();
                        //only representatives have taxes
                        if (rule.KeeperType == (int)PO_KeeperType.Representative)
                            taxesPerDay = rrdWithExtMarkupRuleList.Where(x => x.PriceAfterExternalTaxes > 0).Select(x => new contractsReservations.PriceDay { Date = x.Date.Date, Price = (x.PriceAfterExternalTaxes - x.PriceAfterExternalMarkup) }).ToList();
                        #endregion taxes per day

                        //if calculated price is bigger than price from pull and there are two rules, then give the prices from pull to the rule of channel
                        if (rules.Count() > 1 && (rule.KeeperType == (int)PO_KeeperType.Channel || rule.KeeperType == (int)PO_KeeperType.TPI_PO) && calculatedRRTotalAmountWithMarkupTemp < externalSellingRoomInfoPullTemp.ReservationRoomsTotalAmount)
                        {
                            externalSellingRoomInfoPullTemp.KeeperUID = rule.KeeperUid;
                            externalSellingRoomInfoPullTemp.TaxesPerDay = taxesPerDay;
                            externalSellingRoomInfoPullTemp.KeeperType = (PO_KeeperType)rule.KeeperType;
                            externalSellingRoomInformationByRule.Add(externalSellingRoomInfoPullTemp);
                        }
                        else
                            externalSellingRoomInformationByRule.Add(new contractsReservations.ExternalSellingRoomInformation
                            {
                                KeeperUID = rule.KeeperUid,
                                KeeperType = (PO_KeeperType)rule.KeeperType,
                                PricesPerDay = rrdWithExtMarkupRuleList.Select(x => new contractsReservations.PriceDay { Date = x.Date.Date, Price = x.FinalPrice }).ToList(),
                                TaxesPerDay = taxesPerDay,
                                ReservationRoomsPriceSum = rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice),
                                ReservationRoomsTotalAmount = calculatedRRTotalAmountWithMarkupTemp,
                                ReservationRoomsExtrasSum = (rrExtrasSumWithMarkup ?? 0),
                                TotalTax = (room.TotalTax ?? 0),
                                TaxPolicies = room.ExternalReservationRoomTaxPolicies
                            });
                        i++;
                    }

                    rrAdditionalData.ExternalSellingInformationByRule = externalSellingRoomInformationByRule;
                    #endregion CALCULATE ROOM WITH EXTERNAL MARKUP BY RULE
                }
                #endregion

                //calculate reservationroomstotalamount to room
                room.ReservationRoomsTotalAmount = (room.ReservationRoomsPriceSum ?? 0) + (room.ReservationRoomsExtrasSum ?? 0) + (room.TotalTax ?? 0);

                // calcule ReservationRoomsPriceSum 
                decimal? reservationRoomsPriceSumWithMarkup = rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice);
                // calcule ReservationRoomsTotalAmount
                decimal? reservationRoomsTotalAmountWithMarkup = (reservationRoomsPriceSumWithMarkup ?? 0) +
                                                                 (rrExtrasSumWithMarkup ?? 0) +
                                                                 (totalTaxWithMarkup ?? 0);
                //helper calculate with external markup
                roomsCalculationsWithExternalMarkup.Add(new contractsReservations.ReservationRoomLight()
                {
                    ReservationRoomsPriceSum = reservationRoomsPriceSumWithMarkup,
                    ReservationRoomsTotalAmount = reservationRoomsTotalAmountWithMarkup
                });

                if (rrd.Count != rrdWithExtMarkupRuleList.Count)
                    throw Errors.RateRoomDetailsAreNotSet.ToBusinessLayerException();

                #region VALIDATE PRICE PER DAY
                ValidatePricePerDay(rrdWithExtMarkupRuleList, rrd, failMargin, parameters.ReservationRequest);
                #endregion VALIDATE PRICE PER DAY

                #region VALIDATE RESERVATIONROOMS PRICES
                var calculatedRRDToValidate = new contractsReservations.ReservationRoom
                {
                    ReservationRoomsPriceSum = reservationRoomsPriceSumWithMarkup,
                    ReservationRoomsTotalAmount = reservationRoomsTotalAmountWithMarkup,
                    ReservationRoomsExtrasSum = (rrExtrasSumWithMarkup ?? 0),
                    TotalTax = (room.TotalTax ?? 0)
                };

                var rrdToValidate = new contractsReservations.ReservationRoom
                {
                    ReservationRoomsPriceSum = externalSellingRoomInfoPullTemp.ReservationRoomsPriceSum,
                    ReservationRoomsTotalAmount = externalSellingRoomInfoPullTemp.ReservationRoomsTotalAmount,
                    ReservationRoomsExtrasSum = externalSellingRoomInfoPullTemp.ReservationRoomsExtrasSum,
                    TotalTax = externalSellingRoomInfoPullTemp.TotalTax
                };

                ValidateReservationRoomsPrices(calculatedRRDToValidate, rrdToValidate, failMargin, parameters.ReservationRequest);
                #endregion VALIDATE RESERVATIONROOMS PRICES

            }
            //only when have rule change the rooms, otherwise keep the pull prices
            if (rules != null && rules.Any())
                parameters.Rooms = roomsTemp;

            #endregion RESERVATION ROOMS

            #region RESERVATION
            parameters.Reservation.TotalAmount = (parameters.Rooms.Sum(x => x.ReservationRoomsTotalAmount));
            parameters.Reservation.RoomsTotalAmount = (parameters.Rooms.Sum(x => x.ReservationRoomsTotalAmount) ?? 0);
            parameters.Reservation.RoomsPriceSum = (parameters.Rooms.Sum(x => x.ReservationRoomsPriceSum) ?? 0);
            parameters.Reservation.RoomsTax = (parameters.Rooms.Sum(x => x.TotalTax) ?? 0);
            parameters.Reservation.TotalTax = (parameters.Rooms.Sum(x => x.TotalTax) ?? 0);
            #endregion RESERVATION

            #endregion CALCULATE OB PRICES 

            #region Calculate totals to verification
            var totalAmount = (roomsCalculationsWithExternalMarkup.Sum(x => x.ReservationRoomsTotalAmount) ?? 0);
            var roomsTotalAmount = (roomsCalculationsWithExternalMarkup.Sum(x => x.ReservationRoomsTotalAmount) ?? 0);
            var roomsPriceSum = (roomsCalculationsWithExternalMarkup.Sum(x => x.ReservationRoomsPriceSum) ?? 0);
            #endregion

            #region Copy Prices to Additional Data
            if (rules != null && rules.Any())
            {
                foreach (var rule in rules)
                {
                    var externalSellingResInfo = parameters.AddicionalData.ExternalSellingReservationInformationByRule.Where(x => (int)x.KeeperType == rule.KeeperType).FirstOrDefault();
                    var allExternalSellingInformationByRule = parameters.AddicionalData.ReservationRoomList.Select(x => x.ExternalSellingInformationByRule).ToList();
                    var externalSellingInformationByRule = allExternalSellingInformationByRule.Select(x => x.Where(j => (int)j.KeeperType == rule.KeeperType).First()).ToList();
                    if (externalSellingResInfo != null && externalSellingInformationByRule != null && externalSellingInformationByRule.Any())
                    {
                        externalSellingResInfo.TotalAmount = externalSellingInformationByRule.Sum(x => x.ReservationRoomsTotalAmount);
                        externalSellingResInfo.RoomsPriceSum = externalSellingInformationByRule.Sum(x => x.ReservationRoomsPriceSum);
                        externalSellingResInfo.RoomsTotalAmount = externalSellingResInfo.TotalAmount;
                        externalSellingResInfo.TotalTax = externalSellingInformationByRule.Sum(x => x.TotalTax);
                        externalSellingResInfo.RoomsTax = externalSellingResInfo.TotalTax;
                        externalSellingResInfo.TotalPOTax = externalSellingInformationByRule.Sum(x => x.TaxesPerDay.Sum(y => y.Price));

                        #region MAP EXTERNAL TOTAL COMMISSION

                        externalSellingResInfo.TotalCommission = rule.CommissionIsPercentage
                                ? reservationPricesPOCO.CalculateExternalCommission(rule.Commission, rule.CurrencyBaseUID,
                                    parameters.Reservation.ReservationBaseCurrency_UID, null, totalAmount)
                                : rule.Commission;

                        #endregion
                    }
                }
            }
            #endregion Copy Prices to Additional Data

            #region VALIDATE RESERVATION PRICES
            var calculatedResToValidate = new contractsReservations.Reservation
            {
                TotalTax = parameters.Reservation.TotalTax,
                RoomsTax = parameters.Reservation.RoomsTax,
                RoomsPriceSum = roomsPriceSum,
                RoomsTotalAmount = roomsTotalAmount,
                TotalAmount = totalAmount
            };

            var resToValidate = new contractsReservations.Reservation
            {
                TotalTax = externalSellingReservationInfoPullTemp.TotalTax,
                RoomsTax = externalSellingReservationInfoPullTemp.RoomsTax,
                RoomsPriceSum = externalSellingReservationInfoPullTemp.RoomsPriceSum,
                RoomsTotalAmount = externalSellingReservationInfoPullTemp.RoomsTotalAmount,
                TotalAmount = externalSellingReservationInfoPullTemp.TotalAmount
            };

            ValidateReservationPrices(calculatedResToValidate, resToValidate, failMargin, parameters.ReservationRequest);
            #endregion VALIDATE RESERVATION PRICES

            #region clean reservationadditionaldata if there no rule
            //us 35649
            if (rules == null || !rules.Any())
            {
                parameters.AddicionalData.ExternalSellingReservationInformationByRule = null;
                parameters.AddicionalData.ReservationRoomList.ForEach(x => x.ExternalSellingInformationByRule = null);
            }
            #endregion clean reservationaddicionaldata if there no rule

            parameters.AddicionalData.PaymentMethodTypeNotConfiguredInRate = parameters.PaymentMethodTypeNotConfiguredInRate;
        }

        public void MapSellingPrices(contractsReservations.Reservation reservation, bool includeReservationRooms, bool includeReservationRoomDetails)
        {
            if (reservation.ReservationAdditionalData != null)
            {
                if (reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule != null)
                {
                    var externalSellingResInfo =
                        reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.LastOrDefault();
                    if (externalSellingResInfo != null)
                    {
                        reservation.TotalAmount = externalSellingResInfo.TotalAmount;
                        reservation.RoomsTotalAmount = externalSellingResInfo.RoomsTotalAmount;
                        reservation.RoomsPriceSum = externalSellingResInfo.RoomsPriceSum;
                        reservation.TotalTax = externalSellingResInfo.TotalTax;
                    }
                }

                if (includeReservationRooms)
                {
                    foreach (var rarr in reservation.ReservationAdditionalData.ReservationRoomList)
                    {
                        if (rarr.ExternalSellingInformationByRule != null)
                        {
                            var rr = reservation.ReservationRooms.FirstOrDefault(x => x.UID == rarr.ReservationRoom_UID);
                            if (rr != null)
                            {
                                rr.CommissionType = rarr.CommissionType;
                                var externalSellingRoomInfo = rarr.ExternalSellingInformationByRule.LastOrDefault();
                                if (externalSellingRoomInfo != null)
                                {
                                    rr.ReservationRoomsTotalAmount = externalSellingRoomInfo.ReservationRoomsTotalAmount;
                                    rr.ReservationRoomsPriceSum = externalSellingRoomInfo.ReservationRoomsPriceSum;
                                    rr.ReservationRoomsExtrasSum = externalSellingRoomInfo.ReservationRoomsExtrasSum;
                                    rr.TotalTax = externalSellingRoomInfo.TotalTax;
                                    if (includeReservationRoomDetails)
                                    {
                                        foreach (var rrd in rr.ReservationRoomDetails)
                                        {
                                            var PriceDay = externalSellingRoomInfo.PricesPerDay.FirstOrDefault(x => x.Date == rrd.Date);
                                            if (PriceDay != null)
                                                rrd.Price = PriceDay.Price;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region RESERVATION FILTERS
        public void TreatReservationFilter(TreatReservationFiltersParameters parameters, IUnitOfWork unitOfWork)
        {
            var propertiesRepo = this.RepositoryFactory.GetOBPropertyRepository();
            var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
            var visualStatesRepo = this.RepositoryFactory.GetVisualStateRepository(unitOfWork);
            var currenciesRepo = this.RepositoryFactory.GetOBCurrencyRepository();

            var currencies = currenciesRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest());

            var channel = channelRepo.ListChannelLight(new Contracts.Requests.ListChannelLightRequest { ChannelUIDs = new List<long>() { parameters.NewReservation.Channel_UID ?? 0 } }).FirstOrDefault();

            parameters.NewReservation.Channel = new Reservation.BL.Contracts.Data.Channels.Channel
            {
                Name = channel != null ? ((channel.UID == 1 && parameters.NewReservation.IsMobile.HasValue &&
                   parameters.NewReservation.IsMobile.Value)
                    ? channel.Name + " " + Resources.Resources.lblMobile
                    : channel.Name)
                : string.Empty
            };

            var property = propertiesRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long>() { parameters.NewReservation.Property_UID } }).FirstOrDefault();
            var propertyCurrency = currencies.FirstOrDefault(x => x.UID == (property != null ? property.BaseCurrency_UID : 0));
            parameters.NewReservation.PropertyBaseCurrencySymbol = propertyCurrency != null ? propertyCurrency.CurrencySymbol : string.Empty;

            var reservationBaseCurrency = currencies.FirstOrDefault(x => x.UID == parameters.NewReservation.ReservationBaseCurrency_UID);
            parameters.NewReservation.ReservationBaseCurrencySymbol = reservationBaseCurrency != null ? reservationBaseCurrency.CurrencySymbol : string.Empty;

            long propCurrencyId = propertyCurrency != null ? propertyCurrency.UID : 0;
            parameters.NewReservation.ReservationBaseCurrencyExchangeRate = this.GetExchangeRateBetweenCurrenciesByPropertyId(propCurrencyId, parameters.NewReservation.ReservationBaseCurrency_UID ?? 0, parameters.NewReservation.Property_UID);

            if (parameters.ReservationFilter == null)
            {
                parameters.ReservationFilter = new Domain.Reservations.ReservationFilter();
                parameters.ReservationFilter.ReservationRoomFilters = new List<Domain.Reservations.ReservationRoomFilter>();
            }
            parameters.ReservationFilter.UID = parameters.NewReservation.UID;
            parameters.ReservationFilter.CreatedDate = (System.DateTime)parameters.NewReservation.CreatedDate;
            parameters.ReservationFilter.PropertyUid = parameters.NewReservation.Property_UID;
            parameters.ReservationFilter.PropertyName = property != null ? property.Name : string.Empty;
            parameters.ReservationFilter.Number = parameters.NewReservation.Number;
            parameters.ReservationFilter.IsOnRequest = parameters.NewReservation.IsOnRequest ?? false;
            if (parameters.ServiceName != contractsGeneral.ServiceName.InsertReservation)
            {
                var str = parameters.NewReservation.UID.ToString();
                var visualState = visualStatesRepo.GetQuery(x => x.LookupKey_1 == str).FirstOrDefault();
                if (visualState != null)
                {
                    OB.Reservation.BL.Contracts.Data.VisualStates.ReservationReadStatus reservationReadStatus = null;
                    try { reservationReadStatus = visualState.JSONData.FromJSON<OB.Reservation.BL.Contracts.Data.VisualStates.ReservationReadStatus>(); } catch { }
                    parameters.ReservationFilter.IsReaded = reservationReadStatus != null ? reservationReadStatus.Read : false;
                }
            }
            else
                parameters.ReservationFilter.IsReaded = false;

            string guestFirstName = !string.IsNullOrEmpty(parameters.NewReservation.GuestFirstName) ?
                parameters.NewReservation.GuestFirstName : parameters.Guest?.FirstName;
            string guestLastName = !string.IsNullOrEmpty(parameters.NewReservation.GuestLastName) ?
                parameters.NewReservation.GuestLastName : parameters.Guest?.LastName;

            parameters.ReservationFilter.ModifiedDate = parameters.NewReservation.ModifyDate;
            parameters.ReservationFilter.Guest_UID = parameters.NewReservation.Guest_UID;
            parameters.ReservationFilter.GuestName = string.Format("{0} {1}", guestFirstName, guestLastName).Trim();
            parameters.ReservationFilter.NumberOfAdults = parameters.NewReservation.Adults;
            parameters.ReservationFilter.NumberOfChildren = parameters.NewReservation.Children;
            parameters.ReservationFilter.NumberOfRooms = parameters.NewReservation.NumberOfRooms;
            parameters.ReservationFilter.TPI_UID = parameters.NewReservation.TPI_UID;
            parameters.ReservationFilter.PaymentTypeUid = parameters.NewReservation.PaymentMethodType_UID;
            parameters.ReservationFilter.Status = parameters.NewReservation.Status;
            parameters.ReservationFilter.TotalAmount = parameters.NewReservation.TotalAmount;
            parameters.ReservationFilter.IsMobile = parameters.NewReservation.IsMobile;
            parameters.ReservationFilter.LoyaltyCardNumber = parameters.NewReservation.LoyaltyCardNumber;
            if (parameters.NewReservation.ReservationAdditionalData != null)
            {
                parameters.ReservationFilter.ExternalChannelUid = parameters.NewReservation.ReservationAdditionalData.ExternalChannelId;
                parameters.ReservationFilter.ExternalTPIUid = parameters.NewReservation.ReservationAdditionalData.ExternalTpiId;
                parameters.ReservationFilter.ExternalName = parameters.NewReservation.ReservationAdditionalData.ExternalName;

                var externalSellingReservationInformation = parameters.NewReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule;
                if (externalSellingReservationInformation != null && externalSellingReservationInformation.Any())
                {
                    parameters.ReservationFilter.ExternalTotalAmount = externalSellingReservationInformation.Last().TotalAmount;
                    parameters.ReservationFilter.ExternalCommissionValue = externalSellingReservationInformation.Last().TotalCommission;
                    parameters.ReservationFilter.ExternalIsPaid = externalSellingReservationInformation.Last().IsPaid;

                    var representativeCurrency = currencies.FirstOrDefault(x => x.UID == externalSellingReservationInformation.Last().CurrencyUID);
                    if (externalSellingReservationInformation.Last().CurrencyUID > 0)
                        parameters.ReservationFilter.RepresentativeCurrencyExchangeRate = this.GetExchangeRateBetweenCurrenciesByPropertyId(propertyCurrency.UID, externalSellingReservationInformation.Last().CurrencyUID, parameters.NewReservation.Property_UID);
                    parameters.ReservationFilter.RepresentativeCurrencySymbol = representativeCurrency != null ? representativeCurrency.CurrencySymbol : string.Empty;
                }
                parameters.ReservationFilter.TPI_Name = !string.IsNullOrEmpty(parameters.NewReservation.ReservationAdditionalData.CompanyName) ? parameters.NewReservation.ReservationAdditionalData.CompanyName : parameters.NewReservation.ReservationAdditionalData.AgencyName;
                parameters.ReservationFilter.PartnerUid = parameters.NewReservation.ReservationAdditionalData.ChannelPartnerID;
                parameters.ReservationFilter.PartnerReservationNumber = parameters.NewReservation.ReservationAdditionalData.PartnerReservationNumber;
            }
            parameters.ReservationFilter.ChannelUid = parameters.NewReservation.Channel_UID;
            parameters.ReservationFilter.ChannelName = parameters.NewReservation.Channel.Name;
            parameters.ReservationFilter.IsPaid = parameters.NewReservation.IsPaid;
            parameters.ReservationFilter.ReservationDate = parameters.NewReservation.Date;
            parameters.ReservationFilter.ReservationBaseCurrencyExchangeRate = parameters.NewReservation.ReservationBaseCurrencyExchangeRate;
            parameters.ReservationFilter.ReservationBaseCurrencySymbol = parameters.NewReservation.ReservationBaseCurrencySymbol;
            parameters.ReservationFilter.PropertyBaseCurrencyExchangeRate = parameters.NewReservation.PropertyBaseCurrencyExchangeRate;
            parameters.ReservationFilter.PropertyBaseCurrencySymbol = parameters.NewReservation.PropertyBaseCurrencySymbol;
            parameters.ReservationFilter.CreatedBy = parameters.NewReservation.CreateBy;
            parameters.ReservationFilter.ModifiedBy = parameters.NewReservation.ModifyBy;
            parameters.ReservationFilter.Employee_UID = parameters.NewReservation.Employee_UID;

            //Rooms
            if (parameters.NewReservation.ReservationRooms != null && parameters.NewReservation.ReservationRooms.Count > 0)
            {
                List<contractsReservations.ReservationRoomCancelationCost> cancellationCosts = null;
                if (parameters.ServiceName == contractsGeneral.ServiceName.CancelReservation)
                    cancellationCosts = GetCancelationCosts(parameters.NewReservation);

                parameters.ReservationFilter.NumberOfNights = ((System.DateTime)parameters.NewReservation.ReservationRooms.First().DateTo - (System.DateTime)parameters.NewReservation.ReservationRooms.First().DateFrom).Days;
                foreach (var room in parameters.NewReservation.ReservationRooms)
                {
                    Domain.Reservations.ReservationRoomFilter roomFilter = null;
                    if (parameters.ServiceName != contractsGeneral.ServiceName.InsertReservation && parameters.ReservationFilter.ReservationRoomFilters != null)
                        roomFilter = parameters.ReservationFilter.ReservationRoomFilters.FirstOrDefault(x => x.ReservationRoomNo == room.ReservationRoomNo);

                    if (roomFilter == null)
                    {
                        roomFilter = new Domain.Reservations.ReservationRoomFilter();
                        parameters.ReservationFilter.ReservationRoomFilters.Add(roomFilter);
                        roomFilter.UID = room.UID;
                    }

                    roomFilter.ReservationId = parameters.NewReservation.UID;
                    roomFilter.ReservationRoomNo = room.ReservationRoomNo;
                    roomFilter.ApplyDepositPolicy = room.IsDepositAllowed;
                    roomFilter.GuestName = room.GuestName;
                    roomFilter.CheckIn = room.DateFrom;
                    roomFilter.CheckOut = room.DateTo;
                    roomFilter.DepositCost = room.DepositValue;
                    roomFilter.Status = room.Status;
                    roomFilter.DepositNumberOfNight = room.DepositNrNights;

                    if (cancellationCosts != null)
                    {
                        var cost = cancellationCosts.FirstOrDefault(x => x.Number == room.ReservationRoomNo && !x.NotApply);
                        if (cost != null)
                            roomFilter.CancellationCost = cost.CancelationCosts;
                    }

                }
            }

            parameters.ReservationFilter.BigPullAuthOwner_UID = parameters.ReservationFilter.BigPullAuthOwner_UID ?? parameters.NewReservation.ReservationAdditionalData?.BigPullAuthOwner_UID;
            parameters.ReservationFilter.BigPullAuthRequestor_UID = parameters.ReservationFilter.BigPullAuthRequestor_UID ?? parameters.NewReservation.ReservationAdditionalData?.BigPullAuthRequestor_UID;
        }

        public void ModifyReservationFilter(contractsReservations.Reservation newReservation, contractsGeneral.ServiceName serviceName, contractsCRMOB.Guest guest = null)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                if (serviceName == contractsGeneral.ServiceName.IgnoreTransaction || serviceName == contractsGeneral.ServiceName.UpdateReservation)
                    DeleteReservationRoomFilter(newReservation.UID);

                var reservationFilterRepo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);

                var reservationFilter = reservationFilterRepo.FindByReservationUIDs(new List<long> { newReservation.UID }).FirstOrDefault();

                var param = new TreatReservationFiltersParameters()
                {
                    NewReservation = newReservation,
                    ReservationFilter = reservationFilter,
                    Guest = guest,
                    ServiceName = serviceName
                };
                TreatReservationFilter(param, unitOfWork);

                if (reservationFilter == null)
                    reservationFilterRepo.Add(param.ReservationFilter);
                else
                    reservationFilterRepo.AttachAsModified(param.ReservationFilter);

                unitOfWork.Save();
            }
        }

        #endregion

        /// <summary>
        /// Get Reservation Lookups
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ReservationLookups GetReservationLookups(DL.Common.ListReservationCriteria request,
            IEnumerable<domainReservations.Reservation> result)
        {
            var lookups = new ReservationLookups();

            List<long> reservationIds = new List<long>();
            reservationIds = result.Select(x => x.UID).Distinct().ToList();
            if (!reservationIds.Any())
                return lookups;

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationLookupRepo = this.RepositoryFactory.GetOBReservationLookupsRepository();
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                List<Action> actions = new List<Action>();

                #region RESERVATION LOOKUPS OB.API
                //Action
                Action taskActionLookup = () =>
                {
                    var reservationLookupRequest = PrepareReservationLookupRequest(request, result);

                    var response = reservationLookupRepo.ListReservationLookups(reservationLookupRequest);

                    if (response == null)
                        throw new NullReferenceException("ReservationLookup can't be null to proceed.");

                    QueryResultObjectToBusinessObjectTypeConverter.Map(response, lookups);
                };
                //add action
                actions.Add(taskActionLookup);
                #endregion RESERVATION LOOKUPS OB.API

                #region RESERVATION STATUS NAME

                Action statusNameAction = () =>
                {
                    List<long> reservationStatusUIDs = new List<long>();

                    if (request.IncludeReservationStatusName)
                    {
                        var statusLanguageRepo = this.RepositoryFactory.GetRepository<domainReservations.ReservationStatusLanguage>(unitOfWork);
                        reservationStatusUIDs = result.Select(x => x.Status).Distinct().ToList();

                        if (reservationStatusUIDs.Any())
                        {
                            var langUID = request.LanguageUID > 0 ? request.LanguageUID : 1;
                            var status = statusLanguageRepo.GetQuery(x => x.ReservationStatus_UID.HasValue &&
                                                                    reservationStatusUIDs.Contains(
                                                                        x.ReservationStatus_UID.Value) &&
                                                                    x.Language_UID == langUID);
                            if (status != null)
                                lookups.ReservationStatusNameLookup = status.ToDictionary(
                                    x => x.ReservationStatus_UID.Value,
                                    x => x.Name);
                        }
                    }

                    if (request.IncludeReservationReadStatus)
                    {
                        List<string> reservationsUIDs = new List<string>();
                        var visualRepo = this.RepositoryFactory.GetVisualStateRepository(unitOfWork);
                        reservationsUIDs = result.Select(x => x.UID.ToString()).ToList();
                        if (reservationsUIDs.Any())
                        {
                            var visualStates = visualRepo.GetQuery(x => reservationsUIDs.Contains(x.LookupKey_1));
                            if (visualStates != null)
                            {
                                visualStates = visualStates.GroupBy(x => x.LookupKey_1).Select(x => x.FirstOrDefault());
                                lookups.ReservationReadStatusLookup = visualStates.Where(x => !string.IsNullOrEmpty(x.LookupKey_1)
                                        && !string.IsNullOrEmpty(x.JSONData)).ToDictionary(
                                            x => long.Parse(x.LookupKey_1),
                                            x => x.JSONData.FromJSON<OB.Reservation.BL.Contracts.Data.VisualStates.ReservationReadStatus>());
                            }
                        }
                    }
                };

                //add action
                actions.Add(statusNameAction);


                #endregion RESERVATION STATUS NAME

                #region RESERVATION ADDITIONAL DATA
                if (request.IncludeReservationAddicionalData)
                {
                    //Action
                    Action taskAction = () =>
                    {
                        var results = reservationRepo.FindReservationsAdditionalDataByReservationsUIDs(reservationIds);
                        lookups.ReservationsAdditionalData = results.ToDictionary(x => x.Reservation_UID);
                    };
                    //add action
                    actions.Add(taskAction);
                }
                #endregion RESERVATION ADDITIONAL DATA

                #region RESERVATION TRANSACTION STATUS
                Action taskActionTransaction = () =>
                {
                    var resultsReservationTrans = reservationRepo.FindReservationTransactionStatusByReservationsUIDs(reservationIds);
                    var transactionGroup = resultsReservationTrans.GroupBy(x => x.ReservationUID);
                    lookups.TransactionsLookup = transactionGroup.ToDictionary(x => x.Key, x => x.Last().TransactionUID);
                };
                actions.Add(taskActionTransaction);
                #endregion RESERVATION TRANSACTION STATUS

                Parallel.ForEach(actions, new ParallelOptions { MaxDegreeOfParallelism = 8 }, x => x());
            }

            return lookups;
        }


        private Contracts.Requests.ListReservationLookupsRequest PrepareReservationLookupRequest(DL.Common.ListReservationCriteria request,
            IEnumerable<domainReservations.Reservation> result)
        {
            Contracts.Requests.ListReservationLookupsRequest requestLookups = new Contracts.Requests.ListReservationLookupsRequest();
            requestLookups.LanguageUID = request.LanguageUID;

            requestLookups.IncludeGuests = request.IncludeGuests;
            if (request.IncludeGuests)
                requestLookups.GuestUIDs = result.Select(x => x.Guest_UID).Distinct().ToList();

            requestLookups.IncludeExtras = request.IncludeExtras;
            requestLookups.IncludeExtrasBillingTypes = request.IncludeExtrasBillingTypes;
            if (request.IncludeExtras)
                requestLookups.ExtrasUIDs = result.SelectMany(x => x.ReservationRooms)
                        .SelectMany(x => x.ReservationRoomExtras)
                        .Select(x => x.Extra_UID)
                        .Distinct()
                        .ToList();

            requestLookups.IncludeTaxPolicies = request.IncludeTaxPolicies;
            if (request.IncludeTaxPolicies)
                requestLookups.TaxPoliciesUIDs = result.SelectMany(x => x.ReservationRooms).SelectMany(x => x.ReservationRoomTaxPolicies)
                            .Where(x => x.TaxId.HasValue).Select(x => x.TaxId.Value).Distinct().ToList();

            requestLookups.IncludeRoomTypes = request.IncludeRoomTypes;
            if (request.IncludeRoomTypes)
                requestLookups.RoomUIDs = result.SelectMany(x => x.ReservationRooms)
                        .Where(x => x.RoomType_UID.HasValue)
                        .Select(x => x.RoomType_UID.Value)
                        .Distinct()
                        .ToList();

            requestLookups.IncludeRates = request.IncludeRates;
            if (request.IncludeRates)
            {
                var reservationRooms = result.SelectMany(x => x.ReservationRooms);
                requestLookups.RateUIDs = reservationRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).Distinct().ToList();
                requestLookups.RateUIDs.AddRange(reservationRooms.SelectMany(x => x.ReservationRoomDetails).Where(x => (x != null && x.Rate_UID.HasValue)).Select(x => x.Rate_UID.Value).Distinct().ToList());
                requestLookups.RateUIDs = requestLookups.RateUIDs.Distinct().ToList();

                if (requestLookups.RateUIDs.Count > 0)
                {
                    requestLookups.IncludeOtaCodes = request.IncludeOtaCodes;
                    requestLookups.IncludeRateCategories = request.IncludeRateCategories;
                }
            }

            requestLookups.IncludePromotionalCodes = request.IncludePromotionalCodes;
            if (request.IncludePromotionalCodes)
                requestLookups.PromotionalCodeUIDs = result.Where(x => x.PromotionalCode_UID.HasValue)
                        .Select(x => x.PromotionalCode_UID.Value)
                        .Distinct()
                        .ToList();

            requestLookups.IncludeGroupCodes = request.IncludeGroupCodes;
            if (request.IncludeGroupCodes)
                requestLookups.GroupCodeUIDs =
                        result.Where(x => x.GroupCode_UID.HasValue)
                            .Select(x => x.GroupCode_UID.Value)
                            .Distinct()
                            .ToList();

            requestLookups.IncludeIncentives = request.IncludeIncentives;
            if (request.IncludeIncentives)
                requestLookups.IncentiveUIDs = result.SelectMany(x => x.ReservationRooms.SelectMany(y => y.ReservationRoomDetails
                             .Where(z => z.ReservationRoomDetailsAppliedIncentives != null)
                             .SelectMany(z => z.ReservationRoomDetailsAppliedIncentives.Select(t => t.Incentive_UID))))
                        .Distinct()
                        .ToList();

            requestLookups.IncludeGuestActivities = request.IncludeGuestActivities;
            if (request.IncludeGuestActivities)
                requestLookups.GuestUIDs = result.Select(x => x.Guest_UID).ToList();

            requestLookups.IncludeBESpecialRequests = request.IncludeBESpecialRequests;
            if (request.IncludeBESpecialRequests)
            {
                requestLookups.BESpecialRequestUIDs = result.Where(x => x.BESpecialRequests1_UID.HasValue).Select(y => y.BESpecialRequests1_UID.Value).ToList();
                requestLookups.BESpecialRequestUIDs.AddRange(
                    result.Where(x => x.BESpecialRequests2_UID.HasValue).Select(y => y.BESpecialRequests2_UID.Value));
                requestLookups.BESpecialRequestUIDs.AddRange(
                    result.Where(x => x.BESpecialRequests3_UID.HasValue).Select(y => y.BESpecialRequests3_UID.Value));
                requestLookups.BESpecialRequestUIDs.AddRange(
                    result.Where(x => x.BESpecialRequests4_UID.HasValue).Select(y => y.BESpecialRequests4_UID.Value));
            }

            requestLookups.IncludeTransferLocation = request.IncludeTransferLocation;
            if (request.IncludeTransferLocation)
                requestLookups.TransferLocationsUIDs = result.Where(x => x.TransferLocation_UID.HasValue).Select(y => y.TransferLocation_UID.Value).ToList();

            requestLookups.IncludeReservationBaseCurrency = request.IncludeReservationBaseCurrency;
            if (request.IncludeReservationBaseCurrency)
                requestLookups.ReservationBaseCurrencyUIDs =
                        result.Where(x => x.ReservationBaseCurrency_UID != null)
                            .Select(x => (long)x.ReservationBaseCurrency_UID)
                            .Distinct()
                            .ToList();

            requestLookups.IncludeReservationCurrency = request.IncludeReservationCurrency;
            if (request.IncludeReservationCurrency)
                requestLookups.ReservationCurrencyUIDs =
                       result.Where(x => x.ReservationCurrency_UID.HasValue)
                           .Select(x => x.ReservationCurrency_UID.Value)
                           .Distinct()
                           .ToList();

            requestLookups.IncludeChannel = request.IncludeChannel;
            requestLookups.IncludeChannelOperator = request.IncludeChannelOperator;
            if (request.IncludeChannel || request.IncludeChannelOperator)
                requestLookups.ChannelUIDs = result.Where(x => x.Channel_UID.HasValue).Select(x => x.Channel_UID.Value).Distinct().ToList();

            requestLookups.IncludePropertyCountry = request.IncludePropertyCountry;
            requestLookups.IncludePropertyBaseCurrency = request.IncludePropertyBaseCurrency;
            if (request.IncludePropertyBaseCurrency || request.IncludePropertyCountry)
                requestLookups.PropertiesUIDs = result.Select(x => x.Property_UID).Distinct().ToList();

            requestLookups.IncludeTPIName = request.IncludeTPIName;
            requestLookups.IncludeTPILanguageUID = request.IncludeTPILanguageUID;
            if (request.IncludeTPIName || request.IncludeTPILanguageUID)
                requestLookups.TPIUIDs = result.Where(x => x.TPI_UID.HasValue).Select(x => x.TPI_UID.Value).Distinct().ToList();

            requestLookups.IncludeCompanyName = request.IncludeCompanyName;
            if (request.IncludeCompanyName)
                requestLookups.CompanyUIDs = result.Where(x => x.Company_UID.HasValue).Select(x => x.Company_UID.Value).Distinct().ToList();

            requestLookups.IncludeReservationPaymentDetail = request.IncludeReservationPaymentDetail;
            if (request.IncludeReservationPaymentDetail)
                requestLookups.PaymentMethodsUIDs = result.Where(r => r.ReservationPaymentDetails != null)
                        .SelectMany(
                            x =>
                                x.ReservationPaymentDetails.Where(v => v.PaymentMethod_UID.HasValue)
                                    .Select(j => j.PaymentMethod_UID.Value))
                        .Distinct()
                        .ToList();

            requestLookups.IncludePaymentMethodType = request.IncludePaymentMethodType;
            if (request.IncludePaymentMethodType)
                requestLookups.PaymentMethodTypeUIDs =
                       result.Where(r => r.PaymentMethodType_UID.HasValue)
                           .Select(x => x.PaymentMethodType_UID.Value)
                           .Distinct()
                           .ToList();

            requestLookups.IncludeOnRequestDecisionUser = request.IncludeOnRequestDecisionUser;
            if (request.IncludeOnRequestDecisionUser)
                requestLookups.OnRequestDecisionUserUIDs =
                        result.Where(r => r.OnRequestDecisionUser.HasValue)
                            .Select(x => x.OnRequestDecisionUser.Value)
                            .Distinct()
                            .ToList();

            requestLookups.IncludeReferralSource = request.IncludeReferralSource;
            if (request.IncludeReferralSource)
                requestLookups.ReferralSourceUIDs =
                       result.Where(r => r.ReferralSourceId.HasValue)
                           .Select(x => x.ReferralSourceId.Value)
                           .Distinct()
                           .ToList();

            requestLookups.IncludeExternalSource = request.IncludeExternalSource;
            if (request.IncludeExternalSource)
                requestLookups.ExternalSourceUIDs =
                       result.Where(r => r.ExternalSource_UID.HasValue)
                           .Select(x => x.ExternalSource_UID.Value)
                           .Distinct()
                           .ToList();

            requestLookups.IncludeReservationRooms = request.IncludeReservationRooms;
            requestLookups.IncludeCommissionTypeName = request.IncludeCommissionTypeName;
            if (request.IncludeReservationRooms && request.IncludeCommissionTypeName)
                requestLookups.CommissionTypeUIDs = result.Where(r => r.ReservationRooms != null)
                        .SelectMany(
                            x =>
                                x.ReservationRooms.Where(v => v.CommissionType.HasValue)
                                    .Select(j => j.CommissionType.Value)).Distinct().ToList();

            requestLookups.IncludeBillingCountryName = request.IncludeBillingCountryName;
            requestLookups.IncludeGuestCountryName = request.IncludeGuestCountryName;
            if (request.IncludeBillingCountryName || request.IncludeGuestCountryName)
            {
                requestLookups.CountriesUIDs = new List<long>();

                if (request.IncludeBillingCountryName)
                    requestLookups.CountriesUIDs.AddRange(result.Where(x => x.BillingCountry_UID > 0).Select(x => x.BillingCountry_UID.Value).ToList());

                if (request.IncludeGuestCountryName)
                    requestLookups.CountriesUIDs.AddRange(result.Where(x => x.GuestCountry_UID > 0).Select(x => x.GuestCountry_UID.Value).ToList());

                requestLookups.CountriesUIDs = requestLookups.CountriesUIDs.Distinct().ToList();
            }

            requestLookups.IncludeBillingStateName = request.IncludeBillingStateName;
            requestLookups.IncludeGuestStateName = request.IncludeGuestStateName;
            if (request.IncludeBillingStateName || request.IncludeGuestStateName)
            {
                requestLookups.StatesUIDs = new List<long>();

                if (request.IncludeBillingStateName)
                    requestLookups.StatesUIDs.AddRange(result.Where(x => x.BillingState_UID > 0).Select(x => x.BillingState_UID.Value).ToList());

                if (request.IncludeGuestStateName)
                    requestLookups.StatesUIDs.AddRange(result.Where(x => x.GuestState_UID > 0).Select(x => x.GuestState_UID.Value).ToList());

                requestLookups.StatesUIDs = requestLookups.StatesUIDs.Distinct().ToList();
            }

            requestLookups.IncludeTPICommissions = request.IncludeTPICommissions;
            if (request.IncludeTPICommissions)
            {
                requestLookups.TPIUIDs = result.Where(x => x.TPI_UID.HasValue).Select(y => y.TPI_UID.Value).Distinct().ToList();
                requestLookups.ReservationUIDs = result.Select(y => y.UID).Distinct().ToList();
            }

            return requestLookups;
        }

        #region CRUD Reservations Language Aux
        //there are two methods because they receive diferents types of reservations (domain and business)
        public void SetLanguageToReservation(contractsReservations.Reservation reservation, long property_UID, IOBPropertyRepository propsRepo, Guid? requestGuid = null)
        {
            long? langUid = reservation.ReservationLanguageUsed_UID;

            //Get the property languagevar
            langUid = propsRepo.GetPropetyBaseLanguage(new Contracts.Requests.GetPropertyBaseLanguageRequest { PropertyUId = property_UID, RequestGuid = requestGuid ?? Guid.Empty });
            reservation.ReservationLanguageUsed_UID = langUid;
        }
        public void SetLanguageToReservation(Domain.Reservations.Reservation reservation, long property_UID, IOBPropertyRepository propsRepo)
        {
            long? langUid = reservation.ReservationLanguageUsed_UID;

            //Get the property language
            langUid = propsRepo.GetPropetyBaseLanguage(new Contracts.Requests.GetPropertyBaseLanguageRequest { PropertyUId = property_UID });

            reservation.ReservationLanguageUsed_UID = langUid;
        }
        #endregion

        #region BE RESERVATION

        [Obsolete("This method will be removed on OB version 0.9.48.")]
        public bool IsValidPropertyToValidateBEPrices(long propertyUID)
        {
            var appSettingsRepo = RepositoryFactory.GetOBAppSettingRepository();

            var validPropertiesSetting = appSettingsRepo.ListSettings(new Contracts.Requests.ListSettingRequest()
            {
                Names = new List<string>() { "ValidateReservationPricesForBE_Properties" }
            }).FirstOrDefault();

            return ProjectGeneral.IsValidProperty(validPropertiesSetting, propertyUID);
        }

        [Obsolete("This method will be removed on OB version 0.9.48.")]
        private void TreatBeReservationWithoutPriceValidation(TreatBEReservationParameters parameters)
        {
            if (!parameters.Reservation.Channel_UID.HasValue)
                throw Errors.InvalidChannel.ToBusinessLayerException();

            if (parameters.ReservationRoomDetails == null || !parameters.ReservationRoomDetails.Any())
                throw Errors.RequiredParameter.ToBusinessLayerException(nameof(parameters.ReservationRoomDetails));

            var reservationHelper = Resolve<IReservationHelperPOCO>();
            var reservationPricesPOCO = Resolve<IReservationPricesCalculationPOCO>();
            OB.BL.Contracts.Data.CRM.LoyaltyProgram loyaltyProgram = null;

            // Get valid rates for promocode
            var validatePromoRequest = new ValidatePromocodeForReservationParameters()
            {
                ReservationRooms = parameters.Rooms.Select(rr => new ReservationRoomStayPeriod()
                {
                    RateUID = rr.Rate_UID.Value,
                    CheckIn = rr.DateFrom.Value,
                    CheckOut = rr.DateTo.Value
                }).ToList(),
                PromocodeUID = parameters.Reservation.PromotionalCode_UID,
                CurrencyUID = parameters.Reservation.ReservationBaseCurrency_UID ?? parameters.ReservationContext.PropertyBaseCurrency_UID.Value
            };
            var validPromocodeResponse = reservationHelper.ValidatePromocodeForReservation(validatePromoRequest);

            if (validPromocodeResponse == null || validPromocodeResponse.RejectReservation)
                throw Errors.InvalidPromocodeForReservation.ToBusinessLayerException();

            if (validPromocodeResponse.PromoCodeObj == null || !validPromocodeResponse.PromoCodeObj.IsValid ||
                validPromocodeResponse.NewDaysToApplyDiscount == null || !validPromocodeResponse.NewDaysToApplyDiscount.Any())
            {
                // Reject reservation if promocode has no discounts to apply
                if (parameters.Reservation.PromotionalCode_UID.HasValue)
                    throw Errors.InvalidPromocodeForReservation.ToBusinessLayerException();

                return;
            }
            parameters.PromotionalCode = validPromocodeResponse.PromoCodeObj;

            // Get Loyalty program
            if (parameters.GroupRule.BusinessRules.HasFlag(domainReservations.BusinessRules.LoyaltyDiscount))
            {
                var crmRepo = RepositoryFactory.GetOBCRMRepository();
                int total = 0;
                if (parameters.Guest.LoyaltyLevel_UID.HasValue)
                {
                    var loyaltyRsp = crmRepo.ListLoyaltyPrograms(new OB.BL.Contracts.Requests.ListLoyaltyProgramRequest
                    {
                        Client_UIDs = new List<long> { parameters.ReservationContext.Client_UID },
                        IncludeDeleted = false,
                        IncludeDefaultCurrency = true,
                        IncludeDefaultLanguage = false,
                        IncludeLoyaltyLevels = true,
                        IncludeLoyaltyLevelsCurrencies = true,
                        IncludeLoyaltyLevelsLanguages = false,
                        IncludeLoyaltyProgramLanguages = false,
                        ExcludeInactive = true,
                        LoyaltyLevel_Uids = new List<long> { parameters.Guest.LoyaltyLevel_UID.Value }
                    });

                    total = loyaltyRsp.TotalRecords;
                    loyaltyProgram = loyaltyRsp.Results.FirstOrDefault();
                }
            }

            var discountDays = new List<DateTime>();
            foreach (var room in parameters.Rooms)
            {
                var ages = parameters.ReservationRoomChilds != null
                    ? parameters.ReservationRoomChilds.Where(x => x.ReservationRoom_UID == room.UID)
                        .Where(x => x.Age.HasValue)
                        .Select(x => x.Age.Value)
                        .ToList()
                    : new List<int>();

                var pricesParameters = new CalculateFinalPriceParameters
                {
                    CheckIn = room.DateFrom.Value,
                    CheckOut = room.DateTo.Value,
                    BaseCurrency =
                        parameters.Reservation.ReservationBaseCurrency_UID ??
                        parameters.ReservationContext.PropertyBaseCurrency_UID.Value,
                    AdultCount = room.AdultCount.Value,
                    ChildCount = room.ChildCount ?? 0,
                    Ages = ages,
                    GroupRule = parameters.GroupRule,
                    ExchangeRate = parameters.Reservation.PropertyBaseCurrencyExchangeRate ?? 1,
                    PropertyId = parameters.Reservation.Property_UID,
                    RateId = room.Rate_UID.Value,
                    RoomTypeId = room.RoomType_UID.Value,
                    TpiId = parameters.Reservation.TPI_UID,
                    RateModelId = (int?)room.CommissionType,
                    ChannelId = parameters.ReservationContext.Channel_UID,
                    TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                    TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                    TPIDiscountValue = room.TPIDiscountValue,
                    LoyaltyProgram = loyaltyProgram,
                    ValidPromocodeParameters = validPromocodeResponse
                };

                var rrdList = reservationPricesPOCO.CalculateReservationRoomPrices(pricesParameters);

                if (rrdList != null)
                {
                    var rrddict = rrdList.Where(x => x.AppliedPromotionalCode != null).ToDictionary(k => k.Date.Date, v => v.AppliedPromotionalCode);
                    foreach (var rdd in parameters.ReservationRoomDetails.Where(x => rrddict.ContainsKey(x.Date.Date)))
                    {
                        var appliedPromo = rrddict[rdd.Date.Date];
                        rdd.ReservationRoomDetailsAppliedPromotionalCode = new contractsReservations.ReservationRoomDetailsAppliedPromotionalCode()
                        {
                            PromotionalCode_UID = appliedPromo.PromotionalCode_UID,
                            Date = appliedPromo.Date.Date,
                            DiscountValue = appliedPromo.DiscountValue,
                            DiscountPercentage = appliedPromo.DiscountPercentage
                        };
                        discountDays.Add(rdd.ReservationRoomDetailsAppliedPromotionalCode.Date);
                    }
                }
            }

            // Update reservations completed for each day with discount
            long promocodeUID = validPromocodeResponse.PromoCodeObj.UID;
            if (promocodeUID > 0)
                UpdatePromoCodeReservationsCompleted(promocodeUID, newDays: discountDays.Distinct());
        }

        public void TreatBeReservation(TreatBEReservationParameters parameters)
        {
            if (!IsValidPropertyToValidateBEPrices(parameters.Reservation.Property_UID))
            {
                TreatBeReservationWithoutPriceValidation(parameters);
                return;
            }

            if (!parameters.Reservation.Channel_UID.HasValue)
                throw Errors.InvalidChannel.ToBusinessLayerException();

            if (parameters.ReservationRoomDetails == null || !parameters.ReservationRoomDetails.Any())
                throw Errors.RequiredParameter.ToBusinessLayerException(nameof(parameters.ReservationRoomDetails));

            var reservationHelper = Resolve<IReservationHelperPOCO>();
            var reservationPricesPOCO = Resolve<IReservationPricesCalculationPOCO>();
            var obExtrasRepo = RepositoryFactory.GetOBExtrasRepository();
            OB.BL.Contracts.Data.CRM.LoyaltyProgram loyaltyProgram = null;
            decimal failMargin = 0.01M;

            var originalReservationToValidate = new contractsReservations.Reservation
            {
                TotalTax = parameters.Reservation.TotalTax,
                RoomsTax = parameters.Reservation.RoomsTax,
                RoomsPriceSum = parameters.Reservation.RoomsPriceSum ?? 0,
                RoomsTotalAmount = parameters.Reservation.RoomsTotalAmount ?? 0,
                TotalAmount = parameters.Reservation.TotalAmount ?? 0
            };

            // Get valid rates for promocode
            var validatePromoRequest = new ValidatePromocodeForReservationParameters()
            {
                ReservationRooms = parameters.Rooms.Select(rr => new ReservationRoomStayPeriod()
                {
                    RateUID = rr.Rate_UID.Value,
                    CheckIn = rr.DateFrom.Value,
                    CheckOut = rr.DateTo.Value
                }).ToList(),
                PromocodeUID = parameters.Reservation.PromotionalCode_UID,
                CurrencyUID = parameters.Reservation.ReservationBaseCurrency_UID ?? parameters.ReservationContext.PropertyBaseCurrency_UID.Value
            };
            var validPromocodeResponse = reservationHelper.ValidatePromocodeForReservation(validatePromoRequest);

            bool invalidPromocode = validPromocodeResponse == null || validPromocodeResponse.RejectReservation ||
                ((validPromocodeResponse.PromoCodeObj == null || !validPromocodeResponse.PromoCodeObj.IsValid
                || validPromocodeResponse.NewDaysToApplyDiscount == null || !validPromocodeResponse.NewDaysToApplyDiscount.Any())
                && parameters.Reservation.PromotionalCode_UID.HasValue);

            // Reject reservation if promocode has no discounts to apply
            if (invalidPromocode)
                throw Errors.InvalidPromocodeForReservation.ToBusinessLayerException();

            parameters.PromotionalCode = validPromocodeResponse.PromoCodeObj;

            // Get Loyalty program
            if (parameters.GroupRule.BusinessRules.HasFlag(domainReservations.BusinessRules.LoyaltyDiscount))
            {
                var crmRepo = RepositoryFactory.GetOBCRMRepository();
                long? loyaltyLevelUID = parameters.Rooms.Where(x => x.LoyaltyLevel_UID > 0).Select(x => x.LoyaltyLevel_UID).FirstOrDefault();
                if (loyaltyLevelUID.HasValue)
                {
                    var loyaltyRsp = crmRepo.ListLoyaltyPrograms(new OB.BL.Contracts.Requests.ListLoyaltyProgramRequest
                    {
                        Client_UIDs = new List<long> { parameters.ReservationContext.Client_UID },
                        IncludeDeleted = false,
                        IncludeDefaultCurrency = true,
                        IncludeDefaultLanguage = false,
                        IncludeLoyaltyLevels = true,
                        IncludeLoyaltyLevelsCurrencies = true,
                        IncludeLoyaltyLevelsLanguages = false,
                        IncludeLoyaltyProgramLanguages = false,
                        ExcludeInactive = true,
                        LoyaltyLevel_Uids = new List<long> { loyaltyLevelUID.Value },
                        PageSize = 1
                    });

                    loyaltyProgram = loyaltyRsp.Results.FirstOrDefault();
                }
            }

            // Get TaxPolicies and Extras Not Included of ReservationRooms
            List<OBcontractsRates.TaxPolicy> roomsTaxPolicies = null;
            Dictionary<long, List<Contracts.Data.Rates.Extra>> roomsAdditionalExtras = null;
            var ratesIds = parameters.Rooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).Distinct().ToList();
            if (ratesIds.Any())
            {
                roomsTaxPolicies = reservationHelper.GetTaxPoliciesByRateIds(ratesIds, parameters.ReservationContext.Currency_UID, null);
                roomsAdditionalExtras = obExtrasRepo.ListRatesExtras(new Contracts.Requests.ListRatesExtrasRequest { RateUIDs = ratesIds, IsActive = true, IsIncluded = false });
            }

            var discountDays = new List<DateTime>();
            var calculatedReservationRooms = new List<contractsReservations.ReservationRoom>();
            foreach (var room in parameters.Rooms)
            {
                long roomNo = room.UID;

                var rrdListOrginal = parameters.ReservationRoomDetails.Where(x => x.ReservationRoom_UID == roomNo).OrderBy(x => x.Date).ToList();
                room.DateFrom = room.DateFrom.Value.Date;
                room.DateTo = room.DateTo.Value.Date;

                var resRoomOriginal = new contractsReservations.ReservationRoom
                {
                    ReservationRoomsPriceSum = room.ReservationRoomsPriceSum ?? 0,
                    ReservationRoomsTotalAmount = room.ReservationRoomsTotalAmount ?? 0,
                    ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum,
                    TotalTax = room.TotalTax
                };

                #region CALCULATE AND MAP PRICES

                var ages = parameters.ReservationRoomChilds != null
                    ? parameters.ReservationRoomChilds.Where(x => x.ReservationRoom_UID == roomNo)
                        .Where(x => x.Age.HasValue)
                        .Select(x => x.Age.Value)
                        .ToList()
                    : new List<int>();

                var pricesParameters = new CalculateFinalPriceParameters
                {
                    CheckIn = room.DateFrom.Value,
                    CheckOut = room.DateTo.Value,
                    BaseCurrency =
                        parameters.Reservation.ReservationBaseCurrency_UID ??
                        parameters.ReservationContext.PropertyBaseCurrency_UID.Value,
                    AdultCount = room.AdultCount.Value,
                    ChildCount = room.ChildCount ?? 0,
                    Ages = ages,
                    GroupRule = parameters.GroupRule,
                    ExchangeRate = parameters.Reservation.PropertyBaseCurrencyExchangeRate ?? 1,
                    PropertyId = parameters.Reservation.Property_UID,
                    RateId = room.Rate_UID.Value,
                    RoomTypeId = room.RoomType_UID.Value,
                    TpiId = parameters.Reservation.TPI_UID,
                    RateModelId = (int?)room.CommissionType,
                    ChannelId = parameters.ReservationContext.Channel_UID,
                    TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                    TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                    TPIDiscountValue = room.TPIDiscountValue,
                    LoyaltyProgram = loyaltyProgram,
                    ValidPromocodeParameters = validPromocodeResponse
                };

                // Calculate Reservation Room Prices
                var rrdListCalculated = reservationPricesPOCO.CalculateReservationRoomPrices(pricesParameters).OrderBy(x => x.Date).ToList();

                decimal total = 0;
                this.SetReservationRoomDetails(room, rrdListCalculated, out total);
                room.ReservationRoomsPriceSum = total;

                parameters.ReservationRoomDetails.RemoveAll(x => x.ReservationRoom_UID == room.UID);
                parameters.ReservationRoomDetails.AddRange(room.ReservationRoomDetails);

                // Set reservation applied promotional code
                var orderedRoomDetails = room.ReservationRoomDetails.OrderBy(x => x.Date).ToList();
                if (orderedRoomDetails.Count == rrdListOrginal.Count)
                    for (int i = 0; i < rrdListOrginal.Count; i++)
                        rrdListOrginal[i].ReservationRoomDetailsAppliedPromotionalCode = orderedRoomDetails[i].ReservationRoomDetailsAppliedPromotionalCode;

                #region CALCULATE TAX POLICIES

                if (roomsTaxPolicies != null)
                    SetReservationRoomTaxPolicies(room, roomsTaxPolicies, parameters.Reservation.PropertyBaseCurrencyExchangeRate ?? 1, rrdListCalculated);

                //calculate totaltax to room
                if (room.ReservationRoomTaxPolicies != null)
                    room.TotalTax = room.ReservationRoomTaxPolicies.Sum(x => x.TaxCalculatedValue);

                #endregion

                // Validate Extras
                room.ReservationRoomsExtrasSum = 0;
                if (room.Rate_UID.HasValue && parameters.ReservationRoomExtras != null)
                {
                    List<OBcontractsRates.Extra> extras;
                    var roomExtras = parameters.ReservationRoomExtras.Where(x => x.UID == roomNo).ToList();

                    if (roomExtras.Any() && roomsAdditionalExtras != null && roomsAdditionalExtras.TryGetValue(room.Rate_UID.Value, out extras))
                    {
                        room.ReservationRoomsExtrasSum = ValidateReservationRoomExtras(roomExtras, extras, parameters.Reservation.PropertyBaseCurrencyExchangeRate ?? 1,
                            (room.DateTo.Value - room.DateFrom.Value).Days, room.AdultCount ?? 0, failMargin, parameters.ReservationRequest);
                    }
                }

                room.ReservationRoomsTotalAmount = (room.ReservationRoomsPriceSum ?? 0) + (room.ReservationRoomsExtrasSum ?? 0) + (room.TotalTax ?? 0);

                #endregion CALCULATE AND MAP PRICES

                #region VALIDATE PRICES

                if (rrdListOrginal.Count != rrdListCalculated.Count)
                    throw Errors.RateRoomDetailsAreNotSet.ToBusinessLayerException();

                // Validates prices per day and throws an error if prices don't match
                ValidatePricePerDay(rrdListCalculated, rrdListOrginal, failMargin, parameters.ReservationRequest);

                var resRoomCalculated = new contractsReservations.ReservationRoom();
                resRoomCalculated.ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum;
                resRoomCalculated.TotalTax = room.TotalTax;
                resRoomCalculated.ReservationRoomsPriceSum = rrdListCalculated.Sum(x => x.FinalPrice);
                resRoomCalculated.ReservationRoomsTotalAmount = (resRoomCalculated.ReservationRoomsPriceSum ?? 0) + (resRoomCalculated.ReservationRoomsExtrasSum ?? 0) + (resRoomCalculated.TotalTax ?? 0);

                // Validates Reservation Room prices and throws an error if prices don't match
                ValidateReservationRoomsPrices(resRoomCalculated, resRoomOriginal, failMargin, parameters.ReservationRequest);

                calculatedReservationRooms.Add(resRoomCalculated);

                #endregion VALIDATE PRICES
            }

            parameters.Reservation.TotalAmount = parameters.Rooms.Sum(x => x.ReservationRoomsTotalAmount);
            parameters.Reservation.RoomsTotalAmount = parameters.Rooms.Sum(x => x.ReservationRoomsTotalAmount ?? 0);
            parameters.Reservation.RoomsPriceSum = parameters.Rooms.Sum(x => x.ReservationRoomsPriceSum ?? 0);
            parameters.Reservation.RoomsTax = parameters.Rooms.Sum(x => x.TotalTax ?? 0);
            parameters.Reservation.TotalTax = parameters.Rooms.Sum(x => x.TotalTax ?? 0);

            #region VALIDATE RESERVATION PRICES

            var roomsTotalAmount = calculatedReservationRooms.Sum(x => x.ReservationRoomsTotalAmount) ?? 0;
            var roomTotalTax = calculatedReservationRooms.Sum(x => x.TotalTax ?? 0);

            var calculatedReservationToValidate = new contractsReservations.Reservation
            {
                TotalTax = roomTotalTax,
                RoomsTax = roomTotalTax,
                RoomsPriceSum = calculatedReservationRooms.Sum(x => x.ReservationRoomsPriceSum ?? 0),
                RoomsTotalAmount = roomsTotalAmount,
                TotalAmount = roomsTotalAmount
            };

            // Validates Reservation prices and throws an error if prices don't match
            ValidateReservationPrices(calculatedReservationToValidate, originalReservationToValidate, failMargin, parameters.ReservationRequest);

            #endregion VALIDATE RESERVATION PRICES
        }

        #endregion BE RESERVATION

        public decimal CalculateExtra(OBcontractsRates.Extra extra, int numberOfNights, int numAdults, int qty, decimal? exchangeRate = null)
        {
            if (extra == null) return 0;

            var exchange = exchangeRate ?? 1;
            bool isPerNight = extra.ExtraBillingTypes_UIDs != null && extra.ExtraBillingTypes_UIDs.Contains((long)OB.Reservation.BL.Constants.BillingType.PerNight);
            bool isPerPerson = extra.ExtraBillingTypes_UIDs != null && extra.ExtraBillingTypes_UIDs.Contains((long)OB.Reservation.BL.Constants.BillingType.PerPerson);

            decimal mult1 = isPerNight ? numberOfNights : qty;
            decimal mult2 = isPerPerson ? numAdults : 1;

            return (extra.Value ?? 0) * mult1 * mult2 * exchange;
        }

        #region PRICES VALIDATION

        public void ValidatePricePerDay(List<OBcontractsRates.RateRoomDetailReservation> calculatedRRD, List<contractsReservations.ReservationRoomDetail> originalRRD,
            decimal failMargin, ReservationBaseRequest reservationRequest)
        {
            // Prices Validation need Version >= 0.9.45
            if (!ProjectGeneral.IsValidVersion(0, 9, 45, reservationRequest.Version))
                return;

            // Prices Validation
            for (int i = 0; i < calculatedRRD.Count; i++)
            {
                string priceName = string.Format("PriceDay_{0:dd/MM/yyyy}", originalRRD[i].Date);
                ValidateReservationPrice(originalRRD[i].Price, calculatedRRD[i].FinalPrice, failMargin, priceName, reservationRequest);
            }
        }

        public void ValidateReservationRoomsPrices(contractsReservations.ReservationRoom calculatedRR, contractsReservations.ReservationRoom originalRR,
            decimal failMargin, ReservationBaseRequest reservationRequest)
        {
            // Prices Validation need Version >= 0.9.45
            if (ProjectGeneral.IsValidVersion(0, 9, 45, reservationRequest.Version))
            {
                ValidateReservationPrice(originalRR.TotalTax, calculatedRR.TotalTax, failMargin, "ReservationRoomsTotalTax", reservationRequest);
            }

            
            ValidateReservationPrice(originalRR.ReservationRoomsExtrasSum, calculatedRR.ReservationRoomsExtrasSum, failMargin, "ReservationRoomsExtrasSum", reservationRequest);
            ValidateReservationPrice(originalRR.ReservationRoomsPriceSum, calculatedRR.ReservationRoomsPriceSum, failMargin, "ReservationRoomsPriceSum", reservationRequest);
            ValidateReservationPrice(originalRR.ReservationRoomsTotalAmount, calculatedRR.ReservationRoomsTotalAmount, failMargin, "ReservationRoomsTotalAmount", reservationRequest);
        }

        public void ValidateReservationPrices(contractsReservations.Reservation calculatedRes, contractsReservations.Reservation originalRes,
            decimal failMargin, ReservationBaseRequest reservationRequest)
        {
            // Validate Taxes only if Version >= 0.9.45
            if (ProjectGeneral.IsValidVersion(0, 9, 45, reservationRequest.Version))
            {
                ValidateReservationPrice(originalRes.TotalTax, calculatedRes.TotalTax, failMargin, "TotalTax", reservationRequest);
                ValidateReservationPrice(originalRes.RoomsTax, calculatedRes.RoomsTax, failMargin, "RoomsTax", reservationRequest);
            }
            
            ValidateReservationPrice(originalRes.RoomsPriceSum, calculatedRes.RoomsPriceSum, failMargin, "RoomsPriceSum", reservationRequest);
            ValidateReservationPrice(originalRes.RoomsTotalAmount, calculatedRes.RoomsTotalAmount, failMargin, "RoomsTotalAmount", reservationRequest);
            ValidateReservationPrice(originalRes.TotalAmount, calculatedRes.TotalAmount, failMargin, "TotalAmount", reservationRequest);
        }

        public decimal ValidateReservationRoomExtras(List<contractsReservations.ReservationRoomExtra> roomExtras, List<OBcontractsRates.Extra> extras, decimal propertyBaseCurrencyExchangeRate,
            int roomNightsCount, int roomAdultsCount, decimal failMargin, ReservationBaseRequest reservationRequest)
        {
            decimal rrExtrasSum = 0;
            roomExtras = roomExtras ?? new List<contractsReservations.ReservationRoomExtra>();
            extras = extras ?? new List<OBcontractsRates.Extra>();

            var rrExtras = new List<contractsReservations.ReservationRoomExtra>();
            foreach (var rrextra in roomExtras.Where(x => !x.ExtraIncluded))
            {
                var extra = extras.FirstOrDefault(x => x.UID == rrextra.Extra_UID);
                decimal calculatedValue = 0;
                if (extra != null)
                {
                    calculatedValue = CalculateExtra(extra, roomNightsCount, roomAdultsCount, rrextra.Qty, propertyBaseCurrencyExchangeRate);
                    rrExtrasSum += calculatedValue;
                }

                // Validate Price
                ValidateReservationPrice(rrextra.Total_Price, calculatedValue, failMargin, "ReservationRoomExtra", reservationRequest);
            }

            return rrExtrasSum;
        }

        private void ValidateReservationPrice(decimal? orginalPrice, decimal? calculatedPrice, decimal failMargin, string priceName, ReservationBaseRequest insertReservationRequest)
        {
            if (insertReservationRequest == null)
                return;

            domainReservations.GroupRule groupRule = null;
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                groupRule = RepositoryFactory.GetGroupRulesRepository(unitOfWork)
                    .GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(insertReservationRequest));
            }

            decimal pricesDiff = (calculatedPrice ?? 0) - (orginalPrice ?? 0);
            if (groupRule.BusinessRules.HasFlag(Domain.Reservations.BusinessRules.PriceCalculationAbsoluteTolerance))
                pricesDiff = Math.Abs(pricesDiff);

            if (pricesDiff > failMargin)
            {
                LoggingValidatePrice(orginalPrice, calculatedPrice, failMargin, priceName, insertReservationRequest);
                throw Errors.ReservationPricesAreInvalid.ToBusinessLayerException();
            }
        }

        private void LoggingValidatePrice(decimal? orginalPrice, decimal? calculatedPrice, decimal failMargin, string priceName, ReservationBaseRequest reservationRequest)
        {
            var logObj = new BaseReservationLog()
            {
                DateTimeUTC = DateTime.UtcNow,
                RequestGuid = reservationRequest.RequestGuid,
                ErrorCode = ((int)Errors.ReservationPricesAreInvalid).ToString(),
                RuleType = reservationRequest.RuleType.HasValue ? (int)reservationRequest.RuleType : 0,
                RequestObject = reservationRequest,
                Details = new
                {
                    PriceName = priceName,
                    CalculatedPrice = calculatedPrice,
                    OriginalPrice = orginalPrice,
                    FailMargin = failMargin
                }
            };

            // Build Log according request
            var request = reservationRequest as InsertReservationRequest;
            if (request != null)
            {
                logObj.PropertyUID = request.Reservation != null ? request.Reservation.Property_UID : 0;
                if (request.ReservationRooms != null)
                {
                    logObj.CheckIn = request.ReservationRooms.Where(x => x.DateFrom.HasValue).Min(x => x.DateFrom.Value);
                    logObj.CheckOut = request.ReservationRooms.Where(x => x.DateTo.HasValue).Max(x => x.DateTo.Value);
                }
            }

            Logger.Warn(logObj.ToJSON());

            logObj.RequestObject = null;
            LogEntriesLogger.Warn(logObj.ToJSON());
        }

        #endregion PRICES VALIDATION
    }
}