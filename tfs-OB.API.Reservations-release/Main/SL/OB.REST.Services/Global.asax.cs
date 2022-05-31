using Hangfire;
using Hangfire.Logging.LogProviders;
using Hangfire.SqlServer;
using OB.REST.Services.Loggers;
using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;

namespace OB.REST.Services
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configuration.MessageHandlers.Add(new NLogControllerTraceHandler());
         
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);                     
         
            //It's initialized already in the Startup class
            //UnityConfig.RegisterComponents();

            System.Web.Http.GlobalConfiguration.Configuration.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());

            //var options = new SqlServerStorageOptions
            //{
            //    QueuePollInterval = TimeSpan.FromSeconds(10), // Default value                  
            //};

            //var configuration = Hangfire.GlobalConfiguration.Configuration
            //    // Use connection string name defined in `web.config` or `app.config`
            //    //.UseSqlServerStorage("db_connection")            
            //    .UseSqlServerStorage("OmnibeesJobs", options);
            
            

            //var backgroundServerOptions = new BackgroundJobServerOptions()
            //{
            //    WorkerCount = Environment.ProcessorCount
            //};

            //var sqlStorage = configuration.Entry;
            ////configuration.UseLogProvider(new ColouredConsoleLogProvider());
            //configuration.UseLogProvider(new NLogLogProvider());

            //var server = new BackgroundJobServer(backgroundServerOptions, sqlStorage);
            
        }
    }
}
