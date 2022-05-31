using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Test.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Repositories
{
    [TestClass]
    public class RateChannelRepositoryUnitTest : BaseTest
    {
        protected IUnityContainer Container { get; set; }
        private ISessionFactory sessionFactory;
        private IRepositoryFactory repositoryFactory;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            this.Container = new UnityContainer();
            this.Container = this.Container.AddExtension(new DataAccessLayerModule());

            sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            repositoryFactory = Container.Resolve<IRepositoryFactory>();
        }

        [TestMethod]
        [TestCategory("RatesChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSetIsDeletedRateChannels_ZeroAlreadyDeleted")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSetIsDeletedRateChannels_ZeroAlreadyDeleted")]
        [DeploymentItem("./DL")]
        public void TestSetIsDeletedRateChannels_ZeroAlreadyDeleted()
        {
            var builder = new RateChannelBuilder(Container);

            // Prepare DB - All RateChannels has IsDeleted = 0
            builder.SetRatesChannelsIsDeleted(80, 0);
            builder.SetRatesChannelsIsDeleted(81, 0);  

            // Input Data
            builder.InputData.ChannelUIDsToDelete.Add(80);
            builder.InputData.ChannelUIDsToDelete.Add(81);

            // Expected Data
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(80, 14);
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(81, 11);

            this.ActAndAssertIsDeletedRateChannels(builder);
        }

        [TestMethod]
        [TestCategory("RatesChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSetIsDeletedRateChannels_SomeAlreadyDeleted")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSetIsDeletedRateChannels_SomeAlreadyDeleted")]
        [DeploymentItem("./DL")]
        public void TestSetIsDeletedRateChannels_SomeAlreadyDeleted()
        {
            var builder = new RateChannelBuilder(Container);

            // Prepare DB - Some RateChannels are IsDeleted = 1
            builder.SetRatesChannelsIsDeleted(80, 5);  

            // Input Data
            builder.InputData.ChannelUIDsToDelete.Add(80);
            builder.InputData.ChannelUIDsToDelete.Add(81);

            // Expected Data
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(80, 14);
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(81, 11);

            this.ActAndAssertIsDeletedRateChannels(builder);
        }

        [TestMethod]
        [TestCategory("RatesChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSetIsDeletedRateChannels_AllAlreadyDeleted")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSetIsDeletedRateChannels_AllAlreadyDeleted")]
        [DeploymentItem("./DL")]
        public void TestSetIsDeletedRateChannels_AllAlreadyDeleted() 
        {
            var builder = new RateChannelBuilder(Container);

            // Prepare DB - All RateChannels are IsDeleted = 1
            builder.SetRatesChannelsIsDeleted(80);
            builder.SetRatesChannelsIsDeleted(81);

            // Input Data
            builder.InputData.ChannelUIDsToDelete.Add(80);
            builder.InputData.ChannelUIDsToDelete.Add(81);

            // Expected Data
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(80, 14);
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(81, 11);

            this.ActAndAssertIsDeletedRateChannels(builder);
        }
        
        [TestMethod]
        [TestCategory("RatesChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSetIsDeletedRateChannels_RepeatedChannelUIDs")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSetIsDeletedRateChannels_RepeatedChannelUIDs")]
        [DeploymentItem("./DL")]
        public void TestSetIsDeletedRateChannels_RepeatedChannelUIDs()
        {
            var builder = new RateChannelBuilder(Container);

            // Input Data
            builder.InputData.ChannelUIDsToDelete.Add(80);
            builder.InputData.ChannelUIDsToDelete.Add(81);
            builder.InputData.ChannelUIDsToDelete.Add(80);
            
            // Expected Data
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(80, 14);
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(81, 11);

            this.ActAndAssertIsDeletedRateChannels(builder);
        }

        [TestMethod]
        [TestCategory("RatesChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSetIsDeletedRateChannels_InexistentChannelUID")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSetIsDeletedRateChannels_InexistentChannelUID")]
        [DeploymentItem("./DL")]
        public void TestSetIsDeletedRateChannels_InexistentChannelUID()
        {
            var builder = new RateChannelBuilder(Container);

            // Input Data
            builder.InputData.ChannelUIDsToDelete.Add(125634);

            // Expected Data
            builder.ExpectedData.ChannelUID_CountIsDeleted.Add(125634, 0);

            this.ActAndAssertIsDeletedRateChannels(builder);
        }

        private void ActAndAssertIsDeletedRateChannels(RateChannelBuilder builder) 
        {
            // Act
            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var rateChannelRepo = repositoryFactory.GetRateChannelRepository(unitOfWork);
                rateChannelRepo.DeleteRatesChannels(true, false, 70, builder.InputData.ChannelUIDsToDelete);
            }

            // Get RateChannel after update
            var resultRateChannels = builder.GetRateChannels(builder.ExpectedData.ChannelUID_CountIsDeleted.Keys.ToArray());

            // Asserts
            Assert.IsNotNull(resultRateChannels, "Updated RatesChannels cannot be null.");
            Assert.IsFalse(resultRateChannels.Any(x => x.IsDeleted == false), "All updated RateChannels should be IsDeleted.");
            Assert.AreEqual(builder.ExpectedData.ChannelUID_CountIsDeleted.Count(x => x.Value > 0), resultRateChannels.GroupBy(x => x.Channel_UID).Select(x => x.Key).Count());

            foreach (var resultsGroupedByChannel in resultRateChannels.GroupBy(x => x.Channel_UID))
            {
                Assert.IsTrue(builder.ExpectedData.ChannelUID_CountIsDeleted.ContainsKey(resultsGroupedByChannel.Key), string.Format("RatesChannels with ChannelUID = {0} should be IsDeleted", resultsGroupedByChannel.Key));
                Assert.AreEqual(builder.ExpectedData.ChannelUID_CountIsDeleted[resultsGroupedByChannel.Key], resultsGroupedByChannel.Count(x => x.IsDeleted == true));
            }
        }
    }
}
