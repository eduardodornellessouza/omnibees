using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Interfaces;
using OB.Domain.Reservations;
using Ploeh.SemanticComparison;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OB.DL.Common;
using OB.BL.Contracts.Data.CRM;
using OB.BL.Operations.Test.Domain.CRM;

namespace OB.BL.Operations.Helper
{
    [Serializable]
    public class InsertReservationBuilder
    {        
        public OB.Domain.Reservations.Reservation reservationDetail;
        public Guest guest;
        public List<ReservationRoom> reservationRooms;
        public List<ReservationRoomDetail> reservationRoomDetails;
        public List<long> guestActivity;
        public List<ReservationRoomExtra> reservationRoomExtras;
        public List<ReservationRoomChild> reservationRoomChild;
        public ReservationPaymentDetail reservationPaymentDetail;
        public List<ReservationRoomExtrasSchedule> reservationExtraSchedule;


        protected IUnityContainer Container
        {
            get;
            set;
        }


        public InsertReservationBuilder(IUnityContainer container)
        {
            this.Container = container;

            // initialize
            reservationDetail = new OB.Domain.Reservations.Reservation();
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

            DateTime now = DateTime.UtcNow;
            reservationDetail.CreatedDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            
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

        public InsertReservationBuilder Clone()
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(this, null))
            {
                return default(InsertReservationBuilder);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (InsertReservationBuilder)formatter.Deserialize(stream);
            }
        }

        public InsertReservationBuilder Empty()
        {
            reservationDetail = new OB.Domain.Reservations.Reservation();
            reservationRooms = new List<ReservationRoom>();
            reservationRoomDetails = new List<ReservationRoomDetail>();
            guest = new Guest();
            guestActivity = new List<long>();
            reservationRoomExtras = new List<ReservationRoomExtra>();
            reservationRoomChild = new List<ReservationRoomChild>();
            reservationPaymentDetail = new ReservationPaymentDetail();
            reservationExtraSchedule = new List<ReservationRoomExtrasSchedule>();
            return this;
        }

        public InsertReservationBuilder WithExistingReservation()
        {		
            // reservation object
            reservationDetail.Channel_UID = 28;
            reservationDetail.Number = "Orbitz-CJ1SVI";

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
                CancellationNrNights = null,
                ReservationRoomNo = "Orbitz-CJ1SVI/1"
            });

            return this;
        }

        public InsertReservationBuilder WithChannel(long? channelUID)
        {
            reservationDetail.Channel_UID = channelUID;
            reservationDetail.Number = "91847109382";
            return this;
        }

        public InsertReservationBuilder WithChannelBookingEngine()
        {
            reservationDetail.Channel_UID = 1;
            return this;
        }

        public InsertReservationBuilder WithChannelPull()
        {
            reservationDetail.Channel_UID = 32;
            reservationDetail.Number = "91847109382";
            return this;
        }

        public InsertReservationBuilder WithChannelSabre()
        {
            reservationDetail.Channel_UID = 56;
            return this;
        }

        public InsertReservationBuilder WithNewGuest()
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
            guest.Property_UID = 1263;
            guest.Currency_UID = 34;
            guest.Language_UID = 4;
            guest.Email = "tony.santos@visualforma.pt";
            guest.Birthday = null;
            guest.CreatedByTPI_UID = null;
            guest.IsActive = true;
            guest.CreateDate = new DateTime(2014, 05, 14, 17, 36, 51);
            guest.LastLoginDate = null;
            guest.AllowMarketing = true;
            guest.CreateBy = null;
            guest.CreatedDate = null;
            guest.ModifyBy = null;
            guest.ModifyDate = null;
            guest.Question_UID = null;
            guest.IsFacebookFan = null;
            guest.IsTwitterFollower = null;
            guest.IDCardNumber = "12312312";
            guest.BillingState_UID = null;
            guest.State_UID = 2430214;
            guest.Client_UID = 646;
            guest.UseDifferentBillingInfo = false;
            guest.IsImportedFromExcel = false;
            guest.IsDeleted = false;
            guest.Gender = "M";
            return this;
        }

        //public InsertReservationBuilder WithExistingGuest()
        //{         
        //    using (var unitOfWork = this.Container.Resolve<ISessionFactory>().GetUnitOfWork())
        //    {
                
        //        var guestRepository = this.Container.Resolve<IRepositoryFactory>().GetGuestRepository(unitOfWork);

        //        guest.FirstName = "Anne-Birgitte";
        //        guest.LastName = "Ritz";
        //        guest.Email = "testemail@email.com";
        //        guest.Property_UID = 1263;
        //        guest.Language_UID = 4;
        //        guest.UserName = "testemail@email.com";

        //        // setup db data
        //        var guestDB = guestRepository.Get(63774);
        //        guestDB.Email = guest.Email;
        //        guestDB.UserName = guest.UserName;
        //        unitOfWork.Save();
        //    }
                     
        //    return this;
        //}

        public InsertReservationBuilder AddRoom(DateTime checkIn, DateTime checkout)
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
                CancellationNrNights = null,
                ReservationRoomNo = reservationDetail.Channel_UID == 1 ? null : "blablabla"
            });

            return this;
        }

        public InsertReservationBuilder WithOneDay()
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

        public InsertReservationBuilder WithCreditCardPayment()
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

    public class InsertReservationAsserter : IDisposable
    {
        
        protected IUnityContainer Container { get; set; }

        public InsertReservationAsserter(IUnityContainer container)
        {
            this.Container = container;
        }

        #region asserts - data
        //public void AssertValidReservation(InsertReservationBuilder inputData, long output)
        //{
        //    var unitOfWork = this.Container.Resolve<ISessionFactory>().GetUnitOfWork();
        //    var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();

        //    var reservationRepo = repositoryFactory.GetReservationsRepository(unitOfWork);
        //    var guestActivityRepo = repositoryFactory.GetGuestActivityRepository(unitOfWork);
        //    var reservationPaymentDetailRepo = repositoryFactory.GetRepository<ReservationPaymentDetail>(unitOfWork);
        //    var guestRepo = repositoryFactory.GetGuestRepository(unitOfWork);
        //    var reservationRoomRepo = repositoryFactory.GetRepository<ReservationRoom>(unitOfWork);

        //    // get objects from database
        //    OB.Domain.Reservations.Reservation reservationDetailDB = reservationRepo.GetQuery(r => r.UID == output).Single();
        //    Guest guestDB = guestRepo.GetQuery(g => g.UID == reservationDetailDB.Guest_UID).Single();
        //    List<ReservationRoom> reservationRoomsDB = reservationDetailDB.ReservationRooms.ToList();
        //    List<ReservationRoomDetail> reservationRoomDetailsDB = reservationDetailDB.ReservationRooms.SelectMany(rr => rr.ReservationRoomDetails).ToList();
        //    List<GuestActivity> guestActivityDB = guestActivityRepo.GetQuery(ga => ga.Guest_UID == reservationDetailDB.Guest_UID).ToList();
        //    List<ReservationRoomExtra> reservationRoomExtrasDB = reservationDetailDB.ReservationRooms.SelectMany(rr => rr.ReservationRoomExtras).ToList();
        //    List<ReservationRoomChild> reservationRoomChildDB = reservationDetailDB.ReservationRooms.SelectMany(rr => rr.ReservationRoomChilds).ToList();
        //    ReservationPaymentDetail reservationPaymentDetailDB = reservationPaymentDetailRepo.GetQuery(rpd => rpd.Reservation_UID == output).Single();
        //    List<ReservationRoomExtrasSchedule> reservationExtraScheduleDB = reservationRoomExtrasDB.SelectMany(rre => rre.ReservationRoomExtrasSchedules).ToList();

        //    #region expected results
        //    for (int i = 0; i < inputData.reservationRooms.Count; i++)
        //    {
        //        var room = inputData.reservationRooms.ElementAt(i);

        //        if (!room.CancellationPolicyDays.HasValue)
        //            room.CancellationPolicyDays = 0;

        //        if (string.IsNullOrEmpty(room.ReservationRoomNo))
        //            room.ReservationRoomNo = reservationDetailDB.Number + "/" + (i + 1);

        //        if (!room.IsCancellationAllowed.HasValue)
        //            room.IsCancellationAllowed = true;
        //    }
        //    if (inputData.reservationPaymentDetail != null)
        //        inputData.reservationPaymentDetail.CardNumber = EncodeString.Encode(inputData.reservationPaymentDetail.CardNumber);

        //    List<GuestActivity> guestActivityExpectedResult = new List<GuestActivity>();
        //    foreach (long obj in inputData.guestActivity)
        //    {
        //        GuestActivity expectedEntry = new GuestActivity();
        //        expectedEntry.Guest_UID = inputData.guest.UID;
        //        expectedEntry.Activity_UID = obj;
        //        guestActivityExpectedResult.Add(expectedEntry);
        //    }

        //    if(inputData.reservationDetail.Channel_UID > 1)
        //        inputData.reservationDetail.InternalNotesHistory = reservationDetailDB.CreatedDate + ";";
        //    #endregion

        //    // asserts
        //    AssertReservationDetail(inputData.reservationDetail, reservationDetailDB);
        //    AssertGuest(inputData.guest, guestDB);
        //    AssertReservationRooms(inputData.reservationRooms, reservationRoomsDB);
        //    AssertReservationRoomDetails(inputData.reservationRoomDetails, reservationRoomDetailsDB);
        //    AssertGuestActivity(guestActivityExpectedResult, guestActivityDB);
        //    AssertReservationRoomExtra(inputData.reservationRoomExtras, reservationRoomExtrasDB);
        //    AssertReservationRoomChild(inputData.reservationRoomChild, reservationRoomChildDB);
        //    AssertReservationPaymentDetail(inputData.reservationPaymentDetail, reservationPaymentDetailDB);
        //    AssertReservationRoomExtrasSchedule(inputData.reservationExtraSchedule, reservationExtraScheduleDB);
        //}

        public void AssertReservationDetail(OB.Domain.Reservations.Reservation expectedResult, OB.Domain.Reservations.Reservation result)
        {
            var comparer = new Likeness<OB.Domain.Reservations.Reservation, OB.Domain.Reservations.Reservation>(result);

            if (result.ModifyDate.HasValue)
            {
                comparer = comparer.Without(x => x.ModifyDate);
                comparer = comparer.Without(x => x.InternalNotesHistory);
                Assert.IsTrue(result.InternalNotesHistory.Contains(result.ModifyDate + ""));
            }

            if (expectedResult.Channel_UID == 1)
                comparer = comparer.Without(x => x.Number);
            else
                comparer = comparer.Without(x => x.CreatedDate);

            comparer = comparer
                .Without(x => x.UID)
                .Without(x => x.Guest_UID)
                .Without(x => x.TransferLocation_UID)
                .Without(x => x.BESpecialRequests1_UID)
                .Without(x => x.BESpecialRequests2_UID)
                .Without(x => x.BESpecialRequests3_UID)
                .Without(x => x.BESpecialRequests4_UID)
                .Without(x => x.ReservationPartialPaymentDetails)
                .Without(x => x.ReservationPaymentDetails)
                .Without(x => x.ReservationRooms);

            Assert.IsTrue(result.CreatedDate.HasValue);
            Assert.IsTrue(!string.IsNullOrEmpty(result.Number));
            comparer.ShouldEqual(expectedResult);
        }

        public void AssertGuest(Guest expectedResult, Guest result)
        {
            if (expectedResult.UID == 0)
            {
                var comparer = new Likeness<Guest, Guest>(result);
                comparer
                    .Without(x => x.UID)
                    .Without(x => x.UserPassword)
                    .Without(x => x.CreateDate)
                    .Without(x => x.GuestActivities)
                    .Without(x => x.GuestCategory_UID)
                    .Without(x => x.GuestFavoriteExtras)
                    .Without(x => x.GuestFavoriteSpecialRequests)
                    .ShouldEqual(expectedResult);
            }
            else
            {
                var comparer = new Likeness<Guest, Guest>(result).OmitAutoComparison();
                comparer
                    .WithDefaultEquality(x => x.FirstName)
                    .WithDefaultEquality(x => x.LastName)
                    .WithDefaultEquality(x => x.Email)
                    .ShouldEqual(expectedResult);
            }
        }

        public void AssertReservationRooms(List<ReservationRoom> expectedResult, List<ReservationRoom> result)
        {
            var comparer = new Likeness<List<ReservationRoom>, List<ReservationRoom>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                ReservationRoom expectedEntry = expectedResult.ElementAt(i);
                ReservationRoom entry = result.ElementAt(i);                

                var entryComparer = new Likeness<ReservationRoom, ReservationRoom>(entry);
                entryComparer
                    .Without(x => x.UID)
                    .Without(x => x.Reservation_UID)
                    .Without(x => x.CreatedDate)
                    .Without(x => x.ModifiedDate)
                    .Without(x => x.ReservationRoomChilds)
                    .Without(x => x.ReservationRoomDetails)
                    .Without(x => x.ReservationRoomExtras)
                    .Without(x => x.ReservationRoomTaxPolicies)
                    .Without(x => x.Reservation)
                    .ShouldEqual(expectedEntry);
            }
        }

        public void AssertReservationRoomDetails(List<ReservationRoomDetail> expectedResult, List<ReservationRoomDetail> result)
        {
            var comparer = new Likeness<List<ReservationRoomDetail>, List<ReservationRoomDetail>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                ReservationRoomDetail expectedEntry = expectedResult.ElementAt(i);
                ReservationRoomDetail entry = result.ElementAt(i);

                var entryComparer = new Likeness<ReservationRoomDetail, ReservationRoomDetail>(entry);
                entryComparer
                        .Without(x => x.UID)
                        .Without(x => x.ReservationRoom_UID)
                        .Without(x => x.CreatedDate)
                        .Without(x => x.ReservationRoom)
                        .Without(x => x.ReservationRoomDetailsAppliedIncentives)
                        .ShouldEqual(expectedEntry);
            }
        }

        public void AssertReservationRoomExtrasSchedule(List<ReservationRoomExtrasSchedule> expectedResult, List<ReservationRoomExtrasSchedule> result)
        {
            var comparer = new Likeness<List<ReservationRoomExtrasSchedule>, List<ReservationRoomExtrasSchedule>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                ReservationRoomExtrasSchedule expectedEntry = expectedResult.ElementAt(i);
                ReservationRoomExtrasSchedule entry = result.ElementAt(i);

                var entryComparer = new Likeness<ReservationRoomExtrasSchedule, ReservationRoomExtrasSchedule>(entry);
                entryComparer
                    .ShouldEqual(expectedEntry);
            }
        }

        public void AssertReservationPaymentDetail(ReservationPaymentDetail expectedResult, ReservationPaymentDetail result)
        {
            var comparer = new Likeness<ReservationPaymentDetail, ReservationPaymentDetail>(result);
            comparer
                .Without(x => x.UID)
                .Without(x => x.Reservation_UID)
                .Without(x => x.Reservation)
                .Without(x => x.CreatedDate)
                .ShouldEqual(expectedResult);
        }

        public void AssertReservationRoomChild(List<ReservationRoomChild> expectedResult, List<ReservationRoomChild> result)
        {
            var comparer = new Likeness<List<ReservationRoomChild>, List<ReservationRoomChild>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                ReservationRoomChild expectedEntry = expectedResult.ElementAt(i);
                ReservationRoomChild entry = result.ElementAt(i);

                var entryComparer = new Likeness<ReservationRoomChild, ReservationRoomChild>(entry);
                entryComparer
                    .ShouldEqual(expectedEntry);
            }
        }

        public void AssertReservationRoomExtra(List<ReservationRoomExtra> expectedResult, List<ReservationRoomExtra> result)
        {
            var comparer = new Likeness<List<ReservationRoomExtra>, List<ReservationRoomExtra>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                ReservationRoomExtra expectedEntry = expectedResult.ElementAt(i);
                ReservationRoomExtra entry = result.ElementAt(i);

                var entryComparer = new Likeness<ReservationRoomExtra, ReservationRoomExtra>(entry);
                entryComparer
                    .ShouldEqual(expectedEntry);
            }
        }

        public void AssertGuestActivity(List<GuestActivity> expectedResult, List<GuestActivity> result)
        {
            var comparer = new Likeness<List<GuestActivity>, List<GuestActivity>>(result);
            comparer
                .Without(x => x.Capacity)
                .ShouldEqual(expectedResult);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                GuestActivity expectedEntry = expectedResult.ElementAt(i);
                GuestActivity entry = result.ElementAt(i);

                var entryComparer = new Likeness<GuestActivity, GuestActivity>(entry);
                entryComparer
                    .ShouldEqual(expectedEntry);
            }
        }
        #endregion

        #region asserts - async actions
        //public void AssertValidAsyncCalls(InsertReservationBuilder inputData, long output)
        //{
        //    using (  var unitOfWork = this.Container.Resolve<ISessionFactory>().GetUnitOfWork())
        //    {
              
        //        var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();

        //        var reservationRepo = repositoryFactory.GetReservationsRepository(unitOfWork);
        //        var guestActivityRepo = repositoryFactory.GetGuestActivityRepository(unitOfWork);
        //        var reservationPaymentDetailRepo = repositoryFactory.GetRepository<ReservationPaymentDetail>(unitOfWork);
        //        var guestRepo = repositoryFactory.GetGuestRepository(unitOfWork);
        //        var reservationRoomRepo = repositoryFactory.GetRepository<ReservationRoom>(unitOfWork);


        //        OB.Domain.Reservations.Reservation reservationDetailDB = reservationRepo.GetQuery(r => r.UID == output).Single();

        //        // wait for all async calls to execute
        //        Thread.Sleep(5000);

        //        // check reservation emails
        //        AssertValidReservationEmails(reservationDetailDB);

        //        // check guest emails
        //        if(inputData.guest.UID == 0)
        //            AssertValidNewGuestEmail(reservationDetailDB);

        //        // check extras emails

        //        // check close sales

        //        // check notify connectors

        //        // check tpi credit limit

        //        // check log

        //        // check occ levels

        //        // check real allotment
        //    }
        //}

        //private void AssertValidNewGuestEmail(OB.Domain.Reservations.Reservation reservationDetailDB)
        //{

        //    var unitOfWork = this.Container.Resolve<ISessionFactory>().GetUnitOfWork();
        //    var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();

        //    var guestRepo = repositoryFactory.GetGuestRepository(unitOfWork);
        //    var propertyQueueRepo = repositoryFactory.GetRepository<PropertyQueue>(unitOfWork);
        //    var systemTemplateLanguageRepo = repositoryFactory.GetRepository<SystemTemplatesLanguage>(unitOfWork);

        //    Guest guest = guestRepo.Get(reservationDetailDB.Guest_UID);

        //    // get gest property queue email
        //    var insertedEmail = propertyQueueRepo.GetQuery(pq => pq.TaskType_UID == guest.UID).Single();

        //    // to get the subject
        //    string subject = null;
        //    var systemTemplate = systemTemplateLanguageRepo.FirstOrDefault(x => x.SystemTemplate_UID == (int)Constants.SystemTemplatesIDs.NewGuestTemplate
        //        && x.Language_UID == guest.Language_UID);
        //    if (systemTemplate != null)
        //        subject = systemTemplate.EmailSubject;
        //    else
        //        subject = Resources.Resources.NewGuestDetailMailSubject;

        //    // expected data
        //    var expectedEmail = new PropertyQueue
        //    {
        //        PropertyEvent_UID = null,
        //        IsProcessed = false,
        //        TaskType_UID = guest.UID,
        //        IsProcessing = false,
        //        Retry = 0,
        //        ErrorList = null,
        //        LastProcessingDate = null,
        //        MailTo = guest.Email,
        //        MailFrom = "notifications@protur.pt",
        //        MailSubject = subject,
        //        MailBody = null,
        //        SystemEvent_UID = 39,
        //        SystemTemplate_UID = (long?)Constants.SystemTemplatesIDs.NewGuestTemplate,
        //        ChannelActivityErrorDateFrom = null,
        //        ChannelActivityErrorDateTo = null
        //    };

        //    var comparer = new Likeness<PropertyQueue, PropertyQueue>(insertedEmail);
        //    comparer
        //        .Without(x => x.UID)
        //        .Without(x => x.Date)
        //        .Without(x => x.Property_UID)
        //        .Without(x => x.MailFrom)
        //        .ShouldEqual(expectedEmail);

        //    //TODO !guest birthday
        //}

        //private void AssertValidReservationEmails(OB.Domain.Reservations.Reservation result)
        //{
        //    var unitOfWork = this.Container.Resolve<ISessionFactory>().GetUnitOfWork();
        //    var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();

        //    var propertyQueueRepo = repositoryFactory.GetRepository<PropertyQueue>(unitOfWork);
        //    var propertyEventRepo = repositoryFactory.GetRepository<PropertyEvent>(unitOfWork);
        //    var systemTemplateLanguageRepo = repositoryFactory.GetRepository<SystemTemplatesLanguage>(unitOfWork);


        //    // get all configured emails by property
        //    var configuredEmails = propertyEventRepo.GetQuery(pe => 
        //                                pe.Property_UID == result.Property_UID &&
        //                                !pe.IsDelete && pe.PropertyTemplate_UID.HasValue).ToList();

            

        //    // get all inserted property queue emails
        //    var insertedEmails = propertyQueueRepo.GetQuery(pq => pq.TaskType_UID == result.UID).Include(pq => pq.PropertyEvent).
        //                          ToList();

        //    var configuredReservationEmails = configuredEmails.Where(x => x.SystemTemplate != null
        //        && new List<long>(){            
        //        (long)Constants.SystemTemplatesCodes.NewBookingEmail,
        //        (long)Constants.SystemTemplatesCodes.NewBookingEmailHotel,
        //        (long)Constants.SystemTemplatesCodes.PostStay,
        //        (long)Constants.SystemTemplatesCodes.PreStay
        //    }.Contains(x.SystemTemplate.Code)).Select(x => x.UID).ToList();

        //    var insertedReservationEmails = insertedEmails.Where(x => x.PropertyEvent_UID != null
        //        && configuredReservationEmails.Contains(x.PropertyEvent_UID.Value)).Select(x => x.PropertyEvent_UID.Value).ToList();

        //    // must be equal
        //    Assert.IsTrue(configuredReservationEmails.SequenceEqual(insertedReservationEmails));
        //}
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
