using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using domainReservation = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBPropertyRepository : RestRepository<OB.BL.Contracts.Data.Properties.PropertyLight>, IOBPropertyRepository
    {
        public List<OB.BL.Contracts.Data.Properties.PropertyLight> ListPropertiesLight(OB.BL.Contracts.Requests.ListPropertyRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.PropertyLight>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertyLightResponse>(request, "Properties", "ListPropertiesLight");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.Properties.ListProperty> ListProperties(OB.BL.Contracts.Requests.ListPropertiesRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.ListProperty>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertiesResponse>(request, "Properties", "ListProperties");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Properties.Inventory> UpdateInventoryDetails(OB.BL.Contracts.Requests.UpdateInventoryDetailsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.Inventory>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateInventoryDetailsResponse>(request, "Inventory", "UpdateInventoryDetails");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public long ApplyRealAllotmentToAllChannels(OB.BL.Contracts.Requests.ApplyRealAllotmentToChannelsRequest request)

        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ApplyRealAllotmentToChannelsResponse>(request, "Inventory", "ApplyRealAllotmentToAllChannels").Result;
        }

        public List<OB.BL.Contracts.Data.Properties.Inventory> ListInventory(OB.BL.Contracts.Requests.ListInventoryRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.Inventory>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListInventoryResponse>(request, "Properties", "ListInventory");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<DateTime> ConvertToPropertyTimeZone(OB.BL.Contracts.Requests.ConvertToPropertyTimeZoneRequest request)
        {
            var data = new List<DateTime>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ConvertToPropertyTimeZoneResponse>(request, "Properties", "ConvertToPropertyTimeZone");
            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Channels.ChannelsProperty> ListChannelsProperty(OB.BL.Contracts.Requests.ListChannelsPropertyRequest request)
        {
            var data = new List<BL.Contracts.Data.Channels.ChannelsProperty>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListChannelsPropertyResponse>(request, "Channels", "ListChannelsProperty");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.General.Country> ListCountries(OB.BL.Contracts.Requests.ListCountryRequest request)
        {
            var data = new List<BL.Contracts.Data.General.Country>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCountryResponse>(request, "General", "ListCountries");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.General.State> ListStates(OB.BL.Contracts.Requests.ListStatesRequest request)
        {
            var data = new List<BL.Contracts.Data.General.State>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListStatesResponse>(request, "General", "ListStates");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }


        public List<BL.Contracts.Data.General.Language> ListLanguages(OB.BL.Contracts.Requests.ListLanguageRequest request)
        {
            var data = new List<BL.Contracts.Data.General.Language>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListLanguageResponse>(request, "General", "ListLanguages");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.General.CityData> ListCitiesDataTranslated(OB.BL.Contracts.Requests.ListCityDataRequest request)
        {
            var data = new List<BL.Contracts.Data.General.CityData>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCityDataResponse>(request, "General", "ListCitiesDataTranslated");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public bool ApplyOccupancyLevels(OB.BL.Contracts.Requests.ApplyOccupancyLevelsRequest request)
        {            
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ApplyOccupancyLevelsResponse>(request, "OccupancyLevel", "ApplyOccupancyLevels").Result;
        }

        public List<BL.Contracts.Data.Properties.PropertySecurityConfiguration> GetPropertySecurityConfiguration(OB.BL.Contracts.Requests.ListPropertySecurityConfigurationRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.PropertySecurityConfiguration>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertySecurityConfigurationResponse>(request, "Properties", "GetPropertySecurityConfiguration");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public BL.Contracts.Data.Payments.PaymentGatewayConfiguration GetActivePaymentGatewayConfiguration(OB.BL.Contracts.Requests.ListActivePaymentGatewayConfigurationRequest request)
        {
            BL.Contracts.Data.Payments.PaymentGatewayConfiguration data = null;
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPaymentGatewayConfigurationResponse>(request, "Properties", "GetActivePaymentGatewayConfiguration");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.FirstOrDefault();

            return data;
        }

        public BL.Contracts.Data.Payments.PaymentGatewayConfiguration GetActivePaymentGatewayConfigurationReduced(OB.BL.Contracts.Requests.ListActivePaymentGatewayConfigurationRequest request)
        {
            BL.Contracts.Data.Payments.PaymentGatewayConfiguration data = null;
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPaymentGatewayConfigurationResponse>(request, "Properties", "GetActivePaymentGatewayConfigurationReduced");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.FirstOrDefault();

            return data;
        }

        public List<BL.Contracts.Data.Payments.PaymentGatewayConfiguration> ListPaymentGatewayConfiguration(OB.BL.Contracts.Requests.ListPaymentGatewayConfigurationRequest request)
        {
            var data = new List<BL.Contracts.Data.Payments.PaymentGatewayConfiguration>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPaymentGatewayConfigurationResponse>(request, "Properties", "ListPaymentGatewayConfiguration");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Properties.RoomType> ListRoomTypes(OB.BL.Contracts.Requests.ListRoomTypeRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.RoomType>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRoomTypeResponse>(request, "Properties", "ListRoomTypes");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Properties.PropertiesExternalSourceForOmnibees> ListPropertiesExternalSourceForOmnibees(OB.BL.Contracts.Requests.ListPropertiesExternalSourceForOmnibeesRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.PropertiesExternalSourceForOmnibees>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertiesExternalSourceForOmnibeesResponse>(request, "Properties", "ListPropertiesExternalSourceForOmnibees");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Properties.PropertiesExternalSource> ListPropertiesExternalSource(OB.BL.Contracts.Requests.ListPropertiesExternalSourceRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.PropertiesExternalSource>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertiesExternalSourceResponse>(request, "Properties", "ListPropertiesExternalSource");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public BL.Contracts.Responses.ResponseBase UpdatePropertiesExternalSources(OB.BL.Contracts.Requests.UpdatePropertyExternalSourceRequest request)
        {
            var data = new BL.Contracts.Responses.ResponseBase();
            var response = RESTServicesFacade.Call<BL.Contracts.Responses.ResponseBase>(request, "Properties", "UpdatePropertiesExternalSources");
            return data;
        }

        public List<BL.Contracts.Data.Properties.ExternalSource> ListExternalSource(OB.BL.Contracts.Requests.ListExternalSourceRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.ExternalSource>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListExternalSourceResponse>(request, "General", "ListExternalSources");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.General.ReferralSource> ListReferralSources(OB.BL.Contracts.Requests.ListReferralSourcesRequest request)
        {
            var data = new List<BL.Contracts.Data.General.ReferralSource>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListReferralSourcesResponse>(request, "General", "ListReferralSources");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }



        /// <summary>
        /// Convert to timezone
        /// </summary>
        /// <param name="date"></param>
        /// <param name="timezoneId"></param>
        /// <returns></returns>
        public DateTime ConvertToTimeZone(DateTime date, long timezoneId)
        {
            // TODO: Passar isto para um POCO

            var masterTimeZone = domainReservation.MasterTimeZones.FindByUID(timezoneId);
            if (masterTimeZone != null)
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(masterTimeZone.TimeZoneName);
                return date.AddMinutes(timeZone.GetUtcOffset(date).TotalMinutes);
            }
            else
                return date;
        }


        public T ConvertToPropertyTimeZone<T>(long propertyId, T obj, List<string> datetimeMemberNames)
        {
            // TODO: Passar isto para um POCO

            if (obj == null)
                return obj;

            var timezoneId = ListPropertiesLight(new BL.Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { propertyId } }).Select(x => x.TimeZone_UID).FirstOrDefault();  

            if (timezoneId.HasValue)
            {
                var type = obj.GetType();

                foreach (var member in datetimeMemberNames)
                {
                    PropertyInfo pinfo = type.GetProperty(member);
                    if (pinfo.PropertyType == typeof(DateTime))
                    {
                        var tmpDate = (DateTime)pinfo.GetValue(obj, null);
                        pinfo.SetValue(obj, ConvertToTimeZone(tmpDate, timezoneId.Value), null);
                    }
                    else if (pinfo.PropertyType == typeof(DateTime?))
                    {
                        var tmpDate = (DateTime?)pinfo.GetValue(obj, null);
                        if (tmpDate.HasValue)
                            pinfo.SetValue(obj, ConvertToTimeZone(tmpDate.Value, timezoneId.Value), null);
                    }
                }

                return obj;
            }

            return obj;
        }

        public List<T> ConvertToPropertyTimeZoneList<T>(long propertyId, List<T> objects, List<string> datetimeMemberNames)
        {
            // TODO: Passar isto para um POCO

            if (objects == null || !objects.Any())
                return objects;

            var timezoneId = ListPropertiesLight(new BL.Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { propertyId } }).Select(x => x.TimeZone_UID).FirstOrDefault();

            if (timezoneId.HasValue)
            {
                var type = objects.First().GetType();
                foreach (var item in objects)
                {
                    foreach (var member in datetimeMemberNames)
                    {
                        PropertyInfo pinfo = type.GetProperty(member);
                        if (pinfo.PropertyType == typeof(DateTime))
                        {
                            var tmpDate = (DateTime)pinfo.GetValue(item, null);
                            pinfo.SetValue(item, ConvertToTimeZone(tmpDate, timezoneId.Value), null);
                        }
                        else if (pinfo.PropertyType == typeof(DateTime?))
                        {
                            var tmpDate = (DateTime?)pinfo.GetValue(item, null);
                            if (tmpDate.HasValue)
                                pinfo.SetValue(item, ConvertToTimeZone(tmpDate.Value, timezoneId.Value), null);
                        }
                    }
                }

                return objects;
            }

            return objects;
        }

        public List<BL.Contracts.Data.Properties.TransferLocation> ListTransferLocationsForReservation(OB.BL.Contracts.Requests.ListTransferLocationsForReservationRequest request)
        {
            var data = new List<BL.Contracts.Data.Properties.TransferLocation>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListTransferLocationsForReservationResponse>(request, "Properties", "ListTransferLocationsForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public long GetPropetyBaseLanguage(OB.BL.Contracts.Requests.GetPropertyBaseLanguageRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetPropertyBaseLanguageResponse>(request, "Properties", "GetPropertyBaseLanguage").Result;
        }

        public List<BL.Contracts.Data.ProactiveActions.ProactiveAction> ListProactiveActions(OB.BL.Contracts.Requests.ListProactiveActionsRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListProactiveActionsResponse>(request, "ProactiveActions", "ListProactiveActions").Results;
        }

    }
}

