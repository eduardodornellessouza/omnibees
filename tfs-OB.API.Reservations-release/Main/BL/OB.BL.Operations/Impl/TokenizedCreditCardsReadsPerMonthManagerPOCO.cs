using OB.BL.Operations.Interfaces;
using OB.Reservation.BL.Contracts.Responses;
using OB.Reservation.BL.Contracts.Requests;
using OB.BL.Operations.Internal.TypeConverters;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.ObjectModel;
using System.Linq;

namespace OB.BL.Operations.Impl
{
    public class TokenizedCreditCardsReadsPerMonthManagerPOCO : BusinessPOCOBase, ITokenizedCreditCardsReadsPerMonthManagerPOCO
    {
        public FindTokenizedCreditCardsReadsPerMonthByCriteriaResponse FindTokenizedCreditCardsReadsPerMonthByCriteria(FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest request)
        {
            var response = new FindTokenizedCreditCardsReadsPerMonthByCriteriaResponse();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(true))
            {
                var tokenized = this.RepositoryFactory.GetTokenizedCreditCardsReadsPerMonthRepository(unitOfWork);

                var resultOrigem = tokenized.FindTokenizedCreditCardsReadsPerMonthByCriteria(out int totalRecords, request.UIDs, request.propertyUIDs, request.years, request.months, request.pageIndex, request.pageSize, request.returnTotal);

                response.Result = new ObservableCollection<contractsReservations.TokenizedCreditCardsReadsPerMonth>(resultOrigem.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)));

                response.TotalRecords = totalRecords;

                response.Status = Status.Success;
            }

            return response;
        }

        public IncrementTokenizedCreditCardsReadsPerMonthByCriteriaResponse IncrementTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest request)
        {
            var response = new IncrementTokenizedCreditCardsReadsPerMonthByCriteriaResponse();

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var tokenized = this.RepositoryFactory.GetTokenizedCreditCardsReadsPerMonthRepository(unitOfWork);

                response.Result = request.UID != 0
                    ? tokenized.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(request)
                    : tokenized.InsertTokenizedCreditCardsReadsPerMonthByCriteria(request);

                response.Status = Status.Success;
            }

            return response;
        }
    }
}
