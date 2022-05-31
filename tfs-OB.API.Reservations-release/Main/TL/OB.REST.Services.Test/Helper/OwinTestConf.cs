using Microsoft.Practices.Unity;
using OB.BL.Operations;
using OB.DL.Common;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace OB.REST.Services.Test.Helper
{
    public class OwinTestConf
    {
        public ClaimsPrincipal User { get; set; }
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(LogMiddleware));
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            
            var container = new UnityContainer();
            container.AddExtension(new DataAccessLayerModule());
            container.AddExtension(new BusinessLayerModule());
            config.DependencyResolver = new UnityResolver(container);

            config.MapHttpAttributeRoutes();

            User = UnitTestMessageHandler.SetupClaimsPrincipal();
            var handler = new UnitTestMessageHandler(User);
            config.MessageHandlers.Add(handler);
           
            config.Routes.MapHttpRoute(name: "DefaultServiceApi", routeTemplate: "api/{controller}/{action}/{id}",
                                   defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional }
                //constraints: new { controller = @"Reservation" }
               );

             app.UseWebApi(config);

        }
    }



    /// <summary>
    /// This class is used to help in the Integration Tests, to pass across the Authorization attribute in the BaseController.
    /// </summary>
    public class UnitTestMessageHandler : DelegatingHandler
    {
        public UnitTestMessageHandler(ClaimsPrincipal user)
        {
            this.User = user;
        }

        public ClaimsPrincipal User { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
                                HttpRequestMessage request,
                                CancellationToken cancellationToken)
        {
            // setup the Request's principal
            var ctx = request.GetRequestContext();
            ctx.Principal = this.User;
            request.SetRequestContext(ctx);

            // make all of our requests appear "local"
            request.Properties["MS_IsLocal"] = new Lazy<bool>(() => true);

            return base.SendAsync(request, cancellationToken);
        }

        public static ClaimsPrincipal SetupClaimsPrincipal()
        {
            var claims = new List<Claim>();

            // UserId
            claims.Add(new Claim(
                issuer: @"LOCAL AUTHORITY",
                type: @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                value: @"e427434e-b922-4138-ba0b-6fd88188da12",
                valueType: @"http://www.w3.org/2001/XMLSchema#string"
            ));

            // UserName
            claims.Add(new Claim(
                issuer: @"LOCAL AUTHORITY",
                type: @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                value: @"john.doe@test.com",
                valueType: @"http://www.w3.org/2001/XMLSchema#string"
            ));

            // Provider
            claims.Add(new Claim(
                issuer: @"LOCAL AUTHORITY",
                type: @"http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                value: @"ASP.NET Identity",
                valueType: @"http://www.w3.org/2001/XMLSchema#string"
            ));

            // SecurityStamp
            claims.Add(new Claim(
                issuer: @"LOCAL AUTHORITY",
                type: @"AspNet.Identity.SecurityStamp",
                value: @"a0605331-ac20-42de-9e2c-8bc32bf3299b",
                valueType: @"http://www.w3.org/2001/XMLSchema#string"
            ));

            // Role
            claims.Add(new Claim(
                issuer: @"LOCAL AUTHORITY",
                type: @"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                value: @"Administrator",
                valueType: @"http://www.w3.org/2001/XMLSchema#string"
            ));

            var roleType = @"http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var nameType = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            var identity = new ClaimsIdentity(claims, "ApplicationCookie", nameType, roleType);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}
