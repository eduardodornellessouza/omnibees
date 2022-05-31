using Microsoft.Practices.Unity;
using OB.BL.Operations;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.Log;
using System;
using System.Web.Http;

namespace OB.REST.Services
{
    /// <summary>
    /// Initializes the IoC Container
    /// </summary>
    public static class UnityConfig
    {
        public static ILogger _logger = LogsManager.CreateLogger(typeof(UnityConfig));

        public static bool _hasBeenInitialized;

        public static void RegisterComponents(HttpConfiguration config)
        {
            var before = DateTime.Now.Ticks;

            lock (AppDomain.CurrentDomain)
            {
                if (_hasBeenInitialized)
                {
                    _logger.Warn("IoC Container HAS ALREADY BEEN INITIALIZED, CHECK YOUR CONFIGURATION");
                    return;
                }
                _hasBeenInitialized = true;
            }

            _logger.Info("Starting \"UnityConfig.RegisterComponents\"");


            var container = new UnityContainer();

            // registers all your components with the container here
            // it is NOT necessary to register your controllers            
            // e.g. container.RegisterType<ITestService, TestService>();
            var dataAccessLayerModule = new DataAccessLayerModule();
            var businessLayerModule = new BusinessLayerModule();

            container.AddExtension(dataAccessLayerModule);
            container.AddExtension(businessLayerModule);

            container.RegisterInstance<IRegisteredTasksManager>(new RegisteredObjects.AspNetRegisteredTasksManager(), new ContainerControlledLifetimeManager());

            //AppDomain.CurrentDomain.SetData("IoCContainer", container); //another approach to save an unique reference to the IoC container.
            var unityResolver = new UnityResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = unityResolver;
            config.DependencyResolver = unityResolver;

            dataAccessLayerModule.WarmUp();
            businessLayerModule.WarmUp();

            _logger.Info("Finished \"UnityConfig.RegisterComponents\" . TOOK: " + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
        }

    }

}
