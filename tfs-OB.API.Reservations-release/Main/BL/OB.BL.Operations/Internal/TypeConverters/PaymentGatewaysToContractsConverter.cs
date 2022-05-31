using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using OB.BL.Operations.Extensions;
using OB.Reservation.BL.Contracts.Responses;
using reservationRequest = OB.Reservation.BL.Contracts.Requests.InsertReservationRequest;
using cancelReservationRequest = OB.Reservation.BL.Contracts.Requests.CancelReservationRequest;
using PaymentGatewaysLibrary.BrasPagGateway.Classes;
using payU = PaymentGatewaysLibrary.PayU;
using Newtonsoft.Json;
using ObLog = OB.Log;

namespace OB.BL.Operations.Internal.TypeConverters
{
    internal static class PaymentGatewaysToContractsConverter
    {
        public static List<Error> MapPaymentGatewayErrorsToReservationErrors(string paymentGatewayCode, List<PaymentGatewaysLibrary.Common.Error> gatewayErrors)
        {
            var result = new List<Error>();

            if (Enum.TryParse(paymentGatewayCode, out OB.Reservation.BL.Constants.PaymentGateway code) && gatewayErrors?.Any() == true)
                switch (code)
                {
                    case OB.Reservation.BL.Constants.PaymentGateway.BrasPag:
                        result = MapBraspagGatewayErrorsToReservationErrors(gatewayErrors);
                        break;
                    case OB.Reservation.BL.Constants.PaymentGateway.PayU:
                        result = MapPayUGatewayErrorsToReservationErrors(gatewayErrors);
                        break;
                }

            return result;
        }

        private static List<Error> MapPayUGatewayErrorsToReservationErrors(List<PaymentGatewaysLibrary.Common.Error> gatewayErrors)
        {
            var result = new List<Error>();

            foreach (var gatewayError in gatewayErrors)
            {
                var error = gatewayError.Data?.Any() == true ? gatewayError.Data.First().Value : string.Empty;

                if (gatewayError.ErrorCode == (int)Errors.RequiredParameter)
                {
                    if (string.Equals(error, "PublicKey"))
                        result.Add(Errors.RequiredParameter.ToContractError("PublicKey"));

                    else if (string.Equals(error, "ApiId"))
                        result.Add(Errors.RequiredParameter.ToContractError("ApiId"));

                    else if (string.Equals(error, "PrivateKey"))
                        result.Add(Errors.RequiredParameter.ToContractError("PrivateKey"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{ nameof(payU.CreateTokenRequest.TokenType)}"))
                        result.Add(Errors.RequiredParameter.ToContractError(nameof(payU.CreateTokenRequest.TokenType)));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{ nameof(payU.CreateTokenRequest.HolderName)}") || string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.HolderName)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardName)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{ nameof(payU.CreateTokenRequest.CardNumber)}") || string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.CardNumber)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{ nameof(payU.CreateTokenRequest.ExpirationDate)}") || string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.ExpirationDate)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{nameof(payU.IdentityDocument)}.{nameof(payU.IdentityDocument.Number)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.GuestIDCardNumber)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.CreditCardCvv)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CVV)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreateCustomerRequest)}.{nameof(payU.CreateCustomerRequest.CustomerReference)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(payU.CreateCustomerRequest.CustomerReference)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreatePaymentRequest)}.{nameof(payU.CreatePaymentRequest.CreateCustomer)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"Payment.{nameof(payU.CreatePaymentRequest.CreateCustomer)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreatePaymentRequest)}.{nameof(payU.CreatePaymentRequest.Amount)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.TotalAmount)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreatePaymentRequest)}.{nameof(payU.CreatePaymentRequest.Currency)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.ReservationBaseCurrency_UID)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreatePaymentRequest)}.{nameof(payU.CreatePaymentRequest.StatementSoftDescriptor)}"))
                        result.Add(Errors.RequiredParameter.ToContractError("Statement Soft Descriptor"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.Type)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"Untokenized or Tokenized"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"TokenizedRequest"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.SourceType)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(payU.Enum.PaymentMethodSourceTypeEnum.PaymentMethodSourceType)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.ProviderSpecificData)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails.Cookie)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationsAdditionalData)}.{nameof(reservationRequest.ReservationsAdditionalData.Cookie)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.ProviderSpecificData)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails.CustomerNationalIdentifyNumber)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.GuestIDCardNumber)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.ProviderSpecificData)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails)}.{nameof(payU.ProviderSpecificDataRequest.AdditionalDetails.PayerEmail)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.GuestEmail)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.ProviderSpecificData)}.{nameof(payU.ProviderSpecificDataRequest.DeviceFingerPrint)}.{nameof(payU.ProviderSpecificDataRequest.DeviceFingerPrint.Fingerprint)}"))
                        result.Add(Errors.RequiredParameter.ToContractError(nameof(reservationRequest.AntifraudDeviceFingerPrintId)));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.ProviderSpecificData)}.{nameof(payU.ProviderSpecificDataRequest.DeviceFingerPrint)}.{nameof(payU.ProviderSpecificDataRequest.DeviceFingerPrint.Provider)}"))
                        result.Add(Errors.RequiredParameter.ToContractError(nameof(payU.ProviderSpecificDataRequest.DeviceFingerPrint.Provider)));

                    else if (string.Equals(error, $"{nameof(payU.RefundRequest.PaymentId)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(cancelReservationRequest.ReservationUID)} or {nameof(cancelReservationRequest.ReservationNo)}"));

                    else if (string.Equals(error, $"{nameof(payU.RefundRequest.RefundReason)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(cancelReservationRequest.CancelReservationReason_UID)}"));

                }
                else if (gatewayError.ErrorCode == (int)Errors.InvalidParameter)
                {
                    if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{nameof(payU.CreateTokenRequest.CardNumber)}") || string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.CardNumber)}"))
                        result.Add(Errors.InvalidParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreateTokenRequest)}.{nameof(payU.CreateTokenRequest.ExpirationDate)}") || string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.ExpirationDate)}"))
                        result.Add(Errors.InvalidParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"));

                    else if (string.Equals(error, $"{nameof(payU.CreatePaymentRequest)}.{nameof(payU.CreatePaymentRequest.Currency)}"))
                        result.Add(Errors.InvalidParameter.ToContractError($"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.ReservationBaseCurrency_UID)}"));

                    else if (string.Equals(error, $"{nameof(payU.ChargeRequest.PaymentMethod)}.{nameof(payU.ChargeRequest.PaymentMethod.CreditCardCvv)}"))
                        result.Add(Errors.InvalidParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CVV)}"));
                }
                else
                {
                    try
                    {
                        var errorSerialize = JsonConvert.SerializeObject(gatewayError);
                        result.Add(JsonConvert.DeserializeObject<Error>(errorSerialize));
                    }
                    catch (Exception ex)
                    {
                        var logger = ObLog.LogsManager.CreateLogger(nameof(MapPayUGatewayErrorsToReservationErrors));
                        logger.Error(ex, "Error to Serialize/Deserialize object from paymentgateway to reservation.api");
                    }
                }
            }

            return result;
        }


        private static List<Error> MapBraspagGatewayErrorsToReservationErrors(List<PaymentGatewaysLibrary.Common.Error> gatewayErrors)
        {
            var result = new List<Error>();

            foreach (var gatewayError in gatewayErrors)
            {
                //We can do this because error codes are equal between projects.
                if (gatewayError.ErrorCode == (int)Errors.InvalidGuestCityFormat)
                    result.Add(Errors.InvalidGuestCityFormat.ToContractError());

                if (gatewayError.ErrorCode == (int)Errors.InvalidGuestPhoneFormat)
                    result.Add(Errors.InvalidGuestPhoneFormat.ToContractError());

                //Cant do a switch case because parameters are not static.
                if (gatewayError.ErrorCode == (int)Errors.RequiredParameter)
                {
                    if (string.Equals(gatewayError.Data.First().Value, nameof(AuthorizeRequest.OrderId)))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.Number)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Name)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestFirstName)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Email)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestEmail)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressStreet)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestAddress1)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressZipCode)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestPostalCode)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCity)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCity)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressState)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestState_UID)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCountry)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCountry_UID)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Provider)}"))
                        result.Add(Errors.RequiredParameter.ToContractError("Provider"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardNumber)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardYear)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardMonth)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardHolderName)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardName)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardSecurityCode)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CVV)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardRequestEnum)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"));

                    else if (string.Equals(gatewayError.Data.First().Value, nameof(AuthorizeRequest.DeviceFingerPrintId)))
                        result.Add(Errors.RequiredParameter.ToContractError(nameof(reservationRequest.AntifraudDeviceFingerPrintId)));

                    else if (string.Equals(gatewayError.Data.First().Value, $"{nameof(AuthorizeRequest.AntiFraudRequest) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser.IpAddress)}"))
                        result.Add(Errors.RequiredParameter.ToContractError($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.IPAddress)}"));
                }
            }

            return result;
        }
    }
}