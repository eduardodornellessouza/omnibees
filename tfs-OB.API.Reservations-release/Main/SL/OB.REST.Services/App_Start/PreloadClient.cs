using OB.Log;
using OB.REST.Services.Controllers;
using System.Web.Hosting;
using System.Web.Http;

namespace OB.REST.Services
{
    /// <summary>
    /// Warmup class.
    /// Please follow the instructions in :
    /// <remarks>https://www.simple-talk.com/blogs/2013/03/05/speeding-up-your-application-with-the-iis-auto-start-feature/</remarks>
    /// </summary>
    public class PreloadClient : IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            var logger = LogsManager.CreateLogger();
            logger.Info("CALLED PreloadClient.Preload!!");
            var dependencyResolver = GlobalConfiguration.Configuration.DependencyResolver;
            //var settingsController = dependencyResolver.GetService(typeof(PreferencesAndSettingsController)) as PreferencesAndSettingsController;
            //settingsController.ListCountries(new Reservation.BL.Contracts.Requests.ListPagedRequest());
            //settingsController.ListLanguages(new Reservation.BL.Contracts.Requests.ListPagedRequest());


            
        }
    }
}