using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    public class EntityGroup
    {
        public EntityGroup() { }

        public EntityGroup(string entityType)
        {
            EntityType = entityType;
            AddedEntityNames = new List<string>();
            DeletedEntityNames = new List<string>();
            UpdatedEntityNames = new List<string>();
            Entities = new List<Entity>();
            ChangeGroups = new List<ChangeGroup>();
            EntitiesGroups = new List<EntityGroup>();
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> AddedEntityNames { get; set; }
        public List<string> DeletedEntityNames { get; set; }
        public List<string> UpdatedEntityNames { get; set; }

        public string Name { get; set; }
        public string EntityType { get; set; }

        ///// <summary>
        ///// true - Updated entity group.
        ///// false - New entity group.
        ///// null - Not specified.
        ///// </summary>
        public bool? IsUpdated { get; set; }

        public List<Entity> Entities { get; set; }
        
        public List<ChangeGroup> ChangeGroups { get; set; }

        public List<EntityGroup> EntitiesGroups { get; set; }

    }
}
