using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net;
using System.Security.Claims;
using System.Security.Permissions;

namespace OB.REST.Services.Attributes
{ 
    public class RequiresPermission : AuthorizationFilterAttribute
    {


        public static string ClaimTypePermissions = "http://myhotel.omnibees.com/claims/permissions";
        private static string[] _emptyArray = new string[0];

        public string Clients { get; set; }

        // Summary:
        //     Gets or sets the authorized permissions separated by commas..
        //
        // Returns:
        //     The permissions string.
        public string Permissions { get; set; }

        //public Permissions Permissions { get; set; }
        public RequiresPermission(string permissions)
        {
            this.Permissions = permissions;
        }

        /// <summary>Calls when an action is being authorized.</summary>
        /// <param name="actionContext">The context.</param>
        /// <exception cref="T:System.ArgumentNullException">The context parameter is null.</exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }
            if (SkipAuthorization(actionContext))
            {
                return;
            }
            if (!this.IsAuthorized(actionContext))
            {
                this.HandleUnauthorizedRequest(actionContext);
            }
        }
        // System.Web.Http.AuthorizeAttribute
        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any<AllowAnonymousAttribute>() || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any<AllowAnonymousAttribute>();
        }

        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }
            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Request not authorized");
        }

        // System.Web.Http.AuthorizeAttribute
        internal static string[] SplitString(string original)
        {
            
            if (string.IsNullOrEmpty(original))
            {
                return _emptyArray;
            }
            IEnumerable<string> source =
                from piece in original.Split(new char[]
		{
			','
		})
                let trimmed = piece.Trim()
                where !string.IsNullOrEmpty(trimmed)
                select trimmed;
            return source.ToArray<string>();
        }

      
        protected virtual bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var principal = actionContext.ControllerContext.RequestContext.Principal;
            var isValid = principal != null && principal.Identity != null
                && principal.Identity is ClaimsIdentity
                && principal.Identity.IsAuthenticated;
            if (!isValid)
                return false;

            ClaimsIdentity identity = principal.Identity as ClaimsIdentity;
            
            //ADMIN SUPER USER (Has access to everything).
            if (principal.IsInRole("Admin"))
                return true;

            var permissionsClaims = identity.Claims.Where(x => x.Type == "urn:oauth:permission");

            var permissionsSplitted = SplitString(this.Permissions);

            return permissionsSplitted.Any(p1 => permissionsClaims.Any(p2 => string.Equals(p1,p2.Value.Trim(),StringComparison.InvariantCultureIgnoreCase)));
                        
            //if(permissionsClaim != null)
            //{
            //    var permissionsArray = SplitString(permissionsClaim.Value);
            //    var permissionsEnum = permissionsArray[roleIndex];
            //    return ((Permissions)Enum.Parse(typeof(Permissions), permissionsEnum)).HasFlag(this.Permissions);
            //}
            //return false;
        }


        //[Flags]
        //public enum Permissions
        //{
        //    None = 0,     //00000000
        //    CanRead = 1,  //00000001
        //    CanAdd = 2,   //00000010
        //    CanUpdate = 4,//00000100
        //    CanDelete = 8 //00001000
        //}
    }
}