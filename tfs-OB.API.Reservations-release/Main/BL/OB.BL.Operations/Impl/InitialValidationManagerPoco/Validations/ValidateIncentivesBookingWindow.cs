using OB.Reservation.BL.Contracts.Responses;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using OB.BL.Operations.Interfaces;
using System.Collections.Generic;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.Domain.Reservations;
using OB.BL.Operations.Extensions;
using System.Linq;
using OB.BL.Contracts.Requests;
using OB.Log.Messages;
using OB.BL.Operations.Exceptions;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.DL.Common;

namespace OB.BL.Operations.Impl
{
    internal partial class ReservationValidatorPOCO : BusinessPOCOBase, IReservationValidatorPOCO
    {
        public BusinessLayerException ValidateIncentiveBookingWindow(InitialValidationRequest request)
        {
            BusinessLayerException exception = null;

            if (!request.GroupRule.BusinessRules.HasFlag(BusinessRules.ValidateBookingWindow) || !Configuration.EnableNewOffers)
                return exception;

            if (request.ReservationRooms.IsNullOrEmpty())
            {
                LoggingValidateIncentiveBookingWindow(request, Errors.ReservationError);
                return Errors.ReservationError.ToBusinessLayerException();
            }

            exception = ValidateIncentivePeriod(request);

            return exception;
        }

        private BusinessLayerException ValidateIncentivePeriod(InitialValidationRequest request)
        {
            BusinessLayerException exception = null;
            var incentiveRepo = RepositoryFactory.GetOBIncentiveRepository();

            foreach (var room in request.ReservationRooms)
            {
                var reservationRoomDetailsAppliedIncentives = request.ReservationRoomDetails?.Where(x => x.ReservationRoom_UID == room.UID && x.ReservationRoomDetailsAppliedIncentives != null).SelectMany(x => x.ReservationRoomDetailsAppliedIncentives);

                if (reservationRoomDetailsAppliedIncentives.IsNullOrEmpty())
                    return exception;

                var listIncentivesWithBookingWindowRequest = new ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest
                {
                    CheckIn = room.DateFrom.Value.Date,
                    CheckOut = room.DateTo.Value.Date,
                    RateIds = new List<long> { room.Rate_UID.Value },
                    PeriodType = Constants.PeriodType.BookingWindow,
                    RequestId = request.RequestId
                };

                var response = incentiveRepo.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(listIncentivesWithBookingWindowRequest);
                if (response.Status == Contracts.Responses.Status.Fail)
                {
                    LoggingValidateIncentiveBookingWindow(reservationRoomDetailsAppliedIncentives, listIncentivesWithBookingWindowRequest, response: response);
                    return Errors.IncentivesAppliedError.ToBusinessLayerException();
                }

                var incentives = response.Result;
                if (incentives.IsNullOrEmpty())
                {
                    LoggingValidateIncentiveBookingWindow(reservationRoomDetailsAppliedIncentives, listIncentivesWithBookingWindowRequest);
                    return Errors.IncentivesAppliedError.ToBusinessLayerException();
                }

                foreach (var appliedIncentive in reservationRoomDetailsAppliedIncentives)
                {
                    if (!incentives.ContainsKey(appliedIncentive.Incentive_UID))
                    {
                        LoggingValidateIncentiveBookingWindow(reservationRoomDetailsAppliedIncentives, listIncentivesWithBookingWindowRequest, incentives);
                        return Errors.IncentivesAppliedError.ToBusinessLayerException();
                    }
                }
            }

            return exception;
        }

        private void LoggingValidateIncentiveBookingWindow(IEnumerable<contractsReservations.ReservationRoomDetailsAppliedIncentive> incentiveFromRequest,
            ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest request,
            Dictionary<long, IEnumerable<contractsProperties.Incentive>> incentives = null,
            Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse response = null)
        {
            Logger.Warn(new LogMessageBase
            {
                Area = nameof(InitialValidation),
                MethodName = nameof(ValidateIncentiveBookingWindow),
                Code = ((int)Errors.IncentivesAppliedError).ToString(),
                Description = Errors.IncentivesAppliedError.ToString(),
                RequestId = request.RequestId,
            }, new LogEventPropertiesBase
            {
                OtherInfo = new Dictionary<string, object> { { "OtherInfoFromIncentives", new List<object>{
                        new { Description = "Incentives from reservation request", Incentives = incentiveFromRequest },
                        new { Description = $"Incentives from {nameof(IOBIncentiveRepository.ListIncentivesWithBookingAndStayPeriodsForReservationRoom)} service", Incentives = incentives?.Values }
                        } } },
                Request = request,
                Response = response
            });
        }

        private void LoggingValidateIncentiveBookingWindow(InitialValidationRequest request, Errors error)
        {
            Logger.Warn(new LogMessageBase
            {
                Area = nameof(InitialValidation),
                MethodName = nameof(ValidateIncentiveBookingWindow),
                Code = ((int)error).ToString(),
                Description = error.ToString(),
                RequestId = request.RequestId,
            }, new LogEventPropertiesBase { Request = request });
        }
    }
}
