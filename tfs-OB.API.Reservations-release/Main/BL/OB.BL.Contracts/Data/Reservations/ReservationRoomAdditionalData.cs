using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using static OB.Reservation.BL.Constants;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationRoomAdditionalData : ContractBase
    {
        public ReservationRoomAdditionalData()
        {
            ExternalSellingInformationByRule = new List<ExternalSellingRoomInformation>();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationRoom_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationRoomNo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ChannelReservationRoomId { get; set; }

        #region Deposit Policy

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long DepositPolicy_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDepositCostsAllowed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool DepositCosts { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? DepositDays { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DepositPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DepositPolicyDescription { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? PaymentModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NrNights { get; set; }

        #endregion

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public CancellationPolicy CancellationPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OtherPolicy OtherPolicy { get; set; }

        #region Guarantee type
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DeposityGuaranteeType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DepositInformation { get; set; }
        #endregion

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomTaxPolicy> TaxPolicies { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int CommissionType { get; set; }

        /// <summary>
        /// External Selling Room Information By Rule
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ExternalSellingRoomInformation> ExternalSellingInformationByRule { get; set; }
    }

    [DataContract]
    public class ExternalSellingRoomInformation
    {

        /// <summary>
        /// KeeperUID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long KeeperUID { get; set; }

        /// <summary>
        /// KeeperType
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public PO_KeeperType KeeperType { get; set; }

        /// <summary>
        /// Room Total Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationRoomsTotalAmount { get; set; }

        /// <summary>
        /// Ttal Days Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationRoomsPriceSum { get; set; }

        /// <summary>
        /// Total Room Extras
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? ReservationRoomsExtrasSum { get; set; }

        /// <summary>
        /// Total Room Taxes
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? TotalTax { get; set; }

        /// <summary>
        /// Prices per day
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PriceDay> PricesPerDay { get; set; }

        /// <summary>
        /// Taxes per day
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PriceDay> TaxesPerDay { get; set; }

        /// <summary>
        /// Tax Policies of the room
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomTaxPolicy> TaxPolicies { get; set; }

    }

    [DataContract]
    public class PriceDay
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Price { get; set; }
    }
}
