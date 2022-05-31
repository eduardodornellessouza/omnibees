namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBPMSRepository : IRestRepository<OB.BL.Contracts.Data.PMS.PMS>
    {
        OB.BL.Contracts.Responses.ListPMSServicesPropertyMappingResponse ListPMSServicesPropertyMappings(OB.BL.Contracts.Requests.ListPMSServicesPropertyMappingRequest request);

        OB.BL.Contracts.Responses.ListPMSServiceResponse ListPMSServices(OB.BL.Contracts.Requests.ListPMSServiceRequest request);

        OB.BL.Contracts.Responses.InsertPMSReservationsHistoryResponse InsertPMSReservationsHistory(OB.BL.Contracts.Requests.InsertPMSReservationsHistoryRequest request);
    }
}
