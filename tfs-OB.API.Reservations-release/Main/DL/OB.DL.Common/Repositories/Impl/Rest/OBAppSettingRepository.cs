using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBAppSettingRepository : RestRepository<OB.BL.Contracts.Data.General.Setting>, IOBAppSettingRepository
    {
        public List<OB.BL.Contracts.Data.General.Setting> ListSettings(OB.BL.Contracts.Requests.ListSettingRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.General.Setting>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListSettingResponse>(request, "PreferencesAndSettings", "ListSettings");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.General.TripAdvisorConfiguration> ListTripAdvisorConfiguration(OB.BL.Contracts.Requests.ListTripAdvisorConfigRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.General.TripAdvisorConfiguration>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListTripAdvisorConfigResponse>(request, "TripAdvisor", "ListTripAdvisorConfiguration");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public async Task SendTripAdvisorReviewEmailAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request)

        {
            await RESTServicesFacade.CallAsync<BL.Contracts.Responses.ResponseBase>(request, "TripAdvisor", "SendTripAdvisorReviewEmailAsync");
        }

        public async Task AlterTripAdvisorReviewEmailAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request)

        {
            await RESTServicesFacade.CallAsync<BL.Contracts.Responses.ResponseBase>(request, "TripAdvisor", "AlterTripAdvisorReviewEmailAsync");
        }

        public async Task EraseTripAdvisorReviewAsync(OB.BL.Contracts.Requests.TripAdvisorReviewAsyncRequest request)

        {
            await RESTServicesFacade.CallAsync<BL.Contracts.Responses.ResponseBase>(request, "TripAdvisor", "EraseTripAdvisorReviewAsync");
        }
    }
}
