using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    public class Entity
    {
        public Entity()
        {
            ChangeGroups = new List<ChangeGroup>();
        }

        public bool? IsUpdated { get; set; }

        public string Name { get; set; }

        public List<ChangeGroup> ChangeGroups { get; set; }
    }
}
