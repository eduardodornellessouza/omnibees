using System.Runtime.Serialization;
using static OB.Reservation.BL.Constants;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// This class can override the internal BusinessRules for a specific RuleType.
    /// </summary>
    [DataContract]
    public class BusinessRulesOverrider
    {
        /// <summary>
        /// Add this Rules to the existing Business Rules.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public BusinessRuleFlags? AddedRules { get; set; }

        /// <summary>
        /// Remove this rules from existing Business Rules
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public BusinessRuleFlags? RemovedRules { get; set; }
    }
}