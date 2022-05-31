using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using OB.Api.Core;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Impl;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.Behaviors;
using OB.DL.Common.Interfaces;
using OB.Domain;
using OB.Log;
using PaymentGatewaysLibrary;
using System;
using System.Linq;

namespace OB.BL.Operations
{
    public class BusinessLayerModule : UnityContainerExtension
    {
        private ILogger logger;

        public BusinessLayerModule()
        {
            logger = LogsManager.CreateLogger();
        }

        protected override void Initialize()
        {
            long before = DateTime.Now.Ticks;
            logger.Info("Starting \"BusinessLayerModule.Initialize\"...");
            try
            {
                var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();
                var sessionFactory = this.Container.Resolve<ISessionFactory>();
                             
                //Load DataAccessLayerModule as well.
                Container.RegisterInstance<IThroughputLimiterManagerPOCO>(new ThroughputLimiterManagerPOCO(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IRegisteredTasksManager>(new NullRegisteredTasksManager(), new ContainerControlledLifetimeManager());
                //Este Register foi necessário para fazer os mocks para testes unitários
                Container.RegisterInstance<IPaymentGatewayFactory>(new PaymentGatewayFactory(), new ContainerControlledLifetimeManager());

                //HELPERS

                Container.RegisterType<ILogEmail, LogEmail>(new ContainerControlledLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());

                Container.RegisterType<IProjectGeneral, ProjectGeneral>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IReservationHelperPOCO, ReservationHelperPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IReservationManagerPOCO, ReservationManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IReservationValidatorPOCO, ReservationValidatorPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<ILogsManagerPOCO, LogsManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IReservationFilterManagerPOCO, ReservationFilterManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IPaypalGatewayManagerPOCO, PaypalGatewayManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IPaymentGatewayManagerPOCO, PaymentGatewayManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IEventSystemManagerPOCO, EventSystemManagerPoco>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IReservationPricesCalculationPOCO, ReservationPricesCalculationPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<ITokenizedCreditCardsReadsPerMonthManagerPOCO, TokenizedCreditCardsReadsPerMonthManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
                Container.RegisterType<IAdminManagerPOCO, AdminManagerPOCO>(new PerThreadLifetimeManager(), new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());

                logger.Info("Finished \"BusinessLayerModule.Initialize\". TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed \"BusinessLayerModule.Initialize\"");
                throw;
            }
        }

        public void WarmUp()
        {
            long before = DateTime.Now.Ticks;
            logger.Info("Starting \"BusinessLayerModule.WarmUp\"...");

            try
            {
                var sessionFactory = this.Container.Resolve<ISessionFactory>();
                var unitOfWork = sessionFactory.GetUnitOfWork(DomainScopes.GetAll().ToArray());
                unitOfWork.Dispose();

                this.Container.Resolve<IReservationHelperPOCO>();
                this.Container.Resolve<IReservationManagerPOCO>();
                this.Container.Resolve<IReservationValidatorPOCO>();
                this.Container.Resolve<IThroughputLimiterManagerPOCO>();
                this.Container.Resolve<IReservationFilterManagerPOCO>();

                logger.Info("Finished \"BusinessLayerModule.WarmUp\". TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed \"BusinessLayerModule.WarmUp\"");
                throw;
            }

        }
    }
}
