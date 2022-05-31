using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OB.Services.Jobs.Server
{
    public partial class Startup
    {
        private static AuthenticationTicket _adminTicket;
        private static string _adminToken = null;
        private static IPAddress[] _allowedSubnets = new IPAddress[0];
        private static IPAddress[] _allowedSubnetMasks = new IPAddress[0];
        private NLog.Logger _logger;

        /// <summary>
        /// Configures Authentication.
        /// </summary>
        /// <param name="app"></param>
        public void ConfigureAuth(IAppBuilder app)
        {
            _logger = NLog.LogManager.GetLogger(typeof(Startup).FullName);

            //Configure Admin impersonation for allowed IP Addresses (local subnets).
            ConfigureAdminAcessFromAllowedSubnets(app);

            //Standard SAML Token (Microsoft) Authorization
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                {
                    AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                    Provider = new OAuthBearerAuthenticationProvider()
                    {
                        OnRequestToken = OnOAuthRequestToken,
                        OnValidateIdentity = OnOAuthBearerValidateIdentity
                    }
                });
        }

        /// <summary>
        /// Configures the IP addresses (from allowed subnets) that are identified in Web.Config as the clients that
        /// impersonate a SUper USer /Admin account.
        /// </summary>
        /// <param name="app"></param>
        private void ConfigureAdminAcessFromAllowedSubnets(IAppBuilder app)
        {
            string allowedSubnetsSetting = ConfigurationManager.AppSettings["AllowedSubnets"];
            string allowedSubnetMasksSetting = ConfigurationManager.AppSettings["AllowedSubnetMasks"];

            string[] addresses = string.IsNullOrEmpty(allowedSubnetsSetting) ? new string[] { } : allowedSubnetsSetting.Split(',');
            string[] masks = string.IsNullOrEmpty(allowedSubnetMasksSetting) ? new string[] { } : allowedSubnetMasksSetting.Split(',');

            _allowedSubnets = new IPAddress[addresses.Length];
            _allowedSubnetMasks = new IPAddress[addresses.Length];
            for (int i = 0; i < addresses.Length; i++)
            {
                var address = addresses[i];
                var mask = masks[i];
                _allowedSubnets[i] = IPAddress.Parse(address);
                _allowedSubnetMasks[i] = IPAddressUtility.ParseMask(_allowedSubnets[i], mask);
            }

            _adminTicket = new AuthenticationTicket(new System.Security.Claims.ClaimsIdentity(
            new List<Claim>
            {
                new Claim(ClaimTypes.Name,"Admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "Bearer"),
            new AuthenticationProperties() { ExpiresUtc = new System.DateTimeOffset(DateTime.MaxValue.AddDays(-2)), IsPersistent = true }
            );

            //Generate Admin token from Ticket object
            IDataProtector protector = app.CreateDataProtector(new string[]
				                {
					                typeof(OAuthBearerAuthenticationMiddleware).Namespace,
					                "Access_Token",
					                "v1"
                                });
            var ticketDataFormat = new TicketDataFormat(protector);
            _adminToken = ticketDataFormat.Protect(_adminTicket);
        }

        /// <summary>
        /// Handles processing OAuth bearer token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnOAuthRequestToken(OAuthRequestTokenContext context)
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var ipAddress = IPAddress.Parse(context.Request.RemoteIpAddress);
                if (IPAddressUtility.IsLocalIpAddress(ipAddress) || IPAddressUtility.IsIPAddressInSubnet(ipAddress, _allowedSubnets, _allowedSubnetMasks))
                {
                    context.Token = _adminToken;
                }
                else
                {
                    _logger.Warn("Unathorized access. No Token present for remote IP:" + ipAddress);
                }
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Handles validating the identity produced from an OAuth bearer token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnOAuthBearerValidateIdentity(OAuthValidateIdentityContext context)
        {
            if (context.Ticket == null)
            {
                var ipAddress = IPAddress.Parse(context.Request.RemoteIpAddress);
                if (IPAddressUtility.IsLocalIpAddress(ipAddress) || IPAddressUtility.IsIPAddressInSubnet(ipAddress, _allowedSubnets, _allowedSubnetMasks))
                {
                    context.Validated(_adminTicket);
                }
            }
            return Task.FromResult(0);
        }
    }

}
