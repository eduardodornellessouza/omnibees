using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    public class OBBeSettingsRepository : IOBBeSettingsRepository
    {
        public ListBeSettingsResponse ListBeSettings(ListBeSettingsRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListBeSettingsResponse>(request, "BeSettings", "ListBeSettings");
        }
    }
}
