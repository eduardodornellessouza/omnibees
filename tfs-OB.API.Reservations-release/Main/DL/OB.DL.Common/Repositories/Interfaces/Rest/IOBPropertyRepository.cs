using System;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBPropertyRepository : IRestRepository<OB.BL.Contracts.Data.Properties.PropertyLight>
    {
        List<OB.BL.Contracts.Data.Properties.PropertyLight> ListPropertiesLight(OB.BL.Contracts.Requests.ListPropertyRequest request);

        List<OB.BL.Contracts.Data.Properties.ListProperty> ListProperties(OB.BL.Contracts.Requests.ListPropertiesRequest request);

        List<OB.BL.Contracts.Data.Properties.Inventory> UpdateInventoryDetails(OB.BL.Contracts.Requests.UpdateInventoryDetailsRequest request);

        long ApplyRealAllotmentToAllChannels(OB.BL.Contracts.Requests.ApplyRealAllotmentToChannelsRequest request);

        List<OB.BL.Contracts.Data.Properties.Inventory> ListInventory(OB.BL.Contracts.Requests.ListInventoryRequest request);

        List<DateTime> ConvertToPropertyTimeZone(OB.BL.Contracts.Requests.ConvertToPropertyTimeZoneRequest request);

        List<BL.Contracts.Data.Channels.ChannelsProperty> ListChannelsProperty(OB.BL.Contracts.Requests.ListChannelsPropertyRequest request);

        List<BL.Contracts.Data.General.Country> ListCountries(OB.BL.Contracts.Requests.ListCountryRequest request);

        List<BL.Contracts.Data.General.State> ListStates(OB.BL.Contracts.Requests.ListStatesRequest request);

        List<BL.Contracts.Data.General.Language> ListLanguages(OB.BL.Contracts.Requests.ListLanguageRequest request);

        List<BL.Contracts.Data.General.CityData> ListCitiesDataTranslated(OB.BL.Contracts.Requests.ListCityDataRequest request);

        bool ApplyOccupancyLevels(OB.BL.Contracts.Requests.ApplyOccupancyLevelsRequest request);

        List<BL.Contracts.Data.Properties.PropertySecurityConfiguration> GetPropertySecurityConfiguration(OB.BL.Contracts.Requests.ListPropertySecurityConfigurationRequest request);

        BL.Contracts.Data.Payments.PaymentGatewayConfiguration GetActivePaymentGatewayConfiguration(OB.BL.Contracts.Requests.ListActivePaymentGatewayConfigurationRequest request);
        BL.Contracts.Data.Payments.PaymentGatewayConfiguration GetActivePaymentGatewayConfigurationReduced(OB.BL.Contracts.Requests.ListActivePaymentGatewayConfigurationRequest request);

        List<BL.Contracts.Data.Payments.PaymentGatewayConfiguration> ListPaymentGatewayConfiguration(OB.BL.Contracts.Requests.ListPaymentGatewayConfigurationRequest request);

        List<BL.Contracts.Data.Properties.RoomType> ListRoomTypes(OB.BL.Contracts.Requests.ListRoomTypeRequest request);

        List<BL.Contracts.Data.Properties.PropertiesExternalSourceForOmnibees> ListPropertiesExternalSourceForOmnibees(OB.BL.Contracts.Requests.ListPropertiesExternalSourceForOmnibeesRequest request);

        List<BL.Contracts.Data.Properties.PropertiesExternalSource> ListPropertiesExternalSource(OB.BL.Contracts.Requests.ListPropertiesExternalSourceRequest request);

        BL.Contracts.Responses.ResponseBase UpdatePropertiesExternalSources(OB.BL.Contracts.Requests.UpdatePropertyExternalSourceRequest request);

        DateTime ConvertToTimeZone(DateTime date, long timezoneId);

        T ConvertToPropertyTimeZone<T>(long propertyId, T obj, List<string> datetimeMemberNames);

        List<T> ConvertToPropertyTimeZoneList<T>(long propertyId, List<T> objects, List<string> datetimeMemberNames);

        List<BL.Contracts.Data.Properties.TransferLocation> ListTransferLocationsForReservation(OB.BL.Contracts.Requests.ListTransferLocationsForReservationRequest request);

        long GetPropetyBaseLanguage(OB.BL.Contracts.Requests.GetPropertyBaseLanguageRequest request);

        List<BL.Contracts.Data.Properties.ExternalSource> ListExternalSource(OB.BL.Contracts.Requests.ListExternalSourceRequest request);

        List<BL.Contracts.Data.General.ReferralSource> ListReferralSources(OB.BL.Contracts.Requests.ListReferralSourcesRequest request);
        List<BL.Contracts.Data.ProactiveActions.ProactiveAction> ListProactiveActions(OB.BL.Contracts.Requests.ListProactiveActionsRequest request);
    }
}
