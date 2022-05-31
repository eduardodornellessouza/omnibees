using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.REST.Services.Helper
{
    public class ReservationBuilder
    {
        public Reservation.BL.Contracts.Data.Reservations.Reservation reservationDetail;
        public Guest guest;
        public List<ReservationRoom> reservationRooms;
        public List<ReservationRoomDetail> reservationRoomDetails;
        public List<long> guestActivity;
        public List<ReservationRoomExtra> reservationRoomExtras;
        public List<ReservationRoomChild> reservationRoomChild;
        public ReservationPaymentDetail reservationPaymentDetail;
        public List<ReservationRoomExtrasSchedule> reservationExtraSchedule;

        public ReservationBuilder()
        {
            reservationDetail = new Reservation.BL.Contracts.Data.Reservations.Reservation();
            guest = new Guest();
            reservationRooms = new List<ReservationRoom>();
            reservationRoomDetails = new List<ReservationRoomDetail>();
            guestActivity = new List<long>();
            reservationRoomExtras = new List<ReservationRoomExtra>();
            reservationRoomChild = new List<ReservationRoomChild>();
            reservationPaymentDetail = new ReservationPaymentDetail();
            reservationExtraSchedule = new List<ReservationRoomExtrasSchedule>();

            // reservation object
            reservationDetail.UID = 0;
            reservationDetail.Guest_UID = 0;
            reservationDetail.Channel_UID = 1;

            //reservationDetail.Channel_UID = 2;
            //reservationDetail.Channel = new Contracts.Data.Channels.Channel()
            //{
            //    ChannelCode = "2",
            //    UID = 2,
            //    OperatorType = (int)Constants.OperatorsType.Operators
            //};
            //reservationDetail.Number = "TEST-000000";

            reservationDetail.Date = new DateTime(2014, 05, 14, 17, 36, 0);
            reservationDetail.TotalAmount = 110;
            reservationDetail.Adults = 1;
            reservationDetail.Children = 0;
            reservationDetail.Status = 1;
            reservationDetail.Notes = null;
            reservationDetail.IPAddress = "::1";
            reservationDetail.TPI_UID = null;
            reservationDetail.PromotionalCode_UID = null;
            reservationDetail.ChannelProperties_RateModel_UID = null;
            reservationDetail.ChannelProperties_Value = null;
            reservationDetail.ChannelProperties_IsPercentage = null;
            reservationDetail.InvoicesDetail_UID = null;
            reservationDetail.Property_UID = 1263;
            reservationDetail.CreatedDate = new DateTime(2014, 05, 14, 17, 36, 51);
            reservationDetail.CreateBy = null;
            reservationDetail.ModifyDate = null;
            reservationDetail.ModifyBy = null;
            reservationDetail.BESpecialRequests1_UID = 0;
            reservationDetail.BESpecialRequests2_UID = 0;
            reservationDetail.BESpecialRequests3_UID = 0;
            reservationDetail.BESpecialRequests4_UID = 0;
            reservationDetail.TransferLocation_UID = 0;
            reservationDetail.TransferTime = null;
            reservationDetail.CancellationPolicyDays = null;
            reservationDetail.BillingAddress1 = null;
            reservationDetail.BillingAddress2 = null;
            reservationDetail.BillingContactName = "Test1 Teste2";
            reservationDetail.BillingPostalCode = null;
            reservationDetail.BillingCity = "asdasdasd";
            reservationDetail.BillingCountry_UID = 157;
            reservationDetail.BillingPhone = "123123";
            reservationDetail.Tax = 0;
            reservationDetail.PaymentMethodType_UID = 1;
            reservationDetail.CancelReservationReason_UID = null;
            reservationDetail.OtherLoyaltyCardType_UID = null;
            reservationDetail.LoyaltyCardNumber = null;
            reservationDetail.ReservationCurrency_UID = 34;
            reservationDetail.ReservationBaseCurrency_UID = 34;
            reservationDetail.ReservationCurrencyExchangeRate = 1;
            reservationDetail.ReservationCurrencyExchangeRateDate = new DateTime(2014, 05, 14, 17, 36, 51);
            reservationDetail.ReservationLanguageUsed_UID = 4;
            reservationDetail.TotalTax = 0;
            reservationDetail.NumberOfRooms = 1;
            reservationDetail.RoomsTax = 0;
            reservationDetail.RoomsExtras = 0;
            reservationDetail.RoomsPriceSum = 110;
            reservationDetail.RoomsTotalAmount = 110;
            reservationDetail.GroupCode_UID = null;
            reservationDetail.IsOnRequest = false;
            reservationDetail.OnRequestDecisionUser = null;
            reservationDetail.OnRequestDecisionDate = null;
            reservationDetail.BillingState_UID = 2430214;
            reservationDetail.BillingEmail = "tony.santos@visualforma.pt";
            reservationDetail.IsPartialPayment = false;
            reservationDetail.InstallmentAmount = null;
            reservationDetail.NoOfInstallment = null;
            reservationDetail.InterestRate = null;
            reservationDetail.GuestFirstName = "Test1";
            reservationDetail.GuestLastName = "Teste2";
            reservationDetail.GuestEmail = "tony.santos@visualforma.pt";
            reservationDetail.GuestPhone = "123123";
            reservationDetail.GuestIDCardNumber = "12312312";
            reservationDetail.GuestCountry_UID = 157;
            reservationDetail.GuestState_UID = 2430214;
            reservationDetail.GuestCity = "asdasdasd";
            reservationDetail.GuestAddress1 = null;
            reservationDetail.GuestAddress2 = null;
            reservationDetail.GuestPostalCode = null;
            reservationDetail.UseDifferentBillingInfo = false;
            reservationDetail.PaymentAmountCaptured = null;
            reservationDetail.PaymentGatewayTransactionDateTime = null;
            reservationDetail.IsMobile = false;
            reservationDetail.IsPaid = null;
            reservationDetail.IsPaidDecisionUser = null;
            reservationDetail.IsPaidDecisionDate = null;
            reservationDetail.Salesman_UID = null;
            reservationDetail.SalesmanCommission = null;
            reservationDetail.SalesmanIsCommissionPercentage = null;
            reservationDetail.Company_UID = null;
            reservationDetail.Employee_UID = null;
            reservationDetail.TPICompany_UID = null;
            reservationDetail.PropertyBaseCurrencyExchangeRate = 1;
        }

        public ReservationBuilder WithNewGuest()
        {
            guest.UID = 0;
            guest.Prefix = 1;
            guest.FirstName = "Test1";
            guest.LastName = "Teste2";
            guest.Address1 = null;
            guest.Address2 = null;
            guest.City = "asdasdasd";
            guest.PostalCode = null;
            guest.BillingCountry_UID = null;
            guest.Country_UID = 157;
            guest.Phone = "123123";
            guest.UserName = "tony.santos@visualforma.pt";
            guest.GuestCategory_UID = 1;
            guest.Property_UID = 463;
            guest.Currency_UID = 34;
            guest.Language_UID = 4;
            guest.Email = "tony.santos@visualforma.pt";
            guest.Birthday = null;
            guest.CreatedByTPI_UID = null;
            //guest.IsActive = true;
            guest.CreateDate = new DateTime(2014, 05, 14, 17, 36, 51);
            //guest.LastLoginDate = null;
            guest.AllowMarketing = true;
            //guest.CreateBy = null;
            //guest.CreatedDate = null;
            //guest.ModifyBy = null;
            //guest.ModifyDate = null;
            //guest.Question_UID = null;
            //guest.IsFacebookFan = null;
            //guest.IsTwitterFollower = null;
            guest.IDCardNumber = "12312312";
            guest.BillingState_UID = null;
            guest.State_UID = 2430214;
            guest.Client_UID = 476;
            guest.UseDifferentBillingInfo = false;
            guest.IsImportedFromExcel = false;
            //guest.IsDeleted = false;
            //guest.Gender = "M";
            return this;
        }

        public ReservationBuilder WithOneRoom()
        {
            reservationRooms.Add(new ReservationRoom
            {
                UID = 1,
                Reservation_UID = 0,
                RoomType_UID = 3709,
                GuestName = "Test1 Teste2",
                SmokingPreferences = null,
                DateFrom = new DateTime(2014, 07, 24),
                DateTo = new DateTime(2014, 07, 25),
                AdultCount = 1,
                ChildCount = 0,
                TotalTax = 0,
                Package_UID = 0,
                CancellationPolicyDays = null,
                Status = 1,
                CreatedDate = null,
                ModifiedDate = null,
                Rate_UID = 4639,
                IsCanceledByChannels = null,
                ReservationRoomsPriceSum = 110,
                ReservationRoomsExtrasSum = 0,
                ReservationRoomsTotalAmount = 110,
                ArrivalTime = null,
                TPIDiscountIsPercentage = null,
                TPIDiscountIsValueDecrease = null,
                TPIDiscountValue = null,
                IsCancellationAllowed = null,
                CancellationCosts = false,
                CancellationValue = null,
                CancellationPaymentModel = null,
                CancellationNrNights = null
            });

            return this;
        }

        public ReservationBuilder WithOneDay()
        {
            reservationRoomDetails.Add(new ReservationRoomDetail
            {
                UID = 0,
                RateRoomDetails_UID = 5137349,
                Price = 110,
                ReservationRoom_UID = 1,
                AdultPrice = 110,
                ChildPrice = 0,
                CreatedDate = null,
                ModifiedDate = null,
                Date = new DateTime(2014, 07, 24)
            });

            return this;
        }

        public ReservationBuilder WithCreditCardPayment()
        {
            reservationPaymentDetail.UID = 0;
            reservationPaymentDetail.PaymentMethod_UID = 1;
            reservationPaymentDetail.Reservation_UID = 0;
            reservationPaymentDetail.Amount = 110;
            reservationPaymentDetail.Currency_UID = 34;
            reservationPaymentDetail.CVV = "111";
            reservationPaymentDetail.CardName = "aaa";
            reservationPaymentDetail.CardNumber = "4929828969879794";
            reservationPaymentDetail.ExpirationDate = new DateTime(2016, 01, 01);
            reservationPaymentDetail.CreatedDate = null;
            reservationPaymentDetail.ModifiedDate = null;
            return this;
        }
    }

}
