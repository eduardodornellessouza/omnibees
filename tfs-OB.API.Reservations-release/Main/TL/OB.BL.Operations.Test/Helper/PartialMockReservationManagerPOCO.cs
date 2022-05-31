using OB.BL.Operations.Impl;
using OB.BL.Operations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Helper
{
    public class PartialMockReservationManagerPOCO : ReservationManagerPOCO, IReservationManagerPOCO
    {
        public List<string> MethodNamesCall { get; set; }
        public int? LastCalledTripAdvisorAction { get; set; }

        protected override void QueueBackgroundWork(Func<Task> asyncAction)
        {
            if (MethodNamesCall == null)
                MethodNamesCall = new List<string>();

            MethodNamesCall.Add(asyncAction.Method.Name);

            base.QueueBackgroundWork(asyncAction);
        }

        protected override void SetTripAdvisorReview(long propertyId, long reservationId, string recipientEmail, DateTime checkout, long? countryId, int action,
            long? languageId, DateTime? checkin = null)
        {
            LastCalledTripAdvisorAction = action;

            base.SetTripAdvisorReview(propertyId, reservationId, recipientEmail, checkout, countryId, action, languageId, checkin);
        }
    }
}
