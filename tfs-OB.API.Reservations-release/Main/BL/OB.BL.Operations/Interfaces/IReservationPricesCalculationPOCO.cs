using System;
using System.Collections.Generic;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.Domain.Reservations;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Interfaces
{
    public interface IReservationPricesCalculationPOCO : IBusinessPOCOBase
    {
        List<ChildTermsOccupancy> GetGuestCountAfterApplyChildTerms(long? reservationBaseCurrencyId, List<OB.BL.Contracts.Data.Properties.ChildTerm> propertyChildTerms, int? nChilds, List<int> ages);

        List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> CalculateExternalMarkup(List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList, domainReservations.GroupRule groupRule, SellRule markup);

        decimal CalculateExternalCommission(decimal commission, long currencyUID, long? reservationBaseCurrency, long? propertyUID, decimal? totalAmount);

        List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> GetRateRoomDetailsForReservationRoom(long propertyId, long rateId, long roomTypeId, long reservationBaseCurrency, long channelId,
                    DateTime checkIn, DateTime checkOut, int adultCount, int childCount, List<int> ages, List<OB.BL.Contracts.Data.Properties.ChildTerm> childTerms, int? rateModelId);

        List<OB.BL.Contracts.Data.Properties.Incentive> GetIncentivesForReservationRoom(DateTime checkIn, DateTime checkOut, long rateId, GroupRule groupRule);

        List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> CalculateReservationRoomPrices(CalculateFinalPriceParameters parameters);
    }
}
