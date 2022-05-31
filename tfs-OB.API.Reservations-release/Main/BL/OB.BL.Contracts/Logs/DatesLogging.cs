using System;

namespace OB.Reservation.BL.Contracts.Logs
{
    /// <summary>
    /// Used on class RateLoggingMessageOneLine
    /// </summary>
    public class DatesLogging
    {
        public DatesLogging()
        {
        }

        public DateTime DateTo { get; set; }

        public DateTime DateFrom { get; set; }
    }
}