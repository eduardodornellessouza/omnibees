using Dapper;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Reservation.BL.Contracts.Requests;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class TokenizedCreditCardsReadsPerMonthRepository : Repository<TokenizedCreditCardsReadsPerMonth>, ITokenizedCreditCardsReadsPerMonthRepository
    {
        public TokenizedCreditCardsReadsPerMonthRepository(IObjectContext context)
            : base(context)
        {
        }


        public IEnumerable<TokenizedCreditCardsReadsPerMonth> FindTokenizedCreditCardsReadsPerMonthByCriteria(out int totalRecords, List<long> UIDs = null, List<long> propertyUIDs = null, List<int> years = null, List<short> months = null,
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {

            var result = GetQuery();
            totalRecords = -1;

            if (UIDs != null && UIDs.Count() > 0)
                result = result.Where(x => UIDs.Contains(x.UID));

            if (propertyUIDs != null && propertyUIDs.Count() > 0)
                result = result.Where(x => propertyUIDs.Contains(x.Property_UID));

            if (years != null && years.Count() > 0)
                result = result.Where(x => years.Contains(x.YearNr));

            if (months != null && months.Count() > 0)
                result = result.Where(x => months.Contains(x.MonthNr));

            if (returnTotal)
                totalRecords = result.Count();

            if (pageIndex > 0 && pageSize > 0)
                result = result.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }


        public long IncrementTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest)
        {
            return _context.Context.Database.Connection.Query<long>(
                 QUERY_IncrementTokenizedCreditCardsReadsPerMonthByCriteriaByUID,
                 new
                 {
                     UID = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.UID,
                     Time = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.time,
                     Increment = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.countsToIncrement
                 }).FirstOrDefault();

        }

        public long InsertTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest)
        {

            return _context.Context.Database.Connection.Query<long>(
          QUERY_InsertIncrementTokenizedCreditCardsReadsPerMonthByCriteria,
          new
          {
              Property_UID = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.PropertyUID,
              YearNr = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.YearNr,
              MonthNr = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.MonthNr,
              NrOfCreditCardReads = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.countsToIncrement,
              LastModifiedDate = incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest.time

          }).FirstOrDefault();

        }


        private static readonly string QUERY_IncrementTokenizedCreditCardsReadsPerMonthByCriteriaByUID = @"
                DECLARE @table table(
                    Value bigint NOT NULL
                );

                -- Update the value and output the same into a local table variable
                UPDATE [dbo].[TokenizedCreditCardsReadsPerMonth]
                SET [NrOfCreditCardReads] = [NrOfCreditCardReads] + @Increment, [LastModifiedDate] = @Time
                OUTPUT inserted.[NrOfCreditCardReads] INTO @table
                WHERE [UID] = @UID

                -- Now read the value from the local table variable
                SELECT Value FROM @table;
        ";


        private static readonly string QUERY_InsertIncrementTokenizedCreditCardsReadsPerMonthByCriteria = @"
               DECLARE @table table(
                    Value bigint NOT NULL
               );

               INSERT INTO [dbo].[TokenizedCreditCardsReadsPerMonth]
                    OUTPUT inserted.[NrOfCreditCardReads] INTO @table
                    VALUES(@Property_UID, @YearNr, @MonthNr, @NrOfCreditCardReads, @LastModifiedDate)

                -- Now read the value from the local table variable
                SELECT Value FROM @table;
        ";

    }
}