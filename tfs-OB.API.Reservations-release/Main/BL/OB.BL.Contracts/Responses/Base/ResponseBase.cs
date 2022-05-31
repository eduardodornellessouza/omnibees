using Newtonsoft.Json;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    [KnownType(typeof(UpdateReservationResponse))]
    [KnownType(typeof(InsertReservationResponse))]
    [KnownType(typeof(ListReservationResponse))]
    [KnownType(typeof(ListReservationResponse))]
    //[KnownType(typeof(ListChannelLightResponse))]
    //[KnownType(typeof(ListActivePMSResponse))]
    //[KnownType(typeof(ListLanguageResponse))]
    [KnownType(typeof(PagedResponseBase))]
    public class ResponseBase : ContractBase
    {
        public ResponseBase()
        {
            Errors = new List<Error>();
            Warnings = new List<Warning>();
            Status = Status.Fail;
        }

        public ResponseBase(RequestBase request) : this()
        {
            this.RequestGuid = request.RequestGuid;
            this.RequestId = request.RequestId;
        }

        [DataMember]
        [Obsolete("Use RequestId instead. This property will be removed on OB version 0.9.49")]
        public Guid RequestGuid { get; set; }

        /// <summary>
        /// Gets or sets the List of Errors occurred during the execution of the RESTfull operation.
        /// Usually it's the description of the Exceptions thrown during the execution.
        /// </summary>
        [DataMember]        
        public List<Error> Errors { get; set; }

        /// <summary>
        /// Gets or sets the List of Warnings generated during the execution of the RESTfull operation.
        /// </summary>
        [DataMember]
        public List<Warning> Warnings { get; set; }

        /// <summary>
        /// Gets or sets the Status of the RESTfull operation. If no Exception was thrown, then Status=Success
        /// otherwise it can be PartialSuccess or Fail.
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        [DefaultValue(Status.Success)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Status Status
        {
            get;
            set;
        }

        public void Failed()
        {
            Status = Responses.Status.Fail;
        }

        public void Succeed()
        {
            Status = Responses.Status.Success;
        }

        /// <summary>
        /// Used to make a correlation between messages
        /// </summary>
        [DataMember]
        public string RequestId { get; set; }
    }
}