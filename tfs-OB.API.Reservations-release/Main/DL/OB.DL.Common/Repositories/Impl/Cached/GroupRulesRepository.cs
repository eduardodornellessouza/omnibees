using Dapper;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Criteria;

namespace OB.DL.Common.Repositories.Impl.Cached
{
    /// <summary>
    /// Rules for diferent channel group types
    /// </summary>
    internal class GroupRulesRepository : CachedRepository<GroupRule>, IGroupRulesRepository
    {
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">Injected</param>
        /// <param name="cacheProvider">Injected</param>
        public GroupRulesRepository(ICacheProvider cacheProvider)
            : base(cacheProvider, TimeSpan.FromMinutes(int.MaxValue))
        {
            SetCacheProvider(() => GetDataToCache());
        }

        #region Cache Methods

        private List<GroupRule> GetDataToCache()
        {
            var rules = new List<GroupRule>();
            
            // Pull
            rules.Add(new GroupRule
                {
                    RuleType = RuleType.Pull,
                    BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions 
                    | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.CalculatePriceModel
                    | BusinessRules.GenerateReservationNumber | BusinessRules.EncryptCreditCard | BusinessRules.ConvertValuesToPropertyCurrency
                    | BusinessRules.PullTpiReservationCalculation | BusinessRules.ReturnSellingPrices | BusinessRules.ApplyNewReservationFilter | BusinessRules.IsToPreCheckAvailability
                    | BusinessRules.IgnoreDepositPolicyConcat | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation 
                    | BusinessRules.ValidateRateChannelPartialPayments
            });

            // Omnibees
            rules.Add(new GroupRule
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.ApplyNewReservationFilter
                | BusinessRules.CalculateExtraBedPrice
            });

            // BE
            rules.Add(new GroupRule
            {
                RuleType = RuleType.BE,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount 
                | BusinessRules.GenerateReservationNumber | BusinessRules.BEReservationCalculation | BusinessRules.PriceCalculationAbsoluteTolerance
                | BusinessRules.CalculateExtraBedPrice | BusinessRules.ValidateReservation | BusinessRules.ValidateRateChannelPartialPayments
            });

            // BE API
            rules.Add(new GroupRule
            {
                RuleType = RuleType.BEAPI,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount
                | BusinessRules.GenerateReservationNumber | BusinessRules.PullTpiReservationCalculation | BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.PriceCalculationAbsoluteTolerance
                | BusinessRules.CalculateExtraBedPrice | BusinessRules.ConvertValuesFromClientToRates | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation 
                | BusinessRules.ValidateRateChannelPartialPayments
            });

            // GDS
            rules.Add(new GroupRule
            {
                RuleType = RuleType.GDS,
                BusinessRules = BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.GDSBuyerGroup
            });

            // PMS
            rules.Add(new GroupRule
            {
                RuleType = RuleType.PMS,
                BusinessRules = BusinessRules.IgnoreAvailability
            });

            // PortalOperadoras
            rules.Add(new GroupRule
            {
                RuleType = RuleType.PortalOperadoras,
                BusinessRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToRateCurrency
            });

            return rules;
        }

        public override GroupRule FirstOrDefault(Expression<Func<GroupRule, bool>> predicate)
        {
            return this.GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Get GroupRules by group type code
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public GroupRule GetGroupRule(GetGroupRuleCriteria criteria)
        {
            var groupRule = this.GetAll().AsQueryable().FirstOrDefault(x => x.RuleType == criteria.RuleType);
            OverrideBusinessRules(groupRule, criteria);
            return groupRule;
        }

        private void OverrideBusinessRules(GroupRule groupRule, GetGroupRuleCriteria criteria)
        {
            if (groupRule == null)
                return;

            // Remove specific rules
            if (criteria.BusinessRulesToRemove.HasValue)
                groupRule.BusinessRules &= ~criteria.BusinessRulesToRemove.Value;

            // Add specific rules
            if (criteria.BusinessRulesToAdd.HasValue)
                groupRule.BusinessRules |= criteria.BusinessRulesToAdd.Value;
        }

        #endregion Cache Methods

    }
}