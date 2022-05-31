using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.General;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Test;
using OB.DL.Common;
using OB.DL.Common.Criteria;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Couchbase;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using domainCRM = OB.BL.Contracts.Data.CRM;
using domainReservation = OB.Domain.Reservations;
using QueryableExtensions = OB.DL.Common.QueryableExtensions;
using OB.BL;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.BL.Operations.Helper;
using OB.DL.Common.Criteria;
using OB.DL.Common.Repositories.Interfaces.Couchbase;

namespace OB.REST.Services.Test.Controllers
{
    [TestClass]
    public class ReservationControllerTest : UnitBaseTest
    {

        public IReservationManagerPOCO ReservationPOCO
        {
            get;
            set;
        }

        //repos
        private IReservationsRepository _reservationsRepoMock = null;
        private Mock<IGroupRulesRepository> _groupRulesRepoMock = null;
        private Mock<IOBAppSettingRepository> _obAppSettingsRepoMock = null;
        private Mock<IReservationsFilterRepository> _reservationsFilterRepoMock = null;
        private Mock<IOBCRMRepository> _iOBCRMRepoMock = null;
        private Mock<IOBPropertyRepository> _iOBPPropertyRepoMock = null;
        private Mock<IOBPaymentMethodTypeRepository> _iOBPaymentMethodMock = null;
        private Mock<IOBSecurityRepository> _obSecurityRepoMock = null;
        private Mock<IOBDepositPolicyRepository> _depositPolicyRepoMock = null;
        private Mock<IOBCancellationPolicyRepository> _cancelationPolicyRepoMock = null;
        private Mock<IOBOtherPolicyRepository> _otherPolicyRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoom>> _reservationRoomRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomDetail>> _resRoomDetailsRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationsAdditionalData>> _reservationsAdditionalDataRepoMock = null;
        private Mock<IOBRateRoomDetailsForReservationRoomRepository> _obRateToomDetailsForResRepoMock = null;
        private Mock<IOBRateRepository> _obRatesRepoMock = null;
        private Mock<IOBChannelRepository> _obChannelsRepoMock = null;
        private Mock<IVisualStateRepository> _visualStateRepoMock = null;
        private Mock<IOBCurrencyRepository> _obCurrencyRepoMock = null;
        private Mock<IOBReservationLookupsRepository> _obReservationLookupsRepoMock = null;
        private Mock<ISqlManager> _sqlManagerRepoMock = null;
        private Mock<ILostReservationsRepository> _lostReservationsRepoMock = null;
        private Mock<ILostReservationDetailRepository> _couchBaseRepoMock = null;
        private Mock<IReservationStatusRepository> _reservationStatusRepoMock = null;
        private Mock<IOBPromotionalCodeRepository> _promoCodesRepoMock = null;


        //lists
        List<domainReservation.Reservation> reservationsList = null;
        List<Setting> settingsList = null;
        List<domainReservation.ReservationFilter> reservationFilterList = null;
        List<domainCRM.Guest> guestsList = null;
        List<BL.Contracts.Data.Channels.ChannelsProperty> chPropsList = null;
        List<BL.Contracts.Data.Payments.PaymentMethodType> paymentTypesList = null;
        List<Language> listLanguages = null;
        List<BL.Contracts.Data.Properties.RoomType> listRoomTypes = null;
        List<BL.Contracts.Data.Rates.Rate> listRates = null;
        List<BL.Contracts.Data.Channels.ChannelLight> listChannelsLight = null;
        List<Currency> listCurrencies = null;
        List<domainReservation.VisualState> listVisualStates = null;
        List<BL.Contracts.Data.Properties.PropertyLight> listPropertiesLigth = null;
        List<PaymentMethod> paymentMethodsList = null;
        List<domainReservation.LostReservation> listLostReservations = null;
        Dictionary<Guid, domainReservation.LostReservationDetail> listCouchBaseLostReservation = null;
        List <domainReservation.ReservationStatus> listReservationStatus = null;
        List<domainReservation.ReservationStatusLanguage> listReservationStatusLanguages = null;


        #region Initialize
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            // Mock Repository factory
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var unitOfWorkResMock = new Mock<IUnitOfWork>();
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.Is<DomainScope[]>(arg => arg != null && arg.Any())))
                .Returns((DomainScope[] ds) =>
                {
                    if (ds != null && ds.Any() && ds[0] == DomainScopes.Reservations)
                    {
                        return unitOfWorkResMock.Object;
                    }

                    return unitOfWorkMock.Object;
                });
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<bool>())).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);
            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            // Mock Controller and POCO
            this.ReservationPOCO = this.Container.Resolve<IReservationManagerPOCO>();

            //Iniciate the BD
            reservationsList = new List<domainReservation.Reservation>();
            settingsList = new List<Setting>();
            reservationFilterList = new List<domainReservation.ReservationFilter>();
            guestsList = new List<domainCRM.Guest>();
            chPropsList = new List<BL.Contracts.Data.Channels.ChannelsProperty>();
            paymentTypesList = new List<BL.Contracts.Data.Payments.PaymentMethodType>();
            listLanguages = new List<Language>();
            listRoomTypes = new List<BL.Contracts.Data.Properties.RoomType>();
            listRates = new List<BL.Contracts.Data.Rates.Rate>();
            listChannelsLight = new List<BL.Contracts.Data.Channels.ChannelLight>();
            listCurrencies = new List<Currency>();
            listVisualStates = new List<domainReservation.VisualState>();
            listPropertiesLigth = new List<BL.Contracts.Data.Properties.PropertyLight>();
            paymentMethodsList = new List<PaymentMethod>();
            listLostReservations = new List<domainReservation.LostReservation>();
            listCouchBaseLostReservation = new Dictionary<Guid, domainReservation.LostReservationDetail>();
            listReservationStatus = new List<domainReservation.ReservationStatus>();
            listReservationStatusLanguages = new List<domainReservation.ReservationStatusLanguage>();

            // Mock Data
            FillReservationsMock();
            FillGroupRulesMock();
            FillObAppSettingsMock();
            FillReservationsFilterMock();
            FillGuestsMock();
            FillChPropsListMock();
            FillPaymentMethodsMock();
            FillLanguagesMock();
            FillRoomTypesMock();
            FillRatesMock();
            FillChannelsLightMock();
            FillCurrenciesMock();
            FillVisualStatesMock();
            FillPropertiesLigth();
            FillPaymentMethodsListMock();
            FillLostReservationsListMock();
            FillLostReservationDetailsListMock();
            FillReservationStatusListMock();

            // Repo Mock
            _reservationsRepoMock = new MyMockReservationRepo(reservationsList);
            _obAppSettingsRepoMock = new Mock<IOBAppSettingRepository>(MockBehavior.Default);
            _reservationsFilterRepoMock = new Mock<IReservationsFilterRepository>(MockBehavior.Default);
            _groupRulesRepoMock = new Mock<IGroupRulesRepository>(MockBehavior.Default);
            _iOBCRMRepoMock = new Mock<IOBCRMRepository>(MockBehavior.Default);
            _iOBPPropertyRepoMock = new Mock<IOBPropertyRepository>(MockBehavior.Default);
            _iOBPaymentMethodMock = new Mock<IOBPaymentMethodTypeRepository>(MockBehavior.Default);
            _obSecurityRepoMock = new Mock<IOBSecurityRepository>(MockBehavior.Default);
            _depositPolicyRepoMock = new Mock<IOBDepositPolicyRepository>(MockBehavior.Default);
            _cancelationPolicyRepoMock = new Mock<IOBCancellationPolicyRepository>(MockBehavior.Default);
            _otherPolicyRepoMock = new Mock<IOBOtherPolicyRepository>(MockBehavior.Default);
            _reservationRoomRepoMock = new Mock<IRepository<domainReservation.ReservationRoom>>(MockBehavior.Default);
            _resRoomDetailsRepoMock = new Mock<IRepository<domainReservation.ReservationRoomDetail>>(MockBehavior.Default);
            _reservationsAdditionalDataRepoMock = new Mock<IRepository<domainReservation.ReservationsAdditionalData>>(MockBehavior.Default);
            _obRateToomDetailsForResRepoMock = new Mock<IOBRateRoomDetailsForReservationRoomRepository>(MockBehavior.Default);
            _obRatesRepoMock = new Mock<IOBRateRepository>();
            _obChannelsRepoMock = new Mock<IOBChannelRepository>();
            _obCurrencyRepoMock = new Mock<IOBCurrencyRepository>();
            _visualStateRepoMock = new Mock<IVisualStateRepository>();
            _lostReservationsRepoMock = new Mock<ILostReservationsRepository>();
            _couchBaseRepoMock = new Mock<ILostReservationDetailRepository>();
            _obReservationLookupsRepoMock = new Mock<IOBReservationLookupsRepository>();
            _sqlManagerRepoMock = new Mock<ISqlManager>();
            _reservationStatusRepoMock = new Mock<IReservationStatusRepository>();
            _promoCodesRepoMock = new Mock<IOBPromotionalCodeRepository>();


            repoFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationsRepoMock);
            repoFactoryMock.Setup(x => x.GetOBAppSettingRepository())
                            .Returns(_obAppSettingsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetReservationsFilterRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationsFilterRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetGroupRulesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_groupRulesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBCRMRepository())
                            .Returns(_iOBCRMRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBPropertyRepository())
                            .Returns(_iOBPPropertyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBPaymentMethodTypeRepository())
                            .Returns(_iOBPaymentMethodMock.Object);
            repoFactoryMock.Setup(x => x.GetOBSecurityRepository())
                            .Returns(_obSecurityRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBDepositPolicyRepository())
                            .Returns(_depositPolicyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBCancellationPolicyRepository())
                            .Returns(_cancelationPolicyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBOtherPolicyRepository())
                            .Returns(_otherPolicyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationRoom>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationRoomRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationRoomDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomDetailsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationsAdditionalData>(It.IsAny<IUnitOfWork>()))
                    .Returns(_reservationsAdditionalDataRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBRateRoomDetailsForReservationRoomRepository())
                            .Returns(_obRateToomDetailsForResRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBRateRepository())
                            .Returns(_obRatesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBChannelRepository())
                            .Returns(_obChannelsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBCurrencyRepository())
                            .Returns(_obCurrencyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetVisualStateRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_visualStateRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetLostReservationsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_lostReservationsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetLostReservationDetailRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_couchBaseRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBReservationLookupsRepository())
                            .Returns(_obReservationLookupsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<string>()))
                            .Returns(_sqlManagerRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetReservationStatusRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationStatusRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetOBPromotionalCodeRepository())
                .Returns(_promoCodesRepoMock.Object);

            //Mock SqlManager
            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), It.IsAny<DomainScope>()))
                .Returns(_sqlManagerRepoMock.Object);

            // Mock Repositories Methods
            MockSettingsRepo();
            MockReservationFiltersRepo();
            MockReservationRepo();
            MockGroupRulesRepo();
            MockObCrmRepo();
            MockObPropertyRepo();
            MockObPaymentMethodRepo();
            MockSecurityRepo();
            MockCancelationPolicyRepo();
            MockOtherPolicyRepo();
            MockReservationsRoomsRepo();
            MockResRoomDetailsRepo();
            MockObRateToomDetailsForResRepo();
            MockObRatesRepo();
            MockObChannelsRepo();
            MockObCurrenciesRepo();
            MockVisualStates();
            MockObReservationLookups();
            MockSqlManager();

            MockLostReservations();
            MockCouchBaseLostReservations();
            MockReservationStatusRepo();

        }
        #endregion


        #region Mock Data
        private void FillReservationsMock()
        {
            reservationsList = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 3093,
                    Guest_UID = 5329,
                    Number = "SP-120625-TRIP1297564B1",
                    Channel_UID = 9,
                    Date = new DateTime(2012,06,25),
                    TotalAmount = 975,
                    Adults = 0,
                    Status = 1,
                    Notes = "Wir wissen noch nicht die genaue Ankunftszeit.",
                    Property_UID = 1094,
                    CreatedDate = new DateTime(2012,07,11,14,22,53,487),
                    Tax = 0,
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 0,
                    NumberOfRooms = 1,
                    RoomsTax = 0,
                    RoomsPriceSum = 975,
                    RoomsTotalAmount = 975,
                    PropertyBaseCurrencyExchangeRate = 1
                },
                new domainReservation.Reservation()
                {
                    UID = 3094,
                    Guest_UID = 5330,
                    Number = "SP-120627-CAMA1298522B1",
                    Channel_UID = 9,
                    Date = new DateTime(2012,06,27),
                    TotalAmount = 1440,
                    Adults = 0,
                    Status = 2,
                    Notes = "VISTAS AL JARDIN",
                    Property_UID = 1094,
                    CreatedDate = new DateTime(2012,07,11,14,22,54,267),
                    Tax = 0,
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 0,
                    NumberOfRooms = 1,
                    RoomsTax = 0,
                    RoomsPriceSum = 1440,
                    RoomsTotalAmount = 1440,
                    PropertyBaseCurrencyExchangeRate = 1
                },
                new domainReservation.Reservation()
                {
                    UID = 3095,
                    Guest_UID = 5330,
                    Number = "SP-120627-CAMA1298522B2",
                    Channel_UID = 9,
                    Date = new DateTime(2012,06,27),
                    TotalAmount = 1440,
                    Adults = 0,
                    Status = 1,
                    Notes = "VISTAS AL JARDIN",
                    Property_UID = 1094,
                    CreatedDate = new DateTime(2012,07,11,14,22,54,750),
                    Tax = 0,
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 0,
                    NumberOfRooms = 1,
                    RoomsTax = 0,
                    RoomsPriceSum = 1440,
                    RoomsTotalAmount = 1440,
                    PropertyBaseCurrencyExchangeRate = 1
                },
                new domainReservation.Reservation()
                {
                    UID = 3102,
                    Guest_UID = 5337,
                    Number = "Orbitz-FALQFM",
                    Channel_UID = 28,
                    Date = new DateTime(2012,06,27,11,55,13),
                    TotalAmount = 88,
                    Adults = 2,
                    Children = 0,
                    Status = 1,
                    Notes = "Special Request: \"Non-smoking\"\r\n    Promotion Type:\"Value Add\"; Name: \"FREE use of the Spa\"; Hotel Message: \"We offer the FREE use of the Hotel's Spa(except Spa Treatments).\"",
                    IPAddress = "127.0.0.1",
                    Property_UID = 1263,
                    CreatedDate = new DateTime(2012,07,11,17,17,02,813),
                    Tax = 4.9M,
                    ChannelAffiliateName = "EBFR",
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 4.98M,
                    RoomsTax = 0,
                    RoomsPriceSum = 83.02M,
                    RoomsTotalAmount = 83.02M,
                    PropertyBaseCurrencyExchangeRate = 1
                },
                new domainReservation.Reservation()
                {
                    UID = 3113,
                    Guest_UID = 5346,
                    Number = "Orbitz-1TY3RF",
                    Channel_UID = 28,
                    Date = new DateTime(2012,07,27,12,27,15),
                    TotalAmount = 122.4M,
                    Adults = 3,
                    Children = 0,
                    Status = 2,
                    Notes = "Special Request: \"No Preference\"\r\n    Promotion Type:\"Value Add\"; Name: \"FREE use of the Spa\"; Hotel Message: \"We offer the FREE use of the Hotel's Spa(except Spa Treatments).\"",
                    IPAddress = "127.0.0.1",
                    Property_UID = 1263,
                    CreatedDate = new DateTime(2012,07,11,19,05,58,047),
                    ModifyDate = new DateTime(2012,08,16,11,54,14,457),
                    ModifyBy = 65,
                    Tax = 6.93M,
                    ChannelAffiliateName = "EBIE",
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 6.93M,
                    RoomsTax = 0,
                    RoomsPriceSum = 115.47M,
                    RoomsTotalAmount = 115.47M,
                    PropertyBaseCurrencyExchangeRate = 1
                },
                new domainReservation.Reservation()
                {
                    UID = 3114,
                    Guest_UID = 5347,
                    Number = "Orbitz-69WG8I",
                    Channel_UID = 28,
                    GDSSource = null,
                    Date = new DateTime(2012,07,11,12,55,48,000),
                    TotalAmount = 104.8000M,
                    Adults = 2,
                    Children = 1,
                    Status = 2,
                    Notes = "Special Request: \"No Preference\" Promotion Type:\"Value Add\"; Name: \"FREE use of the Spa\"; Hotel Message: \"We offer the FREE use of the Hotel's Spa(except Spa Treatments).\"",
                    InternalNotes = null,
                    IPAddress = "127.0.0.1",
                    TPI_UID = null,
                    PromotionalCode_UID = null,
                    ChannelProperties_RateModel_UID = null,
                    ChannelProperties_Value = null,
                    ChannelProperties_IsPercentage = null,
                    InvoicesDetail_UID = null,
                    Property_UID = 1263,
                    CreatedDate = new DateTime(2012,07,11,19,05,58,560),
                    CreateBy = null,
                    ModifyDate = new DateTime(2012,08,16,11,54,14,707),
                    ModifyBy = 65,
                    BESpecialRequests1_UID = null,
                    BESpecialRequests2_UID = null,
                    BESpecialRequests3_UID = null,
                    BESpecialRequests4_UID = null,
                    TransferLocation_UID = null,
                    TransferTime = null,
                    TransferOther = null,
                    Tax = 5.9300M,
                    ChannelAffiliateName = "EBIE",
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1.0000000000M,
                    ReservationCurrencyExchangeRateDate = null,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 5.9300M,
                    NumberOfRooms = null,
                    RoomsTax = 0,
                    RoomsExtras = null,
                    RoomsPriceSum = 98.8700M,
                    RoomsTotalAmount = 98.8700M,
                    GroupCode_UID = null,
                    PmsRservationNumber = null,
                    IsOnRequest = null,
                    OnRequestDecisionDate = null,
                    BillingState_UID = null,
                    BillingEmail = null,
                    PropertyBaseCurrencyExchangeRate = 1M,
                    ReservationPartialPaymentDetails = new List<domainReservation.ReservationPartialPaymentDetail>() { },
                    ReservationPaymentDetails = new List<domainReservation.ReservationPaymentDetail>()
                    {
                        new domainReservation.ReservationPaymentDetail()
                        {
                            UID = 2400,
                            PaymentMethod_UID = 1,
                            Reservation_UID = 3114,
                            Amount = 104.8000M,
                            Currency_UID = 34,
                            CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                            CardName = "User",
                            CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                            ExpirationDate = new DateTime(2016,12,28),
                            CreatedDate = new DateTime(2012,07,11,20,05,58,343),
                            ModifiedDate = new DateTime(2012,07,11,20,05,58,343),
                            PaymentGatewayTokenizationIsActive = false,
                            OBTokenizationIsActive = false,
                            CreditCardToken = null,
                            HashCode = null
                        }
                    },
                    ReservationRooms = new List<domainReservation.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 3379,
                            Reservation_UID = 3114,
                            RoomType_UID = 3713,
                            GuestName = "Teste TesteTres",
                            SmokingPreferences = false,
                            DateFrom = new DateTime(2012,10,02,00,00,00,000),
                            DateTo = new DateTime(2012,10,03,00,00,00,000),
                            AdultCount = 2,
                            ChildCount = 1,
                            TotalTax = 0,
                            Package_UID = null,
                            CancellationPolicy = "This information does not exist in your language...!",
                            DepositPolicy = "This information does not exist in your language...!",
                            OtherPolicy = "This information does not exist in your language...!",
                            CancellationPolicyDays = null,
                            ReservationRoomNo = "Orbitz-69WG8I/1",
                            Status = 2,
                            CreatedDate = new DateTime(2012,07,11,19,05,58,640),
                            ModifiedDate = null,
                            RoomName = "Double Suite Sea View",
                            Rate_UID = 4631,
                            IsCanceledByChannels = null,
                            ReservationRoomsPriceSum = 98.8700M,
                            ReservationRoomsExtrasSum = null,
                            ReservationRoomsTotalAmount = 98.8700M,
                            ArrivalTime = null,
                            PmsRservationNumber = null,
                            TPIDiscountIsPercentage = null,
                            TPIDiscountValue = null,
                            IsCancellationAllowed = null,
                            CancellationCosts = false,
                            CancellationValue = null,
                            CancellationPaymentModel = null,
                            CancellationNrNights = null,
                            CommissionType = null,
                            CommissionValue = null,
                            CancellationDate = null,
                            LoyaltyLevel_UID = null,
                            LoyaltyLevelName = null,
                            GuestEmail = null,
                            ReservationRoomChilds = new List<domainReservation.ReservationRoomChild>() { },
                            ReservationRoomDetails = new List<domainReservation.ReservationRoomDetail> ()
                            {
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 8730,
                                    RateRoomDetails_UID = 1470345,
                                    Price = 98.8700M,
                                    ReservationRoom_UID = 3379,
                                    AdultPrice = 98.8700M,
                                    ChildPrice = 0,
                                    CreatedDate = new DateTime(2012,07,11,20,05,58,657),
                                    ModifiedDate = new DateTime(2012,07,11,20,05,58,313),
                                    Date = new DateTime(2012,02,10,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                }
                            },
                            ReservationRoomExtras = new List<domainReservation.ReservationRoomExtra>() { },
                            ReservationRoomTaxPolicies = new List<domainReservation.ReservationRoomTaxPolicy>() { }
                        }
                    }
                },
                new domainReservation.Reservation()
                {
                    UID = 64831,
                    Guest_UID = 63829,
                    Number = "RES000001-1814",
                    Channel_UID = 37,
                    GDSSource = null,
                    Date = new DateTime(2013,06,25,08,45,41,027),
                    TotalAmount = 150.0000M,
                    Adults = 1,
                    Children = 1,
                    Status = 2,
                    Notes = null,
                    InternalNotes = "Notes for room: RES000001-1814/1 Special requests This is my request one This is my request two...",
                    IPAddress = null,
                    TPI_UID = 2015,
                    PromotionalCode_UID = null,
                    ChannelProperties_RateModel_UID = null,
                    ChannelProperties_Value = null,
                    ChannelProperties_IsPercentage = null,
                    InvoicesDetail_UID = null,
                    Property_UID = 1814,
                    CreatedDate = new DateTime(2013,06,25,08,45,44,730),
                    CreateBy = 65,
                    ModifyDate = new DateTime(2013,06,25,08,45,41,027),
                    ModifyBy = 65,
                    BESpecialRequests1_UID = null,
                    BESpecialRequests2_UID = null,
                    BESpecialRequests3_UID = null,
                    BESpecialRequests4_UID = null,
                    TransferLocation_UID = null,
                    TransferTime = null,
                    TransferOther = null,
                    Tax = 0.0000M,
                    ChannelAffiliateName = null,
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 1,
                    ReservationBaseCurrency_UID = 1,
                    ReservationCurrencyExchangeRate = 1.0000000000M,
                    ReservationCurrencyExchangeRateDate = null,
                    ReservationLanguageUsed_UID = 1,
                    TotalTax = 0.0000M,
                    NumberOfRooms = 1,
                    RoomsTax = 0,
                    RoomsExtras = null,
                    RoomsPriceSum = 0.0000M,
                    RoomsTotalAmount = 0.0000M,
                    GroupCode_UID = null,
                    PmsRservationNumber = null,
                    IsOnRequest = false,
                    OnRequestDecisionDate = null,
                    BillingState_UID = null,
                    BillingEmail = null,
                    PropertyBaseCurrencyExchangeRate = 1M,
                    IsMobile = true,
                    IsPaid = true,
                    IsPaidDecisionUser = 65,
                    IsPaidDecisionDate = new DateTime(2013,07,08,18,58,43,940),
                    ReservationPartialPaymentDetails = new List<domainReservation.ReservationPartialPaymentDetail>() { },
                    ReservationPaymentDetails = new List<domainReservation.ReservationPaymentDetail>()
                    {
                        new domainReservation.ReservationPaymentDetail()
                        {
                            UID = 58682,
                            PaymentMethod_UID = 3,
                            Reservation_UID = 64831,
                            Amount = null,
                            Currency_UID = null,
                            CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                            CardName = "User",
                            CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                            ExpirationDate = new DateTime(2013,08,21),
                            CreatedDate = new DateTime(2013,06,25,09,45,47,947),
                            ModifiedDate = new DateTime(2013,04,21,00,00,00,000),
                            PaymentGatewayTokenizationIsActive = false,
                            OBTokenizationIsActive = false,
                            CreditCardToken = null,
                            HashCode = null
                        }
                    },
                    ReservationRooms = new List<domainReservation.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 71853,
                            Reservation_UID = 64831,
                            RoomType_UID = 5820,
                            GuestName = "Carlos; Carlix; ",
                            SmokingPreferences = null,
                            DateFrom = new DateTime(2013,08,11,00,00,00,000),
                            DateTo = new DateTime(2013,08,14,00,00,00,000),
                            AdultCount = 1,
                            ChildCount = 1,
                            TotalTax = null,
                            Package_UID = null,
                            CancellationPolicy = "This information does not exist in your language...!",
                            DepositPolicy = "This information does not exist in your language...!",
                            OtherPolicy = "This information does not exist in your language...!",
                            CancellationPolicyDays = 0,
                            ReservationRoomNo = "RES000001-1814/1",
                            Status = 2,
                            CreatedDate = new DateTime(2013,06,25,08,45,45,417),
                            ModifiedDate = null,
                            RoomName = "Double Suite Sea View",
                            Rate_UID = 12228,
                            IsCanceledByChannels = null,
                            ReservationRoomsPriceSum = 150.0000M,
                            ReservationRoomsExtrasSum = null,
                            ReservationRoomsTotalAmount = 150.0000M,
                            ArrivalTime = null,
                            PmsRservationNumber = null,
                            TPIDiscountIsPercentage = null,
                            TPIDiscountValue = null,
                            IsCancellationAllowed = false,
                            CancellationCosts = false,
                            CancellationValue = null,
                            CancellationPaymentModel = null,
                            CancellationNrNights = null,
                            CommissionType = null,
                            CommissionValue = null,
                            CancellationDate = null,
                            LoyaltyLevel_UID = null,
                            LoyaltyLevelName = null,
                            GuestEmail = null,
                            ReservationRoomChilds = new List<domainReservation.ReservationRoomChild>()
                            {
                                new domainReservation.ReservationRoomChild()
                                {
                                    UID = 4708,
                                    ReservationRoom_UID = 71853,
                                    ChildNo = 0,
                                    Age = 1
                                }
                            },
                            ReservationRoomDetails = new List<domainReservation.ReservationRoomDetail> ()
                            {
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 189926,
                                    RateRoomDetails_UID = 5094327,
                                    Price = 50.0000M,
                                    ReservationRoom_UID = 71853,
                                    AdultPrice = null,
                                    ChildPrice = null,
                                    CreatedDate = new DateTime(2013,06,25,09,45,45,547),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,08,11,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                },
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 189927,
                                    RateRoomDetails_UID = 5094328,
                                    Price = 50.0000M,
                                    ReservationRoom_UID = 71853,
                                    AdultPrice = null,
                                    ChildPrice = null,
                                    CreatedDate = new DateTime(2013,06,25,09,45,45,897),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,08,12,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                },
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 189928,
                                    RateRoomDetails_UID = 5094329,
                                    Price = 50.0000M,
                                    ReservationRoom_UID = 71853,
                                    AdultPrice = null,
                                    ChildPrice = null,
                                    CreatedDate = new DateTime(2013,06,25,09,45,46,180),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,08,13,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                }
                            },
                            ReservationRoomExtras = new List<domainReservation.ReservationRoomExtra>() { },
                            ReservationRoomTaxPolicies = new List<domainReservation.ReservationRoomTaxPolicy>() { }
                        }
                    }
                },
                new domainReservation.Reservation()
                {
                    UID = 44024,
                    Guest_UID = 25718,
                    Number = "RES000011-1635",
                    Channel_UID = 1,
                    GDSSource = null,
                    Date = new DateTime(2013,03,19,16,03,43,243),
                    TotalAmount = 967.7000M,
                    Adults = 2,
                    Children = 0,
                    Status = 1,
                    Notes = string.Empty,
                    InternalNotes = null,
                    IPAddress = "81.90.48.122",
                    TPI_UID = null,
                    PromotionalCode_UID = null,
                    ChannelProperties_RateModel_UID = null,
                    ChannelProperties_Value = null,
                    ChannelProperties_IsPercentage = null,
                    InvoicesDetail_UID = null,
                    Property_UID = 1635,
                    CreatedDate = new DateTime(2013,03,19,16,03,43,243),
                    CreateBy = null,
                    ModifyDate = null,
                    ModifyBy = null,
                    BESpecialRequests1_UID = null,
                    BESpecialRequests2_UID = null,
                    BESpecialRequests3_UID = null,
                    BESpecialRequests4_UID = null,
                    BillingCountry_UID = 157,
                    TransferLocation_UID = null,
                    TransferTime = null,
                    TransferOther = null,
                    Tax = 0.0000M,
                    ChannelAffiliateName = null,
                    PaymentMethodType_UID = 2,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1.0000000000M,
                    ReservationCurrencyExchangeRateDate = null,
                    ReservationLanguageUsed_UID = 4,
                    TotalTax = 140.7000M,
                    NumberOfRooms = 1,
                    RoomsTax = 140.7000M,
                    RoomsExtras = 170.0000M,
                    RoomsPriceSum = 657.0000M,
                    RoomsTotalAmount = 967.7000M,
                    GroupCode_UID = null,
                    PmsRservationNumber = null,
                    IsOnRequest = false,
                    OnRequestDecisionDate = null,
                    BillingState_UID = 2430214,
                    BillingEmail = "tony.santos@visualforma.pt",
                    GuestFirstName = "tony",
                    GuestLastName = "teste",
                    GuestPhone = "123123123",
                    GuestIDCardNumber = "123123",
                    GuestCountry_UID = 157,
                    GuestState_UID = 2430214,
                    GuestCity = "asdasdasd",
                    GuestAddress1 = "asdas ",
                    GuestAddress2 = "asdasd",
                    GuestPostalCode = "asdasd",
                    UseDifferentBillingInfo = false,
                    PropertyBaseCurrencyExchangeRate = 1M,
                    IsMobile = false,
                    IsPaid = null,
                    ReservationPartialPaymentDetails = new List<domainReservation.ReservationPartialPaymentDetail>() { },
                    ReservationPaymentDetails = new List<domainReservation.ReservationPaymentDetail>()
                    {
                    },
                    ReservationRooms = new List<domainReservation.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 48207,
                            Reservation_UID = 44024,
                            RoomType_UID = 5148,
                            GuestName = "tony teste",
                            SmokingPreferences = null,
                            DateFrom = new DateTime(2013,03,27,00,00,00,000),
                            DateTo = new DateTime(2013,03,30,00,00,00,000),
                            AdultCount = 2,
                            ChildCount = 0,
                            TotalTax = 140.7000M,
                            Package_UID = 0,
                            CancellationPolicy = "24h : Se cancelado 24 horas antes da data de chegada, nao sera cobrado nenhum valor.",
                            DepositPolicy = "sem deposito : Nao e necessario qualquer deposito",
                            OtherPolicy = "Geral : polticas gerais do hotel",
                            CancellationPolicyDays = 1,
                            ReservationRoomNo = "RES000011-1635/1",
                            Status = 1,
                            CreatedDate = new DateTime(2013,03,19,16,03,43,727),
                            ModifiedDate = null,
                            RoomName = null,
                            Rate_UID = 8017,
                            IsCanceledByChannels = null,
                            ReservationRoomsPriceSum = 657.0000M,
                            ReservationRoomsExtrasSum = 170.0000M,
                            ReservationRoomsTotalAmount = 967.7000M,
                            ArrivalTime = null,
                            PmsRservationNumber = null,
                            TPIDiscountIsPercentage = null,
                            TPIDiscountValue = null,
                            IsCancellationAllowed = true,
                            CancellationCosts = false,
                            CancellationValue = null,
                            CancellationPaymentModel = null,
                            CancellationNrNights = null,
                            CommissionType = null,
                            CommissionValue = null,
                            CancellationDate = null,
                            LoyaltyLevel_UID = null,
                            LoyaltyLevelName = null,
                            GuestEmail = null,
                            ReservationRoomChilds = new List<domainReservation.ReservationRoomChild>()
                            {
                            },
                            ReservationRoomDetails = new List<domainReservation.ReservationRoomDetail> ()
                            {
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 123267,
                                    RateRoomDetails_UID = 3545164,
                                    Price = 219.0000M,
                                    ReservationRoom_UID = 48207,
                                    AdultPrice = 219.0000M,
                                    ChildPrice = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,740),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,03,27,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                },
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 123268,
                                    RateRoomDetails_UID = 3545165,
                                    Price = 219.0000M,
                                    ReservationRoom_UID = 48207,
                                    AdultPrice = 219.0000M,
                                    ChildPrice = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,787),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,03,28,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                },
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 123269,
                                    RateRoomDetails_UID = 3545166,
                                    Price = 219.0000M,
                                    ReservationRoom_UID = 48207,
                                    AdultPrice = 219.0000M,
                                    ChildPrice = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,820),
                                    ModifiedDate = null,
                                    Date = new DateTime(2013,03,29,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                }
                            },
                            ReservationRoomExtras = new List<domainReservation.ReservationRoomExtra>()
                            {
                                new domainReservation.ReservationRoomExtra()
                                {
                                    UID = 30243,
                                    Extra_UID = 1833,
                                    ReservationRoom_UID = 48207,
                                    Qty = 1,
                                    Total_Price = 0.0000M,
                                    Total_VAT = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,867),
                                    ModifiedDate = null,
                                    ExtraIncluded = true
                                },
                                new domainReservation.ReservationRoomExtra()
                                {
                                    UID = 30244,
                                    Extra_UID = 1833,
                                    ReservationRoom_UID = 48207,
                                    Qty = 3,
                                    Total_Price = 120.0000M,
                                    Total_VAT = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,960),
                                    ModifiedDate = null,
                                    ExtraIncluded = false
                                },
                                new domainReservation.ReservationRoomExtra()
                                {
                                    UID = 30245,
                                    Extra_UID = 1834,
                                    ReservationRoom_UID = 48207,
                                    Qty = 1,
                                    Total_Price = 50.0000M,
                                    Total_VAT = 0.0000M,
                                    CreatedDate = new DateTime(2013,03,19,16,03,43,990),
                                    ModifiedDate = null,
                                    ExtraIncluded = false
                                }
                            },
                            ReservationRoomTaxPolicies = new List<domainReservation.ReservationRoomTaxPolicy>()
                            {
                                new domainReservation.ReservationRoomTaxPolicy()
                                {
                                    UID = 28918,
                                    ReservationRoom_UID = 48207,
                                    BillingType = "3,5",
                                    TaxId = 466,
                                    TaxName = "taxa",
                                    TaxDescription = "deacricao da taxa",
                                    TaxDefaultValue = 75.0000M,
                                    TaxIsPercentage = false,
                                    TaxCalculatedValue = 75.0000M
                                },
                                new domainReservation.ReservationRoomTaxPolicy()
                                {
                                    UID = 28919,
                                    ReservationRoom_UID = 48207,
                                    BillingType = "3,5",
                                    TaxId = 467,
                                    TaxName = "taxa 2",
                                    TaxDescription = "descricao da taxa 2",
                                    TaxDefaultValue = 10.0000M,
                                    TaxIsPercentage = true,
                                    TaxCalculatedValue = 65.7000M
                                }
                            }
                        }
                    }
                },
                new domainReservation.Reservation()
                {
                    UID = 1723,
                    Guest_UID = 4115,
                    Number = "RES000001-893",
                    Channel_UID = 26,
                    GDSSource = null,
                    Date = new DateTime(2012,05,31,18,12,12,847),
                    TotalAmount = 2000.0000M,
                    Adults = 1,
                    Children = 0,
                    Status = 1,
                    Notes = "ISTO É UM TESTE",
                    InternalNotes = null,
                    IPAddress = "127.0.0.1",
                    TPI_UID = null,
                    PromotionalCode_UID = null,
                    ChannelProperties_RateModel_UID = null,
                    ChannelProperties_Value = null,
                    ChannelProperties_IsPercentage = null,
                    InvoicesDetail_UID = null,
                    Property_UID = 1094,
                    CreatedDate = new DateTime(2012,05,31,18,12,12,847),
                    CreateBy = null,
                    ModifyDate = null,
                    ModifyBy = null,
                    BESpecialRequests1_UID = null,
                    BESpecialRequests2_UID = null,
                    BESpecialRequests3_UID = null,
                    BESpecialRequests4_UID = null,
                    BillingCountry_UID = 157,
                    TransferLocation_UID = null,
                    TransferTime = null,
                    TransferOther = null,
                    Tax = 0.0000M,
                    ChannelAffiliateName = string.Empty,
                    PaymentMethodType_UID = 1,
                    ReservationCurrency_UID = 34,
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1.0000000000M,
                    ReservationCurrencyExchangeRateDate = null,
                    ReservationLanguageUsed_UID = null,
                    TotalTax = 0.0000M,
                    NumberOfRooms = 1,
                    RoomsTax = 0.0000M,
                    RoomsExtras = null,
                    RoomsPriceSum = 2000.0000M,
                    RoomsTotalAmount = 2000.0000M,
                    GroupCode_UID = null,
                    PmsRservationNumber = null,
                    IsOnRequest = false,
                    OnRequestDecisionDate = null,
                    UseDifferentBillingInfo = false,
                    PropertyBaseCurrencyExchangeRate = 1M,
                    IsMobile = null,
                    IsPaid = null,
                    ReservationPartialPaymentDetails = new List<domainReservation.ReservationPartialPaymentDetail>() { },
                    ReservationPaymentDetails = new List<domainReservation.ReservationPaymentDetail>()
                    {
                        new domainReservation.ReservationPaymentDetail()
                        {
                            UID = 1072,
                            PaymentMethod_UID = 1,
                            Reservation_UID = 1723,
                            Amount = 2000.0000M,
                            Currency_UID = 34,
                            CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                            CardName = "User",
                            CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                            ExpirationDate = new DateTime(2013,09,01,00,00,00,000),
                            CreatedDate = new DateTime(2012,05,31,19,05,51,377),
                            ModifiedDate = new DateTime(2012,05,31,19,05,51,377),
                            PaymentGatewayTokenizationIsActive = false,
                            OBTokenizationIsActive = false,
                            CreditCardToken = null,
                            HashCode = null
                        }
                    },
                    ReservationRooms = new List<domainReservation.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1795,
                            Reservation_UID = 1723,
                            RoomType_UID = 3167,
                            GuestName = "TESTE",
                            SmokingPreferences = false,
                            DateFrom = new DateTime(2012,06,01,00,00,00,000),
                            DateTo = new DateTime(2012,06,02,00,00,00,000),
                            AdultCount = 1,
                            ChildCount = 0,
                            TotalTax = 0.0000M,
                            Package_UID = null,
                            CancellationPolicy = "Test Cancelation Policy : Test cancelation Policy",
                            DepositPolicy = null,
                            OtherPolicy = null,
                            CancellationPolicyDays = 0,
                            ReservationRoomNo = "RES000001-893/1",
                            Status = 1,
                            CreatedDate = new DateTime(2012,05,31,19,05,51,580),
                            ModifiedDate = null,
                            RoomName = "Test Room 2",
                            Rate_UID = 4119,
                            IsCanceledByChannels = null,
                            ReservationRoomsPriceSum = 2000.0000M,
                            ReservationRoomsExtrasSum = null,
                            ReservationRoomsTotalAmount = 2000.0000M,
                            ArrivalTime = null,
                            PmsRservationNumber = null,
                            TPIDiscountIsPercentage = null,
                            TPIDiscountValue = null,
                            IsCancellationAllowed = null,
                            CancellationCosts = false,
                            CancellationValue = null,
                            CancellationPaymentModel = null,
                            CancellationNrNights = null,
                            CommissionType = null,
                            CommissionValue = null,
                            CancellationDate = null,
                            LoyaltyLevel_UID = null,
                            LoyaltyLevelName = null,
                            GuestEmail = null,
                            ReservationRoomChilds = new List<domainReservation.ReservationRoomChild>()
                            {
                            },
                            ReservationRoomDetails = new List<domainReservation.ReservationRoomDetail> ()
                            {
                                new domainReservation.ReservationRoomDetail()
                                {
                                    UID = 4556,
                                    RateRoomDetails_UID = 1147183,
                                    Price = 2000.0000M,
                                    ReservationRoom_UID = 1795,
                                    AdultPrice = 2000.0000M,
                                    ChildPrice = 0.0000M,
                                    CreatedDate = new DateTime(2012,05,31,19,05,51,593),
                                    ModifiedDate = new DateTime(2012,05,31,19,05,51,347),
                                    Date = new DateTime(2012,06,01,00,00,00,000),
                                    ReservationRoomDetailsAppliedIncentives = null
                                }
                            },
                            ReservationRoomExtras = new List<domainReservation.ReservationRoomExtra>()
                            {
                            },
                            ReservationRoomTaxPolicies = new List<domainReservation.ReservationRoomTaxPolicy>()
                            {
                            }
                        }
                    }
                }
            };
        }

        private void FillObAppSettingsMock()
        {
            settingsList.Add(new Setting
            {
                UID = 1,
                Name = "CreditCardsDummyValue",
                Value = "xxxxxxxxxxxxxxxxxxxxx",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 2,
                Name = "CVVDummyValue",
                Value = "xxx",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 3,
                Name = "OmnibeesNewsCookieValue",
                Value = "version1",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 4,
                Name = "UpdateRatesMaxThreads",
                Value = "5",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 5,
                Name = "NewInsertUpdateDelete_Properties",
                Value = null,
                Category = "All"
            });
            settingsList.Add(new Setting
            {
                UID = 6,
                Name = "<RSAKeyValue><Modulus>49BStnomTkKFC1ERxGA/MhfVuwwNW8JcO52FKXFWfpb2m8a/f0Q3l7FRz6pC8Hs65+BOIwUN/7RxGV+PzIQ4ZwVwEuY7GBQonjXTP3D20yUnXZA/NtGmxWYJlVnD8VUZLQdkKs3hBAfKnWjyOgnGoq7CIFyJ9KKOxRh39hQ+7TE=</Modulus><Exponent>AQAB</Exponent><P>+hwRM4IsoIyrwSlbl01L57gZk41RHSYkcZwnsrjodAc/0KSCPm4JhKn8CKc9Xi8Rjq5ekJ0xSbS9VPAmJrwQrQ==</P><Q>6S3VBl3OF3rVnwg9Cg0bc+FGIfHgbZcCw1gV/k2awmS6GKpQHZ7Dr5UnH+QRXSJ1IQ3PJ6rieEeUsGhZxiarFQ==</Q><DP>ZMj0oYX+R8AH4jGxR9oNEVYdcFkM66sYGnPrh1h9y2u0anYwScn7qer5td72msJq180qLCo711CuztBq/0bfjQ==</DP><DQ>YQ7Zv8el9DIF3ydfuOJRzf8z4Qc8AoG7/bGZnfuRcl7Y81FY/atLCrfLzENzUs/37yU/V+SSVbx90Jvu2kLYLQ==</DQ><InverseQ>aq95HffRtKIVEpCEjyolZxm8ovf5SpfT2dkXzbxd7hIPNuM9mNWu897q9KKv3EyEY36dAryiIetVMhFGK0HzuQ==</InverseQ><D>Dyk6j/VSHkw0AXxMM+b53bITYcbcDrrBG6CQj6EA0hzm3Zgc/3HBR2GgIbNhkBKLaYoWeSMpetZ93mPrNH+qJyTeKIPx7mLGu14GS40uikfYIMhRlihRfNskYfiTXsRA35GFdyTgLCPo+OJXVxqaFZpjwXGoBri7Vlgz9E+LRak=</D></RSAKeyValue>",
                Value = null,
                Category = "All"
            });
            settingsList.Add(new Setting
            {
                UID = 7,
                Name = "AdyenNotificationService",
                Value = "https://mobileservices.omnibees.com/AdyenNotificationService.aspx",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 8,
                Name = "RESTFUL-URL",
                Value = "http://srv-protur3/OB.REST.Services/",
                Category = "All"
            });
            settingsList.Add(new Setting
            {
                UID = 9,
                Name = "NewUpdateRates_Properties",
                Value = "",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 10,
                Name = "NewRateChannelUpdateInsert_v2_Properties",
                Value = "All",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 11,
                Name = "UrlCreditCardInfo",
                Value = "https://192.168.100.103:44390",
                Category = "Omnibees"
            });
            settingsList.Add(new Setting
            {
                UID = 34,
                Name = "ListReservationsMaxDaysWithoutFilters",
                Value = "1",
                Category = "OB.REST"
            });
        }

        private void FillReservationsFilterMock()
        {
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3093,
                Number = "SP-120625-TRIP1297564B1",
                ChannelUid = 9
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3094,
                Number = "SP-120627-CAMA1298522B1",
                ChannelUid = 9
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3095,
                Number = "SP-120627-CAMA1298522B2",
                ChannelUid = 9
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3102,
                Number = "Orbitz-FALQFM",
                ChannelUid = 28
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3113,
                Number = "Orbitz-1TY3RF",
                ChannelUid = 28
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 3114,
                Number = "Orbitz-69WG8I",
                ChannelUid = 28
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 64831,
                Number = "RES000001-1814",
                ChannelUid = 37
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 44024,
                Number = "RES000011-1635",
                ChannelUid = 1
            });
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 1723,
                Number = "RES000001-893",
                ChannelUid = 26
            });
        }

        private void FillGuestsMock()
        {
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 5347,
                Prefix = 1,
                FirstName = "Teste",
                LastName = "TesteTres",
                Address1 = "",
                Address2 = "",
                BillingCountry_UID = null,
                Country_UID = null,
                UserPassword = "OTUzNjIw-MGe5yPWJkOA=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1263,
                Currency_UID = 34,
                Language_UID = 1,
                Email = string.Empty,
                Birthday = null,
                CreatedByTPI_UID = null,
                IsActive = true,
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 646,
                IsDeleted = false
            });
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 86856,
                Prefix = 1,
                FirstName = "Teste 2",
                LastName = "TesteTres 2",
                Address1 = "",
                Address2 = "",
                BillingCountry_UID = null,
                Country_UID = null,
                UserPassword = "OTUzNjIw-MsdfghgthwrthwrGe5yPWJkOA=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1263,
                Currency_UID = 34,
                Language_UID = 1,
                Email = string.Empty,
                Birthday = null,
                CreatedByTPI_UID = null,
                IsActive = true,
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 646,
                IsDeleted = false
            });
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 63829,
                Prefix = 1,
                FirstName = "Carlos",
                LastName = "Eduardo",
                Address1 = "Rua XPTO",
                Address2 = null,
                City = "Lagos",
                PostalCode = "8600",
                BillingCountry_UID = null,
                Country_UID = 159,
                Phone = "282000000",
                MobilePhone = "91000000",
                UserPassword = "NzIxMDAy-CzYvrYwnMz4=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1814,
                Currency_UID = 1,
                Language_UID = 1,
                Email = "fgherthwrterthery@stfhgwert.com",
                Birthday = new DateTime(1981, 06, 16, 00, 00, 00, 000),
                CreatedByTPI_UID = null,
                IsActive = true,
                CreateDate = new DateTime(2013, 06, 25, 00, 00, 00, 000),
                ModifyDate = new DateTime(2013, 07, 01, 00, 00, 00, 000),
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 646,
                IsDeleted = false
            });
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 25718,
                Prefix = 1,
                FirstName = "asd",
                LastName = "sda",
                Address1 = "Rua XPTO",
                Address2 = null,
                City = "Lagos",
                PostalCode = "8600",
                BillingCountry_UID = null,
                Country_UID = 159,
                Phone = "282000000",
                MobilePhone = "91000000",
                UserPassword = "NzIxMDAy-CzYvrYwnMz4=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1814,
                Currency_UID = 1,
                Language_UID = 1,
                Email = "fgherthwrterthery@stfhgwert.com",
                Birthday = new DateTime(1981, 06, 16, 00, 00, 00, 000),
                CreatedByTPI_UID = null,
                IsActive = true,
                CreateDate = new DateTime(2013, 06, 25, 00, 00, 00, 000),
                ModifyDate = new DateTime(2013, 07, 01, 00, 00, 00, 000),
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 646,
                IsDeleted = false
            });
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 4115,
                Prefix = 1,
                FirstName = "TESTE",
                LastName = "TESTE",
                Address1 = string.Empty,
                Address2 = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                BillingPhone = "6346363",
                BillingCountry_UID = null,
                Country_UID = null,
                Phone = "34235423523",
                MobilePhone = null,
                UserPassword = "NTI3NjYw-ah72o3LYUbg=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1094,
                Currency_UID = 34,
                Language_UID = 1,
                Email = "pedro.martins@visualforma.pt",
                Birthday = new DateTime(1980, 06, 19, 00, 00, 00, 000),
                CreatedByTPI_UID = null,
                IsActive = true,
                CreateDate = new DateTime(2012, 05, 31, 00, 00, 00, 000),
                ModifyDate = new DateTime(2012, 09, 14, 12, 29, 11, 300),
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 1011,
                IsDeleted = false
            });
            guestsList.Add(new domainCRM.Guest()
            {
                UID = 0,
                Prefix = 1,
                FirstName = "Test1",
                LastName = "Teste2",
                Address1 = null,
                Address2 = null,
                City = "asdasdasd",
                PostalCode = null,
                BillingCountry_UID = null,
                Country_UID = 157,
                Phone = "123123",
                UserName = "tony.santos@visualforma.pt",
                GuestCategory_UID = 1,
                Property_UID = 463,
                Currency_UID = 34,
                Language_UID = 4,
                Email = "tony.santos@visualforma.pt",
                Birthday = null,
                CreatedByTPI_UID = null,
                CreateDate = new DateTime(2014, 05, 14, 17, 36, 51),
                AllowMarketing = true,
                IDCardNumber = "12312312",
                BillingState_UID = null,
                State_UID = 2430214,
                Client_UID = 476,
                UseDifferentBillingInfo = false,
                IsImportedFromExcel = false
            });
        }

        private void FillChPropsListMock()
        {
            chPropsList.Add(
                new BL.Contracts.Data.Channels.ChannelsProperty()
                {
                    Channel_UID = 1,
                    Property_UID = 1263,
                    IsActive = true,
                    IsActivePrePaymentCredit = true,
                    PrePaymentCreditLimit = 100000,
                    PrePaymentCreditUsed = 0,
                    IsDeleted = false
                }
            );
        }

        private void FillPaymentMethodsMock()
        {
            paymentTypesList.Add(
                new OB.BL.Contracts.Data.Payments.PaymentMethodType
                {
                    UID = 1,
                    Code = 1,
                    Name = "Credit Card",
                    PaymentType = 0,
                    IsBilled = false,
                    Ordering = null,
                    AllowParcialPayments = false,
                });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 2,
                Code = 2,
                Name = "Direct payment at the hotel",
                PaymentType = 2,
                IsBilled = false,
                Ordering = 4,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 3,
                Code = 3,
                Name = "Bank deposit",
                PaymentType = 0,
                IsBilled = false,
                Ordering = null,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 4,
                Code = 4,
                Name = "Faturada",
                PaymentType = 0,
                IsBilled = true,
                Ordering = null,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 5,
                Code = 5,
                Name = "Daily billed",
                PaymentType = 1,
                IsBilled = true,
                Ordering = 1,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 6,
                Code = 6,
                Name = "Daily billed and extras",
                PaymentType = 1,
                IsBilled = true,
                Ordering = 2,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 7,
                Code = 7,
                Name = "Pre payment",
                PaymentType = 1,
                IsBilled = true,
                Ordering = 3,
                AllowParcialPayments = false,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 8,
                Code = 8,
                Name = "PayPal",
                PaymentType = 0,
                IsBilled = false,
                Ordering = 5,
                AllowParcialPayments = true,
            });

            paymentTypesList.Add(new OB.BL.Contracts.Data.Payments.PaymentMethodType
            {
                UID = 9,
                Code = 9,
                Name = "Wire Transfer",
                PaymentType = 1,
                IsBilled = false,
                Ordering = 0,
                AllowParcialPayments = false,
            });
        }

        private void FillLanguagesMock()
        {
            listLanguages.Add(new Language()
            {
                UID = 1,
                Name = "English",
                Code = "en"
            });
            listLanguages.Add(new Language()
            {
                UID = 4,
                Name = "Português BR",
                Code = "pt-BR"
            });
        }

        private void FillRoomTypesMock()
        {
            listRoomTypes.Add(new BL.Contracts.Data.Properties.RoomType()
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
        }

        private void FillRatesMock()
        {
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 4639,
                Name = "EB Long Stay Discount Summer",
                Rate_UID = 4640
            });
        }

        private void FillChannelsLightMock()
        {
            listChannelsLight.Add(new BL.Contracts.Data.Channels.ChannelLight()
            {
                UID = 1,
                Name = "Booking Engine"
            });
            listChannelsLight.Add(new BL.Contracts.Data.Channels.ChannelLight()
            {
                UID = 247,
                Name = "CVC"
            });
        }
        private void FillCurrenciesMock()
        {
            listCurrencies.Add(new Currency()
            {
                UID = 1,
                Name = "Albania Lek",
                Symbol = "ALL",
                CurrencySymbol = "Lek",
                DefaultPositionNumber = null,
                PaypalCurrencyCode = "1"
            });
            listCurrencies.Add(new Currency()
            {
                UID = 16,
                Name = "Brazil Real",
                Symbol = "BRL",
                CurrencySymbol = "R$",
                DefaultPositionNumber = 3,
                PaypalCurrencyCode = "22"
            });
            listCurrencies.Add(new Currency()
            {
                UID = 34,
                Name = "Euro",
                Symbol = "EUR",
                CurrencySymbol = "€",
                DefaultPositionNumber = 1,
                PaypalCurrencyCode = "98"
            });
            listCurrencies.Add(new Currency()
            {
                UID = 109,
                Name = "US Dollar",
                Symbol = "USD",
                CurrencySymbol = "US$",
                DefaultPositionNumber = 2,
                PaypalCurrencyCode = "125"
            });
        }
        private void FillVisualStatesMock()
        {

        }

        private void FillPropertiesLigth()
        {
            listPropertiesLigth.Add(new BL.Contracts.Data.Properties.PropertyLight()
            {
                UID = 1263,
                Name = "Pestana Viking Beach & Spa Resort_",
                BaseCurrency_UID = 34,
                CurrencyISO = "€"
            });
        }

        private void FillPaymentMethodsListMock()
        {
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 1,
                Name = "Visa",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 2,
                Name = "Master Card",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 3,
                Name = "American Express",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 4,
                Name = "Discover",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 5,
                Name = "Hipercard",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 6,
                Name = "Dinners Club",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 7,
                Name = "JCB",
                IsVisible = true
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 8,
                Name = "Electron",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 9,
                Name = "Eurocard",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 10,
                Name = "enRouteCard",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 11,
                Name = "Laser",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 12,
                Name = "Maestro",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 13,
                Name = "Solo",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 14,
                Name = "PayPal",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 15,
                Name = "Aura",
                IsVisible = false
            });
            paymentMethodsList.Add(new PaymentMethod()
            {
                UID = 16,
                Name = "Credit Card / Liberate",
                IsVisible = false
            });
        }

        private void FillLostReservationsListMock()
        {
            listLostReservations.Add(new domainReservation.LostReservation
            {
                UID = 1,
                CheckIn = new DateTime(2012, 3, 10),
                CheckOut = new DateTime(2012, 5, 10),
                CouchBaseId = "aaaa-aaa-aaaa-aaa",
                CreatedDate = new DateTime(2012, 2, 10),
                GuestEmail = "guest@email.com",
                GuestName = "Guest Name",
                NumberOfRooms = 2,
                Property_UID = 1881,
                ReservationTotal = "5000"
            });
            listLostReservations.Add(new domainReservation.LostReservation
            {
                UID = 2,
                CheckIn = new DateTime(2012, 3, 10),
                CheckOut = new DateTime(2012, 5, 10),
                CouchBaseId = "bbbb-bbb-bbbb-bbb",
                CreatedDate = new DateTime(2012, 2, 10),
                GuestEmail = "guest2@email.com",
                GuestName = "Guest Name2",
                NumberOfRooms = 4,
                Property_UID = 1881,
                ReservationTotal = "3000"
            });
        }

        private void FillLostReservationDetailsListMock()
        {
            listCouchBaseLostReservation.Add(Guid.Parse("97a1b877-bb76-4acf-80f5-476755c3b589"), new domainReservation.LostReservationDetail
                {
                    CurrencyUID = 32,
                    CheckIn = new DateTime(2012, 3, 10),
                    CheckOut = new DateTime(2012, 5, 10),
                    DocumentId = "aaaa-aaa-aaaa-aaa",
                    CreatedDate = new DateTime(2012, 2, 10),
                    PropertyUID = 1881,
                    Guest = new domainReservation.LostReservationGuest
                    {
                        Address = "blablabla",
                        Birthday = new DateTime (1993, 10, 10),
                        CityName = "Faro",
                        CityUID = 1,
                        CountryName = "Portugal",
                        CountryUID = 1,
                        Email = "blabla@blabla.com",
                        FirstName = "Guest",
                        LastName = "Name",
                        PhoneNumber = "123456789",
                        VATNumber = "123456789",
                        StateName = "NevadaState",
                        StateUID = 1
                    },
                    Rooms = new List<domainReservation.LostReservationRoom>
                    {
                        new domainReservation.LostReservationRoom
                        {
                            Adults = 1,
                            CheckIn = new DateTime(2012, 3, 10),
                            CheckOut = new DateTime(2012, 5, 10),
                            CurrencyUID = 1,
                            GuestName = "Guest",
                            Nights = 1,
                            UID = 1
                        }
                    }
            });
        }

        private void FillReservationStatusListMock()
        {
            listReservationStatus.Add(new domainReservation.ReservationStatus
            {
                UID = 1,
                Name = "Booked",
                ReservationStatusLanguages = new List<domainReservation.ReservationStatusLanguage>
                {
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 1,
                        Language_UID = 3,
                        Name = "Reservado"
                    },
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 1,
                        Language_UID = 4,
                        Name = "Reservada"
                    },
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 1,
                        Language_UID = 8,
                        Name = "Confirmada"
                    }
                }
            });
            listReservationStatus.Add(new domainReservation.ReservationStatus
            {
                UID = 2,
                Name = "Cancelled",
                ReservationStatusLanguages = new List<domainReservation.ReservationStatusLanguage>
                {
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 2,
                        Language_UID = 3,
                        Name = "Cancelado"
                    },
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 2,
                        Language_UID = 4,
                        Name = "Cancelada"
                    },
                    new domainReservation.ReservationStatusLanguage
                    {
                        ReservationStatus_UID = 2,
                        Language_UID = 8,
                        Name = "Cancelada"
                    }
                }
            });
        }
        #endregion


        #region Mock Repo Methods General
        private void MockSettingsRepo()
        {
            _obAppSettingsRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
                .Returns((ListSettingRequest req) =>
                {
                    if (req != null)
                    {
                        if (req.Names != null && req.Names.Count() == 1)
                        {
                            return settingsList.Where(x => x.Name == req.Names[0]).ToList();
                        }
                    }
                    return null;
                });
        }

        private void MockReservationFiltersRepo()
        {
            int total = -1;
            _reservationsFilterRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationFilterCriteria>(),out total,It.IsAny<bool>()))
                .Returns((ListReservationFilterCriteria request, int t, bool b) =>
                {
                    if (request.ChannelUIDs != null && request.ChannelUIDs.Any())
                    {
                        return reservationFilterList.Where(x => x.ChannelUid.HasValue && request.ChannelUIDs.Contains(x.ChannelUid.Value)).Select(x => x.UID);
                    }

                    return new List<long>();
                });
        }

        private void MockReservationRepo()
        {

        }

        private void MockGroupRulesRepo()
        {
            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<GetGroupRuleCriteria>()))
                .Returns((GetGroupRuleCriteria criteria) =>
                {
                    return groupRuleList.AsQueryable().FirstOrDefault(x => x.RuleType == criteria.RuleType);
                });
        }

        private void MockObCrmRepo()
        {
            _iOBCRMRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<ListGuestLightRequest>()))
                .Returns((ListGuestLightRequest request) =>
                {
                    return guestsList.Where(x => request.UIDs.Contains(x.UID)).ToList();
                });

            _iOBCRMRepoMock.Setup(x => x.InsertGuestReservation(It.IsAny<InsertGuestReservationRequest>()))
                .Returns((InsertGuestReservationRequest req) =>
                {
                    InsertGuestReservationResponse response = new InsertGuestReservationResponse();

                    int i = 0;
                    response.Result = new OB.BL.Contracts.Data.CRM.Guest()
                    {
                        Index = i,
                        UID = req.Guest.UID
                    };
                    response.Succeed();

                    return response;
                });

            _iOBCRMRepoMock.Setup(x => x.ListTpiProperty(It.IsAny<ListTPIPropertyRequest>()))
                .Returns((ListTPIPropertyRequest req) =>
                {
                    return new List<domainCRM.TPIProperty>() { };
                });
        }

        private void MockObPropertyRepo()
        {
            _iOBPPropertyRepoMock.Setup(x => x.ListChannelsProperty(It.IsAny<ListChannelsPropertyRequest>()))
                .Returns((ListChannelsPropertyRequest request) =>
                {
                    if (request != null)
                    {
                        if (request.ChannelUIDs != null && request.PropertyUIDs != null)
                        {
                            return chPropsList.Where(y => request.ChannelUIDs.Contains(y.Channel_UID) && request.PropertyUIDs.Contains(y.Property_UID)).ToList();
                        }

                        return null;
                    }

                    return null;
                });
            _iOBPPropertyRepoMock.Setup(x => x.ListLanguages(It.IsAny<ListLanguageRequest>()))
                .Returns((ListLanguageRequest req) =>
                {
                    var ret = new List<Language>() { };

                    if (req != null)
                    {
                        if (req.UIDs.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.UID).Contains(y.UID)).ToList();
                        }
                        else if (req.Codes.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.Code).Contains(y.Code)).ToList();
                        }
                        else if (req.Names.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.Name).Contains(y.Name)).ToList();
                        }
                    }

                    return ret;
                });

            _iOBPPropertyRepoMock.Setup(x => x.GetPropertySecurityConfiguration(It.IsAny<ListPropertySecurityConfigurationRequest>()))
                .Returns((ListPropertySecurityConfigurationRequest req) =>
                {
                    return new List<BL.Contracts.Data.Properties.PropertySecurityConfiguration>()
                    {
                        new BL.Contracts.Data.Properties.PropertySecurityConfiguration()
                    };
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListRoomTypes(It.IsAny<ListRoomTypeRequest>()))
                .Returns((ListRoomTypeRequest req) =>
                {
                    return listRoomTypes.Where(y => req.UIDs.Contains(y.UID)).ToList();
                });

            _iOBPPropertyRepoMock.Setup(y => y.ListPropertiesLight(It.IsAny<ListPropertyRequest>()))
                .Returns((ListPropertyRequest req) =>
                {
                    if (req.UIDs != null && req.UIDs.Any())
                        return listPropertiesLigth.Where(z => req.UIDs.Contains(z.UID)).ToList();

                    return null;
                });
        }
        private void MockObPaymentMethodRepo()
        {
            _iOBPaymentMethodMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>()))
                .Returns((ListPaymentMethodTypesRequest request) =>
                {
                    if (request.Codes != null)
                        return paymentTypesList.Where(y => request.Codes.Contains(y.Code)).ToList();
                    else if (request.UIDs != null)
                        return paymentTypesList.Where(y => request.UIDs.Contains(y.UID)).ToList();
                    else if (request.Names != null)
                        return paymentTypesList.Where(y => request.Names.Contains(y.Name)).ToList();

                    return null;
                });

            _iOBPaymentMethodMock.Setup(x => x.ListPaymentMethods(It.IsAny<ListPaymentMethodsRequest>()))
                .Returns((ListPaymentMethodsRequest req) =>
                {
                    if (req.UIDs != null)
                        return paymentMethodsList.Where(y => req.UIDs.Contains(y.UID)).ToList();

                    return null;
                });
        }

        private void MockSecurityRepo()
        {
            _obSecurityRepoMock.Setup(x => x.DecryptCreditCards(It.IsAny<ListCreditCardRequest>()))
                .Returns((ListCreditCardRequest req) =>
                {
                    return new List<string>() { "" };
                });
        }

        private void MockDepositPolicyRepo()
        {
            _depositPolicyRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<ListDepositPoliciesRequest>()))
                .Returns((ListDepositPoliciesRequest req) =>
                {
                    return new List<OB.BL.Contracts.Data.Rates.DepositPolicy>();
                });
        }

        private void MockCancelationPolicyRepo()
        {
            _cancelationPolicyRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<ListCancellationPoliciesRequest>()))
                .Returns((ListCancellationPoliciesRequest req) =>
                {
                    return new List<OB.BL.Contracts.Data.Rates.CancellationPolicy>();
                });
        }

        private void MockOtherPolicyRepo()
        {
            _otherPolicyRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<GetOtherPoliciesRequest>()))
                .Returns((GetOtherPoliciesRequest req) =>
                {
                    return null;
                });
        }

        private void MockReservationsRoomsRepo()
        {
            _reservationRoomRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationRoom>()))
                .Returns((domainReservation.ReservationRoom entity) =>
                {
                    return entity;
                });
        }

        private void MockResRoomDetailsRepo()
        {
            _resRoomDetailsRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationRoomDetail>()))
                .Returns((domainReservation.ReservationRoomDetail entity) =>
                {
                    return entity;
                });
        }

        private int _reservationsAdditionalDataId = 0;
        private void MockReservationsAdditionalDataRepo()
        {
            _reservationsAdditionalDataRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationsAdditionalData>()))
                .Returns((domainReservation.ReservationsAdditionalData entity) =>
                {
                    entity.UID = ++_reservationsAdditionalDataId;

                    return entity;
                });
        }

        private void MockObRateToomDetailsForResRepo()
        {
            _obRateToomDetailsForResRepoMock.Setup(x => x.UpdateRateRoomDetailAllotments(It.IsAny<UpdateRateRoomDetailAllotmentsRequest>()))
                .Returns(0);
        }

        private void MockObRatesRepo()
        {
            _obRatesRepoMock.Setup(x => x.ListRatesForReservation(It.IsAny<ListRatesForReservationRequest>()))
                .Returns((ListRatesForReservationRequest req) =>
                {
                    return listRates.Where(y => req.RateUIDs.Contains(y.UID)).ToList();
                });
        }

        private void MockObChannelsRepo()
        {
            _obChannelsRepoMock.Setup(x => x.ListChannelLight(It.IsAny<ListChannelLightRequest>()))
                .Returns((ListChannelLightRequest req) =>
                {
                    if (req.ChannelUIDs != null && req.ChannelUIDs.Any())
                        return listChannelsLight.Where(y => req.ChannelUIDs.Contains(y.UID)).ToList();

                    return null;
                });
        }
        private void MockObCurrenciesRepo()
        {
            _obCurrencyRepoMock.Setup(y => y.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(listCurrencies.ToList());
        }

        private void MockVisualStates()
        {
            _visualStateRepoMock.Setup(y => y.GetQuery())
                .Returns(listVisualStates.AsQueryable());
        }

        private void MockObReservationLookups()
        {
            _obReservationLookupsRepoMock.Setup(x => x.ListReservationLookups(It.IsAny<ListReservationLookupsRequest>()))
                .Returns(new BL.Contracts.Data.Reservations.ReservationLookups
                {

                });
        }
        
        private void MockSqlManager()
        {
            _sqlManagerRepoMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                .Returns(1);

            _sqlManagerRepoMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                    .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                    {
                        new OB.BL.Contracts.Data.Properties.Inventory
                        {

                        }
                    });
        }

      


        private void MockLostReservations()
        {
            _lostReservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListLostReservationCriteria>()))
                .Returns(listLostReservations.AsQueryable);
        }

        private void MockCouchBaseLostReservations()
        {
            var totalRecords = -1;

            _couchBaseRepoMock
                .Setup(x => x.FindByUids(out totalRecords, It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>())).Returns(listCouchBaseLostReservation);
        }

        private void MockReservationStatusRepo()
        {
            _reservationStatusRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationStatusCriteria>()))
                .Returns((ListReservationStatusCriteria criteria) =>
                    {
                        return listReservationStatus.Where(y => criteria.UIDs.Contains(y.UID)).AsQueryable();
                    });
        }

        #endregion


        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod]
        [TestCategory("ListReservation UnitTest")]
        public void Test_ListReservation_FindSingleReservationPaging()
        {
            var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
            {
                RequestGuid = Guid.NewGuid(),
                ChannelUIDs = new List<long> { 28 },
                PageIndex = 1,
                PageSize = 2,
                IncludeGuests = false,
                IncludeReservationPartialPaymentDetails = false,
                IncludeReservationRoomChilds = false,
                IncludeReservationPaymentDetail = false,
                IncludeReservationRoomDetails = false,
                IncludeReservationRoomExtras = false,
                IncludeReservationRooms = false,
                RuleType = Reservation.BL.Constants.RuleType.Omnibees
            };

            var reservationResponse = ReservationPOCO.ListReservations(findReservationRequestObj);

            Assert.AreNotEqual(null, reservationResponse.Result);
            Assert.AreEqual(1, reservationResponse.Result.Count);
            Assert.AreEqual(3102, reservationResponse.Result.First().UID);
            Assert.IsTrue(reservationResponse.Result.All(x => x.Channel_UID == 28));
        }

        //Passed to unit test because calls a service that return 0 results (the records doesn't exists in the real DB)
        [TestMethod]
        [TestCategory("Insert UnitTest")]
        public void TestInsertReservation_OneBedroom_OneDay_WithCreditCard_Post()
        {
            MockReservationsAdditionalDataRepo();
            var reservationBuilder = new ReservationBuilder().WithNewGuest().WithOneRoom().WithOneDay().WithCreditCardPayment();

            var insertReservationRequestObj = new Reservation.BL.Contracts.Requests.InsertReservationRequest
            {
                Guest = reservationBuilder.guest,
                Reservation = reservationBuilder.reservationDetail,
                GuestActivities = reservationBuilder.guestActivity,
                ReservationRooms = reservationBuilder.reservationRooms,
                ReservationRoomDetails = reservationBuilder.reservationRoomDetails,
                ReservationRoomExtras = reservationBuilder.reservationRoomExtras,
                ReservationRoomChilds = reservationBuilder.reservationRoomChild,
                ReservationPaymentDetail = reservationBuilder.reservationPaymentDetail,
                ReservationExtraSchedules = reservationBuilder.reservationExtraSchedule,
                RequestGuid = Guid.NewGuid(),
                RuleType = Reservation.BL.Constants.RuleType.BE,
                UsePaymentGateway = false
            };
            insertReservationRequestObj.Guest.UserName = string.Empty;

            var reservationResponse = ReservationPOCO.InsertReservation(insertReservationRequestObj);

            Assert.IsTrue(reservationResponse.Result > 0);
            Assert.IsTrue(reservationResponse.Errors == null || reservationResponse.Errors.Count == 0);
            Assert.IsTrue(reservationResponse.Warnings == null || reservationResponse.Warnings.Count == 0);
        }

        #region ListLostReservation

        [TestMethod]
        [TestCategory("ListLostReservation")]
        public void ListLostReservationSimpleWithNoAcessToCouchbase_NoIncludeDetails()
        {
            _lostReservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListLostReservationCriteria>()))
                .Returns(listLostReservations.AsQueryable());

            var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListLostReservationsRequest
            {
                PropertyUids = new List<long>{ 1881 },
                Uids = new List<long> { 1 },
                Orders = new List<Reservation.BL.Contracts.Requests.RequestOrderBase> {new Reservation.BL.Contracts.Requests.RequestOrderBase{OrderBy = "PropertyUid"} }
            };

            var reservationResponse = ReservationPOCO.ListLostReservations(findReservationRequestObj);

            Assert.IsTrue(reservationResponse.Result != null);
            Assert.IsTrue(reservationResponse.Result.Count == 2);
            Assert.IsTrue(reservationResponse.Result.First().UID == 1);
        }

        [TestMethod]
        [TestCategory("ListLostReservation")]
        public void ListLostReservationSimpleWithAcessToCouchbase_IncludeDetails()
        {
            _lostReservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListLostReservationCriteria>()))
                .Returns(listLostReservations.AsQueryable());

            var totalRecords = -1;

            _couchBaseRepoMock
                .Setup(x => x.FindByUids(out totalRecords, It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>())).Returns(listCouchBaseLostReservation);

            var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListLostReservationsRequest
            {
                PropertyUids = new List<long> { 1881 },
                Uids = new List<long> { 1 },
                Orders = new List<Reservation.BL.Contracts.Requests.RequestOrderBase> { new Reservation.BL.Contracts.Requests.RequestOrderBase { OrderBy = "PropertyUid" } },
                IncludeDetails = true
            };

            var reservationResponse = ReservationPOCO.ListLostReservations(findReservationRequestObj);

            Assert.IsTrue(reservationResponse.Result != null);
            Assert.IsTrue(reservationResponse.Result.Count == 2);
            Assert.IsTrue(reservationResponse.Result.First().UID == 1);
            Assert.IsTrue(reservationResponse.Result.First().Detail != null);
        }

        #endregion

        #region ListReservationStatus

        [TestMethod]
        [TestCategory("ReservationStatus")]
        public void ListReservationStatusSimpleWithoutLanguages()
        {
            var request = new Reservation.BL.Contracts.Requests.ListReservationStatusesRequest
            {
                Ids = new HashSet<long>
                {
                    1
                }
            };

            var response = ReservationPOCO.ListReservationStatuses(request);

            Assert.IsTrue(response.Result.Count > 0);
            Assert.AreEqual(response.Result.FirstOrDefault().Name, "Booked");
            Assert.IsTrue(response.Errors == null || response.Errors.Count == 0);
            Assert.IsTrue(response.Warnings == null || response.Warnings.Count == 0);
        }

        [TestMethod]
        [TestCategory("ReservationStatus")]
        public void ListReservationStatusSimpleWithLanguages()
        {
            var request = new Reservation.BL.Contracts.Requests.ListReservationStatusesRequest
            {
                Ids = new HashSet<long>
                {
                    1,
                    2
                },
                LanguageUID = 3
            };

            var response = ReservationPOCO.ListReservationStatuses(request);

            Assert.IsTrue(response.Result.Count > 0);
            Assert.AreEqual(response.Result.FirstOrDefault(x => x.Id == 1).Name, "Reservado");
            Assert.AreEqual(response.Result.FirstOrDefault(x => x.Id == 2).Name, "Cancelado");
            Assert.IsTrue(response.Errors == null || response.Errors.Count == 0);
            Assert.IsTrue(response.Warnings == null || response.Warnings.Count == 0);
        }

        [TestMethod]
        [TestCategory("ReservationStatus")]
        public void ListReservationStatusEmpty()
        {
            var request = new Reservation.BL.Contracts.Requests.ListReservationStatusesRequest
            {
                Ids = new HashSet<long>
                {
                    3
                }
            };

            var response = ReservationPOCO.ListReservationStatuses(request);

            Assert.IsTrue(response.Result.Count == 0);
            Assert.IsTrue(response.Errors == null || response.Errors.Count == 0);
            Assert.IsTrue(response.Warnings == null || response.Warnings.Count == 0);
        }

        #endregion

        [TestMethod]
        [TestCategory("InsertGuestReservation")]
        public void InsertGuestFromChannel()
        {
            var reservationResponse = ReservationPOCO.InsertGuest(new OB.BL.Contracts.Data.CRM.Guest { }, new List<long>(), new OB.Reservation.BL.Contracts.Data.Reservations.Reservation { Channel_UID = 2 },
                new List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomExtra>());

            _iOBCRMRepoMock.Verify(x => x.InsertGuestReservation(It.Is<OB.BL.Contracts.Requests.InsertGuestReservationRequest>(y => !(y.Guest.IsFromBe ?? false))));
        }

        [TestMethod]
        [TestCategory("InsertGuestReservation")]
        public void InsertGuestFromBe()
        {
            var reservationResponse = ReservationPOCO.InsertGuest(new OB.BL.Contracts.Data.CRM.Guest { }, new List<long>(), new OB.Reservation.BL.Contracts.Data.Reservations.Reservation { Channel_UID = 1 },
                new List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomExtra>());

            _iOBCRMRepoMock.Verify(x => x.InsertGuestReservation(It.Is<OB.BL.Contracts.Requests.InsertGuestReservationRequest>(y => y.Guest.IsFromBe == true)));
        }
    }

    #region Aux classes - Used in Reservation Repo
    public class MyMockReservationRepo : IReservationsRepository
    {
        public DbConnection connection { get; set; }

        List<domainReservation.Reservation> _resList = null;
        private static int resCount = 1;
        public MyMockReservationRepo(List<domainReservation.Reservation> resList)
        {
            _resList = resList;
        }
        public IEnumerable<domainReservation.Reservation> Add(IEnumerable<domainReservation.Reservation> entitiesToAdd)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation Add(domainReservation.Reservation entity)
        {
            entity.UID = resCount++;
            _resList.Add(entity);
            return entity;
        }

        public bool Any(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IQueryable<domainReservation.Reservation> ApplyPaging<TOrderBy>(IQueryable<domainReservation.Reservation> query, Expression<Func<domainReservation.Reservation, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IQueryable<domainReservation.Reservation> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<domainReservation.Reservation> query, List<Tuple<Expression<Func<domainReservation.Reservation, TOrderBy>>, SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public IQueryable<domainReservation.Reservation> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<domainReservation.Reservation> query, Expression<Func<domainReservation.Reservation, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public void Attach(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public void AttachAsModified(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public int Count(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<domainReservation.Reservation, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> Delete(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> Delete(IEnumerable<domainReservation.Reservation> entitiesToDelete)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation Delete(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public void Detach(ICollection<domainReservation.Reservation> entities)
        {
            throw new NotImplementedException();
        }

        public void Detach(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> Find(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public bool FindAnyRoomTypesBy_ReservationUID_And_SystemEventCode(long reservationUID, string systemEventCode)
        {
            throw new NotImplementedException();
        }

        public Task<List<domainReservation.Reservation>> FindAsync(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> FindByCheckOut(out int totalRecords, List<long> reservationUIDs, List<long> propertyUIDs, List<long> channelUIDs, List<string> reservationNumbers, List<long> reservationStatusCodes, DateTime? checkOutFrom, DateTime? checkOutTo, bool includeReservationRooms = false, bool includeReservationRoomChilds = false, bool includeReservationRoomDetails = false, bool includeReservationRoomDetailsIncentives = false, bool includeReservationRoomExtras = false, bool includeReservationPaymentDetails = false, bool includeReservationPartialPaymentDetails = false, bool includeReservationRoomTaxPolicies = false, int pageIndex = -1, int pageSize = -1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> FindByCriteria(out int totalRecords, ListReservationCriteria request, int pageIndex = -1, int pageSize = -1, bool returnTotal = false)
        {
            totalRecords = -1;

            var query = FindByReservationUIDs(request.ReservationUIDs,
                                                request.IncludeReservationRooms,
                                                request.IncludeReservationRoomChilds,
                                                request.IncludeReservationRoomDetails,
                                                request.IncludeReservationRoomDetailsAppliedIncentives,
                                                request.IncludeReservationRoomExtras,
                                                request.IncludeReservationPaymentDetail,
                                                request.IncludeReservationPartialPaymentDetails,
                                                request.IncludeReservationRoomTaxPolicies,
                                                -1, -1) as IQueryable<domainReservation.Reservation>;//paging is done afterwards

            if (request.PropertyUIDs != null && request.PropertyUIDs.Count > 0)
                query = query.Where(x => request.PropertyUIDs.Contains(x.Property_UID));

            if (request.ChannelUIDs != null && request.ChannelUIDs.Count > 0)
                query = query.Where(x => x.Channel_UID.HasValue && request.ChannelUIDs.Contains(x.Channel_UID.Value));

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.ReservationStatusCodes != null && request.ReservationStatusCodes.Any())
                query = query.Where(x => request.ReservationStatusCodes.Contains(x.Status));

            if (request.DateFrom.HasValue)
                query = query.Where(x => x.CreatedDate.Value >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
            {
                request.DateTo = request.DateTo.Value.AddDays(1);
                query = query.Where(x => x.CreatedDate.Value < request.DateTo.Value);
            }

            if (request.ModifiedFrom.HasValue)
                query = query.Where(x => x.ModifyDate.Value >= request.ModifiedFrom.Value);

            if (request.ModifiedTo.HasValue)
            {
                request.ModifiedTo = request.ModifiedTo.Value.AddDays(1);
                query = query.Where(x => x.ModifyDate.Value < request.ModifiedTo.Value);
            }

            if (request.TpiIds != null && request.TpiIds.Any())
                query = query.Where(x => x.TPI_UID.HasValue && request.TpiIds.Contains(x.TPI_UID.Value));

            if (request.Filters != null && request.Filters.Any())
                query = query.FilterBy(request.Filters);

            if (request.Orders != null && request.Orders.Any())
                query = query.OrderBy(request.Orders);
            else
                query = query.OrderByDescending(x => x.UID);

            if (request.IncludeReservationRooms || request.IncludeRates || request.IncludeRoomTypes || request.IncludeCancelationCosts)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms);

            if (request.CheckIn.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateFrom >= request.CheckIn.Value));

            if (request.CheckOut.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateTo <= request.CheckOut.Value));

            if (request.IncludeReservationRoomDetails || request.IncludeCancelationCosts)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomDetails));

            if (request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)));

            if (request.IncludeReservationRoomChilds)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomChilds));

            if (request.IncludeReservationRoomExtras || request.IncludeExtras)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomExtras));

            if (request.IncludeReservationRoomExtrasSchedules)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)));

            if (request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies));

            if (request.IncludeReservationPaymentDetail)
                query = QueryableExtensions.Include(query, x => x.ReservationPaymentDetails);

            if (request.IncludeReservationPartialPaymentDetails)
                query = QueryableExtensions.Include(query, x => x.ReservationPartialPaymentDetails);

            if (returnTotal)
                totalRecords = query.Count();

            if (pageIndex > 0 && pageSize > 0)
                query = query.Skip(pageIndex * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return query.ToList();
        }

        public IEnumerable<domainReservation.Reservation> FindByCriteria(ListReservationCriteria request)
        {            

            var query = FindByReservationUIDs(request.ReservationUIDs,
                                                request.IncludeReservationRooms,
                                                request.IncludeReservationRoomChilds,
                                                request.IncludeReservationRoomDetails,
                                                request.IncludeReservationRoomDetailsAppliedIncentives,
                                                request.IncludeReservationRoomExtras,
                                                request.IncludeReservationPaymentDetail,
                                                request.IncludeReservationPartialPaymentDetails,
                                                request.IncludeReservationRoomTaxPolicies,
                                                -1, -1) as IQueryable<domainReservation.Reservation>;//paging is done afterwards

          

            if (request.IncludeReservationRooms || request.IncludeRates || request.IncludeRoomTypes || request.IncludeCancelationCosts)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms);

            //if (request.CheckIn.HasValue)
            //    query = query.Where(x => x.ReservationRooms.Any(y => y.DateFrom >= request.CheckIn.Value));

            //if (request.CheckOut.HasValue)
            //    query = query.Where(x => x.ReservationRooms.Any(y => y.DateTo <= request.CheckOut.Value));

            if (request.IncludeReservationRoomDetails || request.IncludeCancelationCosts)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomDetails));

            if (request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)));

            if (request.IncludeReservationRoomChilds)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomChilds));

            if (request.IncludeReservationRoomExtras || request.IncludeExtras)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomExtras));

            if (request.IncludeReservationRoomExtrasSchedules)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)));

            if (request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies)
                query = QueryableExtensions.Include(query, x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies));

            if (request.IncludeReservationPaymentDetail)
                query = QueryableExtensions.Include(query, x => x.ReservationPaymentDetails);

            if (request.IncludeReservationPartialPaymentDetails)
                query = QueryableExtensions.Include(query, x => x.ReservationPartialPaymentDetails);

            return query.ToList();
        }

        public domainReservation.Reservation FindByNumberEagerly(string number)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindByNumberEagerly(string number, long propertyUID)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindByReservationNumberAndChannelUID(string reservationNumber, long channelUID)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindByReservationNumberAndChannelUIDAndPropertyUID(string reservationNumber, long channelUID, long propertyUID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> FindByReservationUIDs(IEnumerable<long> ids, bool includeReservationRooms = false, bool includeReservationRoomChilds = false, bool includeReservationRoomDetails = false, bool includeReservationRoomDetailsIncentives = false, bool includeReservationRoomExtras = false, bool includeReservationPaymentDetails = false, bool includeReservationPartialPaymentDetails = false, bool includeReservationRoomTaxPolicies = false, int pageIndex = -1, int pageSize = -1)
        {
            var query = _resList.AsQueryable();

            if (ids != null && ids.Count() > 0)
                query = query.Where(x => ids.Contains(x.UID));

            if (pageIndex > 0 && pageSize > 0)
                query = query.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            //Don't call ToList because it will make a Database query at this point. Since this method is called by others in this class,
            //the query shouldn't execute at this point...
            return query;
        }

        public IEnumerable<PEOccupancyAlertQR1> FindByRoomTypeUID_And_Code_And_PropertyUID_And_RoomTypeDate(long roomTypeUID, long propertyUID, List<DateTime> roomTypeDates, string code)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindByUIDEagerly(long uid)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindByUIDEagerly(long uid, long propertyUID)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FindOne(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelOrTPI(long channelUID, long? tpiUID, List<long> propertyUIDs)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelsTpis(List<long> propertyUIDs)
        {
            throw new NotImplementedException();
        }

        public List<domainReservation.ReservationsAdditionalData> FindReservationsAdditionalDataByReservationsUIDs(List<long> reservationIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> FindReservationsLightByCriteria(out int totalRecords, List<long> UIDs, List<long> channelUIDs, List<long> propertyUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false, bool includeReservationRooms = false, int pageIndex = -1, int pageSize = -1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public List<Item> FindReservationTransactionStatusByReservationsUIDs(List<long> reservationIds)
        {
            return reservationIds.Select(x => new Item { ReservationUID = x, TransactionUID = "000000000" }).ToList();
        }

        public IEnumerable<long> FindReservationUIDSByRateRoomsAndDateOfModifOrStay(List<long> propertyUIDs, List<long> rateUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation First(Expression<Func<domainReservation.Reservation, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation FirstOrDefault(Expression<Func<domainReservation.Reservation, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<domainReservation.Reservation> FirstOrDefaultAsync(Expression<Func<domainReservation.Reservation, bool>> predicate = null)
        {
            throw new NotImplementedException();
        }

        public string GenerateReservationNumber(long propertyId)
        {
            return "TEST-" + (resCount++) + propertyId;
        }

        public domainReservation.Reservation Get(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> Get<TOrderBy>(Expression<Func<domainReservation.Reservation, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> Get<TOrderBy>(Expression<Func<domainReservation.Reservation, bool>> criteria, Expression<Func<domainReservation.Reservation, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<domainReservation.Reservation> GetAll()
        {
            throw new NotImplementedException();
        }

        public long GetBookingEngineChannelUID()
        {
            throw new NotImplementedException();
        }

        public IQueryable<domainReservation.Reservation> GetQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<domainReservation.Reservation> GetQuery(Expression<Func<domainReservation.Reservation, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public ReservationBasicInfoForTransactionIdQR1 GetReservationBasicInfoForPaymentGatewayForTransaction(long propertyUID, string transactionId)
        {
            throw new NotImplementedException();
        }

        public ReservationDataContext GetReservationContext(string existingReservationNumber, long channelUID, long tpiUID, long companyUID, long propertyUID, long reservationUID, long currencyUID, long paymentMethodTypeUID, IEnumerable<long> rateUIDs, string guestFirstName, string guestLastName, string guestEmail, string guestUsername, long? languageId, Guid? requestGuid = null)
        {

            var ret = new ReservationDataContext()
            {
                Client_UID = 646,
                PropertyName = "Pestana Viking Beach & Spa Resort_",
                PropertyBaseLanguage_UID = languageId.HasValue ? languageId.Value : 1,
                PropertyBaseCurrency_UID = 34,
                PropertyCountry_UID = 157,
                PropertyCity_UID = null,
                BookingEngineChannel_UID = channelUID,
                Channel_UID = channelUID,
                ChannelName = "Booking Engine",
                ChannelType = 0,
                ChannelOperatorType = 0,
                ChannelHandleCredit = false,
                IsChannelValid = true,
                Guest_UID = null,
                IsFromChannel = false,
                TPIProperty_UID = null,
                TPICompany_CommissionIsPercentage = null,
                TPIProperty_Commission = null,
                TPI_UID = null,
                TPICompany_UID = null,
                SalesmanCommission_UID = 6,
                Salesman_UID = 5,
                SalesmanBaseCommission = 10.00M,
                SalesmanIsBaseCommissionPercentage = false,
                ReservationRoomDetails = null,
                Currency_UID = 34,
                Currency_Symbol = "EUR",
                IsExistingReservation = false,
                ReservationUID = 0,
                ChannelProperty_UID = 3015,
                IsOnRequestEnable = false
            };

            return ret;
        }

        public ReservationDetailSearchQR1 GetReservationDetail(long reservationUID, long languageUID, string languageIso)
        {
            throw new NotImplementedException();
        }

        public int GetReservationTransactionState(string transactionId, long channelId, out long reservationId, out long hangfireId)
        {
            throw new NotImplementedException();
        }

        public ReservationTransactionStatusBasicInfoForReservationUidQR1 GetReservationTransactionStatusBasicInfoForReservationUID(long reservationUID)
        {
            throw new NotImplementedException();
        }

        public ListReservationsExternalSourceResponse ListReservationsExternalSource(ListReservationsExternalSourceRequest request)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(IQueryable<domainReservation.Reservation> source)
        {
            throw new NotImplementedException();
        }

        public void Refresh(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public void SetSequenceReservationNumberRange(int interval)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation Single(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public domainReservation.Reservation SingleOrDefault(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public List<domainReservation.Reservation> ToList(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public Task<List<domainReservation.Reservation>> ToListAsync(Expression<Func<domainReservation.Reservation, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public void Update(domainReservation.Reservation entity)
        {
            throw new NotImplementedException();
        }

        public int UpdateCreditUsed(long propertyUID, long channelUID, string creditType, string isActiveProperty, decimal creditValue)
        {
            throw new NotImplementedException();
        }

        public void UpdateReservationStatus(long reservationId, int reservationStatus, string transactionId, int transactionState, string reservationStatusName, long? userId, bool updateReservationHistory, string paymentGatewayOrderId)
        {
            throw new NotImplementedException();
        }

        public int DeleteAllActivitiesForGuestUID(long guestUID)
        {
            return 1;
        }

        public DbContext dbContext { get; set; }
        public string GenerateReservationNumber(long propertyId, ref IDbTransaction scope)
        {
            return "ResTest";
        }

        public int InsertReservationTransaction(string transactionId, long reservationId, string reservationNumber, long reservationStatus, OB.BL.Constants.ReservationTransactionStatus transactionStatus, long channelId, long hangfireId, int retries)
        {
            return 0;
        }

        public decimal GetExchangeRateBetweenCurrenciesByPropertyId(long baseCurrencyId, long currencyId, long propertyId)
        {
            return 1;
        }

        public int UpdateRateRoomDetailAllotments(Dictionary<long, int> rateRoomDetailsVsAllotmentToAdd, bool validateAllotment, string correlationId = null)
        {
            throw new NotImplementedException();
        }

        public List<ReservationBEOverviewQR1> ListMyAccountReservationsOverview(long UserUID, int UserType, DateTime? DateFrom, DateTime? DateTo)
        {
            throw new NotImplementedException();
        }

        public int ValidateReservation(ValidateReservationCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public int UpdateReservationTransactionStatus(string transactionId, long channelId, Reservation.BL.Constants.ReservationTransactionStatus transactionStatus)
        {
            throw new NotImplementedException();
        }

        public int UpdateReservationVcn(UpdateReservationVcnCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public long GetPropertyIdByReservationId(long reservationId)
        {
            throw new NotImplementedException();
        }

        public List<domainReservation.Reservation> FindReservationByNumber(string reservationNumber)
        {
            throw new NotImplementedException();
        }

        public int UpdateReservationAdditionalDataJson(long id, string reservationAdditionalDataJson)
        {
            throw new NotImplementedException();
        }

        public int InsertReservationAdditionalDataJson(long reservationId, string reservationAdditionalDataJson)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
