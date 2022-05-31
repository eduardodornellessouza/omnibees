using Hangfire;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.CRM;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Services.IntegrationTests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static OB.Reservation.BL.Constants;
using contractReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservation = OB.Domain.Reservations;
using internalObjs = OB.BL.Operations.Internal.BusinessObjects;
//using IReservationHistoryRepository = OB.DL.Common.Repositories.Interfaces.Entity.IReservationHistoryRepository;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class ModifyReservationUnitTest : UnitBaseTest
    {
        private const string MISSED_TRANLATION = "This information does not exist in your language...!";
        private Fixture fixture;

        private IReservationManagerPOCO _reservationManagerPOCO;
        public IReservationManagerPOCO ReservationManagerPOCO
        {
            get
            {
                if (_reservationManagerPOCO == null)
                    _reservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();

                return _reservationManagerPOCO;
            }
            set { _reservationManagerPOCO = value; }

        }


        //DB Mock
        List<Guest> guestsList = null;
        List<domainReservation.Reservation> reservationsList = null;
        List<Contracts.Data.General.Setting> settingsList = null;
        List<domainReservation.ReservationFilter> reservationFilterList = null;
        List<BL.Contracts.Data.Channels.ChannelsProperty> chPropsList = null;
        List<BL.Contracts.Data.Payments.PaymentMethodType> paymentTypesList = null;
        List<Reservation.BL.Contracts.Data.General.Language> listLanguages = null;
        List<BL.Contracts.Data.Properties.RoomType> listRoomTypes = null;
        List<BL.Contracts.Data.Rates.Rate> listRates = null;
        List<BL.Contracts.Data.Channels.ChannelLight> listChannelsLight = null;
        List<Currency> listCurrencies = null;
        List<domainReservation.VisualState> listVisualStates = null;
        List<BL.Contracts.Data.Properties.PropertyLight> listPropertiesLigth = null;
        List<PaymentMethodType> _paymentMethodTypeInDataBase = new List<PaymentMethodType>();
        List<Domain.ChannelProperties> channelPropsList = null;
        List<RateRoomDetailReservation> rateRoomDetailReservationList = null;
        private List<SellRule> sellRulesList = null;
        domainReservation.ReservationsAdditionalData _reservationAdditionalData = null;
        IDbTransaction transaction;

        //Mock SqlManager
        private Mock<ISqlManager> _sqlManagerMock = null;

        // POCO Mock
        private Mock<IReservationHelperPOCO> _reservationHelperMock = null;

        // Repo Mock
        private Mock<IOBAppSettingRepository> _appSettingRepoMock = null;
        private Mock<IOBCancellationPolicyRepository> _cancellationRepoMock = null;
        private Mock<IPortalRepository> _portalMock = null;
        private Mock<IOBDepositPolicyRepository> _depositRepoMock = null;
        private Mock<IOBOtherPolicyRepository> _otherRepoMock = null;
        private Mock<IOBExtrasRepository> _extrasRepoMock = null;
        private Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtra>> _reservationRoomsExtrasRepoMock;
        private Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasAvailableDate>> _reservationRoomsExtrasDatesRepoMock;
        private Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasSchedule>> _reservationRoomsExtrasScheduleRepoMock;
        private Mock<IOBIncentiveRepository> _incentivesRepoMock;
        private Mock<IOBRateRoomDetailsForReservationRoomRepository> _rrdRepoMock;
        private Mock<IOBPaymentMethodTypeRepository> _paymentMethodTypeRepoMock;
        private Mock<IOBChannelRepository> _channelPropertiesRepoMock;
        private Mock<IOBCRMRepository> _tpiRepoMock;
        private Mock<IGroupRulesRepository> _groupRulesRepo;
        private Mock<IOBRateBuyerGroupRepository> _buyerGroupRepo = null;
        private Mock<IOBPromotionalCodeRepository> _promoCodesRepo = null;
        private Mock<IReservationsRepository> _reservationsRepoMock = null;
        private Mock<IReservationHistoryRepository> _reservationhistoryRepoMock = null;
        private Mock<IPaymentGatewayTransactionRepository> _paymentGatewayRepoMock = null;

        private Mock<IReservationsFilterRepository> _reservationsFilterRepoMock = null;
        private Mock<IOBPropertyRepository> _iOBPPropertyRepoMock = null;
        private Mock<IOBPropertyEventsRepository> _propertyEventsRepoMock = null;
        private Mock<IOBSecurityRepository> _obSecurityRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoom>> _reservationRoomRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomDetail>> _resRoomDetailsRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomChild>> _resRoomRoomChildRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationsAdditionalData>> _resAddDataRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomDetailsAppliedIncentive>> _resRoomAppliedIncentiveRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomDetailsAppliedPromotionalCode>> _resRoomAppliedPromotionalCodeRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationRoomTaxPolicy>> _resRoomTaxPolicyRepoMock = null;
        private Mock<IOBRateRepository> _obRatesRepoMock = null;
        private Mock<IVisualStateRepository> _visualStateRepoMock = null;
        private Mock<IOBCurrencyRepository> _obCurrencyRepoMock = null;
        private Mock<IOBChildTermsRepository> _obChildTermsRepoMock = null;
        private Mock<IReservationRoomDetailRepository> _resRoomDetailsRepoSpecMock = null;
        private Mock<IOBReservationLookupsRepository> _OBReservationLookupsRepoMock = null;
        private Mock<IRepository<domainReservation.ReservationStatusLanguage>> _resStatusLangRepoMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            //Initialize lists mock db
            channelPropsList = new List<Domain.ChannelProperties>();
            channelPropsList = new List<Domain.ChannelProperties>();
            reservationsList = new List<domainReservation.Reservation>();
            settingsList = new List<Contracts.Data.General.Setting>();
            reservationFilterList = new List<domainReservation.ReservationFilter>();
            guestsList = new List<Guest>();
            chPropsList = new List<Contracts.Data.Channels.ChannelsProperty>();
            paymentTypesList = new List<PaymentMethodType>();
            listLanguages = new List<Language>();
            listRoomTypes = new List<RoomType>();
            listRates = new List<Rate>();
            listChannelsLight = new List<Contracts.Data.Channels.ChannelLight>();
            listCurrencies = new List<Currency>();
            listVisualStates = new List<domainReservation.VisualState>();
            listPropertiesLigth = new List<PropertyLight>();
            _paymentMethodTypeInDataBase = new List<PaymentMethodType>();
            rateRoomDetailReservationList = new List<RateRoomDetailReservation>();

            // POCO Mock
            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();
            _reservationHelperMock = new Mock<IReservationHelperPOCO>(MockBehavior.Default);

            //SQLManager Mock
            _sqlManagerMock = new Mock<ISqlManager>();

            // Repo Mock
            _appSettingRepoMock = new Mock<IOBAppSettingRepository>(MockBehavior.Default);
            _cancellationRepoMock = new Mock<IOBCancellationPolicyRepository>(MockBehavior.Default);
            _depositRepoMock = new Mock<IOBDepositPolicyRepository>(MockBehavior.Default);
            _otherRepoMock = new Mock<IOBOtherPolicyRepository>(MockBehavior.Default);
            _reservationRoomsExtrasRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtra>>(MockBehavior.Default);
            _reservationRoomsExtrasDatesRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasAvailableDate>>(MockBehavior.Default);
            _reservationRoomsExtrasScheduleRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasSchedule>>(MockBehavior.Default);
            _extrasRepoMock = new Mock<IOBExtrasRepository>(MockBehavior.Default);
            _incentivesRepoMock = new Mock<IOBIncentiveRepository>(MockBehavior.Default);
            _rrdRepoMock = new Mock<IOBRateRoomDetailsForReservationRoomRepository>(MockBehavior.Default);
            _paymentMethodTypeRepoMock = new Mock<IOBPaymentMethodTypeRepository>(MockBehavior.Default);
            _channelPropertiesRepoMock = new Mock<IOBChannelRepository>(MockBehavior.Default);
            _tpiRepoMock = new Mock<IOBCRMRepository>(MockBehavior.Default);
            _groupRulesRepo = new Mock<IGroupRulesRepository>(MockBehavior.Default);
            _buyerGroupRepo = new Mock<IOBRateBuyerGroupRepository>(MockBehavior.Default);
            _promoCodesRepo = new Mock<IOBPromotionalCodeRepository>(MockBehavior.Default);
            _reservationsRepoMock = new Mock<IReservationsRepository>(MockBehavior.Default);
            _reservationsFilterRepoMock = new Mock<IReservationsFilterRepository>(MockBehavior.Default);
            _iOBPPropertyRepoMock = new Mock<IOBPropertyRepository>(MockBehavior.Default);
            _propertyEventsRepoMock = new Mock<IOBPropertyEventsRepository>(MockBehavior.Default);
            _reservationRoomRepoMock = new Mock<IRepository<domainReservation.ReservationRoom>>(MockBehavior.Default);
            _resRoomDetailsRepoMock = new Mock<IRepository<domainReservation.ReservationRoomDetail>>(MockBehavior.Default);
            _resRoomRoomChildRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomChild>>(MockBehavior.Default);
            _obRatesRepoMock = new Mock<IOBRateRepository>();
            _obCurrencyRepoMock = new Mock<IOBCurrencyRepository>();
            _visualStateRepoMock = new Mock<IVisualStateRepository>();
            _resAddDataRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationsAdditionalData>>(MockBehavior.Default);
            _obChildTermsRepoMock = new Mock<IOBChildTermsRepository>(MockBehavior.Default);
            _resRoomAppliedIncentiveRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomDetailsAppliedIncentive>>(MockBehavior.Default);
            _resRoomAppliedPromotionalCodeRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomDetailsAppliedPromotionalCode>>(MockBehavior.Default);
            _resRoomDetailsRepoSpecMock = new Mock<IReservationRoomDetailRepository>(MockBehavior.Default);
            _resRoomTaxPolicyRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomTaxPolicy>>();
            _portalMock = new Mock<IPortalRepository>(MockBehavior.Default);
            _OBReservationLookupsRepoMock = new Mock<IOBReservationLookupsRepository>(MockBehavior.Default);
            _resStatusLangRepoMock = new Mock<IRepository<domainReservation.ReservationStatusLanguage>>(MockBehavior.Default);
            _reservationhistoryRepoMock = new Mock<IReservationHistoryRepository>(MockBehavior.Default);
            _paymentGatewayRepoMock = new Mock<IPaymentGatewayTransactionRepository>(MockBehavior.Default);

            Mock<IDbTransaction> tran = new Mock<IDbTransaction>(MockBehavior.Default);
            transaction = tran.Object;
            UnitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<System.Data.IsolationLevel>())).Returns(transaction);


            //Mock call to Repository Factory
            RepositoryFactoryMock.Setup(x => x.GetOBCancellationPolicyRepository())
                            .Returns(_cancellationRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBDepositPolicyRepository())
                            .Returns(_depositRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBOtherPolicyRepository())
                            .Returns(_otherRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBRateRoomDetailsForReservationRoomRepository())
                            .Returns(_rrdRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBExtrasRepository())
                            .Returns(_extrasRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPaymentMethodTypeRepository())
                            .Returns(_paymentMethodTypeRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBIncentiveRepository())
                            .Returns(_incentivesRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBChannelRepository())
                            .Returns(_channelPropertiesRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBCRMRepository())
                            .Returns(_tpiRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomExtra>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationRoomsExtrasRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomExtrasAvailableDate>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationRoomsExtrasDatesRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomExtrasSchedule>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationRoomsExtrasScheduleRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomChild>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomRoomChildRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetGroupRulesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_groupRulesRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBRateBuyerGroupRepository())
                            .Returns(_buyerGroupRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPromotionalCodeRepository())
                            .Returns(_promoCodesRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationsFilterRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationsFilterRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPropertyRepository())
                            .Returns(_iOBPPropertyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPropertyEventsRepository())
                            .Returns(_propertyEventsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationRoom>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationRoomRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationRoomDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomDetailsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBRateRepository())
                            .Returns(_obRatesRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBCurrencyRepository())
                            .Returns(_obCurrencyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetVisualStateRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_visualStateRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationsAdditionalData>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resAddDataRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBAppSettingRepository())
                            .Returns(_appSettingRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBChildTermsRepository())
                            .Returns(_obChildTermsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomDetailsAppliedIncentive>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomAppliedIncentiveRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomDetailsAppliedPromotionalCode>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomAppliedPromotionalCodeRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationRoomDetailRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomDetailsRepoSpecMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomTaxPolicy>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomTaxPolicyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetPortalRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_portalMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBReservationLookupsRepository())
                           .Returns(_OBReservationLookupsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationStatusLanguage>(It.IsAny<IUnitOfWork>()))
                .Returns(_resStatusLangRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationHistoryRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationhistoryRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetPaymentGatewayTransactionRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_paymentGatewayRepoMock.Object);

            //Mock SqlManager
            RepositoryFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), It.IsAny<DomainScope>()))
                .Returns(_sqlManagerMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<string>()))
                .Returns(_sqlManagerMock.Object);

            //Mock data
            MockReservationFiltersRepo();
            MockReservationHelperPockMethods();
            MockBeginTransactions();
            FillGroupRulesMock();
            FillGuestsMock();
            FillObAppSettingsMock();
            FillLanguagesMock();
            FillRoomTypesMock();
            FillCurrenciesMock();
            FillVisualStatesMock();

            //Mock Repos
            MockGroupRulesRepo();
            MockObCrmRepo();
            InitializeLookupsMock();
        }


        #region Mock Data

        private void FillGuestsMock()
        {
            guestsList.Add(new Guest()
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
            guestsList.Add(new Guest()
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
            guestsList.Add(new Guest()
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
            guestsList.Add(new Guest()
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
            guestsList.Add(new Guest()
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
            guestsList.Add(new Guest()
            {
                UID = 1,
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

        private void FillThePaymentMethodTypeforTest()
        {
            _paymentMethodTypeInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 1,
                    PaymentType = 0,
                    Ordering= null,
                    Name = "Credit Card",
                    IsBilled = false,
                    Code = 1,
                    AllowParcialPayments = false
                });
            _paymentMethodTypeInDataBase.Add(
                     new PaymentMethodType
                     {
                         UID = 2,
                         PaymentType =2,
                         Ordering = 4,
                         Name = "Direct payment at the hotel",
                         IsBilled = false,
                         Code = 2,
                         AllowParcialPayments = false
                     });

            _paymentMethodTypeInDataBase.Add(
                      new PaymentMethodType
                      {
                          UID = 1,
                          PaymentType = 0,
                          Ordering = null,
                          Name = "Credit Card",
                          IsBilled = false,
                          Code = 1,
                          AllowParcialPayments = false
                      });

            _paymentMethodTypeInDataBase.Add(
                 new PaymentMethodType
                 {
                     UID = 1,
                     PaymentType = 0,
                     Ordering = null,
                     Name = "Credit Card",
                     IsBilled = false,
                     Code = 1,
                     AllowParcialPayments = false
                 });

            _paymentMethodTypeInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 1,
                    PaymentType = 0,
                    Ordering = null,
                    Name = "Credit Card",
                    IsBilled = false,
                    Code = 1,
                    AllowParcialPayments = false
                });

            _paymentMethodTypeInDataBase.Add(
                  new PaymentMethodType
                  {
                      UID = 1,
                      PaymentType = 0,
                      Ordering = null,
                      Name = "Credit Card",
                      IsBilled = false,
                      Code = 1,
                      AllowParcialPayments = false
                  });
        }

        private void FillObAppSettingsMock()
        {
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 1,
                Name = "CreditCardsDummyValue",
                Value = "xxxxxxxxxxxxxxxxxxxxx",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 2,
                Name = "CVVDummyValue",
                Value = "xxx",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 3,
                Name = "OmnibeesNewsCookieValue",
                Value = "version1",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 4,
                Name = "UpdateRatesMaxThreads",
                Value = "5",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 5,
                Name = "NewInsertUpdateDelete_Properties",
                Value = null,
                Category = "All"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 6,
                Name = "<RSAKeyValue><Modulus>49BStnomTkKFC1ERxGA/MhfVuwwNW8JcO52FKXFWfpb2m8a/f0Q3l7FRz6pC8Hs65+BOIwUN/7RxGV+PzIQ4ZwVwEuY7GBQonjXTP3D20yUnXZA/NtGmxWYJlVnD8VUZLQdkKs3hBAfKnWjyOgnGoq7CIFyJ9KKOxRh39hQ+7TE=</Modulus><Exponent>AQAB</Exponent><P>+hwRM4IsoIyrwSlbl01L57gZk41RHSYkcZwnsrjodAc/0KSCPm4JhKn8CKc9Xi8Rjq5ekJ0xSbS9VPAmJrwQrQ==</P><Q>6S3VBl3OF3rVnwg9Cg0bc+FGIfHgbZcCw1gV/k2awmS6GKpQHZ7Dr5UnH+QRXSJ1IQ3PJ6rieEeUsGhZxiarFQ==</Q><DP>ZMj0oYX+R8AH4jGxR9oNEVYdcFkM66sYGnPrh1h9y2u0anYwScn7qer5td72msJq180qLCo711CuztBq/0bfjQ==</DP><DQ>YQ7Zv8el9DIF3ydfuOJRzf8z4Qc8AoG7/bGZnfuRcl7Y81FY/atLCrfLzENzUs/37yU/V+SSVbx90Jvu2kLYLQ==</DQ><InverseQ>aq95HffRtKIVEpCEjyolZxm8ovf5SpfT2dkXzbxd7hIPNuM9mNWu897q9KKv3EyEY36dAryiIetVMhFGK0HzuQ==</InverseQ><D>Dyk6j/VSHkw0AXxMM+b53bITYcbcDrrBG6CQj6EA0hzm3Zgc/3HBR2GgIbNhkBKLaYoWeSMpetZ93mPrNH+qJyTeKIPx7mLGu14GS40uikfYIMhRlihRfNskYfiTXsRA35GFdyTgLCPo+OJXVxqaFZpjwXGoBri7Vlgz9E+LRak=</D></RSAKeyValue>",
                Value = null,
                Category = "All"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 7,
                Name = "AdyenNotificationService",
                Value = "https://mobileservices.omnibees.com/AdyenNotificationService.aspx",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 8,
                Name = "RESTFUL-URL",
                Value = "http://srv-protur3/OB.REST.Services/",
                Category = "All"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 9,
                Name = "NewUpdateRates_Properties",
                Value = "",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 10,
                Name = "NewRateChannelUpdateInsert_v2_Properties",
                Value = "All",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 11,
                Name = "UrlCreditCardInfo",
                Value = "https://192.168.100.103:44390",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 12,
                Name = "IgnoreReservationTransactionTime",
                Value = "0",
                Category = "Omnibees"
            });
            settingsList.Add(new Contracts.Data.General.Setting
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

        private enum PORule
        {
            None = 0,
            IsOBTPIRule = 1,
            IsPOTPIRule = 2,
            IsRepresentativeRule = 3,
            IsOperatorRule = 4,
            IsRepresentativeAndOperatorRule = 5,
        };
        private void CreatePortalRules(PORule rule)
        {
            switch (rule)
            {
                case PORule.IsOBTPIRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 3,
                            ExternalName = "A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 3,
                            ExternalName = "A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 15,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsPOTPIRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 1,
                            ExternalName = "Empresa A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 15,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 1,
                            ExternalName = "Empresa A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 18,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsOperatorRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsRepresentativeRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 0,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 10,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 15,
                            CurrencyValueUID = 34
                        }
                    };
                    break;
                case PORule.IsRepresentativeAndOperatorRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 0,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 10,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 15,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = true,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                default:
                    sellRulesList = new List<SellRule>();
                    break;
            }
        }
        #endregion

        #region Mock Repo Call's
        private void MockReservationFiltersRepo()
        {
            _reservationsFilterRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationFilter>()))
                .Returns((domainReservation.ReservationFilter entity) =>
                {
                    return entity;
                });

            int totalRecords = 1;
            _reservationsFilterRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationFilterCriteria>(), out totalRecords, It.IsAny<bool>())).Returns((ListReservationFilterCriteria rf, int total, bool returnTotal) =>
            {
                if (rf?.ReservationUIDs != null && rf.ReservationUIDs.Any())
                {
                    return reservationsList.Where(x => rf.ReservationUIDs.Contains(x.UID)).Select(x => x.UID).ToList();
                }
                return null;
            });
        }

        private void MockGetReservationLookups()
        {
            _reservationHelperMock.Setup(x => x.GetReservationLookups(It.IsAny<ListReservationCriteria>(), It.IsAny<IEnumerable<domainReservation.Reservation>>())).Returns(
                (ListReservationCriteria r, IEnumerable<domainReservation.Reservation> domainList) =>
                {
                    return new ReservationLookups { GuestsLookup = new Dictionary<long, Guest> { { 1, new Guest() } } };
                });
        }

        private void MockResAddDataRepo()
        {

            _resAddDataRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationsAdditionalData>()))
                .Returns((domainReservation.ReservationsAdditionalData req) =>
                {
                    return req;
                });

            _reservationAdditionalData = new domainReservation.ReservationsAdditionalData
            {
                Reservation_UID = 1,
                ReservationAdditionalDataJSON = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    ReservationRoomList = new List<object>
                    {
                        new
                        {
                            ReservationRoom_UID = 179595,
                            ReservationRoomNo = "RES000024-1806/1",
                            CancellationPolicy = new
                            {
                                UID = 12151,
                                Name = "politica_cancelamento_5",
                                Description = "0 dias 0€",
                                Days = 0,
                                IsCancellationAllowed = true,
                                CancellationCosts = true,
                                Value = 0,
                                PaymentModel = 1
                            },
                            OtherPolicy = new
                            {
                                UID = 0,
                                OtherPolicy_Name = (string) null,
                                OtherPolicy_Description = (string) null,
                                TranslatedName = (string) null,
                                TranslatedDescription = (string) null,
                                Property_UID = 0,
                                IsDeleted = false,
                            },
                            TaxPolicies = new List<object>
                            {
                                new
                                {
                                    UID = 0,
                                    BillingType = "2,5",
                                    TaxId = 713,
                                    TaxName = "taxa_26",
                                    TaxDescription = "por estada / por pessoa",
                                    TaxDefaultValue = 15,
                                    TaxIsPercentage = false,
                                    TaxCalculatedValue = 15,
                                }
                            },
                            ExternalSellingInformationByRule = new List<object>
                            {
                                new
                                {
                                    KeeperType = 2,
                                    ReservationRoomsTotalAmount = 63,
                                    ReservationRoomsPriceSum = 48,
                                    TotalTax = 15,
                                    PricesPerDay = new List<object>
                                    {
                                        new
                                        {
                                            Date = "2017-04-04T00:00:00",
                                            Price = 8,
                                        },
                                        new
                                        {
                                            Date = "2017-04-05T00:00:00",
                                            Price = 8,
                                        },
                                        new
                                        {
                                            Date = "2017-04-06T00:00:00",
                                            Price = 8,
                                        },
                                        new
                                        {
                                            Date = "2017-04-07T00:00:00",
                                            Price = 8,
                                        },
                                        new
                                        {
                                            Date = "2017-04-08T00:00:00",
                                            Price = 8,
                                        },
                                        new
                                        {
                                            Date = "2017-04-09T00:00:00",
                                            Price = 8,
                                        },
                                    }
                                }
                            }
                        }
                    },
                    ExternalSellingReservationInformationByRule = new List<object>
                    {
                        new
                        {
                            KeeperType = 2,
                            TotalAmount = 582,
                            RoomsTotalAmount = 582,
                            RoomsPriceSum = 552,
                            TotalTax = 30,
                            IsPaid = false,
                            TotalCommission = 5,
                            CurrencyUID = 16,
                            CurrencySymbol = (string) null,
                            ExchangeRate = 4.3393168771,
                            Markup = 5,
                            MarkupType = 1,
                            MarkupIsPercentage = true,
                            Commission = 5,
                            CommissionType = 1,
                            CommissionIsPercentage = true,
                            Tax = 0,
                            TaxIsPercentage = false,
                        }
                    }
                }),
            };
            var resAdditionalDataList = new List<domainReservation.ReservationsAdditionalData>
            {
                _reservationAdditionalData
            };
            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(resAdditionalDataList.AsQueryable());
        }

        private void MockAppRepository()
        {
            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
                .Returns(new List<OB.BL.Contracts.Data.General.TripAdvisorConfiguration>() { });

            _appSettingRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
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

        private void MockReservationRepo()
        {
            _reservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationCriteria>())).Returns((ListReservationCriteria crit) =>
            {
                if (crit?.ReservationUIDs != null && crit.ReservationUIDs.Any())
                {
                    return reservationsList.Where(x => crit.ReservationUIDs.Contains(x.UID)).ToList();
                }
                return null;
            });

            _reservationsRepoMock.Setup(x => x.dbContext)
                .Returns(() =>
                {
                    Mock<System.Data.Entity.DbContext> dbContextMock = new Mock<System.Data.Entity.DbContext>();

                    dbContextMock.Setup(y => y.Database).Returns(() =>
                    {
                        Mock<System.Data.Entity.Database> databaseMock = new Mock<System.Data.Entity.Database>();
                        return databaseMock.Object;
                    });

                    return dbContextMock.Object;
                });
        }

        private void MockObPropertyRepo(ReservationDataBuilder resBuilder)
        {
            _iOBPPropertyRepoMock.Setup(x => x.ListLanguages(It.IsAny<ListLanguageRequest>()))
                .Returns((ListLanguageRequest req) =>
                {
                    var ret = new List<Contracts.Data.General.Language>() { };

                    if (req != null)
                    {
                        if (req.UIDs.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.UID).Contains(y.UID)).Select(y => new Contracts.Data.General.Language { UID = y.UID, Code = y.Code, Name = y.Name }).ToList();
                        }
                        else if (req.Codes.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.Code).Contains(y.Code)).Select(y => new Contracts.Data.General.Language { UID = y.UID, Code = y.Code, Name = y.Name }).ToList();
                        }
                        else if (req.Names.Any())
                        {
                            ret = listLanguages.Where(y => listLanguages.Select(z => z.Name).Contains(y.Name)).Select(y => new Contracts.Data.General.Language { UID = y.UID, Code = y.Code, Name = y.Name }).ToList();
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

            var roomTypes = new List<RoomType>();
            int i = 3709;
            resBuilder.InputData.reservationRooms.ForEach(x =>
            {
                roomTypes.Add(new RoomType()
                {
                    UID = i++,
                    Name = "Builder Test",
                    Qty = 37,
                    ShortDescription = null,
                    AcceptsChildren = true,
                    AcceptsExtraBed = null,
                    AdultMaxOccupancy = resBuilder.InputData.reservationDetail.Adults,
                    AdultMinOccupancy = 1,
                    ChildMaxOccupancy = resBuilder.InputData.reservationDetail.Children,
                    ChildMinOccupancy = 0,
                    BasePrice = resBuilder.InputData.reservationRoomDetails[0].AdultPrice,
                    CreatedDate = DateTime.Now,
                    Description = "This is a test based on the builder",
                    IsBase = true,
                    IsDeleted = false,
                    Value = null,
                    IsPercentage = null,
                    IsValueDecrease = null,
                    MaxValue = 500.00M,
                    MinValue = 5.00M,
                    MaxFreeChild = null
                });
            });

            _iOBPPropertyRepoMock.Setup(x => x.ListRoomTypes(It.IsAny<ListRoomTypeRequest>()))
                .Returns((ListRoomTypeRequest req) =>
                {
                    return roomTypes;
                });

            var propUid = resBuilder.InputData.reservationDetail.Property_UID;
            var currencyUid = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault();
            listPropertiesLigth.Add(new BL.Contracts.Data.Properties.PropertyLight()
            {
                UID = propUid,
                Name = "hotel test",
                BaseCurrency_UID = currencyUid,  //we use the same as the reservation
                CurrencyISO = listCurrencies.Where(x => x.UID == currencyUid).Select(x => x.CurrencySymbol).SingleOrDefault()
            });

            _iOBPPropertyRepoMock.Setup(y => y.ListPropertiesLight(It.IsAny<ListPropertyRequest>()))
                .Returns((ListPropertyRequest req) =>
                {
                    if (req.UIDs != null && req.UIDs.Any())
                        return listPropertiesLigth.Where(z => req.UIDs.Contains(z.UID)).ToList();

                    return null;
                });

            _iOBPPropertyRepoMock.Setup(x => x.UpdateInventoryDetails(It.IsAny<UpdateInventoryDetailsRequest>()))
                .Returns(new List<Inventory>());
        }
        private void MockObPaymentMethodRepo()
        {
            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>()))
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
        }

        private void MockPropertyEventsRepo()
        {
            _propertyEventsRepoMock.Setup(x => x.InsertPropertyQueue(It.IsAny<Contracts.Requests.InsertPropertyQueueRequest>()))
                .Returns(1);
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
            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<ListDepositPoliciesRequest>()))
                .Returns((ListDepositPoliciesRequest req) =>
                {
                    return new List<OB.BL.Contracts.Data.Rates.DepositPolicy>();
                });
        }

        private void MockCancelationPolicyRepo()
        {
            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<ListCancellationPoliciesRequest>()))
                .Returns((ListCancellationPoliciesRequest req) =>
                {
                    return new List<OB.BL.Contracts.Data.Rates.CancellationPolicy>();
                });
        }

        private void MockOtherPolicyRepo()
        {
            _otherRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<GetOtherPoliciesRequest>()))
                .Returns((GetOtherPoliciesRequest req) =>
                {
                    return null;
                });
        }

        private int ReservationRoomCount = 0;
        private void MockReservationsRoomsRepo()
        {
            _reservationRoomRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationRoom>()))
                .Returns((domainReservation.ReservationRoom entity) =>
                {
                    entity.UID = ++ReservationRoomCount;

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
            _resRoomDetailsRepoSpecMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomDetail>()))
                .Returns((domainReservation.ReservationRoomDetail entity) =>
                {
                    return entity;
                });
        }

        private int ReservationChildRoomCount = 0;
        private void MockResChildRepo(ReservationDataBuilder builder)
        {
            _resRoomRoomChildRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationRoomChild>()))
                .Returns((domainReservation.ReservationRoomChild entity) =>
                {
                    //We assume that the uid of the room where we will insert the child is the first that we get on the list
                    var roomUid = builder.InputData.reservationRoomChild.Select(y => y.ReservationRoom_UID).Distinct().FirstOrDefault();

                    entity.UID = ++ReservationChildRoomCount;
                    entity.ReservationRoom_UID = roomUid;

                    return entity;
                });
        }

        private void MockObRateToomDetailsForResRepo(int roomsToUpdate)
        {
            //We are here - see in the OB API
            _rrdRepoMock.Setup(x => x.UpdateRateRoomDetailAllotments(It.IsAny<UpdateRateRoomDetailAllotmentsRequest>()))
                .Returns((UpdateRateRoomDetailAllotmentsRequest req) => {
                    UpdateRateRoomDetailAllotmentsResponse ret = new UpdateRateRoomDetailAllotmentsResponse();

                    //we dont do the same of trhe repository in the OB API do because it only changes the database (to change if we need later)

                    return roomsToUpdate;
                });
        }

        private void MockObRatesRepo(SearchBuilder builder)
        {
            builder.InputData.Rates.ForEach(x =>
            {
                listRates.Add(new Rate()
                {
                    UID = x.UID,
                    Name = x.Name,
                    Rate_UID = x.UID
                });
            });

            _obRatesRepoMock.Setup(x => x.ListRatesForReservation(It.IsAny<ListRatesForReservationRequest>()))
                .Returns((ListRatesForReservationRequest req) =>
                {
                    return listRates.Where(y => req.RateUIDs.Contains(y.UID)).ToList();
                });

            _obRatesRepoMock.Setup(x => x.ListRatesAvailablityType(It.IsAny<OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest request) =>
                {
                    return listRates.ToDictionary(x => x.UID, y => (int)y.RateAvailabilityType);
                });
        }

        private void MockObChannelsRepo(ReservationDataBuilder resBuilder)
        {
            listChannelsLight.Add(new Contracts.Data.Channels.ChannelLight()
            {
                UID = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                Name = "teste channel"
            });

            _channelPropertiesRepoMock.Setup(x => x.ListChannelLight(It.IsAny<ListChannelLightRequest>()))
                .Returns((ListChannelLightRequest req) =>
                {
                    if (req.ChannelUIDs != null && req.ChannelUIDs.Any())
                        return listChannelsLight.Where(y => req.ChannelUIDs.Contains(y.UID)).ToList();

                    return null;
                });

            _channelPropertiesRepoMock.Setup(x => x.ListChannel(It.IsAny<Contracts.Requests.ListChannelRequest>()))
                .Returns(new List<OB.BL.Contracts.Data.Channels.Channel>());
        }
        private void MockObCurrenciesRepo()
        {
            _obCurrencyRepoMock.Setup(y => y.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(listCurrencies.Select(y => new Contracts.Data.General.Currency { UID = y.UID, CurrencySymbol = y.CurrencySymbol, DefaultPositionNumber = y.DefaultPositionNumber, Name = y.Name, PaypalCurrencyCode = y.PaypalCurrencyCode, Symbol = y.Symbol }).ToList());
        }
        private void MockVisualStates()
        {
            _visualStateRepoMock.Setup(y => y.GetQuery())
                .Returns(listVisualStates.AsQueryable());
        }

        private void MockIncentives(Operations.Helper.SearchBuilder builder, bool? freeNights = null)
        {
            _incentivesRepoMock.Setup(x => x.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(It.IsAny<ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest>()))
        .Returns((ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest req) =>
        {
            if (freeNights.HasValue)
            {
                var fNs = freeNights.GetValueOrDefault();
                var incentives = builder.InputData.Incentives.Where(y => (fNs ? (y.FreeDays > 0) : (y.FreeDays == 0))).ToList();
                var fNIncentivesDict = incentives.GroupBy(x => x.UID).ToDictionary(x => x.Key, x => x.Select(j => j));
                return new ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse { Status = Status.Success, Result = fNIncentivesDict };
            }

            var incentivesDict = builder.InputData.Incentives.GroupBy(x => x.UID).ToDictionary(x => x.Key, x => x.Select(j => j));
            return new ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse { Status = Status.Success, Result = incentivesDict };
        });
            _incentivesRepoMock.Setup(x => x.ListIncentivesForReservationRoom(It.IsAny<OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest req) =>
                {
                    if (freeNights.HasValue)
                    {
                        var fNs = freeNights.GetValueOrDefault();
                        return builder.InputData.Incentives.Where(y => (fNs ? (y.FreeDays > 0 && req.FreeNights) : (y.FreeDays == 0 && !req.FreeNights))).ToList();
                    }

                    return builder.InputData.Incentives;
                });
            _incentivesRepoMock.Setup(x => x.ListIncentives(It.IsAny<ListIncentiveRequest>()))
                .Returns((ListIncentiveRequest req) =>
                {
                    var ret = new List<Incentive>() { };

                    if (builder.InputData.Incentives != null)
                    {
                        ret = builder.InputData.Incentives.Where(y => req.IncentiveUIDs.Contains(y.UID))
                                        .GroupBy(y => y.UID).Select(y => y.First()).ToList();
                    }

                    return ret;
                });
        }

        private void MockListRateRoomDetail(Operations.Helper.SearchBuilder builder, ReservationRoom room, CalculateFinalPriceParameters parameters, int numFreeChilds, bool countAsAdult, bool priceModel = false, int? commissionType = null)
        {
            _rrdRepoMock.Setup(x => x.ListRateRoomDetailForReservationRoom(It.IsAny<OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest req) =>
                {
                    var res = new List<RateRoomDetailReservation>();

                    var rrd = builder.InputData.RateRoomDetails[0];
                    var rChs = builder.InputData.RateChannels[0];
                    decimal? addOnValue = rChs.PriceAddOnValue;
                    bool? addOnValueDecrease = rChs.PriceAddOnIsValueDecrease;
                    bool? addOnIsPercentage = rChs.PriceAddOnIsPercentage;

                    decimal? packageValue = builder.InputData.RateChannels[0].Package;
                    decimal? markupValue = builder.InputData.RateChannels[0].Markup;
                    decimal? commissionValue = builder.InputData.RateChannels[0].Commission;

                    decimal? rateModuleValue = null;
                    switch (commissionType)
                    {
                        case 1:  //NET commission
                            rateModuleValue = commissionValue;
                            break;
                        case 2:  //Markup
                            rateModuleValue = markupValue;
                            break;
                        case 3: //Commission
                            rateModuleValue = commissionValue;
                            break;
                        case 5:  //Package
                            rateModuleValue = packageValue;
                            break;
                    }

                    var dateRange = (int?)(parameters.CheckOut - parameters.CheckIn).TotalDays;
                    for (int i = 0; i < dateRange; i++)
                    {
                        var appliedIncentives = builder.InputData.Incentives.Where(y => !y.IncentiveFrom.HasValue &&
                                                    parameters.CheckIn >= y.IncentiveFrom && parameters.CheckOut <= y.IncentiveTo).ToList();

                        var tmpRRD = new RateRoomDetailReservation()
                        {
                            UID = i + 1,
                            AdultPrice = GetAdultPriceTest(rrd, room.AdultCount.GetValueOrDefault()),
                            Adults = room.AdultCount.GetValueOrDefault(),
                            Allotment = (int?)(room.AdultCount.GetValueOrDefault() + room.ChildCount),
                            AllotmentUsed = (int?)(room.AdultCount.GetValueOrDefault() + room.ChildCount),
                            AppliedIncentives = appliedIncentives,
                            Channel_UID = parameters.ChannelId,
                            ChildPrice = GetChildPriceTest(rrd, room.ChildCount.GetValueOrDefault(), numFreeChilds, countAsAdult),
                            Childs = room.ChildCount,
                            ClosedOnArrival = builder.InputData.ClosedOnArrival,
                            ClosedOnDeparture = builder.InputData.ClosedOnDeparture,
                            CurrencyId = (int)builder.InputData.Rates[0].Currency_UID,
                            Date = parameters.CheckIn.AddDays(i).Date,
                            DateRangeCount = (int?)(builder.InputData.Rates[0].BeginSale.GetValueOrDefault() - builder.InputData.Rates[0].EndSale.GetValueOrDefault()).TotalDays,
                            FinalPrice = builder.InputData.Rates[0].Value.GetValueOrDefault(),
                            IsAvailableToTPI = builder.InputData.Rates[0].IsAvailableToTPI,
                            MaxFreeChilds = numFreeChilds,
                            RateModelIsPercentage = false,
                            PriceAddOnIsValueDecrease = addOnValueDecrease,
                            PriceAddOnValue = addOnValue,
                            PriceAddOnIsPercentage = addOnIsPercentage,
                            PriceModel = priceModel,
                            IsMarkup = commissionType == 5 || commissionType == 2,
                            IsCommission = commissionType == 1,
                            RateModelValue = rateModuleValue,
                            Rate_UID = builder.InputData.Rates[0].UID
                        };

                        if (countAsAdult)
                        {
                            tmpRRD.AdultPrice = GetAdultPriceTest(rrd, room.AdultCount.GetValueOrDefault() + room.ChildCount.GetValueOrDefault());
                        }

                        res.Add(tmpRRD);
                    }

                    return res;
                });
        }

        private void MockListRateRoomDetail(Operations.Helper.SearchBuilder builder, domainReservation.ReservationRoom room, CalculateFinalPriceParameters parameters, int numFreeChilds, bool countAsAdult, bool priceModel = false, int? commissionType = null)
        {
            _rrdRepoMock.Setup(x => x.ListRateRoomDetailForReservationRoom(It.IsAny<OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest req) =>
                {
                    var res = new List<RateRoomDetailReservation>();

                    var rrd = builder.InputData.RateRoomDetails[0];
                    var rChs = builder.InputData.RateChannels[0];
                    decimal? addOnValue = rChs.PriceAddOnValue;
                    bool? addOnValueDecrease = rChs.PriceAddOnIsValueDecrease;
                    bool? addOnIsPercentage = rChs.PriceAddOnIsPercentage;

                    decimal? packageValue = builder.InputData.RateChannels[0].Package;
                    decimal? markupValue = builder.InputData.RateChannels[0].Markup;
                    decimal? commissionValue = builder.InputData.RateChannels[0].Commission;

                    decimal? rateModuleValue = null;
                    switch (commissionType)
                    {
                        case 1:  //NET commission
                            rateModuleValue = commissionValue;
                            break;
                        case 2:  //Markup
                            rateModuleValue = markupValue;
                            break;
                        case 3: //Commission
                            rateModuleValue = commissionValue;
                            break;
                        case 5:  //Package
                            rateModuleValue = packageValue;
                            break;
                    }

                    var dateRange = (int?)(parameters.CheckOut - parameters.CheckIn).TotalDays;
                    for (int i = 0; i < dateRange; i++)
                    {
                        var appliedIncentives = builder.InputData.Incentives.Where(y => !y.IncentiveFrom.HasValue &&
                                                    parameters.CheckIn >= y.IncentiveFrom && parameters.CheckOut <= y.IncentiveTo).ToList();

                        var tmpRRD = new RateRoomDetailReservation()
                        {
                            UID = i + 1,
                            AdultPrice = GetAdultPriceTest(rrd, room.AdultCount.GetValueOrDefault()),
                            Adults = room.AdultCount.GetValueOrDefault(),
                            Allotment = (int?)(room.AdultCount.GetValueOrDefault() + room.ChildCount),
                            AllotmentUsed = (int?)(room.AdultCount.GetValueOrDefault() + room.ChildCount),
                            AppliedIncentives = appliedIncentives,
                            Channel_UID = parameters.ChannelId,
                            ChildPrice = GetChildPriceTest(rrd, room.ChildCount.GetValueOrDefault(), numFreeChilds, countAsAdult),
                            Childs = room.ChildCount,
                            ClosedOnArrival = builder.InputData.ClosedOnArrival,
                            ClosedOnDeparture = builder.InputData.ClosedOnDeparture,
                            CurrencyId = (int)builder.InputData.Rates[0].Currency_UID,
                            Date = parameters.CheckIn.AddDays(i).Date,
                            DateRangeCount = (int?)(builder.InputData.Rates[0].BeginSale.GetValueOrDefault() - builder.InputData.Rates[0].EndSale.GetValueOrDefault()).TotalDays,
                            FinalPrice = builder.InputData.Rates[0].Value.GetValueOrDefault(),
                            IsAvailableToTPI = builder.InputData.Rates[0].IsAvailableToTPI,
                            MaxFreeChilds = numFreeChilds,
                            RateModelIsPercentage = false,
                            PriceAddOnIsValueDecrease = addOnValueDecrease,
                            PriceAddOnValue = addOnValue,
                            PriceAddOnIsPercentage = addOnIsPercentage,
                            PriceModel = priceModel,
                            IsMarkup = commissionType == 5 || commissionType == 2,
                            IsCommission = commissionType == 1,
                            RateModelValue = rateModuleValue,
                            Rate_UID = builder.InputData.Rates[0].UID
                        };

                        if (countAsAdult)
                        {
                            tmpRRD.AdultPrice = GetAdultPriceTest(rrd, room.AdultCount.GetValueOrDefault() + room.ChildCount.GetValueOrDefault());
                        }

                        res.Add(tmpRRD);
                    }

                    return res;
                });
        }

        private void MockBuyerGroupRepo(Operations.Helper.SearchBuilder builder)
        {
            _buyerGroupRepo.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>()))
                .Returns((long rateUid, long? tpiUid) =>
                {
                    return builder.BuyerGroups.Where(y => y.Rate_UID == rateUid && tpiUid == y.TPI_UID).SingleOrDefault();
                });
        }

        private void MockPromoCodesRepo(Operations.Helper.SearchBuilder builder)
        {
            _promoCodesRepo.Setup(x => x.ListPromotionalCodeForReservation(It.IsAny<Contracts.Requests.ListPromotionalCodeForReservationRequest>()))
                .Returns((Contracts.Requests.ListPromotionalCodeForReservationRequest req) =>
                {
                    return builder.InputData.PromoCode;
                });
        }

        private int numberOfReservations = 0;
        private void MockReservationsRepo(Operations.Helper.SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId, bool isExisting = false)  //Use the builder to construct the reservation context
        {
            numberOfReservations = 0;

            var ret = new ReservationDataContext()
            {
                Client_UID = resBuilder.InputData.guest.Client_UID,
                PropertyName = "Property Test",
                PropertyBaseLanguage_UID = resBuilder.InputData.reservationDetail.ReservationLanguageUsed_UID,  //same of the reservation
                PropertyBaseCurrency_UID = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID,  //same of the reservation
                PropertyCountry_UID = null,
                PropertyCity_UID = null,
                BookingEngineChannel_UID = searchBuilder.InputData.RateChannels != null && searchBuilder.InputData.RateChannels.Any() ? searchBuilder.InputData.RateChannels[0].Channel_UID : 0,
                Channel_UID = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                ChannelName = resBuilder.InputData.reservationDetail.Number,
                ChannelType = 0,
                ChannelOperatorType = !resBuilder.InputData.reservationDetail.TPI_UID.HasValue ? 2 : (resBuilder.InputData.reservationDetail.Company_UID.HasValue ? 4 : 3),
                ChannelHandleCredit = false,
                IsChannelValid = true,
                Guest_UID = resBuilder.InputData.guest != null ? resBuilder.InputData.guest.UID : (long?)null,
                IsFromChannel = false,
                TPIProperty_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.Property_UID : null,
                TPICompany_CommissionIsPercentage = resBuilder.InputData.reservationRooms != null ? resBuilder.InputData.reservationRooms[0].TPIDiscountIsPercentage : null,
                TPIProperty_Commission = resBuilder.InputData.reservationRooms != null ? resBuilder.InputData.reservationRooms[0].TPIDiscountValue : null,
                TPI_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.UID : (long?)null,
                TPICompany_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.Company_UID : null,
                SalesmanCommission_UID = null,
                Salesman_UID = resBuilder.InputData.reservationDetail.Salesman_UID,
                SalesmanBaseCommission = resBuilder.InputData.reservationDetail.SalesmanCommission,
                SalesmanIsBaseCommissionPercentage = resBuilder.InputData.reservationDetail.SalesmanIsCommissionPercentage,
                ReservationRoomDetails = resBuilder.InputData.reservationRoomDetails,
                Currency_UID = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.HasValue ? resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.Value : 34,
                Currency_Symbol = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.HasValue ? GetCurrencySymbol(resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.Value) : "EUR",
                IsExistingReservation = isExisting,
                ReservationUID = resBuilder.InputData.reservationDetail.UID,
                ChannelProperty_UID = searchBuilder.InputData.PropertyChannels != null && searchBuilder.InputData.PropertyChannels.Any() ? searchBuilder.InputData.PropertyChannels[0].UID : (long?)null,
                IsOnRequestEnable = false,
                RatesAvailabilityType = new Dictionary<long, int> {
                    [resBuilder.InputData.reservationRooms.First().Rate_UID.Value] = (int)Contracts.Data.RateAvailabilityTypes.Allotment
                }
            };
            _reservationsRepoMock.Setup(x => x.GetReservationContext(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IEnumerable<long>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<Guid?>()))
                .Returns(ret);

            var resNumberGenerated = "RES00" + (reservationsList.Count + 1) + "-" + resBuilder.InputData.reservationDetail.Property_UID;
            _reservationsRepoMock.Setup(x => x.GenerateReservationNumber(It.IsAny<long>()))
                .Returns(resNumberGenerated);

            _reservationsRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.Reservation>()))
                .Returns((domainReservation.Reservation res) =>
                {
                    res.UID = ++numberOfReservations;
                    reservationsList.Add(res);
                    return res;
                });

            _reservationsRepoMock.Setup(x => x.FindReservationTransactionStatusByReservationsUIDs(It.IsAny<List<long>>()))
                .Returns((List<long> transactionUids) => {
                    var res = new List<Item>() { };

                    transactionUids.ForEach(y =>
                    {
                        res.Add(new Item()
                        {
                            TransactionUID = transactionId,
                            ReservationUID = 0
                        });
                    });

                    return res;
                });

            int total = -1;
            _reservationsRepoMock.Setup(x => x.FindByCriteria(out total, It.IsAny<ListReservationCriteria>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(reservationsList.AsQueryable());

            _reservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationCriteria>())).Returns((ListReservationCriteria crit) =>
            {
                if (crit?.ReservationUIDs != null && crit.ReservationUIDs.Any())
                {
                    return reservationsList.Where(x => crit.ReservationUIDs.Contains(x.UID)).ToList();
                }
                return null;
            });

            _reservationsRepoMock.Setup(x => x.FindReservationsAdditionalDataByReservationsUIDs(It.IsAny<List<long>>()))
                .Returns((List<long> uids) =>
                {
                    //Attention: we dont take in consideration the builder here
                    return new List<domainReservation.ReservationsAdditionalData>() { };
                });

            _reservationsRepoMock.Setup(x => x.FindByReservationNumberAndChannelUID(It.IsAny<string>(), It.IsAny<long>()))
                .Returns((string resNumber, long chUid) =>
                {
                    return reservationsList.Where(y => y.Number == resNumber && y.Channel_UID == chUid).SingleOrDefault();
                });

            _reservationsRepoMock.Setup(x => x.InsertReservationTransaction(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Constants.ReservationTransactionStatus>(),
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()));
        }

        private void MockResRoomAppliedIncentives()
        {
            _resRoomAppliedIncentiveRepoMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomDetailsAppliedIncentive>()))
                .Returns((domainReservation.ReservationRoomDetailsAppliedIncentive entity) =>
                {
                    return entity;
                });
        }

        private void MockResRoomAppliedPromotionalCodes()
        {
            _resRoomAppliedPromotionalCodeRepoMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomDetailsAppliedPromotionalCode>()))
                .Returns((domainReservation.ReservationRoomDetailsAppliedPromotionalCode entity) =>
                {
                    return entity;
                });
        }

        private void MockPortalRulesRepository(PORule rule)
        {
            CreatePortalRules(rule);
            _portalMock.Setup(x => x.ListMarkupCommissionRules(It.IsAny<PO.BL.Contracts.Requests.ListMarkupCommissionRulesRequest>()))
                .Returns(sellRulesList);
        }

        private void MockReservationTransactionState(int value)
        {
            long reservationid = 0;
            long hangfireid = 0;
            _reservationsRepoMock.Setup(x => x.GetReservationTransactionState(It.IsAny<string>(), It.IsAny<long>(), out reservationid, out hangfireid)).Returns(value);
        }

        private void InitializeLookupsMock(IList<Rate> rates = null, IList<Guest> guests = null)
        {
            _OBReservationLookupsRepoMock.Setup(x => x.ListReservationLookups(It.IsAny<ListReservationLookupsRequest>()))
                .Returns((ListReservationLookupsRequest request) =>
                {
                    return new Contracts.Data.Reservations.ReservationLookups
                    {
                        RatesLookup = rates?.ToDictionary(x => x.UID) ?? listRates.ToDictionary(x => x.UID),
                        GuestsLookup = guests?.ToDictionary(x => x.UID) ?? guestsList.ToDictionary(x => x.UID)
                    };
                });

            _obRatesRepoMock.Setup(x => x.ListRatesAvailablityType(It.IsAny<ListRateAvailabilityTypeRequest>()))
                .Returns((ListRateAvailabilityTypeRequest request) =>
                {
                    return rates?.ToDictionary(x => x.UID, y => y.AvailabilityType) ?? listRates.ToDictionary(x => x.UID, y => y.AvailabilityType);
                });
        }

        private void MockPropertyBaseCurrencyExchangeRate(decimal v)
        {
            _reservationsRepoMock.Setup(x => x.GetExchangeRateBetweenCurrenciesByPropertyId(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(v);
        }

        #region General Mock Repo Methods
        private void MockGroupRulesRepo()
        {
            _groupRulesRepo.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>()))
                .Returns((DL.Common.Criteria.GetGroupRuleCriteria criteria) =>
                {
                    return groupRuleList.AsQueryable().FirstOrDefault(x => x.RuleType == criteria.RuleType);
                });
        }

        private void MockBeginTransactions()
        {
            _sqlManagerMock.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Returns(transaction);
        }

        private void MockObCrmRepo()
        {
            _tpiRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<ListGuestLightRequest>()))
                .Returns((ListGuestLightRequest request) =>
                {
                    return guestsList.Where(x => request.UIDs.Contains(x.UID)).ToList();
                });

            _tpiRepoMock.Setup(x => x.UpdateGuestReservation(It.IsAny<UpdateGuestReservationRequest>()))
                .Returns((UpdateGuestReservationRequest req) =>
                {
                    UpdateGuestReservationResponse res = new UpdateGuestReservationResponse();
                    res.Result = new Guest()
                    {
                        UID = req.Guest.UID,
                        Index = 0
                    };
                    res.Succeed();
                    return res;
                });

            _tpiRepoMock.Setup(x => x.InsertGuestReservation(It.IsAny<InsertGuestReservationRequest>()))
                .Returns((InsertGuestReservationRequest req) =>
                {
                    InsertGuestReservationResponse response = new InsertGuestReservationResponse();
                    response.Result = new Guest();
                    int i = 0;
                    response.Result = new Guest()
                    {
                        Index = i,
                        UID = req.Guest.UID
                    };
                    response.Succeed();

                    return response;
                });

            _tpiRepoMock.Setup(x => x.ListTpiProperty(It.IsAny<ListTPIPropertyRequest>()))
                .Returns((ListTPIPropertyRequest req) =>
                {
                    return new List<TPIProperty>() { };
                });
        }

        private void MockChildTermsRepo(SearchBuilder builder)
        {
            _obChildTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>()))
                .Returns((ListChildTermsRequest req) =>
                {
                    var ret = new ListChildTermsResponse();
                    ret.Result = builder.InputData.ChildTerms;
                    ret.Succeed();
                    return ret;
                });
        }

        private void MockExtrasRepo(ReservationDataBuilder resBuilder)
        {
            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<ListIncludedRateExtrasRequest>()))
                .Returns((ListIncludedRateExtrasRequest req) =>
                {
                    List<Extra> ret = new List<Extra>() { };

                    if (resBuilder.InputData.reservationRoomExtras!= null)
                        resBuilder.InputData.reservationRoomExtras.Where(y => y.ExtraIncluded == true).ToList();

                    return ret;
                });
        }

        private void MockTaxPoliciesRepo()
        {
            _resRoomTaxPolicyRepoMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomTaxPolicy>()))
                .Returns((domainReservation.ReservationRoomTaxPolicy entity) =>
                {
                    return entity;
                });
        }
        #endregion
        #endregion

        #region Mock POCO Calls
        void MockReservationHelperPockMethods()
        {
            _reservationHelperMock.Setup(x => x.GetReservationLookups(It.IsAny<ListReservationCriteria>(), It.IsAny<IEnumerable<domainReservation.Reservation>>()))
                .Returns(new ReservationLookups { });
        }
        #endregion

        #region POLICIES

        #region CANCELATION POLICIES

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_NoCosts()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 0,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 1,
                        IsCancellationAllowed = true,
                        ReservationRoomsTotalAmount = 100,
                        CancellationNrNights = 1,
                        CancellationValue = 10
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 0, "Expected CancelationCosts = 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_NumberOfNights_OneNight()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 3,
                        ReservationRoomsTotalAmount = 400,
                        IsCancellationAllowed = true,
                        CancellationNrNights = 1,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 100, "Expected CancelationCosts = 100");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_NumberOfNights_TwoNight()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 3,
                        ReservationRoomsTotalAmount = 400,
                        IsCancellationAllowed = true,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 300, "Expected CancelationCosts = 300");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_NoPolicy()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = null,
                        ReservationRoomsTotalAmount = 400,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 400, "Expected CancelationCosts = 400");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_Percentage()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 2,
                        ReservationRoomsTotalAmount = 400,
                        IsCancellationAllowed = true,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 40, "Expected CancelationCosts = 40");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetCancelationCosts_Value()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 1,
                        ReservationRoomsTotalAmount = 400,
                        IsCancellationAllowed = true,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetCancelationCosts(reservation, true);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == reservation.ReservationRooms.Count, string.Format("Expected count = {0}", reservation.ReservationRooms.Count));
            Assert.IsTrue(result.First().CancelationCosts == 10, "Expected CancelationCosts = 10");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CheckIfCancelationPolicyChanged_True()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new OB.Domain.Reservations.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                {
                    new OB.Domain.Reservations.ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 1,
                        ReservationRoomsTotalAmount = 400,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<OB.Domain.Reservations.ReservationRoomDetail>()
                        {
                            new OB.Domain.Reservations.ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new OB.Domain.Reservations.ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.CheckIfCancelationPolicyChanged(reservation.ReservationRooms.First(),
                                fixture.Create<Contracts.Data.Rates.CancellationPolicy>());

            Assert.IsTrue(result, "Expected True");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CheckIfCancelationPolicyChanged_False()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new OB.Domain.Reservations.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                {
                    new OB.Domain.Reservations.ReservationRoom
                    {
                        CancellationPolicyDays = 5,
                        DateFrom = DateTime.Today.AddDays(1),
                        CancellationPaymentModel = 1,
                        ReservationRoomsTotalAmount = 400,
                        CancellationNrNights = 2,
                        CancellationValue = 10,
                        ReservationRoomDetails = new List<OB.Domain.Reservations.ReservationRoomDetail>()
                        {
                            new OB.Domain.Reservations.ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new OB.Domain.Reservations.ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.CheckIfCancelationPolicyChanged(reservation.ReservationRooms.First(),
                                new Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 5,
                                    PaymentModel = 1,
                                    NrNights = 2,
                                    Value = 10
                                });

            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetCancellationPolicies()
        {
            var cancellationPolicy = fixture.Create<CancellationPolicy>();
            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x))
                    .Returns(new List<CancellationPolicy>() { cancellationPolicy });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(cancellationPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            helper.SetCancellationPolicies(room, new List<RateRoomDetailReservation>(), 34, 1);
            var result = helper.CheckIfCancelationPolicyChanged(room, cancellationPolicy);
            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetCancellationPolicies_WithoutTranslations()
        {
            var cancellationPolicy = fixture.Build<CancellationPolicy>()
                .Without(x => x.TranslatedDescription)
                .Without(x => x.TranslatedName).Create();
            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x))
                    .Returns(new List<CancellationPolicy>() { cancellationPolicy });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(cancellationPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            helper.SetCancellationPolicies(room, new List<RateRoomDetailReservation>(), 34, 1);
            var result = helper.CheckIfCancelationPolicyChanged(room, cancellationPolicy);
            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveCancelationPolicy_MoreExpensive()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x))
                    .Returns(new List<CancellationPolicy>() { policyNrNights, policyPercentage, policyValue });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(policyNrNights);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveCancelationPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.CancellationPolicy, Contracts.Data.Rates.CancellationPolicy>(result);
            comparer.ShouldEqual(policyNrNights);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveCancelationPolicy_Days()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 1)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x)).Returns(new List<CancellationPolicy>() { policyNrNights, policyPercentage, policyValue });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(policyValue);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveCancelationPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.CancellationPolicy, Contracts.Data.Rates.CancellationPolicy>(result);
            comparer.ShouldEqual(policyValue);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveCancelationPolicy_NonRefundable()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 1)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyNonRefundable = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 1)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, false)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x)).Returns(new List<CancellationPolicy>() { policyNrNights, policyPercentage, policyValue, policyNonRefundable });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(policyNonRefundable);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveCancelationPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.CancellationPolicy, Contracts.Data.Rates.CancellationPolicy>(result);
            comparer.ShouldEqual(policyNonRefundable);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveCancelationPolicy_SortOrder()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.SortOrder, 4)
                .Create();

            var policyPercentage = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 1)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 3)
                .Create();

            var policyNonRefundable = fixture.Build<CancellationPolicy>()
                .With(x => x.CancellationCosts, true)
                .With(x => x.Days, 1)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsCancellationAllowed, false)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 2)
                .Create();

            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<Contracts.Requests.ListCancellationPoliciesRequest>())).Callback<Contracts.Requests.ListCancellationPoliciesRequest>(x => Send(x))
                    .Returns(new List<CancellationPolicy>() { policyNrNights, policyPercentage, policyValue, policyNonRefundable });

            _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>())).Callback<OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest>(x => Send(x)).Returns(policyPercentage);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveCancelationPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.CancellationPolicy, Contracts.Data.Rates.CancellationPolicy>(result);
            comparer.ShouldEqual(policyPercentage);
        }

        #endregion CANCELATION POLICIES

        #region DEPOSIT POLICIES

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_NoCosts1()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var result = helper.GetDepositCosts(reservation, null);

            //Response should contain 0 errors
            Assert.IsTrue(result.Count == 0, string.Format("Expected count = {0}", 0));
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_NoCosts2()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 0,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 2
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            //Response should contain 0 errors
            Assert.IsTrue(result.First().DepositCosts == 0, string.Format("Expected count = {0}", 0));
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_NrNights_OneNight()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 10,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 3,
                                                        'NrNights' : 1
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            Assert.IsTrue(result.First().DepositCosts == 100, "Expected CancelationCosts = 100");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_NrNights_TwoNights()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 10,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 3,
                                                        'NrNights' : 2
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            Assert.IsTrue(result.First().DepositCosts == 300, "Expected CancelationCosts = 300");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_Percentage()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 10,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 2,
                                                        'NrNights' : 2
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            Assert.IsTrue(result.First().DepositCosts == 80, "Expected CancelationCosts = 80");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_Value()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 10,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 1,
                                                        'NrNights' : 2
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            Assert.IsTrue(result.First().DepositCosts == 20, "Expected CancelationCosts = 20");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetDepositCosts_PolicyDaysTooBig()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new OB.Domain.Reservations.ReservationsAdditionalData
            {
                ReservationAdditionalDataJSON = @"{
                                                    'ReservationRoomList': [
                                                    {
                                                        'ReservationRoom_UID': 1,
                                                        'DepositPolicy_UID': 1024,
                                                        'IsDepositCostsAllowed': true,
                                                        'DepositCosts': true,
                                                        'DepositDays': 99999999,
                                                        'DepositPolicy': 'This information does not exist in your language...! \n teste pm',
                                                        'Value': 20,
                                                        'PaymentModel': 1,
                                                        'NrNights' : 2
                                                    }
                                                    ]
                                                }"
            };

            var result = helper.GetDepositCosts(reservation, additionalData);

            Assert.IsTrue(result.First().DepositCosts == 20, "Expected CancelationCosts = 20");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CompareDepositPolicies_False()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var dep1 = fixture.Create<Contracts.Data.Rates.DepositPolicy>();
            var dep2 = fixture.Create<Contracts.Data.Rates.DepositPolicy>();
            var result = helper.CompareDepositPolicies(dep1, dep2);

            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CompareDepositPolicies_True()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var dep1 = fixture.Create<Contracts.Data.Rates.DepositPolicy>();
            var dep2 = dep1;
            var result = helper.CompareDepositPolicies(dep1, dep2);

            Assert.IsTrue(result, "Expected True");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CheckIfDepositPolicyChanged_True()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
               {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new ReservationsAdditionalData
            {
                ReservationRoomList = new List<ReservationRoomAdditionalData>(){
                    new ReservationRoomAdditionalData(){
                        ReservationRoom_UID =1,
                        DepositPolicy_UID = 1024,
                        IsDepositCostsAllowed = true,
                        DepositCosts = true,
                        DepositDays = 10,
                        DepositPolicy = "This information does not exist in your language...! \n teste pm",
                        Value =20,
                        PaymentModel = 1,
                        NrNights = 2
                    }
                }
            };

            var dep1 = fixture.Create<Contracts.Data.Rates.DepositPolicy>();
            var result = helper.CheckIfDepositPolicyChanged(1, additionalData, dep1);

            Assert.IsTrue(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_CheckIfDepositPolicyChanged_False()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            var reservation = new contractReservation.Reservation
            {
                PropertyBaseCurrencyExchangeRate = 1,
                ReservationCurrency_UID = 34,
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom
                    {
                        UID = 1,
                        DateFrom = DateTime.Today.AddDays(1),
                        ReservationRoomsTotalAmount = 400,
                        ReservationRoomDetails = new List<ReservationRoomDetail>()
                        {
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(1),
                                Price = 100
                            },
                            new ReservationRoomDetail
                            {
                                Date = DateTime.Today.AddDays(2),
                                Price = 200
                            }
                        }
                    }
                }
            };

            var additionalData = new ReservationsAdditionalData
            {
                ReservationRoomList = new List<ReservationRoomAdditionalData>(){
                    new ReservationRoomAdditionalData(){
                        ReservationRoom_UID =1,
                        DepositPolicy_UID = 1024,
                        IsDepositCostsAllowed = true,
                        DepositCosts = true,
                        DepositDays = 10,
                        DepositPolicy = "This information does not exist in your language...! \n teste pm",
                        Value =20,
                        PaymentModel = 1,
                        NrNights = 2
                    }
                }
            };

            var dep1 = new Contracts.Data.Rates.DepositPolicy
            {
                UID = 1024,
                IsDepositCostsAllowed = true,
                DepositCosts = true,
                Days = 10,
                Description = "This information does not exist in your language...! \n teste pm",
                Value = 20,
                PaymentModel = 1,
                NrNights = 2
            };


            var result = helper.CheckIfDepositPolicyChanged(1, additionalData, dep1);

            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetDepositPolicies()
        {
            var depositPolicy = fixture.Create<DepositPolicy>();
            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { depositPolicy });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
.Returns(depositPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            var reservationAdditionalData = new ReservationsAdditionalData();
            helper.SetDepositPolicies(room, new List<RateRoomDetailReservation>(), ref reservationAdditionalData,
                fixture.Create<long>(), fixture.Create<long?>(), fixture.Create<long?>(), 1);

            var result = helper.CheckIfDepositPolicyChanged(room.UID, reservationAdditionalData,
                depositPolicy);

            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetDepositPolicies_WithoutTransalations()
        {
            var depositPolicy = fixture.Build<DepositPolicy>()
                .Without(x => x.TranslatedDescription)
                .Without(x => x.TranslatedName).Create();
            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { depositPolicy });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
.Returns(depositPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            var reservationAdditionalData = new ReservationsAdditionalData();
            helper.SetDepositPolicies(room, new List<RateRoomDetailReservation>(), ref reservationAdditionalData,
                fixture.Create<long>(), fixture.Create<long?>(), fixture.Create<long?>(), 1);

            var result = helper.CheckIfDepositPolicyChanged(room.UID, reservationAdditionalData,depositPolicy);

            Assert.IsFalse(result, "Expected False");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveDepositPolicy_MoreExpensive()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { policyNrNights, policyPercentage, policyValue });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
        .Returns(policyNrNights);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveDepositPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.DepositPolicy, Contracts.Data.Rates.DepositPolicy>(result);
            comparer.ShouldEqual(policyNrNights);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveDepositPolicy_Days()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 5)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { policyNrNights, policyPercentage, policyValue });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
                    .Returns(policyPercentage);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveDepositPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.DepositPolicy, Contracts.Data.Rates.DepositPolicy>(result);
            comparer.ShouldEqual(policyPercentage);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveDepositPolicy_NoCosts()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 5)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyValue = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyNoCosts = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, false)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, false)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 1)
                .Create();

            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { policyNrNights, policyPercentage, policyValue });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
.Returns(policyPercentage);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveDepositPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.DepositPolicy, Contracts.Data.Rates.DepositPolicy>(result);
            comparer.ShouldEqual(policyPercentage);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_GetMostRestrictiveDepositPolicy_SortOrder()
        {
            #region Arrange

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var policyNrNights = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.NrNights, 2)
                .With(x => x.PaymentModel, 3)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.SortOrder, 1)
                .Create();

            var policyPercentage = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 5)
                .With(x => x.PaymentModel, 2)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 10)
                .With(x => x.SortOrder, 2)
                .Create();

            var policyValue = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, true)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, true)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 3)
                .Create();

            var policyNoCosts = fixture.Build<DepositPolicy>()
                .With(x => x.DepositCosts, false)
                .With(x => x.Days, 0)
                .With(x => x.PaymentModel, 1)
                .With(x => x.IsDepositCostsAllowed, false)
                .With(x => x.Value, 100)
                .With(x => x.SortOrder, 4)
                .Create();

            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<Contracts.Requests.ListDepositPoliciesRequest>())).Callback<Contracts.Requests.ListDepositPoliciesRequest>(x => Send(x))
                    .Returns(new List<DepositPolicy>() { policyNrNights, policyPercentage, policyValue });

            _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>())).Callback<Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest>(x => Send(x))
.Returns(policyNrNights);

            var rrd = new List<RateRoomDetailReservation>();
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today).With(x => x.FinalPrice, 100).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(1)).With(x => x.FinalPrice, 200).Create());
            rrd.Add(fixture.Build<RateRoomDetailReservation>().With(x => x.Date, DateTime.Today.AddDays(2)).With(x => x.FinalPrice, 300).Create());

            #endregion Arrange

            var result = helper.GetMostRestrictiveDepositPolicy(DateTime.Today, DateTime.Today.AddDays(2), fixture.Create<long>(),
                fixture.Create<long>(), fixture.Create<long>(), rrd);

            var comparer = new Likeness<Contracts.Data.Rates.DepositPolicy, Contracts.Data.Rates.DepositPolicy>(result);
            comparer.ShouldEqual(policyNrNights);
        }

        #endregion DEPOSIT POLICIES

        #region OTHER POLICIES

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetOtherPolicies_NoTransalation()
        {
            var otherPolicy = fixture.Build<OtherPolicy>()
                .Without(x => x.TranslatedDescription)
                .Without(x => x.TranslatedName).Create();

            _otherRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<Contracts.Requests.GetOtherPoliciesRequest>()))
                .Callback<Contracts.Requests.GetOtherPoliciesRequest>(x=>Send(x))
                    .Returns(otherPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            helper.SetOtherPolicy(room, fixture.Create<long>(), fixture.Create<long>());

            Assert.AreEqual(MISSED_TRANLATION + " \n " + otherPolicy.OtherPolicy_Name, room.OtherPolicy);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Policies")]
        public void Test_SetOtherPolicies_WithTransalation()
        {
            var otherPolicy = fixture.Build<OtherPolicy>().Create();

            _otherRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<Contracts.Requests.GetOtherPoliciesRequest>()))
                .Callback<Contracts.Requests.GetOtherPoliciesRequest>(x => Send(x))
                    .Returns(otherPolicy);

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Create<OB.Domain.Reservations.ReservationRoom>();

            helper.SetOtherPolicy(room, fixture.Create<long>(), fixture.Create<long>());

            Assert.AreEqual(otherPolicy.TranslatedName + " : " + otherPolicy.TranslatedDescription, room.OtherPolicy);
        }

        #endregion OTHER POLICIES

        #endregion POLICIES

        #region INCENTIVES

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_NoExtrasIncludedOnInsert()
        {
            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(fixture.Build<Extra>()
                .Without(x => x.ExtrasLanguages)
                .Without(x => x.BillingTypes)
                .CreateMany(3)
                .ToList());

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .Without(x => x.ReservationRoomExtras).Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            Assert.IsTrue(room.ReservationRoomExtras.Count == 3);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_NoIncludedExtrasOnModify()
        {
            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(Enumerable.Empty<Extra>().ToList());

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .Without(x => x.ReservationRoomExtras).Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            Assert.IsTrue(room.ReservationRoomExtras.Count == 0);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_OneIncludedExtrasOnInsertAndTheSameOnModify()
        {
            fixture.Customize<Extra>(x => x
                .Without(y => y.ExtrasLanguages)
                .Without(y => y.BillingTypes));
            var extra = fixture.Create<Extra>();
            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(new List<Extra>() { extra });

            var helper = Container.Resolve<IReservationHelperPOCO>();

            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .With(x => x.UID, 1)
                .Without(x => x.ReservationRoomExtras)
                .Do(x => x.ReservationRoomExtras.Add(new OB.Domain.Reservations.ReservationRoomExtra
                {
                    CreatedDate = DateTime.UtcNow,
                    Extra_UID = extra.UID,
                    ExtraIncluded = true,
                    Qty = 1,
                    Total_Price = 0,
                    ReservationRoom_UID = 1,
                    UID = fixture.Create<long>()
                }))
                .Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            var roomExtra = room.ReservationRoomExtras.First();

            Assert.IsTrue(room.ReservationRoomExtras.Count == 1);
            Assert.IsTrue(extra.UID == roomExtra.Extra_UID);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_OneIncludedExtrasOnInsertAndDiferentOnModify()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            fixture.Customize<Extra>(x => x
                .Without(y => y.BillingTypes)
                .Without(y => y.ExtrasLanguages));

            var extra = fixture.Create<Extra>();
            var extra2 = fixture.Create<Extra>();

            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(new List<Extra>() { extra2 });


            _reservationRoomsExtrasRepoMock.Setup(x => x.Delete(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>()))
                .Callback<OB.Domain.Reservations.ReservationRoomExtra>(x=> Send(x))
                .Returns(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>());

            var rrExtra = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = extra.UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .With(x => x.UID, 1)
                .Without(x => x.ReservationRoomExtras)
                .Do(x => x.ReservationRoomExtras.Add(rrExtra))
                .Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            _reservationRoomsExtrasRepoMock.Verify(x => x.Delete(It.Is<OB.Domain.Reservations.ReservationRoomExtra>(y => y.UID == rrExtra.UID)), Times.Once());
            room.ReservationRoomExtras.Remove(rrExtra);

            Assert.IsTrue(room.ReservationRoomExtras.Count == 1);
            Assert.IsTrue(extra2.UID == room.ReservationRoomExtras.First().Extra_UID);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_TwoIncludedExtrasOnInsertAndOneDiferentOnModify()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();

            #region ARRANGE

            fixture.Customize<Extra>(x => x
                .Without(y => y.BillingTypes)
                .Without(y => y.ExtrasLanguages));

            var extra = fixture.Create<Extra>();
            var includedExtras = fixture.CreateMany<Extra>(2);
            var includedExtra1 = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = includedExtras.ElementAt(0).UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            var includedExtra2 = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = includedExtras.ElementAt(1).UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            var newIncludedExtra = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = extra.UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(new List<Extra>() { extra, includedExtras.ElementAt(0) });

            _reservationRoomsExtrasRepoMock.Setup(x => x.Delete(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>()))
                .Callback<OB.Domain.Reservations.ReservationRoomExtra>(x=> Send(x))
                .Returns(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>());

            #endregion ARRANGE

            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .With(x => x.UID, 1)
                .Without(x => x.ReservationRoomExtras)
                .Do(x => x.ReservationRoomExtras.Add(includedExtra1))
                .Do(x => x.ReservationRoomExtras.Add(includedExtra2))
                .Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            _reservationRoomsExtrasRepoMock.Verify(x => x.Delete(It.Is<OB.Domain.Reservations.ReservationRoomExtra>(y => y.UID == includedExtra2.UID)), Times.Once());
            room.ReservationRoomExtras.Remove(includedExtra2);

            Assert.IsTrue(room.ReservationRoomExtras.Count == 2);
            Assert.IsTrue(room.ReservationRoomExtras.Contains(includedExtra1));
            Assert.IsTrue(room.ReservationRoomExtras.Count(x => x.Extra_UID == extra.UID) == 1);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_Extras")]
        public void Test_SetIncludedExtras_TwoIncludedExtrasOnInsertAndTwoDiferentOnModify()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();

            #region ARRANGE

            fixture.Customize<Extra>(x => x
                .Without(y => y.BillingTypes)
                .Without(y => y.ExtrasLanguages));

            var extra = fixture.Create<Extra>();
            var extra2 = fixture.Create<Extra>();

            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<Contracts.Requests.ListIncludedRateExtrasRequest>()))
                .Callback<Contracts.Requests.ListIncludedRateExtrasRequest>(x => Send(x))
                .Returns(new List<Extra>() { extra, extra2 });

            _reservationRoomsExtrasRepoMock.Setup(x => x.Delete(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>()))
                .Callback<OB.Domain.Reservations.ReservationRoomExtra>(x => Send(x))
                .Returns(It.IsAny<OB.Domain.Reservations.ReservationRoomExtra>());

            var includedExtras = fixture.CreateMany<Extra>(2);
            var includedExtra1 = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = includedExtras.ElementAt(0).UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            var includedExtra2 = new OB.Domain.Reservations.ReservationRoomExtra
            {
                CreatedDate = DateTime.UtcNow,
                Extra_UID = includedExtras.ElementAt(1).UID,
                ExtraIncluded = true,
                Qty = 1,
                Total_Price = 0,
                ReservationRoom_UID = 1,
                UID = fixture.Create<long>()
            };

            #endregion ARRANGE

            var room = fixture.Build<OB.Domain.Reservations.ReservationRoom>()
                .With(x => x.UID, 1)
                .Without(x => x.ReservationRoomExtras)
                .Do(x => x.ReservationRoomExtras.Add(includedExtra1))
                .Do(x => x.ReservationRoomExtras.Add(includedExtra2))
                .Create();

            helper.SetIncludedExtras(room, fixture.Create<long>());

            _reservationRoomsExtrasRepoMock.Verify(x => x.Delete(It.Is<OB.Domain.Reservations.ReservationRoomExtra>(y => y.UID == includedExtra1.UID)), Times.Once());
            room.ReservationRoomExtras.Remove(includedExtra1);

            _reservationRoomsExtrasRepoMock.Verify(x => x.Delete(It.Is<OB.Domain.Reservations.ReservationRoomExtra>(y => y.UID == includedExtra2.UID)), Times.Once());
            room.ReservationRoomExtras.Remove(includedExtra2);

            Assert.IsTrue(room.ReservationRoomExtras.Count == 2);
            Assert.IsTrue(room.ReservationRoomExtras.Count(x => x.Extra_UID == extra.UID) == 1);
            Assert.IsTrue(room.ReservationRoomExtras.Count(x => x.Extra_UID == extra2.UID) == 1);
        }

        #endregion INCENTIVES

        #region RESERVATION TRANSACTION STATE MACHINE

        #region RESERVATION ACTION MODIFY

        #region NO TRANSACTION IN DATABASE

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Initiate_NoTransactionInDatabase()
        {
            ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                Reservation.BL.Constants.ReservationTransactionStatus.None);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Modify_NoTransactionInDatabase()
        {
            ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Modify,
                Reservation.BL.Constants.ReservationTransactionStatus.None);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Ignore_NoTransactionInDatabase()
        {
            ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Ignore,
                Reservation.BL.Constants.ReservationTransactionStatus.None);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Commit_NoTransactionInDatabase()
        {
            ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Commit,
                Reservation.BL.Constants.ReservationTransactionStatus.None);
        }

        #endregion NO TRANSACTION IN DATABASE

        #region TRANSACTION IN PENDING STATE

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Initiate_PendingTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                Reservation.BL.Constants.ReservationTransactionStatus.Pending);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Modify_PendingTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Modify,
                Reservation.BL.Constants.ReservationTransactionStatus.Pending);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Pending);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Ignore_PendingTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Ignore,
                Reservation.BL.Constants.ReservationTransactionStatus.Pending);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Ignored);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Commit_PendingTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Commit,
                Reservation.BL.Constants.ReservationTransactionStatus.Pending);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Commited);
        }

        #endregion TRANSACTION IN PENDING STATE

        #region TRANSACTION IN COMMITED STATE

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Initiate_CommitedTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                Reservation.BL.Constants.ReservationTransactionStatus.Commited);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Pending);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Modify_CommitedTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Modify,
                Reservation.BL.Constants.ReservationTransactionStatus.Commited);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Ignore_CommitedTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Ignore,
                Reservation.BL.Constants.ReservationTransactionStatus.Commited);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Commit_CommitedTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Commit,
                Reservation.BL.Constants.ReservationTransactionStatus.Commited);
        }

        #endregion TRANSACTION IN COMMITED STATE

        #region TRANSACTION IN IGNORE STATE

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Initiate_IgnoreTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                Reservation.BL.Constants.ReservationTransactionStatus.Ignored);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Pending);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Modify_IgnoreTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Modify,
                Reservation.BL.Constants.ReservationTransactionStatus.Ignored);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Ignore_IgnoreTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Ignore,
                Reservation.BL.Constants.ReservationTransactionStatus.Ignored);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Commit_IgnoreTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Commit,
                Reservation.BL.Constants.ReservationTransactionStatus.Ignored);
        }

        #endregion TRANSACTION IN IGNORE STATE

        #region TRANSACTION IN IGNORE STATE

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        public void Test_StateMachine_Modify_TypeA_Initiate_CancelledTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);

            Assert.IsTrue(status == Reservation.BL.Constants.ReservationTransactionStatus.Pending);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Modify_CancelledTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Modify,
                Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Ignore_CancelledTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Ignore,
                Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);
        }

        [TestMethod]
        [TestCategory("ModifyReservation_StateMachine")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_StateMachine_Modify_TypeA_Commit_CancelledTransaction()
        {
            var status = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify,
                Reservation.BL.Constants.ReservationTransactionType.A, Reservation.BL.Constants.ReservationTransactionAction.Commit,
                Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);
        }

        #endregion TRANSACTION IN IGNORE STATE

        #endregion RESERVATION ACTION MODIFY

        #endregion RESERVATION TRANSACTION STATE MACHINE

        #region CHILDTERMS

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        [DataRow(new int[] { 0 }, 1)]
        [DataRow(new int[] { 0, 1 }, 2)]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_GetGuestCountAfterApplyChildTerms_NoChildTerms(int[] ages, int nChilds)
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var childTerms = new List<ChildTerm>();

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, nChilds, ages.ToList());

        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_FreeChilds()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 0, "Expected count = 0");

            ages = new List<int> { 0, 1 };
            result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).First().NumberOfChilds == 2, "Expected count = 2");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_NormalChilds()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 0, "Expected count = 0");

            ages = new List<int> { 0, 1 };
            result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 2, "Expected count = 2");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_AdultChilds()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, true)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 1, "Expected count = 1");

            ages = new List<int> { 0, 1 };
            result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 0, "Expected count = 0");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).First().NumberOfChilds == 2, "Expected count = 2");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_GetGuestCountAfterApplyChildTerms_NoChildTerms()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0, 5 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);


        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_FreeAndChild()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0, 5 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 3)
                .With(x => x.AgeTo, 5)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 0, "Expected count = 0");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Adult).Any(), "Expected count = 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_Free()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 6)
                .With(x => x.AgeTo, 10)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 0, "Expected count = 0");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Child).Any(), "Expected count = 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_GetGuestCountAfterApplyChildTerms_MissingTerms()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 5 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 1, ages);

        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_FreeAndChildAndAdult()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0, 7, 11 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, true)
                .With(x => x.CountsAsAdult, false)
                .Create());

            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 6)
                .With(x => x.AgeTo, 10)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .Create());

            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 8)
                .With(x => x.AgeTo, 11)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, true)
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(fixture.Create<long>(), childTerms, 2, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).Count() == 1, "Expected count = 1");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Free).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Adult).First().NumberOfChilds == 1, "Expected count = 1");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_ChildWithPercentageVariation()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .With(x => x.IsPercentage, true)
                .With(x => x.IsValueDecrease, true)
                .With(x => x.IsActivePriceVariation, true)
                .With(x => x.Value, 15)
                .Without(x => x.ChildTermsCurrencies)
                .Do(x => x.ChildTermsCurrencies.Add(new ChildTermsCurrency
                {
                    Currency_UID = 34,
                    Value = 10,
                }))
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(34, childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).IsValueDecrease, "Expected true");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).IsPriceVariationPerc, "Expected true");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).PriceVariation == 15, "Expected 10");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_ChildWithValueVariation()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .With(x => x.IsPercentage, false)
                .With(x => x.IsValueDecrease, false)
                .With(x => x.IsActivePriceVariation, true)
                .Without(x => x.ChildTermsCurrencies)
                .Do(x => x.ChildTermsCurrencies.Add(new ChildTermsCurrency
                {
                    Currency_UID = 34,
                    Value = 10,
                }))
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(34, childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).IsValueDecrease, "Expected false");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).IsPriceVariationPerc, "Expected false");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 34).PriceVariation == 10, "Expected 10");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_ChildTerms")]
        public void Test_GetGuestCountAfterApplyChildTerms_ChildWithMultipleCurrencyVariation()
        {
            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var ages = new List<int> { 0 };
            var childTerms = new List<ChildTerm>();
            childTerms.Add(fixture.Build<ChildTerm>()
                .With(x => x.AgeFrom, 0)
                .With(x => x.AgeTo, 2)
                .With(x => x.IsFree, false)
                .With(x => x.CountsAsAdult, false)
                .With(x => x.IsPercentage, false)
                .With(x => x.IsValueDecrease, false)
                .With(x => x.IsActivePriceVariation, true)
                .Without(x => x.ChildTermsCurrencies)
                .Do(x => x.ChildTermsCurrencies.Add(new ChildTermsCurrency
                {
                    Currency_UID = 34,
                    Value = 10,
                }))
                .Do(x => x.ChildTermsCurrencies.Add(new ChildTermsCurrency
                {
                    Currency_UID = 1,
                    Value = 20,
                }))
                .Create());

            var result = helper.GetGuestCountAfterApplyChildTerms(1, childTerms, 1, ages);

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).Count() == 1, "Expected count = 1");

            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().NumberOfChilds == 1, "Expected count = 1");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 1).IsValueDecrease, "Expected false");
            Assert.IsTrue(!result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 1).IsPriceVariationPerc, "Expected false");
            Assert.IsTrue(result.Where(x => x.PriceType == ChildPriceType.Child).First().PriceVariations.First(x => x.Currency_UID == 1).PriceVariation == 20, "Expected 10");
        }

        #endregion CHILDTERMS

        #region UPDATE CREDITS

        #region OPERATORS

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateOperatorCreditUsed_NoChannel()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();

            bool sendCreditLimitExceedEmail = true;
            string channelName = "asdasd";
            decimal creditLimit = 10;

            helper.UpdateOperatorCreditUsed(fixture.Create<long>(), null, fixture.Create<long>(), fixture.Create<bool>(),
                fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
            Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
            Assert.IsTrue(creditLimit == 0, "Expected 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateOperatorCreditUsed_NoPaymentMethodType()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();

            bool sendCreditLimitExceedEmail = true;
            string channelName = "asdasd";
            decimal creditLimit = 10;

            helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), null, fixture.Create<bool>(),
                fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
            Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
            Assert.IsTrue(creditLimit == 0, "Expected 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateOperatorCreditUsed_IsOnRequest()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            bool sendCreditLimitExceedEmail = true;
            string channelName = "asdasd";
            decimal creditLimit = 10;

            helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), fixture.Create<long>(), true,
                fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
            Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
            Assert.IsTrue(creditLimit == 0, "Expected 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateOperatorCreditUsed_InvalidPaymentType()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            bool sendCreditLimitExceedEmail = true;
            string channelName = "asdasd";
            decimal creditLimit = 10;
            var paymentApproved = true;

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { }));

            _channelPropertiesRepoMock.Setup(x => x.UpdateOperatorCreditUsed(new Contracts.Requests.UpdateOperatorCreditUsedRequest { ChannelId = It.IsAny<long>(), PropertyId = It.IsAny<long>(), PaymentMethodCode = It.IsAny<int>(), CreditValue = It.IsAny<decimal>(),
                SendCreditLimitExcededEmail = sendCreditLimitExceedEmail, ChannelName = channelName, CreditLimit = creditLimit, PaymentApproved = paymentApproved }));

            helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), fixture.Create<long>(), true,
                fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
            Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
            Assert.IsTrue(creditLimit == 0, "Expected 0");
        }

        #endregion OPERATORS

        #region TPI

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateTPICreditUsed_NoTPI()
        {
            FillThePaymentMethodTypeforTest();
            var helper = Container.Resolve<IReservationHelperPOCO>();

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<Contracts.Requests.ListPaymentMethodTypesRequest>()))
      .Callback<Contracts.Requests.ListPaymentMethodTypesRequest>(x => Send(x)).Returns(_paymentMethodTypeInDataBase);

            _channelPropertiesRepoMock.Setup(x => x.UpdateTPICreditUsed(It.IsAny<Contracts.Requests.UpdateTPICreditUsedRequest>()))
                .Callback<Contracts.Requests.UpdateTPICreditUsedRequest>(x => Send(x)).Returns(new Contracts.Responses.UpdateTPICreditUsedResponse
                {
                    PaymentApproved = true,
                    SendCreditLimitExcededEmail = false
                });

            bool sendCreditLimitExceedEmail = true;

            helper.UpdateTPICreditUsed(fixture.Create<long>(), null, fixture.Create<long>(), fixture.Create<decimal>(),
                out sendCreditLimitExceedEmail);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        public void Test_UpdateTPICreditUsed_NoPaymentMethodType()
        {
            FillThePaymentMethodTypeforTest();
            var helper = Container.Resolve<IReservationHelperPOCO>();

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<Contracts.Requests.ListPaymentMethodTypesRequest>()))
                .Callback<Contracts.Requests.ListPaymentMethodTypesRequest>(x => Send(x)).Returns(_paymentMethodTypeInDataBase);

            _channelPropertiesRepoMock.Setup(x => x.UpdateTPICreditUsed(It.IsAny<Contracts.Requests.UpdateTPICreditUsedRequest>()))
                .Callback<Contracts.Requests.UpdateTPICreditUsedRequest>(x => Send(x)).Returns(new Contracts.Responses.UpdateTPICreditUsedResponse
                {
                    PaymentApproved = true,
                    SendCreditLimitExcededEmail = false
                });

            bool sendCreditLimitExceedEmail = true;

            helper.UpdateTPICreditUsed(fixture.Create<long>(), fixture.Create<long>(), null, fixture.Create<decimal>(),
                out sendCreditLimitExceedEmail);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        //[ExpectedException(typeof(BusinessLayerException))]
        public void Test_UpdateTPICreditUsed_InvalidPaymentType()
        {
            FillThePaymentMethodTypeforTest();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<Contracts.Requests.ListPaymentMethodTypesRequest>()))
                .Callback<Contracts.Requests.ListPaymentMethodTypesRequest>(x=> Send(x)).Returns(_paymentMethodTypeInDataBase);

            _channelPropertiesRepoMock.Setup(x => x.UpdateTPICreditUsed(It.IsAny<Contracts.Requests.UpdateTPICreditUsedRequest>()))
                .Callback<Contracts.Requests.UpdateTPICreditUsedRequest>(x => Send(x)).Returns(new Contracts.Responses.UpdateTPICreditUsedResponse {
                    PaymentApproved = true,
                    SendCreditLimitExcededEmail = false});

            bool sendCreditLimitExceedEmail = true;

            helper.UpdateTPICreditUsed(fixture.Create<long>(), fixture.Create<long>(), 0, fixture.Create<decimal>(),
                out sendCreditLimitExceedEmail);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        }

        #endregion TPI

        #endregion UPDATE CREDITS

        #region PRICE CALCULATION
        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculation()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion

            var rrdList = helper.CalculateReservationRoomPrices(parameters).ToList();

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);

            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;

            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 190, 190, 190, 190, 190, 190);

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;

            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #region CHILD TERMS
        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsFreeChilds()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 0 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20, 2)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 0, 2, false, null, null, null, true, true, false, false, null) // Free Child
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 2, false);
            #endregion


            // Free Child
            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 110, 110, 110, 110, 110, 110);

            // 2 Free Child
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Add(1);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 110, 110, 110, 110, 110, 110);

            // 3 Free Child tieh max free childs = 2
            room.AdultCount = 1;
            room.ChildCount = 3;
            ages.Add(1);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChild()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null) // Normal Child
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 3;
            ages.Add(4);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 80, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentagePlus()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 9, 10, false, 10, true, false, false, true, false, true, currencies) // Variation percentage +
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 66, 176, 176, 176, 176, 176, 176);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageMinus()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 11 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 11, 12, false, 10, true, false, false, true, true, true, currencies) // Variation percentage -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 2,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 54, 174, 174, 174, 174, 174, 174);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationValuePlus()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 13 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 13, 14, false, 10, false, false, false, true, false, true, currencies) // Variation Value +
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion

            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationValueMinus()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 15, 16, false, 10, false, false, false, true, true, true, currencies) // Variation Value -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            // Variation Value -
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Clear();
            ages.Add(15);
            ages.Add(16);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 50, 160, 160, 160, 160, 160, 160);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageAndValue()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9, 15 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 9, 10, false, 10, true, false, false, true, false, true, currencies) // Variation percentage +
                        .AddChiltTerm("", 15, 16, false, 10, false, false, false, true, true, true, currencies) // Variation Value -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 2
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            //  Variation percentage +
            //  Variation Value -
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 63.50M, 173.5M, 173.5M, 173.5M, 173.5M, 173.5M, 173.5M);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculation")]
        public void Test_PriceCalculationWithChildTermsAdultChild()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 6 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 6, 8, true, null, null, null, false, true, false, false, null) // Child As Adult
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, true);
            #endregion


            // Free Child
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 0, 120, 120, 120, 120, 120, 120);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            // Normal Child
            room.AdultCount = 2;
            room.ChildCount = 1;
            ages.Clear();
            ages.Add(8);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 130, 0, 130, 130, 130, 130, 130, 130);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            // Child as Adult
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Clear();
            ages.Add(7);
            ages.Add(7);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 130, 0, 130, 130, 130, 130, 130, 130);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region PRICE ADDON
        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationAddon")]
        public void Test_PriceCalculationPlusValueAddon()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, false)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();
            //Obter do ratechannels o valor a adicionar

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 200, 200, 200, 200, 200, 200);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationAddon")]
        public void Test_PriceCalculationMinusValueAddon()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, true)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 160, 160, 160, 160, 160, 160);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationAddon")]
        public void Test_PriceCalculationPlusPercentageAddon()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, true, false)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion

            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 187, 187, 187, 187, 187, 187);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 209, 209, 209, 209, 209, 209);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 198, 198, 198, 198, 198, 198);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationAddon")]
        public void Test_PriceCalculationMinusPercentageAddon()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, true, true)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 153, 153, 153, 153, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 171, 171, 171, 171, 171, 171);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 162, 162, 162, 162, 162, 162);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region PRICE RATE BUYER GROUP
        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationBuyerGroup")]
        public void Test_PriceCalculationGDSRateBuyerGroup()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.GDS));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
                TpiId = builder.InputData.Tpi.UID,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 190, 190, 190, 190, 190);
            Assert.AreEqual(5, rrdList.Count, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationBuyerGroup")]
        public void Test_PriceCalculationRateBuyerGroup()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
                TpiId = builder.InputData.Tpi.UID
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region PRICE PROMOTIONAL CODE
        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationPromoCodes")]
        public void Test_PriceCalculationPromotionalCodeValue()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var dateNow = DateTime.Now;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = dateNow,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", dateNow.Date, DateTime.Now.AddDays(10).Date, 10M, false)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var validPromocodeInfo = new ValidPromocodeParameters()
            {
                PromoCodeObj = builder.InputData.PromoCode,
                ReservationRoomsPeriods = new List<internalObjs.ReservationRoomStayPeriod>()
                {
                    new internalObjs.ReservationRoomStayPeriod(true) { RateUID = room.Rate_UID.Value, CheckIn = room.DateFrom.Value, CheckOut = room.DateTo.Value }
                },
                NewDaysToApplyDiscount = new List<DateTime>() { dateNow.Date, dateNow.Date.AddDays(1), dateNow.Date.AddDays(2), dateNow.Date.AddDays(3), dateNow.Date.AddDays(4) }
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                ValidPromocodeParameters = validPromocodeInfo
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion

            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 168, 170, 168, 168);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationPromoCodes")]
        public void Test_PriceCalculationPromotionalCodePercentage()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var dateNow = DateTime.Now;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = dateNow,
                            CheckOut = dateNow.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", dateNow.Date, dateNow.AddDays(10).Date, 10M, true)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var validPromocodeInfo = new ValidPromocodeParameters()
            {
                PromoCodeObj = builder.InputData.PromoCode,
                ReservationRoomsPeriods = new List<internalObjs.ReservationRoomStayPeriod>()
                {
                    new internalObjs.ReservationRoomStayPeriod(true) { RateUID = room.Rate_UID.Value, CheckIn = room.DateFrom.Value, CheckOut = room.DateTo.Value }
                },
                NewDaysToApplyDiscount = new List<DateTime>() { dateNow.Date, dateNow.Date.AddDays(1), dateNow.Date.AddDays(2), dateNow.Date.AddDays(3), dateNow.Date.AddDays(4) }
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                ValidPromocodeParameters = validPromocodeInfo
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 153, 170, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationPromoCodes")]
        public void Test_PriceCalculationInvalidPromotionalCode()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var dateNow = DateTime.Now;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = dateNow,
                            CheckOut = dateNow.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123", dateNow.Date.AddDays(5), dateNow.AddDays(10).Date, 10, false)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var validPromocodeInfo = new ValidPromocodeParameters()
            {
                PromoCodeObj = builder.InputData.PromoCode,
                ReservationRoomsPeriods = new List<internalObjs.ReservationRoomStayPeriod>()
                {
                    new internalObjs.ReservationRoomStayPeriod(true) { RateUID = room.Rate_UID.Value, CheckIn = room.DateFrom.Value, CheckOut = room.DateTo.Value }
                }
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(5),
                BaseCurrency = 34,
                AdultCount = 1,
                ChildCount = 1,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = false,
                PropertyId = 1806,
                RateId = builder.InputData.Rates[0].UID,
                RoomTypeId = builder.InputData.RoomTypes[0].UID,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                ValidPromocodeParameters = validPromocodeInfo
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceCalculationPromoCodes")]
        public void Test_PriceCalculationUsedPromotionalCode()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var dateNow = DateTime.Now;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10, false, 10, 10)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var validPromocodeInfo = new ValidPromocodeParameters()
            {
                PromoCodeObj = builder.InputData.PromoCode,
                ReservationRoomsPeriods = new List<internalObjs.ReservationRoomStayPeriod>()
                {
                    new internalObjs.ReservationRoomStayPeriod(true) { RateUID = room.Rate_UID.Value, CheckIn = room.DateFrom.Value, CheckOut = room.DateTo.Value }
                }
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(5),
                BaseCurrency = 34,
                AdultCount = 1,
                ChildCount = 1,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = false,
                PropertyId = 1806,
                RateId = builder.InputData.Rates[0].UID,
                RoomTypeId = builder.InputData.RoomTypes[0].UID,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                ValidPromocodeParameters = validPromocodeInfo
            };

            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion

        #region PRICE INCENTIVES
        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_PriceCalculationIncentive()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 153, 153, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_PriceCalculationIncentiveCumulative()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder, true);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 137.7M, 137.7M, 137.7M, 137.7M);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }


        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_AppliedIncentivesCumulative()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, false, true);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder, true);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion

            #region EXPECTED DATA
            var expectedRrdList = new List<RateRoomDetailReservation>();

            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now,
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 0,
                PriceAfterIncentives = 0,
                PriceAfterRateModel = 0,
                FinalPrice = 0
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(1),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 0,
                PriceAfterIncentives = 0,
                PriceAfterRateModel = 0,
                FinalPrice = 0
            });

            #endregion

            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.SelectMany(s => s.AppliedIncentives).Count()  == 2, "Expected Applied Incentives = 2");
        }


        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_SetReservationRoomTaxPolicies_With_FreeNights()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var helperPriceCalculation = Container.Resolve<IReservationPricesCalculationPOCO>();
            var helper = Container.Resolve<IReservationHelperPOCO>();

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, false, true);


            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();


            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                ReservationRoomDetails = new List<ReservationRoomDetail>()
            };

            for (int i = 0; i < 5; i++)
                room.ReservationRoomDetails.Add(new ReservationRoomDetail());

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var taxPolicies = new List<TaxPolicy>
            {
                new TaxPolicy
                {
                    Value = 10,
                    IsPerNight = true,
                    Rate_UID = 1
                }
            };

            #region Mock Repo Calls
            MockIncentives(builder, true);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion

            var rrdList = helperPriceCalculation.CalculateReservationRoomPrices(parameters);
            helper.SetReservationRoomTaxPolicies(room, taxPolicies, 1, rrdList);
            Assert.IsTrue(rrdList.SelectMany(s => s.AppliedIncentives).Count() == 2, "Expected Applied Incentives = 2");
            Assert.IsTrue(room.TotalTax == 30.00m, "Expected Total Tax = 30.00");
        }


        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_SetReservationRoomTaxPolicies_RoomDomainReservation_With_FreeNights()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var helperPriceCalculation = Container.Resolve<IReservationPricesCalculationPOCO>();
            var helper = Container.Resolve<IReservationHelperPOCO>();

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 0, 1, 3, false, true);


            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();


            var room = new  domainReservation.ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                ReservationRoomDetails = new List<domainReservation.ReservationRoomDetail>()
            };

            for (int i = 0; i < 5; i++)
                room.ReservationRoomDetails.Add(new domainReservation.ReservationRoomDetail());

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var taxPolicies = new List<TaxPolicy>
            {
                new TaxPolicy
                {
                    Value = 10,
                    IsPerNight = true,
                    Rate_UID = 1
                }
            };

            #region Mock Repo Calls
            MockIncentives(builder, true);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion

            var rrdList = helperPriceCalculation.CalculateReservationRoomPrices(parameters);
            helper.SetReservationRoomTaxPolicies(room, taxPolicies, 1, rrdList);
            Assert.IsTrue(rrdList.SelectMany(s => s.AppliedIncentives).Count() == 2, "Expected Applied Incentives = 2");
            Assert.IsTrue(room.TotalTax == 30.00m, "Expected Total Tax = 30.00");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceIncentives")]
        public void Test_PriceCalculationIncentiveDiferentPriceDays()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 10, 1, 3, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder, true);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            var expectedRrdList = new List<RateRoomDetailReservation>();

            #region EXPECTED DATA

            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now,
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(1),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 0,
                PriceAfterIncentives = 0,
                PriceAfterRateModel = 0,
                FinalPrice = 0
            });

            #endregion

            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region PRICE RATE MODEL
        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelPackage()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 5
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: true, commissionType: 5);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelMarkup()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 2
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: true, commissionType: 2);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 136, 136);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelCommission()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: true, commissionType: 1);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelNETCommission()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, false).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: false, commissionType: 1);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 242.86M, 242.86M);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelNETNotHoteisNet()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, false).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: true, commissionType: 1);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_PriceRateModel")]
        public void Test_PriceCalculationRateModelRetailNotHoteisNet()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, priceModel: true, commissionType: 1);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region MIX MULTIPLE
        [TestMethod]
        [TestCategory("ModifyReservation_PriceMixMultiple")]
        public void Test_PriceCalculationMixMultiple()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 1 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 0, 2, false, null, null, null, true, true, false, false, null) // Free Child
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationPricesCalculationPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TpiId = builder.InputData.Tpi.UID
            };


            #region Mock Repo Calls
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 2, false);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            #endregion


            var rrdList = helper.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 120, 130, 117, 117, 117, 117);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }
        #endregion


        #region Aux Methods
        #region Reservation Operations Aux
        private contractsReservations.Reservation InsertReservation(SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId = "", bool singleRoom = false)
        {
            //TODO: Attention, in the future we should remove the singleRoom parameter and review the builder logic. This requires to change all the unit tests with one room

            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 0, 2 };
            var currencies = new List<ChildTermsCurrency>() { new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            }};

            searchBuilder
                .AddRate()
                .AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                .AddRateRoomsAll()
                .AddRateChannelsAll(channels)
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 1,
                    ChildCount = 0,
                    CheckIn = DateTime.Now.AddDays(10),
                    CheckOut = DateTime.Now.AddDays(15)
                })
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 1,
                    ChildCount = 2,
                    CheckIn = DateTime.Now.AddDays(11),
                    CheckOut = DateTime.Now.AddDays(15),
                })
                .AddPropertyChannels(channels)
                .WithAdultPrices(3, 100)
                .WithChildPrices(3, 50)
                .AddChiltTerm()
                .AddChiltTerm("", 2, 3, false, 10, false, false, true, true, true, true, currencies)
                .WithAllotment(100)
                .AddPromoCode(1806, searchBuilder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10M, false)
                .AddPromoCodeCurrencies(searchBuilder.InputData.PromoCode.UID, 34, 15M)
                .AddIncentive(1806, searchBuilder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false);

            searchBuilder.AddRateCancellationPolicy(searchBuilder.InputData.Rates[0].UID);
            searchBuilder.CreateRateRoomDetails();

            // Prices For The Modification
            searchBuilder.CreateRateRoomDetails(DateTime.Now.AddDays(16), DateTime.Now.AddDays(20), 50);

            resBuilder = GetReservationBuilder(searchBuilder, childAges, singleRoom);

            //Mock here!!!
            MockReservationsRepo(searchBuilder, resBuilder, transactionId);
            MockObPropertyRepo(resBuilder);
            MockReservationsRoomsRepo();
            MockReservationHelperPockMethods();
            MockResRoomDetailsRepo();
            MockResChildRepo(resBuilder);
            MockObRatesRepo(searchBuilder);
            MockObCurrenciesRepo();
            MockVisualStates();
            MockObChannelsRepo(resBuilder);
            MockReservationFiltersRepo();
            MockGetReservationLookups();
            MockResAddDataRepo();
            MockAppRepository();
            MockChildTermsRepo(searchBuilder);
            MockTaxPoliciesRepo();
            resBuilder.InputData.guest.UID = 1;
            //InitializeLookupsMock(null, new List<Guest>{
            //    OB.BL.Operations.Internal.TypeConverters.OtherConverter.Convert(resBuilder.InputData.guest)
            //});
            var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, false, true, false, false, false, transactionId);

            var response = resBuilder.GetReservation(ReservationManagerPOCO, result);
            return response.Result.First();
        }

        private ReservationDataBuilder GetReservationBuilder(SearchBuilder builder, List<int> childAges = null, bool addOneRoomOnly = false)
        {
            var resBuilder = new ReservationDataBuilder(1, 1806).WithNewGuest();

            resBuilder.WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                            builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                            builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID);

            resBuilder.WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

            resBuilder.WithRoomDetails(1, builder.InputData.RateRoomDetails.Where(x => x.Date >= builder.InputData.SearchParameter[0].CheckIn
                && x.Date <= builder.InputData.SearchParameter[0].CheckOut).ToList());

            // Second Room
            if (!addOneRoomOnly)
            {
                resBuilder.WithRoom(2, builder.InputData.SearchParameter[1].AdultCount,
                                builder.InputData.SearchParameter[1].ChildCount, builder.InputData.SearchParameter[1].CheckIn,
                                builder.InputData.SearchParameter[1].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID);

                resBuilder.WithChildren(2, builder.InputData.SearchParameter[1].ChildCount, childAges);

                resBuilder.WithRoomDetails(2, builder.InputData.RateRoomDetails.Where(x => x.Date >= builder.InputData.SearchParameter[1].CheckIn
                    && x.Date <= builder.InputData.SearchParameter[1].CheckOut).ToList());
            }

            resBuilder.WithCancelationPolicy(false, 0, true, 1, null, 0);

            if(!addOneRoomOnly)
                resBuilder.WithCancelationPolicy(false, 0, true, 1, null, 1);

            return resBuilder;
        }
        #endregion
        private int GetAdultPriceTest(RateRoomDetail rrd, int numAds)
        {
            switch(numAds)
            {
                case 1:
                    return (int)rrd.Adult_1.GetValueOrDefault();
                case 2:
                    return (int)rrd.Adult_2.GetValueOrDefault();
                case 3:
                    return (int)rrd.Adult_3.GetValueOrDefault();
            }

            return 0;
        }
        private int GetChildPriceTest(RateRoomDetail rrd, int numChs, int freeChilds = 0, bool countAsAdult = false)
        {
            if (countAsAdult)
                return 0;

            numChs -= freeChilds;

            switch (numChs)
            {
                case 1:
                    return (int)rrd.Child_1.GetValueOrDefault();
                case 2:
                    return (int)rrd.Child_2.GetValueOrDefault();
                case 3:
                    return (int)rrd.Child_3.GetValueOrDefault();
            }

            return 0;
        }

        private string GetCurrencySymbol(long currencyUid)
        {
            switch(currencyUid)
            {
                case 34:
                    return "EUR";
                case 16:
                    return "BRL";
                default:
                    return string.Empty;
            }
        }

        #endregion
        #endregion PRICE CALCULATION

        /// <summary>
        /// Test Adult Count
        /// Test Child Count
        /// Test Child Ages
        /// Test CheckIn
        /// Test CheckOut
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModificationBasicOneRoom()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockObRatesRepo(builder);
            MockReservationRepo();
            #endregion

            #region Mock SQL Manager
            int callTime = 0;
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns((string query) =>
                            {
                                callTime++;
                                if (callTime == 4)
                                    return 5;

                                return 0;
                            });


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });

            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room

            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            //Compare the reservations
            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }


        #region ModifyReservation with PO Rules
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModificationBasicOneRoom_WithRepresentativeAndOperatorRulePortal()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockReservationTransactionState(2);
            MockResAddDataRepo();
            #endregion

            #region Mock SQL Manager
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns(5);


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });
            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room
            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false,true,5);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            var request = new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.Pull,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            };

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(request, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            //Compare the reservations
            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);

            var reservationHelper = _reservationManagerPOCO.Resolve<IReservationHelperPOCO>();
            var resAdditionalData = reservationHelper.GetReservationAdditionalDataJsonObject(ref _reservationAdditionalData, 0);

            string areEqMsg = "'{0}' value is different.";

            Assert.IsNotNull(resAdditionalData);

            var result = _rrdRepoMock.Object.ListRateRoomDetailForReservationRoom(new OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest
            {
                CheckIn = request.ReservationRooms[0].DateFrom.Value,
                CheckOut = request.ReservationRooms[0].DateTo.Value,
                PropertyId = oldReservation.Property_UID,
                RateId = oldReservation.ReservationRooms[0].Rate_UID.Value,
                RoomtypeId = oldReservation.ReservationRooms[0].RoomType_UID.Value,
                AdultCount = request.ReservationRooms[0].AdultCount.Value,
                ChildCount = request.ReservationRooms[0].ChildCount.Value,
                ChannelId = oldReservation.Channel_UID.Value,
            });

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = resAdditionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 787.500M; //783.750M
            decimal expectedChannel = 862.500M;// 851.250M;

            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = resAdditionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModificationBasicOneRoom_WithRepresentativeRulePortal()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockReservationTransactionState(2);
            MockResAddDataRepo();
            #endregion

            #region Mock SQL Manager
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns(5);


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });

            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room
            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, true, 5);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.Pull,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            //Compare the reservations
            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);

            var reservationHelper = _reservationManagerPOCO.Resolve<IReservationHelperPOCO>();
            var resAdditionalData = reservationHelper.GetReservationAdditionalDataJsonObject(ref _reservationAdditionalData, 0);

            string areEqMsg = "'{0}' value is different.";

            Assert.IsNotNull(resAdditionalData);

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = resAdditionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 787.500M; //783.750M

            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = resAdditionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservationWithTwoPORulesAndOmnibeesRateTypePVP()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockReservationTransactionState(2);
            MockResAddDataRepo();
            #endregion

            #region Mock SQL Manager
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns(5);


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });
            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room
            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, true, 1);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.Pull,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            //Compare the reservations
            Assert.AreEqual(0, modifyResponse.Errors.Count);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);

            var reservationHelper = _reservationManagerPOCO.Resolve<IReservationHelperPOCO>();
            var resAdditionalData = reservationHelper.GetReservationAdditionalDataJsonObject(ref _reservationAdditionalData, 0);

            string areEqMsg = "'{0}' value is different.";

            Assert.IsNotNull(resAdditionalData);

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = resAdditionalData.ExternalSellingReservationInformationByRule;
            //REP
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = resAdditionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(0, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(675M, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_BeforeWithRulesAndAfterModificationWithoutRules()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockPortalRulesRepository(PORule.None);
            MockReservationTransactionState(2);
            MockResAddDataRepo();
            #endregion

            #region Mock SQL Manager
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns(5);


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });
            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.Pull));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room
            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false, true, 5);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.Pull,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            //Compare the reservations
            Assert.AreEqual(0, modifyResponse.Errors.Count);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);

            var reservationHelper = _reservationManagerPOCO.Resolve<IReservationHelperPOCO>();
            var resAdditionalData = reservationHelper.GetReservationAdditionalDataJsonObject(ref _reservationAdditionalData, 0);

            string areEqMsg = "'{0}' value is different.";

            Assert.IsNotNull(resAdditionalData);

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = resAdditionalData.ExternalSellingReservationInformationByRule;
            var inputExternalSellingRoomInfoRule = resAdditionalData.ReservationRoomList.First().ExternalSellingInformationByRule;

            Assert.IsTrue(inputExternalSellingResInfoRule.Count == 0);
            Assert.IsTrue(inputExternalSellingRoomInfoRule.Count == 0);
        }
        #endregion


        /// <summary>
        /// Test Only Change Main Guest Name
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_ChangeOnlyMainGuestName()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockReservationRepo();
            MockReservationFiltersRepo();
            #endregion

            //Do reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms[1];
            room.AdultCount = 1;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(11).Date;
            room.DateTo = DateTime.Now.AddDays(15).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 0, 2 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;


            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                Guest = new contractsReservations.ReservationGuest()
                {
                    FirstName = "Modified",
                    LastName = "Name Test"
                },
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.GuestFirstName != oldReservation.GuestFirstName);
            Assert.IsTrue(retunedReservation.GuestLastName != oldReservation.GuestLastName);
            ReservationAssert.AssertAllReservationObjectsForMainGuestChange(oldReservation, modifiedReservation);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }

        /// <summary>
        /// Test Only Change guest information
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_ChangeOnlyGuestInformation()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);
            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockReservationRepo();
            MockReservationFiltersRepo();
            #endregion

            #region Do reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms[1];
            room.AdultCount = 1;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(11).Date;
            room.DateTo = DateTime.Now.AddDays(15).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 0, 2 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;
            #endregion

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                Guest = new contractsReservations.ReservationGuest()
                {
                    FirstName = "Modified",
                    LastName = "Name Test",
                    Address1 = "rua do arroz",
                    Address2 = "rua do adeus",
                    City = "caca",
                    CountryId = 70,
                    Email = "ze.manel@hotmail.com",
                    IdCardNumber = "34837493274",
                    Phone = "7394839473",
                    PostalCode = "3435",
                    StateId = 321
                },
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.GuestFirstName != oldReservation.GuestFirstName);
            Assert.IsTrue(retunedReservation.GuestLastName != oldReservation.GuestLastName);
            Assert.IsTrue(retunedReservation.GuestAddress1 != oldReservation.GuestAddress1);
            Assert.IsTrue(retunedReservation.GuestAddress2 != oldReservation.GuestAddress2);
            Assert.IsTrue(retunedReservation.GuestCity != oldReservation.GuestCity);
            Assert.IsTrue(retunedReservation.GuestCountry_UID != oldReservation.GuestCountry_UID);
            Assert.IsTrue(retunedReservation.GuestEmail != oldReservation.GuestEmail);
            Assert.IsTrue(retunedReservation.GuestIDCardNumber != oldReservation.GuestIDCardNumber);
            Assert.IsTrue(retunedReservation.GuestPhone != oldReservation.GuestPhone);
            Assert.IsTrue(retunedReservation.GuestPostalCode != oldReservation.GuestPostalCode);
            Assert.IsTrue(retunedReservation.GuestState_UID != oldReservation.GuestState_UID);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }

        /// <summary>
        /// Test Only Change billing information
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_ChangeOnlyBillingInformation()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);
            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockReservationRepo();
            MockReservationFiltersRepo();
            #endregion

            #region Do reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms[1];
            room.AdultCount = 1;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(11).Date;
            room.DateTo = DateTime.Now.AddDays(15).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 0, 2 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;
            #endregion

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                BillingInfo = new contractsReservations.BillingInfo()
                {
                    ContactName = "este",
                    Address1 = "rua do arroz",
                    Address2 = "rua do adeus",
                    City = "caca",
                    CountryId = 70,
                    Email = "ze.manel@hotmail.com",
                    TaxCardNumber = "34837493274",
                    Phone = "7394839473",
                    PostalCode = "3435",
                    StateId = 321
                },
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.BillingContactName != oldReservation.BillingContactName);
            Assert.IsTrue(retunedReservation.BillingAddress1 != oldReservation.BillingAddress1);
            Assert.IsTrue(retunedReservation.BillingAddress2 != oldReservation.BillingAddress2);
            Assert.IsTrue(retunedReservation.BillingCity != oldReservation.BillingCity);
            Assert.IsTrue(retunedReservation.BillingCountry_UID != oldReservation.BillingCountry_UID);
            Assert.IsTrue(retunedReservation.BillingEmail != oldReservation.BillingEmail);
            Assert.IsTrue(retunedReservation.BillingTaxCardNumber != oldReservation.BillingTaxCardNumber);
            Assert.IsTrue(retunedReservation.BillingPhone != oldReservation.BillingPhone);
            Assert.IsTrue(retunedReservation.BillingPostalCode != oldReservation.BillingPostalCode);
            Assert.IsTrue(retunedReservation.BillingState_UID != oldReservation.BillingState_UID);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }

        /// <summary>
        /// Test Only Change guest information and billing information
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_ChangeOnlyGuestAndBillingInformation()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);
            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockReservationRepo();
            MockReservationFiltersRepo();
            #endregion

            #region Do reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms[1];
            room.AdultCount = 1;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(11).Date;
            room.DateTo = DateTime.Now.AddDays(15).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 0, 2 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;
            #endregion

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                Guest = new contractsReservations.ReservationGuest()
                {
                    FirstName = "Modified",
                    LastName = "Name Test",
                    Address1 = "rua do arroz",
                    Address2 = "rua do adeus",
                    City = "caca",
                    CountryId = 70,
                    Email = "ze.manel@hotmail.com",
                    IdCardNumber = "34837493274",
                    Phone = "7394839473",
                    PostalCode = "3435",
                    StateId = 321
                },
                BillingInfo = new contractsReservations.BillingInfo()
                {
                    ContactName = "este",
                    Address1 = "rua do arroz",
                    Address2 = "rua do adeus",
                    City = "caca",
                    CountryId = 70,
                    Email = "ze.manel@hotmail.com",
                    TaxCardNumber = "34837493274",
                    Phone = "7394839473",
                    PostalCode = "3435",
                    StateId = 321
                },
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.BillingContactName != oldReservation.BillingContactName);
            Assert.IsTrue(retunedReservation.BillingAddress1 != oldReservation.BillingAddress1);
            Assert.IsTrue(retunedReservation.BillingAddress2 != oldReservation.BillingAddress2);
            Assert.IsTrue(retunedReservation.BillingCity != oldReservation.BillingCity);
            Assert.IsTrue(retunedReservation.BillingCountry_UID != oldReservation.BillingCountry_UID);
            Assert.IsTrue(retunedReservation.BillingEmail != oldReservation.BillingEmail);
            Assert.IsTrue(retunedReservation.BillingTaxCardNumber != oldReservation.BillingTaxCardNumber);
            Assert.IsTrue(retunedReservation.BillingPhone != oldReservation.BillingPhone);
            Assert.IsTrue(retunedReservation.BillingPostalCode != oldReservation.BillingPostalCode);
            Assert.IsTrue(retunedReservation.BillingState_UID != oldReservation.BillingState_UID);
            Assert.IsTrue(retunedReservation.GuestFirstName != oldReservation.GuestFirstName);
            Assert.IsTrue(retunedReservation.GuestLastName != oldReservation.GuestLastName);
            Assert.IsTrue(retunedReservation.GuestAddress1 != oldReservation.GuestAddress1);
            Assert.IsTrue(retunedReservation.GuestAddress2 != oldReservation.GuestAddress2);
            Assert.IsTrue(retunedReservation.GuestCity != oldReservation.GuestCity);
            Assert.IsTrue(retunedReservation.GuestCountry_UID != oldReservation.GuestCountry_UID);
            Assert.IsTrue(retunedReservation.GuestEmail != oldReservation.GuestEmail);
            Assert.IsTrue(retunedReservation.GuestIDCardNumber != oldReservation.GuestIDCardNumber);
            Assert.IsTrue(retunedReservation.GuestPhone != oldReservation.GuestPhone);
            Assert.IsTrue(retunedReservation.GuestPostalCode != oldReservation.GuestPostalCode);
            Assert.IsTrue(retunedReservation.GuestState_UID != oldReservation.GuestState_UID);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }

        /// <summary>
        /// Test Only Change Main Guest Name
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModifyReservation_ChangeOnlyRoomGuestName()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockReservationRepo();
            MockReservationFiltersRepo();
            #endregion

            //Do reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms[1];
            room.AdultCount = 1;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(11).Date;
            room.DateTo = DateTime.Now.AddDays(15).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 0, 2 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion


            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo,
                            Guest = new contractsReservations.ReservationGuest()
                            {
                                FirstName = "Modified",
                                LastName = "Name Test"
                            }
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo,
                            Guest = new contractsReservations.ReservationGuest()
                            {
                                FirstName = "Modified",
                                LastName = "Name Test"
                            }
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId,
                RequestGuid = new Guid ("b89dc800-e3da-4914-827a-40a2ec52ef4e"),
                RequestId = "For Control"
            }, ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.ReservationRooms[0].GuestName != oldReservation.ReservationRooms[0].GuestName);
            Assert.IsTrue(retunedReservation.ReservationRooms[1].GuestName != oldReservation.ReservationRooms[1].GuestName);
            ReservationAssert.AssertAllReservationObjectsForRoomsGuestChange(oldReservation, modifiedReservation);
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
            Assert.AreEqual(modifyResponse.RequestGuid, new Guid("b89dc800-e3da-4914-827a-40a2ec52ef4e"));
            Assert.AreEqual(modifyResponse.RequestId, "For Control");

        }


        #region Modify Test ExchangeRate Changes

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModificationTwoRooms_NoChangeExchangeRate()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockObRatesRepo(builder);
            MockReservationRepo();
            #endregion

            #region Mock SQL Manager
            int callTime = 0;
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns((string query) =>
                            {
                                callTime++;
                                if (callTime == 4)
                                    return 5;

                                return 0;
                            });


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });

            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room

            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var exchangeRate = 0.0111M;

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockPropertyBaseCurrencyExchangeRate(exchangeRate);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(13).Date,
                            DateTo = DateTime.Now.AddDays(18).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            //Compare the reservations
            Assert.AreEqual(0, modifyResponse.Errors.Count, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
            Assert.AreNotEqual(exchangeRate, modifiedReservation.PropertyBaseCurrencyExchangeRate, "The field PropertyBaseCurrencyExchangeRate cannot be changed!");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        public void Test_ModificationOneRoom_ChangeExchangeRate()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new Operations.Helper.ReservationDataBuilder(1, 1806).WithRoom(1, 1);

            var transactionId = Guid.NewGuid().ToString();

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockObRatesRepo(builder);
            MockReservationRepo();
            #endregion

            #region Mock SQL Manager
            _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                            .Returns(5);


            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                            .Returns(new List<OB.BL.Contracts.Data.Properties.Inventory>
                            {
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                                new OB.BL.Contracts.Data.Properties.Inventory
                                {

                                },
                            });

            #endregion

            //Do the reservation
            var oldReservation = InsertReservation(builder, resBuilder, transactionId, true);

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
            var room = oldReservation.ReservationRooms.First();  //We will modify the first room

            room.AdultCount = 2;
            room.ChildCount = 2;
            room.DateFrom = DateTime.Now.AddDays(12).Date;
            room.DateTo = DateTime.Now.AddDays(17).Date;

            var childTerms = builder.InputData.ChildTerms;
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault(),
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = new List<int>() { 1, 1 },
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = resBuilder.InputData.reservationDetail.Property_UID,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault(),
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var exchangeRate = 0.0111M;

            #region Mock repos for price calculation and modify
            MockIncentives(builder);
            MockListRateRoomDetail(builder, room, parameters, 0, false);
            MockResRoomAppliedIncentives();
            MockResRoomAppliedPromotionalCodes();
            MockObRateToomDetailsForResRepo(1);
            MockPropertyBaseCurrencyExchangeRate(exchangeRate);
            MockIncentives(builder);  //re-call because the values can change in the modify
            #endregion

            //Mock Hangfire
            var client = new Mock<JobStorage>();
            JobStorage.Current = client.Object;
            UnitTestDetector.IsRunningInUnitTest = true;

            //Modify it
            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(13).Date,
                            DateTo = DateTime.Now.AddDays(18).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = RuleType.BE,
                ChannelId = 1,
                TransactionAction = ReservationTransactionAction.Initiate,
                TransactionType = ReservationTransactionType.A,
                TransactionId = transactionId
            }, ReservationAction.Modify);

            //Get the new reservation calling ListReservation of the ReservationManagerPOCO
            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((OB.Reservation.BL.Contracts.Responses.ModifyReservationResponse)modifyResponse).Reservation;

            //Compare the reservations
            Assert.AreEqual(0, modifyResponse.Errors.Count, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
            Assert.AreEqual((decimal?)exchangeRate, modifiedReservation.PropertyBaseCurrencyExchangeRate, $"PropertyBaseCurrencyExchangeRate needs to be equal to {exchangeRate}");
        }

        #endregion Test ExchangeRate Changes



        private void Send<T>(T msg)
        {

        }

        #region Assert Extensions
        private void AssertCalculateReservationRoomPrices(List<RateRoomDetailReservation> rrdList, DateTime dateFrom, decimal adultPrice, decimal childPrice,
            decimal priceAfterAddOn, decimal priceAfterBuyerGroups, decimal priceAfterPromoCodes, decimal priceAfterIncentives, decimal priceAfterRateModel,
            decimal finalPrice)
        {
            for (int i = 0; i < rrdList.Count; i++)
            {
                Assert.AreEqual(dateFrom.AddDays(i).Date, rrdList[i].Date.Date);
                Assert.AreEqual(childPrice, rrdList[i].ChildPrice);
                Assert.AreEqual(adultPrice, rrdList[i].AdultPrice);
                Assert.AreEqual(priceAfterAddOn, rrdList[i].PriceAfterAddOn);
                Assert.AreEqual(priceAfterBuyerGroups, rrdList[i].PriceAfterBuyerGroups);
                Assert.AreEqual(priceAfterPromoCodes, rrdList[i].PriceAfterPromoCodes);
                Assert.AreEqual(priceAfterIncentives, rrdList[i].PriceAfterIncentives);
                Assert.AreEqual(priceAfterRateModel, rrdList[i].PriceAfterRateModel);
                Assert.AreEqual(finalPrice, rrdList[i].FinalPrice);
            }
        }
        private void AssertCalculateReservationRoomPrices(List<RateRoomDetailReservation> rrdList, List<RateRoomDetailReservation> expectedRrdList)
        {
            for (int i = 0; i < rrdList.Count; i++)
            {
                Assert.IsTrue(rrdList[i].Date.Date == expectedRrdList[i].Date.Date);
                Assert.IsTrue(rrdList[i].ChildPrice == expectedRrdList[i].ChildPrice);
                Assert.IsTrue(rrdList[i].AdultPrice == expectedRrdList[i].AdultPrice);
                Assert.IsTrue(rrdList[i].PriceAfterAddOn == expectedRrdList[i].PriceAfterAddOn);
                Assert.IsTrue(rrdList[i].PriceAfterBuyerGroups == expectedRrdList[i].PriceAfterBuyerGroups);
                Assert.IsTrue(rrdList[i].PriceAfterPromoCodes == expectedRrdList[i].PriceAfterPromoCodes);
                Assert.IsTrue(rrdList[i].PriceAfterIncentives == expectedRrdList[i].PriceAfterIncentives);
                Assert.IsTrue(rrdList[i].PriceAfterRateModel == expectedRrdList[i].PriceAfterRateModel);
                Assert.IsTrue(rrdList[i].FinalPrice == expectedRrdList[i].FinalPrice);
            }
        }
        #endregion
    }
}