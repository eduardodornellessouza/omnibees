using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBBeSettingsRepository
    {
        ListBeSettingsResponse ListBeSettings(ListBeSettingsRequest request);
    }
}
