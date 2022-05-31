using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Responses;
using System.Collections.Generic;
using System.Linq;

namespace OB.BL.Operations.Impl
{
    internal partial class ReservationValidatorPOCO : BusinessPOCOBase, IReservationValidatorPOCO
    {
        /// <summary>
        /// Validates if reservations have Partial Payments when Reservation is from Booking Engine and the payment method is Credit Card.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Exception is fail validation, otherwise returns <c>null</c></returns>
        public BusinessLayerException ValidateRateChannelPaymentParcels(InitialValidationRequest request)
        {
            BusinessLayerException exception = null;

            if (request.GroupRule?.BusinessRules.HasFlag(BusinessRules.ValidateRateChannelPartialPayments) != true)
                return exception;

            // If reservation or reservation rooms are not filled then ignores the validation
            if (request.Reservation == null || request.ReservationRooms == null)
                return exception;

            // The flag BlockPartialPayment is only applied to the Booking Engine Channel
            if (request.Reservation.Channel_UID != Constants.BookingEngineChannelId)
                return exception;

            // The flag BlockPartialPayment is only applied to Credit Card payments
            if (request.Reservation.PaymentMethodType_UID != (long)Constants.PaymentMethodTypesCode.CreditCard)
                return exception;

            // If reservation has no Partial Payments then it is valid for this validation
            if (request.Reservation.IsPartialPayment != true)
                return exception;

            // Get Rate Channels from OB.API
            var rateChannelsRepo = RepositoryFactory.GetOBRateRepository();
            var rateChannels = rateChannelsRepo.ListRateChannelsDetails(new Contracts.Requests.ListRateChannelsRequest
            {
                RequestId = request.RequestId,
                PropertyIds = new HashSet<long> { request.Reservation.Property_UID },
                ChannelIds = new HashSet<long> { request.Reservation.Channel_UID.Value },
                RateIds = request.ReservationRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).Distinct().ToHashSet(),
                IncludePaymentMethodIds = true,
                CategoryType = Contracts.Enums.RateCategoryType.All
            });

            // Should return allways at least one RateChannel. 
            // When there are no RateChannels we consider that reservation is invalid because we dont know the payment configurations and it more secure to reject it.
            if (!rateChannels.Any())
                return Errors.PaymentMethodNotSupportPartialPayments.ToBusinessLayerException();

            // Get Credit Card Payment of RateChannels related to the reservation
            var rateChannelsCCPayments = rateChannels
                .Where(x => x.PaymentTypes != null)
                .SelectMany(x => x.PaymentTypes)
                .Where(x => x.Key == request.Reservation.PaymentMethodType_UID && !x.IsDeleted);

            // If any rate channels has blocked partial payments and the reservation have parcels
            if (rateChannelsCCPayments.Any(x => x.BlockPartialPayments))
                return Errors.PaymentMethodNotSupportPartialPayments.ToBusinessLayerException();

            return exception;
        }
    }
}
