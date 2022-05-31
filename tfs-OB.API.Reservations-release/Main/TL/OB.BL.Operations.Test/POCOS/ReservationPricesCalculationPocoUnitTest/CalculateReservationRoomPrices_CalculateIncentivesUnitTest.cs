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
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Services.IntegrationTests.Helpers;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservation = OB.Domain.Reservations;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class CalculateReservationRoomPrices_CalculateIncentivesUnitTest : UnitBaseTest
    {
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

        private IReservationPricesCalculationPOCO _reservationPricesCalculationPoco;
        public IReservationPricesCalculationPOCO ReservationPricesCalculationPoco
        {
            get
            {
                if (_reservationPricesCalculationPoco == null)
                    _reservationPricesCalculationPoco = this.Container.Resolve<IReservationPricesCalculationPOCO>();

                return _reservationPricesCalculationPoco;
            }
            set { _reservationPricesCalculationPoco = value; }

        }

        //DB Mock
        List<Guest> guestsList = null;
        List<domainReservation.Reservation> reservationsList = null;
        List<Contracts.Data.General.Setting> settingsList = null;
        List<domainReservation.ReservationFilter> reservationFilterList = null;
        List<BL.Contracts.Data.Channels.ChannelsProperty> chPropsList = null;
        List<BL.Contracts.Data.Channels.ChannelLight> listChannelsLight = null;
        List<Currency> listCurrencies = null;
        List<domainReservation.VisualState> listVisualStates = null;
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


            //Initialize lists mock db
            reservationsList = new List<domainReservation.Reservation>();
            settingsList = new List<Contracts.Data.General.Setting>();
            reservationFilterList = new List<domainReservation.ReservationFilter>();
            guestsList = new List<Guest>();
            chPropsList = new List<Contracts.Data.Channels.ChannelsProperty>();
            listChannelsLight = new List<Contracts.Data.Channels.ChannelLight>();
            listCurrencies = new List<Currency>();
            listVisualStates = new List<domainReservation.VisualState>();            

            // POCO Mock
            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();
            this.ReservationPricesCalculationPoco = this.Container.Resolve<IReservationPricesCalculationPOCO>();
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
            FillCurrenciesMock();            

            //Mock Repos
            MockGroupRulesRepo();
            MockObCrmRepo();
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
                RatesAvailabilityType = new Dictionary<long, int>
                {
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
                .Returns((List<long> transactionUids) =>
                {
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

        #region Aux Methods
        private contractsReservations.Reservation InsertReservation(SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId = "", bool singleRoom = false)
        {
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

            //Mock
            MockReservationsRepo(searchBuilder, resBuilder, transactionId);
            MockReservationsRoomsRepo();
            MockReservationHelperPockMethods();
            MockResRoomDetailsRepo();
            MockResChildRepo(resBuilder);
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

            if (!addOneRoomOnly)
                resBuilder.WithCancelationPolicy(false, 0, true, 1, null, 1);

            return resBuilder;
        }
        
        private int GetAdultPriceTest(RateRoomDetail rrd, int numAds)
        {
            switch (numAds)
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
            switch (currencyUid)
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


        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]       
        public void Test_CalculationIncentive_StayWindowWeekDays_IgnoreDayOfWeekFromCheckIn()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays); //LastMinuteBooking

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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

            //act
            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);

            #region EXPECTED DATA
            var expectedRrdList = new List<RateRoomDetailReservation>();

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
                PriceAfterPromoCodes = 153,
                PriceAfterIncentives = 153,
                PriceAfterRateModel = 153,
                FinalPrice = 153
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 153,
                PriceAfterIncentives = 153,
                PriceAfterRateModel = 153,
                FinalPrice = 153
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 153,
                PriceAfterIncentives = 153,
                PriceAfterRateModel = 153,
                FinalPrice = 153
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 153,
                PriceAfterIncentives = 153,
                PriceAfterRateModel = 153,
                FinalPrice = 153
            });
            #endregion

            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_Cumulative_IgnoreDayOfWeekFromCheckIn()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 1, true, true,includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //EarlyBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 4, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //Discount
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //LastMinuteBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 5, true, true, minDays: 2, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays); //StayDiscount

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });

            #endregion


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_Cumulative_IgnoreTwoDaysOfWeek_BetweenCheckInAndCheckOut()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.AddDays(1).DayOfWeek] = false;
            stayWindowWeekDays[checkIn.AddDays(2).DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 1, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //EarlyBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 4, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //Discount
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //LastMinuteBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 5, true, true, minDays: 2, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays); //StayDiscount

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
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
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.54M,
                FinalPrice = 111.54M
            });

            #endregion


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_WithFreeNights_IgnoreDayOfWeekFromCheckIn()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 10, 1, 3, false, false, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays); //FreeNights

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);

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

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_WithFreeNightsAtBegin_IgnoreTwoDaysOfWeek_BetweenCheckInAndCheckOut()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.AddDays(1).DayOfWeek] = false;
            stayWindowWeekDays[checkIn.AddDays(2).DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 10, 2, 3, true, false, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays); //FreeNights isFreeDaysAtBegin 2 days

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);

            var expectedRrdList = new List<RateRoomDetailReservation>();

            #region EXPECTED DATA

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
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });

            #endregion

            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_DaysOfWeekNotDefined()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false); //LastMinuteBooking

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            builder.InputData.Incentives.ForEach(x => x.StayWindowWeekDays = null);

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

            //act
            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_StayWindowWeekDays_IgnoreStayWindowWeekDays_GroupRuleBE()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 1, true, true, stayWindowWeekDays: stayWindowWeekDays) //EarlyBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 4, true, true, stayWindowWeekDays: stayWindowWeekDays) //Discount
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true, stayWindowWeekDays: stayWindowWeekDays) //LastMinuteBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 5, true, true, minDays: 2, stayWindowWeekDays: stayWindowWeekDays); //StayDiscount

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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
            var groupRule = groupRepo.GetGroupRule(new DL.Common.Criteria.GetGroupRuleCriteria(OB.Domain.Reservations.RuleType.BE));
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
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.5370M,
                FinalPrice = 111.5370M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(1),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.5370M,
                FinalPrice = 111.5370M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.5370M,
                FinalPrice = 111.5370M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.5370M,
                FinalPrice = 111.5370M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 111.5370M,
                PriceAfterIncentives = 111.5370M,
                PriceAfterRateModel = 111.5370M,
                FinalPrice = 111.5370M
            });

            #endregion


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }


        [TestMethod]
        [TestCategory("CalculateIncentives")]
        public void Test_CalculationIncentive_WithoutIncentiveType_NoApplyDiscount()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 0, true, false); //No type

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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

            //act
            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("CalculateIncentives_StayWindowWeekDays")]
        public void Test_CalculationIncentive_ApplyIncentivesWithValidPeriodsBwAndSw()
        {
            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var checkIn = DateTime.Now;
            var checkOut = DateTime.Now.AddDays(5);
            var stayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                            {DayOfWeek.Monday, true },
                            {DayOfWeek.Tuesday, true },
                            {DayOfWeek.Wednesday, true },
                            {DayOfWeek.Thursday, true },
                            {DayOfWeek.Friday, true },
                            {DayOfWeek.Saturday, true },
                            {DayOfWeek.Sunday, true }
                };
            stayWindowWeekDays[checkIn.AddDays(1).DayOfWeek] = false;
            stayWindowWeekDays[checkIn.AddDays(2).DayOfWeek] = false;

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = checkIn,
                            CheckOut = checkOut
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 1, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //EarlyBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 4, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //Discount
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true, includeIncentiveWithBwToSimulateSpResults: true, stayWindowWeekDays: stayWindowWeekDays) //LastMinuteBooking
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 5, true, true, minDays: 2, stayWindowWeekDays: stayWindowWeekDays); //StayDiscount

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

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
                PriceAfterPromoCodes = 123.930M,
                PriceAfterIncentives = 123.930M,
                PriceAfterRateModel = 123.93M,
                FinalPrice = 123.93M
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
                PriceAfterPromoCodes = 123.930M,
                PriceAfterIncentives = 123.930M,
                PriceAfterRateModel = 123.93M,
                FinalPrice = 123.93M
            });
            expectedRrdList.Add(new RateRoomDetailReservation
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 123.930M,
                PriceAfterIncentives = 123.930M,
                PriceAfterRateModel = 123.93M,
                FinalPrice = 123.93M
            });

            #endregion


            var rrdList = ReservationPricesCalculationPoco.CalculateReservationRoomPrices(parameters);
            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(builder.InputData.Incentives.GroupBy(x => x.UID).Count() == 4, "Expected 3 incentives");
            Assert.IsTrue(rrdList.First().AppliedIncentives.Count() == 3, "Expected 3 applied incentives");
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #region Assert Extensions
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
        #endregion
    }
}