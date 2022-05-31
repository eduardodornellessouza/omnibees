namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IExternalSystemsRepository : IRestRepository<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>
    {
        ES.API.Contracts.Response.CheckRemoteAvailabilityResponse CheckRemoteAvailability(ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest request);
    }
}
