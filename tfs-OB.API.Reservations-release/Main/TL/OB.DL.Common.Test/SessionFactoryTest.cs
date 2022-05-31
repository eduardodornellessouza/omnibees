using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.DL.Common.Test
{

    [TestClass]
    public class SessionFactoryTest : BaseTest
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

        [TestCleanup]
        public override void Cleanup()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>();
            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            if (sessionFactory.CurrentUnitOfWork != null)
                sessionFactory.CurrentUnitOfWork.Dispose();

            (sessionFactory as SessionFactory).GetUnitOfWorkStorage().Clear();
            (sessionFactory as SessionFactory).GetThreadDataStorage().Clear();

            Container.Teardown(sessionFactory);
            Container.Teardown(repositoryFactory);

            Container = Container.RemoveAllExtensions();

            Container.Dispose();

            Container = null;
            
            base.Cleanup();

            GC.Collect();
            
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkDifferentThreads()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork = null;
            IUnitOfWork unitOfWork1 = null;
            IUnitOfWork unitOfWork2 = null;

            unitOfWork = sessionFactory.GetUnitOfWork();

            var thread1 = new Thread(new ThreadStart(() =>
            {
                unitOfWork1 = sessionFactory.GetUnitOfWork();
            }));
            thread1.Start();

            var thread2 = new Thread(new ThreadStart(() =>
            {
                unitOfWork2 = sessionFactory.GetUnitOfWork();
            }));
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Assert.IsNotNull(unitOfWork);
            Assert.IsNotNull(unitOfWork1);
            Assert.IsNotNull(unitOfWork2);
            Assert.AreNotEqual(unitOfWork, unitOfWork1, "UnitOfWorks instances should be different across different threads");
            Assert.AreNotEqual(unitOfWork1, unitOfWork2, "UnitOfWorks instances should be different across different threads");
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksMultipleThreadUnitOfWorkStorageCreation()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork = null;
            IUnitOfWork unitOfWork1 = null;
            IUnitOfWork unitOfWork2 = null;

            unitOfWork = sessionFactory.GetUnitOfWork();

            var currentUnitOfWorkStorage = sessionFactory.GetUnitOfWorkStorage();
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage1 = null;
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage2 = null;

            var manualResetEvents = new List<ManualResetEventSlim>
            {
                new ManualResetEventSlim(),
                new ManualResetEventSlim(),
            };

            var thread1 = new Thread(new ThreadStart(() =>
            {
                try
                {
                    unitOfWork1 = sessionFactory.GetUnitOfWork();
                    currentUnitOfWorkStorage1 = sessionFactory.GetUnitOfWorkStorage();
                }
                finally
                {
                    manualResetEvents[0].Set();
                }

            }));
            thread1.Start();

            var thread2 = new Thread(new ThreadStart(() =>
            {
                try
                {
                    unitOfWork2 = sessionFactory.GetUnitOfWork();
                    currentUnitOfWorkStorage2 = sessionFactory.GetUnitOfWorkStorage();
                }
                finally
                {
                    manualResetEvents[1].Set();
                }
            }));
            thread2.Start();

            manualResetEvents.ForEach((resetEvent) => resetEvent.Wait());

            //Change from ThreadStatic to ConcurrentDictionary
            //Assert.AreNotEqual(currentUnitOfWorkStorage, currentUnitOfWorkStorage1);
            //Assert.AreNotEqual(currentUnitOfWorkStorage, currentUnitOfWorkStorage2);

            var numberOfContextForCurrentUnitOfWork = currentUnitOfWorkStorage.Count(x => x.Key == sessionFactory.GetUnitOfWorkStorageKey(unitOfWork));
            var numberOfContextForCurrentUnitOfWork1 = currentUnitOfWorkStorage1.Count(x => x.Key == sessionFactory.GetUnitOfWorkStorageKey(unitOfWork1));
            var numberOfContextForCurrentUnitOfWork2 = currentUnitOfWorkStorage2.Count(x => x.Key == sessionFactory.GetUnitOfWorkStorageKey(unitOfWork2));


            Assert.IsTrue(numberOfContextForCurrentUnitOfWork == 1, "Only one UnitOfWork should exist for the current thread");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork1 == 1, "Only one UnitOfWork should exist for the Thread 1");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork2 == 1, "Only one UnitOfWork should exist for the Thread 2");

        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksMultipleThreadsThreadDataStorageCreation()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            IUnitOfWork unitOfWork = null;
            IUnitOfWork unitOfWork1 = null;
            IUnitOfWork unitOfWork2 = null;

            unitOfWork = sessionFactory.GetUnitOfWork();

            var currentThreadDataStorage = sessionFactory.GetThreadDataStorage();
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage1 = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage2 = null;

            var manualResetEvents = new List<ManualResetEventSlim>
            {
                new ManualResetEventSlim(),
                new ManualResetEventSlim(),
            };

            var thread1 = new Thread(new ThreadStart(() =>
            {
                try
                {
                    unitOfWork1 = sessionFactory.GetUnitOfWork();
                    var reservationRepository1 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
                    currentThreadDataStorage1 = sessionFactory.GetThreadDataStorage();
                }
                finally
                {
                    manualResetEvents[0].Set();
                }
            }));
            thread1.Start();

            var thread2 = new Thread(new ThreadStart(() =>
            {
                try
                {
                    unitOfWork2 = sessionFactory.GetUnitOfWork();
                    var reservationRepository2 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);
                    var propertyQuequeRepository2 = repositoryFactory.GetPaymentGatewayTransactionRepository(unitOfWork2);
                    var reservationFilterRepository2 = repositoryFactory.GetReservationsFilterRepository(unitOfWork2);
                    currentThreadDataStorage2 = sessionFactory.GetThreadDataStorage();
                }
                finally
                {
                    manualResetEvents[1].Set();
                }
            }));
            thread2.Start();

            manualResetEvents.ForEach((resetEvent) => resetEvent.Wait());


            var numberOfContextForCurrentUnitOfWork = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork.Guid.ToString()));
            var numberOfContextForCurrentUnitOfWork1 = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork1.Guid.ToString()));
            var numberOfContextForCurrentUnitOfWork2 = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork2.Guid.ToString()));


            Assert.IsTrue(numberOfContextForCurrentUnitOfWork == 0, "There shouldn't be any contexts for the UnitOfWork in the current thread");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork1 == 1, "There should be 1 context for the UnitOfWork in thread 1");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork2 == 2, "There should be 2 contexts for the UnitOfWork in thread 2");
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksMultipleThreadsThreadDataStorageDispose()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            IUnitOfWork unitOfWork = null;
            IUnitOfWork unitOfWork1 = null;
            IUnitOfWork unitOfWork2 = null;

            unitOfWork = sessionFactory.GetUnitOfWork();

            var currentThreadDataStorage = sessionFactory.GetThreadDataStorage();
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage1 = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage2 = null;

            var manualResetEvents = new List<ManualResetEventSlim>
            {
                new ManualResetEventSlim(),
                new ManualResetEventSlim(),
            };

            var thread1 = new Thread(new ThreadStart(() =>
            {
                try
                {


                    unitOfWork1 = sessionFactory.GetUnitOfWork();
                    var reservationRepository1 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
                    currentThreadDataStorage1 = sessionFactory.GetThreadDataStorage();
                    unitOfWork1.Dispose();
                }
                finally
                {
                    manualResetEvents[0].Set();
                }

            }));
            thread1.Start();

            var thread2 = new Thread(new ThreadStart(() =>
            {
                try
                {
                    unitOfWork2 = sessionFactory.GetUnitOfWork();
                    var reservationRepository2 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);
                    var propertyQuequeRepository2 = repositoryFactory.GetPaymentGatewayTransactionRepository(unitOfWork2);
                    var reservationFilterRepository2 = repositoryFactory.GetReservationsFilterRepository(unitOfWork2);
                    currentThreadDataStorage2 = sessionFactory.GetThreadDataStorage();
                }
                finally
                {
                    manualResetEvents[1].Set();
                }
            }));
            thread2.Start();

            manualResetEvents.ForEach((resetEvent) => resetEvent.Wait());

            var numberOfContextForCurrentUnitOfWork = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork.Guid.ToString()));
            var numberOfContextForCurrentUnitOfWork1 = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork1.Guid.ToString()));
            var numberOfContextForCurrentUnitOfWork2 = currentThreadDataStorage.Count(x => x.Key.EndsWith(unitOfWork2.Guid.ToString()));

            Assert.IsTrue(numberOfContextForCurrentUnitOfWork == 0, "There shouldn't be any contexts for the UnitOfWork in the current thread");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork1 == 0, "There shouldn't be any contexts for the UnitOfWork in thread 1 since it was disposed");
            Assert.IsTrue(numberOfContextForCurrentUnitOfWork2 == 2, "There should be 2 contexts for the UnitOfWork in thread 2");
            foreach (var context in currentThreadDataStorage2.Values)
            {
                var disposable = context.Item1 as IObjectContext;
                Assert.IsFalse(disposable.IsDisposed);
            }

        }


        [TestMethod]
        [TestCategory("SessionFactory")]
        //[DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksSingleThread()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            IUnitOfWork unitOfWork2 = sessionFactory.GetUnitOfWork();

            Assert.IsNotNull(unitOfWork1);
            Assert.IsNotNull(unitOfWork2);
            Assert.AreEqual(unitOfWork1, unitOfWork2, "UnitOfWork instances should be the same");
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksSingleThreadUnitOfWorkStorageCreation()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            IUnitOfWork unitOfWork2 = sessionFactory.GetUnitOfWork();

            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            var uow1Guid = unitOfWork1.Guid;
            var uow2Guid = unitOfWork2.Guid;


            Thread.Sleep(50);

            var unitOfWorkStorage = sessionFactory.GetUnitOfWorkStorage();

            Assert.IsTrue(unitOfWorkStorage.Count == 1, "UnitOfWorkStorage should only have 1 UnitOfWork instance");
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksSingleThreadUnitOfWorkStorageDispose()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            IUnitOfWork unitOfWork2 = sessionFactory.GetUnitOfWork();

            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            var uow1Guid = unitOfWork1.Guid;
            var uow2Guid = unitOfWork2.Guid;

            var rateRepository = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);


            unitOfWork1.Dispose();

            Thread.Sleep(300);

            var unitOfWorkStorage = sessionFactory.GetUnitOfWorkStorage();
            var contextStorage = sessionFactory.GetThreadDataStorage();

            Assert.IsTrue(unitOfWorkStorage.Count == 0, "UnitOfWork was not disposed correctly. Still exists in SessionFactory.UnitOfWorkStorage");
        }


        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksSingleThreadThreadDataStorageCreation()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            IUnitOfWork unitOfWork2 = sessionFactory.GetUnitOfWork();

            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            var uow1Guid = unitOfWork1.Guid;
            var uow2Guid = unitOfWork2.Guid;

            var rateRepository = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
            var rateRepository2 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
            var rateRepository3 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
            var rateRepository4 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
            var rateRepository5 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);
            var rateRepository6 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);
            var rateRepository7 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);

            var contextStorage = sessionFactory.GetThreadDataStorage();

            Assert.IsTrue(contextStorage.Count == 1, "1 RateContext should exists because Uows are the same");
        }

        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestMultipleUnitOfWorksSingleThreadThreadDataStorageDispose()
        {
            var sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;

            IUnitOfWork unitOfWork1 = sessionFactory.GetUnitOfWork();
            IUnitOfWork unitOfWork2 = sessionFactory.GetUnitOfWork();

            var repositoryFactory = Container.Resolve<IRepositoryFactory>();

            var uow1Guid = unitOfWork1.Guid;
            var uow2Guid = unitOfWork2.Guid;

            var rateRepository = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork1);
            var rateRepository2 = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork2);


            var threadDataStorage = sessionFactory.GetThreadDataStorage();

            Assert.IsTrue(threadDataStorage.Count == 1, "1 RateContext should exists because Uows are the same");


            unitOfWork1.Dispose();

            Assert.IsTrue(threadDataStorage.Count == 0, "SessionFactory.ThreadDataStorage should be empty");



        }

        [Ignore]
        [TestMethod]
        [TestCategory("SessionFactory")]
        [DeploymentItem("./DL")]
        public void TestUnitOfWorkWithTasksMultipleBackgroundThreadsAvailable()
        {
            var random = new Random();
            for (int i = 0; i < 1; i++)
            {
                // NOTE: Tests should not be random, unless they're fuzzy by nature
                ExecuteTasksTests(random.Next(2,16));
            }
        }



        
        //[TestMethod]
        //[TestCategory("SessionFactory")]
        //[DeploymentItem("./DL")]
        //public void TestUnitOfWorkWithTasksSingleBackgroundThreadAvailable()
        //{
        //    ExecuteTasksTests(1);
        //}

        private void ExecuteTasksTests(int numberOfBackgroundThreads)
        {
            // NOTE: This test should be reviewed
            //       If it should test threads, it should use threads directly, not tasks

            int minWorkerThreads, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            int maxWorkerThreads, maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);

            SessionFactory sessionFactory = null;
            IRepositoryFactory repositoryFactory = null;
            IUnitOfWork unitOfWork = null;
            IUnitOfWork unitOfWork1 = null;
            IUnitOfWork unitOfWork2 = null;
            IUnitOfWork unitOfWork3 = null;
            IUnitOfWork unitOfWork4 = null;

            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage = null;
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage1 = null;
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage2 = null;
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage3 = null;
            ConcurrentDictionary<string, WeakReference<IUnitOfWork>> currentUnitOfWorkStorage4 = null;

            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage1 = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage2 = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage3 = null;
            ConcurrentDictionary<string, Tuple<object, int>> currentThreadDataStorage4 = null;

            try
            {
                //Guarantee the number of Available Background threads to process Tasks
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
                ThreadPool.SetMinThreads(numberOfBackgroundThreads, numberOfBackgroundThreads);
                ThreadPool.SetMaxThreads(numberOfBackgroundThreads*2, numberOfBackgroundThreads*2);

                sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
                repositoryFactory = Container.Resolve<IRepositoryFactory>();



                unitOfWork = sessionFactory.GetUnitOfWork();

                currentUnitOfWorkStorage = sessionFactory.GetUnitOfWorkStorage();


                currentThreadDataStorage = sessionFactory.GetThreadDataStorage();

                Exception taskEx = null;

                Task task3 = null;
                Task task4 = null;

                var task2 = Task.Factory.StartNew(() =>
                {
                    unitOfWork2 = sessionFactory.GetUnitOfWork();

                    Volatile.Write(ref task3,
                        Task.Factory.StartNew(() =>
                        {
                            using (unitOfWork3 = sessionFactory.GetUnitOfWork())
                            {
                                currentUnitOfWorkStorage3 = sessionFactory.GetUnitOfWorkStorage();
                                currentThreadDataStorage3 = sessionFactory.GetThreadDataStorage();
                                Thread.Sleep(500);                         
                            }

                        }, TaskCreationOptions.AttachedToParent)); // WHY?

                    Volatile.Write(ref task4,
                        Task.Factory.StartNew(() =>
                        {
                            unitOfWork4 = sessionFactory.GetUnitOfWork();
                            currentUnitOfWorkStorage4 = sessionFactory.GetUnitOfWorkStorage();
                            currentThreadDataStorage4 = sessionFactory.GetThreadDataStorage();
                            Thread.Sleep(500);
                            unitOfWork4.Dispose();
                        }, TaskCreationOptions.LongRunning));

                    Thread.Sleep(1250);

                    currentUnitOfWorkStorage2 = sessionFactory.GetUnitOfWorkStorage();
                    currentThreadDataStorage2 = sessionFactory.GetThreadDataStorage();
                    unitOfWork2.Dispose();
                }, TaskCreationOptions.LongRunning);

                var task1 = Task.Factory.StartNew(() =>
                {
                    unitOfWork1 = sessionFactory.GetUnitOfWork();

                    Thread.Sleep(2500);

                    currentUnitOfWorkStorage1 = sessionFactory.GetUnitOfWorkStorage();
                    currentThreadDataStorage1 = sessionFactory.GetThreadDataStorage();
                    try
                    {
                        var reservationRepo = repositoryFactory.GetReservationsRepository(unitOfWork1);

                        //This fails if the UnitOfWork was disposed by other Tasks/Threads.
                        var res = reservationRepo.GetQuery().Take(1);
                    }
                    catch (Exception ex)
                    {
                        taskEx = ex;
                    }
                }, TaskCreationOptions.LongRunning);

                long before = DateTime.Now.Ticks;

                //task3 and task4 might be null because were not sure if Task2 has been executed, so we need to be sure before invoking Task.WaitAll on the task instance references.
                while (Volatile.Read(ref task3) == null || Volatile.Read(ref task4) == null)
                {
                    Thread.SpinWait(1000);
                    if (TimeSpan.FromTicks(DateTime.Now.Ticks - before).TotalSeconds > 30)
                        break;
                }
                Task.WaitAll(new Task[] { task1, task2, task3, task4 });

                Assert.IsNull(taskEx, "There are concurrency problems with UnitOfWork instances and different Tasks");

                Assert.AreNotEqual(unitOfWork1, unitOfWork2);
                Assert.AreNotEqual(unitOfWork2, unitOfWork3);
                Assert.AreNotEqual(unitOfWork3, unitOfWork4);

                Assert.IsTrue(currentUnitOfWorkStorage != currentUnitOfWorkStorage1);
                Assert.IsTrue(currentUnitOfWorkStorage1 != currentUnitOfWorkStorage2);
                Assert.IsTrue(currentUnitOfWorkStorage2 != currentUnitOfWorkStorage3);
                Assert.IsTrue(currentUnitOfWorkStorage3 != currentUnitOfWorkStorage4);
                Assert.IsTrue(currentUnitOfWorkStorage4 != currentUnitOfWorkStorage1);

                Assert.IsTrue(currentUnitOfWorkStorage.Count == 1, "Only one UnitOfWork should exist for the current thread");
                Assert.IsTrue(currentUnitOfWorkStorage1.Count == 1, "Only one UnitOfWork should exist for Task 1");
                Assert.IsTrue(currentUnitOfWorkStorage2.Count == 0, "No UnitOfWork instances should exist for Task 2");
                Assert.IsTrue(currentUnitOfWorkStorage3.Count == 0, "No UnitOfWork instances should exist for Task 3");
                Assert.IsTrue(currentUnitOfWorkStorage4.Count == 0, "No UnitOfWork instances should exist for Task 4");

                Assert.IsTrue(currentThreadDataStorage.Count == 0, "No context instances should exist for the current thread");
                Assert.IsTrue(currentThreadDataStorage1.Count == 1, "Only one Context instance (PropertyRepo) should exist for Task 1");
                Assert.IsTrue(currentThreadDataStorage2.Count == 0, "No context instances should exist for Task 2");
                Assert.IsTrue(currentThreadDataStorage3.Count == 0, "No context instances should exist for Task 3");
                Assert.IsTrue(currentThreadDataStorage4.Count == 0, "No context instances should exist for Task 4");

            }
            finally
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
                ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
                ThreadPool.SetMaxThreads(maxWorkerThreads, maxCompletionPortThreads);
                
                if(unitOfWork != null && !unitOfWork.IsDisposed)
                    unitOfWork.Dispose();
                if(unitOfWork1 != null && !unitOfWork1.IsDisposed)
                    unitOfWork1.Dispose();
                if(unitOfWork2 != null && !unitOfWork2.IsDisposed)
                    unitOfWork2.Dispose();
                if(unitOfWork3 != null && !unitOfWork3.IsDisposed)
                    unitOfWork3.Dispose();
                if(unitOfWork4 != null && !unitOfWork4.IsDisposed)
                    unitOfWork4.Dispose();

                if(currentUnitOfWorkStorage != null)
                    currentUnitOfWorkStorage.Clear();
                if(currentUnitOfWorkStorage1 != null)
                    currentUnitOfWorkStorage1.Clear();
                if(currentUnitOfWorkStorage2 != null)
                    currentUnitOfWorkStorage2.Clear();
                if(currentUnitOfWorkStorage3 != null)
                    currentUnitOfWorkStorage3.Clear();
                if(currentUnitOfWorkStorage4 != null)
                    currentUnitOfWorkStorage4.Clear();

                if (currentThreadDataStorage != null)
                    currentThreadDataStorage .Clear();
                if (currentThreadDataStorage1 != null)
                currentThreadDataStorage1 .Clear();
                if (currentThreadDataStorage2 != null)
                currentThreadDataStorage2 .Clear();
                if (currentThreadDataStorage3 != null)
                currentThreadDataStorage3 .Clear();
                if (currentThreadDataStorage4 != null)
                currentThreadDataStorage4.Clear();
            }

        }

    */
    }
}
