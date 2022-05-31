using OB.BL.Contracts.Data.BaseLogDetails;
using OB.BL.Operations.Converters.BaseLogDetailInterpreter.Interpreters;
using OB.Events.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using events = OB.Events.Contracts;

namespace OB.BL.Operations.Converters.BaseLogDetailInterpreter
{
    public class NotificationHandler : IDisposable
    {
        protected readonly Interpreter _notificationInterpreter;
        protected readonly NotificationBase _notification;

        public NotificationHandler(NotificationBase notification)
        {
            _notification = notification;

            switch (notification.Action)
            {
                // Custom Interpreters
                case events.Action.UpdateRates:
                    _notificationInterpreter = new UpdateRatesInterpreter(notification);
                    break;

                // Default Interpreter
                // Keep the Structure but Translate Entities and Properties
                default:
                    _notificationInterpreter = new DefaultInterpreter(notification);
                    break;
            }
        }

        public BaseLogDetail GetFriendlyNotification()
        {
            return _notificationInterpreter.Handle(_notification.EntityDeltas);
        }

        public GridLineDetail GetFriendlyNotificationGridLine()
        {
            return _notificationInterpreter.GetGridLine(_notification);
        }

        public void Dispose()
        {
            _notificationInterpreter.Dispose();
        }
    }
}
