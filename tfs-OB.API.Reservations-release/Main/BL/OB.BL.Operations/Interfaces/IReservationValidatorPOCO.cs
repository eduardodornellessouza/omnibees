using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ValidationRequests;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using OB.BL.Operations.Exceptions;

namespace OB.BL.Operations.Interfaces
{
    public interface IReservationValidatorPOCO : IBusinessPOCOBase
    {
        #region RESTful Operations

        /// <summary>
        /// Implementation of the InitialValidation operation.
        /// This method will contain all the initial validations from Reservation into the Datastore given it's associated objects.     
        /// Suggestion: whenever you want to add a new validation you must create a new class with a single scope for the validation purpose,
        /// the method of this new class must return a BusinessLayerException and should be called within the main InitialValidation method using the ValidateToThrowException extension.
        /// As an example you can see the class ValidateIncentiveBookingWindow
        /// </summary>
        /// <param name="request">Need the group rules and the object reservation to validate a reservation.</param>

        void InitialValidation(InitialValidationRequest request);

        /// <summary>
        /// Implementation of the ValidateReservation operation.
        /// Validate a Reservation into the Datastore given it's associated objects.
        /// </summary>
        /// <param name="request">Need the flags and the object reservation to validate a reservation.</param>
        /// <returns>A ObservableCollection that contains the errors of validation.</returns>
        ObservableCollection<Error> ValidateReservation(contractsReservations.Reservation reservation, ValidateReservationRequest request);

        /// <summary>
        /// Validate a reservation modification
        /// </summary>
        /// <param name="validateAllotment">check if allotment has to be validated</param>
        /// <param name="reservation">the reservation to validate</param>
        /// <param name="roomsToUpdate">Room changes</param>
        /// <returns></returns>
        void ValidateModifyReservation(OB.Domain.Reservations.Reservation newReservation, contractsReservations.Reservation oldReservation, ValidateReservationRequest request,
            List<contractsReservations.UpdateRoom> roomsToUpdate);

        void ValidateRestrictions(contractsReservations.Reservation reservation, ValidateReservationRequest request);

        /// <summary>
        /// Validade Guest Loyalty Discount
        /// </summary>
        /// <param name="reservationContext"></param>
        /// <param name="guest"></param>
        /// <param name="reservation"></param>
        /// <param name="rooms"></param>
        bool ValidateGuestLoyaltyLevel(LoyaltyLevelValidationCriteria criteria);

        /// <summary>
        /// Validade Guest Loyalty Discount
        /// </summary>
        /// <param name="guestId"></param>
        /// <param name="reservationRooms"></param>
        /// <returns></returns>
        bool HaveGuestExceededLoyaltyDiscount(long guestId);

        AggregatedGuestPastReservationsValues CalculateGuestPastReservationsValues(long guestId, int periodicityLimitType, int periodicityLimitValue, long loyaltyBaseCurrencyUID);


        bool ValidateReservationRestrictions(OB.BL.Contracts.Data.Rates.RateRoomDetail rateRoomDetail, DateTime checkIn, DateTime checkOut, long channelId);

        #endregion

        #region Initial Validation

        BusinessLayerException ValidateIncentiveBookingWindow(InitialValidationRequest request);
        
        BusinessLayerException ValidateRateChannelPaymentParcels(InitialValidationRequest request);

        #endregion
    }
}
