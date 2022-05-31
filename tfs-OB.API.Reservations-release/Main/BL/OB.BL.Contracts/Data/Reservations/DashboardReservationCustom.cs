using OB.Reservation.BL.Contracts.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    public partial class DashboardReservationCustom
    {
        public DashboardReservationCustom()
        {
        }

        [Key]
        public long UID { get; set; }

        [Required]
        public DateTime? FromDate { get; set; }

        [Required]
        [CustomValidation(typeof(CustomValidator), "ValidateResSummaryFromToDate")]
        public DateTime? ToDate { get; set; }

        public int RoomReservations { get; set; }

        public int NumberOfRooms { get; set; }

        public int RoomNights { get; set; }

        public decimal RoomRevenue { get; set; }

        public decimal ExtraReservations { get; set; }

        public decimal NoOfAddOns { get; set; }

        public decimal ExtraRevenue { get; set; }

        public int TotalReservation { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal RevenuePerReservation { get; set; }

        public decimal RevenuePerRoom { get; set; }

        public decimal RevenuePerRoomNight { get; set; }

        public decimal TotalRevenuePerTotalReservations { get; set; }
    }
}