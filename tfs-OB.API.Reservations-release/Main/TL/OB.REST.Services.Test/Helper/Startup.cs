using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OB.REST.Services.Test.Helper.Startup))]
namespace OB.REST.Services.Test.Helper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(LogMiddleware));
        }
    }
}