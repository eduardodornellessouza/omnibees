using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;


namespace OB.DL.Common.Repositories.Interfaces.Entity
{

    public interface ITokenizedCreditCardsReadsPerMonthRepository : IRepository<TokenizedCreditCardsReadsPerMonth>
    {

        IEnumerable<TokenizedCreditCardsReadsPerMonth> FindTokenizedCreditCardsReadsPerMonthByCriteria(out int totalRecords, List<long> UIDs = null, List<long> propertyUIDs = null, List<int> years = null, List<short> months = null,
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false);
        

        long IncrementTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest request);

        long InsertTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest request);

    }
}