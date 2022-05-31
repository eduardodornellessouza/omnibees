using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.Domain.Reservations;
using OB.BL.Operations.Extensions;

namespace OB.BL.Operations.Impl
{
    internal partial class ReservationValidatorPOCO : BusinessPOCOBase, IReservationValidatorPOCO
    {
        /// <summary>
        /// Implementation of the InitialValidation operation.
        /// This method will contain all the initial validations from Reservation into the Datastore given it's associated objects.     
        /// Suggestion: whenever you want to add a new validation you must create a new class with a single scope for the validation purpose,
        /// the method of this new class must return a BusinessLayerException and should be called within the main InitialValidation method using the ValidateToThrowException extension.
        /// As an example you can see the class ValidateIncentiveBookingWindow
        /// </summary>
        /// <param name="request">Need the group rules and the object reservation to validate a reservation.</param>
        public void InitialValidation(InitialValidationRequest request)
        {
            if (request.GroupRule?.BusinessRules.HasFlag(BusinessRules.ValidateReservation) != true)
                return;

            var validator = Resolve<IReservationValidatorPOCO>();

            validator
                .ValidateIncentiveBookingWindow(request)
                .ValidateToThrowException();

            validator
                .ValidateRateChannelPaymentParcels(request)
                .ValidateToThrowException();
        }
    }
}
