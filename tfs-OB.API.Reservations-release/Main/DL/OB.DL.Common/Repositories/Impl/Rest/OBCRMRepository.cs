using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBCRMRepository : RestRepository<OB.BL.Contracts.Data.General.Currency>, IOBCRMRepository
    {
        public List<OB.BL.Contracts.Data.CRM.Guest> ListGuestsByLightCriteria(OB.BL.Contracts.Requests.ListGuestLightRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.CRM.Guest>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListGuestResponse>(request, "CRM", "ListGuestsByLightCriteria");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public OB.BL.Contracts.Responses.InsertGuestReservationResponse InsertGuestReservation(OB.BL.Contracts.Requests.InsertGuestReservationRequest request)

        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.InsertGuestReservationResponse>(request, "CRM", "InsertGuestReservation");
        }

        public OB.BL.Contracts.Responses.UpdateGuestReservationResponse UpdateGuestReservation(OB.BL.Contracts.Requests.UpdateGuestReservationRequest request)

        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateGuestReservationResponse>(request, "CRM", "UpdateGuestReservation");
        }


        public List<OB.BL.Contracts.Data.CRM.TPIProperty> ListTpiProperty(OB.BL.Contracts.Requests.ListTPIPropertyRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.CRM.TPIProperty>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListTPIPropertyResponse>(request, "CRM", "ListTpiProperty");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }


        public List<OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryLight> ListThirdPartyIntermediariesLight(OB.BL.Contracts.Requests.ListThirdPartyIntermediariesLightRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryLight>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListThirdPartyIntermediariesLightResponse>(request, "CRM", "ListThirdPartyIntermediariesLight");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.CRM.TPICustom> ListThirdPartyIntermediariesByLightCriteria(OB.BL.Contracts.Requests.ListThirdPartyIntermediaryLightRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.CRM.TPICustom>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListThirdPartyIntermediaryResponse>(request, "CRM", "ListThirdPartyIntermediariesByLightCriteria");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryAdditionalData GetTpiReservationAdditionalData(OB.BL.Contracts.Requests.GetTpiReservationAdditionalDataRequest request)

        {
            var data = new OB.BL.Contracts.Data.CRM.ThirdPartyIntermediaryAdditionalData();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetTpiReservationAdditionalDataResponse>(request, "CRM", "GetTpiReservationAdditionalData");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public OB.BL.Contracts.Responses.ListLoyaltyProgramResponse ListLoyaltyPrograms(OB.BL.Contracts.Requests.ListLoyaltyProgramRequest request)

        {
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListLoyaltyProgramResponse>(request, "CRM", "ListLoyaltyPrograms");
            return response;
        }

        public List<OB.BL.Contracts.Data.General.User> ListUsers(OB.BL.Contracts.Requests.ListUserRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.General.User>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListUserResponse>(request, "General", "ListUsers");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }


        public List<OB.BL.Contracts.Data.CRM.GuestActivity> ListGuestActivitiesForReservation(OB.BL.Contracts.Requests.ListGuestActivitiesForReservationRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.CRM.GuestActivity>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListGuestActivitiesForReservationResponse>(request, "CRM", "ListGuestActivitiesForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.BE.BESpecialRequest> ListBESpecialRequestForReservation(OB.BL.Contracts.Requests.ListBESpecialRequestForReservationRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.BE.BESpecialRequest>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListBESpecialRequestForReservationResponse>(request, "CRM", "ListBESpecialRequestForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public OB.BL.Contracts.Responses.UpdateGuestNameResponse UpdateGuestName(OB.BL.Contracts.Requests.UpdateGuestNameRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateGuestNameResponse>(request, "Guest", "UpdateGuestName");
        }

    }
}