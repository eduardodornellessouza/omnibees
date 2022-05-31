using OB.BL.Operations.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Mocks
{
    public class ProjectGeneralMock : ProjectGeneral
    {
        private List<MailMessage> _sentMails = new List<MailMessage>();

        public IList<MailMessage> SentMails { get { return _sentMails; } }

        public override void SendMailMessage(System.Net.Mail.MailMessage message, string serverName, int? port, bool? useSSL)
        {        
            _sentMails.Add(message);
        }
    }
}
