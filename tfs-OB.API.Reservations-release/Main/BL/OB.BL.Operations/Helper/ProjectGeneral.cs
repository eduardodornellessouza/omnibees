using OB.Reservation.BL.Contracts.Requests;
using OB.BL.Operations.Helper.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace OB.BL.Operations.Helper
{
    public class ProjectGeneral : IProjectGeneral
    {
        public static readonly string FromEmail = string.IsNullOrEmpty(WebConfigurationManager.AppSettings["FromEmail"]) ? "" : WebConfigurationManager.AppSettings["FromEmail"].ToString();

        public static readonly string ImagePath = string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ImagePatha"]) ? "" : WebConfigurationManager.AppSettings["ImagePatha"].ToString();

        public static readonly string BookingEnginePath2 = string.IsNullOrEmpty(WebConfigurationManager.AppSettings["BookingEnginePath2"]) ? "" : WebConfigurationManager.AppSettings["BookingEnginePath2"].ToString();

        public static readonly string omnibeesURL = string.IsNullOrEmpty(WebConfigurationManager.AppSettings["OmnibeesBaseUrl"]) ? "" : WebConfigurationManager.AppSettings["OmnibeesBaseUrl"].ToString();

        /// <summary>
        /// Send Mail
        /// </summary>
        /// <param name="FromAddress"></param>
        /// <param name="FromDisplay"></param>
        /// <param name="To"></param>
        /// <param name="BCC"></param>
        /// <param name="CC"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="IsHTML"></param>
        /// <returns></returns>
        public bool SendMail(string FromAddress, string FromDisplay, string To, string BCC, string CC, string Subject, string Body, bool IsHTML, string ServerName, int? Port, bool? UseSSL)
        {
            // Instantiate a new instance of MailMessage
            MailMessage mMailMessage = new MailMessage();

            if (!string.IsNullOrEmpty(FromAddress))
            {
                // Set the sender address of the mail message
                mMailMessage.From = new MailAddress(FromAddress, FromDisplay);
                //mMailMessage.fr.
            }

            char[] arrSplitChar = { ',' };

            if (!string.IsNullOrEmpty(To))
            {
                string[] arrTo = To.Split(arrSplitChar);
                foreach (string strTO in arrTo)
                {
                    if (strTO.Contains(";"))
                    {
                        char[] arrSplitTempChar = { ';' };
                        string[] arrTempTo = strTO.Split(arrSplitTempChar);
                        foreach (string strTOTemp in arrTempTo)
                        {
                            if (!string.IsNullOrEmpty(strTOTemp))
                            {
                                // Set the recepient address of the mail message
                                mMailMessage.To.Add(new MailAddress(strTOTemp));
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strTO))
                        {
                            // Set the recepient address of the mail message
                            mMailMessage.To.Add(new MailAddress(strTO));
                        }
                    }
                }
            }


            // Check if the bcc value is nothing or an empty string
            if (!string.IsNullOrEmpty(BCC))
            {
                string[] arrBCC = BCC.Split(arrSplitChar);
                foreach (string strBCC in arrBCC)
                {
                    if (strBCC.Contains(";"))
                    {
                        char[] arrSplitTempChar = { ';' };
                        string[] arrTempBCC = strBCC.Split(arrSplitTempChar);
                        foreach (string strBCCTemp in arrTempBCC)
                        {
                            // Set the recepient address of the mail message
                            mMailMessage.Bcc.Add(new MailAddress(strBCCTemp));
                        }
                    }
                    else
                    {
                        // Set the recepient address of the mail message
                        mMailMessage.Bcc.Add(new MailAddress(strBCC));
                    }
                }
            }

            // Check if the cc value is nothing or an empty value
            if (!string.IsNullOrEmpty(CC))
            {
                string[] arrCC = CC.Split(arrSplitChar);
                foreach (string strCC in arrCC)
                {
                    if (strCC.Contains(";"))
                    {
                        char[] arrSplitTempChar = { ';' };
                        string[] arrTempCC = strCC.Split(arrSplitTempChar);
                        foreach (string strCCTemp in arrTempCC)
                        {
                            // Set the recepient address of the mail message
                            mMailMessage.CC.Add(new MailAddress(strCCTemp));
                        }
                    }
                    else
                    {
                        // Set the recepient address of the mail message
                        mMailMessage.CC.Add(new MailAddress(strCC));
                    }
                }
            }

            // Set the subject of the mail message
            mMailMessage.Subject = Subject;

            // Set the body of the mail message
            mMailMessage.Body = Body;

            // Set the format of the mail message body as HTML
            mMailMessage.IsBodyHtml = IsHTML;
            // Set the priority of the mail message to normal
            mMailMessage.Priority = MailPriority.Normal;

            try
            {
                // Send the mail message
                this.SendMailMessage(mMailMessage, ServerName, Port, UseSSL);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the given MailMessage to the serverName SMTP server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="serverName"></param>
        /// <param name="port"></param>
        /// <param name="useSSL"></param>
        public virtual void SendMailMessage(MailMessage message, string serverName, int? port, bool? useSSL)
        {
            // Instantiate a new instance of SmtpClient
            SmtpClient mSmtpClient = new SmtpClient();

            if (!string.IsNullOrEmpty(serverName))
                mSmtpClient.Host = serverName;
            if (port != null)
                mSmtpClient.Port = port.Value;
            if (useSSL != null)
                mSmtpClient.EnableSsl = useSSL.Value;

            mSmtpClient.Send(message);
        }

        //ntelo
        /// ---- SerializeAnObject -----------------------------
        /// <summary>
        /// Serializes an object to an XML string
        /// </summary>
        /// <param name="AnObject">The Object to serialize</param>

        /// <returns>XML string</returns>
        public string SerializeAnObject(object AnObject)
        {
            if (AnObject != null)
            {
                try
                {
                    XmlSerializer Xml_Serializer = new XmlSerializer(AnObject.GetType());

                    StringWriter Writer = new StringWriter();

                    Xml_Serializer.Serialize(Writer, AnObject);

                    return Writer.ToString();
                }
                catch (Exception)
                {
                    return AnObject.ToJSON();
                }
            }
            else
                return string.Empty;
        }

        /// ---- DeSerializeAnObject ------------------------------
        /// <summary>
        /// DeSerialize an object
        /// </summary>

        /// <param name="XmlOfAnObject">The XML string</param>
        /// <param name="ObjectType">The type of object</param>
        /// <returns>A deserialized object...must be cast to correct type</returns>
        public Object DeSerializeAnObject(string XmlOfAnObject, Type ObjectType)
        {
            StringReader StrReader = new StringReader(XmlOfAnObject);
            XmlSerializer Xml_Serializer = new XmlSerializer(ObjectType);
            XmlTextReader XmlReader = new XmlTextReader(StrReader);
            try
            {
                Object AnObject = Xml_Serializer.Deserialize(XmlReader);
                return AnObject;
            }

            finally
            {
                XmlReader.Close();
                StrReader.Close();
            }


        }

        /// <summary>
        /// Serializes any object to xml string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="theObject"></param>
        /// <returns></returns>
        public string Serialize<T>(T theObject)
        {
            return Serialize<T>(theObject, null, null);
        }
        public string Serialize<T>(T theObject, string rootItem, string rootNS)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = null;
                if (rootItem == null)
                    serializer = new DataContractSerializer(typeof(T));
                else
                    serializer = new DataContractSerializer(typeof(T), rootItem, rootNS);
                serializer.WriteObject(memoryStream, theObject);
                memoryStream.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(memoryStream);
                string content = reader.ReadToEnd();
                return content;
            }
        }

        public bool IsTheSameDay(DateTime date1, DateTime date2)
        {
            return (date1.Year == date2.Year && date1.DayOfYear == date2.DayOfYear);
        }

        public static string GetEnumDescription(Enum value)
        {
            if (value == null)
                return string.Empty;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Validate if the PropertyUID is contained in the Setting Value.
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="propertyUID"></param>
        /// <returns></returns>
        public static bool IsValidProperty(OB.BL.Contracts.Data.General.Setting setting, long propertyUID)
        {
            return setting != null && !string.IsNullOrEmpty(setting.Value) && 
                ("all".Equals(setting.Value, StringComparison.InvariantCultureIgnoreCase) || setting.Value.Split(',').Any(x => propertyUID.ToString() == x.Trim()));
        }

        public static bool IsValidVersion(OB.Reservation.BL.Contracts.Data.Version minimumVersion, OB.Reservation.BL.Contracts.Data.Version currentVersion)
        {
            return minimumVersion != null && currentVersion != null && 
                currentVersion.Major >= minimumVersion.Major && currentVersion.Minor >= minimumVersion.Minor && currentVersion.Patch >= minimumVersion.Patch;
        }

        public static bool IsValidVersion(int minMajor, int minMinor, int minPatch, OB.Reservation.BL.Contracts.Data.Version currentVersion)
        {
            return currentVersion != null && currentVersion.Major >= minMajor && currentVersion.Minor >= minMinor && currentVersion.Patch >= minPatch;
        }
    }
}