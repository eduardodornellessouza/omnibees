using System;
using System.Runtime.Serialization;

namespace OB.BL.Operations.Exceptions
{
    public class InvalidAllotmentException : BusinessLayerException
    {
        void SetCodeAndType()
        {
            this.ErrorType = OB.Reservation.BL.Contracts.Responses.Errors.AllotmentNotAvailable.ToString();
            this.ErrorCode = (int)OB.Reservation.BL.Contracts.Responses.Errors.AllotmentNotAvailable;
        }

        // Summary:
        //     Initializes a new instance of the System.ApplicationException class.
        public InvalidAllotmentException() : base()
        {
            SetCodeAndType();
        }
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with
        //     a specified error message.
        //
        // Parameters:
        //   message:
        //     A message that describes the error.
        public InvalidAllotmentException(string message) : base(message)
        {
            SetCodeAndType();
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
        protected InvalidAllotmentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SetCodeAndType();
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
        public InvalidAllotmentException(string message, Exception innerException)
            : base(message, innerException)
        {
            SetCodeAndType();
        }

    }
}
