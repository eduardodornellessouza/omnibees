using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBCRMRepository : IRestRepository<OB.BL.Contracts.Data.General.Currency>
    {
        List<OB.BL.Contracts.Data.CRM.Guest> ListGuestsByLightCriteria(OB.BL.Contracts.Requests.ListGuestLightRequest request);

        OB.BL.Contracts.Responses.InsertGuestReservationResponse InsertGuestReservation(OB.BL.Contracts.Requests.InsertGuestReservationRequest request);

        OB.BL.Contracts.Responses.UpdateGuestReservationResponse UpdateGuestReservation(OB.BL.Contracts.Requests.UpdateGuestReservationRequest request);

        List<OB.BL.Contracts.Data.CRM.TPIProperty> ListTpiProperty(OB.BL.Contracts.Requests.ListTPIPropertyRequest request);

        List<OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryLight> ListThirdPartyIntermediariesLight(OB.BL.Contracts.Requests.ListThirdPartyIntermediariesLightRequest request);

        OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryAdditionalData GetTpiReservationAdditionalData(OB.BL.Contracts.Requests.GetTpiReservationAdditionalDataRequest request);

        List<OB.BL.Contracts.Data.CRM.TPICustom> ListThirdPartyIntermediariesByLightCriteria(OB.BL.Contracts.Requests.ListThirdPartyIntermediaryLightRequest request);

        OB.BL.Contracts.Responses.ListLoyaltyProgramResponse ListLoyaltyPrograms(OB.BL.Contracts.Requests.ListLoyaltyProgramRequest request);

        List<OB.BL.Contracts.Data.General.User> ListUsers(OB.BL.Contracts.Requests.ListUserRequest request);

        List<OB.BL.Contracts.Data.CRM.GuestActivity> ListGuestActivitiesForReservation(OB.BL.Contracts.Requests.ListGuestActivitiesForReservationRequest request);

        List<OB.BL.Contracts.Data.BE.BESpecialRequest> ListBESpecialRequestForReservation(OB.BL.Contracts.Requests.ListBESpecialRequestForReservationRequest request);

        OB.BL.Contracts.Responses.UpdateGuestNameResponse UpdateGuestName(OB.BL.Contracts.Requests.UpdateGuestNameRequest request);
    }
}
