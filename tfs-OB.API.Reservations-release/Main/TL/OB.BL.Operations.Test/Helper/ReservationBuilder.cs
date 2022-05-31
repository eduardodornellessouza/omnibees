using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using OB.BL.Contracts.Data.BE;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.TypeConverters;
using OB.BL.Operations.Test.Domain.CRM;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using contractCRMOB = OB.BL.Contracts.Data.CRM;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Helper
{
    //Atention: Some methods are commented and not removed yet because can be necessary in the future.
    public class ReservationDataBuilder
    {
        private DateTime now = DateTime.UtcNow;

        public ReservationInputData InputData { get; set; }
        public ReservationData ExpectedData { get; set; }
        private long? _propertyId;

        // arrange
        public ReservationDataBuilder(long? channelUID, long? propertyId = null, long? UID = null, bool ignoreChangeResNumberIfBE = false, 
            string resNumber = null, long resLang = 4, bool detailOnly = false, int dayOffset = 0, long resCurrencyId = 34, long rateCurrencyId = 34)
        {
            // initialize
            InputData = new ReservationInputData(detailOnly: detailOnly);
            ExpectedData = new ReservationData();
            _propertyId = propertyId;

            // reservation detail data
            if (resNumber != null)
                InputData.reservationDetail.Number = resNumber;
            else if (channelUID != 1)
                InputData.reservationDetail.Number = "CHANNELX-RES123";
            else
                InputData.reservationDetail.Number = "RES000024-1806";
            InputData.reservationDetail.UID = UID ?? 0;
            InputData.reservationDetail.Guest_UID = 0;
            InputData.reservationDetail.Channel_UID = channelUID;
            InputData.reservationDetail.Date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(dayOffset);
            InputData.reservationDetail.TotalAmount = 110;
            InputData.reservationDetail.Children = 0;
            InputData.reservationDetail.Status = 1;
            InputData.reservationDetail.Notes = null;
            InputData.reservationDetail.IPAddress = "::1";
            InputData.reservationDetail.TPI_UID = null;
            InputData.reservationDetail.PromotionalCode_UID = null;
            InputData.reservationDetail.ChannelProperties_RateModel_UID = null;
            InputData.reservationDetail.ChannelProperties_Value = null;
            InputData.reservationDetail.ChannelProperties_IsPercentage = null;
            InputData.reservationDetail.InvoicesDetail_UID = null;
            InputData.reservationDetail.Property_UID = _propertyId ?? 1263;
            InputData.reservationDetail.CreatedDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            InputData.reservationDetail.CreateBy = null;
            InputData.reservationDetail.ModifyDate = null;
            InputData.reservationDetail.ModifyBy = null;
            InputData.reservationDetail.BESpecialRequests1_UID = null;
            InputData.reservationDetail.BESpecialRequests2_UID = null;
            InputData.reservationDetail.BESpecialRequests3_UID = null;
            InputData.reservationDetail.BESpecialRequests4_UID = null;
            InputData.reservationDetail.TransferLocation_UID = 0;
            InputData.reservationDetail.TransferTime = null;
            InputData.reservationDetail.CancellationPolicyDays = 0;  //was null
            InputData.reservationDetail.BillingAddress1 = null;
            InputData.reservationDetail.BillingAddress2 = null;
            InputData.reservationDetail.BillingContactName = "Test1 Teste2";
            InputData.reservationDetail.BillingPostalCode = null;
            InputData.reservationDetail.BillingCity = "asdasdasd";
            InputData.reservationDetail.BillingCountry_UID = 157;
            InputData.reservationDetail.BillingPhone = "123123";
            InputData.reservationDetail.Tax = 0;
            InputData.reservationDetail.CancelReservationReason_UID = null;
            InputData.reservationDetail.OtherLoyaltyCardType_UID = null;
            InputData.reservationDetail.LoyaltyCardNumber = null;
            InputData.reservationDetail.ReservationCurrency_UID = resCurrencyId;
            InputData.reservationDetail.ReservationBaseCurrency_UID = rateCurrencyId;
            InputData.reservationDetail.ReservationCurrencyExchangeRate = 1;
            InputData.reservationDetail.ReservationCurrencyExchangeRateDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            InputData.reservationDetail.ReservationLanguageUsed_UID = resLang;
            InputData.reservationDetail.TotalTax = 0;
            InputData.reservationDetail.RoomsTax = 0;
            InputData.reservationDetail.RoomsExtras = 0;
            InputData.reservationDetail.RoomsPriceSum = 110;
            InputData.reservationDetail.RoomsTotalAmount = 110; //TODO !assert final values
            InputData.reservationDetail.GroupCode_UID = null;
            InputData.reservationDetail.IsOnRequest = false;
            InputData.reservationDetail.OnRequestDecisionUser = null;
            InputData.reservationDetail.OnRequestDecisionDate = null;
            InputData.reservationDetail.BillingState_UID = 2430214;
            InputData.reservationDetail.BillingEmail = "tony.santos@visualforma.pt";
            InputData.reservationDetail.IsPartialPayment = false;
            InputData.reservationDetail.InstallmentAmount = null;
            InputData.reservationDetail.NoOfInstallment = null;
            InputData.reservationDetail.InterestRate = null;
            InputData.reservationDetail.GuestFirstName = "Test1";
            InputData.reservationDetail.GuestLastName = "Teste2";
            InputData.reservationDetail.GuestEmail = "tony.santos@visualforma.pt";
            InputData.reservationDetail.GuestPhone = "123123";
            InputData.reservationDetail.GuestIDCardNumber = "12312312";
            InputData.reservationDetail.GuestCountry_UID = 157;
            InputData.reservationDetail.GuestState_UID = 2430214;
            InputData.reservationDetail.GuestCity = "asdasdasd";
            InputData.reservationDetail.GuestAddress1 = null;
            InputData.reservationDetail.GuestAddress2 = null;
            InputData.reservationDetail.GuestPostalCode = null;
            InputData.reservationDetail.UseDifferentBillingInfo = false;
            InputData.reservationDetail.PaymentAmountCaptured = null;
            InputData.reservationDetail.PaymentGatewayTransactionDateTime = null;
            InputData.reservationDetail.IsMobile = false;
            InputData.reservationDetail.IsPaid = null;
            InputData.reservationDetail.IsPaidDecisionUser = null;
            InputData.reservationDetail.IsPaidDecisionDate = null;
            InputData.reservationDetail.Salesman_UID = null;
            InputData.reservationDetail.SalesmanCommission = null;
            InputData.reservationDetail.SalesmanIsCommissionPercentage = null;
            InputData.reservationDetail.Company_UID = null;
            InputData.reservationDetail.Employee_UID = null;
            InputData.reservationDetail.TPICompany_UID = null;
            InputData.reservationDetail.PropertyBaseCurrencyExchangeRate = 1;

            // expectedData            
            ExpectedData.reservationDetail = (OB.Domain.Reservations.Reservation)Cloner.Clone(InputData.reservationDetail);
            ExpectedData.reservationDetail.ModifyDate = null;
            ExpectedData.reservationDetail.CreatedDate = now.Date;
            ExpectedData.reservationDetail.TransferLocation_UID = null;
            if (channelUID == 1 && !ignoreChangeResNumberIfBE) ExpectedData.reservationDetail.Number = "RES000322-" + (_propertyId ?? 1263);

            // TODO !allotment

            // TODO !inventory

            // reservation - internal notes
            if (channelUID > 1)
            {
                ExpectedData.reservationDetail.InternalNotesHistory = DateTime.Now.Year.ToString();
                ExpectedData.reservationDetail.InternalNotesHistory = ExpectedData.reservationDetail.InternalNotesHistory.Replace("00:00:00", "");
            }
        }

        public ReservationDataBuilder WithEmpty()
        {
            // initialize
            InputData = new ReservationInputData();
            ExpectedData = new ReservationData();
            return this;
        }

        public ReservationDataBuilder WithGuestNull()
        {
            // initialize
            InputData.guest = null;
            ExpectedData.guest = null;
            return this;
        }

        public ReservationDataBuilder WithGuestEmpty()
        {
            // input data
            InputData.guest.FirstName = string.Empty;
            InputData.guest.LastName = string.Empty;
            InputData.guest.Email = string.Empty;
            InputData.guest.Property_UID = _propertyId ?? 1263;
            InputData.guest.Client_UID = 646;
            InputData.guest.Language_UID = 4;

            //expected data
            ExpectedData.guest.FirstName = string.Empty;
            ExpectedData.guest.LastName = string.Empty;
            ExpectedData.guest.Email = string.Empty;
            ExpectedData.guest.Property_UID = _propertyId ?? 1263;
            ExpectedData.guest.Client_UID = 646;
            ExpectedData.guest.Language_UID = 4;

            return this;
        }

        public ReservationDataBuilder WithCancelationPolicy()
        {
            InputData.reservationDetail.CancellationPolicy = "Teste";
            InputData.reservationDetail.CancellationPolicyDays = 2;

            ExpectedData.reservationDetail.CancellationPolicy = "Teste";
            ExpectedData.reservationDetail.CancellationPolicyDays = 2;

            // Add to first Room
            InputData.reservationRooms.ElementAt(0).CancellationCosts = true;
            InputData.reservationRooms.ElementAt(0).CancellationDate = DateTime.Today;
            InputData.reservationRooms.ElementAt(0).CancellationPolicyDays = 1;
            InputData.reservationRooms.ElementAt(0).IsCancellationAllowed = true;

            ExpectedData.reservationRooms.ElementAt(0).CancellationCosts = true;
            ExpectedData.reservationRooms.ElementAt(0).CancellationDate = DateTime.Today;
            ExpectedData.reservationRooms.ElementAt(0).CancellationPolicyDays = 1;
            ExpectedData.reservationRooms.ElementAt(0).IsCancellationAllowed = true;

            return this;
        }

        public ReservationDataBuilder WithCancelationPolicy(bool cancellationCosts, int cancellationPolicyDays, bool isCancellationAllowed,
            int cancellationPaymentModel, decimal? cancelationValue = null, int roomNo = 0)
        {
            InputData.reservationDetail.CancellationPolicy = "Teste";
            InputData.reservationDetail.CancellationPolicyDays = 2;

            ExpectedData.reservationDetail.CancellationPolicy = "Teste";
            ExpectedData.reservationDetail.CancellationPolicyDays = 2;

            // Add to first Room
            InputData.reservationRooms.ElementAt(roomNo).CancellationCosts = cancellationCosts;
            InputData.reservationRooms.ElementAt(roomNo).CancellationDate = DateTime.Today;
            InputData.reservationRooms.ElementAt(roomNo).CancellationPolicyDays = cancellationPolicyDays;
            InputData.reservationRooms.ElementAt(roomNo).IsCancellationAllowed = isCancellationAllowed;
            InputData.reservationRooms.ElementAt(roomNo).CancellationPaymentModel = cancellationPaymentModel;
            InputData.reservationRooms.ElementAt(roomNo).CancellationValue = cancelationValue;

            ExpectedData.reservationRooms.ElementAt(roomNo).CancellationCosts = cancellationCosts;
            ExpectedData.reservationRooms.ElementAt(roomNo).CancellationDate = DateTime.Today;
            ExpectedData.reservationRooms.ElementAt(roomNo).CancellationPolicyDays = cancellationPolicyDays;
            ExpectedData.reservationRooms.ElementAt(roomNo).IsCancellationAllowed = isCancellationAllowed;

            return this;
        }

        public ReservationDataBuilder WithExtrasNull()
        {
            // initialize
            InputData.reservationRoomExtras = null;
            ExpectedData.reservationRoomExtras = new List<ReservationRoomExtra>();
            return this;
        }

        public ReservationDataBuilder WithRoom(int roomIndex, int numAdults, bool cancelationAllowed = true, bool ignoreBEResNumber = false, bool onRequest = false,
            long? rateId = 4639)
        {
            DateTime nowPlusOne = now.AddDays(1);
            DateTime nowPlusTwo = now.AddDays(2);
            DateTime dateCheckin = new DateTime(nowPlusOne.Year, nowPlusOne.Month, nowPlusOne.Day).Date;
            DateTime dateCheckout = new DateTime(nowPlusTwo.Year, nowPlusTwo.Month, nowPlusTwo.Day).Date;

            return WithRoom(roomIndex, numAdults, dateCheckin, dateCheckout, cancelationAllowed: cancelationAllowed, ignoreBEResNumber: ignoreBEResNumber,
                onRequest: onRequest, rateId: rateId);
        }

        public ReservationDataBuilder WithRoom(IEnumerable<ReservationRoom> rrList)
        {
            InputData.reservationRooms.AddRange(rrList);
            ExpectedData.reservationRooms.AddRange(rrList);

            return this;
        }

        public ReservationDataBuilder WithRoom(int roomIndex, int numAdults, DateTime? dateCheckin, DateTime? dateCheckout, bool cancelationAllowed = true,
            bool ignoreBEResNumber = false, bool onRequest = false, long? rateId = 4639)
        {
            // input data
            InputData.reservationDetail.Adults += numAdults;
            InputData.reservationDetail.NumberOfRooms++;
            ReservationRoom room = new ReservationRoom
            {
                UID = roomIndex,
                Reservation_UID = 0,
                RoomType_UID = 3709,
                GuestName = "Test1 Teste2",
                SmokingPreferences = null,
                DateFrom = dateCheckin,
                DateTo = dateCheckout,
                AdultCount = numAdults,
                TotalTax = 0,
                Package_UID = 0,
                CancellationPolicyDays = 0,  //was null
                Status = onRequest ? (int)Constants.ReservationStatus.BookingOnRequest : 1,
                CreatedDate = null,
                ModifiedDate = null,
                Rate_UID = rateId,
                IsCanceledByChannels = null,
                ReservationRoomsPriceSum = 110,
                ReservationRoomsExtrasSum = 0,
                ReservationRoomsTotalAmount = 110,
                ArrivalTime = null,
                TPIDiscountIsPercentage = null,
                TPIDiscountIsValueDecrease = null,
                TPIDiscountValue = null,
                IsCancellationAllowed = cancelationAllowed,
                CancellationCosts = false,
                CancellationValue = null,
                CancellationPaymentModel = null,
                CancellationNrNights = null,
                ReservationRoomNo = roomIndex.ToString(),
                ChildCount = 0  //Added
            };
            InputData.reservationRooms.Add(room);

            // expected data
            ReservationRoom expectedRoom = (ReservationRoom)Cloner.Clone(room);
            ExpectedData.reservationDetail.Adults += numAdults;
            ExpectedData.reservationDetail.NumberOfRooms++;
            expectedRoom.CancellationPolicyDays = 0;
            expectedRoom.IsCancellationAllowed = cancelationAllowed;  //was false;
            if (InputData.reservationDetail.Channel_UID == 1 && !ignoreBEResNumber)
                expectedRoom.ReservationRoomNo = "RES000322-" + (_propertyId ?? 1263) + "/" + expectedRoom.ReservationRoomNo;
            else
                expectedRoom.ReservationRoomNo = this.InputData.reservationDetail.Number + "/" + expectedRoom.ReservationRoomNo;

            ExpectedData.reservationRooms.Add(expectedRoom);

            // build room details
            if (dateCheckin.HasValue && dateCheckout.HasValue)
                return WithRoomDetails(roomIndex, dateCheckin.Value, dateCheckout.Value);
            else
                return this;
        }

        public ReservationDataBuilder WithRoom(int roomIndex, int numAdults, int numChildren, DateTime? dateCheckin, DateTime? dateCheckout,
            long? rateId = null, long? roomTypeId = null, string reservationNumber = null, bool ignoreBEResNumber = false, bool cancellationAllowed = true)
        {
            // input data
            InputData.reservationDetail.Adults += numAdults;
            InputData.reservationDetail.Children += numChildren;
            InputData.reservationDetail.NumberOfRooms++;
            ReservationRoom room = new ReservationRoom
            {
                UID = roomIndex,
                Reservation_UID = 0,
                RoomType_UID = roomTypeId ?? 3709,
                GuestName = "Test1 Teste2",
                SmokingPreferences = null,
                DateFrom = dateCheckin,
                DateTo = dateCheckout,
                AdultCount = numAdults,
                ChildCount = numChildren,
                TotalTax = 0,
                Package_UID = 0,
                CancellationPolicyDays = 0,  //was null
                Status = 1,
                CreatedDate = null,
                ModifiedDate = null,
                Rate_UID = rateId ?? 4639,
                IsCanceledByChannels = null,
                ReservationRoomsPriceSum = 110,
                ReservationRoomsExtrasSum = 0,
                ReservationRoomsTotalAmount = 110,
                ArrivalTime = null,
                TPIDiscountIsPercentage = null,
                TPIDiscountIsValueDecrease = null,
                TPIDiscountValue = null,
                IsCancellationAllowed = cancellationAllowed,
                CancellationCosts = false,
                CancellationValue = null,
                CancellationPaymentModel = null,
                CancellationNrNights = null,
                ReservationRoomNo = reservationNumber + "/" + roomIndex
            };
            InputData.reservationRooms.Add(room);

            // expected data
            ReservationRoom expectedRoom = (ReservationRoom)Cloner.Clone(room);
            ExpectedData.reservationDetail.Adults += numAdults;
            ExpectedData.reservationDetail.NumberOfRooms++;
            expectedRoom.CancellationPolicyDays = 0;
            expectedRoom.IsCancellationAllowed = false;
            if (InputData.reservationDetail.Channel_UID == 1 && !ignoreBEResNumber)
                expectedRoom.ReservationRoomNo = "RES000322-" + (_propertyId ?? 1263) + "/" + expectedRoom.ReservationRoomNo;

            ExpectedData.reservationRooms.Add(expectedRoom);

            // build room details
            if (dateCheckin.HasValue && dateCheckout.HasValue)
                return WithRoomDetails(roomIndex, dateCheckin.Value, dateCheckout.Value);
            else
                return this;
        }

        public ReservationDataBuilder WithRoomDetails(int roomIndex, DateTime dateCheckin, DateTime dateCheckout)
        {
            for (DateTime date = dateCheckin; date < dateCheckout; date = date.AddDays(1))
            {
                // input data
                InputData.reservationRoomDetails.Add(new ReservationRoomDetail
                {
                    UID = 0,
                    RateRoomDetails_UID = 5137349,
                    Price = 110,
                    ReservationRoom_UID = roomIndex,
                    AdultPrice = 110,
                    CreatedDate = null,
                    ModifiedDate = null,
                    Date = date.Date,  //.Date was added
                });

                // expected data
                ExpectedData.reservationRoomDetails.Add(new ReservationRoomDetail
                {
                    UID = 0,
                    RateRoomDetails_UID = 5137349,
                    Price = 110,
                    ReservationRoom_UID = roomIndex,
                    AdultPrice = 110,
                    CreatedDate = null,
                    ModifiedDate = null,
                    Date = date.Date  //.Date was added
                });
            }

            return this;
        }

        public ReservationDataBuilder WithRoomDetails(int roomIndex, List<RateRoomDetail> rateRoomDetails)
        {
            foreach (var item in rateRoomDetails)
            {
                // input data
                InputData.reservationRoomDetails.Add(new ReservationRoomDetail
                {
                    UID = 0,
                    RateRoomDetails_UID = item.UID,
                    Price = 110,
                    ReservationRoom_UID = roomIndex,
                    AdultPrice = 110,
                    CreatedDate = null,
                    ModifiedDate = null,
                    Date = item.Date
                });

                // expected data
                ExpectedData.reservationRoomDetails.Add(new ReservationRoomDetail
                {
                    UID = 0,
                    RateRoomDetails_UID = item.UID,
                    Price = 110,
                    ReservationRoom_UID = roomIndex,
                    AdultPrice = 110,
                    CreatedDate = null,
                    ModifiedDate = null,
                    Date = item.Date
                });
            }

            var x = ExpectedData.reservationRoomDetails.Count;

            return this;
        }

        public ReservationDataBuilder WithRoomDetails(IEnumerable<ReservationRoomDetail> roomDetailsList)
        {
            InputData.reservationRoomDetails.AddRange(roomDetailsList);
            ExpectedData.reservationRoomDetails.AddRange(roomDetailsList);

            return this;
        }

        public ReservationDataBuilder WithNewGuest(long? languageUID = null, long? UID = null)
        {
            InputData.guest.UID = UID ?? 0;
            InputData.guest.Prefix = 1;
            InputData.guest.FirstName = "Test1";
            InputData.guest.LastName = "Teste2";
            InputData.guest.Address1 = null;
            InputData.guest.Address2 = null;
            InputData.guest.City = "asdasdasd";
            InputData.guest.PostalCode = null;
            InputData.guest.BillingCountry_UID = null;
            InputData.guest.Country_UID = 157;
            InputData.guest.Phone = "123123";
            InputData.guest.UserName = "tony.santos@visualforma.pt";
            InputData.guest.GuestCategory_UID = 1;
            InputData.guest.Property_UID = _propertyId ?? 1263;
            InputData.guest.Currency_UID = 34;
            InputData.guest.Language_UID = languageUID.HasValue ? languageUID.Value : 4;
            InputData.guest.Email = "tony.santos@visualforma.pt";
            InputData.guest.Birthday = null;
            InputData.guest.CreatedByTPI_UID = null;
            InputData.guest.IsActive = true;
            InputData.guest.CreateDate = new DateTime(2014, 05, 14, 17, 36, 51);
            InputData.guest.LastLoginDate = null;
            InputData.guest.AllowMarketing = true;
            InputData.guest.CreateBy = null;
            InputData.guest.CreatedDate = null;
            InputData.guest.ModifyBy = null;
            InputData.guest.ModifyDate = null;
            InputData.guest.Question_UID = null;
            InputData.guest.IsFacebookFan = null;
            InputData.guest.IsTwitterFollower = null;
            InputData.guest.IDCardNumber = "12312312";
            InputData.guest.BillingState_UID = null;
            InputData.guest.State_UID = 2430214;
            InputData.guest.Client_UID = 646;
            InputData.guest.UseDifferentBillingInfo = false;
            InputData.guest.IsImportedFromExcel = false;
            InputData.guest.IsDeleted = false;
            InputData.guest.Gender = "M";

            // expected data
            ExpectedData.guest = (Guest)Cloner.Clone(InputData.guest);
            ExpectedData.guest.UID = 284630;
            var expectedEmail = new Test.Domain.CRM.PropertyQueue
            {
                PropertyEvent_UID = null,
                IsProcessed = false,
                TaskType_UID = 123151,
                IsProcessing = false,
                Retry = 0,
                ErrorList = null,
                LastProcessingDate = null,
                MailTo = "tony.santos@visualforma.pt",
                MailFrom = "notifications@protur.pt",
                MailSubject = "blablabla",
                MailBody = null,
                SystemEvent_UID = 39,
                SystemTemplate_UID = (long?)Constants.SystemTemplatesIDs.NewGuestTemplate,
                ChannelActivityErrorDateFrom = null,
                ChannelActivityErrorDateTo = null
            };
            ExpectedData.expectedEmailList.Add(expectedEmail);

            return this;
        }

        public ReservationDataBuilder WithExistingGuest(Guest guest, bool emptyAddress)
        {
            // input data
            InputData.guest.UID = guest.UID;
            InputData.guest.FirstName = "Teste";
            InputData.guest.LastName = guest.LastName;
            InputData.guest.Email = guest.Email;
            InputData.guest.Property_UID = guest.Property_UID;
            InputData.guest.Language_UID = guest.Language_UID;
            InputData.guest.UserName = guest.UserName;
            InputData.guest.GuestActivities = guest.GuestActivities;

            // expected data
            ExpectedData.guest = (Guest)Cloner.Clone(InputData.guest);
            ExpectedData.guest.UID = 63774;
            ExpectedData.guest.Prefix = 1;
            ExpectedData.guest.FirstName = "Teste";
            ExpectedData.guest.LastName = guest.LastName;
            ExpectedData.guest.Address1 = string.Empty;
            ExpectedData.guest.Address2 = string.Empty;
            ExpectedData.guest.City = string.Empty;
            ExpectedData.guest.PostalCode = string.Empty;
            ExpectedData.guest.BillingAddress1 = string.Empty;
            ExpectedData.guest.BillingAddress2 = string.Empty;
            ExpectedData.guest.BillingCity = string.Empty;
            ExpectedData.guest.BillingPostalCode = string.Empty;
            ExpectedData.guest.BillingPhone = string.Empty;

            if (!emptyAddress)
            {
                InputData.guest.Address1 = "Street";
                InputData.guest.Address2 = "Postal Box";
                InputData.guest.City = "City";
                InputData.guest.PostalCode = "Postal Code";

                ExpectedData.guest.Address1 = "Street";
                ExpectedData.guest.Address2 = "Postal Box";
                ExpectedData.guest.City = "City";
                ExpectedData.guest.PostalCode = "Postal Code";
            }
            else
            {
                ExpectedData.guest.Address1 = string.Empty;
                ExpectedData.guest.Address2 = string.Empty;
                ExpectedData.guest.City = string.Empty;
                ExpectedData.guest.PostalCode = string.Empty;
            }

            return this;
        }

        public ReservationDataBuilder WithCreditCardPayment()
        {
            // input data
            InputData.reservationPaymentDetail = new ReservationPaymentDetail();
            InputData.reservationPaymentDetail.UID = 0;
            InputData.reservationPaymentDetail.PaymentMethod_UID = 1;
            InputData.reservationPaymentDetail.Reservation_UID = 0;
            InputData.reservationPaymentDetail.Amount = 110;
            InputData.reservationPaymentDetail.Currency_UID = 34;
            InputData.reservationPaymentDetail.CVV = "HhG4qzNJhgB+pD4vgNoDEKKiXnupNgB7Wmt0feYArTjnQ/zK5w8PXRkojdFj9SLK+ogFUfDpiKaOEwZ36+96F7Be6ANd9AfFz4pLa/IRoqcdp5TCNW/ZPZjR7QYd+7LbjhQF1up0ousvWBovyHrdRu7mE4KHIO2/a3c3q/41w58=";
            InputData.reservationPaymentDetail.CardName = "aaa";
            InputData.reservationPaymentDetail.CardNumber = "ACUvivo7uKrw2x3kGyyLl3tMwwwngUwBQo0lxjFpmgL5h+pHhLB+A5F583xJsxAKS8sc0Y3QgoygjdopurE8IWa5X2Ih5dFExrbrMHjfwjreSCyoa6MtcUXtEo1rzaPzKT+jPRJ/TsLKj0UfaveAYiMUo8XagPhan6BaedUGu2w=";
            InputData.reservationPaymentDetail.ExpirationDate = new DateTime(2016, 01, 01);
            InputData.reservationPaymentDetail.CreatedDate = null;
            InputData.reservationPaymentDetail.ModifiedDate = null;
            InputData.reservationDetail.PaymentMethodType_UID = 1;

            // Hash Code
            var hashCode = JsonConvert.DeserializeObject<GetCreditCardHashResponse>("{\"CreditCardHashCode\": \"6Jtf2rgB0SP6I6DvllHXjjrkIq1dOzPgWbf8ucEjEY0=\",\"RequestGuid\": \"00000000-0000-0000-0000-000000000000\",\"Errors\": [],\"Warnings\": [],\"Status\": 2}");
            InputData.reservationPaymentDetail.HashCode = hashCode.CreditCardHashCode;

            // expected data
            ExpectedData.reservationPaymentDetail = (ReservationPaymentDetail)Cloner.Clone(InputData.reservationPaymentDetail);
            ExpectedData.reservationPaymentDetail.CardNumber = EncodeString.Encode(ExpectedData.reservationPaymentDetail.CardNumber);
            ExpectedData.reservationDetail.PaymentMethodType_UID = 1;


            return this;
        }

        public ReservationDataBuilder WithCreditCardPayment(ReservationPaymentDetail resPaymentDetail)
        {
            InputData.reservationPaymentDetail = resPaymentDetail.Clone();
            ExpectedData.reservationPaymentDetail = resPaymentDetail.Clone();

            return this;
        }

        public ReservationDataBuilder WithInvalidCreditCardPayment()
        {
            // input data
            InputData.reservationPaymentDetail = new ReservationPaymentDetail();
            InputData.reservationPaymentDetail.UID = 0;
            InputData.reservationPaymentDetail.PaymentMethod_UID = 1;
            InputData.reservationPaymentDetail.Reservation_UID = 0;
            InputData.reservationPaymentDetail.Amount = 110;
            InputData.reservationPaymentDetail.Currency_UID = 34;
            InputData.reservationPaymentDetail.CVV = "GUVY/JJNlTE04uAYVQG6OHqGYLalZbdns/roXhkIroxbmFLIDd3kz6KbC4fkOsLEx8SCZBWS01rtyhGCBb60gYahlNTxdcvcbSCHoI0fdB5EpgpI1OjOaC5GgpXgGGMT+FNyIGvEHp8JFZdFUX8wdr6UlwLuNDISNVvn0bhjN+0=";
            InputData.reservationPaymentDetail.CardName = "aaa";
            InputData.reservationPaymentDetail.CardNumber = "zIN+fFJ/lJF3xa9JL74zbPw8ZXHO/VIeyECIAac3J4nN3jkOtmaXFUw2YBzXyM69W6ycsfbwyp1YvAUf7w4GqbRdlobHOFlvxOZdC6dL42j84KH5wiwGGxNzSsWXlU4h0KzuhHbxdSGVM/84uIb3lsN9oW2tz0reTUl92NyIvk8=";
            InputData.reservationPaymentDetail.ExpirationDate = new DateTime(2016, 01, 01);
            InputData.reservationPaymentDetail.CreatedDate = null;
            InputData.reservationPaymentDetail.ModifiedDate = null;
            InputData.reservationDetail.PaymentMethodType_UID = 1;

            // expected data
            ExpectedData.reservationPaymentDetail = (ReservationPaymentDetail)Cloner.Clone(InputData.reservationPaymentDetail);
            ExpectedData.reservationPaymentDetail.CardNumber = EncodeString.Encode(ExpectedData.reservationPaymentDetail.CardNumber);
            ExpectedData.reservationDetail.PaymentMethodType_UID = 1;


            return this;
        }

        public ReservationDataBuilder WithExistingReservation(ReservationData reservation)
        {
            WithEmpty();



            // input data
            InputData.reservationDetail = (OB.Domain.Reservations.Reservation)Cloner.Clone(reservation.reservationDetail);
            InputData.guest = (Guest)Cloner.Clone(reservation.guest);
            InputData.reservationRooms = (List<ReservationRoom>)Cloner.Clone(reservation.reservationRooms);
            InputData.reservationRoomDetails = (List<ReservationRoomDetail>)Cloner.Clone(reservation.reservationRoomDetails);
            InputData.guestActivity = reservation.guestActivityObj != null && reservation.guestActivityObj.Any() ? reservation.guestActivityObj.Select(x => x.Activity_UID).ToList() : new List<long>();
            InputData.reservationRoomExtras = (List<ReservationRoomExtra>)Cloner.Clone(reservation.reservationRoomExtras);
            InputData.reservationRoomChild = (List<ReservationRoomChild>)Cloner.Clone(reservation.reservationRoomChild);
            InputData.reservationPaymentDetail = (ReservationPaymentDetail)Cloner.Clone(reservation.reservationPaymentDetail);
            InputData.reservationExtraSchedule = (List<ReservationRoomExtrasSchedule>)Cloner.Clone(reservation.reservationExtraSchedule);
            InputData.reservationRoomExtrasAvailableDates = (List<ReservationRoomExtrasAvailableDate>)Cloner.Clone(reservation.reservationRoomExtrasAvailableDates);
            if (InputData.reservationPaymentDetail != null)
                InputData.reservationPaymentDetail.CardNumber = EncodeString.Decode(InputData.reservationPaymentDetail.CardNumber);

            InputData.reservationDetail.UID = reservation.reservationDetail.UID;  //Added

            // expected data
            ExpectedData.reservationDetail = (OB.Domain.Reservations.Reservation)Cloner.Clone(reservation.reservationDetail);
            ExpectedData.guest = (Guest)Cloner.Clone(reservation.guest);
            ExpectedData.reservationRooms = (List<ReservationRoom>)Cloner.Clone(reservation.reservationRooms);
            ExpectedData.reservationRoomDetails = (List<ReservationRoomDetail>)Cloner.Clone(reservation.reservationRoomDetails);
            ExpectedData.guestActivityObj = (List<GuestActivity>)Cloner.Clone(reservation.guestActivityObj);
            ExpectedData.reservationRoomExtras = (List<ReservationRoomExtra>)Cloner.Clone(reservation.reservationRoomExtras);
            ExpectedData.reservationRoomChild = (List<ReservationRoomChild>)Cloner.Clone(reservation.reservationRoomChild);
            ExpectedData.reservationPaymentDetail = (ReservationPaymentDetail)Cloner.Clone(reservation.reservationPaymentDetail);
            ExpectedData.reservationExtraSchedule = (List<ReservationRoomExtrasSchedule>)Cloner.Clone(reservation.reservationExtraSchedule);
            ExpectedData.reservationRoomExtrasAvailableDates = (List<ReservationRoomExtrasAvailableDate>)Cloner.Clone(reservation.reservationRoomExtrasAvailableDates);
            ExpectedData.reservationDetail.InternalNotesHistory = "----------------------------------------------------;" + DateTime.UtcNow.Date.ToShortDateString();
            ExpectedData.reservationDetail.ModifyDate = now.Date;
            ExpectedData.reservationDetail.PropertyBaseCurrencyExchangeRate = 1;

            ExpectedData.reservationDetail.UID = reservation.reservationDetail.UID;  //Added

            return this;
        }

        public ReservationDataBuilder WithExtra(int roomIndex, bool isIncluded, short qty)
        {
            ReservationRoomExtra extra = new ReservationRoomExtra();
            extra.UID = InputData.reservationRoomExtras.Count + 1;
            extra.Extra_UID = 909;
            extra.ExtraIncluded = isIncluded;
            extra.Qty = qty;
            extra.ReservationRoom_UID = roomIndex;
            extra.Total_Price = 20;
            extra.Total_VAT = 6;
            InputData.reservationRoomExtras.Add(extra);

            // expected data
            ExpectedData.reservationRoomExtras.Add((ReservationRoomExtra)Cloner.Clone(extra));

            return this;
        }

        public ReservationDataBuilder WithExtra(IEnumerable<ReservationRoomExtra> extras)
        {
            InputData.reservationRoomExtras.AddRange(extras);
            ExpectedData.reservationRoomExtras.AddRange(extras);

            return this;
        }

        public ReservationDataBuilder WithExtraSchedule(int roomIndex, int extraIndex)
        {
            List<DateTime> schedule = new List<DateTime>() { new DateTime(2014, 06, 23) };

            foreach (DateTime date in schedule)
            {
                ReservationRoomExtrasSchedule extraSchedule = new ReservationRoomExtrasSchedule();
                extraSchedule.UID = roomIndex;
                extraSchedule.Date = date;
                extraSchedule.ReservationRoomExtra_UID = this.InputData.reservationRoomExtras.Where(x => x.UID == extraIndex).First().Extra_UID;
                InputData.reservationExtraSchedule.Add(extraSchedule);

                // expected data
                ExpectedData.reservationExtraSchedule.Add((ReservationRoomExtrasSchedule)Cloner.Clone(extraSchedule));
            }

            return this;
        }

        public ReservationDataBuilder WithExtraPeriod(RatesExtrasPeriod period)
        {
            ReservationRoomExtrasAvailableDate extraSchedule = new ReservationRoomExtrasAvailableDate
            {
                DateFrom = period.DateFrom,
                DateTo = period.DateTo
            };
            this.InputData.reservationRoomExtrasAvailableDates.Add(extraSchedule);

            // expected data
            ExpectedData.reservationRoomExtrasAvailableDates.Add((ReservationRoomExtrasAvailableDate)Cloner.Clone(extraSchedule));

            return this;
        }

        public ReservationDataBuilder WithChildren(int roomIndex, int numChildren, List<int> childAges)
        {
            for (int i = 0; i < numChildren; i++)
            {
                int? childAge = (childAges != null && childAges.Count <= numChildren) ? (int?)childAges[i] : null;  //was <

                ReservationRoomChild child = new ReservationRoomChild()
                {
                    Age = childAge,
                    ChildNo = i + 1,
                    ReservationRoom_UID = roomIndex,
                };
                InputData.reservationRoomChild.Add(child);

                // expected data
                ExpectedData.reservationRoomChild.Add((ReservationRoomChild)Cloner.Clone(child));
            }
            return this;
        }

        public ReservationDataBuilder WithChildren(IEnumerable<ReservationRoomChild> resRoomChild)
        {
            InputData.reservationRoomChild.AddRange(resRoomChild.Clone());
            ExpectedData.reservationRoomChild.AddRange(resRoomChild.Clone());

            return this;
        }

        private ReservationDataBuilder WithUserActivities()
        {
            //// guest activity
            //if (ExpectedData.guestActivityObj != null)
            //{
            //    List<GuestActivity> guestActivityExpectedResult = new List<GuestActivity>();
            //    foreach (long obj in ExpectedData.guestActivityObj)
            //    {
            //        GuestActivity expectedEntry = new GuestActivity();
            //        expectedEntry.Guest_UID = inputData.guest.UID;
            //        expectedEntry.Activity_UID = obj;
            //        guestActivityExpectedResult.Add(expectedEntry);
            //    }
            //    ExpectedData.guestActivityObj = guestActivityExpectedResult;
            //}

            return this;
        }

        public ReservationDataBuilder WithTPICompany(contractCRMOB.TPICustom tpi, contractCRMOB.TPICustom company)
        {
            this.InputData.reservationDetail.TPI_UID = tpi.UID;
            this.InputData.reservationDetail.Company_UID = company.Company_UID;
            this.ExpectedData.reservationDetail.TPI_UID = tpi.UID;
            this.ExpectedData.reservationDetail.Company_UID = company.Company_UID;
            return this;
        }

        public ReservationDataBuilder WithTPI(ThirdPartyIntermediary tpi)
        {
            this.InputData.reservationDetail.TPI_UID = tpi.UID;
            this.ExpectedData.reservationDetail.TPI_UID = tpi.UID;
            this.ExpectedData.reservationDetail.Company_UID = null;
            return this;
        }

        public ReservationDataBuilder WithTPICommission(decimal? commission)
        {
            foreach (var item in ExpectedData.reservationRooms)
            {
                item.CommissionType = commission != null ? 1 : new Nullable<long>();
                item.CommissionValue = commission;
            }
            return this;
        }

        public ReservationDataBuilder WithBilledTypePayment(long type)
        {
            InputData.reservationDetail.PaymentMethodType_UID = type;
            ExpectedData.reservationDetail.PaymentMethodType_UID = type;

            if (type == 7)
                ExpectedData.reservationDetail.IsPaid = true;

            return this;
        }

        public ReservationDataBuilder WithSalesmanComission(Salesman salesman, SalesmanThirdPartyIntermediariesComission comission)
        {
            this.InputData.reservationDetail.Salesman_UID = salesman.UID;
            this.InputData.reservationDetail.SalesmanCommission = comission.SalesmanBaseCommission;
            this.InputData.reservationDetail.SalesmanIsCommissionPercentage = comission.SalesmanIsBaseCommissionPercentage;

            this.ExpectedData.reservationDetail.Salesman_UID = salesman.UID;
            this.ExpectedData.reservationDetail.SalesmanCommission = comission.SalesmanBaseCommission;
            this.ExpectedData.reservationDetail.SalesmanIsCommissionPercentage = comission.SalesmanIsBaseCommissionPercentage;
            return this;
        }

        public ReservationDataBuilder WithMobile()
        {
            InputData.reservationDetail.IsMobile = true;
            ExpectedData.reservationDetail.IsMobile = true;
            return this;
        }

        public ReservationDataBuilder WithPromotionalCodes(PromotionalCode promoCode)
        {
            InputData.reservationDetail.PromotionalCode_UID = promoCode.UID;
            ExpectedData.reservationDetail.PromotionalCode_UID = promoCode.UID;
            return this;
        }

        public ReservationDataBuilder WithGuestActivity(GuestActivity activity)
        {
            InputData.guestActivity.Add(activity.UID);
            if (ExpectedData.guest != null && ExpectedData.guest.GuestActivities != null)
            {
                ExpectedData.guest.GuestActivities.Add(activity);
            }
            else if (ExpectedData.guest != null)
            {
                ExpectedData.guest.GuestActivities = new List<GuestActivity>() { activity };
            }

            return this;
        }

        public ReservationDataBuilder WithTPIPromotionalCodes()
        {
            throw new NotImplementedException();
        }

        public ReservationDataBuilder WithSpecialRequest(BESpecialRequest specialRequest1, BESpecialRequest specialRequest2, BESpecialRequest specialRequest3, BESpecialRequest specialRequest4)
        {
            InputData.reservationDetail.BESpecialRequests1_UID = specialRequest1.UID;
            InputData.reservationDetail.BESpecialRequests2_UID = specialRequest2.UID;
            InputData.reservationDetail.BESpecialRequests3_UID = specialRequest3.UID;
            InputData.reservationDetail.BESpecialRequests4_UID = specialRequest4.UID;

            ExpectedData.reservationDetail.BESpecialRequests1_UID = specialRequest1.UID;
            ExpectedData.reservationDetail.BESpecialRequests2_UID = specialRequest2.UID;
            ExpectedData.reservationDetail.BESpecialRequests3_UID = specialRequest3.UID;
            ExpectedData.reservationDetail.BESpecialRequests4_UID = specialRequest4.UID;

            return this;
        }

        public ReservationDataBuilder WithPartialPayment(BEPartialPaymentCCMethod partialPayment, decimal installmentAmount)
        {
            InputData.reservationDetail.IsPartialPayment = true;
            InputData.reservationDetail.InterestRate = partialPayment.InterestRate;
            InputData.reservationDetail.InstallmentAmount = installmentAmount;
            InputData.reservationDetail.NoOfInstallment = partialPayment.Parcel;

            ExpectedData.reservationDetail.IsPartialPayment = true;
            ExpectedData.reservationDetail.InterestRate = partialPayment.InterestRate;
            ExpectedData.reservationDetail.InstallmentAmount = InputData.reservationDetail.InstallmentAmount;
            ExpectedData.reservationDetail.NoOfInstallment = partialPayment.Parcel;

            return this;
        }

        public ReservationDataBuilder WithGroupCode(GroupCode groupCode)
        {
            InputData.reservationDetail.GroupCode_UID = groupCode.UID;
            ExpectedData.reservationDetail.GroupCode_UID = groupCode.UID;
            return this;
        }

        public ReservationDataBuilder WithTransferLocation(TransferLocation transferLocation)
        {
            InputData.reservationDetail.TransferLocation_UID = transferLocation.UID;
            ExpectedData.reservationDetail.TransferLocation_UID = transferLocation.UID;
            return this;
        }

        public ReservationDataBuilder WithBookingOnRequest()
        {
            InputData.reservationDetail.IsOnRequest = true;
            InputData.reservationDetail.Status = (long)Constants.ReservationStatus.BookingOnRequest;
            ExpectedData.reservationDetail.IsOnRequest = true;
            ExpectedData.reservationDetail.Status = (long)Constants.ReservationStatus.BookingOnRequest;
            return this;
        }

        public ReservationDataBuilder WithLanguage(int languageUID)
        {
            this.InputData.reservationDetail.ReservationLanguageUsed_UID = languageUID;
            this.ExpectedData.reservationDetail.ReservationLanguageUsed_UID = languageUID;
            return this;
        }

        public ReservationDataBuilder WithIncentives(int roomIndex)
        {
            int i = 0;
            var inputRRD = InputData.reservationRoomDetails.Where(rrd => rrd.ReservationRoom_UID == roomIndex);
            List<ReservationRoomDetail> expectedRRD = ExpectedData.reservationRoomDetails.Where(rrd => rrd.ReservationRoom_UID == roomIndex).ToList();
            foreach (var rrd in inputRRD)
            {
                ReservationRoomDetailsAppliedIncentive incentive = new ReservationRoomDetailsAppliedIncentive();
                //incentive.UID = rrd.ReservationRoomDetailsAppliedIncentives.Count + 1;
                incentive.Incentive_UID = 423;
                incentive.IsFreeDaysAtBegin = true;
                incentive.Name = "Incentive Test";
                incentive.ReservationRoomDetails_UID = rrd.UID;
                rrd.ReservationRoomDetailsAppliedIncentives.Add(incentive);

                // expected data
                expectedRRD[i].ReservationRoomDetailsAppliedIncentives.Add((ReservationRoomDetailsAppliedIncentive)Cloner.Clone(incentive));
                i++;
            }

            return this;


        }


        public Reservation.BL.Contracts.Data.Reservations.Reservation GetReservationsContract(ReservationDataBuilder builder)
        {
            var res = new Reservation.BL.Contracts.Data.Reservations.Reservation();
            if (builder != null)
            {
                var resDomain = builder.InputData.reservationDetail;
                resDomain.ReservationRooms = builder.InputData.reservationRooms;
                if (builder.InputData.reservationPaymentDetail != null)
                    resDomain.ReservationPaymentDetails = new List<ReservationPaymentDetail>
                    {
                        builder.InputData.reservationPaymentDetail
                    };

                foreach (var room in resDomain.ReservationRooms)
                {
                    var roomBuilder = builder.InputData.reservationRooms.FirstOrDefault(x => x.UID == room.UID);
                    if (roomBuilder == null)
                        continue;

                    room.ReservationRoomDetails = roomBuilder.ReservationRoomDetails;
                    room.ReservationRoomExtras = roomBuilder.ReservationRoomExtras;
                    room.ReservationRoomChilds = roomBuilder.ReservationRoomChilds;
                }

                res = DomainToBusinessObjectTypeConverter.Convert(resDomain,
                    new Reservation.BL.Contracts.Requests.ListReservationRequest
                    {
                        IncludeBESpecialRequests = true,
                        IncludeCancelationCosts = true,
                        IncludeExtras = true,
                        IncludeExtrasBillingTypes = true,
                        IncludeGroupCodes = true,
                        IncludeGuestActivities = true,
                        IncludeGuests = true,
                        IncludeIncentives = true,
                        IncludePromotionalCodes = true,
                        IncludeRates = true,
                        IncludeReservationAddicionalData = true,
                        IncludeReservationBaseCurrency = true,
                        IncludeReservationRooms = true,
                        IncludeRoomTypes = true,
                        IncludeReservationPaymentDetail = builder.InputData.reservationPaymentDetail != null,
                        IncludeReservationRoomDetails = true
                    });

            }

            return res;
        }

        public long InsertReservation(ReservationDataBuilder state, IUnityContainer Container, IReservationManagerPOCO ReservationManagerPOCO, bool handleCancelationCosts = true,
            bool handleDepositCosts = true, bool validate = true, bool validateAllotment = true, bool validateGuarantee = false, string transactionId = "",
            GroupRule groupRule = null, bool skipInterestCalculation = false, bool ignoreAvailability = false)
        {
            var builder = state;//s as ReservationDataBuilder;

            long result = -1;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method
                    ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();

                    var extrasList = (!builder.InputData.reservationExtraSchedule.Any() ? new List<contractsReservations.ReservationRoomExtrasSchedule>() :
                        builder.InputData.reservationExtraSchedule != null ? builder.InputData.reservationExtraSchedule.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null);

                    result = ReservationManagerPOCO.InsertReservation(new Reservation.BL.Contracts.Requests.InsertReservationRequest
                    {
                        Guest = builder.InputData.guest,
                        Reservation = builder.InputData.reservationDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail) : null,
                        GuestActivities = builder.InputData.guestActivity,
                        ReservationRooms = builder.InputData.reservationRooms != null ? builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                        ReservationRoomDetails = builder.InputData.reservationRoomDetails != null ? builder.InputData.reservationRoomDetails.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                        ReservationRoomExtras = builder.InputData.reservationRoomExtras != null ? builder.InputData.reservationRoomExtras.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                        ReservationRoomChilds = builder.InputData.reservationRoomChild != null ? builder.InputData.reservationRoomChild.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                        ReservationPaymentDetail = builder.InputData.reservationPaymentDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail) : null,
                        ReservationExtraSchedules = extrasList,
                        HandleCancelationCost = handleCancelationCosts,
                        HandleDepositCost = handleDepositCosts,
                        ValidateAllotment = validateAllotment,
                        ReservationsAdditionalData = builder.InputData.reservationsAdditionalData,
                        ValidateGuarantee = validateGuarantee,
                        TransactionId = transactionId,
                        UsePaymentGateway = true,
                        Version = null,
                        RuleType = Convert(groupRule?.RuleType),
                        SkipInterestCalculation = skipInterestCalculation,
                        IgnoreAvailability = ignoreAvailability
                    }, groupRule, validate).UID;

                    ReservationManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();

            if (ex != null)
                ExceptionDispatchInfo.Capture(ex).Throw();

            //return task.Result;
            return result;
        }

        public OB.Reservation.BL.Contracts.Responses.InsertReservationResponse InsertReservationV2(ReservationDataBuilder state, IUnityContainer Container, IReservationManagerPOCO ReservationManagerPOCO,
            bool handleCancelationCosts = true, bool validateAllotment = true, string transactionId = "", GroupRule groupRule = null, bool skipInterestCalculation = false)
        {
            var builder = state;//s as ReservationDataBuilder;

            var result = new Reservation.BL.Contracts.Responses.InsertReservationResponse();
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            
                // call the method
                ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();

                var extrasList = (!builder.InputData.reservationExtraSchedule.Any() ? new List<contractsReservations.ReservationRoomExtrasSchedule>() :
                    builder.InputData.reservationExtraSchedule != null ? builder.InputData.reservationExtraSchedule.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null);

                result = ReservationManagerPOCO.InsertReservation(new Reservation.BL.Contracts.Requests.InsertReservationRequest
                {
                    Guest = builder.InputData.guest,
                    Reservation = builder.InputData.reservationDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail) : null,
                    GuestActivities = builder.InputData.guestActivity,
                    ReservationRooms = builder.InputData.reservationRooms != null ? builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                    ReservationRoomDetails = builder.InputData.reservationRoomDetails != null ? builder.InputData.reservationRoomDetails.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                    ReservationRoomExtras = builder.InputData.reservationRoomExtras != null ? builder.InputData.reservationRoomExtras.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                    ReservationRoomChilds = builder.InputData.reservationRoomChild != null ? builder.InputData.reservationRoomChild.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                    ReservationPaymentDetail = builder.InputData.reservationPaymentDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail) : null,
                    ReservationExtraSchedules = extrasList,
                    HandleCancelationCost = handleCancelationCosts,
                    HandleDepositCost = true,
                    ValidateAllotment = validateAllotment,
                    ReservationsAdditionalData = builder.InputData.reservationsAdditionalData,
                    ValidateGuarantee = false,
                    TransactionId = transactionId,
                    UsePaymentGateway = true,
                    Version = null,
                    RuleType = Convert(groupRule?.RuleType),
                    SkipInterestCalculation = skipInterestCalculation
                });               

            //return task.Result;
            return result;
        }

        public Reservation.BL.Contracts.Responses.ListReservationResponse GetReservation(IReservationManagerPOCO ReservationManagerPOCO, long reservationId)
        {
            if (reservationId < 0)
                return null;

            var reservationRequest = new Reservation.BL.Contracts.Requests.ListReservationRequest();
            reservationRequest.ReservationUIDs = new List<long>() { reservationId };
            reservationRequest.IncludeReservationRoomChilds = true;
            reservationRequest.IncludeReservationRoomDetails = true;
            reservationRequest.IncludeReservationRoomDetailsAppliedIncentives = true;
            reservationRequest.IncludeReservationRoomExtras = true;
            reservationRequest.IncludeReservationRooms = true;
            reservationRequest.IncludeReservationRoomTaxPolicies = true;
            reservationRequest.IncludeReservationPaymentDetail = true;
            reservationRequest.IncludeReservationPartialPaymentDetails = true;
            reservationRequest.IncludeReservationRoomExtrasSchedules = true;
            reservationRequest.IncludeReservationAddicionalData = true;
            reservationRequest.IncludeRoomTypes = true;
            reservationRequest.IncludeRates = true;
            reservationRequest.IncludeExtras = true;
            reservationRequest.IncludeGuests = true;
            reservationRequest.IncludeIncentives = true;
            reservationRequest.IncludeReservationResumeInfo = true;
            reservationRequest.IncludeReservationCurrency = true;
            reservationRequest.IncludePropertyBaseCurrency = true;
            reservationRequest.IncludeReservationBaseCurrency = true;
            reservationRequest.IncludeChannelOperator = true;
            reservationRequest.IncludeGuestActivities = true;
            reservationRequest.IncludeReservationStatusName = true;
            reservationRequest.IncludeReferralSource = true;
            reservationRequest.IncludeChannel = true;
            reservationRequest.IncludeReservationReadStatus = true;
            reservationRequest.IncludeTPIName = true;
            reservationRequest.IncludePaymentMethodType = true;
            reservationRequest.IncludeExternalSource = true;
            reservationRequest.IncludeCompanyName = true;
            reservationRequest.IncludeCommissionTypeName = true;
            reservationRequest.IncludeTPICommissions = true;
            reservationRequest.IncludeGuestCountryName = true;
            reservationRequest.IncludeGuestStateName = true;
            reservationRequest.IncludeGuestPrefixName = true;
            reservationRequest.IncludeTransferLocation = true;
            reservationRequest.IncludePromotionalCodes = true;
            reservationRequest.IncludeReservationRoomDetailsAppliedPromotionalCode = true;
            reservationRequest.IncludeGroupCodes = true;
            reservationRequest.IncludeBillingCountryName = true;
            reservationRequest.IncludeBillingStateName = true;
            reservationRequest.IncludeExtrasBillingTypes = true;
            reservationRequest.IncludeReservationRoomExtrasAvailableDates = true;
            reservationRequest.IncludeTaxPolicies = true;
            reservationRequest.IncludeBESpecialRequests = true;
            reservationRequest.IncludeCancelationCosts = true;
            reservationRequest.IncludeTPILanguageUID = true;
            reservationRequest.IncludeOnRequestDecisionUser = true;
            reservationRequest.IncludePaymentGatewayTransactions = true;
            reservationRequest.IncludeReservationRoomIncentivePeriods = true;
            reservationRequest.RuleType = Reservation.BL.Constants.RuleType.Omnibees; //Added to work in the ListReservations

            return ReservationManagerPOCO.ListReservations(reservationRequest);
        }

        private Reservation.BL.Constants.RuleType Convert(OB.Domain.Reservations.RuleType? type)
        {
            if(!type.HasValue)
                return Reservation.BL.Constants.RuleType.Push;

            switch (type)
            {
                case RuleType.Push: return Reservation.BL.Constants.RuleType.Push;
                case RuleType.GDS: return Reservation.BL.Constants.RuleType.GDS;
                case RuleType.Pull: return Reservation.BL.Constants.RuleType.Pull;
                case RuleType.PMS: return Reservation.BL.Constants.RuleType.PMS;
                case RuleType.BE: return Reservation.BL.Constants.RuleType.BE;
                case RuleType.Omnibees: return Reservation.BL.Constants.RuleType.Omnibees;
                case RuleType.PortalOperadoras: return Reservation.BL.Constants.RuleType.PortalOperadoras;
                case RuleType.BEAPI: return Reservation.BL.Constants.RuleType.BEAPI;
                default: return Reservation.BL.Constants.RuleType.Push;
            }
        }
    }

    //TODO: the class above can be adapted and used on the unit tests in ReservationControllerTest
    public class ReservationBuilder
    {
        public Reservation.BL.Contracts.Data.Reservations.Reservation reservationDetail;
        public Guest guest;
        public List<contractsReservations.ReservationRoom> reservationRooms;
        public List<contractsReservations.ReservationRoomDetail> reservationRoomDetails;
        public List<long> guestActivity;
        public List<contractsReservations.ReservationRoomExtra> reservationRoomExtras;
        public List<contractsReservations.ReservationRoomChild> reservationRoomChild;
        public contractsReservations.ReservationPaymentDetail reservationPaymentDetail;
        public List<contractsReservations.ReservationRoomExtrasSchedule> reservationExtraSchedule;

        public ReservationBuilder()
        {
            reservationDetail = new Reservation.BL.Contracts.Data.Reservations.Reservation();
            guest = new Guest();
            reservationRooms = new List<contractsReservations.ReservationRoom>();
            reservationRoomDetails = new List<contractsReservations.ReservationRoomDetail>();
            guestActivity = new List<long>();
            reservationRoomExtras = new List<contractsReservations.ReservationRoomExtra>();
            reservationRoomChild = new List<contractsReservations.ReservationRoomChild>();
            reservationPaymentDetail = new contractsReservations.ReservationPaymentDetail();
            reservationExtraSchedule = new List<contractsReservations.ReservationRoomExtrasSchedule>();

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
            reservationRooms.Add(new contractsReservations.ReservationRoom
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
            reservationRoomDetails.Add(new contractsReservations.ReservationRoomDetail
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
