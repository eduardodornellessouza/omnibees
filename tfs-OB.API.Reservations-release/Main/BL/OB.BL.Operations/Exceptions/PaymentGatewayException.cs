using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Operations.Exceptions
{
    public class PaymentGatewayException : BusinessLayerException
    {
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class.
        public PaymentGatewayException()
            : base()
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.ErrorType.PaymentWithoutEnoughFundsError;
            this.Errors = new List<Error>();
        }
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with
        //     a specified error message.
        //
        // Parameters:
        //   message:
        //     A message that describes the error.
        public PaymentGatewayException(string message)
            : base(message)
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.ErrorType.PaymentWithoutEnoughFundsError;
        }

        public PaymentGatewayException(string message, List<Error> errors)
            : base(message)
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.ErrorType.PaymentWithoutEnoughFundsError;
            this.Errors = errors;
        }

        public PaymentGatewayException(string message, string errorType, int errorCode)
            : base(message, errorType, errorCode)
        {

        }

        public PaymentGatewayException(string message, string errorType, int errorCode, List<Error> errors)
            : base(message, errorType, errorCode, errors)
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with
        //     serialized data.
        //
        // Parameters:
        //   info:
        //     The object that holds the serialized object data.
        //
        //   context:
        //     The contextual information about the source or destination.
        protected PaymentGatewayException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.ErrorType.PaymentWithoutEnoughFundsError;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with
        //     a specified error message and a reference to the inner exception that is
        //     the cause of this exception.
        //
        // Parameters:
        //   message:
        //     The error message that explains the reason for the exception.
        //
        //   innerException:
        //     The exception that is the cause of the current exception. If the innerException
        //     parameter is not a null reference, the current exception is raised in a catch
        //     block that handles the inner exception.
        public PaymentGatewayException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.ErrorType.PaymentWithoutEnoughFundsError;
        }

    }
}
