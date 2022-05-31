using OB.BL.Operations.Helper.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace OB.BL.Operations.Helper
{
    public class LogEmail : BusinessPOCOBase, ILogEmail
    {
        private IProjectGeneral _projectGeneral;

        public LogEmail(IProjectGeneral projectGeneral)
        {
            _projectGeneral = projectGeneral;
        }

        /// <summary>
        /// Method used to send the error from silverlight
        /// </summary>        
        public void SendEmail(string jsonArguments)
        {
            try
            {
                LogJsonArguments args = new LogJsonArguments();
                try
                {
                    // method to serialize to json using system libraries            
                    args = JsonSerializer.DeserializeFromJson<LogJsonArguments>(jsonArguments);
                }
                catch (Exception ex)
                {
                    args = new LogJsonArguments();
                    args.MethodSignature = "ProturRIAServices.Web.Helper.LogEmail.SendEmail(string jsonArguments)";
                    args.SendToConnectorTeam = false;
                    args.SendToOBTeam = true;
                    args.Severity = LogSeverity.Medium;
                    args.Subject = "Error on LogEmail";
                    args.ExceptionToString = ex.ToString();
                    args.Arguments = new Dictionary<string, string>();
                    args.Arguments.Add("jsonArguments", jsonArguments);
                }

                SendEmailAllLogic(args.MethodSignature, args.ExceptionToString, args.Severity, args.Arguments, args.ManualInfo, args.Subject, args.To, args.SendToOBTeam, args.SendToConnectorTeam);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Method used to send the error from ria services
        /// </summary>
        public void SendEmail(MethodBase method, Exception ex, LogSeverity severity, Dictionary<string, object> arguments,
            Dictionary<string, object> manualInfo = null,
            string subject = null,
            string to = null,
            bool sendToOBTeam = true,
            bool sendToConnectorTeam = false)
        {
            // This will start your operation in the background           
            QueueBackgroundWork(() =>
            {
                SendEmailAsync(method, ex, severity, arguments, manualInfo, subject, to, sendToOBTeam, sendToConnectorTeam);
            }, false);
        }

        /// <summary>
        /// Method used to send the error from ria services
        /// </summary>
        private void SendEmailAsync(MethodBase method, Exception ex, LogSeverity severity, Dictionary<string, object> arguments,
            Dictionary<string, object> manualInfo = null,
            string subject = null,
            string to = null,
            bool sendToOBTeam = true,
            bool sendToConnectorTeam = false)
        {
            try
            {
                LogHelper logHelper = new LogHelper();
                SendEmailAllLogic(logHelper.GetMethodSignature(method), logHelper.GetExceptionMessage(ex), severity, GetDictionaryJSON(arguments), GetDictionaryJSON(manualInfo), subject, to, sendToOBTeam, sendToConnectorTeam);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Main method to send the error to email
        /// </summary>
        private void SendEmailAllLogic(string methodSignature, string exceptionMessage, LogSeverity severity, Dictionary<string, string> arguments,
            Dictionary<string, string> manualInfo = null,
            string subject = null,
            string to = null,
            bool sendToOBTeam = true,
            bool sendToConnectorTeam = false)
        {
            try
            {
                StringBuilder emailContent = new StringBuilder();
                string EnvironmentMachine = ConfigurationManager.AppSettings["EnvironmentMachine"] ?? "Unknown Environment";
                string SourceApplication = ConfigurationManager.AppSettings["SourceApplicationName"] ?? "Unknown Application";
                string MethodSignature = string.Empty;

                #region Exception
                if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    emailContent.Append(exceptionMessage);
                    emailContent.Append("<br />");
                    emailContent.Append("<br />");
                }
                #endregion

                #region General details
                emailContent.Append("<strong>General info</strong>");
                emailContent.Append("<table cellspacing='0' cellpadding='0' border='0' width='100%' style='padding:5px; border: 1px solid #000'>");

                // IP Address
                emailContent.Append(TwoColumnRow("IP Address", System.Net.Dns.GetHostAddresses(Environment.MachineName)[0].ToString()));

                // machine key
                emailContent.Append(TwoColumnRow("Environment.MachineName", Environment.MachineName));

                // server
                emailContent.Append(TwoColumnRow("Server", EnvironmentMachine));

                // source application
                emailContent.Append(TwoColumnRow("Application", SourceApplication));

                // date
                emailContent.Append(TwoColumnRow("Date", DateTime.UtcNow.ToString("r")));

                // severity
                emailContent.Append(TwoColumnRow("Severity", severity.ToString()));

                // request info
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    if (HttpContext.Current.Request.Url != null && HttpContext.Current.Request.Url.AbsoluteUri != null)
                        emailContent.Append(TwoColumnRow("Url", HttpContext.Current.Request.Url.AbsoluteUri));

                    if (HttpContext.Current.Request.UserHostName != null)
                        emailContent.Append(TwoColumnRow("UserHostName", HttpContext.Current.Request.UserHostName));

                    if (HttpContext.Current.Request.UserHostName != HttpContext.Current.Request.UserHostAddress)
                        emailContent.Append(TwoColumnRow("UserHostAddress", HttpContext.Current.Request.UserHostAddress));

                    if (HttpContext.Current.Request.UserAgent != null)
                        emailContent.Append(TwoColumnRow("UserAgent", HttpContext.Current.Request.UserAgent));
                }

                emailContent.Append("</table>");
                emailContent.Append("<br />");
                #endregion

                #region Method info
                // method info
                if (!string.IsNullOrEmpty(methodSignature))
                {
                    MethodSignature = methodSignature;
                    emailContent.Append("<strong>Method info</strong>");
                    emailContent.Append("<table cellspacing='0' cellpadding='0' border='0' width='100%' style='padding:5px; border: 1px solid #000'>");
                    emailContent.Append(TwoColumnRow("MethodSignature", MethodSignature));
                    emailContent.Append("</table>");
                    emailContent.Append("<br />");
                }
                #endregion

                #region Arguments
                // extra info
                if (arguments != null && arguments.Any())
                {
                    emailContent.Append("<strong>Method arguments</strong>");
                    emailContent.Append("<table cellspacing='0' cellpadding='0' border='0' width='100%' style='padding:5px; border: 1px solid #000'>");
                    foreach (KeyValuePair<string, string> argumentInfo in arguments)
                        emailContent.Append(TwoColumnRow(argumentInfo.Key, argumentInfo.Value));
                    emailContent.Append("</table>");
                    emailContent.Append("<br />");
                }
                #endregion Arguments

                #region Extra Info
                // extra info
                if (manualInfo != null && manualInfo.Any())
                {
                    emailContent.Append("<strong>Extra info</strong>");
                    emailContent.Append("<table cellspacing='0' cellpadding='0' border='0' width='100%' style='padding:5px; border: 1px solid #000'>");
                    foreach (KeyValuePair<string, string> infoEntry in manualInfo)
                        emailContent.Append(TwoColumnRow(infoEntry.Key, infoEntry.Value));
                    emailContent.Append("</table>");
                    emailContent.Append("<br />");
                }
                #endregion Extra Info
                
                if (subject == null)
                    subject = "Error " + SourceApplication + " " + EnvironmentMachine + " " + Environment.MachineName;
                else
                    subject = "Error " + SourceApplication + " " + EnvironmentMachine + " " + Environment.MachineName + " - " + subject;

                string from = ConfigurationManager.AppSettings["FromAddress"];
                string servername = ConfigurationManager.AppSettings["FromAddress"];

                var destinationEmailsList = new List<string>();

                if (!string.IsNullOrEmpty(to))
                    destinationEmailsList.Add(to);

                if (sendToOBTeam)
                    destinationEmailsList.Add(ConfigurationManager.AppSettings["EmailsErrorDistributionList"]);

                if (sendToConnectorTeam)
                    destinationEmailsList.Add(ConfigurationManager.AppSettings["IntegrationTeamDistributionList"]);

                // Build destination emails addresses
                to = string.Join(";", destinationEmailsList).Replace(";;",";");
                if (to != null && !to.EndsWith(";", StringComparison.InvariantCulture))
                    to += ";";

                _projectGeneral.SendMail(from, subject, to, "", "", subject, emailContent.ToString(), true, null, null, false);
            }
            catch (Exception ex1)
            {
                EventLog.WriteEntry(subject + " at Log.SendEmail", ex1.ToString(), EventLogEntryType.Error);
            }
        }

        private string GetMethodInfo(MethodBase method)
        {
            StringBuilder result = new StringBuilder();
            result.Append(TwoColumnRow("Method Name", method.Name));
            result.Append(TwoColumnRow("Method ScopeName", method.Module.ScopeName));
            return result.ToString();
        }

        private string TwoColumnRow(string column1Text, string column2Text)
        {
            return
                "<tr>" +
                     "<td style='border-top:1px solid #000; vertical-align:text-top;' width='150' valign='top'><b>" + column1Text + "</b></td>" +
                     "<td style='border-top:1px solid #000;'>" + column2Text + "</td>" +
                "</tr>";
        }

        private Dictionary<string, string> GetDictionaryJSON(Dictionary<string, object> original)
        {
            if (original != null && original.Any())
            {
                try
                {
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, object> pair in original)
                    {
                        result.Add(pair.Key, pair.Value != null ? HttpUtility.HtmlEncode(pair.Value.ToJSON(true)) : "null");
                    }

                    return result;
                }
                catch 
                {
                    return null;
                }
            }
            else
                return null;
        }
    }
}
