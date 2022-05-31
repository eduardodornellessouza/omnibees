using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Impl.Entity;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Repositories
{
    internal class ReservationsFilterMockRepository : ReservationsFilterRepository
    {
        public List<ReservationFilter> reservationFilters = new List<ReservationFilter>();
        public ReservationsFilterMockRepository(IObjectContext context) : base(context)
        {
        }
        public override IQueryable<ReservationFilter> GetQuery()
        {
            return reservationFilters.AsQueryable();
        }
    }

    [TestClass]
    public class ReservationFilterRepositoryTest
    {
        private ReservationsFilterMockRepository _reservationFilterRepo;

        [TestInitialize]
        public void Initialize()
        {
            var dbContext = new Mock<DbContext>();
            var connection = new Mock<DbConnection>();
            var objectContextMock = new Mock<IObjectContext>();
            objectContextMock.Setup(x => x.Context).Returns(new DbContext(connection.Object, true));

            _reservationFilterRepo = new ReservationsFilterMockRepository(objectContextMock.Object);

            connection.Setup(x => x.Open());


        }

        [TestMethod]
        [TestCategory("ReservationFilterRepository.FindByCriteria")]
        public void Test_FindByCriteria_FilterBigPullUids_NoReservations()
        {
            _reservationFilterRepo.reservationFilters = new List<ReservationFilter>
            {
                new ReservationFilter
                {
                    UID = 3,
                    ChannelUid=10
                }
            };

            var request = new ListReservationFilterCriteria 
            { 
                 BigPullAuthRequestorUIDs = new List<long> { 1 }   
            };
            
            int total = -1;
            var response = _reservationFilterRepo.FindByCriteria(request, out total, true);

            Assert.AreEqual(0, total);
        }

        [TestMethod]
        [TestCategory("ReservationFilterRepository.FindByCriteria")]
        public void Test_FindByCriteria_FilterBigPullUids_OneReservations()
        {
            _reservationFilterRepo.reservationFilters = new List<ReservationFilter>
            {
                new ReservationFilter
                {
                    UID = 3,
                    ChannelUid = 10,
                },
                 new ReservationFilter
                {
                    UID = 4,
                    ChannelUid = 10,
                    BigPullAuthRequestor_UID = 5
                }
            };

            var request = new ListReservationFilterCriteria
            {
                BigPullAuthRequestorUIDs = new List<long> { 5 }
            };

            int total = -1;
            var response = _reservationFilterRepo.FindByCriteria(request, out total, true);

            Assert.AreEqual(1, total);
            Assert.AreEqual(4, response.First());
        }
    }
}
