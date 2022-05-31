using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    public class Change
    {
        public Change()
        {

        }

        public Change(string field, string newValue, string oldValue)
            : this()
        {
            Field = field;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
