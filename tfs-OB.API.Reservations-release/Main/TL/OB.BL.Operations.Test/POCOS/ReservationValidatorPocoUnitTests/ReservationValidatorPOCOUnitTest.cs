using Dapper;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.Channels;
using OB.BL.Contracts.Data.General;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Requests;
using OB.Services.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Test.ReservationValidator
{
    [TestClass]
    public class ReservationValidatorPOCOUnitTest : UnitBaseTest
    {

        public IReservationValidatorPOCO ReservationValidatorPOCO { get; set; }

        public IReservationManagerPOCO ReservationManagerPOCO { get; set; }

        #region Variables

        private List<GuaranteeType> _guaranteeTypeInDataBase = null;
        private List<Setting> _appSettingInDataBase = null;
        private List<DepositPoliciesGuaranteeType> _depositPoliciesGuaranteeTypeInDataBase = null;
        private List<contractsReservation.Reservation> _reservationsInDataBase = null;
        private List<OB.BL.Contracts.Data.CRM.Guest> _guestInDataBase = null;
        private List<RoomType> _roomTypeInDataBase = null;
        private List<PropertyLight> _propertyLightInDataBase = null;
        private List<OB.BL.Contracts.Data.Rates.Rate> _ratesInDataBase = null;
        private List<OB.Domain.Reservations.ReservationsAdditionalData> _reservationAdditionalData = null;
        private List<RateRoom> _rateRoomsInDataBase = null;
        private List<ChildTerm> _childTermsInDataBase = null;
        private List<Incentive> _incentivesInDataBase = null;
        private List<TaxPolicy> _taxPolicyInDataBase = null;
        private List<RateRoomDetailReservation> _rateRoomDetailResInDataBase = null;
        private List<PromotionalCode> _promotionalCodesInDataBase = null;
        private List<RateBuyerGroup> _rateBuyerGroupInDataBase = null;
        private List<DepositPolicy> _depositPoliciesInDataBase = null;
        private List<CancellationPolicy> _cancellationPolicyInDataBase = null;
        private List<OtherPolicy> _otherPoliciesInDataBase = null;
        private List<Extra> _extrasInDataBase = null;
        private List<PaymentMethodType> _paymentMethodTypesInDataBase = null;
        private List<ExtrasBillingType> _extrasBillingTypeInDataBase = null;
        private List<GroupCode> _groupCodesInDataBase = null;
        private List<ChannelLight> _channelLightInDataBase = null;
        private List<Currency> _currenciesInDataBase = null;
        private List<domainReservations.VisualState> _visualStateInDataBase = null;

        private Mock<IOBAppSettingRepository> _appSettingRepoMock = null;
        private Mock<IOBDepositPolicyRepository> _depositPoliciesGuaranteeTypeRepoMock = null;
        private Mock<IOBSecurityRepository> _securityRepoMock = null;
        private Mock<IGroupRulesRepository> _groupRulesRepoMock = null;
        private Mock<IOBPropertyRepository> _propertyRepoMock = null;
        private Mock<IReservationsRepository> _reservationRepoMock = null;
        private Mock<IOBCRMRepository> _crmRepoMock = null;
        private Mock<IOBRateRepository> _rateRepoMock = null;
        private Mock<IOBChildTermsRepository> _childTermsRepoMock = null;
        private Mock<IRepository<domainReservations.ReservationsAdditionalData>> _reservationAdditionalRepoMock = null;
        private Mock<ISqlManager> _sqlManagerRepoMock = null;
        private Mock<IOBIncentiveRepository> _incentivesRepoMock = null;
        private Mock<IOBOtherPolicyRepository> _othersRepoMock = null;
        private Mock<IOBRateRoomDetailsForReservationRoomRepository> _rateRoomDetailsForReservationRoomRepoMock = null;
        private Mock<IOBPromotionalCodeRepository> _promotionalCodeRepoMock = null;
        private Mock<IOBRateBuyerGroupRepository> _rateBuyerGroupsRepoMock = null;
        private Mock<IOBCancellationPolicyRepository> _cancelationPolicyRepoMock = null;
        private Mock<IOBExtrasRepository> _extrasRepoMock = null;
        private Mock<IOBPaymentMethodTypeRepository> _paymentMethodTypeRepoMock = null;
        private Mock<IOBChannelRepository> _channelsRepoMock = null;
        private Mock<IReservationRoomDetailRepository> _reservationRoomDetailRepoMock = null;
        private Mock<IRepository<domainReservations.ReservationRoomDetailsAppliedIncentive>> _reservationDetailsAppliedIncentiveRepoMock = null;
        private Mock<IRepository<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>> _resRoomAppliedPromotionalCodeRepoMock = null;
        private Mock<IRepository<domainReservations.ReservationRoomTaxPolicy>> _reservationRoomTaxPolicyRepoMock = null;
        private Mock<IOBCurrencyRepository> _currencyRepoMock = null;
        private Mock<IReservationsFilterRepository> _reservationFilterRepoMock = null;
        private Mock<IVisualStateRepository> _visualStateRepoMock = null;
        private Mock<IRepository<domainReservations.ReservationRoomChild>> _reservationRoomChildRepoMock = null;
        private Mock<IOBReservationLookupsRepository> _obReservationsLookupsRepoMock = null;
        private Mock<IOBPropertyEventsRepository> _obPropertyEventsRepository = null;
        private Mock<IRepository<domainReservations.ReservationStatusLanguage>> _resStatusLangRepoMock = null;
        private Mock<IReservationHistoryRepository> _reservationhistoryRepoMock = null;


        //GetRepository<PropertyQueue>
        private Mock<IReservationManagerPOCO> _reservationManagerPOCOMock = null;
        private IReservationManagerPOCO _ReservationManagerPOCOMock;

        #endregion

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            #region initialize data bases mock

            _guaranteeTypeInDataBase = new List<GuaranteeType>();
            _depositPoliciesGuaranteeTypeInDataBase = new List<DepositPoliciesGuaranteeType>();
            _appSettingInDataBase = new List<Setting>();
            _reservationsInDataBase = new List<contractsReservation.Reservation>();
            _guestInDataBase = new List<OB.BL.Contracts.Data.CRM.Guest>();
            _roomTypeInDataBase = new List<RoomType>();
            _ratesInDataBase = new List<Rate>();
            _childTermsInDataBase = new List<ChildTerm>();
            _propertyLightInDataBase = new List<PropertyLight>();
            _reservationAdditionalData = new List<OB.Domain.Reservations.ReservationsAdditionalData>();
            _rateRoomsInDataBase = new List<RateRoom>();
            _incentivesInDataBase = new List<Incentive>();
            _taxPolicyInDataBase = new List<TaxPolicy>();
            _rateRoomDetailResInDataBase = new List<RateRoomDetailReservation>();
            _promotionalCodesInDataBase = new List<PromotionalCode>();
            _rateBuyerGroupInDataBase = new List<RateBuyerGroup>();
            _depositPoliciesInDataBase = new List<DepositPolicy>();
            _cancellationPolicyInDataBase = new List<CancellationPolicy>();
            _otherPoliciesInDataBase = new List<OtherPolicy>();
            _extrasInDataBase = new List<Extra>();
            _paymentMethodTypesInDataBase = new List<PaymentMethodType>();
            _extrasBillingTypeInDataBase = new List<ExtrasBillingType>();
            _groupCodesInDataBase = new List<GroupCode>();
            _channelLightInDataBase = new List<ChannelLight>();
            _currenciesInDataBase = new List<Currency>();
            _visualStateInDataBase = new List<OB.Domain.Reservations.VisualState>();

            FillPropertyLightForTest();
            FillChildTermsForTest();
            FillTheReservationAdditionalDataforTest();
            FillTheRoomTypesForTest();
            FillRatesForTest();
            FillTheGuestforTest();
            FillTheReservationsforTest();
            FillIncentivesForTest();
            FillTaxPoliciesForTest();
            FillRateRoomDetailsForTest();
            FillPromotionalCodesForTest();
            FillRateBuyerGroupForTest();
            FillDepositPoliciesForTest();
            FillCancellationPoliciesForTest();
            FillOtherPoliciesForTest();
            FillExtrasForTest();
            FillPaymentMethodTypesForTest();
            FillExtrasBillingTypesForTest();
            FillGroupCodesForTest();
            FillChannelLightForTest();
            FillCurrenciesForTest();
            FillVisualStateForTest();
            #endregion

            //Mock Repository factory to return mocked RateRepository
            InitializeMocks(RepositoryFactoryMock);
            InitializeLookupsMock();

            SessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(UnitOfWorkMock.Object);
            SessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>(), It.IsAny<DomainScope>())).Returns(UnitOfWorkMock.Object);

            UnitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<IsolationLevel>()))
                .Returns(new Mock<IDbTransaction>(MockBehavior.Default).Object);

            #region Container
            this.Container = this.Container.RegisterInstance<IOBAppSettingRepository>(_appSettingRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBDepositPolicyRepository>(_depositPoliciesGuaranteeTypeRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBSecurityRepository>(_securityRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IGroupRulesRepository>(_groupRulesRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBCRMRepository>(_crmRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBChildTermsRepository>(_childTermsRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBRateRepository>(_rateRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBPropertyRepository>(_propertyRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IReservationsRepository>(_reservationRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<ISqlManager>(_sqlManagerRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBIncentiveRepository>(_incentivesRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBOtherPolicyRepository>(_othersRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBRateRoomDetailsForReservationRoomRepository>(_rateRoomDetailsForReservationRoomRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBPromotionalCodeRepository>(_promotionalCodeRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBRateBuyerGroupRepository>(_rateBuyerGroupsRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBCancellationPolicyRepository>(_cancelationPolicyRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBExtrasRepository>(_extrasRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBPaymentMethodTypeRepository>(_paymentMethodTypeRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBChannelRepository>(_channelsRepoMock.Object, new TransientLifetimeManager());
            #endregion Container

            this.ReservationValidatorPOCO = this.Container.Resolve<IReservationValidatorPOCO>();
            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();
        }

        private void InitializeMocks(Mock<IRepositoryFactory> repoFactoryMock)
        {
            //Mock DepositPoliciesGuaranteeTypeRepository to return list of DepositPoliciesGuaranteeType instead of macking queries to the databse.
            _depositPoliciesGuaranteeTypeRepoMock = new Mock<IOBDepositPolicyRepository>(MockBehavior.Default);
            //Mock AppSettingRepository to return list of AppSetting instead of macking queries to the databse.
            _appSettingRepoMock = new Mock<IOBAppSettingRepository>(MockBehavior.Default);
            _securityRepoMock = new Mock<IOBSecurityRepository>(MockBehavior.Default);
            _groupRulesRepoMock = new Mock<IGroupRulesRepository>(MockBehavior.Default);
            _propertyRepoMock = new Mock<IOBPropertyRepository>(MockBehavior.Default);
            _crmRepoMock = new Mock<IOBCRMRepository>(MockBehavior.Default);
            _rateRepoMock = new Mock<IOBRateRepository>(MockBehavior.Default);
            _reservationRepoMock = new Mock<IReservationsRepository>(MockBehavior.Default);
            _childTermsRepoMock = new Mock<IOBChildTermsRepository>(MockBehavior.Default);
            _reservationAdditionalRepoMock = new Mock<IRepository<domainReservations.ReservationsAdditionalData>>(MockBehavior.Default);
            _sqlManagerRepoMock = new Mock<ISqlManager>(MockBehavior.Default);
            _incentivesRepoMock = new Mock<IOBIncentiveRepository>(MockBehavior.Default);
            _othersRepoMock = new Mock<IOBOtherPolicyRepository>(MockBehavior.Default);
            _rateRoomDetailsForReservationRoomRepoMock = new Mock<IOBRateRoomDetailsForReservationRoomRepository>(MockBehavior.Default);
            _promotionalCodeRepoMock = new Mock<IOBPromotionalCodeRepository>(MockBehavior.Default);
            _rateBuyerGroupsRepoMock = new Mock<IOBRateBuyerGroupRepository>(MockBehavior.Default);
            _cancelationPolicyRepoMock = new Mock<IOBCancellationPolicyRepository>(MockBehavior.Default);
            _extrasRepoMock = new Mock<IOBExtrasRepository>(MockBehavior.Default);
            _paymentMethodTypeRepoMock = new Mock<IOBPaymentMethodTypeRepository>(MockBehavior.Default);
            _channelsRepoMock = new Mock<IOBChannelRepository>(MockBehavior.Default);
            _reservationRoomDetailRepoMock = new Mock<IReservationRoomDetailRepository>(MockBehavior.Default);
            _reservationDetailsAppliedIncentiveRepoMock = new Mock<IRepository<domainReservations.ReservationRoomDetailsAppliedIncentive>>(MockBehavior.Default);
            _resRoomAppliedPromotionalCodeRepoMock = new Mock<IRepository<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>>(MockBehavior.Default);
            _reservationRoomTaxPolicyRepoMock = new Mock<IRepository<domainReservations.ReservationRoomTaxPolicy>>(MockBehavior.Default);
            _reservationRoomChildRepoMock = new Mock<IRepository<domainReservations.ReservationRoomChild>>(MockBehavior.Default);
            _reservationManagerPOCOMock = new Mock<IReservationManagerPOCO>(MockBehavior.Default);
            _currencyRepoMock = new Mock<IOBCurrencyRepository>(MockBehavior.Default);
            _reservationFilterRepoMock = new Mock<IReservationsFilterRepository>(MockBehavior.Default);
            _visualStateRepoMock = new Mock<IVisualStateRepository>(MockBehavior.Default);
            _obReservationsLookupsRepoMock = new Mock<IOBReservationLookupsRepository>(MockBehavior.Default);
            _obPropertyEventsRepository = new Mock<IOBPropertyEventsRepository>(MockBehavior.Default);
            _resStatusLangRepoMock = new Mock<IRepository<domainReservations.ReservationStatusLanguage>>(MockBehavior.Default);
            _reservationhistoryRepoMock = new Mock<IReservationHistoryRepository>(MockBehavior.Default);


            #region Default Mock Setups
            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
                .Returns(new List<TripAdvisorConfiguration>
                {
                });
            #endregion

            //Mock AppSettingRepository to return list of AppSettig instead of macking queries to the databse.
            repoFactoryMock.Setup(x => x.GetOBAppSettingRepository())
                .Returns(_appSettingRepoMock.Object);

            //Mock DepositPoliciesGuaranteeTypeRepository to return list of DepositPoliciesGuaranteeType instead of macking queries to the databse.
            repoFactoryMock.Setup(x => x.GetOBDepositPolicyRepository())
                .Returns(_depositPoliciesGuaranteeTypeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetGroupRulesRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_groupRulesRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBCRMRepository())
                .Returns(_crmRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBPropertyRepository())
                .Returns(_propertyRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBRateRepository())
                .Returns(_rateRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBChildTermsRepository())
                .Returns(_childTermsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationsAdditionalData>(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationAdditionalRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationRoomChild>(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationRoomChildRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBSecurityRepository())
                .Returns(_securityRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBIncentiveRepository())
                .Returns(_incentivesRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBOtherPolicyRepository())
                .Returns(_othersRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBRateRoomDetailsForReservationRoomRepository())
                .Returns(_rateRoomDetailsForReservationRoomRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBPromotionalCodeRepository())
                .Returns(_promotionalCodeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBRateBuyerGroupRepository())
                .Returns(_rateBuyerGroupsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBCancellationPolicyRepository())
                .Returns(_cancelationPolicyRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBExtrasRepository())
                .Returns(_extrasRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBPaymentMethodTypeRepository())
                .Returns(_paymentMethodTypeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBChannelRepository())
                .Returns(_channelsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetReservationRoomDetailRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationRoomDetailRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBCurrencyRepository())
                .Returns(_currencyRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationRoomDetailsAppliedIncentive>(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationDetailsAppliedIncentiveRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>(It.IsAny<IUnitOfWork>()))
                .Returns(_resRoomAppliedPromotionalCodeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationRoomTaxPolicy>(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationRoomTaxPolicyRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), domainReservations.Reservation.DomainScope))
                .Returns(_sqlManagerRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<string>()))
                .Returns(_sqlManagerRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetReservationsFilterRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationFilterRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetVisualStateRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_visualStateRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBReservationLookupsRepository())
                .Returns(_obReservationsLookupsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBPropertyEventsRepository())
                .Returns(_obPropertyEventsRepository.Object);

            repoFactoryMock.Setup(x => x.GetRepository<domainReservations.ReservationStatusLanguage>(It.IsAny<IUnitOfWork>()))
                .Returns(_resStatusLangRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetReservationHistoryRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_reservationhistoryRepoMock.Object);

            _ReservationManagerPOCOMock = Container.Resolve<IReservationManagerPOCO>();
        }

        private void InitializeLookupsMock(IList<Rate> rates = null)
        {
            _obReservationsLookupsRepoMock.Setup(x => x.ListReservationLookups(It.IsAny<ListReservationLookupsRequest>()))
                .Returns((ListReservationLookupsRequest request) =>
                {
                    return new Contracts.Data.Reservations.ReservationLookups
                    {
                        RatesLookup = (rates ?? _ratesInDataBase).ToDictionary(x => x.UID),
                    };
                });

            _rateRepoMock.Setup(x => x.ListRatesAvailablityType(It.IsAny<ListRateAvailabilityTypeRequest>()))
                .Returns((ListRateAvailabilityTypeRequest request) =>
                {
                    return (rates ?? _ratesInDataBase).ToDictionary(x => x.UID, y=> y.AvailabilityType);
                });
        }

        #region Fill Testing Data

        private void FillTheReservationAdditionalDataforTest()
        {
            _reservationAdditionalData.Add(
                new OB.Domain.Reservations.ReservationsAdditionalData
                {
                    UID = 1,
                    Reservation_UID = 76520,
                    IsFromNewInsert = false,
                    ChannelPartnerID = 2332,
                    PartnerReservationNumber = "343243242",
                    ReservationAdditionalDataJSON =
                        "{'CancellationPolicy':{'Days':0,'Description':'','IsCancellationAllowed':true,'Name':'','PaymentModel':1,'UID':2022},'ReservationRoomNo':'RES000024 - 1806 / 1','ReservationRoom_UID':84029}"
                }
            );
        }

        private void FillTheReservationsforTest()
        {
            #region reservation with one room
            _reservationsInDataBase.Add(
                new contractsReservation.Reservation
                {
                    Adults = 1,
                    BillingCity = "asdasdasd",
                    BillingContactName = "Test1 Teste2",
                    BillingCountry_UID = 157,
                    BillingEmail = "tony.santos@visualforma.pt",
                    BillingPhone = "123123",
                    BillingState_UID = 2430214,
                    CancellationPolicy = "Teste",
                    CancellationPolicyDays = 2,
                    Channel_UID = 1,
                    Children = 1,
                    CreatedDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Guest = new Guest
                    {
                        AllowMarketing = true,
                        City = "asdasdasd",
                        Client_UID = 1011,
                        Country_UID = 157,
                        CreateDate = DateTime.UtcNow,
                        Currency_UID = 34,
                        Email = "tony.santos@visualforma.pt",
                        FirstName = "Test1",
                        Gender = "M",
                        GuestCategory_UID = 1,
                        IDCardNumber = "12312312",
                        Index = 0,
                        IsActive = true,
                        IsImportedFromExcel = false,
                        Language_UID = 4,
                        LastName = "Teste2",
                        Phone = "123123123",
                        Prefix = 1,
                        Property_UID = 1806,
                        State_UID = 2430214,
                        UID = 284630,
                        UseDifferentBillingInfo = false,
                        UserName = "tony.santos@visualforma.pt",
                        UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                    },
                    GuestCity = "asdasdasd",
                    GuestCountry_UID = 157,
                    GuestEmail = "tony.santos@visualforma.pt",
                    GuestFirstName = "Test1",
                    GuestIDCardNumber = "123123123",
                    GuestLastName = "Teste2",
                    GuestPhone = "123123",
                    GuestState_UID = 2430214,
                    Guest_UID = 284630,
                    IPAddress = "::1",
                    InternalNotesHistory = DateTime.UtcNow.ToString(),
                    Number = "RES000024-1806",
                    PropertyBaseCurrencyExchangeRate = 1,
                    Property_UID = 1806,
                    ReservationAdditionalData = new contractsReservation.ReservationsAdditionalData
                    {
                        ReservationRoomList = new List<contractsReservation.ReservationRoomAdditionalData>
                        {
                            new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                }
                            }
                        }
                    },
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationCurrencyExchangeRateDate = DateTime.UtcNow,
                    ReservationCurrency_UID = 34,
                    ReservationLanguageUsed_UID = 4,
                    ReservationPartialPaymentDetails = new List<contractsReservation.ReservationPartialPaymentDetail>(),
                    ReservationRooms = new List<contractsReservation.ReservationRoom>
                    {
                        new Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                        {
                            AdultCount = 1,
                            CancellationDate = DateTime.UtcNow,
                            CancellationPaymentModel = 1,
                            CancellationPolicyDays = 0,
                            ChildCount = 1,
                            CreatedDate = DateTime.UtcNow,
                            DateFrom = DateTime.UtcNow.AddDays(1),
                            DateTo = DateTime.UtcNow.AddDays(5),
                            GuestName = "Test1 Teste2",
                            IsCancellationAllowed = true,
                            Package_UID = 0,
                            Rate = new Reservation.BL.Contracts.Data.Rates.Rate
                            {
                                Currency_UID = 34,
                                Description = null,
                                Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                                Property_UID = 1806,
                                UID = 22645
                            },
                            Rate_UID = 22645,
                            ReservationRoomAdditionalData = new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                },
                                ReservationRoomNo = "RES000024-1806/1",
                                ReservationRoom_UID = 84029
                            },
                            ReservationRoomChilds = new List<contractsReservation.ReservationRoomChild>
                            {
                                new contractsReservation.ReservationRoomChild
                                {
                                    Age = null,
                                    ChildNo = 1,
                                    ReservationRoom_UID = 84029,
                                    UID = 5334
                                }
                            },
                            ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail>
                            {
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203628
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203629
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203630
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203631
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203632
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203633
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203634
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203635
                                }
                            },
                            ReservationRoomNo = "RES000024-1806/1",
                            ReservationRoomsExtrasSum = 0,
                            ReservationRoomsPriceSum = 110,
                            ReservationRoomsTotalAmount = 110,
                            Reservation_UID = 76520,
                            RoomType = new Reservation.BL.Contracts.Data.Properties.RoomType
                            {
                                AcceptsChildren = true,
                                AcceptsExtraBed = false,
                                AdultMaxOccupancy = 3,
                                AdultMinOccupancy = 1,
                                ChildMaxOccupancy = 1,
                                ChildMinOccupancy = 0,
                                CreatedDate = DateTime.UtcNow,
                                MaxOccupancy = 4,
                                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                                Property_UID = 1806,
                                Qty = 100,
                                UID = 5901
                            },
                            RoomType_UID = 5901,
                            Status = 1,
                            TotalTax = 0,
                            UID = 84029
                        }
                    },
                    RoomsExtras = 0,
                    RoomsPriceSum = 110,
                    RoomsTax = 0,
                    RoomsTotalAmount = 110,
                    Status = 1,
                    Tax = 0,
                    TotalAmount = 110,
                    TotalTax = 0,
                    TransactionId = Guid.NewGuid().ToString(),
                    UID = 76520
                });
            #endregion

            #region reservation with two rooms
            _reservationsInDataBase.Add(
                new contractsReservation.Reservation
                {
                    Adults = 1,
                    BillingCity = "asdasdasd",
                    BillingContactName = "Test1 Teste2",
                    BillingCountry_UID = 157,
                    BillingEmail = "tony.santos@visualforma.pt",
                    BillingPhone = "123123",
                    BillingState_UID = 2430214,
                    CancellationPolicy = "Teste",
                    CancellationPolicyDays = 2,
                    Channel_UID = 1,
                    Children = 1,
                    CreatedDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Guest = new Guest
                    {
                        AllowMarketing = true,
                        City = "asdasdasd",
                        Client_UID = 1011,
                        Country_UID = 157,
                        CreateDate = DateTime.UtcNow,
                        Currency_UID = 34,
                        Email = "tony.santos@visualforma.pt",
                        FirstName = "Test1",
                        Gender = "M",
                        GuestCategory_UID = 1,
                        IDCardNumber = "12312312",
                        Index = 0,
                        IsActive = true,
                        IsImportedFromExcel = false,
                        Language_UID = 4,
                        LastName = "Teste2",
                        Phone = "123123123",
                        Prefix = 1,
                        Property_UID = 1806,
                        State_UID = 2430214,
                        UID = 284630,
                        UseDifferentBillingInfo = false,
                        UserName = "tony.santos@visualforma.pt",
                        UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                    },
                    GuestCity = "asdasdasd",
                    GuestCountry_UID = 157,
                    GuestEmail = "tony.santos@visualforma.pt",
                    GuestFirstName = "Test1",
                    GuestIDCardNumber = "123123123",
                    GuestLastName = "Teste2",
                    GuestPhone = "123123",
                    GuestState_UID = 2430214,
                    Guest_UID = 284630,
                    IPAddress = "::1",
                    InternalNotesHistory = DateTime.UtcNow.ToString(),
                    Number = "RES000024-1806",
                    PropertyBaseCurrencyExchangeRate = 1,
                    Property_UID = 1806,
                    ReservationAdditionalData = new contractsReservation.ReservationsAdditionalData
                    {
                        ReservationRoomList = new List<contractsReservation.ReservationRoomAdditionalData>
                        {
                            new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                }
                            }
                        }
                    },
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationCurrencyExchangeRateDate = DateTime.UtcNow,
                    ReservationCurrency_UID = 34,
                    ReservationLanguageUsed_UID = 4,
                    ReservationPartialPaymentDetails = new List<contractsReservation.ReservationPartialPaymentDetail>(),
                    ReservationRooms = new List<contractsReservation.ReservationRoom>
                    {
                        new Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                        {
                            AdultCount = 1,
                            CancellationDate = DateTime.UtcNow,
                            CancellationPaymentModel = 1,
                            CancellationPolicyDays = 0,
                            ChildCount = 1,
                            CreatedDate = DateTime.UtcNow,
                            DateFrom = DateTime.UtcNow.AddDays(1),
                            DateTo = DateTime.UtcNow.AddDays(5),
                            GuestName = "Test1 Teste2",
                            IsCancellationAllowed = true,
                            Package_UID = 0,
                            Rate = new Reservation.BL.Contracts.Data.Rates.Rate
                            {
                                Currency_UID = 34,
                                Description = null,
                                Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                                Property_UID = 1806,
                                UID = 22645
                            },
                            Rate_UID = 22645,
                            ReservationRoomAdditionalData = new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                },
                                ReservationRoomNo = "RES000024-1806/1",
                                ReservationRoom_UID = 84029
                            },
                            ReservationRoomChilds = new List<contractsReservation.ReservationRoomChild>
                            {
                                new contractsReservation.ReservationRoomChild
                                {
                                    Age = null,
                                    ChildNo = 1,
                                    ReservationRoom_UID = 84029,
                                    UID = 5334
                                }
                            },
                            ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail>
                            {
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203628
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203629
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203630
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203631
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203632
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203633
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203634
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203635
                                }
                            },
                            ReservationRoomNo = "RES000024-1806/1",
                            ReservationRoomsExtrasSum = 0,
                            ReservationRoomsPriceSum = 110,
                            ReservationRoomsTotalAmount = 110,
                            Reservation_UID = 76524,
                            RoomType = new Reservation.BL.Contracts.Data.Properties.RoomType
                            {
                                AcceptsChildren = true,
                                AcceptsExtraBed = false,
                                AdultMaxOccupancy = 3,
                                AdultMinOccupancy = 1,
                                ChildMaxOccupancy = 1,
                                ChildMinOccupancy = 0,
                                CreatedDate = DateTime.UtcNow,
                                MaxOccupancy = 4,
                                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                                Property_UID = 1806,
                                Qty = 100,
                                UID = 5901
                            },
                            RoomType_UID = 5901,
                            Status = 1,
                            TotalTax = 0,
                            UID = 84029
                        },
                          new Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                        {
                            AdultCount = 1,
                            CancellationDate = DateTime.UtcNow,
                            CancellationPaymentModel = 1,
                            CancellationPolicyDays = 0,
                            ChildCount = 1,
                            CreatedDate = DateTime.UtcNow,
                            DateFrom = DateTime.UtcNow.AddDays(1),
                            DateTo = DateTime.UtcNow.AddDays(5),
                            GuestName = "Test1 Teste2",
                            IsCancellationAllowed = true,
                            Package_UID = 0,
                            Rate = new Reservation.BL.Contracts.Data.Rates.Rate
                            {
                                Currency_UID = 34,
                                Description = null,
                                Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                                Property_UID = 1806,
                                UID = 22645
                            },
                            Rate_UID = 22645,
                            ReservationRoomAdditionalData = new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                },
                                ReservationRoomNo = "RES000024-1806/2",
                                ReservationRoom_UID = 84030
                            },
                            ReservationRoomChilds = new List<contractsReservation.ReservationRoomChild>
                            {
                                new contractsReservation.ReservationRoomChild
                                {
                                    Age = null,
                                    ChildNo = 1,
                                    ReservationRoom_UID = 84030,
                                    UID = 5334
                                }
                            },
                            ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail>
                            {
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203628
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203629
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203630
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203631
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203632
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203633
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203634
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84030,
                                    UID = 203635
                                }
                            },
                            ReservationRoomNo = "RES000024-1806/2",
                            ReservationRoomsExtrasSum = 0,
                            ReservationRoomsPriceSum = 110,
                            ReservationRoomsTotalAmount = 110,
                            Reservation_UID = 76524,
                            RoomType = new Reservation.BL.Contracts.Data.Properties.RoomType
                            {
                                AcceptsChildren = true,
                                AcceptsExtraBed = false,
                                AdultMaxOccupancy = 3,
                                AdultMinOccupancy = 1,
                                ChildMaxOccupancy = 1,
                                ChildMinOccupancy = 0,
                                CreatedDate = DateTime.UtcNow,
                                MaxOccupancy = 4,
                                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                                Property_UID = 1806,
                                Qty = 100,
                                UID = 5901
                            },
                            RoomType_UID = 5901,
                            Status = 1,
                            TotalTax = 0,
                            UID = 84030
                        }
                    },
                    RoomsExtras = 0,
                    RoomsPriceSum = 110,
                    RoomsTax = 0,
                    RoomsTotalAmount = 110,
                    Status = 1,
                    Tax = 0,
                    TotalAmount = 110,
                    TotalTax = 0,
                    TransactionId = Guid.NewGuid().ToString(),
                    UID = 76524
                });
            #endregion


            #region reservation with cancelation policy
            _reservationsInDataBase.Add(
                new contractsReservation.Reservation
                {
                    Adults = 1,
                    BillingCity = "asdasdasd",
                    BillingContactName = "Test1 Teste2",
                    BillingCountry_UID = 157,
                    BillingEmail = "tony.santos@visualforma.pt",
                    BillingPhone = "123123",
                    BillingState_UID = 2430214,
                    CancellationPolicy = "Teste",
                    CancellationPolicyDays = 2,
                    Channel_UID = 1,
                    Children = 1,
                    CreatedDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Guest = new Guest
                    {
                        AllowMarketing = true,
                        City = "asdasdasd",
                        Client_UID = 1011,
                        Country_UID = 157,
                        CreateDate = DateTime.UtcNow,
                        Currency_UID = 34,
                        Email = "tony.santos@visualforma.pt",
                        FirstName = "Test1",
                        Gender = "M",
                        GuestCategory_UID = 1,
                        IDCardNumber = "12312312",
                        Index = 0,
                        IsActive = true,
                        IsImportedFromExcel = false,
                        Language_UID = 4,
                        LastName = "Teste2",
                        Phone = "123123123",
                        Prefix = 1,
                        Property_UID = 1806,
                        State_UID = 2430214,
                        UID = 284630,
                        UseDifferentBillingInfo = false,
                        UserName = "tony.santos@visualforma.pt",
                        UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                    },
                    GuestCity = "asdasdasd",
                    GuestCountry_UID = 157,
                    GuestEmail = "tony.santos@visualforma.pt",
                    GuestFirstName = "Test1",
                    GuestIDCardNumber = "123123123",
                    GuestLastName = "Teste2",
                    GuestPhone = "123123",
                    GuestState_UID = 2430214,
                    Guest_UID = 284630,
                    IPAddress = "::1",
                    InternalNotesHistory = DateTime.UtcNow.ToString(),
                    Number = "RES000024-1806",
                    PropertyBaseCurrencyExchangeRate = 1,
                    Property_UID = 1806,
                    ReservationAdditionalData = new contractsReservation.ReservationsAdditionalData
                    {
                        ReservationRoomList = new List<contractsReservation.ReservationRoomAdditionalData>
                        {
                            new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                }
                            }
                        }
                    },
                    ReservationBaseCurrency_UID = 34,
                    ReservationCurrencyExchangeRate = 1,
                    ReservationCurrencyExchangeRateDate = DateTime.UtcNow,
                    ReservationCurrency_UID = 34,
                    ReservationLanguageUsed_UID = 4,
                    ReservationPartialPaymentDetails = new List<contractsReservation.ReservationPartialPaymentDetail>(),
                    ReservationRooms = new List<contractsReservation.ReservationRoom>
                    {
                        new Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                        {
                            AdultCount = 1,
                            CancellationDate = DateTime.UtcNow,
                            CancellationPaymentModel = 1,
                            CancellationPolicyDays = 1,
                            ChildCount = 1,
                            CreatedDate = DateTime.UtcNow,
                            DateFrom = DateTime.UtcNow.AddDays(1),
                            DateTo = DateTime.UtcNow.AddDays(5),
                            GuestName = "Test1 Teste2",
                            IsCancellationAllowed = true,
                            CancellationCosts = true,
                            CancellationValue = 1,
                            CancellationNrNights = 2,
                            Package_UID = 0,
                            Rate = new Reservation.BL.Contracts.Data.Rates.Rate
                            {
                                Currency_UID = 34,
                                Description = null,
                                Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                                Property_UID = 1806,
                                UID = 22645
                            },
                            Rate_UID = 22645,
                            ReservationRoomAdditionalData = new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    CancellationCosts = true,
                                    Value = 2,
                                    Property_UID = 1806,
                                    Days = 1,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                },
                                ReservationRoomNo = "RES000024-1806/1",
                                ReservationRoom_UID = 84029
                            },
                            ReservationRoomChilds = new List<contractsReservation.ReservationRoomChild>
                            {
                                new contractsReservation.ReservationRoomChild
                                {
                                    Age = null,
                                    ChildNo = 1,
                                    ReservationRoom_UID = 84029,
                                    UID = 5334
                                }
                            },
                            ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail>
                            {
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203628
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203629
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203630
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203631
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203632
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203633
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203634
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203635
                                }
                            },
                            ReservationRoomNo = "RES000024-1806/1",
                            ReservationRoomsExtrasSum = 0,
                            ReservationRoomsPriceSum = 110,
                            ReservationRoomsTotalAmount = 110,
                            Reservation_UID = 76520,
                            RoomType = new Reservation.BL.Contracts.Data.Properties.RoomType
                            {
                                AcceptsChildren = true,
                                AcceptsExtraBed = false,
                                AdultMaxOccupancy = 3,
                                AdultMinOccupancy = 1,
                                ChildMaxOccupancy = 1,
                                ChildMinOccupancy = 0,
                                CreatedDate = DateTime.UtcNow,
                                MaxOccupancy = 4,
                                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                                Property_UID = 1806,
                                Qty = 100,
                                UID = 5901
                            },
                            RoomType_UID = 5901,
                            Status = 1,
                            TotalTax = 0,
                            UID = 84029
                        }
                    },
                    RoomsExtras = 0,
                    RoomsPriceSum = 110,
                    RoomsTax = 0,
                    RoomsTotalAmount = 110,
                    Status = 1,
                    Tax = 0,
                    TotalAmount = 110,
                    TotalTax = 0,
                    TransactionId = Guid.NewGuid().ToString(),
                    UID = 76522
                });
            #endregion

            #region reservation with channel pull
            _reservationsInDataBase.Add(
               new contractsReservation.Reservation
               {
                   Adults = 1,
                   BillingCity = "asdasdasd",
                   BillingContactName = "Test1 Teste2",
                   BillingCountry_UID = 157,
                   BillingEmail = "tony.santos@visualforma.pt",
                   BillingPhone = "123123",
                   BillingState_UID = 2430214,
                   CancellationPolicy = "Teste",
                   CancellationPolicyDays = 2,
                   Channel_UID = 84,
                   Children = 1,
                   CreatedDate = DateTime.UtcNow,
                   Date = DateTime.UtcNow,
                   Guest = new Guest
                   {
                       AllowMarketing = true,
                       City = "asdasdasd",
                       Client_UID = 1011,
                       Country_UID = 157,
                       CreateDate = DateTime.UtcNow,
                       Currency_UID = 34,
                       Email = "tony.santos@visualforma.pt",
                       FirstName = "Test1",
                       Gender = "M",
                       GuestCategory_UID = 1,
                       IDCardNumber = "12312312",
                       Index = 0,
                       IsActive = true,
                       IsImportedFromExcel = false,
                       Language_UID = 4,
                       LastName = "Teste2",
                       Phone = "123123123",
                       Prefix = 1,
                       Property_UID = 1806,
                       State_UID = 2430214,
                       UID = 284630,
                       UseDifferentBillingInfo = false,
                       UserName = "tony.santos@visualforma.pt",
                       UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                   },
                   GuestCity = "asdasdasd",
                   GuestCountry_UID = 157,
                   GuestEmail = "tony.santos@visualforma.pt",
                   GuestFirstName = "Test1",
                   GuestIDCardNumber = "123123123",
                   GuestLastName = "Teste2",
                   GuestPhone = "123123",
                   GuestState_UID = 2430214,
                   Guest_UID = 284630,
                   IPAddress = "::1",
                   InternalNotesHistory = DateTime.UtcNow.ToString(),
                   Number = "CHANNELX-RES123",
                   PropertyBaseCurrencyExchangeRate = 1,
                   Property_UID = 1806,
                   ReservationAdditionalData = new contractsReservation.ReservationsAdditionalData
                   {
                       ReservationRoomList = new List<contractsReservation.ReservationRoomAdditionalData>
                       {
                            new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                }
                            }
                       }
                   },
                   ReservationBaseCurrency_UID = 34,
                   ReservationCurrencyExchangeRate = 1,
                   ReservationCurrencyExchangeRateDate = DateTime.UtcNow,
                   ReservationCurrency_UID = 34,
                   ReservationLanguageUsed_UID = 4,
                   ReservationPartialPaymentDetails = new List<contractsReservation.ReservationPartialPaymentDetail>(),
                   ReservationRooms = new List<contractsReservation.ReservationRoom>
                   {
                        new Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                        {
                            AdultCount = 1,
                            CancellationDate = DateTime.UtcNow,
                            CancellationPaymentModel = 1,
                            CancellationPolicyDays = 0,
                            ChildCount = 1,
                            CreatedDate = DateTime.UtcNow,
                            DateFrom = DateTime.UtcNow.AddDays(1),
                            DateTo = DateTime.UtcNow.AddDays(5),
                            GuestName = "Test1 Teste2",
                            IsCancellationAllowed = true,
                            Package_UID = 0,
                            Rate = new Reservation.BL.Contracts.Data.Rates.Rate
                            {
                                Currency_UID = 34,
                                Description = null,
                                Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                                Property_UID = 1806,
                                UID = 22645
                            },
                            Rate_UID = 22645,
                            ReservationRoomAdditionalData = new contractsReservation.ReservationRoomAdditionalData
                            {
                                CancellationPolicy = new Reservation.BL.Contracts.Data.Rates.CancellationPolicy
                                {
                                    Days = 0,
                                    Description = string.Empty,
                                    IsCancellationAllowed = true,
                                    Name = string.Empty,
                                    PaymentModel = 1,
                                    UID = 2022
                                },
                                ReservationRoomNo = "CHANNELX-RES123/1",
                                ReservationRoom_UID = 84029
                            },
                            ReservationRoomChilds = new List<contractsReservation.ReservationRoomChild>
                            {
                                new contractsReservation.ReservationRoomChild
                                {
                                    Age = null,
                                    ChildNo = 1,
                                    ReservationRoom_UID = 84029,
                                    UID = 5334
                                }
                            },
                            ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail>
                            {
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203628
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203629
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203630
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203631
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203632
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203633
                                }
                                ,
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203634
                                },
                                new contractsReservation.ReservationRoomDetail
                                {
                                    AdultPrice = 110,
                                    ChildPrice = null,
                                    CreatedDate = DateTime.UtcNow,
                                    Date = DateTime.UtcNow,
                                    Price = 110,
                                    RateRoomDetails_UID = 5137349,
                                    ReservationRoomDetailsAppliedIncentives =
                                        new List<contractsReservation.ReservationRoomDetailsAppliedIncentive>(),
                                    ReservationRoom_UID = 84029,
                                    UID = 203635
                                }
                            },
                            ReservationRoomNo = "CHANNELX-RES123/1",
                            ReservationRoomsExtrasSum = 0,
                            ReservationRoomsPriceSum = 110,
                            ReservationRoomsTotalAmount = 110,
                            Reservation_UID = 76520,
                            RoomType = new Reservation.BL.Contracts.Data.Properties.RoomType
                            {
                                AcceptsChildren = true,
                                AcceptsExtraBed = false,
                                AdultMaxOccupancy = 3,
                                AdultMinOccupancy = 1,
                                ChildMaxOccupancy = 1,
                                ChildMinOccupancy = 0,
                                CreatedDate = DateTime.UtcNow,
                                MaxOccupancy = 4,
                                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                                Property_UID = 1806,
                                Qty = 100,
                                UID = 5901
                            },
                            RoomType_UID = 5901,
                            Status = 1,
                            TotalTax = 0,
                            UID = 84029
                        }
                   },
                   RoomsExtras = 0,
                   RoomsPriceSum = 110,
                   RoomsTax = 0,
                   RoomsTotalAmount = 110,
                   Status = 1,
                   Tax = 0,
                   TotalAmount = 110,
                   TotalTax = 0,
                   TransactionId = Guid.NewGuid().ToString(),
                   UID = 76521
               });
            #endregion
        }

        private void FillTheGuaranteeTypeforTest()
        {
            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 4,
                    GuaranteeTypeName = "Credit Card",
                    Guarantee_TypeCode = 5
                });
            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 5,
                    GuaranteeTypeName = "Agency Name/address For Guarantee",
                    Guarantee_TypeCode = 18
                });

            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 6,
                    GuaranteeTypeName = "Company Name/address For Guarantee",
                    Guarantee_TypeCode = 29
                });

            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 7,
                    GuaranteeTypeName = "Guest Name/address For Guarantee",
                    Guarantee_TypeCode = 24
                });

            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 8,
                    GuaranteeTypeName = "Guarantee To Agency IATA Number",
                    Guarantee_TypeCode = 19
                });

            _guaranteeTypeInDataBase.Add(
                new GuaranteeType
                {
                    GuaranteeType_UID = 9,
                    GuaranteeTypeName = "Corporate ID For Guarantee",
                    Guarantee_TypeCode = 30
                });
        }

        private void FillTheDepositPoliciesGuaranteeTypeforTest()
        {
            #region Active Credit Card Guarantee

            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 4,
                    IsActive = true,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 5,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 6,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 7,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 8,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 321,
                    DepositPolicy_UID = 1821,
                    GuaranteeType_UID = 9,
                    IsActive = false,
                });

            #endregion

            #region Active Agency IATA Number Guarantee

            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 4,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 5,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 6,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 7,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 8,
                    IsActive = true,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 320,
                    DepositPolicy_UID = 1820,
                    GuaranteeType_UID = 9,
                    IsActive = false,
                });

            #endregion

            #region Without guarantees selected

            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 4,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 5,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 6,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 7,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 8,
                    IsActive = false,
                });
            _depositPoliciesGuaranteeTypeInDataBase.Add(
                new DepositPoliciesGuaranteeType
                {
                    UID = 322,
                    DepositPolicy_UID = 1822,
                    GuaranteeType_UID = 9,
                    IsActive = false,
                });

            #endregion
        }

        private void FillTheAppSettingforTest()
        {
            _appSettingInDataBase.Add(
                new Setting
                {
                    UID = 1,
                    Name = "RsaEncryptionKey",
                    Value =
                        "<RSAKeyValue><Modulus>49BStnomTkKFC1ERxGA/MhfVuwwNW8JcO52FKXFWfpb2m8a/f0Q3l7FRz6pC8Hs65+BOIwUN/7RxGV+PzIQ4ZwVwEuY7GBQonjXTP3D20yUnXZA/NtGmxWYJlVnD8VUZLQdkKs3hBAfKnWjyOgnGoq7CIFyJ9KKOxRh39hQ+7TE=</Modulus><Exponent>AQAB</Exponent><P>+hwRM4IsoIyrwSlbl01L57gZk41RHSYkcZwnsrjodAc/0KSCPm4JhKn8CKc9Xi8Rjq5ekJ0xSbS9VPAmJrwQrQ==</P><Q>6S3VBl3OF3rVnwg9Cg0bc+FGIfHgbZcCw1gV/k2awmS6GKpQHZ7Dr5UnH+QRXSJ1IQ3PJ6rieEeUsGhZxiarFQ==</Q><DP>ZMj0oYX+R8AH4jGxR9oNEVYdcFkM66sYGnPrh1h9y2u0anYwScn7qer5td72msJq180qLCo711CuztBq/0bfjQ==</DP><DQ>YQ7Zv8el9DIF3ydfuOJRzf8z4Qc8AoG7/bGZnfuRcl7Y81FY/atLCrfLzENzUs/37yU/V+SSVbx90Jvu2kLYLQ==</DQ><InverseQ>aq95HffRtKIVEpCEjyolZxm8ovf5SpfT2dkXzbxd7hIPNuM9mNWu897q9KKv3EyEY36dAryiIetVMhFGK0HzuQ==</InverseQ><D>Dyk6j/VSHkw0AXxMM+b53bITYcbcDrrBG6CQj6EA0hzm3Zgc/3HBR2GgIbNhkBKLaYoWeSMpetZ93mPrNH+qJyTeKIPx7mLGu14GS40uikfYIMhRlihRfNskYfiTXsRA35GFdyTgLCPo+OJXVxqaFZpjwXGoBri7Vlgz9E+LRak=</D></RSAKeyValue>",
                    Category = "All"
                });
        }

        private void FillTheRoomTypesForTest()
        {
            _roomTypeInDataBase.Add(new BL.Contracts.Data.Properties.RoomType
            {
                AcceptsChildren = true,
                AcceptsExtraBed = false,
                AdultMaxOccupancy = 2,
                AdultMinOccupancy = 1,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 0,
                CreatedDate = DateTime.UtcNow,
                MaxOccupancy = 4,
                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                Property_UID = 1806,
                Qty = 100,
                UID = 5901
            });
            _roomTypeInDataBase.Add(new BL.Contracts.Data.Properties.RoomType
            {
                AcceptsChildren = true,
                AcceptsExtraBed = false,
                AdultMaxOccupancy = 2,
                AdultMinOccupancy = 1,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 0,
                CreatedDate = DateTime.UtcNow,
                MaxOccupancy = 4,
                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                Property_UID = 1806,
                Qty = 150,
                UID = 5902
            });
            _roomTypeInDataBase.Add(new BL.Contracts.Data.Properties.RoomType
            {
                AcceptsChildren = true,
                AcceptsExtraBed = false,
                AdultMaxOccupancy = 5,
                AdultMinOccupancy = 1,
                ChildMaxOccupancy = 1,
                ChildMinOccupancy = 0,
                CreatedDate = DateTime.UtcNow,
                MaxOccupancy = 4,
                Name = "16f857ee-3fea-4e9c-a4ff-8884e1381cf6",
                Property_UID = 1635,
                Qty = 50,
                UID = 5900
            });
        }

        private void FillTheGuestforTest()
        {
            _guestInDataBase.Add(
                new OB.BL.Contracts.Data.CRM.Guest
                {
                    AllowMarketing = true,
                    City = "asdasdasd",
                    Client_UID = 1011,
                    Country_UID = 157,
                    CreateDate = DateTime.UtcNow,
                    Currency_UID = 34,
                    Email = "tony.santos@visualforma.pt",
                    FirstName = "Test1",
                    Gender = "M",
                    GuestCategory_UID = 1,
                    IDCardNumber = "12312312",
                    Index = 0,
                    IsActive = true,
                    IsImportedFromExcel = false,
                    Language_UID = 4,
                    LastName = "Teste2",
                    Phone = "123123123",
                    Prefix = 1,
                    Property_UID = 1806,
                    State_UID = 2430214,
                    UID = 284630,
                    UseDifferentBillingInfo = false,
                    UserName = "tony.santos@visualforma.pt",
                    UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                });
            _guestInDataBase.Add(
                new OB.BL.Contracts.Data.CRM.Guest
                {
                    AllowMarketing = true,
                    City = "asdasdasd",
                    Client_UID = 1011,
                    Country_UID = 157,
                    CreateDate = DateTime.UtcNow,
                    Currency_UID = 34,
                    Email = "tony@visualforma.pt",
                    FirstName = "Test1",
                    Gender = "M",
                    GuestCategory_UID = 1,
                    IDCardNumber = "123123212",
                    Index = 0,
                    IsActive = true,
                    IsImportedFromExcel = false,
                    Language_UID = 4,
                    LastName = "Teste2",
                    Phone = "1231231213",
                    Prefix = 1,
                    Property_UID = 1806,
                    State_UID = 2430214,
                    UID = 284631,
                    UseDifferentBillingInfo = false,
                    UserName = "tony@visualforma.pt",
                    UserPassword = "MjgwMjgx-d1WhKnSm1V4="
                });
        }

        private void FillRatesForTest()
        {
            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1806,
                    UID = 22645
                });
            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 4639
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {

                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 2
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 3
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {

                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 4
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 5
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    BeginSale = new DateTime(2016, 01, 01),
                    EndSale = new DateTime(2016, 02, 01),
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 6
                });

            _ratesInDataBase.Add(
                new OB.BL.Contracts.Data.Rates.Rate
                {
                    Currency_UID = 34,
                    Description = null,
                    Name = "6a3fe6cc-ffce-4e1a-95c8-f5f57c55d3b0",
                    Property_UID = 1263,
                    UID = 7
                });
        }

        private void FillChildTermsForTest()
        {
            _childTermsInDataBase.Add(
                new ChildTerm
                {
                    UID = 1,
                    IsDeleted = false,
                    Property_UID = 1806,
                    AgeFrom = 0,
                    AgeTo = 3,
                    CountsAsAdult = false,
                    IsCountForOccupancy = true
                });
            _childTermsInDataBase.Add(
                new ChildTerm
                {
                    UID = 2,
                    IsDeleted = false,
                    Property_UID = 1263,
                    AgeFrom = 0,
                    AgeTo = 3,
                    CountsAsAdult = false,
                    IsCountForOccupancy = true
                });
            _childTermsInDataBase.Add(
                new ChildTerm
                {
                    UID = 3,
                    IsDeleted = false,
                    Property_UID = 1806,
                    AgeFrom = 0,
                    AgeTo = 7,
                    CountsAsAdult = true,
                    IsCountForOccupancy = true
                });
        }

        private void FillPropertyLightForTest()
        {
            _propertyLightInDataBase.Add(
                new PropertyLight
                {
                    Address1 = "dwedwadwd",
                    Address2 = "defwefwef",
                    BaseCurrency_UID = 34,
                    City = "Faro",
                    City_UID = 23123,
                    ClientName = "dwdw",
                    Client_UID = 198,
                    Country_Name = "Portugal",
                    Country_UID = 157,
                    CurrencyISO = "EUR",
                    CurrencySymbol = "€",
                    Email = "teste@visualforma.com",
                    Zone_UID = 8232,
                    Zone_Name = "dwqdwq",
                    UID = 1806,
                    TimeZone_UID = 1,
                    State_UID = 4343,
                    State = "efefefe",
                    InvoicingCity = "dasdwdw",
                    InvoicingCity_UID = 34232,
                    Name = "Hotel Teste"
                });
        }

        private void FillIncentivesForTest()
        {
            _incentivesInDataBase.Add(
                new Incentive
                {
                    UID = 1,
                    DayDiscount = new List<decimal> { 12M, 34M },
                    Days = 2,
                    DiscountPercentage = 20,
                    Property_UID = 1806,
                    Rate_UID = 22645,
                    IsLastMinuteInHours = true,
                    IncentiveFrom = DateTime.UtcNow.AddDays(-2),
                    IncentiveTo = DateTime.UtcNow.AddDays(3),
                    IncentiveType = Constants.IncentiveType.LastMinuteBooking,
                    IncentiveType_UID = 2
                });
        }

        private void FillTaxPoliciesForTest()
        {
            _taxPolicyInDataBase.Add(
                new TaxPolicy
                {
                    UID = 1,
                    Value = 10,
                    ReservationRoom_UID = 84029,
                    Rate_UID = 22645,
                    Property_UID = 1806,
                    IsPerPerson = true,
                    IsPercentage = true
                });
        }

        private void FillTaxPoliciesPerNightForTest()
        {
            _taxPolicyInDataBase.Add(
                new TaxPolicy
                {
                    UID = 1,
                    Value = 10,
                    ReservationRoom_UID = 84029,
                    Rate_UID = 22645,
                    Property_UID = 1806,
                    IsPerPerson = false,
                    IsPercentage = false,
                    IsPerNight = true
                });
        }

        private void FillRateRoomDetailsForTest()
        {
            _rateRoomDetailResInDataBase.Add(
                new RateRoomDetailReservation
                {
                    UID = 5137349,
                    AdultPrice = 100,
                    Adults = 2,
                    Allotment = 10,
                    Channel_UID = 1,
                    ChildPrice = 80,
                    Childs = 0,
                    CurrencyId = 4,
                    Rate_UID = 22645,
                    Property_UID = 1806,
                    IsCommission = true,
                    RoomType_UID = 5901
                }
                );
        }

        private void FillPromotionalCodesForTest()
        {
            _promotionalCodesInDataBase = new List<PromotionalCode>();
        }

        private void FillRateBuyerGroupForTest()
        {
            _rateBuyerGroupInDataBase.Add(
                new RateBuyerGroup
                {
                    UID = 1,
                    Value = 10,
                    Rate_UID = 22645,
                    IsPercentage = true,
                    IsValueDecrease = true,
                    GDSValue = 12,
                    GDSValueIsDecrease = false,
                    GDSValueIsPercentage = false,
                    BuyerGroup_UID = 1
                });
        }

        private void FillDepositPoliciesForTest()
        {
            _depositPoliciesInDataBase.Add(new DepositPolicy
            {
                UID = 1820,
                DepositCosts = true,
                IsDepositCostsAllowed = true,
                Value = 20,
                RateUID = 22645,
                Property_UID = 1806,
                Percentage = 1,
                PaymentModel = 2,
                Days = 2
            });
        }

        private void FillCancellationPoliciesForTest()
        {
            _cancellationPolicyInDataBase.Add(new CancellationPolicy
            {
                UID = 2022,
                CancellationCosts = true,
                IsCancellationAllowed = true,
                Value = 20,
                RateUID = 22645,
                Property_UID = 1806,
                NrNights = 2,
                PaymentModel = 1,
                Days = 2
            });
            _cancellationPolicyInDataBase.Add(new CancellationPolicy
            {
                UID = 2023,
                CancellationCosts = true,
                IsCancellationAllowed = true,
                Value = 20,
                RateUID = 22645,
                Property_UID = 1806,
                NrNights = 1,
                PaymentModel = 2,
                Days = 1
            });

        }

        private void FillOtherPoliciesForTest()
        {
            _otherPoliciesInDataBase.Add(
            new OtherPolicy
            {
                UID = 1,
                Property_UID = 1806,
                OtherPolicy_Name = "other policy teste",
                OtherPolicy_Description = "teste"
            });
        }

        private void FillExtrasForTest()
        {
            _extrasInDataBase.Add(new Extra
            {
                UID = 1,
                Value = 10,
                Property_UID = 1806,
                IsActive = true
            });
        }

        private void FillPaymentMethodTypesForTest()
        {
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 1,
                    Code = 1,
                    Name = "Credit Card",
                    PaymentType = 0,
                    IsBilled = false,
                    Ordering = null,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 2,
                    Code = 2,
                    Name = "Direct payment at the hotel",
                    PaymentType = 2,
                    IsBilled = false,
                    Ordering = 4,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 3,
                    Code = 3,
                    Name = "Bank deposit",
                    PaymentType = 0,
                    IsBilled = false,
                    Ordering = null,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 4,
                    Code = 4,
                    Name = "Faturada",
                    PaymentType = 0,
                    IsBilled = true,
                    Ordering = null,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 5,
                    Code = 5,
                    Name = "Daily billed",
                    PaymentType = 1,
                    IsBilled = true,
                    Ordering = 1,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 6,
                    Code = 6,
                    Name = "Daily billed and extras",
                    PaymentType = 1,
                    IsBilled = true,
                    Ordering = 2,
                    AllowParcialPayments = false
                });
            _paymentMethodTypesInDataBase.Add(
                new PaymentMethodType
                {
                    UID = 7,
                    Code = 7,
                    Name = "Pre payment",
                    PaymentType = 1,
                    IsBilled = true,
                    Ordering = 3,
                    AllowParcialPayments = false
                });
        }

        private void FillExtrasBillingTypesForTest()
        {
            _extrasBillingTypeInDataBase.Add(new ExtrasBillingType
            {
                UID = 1,
                Type = 1,
                Extras_UID = 1
            });
        }

        private void FillGroupCodesForTest()
        {
            _groupCodesInDataBase.Add(new GroupCode
            {
                UID = 1,
                Rate_UID = 22645,
                Property_UID = 1806,
                InternalCode = "test",
                DateFrom = DateTime.UtcNow.AddDays(-2),
                DateTo = DateTime.UtcNow.AddDays(2)
            });
        }

        private void FillChannelLightForTest()
        {
            _channelLightInDataBase.Add(new ChannelLight
            {
                UID = 1,
                Name = "Booking Engine"
            });
        }

        private void FillCurrenciesForTest()
        {
            _currenciesInDataBase.Add(
                new Currency
                {
                    UID = 16,
                    Name = "Brazil Real",
                    Symbol = "BRL",
                    CurrencySymbol = "R$",
                    DefaultPositionNumber = 3,
                    PaypalCurrencyCode = "22"
                });

            _currenciesInDataBase.Add(
                new Currency
                {
                    UID = 34,
                    Name = "Euro",
                    Symbol = "EUR",
                    CurrencySymbol = "€",
                    DefaultPositionNumber = 1,
                    PaypalCurrencyCode = "98"
                });

            _currenciesInDataBase.Add(
            new Currency
            {
                UID = 109,
                Name = "US Dollar",
                Symbol = "USD",
                CurrencySymbol = "€",
                DefaultPositionNumber = 1,
                PaypalCurrencyCode = "98"
            });
        }

        private void FillVisualStateForTest()
        {
            _visualStateInDataBase.Add(
                new OB.Domain.Reservations.VisualState
                {
                    LookupKey_1 = "7652076520",
                    JSONData = "{ \"UserId\":70, \"Date\":\"2016-09-02T15:46:44.318318Z\", \"Read\":false }"
                });
        }

        #endregion

        #region Test Validate Guarantee

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_ValidCreditCard()
        {
            FillTheAppSettingforTest();
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _appSettingRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
                .Callback<ListSettingRequest>(x => Send(x)).Returns(_appSettingInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 5).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1821 &&
     x.IsActive == true).ToList());

            _securityRepoMock.Setup(x => x.DecryptCreditCards(It.IsAny<Contracts.Requests.ListCreditCardRequest>())).Callback<Contracts.Requests.ListCreditCardRequest>(x => Send(x)).Returns(new List<string> { "4111111111111111", "737" });

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1).WithCreditCardPayment();
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            var resPaymentDetails = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail);
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationPaymentDetail = resPaymentDetails;
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1821;
                resRoom.DeposityGuaranteeType = "5";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_InvalidCreditCard()
        {
            FillTheAppSettingforTest();
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _appSettingRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
                .Callback<ListSettingRequest>(x => Send(x)).Returns(_appSettingInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 5).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1821 &&
                 x.IsActive == true).ToList());

            _securityRepoMock.Setup(x => x.DecryptCreditCards(It.IsAny<Contracts.Requests.ListCreditCardRequest>())).Callback<Contracts.Requests.ListCreditCardRequest>(x => Send(x)).Returns(new List<string> { "411111111111", "737" });

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1).WithInvalidCreditCardPayment();
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            var resPaymentDetails = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail);
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationPaymentDetail = resPaymentDetails;
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1821;
                resRoom.DeposityGuaranteeType = "5";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("InvalidCreditCard", errors.First().ErrorType);
            Assert.AreEqual("Error making payment on gateway - Error Code = -4000", errors.First().Description);
            Assert.AreEqual(-4000, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_CreditCardIsEmpty()
        {
            FillTheAppSettingforTest();
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _appSettingRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
                .Callback<ListSettingRequest>(x => Send(x)).Returns(_appSettingInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 5).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1821 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1).WithInvalidCreditCardPayment();
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            var resPaymentDetails = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail);
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationPaymentDetail = resPaymentDetails;
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1821;
                resRoom.DeposityGuaranteeType = "5";
            }
            res.ReservationPaymentDetail.CardNumber = string.Empty;
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("InvalidCreditCard", errors.First().ErrorType);
            Assert.AreEqual("Error must occur when CardNumber field is empty - Error Code = -4100", errors.First().Description);
            Assert.AreEqual(-4100, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_PaymentMethodTypeIsNull()
        {
            FillTheAppSettingforTest();
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _appSettingRepoMock.Setup(x => x.ListSettings(It.IsAny<ListSettingRequest>()))
                .Callback<ListSettingRequest>(x => Send(x)).Returns(_appSettingInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 5).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1821 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1).WithInvalidCreditCardPayment();
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            var resPaymentDetails = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail);
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationPaymentDetail = resPaymentDetails;
            res.ReservationRooms = resRooms.ToList();
            res.PaymentMethodType_UID = null;
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1821;
                resRoom.DeposityGuaranteeType = "5";
            }
            res.ReservationPaymentDetail.CardNumber = string.Empty;
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("InvalidPaymentMethod", errors.First().ErrorType);
            Assert.AreEqual("Invalid Payment Method", errors.First().Description);
            Assert.AreEqual(-524, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_IATANumberGuaranteeIsSelectedInOB()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 19).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1820 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1820;
                resRoom.DeposityGuaranteeType = "19";
                resRoom.DepositInformation = "IATA Number";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_CorporateIDGuaranteeIsNotSelectedInOB()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1820 && x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1820;
                resRoom.DeposityGuaranteeType = "30";
                resRoom.DepositInformation = "CorporateID";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("GuaranteeTypeNotSelectedError", errors.First().ErrorType.ToString());
            Assert.AreEqual("Guarantee type is not selected in OB", errors.First().Description);
            Assert.AreEqual(-4102, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_IATANumberGuaranteeWithoutDepositInformation()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<ListGuaranteeTypesFilterRequest>())).Callback<ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase.Where(x => x.Guarantee_TypeCode == 19).ToList());

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1820 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1820;
                resRoom.DeposityGuaranteeType = "19";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("GuaranteeTypeInformationError", errors.First().ErrorType);
            Assert.AreEqual("Invalid guarantee type information", errors.First().Description);
            Assert.AreEqual(-4101, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void Test_ReservationValidator_ValidateGuarantee_ReservationIsNull()
        {
            contractsReservation.Reservation res = null;
            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("ReservationError", errors.First().ErrorType);
            Assert.AreEqual("Reservation detail must be provided - Error Code = -4104", errors.First().Description);
            Assert.AreEqual(-4104, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_IATANumberGuaranteeWithDepositPolicyUIDZero()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<Contracts.Requests.ListGuaranteeTypesFilterRequest>())).Callback<Contracts.Requests.ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 0 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 0;
                resRoom.DeposityGuaranteeType = "19";
                resRoom.DepositInformation = "IATA Number";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_WithoutDepositGuaranteeType()
        {
            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1820;
                resRoom.DeposityGuaranteeType = "";
                resRoom.DepositInformation = "AgencyNameGuarantee";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_WithDepositGuaranteeTypeWrongInformationText()
        {
            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1820;
                resRoom.DeposityGuaranteeType = "type";
                resRoom.DepositInformation = "AgencyNameGuarantee";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("GuaranteeTypeNotSelectedError", errors.First().ErrorType.ToString());
            Assert.AreEqual("Guarantee type is not selected in OB", errors.First().Description);
            Assert.AreEqual(-4102, errors.First().ErrorCode);
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_WithoutGuaranteesForThisDeposit_WithDeposityGuaranteeType()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<Contracts.Requests.ListGuaranteeTypesFilterRequest>())).Callback<Contracts.Requests.ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1823 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1823;
                resRoom.DeposityGuaranteeType = "19";
                resRoom.DepositInformation = "IATA Number";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_WithAllInactiveGuarantees_WithDeposityGuaranteeType()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<Contracts.Requests.ListGuaranteeTypesFilterRequest>())).Callback<Contracts.Requests.ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1822 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1822;
                resRoom.DeposityGuaranteeType = "19";
                resRoom.DepositInformation = "IATA Number";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain 0 errors
            Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ReservationValidator_ValidateGuarantee_WithAllInactiveGuarantees_WithoutDepositInformation()
        {
            FillTheGuaranteeTypeforTest();
            FillTheDepositPoliciesGuaranteeTypeforTest();

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListGuaranteeTypesFilter(It.IsAny<Contracts.Requests.ListGuaranteeTypesFilterRequest>())).Callback<Contracts.Requests.ListGuaranteeTypesFilterRequest>(x => Send(x)).Returns(_guaranteeTypeInDataBase);

            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPoliciesGuaranteeTypes(It.IsAny<ListDepositPoliciesGuaranteeTypesRequest>())).Callback<ListDepositPoliciesGuaranteeTypesRequest>(x => Send(x)).Returns(_depositPoliciesGuaranteeTypeInDataBase.Where(x => x.DepositPolicy_UID == 1822 &&
                 x.IsActive == true).ToList());

            #region Reservation
            ReservationDataBuilder builder = new ReservationDataBuilder(59).WithNewGuest().WithRoom(1, 1);
            var resRooms = builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x));
            contractsReservation.Reservation res = DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail);
            res.ReservationRooms = resRooms.ToList();
            foreach (contractsReservation.ReservationRoom resRoom in res.ReservationRooms)
            {
                resRoom.DepositPolicy_UID = 1822;
                resRoom.DeposityGuaranteeType = "19";
            }
            #endregion

            var errors = ReservationValidatorPOCO.ValidateReservation(res, new ValidateReservationRequest()
            {
                ValidateGuarantee = true
            });

            //Response should contain only 1 error
            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            //Assert Response Format
            Assert.AreEqual("GuaranteeTypeInformationError", errors.First().ErrorType.ToString());
            Assert.AreEqual("Invalid guarantee type information", errors.First().Description);
            Assert.AreEqual(-4101, errors.First().ErrorCode);
        }

        #endregion

        [TestMethod]
        [TestCategory("ReservationValidator")]
        public void Test_ValidRestrictions()
        {
            OB.BL.Operations.Test.Helper.UnitTestDetector.IsRunningInUnitTest = true;

            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var cancellationPolicy = new CancellationPolicy
            {
                CancellationCosts = false,
                Days = 0,
                IsCancellationAllowed = true,
                PaymentModel = 1,
                Name = "Teste",
                Description = "teste description"
            };

            var periods = new List<Period>() { new Period { DateFrom = DateTime.Now.AddDays(1), DateTo = DateTime.Now.AddDays(5) } };
            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll().AddRateChannelsAll(channels)
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 1,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1),
                    CheckOut = DateTime.Now.AddDays(5)
                })
                .AddPropertyChannels(channels)
                .WithAdultPrices(3, 100)
                .WithChildPrices(3, 50).WithAllotment(1000);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID, "", "", false, 0, true, 1, periods);
            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806, 76520)
                            .WithNewGuest(null, 284630)
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(1);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>())).Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
            {
                Errors = null,
                Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                TotalRecords = -1,
                Status = Reservation.BL.Contracts.Responses.Status.Success
            });
            #endregion SETUP MOCK POCO

            #region SETUP SQL MANAGER
            _sqlManagerRepoMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                .Returns(1);

            _sqlManagerRepoMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                .Returns((string s, DynamicParameters parameters) =>
                {
                    return new List<OB.BL.Contracts.Data.Properties.Inventory>
                    {
                        new OB.BL.Contracts.Data.Properties.Inventory
                        {

                        }
                    };
                });
            #endregion

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Initiate,
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(0, errors.Count, "Expected errors count = 0");
        }

        [TestMethod]
        [TestCategory("Test_AllotmentNotAvailable")]
        public void Test_AllotmentNotAvailable()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5),
                        })
                        .AddPropertyChannels(channels)
                        .WithAllotment(0);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806, 76520)
                            .WithNewGuest(null, 284630)
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(1);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 0);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.AllotmentNotAvailable.ToString()), "Expected AllotmentNotAvailable");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidPropertyChannel")]
        public void Test_InvalidPropertyChannel()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        });

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-501);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPropertyChannel.ToString()), "Expected InvalidPropertyChannel");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidRateChannel")]
        public void Test_InvalidRateChannel()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        //.AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5),
                        })
                        .AddPropertyChannels(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-502);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRateChannel.ToString()), "Expected InvalidRateChannel");
        }

        [TestMethod]
        [TestCategory("Test_InvalidRateRoomDetails")]
        public void Test_InvalidRateRoomDetails()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        //.AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(10),
                            CheckOut = DateTime.Now.AddDays(12),
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-500);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.AllotmentNotAvailable.ToString()), "Expected AllotmentNotAvailable");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_MaxOccupancyExceeded")]
        public void Test_MaxOccupancyExceeded()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5),
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-511);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            _obReservationsLookupsRepoMock.Setup(x => x.ListReservationLookups(It.IsAny<ListReservationLookupsRequest>()))
                .Returns(new Contracts.Data.Reservations.ReservationLookups
                {
                    RatesLookup = builder.InputData.Rates.ToDictionary(x => x.UID),
                });
            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 4,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxOccupancyExceeded.ToString()), "Expected MaxOccupancyExceeded");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_AdultMaxOccupancyExceeded")]
        public void Test_AdultMaxOccupancyExceeded()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5),
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-512);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            InitializeLookupsMock(builder.InputData.Rates);
            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            #region SETUP SQL MANAGER
            _sqlManagerRepoMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                .Returns(1);

            _sqlManagerRepoMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                .Returns((string s, DynamicParameters parameters) =>
                {
                    return new List<OB.BL.Contracts.Data.Properties.Inventory>
                    {
                        new OB.BL.Contracts.Data.Properties.Inventory
                        {

                        }
                    };
                });
            #endregion

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 3,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxAdultsExceeded.ToString()), "Expected MaxAdultsExceeded");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_ChildMaxOccupancyExceeded")]
        public void Test_ChildMaxOccupancyExceeded()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels)
                        .AddChiltTerm();

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                .WithNewGuest().WithChildren(5334, 1, new List<int> { 1, 1 })
                .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                    builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                    builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID,
                    builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                .WithCancelationPolicy(false, 0, true, 1)
                .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn,
                    builder.InputData.SearchParameter[0].CheckOut)
                .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-513);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>(){1 , 1},
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxChildrenExceeded.ToString()), "Expected MaxChildrenExceeded");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_CloseDaysRestriction")]
        public void Test_CloseDaysRestriction()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithBlockedChannelsListUID(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-514);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedDayRestrictionError.ToString()), "Expected ClosedDayRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_MinLenghtOfStay")]
        public void Test_MinLenghtOfStay()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithMinLenghtOfStay(10)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-515);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });


            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MinimumLengthOfStayRestrictionError.ToString()), "Expected MinimumLengthOfStayRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_MaxLenghtOfStay")]
        public void Test_MaxLenghtOfStay()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithMaxLenghtOfStay(2)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-516);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaximumLengthOfStayRestrictionError.ToString()), "Expected MaximumLengthOfStayRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidStayTrought")]
        public void Test_InvalidStayTrought()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithStayTrought(4)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-517);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.StayTroughtRestrictionError.ToString()), "Expected StayTroughtRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidReleaseDays")]
        public void Test_InvalidReleaseDays()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1),
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithReleaseDays(5)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-518);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2),
                            DateTo = DateTime.Now.AddDays(5),
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReleaseDaysRestrictionError.ToString()), "Expected ReleaseDaysRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidCloseOnArrival")]
        public void Test_InvalidCloseOnArrival()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1).Date,
                            CheckOut = DateTime.Now.AddDays(5).Date
                        })
                        .AddPropertyChannels(channels)
                        .WithCloseOnArrival(true)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-519);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedOnArrivalRestrictionError.ToString()), "Expected ClosedOnArrivalRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_InvalidCloseOnDeparture")]
        public void Test_InvalidCloseOnDeparture()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645).AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1).Date,
                            CheckOut = DateTime.Now.AddDays(5).Date
                        })
                        .AddPropertyChannels(channels)
                        .WithCloseOnDeparture(true)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-520);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedOnDepartureRestrictionError.ToString()), "Expected ClosedOnDepartureRestrictionError");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_RateBeginSellingPeriod")]
        public void Test_RateBeginSellingPeriod()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, DateTime.Now.AddDays(10).Date, null, false, 22645)
                        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1).Date,
                            CheckOut = DateTime.Now.AddDays(5).Date
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-523);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.RateIsNotForSale.ToString()), "Expected RateIsNotForSale");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_RateEndSellingPeriod")]
        public void Test_RateEndSellingPeriod()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };

            builder = builder.AddRate(string.Empty, DateTime.Now.Date.AddDays(-5), DateTime.Now.AddDays(-1).Date, false, 22645)
                        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                        .AddRateRoomsAll()
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            CheckIn = DateTime.Now.AddDays(1).Date,
                            CheckOut = DateTime.Now.AddDays(5).Date
                        })
                        .AddPropertyChannels(channels)
                        .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, "RES000024-1806")
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-523);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.RateIsNotForSale.ToString()), "Expected RateIsNotForSale");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_OperatorPaymentMethodNotAllowed")]
        public void Test_OperatorPaymentMethodNotAllowed()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(84, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.Last().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(4);

            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase.Where(x => x.UID == 76521).ToList());

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-524);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO

            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 84
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPaymentMethod.ToString()), "Expected InvalidPaymentMethod");
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("Test_BeTpiPaymentMethodNotAllowed")]
        public void Test_BeTpiPaymentMethodNotAllowed()
        {
            #region BUILDERS
            // arrange
            var propertyId = 1806;
            var builder = new SearchBuilder(Container, propertyId);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddTPI(propertyId, Constants.TPIType.TravelAgent)
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels)
                .AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, propertyId)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithTPI(builder.InputData.Tpi)
                            .WithBilledTypePayment(4);

            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-524);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            #region SETUP SQL MANAGER
            _sqlManagerRepoMock.Setup(x => x.ExecuteSql(It.IsAny<string>()))
                .Returns(1);

            _sqlManagerRepoMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), null))
                .Returns((string s, DynamicParameters parameters) =>
                {
                    return new List<OB.BL.Contracts.Data.Properties.Inventory>
                    {
                        new OB.BL.Contracts.Data.Properties.Inventory
                        {

                        }
                    };
                });
            #endregion

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.AreEqual(1, errors.Count, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPaymentMethod.ToString()), "Expected InvalidPaymentMethod");
        }

        [TestMethod]
        [TestCategory("Test_InvalidReservation")]
        public void Test_InvalidReservation()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-529);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationUid = long.MaxValue,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = long.MaxValue + "/1"
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReservationDoesNotExist.ToString()), "Expected ReservationDoesNotExist");
        }

        [TestMethod]
        [TestCategory("Test_ReservationIsOnRequest")]
        public void Test_ReservationIsOnRequest()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels)
            .AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1)
                            .WithBookingOnRequest();
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            _reservationsInDataBase.First().IsOnRequest = true;
            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-560);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReservationIsOnRequest.ToString()), "Expected ReservationIsOnRequest");
        }

        [TestMethod]
        [TestCategory("Test_EmptyRoomNumber")]
        public void Test_EmptyRoomNumber()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-542);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = ""
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        }

        [TestMethod]
        [TestCategory("Test_InvalidUpdateRequest")]
        public void Test_InvalidUpdateRequest()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-542);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>(),
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        }

        [TestMethod]
        [TestCategory("Test_InvalidUpdateRequest2")]
        public void Test_InvalidUpdateRequest2()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase);

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-542);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                {
                    new contractsReservation.UpdateRoom()
                },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Modify
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        }

        [TestMethod]
        [TestCategory("Test_CancelationCosts")]
        public void Test_CancelationCosts()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels)
            .AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(true, 2, true, 1, 100)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO

            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
                .Returns(new List<TripAdvisorConfiguration>
                {
                    new TripAdvisorConfiguration
                    {
                        APIKey = "dwkqdqwodjwp",
                        IsCommentsActive = false,
                        IsTrackingCodeActive = false,
                        PropertyUid = 1806,
                        UID = 1
                    }
                });

            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(), _reservationsInDataBase.Where(x => x.UID == 76522).ToList());

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == 76522).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-544);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase.Where(x => x.UID == 2023).ToList());

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 1,
                            ChildAges = childAges,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Modify
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.CancelationCostsAppliedError.ToString()), "Expected CancelationCostsAppliedError");
        }

        [TestMethod]
        [TestCategory("Test_ChildrenAgesMissing_NULL")]
        public void Test_ChildrenAgesMissing_NULL()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels)
            .AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(),
                _reservationsInDataBase.Where(x => x.UID == 76524).ToList());

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-534);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
    .Returns(new List<TripAdvisorConfiguration>
    {
                    new TripAdvisorConfiguration
                    {
                        APIKey = "dwkqdqwodjwp",
                        IsCommentsActive = false,
                        IsTrackingCodeActive = false,
                        PropertyUid = 1806,
                        UID = 1
                    }
    });

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = "RES000024-1806/1"
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Modify
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        }

        [TestMethod]
        [TestCategory("Test_ChildrenAgesMissing_Empty")]
        public void Test_ChildrenAgesMissing_Empty()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(),
    _reservationsInDataBase.Where(x => x.UID == 76524).ToList());

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-534);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
    .Returns(new List<TripAdvisorConfiguration>
    {
                    new TripAdvisorConfiguration
                    {
                        APIKey = "dwkqdqwodjwp",
                        IsCommentsActive = false,
                        IsTrackingCodeActive = false,
                        PropertyUid = 1806,
                        UID = 1
                    }
    });

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>(),
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Modify
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        }

        [TestMethod]
        [TestCategory("Test_ChildrenAgesMissing_IncorrectNumber")]
        public void Test_ChildrenAgesMissing_IncorrectNumber()
        {
            #region BUILDERS
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 1 };
            var paymentTypes = new List<long>() { 1 };

            builder = builder.AddRate(string.Empty, null, null, false, 22645)
                .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4, null, 5901)
                .AddRateRoomsAll()
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 2,
                    ChildCount = 1,
                    CheckIn = DateTime.Now.AddDays(1).Date,
                    CheckOut = DateTime.Now.AddDays(5).Date
                })
                .AddPropertyChannels(channels)
                .AddRateChannelsAll(channels);

            builder.CreateRateRoomDetails();

            var resBuilder = new ReservationDataBuilder(1, 1806)
                            .WithNewGuest()
                            .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                                builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                                builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID, _reservationsInDataBase.First().Number)
                            .WithCancelationPolicy(false, 0, true, 1)
                            .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
                            .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
                            .WithBilledTypePayment(1);
            #endregion BUILDERS

            #region SETUP MOCK REPO
            RateRepoSetup(builder.InputData.Rates, _groupCodesInDataBase, _taxPolicyInDataBase);

            PropertyRepoSetup(builder.InputData.RoomTypes, _propertyLightInDataBase.Where(x => x.UID == 1806).ToList());

            _crmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<Contracts.Requests.ListGuestLightRequest>())).Returns(_guestInDataBase.Where(x => x.UID == 284630).ToList());

            ReservationRepoSetup(_reservationAdditionalData.Where(x => x.UID == 1).ToList(),
    _reservationsInDataBase.Where(x => x.UID == 76524).ToList());

            _visualStateRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.VisualState, bool>>>())).Returns(_visualStateInDataBase.AsQueryable());

            _childTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>())).Returns(new ListChildTermsResponse { Errors = null, Status = Status.Success, TotalRecords = 0, Result = _childTermsInDataBase.Where(x => x.UID == 1).ToList() });

            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>())).Returns(new OB.Domain.Reservations.GroupRule
            {
                RuleType = domainReservations.RuleType.BE,
                BusinessRules = domainReservations.BusinessRules.ValidateAllotment | domainReservations.BusinessRules.ValidateCancelationCosts | domainReservations.BusinessRules.ValidateGuarantee | domainReservations.BusinessRules.ValidateRestrictions
                | domainReservations.BusinessRules.HandleDepositPolicy | domainReservations.BusinessRules.HandleCancelationPolicy | domainReservations.BusinessRules.HandlePaymentGateway | domainReservations.BusinessRules.LoyaltyDiscount | domainReservations.BusinessRules.GenerateReservationNumber
            });

            _reservationAdditionalRepoMock.Setup(x => x.GetQuery()).Returns(_reservationAdditionalData.Where(x => x.Reservation_UID == _reservationsInDataBase.First().UID).AsQueryable());

            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<int>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(-534);
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(0.23283M);

            IncentivesRepoSetup(_incentivesInDataBase);

            OthersRepoSetup(_taxPolicyInDataBase, _otherPoliciesInDataBase.FirstOrDefault());

            ExtrasRepoSetup(_extrasInDataBase, _extrasBillingTypeInDataBase);

            ChannelsRepoSetup(new UpdateOperatorCreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true, CreditLimit = 300M }
            , new UpdateTPICreditUsedResponse { Status = Status.Success, SendCreditLimitExcededEmail = true, PaymentApproved = true }, _channelLightInDataBase);

            RateRoomDetailsRepoSetup(_rateRoomDetailResInDataBase, 1);

            _rateBuyerGroupsRepoMock.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>())).Returns(_rateBuyerGroupInDataBase.FirstOrDefault());

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethodTypes(It.IsAny<ListPaymentMethodTypesRequest>())).Returns(_paymentMethodTypesInDataBase);

            DeleteRepoSetup();

            _currencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(_currenciesInDataBase);

            PromotionalCodeRepoSetup(_promotionalCodesInDataBase);

            DepositPoliciesGuaranteeTypeRepoSetup(_depositPoliciesInDataBase);

            CancelationPoliciesRepoSetup(_cancellationPolicyInDataBase);

            _appSettingRepoMock.Setup(x => x.ListTripAdvisorConfiguration(It.IsAny<ListTripAdvisorConfigRequest>()))
    .Returns(new List<TripAdvisorConfiguration>
    {
                    new TripAdvisorConfiguration
                    {
                        APIKey = "dwkqdqwodjwp",
                        IsCommentsActive = false,
                        IsTrackingCodeActive = false,
                        PropertyUid = 1806,
                        UID = 1
                    }
    });

            #endregion SETUP MOCK REPO

            #region SETUP MOCK POCO
            _reservationManagerPOCOMock.Setup(
                    x => x.ListReservations(It.IsAny<Reservation.BL.Contracts.Requests.ListReservationRequest>()))
                .Returns(new Reservation.BL.Contracts.Responses.ListReservationResponse
                {
                    Errors = null,
                    Result = new List<contractsReservation.Reservation> { resBuilder.GetReservationsContract(resBuilder) },
                    TotalRecords = -1,
                    Status = Reservation.BL.Contracts.Responses.Status.Success
                });
            #endregion SETUP MOCK POCO

            var response = _reservationManagerPOCOMock.Object.ListReservations(new Reservation.BL.Contracts.Requests.ListReservationRequest { });

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
            {
                ReservationNumber = response.Result.First().Number,
                ReservationRooms = new List<contractsReservation.UpdateRoom>()
                    {
                        new contractsReservation.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>(){1 ,2,6},
                            DateFrom = DateTime.Now.AddDays(2).Date,
                            DateTo = DateTime.Now.AddDays(5).Date,
                            Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = Reservation.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Modify
            }, Reservation.BL.Constants.ReservationAction.Modify);

            ReservationManagerPOCO.WaitForAllBackgroundWorkers();

            var errors = modifyResponse.Errors;

            Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
            Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        }

        [TestMethod]
        [TestCategory("UpdateReservationCancelReason_Success")]
        public void UpdateReservationCancelReason_Success()
        {     
           var request = new Reservation.BL.Contracts.Requests.UpdateReservationCancelReasonRequest
           {
               CancelReservationComments = "After",
               CancelReservationReasonID = 2,
               ReservationId = 1
           };

            var reservationInDatabase = new List<domainReservations.Reservation>
            {
                new domainReservations.Reservation
                {
                    UID = 1,
                    CancelReservationComments = "Before",
                    CancelReservationReason_UID = 1
                }
            };

            var reservationHistoryInDatabase = new List<domainReservations.ReservationsHistory>
            {
                new domainReservations.ReservationsHistory
                {
                    Status = "2",
                    ReservationUID = 1
                }
            };

            _reservationRepoMock.Setup(x=> x.GetQuery(It.IsAny<Expression<Func<domainReservations.Reservation, bool>>>())).Returns(reservationInDatabase.AsQueryable());
            _reservationhistoryRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservations.ReservationsHistory, bool>>>())).Returns(reservationHistoryInDatabase.AsQueryable());

            var response = _ReservationManagerPOCOMock.UpdateReservationCancelReason(request);           
           
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, response.Status, "Expected status success");
            Assert.AreEqual(request.ReservationId, response.Result, "Expected result to be 1");
        }

        [TestMethod]
        [TestCategory("UpdateReservationCancelReason_Fail")]
        public void UpdateReservationCancelReason_Fail()
        {     
           var request = new Reservation.BL.Contracts.Requests.UpdateReservationCancelReasonRequest
           {
               CancelReservationComments = "After",
               CancelReservationReasonID = 2,
               ReservationId = 5
           };

            var reservationInDatabase = new List<domainReservations.Reservation>
            {                
            };

            _reservationRepoMock.Setup(x=> x.GetQuery(It.IsAny<Expression<Func<domainReservations.Reservation, bool>>>())).Returns(reservationInDatabase.AsQueryable());

            var response = _ReservationManagerPOCOMock.UpdateReservationCancelReason(request);           
           
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, response.Status, "Expected fail success");
            Assert.AreEqual(0, response.Result, "Expected result to be 0 (Meaning the update failed/ did not found a reservation)");
        }

        #region REPOSITORY SETUP
        private void RateRepoSetup(List<Rate> rates, List<GroupCode> groupCodes, List<TaxPolicy> taxPolicies)
        {
            _rateRepoMock.Setup(
                    x => x.ListRatesForReservation(It.IsAny<Contracts.Requests.ListRatesForReservationRequest>()))
                .Returns(rates); //_ratesInDataBase.Where(x => x.UID == 22645).ToList()

            _rateRepoMock.Setup(x => x.ListGroupCodesForReservation(It.IsAny<ListGroupCodesForReservationRequest>()))
                .Returns(groupCodes);

            _rateRepoMock.Setup(x => x.ListTaxPolicies(It.IsAny<ListTaxPoliciesRequest>())).Returns(taxPolicies);
        }

        private void PropertyRepoSetup(List<RoomType> roomTypes, List<PropertyLight> properties)
        {
            _propertyRepoMock.Setup(x => x.ListRoomTypes(It.IsAny<Contracts.Requests.ListRoomTypeRequest>())).Returns(roomTypes); //_roomTypeInDataBase.Where(x => x.UID == 5901).ToList()

            _propertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(properties);

        }

        private void ReservationRepoSetup(List<domainReservations.ReservationsAdditionalData> reservationAdditionalData, List<contractsReservation.Reservation> reservation)
        {

            _reservationRepoMock.Setup(x => x.FindReservationsAdditionalDataByReservationsUIDs(It.IsAny<List<long>>())).Returns(reservationAdditionalData);

            _reservationRepoMock.Setup(x => x.FindReservationTransactionStatusByReservationsUIDs(It.IsAny<List<long>>())).Returns(new List<Item> { new Item { ReservationUID = reservation.First().UID, TransactionUID = reservation.First().TransactionId } });

            _reservationRepoMock.Setup(x => x.FindByReservationNumberAndChannelUID(It.IsAny<string>(), It.IsAny<long>()))
               .Returns(BusinessObjectToDomainTypeConverter.Convert(reservation.FirstOrDefault(), new Reservation.BL.Contracts.Requests.ListReservationRequest
               {
                   IncludeRoomTypes = true,
                   IncludeCancelationCosts = true,
                   IncludeGuests = true,
                   IncludeRates = true,
                   IncludeReservationAddicionalData = true,
                   IncludeReservationRooms = true,
                   IncludeReservationRoomDetails = true,
                   IncludeReservationRoomDetailsAppliedIncentives = true,
                   IncludeReservationRoomChilds = true
               }));
        }

        private void IncentivesRepoSetup(List<Incentive> incentives)
        {
            _incentivesRepoMock.Setup(x => x.ListIncentivesForReservationRoom(It.IsAny<ListIncentivesForReservationRoomRequest>())).Returns(incentives);
            _incentivesRepoMock.Setup(x => x.ListIncentives(It.IsAny<ListIncentiveRequest>())).Returns(incentives);
        }

        private void OthersRepoSetup(List<TaxPolicy> taxPolicies, OtherPolicy otherpolicies)
        {
            _othersRepoMock.Setup(x => x.ListTaxPoliciesByRateIds(It.IsAny<ListTaxPoliciesByRateIdsRequest>())).Returns(_taxPolicyInDataBase);
            _othersRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<GetOtherPoliciesRequest>())).Returns(_otherPoliciesInDataBase.FirstOrDefault());
        }

        private void ExtrasRepoSetup(List<Extra> extras, List<ExtrasBillingType> extrasBillingType)
        {

            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<ListIncludedRateExtrasRequest>())).Returns(extras);

            _extrasRepoMock.Setup(x => x.ListExtras(It.IsAny<ListExtraRequest>())).Returns(extras);

            _extrasRepoMock.Setup(x => x.ListExtrasBillingTypesForReservation(It.IsAny<ListExtrasBillingTypesForReservationRequest>())).Returns(extrasBillingType);
        }

        private void ChannelsRepoSetup(UpdateOperatorCreditUsedResponse updateOperadorCredit, UpdateTPICreditUsedResponse updateTpiCredit, List<ChannelLight> channelLights)
        {
            _channelsRepoMock.Setup(x => x.UpdateOperatorCreditUsed(It.IsAny<UpdateOperatorCreditUsedRequest>())).Returns(updateOperadorCredit);
            _channelsRepoMock.Setup(x => x.UpdateTPICreditUsed(It.IsAny<UpdateTPICreditUsedRequest>())).Returns(updateTpiCredit);
            _channelsRepoMock.Setup(x => x.ListChannelLight(It.IsAny<ListChannelLightRequest>())).Returns(channelLights);
        }

        private void RateRoomDetailsRepoSetup(List<RateRoomDetailReservation> rateRoomDetailsRes, int updateRateRoomDetailResult)
        {
            _rateRoomDetailsForReservationRoomRepoMock.Setup(x => x.ListRateRoomDetailForReservationRoom(It.IsAny<ListRateRoomDetailForReservationRoomRequest>())).Returns(rateRoomDetailsRes);
            _rateRoomDetailsForReservationRoomRepoMock.Setup(x => x.UpdateRateRoomDetailAllotments(It.IsAny<UpdateRateRoomDetailAllotmentsRequest>())).Returns(updateRateRoomDetailResult);
        }

        private void PromotionalCodeRepoSetup(List<PromotionalCode> promotionalCode)
        {
            _promotionalCodeRepoMock.Setup(x => x.ListPromotionalCodeForReservation(It.IsAny<ListPromotionalCodeForReservationRequest>())).Returns(promotionalCode.FirstOrDefault());
            _promotionalCodeRepoMock.Setup(x => x.ListPromotionalCode(It.IsAny<ListPromotionalCodeRequest>())).Returns(promotionalCode);
        }

        private void DepositPoliciesGuaranteeTypeRepoSetup(List<DepositPolicy> depositPolicies)
        {
            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<ListDepositPoliciesRequest>())).Returns(depositPolicies);
            _depositPoliciesGuaranteeTypeRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<CalculateMostRestrictiveDepositPolicyRequest>())).Returns(depositPolicies.FirstOrDefault());
        }

        private void CancelationPoliciesRepoSetup(List<CancellationPolicy> cancellationPolicies)
        {
            _cancelationPolicyRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<ListCancellationPoliciesRequest>())).Returns(cancellationPolicies);
            _cancelationPolicyRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<CalculateMostRestrictiveCancellationPolicyRequest>())).Returns(cancellationPolicies.FirstOrDefault());
        }

        private void DeleteRepoSetup()
        {
            _reservationRoomDetailRepoMock.Setup(x => x.Delete(It.IsAny<domainReservations.ReservationRoomDetail>()))
                .Returns(new domainReservations.ReservationRoomDetail());

            _reservationDetailsAppliedIncentiveRepoMock.Setup(
                    x => x.Delete(It.IsAny<domainReservations.ReservationRoomDetailsAppliedIncentive>()))
                .Returns(new domainReservations.ReservationRoomDetailsAppliedIncentive());

            _resRoomAppliedPromotionalCodeRepoMock.Setup(
                    x => x.Delete(It.IsAny<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>()))
                .Returns(new domainReservations.ReservationRoomDetailsAppliedPromotionalCode());

            _reservationRoomTaxPolicyRepoMock.Setup(
                    x => x.Delete(It.IsAny<domainReservations.ReservationRoomTaxPolicy>()))
                .Returns(new domainReservations.ReservationRoomTaxPolicy());

            _reservationRoomChildRepoMock.Setup(x => x.Delete(It.IsAny<domainReservations.ReservationRoomChild>()))
                .Returns(new domainReservations.ReservationRoomChild());
        }

        #endregion REPOSITORY SETUP

        private void Send<T>(T msg)
        {

        }
    }
}
