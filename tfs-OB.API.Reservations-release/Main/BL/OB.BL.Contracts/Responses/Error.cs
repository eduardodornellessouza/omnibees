using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class to store Errors occurred at the Business layer while invoking a RESTfull operation.
    /// </summary>
    [DataContract]
    public class Error : ContractBase
    {
        public Error()
        {
            Data = new Dictionary<string, string>();
        }

        public Error(string errorType, int code, string description, Dictionary<string, string> Data) : this(errorType, code, description)
        {
            this.Data = Data;
        }

        public Error(Exception e)
            : this(e.GetType().Name, e.GetHashCode(), e.Message)
        {
            Data = new Dictionary<string, string>();

            if (e != null)
            {
                if (e.Data.Count > 0)
                {
                    foreach (var dataItem in e.Data.Keys)
                    {
                        string keyString = string.Empty;
                        if (!(dataItem is ValueType) && !(dataItem is string))
                            keyString = JsonConvert.SerializeObject(dataItem);
                        else keyString = dataItem.ToString();
                                                
                        string valueString = string.Empty;
                        object obj = e.Data[dataItem];
                        if (obj != null)
                        {
                            if (!(obj is ValueType) && !(obj is string))
                                valueString = JsonConvert.SerializeObject(e.Data[dataItem]);
                            else valueString = e.Data[dataItem].ToString();
                        }

                        Data.Add(keyString, valueString);
                    }
                }
            }
        }

        public Error(string errorType, int code, string description)
        {
            this.ErrorType = errorType;
            this.ErrorCode = code;
            this.Description = description;
            Data = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or Sets the type of the Error. Usually this is the short name of the Exception class that was the cause of the failure.
        /// For example (ValidationError, ApplicationError, CommunicationError, etc).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ErrorType { get; set; }

        /// <summary>
        /// Gets or Sets the sub type of the Error. This should identify the type of business error. For example, the cause of a validation error
        /// for a insertreservation operation like the NotEnoughAllotment, etc.
        /// for example
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ErrorSubType { get; set; }

        /// <summary>
        /// Gets or Sets a integer code. Usually this is the HResult integer code associated with the Exception thrown in the failure of the operation.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or Sets the Description of the Error. Usually this is the Message field of the Exception thrown in the failure of the operation.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the data of the error (key/value).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string,string> Data { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MethodName { get; set; }
    }
}