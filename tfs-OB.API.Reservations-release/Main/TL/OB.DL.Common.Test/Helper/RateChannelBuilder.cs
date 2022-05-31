using Microsoft.Practices.Unity;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Helper
{
    class RateChannelBuilder
    {
        public RateChannelInputData InputData { get; set; }
        public RateChannelExpectedData ExpectedData { get; set; }

        private ISessionFactory sessionFactory;
        private IRepositoryFactory repositoryFactory;

        public RateChannelBuilder(IUnityContainer container)
        {
            InputData = new RateChannelInputData();
            ExpectedData = new RateChannelExpectedData();

            sessionFactory = container.Resolve<ISessionFactory>() as SessionFactory;
            repositoryFactory = container.Resolve<IRepositoryFactory>();
        }

        public void SetRatesChannelsIsDeleted(long channelUID, int? isDeletedCount = null)
        {
            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var rateChannelRepo = repositoryFactory.GetRateChannelRepository(unitOfWork);
                var rateChannelsToUpdate = rateChannelRepo.GetQuery(x => x.Channel_UID == channelUID).ToList();

                if (isDeletedCount.HasValue)
                {
                    // Set all IsDeleted = 0
                    rateChannelsToUpdate.ForEach(x => x.IsDeleted = false);

                    // Set as deleted isDeletedCount elements
                    rateChannelsToUpdate.Take((int)isDeletedCount).ToList().ForEach(x => x.IsDeleted = true);
                }

                // Set all IsDeleted = 1
                else    
                    rateChannelsToUpdate.ForEach(x => x.IsDeleted = true);

                unitOfWork.Save();
            }
        }

        public List<OB.Domain.Rates.RatesChannel> GetRateChannels(params long[] channelUIDs) 
        {
            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var distinctChannelUIDs = channelUIDs.Distinct();
                var rateChannelRepo = repositoryFactory.GetRateChannelRepository(unitOfWork);
                
                return rateChannelRepo.GetQuery(x => distinctChannelUIDs.Contains(x.Channel_UID)).ToList();
            }
        }
    }
}
