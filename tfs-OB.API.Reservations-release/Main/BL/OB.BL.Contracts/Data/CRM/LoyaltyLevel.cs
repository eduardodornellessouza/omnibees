using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyLevel : ContractBase
    {
        public LoyaltyLevel()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long LoyaltyProgram_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte LevelNr { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal DiscountValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAutomatic { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsLimitsForPeriodicityActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> LoyaltyLevelLimitsPeriodicityValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> LoyaltyLevelLimitsPeriodicity_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsForNumberOfReservationsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> IsForNumberOfReservationsValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsForNightsRoomActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> IsForNightsRoomValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsForTotalReservationsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> IsForTotalReservationsValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsForReservationActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> IsForReservationRoomNightsValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> IsForReservationValue { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyLevelsCurrency> LoyaltyLevelsCurrencies { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyLevelsLanguage> LoyaltyLevelsLanguages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RateLoyaltyLevel> RateLoyaltyLevels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public LoyaltyLevelLimitsPeriodicity LoyaltyLevelLimitsPeriodicity { get; set; }
    }
}
