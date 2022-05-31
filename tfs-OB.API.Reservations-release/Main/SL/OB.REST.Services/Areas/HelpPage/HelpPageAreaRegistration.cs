using System.Web.Http;
using System.Web.Mvc;

namespace OB.REST.Services.Areas.HelpPage
{
    /// <summary>
    /// Class that is fetch by the ASP.NET MVC engine. The engine looks for all classes that inherit from AreaRegistration and calls
    /// calling RegisterArea method.
    /// </summary>
    /// <see cref="AreaRegistration"/>
    public class HelpPageAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// Name of the Area
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        /// <summary>
        /// Registers all routes for the HelpPage Area.
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            context.Routes.IgnoreRoute("HelpPage/Content/Images/*.png");
            context.Routes.IgnoreRoute("Help/Content/Images/*.png");

            context.Routes.IgnoreRoute("HelpPage/Content/*.zip");
            context.Routes.IgnoreRoute("Help/Content/*.zip");


            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}