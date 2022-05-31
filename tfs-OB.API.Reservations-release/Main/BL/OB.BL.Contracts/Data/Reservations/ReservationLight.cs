using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationLight : ContractBase
    {
        public ReservationLight()
        {
        }

        [DataMember]
        public long UID { get; set; }
        
        [DataMember]
        public string Number { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GDSSource { get; set; }

        [DataMember]
        public System.DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> TotalAmount { get; set; }

        [DataMember]
        public int Adults { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> Children { get; set; }

        [DataMember]
        public long Status { get; set; }
        
        [DataMember]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifyDate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Tax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ChannelAffiliateName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ReservationCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ReservationBaseCurrency_UID { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationBaseCurrency_ISO { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationBaseCurrency_Symbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> ReservationCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ReservationCurrencyExchangeRateDate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> TotalTax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> NumberOfRooms { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> RoomsTax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> RoomsExtras { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> RoomsPriceSum { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> RoomsTotalAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> GroupCode_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PmsRservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsOnRequest { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestFirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestLastName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> PropertyBaseCurrencyExchangeRate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<ReservationRoomLight> ReservationRooms { get; set; }
    }
}