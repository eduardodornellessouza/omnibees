using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations;
using OB.Log;
using System;
using System.Net;
using System.Web.Http;


namespace OB.REST.Services.Controllers
{
    /// <summary>
    /// Controller responsible for Administrative operations over the REST services.
    /// <see href="http://www.iis.net/configreference/system.webserver/applicationinitialization"/>
    /// Called by IIS initialization feature as described in the link above.
    /// </summary>
    public class AdminController : BaseController
    {
        private ILogger _logger;
        private IAdminManagerPOCO _adminManagerPOCO;
       

        public AdminController(IAdminManagerPOCO adminManagerPOCO)
        {
            _logger = LogsManager.CreateLogger();
            _adminManagerPOCO = adminManagerPOCO;
        }

         /// <summary>
        /// Action responsible for Warming up the REST services.
        /// <see href="http://www.iis.net/configreference/system.webserver/applicationinitialization"/>
        /// Called by IIS initialization feature as described in the link above.
        /// </summary>
        [AcceptVerbs("GET")]
        [HttpGet]
        public void Get()
        {
            var before = DateTime.Now.Ticks;
            _logger.Debug("Calling WarmUpController...");

            //lock (AppDomain.CurrentDomain)
            //{
            if (AppDomain.CurrentDomain.GetData("WasWarmUpControllerCalled") != null)
            {
                _logger.Debug("WarmUpController was already called before, CHECK YOUR CONFIGURATION!");
                return;
            }

            AppDomain.CurrentDomain.SetData("WasWarmUpControllerCalled", true);
            //}


            try
            {
                //var dependencyResolver = this.Configuration.DependencyResolver;
                //dependencyResolver.GetService(typeof(ReservationController));
                //dependencyResolver.GetService(typeof(PreferencesAndSettingsController));                
                //dependencyResolver.GetService(typeof(ChannelActivityController));

                //dependencyResolver.GetService(typeof(PMSController));                
                //dependencyResolver.GetService(typeof(PropertiesController));
                //dependencyResolver.GetService(typeof(ChannelsController));               
                
                //MakeHttpRequest(this.Url.Content("../") + "api/PreferencesAndSettings/ListCountries");
                //MakeHttpRequest(this.Url.Content("../") + "api/PreferencesAndSettings/ListLanguages");
                //MakeHttpRequest(this.Url.Content("../") + "api/Reservation/ListReservations", "{ \"PageSize\" : 1 }");
                //MakeHttpRequest(this.Url.Content("../") + "api/Properties/ListPropertiesLight", "{ \"PageSize\" : 1 }");

                _logger.Debug("Finished calling WarmUpController. TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed while calling WarmUpController");
            }
        }


        private string MakeHttpRequest(string relativeUrl, string jsonRequest = null)
        {
            try
            {
                WebClient client = new WebClient();                
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                var requestUri = new Uri(this.ControllerContext.Request.RequestUri, relativeUrl);
                
                _logger.Debug(() => "REQUEST URI:" + requestUri);
                
                string response = client.UploadString(requestUri, jsonRequest == null ? "{}" : jsonRequest);
                
                _logger.Debug(() => "RESPONSE:" + response);

                return response;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed while warming up \"" + relativeUrl + "\"");
            }
            return null;

        }

          /// <summary>
        /// RESTful implementation of the UpdateCache operation.
        /// This operation updates the entries in the internal cache given a list of cache entry keys or all if RefreshAllEntries is true.
        /// </summary>
        /// <param name="request">A UpdateCacheRequest object containing the cache entry keys to update and/or the flag to update the cache immediatelly</param>
        /// <returns>A ListLanguageResponse containing the List of matching Language objects</returns>
        [AcceptVerbs("POST")]
        public UpdateCacheResponse UpdateCache(UpdateCacheRequest request)
        {
            return _adminManagerPOCO.UpdateCache(request);
        }
    }
}