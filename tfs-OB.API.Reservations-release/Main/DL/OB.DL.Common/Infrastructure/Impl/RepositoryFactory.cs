using Couchbase;
using Couchbase.Authentication;
using Couchbase.Core;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using OB.Api.Core;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Infrastructure.Impl.Interceptors;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Repositories.Impl.Cached;
using OB.DL.Common.Repositories.Impl.Couchbase;
using OB.DL.Common.Repositories.Impl.Entity;
using OB.DL.Common.Repositories.Impl.Rest;
using OB.DL.Common.Repositories.Impl.SqlServer;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Couchbase;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.DL.Common.Repositories.Interfaces.SqlServer;
using OB.Domain;
using OB.Domain.Payments;
using OB.Domain.Reservations;
using OB.Log;
using OB.Reservation.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

//using OB.Domain.ProactiveActions;

namespace OB.DL.Common.Impl
{
    internal class PerUnitOfWorkLifetimeManager : LifetimeManager
    {    
        private ISessionFactory _sessionFactory;

        [ThreadStatic]
        private static Dictionary<string, object> values;

        private readonly string key;
      
        public PerUnitOfWorkLifetimeManager(ISessionFactory factory)
            : base()
        {
            _sessionFactory = factory;
            key = Guid.NewGuid().ToString();
        }

        public PerUnitOfWorkLifetimeManager(ISessionFactory factory, IUnitOfWork unitOfWork)
            : this(factory)
        {
            _sessionFactory = factory;
            key = Guid.NewGuid().ToString();           
        }

        public override object GetValue()
        {         
            var unitOfWorkGuid = _sessionFactory.GetUnitOfWork().Guid;
                
            
            PerUnitOfWorkLifetimeManager.EnsureValues();
            object result;
            PerUnitOfWorkLifetimeManager.values.TryGetValue(key + "_" + unitOfWorkGuid.ToString(), out result);
            
            return result;            
        }

        public override void RemoveValue()
        {           
            if (PerUnitOfWorkLifetimeManager.values != null) {
                var localUoW = _sessionFactory.CurrentUnitOfWork;
                if (localUoW != null)
                {
                    RemoveValue(localUoW.Guid);
                }             
            }           
        }

        private void RemoveValue(Guid uowGuid)
        {
            string guid = key + "_" + uowGuid.ToString();
            object result;
            if (PerUnitOfWorkLifetimeManager.values.TryGetValue(guid, out result))
            {
                if (result is IDisposable)
                    ((IDisposable)result).Dispose();
                PerUnitOfWorkLifetimeManager.values.Remove(guid);
            }
        }

        public override void SetValue(object newValue)
        {
            PerUnitOfWorkLifetimeManager.EnsureValues();

            var localUnitOfWork = _sessionFactory.GetUnitOfWork();
            string guid = key + "_" + localUnitOfWork.Guid.ToString();

            object result;
            if (PerUnitOfWorkLifetimeManager.values.TryGetValue(guid, out result))
            {
                RemoveValue();                
            }
            localUnitOfWork.OnDispose += LocalUnitOfWork_OnDispose; ;


            PerUnitOfWorkLifetimeManager.values[guid] = newValue;            
        }

        private void LocalUnitOfWork_OnDispose(IUnitOfWork obj)
        {
            RemoveValue(obj.Guid);
        }

        void _unitOfWork_OnDispose(object sender, EventArgs e)
        {
            RemoveValue(((IUnitOfWork)sender).Guid);
        }

        private static void EnsureValues()
        {
            if (PerUnitOfWorkLifetimeManager.values == null)
            {
                PerUnitOfWorkLifetimeManager.values = new Dictionary<string, object>();
            }
        }
    }

    /// <summary>   Repository factory. </summary>
    /// <seealso cref="IRepositoryFactory"/>
    class RepositoryFactory : IRepositoryFactory
    {
        readonly ILogger _logger;

        public RepositoryFactory(IUnityContainer container)
        {
            _logger = LogsManager.CreateLogger(typeof(RepositoryFactory));

            Container = container;

            var sessionFactory = container.Resolve<ISessionFactory>();

            InjectionFactory injectionFactory = null;
            var interceptor = new Interceptor<InterfaceInterceptor>();
            var interceptorBehavior = new InterceptionBehavior<LoggingInterceptionBehavior>();

            #region GENERAL REPOSITORIES

            Container.RegisterType<IRepository<OB.Domain.Reservations.Reservation>, ReservationsRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationsRepository(FindContext(sessionFactory.GetUnitOfWork(), OB.Domain.Reservations.Reservation.DomainScope), GetRepository<ReservationsAdditionalData>(FindContext(sessionFactory.GetUnitOfWork(), OB.Domain.Reservations.Reservation.DomainScope)), GetOBRateRepository())), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationPartialPaymentDetail>, Repository<ReservationPartialPaymentDetail>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationPartialPaymentDetail>(FindContext(sessionFactory.GetUnitOfWork(), ReservationPartialPaymentDetail.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationPaymentDetail>, Repository<ReservationPaymentDetail>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationPaymentDetail>(FindContext(sessionFactory.GetUnitOfWork(), ReservationPaymentDetail.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoom>, Repository<ReservationRoom>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoom>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoom.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoom>, ReservationRoomRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoom.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomChild>, Repository<ReservationRoomChild>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoomChild>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomChild.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomDetail>, Repository<ReservationRoomDetail>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoomDetail>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomDetail.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomDetail>, ReservationRoomDetailRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomDetailRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomDetail.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomExtra>, Repository<ReservationRoomExtra>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoomExtra>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomExtra.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomExtrasAvailableDate>, Repository<ReservationRoomExtrasAvailableDate>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoomExtrasAvailableDate>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomExtrasAvailableDate.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomExtrasSchedule>, Repository<ReservationRoomExtrasSchedule>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationRoomExtrasSchedule>(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomExtrasSchedule.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationsAdditionalData>, Repository<ReservationsAdditionalData>>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new Repository<ReservationsAdditionalData>(FindContext(sessionFactory.GetUnitOfWork(), ReservationsAdditionalData.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationsHistory>, ReservationHistoryRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationHistoryRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationsHistory.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationFilter>, ReservationsFilterRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationsFilterRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationFilter.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<ReservationRoomFilter>, ReservationRoomFilterRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomFilterRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomFilter.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<TokenizedCreditCardsReadsPerMonth>, TokenizedCreditCardsReadsPerMonthRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new TokenizedCreditCardsReadsPerMonthRepository(FindContext(sessionFactory.GetUnitOfWork(), TokenizedCreditCardsReadsPerMonth.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<VisualState>, VisualStateRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new VisualStateRepository(FindContext(sessionFactory.GetUnitOfWork(), VisualState.DomainScope))), interceptor, interceptorBehavior);
            Container.RegisterType<IRepository<PaymentGatewayTransaction>, PaymentGatewayTransactionRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new PaymentGatewayTransactionRepository(FindContext(sessionFactory.GetUnitOfWork(), PaymentGatewayTransaction.DomainScope))), interceptor, interceptorBehavior);
            
            #endregion

            #region SPECIALIZED REPOSITORIES
            Container.RegisterType<IReservationHistoryRepository, ReservationHistoryRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationHistoryRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationsHistory.DomainScope))));
            Container.RegisterType<IReservationRoomDetailRepository, ReservationRoomDetailRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomDetailRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomDetail.DomainScope))));
            Container.RegisterType<IReservationRoomRepository, ReservationRoomRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoom.DomainScope))));
            Container.RegisterType<IReservationsRepository, ReservationsRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationsRepository(FindContext(sessionFactory.GetUnitOfWork(), OB.Domain.Reservations.Reservation.DomainScope), GetRepository<ReservationsAdditionalData>(FindContext(sessionFactory.GetUnitOfWork(), OB.Domain.Reservations.Reservation.DomainScope)), GetOBRateRepository())));
            Container.RegisterType<IPortalRepository, PortalRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new PortalRepository()));
            Container.RegisterType<IReservationsFilterRepository, ReservationsFilterRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationsFilterRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationFilter.DomainScope))));
            Container.RegisterType<IReservationRoomFilterRepository, ReservationRoomFilterRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ReservationRoomFilterRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationRoomFilter.DomainScope))));
            Container.RegisterType<ITokenizedCreditCardsReadsPerMonthRepository, TokenizedCreditCardsReadsPerMonthRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new TokenizedCreditCardsReadsPerMonthRepository(FindContext(sessionFactory.GetUnitOfWork(), TokenizedCreditCardsReadsPerMonth.DomainScope))));
            Container.RegisterType<IVisualStateRepository, VisualStateRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new VisualStateRepository(FindContext(sessionFactory.GetUnitOfWork(), VisualState.DomainScope))));
            //Container.RegisterType<IPropertyEventsRepository, PropertyEventsRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new PropertyEventsRepository(FindContext(sessionFactory.GetUnitOfWork(), PropertyEvent.DomainScope))));
            //Container.RegisterType<IPropertyQueueRepository, PropertyQueueRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new PropertyQueueRepository(FindContext(sessionFactory.GetUnitOfWork(), PropertyQueue.DomainScope))));
            Container.RegisterType<IPaymentGatewayTransactionRepository, PaymentGatewayTransactionRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new PaymentGatewayTransactionRepository(FindContext(sessionFactory.GetUnitOfWork(), PaymentGatewayTransaction.DomainScope))));
            Container.RegisterType<ILostReservationsRepository, LostReservationsRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new LostReservationsRepository(FindContext(sessionFactory.GetUnitOfWork(), LostReservation.DomainScope))));

            #endregion

            #region REST REPOSITORIES

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.PromotionalCode>, RestRepository<OB.BL.Contracts.Data.Rates.PromotionalCode>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.PromotionalCode>()), interceptor, interceptorBehavior);

            Container.RegisterType<IOBPromotionalCodeRepository, OBPromotionalCodeRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBPromotionalCodeRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.RateBuyerGroup>, RestRepository<OB.BL.Contracts.Data.Rates.RateBuyerGroup>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.RateBuyerGroup>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBRateBuyerGroupRepository, OBRateBuyerGroupRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBRateBuyerGroupRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Properties.ChildTerm>, RestRepository<OB.BL.Contracts.Data.Properties.ChildTerm>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Properties.ChildTerm>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBChildTermsRepository, OBChildTermsRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBChildTermsRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Properties.Incentive>, RestRepository<OB.BL.Contracts.Data.Properties.Incentive>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Properties.Incentive>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBIncentiveRepository, OBIncentiveRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBIncentiveRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>, RestRepository<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBRateRoomDetailsForReservationRoomRepository, OBRateRoomDetailsForReservationRoomRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBRateRoomDetailsForReservationRoomRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.General.Currency>, RestRepository<OB.BL.Contracts.Data.General.Currency>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.General.Currency>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBCurrencyRepository, OBCurrencyRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBCurrencyRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.CancellationPolicy>, RestRepository<OB.BL.Contracts.Data.Rates.CancellationPolicy>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.CancellationPolicy>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBCancellationPolicyRepository, OBCancellationPolicyRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBCancellationPolicyRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.DepositPolicy>, RestRepository<OB.BL.Contracts.Data.Rates.DepositPolicy>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.DepositPolicy>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBDepositPolicyRepository, OBDepositPolicyRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBDepositPolicyRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.OtherPolicy>, RestRepository<OB.BL.Contracts.Data.Rates.OtherPolicy>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.OtherPolicy>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBOtherPolicyRepository, OBOtherPolicyRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBOtherPolicyRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.Extra>, RestRepository<OB.BL.Contracts.Data.Rates.Extra>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.Extra>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBExtrasRepository, OBExtrasRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBExtrasRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Payments.PaymentMethodType>, RestRepository<OB.BL.Contracts.Data.Payments.PaymentMethodType>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Payments.PaymentMethodType>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBPaymentMethodTypeRepository, OBPaymentMethodTypeRepository>(new PerThreadLifetimeManager(), new InjectionFactory((c) => new OBPaymentMethodTypeRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.General.Setting>, RestRepository<OB.BL.Contracts.Data.General.Setting>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.General.Setting>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBAppSettingRepository, OBAppSettingRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBAppSettingRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Properties.PropertyLight>, RestRepository<OB.BL.Contracts.Data.Properties.PropertyLight>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Properties.PropertyLight>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBPropertyRepository, OBPropertyRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBPropertyRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Channels.Channel>, RestRepository<OB.BL.Contracts.Data.Channels.Channel>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Channels.Channel>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBChannelRepository, OBChannelRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBChannelRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.CRM.Guest>, RestRepository<OB.BL.Contracts.Data.CRM.Guest>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.CRM.Guest>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBCRMRepository, OBCRMRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBCRMRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.General.User>, RestRepository<OB.BL.Contracts.Data.General.User>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.General.User>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBUserRepository, OBUserRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBUserRepository()));

            Container.RegisterType<IOBSecurityRepository, OBSecurityRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBSecurityRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Rates.Rate>, RestRepository<OB.BL.Contracts.Data.Rates.Rate>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Rates.Rate>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBRateRepository, OBRateRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBRateRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.ProactiveActions.ProactiveAction>, RestRepository<OB.BL.Contracts.Data.ProactiveActions.ProactiveAction>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.ProactiveActions.ProactiveAction>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBPropertyEventsRepository, OBPropertyEventsRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBPropertyEventsRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.PMS.PMS>, RestRepository<OB.BL.Contracts.Data.PMS.PMS>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.PMS.PMS>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBPMSRepository, OBPMSRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBPMSRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Reservations.ReservationLookups>, RestRepository<OB.BL.Contracts.Data.Reservations.ReservationLookups>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Reservations.ReservationLookups>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBReservationLookupsRepository, OBReservationLookupsRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBReservationLookupsRepository()));

            Container.RegisterType<IRestRepository<OB.BL.Contracts.Data.Payments.BePartialPaymentCcMethod>, RestRepository<OB.BL.Contracts.Data.Payments.BePartialPaymentCcMethod>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<OB.BL.Contracts.Data.Payments.BePartialPaymentCcMethod>()), interceptor, interceptorBehavior);
            Container.RegisterType<IOBBePartialPaymentCcMethodRepository, OBBePartialPaymentCcMethodRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBBePartialPaymentCcMethodRepository()));
            Container.RegisterType<IOBBeSettingsRepository, OBBeSettingsRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new OBBeSettingsRepository()));

            Container.RegisterType<IRestRepository<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>, RestRepository<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new RestRepository<ES.API.Contracts.Requests.CheckRemoteAvailabilityRequest>()), interceptor, interceptorBehavior);
            Container.RegisterType<IExternalSystemsRepository, ExternalSystemsRepository>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new ExternalSystemsRepository()));
            #endregion

            #region SQL REPOSITORIES

            Container.RegisterType<ISqlManager, SqlManager>(Constants.OmnibeesConnectionString, new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new SqlManager(FindSqlContext(sessionFactory.GetUnitOfWork(), DomainScopes.Omnibees))));
            Container.RegisterType<IThirdPartyIntermediarySqlRepository, ThirdPartyIntermediarySqlRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new ThirdPartyIntermediarySqlRepository(FindSqlContext(sessionFactory.GetUnitOfWork(), DomainScopes.Omnibees))));
            Container.RegisterType<IOperatorSqlRepository, OperatorSqlRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), new InjectionFactory((c) => new OperatorSqlRepository(FindSqlContext(sessionFactory.GetUnitOfWork(), DomainScopes.Omnibees))));

            #endregion

            #region CACHED REPOSITORIES

            injectionFactory = new InjectionFactory((c) => new GroupRulesRepository(c.Resolve<ICacheProvider>()));
            Container.RegisterType<IRepository<GroupRule>, GroupRulesRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<IGroupRulesRepository, GroupRulesRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            injectionFactory = new InjectionFactory((c) => new ReservationStatusRepository(FindContext(sessionFactory.GetUnitOfWork(), ReservationStatus.DomainScope), c.Resolve<ICacheProvider>()));
            Container.RegisterType<IRepository<ReservationStatus>, ReservationStatusRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<IReservationStatusRepository, ReservationStatusRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            injectionFactory = new InjectionFactory((c) => new CancelReservationReasonRepository(FindContext(sessionFactory.GetUnitOfWork(), CancelReservationReason.DomainScope), c.Resolve<ICacheProvider>()));
            Container.RegisterType<IRepository<CancelReservationReason>, CancelReservationReasonRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<ICancelReservationReasonRepository, CancelReservationReasonRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            #endregion

            #region COUCHDB BUCKETS AND REPOSITORIES

            ICluster cluster = null;

            try
            {
                cluster = new Cluster("couchbaseClients/couchbase");

                if (Configuration.CouchbaseVersion == 5)
                {
                    var authenticator = new PasswordAuthenticator(Configuration.CouchbaseUsername, Configuration.CouchbasePassword);
                    cluster.Authenticate(authenticator);
                }

                Container.RegisterInstance<Couchbase.Core.ICluster>(cluster, new ContainerControlledLifetimeManager());
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couchbase configuration section 'couchbaseClients/couchbase' missing in app.config or web.config. This WILL AFFECT ALL COUCHBASE REPOSITORY DEPENDENCIES");
            }

            injectionFactory = new InjectionFactory((c) => cluster.OpenBucket("LostReservations"));
            Container.RegisterType<Couchbase.Core.IBucket>("LostReservations", new ContainerControlledLifetimeManager(), injectionFactory);

            injectionFactory = new InjectionFactory((c) => cluster.OpenBucket("EventLog"));
            Container.RegisterType<Couchbase.Core.IBucket>("NotificationBase", new ContainerControlledLifetimeManager(), injectionFactory);

            injectionFactory = new InjectionFactory((c) => cluster.OpenBucket("OmnibeesEvents"));
            Container.RegisterType<Couchbase.Core.IBucket>("OmnibeesEvents", new ContainerControlledLifetimeManager(), injectionFactory);


            injectionFactory = new InjectionFactory((c) => new LostReservationDetailRepository(c.Resolve<Couchbase.Core.IBucket>("LostReservations")));
            Container.RegisterType<IRepository<LostReservationDetail>, LostReservationDetailRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<ILostReservationDetailRepository, LostReservationDetailRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            injectionFactory = new InjectionFactory((c) => new NotificationBaseRepository(c.Resolve<Couchbase.Core.IBucket>("NotificationBase")));
            Container.RegisterType<IRepository<NotificationBase>, NotificationBaseRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<INotificationBaseRepository, NotificationBaseRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            injectionFactory = new InjectionFactory((c) => new ReservationLogRepository(c.Resolve<Couchbase.Core.IBucket>("OmnibeesEvents")));
            Container.RegisterType<IRepository<OB.Domain.Reservations.ReservationGridLineDetail>, ReservationLogRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory, interceptor, interceptorBehavior);
            Container.RegisterType<IReservationLogRepository, ReservationLogRepository>(new PerUnitOfWorkLifetimeManager(sessionFactory), injectionFactory);

            #endregion
        }

        /// <summary>
        /// Injected by IoC
        /// </summary>
        [Microsoft.Practices.Unity.Dependency]
        public IUnityContainer Container { get; set; }

        /// <summary>
        /// Gets the SQL manager that allows query execution agains the given UnitOfWork.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public virtual ISqlManager GetSqlManager(IUnitOfWork unitOfWork, DomainScope scope)
        {
            Contract.Requires(unitOfWork != null);
            return new SqlManager(FindContext(unitOfWork, scope));
        }

        /// <summary>
        /// Gets the SQL manager that allows query execution agains PortalOperators Database.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        protected ISqlManager GetSqlManager(IUnitOfWork unitOfWork, string connectionString)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
            Contract.Requires(unitOfWork != null);
            return new SqlManager(new SqlConnection(connectionString));
        }

        /// <summary>
        /// Gets the SQL manager that allows query execution agains PortalOperators Database.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public ISqlManager GetSqlManager(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));          
            return Container.Resolve<ISqlManager>(key);
        }

        /// <summary>Gets the repository.</summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        public virtual IRepository<TEntity> GetRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : DomainObject
        {
            try
            {
                return Container.Resolve<IRepository<TEntity>>();
            }
            catch (ResolutionFailedException ex)
            {
                //ignore and try to register missing type
            }

            var sessionFactory = Container.Resolve<ISessionFactory>();
            Container.RegisterType(typeof(IRepository<TEntity>), new PerUnitOfWorkLifetimeManager(sessionFactory),
                new InjectionFactory((c) =>
                {
                    var domainScope = GetEntityDomainScope<TEntity>();
                    var unitOfWorkForThread = c.Resolve<ISessionFactory>().GetUnitOfWork(domainScope);
                    var innerContext = FindContext(unitOfWorkForThread, domainScope);
                    return new Repository<TEntity>(innerContext);
                }));
            return Container.Resolve<IRepository<TEntity>>();

        }

        /// <summary>Gets the repository.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext">The db context.</param>
        /// <returns></returns>
        public virtual IRepository<TEntity> GetRepository<TEntity>(IObjectContext dbContext) where TEntity : DomainObject
        {
            if (Container.IsRegistered<IRepository<TEntity>>())
            {
                return Container.Resolve<IRepository<TEntity>>();
            }

            Contract.Requires(dbContext != null);

            Container.RegisterType(typeof(IRepository<TEntity>), new PerUnitOfWorkLifetimeManager(Container.Resolve<ISessionFactory>()),
                new InjectionFactory((c) => new Repository<TEntity>(dbContext)));

            return Container.Resolve<IRepository<TEntity>>();
        }

        /// <summary>
        /// Gets the DomainScope from an entity that implements DomainObject
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DomainScope GetEntityDomainScope<TEntity>() where TEntity : DomainObject
        {
            var entityType = typeof(TEntity);
            DomainScope entityDomainScope = entityType.GetField("DomainScope").GetValue(entityType) as DomainScope;
            return entityDomainScope;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IObjectContext FindContext(IUnitOfWork unitOfWork, DomainScope scope)
        {
            IObjectContext context = null;
            if (unitOfWork is UnitOfWorkBase uof)
            {
                context = uof.GetContext(scope);
            }
            else throw new NotSupportedException($"Only {nameof(UnitOfWorkBase)} class is supported as an implementation. Please override this method and implement the specific behavior");

            return context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IDbConnection FindSqlContext(IUnitOfWork unitOfWork, DomainScope scope)
        {
            IDbConnection context = null;
            if (unitOfWork is UnitOfWorkBase uof)
            {
                context = uof.GetSqlContext(scope);
            }
            else throw new NotSupportedException($"Only {nameof(UnitOfWorkBase)} class is supported as an implementation. Please override this method and implement the specific behavior");

            return context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IObjectContext FindContext(IEnumerable<IObjectContext> contexts, DomainScope scope)
        {
            //find the correct context
            foreach (var innerContext in contexts)
            {
                if (innerContext.Context.GetType().Name.Split(new string[] { "Context" },
                    StringSplitOptions.RemoveEmptyEntries)[0].Equals(scope.Name))
                {
                    return innerContext;
                };
            }
            return null;
        }

        #region Specialized Repository Getters

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual INotificationBaseRepository GetNotificationBaseRepository()
        {
            return Container.Resolve<INotificationBaseRepository>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationLogRepository GetReservationLogRepository()
        {
            return Container.Resolve<IReservationLogRepository>();
        }

        /// <summary>
        /// Gets the ReservationStatus specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationStatusRepository GetReservationStatusRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IReservationStatusRepository>();
        }

        /// <summary>
        /// Gets the CancelReservationReason specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ICancelReservationReasonRepository GetCancelReservationReasonRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<ICancelReservationReasonRepository>();
        }

        /// <summary>
        /// Gets the Reservations specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationsRepository GetReservationsRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IReservationsRepository>();
        }

        /// <summary>
        /// Gets the ReservationsHistoryRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationHistoryRepository GetReservationHistoryRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IReservationHistoryRepository>();
        }

        /// <summary>
        /// Gets the ReservationRoom specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationRoomRepository GetReservationRoomRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IReservationRoomRepository>();
        }

        /// <summary>
        /// Gets the ReservationRoomDetail specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationRoomDetailRepository GetReservationRoomDetailRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IReservationRoomDetailRepository>();
        }

        /// <summary>
        /// Gets the GroupRules Cached Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IGroupRulesRepository GetGroupRulesRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IGroupRulesRepository>();
        }

        /// <summary>
        /// Gets the Portal Repository Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPortalRepository GetPortalRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);

            return Container.Resolve<IPortalRepository>();
        }

        /// <summary>
        /// Gets the ReservationsFilter Repository Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationsFilterRepository GetReservationsFilterRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);

            return Container.Resolve<IReservationsFilterRepository>();
        }

        /// <summary>
        /// Gets the ReservationRoomFilter Repository Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IReservationRoomFilterRepository GetReservationRoomFilterRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);

            return Container.Resolve<IReservationRoomFilterRepository>();
        }

        /// <summary>
        /// Gets the TokenizedCreditCardsReadsPerMonth Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITokenizedCreditCardsReadsPerMonthRepository GetTokenizedCreditCardsReadsPerMonthRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);

            return Container.Resolve<ITokenizedCreditCardsReadsPerMonthRepository>();
        }

        #region CouchBase Repositories

 

        #endregion

        #endregion Specialized Repository Getters


        public ILostReservationDetailRepository GetLostReservationDetailRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);

            return Container.Resolve<ILostReservationDetailRepository>();
        }

        /// <summary>
        /// Gets the VisualStateRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IVisualStateRepository GetVisualStateRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IVisualStateRepository>();
        }

        /// <summary>
        /// Gets the OBPromotionalCodeRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBPromotionalCodeRepository GetOBPromotionalCodeRepository()
        {
            return Container.Resolve<IOBPromotionalCodeRepository>();
        }

        /// <summary>
        /// Gets the OBRateBuyerGroupRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBRateBuyerGroupRepository GetOBRateBuyerGroupRepository()
        {
            return Container.Resolve<IOBRateBuyerGroupRepository>();
        }

        /// <summary>
        /// Gets the OBChildTermsRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBChildTermsRepository GetOBChildTermsRepository()
        {
            return Container.Resolve<IOBChildTermsRepository>();
        }

        /// <summary>
        /// Gets the OBIncentiveRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBIncentiveRepository GetOBIncentiveRepository()
        {
            return Container.Resolve<IOBIncentiveRepository>();
        }

        /// <summary>
        /// Gets the OBRateRoomDetailsForReservationRoomRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBRateRoomDetailsForReservationRoomRepository GetOBRateRoomDetailsForReservationRoomRepository()
        {
            return Container.Resolve<IOBRateRoomDetailsForReservationRoomRepository>();
        }

        /// <summary>
        /// Gets the OBCurrencyRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBCurrencyRepository GetOBCurrencyRepository()
        {
            return Container.Resolve<IOBCurrencyRepository>();
        }

        /// <summary>
        /// Gets the OBCancellationPolicyRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBCancellationPolicyRepository GetOBCancellationPolicyRepository()
        {
            return Container.Resolve<IOBCancellationPolicyRepository>();
        }

        /// <summary>
        /// Gets the OBDepositPolicyRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBDepositPolicyRepository GetOBDepositPolicyRepository()
        {
            return Container.Resolve<IOBDepositPolicyRepository>();
        }

        /// <summary>
        /// Gets the OBOtherPolicyRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBOtherPolicyRepository GetOBOtherPolicyRepository()
        {
            return Container.Resolve<IOBOtherPolicyRepository>();
        }

        /// <summary>
        /// Gets the OBExtrasRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBExtrasRepository GetOBExtrasRepository()
        {
            return Container.Resolve<IOBExtrasRepository>();
        }

        /// <summary>
        /// Gets the OBPaymentMethodTypeRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBPaymentMethodTypeRepository GetOBPaymentMethodTypeRepository()
        {
            return Container.Resolve<IOBPaymentMethodTypeRepository>();
        }

        /// <summary>
        /// Gets the OBAppSettingRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBAppSettingRepository GetOBAppSettingRepository()
        {
            return Container.Resolve<IOBAppSettingRepository>();
        }

        /// <summary>
        /// Gets the OBChannelRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBChannelRepository GetOBChannelRepository()
        {
            return Container.Resolve<IOBChannelRepository>();
        }

        /// <summary>
        /// Gets the OBPropertyRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBPropertyRepository GetOBPropertyRepository()
        {
            return Container.Resolve<IOBPropertyRepository>();
        }

        /// <summary>
        /// Gets the OBCRMRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBCRMRepository GetOBCRMRepository()
        {
            return Container.Resolve<IOBCRMRepository>();
        }

        /// <summary>
        /// Gets the IOBUserRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBUserRepository GetOBUserRepository()
        {
            return Container.Resolve<IOBUserRepository>();
        }

        /// <summary>
        /// Gets the IOBSecurityRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBSecurityRepository GetOBSecurityRepository()
        {
            return Container.Resolve<IOBSecurityRepository>();
        }

        /// <summary>
        /// Gets the IOBRateRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBRateRepository GetOBRateRepository()
        {
            return Container.Resolve<IOBRateRepository>();
        }

        /// <summary>
        /// Gets the IOBPropertyEventsRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBPropertyEventsRepository GetOBPropertyEventsRepository()
        {
            return Container.Resolve<IOBPropertyEventsRepository>();
        }

        /// <summary>
        /// Gets the IOBPMSRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBPMSRepository GetOBPMSRepository()
        {
            return Container.Resolve<IOBPMSRepository>();
        }

        /// <summary>
        /// Gets the IOBReservationLookupsRepository specialized Repository.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBReservationLookupsRepository GetOBReservationLookupsRepository()
        {
            return Container.Resolve<IOBReservationLookupsRepository>();
        }

        /// <summary>
        /// Gets the IOBBePartialPaymentCcMethodRepository specialized Repository.
        /// </summary>
        /// <returns>The repository</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBBePartialPaymentCcMethodRepository GetOBBePartialPaymentCcMethodRepository()
        {
            return Container.Resolve<IOBBePartialPaymentCcMethodRepository>();
        }

        /// <summary>
        /// Gets the IOBBePartialPaymentCcMethodRepository specialized Repository.
        /// </summary>
        /// <returns>The repository</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOBBeSettingsRepository GetOBBeSettingsRepository()
        {
            return Container.Resolve<IOBBeSettingsRepository>();
        }

        /// <summary>
        /// Gets the PaymentGatewayTransactionRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IPaymentGatewayTransactionRepository GetPaymentGatewayTransactionRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IPaymentGatewayTransactionRepository>();
        }

        /// <summary>
        /// Gets the ThirdPartyIntermediary Sql Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IThirdPartyIntermediarySqlRepository GetThirdPartyIntermediarySqlRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IThirdPartyIntermediarySqlRepository>();
        }

        /// <summary>
        /// Gets the Operator Sql Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IOperatorSqlRepository GetOperatorSqlRepository(IUnitOfWork unitOfWork)
        {
            Contract.Requires(unitOfWork != null);
            return Container.Resolve<IOperatorSqlRepository>();
        }

        /// <summary>
        /// Gets the LostReservations Sql Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ILostReservationsRepository GetLostReservationsRepository(IUnitOfWork unitOfWork)
        {
            return Container.Resolve<ILostReservationsRepository>();
        }


        /// <summary>
        /// Gets the IExternalSystems Repository.
        /// </summary>
        /// <returns>The repository</returns>
        public virtual IExternalSystemsRepository GetExternalSystemsRepository()
        {
            return Container.Resolve<IExternalSystemsRepository>();
        }
    }
}
