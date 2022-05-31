using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Channels;
using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Data.VisualStates;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationFilter : ContractBase
    {
        
        public ReservationFilter()
        {
            ReservationRoomFilters = new List<ReservationRoomFilter>();
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember]
        public long PropertyUid { get; set; }

        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public bool IsOnRequest { get; set; }

        [DataMember]
        public bool IsReaded { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public string GuestName { get; set; }

        [DataMember]
        public Nullable<int> NumberOfNights { get; set; }

        [DataMember]
        public Nullable<int> NumberOfAdults { get; set; }

        [DataMember]
        public Nullable<int> NumberOfChildren { get; set; }

        [DataMember]
        public Nullable<int> NumberOfRooms { get; set; }

        [DataMember]
        public Nullable<long> TPI_UID { get; set; }

        [DataMember]
        public Nullable<long> PaymentTypeUid { get; set; }

        [DataMember]
        public Nullable<long> Status { get; set; }

        [DataMember]
        public Nullable<decimal> TotalAmount { get; set; }

        [DataMember]
        public Nullable<decimal> ExternalTotalAmount { get; set; }

        [DataMember]
        public Nullable<decimal> ExternalCommissionValue { get; set; }

        [DataMember]
        public Nullable<bool> ExternalIsPaid { get; set; }

        [DataMember]
        public Nullable<long> ChannelUid { get; set; }

        [DataMember]
        public string ChannelName { get; set; }

        [DataMember]
        public string TPI_Name { get; set; }

        [DataMember]
        public Nullable<bool> IsPaid { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ReservationDate { get; set; }

        [DataMember]
        public Nullable<decimal> ReservationBaseCurrencyExchangeRate { get; set; }

        [DataMember]
        public string ReservationBaseCurrencySymbol { get; set; }

        [DataMember]
        public Nullable<decimal> PropertyBaseCurrencyExchangeRate { get; set; }

        [DataMember]
        public string PropertyBaseCurrencySymbol { get; set; }

        [DataMember]
        public Nullable<decimal> RepresentativeCurrencyExchangeRate { get; set; }

        [DataMember]
        public string RepresentativeCurrencySymbol { get; set; }

        [DataMember]
        public Nullable<long> PartnerUid { get; set; }

        [DataMember]
        public string PartnerReservationNumber { get; set; }

        [DataMember]
        public Nullable<long> CreatedBy { get; set; }

        [DataMember]
        public Nullable<long> ModifiedBy { get; set; }

        [DataMember]
        public Nullable<long> Guest_UID { get; set; }

        [DataMember]
        public Nullable<long> ExternalChannelUid { get; set; }

        [DataMember]
        public Nullable<long> ExternalTPIUid { get; set; }

        [DataMember]
        public string ExternalName { get; set; }

        [DataMember]
        public Nullable<bool> IsMobile { get; set; }

        [DataMember]
        public string LoyaltyCardNumber { get; set; }

        [DataMember]
        public List<ReservationRoomFilter> ReservationRoomFilters { get; set; }

        /// <summary>
        /// The Big Pull Authentication User that made the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BigPullAuthRequestor_UID { get; set; }

        /// <summary>
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BigPullAuthOwner_UID { get; set; }
    }
}