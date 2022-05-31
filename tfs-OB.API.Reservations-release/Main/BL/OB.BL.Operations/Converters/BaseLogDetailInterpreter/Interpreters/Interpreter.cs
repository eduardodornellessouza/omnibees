using OB.BL.Contracts.Data.BaseLogDetails;
using OB.Events.Contracts;
using OB.Events.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Converters.BaseLogDetailInterpreter.Interpreters
{
    public abstract class Interpreter : IDisposable
    {
        protected BaseLogDetail _baseLogDetail;
        protected NotificationBase _notification;
        protected bool _executed;

        public Interpreter(NotificationBase notification)
        {
            _baseLogDetail = new BaseLogDetail();
            _notification = notification;
            _executed = false;
        }

        public abstract GridLineDetail GetGridLine(NotificationBase notification);

        public virtual BaseLogDetail Handle(List<EntityDelta> deltas)
        {
            if (_executed)
            {
                return SortLog(_baseLogDetail);
            }

            foreach (var delta in deltas)
            {
                this.HandleGroup(delta);
            }
            
            _executed = true;
            return SortLog(_baseLogDetail);
        }

        private BaseLogDetail SortLog(BaseLogDetail log)
        {
            var order = GetOrder();

            return log;
        }

        public virtual string GetResource(string enumValue)
        {
            // Get Resource
            
            return enumValue;
        }

        protected virtual void HandleGroup(EntityDelta delta)
        {
            var deltaGroup = GetDeltaGroup(delta, ! HasDeltaGroup(delta));

            switch (delta.EntityState)
            {
                case EntityState.Created:
                    HandleCreatedDelta(delta, deltaGroup);
                    break;
                case EntityState.Deleted:
                    HandleDeletedDelta(delta, deltaGroup);
                    break;
                case EntityState.Modified:
                    HandleModifiedDelta(delta, deltaGroup);
                    break;
                default:

                    break;
            }
        }

        protected bool HasDeltaGroup(EntityDelta delta)
        {
            return _baseLogDetail.EntityGroups.Any(q => q.EntityType == delta.EntityType);
        }

        protected EntityGroup GetDeltaGroup(EntityDelta delta, bool isNew = false)
        {
            if (isNew)
            {
                var newEntityGroup = new EntityGroup(delta.EntityType);
                
                newEntityGroup.Name = GetResource(newEntityGroup.EntityType);
                
                _baseLogDetail.EntityGroups.Add(newEntityGroup);
            }

            return _baseLogDetail.EntityGroups.FirstOrDefault(q => q.EntityType == delta.EntityType);
        }

        public abstract Dictionary<string, int> GetOrder();

        private void HandleCreatedDelta(EntityDelta delta, EntityGroup group)
        {
            var name = GetDeltaName(delta);

            if (name != null)
            {
                group.AddedEntityNames.Add(name);
            }

            var newEntity = new Entity();
            newEntity.Name = GetResource(delta.EntityType);
            newEntity.ChangeGroups.Add(GetChangeGroup(delta));

            group.Entities.Add(newEntity);
        }

        private void HandleDeletedDelta(EntityDelta delta, EntityGroup group)
        {
            var name = GetDeltaName(delta);

            if (name != null)
            {
                group.DeletedEntityNames.Add(name);
            }
        }

        private void HandleModifiedDelta(EntityDelta delta, EntityGroup group)
        {
            var newEntity = new Entity();
            newEntity.Name = GetResource(delta.EntityType);
            newEntity.ChangeGroups.Add(GetChangeGroup(delta));

            group.Entities.Add(newEntity);
        }

        private string GetDeltaName(EntityDelta delta)
        {
            try {

                if (delta.EntityProperties.Any(q => q.Key == "Name"))
                {
                    var nameField = delta.EntityProperties.FirstOrDefault(q => q.Key == "Name").Value;

                    return nameField.CurrentValue != null ? nameField.CurrentValue.ToString() : nameField.PreviousValue.ToString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private ChangeGroup GetChangeGroup(EntityDelta delta)
        {
            var changeGroup = new ChangeGroup();
            var listOfChanges = new List<Change>();

            foreach (var property in delta.EntityProperties.Select(q => q.Value))
            {
                var currentValue = property.CurrentValue != null ? property.CurrentValue.ToString() : "";
                var previousValue = property.PreviousValue != null ? property.PreviousValue.ToString() : "";

                if (currentValue != previousValue)
                {
                    listOfChanges.Add(new Change(GetResource(property.PropertyName), currentValue, previousValue != "" ? previousValue : null));
                }
            }

            changeGroup.Changes.AddRange(listOfChanges);
            return changeGroup;
        }


        // List<EntityProperty> QueryEntityProperties(propertyName)

        #region Helpers
        public List<EntityDelta> QueryEntityDeltas(EntityEnum entityDeltaKey)
        {
            return _notification.EntityDeltas.Where(q => q.EntityType == entityDeltaKey.ToString()).ToList();
        }

        public EntityDelta QueryEntityDelta(EntityEnum entityDeltaKey)
        {
            return QueryEntityDeltas(entityDeltaKey).FirstOrDefault();
        }

        public EntityProperty QueryEntityProperty(EntityEnum entityDeltaKey, EntityPropertyEnum entityPropertyName)
        {
            var delta = QueryEntityDelta(entityDeltaKey);

            return QueryEntityProperty(delta, entityPropertyName);
        }

        public EntityProperty QueryEntityProperty(EntityDelta delta, EntityPropertyEnum entityPropertyName)
        {
            EntityProperty entityProperty = null;

            if (delta != null && delta.EntityProperties.TryGetValue(entityPropertyName.ToString().ToCamelCase(), out entityProperty))
            {
                return entityProperty;
            }

            return null;
        }

        public List<EntityProperty> QueryEntityProperties(EntityEnum entityDeltaKey, EntityPropertyEnum entityPropertyName)
        {
            var deltas = QueryEntityDeltas(entityDeltaKey);
            var entityProperties = new List<EntityProperty>();

            foreach (var delta in deltas)
            {
                entityProperties.Add(QueryEntityProperty(delta, entityPropertyName));
            }

            return entityProperties;
        }

        public List<EntityProperty> QueryEntityProperties(EntityPropertyEnum entityPropertyName)
        {
            var deltas = _notification.EntityDeltas;
            var entityProperties = new List<EntityProperty>();

            foreach (var delta in deltas)
            {
                entityProperties.Add(QueryEntityProperty(delta, entityPropertyName));
            }

            return entityProperties;
        }

        #endregion


        public void Dispose()
        {
            _baseLogDetail = null;
        }
    }
}
