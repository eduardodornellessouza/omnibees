using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using OB.Api.Core;

namespace OB.DL.Common.Test
{
    [TestClass]
    public class UnitOfWorkTest : BaseTest
    {
        //TODO: passar estes testes para a Api.Core
        /*
        protected IUnityContainer Container { get; set; }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            IUnityContainer containerScope = new UnityContainer();
            containerScope = containerScope.AddExtension(new DataAccessLayerModule());


            Container = containerScope;
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./Databases_WithData", "TestSaveAsync")]
        [DeploymentItem("./DL")]
        public void TestSaveAsync()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {

                var resRepository1 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork);
                var res = resRepository1.GetQuery().Take(1).First();
                res.Status = 4;


                var result = unitOfWork.SaveAsync();

                Assert.IsTrue(result.Count() == 1, "Expected exactly one context to have updated records");

                // NOTE: Blocking wait on I/O async operations may deadlock
                var modified = result.First().GetAwaiter().GetResult();

                Assert.IsTrue(modified == 1, "Expected one modified record asynchronously");
            }
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkThreadIdCurrentThread()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var currentUnitOfWorkThreadId = unitOfWork1.ThreadId;

            
            Assert.AreEqual(currentThreadId, currentUnitOfWorkThreadId);
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkThreadIdNewTask()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;


            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            var currentUnitOfWorkThreadId = unitOfWork1.ThreadId;

            int taskUnitOfWorkThreadId = -1;
            IUnitOfWork unitOfWork2 = null;


            var task1 = Task.Factory.StartNew(() =>
            {
                unitOfWork2 = sessionFactory.GetUnitOfWork();
                taskUnitOfWorkThreadId = unitOfWork2.ThreadId;
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);


            task1.GetAwaiter().GetResult();

            Assert.AreNotEqual(currentUnitOfWorkThreadId, taskUnitOfWorkThreadId);
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkThreadIdDifferentThread()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;


            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            var currentUnitOfWorkThreadId = unitOfWork1.ThreadId;

            int threadUnitOfWorkThreadId = -1;
            IUnitOfWork unitOfWork2 = null;

            var thread1 = new Thread(new ThreadStart(() =>
            {
                unitOfWork2 = sessionFactory.GetUnitOfWork();
                threadUnitOfWorkThreadId = unitOfWork2.ThreadId;
            }));
            thread1.Start();

            thread1.Join();
            

            Assert.AreNotEqual(currentUnitOfWorkThreadId, threadUnitOfWorkThreadId);
        }


        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkIsInTransactionNoTransactionScope()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            
            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();

            Assert.IsFalse(unitOfWork1.IsInAmbientTransaction,"Expected false because no TransactionScope was created");

        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkIsInTransactionWithTransactionScopeRequired()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var transactionScope = new TransactionScope(TransactionScopeOption.Required);
            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();

            Assert.IsTrue(unitOfWork1.IsInAmbientTransaction, "Expected true because a transactionScope was created");
        }


        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkIsInTransactionWithTransactionScopeRequiresNew()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew);

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();

            Assert.IsTrue(unitOfWork1.IsInAmbientTransaction, "Expected false because no TransactionScope was created");
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkIsInTransactionWithTransactionScopeSuppress()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            var transactionScope = new TransactionScope(TransactionScopeOption.Suppress);

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();

            Assert.IsFalse(unitOfWork1.IsInAmbientTransaction, "Expected false because the TransactionScope was Supress (no ambient transaction is created)");
        }

        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void TestUnitOfWorkIsInTransactionOutOfTransactionScope()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required))
            {
            }
            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();

            Assert.IsFalse(unitOfWork1.IsInAmbientTransaction, "Expected false because the TransactionScope was disposed");
        }


        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkDispose()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var repositoryFactory = Container.Resolve<IRepositoryFactory>() as RepositoryFactory;

            UnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork() as UnitOfWork;

            var reservationRepo = repositoryFactory.GetReservationsRepository(unitOfWork1);
            var reservationFilterRepo = repositoryFactory.GetReservationsFilterRepository(unitOfWork1);

            
            var contextsList = unitOfWork1.Contexts.ToArray();

            unitOfWork1.Dispose();

            foreach (var context in contextsList)
            {
                Assert.IsTrue(context.IsDisposed, "Context in unitofwork was not disposed correctly");
            }            

   
            Assert.IsTrue(unitOfWork1.IsDisposed, "UnitOfWork IsDisposed flag not set correctly after dispose");
        }


        [TestMethod]
        [TestCategory("UnitOfWork")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkGetContext()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var repositoryFactory = Container.Resolve<IRepositoryFactory>() as RepositoryFactory;

            UnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork() as UnitOfWork;

            var reservationRepo = repositoryFactory.GetReservationsRepository(unitOfWork1);
            var reservationFilterRepo = repositoryFactory.GetReservationsFilterRepository(unitOfWork1);

            var contextsList = unitOfWork1.Contexts.ToArray();

            Assert.IsTrue(contextsList.Count() >= 1);

            var reservationContext = unitOfWork1.GetContext(OB.Domain.Reservations.Reservation.DomainScope);
            Assert.IsTrue(contextsList.Contains(reservationContext));
            Assert.IsNotNull(reservationContext);
            
            var reservationFilterContext = unitOfWork1.GetContext(OB.Domain.Reservations.ReservationFilter.DomainScope);
            Assert.IsTrue(contextsList.Contains(reservationFilterContext));
            Assert.IsNotNull(reservationFilterContext);

            //var appSettingContext = unitOfWork1.GetContext(AppSetting.DomainScope);
            //Assert.IsFalse(contextsList.Contains(appSettingContext));
            //Assert.IsNotNull(appSettingContext);
            
        }
        */
    }


}
