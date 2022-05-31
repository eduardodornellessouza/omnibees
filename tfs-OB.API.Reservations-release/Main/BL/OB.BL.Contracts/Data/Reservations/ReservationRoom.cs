namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using OB.Reservation.BL.Contracts.Data.Properties;
    using OB.Reservation.BL.Contracts.Data.Rates;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoom : ContractBase
    {
        public ReservationRoom()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long Reservation_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> RoomType_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string GuestName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string GuestEmail { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<bool> SmokingPreferences { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> DateFrom { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> DateTo { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> AdultCount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> ChildCount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> TotalTax { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> Package_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string CancellationPolicy { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string OtherPolicy { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> CancellationPolicyDays { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string ReservationRoomNo { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> Status { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string RoomName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> Rate_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<bool> IsCanceledByChannels { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsPriceSum { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsExtrasSum { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsTotalAmount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.TimeSpan> ArrivalTime { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string PmsRservationNumber { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<bool> TPIDiscountIsPercentage { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<bool> TPIDiscountIsValueDecrease { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> TPIDiscountValue { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<bool> IsCancellationAllowed { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool CancellationCosts { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> CancellationValue { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> CancellationPaymentModel { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> CancellationNrNights { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> CommissionType { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string CommissionTypeName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> CommissionValue { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<DateTime> CancellationDate { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IList<ReservationRoomChild> ReservationRoomChilds { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IList<ReservationRoomDetail> ReservationRoomDetails { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IList<ReservationRoomExtra> ReservationRoomExtras { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IList<ReservationRoomTaxPolicy> ReservationRoomTaxPolicies { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<ReservationRoomTaxPolicy> ExternalReservationRoomTaxPolicies { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public RoomType RoomType { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Rate Rate { get; set; }

        #region Deposit Policy

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string DepositPolicy { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long DepositPolicy_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool IsDepositAllowed { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool DepositCosts { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? DepositDays { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? DepositPaymentModel { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public decimal? DepositValue { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? DepositNrNights { get; set; }

        #endregion

        #region Guarantee type

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string DeposityGuaranteeType { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string DepositInformation { get; set; }

        #endregion

        #region LOYALTY LEVEL

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long? LoyaltyLevel_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string LoyaltyLevelName { get; set; }

        #endregion

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ReservationRoomAdditionalData ReservationRoomAdditionalData { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ReservationRoomCancelationCost ReservationRoomCancelationCost { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<ReservationRoomIncentive> ReservationRoomIncentivesPeriods { get; set; }
    }
}