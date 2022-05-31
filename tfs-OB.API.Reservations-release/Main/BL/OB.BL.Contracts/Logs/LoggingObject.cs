using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Payments;
using System;

namespace OB.Reservation.BL.Contracts.Logs
{
    public class LoggingObject
    {
        public LoggingObject()
        {
        }

        public long propertyID { get; set; }

        public string propertyName { get; set; }        

        public string ChannelName { get; set; }

        public long? PropertyBaseCurrencyId { get; set; }

        public Nullable<DateTime> MessageDate { get; set; }

        public ServiceName ServiceName { get; set; }

        public bool UserIsGuest { get; set; }

        public string UserName { get; set; }

        public long UserID { get; set; }

        public string CorrelationID { get; set; }

        public Operation Operation { get; set; }

        public TypeMessage MessageType { get; set; }

        public string UserHostAddress { get; set; }

        public string UserHostName { get; set; }

        /// <summary>
        /// The Big Pull user that made the reservation operation (insert, modify, cancel).
        /// </summary>
        public long? BigPullAuthRequestor_UID { get; set; }
    }
}