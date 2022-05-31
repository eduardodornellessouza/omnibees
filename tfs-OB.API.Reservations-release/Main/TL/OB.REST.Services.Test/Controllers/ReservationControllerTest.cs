using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Test;
using OB.REST.Services.Controllers;
using OB.REST.Services.Test.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OB.REST.Services.Test.Controllers
{
    [TestClass]
    public class ReservationControllerTest : IntegrationBaseTest
    {
        public ReservationController ReservationController
        {
            get;
            set;
        }
        public IReservationManagerPOCO ReservationPOCO
        {
            get;
            set;
        }


        #region Initialize
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            // Mock Controller and POCO
            this.ReservationPOCO = this.Container.Resolve<IReservationManagerPOCO>();
            this.ReservationController = new ReservationController(ReservationPOCO);

        }
        #endregion

        #region Integration Tests
        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_ListSingleReservation_Get_NotAllowed")]
        public async Task Test_ReservationController_ListSingleReservation_Get_NotAllowed()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var response = await server.CreateRequest("/api/Reservation/ListReservations?id=1723").AddHeader("Accept", "application/json").GetAsync();
                var result = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Method Not Allowed", response.ReasonPhrase);

                //var reservationResponse = JsonConvert.DeserializeObject<ListReservationResponse>(result);
                //
                //Assert.IsTrue(reservationResponse.Result != null);
                //Assert.IsTrue(reservationResponse.Result.Count == 1);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_ListMultipleReservation_Post")]
        public async Task Test_ReservationController_ListMultipleReservation_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var listReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationUIDs = new List<long> { 1723, 2122 },
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(listReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 2);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_ListSingleReservation_Post")]
        public async Task Test_ReservationController_ListSingleReservation_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var listReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationUIDs = new List<long> { 1723 },
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(listReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationEntireGraph_Post")]
        public async Task Test_ReservationController_FindSingleReservationEntireGraph_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationNumbers = new List<string> { "RES000001-893" },
                    IncludeGuests = true,
                    IncludeReservationPartialPaymentDetails = true,
                    IncludeReservationRoomChilds = true,
                    IncludeReservationPaymentDetail = true,
                    IncludeReservationRoomDetails = true,
                    IncludeReservationRoomExtras = true,
                    IncludeReservationRooms = true,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationSimple_Post")]
        public async Task Test_ReservationController_FindSingleReservationSimple_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    PropertyUIDs = new List<long> { 1094 },
                    ChannelUIDs = new List<long> { 26 },
                    ReservationNumbers = new List<string> { "RES000001-893" },
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = false,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = false,
                    IncludeReservationRoomExtras = false,
                    IncludeReservationRooms = false,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);

                Assert.IsNull(reservationResponse.Result.First().ReservationPaymentDetail);
                Assert.IsNull(reservationResponse.Result.First().ReservationPartialPaymentDetails);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationIncludeReservationRooms_Post")]
        public async Task Test_ReservationController_FindSingleReservationIncludeReservationRooms_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationNumbers = new List<string> { "RES000001-893" },
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = false,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = false,
                    IncludeReservationRoomExtras = false,
                    IncludeReservationRooms = true,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);

                Assert.IsNull(reservationResponse.Result.First().ReservationPaymentDetail);
                Assert.IsNull(reservationResponse.Result.First().ReservationPartialPaymentDetails);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomExtras);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomDetails);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomChilds);
            }
        }


        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationIncludeReservationRoomExtras_Post")]
        public async Task Test_ReservationController_FindSingleReservationIncludeReservationRoomExtras_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationNumbers = new List<string> { "RES000011-1635" },
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = false,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = false,
                    IncludeReservationRoomExtras = true,
                    IncludeReservationRooms = false,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);

                Assert.IsNull(reservationResponse.Result.First().ReservationPaymentDetail);
                Assert.IsNull(reservationResponse.Result.First().ReservationPartialPaymentDetails);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomExtras);
                Assert.IsTrue(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomExtras.Count > 0);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomDetails);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomChilds);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationIncludeReservationRoomDetails_Post")]
        public async Task Test_ReservationController_FindSingleReservationIncludeReservationRoomDetails_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationNumbers = new List<string> { "RES000011-1635" },
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = false,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = true,
                    IncludeReservationRoomExtras = false,
                    IncludeReservationRooms = false,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);

                Assert.IsNull(reservationResponse.Result.First().ReservationPaymentDetail);
                Assert.IsNull(reservationResponse.Result.First().ReservationPartialPaymentDetails);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomDetails);
                Assert.IsTrue(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomDetails.Count > 0);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomExtras);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomChilds);
            }
        }

        [Ignore("Migrate me to SOAP UI IT.")]
        [TestMethod, TestCategory("ReservationController Integration Tests")]
        [DeploymentItem("./DL")]
        [DeploymentItem("./Databases_WithData", "Test_ReservationController_FindSingleReservationIncludeReservationRoomChilds_Post")]
        public async Task Test_ReservationController_FindSingleReservationIncludeReservationRoomChilds_Post()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var findReservationRequestObj = new Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),
                    ReservationNumbers = new List<string> { "RES000001-1814" },
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = true,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = false,
                    IncludeReservationRoomExtras = false,
                    IncludeReservationRooms = false,
                    RuleType = Reservation.BL.Constants.RuleType.Omnibees
                };

                var jsonListReservationRequest = JsonConvert.SerializeObject(findReservationRequestObj);

                var request = server.CreateRequest("/api/Reservation/ListReservations").AddHeader("Accept", "application/json");
                request.And((message) =>
                {
                    var content = new StringContent(jsonListReservationRequest);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    message.Content = content;
                });

                var response = await request.PostAsync();
                var result = await response.Content.ReadAsStringAsync();


                var reservationResponse = JsonConvert.DeserializeObject<Reservation.BL.Contracts.Responses.ListReservationResponse>(result);

                Assert.IsTrue(reservationResponse.Result != null);
                Assert.IsTrue(reservationResponse.Result.Count == 1);

                Assert.IsNull(reservationResponse.Result.First().ReservationPaymentDetail);
                Assert.IsNull(reservationResponse.Result.First().ReservationPartialPaymentDetails);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms);
                Assert.IsNotNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomChilds);
                Assert.IsTrue(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomChilds.Count > 0);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomExtras);
                Assert.IsNull(reservationResponse.Result.First().ReservationRooms.First().ReservationRoomDetails);
            }
        }


        [TestMethod]
        [TestCategory("ReservationController Integration Tests")]
        public async Task Test_ReservationController_ListMultipleReservations_Get_NotAllowed()
        {
            using (var server = TestServer.Create<OwinTestConf>())
            {
                var response = await server.CreateRequest("/api/Reservation/ListReservations?id=1723,2122").AddHeader("Accept", "application/json").GetAsync();
                var result = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Method Not Allowed", response.ReasonPhrase);
            }
        }
        #endregion

    }

}
