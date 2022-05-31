using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.DL.Common.Cache;
using OB.DL.Common.Criteria;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Impl.Cached;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Repositories
{
    [TestClass]
    public class GroupRulesRepositoryTest
    {
        private IGroupRulesRepository _groupRulesRepo;
        private Mock<ICacheProvider> _cacheProviderMock;

        [TestInitialize]
        public void Initialize()
        {
            _cacheProviderMock = new Mock<ICacheProvider>();

            Setup_SetItemInCache();

            _groupRulesRepo = new GroupRulesRepository(_cacheProviderMock.Object);
        }

        #region Mocks

        private CacheEntry _cacheInput;
        private void Setup_SetItemInCache()
        {
            _cacheProviderMock
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<CacheEntry>()))
                .Callback((string key, CacheEntry entry) => _cacheInput = entry);
        }

        private void Setup_GetItemFromCache(GroupRule mockedResult)
        {
            _cacheProviderMock
                .Setup(x => x.Get<List<GroupRule>>(It.IsAny<string>()))
                .Returns(new List<GroupRule> { mockedResult });
        }

        private GroupRule BuildGroupRule(RuleType type, BusinessRules rules)
        {
            return new GroupRule
            {
                RuleType = type,
                BusinessRules = rules
            };
        }

        #endregion

        #region Load GroupRules Tests


        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_Push()
        {
            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.Push);
            Assert.IsNull(pushRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_Pull()
        {
            var expectedRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                    | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.CalculatePriceModel
                    | BusinessRules.GenerateReservationNumber | BusinessRules.EncryptCreditCard | BusinessRules.ConvertValuesToPropertyCurrency
                    | BusinessRules.PullTpiReservationCalculation | BusinessRules.ReturnSellingPrices | BusinessRules.ApplyNewReservationFilter | BusinessRules.IsToPreCheckAvailability
                    | BusinessRules.IgnoreDepositPolicyConcat | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.Pull);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_Omnibees()
        {
            var expectedRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.ApplyNewReservationFilter
                | BusinessRules.CalculateExtraBedPrice;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.Omnibees);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_BE()
        {
            var expectedRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount
                | BusinessRules.GenerateReservationNumber | BusinessRules.BEReservationCalculation | BusinessRules.PriceCalculationAbsoluteTolerance
                | BusinessRules.CalculateExtraBedPrice;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.BE);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_BEApi()
        {
            var expectedRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount
                | BusinessRules.GenerateReservationNumber | BusinessRules.PullTpiReservationCalculation | BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.PriceCalculationAbsoluteTolerance
                | BusinessRules.CalculateExtraBedPrice | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.BEAPI);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_GDS()
        {
            var expectedRules = BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.GDSBuyerGroup;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.GDS);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_PMS()
        {
            var expectedRules = BusinessRules.IgnoreAvailability;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.PMS);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Load")]
        public void Test_LoadDefaultBusinessRules_PortalOperadoras()
        {
            var expectedRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToRateCurrency;

            // Act
            var cacheData = _cacheInput.GetDataCallback.Invoke() as List<GroupRule>;

            // Assert
            Assert.IsNotNull(cacheData);
            var pushRules = cacheData.FirstOrDefault(x => x.RuleType == RuleType.PortalOperadoras);
            Assert.IsNotNull(pushRules);
            Assert.AreEqual(expectedRules, pushRules.BusinessRules & expectedRules);
        }

        #endregion

        #region Get GroupRule Tests

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_KeepDefaultRules()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Omnibees,
                BusinessRulesToAdd = null,
                BusinessRulesToRemove = null
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(originalRules, actualResult.BusinessRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_NotFoundGroupRule()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Push,
                BusinessRulesToAdd = null,
                BusinessRulesToRemove = null
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNull(actualResult);
        }

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_AddDuplicatedRules()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Omnibees,
                BusinessRulesToAdd = BusinessRules.ApplyNewReservationFilter,
                BusinessRulesToRemove = null
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(originalRules, actualResult.BusinessRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_RemoveNotExistingRules()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Omnibees,
                BusinessRulesToAdd = null,
                BusinessRulesToRemove = BusinessRules.CalculateExtraBedPrice
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(originalRules, actualResult.BusinessRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_RemoveExistingRules()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Omnibees,
                BusinessRulesToAdd = null,
                BusinessRulesToRemove = BusinessRules.ApplyNewReservationFilter
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(BusinessRules.ConvertValuesToPropertyCurrency, actualResult.BusinessRules);
        }

        [TestMethod]
        [TestCategory("GroupRule.Get")]
        public void Test_GetGroupRule_AddAndRemoveRules()
        {
            // Assemble
            var originalRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToPropertyCurrency;
            var groupRule = BuildGroupRule(RuleType.Omnibees, originalRules);
            Setup_GetItemFromCache(groupRule);
            var criteria = new GetGroupRuleCriteria
            {
                RuleType = RuleType.Omnibees,
                BusinessRulesToAdd = BusinessRules.ValidateGuarantee,
                BusinessRulesToRemove = BusinessRules.ApplyNewReservationFilter
            };

            // Act
            var actualResult = _groupRulesRepo.GetGroupRule(criteria);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(BusinessRules.ValidateGuarantee | BusinessRules.ConvertValuesToPropertyCurrency, actualResult.BusinessRules);
        }

        #endregion
    }
}
