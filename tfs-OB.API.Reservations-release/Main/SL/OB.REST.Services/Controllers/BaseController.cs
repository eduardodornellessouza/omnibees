using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace OB.REST.Services.Controllers
{
    /// <summary>
    /// Base class for all Controllers in the REST Service.
    /// This class adds Authorization to all Actions.
    /// Any Controller that extends directly from ApiController besides BaseController will break the BUILD in TeamCity!
    /// </summary>
    [Authorize]
    public abstract class BaseController : ApiController
    {
       
    }
}