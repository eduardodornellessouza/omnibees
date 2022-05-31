using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Operations.Exceptions
{
    public class BusinessLayerException : ApplicationException
    {
        public string ErrorType { get; set; }

        public int ErrorCode { get; set; }

        public List<Error> Errors { get; set; }

        // Summary:
        //     Initializes a new instance of the System.ApplicationException class.
        public BusinessLayerException() : base()
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with
        //     a specified error message.
        //
        // Parameters:
        //   message:
        //     A message that describes the error.
        public BusinessLayerException(string message) : base(message)
        {

        }

        public BusinessLayerException(string message, string errorType) : base(message)
        {
            this.ErrorType = errorType;
        }

        public BusinessLayerException(string message, string errorType, int errorCode) : base(message)
        {
            this.ErrorType = errorType;
            this.ErrorCode = errorCode;
        }

        public BusinessLayerException(string message, string errorType, int errorCode, List<Error> errors) : base(message)
        {
            this.ErrorType = errorType;
            this.ErrorCode = errorCode;
            this.Errors = errors;
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
        protected BusinessLayerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

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
        public BusinessLayerException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public BusinessLayerException(string message, string errorType, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorType = errorType;
        }

        public BusinessLayerException(string message, string errorType, int errorCode, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorType = errorType;
            this.ErrorCode = errorCode;
        }
    }
}
