using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    public class Identifier
    {
        public Identifier() { }

        public Identifier(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }

    }
}
