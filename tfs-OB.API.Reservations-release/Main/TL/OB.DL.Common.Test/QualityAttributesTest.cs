using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Test.Helper;
using OB.Domain.Reservations;
using System;
using System.Linq;
using OB.Api.Core;

namespace OB.DL.Common.Test
{
    //TODO: Move these tests to new API.Core
    [TestClass]
    public class QualityAttributesTest : BaseTest
    {
        //protected IUnityContainer Container { get; set; }

        //[TestInitialize]
        //public override void Initialize()
        //{
        //    base.Initialize();

        //    IUnityContainer containerScope = new UnityContainer();

        //    containerScope = containerScope.AddExtension(new DataAccessLayerModule());

        //    Container = containerScope;
        //}

        //[TestMethod]
        //[TestCategory("Repository")]
        //[DeploymentItem("./DL")]
        //public void TestSessionFactoryTestabilityQA()
        //{
        //    //add data to the mocked database
        //    var rrdDbSet = UnitTestContext.GetStaticDbSet<VisualState>();
        //    for (int i = 0; i < 10000; i++)
        //    {
        //        rrdDbSet.Add(new VisualState
        //        {
        //            UID = i + 1,
        //            LookupKey_1 = "000000",
        //            JSONData = string.Empty
        //        });
        //    }

        //    this.Container = this.Container.RegisterType<ISessionFactory, UnitTestSessionFactory>(new ContainerControlledLifetimeManager());

        //    var sessionFactory = Container.Resolve<ISessionFactory>() as ISessionFactory;

        //    var repositoryFactory = Container.Resolve<IRepositoryFactory>();


        //    using (var unitOfWork = sessionFactory.GetUnitOfWork())
        //    {
        //        var rateRoomDetailRepo = repositoryFactory.GetRepository<VisualState>(unitOfWork);

        //        var uids = rateRoomDetailRepo.GetQuery().Take(5000).OrderBy(x => x.UID).Select(x => x.UID).ToList();
        //        Assert.IsTrue(uids.Count == 5000);
        //    }
        //}
    }
}
