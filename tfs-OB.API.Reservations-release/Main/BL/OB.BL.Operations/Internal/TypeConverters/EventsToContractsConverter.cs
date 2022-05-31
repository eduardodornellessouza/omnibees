using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using contractsLog = OB.Reservation.BL.Contracts.Data.BaseLogDetails;
using eventsLog = OB.Events.Contracts.Data.BaseLogDetails;

namespace OB.BL.Operations.Internal.TypeConverters
{
    public class EventsToContractsConverter
    {
        public static contractsLog.BaseLogDetail Convert(eventsLog.BaseLogDetail obj)
        {
            var newObj = new contractsLog.BaseLogDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(eventsLog.BaseLogDetail obj, contractsLog.BaseLogDetail objDestination)
        {
            objDestination.Action =             obj.Action;
            objDestination.Summary =            obj.Summary;
            objDestination.CreatedAt =          obj.CreatedAt;
            objDestination.CreatedBy =          obj.CreatedBy;
            objDestination.EntityGroups = obj.EntityGroups != null ? obj.EntityGroups.Select(x => Convert(x)).ToList() : new List<contractsLog.EntityGroup>();
            objDestination.Identifiers = obj.Identifiers != null ? obj.Identifiers.Select(x => Convert(x)).ToList() : new List<contractsLog.Identifier>();
        }

        public static OB.BL.Contracts.Data.Rates.UpdatePeriod Convert(OB.Events.Contracts.Data.UpdateRates.UpdatePeriod obj)
        {
            var newObj = new OB.BL.Contracts.Data.Rates.UpdatePeriod();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.Events.Contracts.Data.UpdateRates.UpdatePeriod obj, OB.BL.Contracts.Data.Rates.UpdatePeriod objDestination)
        {
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
        }

        public static contractsLog.Identifier Convert(eventsLog.Identifier obj)
        {
            var newObj = new contractsLog.Identifier(obj.Key, obj.Value);
            return newObj;
        }

        public static contractsLog.EntityGroup Convert(eventsLog.EntityGroup obj)
        {
            var newObj = new contractsLog.EntityGroup(obj.EntityType);
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(eventsLog.EntityGroup obj, contractsLog.EntityGroup objDestination)
        {
            objDestination.AddedEntityNames         = obj.AddedEntityNames;
            objDestination.DeletedEntityNames       = obj.DeletedEntityNames;
            objDestination.UpdatedEntityNames       = obj.UpdatedEntityNames;
            objDestination.IsUpdated                = obj.IsUpdated;
            objDestination.Name                     = obj.Name;
            objDestination.EntityType               = obj.EntityType;
            objDestination.Entities = obj.Entities != null ? obj.Entities.Select(x => Convert(x)).ToList() : new List<contractsLog.Entity>();
            objDestination.ChangeGroups = obj.ChangeGroups != null ? obj.ChangeGroups.Select(x => Convert(x)).ToList() : new List<contractsLog.ChangeGroup>();
            objDestination.EntitiesGroups = obj.EntitiesGroups != null ? obj.EntitiesGroups.Select(x => Convert(x)).ToList() : new List<contractsLog.EntityGroup>();
        }

        public static contractsLog.ChangeGroup Convert(eventsLog.ChangeGroup obj)
        {
            var newObj = new contractsLog.ChangeGroup();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(eventsLog.ChangeGroup obj, contractsLog.ChangeGroup objDestination)
        {
            objDestination.Changes = obj.Changes != null ? obj.Changes.Select(x => Convert(x)).ToList() : new List<contractsLog.Change>();
        }

        public static contractsLog.Change Convert(eventsLog.Change obj)
        {
            var newObj = new contractsLog.Change(obj.Field, obj.NewValue, obj.OldValue);
            return newObj;
        }

        public static contractsLog.Entity Convert(eventsLog.Entity obj)
        {
            var newObj = new contractsLog.Entity();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(eventsLog.Entity obj, contractsLog.Entity objDestination)
        {
            objDestination.ChangeGroups = obj.ChangeGroups != null ? obj.ChangeGroups.Select(x => Convert(x)).ToList() : new List<contractsLog.ChangeGroup>();
            objDestination.IsUpdated = obj.IsUpdated;
            objDestination.Name = obj.Name;
        }
    }
}
