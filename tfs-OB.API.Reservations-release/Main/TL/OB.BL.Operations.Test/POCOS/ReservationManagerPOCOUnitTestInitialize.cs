using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.Channels;
using OB.BL.Contracts.Data.CRM;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Internal.TypeConverters;
using OB.BL.Operations.Test.Domain;
using OB.BL.Operations.Test.Domain.CRM;
using OB.BL.Operations.Test.Domain.Rates;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.General;
using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.BrasPagGateway;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using contractsCRM = OB.BL.Contracts.Data.CRM;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservation = OB.Domain.Reservations;
using Microsoft.Practices.Unity.InterceptionExtension;
using OB.BL.Operations.Internal.Behaviors;
using PaymentGatewaysLibrary.PayU.Services.Interface;
using OB.DL.Common.Infrastructure;

namespace OB.BL.Operations.Test
{
    [TestClass]
    [System.Runtime.InteropServices.Guid("002B614E-8F9E-4005-9625-EEE46A0CB447")]
    public class ReservationManagerPOCOUnitTestInitialize : UnitBaseTest
    {
        protected const int BACKGROUND_THREAD_WAIT_TIME = 12000;
        protected const int BACKGROUND_THREAD_MAX_WAIT_TIME = 20000;

        protected Fixture fixture;

        protected IReservationManagerPOCO _reservationManagerPOCO;
        public IReservationManagerPOCO ReservationManagerPOCO
        {
            get
            {
                if (_reservationManagerPOCO == null)
                {
                    Container = Container.RegisterType<IReservationManagerPOCO, PartialMockReservationManagerPOCO>(
                                            new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                    _reservationManagerPOCO = this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();
                }

                return _reservationManagerPOCO;
            }
            set { _reservationManagerPOCO = value; }

        }

        //DB Mock
        protected List<contractsCRM.Guest> guestsList = null;
        protected List<domainReservation.Reservation> reservationsList = null;
        protected List<Contracts.Data.General.Setting> settingsList = null;
        protected List<domainReservation.ReservationFilter> reservationFilterList = null;
        protected List<BL.Contracts.Data.Channels.ChannelsProperty> chPropsList = null;
        protected List<BL.Contracts.Data.Payments.PaymentMethodType> paymentTypesList = null;
        protected List<Reservation.BL.Contracts.Data.General.Language> listLanguages = null;
        protected List<BL.Contracts.Data.Properties.RoomType> listRoomTypes = null;
        protected List<BL.Contracts.Data.Rates.Rate> listRates = null;
        protected List<BL.Contracts.Data.Channels.ChannelLight> listChannelsLight = null;
        protected List<Currency> listCurrencies = null;
        protected List<domainReservation.VisualState> listVisualStates = null;
        protected List<BL.Contracts.Data.Properties.PropertyLight> listPropertiesLigth = null;
        protected List<BL.Contracts.Data.Properties.ListProperty> listProperties = null;
        protected List<PaymentMethodType> _paymentMethodTypeInDataBase = new List<PaymentMethodType>();
        protected List<Domain.ChannelProperties> channelPropsList = null;
        protected List<RateRoomDetailReservation> rateRoomDetailReservationList = null;
        protected List<OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary> tpisList = null;
        protected List<OB.BL.Contracts.Data.Properties.Inventory> inventoryList = null;
        protected List<TPIProperty> tpiPropertyList = null;
        protected List<OB.BL.Contracts.Data.Rates.PromotionalCode> promocodesList = null;
        protected List<PaymentGatewayConfiguration> listPaymentGetwayConfigs = null;
        protected List<OB.Reservation.BL.Contracts.Data.CRM.GuestActivity> guestActivityList = null;
        protected List<OB.BL.Contracts.Data.BE.BESpecialRequest> beSpecRequestsList = null;
        protected List<BEPartialPaymentCCMethod> partialPaymentCCMethodList = null;
        protected List<ReservationPartialPaymentDetail> resPartialPaymentList = null;
        protected List<GroupCode> groupCodeList = null;
        protected List<TransferLocation> transferLocationList = null;
        protected List<CancellationPolicy> cancellationPolicyList = null;
        protected List<DepositPolicy> depositPolicyList = null;
        protected List<OtherPolicy> otherPolicyList = null;
        protected List<RateRoomDetail> rateRoomDetailList = null;
        protected List<CancellationPoliciesLanguage> cancellationPolicyLanguageList = null;
        protected List<DepositPoliciesLanguage> depositPolicyLanguageList = null;
        protected List<Domain.OtherPoliciesLanguage> otherPolicyLanguageList = null;
        protected List<Domain.CRM.Salesman> salesManList = null;
        protected List<SalesmanThirdPartyIntermediariesComission> salesmanTPICommissionsList = null;
        protected List<domainReservation.ReservationsAdditionalData> reservationAddicionaldataList = null;
        protected List<PaymentMethod> paymentMethodsList = null;
        protected List<OB.BL.Contracts.Data.Channels.Channel> channelsList = null;
        protected List<RatesChannel> ratesChannelList = null;
        protected List<RatesChannelsPaymentMethod> ratesChannelsPaymentMethodsList = null;
        protected List<BePartialPaymentCcMethod> bePartialPaymentCcMethodList = null;
        protected List<BL.Contracts.Data.ProactiveActions.ProactiveAction> proactiveActionsInDataBase = null;

        public PaymentMessageResult paymentResult = new PaymentMessageResult
        {
            IsTransactionValid = true,
            PaymentGatewayTransactionStatusCode = "1",
            PaymentGatewayName = "BrasPag",
            PaymentGatewayProcessorName = "TEST SIMULATOR",
            PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e",
            PaymentAmountCaptured = 0,
            PaymentGatewayTransactionType = "Auth",
            PaymentGatewayTransactionDateTime = new DateTime(2018, 7, 12),
            PaymentGatewayAutoGeneratedUID = "1",
            TransactionMessage = "1 - ",
            ErrorMessage = null,
            PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:",
            PaymentGatewayResponseXml = "{\"OrderId\":null,\"MerchantOrderId\":\"c7e74245-2b48-4a44-90f6-70209636778a\",\"Customer\":{\"Name\":\"customerName\",\"Identity\":null,\"IdentityType\":null,\"Email\":\"customerEmail\",\"PhoneAreaCode\":null,\"Phone\":null,\"Birthdate\":\"1986-02-05T00:00:00\",\"Address\":{\"Street\":null,\"Number\":null,\"Complement\":null,\"ZipCode\":null,\"City\":null,\"State\":null,\"Country\":null,\"District\":null},\"DeliveryAddress\":{\"Street\":null,\"Number\":null,\"Complement\":null,\"ZipCode\":null,\"City\":null,\"State\":null,\"Country\":null,\"District\":null,\"Comment\":null},\"IpAddress\":null,\"Status\":null,\"WorkPhone\":null,\"Mobile\":null,\"DocumentNumber\":null},\"Payment\":{\"ServiceTaxAmount\":0,\"Installments\":1,\"Interest\":0,\"Capture\":false,\"Authenticate\":true,\"Recurrent\":false,\"CreditCard\":{\"CardNumber\":\"000000******0001\",\"Holder\":\"cardname\",\"ExpirationDate\":\"01/2018\",\"SecurityCode\":null,\"SaveCard\":false,\"CardToken\":null,\"Alias\":null,\"PaymentToken\":null,\"Brand\":1},\"ProofOfSale\":\"024979\",\"AcquirerTransactionId\":\"0712110024979\",\"AuthorizationCode\":null,\"SoftDescriptor\":null,\"ReturnUrl\":\"http://www.omnibees.com\",\"Eci\":null,\"FraudAnalysis\":null,\"NewCard\":null,\"VelocityAnalysis\":null,\"PaymentId\":\"b3315347-5771-4a64-ab0b-54dc89b6cc4e\",\"Type\":\"CreditCard\",\"Amount\":0,\"ReceivedDate\":\"2018-07-12T11:00:23\",\"CapturedAmount\":null,\"CapturedDate\":null,\"VoidedAmount\":null,\"VoidedDate\":null,\"Currency\":\"BRL\",\"Country\":\"BRA\",\"Provider\":2,\"ExtraDataCollection\":null,\"ReasonCode\":9,\"ReasonMessage\":\"Waiting\",\"Status\":0,\"ProviderReturnCode\":\"1\",\"ProviderReturnMessage\":null,\"Links\":[{\"Method\":\"GET\",\"Rel\":\"self\",\"Href\":\"https://apiquerydev.braspag.com.br/v2/sales/b3315347-5771-4a64-ab0b-54dc89b6cc4e\"}]},\"HttpStatusCode\":null,\"Errors\":null}",
            PaymentGatewayRequestXml = "{\"OrderId\":null,\"MerchantOrderId\":\"c7e74245-2b48-4a44-90f6-70209636778a\",\"Customer\":{\"Name\":\"customerName\",\"Identity\":null,\"IdentityType\":null,\"Email\":\"customerEmail\",\"PhoneAreaCode\":null,\"Phone\":null,\"Birthdate\":\"1986-02-05T00:00:00\",\"Address\":{\"Street\":null,\"Number\":null,\"Complement\":null,\"ZipCode\":null,\"City\":null,\"State\":null,\"Country\":null,\"District\":null},\"DeliveryAddress\":{\"Street\":null,\"Number\":null,\"Complement\":null,\"ZipCode\":null,\"City\":null,\"State\":null,\"Country\":null,\"District\":null,\"Comment\":null},\"IpAddress\":null,\"Status\":null,\"WorkPhone\":null,\"Mobile\":null,\"DocumentNumber\":null},\"Payment\":{\"ServiceTaxAmount\":0,\"Installments\":1,\"Interest\":0,\"Capture\":false,\"Authenticate\":true,\"Recurrent\":false,\"CreditCard\":{\"CardNumber\":\"\",\"Holder\":\"cardname\",\"ExpirationDate\":\"\",\"SecurityCode\":\"\",\"SaveCard\":false,\"CardToken\":null,\"Alias\":null,\"PaymentToken\":null,\"Brand\":1},\"ProofOfSale\":null,\"AcquirerTransactionId\":null,\"AuthorizationCode\":null,\"SoftDescriptor\":null,\"ReturnUrl\":\"http://www.omnibees.com\",\"Eci\":null,\"FraudAnalysis\":null,\"NewCard\":null,\"VelocityAnalysis\":null,\"PaymentId\":null,\"Type\":\"CreditCard\",\"Amount\":0,\"ReceivedDate\":null,\"CapturedAmount\":null,\"CapturedDate\":null,\"VoidedAmount\":null,\"VoidedDate\":null,\"Currency\":\"BRL\",\"Country\":\"BRA\",\"Provider\":2,\"ExtraDataCollection\":null,\"ReasonCode\":null,\"ReasonMessage\":null,\"Status\":0,\"ProviderReturnCode\":null,\"ProviderReturnMessage\":null,\"Links\":null},\"HttpStatusCode\":null,\"Errors\":null}",
            TransactionEnvironment = "TEST",
            PaymentGatewayTransactionStatusDescription = "0 - Transaction Captured"
        };

        protected Mock<ISqlManager> _sqlManagerMock = null;

        // POCO Mock
        protected Mock<IReservationHelperPOCO> _reservationHelperMock = null;
        protected Mock<IEventSystemManagerPOCO> _eventSystemManagerPocoMock = null;
        protected Mock<IBrasPag> _brasPag = null;
        protected Mock<IPayUColombia> _payUPag = null;
        protected Mock<IPaymentGatewayFactory> _paymentGatewayFactory = null;

        // POCO Mock
        protected Mock<IOBAppSettingRepository> _appSettingRepoMock = null;
        protected Mock<IOBCancellationPolicyRepository> _cancellationRepoMock = null;
        protected Mock<IOBDepositPolicyRepository> _depositRepoMock = null;
        protected Mock<IOBOtherPolicyRepository> _otherRepoMock = null;
        protected Mock<IOBExtrasRepository> _extrasRepoMock = null;
        protected Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtra>> _reservationRoomsExtrasRepoMock;
        protected Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasAvailableDate>> _reservationRoomsExtrasDatesRepoMock;
        protected Mock<IRepository<OB.Domain.Reservations.ReservationRoomExtrasSchedule>> _reservationRoomsExtrasScheduleRepoMock;
        protected Mock<IOBIncentiveRepository> _incentivesRepoMock;
        protected Mock<IOBRateRoomDetailsForReservationRoomRepository> _rrdRepoMock;
        protected Mock<IOBPaymentMethodTypeRepository> _paymentMethodTypeRepoMock;
        protected Mock<IOBChannelRepository> _channelPropertiesRepoMock;
        protected Mock<IOBCRMRepository> _tpiRepoMock;
        protected Mock<IGroupRulesRepository> _groupRulesRepo;
        protected Mock<IOBRateBuyerGroupRepository> _buyerGroupRepo = null;
        protected Mock<IOBPromotionalCodeRepository> _promoCodesRepo = null;
        protected Mock<IReservationsRepository> _reservationsRepoMock = null;
        protected Mock<IReservationsFilterRepository> _reservationsFilterRepoMock = null;
        protected Mock<IOBPropertyRepository> _iOBPPropertyRepoMock = null;
        protected Mock<IExternalSystemsRepository> _iExternalSystemsRepositoryMock = null;
        protected Mock<IOBPMSRepository> _iOBPMSRepositoryMock = null;
        protected Mock<IOBPropertyEventsRepository> _propertyEventsRepoMock = null;
        protected Mock<IOBSecurityRepository> _obSecurityRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationRoom>> _reservationRoomRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationRoomDetail>> _resRoomDetailsRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationRoomChild>> _resRoomRoomChildRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationsAdditionalData>> _resAddDataRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationRoomDetailsAppliedIncentive>> _resRoomAppliedIncentiveRepoMock = null;
        protected Mock<IRepository<domainReservation.ReservationRoomTaxPolicy>> _resRoomTaxPolicyRepoMock = null;
        protected Mock<IOBRateRepository> _obRatesRepoMock = null;
        protected Mock<IVisualStateRepository> _visualStateRepoMock = null;
        protected Mock<IOBCurrencyRepository> _obCurrencyRepoMock = null;
        protected Mock<IOBChildTermsRepository> _obChildTermsRepoMock = null;
        protected Mock<IReservationRoomDetailRepository> _resRoomDetailsRepoSpecMock = null;
        protected Mock<IRepository<domainReservation.ReservationPaymentDetail>> _reservationPaymentDetailGenericRepoMock;
        protected Mock<IRepository<domainReservation.ReservationPartialPaymentDetail>> _reservationPartialPaymentDetailGenericRepoMock;
        protected Mock<IRepository<ReservationPartialPaymentDetail>> _resPartialPaymentDetailsRepo = null;
        protected Mock<IOBReservationLookupsRepository> _OBReservationLookupsRepoMock = null;
        protected Mock<IReservationHistoryRepository> _ReservationHistoryRepoMock = null;
        protected Mock<ICancelReservationReasonRepository> _CancelReservationReasonRepoMock = null;
        protected Mock<IOBBePartialPaymentCcMethodRepository> _OBBePartialPaymentCcMethodRepository = null;
        protected Mock<IOBBeSettingsRepository> _OBBeSettingsRepository = null;
        protected Mock<IPaymentGatewayTransactionRepository> _paymentGatewayRepoMock = null;

        protected bool ValidateReservationV2Called;
        protected Mock<ISessionFactory> sessionFactoryMock;
        protected Mock<IUnitOfWork> unitOfWorkMock;

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
            channelPropsList = new List<Domain.ChannelProperties>();
            reservationsList = new List<domainReservation.Reservation>();
            settingsList = new List<Contracts.Data.General.Setting>();
            reservationFilterList = new List<domainReservation.ReservationFilter>();
            guestsList = new List<contractsCRM.Guest>();
            chPropsList = new List<Contracts.Data.Channels.ChannelsProperty>();
            paymentTypesList = new List<PaymentMethodType>();
            listLanguages = new List<Language>();
            listRoomTypes = new List<RoomType>();
            listRates = new List<Rate>();
            listChannelsLight = new List<Contracts.Data.Channels.ChannelLight>();
            listCurrencies = new List<Currency>();
            listVisualStates = new List<domainReservation.VisualState>();
            listPropertiesLigth = new List<PropertyLight>();
            listProperties = new List<ListProperty>();
            _paymentMethodTypeInDataBase = new List<PaymentMethodType>();
            rateRoomDetailReservationList = new List<RateRoomDetailReservation>();
            tpisList = new List<OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary>();
            inventoryList = new List<Inventory>();
            tpiPropertyList = new List<TPIProperty>();
            promocodesList = new List<OB.BL.Contracts.Data.Rates.PromotionalCode>();
            listPaymentGetwayConfigs = new List<PaymentGatewayConfiguration>();
            guestActivityList = new List<Reservation.BL.Contracts.Data.CRM.GuestActivity>();
            beSpecRequestsList = new List<Contracts.Data.BE.BESpecialRequest>();
            partialPaymentCCMethodList = new List<BEPartialPaymentCCMethod>();
            resPartialPaymentList = new List<ReservationPartialPaymentDetail>();
            groupCodeList = new List<GroupCode>();
            transferLocationList = new List<TransferLocation>();
            cancellationPolicyList = new List<CancellationPolicy>();
            depositPolicyList = new List<DepositPolicy>();
            otherPolicyList = new List<OtherPolicy>();
            rateRoomDetailList = new List<RateRoomDetail>();
            cancellationPolicyLanguageList = new List<CancellationPoliciesLanguage>();
            depositPolicyLanguageList = new List<DepositPoliciesLanguage>();
            otherPolicyLanguageList = new List<Domain.OtherPoliciesLanguage>();
            salesManList = new List<Domain.CRM.Salesman>();
            salesmanTPICommissionsList = new List<SalesmanThirdPartyIntermediariesComission>();
            reservationAddicionaldataList = new List<ReservationsAdditionalData>();
            paymentMethodsList = new List<PaymentMethod>();
            channelsList = new List<Contracts.Data.Channels.Channel>();
            ratesChannelList = new List<RatesChannel>();
            ratesChannelsPaymentMethodsList = new List<RatesChannelsPaymentMethod>();
            bePartialPaymentCcMethodList = new List<BePartialPaymentCcMethod>();
            proactiveActionsInDataBase = new List<Contracts.Data.ProactiveActions.ProactiveAction>();

            // POCO Mock
            Container = Container.RegisterType<IReservationManagerPOCO, PartialMockReservationManagerPOCO>(
                new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();

            //SQLManager Mock
            _sqlManagerMock = new Mock<ISqlManager>();

            // Repo Mock
            _reservationHelperMock = new Mock<IReservationHelperPOCO>(MockBehavior.Default);
            _eventSystemManagerPocoMock = new Mock<IEventSystemManagerPOCO>(MockBehavior.Default);
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
            _resRoomDetailsRepoSpecMock = new Mock<IReservationRoomDetailRepository>(MockBehavior.Default);
            _resRoomTaxPolicyRepoMock = new Mock<IRepository<OB.Domain.Reservations.ReservationRoomTaxPolicy>>();
            _reservationPaymentDetailGenericRepoMock = new Mock<IRepository<ReservationPaymentDetail>>(MockBehavior.Default);
            _reservationPartialPaymentDetailGenericRepoMock = new Mock<IRepository<ReservationPartialPaymentDetail>>(MockBehavior.Default);
            _obSecurityRepoMock = new Mock<IOBSecurityRepository>(MockBehavior.Default);
            _resPartialPaymentDetailsRepo = new Mock<IRepository<ReservationPartialPaymentDetail>>(MockBehavior.Default);
            _OBReservationLookupsRepoMock = new Mock<IOBReservationLookupsRepository>(MockBehavior.Default);
            _ReservationHistoryRepoMock = new Mock<IReservationHistoryRepository>(MockBehavior.Default);
            _CancelReservationReasonRepoMock = new Mock<ICancelReservationReasonRepository>(MockBehavior.Default);
            _brasPag = new Mock<IBrasPag>(MockBehavior.Default);
            _payUPag = new Mock<IPayUColombia>(MockBehavior.Default);
            _paymentGatewayFactory = new Mock<IPaymentGatewayFactory>(MockBehavior.Default);
            _OBBePartialPaymentCcMethodRepository = new Mock<IOBBePartialPaymentCcMethodRepository>(MockBehavior.Default);
            _paymentGatewayRepoMock = new Mock<IPaymentGatewayTransactionRepository>(MockBehavior.Default);
            _OBBeSettingsRepository = new Mock<IOBBeSettingsRepository>(MockBehavior.Default);
            _iExternalSystemsRepositoryMock = new Mock<IExternalSystemsRepository>();
            _iOBPMSRepositoryMock = new Mock<IOBPMSRepository>();

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
            RepositoryFactoryMock.Setup(x => x.GetExternalSystemsRepository())
                            .Returns(_iExternalSystemsRepositoryMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPMSRepository())
                            .Returns(_iOBPMSRepositoryMock.Object);
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
            RepositoryFactoryMock.Setup(x => x.GetReservationRoomDetailRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomDetailsRepoSpecMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Reservations.ReservationRoomTaxPolicy>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resRoomTaxPolicyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationPaymentDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationPaymentDetailGenericRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<domainReservation.ReservationPartialPaymentDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationPartialPaymentDetailGenericRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBSecurityRepository())
                            .Returns(_obSecurityRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetRepository<ReservationPartialPaymentDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_resPartialPaymentDetailsRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBReservationLookupsRepository())
                           .Returns(_OBReservationLookupsRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationHistoryRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_ReservationHistoryRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetCancelReservationReasonRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_CancelReservationReasonRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBBePartialPaymentCcMethodRepository())
                            .Returns(_OBBePartialPaymentCcMethodRepository.Object);
            RepositoryFactoryMock.Setup(x => x.GetPaymentGatewayTransactionRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_paymentGatewayRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBBeSettingsRepository())
                            .Returns(_OBBeSettingsRepository.Object);

            //Mock SqlManager
            RepositoryFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), It.IsAny<DomainScope>()))
                .Returns(_sqlManagerMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<string>()))
                .Returns(_sqlManagerMock.Object);

            UnitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<System.Data.IsolationLevel>())).Returns(new Mock<IDbTransaction>().Object);
            unitOfWorkResMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<System.Data.IsolationLevel>())).Returns(new Mock<IDbTransaction>().Object);

            _reservationHelperMock.Setup(x => x.GetCancelationCosts(It.IsAny<contractsReservations.Reservation>(), It.IsAny<bool>(), It.IsAny<DateTime?>()))
                .Returns(new List<contractsReservations.ReservationRoomCancelationCost> { });

            _eventSystemManagerPocoMock.Setup(x => x.SendMessage(It.IsAny<OB.Events.Contracts.ICustomNotification>()));

            //Mock data
            FillListProperties();
            FillGroupRulesMock();
            FillGuestsMock();
            FillObAppSettingsMock();
            FillLanguagesMock();
            FillRoomTypesMock();
            FillCurrenciesMock();
            FillVisualStatesMock();
            FillPaymentMethodsMock();
            FillTpisMock();
            FillInventoriesMock();
            FillPaymentGetwayConfigsMock();
            FillGuestActivityMock();
            FillBESpecialRequestsMock();
            FillGroupCodesMock();
            FillTransferLocationList();
            FillCancellationPoliciesListMock();
            FillDepositPoliciesListMock();
            FillOtherPoliciesListMock();
            FillRateRoomDetailListMock();
            FillRatesMock();
            FillCancellationPoliciesLanguagesMock();
            FillDepositPoliciesLanguagesMock();
            FillSallesmanListMock();
            FillSalesmanTPICommissionsListMock();
            FillPaymentMethodsListMock();
            FillReservationsListMock();
            FillChannels();
            FillReservationsFilterMock();
            FillReservationAdditionalData();
            FillChPropsListMock();
            FillChannelPropertiesListMock();
            FillBePartialPaymentCcMethodListMock();
            FillProactiveActions();

            //Mock Repos
            MockGroupRulesRepo();
            MockObCrmRepo();
            MockInventoryRepo();
            MockGroupCodes();
            MockTransfersLocations();
            MockObRatesRepoQuery();
            MockDepositPolicyRepo();
            MockCancelationPolicyRepo();
            MockOtherPolicyRepo();
            MockObRatesRepoQuery();
            MockOBBePartialPaymentMethodsRepo();
            MockOBBeSettingsRepo();
            MockCancelReservationReasonRepo();
            MockObListPropertyRepo();

            ValidateReservationV2Called = false;

            MockReservationHelperPockMethods();

            this.Container = this.Container.RegisterInstance<IReservationHelperPOCO>(_reservationHelperMock.Object);
            this.Container = this.Container.RegisterInstance<IPaymentGatewayFactory>(_paymentGatewayFactory.Object);
            this.Container = this.Container.RegisterInstance<IEventSystemManagerPOCO>(_eventSystemManagerPocoMock.Object);
        }

        #region Mock Data

        protected void FillReservationAdditionalData()
        {
            reservationAddicionaldataList = new List<ReservationsAdditionalData>
            {
                new ReservationsAdditionalData
                {
                    Reservation_UID = 66242,
                    ReservationAdditionalDataJSON = @"{'ReservationRoomList':[{'ReservationRoom_UID':179595,'ReservationRoomNo':'635290249597293545/1','ChannelReservationRoomId':'TESTE179595','CancellationPolicy':{'UID':12151,'Name':'politica_cancelamento_5','Description':'0 dias 0€','Days':0,'IsCancellationAllowed':true,'CancellationCosts':true,'Value':0,'PaymentModel':1},'OtherPolicy':{'UID':0,'OtherPolicy_Name':null,'OtherPolicy_Description':null,'TranslatedName':null,'TranslatedDescription':null,'Property_UID':0,'IsDeleted':false},'TaxPolicies':[{'UID':0,'BillingType':'2,5','TaxId':713,'TaxName':'taxa_26','TaxDescription':'por estada / por pessoa','TaxDefaultValue':15,'TaxIsPercentage':false,'TaxCalculatedValue':15}],'ExternalSellingInformationByRule':[{'KeeperType':2,'ReservationRoomsTotalAmount':63,'ReservationRoomsPriceSum':48,'TotalTax':15,'PricesPerDay':[{'Date':'2017-04-04T00:00:00','Price':8},{'Date':'2017-04-05T00:00:00','Price':8},{'Date':'2017-04-06T00:00:00','Price':8},{'Date':'2017-04-07T00:00:00','Price':8},{'Date':'2017-04-08T00:00:00','Price':8},{'Date':'2017-04-09T00:00:00','Price':8}]}]},{'ReservationRoom_UID':179596,'ReservationRoomNo':'635290249597293545/2','CancellationPolicy':{'UID':12151,'Name':'politica_cancelamento_5','Description':'0 dias 0€','Days':0,'IsCancellationAllowed':true,'CancellationCosts':true,'Value':0,'PaymentModel':1},'OtherPolicy':{'UID':0,'OtherPolicy_Name':null,'OtherPolicy_Description':null,'TranslatedName':null,'TranslatedDescription':null,'Property_UID':0,'IsDeleted':false},'TaxPolicies':[{'UID':0,'BillingType':'2,5','TaxId':713,'TaxName':'taxa_26','TaxDescription':'por estada / por pessoa','TaxDefaultValue':15,'TaxIsPercentage':false,'TaxCalculatedValue':15}],'ExternalSellingInformationByRule':[{'KeeperType':2,'ReservationRoomsTotalAmount':519,'ReservationRoomsPriceSum':504,'TotalTax':15,'PricesPerDay':[{'Date':'2017-04-04T00:00:00','Price':84},{'Date':'2017-04-05T00:00:00','Price':84},{'Date':'2017-04-06T00:00:00','Price':84},{'Date':'2017-04-07T00:00:00','Price':84},{'Date':'2017-04-08T00:00:00','Price':84},{'Date':'2017-04-09T00:00:00','Price':84}]}]}],'ExternalSellingReservationInformationByRule':[{'KeeperType':2,'TotalAmount':582,'RoomsTotalAmount':582,'RoomsPriceSum':552,'TotalTax':30,'IsPaid':false,'TotalCommission':5,'CurrencyUID':16,'CurrencySymbol':null,'ExchangeRate':4.3393168771,'Markup':5,'MarkupType':1,'MarkupIsPercentage':true,'Commission':5,'CommissionType':1,'CommissionIsPercentage':true,'Tax':0,'TaxIsPercentage':false}]}",
                },
                new ReservationsAdditionalData
                {
                    Reservation_UID = 66056,
                    ReservationAdditionalDataJSON = @"{}",
                    BookerIsGenius = null,
                },
                new ReservationsAdditionalData
                {
                    Reservation_UID = 66057,
                    ReservationAdditionalDataJSON = @"{'BookerIsGenius':false}",
                    BookerIsGenius = false,
                },
                new ReservationsAdditionalData
                {
                    Reservation_UID = 67242,
                    ReservationAdditionalDataJSON = @"{'BookerIsGenius':true}",
                    BookerIsGenius = true,
                },
            };

        }

        protected void FillGuestsMock()
        {
            guestsList.Add(new contractsCRM.Guest()
            {
                UID = 284528,
                Prefix = 1,
                FirstName = "Donnie",
                LastName = "Donnie Burt",
                Address1 = "1870 Eigth Rd.",
                Address2 = "1870 Eigth Rd.",
                City = "Waterville",
                BillingCountry_UID = null,
                BillingAddress1 = "1870 Eigth Rd.",
                BillingAddress2 = "1870 Eigth Rd.",
                BillingCity = "Waterville",
                BillingPostalCode = "22761",
                BillingPhone = null,
                BillingExt = "52",
                BillingState = "Florida",
                BillingEmail = "Donnie.Farr@clobochem.net",
                BillingContactName = "Donnie Burt",
                BillingTaxCardNumber = "FL",
                Country_UID = 869,
                PostalCode = "22761",
                Phone = "(131) 131 - 1311",
                PhoneExt = string.Empty,
                UserName = "Donnie.Farr@clobochem.net",
                UserPassword = "MjkxMzky-bXD8hSk7dYQ=",
                PasswordHint = string.Empty,
                GuestCategory_UID = 1,
                Property_UID = 1635,
                Currency_UID = 86,
                Language_UID = 6,
                Email = "Donnie.Farr@clobochem.net",
                Birthday = new DateTime(1992, 02, 26, 15, 22, 39),
                CreatedByTPI_UID = null,
                IsActive = true,
                LastLoginDate = null,
                AllowMarketing = true,
                Client_UID = 646,
                CreateDate = new DateTime(2014, 02, 26, 15, 22, 39),
                CreateBy = 70,
                State = "Florida",
                IDCardNumber = "1767011820",
                Gender = "M"
            });
            guestsList.Add(new contractsCRM.Guest()
            {
                UID = 63774,
                Prefix = 1,
                FirstName = "Anne-Birgitte",
                LastName = " Ritz",
                Address1 = "",
                Address2 = "",
                BillingCountry_UID = null,
                Country_UID = null,
                Phone = string.Empty,
                PhoneExt = string.Empty,
                UserPassword = "ODI0NDgw-BIemoiW3srE=",
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
                CreateDate = new DateTime(2013, 05, 16)
            });
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
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
            guestsList.Add(new contractsCRM.Guest()
            {
                UID = 38175,
                Prefix = 1,
                FirstName = "Heike",
                LastName = "Röck",
                Address1 = string.Empty,
                Address2 = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                BillingAddress1 = string.Empty,
                BillingAddress2 = string.Empty,
                BillingCity = string.Empty,
                BillingPostalCode = string.Empty,
                BillingPhone = string.Empty,
                BillingExt = string.Empty,
                BillingCountry_UID = null,
                Country_UID = null,
                Phone = string.Empty,
                UserName = null,
                GuestCategory_UID = 1,
                Property_UID = 1263,
                Currency_UID = 34,
                Language_UID = 1,
                Email = string.Empty,
                Birthday = null,
                CreatedByTPI_UID = null,
                CreateDate = new DateTime(2013, 03, 06),
                AllowMarketing = true,
                BillingState_UID = null,
                UseDifferentBillingInfo = false,
                IsImportedFromExcel = false
            });
            guestsList.Add(new contractsCRM.Guest()
            {
                UID = 66242,
                Prefix = 1,
                FirstName = "Heike",
                LastName = "Röck",
                Address1 = string.Empty,
                Address2 = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                BillingAddress1 = string.Empty,
                BillingAddress2 = string.Empty,
                BillingCity = string.Empty,
                BillingPostalCode = string.Empty,
                BillingPhone = string.Empty,
                BillingExt = string.Empty,
                BillingCountry_UID = null,
                Country_UID = null,
                Phone = string.Empty,
                UserName = null,
                GuestCategory_UID = 1,
                Property_UID = 1263,
                Currency_UID = 34,
                Language_UID = 1,
                Email = string.Empty,
                Birthday = null,
                CreatedByTPI_UID = null,
                CreateDate = new DateTime(2013, 03, 06),
                AllowMarketing = true,
                BillingState_UID = null,
                UseDifferentBillingInfo = false,
                IsImportedFromExcel = false
            });
            guestsList.Add(new contractsCRM.Guest()
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                Property_UID = 1806,
                Client_UID = 646,
                Language_UID = 4
            });
        }

        protected void FillThePaymentMethodTypeforTest()
        {
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
                         UID = 2,
                         PaymentType = 2,
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

        protected void FillObAppSettingsMock()
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
                UID = 24,
                Name = "BPagEncryptionKey",
                Value = "<RSAKeyValue><Modulus>yQlU7z6szmf+vz7R6nIaH+oyW111dx/MAnsItBCAE+VtkAfJpT0p8aZ3/07lNJiIJXSTZrsMGrcwoELKpSI7HalyebiL7qhEkcbbprR+zXT5ySmeUcZk+ZrwRzdzNExc7ut3kSsgfMYj60G3WBcnT1AI2G/IMk4YO+jihRKTB48=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
                Category = "Reservations"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 34,
                Name = "ListReservationsMaxDaysWithoutFilters",
                Value = "1",
                Category = "OB.REST"
            });
            settingsList.Add(new Contracts.Data.General.Setting
            {
                UID = 35,
                Name = "ValidateReservationPricesForBE_Properties",
                Value = "all",
                Category = "OB.REST"
            });
        }

        protected void FillReservationsFilterMock()
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
            reservationFilterList.Add(new domainReservation.ReservationFilter
            {
                UID = 66242,
                Number = "635290249598293659",
                ChannelUid = 32
            });
        }

        protected void FillChPropsListMock()
        {
            chPropsList.Add(
                new BL.Contracts.Data.Channels.ChannelsProperty()
                {
                    UID = 1,
                    Channel_UID = 1,
                    Property_UID = 1263,
                    IsActive = true,
                    IsActivePrePaymentCredit = true,
                    PrePaymentCreditLimit = 100000,
                    PrePaymentCreditUsed = 0,
                    IsDeleted = false
                }
            );
            chPropsList.Add(
                new BL.Contracts.Data.Channels.ChannelsProperty()
                {
                    UID = 3631,
                    Channel_UID = 1,
                    Property_UID = 1635,
                    IsActive = true,
                    RateModel_UID = null,
                    Value = 0.00M,
                    IsPercentage = true,
                    ContractPdf = null,
                    IsDeleted = false,
                    IsPendingRequest = false,
                    ChannelCommissionCategory_UID = null,
                    IncomingOfficeCode = null,
                    Sequence = null,
                    HotelId = null,
                    ChainId = null,
                    OperatorBillingType = null,
                    OperatorCreditLimit = 10.00M,
                    OperatorCreditUsed = 360.00M,
                    IsOperatorsCreditLimit = false,
                    Commission = null,
                    Markup = null,
                    Package = null,
                    PrePaymentCreditLimit = null,
                    PrePaymentCreditUsed = null,
                    IsActivePrePaymentCredit = false,
                }
            );

            chPropsList.Add(
                new BL.Contracts.Data.Channels.ChannelsProperty()
                {
                    UID = 3631,
                    Channel_UID = 80,
                    Property_UID = 1635,
                    IsActive = true,
                    RateModel_UID = null,
                    Value = 0.00M,
                    IsPercentage = true,
                    ContractPdf = null,
                    IsDeleted = false,
                    IsPendingRequest = false,
                    ChannelCommissionCategory_UID = null,
                    IncomingOfficeCode = null,
                    Sequence = null,
                    HotelId = null,
                    ChainId = null,
                    OperatorBillingType = null,
                    OperatorCreditLimit = 10.00M,
                    OperatorCreditUsed = 360.00M,
                    IsOperatorsCreditLimit = false,
                    Commission = null,
                    Markup = null,
                    Package = null,
                    PrePaymentCreditLimit = null,
                    PrePaymentCreditUsed = null,
                    IsActivePrePaymentCredit = false,
                }
            );
        }

        protected void FillPaymentMethodsMock()
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

        protected void FillLanguagesMock()
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

        protected void FillRoomTypesMock()
        {
            listRoomTypes.Add(new BL.Contracts.Data.Properties.RoomType()
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
            listRoomTypes.Add(new BL.Contracts.Data.Properties.RoomType()
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
            listRoomTypes.Add(new BL.Contracts.Data.Properties.RoomType()
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
            listRoomTypes.Add(new BL.Contracts.Data.Properties.RoomType()
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

        protected void FillRatesMock()
        {
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 9175,
                Name = "General Rate",
                IsPriceDerived = true,
                Value = 5.00M,
                IsPercentage = true,
                IsValueDecrease = false,
                IsYielding = false,
                IsAvailableToTPI = false,
                DepositPolicy_UID = null,
                CancellationPolicy_UID = null,
                OtherPolicy_UID = null,
                Property_UID = 1635,
                IsParity = false,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                GDSSabreRateName = null,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 8017,
                AvailabilityType = 1
            });
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 12388,
                Name = "teste politica",
                IsPriceDerived = false,
                Value = null,
                IsPercentage = false,
                IsValueDecrease = false,
                IsYielding = false,
                IsAvailableToTPI = false,
                DepositPolicy_UID = 813,
                CancellationPolicy_UID = 1989,
                OtherPolicy_UID = null,
                Property_UID = 1635,
                IsParity = false,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                GDSSabreRateName = null,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 8017,
                AvailabilityType = 1
            });
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 4639,
                Name = "EB Long Stay Discount Summer",
                IsPriceDerived = true,
                Value = 10,
                IsPercentage = true,
                IsValueDecrease = true,
                IsYielding = false,
                IsAvailableToTPI = false,
                DepositPolicy_UID = null,
                CancellationPolicy_UID = null,
                OtherPolicy_UID = null,
                Property_UID = 1263,
                IsParity = false,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                GDSSabreRateName = null,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = rateUidMock
            });
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 12372,
                Name = "teste ao extra com o All por defeito",
                IsPriceDerived = true,
                Value = 10,
                IsPercentage = true,
                IsValueDecrease = true,
                IsYielding = false,
                IsAvailableToTPI = false,
                DepositPolicy_UID = null,
                CancellationPolicy_UID = null,
                OtherPolicy_UID = null,
                Property_UID = 1635,
                IsParity = false,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                GDSSabreRateName = null,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = rateUidMock
            });
            listRates.Add(new BL.Contracts.Data.Rates.Rate()
            {
                UID = 12431,
                Name = "PM derivada derivada 1",
                IsPriceDerived = true,
                Value = 10,
                IsPercentage = true,
                IsValueDecrease = true,
                IsYielding = false,
                IsAvailableToTPI = false,
                DepositPolicy_UID = null,
                CancellationPolicy_UID = null,
                OtherPolicy_UID = null,
                Property_UID = 1635,
                IsParity = false,
                BeginSale = null,
                EndSale = null,
                Description = null,
                PriceModel = true,
                GDSSabreRateName = null,
                IsAllExtrasIncluded = true,
                Currency_UID = 34,
                CurrencyISO = "€",
                Rate_UID = 12430
            });
        }

        protected void FillChannelsLightMock()
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

        protected void FillCurrenciesMock()
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

        protected void FillVisualStatesMock()
        {

        }

        protected void FillPropertiesLigth()
        {
            listPropertiesLigth.Add(new BL.Contracts.Data.Properties.PropertyLight()
            {
                UID = 1263,
                Name = "Pestana Viking Beach & Spa Resort_",
                BaseCurrency_UID = 34,
                CurrencyISO = "€"
            });
            listPropertiesLigth.Add(new BL.Contracts.Data.Properties.PropertyLight()
            {
                UID = 1635,
                Name = "_Hotel Newcity Demo FL",
                BaseCurrency_UID = 34,
                CurrencyISO = "€"
            });
        }

        protected void FillListProperties()
        {
            listProperties.Add(new BL.Contracts.Data.Properties.ListProperty()
            {
                Id = 1,
                IsActive = true,
                IsToPreCheckAvail = true,
                CreatedDate = DateTime.Today,
            });
            listProperties.Add(new BL.Contracts.Data.Properties.ListProperty()
            {
                Id = 1806,
                PropertySettings = new PropertySettings { Name = "Property test" }
            });
            listProperties.Add(new BL.Contracts.Data.Properties.ListProperty()
            {
                Id = 1263,
                PropertySettings = new PropertySettings { Name = "Pestana Viking Beach & Spa Resort_" }
            });
        }

        protected void FillProactiveActions()
        {
            proactiveActionsInDataBase.Add(new Contracts.Data.ProactiveActions.ProactiveAction
            {
                Emails = new List<string>
                {
                    "hotel1@omnibees.com",
                    "hotel2@omnibees.com",
                    "hotel3@omnibees.com"
                },
                IsDeleted = false
            });
        }

        protected void FillTpisMock()
        {
            tpisList.Add(new OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary()
            {
                UID = 61,
                Name = "Chuck Norris",
                UserName = "testetravelaa",
                UserPassword = "MTIzNDU2-P+isT6EczlI=",
                Country_UID = 157,
                Language_UID = 4,
                Currency_UID = 34,
                Address1 = "rtertert",
                Address2 = null,
                City = "Faro",
                PostalCode = "8000",
                Phone = "289111111",
                MobilePhone = string.Empty,
                Fax = null,
                Email = "ricardo.rodrigues@omnibees.com",
                IATA = "IATA",
                TPICategory_UID = 44,
                CreateDate = new DateTime(2013, 06, 21, 15, 52, 57),
                IsDeleted = false,
                Property_UID = 1635,
                AllowsMarketing = true,
                IsCompany = false,
                CreateBy = 70,
                ModifyBy = 70,
                ModifyDate = new DateTime(2013, 10, 24, 16, 49, 06),
                Question_UID = null,
                CommercialName = "Comercial Name",
                VATNumber = "444444444",
                WebSite = string.Empty,
                State = "Faro",
                IsActive = true,
                DirectContactName = "Direct Name",
                DirectContactPhone = "Direct Phone",
                DirectContactMobile = "Direct Mobile",
                PromotionalCode_UID = null,
                Company_UID = null,
                CompanyCode = null,
                PseudoCityCode = null,
                TravelAgent_UID = null
            });
        }

        protected void FillInventoriesMock()
        {
            inventoryList.Add(new Inventory()
            {
                Date = DateTime.UtcNow.AddDays(30),
                QtyRoomOccupied = 10,
                RoomType_UID = 3709,
                UID = 1
            });
        }

        protected void FillPaymentGetwayConfigsMock()
        {
            listPaymentGetwayConfigs.Add(new PaymentGatewayConfiguration()
            {
                UID = 17,
                PropertyUID = 1075,
                GatewayUID = 1,
                GatewayCode = "100",
                ProcessorCode = 1,
                ProcessorName = "TEST SIMULATOR",
                MerchantID = "246",
                MerchantKey = "7o94laxu8oqhni51q1nhxpkh",
                IsActive = true,
                Comission = null,
                CreatedBy = 1105,
                CreatedDate = new DateTime(2013, 03, 12, 20, 26, 54),
                ModifiedBy = 1105,
                ModifiedDate = new DateTime(2013, 03, 12, 20, 29, 30),
                DefaultAuthorizationOnly = true,
                DefaultAuthorizationSale = null,
                ApiSignatureKey = null,
                MerchantAccount = null,
                PaymentAuthorizationTypeId = null,
                IsAntiFraudeControlEnable = false
            });
        }

        protected void FillGuestActivityMock()
        {
            guestActivityList.Add(new Reservation.BL.Contracts.Data.CRM.GuestActivity()
            {
                UID = 2451,
                Activity_UID = 2451,
                Guest_UID = 1
            });
        }

        protected void FillBESpecialRequestsMock()
        {
            beSpecRequestsList.Add(new Contracts.Data.BE.BESpecialRequest()
            {
                UID = 1,
                Property_UID = 1325,
                Name = "SPEC REQUEST 1",
                IsDeleted = false
            });
            beSpecRequestsList.Add(new Contracts.Data.BE.BESpecialRequest()
            {
                UID = 2,
                Property_UID = 1325,
                Name = "SPEC REQUEST 2",
                IsDeleted = false
            });
            beSpecRequestsList.Add(new Contracts.Data.BE.BESpecialRequest()
            {
                UID = 3,
                Property_UID = 1325,
                Name = "SPEC REQUEST 3",
                IsDeleted = false
            });
            beSpecRequestsList.Add(new Contracts.Data.BE.BESpecialRequest()
            {
                UID = 4,
                Property_UID = 1325,
                Name = "SPEC REQUEST 4",
                IsDeleted = false
            });
        }

        protected void FillGroupCodesMock()
        {
            groupCodeList.Add(new GroupCode()
            {
                BeginSell = new DateTime(2013, 10, 01),
                DateFrom = new DateTime(2013, 10, 01),
                DateTo = new DateTime(2014, 03, 28),
                Description = null,
                EndSell = new DateTime(2014, 03, 28),
                GroupCode1 = "47811",
                InternalCode = "47811",
                IsActive = true,
                IsDeleted = false,
                ModifiedDate = null,
                Name = "Novo codigo grupo",
                Property_UID = 1635,
                Rate_UID = null,
                UID = 79
            });
        }

        protected void FillTransferLocationList()
        {
            transferLocationList.Add(new TransferLocation()
            {
                Address = "Rua X, nº1",
                City = "Faro",
                IsDeleted = false,
                Name = "Transfer location number #1",
                Price = 10,
                Property_UID = 1635,
                UID = 1
            });
        }

        protected void FillCancellationPoliciesListMock()
        {
            cancellationPolicyList.Add(new CancellationPolicy()
            {
                CancellationCosts = true,
                CreatedDate = new DateTime(2012, 07, 18, 18, 58, 04),
                Days = 0,
                Description = "Se cancelado 24 horas antes da data de chegada, nao sera cobrado nenhum valor.",
                IsCancellationAllowed = true,
                IsDeleted = false,
                ModifiedDate = new DateTime(2014, 04, 10, 10, 55, 32),
                Name = "24h PT",
                NrNights = 1,
                PaymentModel = 1,
                Property_UID = 1635,
                RateUID = rateUidMock,
                //SortOrder = ,
                TranslatedDescription = null,
                TranslatedName = null,
                UID = 1783,
                Value = 11.00M,
            });
        }

        protected void FillDepositPoliciesListMock()
        {
            depositPolicyList.Add(new DepositPolicy()
            {
                CreatedDate = new DateTime(2012, 07, 18, 18, 59, 04),
                Days = 0,
                DepositCosts = true,
                Description = "Nao e necessario qualquer deposito",
                IsDeleted = false,
                IsDepositCostsAllowed = false,
                ModifiedDate = null,
                Name = "sem deposito",
                NrNights = null,
                PaymentModel = null,
                Percentage = null,
                Property_UID = 1635,
                RateUID = rateUidMock,
                //SortOrder = ,
                TranslatedDescription = null,
                TranslatedName = null,
                UID = 813,
                Value = null
            });
        }

        protected void FillOtherPoliciesListMock()
        {
            otherPolicyList.Add(new OtherPolicy()
            {
                CreatedDate = new DateTime(2012, 07, 18, 18, 57, 21),
                IsDeleted = false,
                //IsSelected = ,
                Language = null,
                ModifiedDate = new DateTime(2014, 01, 24, 09, 38, 46),
                OtherPolicyCategory_UID = null,
                OtherPolicy_Description = "polticas gerais do hotel",
                OtherPolicy_Name = "Geral",
                Property_UID = 1635,
                TranslatedDescription = null,
                TranslatedName = null,
                UID = 963
            });
        }

        protected void FillRateRoomDetailListMock()
        {
            rateRoomDetailList.Add(new RateRoomDetail()
            {
                UID = 5137349,
                Date = new DateTime(2014, 07, 24),
                Allotment = 100,
                RateRoom_UID = 16650,
                DepositPolicy_UID = null,
                CancellationPolicy_UID = null,
                CreatedDate = new DateTime(2013, 08, 01, 17, 44, 38),
                CreateBy = 1379,
                ModifyBy = 1379,
                ModifyDate = new DateTime(2013, 08, 01, 18, 06, 22),
                AllotmentUsed = 0,
                ExtraBedPrice = 0.00M,
                correlationID = "d8d3b8ef-8286-4e0a-83cf-5df9c406584a",
                Adult_1 = 180.00M,
                Adult_2 = 180.00M,
                BlockedChannelsListUID = string.Empty,
                isBookingEngineBlocked = false,
                ChannelsListUID = string.Empty
            });
        }

        protected void FillChannelPropertiesListMock()
        {
            channelPropsList.Add(new ChannelProperties
            {
                Property_UID = 1635,
                Channel_UID = 80,
                IsOperatorsCreditLimit = false,
                OperatorCreditLimit = 1000,
                OperatorCreditUsed = 3000,
                IsActivePrePaymentCredit = true,
                PrePaymentCreditLimit = 1200,
                PrePaymentCreditUsed = 2366
            });
        }

        protected void FillCancellationPoliciesLanguagesMock()
        {
            cancellationPolicyLanguageList.Add(new CancellationPoliciesLanguage()
            {
                UID = 1662,
                Name = "Si cancela 24 horas antes de la fecha de llegada, no se le cobrará ningún valor.",
                Description = "Si cancela 24 horas antes de la fecha de llegada, no se le cobrará ningún valor.",
                CancellationPolicies_UID = 1783,
                Language_UID = 3,
                CreatedDate = new DateTime(2014, 01, 14, 09, 48, 20),
                ModifiedDate = new DateTime(2014, 01, 14, 09, 48, 20)
            });
        }

        protected void FillDepositPoliciesLanguagesMock()
        {
            depositPolicyLanguageList.Add(new DepositPoliciesLanguage()
            {
                UID = 123456,
                Name = "Teste nombre",
                Description = "Teste descripción",
                Language_UID = 3,
                CreatedDate = new DateTime(2014, 01, 14, 09, 48, 20),
                ModifiedDate = new DateTime(2014, 01, 14, 09, 48, 20),
                DepositPolicy_UID = 813  //was 4639
            });
        }

        protected void FillSallesmanListMock()
        {
            salesManList.Add(new Domain.CRM.Salesman()
            {
                UID = 4,
                Prefix_UID = 1,
                Property_UID = 1635,
                FirstName = "Francisco",
                LastName = "Loule",
                Email = "francisco.loule@omnibees.com",
                Birthday = new DateTime(1978, 10, 24),
                Phone = null,
                Address1 = null,
                Address2 = null,
                PostalCode = null,
                Country_UID = null,
                State_UID = null,
                City_UID = null,
                Status = 1,
                IsDeleted = false,
                CreatedBy = 70,
                CreatedDate = new DateTime(2013, 05, 18, 16, 21, 49),
                ModifiedBy = 70,
                ModifiedDate = new DateTime(2013, 05, 18, 16, 21, 49)
            });
        }

        protected void FillSalesmanTPICommissionsListMock()
        {
            salesmanTPICommissionsList.Add(new SalesmanThirdPartyIntermediariesComission()
            {
                UID = 2,
                SalesmanUID = 4,
                ThirdPartyIntermediariesUID = 46,
                SalesmanIsBaseCommissionPercentage = false,
                SalesmanBaseCommission = 5.00M,
                CreatedBy = 70,
                CreatedDate = new DateTime(2013, 05, 18, 16, 22, 40),
                ModifiedBy = 70,
                ModifiedDate = new DateTime(2013, 05, 18, 16, 22, 40)
            });
        }

        protected void FillPaymentMethodsListMock()
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

        protected void FillReservationsListMock()
        {
            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 40000,
                Guest_UID = 38175,  //verify if exists
                Number = "Orbitz-CJ1SVI",
                Channel_UID = 28,
                Date = new DateTime(2013, 03, 06, 15, 35, 03),
                TotalAmount = 733.0400M,
                Adults = 1,
                Children = 1,
                Status = 1,
                Notes = "\n\tSpecial Request: \"Non-smoking\"\n\n\tPromotion Type:\"Value Add\"; Name: \"FREE use of the Spa\"; Hotel Message: \"We offer the FREE use of the Hotel's Spa(except Spa Treatments).\"\n\n\tPromotion Type:\"CreatePercentOffDiscount\"; Name: \"NonRef+Long Stay\"; Hotel Message: \"Book 90 days in advance & Save 23%\"BookType: Land\"",
                IPAddress = "127.0.0.1",
                TPI_UID = null,
                PromotionalCode_UID = null,
                Property_UID = 1263,
                CreatedDate = new DateTime(2013, 03, 06, 21, 58, 20),
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

                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>() {
                    new ReservationPartialPaymentDetail()
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
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 36612,
                        PaymentMethod_UID = 3,
                        Reservation_UID = 40000,
                        Amount = 733.0400M,
                        Currency_UID = 34,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2013,04,28),
                        CreatedDate = new DateTime(2013,03,06,21,58,21),
                        ModifiedDate = new DateTime(2013,03,06,21,58,20),
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    new ReservationRoom()
                    {
                        UID = 43588,
                        Reservation_UID = 40000,
                        RoomType_UID = 3709,
                        GuestName = "Heike Röck",
                        SmokingPreferences = false,
                        DateFrom = new DateTime(2013,07,28),
                        DateTo = new DateTime(2013,08,04),
                        AdultCount = 1,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "Orbitz-CJ1SVI/1",
                        Status = 1,
                        CreatedDate = new DateTime(2013,03,06,21,58,20),
                        ModifiedDate = null,
                        RoomName = "Twin Classic Garden View ",
                        Rate_UID = null,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 691.5300M,
                        ReservationRoomsExtrasSum = null,
                        ArrivalTime = null,
                        ReservationRoomsTotalAmount = 691.5300M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() { },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            },
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>()
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66242,
                Guest_UID = 284528,  //verify if exists
                Number = "635290249597293545",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 1061.0000M,
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


                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66242,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
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
                        ReservationRoomNo = "635290249597293545/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66242,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "635290249597293545/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                                                        new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            //reservation with wrong occupancy but are well in rooms 
            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66243,
                Guest_UID = 284528,  //verify if exists
                Number = "6352902495972935451",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 1061.0000M,
                Adults = 1,
                Children = 1,
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


                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66243,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66243,
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
                        ReservationRoomNo = "6352902495972935451/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66243,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935451/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                                                        new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 3
                    new ReservationRoom()
                    {
                        UID = 73671,
                        Reservation_UID = 66243,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 1,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935451/3",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73671,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
                            {
                                UID = 192936,
                                RateRoomDetails_UID = null,
                                Price = 341.0000M,
                                ReservationRoom_UID = 73671,
                                AdultPrice = 283.0000M,
                                ChildPrice = 58.0000M,
                                CreatedDate = new DateTime(2014,02,26,15,22,40),
                                ModifiedDate = null,
                                Date = new DateTime(2014,02,27),
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73671,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });
            //reservation without occupancy in one room
            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66244,
                Guest_UID = 284528,  //verify if exists
                Number = "6352902495972935452",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 1061.0000M,
                Adults = 1,
                Children = 1,
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


                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66244,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66244,
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
                        ReservationRoomNo = "6352902495972935452/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66244,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935452/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                                                        new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 3
                    new ReservationRoom()
                    {
                        UID = 73671,
                        Reservation_UID = 66244,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935452/3",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73671,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
                            {
                                UID = 192936,
                                RateRoomDetails_UID = null,
                                Price = 341.0000M,
                                ReservationRoom_UID = 73671,
                                AdultPrice = 283.0000M,
                                ChildPrice = 58.0000M,
                                CreatedDate = new DateTime(2014,02,26,15,22,40),
                                ModifiedDate = null,
                                Date = new DateTime(2014,02,27),
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73671,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });
            //reservation without children occupancy
            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66245,
                Guest_UID = 284528,  //verify if exists
                Number = "6352902495972935453",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 1061.0000M,
                Adults = 1,
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


                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66245,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66245,
                        RoomType_UID = 5827,
                        GuestName = "Harlen Rupert",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 3,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935453/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66245,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        AdultCount = 2,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935453/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                                                        new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 3
                    new ReservationRoom()
                    {
                        UID = 73671,
                        Reservation_UID = 66245,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39).Date,
                        DateTo = new DateTime(2014,02,28,15,22,39).Date,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = 0,
                        ReservationRoomNo = "6352902495972935453/3",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73671,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
                            {
                                UID = 192936,
                                RateRoomDetails_UID = null,
                                Price = 341.0000M,
                                ReservationRoom_UID = 73671,
                                AdultPrice = 283.0000M,
                                ChildPrice = 58.0000M,
                                CreatedDate = new DateTime(2014,02,26,15,22,40),
                                ModifiedDate = null,
                                Date = new DateTime(2014,02,27),
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73671,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            //TODO: WE ARE ADDING THIS RESERVATION TO THE TEST TestCancelReservation_CreditOperator
            //WE WILL ALSO NEED TO ADD THE RATE, ROOMTYPE, RATE AND OTHER POSSIBLE ASSOCIATED OBJETCS TO THIS RATE
            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66056,
                Guest_UID = 284443,  //verify if exists
                Number = "635290249597293559",
                Channel_UID = 80,
                Date = new DateTime(2014, 02, 03, 15, 03, 58),
                TotalAmount = 2163.0000M,
                Adults = 4,
                Children = 2,
                Status = 2,
                Notes = string.Empty,
                IPAddress = null,
                TPI_UID = null,
                PromotionalCode_UID = null,
                Property_UID = 1635,
                CreatedDate = new DateTime(2014, 02, 03, 15, 04, 18),
                ModifyDate = new DateTime(2014, 02, 03, 15, 04, 50),
                BillingAddress1 = "1255 Park Blvd.",
                BillingAddress2 = "1255 Elm Blvd.",
                BillingContactName = "Ilythia James",
                BillingPostalCode = "52361",
                BillingCity = "North Riverside",
                BillingCountry_UID = 39,
                CreateBy = null,
                Tax = 0.0000M,
                ChannelAffiliateName = "80",
                PaymentMethodType_UID = 4,
                ReservationCurrency_UID = 22,
                ReservationBaseCurrency_UID = 34,
                ReservationCurrencyExchangeRate = 0.4237000000M,
                ReservationCurrencyExchangeRateDate = new DateTime(2014, 02, 03, 15, 03, 58),
                ReservationLanguageUsed_UID = 1,
                BillingEmail = "Koto.James@threewaters.co.uk",
                TotalTax = 0.0000M,
                NumberOfRooms = 2,
                RoomsTax = 0.0000M,
                RoomsExtras = 72.0000M,
                RoomsPriceSum = 2091.0000M,
                RoomsTotalAmount = 2163.0000M,
                PropertyBaseCurrencyExchangeRate = 1.0000000000M,
                IsOnRequest = false,
                GuestFirstName = "Ilythia",
                GuestLastName = "Ilythia James",
                GuestEmail = "Koto.James@threewaters.co.uk",  //We are here
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

                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66242,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66242,
                        RoomType_UID = 5827,
                        GuestName = "Harlen Rupert",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39),
                        DateTo = new DateTime(2014,02,28,15,22,39),
                        AdultCount = 3,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "635290249597293559/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66242,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39),
                        DateTo = new DateTime(2014,02,28,15,22,39),
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "635290249597293559/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 66057,
                Guest_UID = 284443,  //verify if exists
                Number = "635290249597293559",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 03, 15, 03, 58),
                TotalAmount = 2163.0000M,
                Adults = 4,
                Children = 2,
                Status = 2,
                Notes = string.Empty,
                IPAddress = null,
                TPI_UID = null,
                PromotionalCode_UID = null,
                Property_UID = 1635,
                CreatedDate = new DateTime(2014, 02, 03, 15, 04, 18),
                ModifyDate = new DateTime(2014, 02, 03, 15, 04, 50),
                BillingAddress1 = "1255 Park Blvd.",
                BillingAddress2 = "1255 Elm Blvd.",
                BillingContactName = "Ilythia James",
                BillingPostalCode = "52361",
                BillingCity = "North Riverside",
                BillingCountry_UID = 39,
                CreateBy = null,
                Tax = 0.0000M,
                ChannelAffiliateName = "32",
                PaymentMethodType_UID = 4,
                ReservationCurrency_UID = 22,
                ReservationBaseCurrency_UID = 34,
                ReservationCurrencyExchangeRate = 0.4237000000M,
                ReservationCurrencyExchangeRateDate = new DateTime(2014, 02, 03, 15, 03, 58),
                ReservationLanguageUsed_UID = 1,
                BillingEmail = "Koto.James@threewaters.co.uk",
                TotalTax = 0.0000M,
                NumberOfRooms = 2,
                RoomsTax = 0.0000M,
                RoomsExtras = 72.0000M,
                RoomsPriceSum = 2091.0000M,
                RoomsTotalAmount = 2163.0000M,
                PropertyBaseCurrencyExchangeRate = 1.0000000000M,
                IsOnRequest = false,
                GuestFirstName = "Ilythia",
                GuestLastName = "Ilythia James",
                GuestEmail = "Koto.James@threewaters.co.uk",  //We are here
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

                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 66242,
                        Amount = 1061.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 66242,
                        RoomType_UID = 5827,
                        GuestName = "Harlen Rupert",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39),
                        DateTo = new DateTime(2014,02,28,15,22,39),
                        AdultCount = 3,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "635290249597293559/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    },
                    //Room 2
                    new ReservationRoom()
                    {
                        UID = 73670,
                        Reservation_UID = 66242,
                        RoomType_UID = 5855,
                        GuestName = "Winnifred Seymour",
                        SmokingPreferences = null,
                        DateFrom = new DateTime(2014,02,27,15,22,39),
                        DateTo = new DateTime(2014,02,28,15,22,39),
                        AdultCount = 2,
                        ChildCount = 1,
                        TotalTax = 0.0000M,
                        Package_UID = null,
                        CancellationPolicyDays = null,
                        ReservationRoomNo = "635290249597293559/2",
                        Status = 1,
                        CreatedDate = new DateTime(2014,02,26,15,22,40),
                        ModifiedDate = null,
                        RoomName = "quarto teste occ",
                        Rate_UID = 12388,
                        IsCanceledByChannels = null,
                        ReservationRoomsPriceSum = 341.0000M,
                        ReservationRoomsExtrasSum = 15.0000M,
                        ArrivalTime = new TimeSpan(13,00,00),
                        ReservationRoomsTotalAmount = 356.0000M,
                        CancellationCosts = false,

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5178,
                                ReservationRoom_UID = 73670,
                                ChildNo = 2,
                                Age = 12
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 12388
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>() { },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

            reservationsList.Add(new OB.Domain.Reservations.Reservation()
            {
                UID = 67242,
                Guest_UID = 284528,  //verify if exists
                Number = "672420249597293545",
                Channel_UID = 32,
                Date = new DateTime(2014, 02, 26, 15, 22, 39),
                TotalAmount = 705.0000M,
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
                RoomsExtras = 45.0000M,
                RoomsPriceSum = 660.0000M,
                RoomsTotalAmount = 705.0000M,
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
                PaymentGatewayName = "BrasPag",
                PaymentGatewayTransactionID = new Guid().ToString(),
                IsPartialPayment = true,
                NoOfInstallment = 3,
                ReservationPartialPaymentDetails = new List<ReservationPartialPaymentDetail>()
                {
                },
                ReservationPaymentDetails = new List<ReservationPaymentDetail>()
                {
                    new ReservationPaymentDetail()
                    {
                        UID = 59205,
                        PaymentMethod_UID = 1,
                        Reservation_UID = 67242,
                        Amount = 705.0000M,
                        Currency_UID = 86,
                        CVV = "ZdoQvjEwNunKXKCNVyBNftEN2/iva60Jxu2TTRbk9eQC8U22tX6nn1sxArwFE10kZKlayC8OfzbvLDW608G8d5XlMWTyRdCF5YWfWTGs+jFF9gKQoljMp9V7QCOoq4bwlzkY+0cn0IiaTWlhA5hjiVCundme3Pr/BfsgI6dPKos=",
                        CardName = "User",
                        CardNumber = "TtAudVS+PzK4qAxnaWqsBXFIzT60BZgPW3wDz+X7dIGH7rmuU9aTfzpt/R3RmbLA7kZECnmfCnz5nbf0dTQ06gWa/okyFjKQGGyjEilNDDK3/zas3RkDfXxUE/uvjeEasBc1T0J7YyU75B8Xs4SNYhq5YbuMIjewLy72mh19J+o=",
                        ExpirationDate = new DateTime(2014,03,26,15,22,39),
                        CreatedDate = new DateTime(2014,02,26,15,22,47),
                        ModifiedDate = null,
                        PaymentGatewayTokenizationIsActive = false,
                        OBTokenizationIsActive = false,
                        CreditCardToken = null,
                        HashCode = null
                    }
                },
                ReservationRooms = new List<ReservationRoom>()
                {
                    //Room 1
                    new ReservationRoom()
                    {
                        UID = 73669,
                        Reservation_UID = 67242,
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
                        ReservationRoomNo = "635290249597293545/1",
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

                        ReservationRoomChilds = new List<ReservationRoomChild>() {
                            new ReservationRoomChild()
                            {
                                UID = 5177,
                                ReservationRoom_UID = 73669,
                                ChildNo = 1,
                                Age = 6
                            }
                        },
                        ReservationRoomDetails = new List<ReservationRoomDetail>() {
                            new ReservationRoomDetail()
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
                                ReservationRoomDetailsAppliedIncentives = new List<ReservationRoomDetailsAppliedIncentive>(),
                                ReservationRoomDetailsAppliedPromotionalCodes = null,
                                Rate_UID = 9175
                            }
                        },
                        ReservationRoomExtras = new List<ReservationRoomExtra>()
                        {
                            new ReservationRoomExtra()
                            {
                                UID = 7383,
                                ReservationRoom_UID = 73669,
                                Qty =  2,
                                Extra_UID =  1833
                            }
                        },
                        ReservationRoomTaxPolicies = new List<ReservationRoomTaxPolicy>() { }
                    }
                }
            });

        }

        protected void FillChannels()
        {
            channelsList.Add(new Contracts.Data.Channels.Channel()
            {
                ChannelConfiguration = new Contracts.Data.Channels.ChannelConfiguration()
                {
                    GroupId = null,
                    GroupName = null,
                    HandleCredit = false,
                    HandlePaymentMethodsByRate = null,
                    UID = 59
                },
                Name = "Sabre", 
                UID = 59
            });
            channelsList.Add(new Contracts.Data.Channels.Channel()
            {
                ChannelCode = null,
                ChannelConfiguration = new Contracts.Data.Channels.ChannelConfiguration()
                {
                    GroupId = null,
                    GroupName = null,
                    HandleCredit = false,
                    HandlePaymentMethodsByRate = null,
                    UID = 32
                },
                Description = "test description",
                Enabled = true,
                IATA_Name = null,
                IATA_Number = null,
                IsBookingEngine = false,
                IsExtended = false,
                Name = "Booking",
                OperatorCode = null,
                OperatorType = 0,
                RealTimeType = null,
                Type = 0,
                UID = 32
            });
            channelsList.Add(new Contracts.Data.Channels.Channel()
            {
                ChannelCode = "4CANTOSt",

                ChannelConfiguration = new Contracts.Data.Channels.ChannelConfiguration()
                {
                    GroupId = null,
                    GroupName = null,
                    HandleCredit = true,
                    HandlePaymentMethodsByRate = null,
                    UID = 113
                },
                Description = null,
                Enabled = true,
                IATA_Name = null,
                IATA_Number = null,
                IsBookingEngine = false,
                IsExtended = false,
                Name = "4 Cantos",
                OperatorCode = null,
                OperatorType = 0,
                RealTimeType = null,
                Type = 2,
                UID = 80
            });

            channelsList.Add(new Contracts.Data.Channels.Channel()
            {
                ChannelCode = "DAGEN",

                ChannelConfiguration = new Contracts.Data.Channels.ChannelConfiguration()
                {
                    GroupId = null,
                    GroupName = null,
                    HandleCredit = true,
                    HandlePaymentMethodsByRate = null,
                    UID = 113
                },
                Description = null,
                Enabled = true,
                IATA_Name = null,
                IATA_Number = null,
                IsBookingEngine = false,
                IsExtended = false,
                Name = "Canal Directo Agencias",
                OperatorCode = null,
                OperatorType = 1,
                RealTimeType = null,
                Type = 2,
                UID = 40
            });

            channelsList.Add(new Channel
            {
                ChannelCode = "DCORP",
                Name = "Canal Directo Empresas",
                IsBookingEngine = false,
                IsExtended = null,
                Description = null,
                Enabled = true,
                Type = 2,
                IATA_Name = null,
                OperatorType = 1,
                OperatorCode = null,
                RealTimeType = null,
                UID = 50

            });

            channelsList.Add(new Contracts.Data.Channels.Channel()
            {
                ChannelCode = null,
                ChannelConfiguration = new Contracts.Data.Channels.ChannelConfiguration()
                {
                    GroupId = null,
                    GroupName = null,
                    HandleCredit = false,
                    HandlePaymentMethodsByRate = null,
                    UID = 32
                },
                Description = "test description",
                Enabled = true,
                IATA_Name = null,
                IATA_Number = null,
                IsBookingEngine = true,
                IsExtended = false,
                Name = "BE",
                OperatorCode = null,
                OperatorType = 0,
                RealTimeType = null,
                Type = 0,
                UID = 1
            });
        }

        protected void FillBePartialPaymentCcMethodListMock()
        {
            bePartialPaymentCcMethodList.Add(new BePartialPaymentCcMethod
            {
                Id = 1,
                InterestRate = 5,
                Parcel = 3,
                PartialPaymentMinimumValue = 10,
                PaymentMethodsId = 1,
                PaymentMethodTypeId = null,
                PropertyId = 1
            });

            bePartialPaymentCcMethodList.Add(new BePartialPaymentCcMethod
            {
                Id = 2,
                InterestRate = 3,
                Parcel = 2,
                PartialPaymentMinimumValue = decimal.MaxValue,
                PaymentMethodsId = 1,
                PaymentMethodTypeId = null,
                PropertyId = 1
            });

            bePartialPaymentCcMethodList.Add(new BePartialPaymentCcMethod
            {
                Id = 3,
                InterestRate = 10,
                Parcel = 10,
                PartialPaymentMinimumValue = null,
                PaymentMethodsId = 1,
                PaymentMethodTypeId = null,
                PropertyId = 1
            });

            bePartialPaymentCcMethodList.Add(new BePartialPaymentCcMethod
            {
                Id = 4,
                InterestRate = null,
                Parcel = 4,
                PartialPaymentMinimumValue = null,
                PaymentMethodsId = 1,
                PaymentMethodTypeId = null,
                PropertyId = 1
            });

            bePartialPaymentCcMethodList.Add(new BePartialPaymentCcMethod
            {
                Id = 6,
                InterestRate = 5,
                Parcel = 3,
                PartialPaymentMinimumValue = 10,
                PaymentMethodsId = 1,
                PaymentMethodTypeId = null,
                PropertyId = 1635
            });
        }

        #endregion

        #region Mock Repositories Calls
        protected void MockReservationFiltersRepo()
        {
            _reservationsFilterRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationFilter>()))
                .Returns((domainReservation.ReservationFilter entity) =>
                {
                    return entity;
                });

            _reservationsFilterRepoMock.Setup(x => x.FindByReservationUIDs(It.IsAny<List<long>>()))
                .Returns((List<long> resUids) =>
                {
                    return reservationFilterList.Where(y => resUids.Contains(y.UID));
                });
            int totalRecords = 0;
            _reservationsFilterRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationFilterCriteria>(), out totalRecords, It.IsAny<bool>()))
                .Returns((ListReservationFilterCriteria req, int TotalRecords, bool returnTotal) =>
                {
                    TotalRecords = 1;
                    return reservationsList.Where(y => req.ReservationUIDs.Contains(y.UID)).Select(y => y.UID);
                });
        }

        private int _reservationsAdditionalDataId = 0;
        protected void MockResAddDataRepo()
        {
            _resAddDataRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationsAdditionalData>()))
                .Returns((domainReservation.ReservationsAdditionalData entity) =>
                {
                    entity.UID = ++_reservationsAdditionalDataId;

                    reservationAddicionaldataList.Add(entity);

                    return entity;
                });

            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationAddicionaldataList.AsQueryable());

            _resAddDataRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<ReservationsAdditionalData, bool>>>()))
                .Returns((Expression<Func<ReservationsAdditionalData, bool>> exp) =>
                {
                    return reservationAddicionaldataList.AsQueryable().Where(exp);
                });
        }

        protected void MockAppRepository()
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

        protected void MockObPropertyRepo(ReservationDataBuilder resBuilder, int availabilityType = 0)
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
                    if (req.UIDs != null && req.UIDs.Any())
                    {
                        return listRoomTypes.Where(y => req.UIDs.Contains(y.UID)).ToList();
                    }

                    return roomTypes;
                });

            var propUid = resBuilder.InputData.reservationDetail.Property_UID;
            var currencyUid = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.GetValueOrDefault();
            listPropertiesLigth.Add(new BL.Contracts.Data.Properties.PropertyLight()
            {
                UID = propUid,
                Name = "hotel test",
                BaseCurrency_UID = currencyUid,  //we use the same as the reservation
                CurrencyISO = listCurrencies.Where(x => x.UID == currencyUid).Select(x => x.CurrencySymbol).SingleOrDefault(),
                AvailabilityType = availabilityType
            });

            _iOBPPropertyRepoMock.Setup(y => y.ListPropertiesLight(It.IsAny<ListPropertyRequest>()))
                .Returns((ListPropertyRequest req) =>
                {
                    if (req.UIDs != null && req.UIDs.Any())
                        return listPropertiesLigth.Where(z => req.UIDs.Contains(z.UID)).ToList();

                    return null;
                });

            _iOBPPropertyRepoMock.Setup(y => y.ListProperties(It.IsAny<ListPropertiesRequest>()))
                .Returns((ListPropertiesRequest req) =>
                {
                    if (req.Ids != null && req.Ids.Any())
                        return listProperties.Where(z => req.Ids.Contains(z.Id)).ToList();

                    return null;
                });

            _iOBPPropertyRepoMock.Setup(x => x.ConvertToPropertyTimeZone(It.IsAny<ConvertToPropertyTimeZoneRequest>()))
                .Returns((ConvertToPropertyTimeZoneRequest req) =>
                {
                    return req.Dates.ToList();
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListChannelsProperty(It.IsAny<ListChannelsPropertyRequest>()))
                .Returns((ListChannelsPropertyRequest req) =>
                {
                    if (req.ChannelUIDs != null && req.ChannelUIDs.Any() && req.PropertyUIDs != null && req.PropertyUIDs.Any())
                    {
                        return chPropsList.Where(y => req.ChannelUIDs.Contains(y.Channel_UID) && req.PropertyUIDs.Contains(y.Property_UID)).ToList();
                    }

                    return null;
                });

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfigurationReduced(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                                .Returns((ListActivePaymentGatewayConfigurationRequest request) =>
                                {

                                    return listPaymentGetwayConfigs.FirstOrDefault();
                                });
        }

        protected void MockObListPropertyRepo()
        {
            _iOBPPropertyRepoMock.Setup(y => y.ListPropertiesLight(It.IsAny<ListPropertyRequest>()))
                .Returns((ListPropertyRequest req) =>
                {
                    if (req.UIDs != null && req.UIDs.Any())
                        return listPropertiesLigth.Where(z => req.UIDs.Contains(z.UID)).ToList();

                    return null;
                });

            _iOBPPropertyRepoMock.Setup(y => y.ListProperties(It.IsAny<ListPropertiesRequest>()))
                .Returns((ListPropertiesRequest req) =>
                {
                    if (req.Ids != null && req.Ids.Any())
                        return listProperties.Where(z => req.Ids.Contains(z.Id)).ToList();

                    return null;
                });
        }

        protected void MockExternalSystemsRepo()
        {
            _iExternalSystemsRepositoryMock.Setup(y => y.CheckRemoteAvailability(It.IsAny<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>()))
                .Returns((ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest req) =>
                {
                    var response = new ES.API.Contracts.Response.CheckRemoteAvailabilityResponse();

                    if (!string.IsNullOrWhiteSpace(req.RemoteReservation) && req.RemoteReservation != "null")
                    {
                        response = new ES.API.Contracts.Response.CheckRemoteAvailabilityResponse()
                        {
                            PreCheckStatus = ES.API.Contracts.PreCheckStatus.IsAllowed,
                            MessageIdentifierToken = req.MessageIdentifierToken,
                            Status = ES.API.Contracts.Status.Success,
                            ReservationExternalIdentifier = new ES.API.Contracts.Reservations.ReservationExternalIdentifier
                            {
                                ClientId = 1,
                                InternalId = 1,
                                IsByReservationRoom = false,
                                ExternalNumber = "PmsReservationNumber",
                                PmsId = 1,
                                ReservationRoomExternalIdentifiers = new List<ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier>
                                {
                                    new ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier
                                    {
                                        ExternalNumber = "PmsReservationRoomNumber",
                                        InternalId = 1,
                                        Status = 1
                                    }
                                }
                            }
                        };

                        return response;
                    }
                    else
                    {
                        response = new ES.API.Contracts.Response.CheckRemoteAvailabilityResponse()
                        {
                            PreCheckStatus = ES.API.Contracts.PreCheckStatus.IsNotAllowed,
                            MessageIdentifierToken = req.MessageIdentifierToken,
                            Status = ES.API.Contracts.Status.Fail
                        };

                        return response;
                    }
                });

            _iOBPPropertyRepoMock.Setup(y => y.ListProperties(It.IsAny<ListPropertiesRequest>()))
                .Returns((ListPropertiesRequest req) =>
                {
                    if (req.Ids != null && req.Ids.Any())
                        return listProperties.Where(z => req.Ids.Contains(z.Id)).ToList();

                    return null;
                });
        }

        protected void MockPMSRepo()
        {
            _iOBPMSRepositoryMock.Setup(x => x.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()))
                .Returns((ListPMSServicesPropertyMappingRequest req) =>
                {
                    var response = new ListPMSServicesPropertyMappingResponse
                    {
                        RequestId = req.RequestId,
                        TotalRecords = 0,
                        Status = Status.Fail
                    };

                    if (req.PropertyUIDs?.Any() == false)
                        return response;

                    else if (req.PropertyUIDs.FirstOrDefault() <= 0)
                    {
                        response = new ListPMSServicesPropertyMappingResponse
                        {
                            RequestId = req.RequestId,
                            TotalRecords = 0,
                            Status = Status.Success,
                            Warnings = null,
                            Errors = null
                        };

                        return response;
                    }
                    else
                    {
                        response = new ListPMSServicesPropertyMappingResponse
                        {
                            RequestId = req.RequestId,
                            TotalRecords = 1,
                            Status = Status.Success,
                            Warnings = null,
                            Errors = null,
                            Result = new List<Contracts.Data.PMS.PMSServicesPropertyMapping>()
                            {
                                new Contracts.Data.PMS.PMSServicesPropertyMapping()
                                {
                                    ServiceName = "SBRQ"
                                }
                            }
                        };

                        return response;
                    }
                });
        }

        protected void MockInventoryRepo()
        {
            _iOBPPropertyRepoMock.Setup(x => x.ListInventory(It.IsAny<ListInventoryRequest>()))
                .Returns((ListInventoryRequest req) =>
                {
                    List<OB.BL.Contracts.Data.Properties.Inventory> ret = null;

                    if (req.roomTypeIdsAndDateRange != null && req.roomTypeIdsAndDateRange.Any())
                        ret = inventoryList.Where(y => req.roomTypeIdsAndDateRange.Select(z => z.RoomType_UID).Contains(y.RoomType_UID)).ToList();

                    return ret;
                });

            _iOBPPropertyRepoMock.Setup(y => y.UpdateInventoryDetails(It.IsAny<UpdateInventoryDetailsRequest>()))
                .Returns((UpdateInventoryDetailsRequest req) =>
                {
                    List<OB.BL.Contracts.Data.Properties.Inventory> ret = null;

                    if (req.InventoriesToUpdate != null && req.InventoriesToUpdate.Any())
                    {
                        foreach (var inventory in req.InventoriesToUpdate)
                        {
                            //We dont considere the date here (it can be changed if we need)
                            var current = inventoryList.Where(y => y.RoomType_UID == inventory.RoomType_UID).FirstOrDefault();
                            if (current != null)
                            {
                                inventoryList.Remove(current);
                                inventoryList.Add(inventory.Clone());
                                if (ret == null)
                                    ret = new List<Inventory>();
                                ret.Add(inventory.Clone());
                            }
                        }
                    }

                    return ret;
                });
        }

        protected void MockObPaymentMethodRepo()
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

            _paymentMethodTypeRepoMock.Setup(x => x.ListPaymentMethods(It.IsAny<ListPaymentMethodsRequest>()))
                .Returns((ListPaymentMethodsRequest req) =>
                {
                    if (req.UIDs != null)
                        return paymentMethodsList.Where(y => req.UIDs.Contains(y.UID)).ToList();

                    return null;
                });
        }

        protected void MockPropertyEventsRepo(List<PropertyEventQR1> propertyEventQueryResultList = null)
        {
            _propertyEventsRepoMock.Setup(
                    x => x.InsertPropertyQueue(It.IsAny<Contracts.Requests.InsertPropertyQueueRequest>()))
                .Returns(1);
        }

        protected void MockSecurityRepo()
        {
            _obSecurityRepoMock.Setup(x => x.DecryptCreditCards(It.IsAny<ListCreditCardRequest>()))
                .Returns((ListCreditCardRequest req) =>
                {
                    var ret = new List<string>();

                    if (req.CreditCards != null && req.CreditCards.Any())
                    {
                        req.CreditCards.ForEach(y =>
                        {
                            ret.Add(DecryptCreditCard(y));
                        });
                    }

                    return ret;
                });
        }

        protected void MockDepositPolicyRepo(bool mockMostRestrictivePolicies = false)
        {
            _depositRepoMock.Setup(x => x.ListDepositPolicies(It.IsAny<ListDepositPoliciesRequest>()))
                .Returns((ListDepositPoliciesRequest req) =>
                {
                    if (req.RateId.HasValue)
                        return depositPolicyList.Where(y => y.RateUID == req.RateId).ToList();

                    return new List<OB.BL.Contracts.Data.Rates.DepositPolicy>();
                });

            if (mockMostRestrictivePolicies)
            {
                _depositRepoMock.Setup(x => x.CalculateMostRestrictiveDepositPolicy(It.IsAny<CalculateMostRestrictiveDepositPolicyRequest>()))
                .Returns((CalculateMostRestrictiveDepositPolicyRequest req) =>
                {
                    return req.collection.FirstOrDefault();  //For now we only need to work with one policy (it can be changed if we need more policies)
                });
            }
        }

        protected void MockCancelationPolicyRepo(bool mockMostRestrictivePolicies = false)
        {
            _cancellationRepoMock.Setup(x => x.ListCancelationPolicies(It.IsAny<ListCancellationPoliciesRequest>()))
                .Returns((ListCancellationPoliciesRequest req) =>
                {
                    if (req.RateId.HasValue && mockMostRestrictivePolicies)
                        return cancellationPolicyList.Where(x => x.RateUID == req.RateId).ToList();

                    return new List<OB.BL.Contracts.Data.Rates.CancellationPolicy>();
                });

            if (mockMostRestrictivePolicies)
            {
                _cancellationRepoMock.Setup(x => x.CalculateMostRestrictiveCancellationPolicy(It.IsAny<CalculateMostRestrictiveCancellationPolicyRequest>()))
                .Returns((CalculateMostRestrictiveCancellationPolicyRequest req) =>
                {
                    return req.collection.FirstOrDefault();  //For now we only need one (if needed we will change)
                });
            }
        }

        protected void MockOtherPolicyRepo(bool mockMostRestrictivePolicies = false)
        {
            _otherRepoMock.Setup(x => x.GetOtherPoliciesByRateId(It.IsAny<GetOtherPoliciesRequest>()))
                .Returns((GetOtherPoliciesRequest req) =>
                {
                    if (req.RateId != null && mockMostRestrictivePolicies)
                        return otherPolicyList.FirstOrDefault();  //We send one because we cannot filter by RateId
                    return null;
                });
        }

        protected int ReservationRoomCount = 0;
        protected void MockReservationsRoomsRepo()
        {
            _reservationRoomRepoMock.Setup(x => x.Add(It.IsAny<domainReservation.ReservationRoom>()))
                .Returns((domainReservation.ReservationRoom entity) =>
                {
                    entity.UID = ++ReservationRoomCount;

                    return entity;
                });

            _reservationRoomRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<ReservationRoom, bool>>>()))
                .Returns((Expression<Func<ReservationRoom, bool>> exp) =>
                {
                    return reservationsList.SelectMany(x => x.ReservationRooms).AsQueryable().Where(exp);
                });
        }

        protected void MockResRoomDetailsRepo()
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

        protected int ReservationChildRoomCount = 0;
        protected void MockResChildRepo(ReservationDataBuilder builder)
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

        protected static readonly long roomTypeUidMock = 3709;
        protected static readonly long rateUidMock = 4639;
        protected void MockObRateToomDetailsForResRepo(int roomsToUpdate)
        {
            _rrdRepoMock.Setup(x => x.UpdateRateRoomDetailAllotments(It.IsAny<UpdateRateRoomDetailAllotmentsRequest>()))
                .Returns((UpdateRateRoomDetailAllotmentsRequest req) =>
                {
                    UpdateRateRoomDetailAllotmentsResponse ret = new UpdateRateRoomDetailAllotmentsResponse();

                    //we dont do the same of the repository in the OB API do because it only changes the database (to change if we need later)
                    if (req.RateRoomDetailUIDs != null && req.RateRoomDetailUIDs.Any() && req.AllotmentIncrements != null && req.AllotmentIncrements.Any())
                    {
                        var rateRoomDetail = rateRoomDetailList.Where(y => req.RateRoomDetailUIDs.Contains(y.UID)).FirstOrDefault();
                        var rateRoomDetailTmp = rateRoomDetail.Clone();

                        foreach (var allotmentInc in req.AllotmentIncrements)
                        {
                            rateRoomDetailTmp.AllotmentUsed += allotmentInc;
                        }

                        rateRoomDetailList.Remove(rateRoomDetail);
                        rateRoomDetailList.Add(rateRoomDetailTmp);
                    }

                    return roomsToUpdate;
                });

            _rrdRepoMock.Setup(x => x.ListRateRoomDetails(It.IsAny<ListRateRoomDetailsRequest>()))
                .Returns((ListRateRoomDetailsRequest req) =>
                {
                    var ret = new List<OB.BL.Contracts.Data.Rates.RateRoomDetail>();

                    if (req.UIDs != null && req.UIDs.Any())
                        ret = rateRoomDetailList.Where(y => req.UIDs.Contains(y.UID)).ToList();

                    return ret;
                });
        }

        protected void MockObRatesRepo(SearchBuilder builder)
        {
            if (listRates == null || !listRates.Any())
            {
                builder.InputData.Rates.ForEach(x =>
                {
                    listRates.Add(new Rate()
                    {
                        UID = rateUidMock,
                        Name = x.Name,
                        Rate_UID = rateUidMock
                    });
                });
            }

            _obRatesRepoMock.Setup(x => x.ListRatesForReservation(It.IsAny<ListRatesForReservationRequest>()))
                .Returns((ListRatesForReservationRequest req) =>
                {
                    return listRates.Where(y => req.RateUIDs.Contains(y.UID)).ToList();
                });

            _obRatesRepoMock.Setup(x => x.ListRatesAvailablityType(It.IsAny<ListRateAvailabilityTypeRequest>()))
                .Returns((ListRateAvailabilityTypeRequest req) =>
                {
                    return listRates.Where(y => req.RatesUIDs.Contains(y.UID))
                        .ToDictionary(y => y.UID, y => y.AvailabilityType);
                });
        }

        //Is separated to can be called before define the SearchBuilder object
        protected void MockObRatesRepoQuery()
        {
        }

        protected void MockGroupCodes()
        {
            _obRatesRepoMock.Setup(x => x.ListGroupCodesForReservation(It.IsAny<ListGroupCodesForReservationRequest>()))
                .Returns((ListGroupCodesForReservationRequest req) =>
                {
                    if (req.GroupCodesIds != null && req.GroupCodesIds.Any())
                        return groupCodeList.Where(y => req.GroupCodesIds.Contains(y.UID)).ToList();
                    else
                        return groupCodeList;
                });
        }

        protected void MockObChannelsRepo(ReservationDataBuilder resBuilder)
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

            _channelPropertiesRepoMock.Setup(x => x.ListChannel(It.IsAny<ListChannelRequest>()))
                .Returns((ListChannelRequest req) =>
                {
                    if (req.ChannelUIDs != null && req.ChannelUIDs.Any())
                        return channelsList.Where(y => req.ChannelUIDs.Contains(y.UID)).ToList();

                    if (req.ChannelIds != null && req.ChannelIds.Any())
                        return channelsList.Where(y => req.ChannelIds.Contains(y.UID)).ToList();

                    return null;
                });
        }
        protected void MockObCurrenciesRepo()
        {
            _obCurrencyRepoMock.Setup(y => y.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns(listCurrencies.Select(y => new Contracts.Data.General.Currency { UID = y.UID, CurrencySymbol = y.CurrencySymbol, DefaultPositionNumber = y.DefaultPositionNumber, Name = y.Name, PaypalCurrencyCode = y.PaypalCurrencyCode, Symbol = y.Symbol }).ToList());
        }
        protected void MockVisualStates()
        {
            _visualStateRepoMock.Setup(y => y.GetQuery())
                .Returns(listVisualStates.AsQueryable());
        }

        protected void MockIncentives(SearchBuilder builder, bool? freeNights = null)
        {
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

        protected void MockListRateRoomDetail(SearchBuilder builder, ReservationRoom room, CalculateFinalPriceParameters parameters, int numFreeChilds, bool countAsAdult, bool priceModel = false, int? commissionType = null)
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
                            RateModelValue = rateModuleValue
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

        protected void MockBuyerGroupRepo(SearchBuilder builder, ReservationDataBuilder resBuilder)
        {
            _buyerGroupRepo.Setup(x => x.GetRateBuyerGroup(It.IsAny<long>(), It.IsAny<long>()))
                .Returns((long rateUid, long? tpiUid) =>
                {
                    return builder.BuyerGroups.Where(y => y.Rate_UID == rateUid && tpiUid == y.TPI_UID).SingleOrDefault();
                });
            _buyerGroupRepo.Setup(x => x.ListRatesChannels(It.IsAny<ListRatesChannelsRequest>()))
                .Returns((ListRatesChannelsRequest req) =>
                {
                    var ret = new List<BL.Contracts.Data.Rates.RateChannel>() { };

                    if (builder.InputData.RateChannels != null && builder.InputData.RateChannels.Any())
                    {
                        ret.AddRange(builder.InputData.RateChannels.Select(y => ConvertRateChannel(y)).ToList());
                    }

                    return ret;
                });
        }

        protected void MockReservationRoomsExtraRepoGeneric(ReservationDataBuilder resBuilder)
        {
            _reservationRoomsExtrasRepoMock.Setup(x => x.Add(It.IsAny<ReservationRoomExtra>()))
                .Returns((ReservationRoomExtra entry) =>
                {
                    //We considere only the first roomExtra
                    entry.UID = resBuilder.InputData.reservationRoomExtras[0].UID;
                    var resRoomId = resBuilder.InputData.reservationRoomExtras[0].ReservationRoom_UID;
                    entry.ReservationRoom_UID = resRoomId;
                    return entry;
                });
        }

        protected OB.BL.Contracts.Data.Rates.RateChannel ConvertRateChannel(OB.BL.Operations.Test.Domain.Rates.RatesChannel y)
        {
            return new OB.BL.Contracts.Data.Rates.RateChannel()
            {
                ChannelCommissionCategory_UID = y.ChannelCommissionCategory_UID,
                Channel_UID = y.Channel_UID,
                Commission = y.Commission,
                ContractName = y.ContractName,
                Createdby = y.Createdby,
                CreatedDate = y.CreatedDate,
                Currency_UID = y.Currency_UID,
                CustomExchangeRate = y.CustomExchangeRate,
                IncomingOfficeCode = y.IncomingOfficeCode,
                IsCustomExchangeRateSelected = y.IsCustomExchangeRateSelected,
                IsDeleted = y.IsDeleted,
                IsPercentage = y.IsPercentage,
                Markup = y.Markup,
                ModifiedBy = y.ModifiedBy,
                ModifiedDate = y.ModifiedDate,
                Package = y.Package,
                //PaymentTypesUIDs = y.PaymentTypesUIDs,
                PriceAddOnIsPercentage = y.PriceAddOnIsPercentage,
                PriceAddOnIsValueDecrease = y.PriceAddOnIsValueDecrease,
                PriceAddOnValue = y.PriceAddOnValue,
                RateCode = y.RateCode,
                RateModel_UID = y.RateModel_UID,
                Rate_UID = y.Rate_UID,
                Sequence = y.Sequence,
                UID = y.UID,
                Value = y.Value
            };
        }

        protected void MockPromoCodesRepo(SearchBuilder builder)
        {
            _promoCodesRepo.Setup(x => x.ListPromotionalCode(It.IsAny<ListPromotionalCodeRequest>()))
                .Returns((ListPromotionalCodeRequest req) =>
                {
                    if (req.PromoCodeIds != null && req.PromoCodeIds.Any())
                    {
                        return promocodesList.Where(y => req.PromoCodeIds.Contains(y.UID)).ToList();
                    }

                    return null;
                });
            _promoCodesRepo.Setup(x => x.ListPromotionalCodeForReservation(It.IsAny<Contracts.Requests.ListPromotionalCodeForReservationRequest>()))
                .Returns((Contracts.Requests.ListPromotionalCodeForReservationRequest req) =>
                {
                    return promocodesList.Where(y => y.UID == req.PromoCodeId).SingleOrDefault();
                });
        }

        protected int numberOfReservations = 0;
        protected void MockReservationsRepo(SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId, bool isExisting = false, List<Inventory> inventories = null, bool addTranlations = false, long propBaseLang = 3, bool? hndCredit = null, bool onRequestEnable = false, int channelOperatorType = 0)  //Use the builder to construct the reservation context
        {
            numberOfReservations = 0;

            Dictionary<long, DepositPolicyQR1> depositPolicies = new Dictionary<long, DepositPolicyQR1>();
            Dictionary<long, CancellationPolicyQR1> cancelationPolicies = new Dictionary<long, CancellationPolicyQR1>();
            Dictionary<long, OtherPolicyQR1> otherPolicies = new Dictionary<long, OtherPolicyQR1>();
            var rateUids = resBuilder.InputData.reservationRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).ToList();

            //deposit policies + translate
            if (resBuilder.InputData.reservationDetail.DepositPolicy != null)
            {
                if (rateUids.Any())
                {
                    var depPolicy = depositPolicyList.Where(x => x.RateUID.HasValue && rateUids.Contains(x.RateUID.Value))
                        .Select(x => OtherConverter.Convert(x)).SingleOrDefault();
                    if (addTranlations)
                    {
                        var policyTranslate = depositPolicyLanguageList.FirstOrDefault();
                        depPolicy.TranslatedDepositPolicyName = policyTranslate.Name;
                        depPolicy.TranslatedDepositPolicyDescription = policyTranslate.Description;
                        AddTranslateToMockDepositPolicy(depositPolicyList.SingleOrDefault(), policyTranslate);
                    }
                    depositPolicies = new List<DepositPolicyQR1>() { depPolicy }.ToDictionary(x => rateUids[0], x => x);
                }
            }

            //cancellation policies + translate
            if (resBuilder.InputData.reservationDetail.CancellationPolicy != null)
            {
                if (rateUids.Any())
                {
                    var canPolicy = cancellationPolicyList.Where(x => x.RateUID.HasValue && rateUids.Contains(x.RateUID.Value))
                        .Select(x => OtherConverter.Convert(x)).SingleOrDefault();
                    if (addTranlations)
                    {
                        var policyTranslate = cancellationPolicyLanguageList.FirstOrDefault();  //We dont have how to get using the policy uid but there are only one in the list
                        canPolicy.TranslatedCancellationPolicy_Description = policyTranslate.Name;
                        canPolicy.TranslatedCancellationPolicy_Description = policyTranslate.Description;
                        AddTranslateToMockCancellationPolicy(cancellationPolicyList.SingleOrDefault(), policyTranslate);
                    }
                    cancelationPolicies = new List<CancellationPolicyQR1>() { canPolicy }.ToDictionary(x => rateUids[0], x => x);
                }
            }

            //other policies + translate
            if (resBuilder.InputData.reservationDetail.OtherPolicy != null)
            {
                var otherPolicy = otherPolicyList.Select(x => OtherConverter.Convert(x)).SingleOrDefault();
                if (addTranlations)
                {
                    var policyTranslate = otherPolicyLanguageList.FirstOrDefault();  //We dont have how to get using the policy uid but there are only one in the list
                    otherPolicy.TranslatedName = policyTranslate.Name;
                    otherPolicy.TranslatedDescription = policyTranslate.Description;
                    AddTranslateToMockOtherPolicy(otherPolicyList.SingleOrDefault(), policyTranslate);
                }
                otherPolicies = new List<OtherPolicyQR1>() { otherPolicy }.ToDictionary(x => rateUids[0], x => x);
            }

            long? salesmanCommissionUid = null;
            if (resBuilder.InputData.reservationDetail.Salesman_UID != 0 && resBuilder.InputData.reservationDetail.SalesmanCommission.HasValue)
                salesmanCommissionUid = salesmanTPICommissionsList.FirstOrDefault(x => x.SalesmanUID == resBuilder.InputData.reservationDetail.Salesman_UID).UID;

            var opBillingType = chPropsList.Where(x => x.Channel_UID == resBuilder.InputData.reservationDetail.Channel_UID).Select(x => x.OperatorBillingType).FirstOrDefault();

            var rates = resBuilder.InputData.reservationRooms.Select(x => x.Rate_UID);
            var paymentMethodTypeUid = resBuilder.InputData.reservationDetail.PaymentMethodType_UID;
            var rateChannelsAndPaymentsCount = (from rc in ratesChannelList
                                                join rcpm in ratesChannelsPaymentMethodsList on rc.UID equals rcpm.RateChannel_UID
                                                where rcpm.PaymentMethod_UID == paymentMethodTypeUid && rates.Contains(rc.Rate_UID)
                                                select rc).Count();
            //var ch = chPropsList.Where(x => x.Channel_UID == resBuilder.InputData.reservationDetail.Channel_UID).SingleOrDefault();
            //var isOpCreditLimit = ch != null ? ch.IsOperatorsCreditLimit : true;

            var handleCredit = !hndCredit.HasValue ? (channelsList.Where(x => x.UID == resBuilder.InputData.reservationDetail.Channel_UID) != null ?
                                    channelsList.Where(x => x.UID == resBuilder.InputData.reservationDetail.Channel_UID).Select(x => x.ChannelConfiguration.HandleCredit).SingleOrDefault() :
                                    (bool?)null) : hndCredit.Value;

            var channelTmp = channelsList.Where(x => x.UID == resBuilder.InputData.reservationDetail.Channel_UID);
            var channelName = string.Empty;
            if (channelTmp != null && channelTmp.Any())
            {
                channelName = channelTmp.Select(x => x.Name).FirstOrDefault();
            }

            var ret = new ReservationDataContext();

            ret.Client_UID = resBuilder.InputData.guest != null ? resBuilder.InputData.guest.Client_UID : 0;
            ret.PropertyName = "Property Test";
            ret.PropertyBaseLanguage_UID = propBaseLang;  //same of the reservation
            ret.PropertyBaseCurrency_UID = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID;  //same of the reservation
            ret.PropertyCountry_UID = null;
            ret.PropertyCity_UID = null;
            ret.BookingEngineChannel_UID = searchBuilder.InputData.RateChannels != null && searchBuilder.InputData.RateChannels.Any() ? searchBuilder.InputData.RateChannels[0].Channel_UID : 0;
            ret.Channel_UID = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault();
            ret.ChannelName = channelName;
            ret.ChannelType = 0;
            ret.ChannelOperatorType = channelOperatorType;
            ret.ChannelHandleCredit = handleCredit;
            ret.IsChannelValid = resBuilder.InputData.reservationDetail.Channel_UID.GetValueOrDefault() != 0;
            ret.Guest_UID = resBuilder.InputData.guest != null ? resBuilder.InputData.guest.UID : (long?)null;
            ret.IsFromChannel = resBuilder.InputData.reservationDetail.Channel_UID != 1;  //Check if is from BE or from Channel
            ret.TPIProperty_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.Property_UID : null;
            ret.TPICompany_CommissionIsPercentage = resBuilder.InputData.reservationRooms != null && resBuilder.InputData.reservationRooms.Any() ? resBuilder.InputData.reservationRooms[0].TPIDiscountIsPercentage : null;
            ret.TPIProperty_Commission = resBuilder.InputData.reservationRooms != null && resBuilder.InputData.reservationRooms.Any() ? resBuilder.InputData.reservationRooms[0].TPIDiscountValue : null;
            ret.TPI_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.UID : (long?)null;
            ret.TPICompany_UID = searchBuilder.InputData.Tpi != null ? searchBuilder.InputData.Tpi.Company_UID : null;
            ret.SalesmanCommission_UID = salesmanCommissionUid;
            ret.Salesman_UID = resBuilder.InputData.reservationDetail.Salesman_UID;
            ret.SalesmanBaseCommission = resBuilder.InputData.reservationDetail.SalesmanCommission;
            ret.SalesmanIsBaseCommissionPercentage = resBuilder.InputData.reservationDetail.SalesmanIsCommissionPercentage;
            ret.ReservationRoomDetails = resBuilder.InputData.reservationRoomDetails;
            ret.Currency_UID = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.HasValue ? resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.Value : 34;
            ret.Currency_Symbol = resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.HasValue ? GetCurrencySymbol(resBuilder.InputData.reservationDetail.ReservationBaseCurrency_UID.Value) : "EUR";
            ret.ReservationUID = resBuilder.InputData.reservationDetail.UID;
            ret.ChannelProperty_UID = searchBuilder.InputData.PropertyChannels != null && searchBuilder.InputData.PropertyChannels.Any() ? searchBuilder.InputData.PropertyChannels[0].UID : (long?)null;
            ret.IsOnRequestEnable = onRequestEnable;
            ret.Inventories = inventories;
            ret.CancellationPolicies = cancelationPolicies;  //TODO: change this to the correct values
            ret.DepositPolicies = depositPolicies;
            ret.OtherPolicies = otherPolicies;
            ret.ChannelPropertyOperatorBillingType = (int)opBillingType.GetValueOrDefault();
            ret.RateChannelsAndPaymentsCount = rateChannelsAndPaymentsCount;


            _reservationsRepoMock.Setup(x => x.GetReservationContext(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
    It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IEnumerable<long>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<Guid?>()))
    .Returns(ret);

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

            var resNum = resBuilder.InputData.reservationDetail.Number;
            int total = -1;
            _reservationsRepoMock.Setup(x => x.FindByCriteria(out total, It.IsAny<ListReservationCriteria>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns((int Total, ListReservationCriteria list, int pageindex, int pagesize, bool v) =>
                {
                    Total = 1;
                    return reservationsList.Where(x => list.ReservationUIDs.Contains(x.UID));
                });

            _reservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationCriteria>()))
                .Returns((ListReservationCriteria list) =>
                {
                    return reservationsList.Where(x => list.ReservationUIDs.Contains(x.UID));
                });

            _reservationsRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().Where(expr);
                });

            _reservationsRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationsList.AsQueryable());

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

            _reservationsRepoMock.Setup(x => x.FindByUIDEagerly(It.IsAny<long>()))
                .Returns((long resUid) =>
                {
                    return reservationsList.Where(y => y.UID == resUid).SingleOrDefault();
                });

            _reservationsRepoMock.Setup(x => x.Single(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().Single(expr);
                });

            _reservationsRepoMock.Setup(x => x.ValidateReservation(It.IsAny<DL.Common.Criteria.ValidateReservationCriteria>()))
                .Returns(0)
                .Callback(() =>
                {
                    ValidateReservationV2Called = true;
                });

            MockReservationRepoFirstOrDefault();

            MockResNumbersGenerator();

            MockLookups();
        }

        protected void MockReservationRepoFirstOrDefault()
        {
            _reservationsRepoMock.Setup(x => x.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<OB.Domain.Reservations.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<OB.Domain.Reservations.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().FirstOrDefault(expr);
                });

            _reservationsRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().Where(expr);
                });

            _reservationsRepoMock.Setup(x => x.FindByNumberEagerly(It.IsAny<string>()))
                .Returns((string resNo) =>
                {
                    return reservationsList.Where(y => y.Number == resNo).FirstOrDefault();
                });
        }

        protected void MockResRoomAppliedIncentives()
        {
            _resRoomAppliedIncentiveRepoMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomDetailsAppliedIncentive>()))
                .Returns((domainReservation.ReservationRoomDetailsAppliedIncentive entity) =>
                {
                    return entity;
                });
        }

        protected void MockGroupRulesRepo()
        {
            _groupRulesRepo.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>()))
                .Returns((DL.Common.Criteria.GetGroupRuleCriteria criteria) =>
                {
                    return groupRuleList.AsQueryable().FirstOrDefault(x => x.RuleType == criteria.RuleType);
                });
        }

        protected void MockObCrmRepo()
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

                    var currentGuest = guestsList.Where(y => y.UID == req.Guest.UID).SingleOrDefault();
                    guestsList.Remove(currentGuest);
                    guestsList.Add(req.Guest);

                    res.Result = new contractsCRM.Guest()
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
                    int i = 0;
                    var lastGuest = guestsList.OrderBy(x => x.UID).Last();

                    req.Guest.UID = lastGuest.UID + 1;
                    guestsList.Add(req.Guest);

                    response.Result = new contractsCRM.Guest()
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
                    var ret = new List<TPIProperty>() { };

                    if (req.TPI_Ids != null && req.TPI_Ids.Any())
                    {
                        ret = tpiPropertyList.Where(y => req.TPI_Ids.Contains(y.TPI_UID)).ToList();
                    }
                    else if (req.PropertyIds != null && req.PropertyIds.Any())
                    {
                        ret = tpiPropertyList.Where(y => req.PropertyIds.Contains(y.Property_UID)).ToList();
                    }

                    return ret;
                });

            _tpiRepoMock.Setup(x => x.ListThirdPartyIntermediariesLight(It.IsAny<ListThirdPartyIntermediariesLightRequest>()))
                .Returns((ListThirdPartyIntermediariesLightRequest req) =>
                {
                    if (req.UIDs != null && req.UIDs.Any())
                    {
                        return tpisList.Where(y => req.UIDs.Contains(y.UID)).Select(y => ConvertToLigth(y)).ToList();
                    }

                    return null;
                });

            _tpiRepoMock.Setup(x => x.ListBESpecialRequestForReservation(It.IsAny<ListBESpecialRequestForReservationRequest>()))
                .Returns((ListBESpecialRequestForReservationRequest req) =>
                {
                    if (req.BESpecialRequestsIds != null && req.BESpecialRequestsIds.Any())
                    {
                        return beSpecRequestsList.Where(y => req.BESpecialRequestsIds.Contains(y.UID)).ToList();
                    }

                    return beSpecRequestsList.ToList();
                });
        }

        protected void MockChildTermsRepo(SearchBuilder builder)
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

        protected void MockExtrasRepo(ReservationDataBuilder resBuilder)
        {
            _extrasRepoMock.Setup(x => x.ListIncludedRateExtras(It.IsAny<ListIncludedRateExtrasRequest>()))
                .Returns((ListIncludedRateExtrasRequest req) =>
                {
                    List<Extra> ret = new List<Extra>() { };

                    if (resBuilder.InputData.reservationRoomExtras != null)
                    {
                        resBuilder.InputData.reservationRoomExtras.Where(y => y.ExtraIncluded == true).ToList();
                    }

                    return ret;
                });
            _extrasRepoMock.Setup(x => x.ListExtras(It.IsAny<OB.BL.Contracts.Requests.ListExtraRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListExtraRequest req) =>
                {
                    var ret = resBuilder.InputData.reservationRoomExtras.Where(y => req.UIDs.Contains(y.Extra_UID));

                    return ret.Select(y => GetExtraFromRoomExtra(y)).ToList();
                });
            _extrasRepoMock.Setup(x => x.ListRatesExtrasPeriod(It.IsAny<ListRatesExtrasPeriodRequest>()))
                .Returns((ListRatesExtrasPeriodRequest req) =>
                {
                    List<RatesExtrasPeriod> ret = null;
                    var tmp = resBuilder.InputData.reservationRoomExtrasAvailableDates;

                    if (tmp != null)
                    {
                        ret = tmp.Select(y => OtherConverter.Convert(y)).ToList();
                    }

                    return ret;
                });
        }

        protected OB.BL.Contracts.Data.Rates.Extra GetExtraFromRoomExtra(ReservationRoomExtra rre)
        {
            if (rre == null)
                return null;

            return new OB.BL.Contracts.Data.Rates.Extra()
            {
                UID = rre.Extra_UID,
                Value = 20,
                VAT = 6,
                NotificationEmail = "xpto@lalala.com"
            };
        }

        protected void MockTaxPoliciesRepo()
        {
            _resRoomTaxPolicyRepoMock.Setup(x => x.Delete(It.IsAny<domainReservation.ReservationRoomTaxPolicy>()))
                .Returns((domainReservation.ReservationRoomTaxPolicy entity) =>
                {
                    return entity;
                });
        }

        protected void MockPropertiesRepo(ReservationDataBuilder resBuilder)
        {
            _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>()))
                .Returns((ListPropertyRequest req) =>
                {
                    var ret = listPropertiesLigth.AsQueryable();

                    if (req.ClientNames != null && req.ClientNames.Any())
                        ret = ret.Where(x => req.ClientNames.Contains(x.ClientName));

                    if (req.ClientUIDs != null && req.ClientUIDs.Any())
                        ret = ret.Where(x => req.ClientUIDs.Contains(x.Client_UID));

                    if (req.UIDs != null && req.UIDs.Any())
                        ret = ret.Where(x => req.UIDs.Contains(x.UID));

                    return listPropertiesLigth;
                });

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
            int i = 1;
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
        }

        protected void MockResNumbersGenerator()
        {
            Dictionary<long, List<uint>> __reservationsSeqCache = new Dictionary<long, List<uint>>();
            _reservationsRepoMock.Setup(x => x.GenerateReservationNumber(It.IsAny<long>()))
                .Returns((long propertyId) =>
                {
                    Random rnd = new Random();
                    var number = "" + rnd.Next(1000, 9999);
                    return "RES" + number.PadLeft(6, '0') + "-" + propertyId;
                });
        }

        protected void MockPaymentdetailsGenericRepoMock(ReservationDataBuilder resBuilder, SearchBuilder searchBuilder)
        {
            var ret = new List<domainReservation.ReservationPaymentDetail>();

            //We consider here that we only have one ReservationPaymentDetail (can be changed if we need it)
            if (resBuilder.InputData.reservationPaymentDetail != null)
            {
                ret.Add(resBuilder.InputData.reservationPaymentDetail);
            }

            _reservationPaymentDetailGenericRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.ReservationPaymentDetail, bool>>>()))
                .Returns(ret.AsQueryable());

            _reservationPaymentDetailGenericRepoMock.Setup(x => x.GetQuery())
                .Returns(ret.AsQueryable());
        }

        protected void MockPartialPaymentGenericRepoMock(ReservationDataBuilder resBuilder, SearchBuilder searchBuilder)
        {
            var ret = new List<domainReservation.ReservationPartialPaymentDetail>();

            if (resBuilder.ExpectedData.reservationPartialPaymentDetail != null && resBuilder.ExpectedData.reservationPartialPaymentDetail.Any())
                ret.AddRange(resBuilder.ExpectedData.reservationPartialPaymentDetail);

            _reservationPartialPaymentDetailGenericRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.ReservationPartialPaymentDetail, bool>>>()))
                .Returns(ret.AsQueryable());
        }

        protected void MockRoomExtrasSchedulesGenericRepo(ReservationDataBuilder resBuilder)
        {
            _reservationRoomsExtrasScheduleRepoMock.Setup(x => x.Add(It.IsAny<ReservationRoomExtrasSchedule>()))
                .Returns((ReservationRoomExtrasSchedule entity) =>
                {
                    var tmp = resBuilder.InputData.reservationExtraSchedule[0];  //We considere only the first schedule (can be changed if we need it)
                    entity.UID = tmp.UID;
                    entity.ReservationRoomExtra_UID = tmp.ReservationRoomExtra_UID;

                    return entity;
                });
        }

        protected long ResPartialPaymentCount = 0;
        protected void MockResPartialPayment()
        {
            _resPartialPaymentDetailsRepo.Setup(x => x.Add(It.IsAny<ReservationPartialPaymentDetail>()))
                .Returns((ReservationPartialPaymentDetail entity) =>
                {
                    entity.UID = ++ResPartialPaymentCount;
                    resPartialPaymentList.Add(entity);

                    return entity;
                });

            _resPartialPaymentDetailsRepo.Setup(x => x.GetQuery())
                .Returns(() =>
                {
                    return resPartialPaymentList.AsQueryable();
                });

            _resPartialPaymentDetailsRepo.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<ReservationPartialPaymentDetail, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<ReservationPartialPaymentDetail, bool>> q) =>
                {
                    return resPartialPaymentList.AsQueryable();
                });
        }

        protected void MockTransfersLocations()
        {
            _iOBPPropertyRepoMock.Setup(x => x.ListTransferLocationsForReservation(It.IsAny<ListTransferLocationsForReservationRequest>()))
                .Returns((ListTransferLocationsForReservationRequest req) =>
                {
                    if (req.TransferLocationsIds != null && req.TransferLocationsIds.Any())
                    {
                        return transferLocationList.Where(y => req.TransferLocationsIds.Contains(y.UID)).ToList();
                    }

                    return transferLocationList;
                });
        }

        protected void MockLookups()
        {
            _OBReservationLookupsRepoMock.Setup(x => x.ListReservationLookups(It.IsAny<ListReservationLookupsRequest>()))
                .Returns(new Contracts.Data.Reservations.ReservationLookups
                {
                    ReservationsAdditionalData = new Dictionary<long, Contracts.Data.Reservations.ReservationsAdditionalData>
                    {
                        {1 , new Contracts.Data.Reservations.ReservationsAdditionalData() }
                    },
                    GroupCodesLookup = new Dictionary<long, GroupCode>
                    {
                        {79, new GroupCode() }
                    },
                    PromotionalCodesLookup = new Dictionary<long, PromotionalCode>
                    {
                        {
                            1,
                            new PromotionalCode{
                                UID = 1,
                                Name = "PROMOCODE",
                                Code = "PROMOCODE",
                                MaxReservations = 1,
                                ReservationsCompleted = null,
                                DiscountValue = 0,
                                IsCommission = false,
                                IsDeleted = false,
                                IsPercentage = false,
                                IsPromotionalCodeVisibleRate = true,
                                IsRegisterTPI = false,
                                IsValid = true,
                                URL = "www.promocode.com"
                            }
                        }
                    },
                    GuestsLookup = new Dictionary<long, Guest>
                    {
                        { 284528, new Guest() }
                    },
                });
        }

        //trying to centralize sqlManagerMock for each situation
        protected void MockSqlManager(int basicType = 0)
        {
            MockSqlManager<object>(basicType);
        }

        //trying to centralize sqlManagerMock for each situation
        protected void MockSqlManager<T>(int basicType = 0, List<T> items = null)
        {
            IDbTransaction transaction = null;

            switch (basicType)
            {
                case 0:
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>()));
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>(), ref transaction));
                    _sqlManagerMock.Setup(x => x.ExecuteSql<T>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>(), transaction));
                    break;
                case 1:
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>())).Returns(1);
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>(), ref transaction)).Returns(1);
                    _sqlManagerMock.Setup(x => x.ExecuteSql<T>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>(), transaction)).Returns(items);
                    break;
                case 2:
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>())).Returns(1);
                    _sqlManagerMock.Setup(x => x.ExecuteSql(It.IsAny<string>(), ref transaction)).Returns(1);
                    _sqlManagerMock.Setup(x => x.ExecuteSql<T>(It.IsAny<string>(), null)).Returns(items);
                    break;
            }
        }

        protected void MockCancelReservationReasonRepo()
        {
            var list = new List<CancelReservationReason>
            {
                new CancelReservationReason
                {
                    UID = 1,
                    Name = "name 1",
                    CancelReservationReasonLanguages = new List<CancelReservationReasonLanguage>
                    {
                        new CancelReservationReasonLanguage
                        {
                            UID = 1,
                            Name = "translated name 11",
                            CancelReservationReason_UID = 1,
                            Language_UID = 1
                        },
                        new CancelReservationReasonLanguage
                        {
                            UID = 2,
                            Name = "translated name 12",
                            CancelReservationReason_UID = 1,
                            Language_UID = 2
                        }
                    }
                },
                new CancelReservationReason
                {
                    UID = 2,
                    Name = "name 2",
                    CancelReservationReasonLanguages = new List<CancelReservationReasonLanguage>
                    {
                        new CancelReservationReasonLanguage
                        {
                            UID = 3,
                            Name = "translated name 21",
                            CancelReservationReason_UID = 2,
                            Language_UID = 3
                        }
                    }
                },
                new CancelReservationReason
                {
                    UID = 3,
                    Name = "name 3",
                    CancelReservationReasonLanguages = new List<CancelReservationReasonLanguage>()
                }
            };

            _CancelReservationReasonRepoMock.Setup(x => x.GetAll()).Returns(list);
        }

        protected void MockOBBePartialPaymentMethodsRepo()
        {
            _OBBePartialPaymentCcMethodRepository.Setup(m => m.ListBePartialPaymentCcMethods(It.IsAny<ListBePartialPaymentCcMethodRequest>()))
                                                   .Returns((ListBePartialPaymentCcMethodRequest request) =>
                                                   {
                                                       var result = bePartialPaymentCcMethodList.AsQueryable();

                                                       if (request.Ids != null && request.Ids.Count > 0)
                                                           result = result.Where(x => request.Ids.Contains(x.Id));

                                                       if (request.Parcels != null && request.Parcels.Count > 0)
                                                           result = result.Where(x => request.Parcels.Contains(x.Parcel));

                                                       if (request.PropertyIds != null && request.PropertyIds.Count > 0)
                                                           result = result.Where(x => request.PropertyIds.Contains(x.PropertyId));

                                                       if (request.PaymentMethodsIds != null && request.PaymentMethodsIds.Count > 0)
                                                           result = result.Where(x => x.PaymentMethodsId.HasValue && request.PaymentMethodsIds.Contains(x.PaymentMethodsId.Value));

                                                       if (request.PaymentMethodTypesIds != null && request.PaymentMethodTypesIds.Count > 0)
                                                           result = result.Where(x => x.PaymentMethodTypeId.HasValue && request.PaymentMethodTypesIds.Contains(x.PaymentMethodTypeId.Value));


                                                       return new ListBePartialPaymentCcMethodResponse
                                                       {
                                                           Status = Status.Success,
                                                           Errors = new List<Error>(),
                                                           Warnings = new List<Warning>(),
                                                           RequestGuid = request.RequestGuid,
                                                           RequestId = request.RequestId,
                                                           Result = result.ToList(),
                                                           TotalRecords = result.Count()
                                                       };
                                                   });
        }

        protected void MockOBBeSettingsRepo()
        {
            _OBBeSettingsRepository.Setup(m => m.ListBeSettings(It.IsAny<ListBeSettingsRequest>()))
                                                   .Returns((ListBeSettingsRequest request) =>
                                                   {
                                                       var result = new List<Contracts.Data.BE.BESettings>
                                                        {
                                                            new Contracts.Data.BE.BESettings
                                                            {
                                                                PartialPaymentMinimumAllowed = 0,
                                                                AllowPartialPayment = true,
                                                            }
                                                        };

                                                       return new ListBeSettingsResponse
                                                       {
                                                           Status = Status.Success,
                                                           Errors = new List<Error>(),
                                                           Warnings = new List<Warning>(),
                                                           RequestGuid = request.RequestGuid,
                                                           RequestId = request.RequestId,
                                                           Result = result,
                                                           TotalRecords = result.Count
                                                       };
                                                   });
        }


        protected void MockOBBeSettingsRepo(bool? notAllowCancelReservation)
        {
            _OBBeSettingsRepository.Setup(m => m.ListBeSettings(It.IsAny<ListBeSettingsRequest>()))
                                                   .Returns((ListBeSettingsRequest request) =>
                                                   {
                                                       var result = new List<Contracts.Data.BE.BESettings>
                                                       {
                                                            new Contracts.Data.BE.BESettings
                                                            {
                                                                PartialPaymentMinimumAllowed = 0,
                                                                AllowPartialPayment = true,
                                                                NotAllowCancelReservation = notAllowCancelReservation
                                                            }
                                                        };

                                                       return new ListBeSettingsResponse
                                                       {
                                                           Status = Status.Success,
                                                           Errors = new List<Error>(),
                                                           Warnings = new List<Warning>(),
                                                           RequestGuid = request.RequestGuid,
                                                           RequestId = request.RequestId,
                                                           Result = result,
                                                           TotalRecords = result.Count
                                                       };
                                                   });
        }


        protected void MockOBPropertyRepo()
        {
            _iOBPPropertyRepoMock.Setup(m => m.ListProactiveActions(It.IsAny<ListProactiveActionsRequest>()))
                                                   .Returns((ListProactiveActionsRequest request) =>
                                                   {
                                                       return proactiveActionsInDataBase;
                                                   });
        }


        //_iOBPPropertyRepoMock
        #endregion

        #region Mock POCO Calls
        protected void MockReservationHelperPockMethods(bool mockGuest = false, IEnumerable<OB.BL.Contracts.Data.Rates.Extra> extras = null)
        {
            _reservationHelperMock.Setup(x => x.GetReservationLookups(It.IsAny<ListReservationCriteria>(), It.IsAny<IEnumerable<domainReservation.Reservation>>()))
                .Returns(
                (DL.Common.ListReservationCriteria request, IEnumerable<domainReservation.Reservation> result) =>
                {
                    return new ReservationLookups
                    {
                        GuestsLookup = mockGuest ? new Dictionary<long, Guest>
                                        {
                                            { 284528, new Guest() }
                                        } : new Dictionary<long, Guest>(),
                        GroupCodesLookup = groupCodeList.ToDictionary(x => x.UID),
                        PromotionalCodesLookup = promocodesList.ToDictionary(x => x.UID),
                        ExtrasLookup = (extras ?? Enumerable.Empty<OB.BL.Contracts.Data.Rates.Extra>()).ToDictionary(x => x.UID)
                    };
                });


            _reservationHelperMock.Setup(x => x.GetMostRestrictiveCancelationPolicy(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>>(), It.IsAny<bool>()))
                .Returns((DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList, bool forceDefaultPolicy) =>
                {
                    return cancellationPolicyList.FirstOrDefault();
                });
        }
        #endregion

        #region Aux Methods
        protected string GetCurrencySymbol(long currencyUid)
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

        protected int GetAdultPriceTest(RateRoomDetail rrd, int numAds)
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
        protected int GetChildPriceTest(RateRoomDetail rrd, int numChs, int freeChilds = 0, bool countAsAdult = false)
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

        public static Contracts.Data.CRM.ThirdPartyIntermediaryLight ConvertToLigth(OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi)
        {
            return new ThirdPartyIntermediaryLight()
            {
                IsActive = tpi.IsActive.GetValueOrDefault(),
                IsDeleted = tpi.IsDeleted.GetValueOrDefault(),
                Name = tpi.Name,
                TravelAgent_UID = tpi.TravelAgent_UID,
                UID = tpi.UID
            };
        }

        #region Aux Credit
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedValue"></param>
        /// <returns></returns>
        protected string DecryptCreditCard(string encryptedValue)
        {
            // Kill the Exception
            if (String.IsNullOrEmpty(encryptedValue)) return encryptedValue;

            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(GetEncryptionRsaKey());

                    var resultBytes = Convert.FromBase64String(encryptedValue);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);

                    return ShakeCreditCard(Encoding.UTF8.GetString(decryptedBytes));
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ccNumber"></param>
        /// <returns></returns>
        protected string ShakeCreditCard(string ccNumber)
        {
            // Kill the Exception
            if (String.IsNullOrEmpty(ccNumber)) return ccNumber;

            var lenght = ccNumber.Length;
            if (lenght < 4) return ccNumber;

            var lastFourDigits = ccNumber.Substring(lenght - 4);
            var initialDigits = ccNumber.Substring(0, lenght - 4);

            // Invert the Order
            char[] charArray = lastFourDigits.ToCharArray();
            Array.Reverse(charArray);
            lastFourDigits = new string(charArray);

            // Return the transformed number
            return initialDigits + lastFourDigits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetEncryptionRsaKey()
        {
            return "<RSAKeyValue><Modulus>49BStnomTkKFC1ERxGA/MhfVuwwNW8JcO52FKXFWfpb2m8a/f0Q3l7FRz6pC8Hs65+BOIwUN/7RxGV+PzIQ4ZwVwEuY7GBQonjXTP3D20yUnXZA/NtGmxWYJlVnD8VUZLQdkKs3hBAfKnWjyOgnGoq7CIFyJ9KKOxRh39hQ+7TE=</Modulus><Exponent>AQAB</Exponent><P>+hwRM4IsoIyrwSlbl01L57gZk41RHSYkcZwnsrjodAc/0KSCPm4JhKn8CKc9Xi8Rjq5ekJ0xSbS9VPAmJrwQrQ==</P><Q>6S3VBl3OF3rVnwg9Cg0bc+FGIfHgbZcCw1gV/k2awmS6GKpQHZ7Dr5UnH+QRXSJ1IQ3PJ6rieEeUsGhZxiarFQ==</Q><DP>ZMj0oYX+R8AH4jGxR9oNEVYdcFkM66sYGnPrh1h9y2u0anYwScn7qer5td72msJq180qLCo711CuztBq/0bfjQ==</DP><DQ>YQ7Zv8el9DIF3ydfuOJRzf8z4Qc8AoG7/bGZnfuRcl7Y81FY/atLCrfLzENzUs/37yU/V+SSVbx90Jvu2kLYLQ==</DQ><InverseQ>aq95HffRtKIVEpCEjyolZxm8ovf5SpfT2dkXzbxd7hIPNuM9mNWu897q9KKv3EyEY36dAryiIetVMhFGK0HzuQ==</InverseQ><D>Dyk6j/VSHkw0AXxMM+b53bITYcbcDrrBG6CQj6EA0hzm3Zgc/3HBR2GgIbNhkBKLaYoWeSMpetZ93mPrNH+qJyTeKIPx7mLGu14GS40uikfYIMhRlihRfNskYfiTXsRA35GFdyTgLCPo+OJXVxqaFZpjwXGoBri7Vlgz9E+LRak=</D></RSAKeyValue>";
        }

        #endregion

        protected void AddTranslateToMockOtherPolicy(OtherPolicy otherPolicy, Domain.OtherPoliciesLanguage policyTranslate)
        {
            var tmp = otherPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            otherPolicyList.Remove(otherPolicy);
            otherPolicyList.Add(tmp);
        }
        protected void AddTranslateToMockCancellationPolicy(CancellationPolicy cancellationPolicy, CancellationPoliciesLanguage policyTranslate)
        {
            var tmp = cancellationPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            cancellationPolicyList.Remove(cancellationPolicy);
            cancellationPolicyList.Add(tmp);
        }
        protected void AddTranslateToMockDepositPolicy(DepositPolicy depositPolicy, DepositPoliciesLanguage policyTranslate)
        {
            var tmp = depositPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            depositPolicyList.Remove(depositPolicy);
            depositPolicyList.Add(tmp);
        }
        protected void AddSpecialRequestToGuest(ReservationDataBuilder resBuilder, List<long> list)
        {
            var listRequests = beSpecRequestsList.Where(x => list.Contains(x.UID));

            foreach (var r in listRequests)
            {
                if (resBuilder.ExpectedData.guest.GuestFavoriteSpecialRequests == null)
                    resBuilder.ExpectedData.guest.GuestFavoriteSpecialRequests = new List<Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest>();

                resBuilder.ExpectedData.guest.GuestFavoriteSpecialRequests.Add(new Reservation.BL.Contracts.Data.CRM.GuestFavoriteSpecialRequest()
                {
                    //UID = r.UID,
                    Guest_UID = resBuilder.InputData.guest.UID,
                    BESpecialRequests_UID = r.UID
                });
            }
        }

        #endregion
    }
}
