using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OB.Services.Jobs.Operations
{
    public class ResponseBase
    {
        public ResponseBase()
        {
            Status = Status.Fail;
            Errors = new List<Error>();
        }

        public Status Status
        {
            get;
            set;
        }

        public List<Error> Errors
        {
            get;
            set;
        }
    }

    public enum Status
    {
        Success = 0,
        PartialSuccess = 1,
        Fail = 2
    }
}