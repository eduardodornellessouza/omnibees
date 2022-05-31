using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;

namespace OB.BL.Operations.Interfaces
{
    public interface ITokenizedCreditCardsReadsPerMonthManagerPOCO
    {
        FindTokenizedCreditCardsReadsPerMonthByCriteriaResponse FindTokenizedCreditCardsReadsPerMonthByCriteria(FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest request);

        IncrementTokenizedCreditCardsReadsPerMonthByCriteriaResponse IncrementTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest request);
    }
}
