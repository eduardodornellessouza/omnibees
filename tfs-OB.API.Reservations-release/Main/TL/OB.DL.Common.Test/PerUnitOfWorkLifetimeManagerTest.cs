using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Test.Mock;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.DL.Common.Test
{ 
    //TODO: Move these tests to new API.Core
    [TestClass]
    public class PerUnitOfWorkLifetimeManagerTest : BaseTest
    {
        //protected IUnityContainer Container { get; set; }

        //[TestInitialize]
        //public override void Initialize()
        //{
        //    base.Initialize();

        //    IUnityContainer containerScope = new UnityContainer();

        //    //containerScope = containerScope.AddExtension(new DataAccessLayerModule());

      
        //    Container = containerScope;
        //}

        //[TestMethod]
        //[TestCategory("PerUnitOfWorkLifetimeManager")]
        //public void TestPerUnitOfWorkLifetimeManagerSingleThreadMultipleAccesses()
        //{
        //   Container.RegisterType<ISessionFactory, SessionFactory>(new ContainerControlledLifetimeManager());

        //    var sessionFactory = Container.Resolve<ISessionFactory>() as ISessionFactory;
        //    Container.RegisterType<IRepository<OB.Domain.Reservations.Reservation>, RepositoryMock<OB.Domain.Reservations.Reservation>>(new PerUnitOfWorkLifetimeManager(sessionFactory));


        //    IRepository<OB.Domain.Reservations.Reservation> repo, repo2, repo3;


        //    using (var unitOfWork = sessionFactory.GetUnitOfWork())
        //    {
        //        repo = Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>();
        //        repo2 = Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>();
        //        repo3 = Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>();

        //    }

        //    Assert.IsNotNull(repo);
        //    Assert.IsNotNull(repo2);
        //    Assert.IsNotNull(repo3);
        //    Assert.IsTrue(repo == repo2);
        //    Assert.IsTrue(repo2 == repo3);
        //}

        //[TestMethod]
        //[TestCategory("PerUnitOfWorkLifetimeManager")]
        //public void TestPerUnitOfWorkLifetimeManagerMultipleThreadsMultipleAccesses()
        //{
        //    // NOTE: This test should be reviewed
        //    //       If it should test threads, it should use threads directly, not tasks

        //    int minWorkerThreads, minCompletionPortThreads;
        //    ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
        //    int maxWorkerThreads, maxCompletionPortThreads;
        //    ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
        //    try
        //    {
        //        ThreadPool.SetMinThreads(1, 1);
        //        ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
        //        ThreadPool.SetMinThreads(50, 50);
        //        ThreadPool.SetMaxThreads(50, 50);

        //        Container.RegisterType<ISessionFactory, SessionFactory>(new ContainerControlledLifetimeManager());
        //        var sessionFactory = Container.Resolve<ISessionFactory>() as ISessionFactory;
        //        Container.RegisterType<IRepository<OB.Domain.Reservations.Reservation>, RepositoryMock<OB.Domain.Reservations.Reservation>>(new PerUnitOfWorkLifetimeManager(sessionFactory));


        //        ConcurrentDictionary<Guid, List<IRepository<OB.Domain.Reservations.Reservation>>> reposPerThread = new ConcurrentDictionary<Guid, List<IRepository<OB.Domain.Reservations.Reservation>>>();
        //        ConcurrentBag<Guid> unitOfWorkGuids = new ConcurrentBag<Guid>();
        //        List<IRepository<OB.Domain.Reservations.Reservation>> currentThreadRepos = new List<IRepository<OB.Domain.Reservations.Reservation>>();
        //        List<IRepository<OB.Domain.Reservations.Reservation>> secondThreadRepos = new List<IRepository<OB.Domain.Reservations.Reservation>>();

        //        System.Collections.Concurrent.ConcurrentBag<Task> tasks = new System.Collections.Concurrent.ConcurrentBag<Task>();

        //        var unitOfWork = sessionFactory.GetUnitOfWork();
        //        unitOfWorkGuids.Add(unitOfWork.Guid);
        //        reposPerThread.TryAdd(unitOfWork.Guid, new List<IRepository<OB.Domain.Reservations.Reservation>>());


        //        reposPerThread[unitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //        Thread.SpinWait(1000);
        //        reposPerThread[unitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //        Thread.SpinWait(1000);

        //        Parallel.ForEach(Enumerable.Range(0, 100), (o) =>
        //        {
        //            tasks.Add(Task.Run(() =>
        //            {
        //                var localUnitOfWork = sessionFactory.GetUnitOfWork();
        //                Thread.SpinWait(1000);
        //                var guid = localUnitOfWork.Guid;
        //                unitOfWorkGuids.Add(guid);
        //                reposPerThread.TryAdd(guid, new List<IRepository<OB.Domain.Reservations.Reservation>>());


        //                reposPerThread[localUnitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //                Thread.SpinWait(1000);
        //                reposPerThread[localUnitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //                Thread.SpinWait(1000);
        //                reposPerThread[localUnitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //                Thread.SpinWait(1000);
        //                Thread.SpinWait(1000);
        //                Thread.SpinWait(1000);
        //                reposPerThread[localUnitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());

        //                if(o % 2 == 0)
        //                    localUnitOfWork.Dispose();
        //            }));
        //        });



        //        reposPerThread[unitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());
        //        Thread.SpinWait(1000);
        //        Thread.SpinWait(1000);
        //        Thread.SpinWait(1000);
        //        reposPerThread[unitOfWork.Guid].Add(Container.Resolve<IRepository<OB.Domain.Reservations.Reservation>>());


        //        Task.WaitAll(tasks.ToArray());

        //        Assert.IsTrue(unitOfWorkGuids.Distinct().Count() == 101);



        //        foreach(var reposPair in reposPerThread)
        //        {
        //            Assert.IsTrue(reposPair.Value.Count == 4);
        //            var firstValue = reposPair.Value.First();

        //            Assert.IsTrue(reposPair.Value.TrueForAll(x => x == firstValue));
        //            Assert.IsTrue(reposPair.Value.TrueForAll(x => ((RepositoryMock<OB.Domain.Reservations.Reservation>)x).UnitOfWorkGuid == reposPair.Key));
        //        }

        //        Assert.IsFalse(unitOfWork.IsDisposed);

        //        unitOfWork.Dispose();
        //    }
        //    finally
        //    {
        //        ThreadPool.SetMinThreads(1, 1);
        //        ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
        //        ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
        //        ThreadPool.SetMaxThreads(maxWorkerThreads, maxCompletionPortThreads);
        //    }
        //}
    }
}
