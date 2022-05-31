using OB.Domain.Reservations;

namespace OB.DL.Common.Criteria
{
    /// <summary>
    /// The input parameters of methods to get GroupRule.
    /// </summary>
    public class GetGroupRuleCriteria
    {
        public GetGroupRuleCriteria()
        {

        }

        public GetGroupRuleCriteria(RuleType ruleType)
        {
            RuleType = ruleType;
        }

        /// <summary>
        /// [Mandatory] The Rule Type.
        /// </summary>
        public RuleType RuleType { get; set; }
        
        /// <summary>
        /// Add Business Rules to existing rules.
        /// </summary>
        public BusinessRules? BusinessRulesToAdd { get; set; }
        
        /// <summary>
        /// Remove Business Rules from existing rules.
        /// </summary>
        public BusinessRules? BusinessRulesToRemove { get; set; }
    }
}
