using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.General;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Criteria;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain;
using OB.Domain.Payments;
using OB.Reservation.BL.Contracts.Requests;

using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.BPag.Classes;
using PaymentGatewaysLibrary.PaypalClasses;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class PaypalGatewayManagerPOCOUnitTest : UnitBaseTest
    {
        private Fixture fixture;

        //Repo Mock
        private Mock<IOBPropertyRepository> _propertyRepoMock = null;
        private Mock<IPaymentGatewayTransactionRepository> _paymentGatewayTranRepoMock = null;
        private Mock<IOBCurrencyRepository> _obCurrencyRepoMock = null;
        private Mock<IOBRateRepository> _obRatesRepoMock = null;
        private Mock<IRepository<PaymentGatewayTransactionsDetail>> _paymentGatewayTransactionsDetailRepoMock;

        // DB Mock
        private List<contractsReservations.Reservation> reservationsList = null;
        private List<PaymentGatewayConfiguration> paymentGatewayConfigurationList = null;
        private List<Currency> currencyList = null;
        private List<RateLight> rateList = null;
        private List<RoomType> roomTypeList = null;
        private List<PaymentGatewayTransaction> paymentGatewayTransactionList = null;

        // POCO Mock
        private Mock<IPaypal> _payPal = null;
        private Mock<IPaymentGatewayFactory> _paymentGatewayFactory = null;

        public IPaypalGatewayManagerPOCO paypalManagerPoco { get; set; }

        public enum AckCodeType
        {
            SUCCESS = 0,
            FAILURE = 1,
            WARNING = 2,
            SUCCESSWITHWARNING = 3,
            FAILUREWITHWARNING = 4,
            PARTIALSUCCESS = 5,
            CUSTOMCODE = 6
        }

        public enum RecurringPaymentsProfileStatusType
        {
            ACTIVEPROFILE = 0,
            PENDINGPROFILE = 1,
            CANCELLEDPROFILE = 2,
            EXPIREDPROFILE = 3,
            SUSPENDEDPROFILE = 4
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //Mock Repository factory
            var unitOfWorkResMock = new Mock<IUnitOfWork>();
            SessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(UnitOfWorkMock.Object);
            SessionFactoryMock.Setup(x => x.GetUnitOfWork(It.Is<DomainScope[]>(arg => arg != null && arg.Any())))
                .Returns((DomainScope[] ds) =>
                {
                    if (ds != null && ds.Any() && ds[0] == DomainScopes.Reservations)
                    {
                        return unitOfWorkResMock.Object;
                    }

                    return UnitOfWorkMock.Object;
                });
            SessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<bool>())).Returns(UnitOfWorkMock.Object);

            //Initialize lists mock db
            reservationsList = new List<contractsReservations.Reservation>();
            paymentGatewayConfigurationList = new List<PaymentGatewayConfiguration>();
            currencyList = new List<Currency>();
            rateList = new List<RateLight>();
            roomTypeList = new List<RoomType>();
            paymentGatewayTransactionList = new List<PaymentGatewayTransaction>();

            // POCO Mock
            this.paypalManagerPoco = this.Container.Resolve<IPaypalGatewayManagerPOCO>();
            _payPal = new Mock<IPaypal>(MockBehavior.Default);
            _paymentGatewayFactory = new Mock<IPaymentGatewayFactory>(MockBehavior.Default);

            //Repo Mock

            _propertyRepoMock = new Mock<IOBPropertyRepository>(MockBehavior.Default);
            _paymentGatewayTranRepoMock = new Mock<IPaymentGatewayTransactionRepository>(MockBehavior.Default);
            _obCurrencyRepoMock = new Mock<IOBCurrencyRepository>();
            _obRatesRepoMock = new Mock<IOBRateRepository>();
            _paymentGatewayTransactionsDetailRepoMock = new Mock<IRepository<PaymentGatewayTransactionsDetail>>(MockBehavior.Default);

            RepositoryFactoryMock.Setup(x => x.GetOBPropertyRepository())
                           .Returns(_propertyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetPaymentGatewayTransactionRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_paymentGatewayTranRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBCurrencyRepository())
                .Returns(_obCurrencyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBRateRepository())
                            .Returns(_obRatesRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<PaymentGatewayTransactionsDetail>(It.IsAny<IUnitOfWork>()))
                           .Returns(_paymentGatewayTransactionsDetailRepoMock.Object);

            this.Container = this.Container.RegisterInstance<IPaymentGatewayFactory>(_paymentGatewayFactory.Object);

            _paymentGatewayFactory.Setup(x => x.Paypal(It.IsAny<Dictionary<string, string>>())).Returns(_payPal.Object);
        }

        #region Mock Data

        private void FillCurrenciesMock()
        {
            currencyList.Add(new Currency()
            {
                UID = 1,
                Name = "Albania Lek",
                Symbol = "ALL",
                CurrencySymbol = "Lek",
                DefaultPositionNumber = null,
                PaypalCurrencyCode = "1"
            });
            currencyList.Add(new Currency()
            {
                UID = 16,
                Name = "Brazil Real",
                Symbol = "BRL",
                CurrencySymbol = "R$",
                DefaultPositionNumber = 3,
                PaypalCurrencyCode = "22"
            });
            currencyList.Add(new Currency()
            {
                UID = 34,
                Name = "Euro",
                Symbol = "EUR",
                CurrencySymbol = "€",
                DefaultPositionNumber = 1,
                PaypalCurrencyCode = "98"
            });
            currencyList.Add(new Currency()
            {
                UID = 109,
                Name = "US Dollar",
                Symbol = "USD",
                CurrencySymbol = "US$",
                DefaultPositionNumber = 2,
                PaypalCurrencyCode = "125"
            });
        }

        private void FillReservationsListMock()
        {
            reservationsList.Add(new contractsReservations.Reservation
            {
                UID = 40000,
                Guest_UID = 38175,  //verify if exists
                Number = "RES00001-1635",
                Channel_UID = 1,
                Date = new DateTime(2018, 12, 25, 15, 35, 03),
                TotalAmount = 733.0400M,
                Adults = 1,
                Children = 1,
                Status = 1,
                Notes = "\n\tSpecial Request: \"Non-smoking\"\n\n\tPromotion Type:\"Value Add\"; Name: \"FREE use of the Spa\"; Hotel Message: \"We offer the FREE use of the Hotel's Spa(except Spa Treatments).\"\n\n\tPromotion Type:\"CreatePercentOffDiscount\"; Name: \"NonRef+Long Stay\"; Hotel Message: \"Book 90 days in advance & Save 23%\"BookType: Land\"",
                IPAddress = "127.0.0.1",
                TPI_UID = null,
                PromotionalCode_UID = null,
                Property_UID = 1635,
                CreatedDate = new DateTime(2018, 12, 25, 21, 58, 20),
                CreateBy = null,
                Tax = 41.5100M,
                ChannelAffiliateName = "EBUK",
                PaymentMethodType_UID = 1,
                ReservationCurrency_UID = 34,
                ReservationBaseCurrency_UID = 34,
                ReservationCurrencyExchangeRate = 1.0000000000M,
                ReservationCurrencyExchangeRateDate = null,
                ReservationLanguageUsed_UID = 1,
                TotalTax = 41.5100M,
                NumberOfRooms = 1,
                RoomsTax = 0.0000M,
                RoomsExtras = null,
                RoomsPriceSum = 691.5300M,
                RoomsTotalAmount = 691.5300M,
                PropertyBaseCurrencyExchangeRate = 1.0000000000M,
                GuestFirstName = "teste",
                GuestEmail = "teste@test1.com",
                Guest = new Reservation.BL.Contracts.Data.CRM.Guest
                {
                    FirstName = "teste",
                    LastName = "t",
                    Email = "teste@test1.com"
                },
                ReservationPartialPaymentDetails = new List<contractsReservations.ReservationPartialPaymentDetail> {
                    new contractsReservations.ReservationPartialPaymentDetail
                    {
                        UID = 1234,
                        Reservation_UID = 40000,
                        InstallmentNo = 1,
                        InterestRate = null,
                        Amount = 733.0400M,
                        IsPaid = true,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    }
                },
                ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail
                {
                    UID = 36612,
                    PaymentMethod_UID = 3,
                    Reservation_UID = 40000,
                    Amount = 733.0400M,
                    Currency_UID = 34,
                    CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                    CardName = "User",
                    CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                    ExpirationDate = new DateTime(2013, 04, 28),
                    CreatedDate = new DateTime(2013, 03, 06, 21, 58, 21),
                    ModifiedDate = new DateTime(2013, 03, 06, 21, 58, 20),
                    PaymentGatewayTokenizationIsActive = false,
                    OBTokenizationIsActive = false,
                    CreditCardToken = null,
                    HashCode = null
                },
                ReservationRooms = new List<contractsReservations.ReservationRoom>()
                {
                    new contractsReservations.ReservationRoom()
                    {
                        UID = 43588,
                        Reservation_UID = 40000,
                        RoomType_UID = 5827,
                        GuestName = "Heike Röck",
                        SmokingPreferences = false,
                        DateFrom = new DateTime(2013,07,28),
                        DateTo = new DateTime(2013,08,04),
                        AdultCount = 1,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "RES00001-1635/1",
                        Status = 1,
                        CreatedDate = new DateTime(2013,03,06,21,58,20),
                        ModifiedDate = null,
                        RoomName = "Twin Classic Garden View ",
                        Rate_UID = 9175,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 691.5300M,
                        ReservationRoomsExtrasSum = null,
                        ArrivalTime = null,
                        ReservationRoomsTotalAmount = 691.5300M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<contractsReservations.ReservationRoomChild>() { },
                        ReservationRoomDetails = new List<contractsReservations.ReservationRoomDetail>() {
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110423,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,07,28),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110424,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,07,29),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110425,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,07,30),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110426,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,07,31),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110427,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,08,01),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110428,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,08,02),
                                ReservationRoomDetailsAppliedIncentives = null
                            },
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 110429,
                                RateRoomDetails_UID = null,
                                Price = 98.7900M,
                                ReservationRoom_UID = 43588,
                                AdultPrice = 98.7900M,
                                ChildPrice = 0.0000M,
                                CreatedDate = new DateTime(2013,03,06,21,58,20),
                                ModifiedDate = new DateTime(2013,03,06,21,58,20),
                                Date = new DateTime(2013,08,03),
                                ReservationRoomDetailsAppliedIncentives = null
                            }
                        },
                        ReservationRoomExtras = new List<contractsReservations.ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<contractsReservations.ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            reservationsList.Add(new contractsReservations.Reservation()
            {
                UID = 66242,
                Guest_UID = 284528,  //verify if exists
                Number = "RES00002-1635",
                Channel_UID = 1,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 400.0000M,
                Adults = 5,
                Children = 2,
                Status = 1,
                Notes = string.Empty,
                IPAddress = null,
                TPI_UID = null,
                PromotionalCode_UID = null,
                Property_UID = 1635,
                CreatedDate = new DateTime(2014, 02, 26, 15, 22, 40),
                BillingAddress1 = "1870 Eigth Rd.",
                BillingAddress2 = "1870 Eigth Rd.",
                BillingContactName = "Donnie Burt",
                BillingPostalCode = "22761",
                BillingCity = "Waterville",
                BillingCountry_UID = 91,
                CreateBy = null,
                Tax = 0.0000M,
                ChannelAffiliateName = "32",
                PaymentMethodType_UID = 1,
                ReservationCurrency_UID = 86,
                ReservationBaseCurrency_UID = 34,
                ReservationCurrencyExchangeRate = 0.2066000000M,
                ReservationCurrencyExchangeRateDate = new DateTime(2014, 02, 26, 15, 22, 39),
                ReservationLanguageUsed_UID = 6,
                BillingEmail = "Donnie.Farr@clobochem.net",
                TotalTax = 0.0000M,
                NumberOfRooms = 2,
                RoomsTax = 0.0000M,
                RoomsExtras = 60.0000M,
                RoomsPriceSum = 1001.0000M,
                RoomsTotalAmount = 1061.0000M,
                PropertyBaseCurrencyExchangeRate = 1.0000000000M,
                IsOnRequest = false,
                GuestFirstName = "Donnie",
                GuestLastName = "Donnie Burt",
                GuestEmail = "Donnie.Farr@clobochem.net",
                GuestPhone = "(131) 131-1311",
                GuestIDCardNumber = "1767011820",
                GuestCountry_UID = 91,
                GuestCity = "Waterville",
                GuestAddress1 = "1870 Eigth Rd.",
                GuestAddress2 = "1870 Eigth Rd.",
                GuestPostalCode = "22761",
                UseDifferentBillingInfo = false,
                BillingTaxCardNumber = "FL",
                IsMobile = false,
                IsPaid = null,
                InternalNotesHistory = "26/02/2014 15:22:40;",
                Company_UID = null,
                Employee_UID = null,
                TPICompany_UID = null,
                NoOfInstallment = 4,
                InstallmentAmount = 100,
                InterestRate = 1.5M,

                ReservationPartialPaymentDetails = new List<contractsReservations.ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail()
                {
                    UID = 59205,
                    PaymentMethod_UID = 1,
                    Reservation_UID = 66242,
                    Amount = 1061.0000M,
                    Currency_UID = 86,
                    CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                    CardName = "User",
                    CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                    ExpirationDate = new DateTime(2014, 03, 26, 15, 22, 39),
                    CreatedDate = new DateTime(2014, 02, 26, 15, 22, 47),
                    ModifiedDate = null,
                    PaymentGatewayTokenizationIsActive = false,
                    OBTokenizationIsActive = false,
                    CreditCardToken = null,
                    HashCode = null
                },
                ReservationRooms = new List<contractsReservations.ReservationRoom>()
                {
                    //Room 1
                    new contractsReservations.ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66242,
                        RoomType_UID = 5827,
                        GuestName = "Harlen Rupert",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 3,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "RES00002-1635/1",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "Quarto Teste",
                        Rate_UID = 9175,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 660.0000M,
                        ReservationRoomsExtrasSum = 45.0000M,
                        ArrivalTime = new TimeSpan(21,00,00),
                        ReservationRoomsTotalAmount = 705.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<contractsReservations.ReservationRoomChild>() {
                            new contractsReservations.ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<contractsReservations.ReservationRoomDetail>() {
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 192935,
                                RateRoomDetails_UID = null,
                                Price = 660.0000M,
                                ReservationRoom_UID = 73669,
                                AdultPrice = 487.0000M,
                                ChildPrice = 173.0000M,
                                CreatedDate = new DateTime(2014,02,26,15,22,40),
                                ModifiedDate = null,
                                Date = new DateTime(2014,02,27),
                                ReservationRoomDetailsAppliedIncentives = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<contractsReservations.ReservationRoomExtra>()
                        {
                            new contractsReservations.ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<contractsReservations.ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new contractsReservations.ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66242,
                        RoomType_UID = 5827,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "RES00002-1635/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 9175,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<contractsReservations.ReservationRoomChild>() {
                            new contractsReservations.ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<contractsReservations.ReservationRoomDetail>() {
                            new contractsReservations.ReservationRoomDetail()
                            {
                                UID = 192936,
                                RateRoomDetails_UID = null,
                                Price = 341.0000M,
                                ReservationRoom_UID = 73670,
                                AdultPrice = 283.0000M,
                                ChildPrice = 58.0000M,
                                CreatedDate = new DateTime(2014,02,26,15,22,40),
                                ModifiedDate = null,
                                Date = new DateTime(2014,02,27),
                                ReservationRoomDetailsAppliedIncentives = null,

                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<contractsReservations.ReservationRoomExtra>()
                        {
                                                        new contractsReservations.ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<contractsReservations.ReservationRoomTaxPolicy>() { }
                    }
                }
            });
        }

        private void FillPaymentGatewayConfigurationMock()
        {
            paymentGatewayConfigurationList.Add(new PaymentGatewayConfiguration
            {
                PropertyUID = 1635,
                GatewayUID = 2,
                GatewayName = nameof(Constants.PaymentGateway.Paypal),
                GatewayCode = ((int)Constants.PaymentGateway.Paypal).ToString(),
                ProcessorCode = null,
                ProcessorName = null,
                MerchantID = "reservas_api1.com",
                MerchantKey = "462BNF7DYLQ4E23Q",
                IsActive = true,
                Comission = null,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.Date,
                ModifiedBy = 1,
                ModifiedDate = DateTime.UtcNow.Date,
                DefaultAuthorizationOnly = null,
                DefaultAuthorizationSale = null,
                ApiSignatureKey = "AFcWxV21C7fd0v3bYYYRCpSSRl31AWJJSL9J1XYGwpufO4EaQ-nVjKAw",
                MerchantAccount = null,
                PaymentAuthorizationTypeId = null,
                IsAntiFraudeControlEnable = false
            });
        }

        private void FillRatesMock()
        {
            rateList.Add(new BL.Contracts.Data.Rates.RateLight()
            {
                UID = 9175,
                Name = "General Rate",
                IsPriceDerived = true,
                IsYielding = false,
                IsAvailableToTPI = false,
                Property_UID = 1635,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 8017
            });
            rateList.Add(new BL.Contracts.Data.Rates.RateLight()
            {
                UID = 12388,
                Name = "teste politica",
                IsPriceDerived = false,
                IsYielding = false,
                IsAvailableToTPI = false,
                Property_UID = 1635,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 8017
            });
            rateList.Add(new BL.Contracts.Data.Rates.RateLight()
            {
                UID = 12431,
                Name = "PM derivada derivada 1",
                IsPriceDerived = true,
                IsYielding = false,
                IsAvailableToTPI = false,
                Property_UID = 1635,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 12430
            });
        }

        private void FillRoomTypesMock()
        {
            roomTypeList.Add(new BL.Contracts.Data.Properties.RoomType()
            {
                UID = 5827,
                Name = "Quarto Teste",
                Qty = 10,
                ShortDescription = null,
                Description = null,
                AdultMaxOccupancy = 1,
                AdultMinOccupancy = 1,
                AcceptsChildren = true,
                ChildMaxOccupancy = 2,
                ChildMinOccupancy = 1,
                BasePrice = null,
                IsBase = false,
                Value = 0.00M,
                IsPercentage = null,
                IsValueDecrease = null,
                MaxValue = null,
                MinValue = null,
                IsDeleted = false,
                AcceptsExtraBed = true,
                CreatedDate = new DateTime(2013, 07, 30, 09, 33, 20),
                ModifiedDate = null,
                MaxOccupancy = 2,
                MaxFreeChild = 1,
                Property_UID = 1635
            });
            roomTypeList.Add(new BL.Contracts.Data.Properties.RoomType()
            {
                UID = 5855,
                Name = "quarto teste occ",
                Qty = 1,
                ShortDescription = null,
                Description = null,
                AdultMaxOccupancy = 3,
                AdultMinOccupancy = 2,
                AcceptsChildren = true,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 1,
                BasePrice = null,
                IsBase = false,
                Value = 0.00M,
                IsPercentage = null,
                IsValueDecrease = null,
                MaxValue = null,
                MinValue = null,
                IsDeleted = false,
                AcceptsExtraBed = true,
                CreatedDate = new DateTime(2014, 01, 28, 16, 31, 06),
                ModifiedDate = new DateTime(2014, 01, 28, 16, 32, 02),
                MaxOccupancy = 3,
                MaxFreeChild = null,
                Property_UID = 1635
            });
            roomTypeList.Add(new BL.Contracts.Data.Properties.RoomType()
            {
                UID = 3709,
                Name = "Twin Classic Garden View ",
                Qty = 37,
                ShortDescription = null,
                Description = null,
                AdultMaxOccupancy = 2,
                AdultMinOccupancy = 1,
                AcceptsChildren = null,
                ChildMaxOccupancy = 0,
                ChildMinOccupancy = 0,
                BasePrice = 0,
                IsBase = true,
                Value = null,
                IsPercentage = null,
                IsValueDecrease = null,
                MaxValue = 500.00M,
                MinValue = 5.00M,
                IsDeleted = false,
                AcceptsExtraBed = null,
                CreatedDate = new DateTime(2012, 07, 09, 00, 25, 31, 033),
                ModifiedDate = new DateTime(2012, 07, 12, 11, 03, 44, 723),
                MaxOccupancy = 2,
                MaxFreeChild = null
            });
            roomTypeList.Add(new BL.Contracts.Data.Properties.RoomType()
            {
                UID = 5148,
                Name = "Standard",
                Qty = 10,
                ShortDescription = "descricao do quarto Standard",
                Description = null,
                Property_UID = 1635,
                AdultMaxOccupancy = 2,
                AdultMinOccupancy = 1,
                AcceptsChildren = true,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 1,
                BasePrice = 0,
                IsBase = true,
                Value = null,
                IsPercentage = null,
                IsValueDecrease = null,
                MaxValue = 2500.00M,
                MinValue = 15.00M,
                IsDeleted = false,
                AcceptsExtraBed = null,
                CreatedDate = new DateTime(2012, 12, 13, 18, 10, 18),
                ModifiedDate = new DateTime(2013, 04, 24, 14, 48, 44),
                MaxOccupancy = 3,
                MaxFreeChild = null
            });
            roomTypeList.Add(new BL.Contracts.Data.Properties.RoomType()
            {
                UID = 5862,
                Name = "Standard 2",
                Qty = 10,
                ShortDescription = "descricao do quarto Standard 2",
                Description = null,
                Property_UID = 1635,
                AdultMaxOccupancy = 2,
                AdultMinOccupancy = 1,
                AcceptsChildren = true,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 1,
                BasePrice = 0,
                IsBase = true,
                Value = null,
                IsPercentage = null,
                IsValueDecrease = null,
                MaxValue = 2500.00M,
                MinValue = 15.00M,
                IsDeleted = false,
                AcceptsExtraBed = null,
                CreatedDate = new DateTime(2012, 12, 13, 18, 10, 18),
                ModifiedDate = new DateTime(2013, 04, 24, 14, 48, 44),
                MaxOccupancy = 3,
                MaxFreeChild = null
            });
        }

        private void FillPaymentGatewayTransactionMock(string reservationNumber)
        {
            paymentGatewayTransactionList.Add(new PaymentGatewayTransaction
            {
                UID = 43434343,
                PaymentGatewayName = "Paypal",
                Property = 1635,
                PaymentGatewayOrderID = "EC-5VU03513JH3188901",
                PaymentGatewayAutoGeneratedUID = reservationNumber,
                OrderType = "Sale",
                TransactionType = "LIVE",
                ServerDate = DateTime.UtcNow
            });
        }

        #endregion Mock DataRefundTransactionAPIOperation

        #region Mock Repositories Calls

        private void MockPaymentGatewayConfigurationRepo()
        {
            _propertyRepoMock.Setup(x => x.ListPaymentGatewayConfiguration(It.IsAny<ListPaymentGatewayConfigurationRequest>())).Returns((ListPaymentGatewayConfigurationRequest request) =>
            {
                return paymentGatewayConfigurationList.Where(x => request.PropertyIds.Contains(x.PropertyUID) && request.GatewayCodes.Contains(x.GatewayCode)).ToList();
            });
        }

        private void MockPaymentGatewayTransactionRepo()
        {
            _paymentGatewayTranRepoMock.Setup(x => x.Update(It.IsAny<PaymentGatewayTransaction>()));

            _paymentGatewayTranRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListPaymentGatewayTransactionsCriteria>())).Returns((ListPaymentGatewayTransactionsCriteria criteria) =>
            {
                return paymentGatewayTransactionList.Where(x => criteria.PaymentGatewayAutoGeneratedIds.Contains(x.PaymentGatewayAutoGeneratedUID));

            });

            _paymentGatewayTranRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<PaymentGatewayTransaction, bool>>>())).Returns((Expression e) =>
             {
                 return paymentGatewayTransactionList.FirstOrDefault();
             });
        }

        private void MockPaymentGatewayTransactionDetailsRepo()
        {
            _paymentGatewayTransactionsDetailRepoMock.Setup(x => x.Add(It.IsAny<PaymentGatewayTransactionsDetail>()));
        }

        private void MockObCurrenciesRepo()
        {
            _obCurrencyRepoMock.Setup(y => y.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(currencyList.Select(y => new Contracts.Data.General.Currency { UID = y.UID, CurrencySymbol = y.CurrencySymbol, DefaultPositionNumber = y.DefaultPositionNumber, Name = y.Name, PaypalCurrencyCode = y.PaypalCurrencyCode, Symbol = y.Symbol }).ToList());
        }

        private void MockObRatesRepo()
        {
            _obRatesRepoMock.Setup(x => x.ListRatesLight(It.IsAny<ListRateLightRequest>()))
                .Returns((ListRateLightRequest req) =>
                {
                    return rateList.Where(y => req.UIDs.Contains(y.UID)).ToList();
                });
        }

        private void MockObRoomTypesRepo()
        {
            _propertyRepoMock.Setup(x => x.ListRoomTypes(It.IsAny<ListRoomTypeRequest>()))
                    .Returns((ListRoomTypeRequest req) =>
                    {
                        return roomTypeList.Where(y => req.UIDs.Contains(y.UID)).ToList();
                    });
        }

        private void MockPaypalDoExpressCheckoutPayment(DoExpressCheckoutPaymentResponse response)
        {
            _payPal.Setup(x => x.DoExpressCheckoutPaymentAPIOperation(It.IsAny<SetExpressCheckoutRequest>())).Returns(response);
        }

        private void MockPaypalGetExpressCheckoutDetails(GetExpressCheckoutResponse response)
        {
            _payPal.Setup(x => x.GetExpressCheckoutDetailsAPIOperation(It.IsAny<string>())).Returns(response);
        }

        private void MockPaypalCreateRecurringBilling(CreateRecurringPaymentsProfileResponse response)
        {
            _payPal.Setup(x => x.CreateRecurringPaymentsProfileAPIOperation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SetExpressCheckoutRequest>())).Returns(response);
        }

        private void MockPaypalTransactionSearchByProfileId(PaymentTransactionSearch response)
        {
            _payPal.Setup(x => x.TransactionSearchByProfileIdAPIOperation(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(response);
        }

        private void MockPaypalGetTransactionDetails(TransactionDetails response)
        {
            _payPal.Setup(x => x.GetTransactionDetails(It.IsAny<string>())).Returns(response);
        }

        private void MockPaypalTransactionSearchByTransaction(PaymentTransactionSearch response)
        {
            _payPal.Setup(x => x.TransactionSearchByTransactionIdAPIOperation(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(response);
        }

        private void MockPaypalRefundTransaction(RefundTransactionResponse response)
        {
            _payPal.Setup(x => x.RefundTransactionAPIOperation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(response);
        }

        private void MockPaypalManageRecurringProfile(ManageRecurringPaymentsProfileStatusResponse response)
        {
            _payPal.Setup(x => x.ManageRecurringProfileAPIOperation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(response);
        }

        private void MockPaypalUpdateRecurringPaymentsProfile(CreateRecurringPaymentsProfileResponse response)
        {
            _payPal.Setup(x => x.UpdateRecurringPaymentsProfileAPIOperation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(response);
        }
        
        #endregion Mock Repositories Calls

        #region Capture

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Success_Transaction()
        {
            #region ARRANGE
            var doexpressCheckoutPaymentResponse = new DoExpressCheckoutPaymentResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                PaymentInfo = new List<PaymentInfo> {
                    new PaymentInfo
                    {
                        BinEligibility = "test",
                        ExchangeRate = "1",
                        ExpectedeCheckClearDate = "2019-01-01",
                        HoldDecision = "test",
                        InsuranceAmount = "500",
                        ParentTransactionID = Guid.NewGuid().ToString(),
                        PaymentDate = "2019-01-01",
                        PaymentError = null,
                        PaymentRequestID = Guid.NewGuid().ToString(),
                        ProtectionEligibility = "test",
                        ProtectionEligibilityType= "t",
                        ReceiptID = "3432432",
                        ReceiptReferenceNumber = "n23232nd232",
                        StoreID = "3434",
                        Subject = "test",
                        TerminalID = "34983943",
                        TransactionID = "9849348394"
                    }
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalDoExpressCheckoutPayment(doexpressCheckoutPaymentResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CapturePaymentRequest
            {
                PayerId = "1",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT
            var paypalTransationId = doexpressCheckoutPaymentResponse.PaymentInfo.FirstOrDefault().TransactionID;
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreNotEqual(string.Empty, response.PaypalTransationId);
            Assert.AreEqual(paypalTransationId, response.PaypalTransationId);
            Assert.AreEqual(paymentGatewayTransactionList.FirstOrDefault().UID, response.Reservation.PaymentGatewayTransactionUID);
            Assert.AreEqual(request.PaypalAutoGeneratedId, response.Reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(reservation.TotalAmount, response.Reservation.PaymentAmountCaptured);
            Assert.AreEqual(DateTime.Parse(doexpressCheckoutPaymentResponse.Timestamp), response.Reservation.PaymentGatewayTransactionDateTime);
            Assert.AreEqual("Paypal", response.Reservation.PaymentGatewayName);
            Assert.AreEqual(request.Token, response.Reservation.PaymentGatewayOrderID);
            Assert.AreEqual(paypalTransationId, response.Reservation.PaymentGatewayTransactionID);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionMessage);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionStatusCode);
        }

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Fail_Transaction()
        {
            #region ARRANGE
            var doexpressCheckoutPaymentResponse = new DoExpressCheckoutPaymentResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                PaymentInfo = new List<PaymentInfo> {
                    new PaymentInfo
                    {
                        BinEligibility = "test",
                        ExchangeRate = "1",
                        ExpectedeCheckClearDate = "2019-01-01",
                        HoldDecision = "test",
                        InsuranceAmount = "500",
                        ParentTransactionID = Guid.NewGuid().ToString(),
                        PaymentDate = "2019-01-01",
                        PaymentError = null,
                        PaymentRequestID = Guid.NewGuid().ToString(),
                        ProtectionEligibility = "test",
                        ProtectionEligibilityType= "t",
                        ReceiptID = "3432432",
                        ReceiptReferenceNumber = "n23232nd232",
                        StoreID = "3434",
                        Subject = "test",
                        TerminalID = "34983943",
                        TransactionID = "9849348394"
                    }
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalDoExpressCheckoutPayment(doexpressCheckoutPaymentResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CapturePaymentRequest
            {
                PayerId = "1",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.IsNull(response.PaypalTransationId);
            Assert.IsNull(response.Reservation);
        }

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Fail_RequiredParameters()
        {
            #region ARRANGE
            FillReservationsListMock();

            #endregion

            //// ACT
            var request = new CapturePaymentRequest();

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT

            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(5, response.Errors.Count);
            Assert.IsTrue(response.Errors.All(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter));
        }

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Fail_RequiredPaymentConfiguration()
        {
            #region ARRANGE
            FillReservationsListMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CapturePaymentRequest
            {
                PayerId = "1",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidPayPalConfiguration, response.Errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Success_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE
            var doexpressCheckoutPaymentResponse = new DoExpressCheckoutPaymentResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                PaymentInfo = new List<PaymentInfo> {
                    new PaymentInfo
                    {
                        BinEligibility = "test",
                        ExchangeRate = "1",
                        ExpectedeCheckClearDate = "2019-01-01",
                        HoldDecision = "test",
                        InsuranceAmount = "500",
                        ParentTransactionID = Guid.NewGuid().ToString(),
                        PaymentDate = "2019-01-01",
                        PaymentError = null,
                        PaymentRequestID = Guid.NewGuid().ToString(),
                        ProtectionEligibility = "test",
                        ProtectionEligibilityType= "t",
                        ReceiptID = "3432432",
                        ReceiptReferenceNumber = "n23232nd232",
                        StoreID = "3434",
                        Subject = "test",
                        TerminalID = "34983943",
                        TransactionID = "9849348394"
                    }
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalDoExpressCheckoutPayment(doexpressCheckoutPaymentResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CapturePaymentRequest
            {
                PayerId = "1",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT
            var paypalTransationId = doexpressCheckoutPaymentResponse.PaymentInfo.FirstOrDefault().TransactionID;
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreNotEqual(string.Empty, response.PaypalTransationId);
            Assert.AreEqual(paypalTransationId, response.PaypalTransationId);
            Assert.IsNull(response.Reservation.PaymentGatewayTransactionUID);
            Assert.AreEqual(request.PaypalAutoGeneratedId, response.Reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(reservation.TotalAmount, response.Reservation.PaymentAmountCaptured);
            Assert.AreEqual(DateTime.Parse(doexpressCheckoutPaymentResponse.Timestamp), response.Reservation.PaymentGatewayTransactionDateTime);
            Assert.AreEqual("Paypal", response.Reservation.PaymentGatewayName);
            Assert.AreEqual(request.Token, response.Reservation.PaymentGatewayOrderID);
            Assert.AreEqual(paypalTransationId, response.Reservation.PaymentGatewayTransactionID);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionMessage);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionStatusCode);
        }

        [TestMethod]
        [TestCategory("CapturePayment")]
        public void TestCapturePayment_Fail_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE
            var doexpressCheckoutPaymentResponse = new DoExpressCheckoutPaymentResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                PaymentInfo = new List<PaymentInfo> {
                    new PaymentInfo
                    {
                        BinEligibility = "test",
                        ExchangeRate = "1",
                        ExpectedeCheckClearDate = "2019-01-01",
                        HoldDecision = "test",
                        InsuranceAmount = "500",
                        ParentTransactionID = Guid.NewGuid().ToString(),
                        PaymentDate = "2019-01-01",
                        PaymentError = null,
                        PaymentRequestID = Guid.NewGuid().ToString(),
                        ProtectionEligibility = "test",
                        ProtectionEligibilityType= "t",
                        ReceiptID = "3432432",
                        ReceiptReferenceNumber = "n23232nd232",
                        StoreID = "3434",
                        Subject = "test",
                        TerminalID = "34983943",
                        TransactionID = "9849348394"
                    }
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalDoExpressCheckoutPayment(doexpressCheckoutPaymentResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CapturePaymentRequest
            {
                PayerId = "1",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CapturePayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.IsNull(response.PaypalTransationId);
            Assert.IsNull(response.Reservation);            
        }

        #endregion Capture

        #region PaypalVerifyInstallmentsAuthorization

        [TestMethod]
        [TestCategory("PaypalVerifyInstallmentsAuthorization")]
        public void TestPaypalVerifyInstallmentsAuthorization_Success_Transaction()
        {
            #region ARRANGE
            var getExpressCheckoutResponse = new GetExpressCheckoutResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                Payer = "teste",
                PayerBusiness = "business teste",
                PayerID = "fdejfoejfoe4539834",
                PayerFirstName = "first",
                PayerLastName = "last",
                PayerStatus = "test",
                BillingAgreementAcceptedStatus = true
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalGetExpressCheckoutDetails(getExpressCheckoutResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new PaypalVerifyInstallmentsAuthorizationRequest
            {
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.PaypalVerifyInstallmentsAuthorization(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
            Assert.AreEqual(0, response.Errors.Count);
        }

        [TestMethod]
        [TestCategory("PaypalVerifyInstallmentsAuthorization")]
        public void TestPaypalVerifyInstallmentsAuthorization_Fail_Transaction()
        {
            #region ARRANGE
            var getExpressCheckoutResponse = new GetExpressCheckoutResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                Payer = "teste",
                PayerBusiness = "business teste",
                PayerID = "fdejfoejfoe4539834",
                PayerFirstName = "first",
                PayerLastName = "last",
                PayerStatus = "test",
                BillingAgreementAcceptedStatus = false
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalGetExpressCheckoutDetails(getExpressCheckoutResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new PaypalVerifyInstallmentsAuthorizationRequest
            {
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.PaypalVerifyInstallmentsAuthorization(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("PaypalVerifyInstallmentsAuthorization")]
        public void TestPaypalVerifyInstallmentsAuthorization_Fail_RequiredParameters()
        {
            #region ARRANGE
            FillReservationsListMock();
            #endregion

            //// ACT
            var request = new PaypalVerifyInstallmentsAuthorizationRequest();

            //Mock 
            var response = this.paypalManagerPoco.PaypalVerifyInstallmentsAuthorization(request);

            //// ASSERT

            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(2, response.Errors.Count);
            Assert.IsTrue(response.Errors.All(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter));
        }

        [TestMethod]
        [TestCategory("PaypalVerifyInstallmentsAuthorization")]
        public void TestPaypalVerifyInstallmentsAuthorization_Fail_RequiredPaymentConfiguration()
        {
            #region ARRANGE
            FillReservationsListMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new PaypalVerifyInstallmentsAuthorizationRequest
            {
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.PaypalVerifyInstallmentsAuthorization(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidPayPalConfiguration, response.Errors.First().ErrorCode);
        }

        #endregion PaypalVerifyInstallmentsAuthorization

        #region CreateRecurringBilling

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Success_Transaction()
        {
            #region ARRANGE
            var createRecurringPaymentsProfileResponse = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                DCCProcessorResponse = string.Empty,
                DCCReturnCode = "200",
                ProfileID = "23jh213i32n3k2h3",
                ProfileStatus = RecurringPaymentsProfileStatusType.ACTIVEPROFILE.ToString(),
                TransactionID = Guid.NewGuid().ToString()
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalCreateRecurringBilling(createRecurringPaymentsProfileResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreNotEqual(string.Empty, response.ProfileId);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.ProfileId);
            Assert.AreEqual(paymentGatewayTransactionList.FirstOrDefault().UID, response.Reservation.PaymentGatewayTransactionUID);
            Assert.AreEqual(request.PaypalAutoGeneratedId, response.Reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(DateTime.Parse(createRecurringPaymentsProfileResponse.Timestamp), response.Reservation.PaymentGatewayTransactionDateTime);
            Assert.AreEqual("Paypal", response.Reservation.PaymentGatewayName);
            Assert.AreEqual(request.Token, response.Reservation.PaymentGatewayOrderID);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.Reservation.PaymentGatewayTransactionID);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionStatusCode);
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Fail_Transaction()
        {
            #region ARRANGE

            var createRecurringPaymentsProfileResponse = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                DCCProcessorResponse = string.Empty,
                DCCReturnCode = "200",
                ProfileID = "23jh213i32n3k2h3",
                ProfileStatus = RecurringPaymentsProfileStatusType.SUSPENDEDPROFILE.ToString(),
                TransactionID = Guid.NewGuid().ToString()
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalCreateRecurringBilling(createRecurringPaymentsProfileResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.IsNull(response.ProfileId);
            Assert.IsNull(response.Reservation);
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Fail_RequiredParameters()
        {
            #region ARRANGE

            FillReservationsListMock();

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest();

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT

            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(6, response.Errors.Count);
            Assert.IsTrue(response.Errors.All(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter));
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Fail_RequiredPaymentConfiguration()
        {
            #region ARRANGE

            FillReservationsListMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidPayPalConfiguration, response.Errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Success_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE

            var createRecurringPaymentsProfileResponse = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                DCCProcessorResponse = string.Empty,
                DCCReturnCode = "200",
                ProfileID = "23jh213i32n3k2h3",
                ProfileStatus = RecurringPaymentsProfileStatusType.ACTIVEPROFILE.ToString(),
                TransactionID = Guid.NewGuid().ToString()
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalCreateRecurringBilling(createRecurringPaymentsProfileResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreNotEqual(string.Empty, response.ProfileId);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.ProfileId);
            Assert.IsNull(response.Reservation.PaymentGatewayTransactionUID);
            Assert.AreEqual(request.PaypalAutoGeneratedId, response.Reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(DateTime.Parse(createRecurringPaymentsProfileResponse.Timestamp), response.Reservation.PaymentGatewayTransactionDateTime);
            Assert.AreEqual("Paypal", response.Reservation.PaymentGatewayName);
            Assert.AreEqual(request.Token, response.Reservation.PaymentGatewayOrderID);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.Reservation.PaymentGatewayTransactionID);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionStatusCode);
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Fail_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE

            var createRecurringPaymentsProfileResponse = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                DCCProcessorResponse = string.Empty,
                DCCReturnCode = "200",
                ProfileID = "23jh213i32n3k2h3",
                ProfileStatus = RecurringPaymentsProfileStatusType.ACTIVEPROFILE.ToString(),
                TransactionID = Guid.NewGuid().ToString()
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalCreateRecurringBilling(createRecurringPaymentsProfileResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);            
            Assert.IsNull(response.ProfileId);
            Assert.IsNull(response.Reservation);      
        }

        [TestMethod]
        [TestCategory("CreateRecurringBilling")]
        public void TestCreateRecurringBilling_Success_WithInstallments()
        {
            #region ARRANGE
            var createRecurringPaymentsProfileResponse = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                DCCProcessorResponse = string.Empty,
                DCCReturnCode = "200",
                ProfileID = "23jh213i32n3k2h3",
                ProfileStatus = RecurringPaymentsProfileStatusType.ACTIVEPROFILE.ToString(),
                TransactionID = Guid.NewGuid().ToString()
            };

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalCreateRecurringBilling(createRecurringPaymentsProfileResponse);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CreateRecurringBillingRequest
            {
                PropertyName = "hotel demo",
                Token = Guid.NewGuid().ToString(),
                PaypalAutoGeneratedId = reservationNr,
                LanguageUID = 1,
                Reservation = reservation,
                ReservationRooms = reservation.ReservationRooms.ToList()
            };

            //Mock 
            var response = this.paypalManagerPoco.CreateRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreNotEqual(string.Empty, response.ProfileId);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.ProfileId);
            Assert.AreEqual(paymentGatewayTransactionList.FirstOrDefault().UID, response.Reservation.PaymentGatewayTransactionUID);
            Assert.AreEqual(request.PaypalAutoGeneratedId, response.Reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(DateTime.Parse(createRecurringPaymentsProfileResponse.Timestamp), response.Reservation.PaymentGatewayTransactionDateTime);
            Assert.AreEqual("Paypal", response.Reservation.PaymentGatewayName);
            Assert.AreEqual(request.Token, response.Reservation.PaymentGatewayOrderID);
            Assert.AreEqual(createRecurringPaymentsProfileResponse.ProfileID, response.Reservation.PaymentGatewayTransactionID);
            Assert.AreEqual("SUCCESS", response.Reservation.PaymentGatewayTransactionStatusCode);
        }

        #endregion CreateRecurringBilling

        #region CancelAndRefundRecurringBilling

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_TransactionSearchByProfileId()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_GetTransactionDetails()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "Fail",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_Completed_RefundPayment()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_PartialRefunded_RefundPayment()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "PARTIALLYREFUNDED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(paymentTransactionSearch);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 100M;

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_UpdateRecurringProfile()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_GetPaypalAlreadyPaidValue()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Ack = AckCodeType.FAILURE.ToString();

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_NegativePaidValue_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Ack = AckCodeType.SUCCESS.ToString();
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 500M;

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_AdjustParcels_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var createRecurringPaymentProfile = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileID = string.Empty
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalUpdateRecurringPaymentsProfile(createRecurringPaymentProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "Fail",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_Completed_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString()
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_PartialRefunded_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 800M,
                    PaymentStatus = "PARTIALLYREFUNDED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString()
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_UpdateRecurringProfile_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "750"
            };

            var createRecurringPaymentProfile = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileID = string.Empty
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalUpdateRecurringPaymentsProfile(createRecurringPaymentProfile);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_RequiredParameters()
        {
            #region ARRANGE

            FillReservationsListMock();

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest();

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT

            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(3, response.Errors.Count);
            Assert.AreEqual(2, response.Errors.Count(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter));
            Assert.AreEqual(1, response.Errors.Count(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidParameter));
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_RequiredPaymentConfiguration()
        {
            #region ARRANGE

            FillReservationsListMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual(false, response.Result);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidPayPalConfiguration, response.Errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Fail_ReservationDoesNotExist()
        {
            #region ARRANGE
            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.UID = 0;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.ReservationDoesNotExist, response.Errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_UpdateRecurringProfile()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_UpdateRecurringProfile_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "750"
            };

            var createRecurringPaymentProfile = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileID = "23jh213i32n3k2h3"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalUpdateRecurringPaymentsProfile(createRecurringPaymentProfile);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_AdjustParcels_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var createRecurringPaymentProfile = new CreateRecurringPaymentsProfileResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileID = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalUpdateRecurringPaymentsProfile(createRecurringPaymentProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "Fail",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_Completed_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 800M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_PartialRefunded_RefundExceededAmount_PartialRecurringPayments()
        {
            #region ARRANGE
            var searchByProfileId = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 800M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "PROCESSING",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 800M,
                    PaymentStatus = "PARTIALLYREFUNDED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(searchByProfileId);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 50M;

            string reservationNr = "RES00002-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(searchByProfileId);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.SUSPEND,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_Completed_RefundPayment()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_PartialRefunded_RefundPayment()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "REFUND"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "PARTIALLYREFUNDED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            var searchByTransaction = (PaymentTransactionSearch)Cloner.Clone(paymentTransactionSearch);
            searchByTransaction.Transactions.FirstOrDefault().GrossAmount = 100M;

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalTransactionSearchByTransaction(searchByTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("CancelAndRefundRecurringBilling")]
        public void TestCancelAndRefundRecurringBilling_Success_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE
            var paymentTransactionSearch = new PaymentTransactionSearch
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Transactions = new List<PaymentTransactionSearchDetails>
                {
                   new PaymentTransactionSearchDetails
                   {
                       FeeAmount = 100M,
                       GrossAmount = 400M,
                       NetAmount = 350M,
                       Payer = "1",
                       PayerDisplayName = "teste",
                       Status = "SUCCESS",
                       Timestamp = DateTime.UtcNow.ToString(),
                       TransactionID = Guid.NewGuid().ToString(),
                       Type = "Cancel"
                   }
                }
            };

            var transactionDetails = new TransactionDetails
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                TransactionInfo = new PaymentInfo
                {
                    BinEligibility = "teste",
                    ExchangeRate = "1.0",
                    ExpectedeCheckClearDate = DateTime.UtcNow.ToString(),
                    HoldDecision = "teste1",
                    FeeAmount = "100",
                    GrossAmount = 400M,
                    PaymentStatus = "COMPLETED",
                    TransactionID = Guid.NewGuid().ToString()
                }
            };

            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "800",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "750",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "FAILDED",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            var manageRecurringPaymentsProfile = new ManageRecurringPaymentsProfileStatusResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                ProfileId = "23jh213i32n3k2h3"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalTransactionSearchByProfileId(paymentTransactionSearch);
            MockPaypalGetTransactionDetails(transactionDetails);
            MockPaypalRefundTransaction(refundTransaction);
            MockPaypalManageRecurringProfile(manageRecurringPaymentsProfile);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);
            reservation.PaymentGatewayTransactionUID = 43434343;
            #endregion

            //// ACT
            var request = new CancelAndRefundRecurringBillingRequest
            {
                PaymentGatewayTransactionId = 43434343,
                ProfileId = "23jh213i32n3k2h3",
                IsPartialRefund = true,
                Action = Reservation.BL.Constants.PayPalAction.CANCEL,
                Note = "dwkdwkdpwqpdwqkdpw",
                LanguageUID = 1,
                Reservation = reservation,
            };

            //Mock 
            var response = this.paypalManagerPoco.CancelAndRefundRecurringBilling(request);

            //// ASSERT            
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        #endregion CancelAndRefundRecurringBilling

        #region Refund

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Success_Transaction()
        {
            #region ARRANGE
            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new RefundPaymentRequest
            {
                TransactionId = 43434343,
                PaypalTransactionId = Guid.NewGuid().ToString(),
                RefundType = Reservation.BL.Constants.PaypalRefundType.FULL,
                RefundAmount = "400",
                ReservationCurrency = 1,
                ReservationNumber = reservationNr,
                PropertyId = 1635
            };

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Fail_Transaction()
        {
            #region ARRANGE
            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockPaymentGatewayTransactionRepo();
            MockPaymentGatewayTransactionDetailsRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new RefundPaymentRequest
            {
                TransactionId = 43434343,
                PaypalTransactionId = Guid.NewGuid().ToString(),
                RefundType = Reservation.BL.Constants.PaypalRefundType.FULL,
                RefundAmount = "400",
                ReservationCurrency = 1,
                ReservationNumber = reservationNr,
                PropertyId = 1635
            };

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Fail_RequiredParameters()
        {
            #region ARRANGE
            FillReservationsListMock();

            #endregion

            //// ACT
            var request = new RefundPaymentRequest();

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT

            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(6, response.Errors.Count);
            Assert.AreEqual(5, response.Errors.Count(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter));
            Assert.AreEqual(1, response.Errors.Count(x => x.ErrorCode == (int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidParameter));
        }

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Fail_RequiredPaymentConfiguration()
        {
            #region ARRANGE
            FillReservationsListMock();

            MockPaymentGatewayConfigurationRepo();

            string reservationNr = "RES00001-1635";
            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new RefundPaymentRequest
            {
                TransactionId = 43434343,
                PaypalTransactionId = Guid.NewGuid().ToString(),
                RefundType = Reservation.BL.Constants.PaypalRefundType.FULL,
                RefundAmount = "400",
                ReservationCurrency = 1,
                ReservationNumber = reservationNr,
                PropertyId = 1635
            };

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidPayPalConfiguration, response.Errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Success_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE
            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.SUCCESS.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new RefundPaymentRequest
            {
                TransactionId = 43434343,
                PaypalTransactionId = Guid.NewGuid().ToString(),
                RefundType = Reservation.BL.Constants.PaypalRefundType.FULL,
                RefundAmount = "400",
                ReservationCurrency = 1,
                ReservationNumber = reservationNr,
                PropertyId = 1635
            };

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [TestCategory("RefundPayment")]
        public void TestRefundPayment_Fail_Transaction_NotFoundPaymentGatewayTransaction()
        {
            #region ARRANGE
            var refundTransaction = new RefundTransactionResponse
            {
                Ack = AckCodeType.FAILURE.ToString(),
                Build = "1",
                CorrelationID = Guid.NewGuid().ToString(),
                Errors = null,
                Timestamp = DateTime.UtcNow.ToString(),
                Version = "1",
                FeeRefundAmount = "100",
                GrossRefundAmount = "400",
                MsgSubID = Guid.NewGuid().ToString(),
                NetRefundAmount = "350",
                ReceiptData = "test",
                PendingReason = "test",
                RefundStatus = "SUCCESS",
                RefundTransactionID = Guid.NewGuid().ToString(),
                TotalRefundedAmount = "350"
            };

            string reservationNr = "RES00001-1635";

            FillReservationsListMock();
            FillPaymentGatewayConfigurationMock();
            FillCurrenciesMock();
            FillRatesMock();
            FillRoomTypesMock();
            FillPaymentGatewayTransactionMock(reservationNr);

            MockPaymentGatewayConfigurationRepo();
            MockObCurrenciesRepo();
            MockObRatesRepo();
            MockObRoomTypesRepo();
            MockPaypalRefundTransaction(refundTransaction);

            var reservation = reservationsList.FirstOrDefault(x => x.Number == reservationNr);

            #endregion

            //// ACT
            var request = new RefundPaymentRequest
            {
                TransactionId = 43434343,
                PaypalTransactionId = Guid.NewGuid().ToString(),
                RefundType = Reservation.BL.Constants.PaypalRefundType.FULL,
                RefundAmount = "400",
                ReservationCurrency = 1,
                ReservationNumber = reservationNr,
                PropertyId = 1635
            };

            //Mock 
            var response = this.paypalManagerPoco.RefundPayment(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(false, response.Result);
        }

        #endregion Refund
    }
}
