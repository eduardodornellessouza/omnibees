using System;
using System.Collections.Generic;
using System.Reflection;

namespace OB.BL.Operations.Helper.Interfaces
{
    public interface ILogEmail
    {
        /// <summary>
        /// Method used to send the error from silverlight
        /// </summary>        
        void SendEmail(string jsonArguments);
        
        /// <summary>
        /// Method used to send the error from ria services
        /// </summary>
        void SendEmail(MethodBase method, Exception ex, LogSeverity severity, Dictionary<string, object> arguments,
            Dictionary<string, object> manualInfo = null,
            string subject = null,
            string to = null,
            bool sendToOBTeam = true,
            bool sendToConnectorTeam = false);
    }
}
