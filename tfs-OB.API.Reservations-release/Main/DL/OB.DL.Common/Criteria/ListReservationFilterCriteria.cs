using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.DL.Common
{
    /// <summary>
    /// Request class to be used in operations that search for a specific Reservation or set of Reservations.
    /// </summary>
    public class ListReservationFilterCriteria
    {
        public ListReservationFilterCriteria()
        {
            ReservationUIDs = new List<long>();
            ReservationNumbers = new List<string>();
        }

        /// <summary>
        /// UIDs used to look for the Reservation.
        /// </summary>
        public List<long> ReservationUIDs { get; set; }

        /// <summary>
        /// CreatedDate used to look for the Reservation.
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// PropertyUIDs used to look for the Reservation.
        /// </summary>
        public List<long> PropertyUIDs { get; set; }


        /// <summary>
        /// PropertyNames used to look for the Reservation.
        /// </summary>
        public List<string> PropertyNames { get; set; }

        /// <summary>
        /// List of Reservation Numbers, e.g. RES000234/01, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<string> ReservationNumbers { get; set; }

        /// <summary>
        /// IsOnRequest used to look for the Reservation.
        /// </summary>
        public bool? IsOnRequest { get; set; }

        /// <summary>
        /// IsReaded used to look for the Reservation.
        /// </summary>
        public bool? IsReaded { get; set; }

        /// <summary>
        /// Filter by Modified From Date
        /// </summary>
        public DateTime? ModifiedFrom { get; set; }

        /// <summary>
        /// Filter by Modified To Date
        /// </summary>
        public DateTime? ModifiedTo { get; set; }

        /// <summary>
        /// GuestName used to look for the Reservation.
        /// </summary>
        public string GuestName { get; set; }

        /// <summary>
        /// NumberOfNights used to look for the Reservation.
        /// </summary>
        public int? NumberOfNights { get; set; }

        /// <summary>
        /// NumberOfAdults used to look for the Reservation.
        /// </summary>
        public int? NumberOfAdults { get; set; }

        /// <summary>
        /// NumberOfChildren used to look for the Reservation.
        /// </summary>
        public int? NumberOfChildren { get; set; }

        /// <summary>
        /// NumberOfRooms used to look for the Reservation.
        /// </summary>
        public int? NumberOfRooms { get; set; }

        /// <summary>
        /// List of PaymentType Ids, that are used to look for reservations for the
        /// </summary>
        public List<long> PaymentTypeIds { get; set; }

        // <summary>
        /// List of Guest Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<long> GuestIds { get; set; }

        /// <summary>
        /// List of Tpi Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<long> TpiIds { get; set; }

        /// <summary>
        /// Filter reservation by status
        /// </summary>
        public List<long> ReservationStatus { get; set; }

        /// <summary>
        /// TotalAmount used to look for the Reservation.
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// ExternalTotalAmount used to look for the Reservation.
        /// </summary>
        public decimal? ExternalTotalAmount { get; set; }

        /// <summary>
        /// ExternalCommissionValue used to look for the Reservation.
        /// </summary>
        public decimal? ExternalCommissionValue { get; set; }

        /// <summary>
        /// ExternalIsPaid used to look for the Reservation.
        /// </summary>
        public bool? ExternalIsPaid { get; set; }

        /// <summary>
        /// ChannelUIDs used to look for the Reservation.
        /// </summary>
        public List<long> ChannelUIDs { get; set; }

        /// <summary>
        /// ChannelNames used to look for the Reservation.
        /// </summary>
        public List<string> ChannelNames { get; set; }

        /// <summary>
        /// TPINames used to look for the Reservation.
        /// </summary>
        public List<string> TPINames { get; set; }

        /// <summary>
        /// IsPaid used to look for the Reservation.
        /// </summary>
        public bool? IsPaid { get; set; }

        /// <summary>
        /// ReservationDate used to look for the Reservation.
        /// </summary>
        public DateTime? ReservationDate { get; set; }

        /// <summary>
        /// ExternalChannelUids used to look for the Reservation.
        /// </summary>
        public List<long> ExternalChannelUids { get; set; }

        /// <summary>
        /// ExternalTPIUids used to look for the Reservation.
        /// </summary>
        public List<long> ExternalTPIUids { get; set; }

        /// <summary>
        /// ExternalNames used to look for the Reservation.
        /// </summary>
        public List<string> ExternalNames { get; set; }

        /// <summary>
        /// List of Partner Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        /// 
        public List<int> PartnerIds { get; set; }

        /// <summary>
        /// List of Partner Reservation Numbers, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<string> PartnerReservationNumbers { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// CheckIn Date
        /// </summary>
        public DateTime? CheckIn { get; set; }

        /// <summary>
        /// CheckInTo Date to filter on the ManageReservation grid
        /// </summary>        
        public DateTime? CheckInTo { get; set; }

        /// <summary>
        /// CheckOut Date
        /// </summary>
        public DateTime? CheckOut { get; set; }

        /// <summary>
        /// CheckOutFrom Date to filter on the ManageReservation grid
        /// </summary>      
        public DateTime? CheckOutFrom { get; set; }

        /// <summary>
        /// Filter Qty exclunding ReservationRooms Cancelled
        /// </summary>
        public bool? FilterQtyExcludeReservationRoomsCancelled { get; set; }

        /// <summary>
        /// Payment type billed with applies deposit cost
        /// </summary>
        public bool? ApplyDepositPolicy { get; set; }

        /// <summary>
        /// List of Employee Ids to look for.
        /// </summary>
        public List<long> EmployeeIds { get; set; }

        //public decimal? DepositCost { get; set; }

        //public int? DepositNumberOfNight { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRooms child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomsFilter { get; set; }

        /// <summary>
        /// Page Index
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page Size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Return Total
        /// </summary>
        //public bool ReturnTotal { get; set; }
        
        /// <summary>
        /// The Big Pull Authentication User that made the Action.
        /// </summary>
        public List<long> BigPullAuthRequestorUIDs { get; set; }

        /// <summary>
        /// The Big Pull Authentication User that made the Reservation.
        /// </summary>
        public List<long> BigPullAuthOwnerUIDs { get; set; }
        public List<OB.DL.Common.Filter.FilterByInfo> Filters { get; set; }

        public List<OB.DL.Common.Filter.SortByInfo> Orders { get; set; }

        public Kendo.DynamicLinq.Filter NestedFilters { get; set; }
    }
}