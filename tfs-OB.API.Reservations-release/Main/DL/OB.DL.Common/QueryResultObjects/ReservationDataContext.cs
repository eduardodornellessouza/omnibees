using OB.Domain.Reservations;
using OB.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System;

namespace OB.DL.Common.QueryResultObjects
{
    public class ReservationDataContext
    {
        public ReservationDataContext()
        {
            CancellationPolicies = new Dictionary<long, CancellationPolicyQR1>();
            DepositPolicies = new Dictionary<long, DepositPolicyQR1>();
            OtherPolicies = new Dictionary<long, OtherPolicyQR1>();
            RatesAvailabilityType = new Dictionary<long, int>();
        }

        public long ReservationUID { get; set; }

        public bool IsExistingReservation { get; set; }

        public string PropertyName { get; set; }

        public long? PropertyBaseLanguage_UID { get; set; }
        public long? PropertyBaseCurrency_UID { get; set; }
        public long? PropertyCountry_UID { get; set; }
        public long? PropertyCity_UID { get; set; }

        public long? TPIProperty_UID { get; set; }

        public bool? TPIProperty_CommissionIsPercentage { get; set; }

        public decimal? TPIProperty_Commission { get; set; }

        public long? TPICompany_UID { get; set; }

        public bool? TPICompany_CommissionIsPercentage { get; set; }

        public decimal? TPICompany_BaseCommission { get; set; }

        public long? TPI_UID { get; set; }

        [Obsolete("Use PromotionalCode_UID of Reservation instead. This field is obsolete since creation of Promocode periods. It will be removed on OB version 0.9.48")]
        public long? TPI_PromotionalCode_UID { get; set; }

        public long? SalesmanCommission_UID { get; set; }

        public long? Salesman_UID { get; set; }

        public decimal? SalesmanBaseCommission { get; set; }

        public bool? SalesmanIsBaseCommissionPercentage { get; set; }

        public long? ReservationPaymentDetail_UID { get; set; }

        public long Currency_UID { get; set; }

        public string Currency_Symbol { get; set; }

        public long Client_UID { get; set; }

        public long Channel_UID { get; set; }

        public string ChannelName { get; set; }

        public bool IsChannelValid { get; set; }

        public long ChannelType { get; set; }

        public int ChannelOperatorType { get; set; }

        public long BookingEngineChannel_UID { get; set; }

        public long? Guest_UID { get; set; }

        public long? ChannelProperty_UID { get; set; }

        public int ChannelPropertyOperatorBillingType { get; set; }

        public int RateChannelsAndPaymentsCount { get; set; }

        public bool? ChannelHandleCredit { get; set; }

        /// <summary>
        /// True when the Request is not from BookingEngine, e.g., it's from a Push,Pull, Sabre , etc channel.
        /// False when the Reservation/Request is from BE.
        /// </summary>
        public bool IsFromChannel { get; set; }

        public bool ValidateAllotment { get; set; }

        public List<Inventory> Inventories { get; set; }

        public List<ReservationRoomDetail> ReservationRoomDetails { get; set; }

        public bool IsOnRequestEnable { get; set; }

        public Dictionary<long, CancellationPolicyQR1> CancellationPolicies { get; set; }
        public Dictionary<long, DepositPolicyQR1> DepositPolicies { get; set; }
        public Dictionary<long, OtherPolicyQR1> OtherPolicies { get; set; }

        // Loyalty
        public long? GuestLoyaltyLevel_UID { get; set; }
        public bool IsLimitsForPeriodicityActive { get; set; }
        public int? LoyaltyLevelLimitsPeriodicity_UID { get; set; }
        public int LoyaltyLevelLimitsPeriodicityValue { get; set; }
        public bool IsForNumberOfReservationsActive { get; set; }
        public int? IsForNumberOfReservationsValue { get; set; }
        public bool IsForNightsRoomActive { get; set; }
        public int? IsForNightsRoomValue { get; set; }
        public bool IsForTotalReservationsActive { get; set; }
        public decimal? IsForTotalReservationsValue { get; set; }
        public bool IsForReservationActive { get; set; }
        public int? IsForReservationRoomNightsValue { get; set; }
        public decimal? IsForReservationValue { get; set; }
        public long? LoyaltyCurrency_Uid { get; set; }
        public string LoyaltyLevelDescription { get; set; }
        
        public string LoyaltyLevelName { get; set; }
        public decimal LoyaltyLevelDiscountValue { get; set; }
        public bool LoyaltyLevelIsPercentage { get; set; }

        //Availability
        public Dictionary<long,int> RatesAvailabilityType { get; set; }
        public bool IsAvailabilityRestoreStopped { get; set; }
    }
}