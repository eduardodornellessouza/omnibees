using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace OB.REST.Services.Test.Helper
{
    public class TestWebApiResolver : IAssembliesResolver
    {
        public ICollection<System.Reflection.Assembly> GetAssemblies()
        {
            var obAssembly = typeof(OB.REST.Services.Controllers.ReservationController).Assembly;
            return new List<Assembly> { obAssembly };
        }
    }
}
