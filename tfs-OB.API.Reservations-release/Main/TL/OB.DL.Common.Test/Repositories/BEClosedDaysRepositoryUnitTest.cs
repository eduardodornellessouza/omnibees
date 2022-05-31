using Couchbase.Core;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Test.Repositories
{
    [TestClass]
    public class BEClosedDaysRepositoryUnitTest : BaseTest
    {
        private BEClosedDaysRepositoryMock beClosedRepoMock;
        protected IUnityContainer Container { get; set; }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            var _sqlManager = new Mock<ISqlManager>(MockBehavior.Default);
            var _appSettingRepoMock = new Mock<IAppSettingRepository>(MockBehavior.Default);

            var _bucketMock = new Mock<IBucket>(MockBehavior.Default);

            //Mock Repository factory to return mocked Repository's
            var repoFactoryMock = new Mock<IRepositoryFactory>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetAppSettingRepository(It.IsAny<IUnitOfWork>())).Returns(_appSettingRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), It.IsAny<OB.Domain.DomainScope>())).Returns(_sqlManager.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Default);
            var sessionFactoryMock = new Mock<ISessionFactory>(MockBehavior.Default);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            Container = new UnityContainer();
            Container = Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            Container = Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);
            Container = Container.RegisterInstance<IBucket>(_bucketMock.Object);
            Container = Container.RegisterInstance<ISqlManager>(_sqlManager.Object);
            Container = Container.RegisterInstance<IAppSettingRepository>(_appSettingRepoMock.Object);
            Container = Container.RegisterType<IBEClosedDaysRepository, BEClosedDaysRepositoryMock>();

            beClosedRepoMock = (BEClosedDaysRepositoryMock)Container.Resolve<IBEClosedDaysRepository>();
        }

        #region Data For Tests

        private long _propertyUID = 1635;
        private long _channelUID = 1;

        private string CreateCouchBaseDocId(int year, int month)
        {
            return string.Format("ClosedDays::1::1635::{0}::{1}", year, month);
        }
        
        private void FillOutputCouchbase(params DateTime[] dates)
        {
            beClosedRepoMock.OutputCouchbase = new Dictionary<string, Domain.BE.BEClosedDays>();
            if (dates != null)
                foreach (var dateGroupedByMonth in dates.GroupBy(x => new { x.Year, x.Month }))
                {
                    beClosedRepoMock.OutputCouchbase.Add(CreateCouchBaseDocId(dateGroupedByMonth.Key.Year, dateGroupedByMonth.Key.Month),
                        new Domain.BE.BEClosedDays() { ClosedDays = dateGroupedByMonth.ToList() });
                }
        }

        private void FillOutputSql(params DateTime[] dates)
        {
            beClosedRepoMock.OutputSql = new List<DateTime>();
            if (dates != null)
                beClosedRepoMock.OutputSql = dates.ToList();
        }

        #endregion

        [TestMethod]
        [TestCategory("BEClosedDays")]
        public void Test_GetBEClosedDays_AllMonthsInCouchbase()
        {
            // Prepare data
            var inputStartDate = new DateTime(2050, 10, 1);
            var inputEndDate = new DateTime(2050, 12, 31);
            var couchbaseDates = new DateTime [] { new DateTime(2050, 10, 3) , new DateTime(2050, 11, 4), new DateTime(2050, 12, 5) };
            var sqlDates = new DateTime[] { new DateTime(2050, 10, 1), new DateTime(2050, 11, 7), new DateTime(2050, 12, 20) };
            FillOutputCouchbase(couchbaseDates);
            FillOutputSql(sqlDates);

            // Act
            var results = beClosedRepoMock.GetBEClosedDays(_propertyUID, _channelUID, inputStartDate, inputEndDate);

            // Asserts
            Assert.IsNotNull(results, "Results must have 3 dates.");
            Assert.AreEqual(3, results.Count, "Results must have exactly 3 dates.");
            Assert.IsFalse(results.Any(x => x.Date.CompareTo(sqlDates[0]) == 0), "Results shouldn't have dates from SQL. Date = '" + sqlDates[0] + "'");
            Assert.IsFalse(results.Any(x => x.Date.CompareTo(sqlDates[1]) == 0), "Results shouldn't have dates from SQL. Date = '" + sqlDates[1] + "'");
            Assert.IsFalse(results.Any(x => x.Date.CompareTo(sqlDates[2]) == 0), "Results shouldn't have dates from SQL. Date = '" + sqlDates[2] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[0]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[1]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[2]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[2] + "'");
        }

        [TestMethod]
        [TestCategory("BEClosedDays")]
        public void Test_GetBEClosedDays_AllMonthsInSQL()
        {
            // Prepare data
            var inputStartDate = new DateTime(2050, 10, 1);
            var inputEndDate = new DateTime(2050, 12, 31);
            var couchbaseDates = new DateTime[] { };
            var sqlDates = new DateTime[] { new DateTime(2050, 10, 1), new DateTime(2050, 11, 7), new DateTime(2050, 12, 20) };
            FillOutputCouchbase(couchbaseDates);
            FillOutputSql(sqlDates);

            // Act
            var results = beClosedRepoMock.GetBEClosedDays(_propertyUID, _channelUID, inputStartDate, inputEndDate);

            // Asserts
            Assert.IsNotNull(results, "Results must have 3 dates.");
            Assert.AreEqual(3, results.Count, "Results must have exactly 3 dates.");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[0]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[1]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[2]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[2] + "'");
        }

        [TestMethod]
        [TestCategory("BEClosedDays")]
        public void Test_GetBEClosedDays_MonthsInCouchbaseAndSQL()
        {
            // Prepare data
            var inputStartDate = new DateTime(2050, 9, 1);
            var inputEndDate = new DateTime(2051, 1, 31);
            var couchbaseDates = new DateTime[] { new DateTime(2050, 10, 3), new DateTime(2050, 10, 4), new DateTime(2050, 10, 5) };
            var sqlDates = new DateTime[] { new DateTime(2050, 11, 7), new DateTime(2051, 1, 20), new DateTime(2051, 1, 21) };
            FillOutputCouchbase(couchbaseDates);
            FillOutputSql(sqlDates);

            // Act
            var results = beClosedRepoMock.GetBEClosedDays(_propertyUID, _channelUID, inputStartDate, inputEndDate);

            // Asserts
            Assert.IsNotNull(results, "Results must have 6 dates.");
            Assert.AreEqual(6, results.Count, "Results must have exactly 6 dates.");
            Assert.IsFalse(results.Any(x => x.Date.Year == 2050 && x.Date.Month == 9), "Couchbase and SQL shouldn't have any dates for this month: '2050/09'");
            Assert.IsFalse(results.Any(x => x.Date.Year == 2050 && x.Date.Month == 12), "Couchbase and SQL shouldn't have any dates for this month: '2050/12'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[0]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[1]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[2]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[2] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[0]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[1]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[2]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[2] + "'");
        }

        [TestMethod]
        [TestCategory("BEClosedDays")]
        public void Test_GetBEClosedDays_MonthsInCouchbaseAndSQL_StartDateGreaterThanEndDate()
        {
            // Prepare data
            var inputStartDate = new DateTime(2051, 1, 31);
            var inputEndDate = new DateTime(2050, 9, 1);
            var couchbaseDates = new DateTime[] { new DateTime(2050, 10, 3), new DateTime(2050, 10, 4), new DateTime(2050, 10, 5) };
            var sqlDates = new DateTime[] { new DateTime(2050, 11, 7), new DateTime(2051, 1, 20), new DateTime(2051, 1, 21) };
            FillOutputCouchbase(couchbaseDates);
            FillOutputSql(sqlDates);

            // Act
            var results = beClosedRepoMock.GetBEClosedDays(_propertyUID, _channelUID, inputStartDate, inputEndDate);

            // Asserts
            Assert.IsNotNull(results, "Results must have 6 dates.");
            Assert.AreEqual(6, results.Count, "Results must have exactly 6 dates.");
            Assert.IsFalse(results.Any(x => x.Date.Year == 2050 && x.Date.Month == 9), "Couchbase and SQL shouldn't have any dates for this month: '2050/09'");
            Assert.IsFalse(results.Any(x => x.Date.Year == 2050 && x.Date.Month == 12), "Couchbase and SQL shouldn't have any dates for this month: '2050/12'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[0]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[1]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(couchbaseDates[2]) == 0), "Results must have exactly one date from Couchbase equals to '" + couchbaseDates[2] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[0]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[0] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[1]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[1] + "'");
            Assert.AreEqual(1, results.Count(x => x.Date.CompareTo(sqlDates[2]) == 0), "Results must have exactly one date from SQL equals to '" + sqlDates[2] + "'");
        }
    }
}
