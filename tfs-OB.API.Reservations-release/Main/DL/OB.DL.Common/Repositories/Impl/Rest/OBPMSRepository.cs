using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBPMSRepository : RestRepository<OB.BL.Contracts.Data.PMS.PMS>, IOBPMSRepository
    {
        public OB.BL.Contracts.Responses.ListPMSServicesPropertyMappingResponse ListPMSServicesPropertyMappings(OB.BL.Contracts.Requests.ListPMSServicesPropertyMappingRequest request)

        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPMSServicesPropertyMappingResponse>(request, "PMS", "ListPMSServicesPropertyMappings");
        }

        public OB.BL.Contracts.Responses.ListPMSServiceResponse ListPMSServices(OB.BL.Contracts.Requests.ListPMSServiceRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPMSServiceResponse>(request, "PMS", "ListPMSServices");
        }

        public OB.BL.Contracts.Responses.InsertPMSReservationsHistoryResponse InsertPMSReservationsHistory(OB.BL.Contracts.Requests.InsertPMSReservationsHistoryRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.InsertPMSReservationsHistoryResponse>(request, "PMS", "InsertPMSReservationsHistory");
        }
    }
}
