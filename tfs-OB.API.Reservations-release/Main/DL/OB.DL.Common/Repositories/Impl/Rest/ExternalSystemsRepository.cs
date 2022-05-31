using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class ExternalSystemsRepository : RestRepository<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>, IExternalSystemsRepository
    {
        public ES.API.Contracts.Response.CheckRemoteAvailabilityResponse CheckRemoteAvailability(ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest request)
        {
            return RESTServicesFacade.Call<ES.API.Contracts.Response.CheckRemoteAvailabilityResponse>(request, "Reservations", "CheckRemoteAvailability");
        }
    }
}
