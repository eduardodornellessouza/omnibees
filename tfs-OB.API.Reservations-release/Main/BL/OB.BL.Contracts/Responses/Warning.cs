using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class to store Errors occurred at the Business layer while invoking a RESTfull operation.
    /// </summary>
    [DataContract]
    public class Warning : ContractBase
    {
        public Warning()
        {
        }

        public Warning(Exception e)
            : this(e.GetType().Name, e.GetHashCode(), e.Message)
        {
        }

        public Warning(string description)
        {
            this.Description = description;
        }

        public Warning(int code, string description)
        {
            this.Description = description;
            this.WarningCode = code;
        }

        public Warning(string errorType, int code, string description)
        {
            this.WarningType = errorType;
            this.WarningCode = code;
            this.Description = description;
        }

        /// <summary>
        /// Gets or Sets the type of warning. If there was an Exception thrown but it is not critical for the success of the operation this Property
        /// stores the name of the Exception class that was thrown.
        /// </summary>
        [DataMember]
        public string WarningType { get; set; }

        /// <summary>
        /// Gets or Sets the code of the warning. Can be the integer of the Exception if there was one.
        /// </summary>
        [DataMember]
        public int WarningCode { get; set; }

        /// <summary>
        /// Gets or Sets the descrption of the warning.
        /// </summary>
        [DataMember]
        public string Description { get; set; }
    }
}