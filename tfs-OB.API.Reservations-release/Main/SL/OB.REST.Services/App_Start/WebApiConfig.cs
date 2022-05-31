using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OB.Log;
using OB.REST.Services.Areas.HelpPage;
using OB.REST.Services.Formatters;
using OB.REST.Services.Loggers;
using System;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;


namespace OB.REST.Services
{
    public static class WebApiConfig
    {
        public static ILogger _logger = LogsManager.CreateLogger(typeof(WebApiConfig));
        public static ILogger _jsonLogger = LogsManager.CreateLogger(typeof(JsonMediaTypeFormatter));
        public static ILogger _bsonLogger = LogsManager.CreateLogger(typeof(BsonMediaTypeFormatter));

        public static void Register(HttpConfiguration config)
        {
            var before = DateTime.Now.Ticks;

            lock (AppDomain.CurrentDomain)
            {
                var isInitialized = AppDomain.CurrentDomain.GetData("webAPIHasBeenInitialized") != null;
                if(isInitialized)
                {
                    _logger.Warn("WebApiConfig.Register HAS ALREADY BEEN CALLED, CHECK YOUR CONFIGURATION");
                    return;
                }
                AppDomain.CurrentDomain.SetData("webAPIHasBeenInitialized",true);                
            }


            _logger.Info("Starting \"WebApiConfig.Register\"");

            // Web API configuration and services


            // change security protocol app wide!
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;

            // Web API routes
            config.MapHttpAttributeRoutes();

            var corsAttributes = new EnableCorsAttribute("*","*", "*");
            config.EnableCors(corsAttributes);

            MapRoutes(config);
          
            var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                //NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                //PreserveReferencesHandling = PreserveReferencesHandling.All,
                //DefaultValueHandling = DefaultValueHandling.Ignore,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,                
                TraceWriter = new NLogJsonTraceWriter(typeof(JsonMediaTypeFormatter).FullName)
            };
            jsonSerializerSettings.Error += (s, args) =>
            {
                var currentObj = args.CurrentObject;
                var errorContext = args.ErrorContext;
                var ex = errorContext.Error;
                var request = HttpContext.Current.Request;

                _jsonLogger.Warn(ex, "JsonMediaTypeFormatter:"
                    + "\tMember:" + (errorContext != null && errorContext.Member != null ? errorContext.Member : string.Empty)
                    + "\tPath: " + errorContext.Path);
            };

            var bsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,                
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                TraceWriter = new NLogJsonTraceWriter(typeof(BsonMediaTypeFormatter).FullName),
            };

            bsonSerializerSettings.Error += (s, args) =>
            {
                var currentObj = args.CurrentObject;
                var errorContext = args.ErrorContext;
                var ex = errorContext.Error;
                var request = HttpContext.Current.Request;

                _bsonLogger.Warn(ex, "BsonMediaTypeFormatter:"
                    + "\tMember:" + (errorContext != null && errorContext.Member != null ? errorContext.Member : string.Empty)
                    + "\tPath: " + errorContext.Path);
            };

        
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings = jsonSerializerSettings;
                           
            config.Formatters.Add(new BsonMediaTypeFormatter() { SerializerSettings = bsonSerializerSettings });
            config.Formatters.Add(new HtmlJsonMediaFormatter());

           
            var assemblyName = typeof(WebApiConfig).Assembly.GetName().Name;
            var xmlDocumentationProvider = new XmlDocumentationProvider(assemblyName,
                            HttpContext.Current.Server.MapPath("~/App_Data/" + assemblyName + ".xml"));

            assemblyName = typeof(OB.Reservation.BL.Contracts.ContractBase).Assembly.GetName().Name;
            xmlDocumentationProvider.AddDocumentationAssembly(assemblyName,
                            HttpContext.Current.Server.MapPath("~/App_Data/" + assemblyName + ".xml"));

            config.SetDocumentationProvider(xmlDocumentationProvider);

            _logger.Info("Finished \"WebApiConfig.Register\" .TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).Seconds + "s");

        }

        public static void MapRoutes(HttpConfiguration config)
        {

            //config.Routes.MapHttpRoute(
            //      name: "DefaultApi",
            //      routeTemplate: "api/{controller}/{id}",
            //      defaults: new { id = RouteParameter.Optional }
            //  );

            //config.Routes.MapHttpRoute(
            // name: "DefaultGetApi",
            // routeTemplate: "api/{controller}/{id}",
            // defaults: new { action = RouteParameter.Optional },
            // constraints: new { controller = @"Reservation", id = @"/d+", action = "Get" }
            //);

           
            config.Routes.MapHttpRoute(
            name: "DefaultServiceApi",
            routeTemplate: "api/{controller}/{action}/{id}",
            defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional }
                //constraints: new { controller = @"Reservation" }
           );
        }
    }
}
