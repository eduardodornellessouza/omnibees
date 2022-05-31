using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using OB.DL.Common.Cache;
using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using OB.Domain;
using OB.Domain.Reservations;
using OB.Log;
using System;
using System.Configuration;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces;

namespace OB.DL.Common
{
    /// <summary>
    /// Data Access Layer IoC extension configuration.
    /// </summary>
    public class DataAccessLayerModule : UnityContainerExtension
    {
        private ILogger logger;

        public DataAccessLayerModule()
        {
            logger = LogsManager.CreateLogger();
        }

        /// <summary>
        /// Initializes the DataAccessLayerModule IoC container registry for the classes:
        /// <para>CacheProvider</para>
        /// <para>SessionFactory</para>
        /// <para>RepositoryFactory</para>
        /// </summary>
        protected override void Initialize()
        {
            long before = DateTime.Now.Ticks;
            logger.Info("Starting \"DataAccessLayerModule.Initialize\"...");
            try
            {
                Type cacheProviderType = null;

                this.Container.AddNewExtension<Interception>();
           
                    

                var cacheProvider = ConfigurationManager.AppSettings["OB.DL.Common.CacheProvider"];
                if (!string.IsNullOrEmpty(cacheProvider))
                {
                    try
                    {
                        cacheProviderType = this.GetType().Assembly.GetType(cacheProvider);
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e, "The CacheProvider \"{0}\" wasn't found in assembly \"{1}\"", cacheProvider, this.GetType().Assembly.FullName);
                    }
                    if (cacheProviderType != null)
                        this.Container.RegisterType(typeof(ICacheProvider), cacheProviderType, new ContainerControlledLifetimeManager());
                }

                //Reverts to the default CacheProvider (using local AppDomain Process cache).
                if (cacheProviderType == null)
                    this.Container.RegisterType<ICacheProvider, DefaultCacheProvider>(new ContainerControlledLifetimeManager());

                var sessionFactory = new SessionFactory(DomainScopes.GetAll().ToList());
                Container.RegisterType<ISessionFactory, SessionFactory>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => sessionFactory));

#if DEBUG
                Container.RegisterType<ITransactionManager, TransactionManagerMock>();
#else
                Container.RegisterType<ITransactionManager, TransactionManager>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => new TransactionManager(sessionFactory)));
#endif

                // Uncomment this line if the MSDTC doesn't work in debug mode. Do not checkin this line uncommented!!!
                //Container.RegisterType<ITransactionManager, TransactionManagerMock>(new ContainerControlledLifetimeManager());

                this.Container.RegisterType<IRepositoryFactory, RepositoryFactory>(new ContainerControlledLifetimeManager());

                logger.Info("Finished \"DataAccessLayerModule.Initialize\". TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed \"DataAccessLayerModule.Initialize\"");
                throw;
            }
        }

        /// <summary>
        /// Optional function to warmup the Cache and respective Repositories.
        /// <para>Countries</para>
        /// <para>Languages</para>
        /// <para>SystemEvents</para>
        /// <para>SystemActions</para>
        /// <para>PaymentMethodTypes</para>
        /// <para>PaymentGatewayConfigurations</para>
        /// </summary>
        public void WarmUp()
        {
            long before = DateTime.Now.Ticks;
            logger.Info("Starting \"DataAccessLayerModule.WarmUp\"...");

            try
            {
                //do some warmup
                var sessionFactory = this.Container.Resolve<ISessionFactory>();
                var unitOfWork = sessionFactory.GetUnitOfWork(DomainScopes.GetAll().ToArray());

                var cacheProvider = this.Container.Resolve<ICacheProvider>();

                var repositoryFactory = this.Container.Resolve<IRepositoryFactory>();
                var reservationStatusRepo = this.Container.Resolve<IRepository<ReservationStatus>>();
                
                logger.Info("Finished \"DataAccessLayerModule.WarmUp\". TOOK:" + TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds + "s");
            }
            catch (Exception e)
            {
                logger.Error(e, "FAILED \"DataAccessLayerModule.WarmUp\", Unity Container failed to instantiate one or more Repositories, PLEASE CHECK THIS AS SOON AS POSSIBLE");
            }
        }
    }
}