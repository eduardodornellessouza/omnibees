using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Logging.LogProviders;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Owin;
using OB.Services.Jobs.Operations;
using Owin;
using System;
using System.Collections.Generic;

[assembly: OwinStartup(typeof(OB.Services.Jobs.Server.Startup))]

namespace OB.Services.Jobs.Server
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {           
            // Authentication
            ConfigureAuth(app);

            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(10), // Default value                  
            };

            var configuration = Hangfire.GlobalConfiguration.Configuration
                // Use connection string name defined in `web.config` or `app.config`
                //.UseSqlServerStorage("db_connection")            
                .UseSqlServerStorage("OmnibeesJobs", options);


            configuration.UseLogProvider(new NLogLogProvider());
            var backgroundServerOptions = new BackgroundJobServerOptions()
            {
                WorkerCount = Environment.ProcessorCount,
                Queues = new[] { "omnibees", "default" }
            };

            //app.UseHangfireDashboard();
            app.UseHangfireDashboard("", new DashboardOptions
            {
                AuthorizationFilters = new[] { new MyRestrictiveAuthorizationFilter() }
            });

            app.UseHangfireServer(backgroundServerOptions, configuration.Entry);

            ApplyGlobalFilters();
        }

        private void ApplyGlobalFilters()
        {
            // Number of retries
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });

            // Filter to Log every state of the job
            //GlobalJobFilters.Filters.Add(new LogEverythingAttribute());

            // Change Delete time of jobs
            //GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());

            // Use On methods
            //[ProlongExpirationTime]
        }
    }
}
