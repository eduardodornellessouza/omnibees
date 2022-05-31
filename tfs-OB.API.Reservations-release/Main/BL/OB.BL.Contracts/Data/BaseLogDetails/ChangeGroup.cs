using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    public class ChangeGroup
    {
        public ChangeGroup()
        {
            Changes = new List<Change>();
        }

        public List<Change> Changes { get; set; }  

    }
}
