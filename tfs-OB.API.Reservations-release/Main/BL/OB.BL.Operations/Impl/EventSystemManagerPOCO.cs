using OB.BL.Contracts.Data.General;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects.Errors;
using OB.Log.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OB.BL.Operations.Impl
{
    public class EventSystemManagerPoco : BusinessPOCOBase, IEventSystemManagerPOCO
    {
        private bool NotificationIsEnableForProperty(long propertyUid)
        {
            Setting enableLogValue = RepositoryFactory.GetOBAppSettingRepository().ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "NewLogs_Properties" } }).FirstOrDefault();

            List<string> propertiesList = (enableLogValue != null && enableLogValue.Value != null) ? enableLogValue.Value.Split(',').ToList() : new List<string>();
            if (propertiesList.Contains(propertyUid.ToString()) || propertiesList.Contains("All"))
                return true;

            return false;
        }

        public void SendMessage(OB.Events.Contracts.ICustomNotification message)
        {
            //If notification is not active for this property
            if (!NotificationIsEnableForProperty(message.PropertyUID))
            {
                Logger.Info(new LogMessageBase
                {
                    MethodName = nameof(SendMessage),
                    Description = $"Notifications disable for property: {message.PropertyUID}",
                }, new LogEventPropertiesBase
                {
                    OtherInfo = new Dictionary<string, object>
                    {
                        { "EventId", message.NotificationGuid },
                        { "EntityKey", message.EntityKey },
                        { "EntityName", message.ActionName },
                        { "ReservationNumber", message.Description }
                    }
                });

                return;
            }

            try
            {
                Events.Queue.Coordinator.Client.CoordinatorClient.SendMessage(message).Wait();
                Logger.Info(new LogMessageBase
                {
                    MethodName = nameof(SendMessage),
                    Description = $"Event Message sent: { message.NotificationGuid}",
                }, new LogEventPropertiesBase
                {
                    OtherInfo = new Dictionary<string, object>
                    {
                        { "EventId", message.NotificationGuid },
                        { "EntityKey", message.EntityKey },
                        { "EntityName", message.ActionName },
                        { "ReservationNumber", message.Description }
                    }
                });
            }
            catch (Exception ex)
            {
                #region Log Exception

                Logger.Error(ex, new LogMessageBase
                {
                    MethodName = nameof(SendMessage),
                    Description = Errors.SendNotificationError.ToString(),
                    Code = $"{(int)Errors.SendNotificationError}",
                }, new LogEventPropertiesBase
                {
                    Request = message,
                    OtherInfo = new Dictionary<string, object>
                    {
                        { "EventId", message.NotificationGuid },
                        { "EntityKey", message.EntityKey },
                        { "EntityName", message.ActionName },
                        { "ReservationNumber", message.Description }
                    }
                });

                // Send Email
                var arguments = new Dictionary<string, object> { { "message", message } };
                Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, arguments, null, "Error Sending Notification to MQ");

                #endregion
            }
        }
    }
}
