namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBChildTermsRepository : IRestRepository<OB.BL.Contracts.Data.Properties.ChildTerm>
    {
        OB.BL.Contracts.Responses.ListChildTermsResponse ListChildTerms(OB.BL.Contracts.Requests.ListChildTermsRequest request);
    }
}
