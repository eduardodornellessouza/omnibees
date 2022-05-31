using Microsoft.Practices.Unity;
using OB.DL.Common.Interfaces;
using OB.Domain.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Helper
{
    public class OccupancyLevelsBuilder : BaseBuilder, IDisposable
    {
        private Property _property;

        public OccupancyLevelsBuilder(long propertyUid, IUnityContainer container) : base(container)
        {
            var propertyRepo = RepositoryFactory.GetPropertyRepository(UnitOfWork);
            _property = propertyRepo.FirstOrDefault(q => q.UID == propertyUid);

            if(_property == null) throw new ArgumentNullException("Property Cannot be Found");
        }

        public OccupancyLevelPeriod CreatePeriod(string Name)
        {
            // Create New Period Object
            var occRepo = RepositoryFactory.GetRepository<OccupancyLevelPeriod>(UnitOfWork);
            var period = new OccupancyLevelPeriod
            {
                IsDeleted = false,
                Name = Name,
                Property_UID = _property.UID,
                Version = 1,
                CreatedDate = DateTime.Now
            };

            // Add Occupancy Level Period to Database
            occRepo.Add(period);
            UnitOfWork.Save();

            // Return Period
            return period;
        }

        public OccupancyLevelPeriodDate AddPeriodDates(OccupancyLevelPeriod period, DateTime startDate, DateTime endDate)
        {
            // Add Occupancy Level Period Date Interval
            var periodDate = new OccupancyLevelPeriodDate { 
                IsDeleted = false,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                Version = 1,
                CreatedDate = DateTime.Now
            };

            period.OccupancyLevelPeriodDates.Add(periodDate);

            // Save Changes
            UnitOfWork.Save();

            // Return Period Date
            return periodDate;
        }

        public OccupancyLevel CreateOccupancyLevel(OccupancyLevelPeriod period, int min, int max, long? roomTypeUid = null)
        {
            // Create new Occupancy Level Object
            var occupancylevel = new OccupancyLevel { 
                IsDeleted = false,
                //IsExpanded = false,
                LimitMin = min,
                LimitMax = max,
                Version = 1,
                CreatedDate = DateTime.Now,
                RoomType_UID = roomTypeUid
            };

            // Add to the Period
            period.OccupancyLevels.Add(occupancylevel);
            occupancylevel.OccupancyLevelPeriod = period;

            // Save Changes
            UnitOfWork.Save();

            // Return Occupancy Level Object
            return occupancylevel;
        }

        public OccupancyLevelRule AddOccupancyLevelRule(OccupancyLevel occupancyLevel, bool isCloseSales, decimal? value, bool isPercentage, bool valueIncrease, long rateRoomUid, long rateUid, long roomUid)
        {
            var occupancyLevelRule = new OccupancyLevelRule {
                CloseSalesRule_IsCloseSales = isCloseSales,
                PriceRule_IsPercentage = isPercentage,
                PriceRule_IsValueDecrease = !valueIncrease,
                PriceRule_Value = value,
                RateRoom_UID = rateRoomUid,
                //RateUID = rateUid,
                //RoomUID = roomUid,
                Version = 1,
                CreatedDate = DateTime.Now
            };

            occupancyLevel.OccupancyLevelRules.Add(occupancyLevelRule);
            occupancyLevelRule.OccupancyLevel = occupancyLevel;

            UnitOfWork.Save();

            return occupancyLevelRule;
        }

        /// <summary>
        /// Has to Reset the Property Configuration
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsBuilder ResetConfiguration()
        {
            _property.IsOccupancyLevelsByHotelOccupancy = null;
            //var occRepo = RepositoryFactory.GetRepository<OccupancyLevelPeriod>(UnitOfWork);

            //foreach(var occupancylevel in occRepo.GetQuery(q => q.Property_UID == _property.UID))
            //{

            //}

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsHotelOccupancyBuilder SetupHotelOccupancyModel()
        {
            _property.IsOccupancyLevelsByHotelOccupancy = true;
            return new OccupancyLevelsHotelOccupancyBuilder(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsRoomOccupancyBuilder SetupRoomOccupancyModel()
        {
            _property.IsOccupancyLevelsByHotelOccupancy = false;
            return new OccupancyLevelsRoomOccupancyBuilder(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsBuilder SaveChanges()
        {
            UnitOfWork.Save();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Property GetProperty()
        {
            return _property;
        }

    

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    var uow = GetUnitOfWork();
                    uow.Dispose();
                }
                disposed = true;
            }
        }
    }

    [Serializable]
    public class OccupancyLevelsHotelOccupancyBuilder : BaseBuilder
    {
        private OccupancyLevelsBuilder _builder;

        public OccupancyLevelsHotelOccupancyBuilder(OccupancyLevelsBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PeriodName"></param>
        /// <param name="PeriodDates"></param>
        /// <returns></returns>
        public OccupancyLevelsHotelOccupancyBuilder CreatePeriod(string PeriodName, List<PeriodDate> PeriodDates)
        {
            var property = _builder.GetProperty();

            #region Create Period
            var newPeriod = new OccupancyLevelPeriod();
            newPeriod.Name = PeriodName;
            newPeriod.Property_UID = property.UID;
            newPeriod.Version = 1;
            newPeriod.CreatedDate = DateTime.UtcNow;
            newPeriod.IsDeleted = false;

            var occRepo = _builder.RepositoryFactory.GetRepository<OccupancyLevelPeriod>(_builder.GetUnitOfWork());
            occRepo.Add(newPeriod);

            #endregion

            #region Create Period Dates

            foreach (var periodDate in PeriodDates)
            {
                var newPeriodDate = new OccupancyLevelPeriodDate();
                newPeriodDate.StartDate = periodDate.From.Date;
                newPeriodDate.EndDate = periodDate.To.Date;
                newPeriodDate.Version = 1;
                newPeriodDate.CreatedDate = DateTime.UtcNow;
                newPeriodDate.CreatedBy = 70;
                newPeriodDate.IsDeleted = false;
                newPeriod.OccupancyLevelPeriodDates.Add(newPeriodDate);
            }

            #endregion

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="rateUid"></param>
        /// <param name="roomUid"></param>
        /// <param name="closeSales"></param>
        /// <param name="value"></param>
        /// <param name="isPercentage"></param>
        /// <param name="isUp"></param>
        /// <returns></returns>
        public OccupancyLevelsHotelOccupancyBuilder AddOccupancyLevel(int min, int max, long rateUid, long roomUid, bool closeSales, decimal value, bool isPercentage, bool isUp)
        {

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsBuilder SaveChanges()
        {
            return _builder.SaveChanges();
        }
    }

    [Serializable]
    public class OccupancyLevelsRoomOccupancyBuilder : BaseBuilder
    {
        private OccupancyLevelsBuilder _builder;

        public OccupancyLevelsRoomOccupancyBuilder(OccupancyLevelsBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OccupancyLevelsBuilder SaveChanges()
        {
            return _builder.SaveChanges();
        }
    }

    [Serializable]
    public class PeriodDate
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
