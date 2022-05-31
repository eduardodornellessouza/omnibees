using System.Collections.Generic;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBAppSettingRepository : IRestRepository<OB.BL.Contracts.Data.General.Setting>
    {
        List<OB.BL.Contracts.Data.General.Setting> ListSettings(OB.BL.Contracts.Requests.ListSettingRequest request);

        List<OB.BL.Contracts.Data.General.TripAdvisorConfiguration> ListTripAdvisorConfiguration(OB.BL.Contracts.Requests.ListTripAdvisorConfigRequest request);

        Task SendTripAdvisorReviewEmailAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request);

        Task AlterTripAdvisorReviewEmailAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request);

        Task EraseTripAdvisorReviewAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request);
    }
}
