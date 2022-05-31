using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a specific Reservation or set of Reservations.
    /// </summary>
    [DataContract]
    public class ListReservationsFilterRequest : GridPagedRequest
    {
        /// <summary>
        /// Filter by Number in ReservationFilter table.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> ReservationNumbers { get; set; }

        /// <summary>
        /// PropertyUIDs used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// PropertyNames used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> PropertyNames { get; set; }

        /// <summary>
        /// ChannelUIDs used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ChannelUIDs { get; set; }

        /// <summary>
        /// ExternalChannelUIDs used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ExternalChannelIds { get; set; }

        /// <summary>
        /// ExternalTPIUIDs used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ExternalTpiIds { get; set; }

        /// <summary>
        /// ExternalNames used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> ExternalNames { get; set; }


        /// <summary>
        /// ChannelNames used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> ChannelNames { get; set; }

        /// <summary>
        /// List of Tpi Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> TpiIds { get; set; }

        /// <summary>
        /// TPINames used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> TPINames { get; set; }

        /// <summary>
        /// List of Partner Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> PartnerIds { get; set; }

        /// <summary>
        /// List of Partner Reservation Numbers, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> PartnerReservationNumbers { get; set; }

        /// <summary>
        /// List of Reservation Status Codes to look for.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ReservationStatusCodes { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ModifiedFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ModifiedTo { get; set; }

        /// <summary>
        /// CheckIn Date
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CheckIn { get; set; }

        /// <summary>
        /// CheckOut Date
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CheckOut { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomsFilters child collection of the found Reservations.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeReservationRoomsFilters { get; set; }

        /// <summary>
        /// if set to <c>true</c> and creditCardToken != null then set CardNumber = Token.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UnifyCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateIncentives lookup collection of the found Reservations -> ReservationRooms -> Rates -> RateIncentives
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ReservationStatus { get; set; }

        /// <summary>
        /// GuestName used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestName { get; set; }

        /// <summary>
        /// NumberOfNights used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NumberOfNights { get; set; }

        /// <summary>
        /// NumberOfAdults used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NumberOfAdults { get; set; }

        /// <summary>
        /// NumberOfChildren used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NumberOfChildren { get; set; }

        /// <summary>
        /// NumberOfRooms used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NumberOfRooms { get; set; }

        /// <summary>
        /// List of PaymentType Ids, that are used to look for reservations for the
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PaymentTypeIds { get; set; }

        /// <summary>
        /// TotalAmount used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// ExternalTotalAmount used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? ExternalTotalAmount { get; set; }

        /// <summary>
        /// ExternalCommissionValue used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? ExternalCommissionValue { get; set; }

        /// <summary>
        /// ExternalIsPaid used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ExternalIsPaid { get; set; }

        /// <summary>
        /// IsPaid used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsPaid { get; set; }

        /// <summary>
        /// ReservationDate used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ReservationDate { get; set; }

        /// <summary>
        /// PO_TPI_PCC used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> PO_TPI_PCC { get; set; }

        /// <summary>
        /// PO_TPI_Name used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> PO_TPI_Name { get; set; }

        /// <summary>
        /// Payment type billed with applies deposit cost
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ApplyDepositPolicy { get; set; }

        /// <summary>
        /// CreatedDate used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// IsOnRequest used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsOnRequest { get; set; }

        /// <summary>
        /// IsReaded used to look for the Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsReaded { get; set; }

        /// <summary>
        /// Filter Qty exclunding ReservationRooms Cancelled
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? FilterQtyExcludeReservationRoomsCancelled { get; set; }

    }
}

