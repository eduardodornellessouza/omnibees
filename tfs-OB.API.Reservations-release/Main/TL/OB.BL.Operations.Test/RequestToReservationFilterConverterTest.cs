using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Internal.TypeConverters;
using domains = OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using static OB.Reservation.BL.Constants;
using OB.DL.Common;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class RequestToReservationFilterConverterTest
    {
        [TestInitialize]
        public void Initialize()
        {

        }

        #region GetGroupRuleCriteria Tests

        [TestMethod]
        [TestCategory("ReservationFilter.Map")]
        public void Test_ConvertToReservationFilter_Map_BigPullAuth_NotNull()
        {
            // Assemble
            var request = new ListReservationRequest
            {
                BigPullAuthRequestorUIDs = new System.Collections.Generic.List<long> { 5, 6 }
            };
            var convertedFilterRequest = new ListReservationFilterCriteria();

            // Act
            OtherConverter.Map(request, convertedFilterRequest);

            // Assert
            Assert.IsNotNull(convertedFilterRequest);
            Assert.IsNotNull(convertedFilterRequest.BigPullAuthRequestorUIDs);
            Assert.AreEqual(2, convertedFilterRequest.BigPullAuthRequestorUIDs.Count);
            Assert.IsTrue(convertedFilterRequest.BigPullAuthRequestorUIDs.Contains(5));
            Assert.IsTrue(convertedFilterRequest.BigPullAuthRequestorUIDs.Contains(6));
        }

        [TestMethod]
        [TestCategory("ReservationFilter.Map")]
        public void Test_ConvertToReservationFilter_Map_BigPullAuth_Null()
        {
            // Assemble
            var request = new ListReservationRequest
            {
                
            };
            var convertedFilterRequest = new ListReservationFilterCriteria();

            // Act
            OtherConverter.Map(request, convertedFilterRequest);

            // Assert
            Assert.IsNotNull(convertedFilterRequest);
            Assert.IsNull(convertedFilterRequest.BigPullAuthRequestorUIDs);
        }
        
        [TestMethod]
        [TestCategory("ReservationFilter.Map")]
        public void Test_ConvertToReservationFilter_Map_BigPullAuth_Empty()
        {
            // Assemble
            var request = new ListReservationRequest
            {
                BigPullAuthRequestorUIDs = new System.Collections.Generic.List<long> {  }
            };
            var convertedFilterRequest = new ListReservationFilterCriteria();

            // Act
            OtherConverter.Map(request, convertedFilterRequest);

            // Assert
            Assert.IsNotNull(convertedFilterRequest);
            Assert.IsNotNull(convertedFilterRequest.BigPullAuthRequestorUIDs);
            Assert.AreEqual(0, convertedFilterRequest.BigPullAuthRequestorUIDs.Count);
        }
        #endregion
    }
}
