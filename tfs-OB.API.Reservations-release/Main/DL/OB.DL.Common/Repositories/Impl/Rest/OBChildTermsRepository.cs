using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBChildTermsRepository : RestRepository<OB.BL.Contracts.Data.Properties.ChildTerm>, IOBChildTermsRepository
    {
        public OB.BL.Contracts.Responses.ListChildTermsResponse ListChildTerms(OB.BL.Contracts.Requests.ListChildTermsRequest request)

        {
           return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListChildTermsResponse>(request, "Properties", "ListChildTerms");            
        }

    }
}
