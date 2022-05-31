using System;
using System.Net.Mail;

namespace OB.BL.Operations.Helper.Interfaces
{
    public interface IProjectGeneral
    {
        /// <summary>
        /// Sends the given MailMessage to the serverName SMTP server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="serverName"></param>
        /// <param name="port"></param>
        /// <param name="useSSL"></param>
        void SendMailMessage(MailMessage message, string serverName, int? port, bool? useSSL);

        bool SendMail(string FromAddress, string FromDisplay, string To, string BCC, string CC, string Subject, string Body, bool IsHTML, string ServerName, int? Port, bool? UseSSL);
        string SerializeAnObject(object AnObject);
        
        /// ---- DeSerializeAnObject ------------------------------
        /// <summary>
        /// DeSerialize an object
        /// </summary>

        /// <param name="XmlOfAnObject">The XML string</param>
        /// <param name="ObjectType">The type of object</param>
        /// <returns>A deserialized object...must be cast to correct type</returns>
        Object DeSerializeAnObject(string XmlOfAnObject, Type ObjectType);

        /// <summary>
        /// Serializes any object to xml string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="theObject"></param>
        /// <returns></returns>
        string Serialize<T>(T theObject);

        string Serialize<T>(T theObject, string rootItem, string rootNS);

        bool IsTheSameDay(DateTime date1, DateTime date2);
    }
}
