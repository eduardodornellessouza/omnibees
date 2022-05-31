using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using OB.REST.Services.Loggers;
using Owin;
using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Tracing;
using Swashbuckle;
using Swashbuckle.Swagger;
using Swashbuckle.Application;
using Swashbuckle.SwaggerUi;

[assembly: OwinStartup(typeof(OB.REST.Services.Startup))]

namespace OB.REST.Services
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #if DEBUG
                //TMOREIRA: EF PROFILER SHOULDN'T BE USED IN PRODUCTION! ONLY IN QA!         
                //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            #endif

            ConfigureAuth(app);

            HttpConfiguration config = new HttpConfiguration();
            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());            
            config.MessageHandlers.Add(new NLogControllerTraceHandler());
         
            SwaggerConfig.Register();
            WebApiConfig.Register(config);

            UnityConfig.RegisterComponents(config);

            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(10), // Default value                  
            };

            Hangfire.GlobalConfiguration.Configuration
                 // Use connection string name defined in `web.config` or `app.config`
                 //.UseSqlServerStorage("db_connection")            
                 .UseSqlServerStorage("OmnibeesJobs", options);
        }
    }
}
