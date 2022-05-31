using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Internal.TypeConverters;
using domains = OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using static OB.Reservation.BL.Constants;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class RequestToCriteriaConverterTest
    {
        [TestInitialize]
        public void Initialize()
        {

        }

        #region GetGroupRuleCriteria Tests

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_Null()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = null
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);
            Assert.IsNull(actualResult.BusinessRulesToAdd);
            Assert.IsNull(actualResult.BusinessRulesToRemove);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_NullRemovedRules_NullAddedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider 
                {
                    AddedRules = null,
                    RemovedRules = null
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);
            Assert.IsNull(actualResult.BusinessRulesToAdd);
            Assert.IsNull(actualResult.BusinessRulesToRemove);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_NotNullAddedRules_NullRemovedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider
                {
                    AddedRules = BusinessRuleFlags.ConvertValuesToPropertyCurrency | BusinessRuleFlags.EncryptCreditCard,
                    RemovedRules = null
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);
            Assert.AreEqual(domains.BusinessRules.ConvertValuesToPropertyCurrency | domains.BusinessRules.EncryptCreditCard, actualResult.BusinessRulesToAdd);
            Assert.IsNull(actualResult.BusinessRulesToRemove);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_NullAddedRules_NotNullRemovedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider
                {
                    AddedRules = null,
                    RemovedRules = BusinessRuleFlags.ConvertValuesToPropertyCurrency | BusinessRuleFlags.EncryptCreditCard
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);
            Assert.AreEqual(domains.BusinessRules.ConvertValuesToPropertyCurrency | domains.BusinessRules.EncryptCreditCard, actualResult.BusinessRulesToRemove);
            Assert.IsNull(actualResult.BusinessRulesToAdd);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_NotNullAddedRules_NotNullRemovedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider
                {
                    AddedRules = BusinessRuleFlags.ApplyNewReservationFilter | BusinessRuleFlags.BEReservationCalculation,
                    RemovedRules = BusinessRuleFlags.ConvertValuesToPropertyCurrency | BusinessRuleFlags.EncryptCreditCard
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(domains.RuleType.Omnibees, actualResult.RuleType);
            Assert.AreEqual(domains.BusinessRules.ApplyNewReservationFilter | domains.BusinessRules.BEReservationCalculation, actualResult.BusinessRulesToAdd);
            Assert.AreEqual(domains.BusinessRules.ConvertValuesToPropertyCurrency | domains.BusinessRules.EncryptCreditCard, actualResult.BusinessRulesToRemove);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_AllAddedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider
                {
                    AddedRules = BusinessRuleFlags.All,
                    RemovedRules = null
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);

            domains.BusinessRules expectedRules = 0;
            foreach (domains.BusinessRules rule in Enum.GetValues(typeof(domains.BusinessRules)))
                expectedRules |= rule;
            Assert.AreEqual(expectedRules, actualResult.BusinessRulesToAdd);
        }

        [TestMethod]
        [TestCategory("GroupRule.Convert")]
        public void Test_ConvertToGroupRuleCriteria_BusinessRules_AllRemovedRules()
        {
            // Assemble
            var request = new RequestBase
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = new BusinessRulesOverrider
                {
                    AddedRules = null,
                    RemovedRules = BusinessRuleFlags.All
                }
            };

            // Act
            var actualResult = RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request);

            // Assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(OB.Domain.Reservations.RuleType.Omnibees, actualResult.RuleType);

            domains.BusinessRules expectedRules = 0;
            foreach (domains.BusinessRules rule in Enum.GetValues(typeof(domains.BusinessRules)))
                expectedRules |= rule;
            Assert.AreEqual(expectedRules, actualResult.BusinessRulesToRemove);
        }

        #endregion
    }
}
